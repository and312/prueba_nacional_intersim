using System;
using System.Collections.Generic;
using NacionalSeguros.Domain.Enums;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Entities;

public class Usuario : Entity<int>
{
    private readonly List<Rol> _roles = new();
    private readonly List<Sesion> _sesiones = new();

    // Requerido por EF Core
    protected Usuario()
    {
    }

    public Usuario(
        string nombre,
        string correo,
        TipoAutenticacion tipoAutenticacion,
        string? activeDirectoryId = null)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        Correo = correo ?? throw new ArgumentNullException(nameof(correo));
        TipoAutenticacion = tipoAutenticacion;
        ActiveDirectoryId = activeDirectoryId;
        Estado = UsuarioEstado.Activo;
        MfaHabilitado = false;
        CreatedBy = "System"; // Por defecto, se sobrescribe en el interceptor de EF
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
        AreaId = 1; // Default Area (ARR-TI)
    }

    public Usuario(
        string nombre,
        string correo,
        TipoAutenticacion tipoAutenticacion,
        int areaId,
        string? nombres,
        string? apellidos,
        string? cargo,
        string? gerencia,
        string? telefono,
        string? extension,
        string? observaciones,
        string? fotografiaUrl,
        string? activeDirectoryId = null)
        : this(nombre, correo, tipoAutenticacion, activeDirectoryId)
    {
        AreaId = areaId;
        Nombres = nombres;
        Apellidos = apellidos;
        Cargo = cargo;
        Gerencia = gerencia;
        Telefono = telefono;
        Extension = extension;
        Observaciones = observaciones;
        FotografiaUrl = fotografiaUrl;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;
    public string? ClaveHash { get; private set; }
    public TipoAutenticacion TipoAutenticacion { get; private set; }
    public string? ActiveDirectoryId { get; private set; }
    public UsuarioEstado Estado { get; private set; }
    public bool MfaHabilitado { get; private set; }
    public string? MfaSecreto { get; private set; }

    // Módulo 01 - Nuevas propiedades de Gestión de Usuarios y RBAC
    public int AreaId { get; private set; }
    public Area Area { get; private set; } = null!;
    public string? Nombres { get; private set; }
    public string? Apellidos { get; private set; }
    public string? Cargo { get; private set; }
    public string? Gerencia { get; private set; }
    public string? Telefono { get; private set; }
    public string? Extension { get; private set; }
    public DateTime? UltimaInteraccionN8N { get; private set; }
    public string? Observaciones { get; private set; }
    public string? FotografiaUrl { get; private set; }
    public string? ResetPasswordToken { get; private set; }
    public DateTime? ResetPasswordTokenExpiration { get; private set; }

    public void RegistrarInteraccionN8N()
    {
        UltimaInteraccionN8N = DateTime.UtcNow;
    }

    // Campos de Auditoría Estándar
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public string? DeletedBy { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    // Propiedades de navegación
    public IReadOnlyCollection<Rol> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<Sesion> Sesiones => _sesiones.AsReadOnly();

    // Métodos del Dominio
    public void CambiarPassword(string claveHash, string modificadoPor)
    {
        if (TipoAutenticacion == TipoAutenticacion.ActiveDirectory)
        {
            throw new InvalidOperationException("No se puede cambiar la contraseña para un usuario autenticado por Active Directory.");
        }

        ClaveHash = claveHash ?? throw new ArgumentNullException(nameof(claveHash));
        ActualizarAuditoria(modificadoPor);
    }

    public void ConfigurarMfa(string secretoBase32, string modificadoPor)
    {
        MfaSecreto = secretoBase32 ?? throw new ArgumentNullException(nameof(secretoBase32));
        MfaHabilitado = false; // Se mantiene deshabilitado hasta la primera validación exitosa
        ActualizarAuditoria(modificadoPor);
    }

    public void ConfirmarMfa(string modificadoPor)
    {
        if (string.IsNullOrEmpty(MfaSecreto))
        {
            throw new InvalidOperationException("No se puede habilitar MFA sin un secreto configurado.");
        }
        MfaHabilitado = true;
        ActualizarAuditoria(modificadoPor);
    }

    public void DeshabilitarMfa(string modificadoPor)
    {
        MfaSecreto = null;
        MfaHabilitado = false;
        ActualizarAuditoria(modificadoPor);
    }

    public void ActualizarDatosBasicos(string nombre, string modificadoPor)
    {
        Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
        ActualizarAuditoria(modificadoPor);
    }

    public void Activar(string modificadoPor)
    {
        Estado = UsuarioEstado.Activo;
        ActualizarAuditoria(modificadoPor);
    }

    public void Inactivar(string modificadoPor)
    {
        Estado = UsuarioEstado.Inactivo;
        ActualizarAuditoria(modificadoPor);
    }

    public void EliminarLogicamente(string eliminadoPor)
    {
        IsDeleted = true;
        DeletedBy = eliminadoPor;
        DeletedDate = DateTime.UtcNow;
    }

    public void AsignarRol(Rol rol)
    {
        if (rol == null) throw new ArgumentNullException(nameof(rol));
        if (!_roles.Contains(rol))
        {
            _roles.Add(rol);
        }
    }

    public void LimpiarRoles()
    {
        _roles.Clear();
    }

    public void Bloquear(string modificadoPor)
    {
        Estado = UsuarioEstado.Bloqueado;
        ActualizarAuditoria(modificadoPor);
    }

    public void Desbloquear(string modificadoPor)
    {
        Estado = UsuarioEstado.Activo;
        ActualizarAuditoria(modificadoPor);
    }

    public void ActualizarDatosCompletos(
        string correo,
        string nombres,
        string apellidos,
        int areaId,
        string? cargo,
        string? gerencia,
        string? telefono,
        string? extension,
        string? observaciones,
        string? fotografiaUrl,
        string modificadoPor)
    {
        Correo = correo;
        Nombres = nombres;
        Apellidos = apellidos;
        Nombre = $"{nombres} {apellidos}".Trim();
        AreaId = areaId;
        Cargo = cargo;
        Gerencia = gerencia;
        Telefono = telefono;
        Extension = extension;
        Observaciones = observaciones;
        FotografiaUrl = fotografiaUrl;
        ActualizarAuditoria(modificadoPor);
    }

    public void SetResetPasswordToken(string token, DateTime expiration)
    {
        ResetPasswordToken = token;
        ResetPasswordTokenExpiration = expiration;
    }

    public void ResetPassword(string newPasswordHash)
    {
        ClaveHash = newPasswordHash;
        ResetPasswordToken = null;
        ResetPasswordTokenExpiration = null;
    }

    private void ActualizarAuditoria(string modificadoPor)
    {
        ModifiedBy = modificadoPor;
        ModifiedDate = DateTime.UtcNow;
    }
}
