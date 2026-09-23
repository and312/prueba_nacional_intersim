using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Application.Catalogos.Commands.ActualizarCatalogo;
using NacionalSeguros.Application.Catalogos.Commands.ActualizarParametro;
using NacionalSeguros.Application.Catalogos.Commands.CrearCatalogo;
using NacionalSeguros.Application.Catalogos.Commands.CrearParametro;
using NacionalSeguros.Application.Catalogos.Commands.EliminarCatalogo;
using NacionalSeguros.Application.Catalogos.Commands.EliminarParametro;
using NacionalSeguros.Application.Catalogos.Queries.BuscarCatalogos;
using NacionalSeguros.Application.Catalogos.Queries.ObtenerCatalogoPorId;
using NacionalSeguros.Application.Catalogos.Queries.ObtenerCatalogos;
using NacionalSeguros.Application.Catalogos.Queries.ObtenerDetalles;
using NacionalSeguros.Application.Catalogos.Queries.ObtenerSlas;
using NacionalSeguros.Contracts.Catalogos;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Contracts.Security;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Persistence.Context;
using NacionalSeguros.Shared.Primitives;
using NacionalSeguros.Application.Perfiles.Commands;
using NacionalSeguros.Contracts.Requests;

namespace NacionalSeguros.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class CatalogosController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ApplicationDbContext _context;

    public CatalogosController(ISender sender, ApplicationDbContext context)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }



    // ==========================================
    // NUEVOS ENDPOINTS DE TABLAS MAESTRAS (CRUD COMPLETO)
    // ==========================================

    // --- REGIONALES ---

    [HttpGet("regionales")]
    [ProducesResponseType(typeof(IEnumerable<MasterDataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegionales([FromQuery] bool? soloActivos = null)
    {
        var query = _context.Regionales.Where(r => !r.IsDeleted);
        if (soloActivos == true || soloActivos == null) // fallback combos default active only
        {
            query = query.Where(r => r.Estado == "Activo");
        }
        var list = await query
            .OrderBy(r => r.Id)
            .Select(r => new MasterDataResponse(r.Id, r.Codigo, r.Nombre, r.Descripcion, r.Estado, r.IsDeleted))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("regionales/{id:int}")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegionalById([FromRoute] int id)
    {
        var regional = await _context.Regionales.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (regional == null) return NotFound();
        return Ok(new MasterDataResponse(regional.Id, regional.Codigo, regional.Nombre, regional.Descripcion, regional.Estado, regional.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("regionales")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearRegional([FromBody] MasterDataCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre es obligatorio.", CorrelationId = GetCorrelationId() });
        if (string.IsNullOrWhiteSpace(request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Código es obligatorio.", CorrelationId = GetCorrelationId() });

        if (await _context.Regionales.AnyAsync(r => r.Codigo == request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Duplicate.Code", Message = $"El código '{request.Codigo}' ya existe.", CorrelationId = GetCorrelationId() });

        var regional = new Regional(request.Codigo, request.Nombre, request.Descripcion);
        regional.SetAuditoriaCreacion(GetUserNombre());

        _context.Regionales.Add(regional);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRegionalById), new { id = regional.Id }, new MasterDataResponse(regional.Id, regional.Codigo, regional.Nombre, regional.Descripcion, regional.Estado, regional.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("regionales/{id:int}")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarRegional([FromRoute] int id, [FromBody] MasterDataUpdateRequest request)
    {
        var regional = await _context.Regionales.FirstOrDefaultAsync(r => r.Id == id);
        if (regional == null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre es obligatorio.", CorrelationId = GetCorrelationId() });

        regional.Actualizar(request.Nombre, request.Descripcion);
        regional.SetEstado(request.Estado);
        if (request.Estado == "Activo" && regional.IsDeleted)
        {
            regional.Reactivar();
        }
        regional.SetAuditoriaModificacion(GetUserNombre());

        await _context.SaveChangesAsync();
        return Ok(new MasterDataResponse(regional.Id, regional.Codigo, regional.Nombre, regional.Descripcion, regional.Estado, regional.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("regionales/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarRegional([FromRoute] int id)
    {
        var regional = await _context.Regionales.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (regional == null) return NotFound();

        bool hasRelations = await _context.Solicitudes.AnyAsync(s => s.RegionalId == id);
        if (hasRelations)
        {
            return BadRequest(new ApiErrorDto 
            { 
                Code = "Deletion.Blocked", 
                Message = "No se puede eliminar la regional porque está relacionada con solicitudes existentes.",
                CorrelationId = GetCorrelationId()
            });
        }

        regional.SoftDelete(GetUserNombre());
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // --- TIPOS DE SOLICITUD ---

    [HttpGet("tipos-solicitud")]
    [ProducesResponseType(typeof(IEnumerable<MasterDataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposSolicitud([FromQuery] bool? soloActivos = null)
    {
        var query = _context.TiposSolicitud.Where(t => !t.IsDeleted);
        if (soloActivos == true || soloActivos == null)
        {
            query = query.Where(t => t.Estado == "Activo");
        }
        var list = await query
            .OrderBy(t => t.Id)
            .Select(t => new MasterDataResponse(t.Id, t.Codigo, t.Nombre, t.Descripcion, t.Estado, t.IsDeleted))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("tipos-solicitud/{id:int}")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTipoSolicitudById([FromRoute] int id)
    {
        var tipo = await _context.TiposSolicitud.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (tipo == null) return NotFound();
        return Ok(new MasterDataResponse(tipo.Id, tipo.Codigo, tipo.Nombre, tipo.Descripcion, tipo.Estado, tipo.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("tipos-solicitud")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearTipoSolicitud([FromBody] MasterDataCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre es obligatorio.", CorrelationId = GetCorrelationId() });
        if (string.IsNullOrWhiteSpace(request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Código es obligatorio.", CorrelationId = GetCorrelationId() });

        if (await _context.TiposSolicitud.AnyAsync(t => t.Codigo == request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Duplicate.Code", Message = $"El código '{request.Codigo}' ya existe.", CorrelationId = GetCorrelationId() });

        var tipo = new TipoSolicitudEntity(request.Codigo, request.Nombre, request.Descripcion);
        tipo.SetAuditoriaCreacion(GetUserNombre());

        _context.TiposSolicitud.Add(tipo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTipoSolicitudById), new { id = tipo.Id }, new MasterDataResponse(tipo.Id, tipo.Codigo, tipo.Nombre, tipo.Descripcion, tipo.Estado, tipo.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("tipos-solicitud/{id:int}")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarTipoSolicitud([FromRoute] int id, [FromBody] MasterDataUpdateRequest request)
    {
        var tipo = await _context.TiposSolicitud.FirstOrDefaultAsync(t => t.Id == id);
        if (tipo == null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre es obligatorio.", CorrelationId = GetCorrelationId() });

        tipo.Actualizar(request.Nombre, request.Descripcion);
        tipo.SetEstado(request.Estado);
        if (request.Estado == "Activo" && tipo.IsDeleted)
        {
            tipo.Reactivar();
        }
        tipo.SetAuditoriaModificacion(GetUserNombre());

        await _context.SaveChangesAsync();
        return Ok(new MasterDataResponse(tipo.Id, tipo.Codigo, tipo.Nombre, tipo.Descripcion, tipo.Estado, tipo.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("tipos-solicitud/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarTipoSolicitud([FromRoute] int id)
    {
        var tipo = await _context.TiposSolicitud.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (tipo == null) return NotFound();

        bool hasRelations = await _context.Solicitudes.AnyAsync(s => s.TipoSolicitudId == id);
        if (hasRelations)
        {
            return BadRequest(new ApiErrorDto 
            { 
                Code = "Deletion.Blocked", 
                Message = "No se puede eliminar el tipo de solicitud porque está relacionado con solicitudes existentes.",
                CorrelationId = GetCorrelationId()
            });
        }

        tipo.SoftDelete(GetUserNombre());
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // --- MODALIDADES DE TRABAJO ---

    [HttpGet("modalidades-trabajo")]
    [ProducesResponseType(typeof(IEnumerable<MasterDataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModalidadesTrabajo([FromQuery] bool? soloActivos = null)
    {
        var query = _context.ModalidadesTrabajo.Where(m => !m.IsDeleted);
        if (soloActivos == true || soloActivos == null)
        {
            query = query.Where(m => m.Estado == "Activo");
        }
        var list = await query
            .OrderBy(m => m.Id)
            .Select(m => new MasterDataResponse(m.Id, m.Codigo, m.Nombre, m.Descripcion, m.Estado, m.IsDeleted))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("modalidades-trabajo/{id:int}")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModalidadTrabajoById([FromRoute] int id)
    {
        var modalidad = await _context.ModalidadesTrabajo.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        if (modalidad == null) return NotFound();
        return Ok(new MasterDataResponse(modalidad.Id, modalidad.Codigo, modalidad.Nombre, modalidad.Descripcion, modalidad.Estado, modalidad.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("modalidades-trabajo")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearModalidadTrabajo([FromBody] MasterDataCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre es obligatorio.", CorrelationId = GetCorrelationId() });
        if (string.IsNullOrWhiteSpace(request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Código es obligatorio.", CorrelationId = GetCorrelationId() });

        if (await _context.ModalidadesTrabajo.AnyAsync(m => m.Codigo == request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Duplicate.Code", Message = $"El código '{request.Codigo}' ya existe.", CorrelationId = GetCorrelationId() });

        var modalidad = new ModalidadTrabajo(request.Codigo, request.Nombre, request.Descripcion);
        modalidad.SetAuditoriaCreacion(GetUserNombre());

        _context.ModalidadesTrabajo.Add(modalidad);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetModalidadTrabajoById), new { id = modalidad.Id }, new MasterDataResponse(modalidad.Id, modalidad.Codigo, modalidad.Nombre, modalidad.Descripcion, modalidad.Estado, modalidad.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("modalidades-trabajo/{id:int}")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarModalidadTrabajo([FromRoute] int id, [FromBody] MasterDataUpdateRequest request)
    {
        var modalidad = await _context.ModalidadesTrabajo.FirstOrDefaultAsync(m => m.Id == id);
        if (modalidad == null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre es obligatorio.", CorrelationId = GetCorrelationId() });

        modalidad.Actualizar(request.Nombre, request.Descripcion);
        modalidad.SetEstado(request.Estado);
        if (request.Estado == "Activo" && modalidad.IsDeleted)
        {
            modalidad.Reactivar();
        }
        modalidad.SetAuditoriaModificacion(GetUserNombre());

        await _context.SaveChangesAsync();
        return Ok(new MasterDataResponse(modalidad.Id, modalidad.Codigo, modalidad.Nombre, modalidad.Descripcion, modalidad.Estado, modalidad.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("modalidades-trabajo/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarModalidadTrabajo([FromRoute] int id)
    {
        var modalidad = await _context.ModalidadesTrabajo.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        if (modalidad == null) return NotFound();

        bool hasRelations = await _context.Solicitudes.AnyAsync(s => s.ModalidadTrabajoId == id);
        if (hasRelations)
        {
            return BadRequest(new ApiErrorDto 
            { 
                Code = "Deletion.Blocked", 
                Message = "No se puede eliminar la modalidad de trabajo porque está relacionada con solicitudes existentes.",
                CorrelationId = GetCorrelationId()
            });
        }

        modalidad.SoftDelete(GetUserNombre());
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // --- ÁREAS DE CARGO ---

    [HttpGet("areas-cargo")]
    [ProducesResponseType(typeof(IEnumerable<MasterDataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAreasCargo([FromQuery] bool? soloActivos = null)
    {
        var query = _context.AreasCargo.Where(a => !a.IsDeleted);
        if (soloActivos == true || soloActivos == null)
        {
            query = query.Where(a => a.Estado == "Activo");
        }
        var list = await query
            .OrderBy(a => a.Id)
            .Select(a => new MasterDataResponse(a.Id, a.Codigo, a.Nombre, a.Descripcion, a.Estado, a.IsDeleted))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("areas-cargo/{id:int}")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAreaCargoById([FromRoute] int id)
    {
        var areaCargo = await _context.AreasCargo.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (areaCargo == null) return NotFound();
        return Ok(new MasterDataResponse(areaCargo.Id, areaCargo.Codigo, areaCargo.Nombre, areaCargo.Descripcion, areaCargo.Estado, areaCargo.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("areas-cargo")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearAreaCargo([FromBody] MasterDataCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre es obligatorio.", CorrelationId = GetCorrelationId() });
        if (string.IsNullOrWhiteSpace(request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Código es obligatorio.", CorrelationId = GetCorrelationId() });

        if (await _context.AreasCargo.AnyAsync(a => a.Codigo == request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Duplicate.Code", Message = $"El código '{request.Codigo}' ya existe.", CorrelationId = GetCorrelationId() });

        var areaCargo = new AreaCargo(request.Codigo, request.Nombre, request.Descripcion);
        areaCargo.SetAuditoriaCreacion(GetUserNombre());

        _context.AreasCargo.Add(areaCargo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAreaCargoById), new { id = areaCargo.Id }, new MasterDataResponse(areaCargo.Id, areaCargo.Codigo, areaCargo.Nombre, areaCargo.Descripcion, areaCargo.Estado, areaCargo.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("areas-cargo/{id:int}")]
    [ProducesResponseType(typeof(MasterDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarAreaCargo([FromRoute] int id, [FromBody] MasterDataUpdateRequest request)
    {
        var areaCargo = await _context.AreasCargo.FirstOrDefaultAsync(a => a.Id == id);
        if (areaCargo == null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre es obligatorio.", CorrelationId = GetCorrelationId() });

        areaCargo.Actualizar(request.Nombre, request.Descripcion);
        areaCargo.SetEstado(request.Estado);
        if (request.Estado == "Activo" && areaCargo.IsDeleted)
        {
            areaCargo.Reactivar();
        }
        areaCargo.SetAuditoriaModificacion(GetUserNombre());

        await _context.SaveChangesAsync();
        return Ok(new MasterDataResponse(areaCargo.Id, areaCargo.Codigo, areaCargo.Nombre, areaCargo.Descripcion, areaCargo.Estado, areaCargo.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("areas-cargo/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarAreaCargo([FromRoute] int id)
    {
        var areaCargo = await _context.AreasCargo.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (areaCargo == null) return NotFound();

        bool hasCargos = await _context.Cargos.AnyAsync(c => c.AreaCargoId == id && !c.IsDeleted);
        if (hasCargos)
        {
            return BadRequest(new ApiErrorDto 
            { 
                Code = "Deletion.Blocked", 
                Message = "No se puede eliminar el Área de Cargo porque contiene cargos asociados.",
                CorrelationId = GetCorrelationId()
            });
        }

        areaCargo.SoftDelete(GetUserNombre());
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // --- CARGOS ---

    [HttpGet("cargos")]
    [ProducesResponseType(typeof(IEnumerable<CargoDataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCargos([FromQuery] int? areaCargoId = null, [FromQuery] bool? soloActivos = null)
    {
        var query = _context.Cargos.Include(c => c.AreaCargo).Where(c => !c.IsDeleted);
        if (areaCargoId.HasValue && areaCargoId.Value > 0)
        {
            query = query.Where(c => c.AreaCargoId == areaCargoId.Value);
        }
        if (soloActivos == true || soloActivos == null)
        {
            query = query.Where(c => c.Estado == "Activo");
        }
        var list = await query
            .OrderBy(c => c.Id)
            .Select(c => new CargoDataResponse(c.Id, c.AreaCargoId, c.AreaCargo.Nombre, c.Codigo, c.Nombre, c.Descripcion, c.Estado, c.IsDeleted))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("cargos/{id:int}")]
    [ProducesResponseType(typeof(CargoDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCargoById([FromRoute] int id)
    {
        var cargo = await _context.Cargos.Include(c => c.AreaCargo).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (cargo == null) return NotFound();
        return Ok(new CargoDataResponse(cargo.Id, cargo.AreaCargoId, cargo.AreaCargo.Nombre, cargo.Codigo, cargo.Nombre, cargo.Descripcion, cargo.Estado, cargo.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("cargos")]
    [ProducesResponseType(typeof(CargoDataResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearCargo([FromBody] CargoCreateRequest request)
    {
        if (request.AreaCargoId <= 0)
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Área de Cargo es obligatoria.", CorrelationId = GetCorrelationId() });
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre del Cargo es obligatorio.", CorrelationId = GetCorrelationId() });
        if (string.IsNullOrWhiteSpace(request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Código es obligatorio.", CorrelationId = GetCorrelationId() });

        var areaCargo = await _context.AreasCargo.FirstOrDefaultAsync(a => a.Id == request.AreaCargoId && !a.IsDeleted);
        if (areaCargo == null)
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Área de Cargo especificada no existe.", CorrelationId = GetCorrelationId() });

        if (await _context.Cargos.AnyAsync(c => c.Codigo == request.Codigo))
            return BadRequest(new ApiErrorDto { Code = "Duplicate.Code", Message = $"El código '{request.Codigo}' ya existe.", CorrelationId = GetCorrelationId() });

        var cargo = new Cargo(request.AreaCargoId, request.Codigo, request.Nombre, request.Descripcion);
        cargo.SetAuditoriaCreacion(GetUserNombre());

        _context.Cargos.Add(cargo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCargoById), new { id = cargo.Id }, new CargoDataResponse(cargo.Id, areaCargo.Id, areaCargo.Nombre, cargo.Codigo, cargo.Nombre, cargo.Descripcion, cargo.Estado, cargo.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("cargos/{id:int}")]
    [ProducesResponseType(typeof(CargoDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarCargo([FromRoute] int id, [FromBody] CargoUpdateRequest request)
    {
        var cargo = await _context.Cargos.Include(c => c.AreaCargo).FirstOrDefaultAsync(c => c.Id == id);
        if (cargo == null) return NotFound();

        if (request.AreaCargoId <= 0)
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Área de Cargo es obligatoria.", CorrelationId = GetCorrelationId() });
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Nombre del Cargo es obligatorio.", CorrelationId = GetCorrelationId() });

        var areaCargo = await _context.AreasCargo.FirstOrDefaultAsync(a => a.Id == request.AreaCargoId);
        if (areaCargo == null)
            return BadRequest(new ApiErrorDto { Code = "Validation.Error", Message = "El Área de Cargo especificada no existe.", CorrelationId = GetCorrelationId() });

        cargo.Actualizar(request.AreaCargoId, request.Nombre, request.Descripcion);
        cargo.SetEstado(request.Estado);
        if (request.Estado == "Activo" && cargo.IsDeleted)
        {
            cargo.Reactivar();
        }
        cargo.SetAuditoriaModificacion(GetUserNombre());

        await _context.SaveChangesAsync();
        return Ok(new CargoDataResponse(cargo.Id, areaCargo.Id, areaCargo.Nombre, cargo.Codigo, cargo.Nombre, cargo.Descripcion, cargo.Estado, cargo.IsDeleted));
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("cargos/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarCargo([FromRoute] int id)
    {
        var cargo = await _context.Cargos.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (cargo == null) return NotFound();

        cargo.SoftDelete(GetUserNombre());
        await _context.SaveChangesAsync();
        return NoContent();
    }


    // ==========================================
    // ENDPOINTS DE CONFIGURACIÓN PÚBLICOS / AUTENTICADOS (OpenAPI)
    // ==========================================

    [HttpGet("config/catalogos")]
    [ProducesResponseType(typeof(IEnumerable<ParametroResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConsultarCatalogo([FromQuery] string catalogoNombre)
    {
        var query = new ObtenerDetallesQuery(null, catalogoNombre);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron obtener los detalles del catálogo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("config/slas")]
    [ProducesResponseType(typeof(IEnumerable<SlaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConsultarSlas()
    {
        var query = new ObtenerSlasQuery();
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron obtener los parámetros de SLAs.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    // ==========================================
    // CRUD DE CATÁLOGOS (RBAC)
    // ==========================================

    [Authorize(Roles = "Administrador,RRHH,Reclutador,Auditor")]
    [HttpGet("catalogos")]
    [ProducesResponseType(typeof(IEnumerable<CatalogoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? search = null)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchQuery = new BuscarCatalogosQuery(search);
            var searchResult = await _sender.Send(searchQuery);

            if (searchResult.IsFailure)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = searchResult.Error.Code,
                    Message = searchResult.Error.Message,
                    Detail = "Error en la búsqueda de catálogos.",
                    CorrelationId = GetCorrelationId()
                });
            }

            return Ok(searchResult.Value);
        }

        var query = new ObtenerCatalogosQuery();
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron obtener los catálogos.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador,RRHH,Reclutador,Auditor")]
    [HttpGet("catalogos/{id:int}")]
    [ProducesResponseType(typeof(CatalogoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id)
    {
        var query = new ObtenerCatalogoPorIdQuery(id);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = $"No se encontró el catálogo con el ID {id}.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("catalogos")]
    [ProducesResponseType(typeof(CatalogoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearCatalogoRequest request)
    {
        var command = new CrearCatalogoCommand(request.Nombre, request.Codigo, GetUserNombre());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar crear el catálogo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Value.CatalogoId }, result.Value);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("catalogos/{id:int}")]
    [ProducesResponseType(typeof(CatalogoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] ActualizarCatalogoRequest request)
    {
        var command = new ActualizarCatalogoCommand(id, request.Nombre, GetUserNombre());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar actualizar el catálogo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("catalogos/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        var command = new EliminarCatalogoCommand(id, GetUserNombre());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar inactivar el catálogo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return NoContent();
    }

    // ==========================================
    // DETALLES / PARÁMETROS DE CATÁLOGOS (RBAC)
    // ==========================================

    [Authorize(Roles = "Administrador,RRHH,Reclutador,Auditor")]
    [HttpGet("catalogos/{catalogoId:int}/detalles")]
    [ProducesResponseType(typeof(IEnumerable<ParametroResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarDetalles([FromRoute] int catalogoId)
    {
        var query = new ObtenerDetallesQuery(catalogoId, null);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "No se pudieron obtener los detalles del catálogo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("catalogos/{catalogoId:int}/detalles")]
    [ProducesResponseType(typeof(ParametroResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearDetalle([FromRoute] int catalogoId, [FromBody] CrearParametroRequest request)
    {
        var command = new CrearParametroCommand(catalogoId, request.Codigo, request.Valor, request.ParametroIdPadre, GetUserNombre());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar registrar el parámetro en el catálogo.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("catalogos/detalles/{id:int}")]
    [ProducesResponseType(typeof(ParametroResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarDetalle([FromRoute] int id, [FromBody] ActualizarParametroRequest request)
    {
        var command = new ActualizarParametroCommand(id, request.Valor, request.ParametroIdPadre, GetUserNombre());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar actualizar el parámetro.",
                CorrelationId = GetCorrelationId()
            });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("catalogos/detalles/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EliminarDetalle([FromRoute] int id)
    {
        var command = new EliminarParametroCommand(id, GetUserNombre());
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorDto
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Detail = "Error al intentar inactivar el parámetro.",
                CorrelationId = GetCorrelationId()
            });
        }

        return NoContent();
    }

    // ==========================================
    // MÉTODOS AUXILIARES
    // ==========================================

    private string GetUserNombre()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return User.Identity.Name ??
                   User.FindFirst(ClaimTypes.Email)?.Value ??
                   User.FindFirst("email")?.Value ??
                   "Admin";
        }
        return "SYSTEM";
    }

    private string GetCorrelationId()
    {
        if (HttpContext.Items.TryGetValue("X-Correlation-ID", out var cid) && cid != null)
        {
            return cid.ToString()!;
        }
        return Guid.NewGuid().ToString();
    }
}
