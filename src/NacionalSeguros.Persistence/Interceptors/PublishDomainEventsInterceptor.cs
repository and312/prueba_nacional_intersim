using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NacionalSeguros.Application.Abstractions.Events;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Primitives;
using NacionalSeguros.Domain.Repositories;

namespace NacionalSeguros.Persistence.Interceptors;

public class PublishDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly List<(object Entity, StateHistory History)> _pendingAddedAudits = new();

    public PublishDomainEventsInterceptor(IPublisher publisher, IHttpContextAccessor httpContextAccessor)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            AuditStateHistory(eventData.Context, default).GetAwaiter().GetResult();
        }
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            await AuditStateHistory(eventData.Context, cancellationToken);
        }
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context != null && _pendingAddedAudits.Any())
        {
            var historiesToAdd = new List<StateHistory>();
            foreach (var pending in _pendingAddedAudits)
            {
                if (pending.Entity is PerfilCargo perfil && perfil.Id > 0)
                {
                    var history = new StateHistory(
                        entidad: "PerfilCargo",
                        entidadId: perfil.Id,
                        estadoAnteriorId: null,
                        estadoNuevoId: pending.History.EstadoNuevoId,
                        usuarioId: pending.History.UsuarioId,
                        comentario: pending.History.Comentario,
                        correlationId: pending.History.CorrelationId,
                        iteracion: perfil.Version,
                        rol: pending.History.Rol
                    );
                    historiesToAdd.Add(history);
                }
            }
            _pendingAddedAudits.Clear();
            if (historiesToAdd.Any())
            {
                eventData.Context.Set<StateHistory>().AddRange(historiesToAdd);
                eventData.Context.SaveChanges();
            }
        }
        PublishDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null && _pendingAddedAudits.Any())
        {
            var historiesToAdd = new List<StateHistory>();
            foreach (var pending in _pendingAddedAudits)
            {
                if (pending.Entity is PerfilCargo perfil && perfil.Id > 0)
                {
                    var history = new StateHistory(
                        entidad: "PerfilCargo",
                        entidadId: perfil.Id,
                        estadoAnteriorId: null,
                        estadoNuevoId: pending.History.EstadoNuevoId,
                        usuarioId: pending.History.UsuarioId,
                        comentario: pending.History.Comentario,
                        correlationId: pending.History.CorrelationId,
                        iteracion: perfil.Version,
                        rol: pending.History.Rol
                    );
                    historiesToAdd.Add(history);
                }
            }
            _pendingAddedAudits.Clear();
            if (historiesToAdd.Any())
            {
                await eventData.Context.Set<StateHistory>().AddRangeAsync(historiesToAdd, cancellationToken);
                await eventData.Context.SaveChangesAsync(cancellationToken);
            }
        }
        await PublishDomainEvents(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task AuditStateHistory(DbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = dbContext.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added)
            .ToList();

        var stateHistories = new List<StateHistory>();

        var httpContext = _httpContextAccessor.HttpContext;
        string userMail = "system@nacionalseguros.com.bo";
        Guid correlationId = Guid.NewGuid();
        string comment = "Transición de estado registrada por el sistema";

        if (httpContext != null)
        {
            userMail = httpContext.User?.FindFirst(ClaimTypes.Email)?.Value ?? userMail;
            if (httpContext.Items.TryGetValue("X-Correlation-ID", out var cid) && cid != null && Guid.TryParse(cid.ToString(), out var parsedId))
            {
                correlationId = parsedId;
            }
            if (httpContext.Items.TryGetValue("TransitionComment", out var cmt) && cmt != null)
            {
                comment = cmt.ToString()!;
            }
        }

        // También intentar obtener el comentario del UnitOfWork si está disponible
        try
        {
            var unitOfWork = dbContext.GetService<IUnitOfWork>();
            if (unitOfWork != null && !string.IsNullOrWhiteSpace(unitOfWork.TransitionComment))
            {
                comment = unitOfWork.TransitionComment;
            }
        }
        catch { }

        // Obtener el ID del usuario actual y su rol
        int usuarioId = 1; // Default/System
        string userRole = "System";
        var usuario = await dbContext.Set<Usuario>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Correo == userMail, cancellationToken);
        if (usuario != null)
        {
            usuarioId = usuario.Id;
            userRole = usuario.Roles.FirstOrDefault()?.Nombre ?? "Usuario";
        }

        foreach (var entry in entries)
        {
            if (entry.Entity is Solicitud solicitud)
            {
                var estadoIdProp = entry.Property("EstadoId");
                if (estadoIdProp.IsModified)
                {
                    int estadoAnteriorId = (int)estadoIdProp.OriginalValue!;
                    int estadoNuevoId = (int)estadoIdProp.CurrentValue!;

                    if (estadoAnteriorId != estadoNuevoId)
                    {
                        stateHistories.Add(new StateHistory(
                            entidad: "Solicitud",
                            entidadId: solicitud.Id,
                            estadoAnteriorId: estadoAnteriorId > 0 ? estadoAnteriorId : (int?)null,
                            estadoNuevoId: estadoNuevoId,
                            usuarioId: usuarioId,
                            comentario: comment,
                            correlationId: correlationId,
                            iteracion: solicitud.Iteracion,
                            rol: userRole
                        ));
                    }
                }
            }
            else if (entry.Entity is Vacante vacante)
            {
                var estadoIdProp = entry.Property("EstadoId");
                if (estadoIdProp.IsModified)
                {
                    int estadoAnteriorId = (int)estadoIdProp.OriginalValue!;
                    int estadoNuevoId = (int)estadoIdProp.CurrentValue!;

                    if (estadoAnteriorId != estadoNuevoId)
                    {
                        stateHistories.Add(new StateHistory(
                            entidad: "Vacante",
                            entidadId: vacante.Id,
                            estadoAnteriorId: estadoAnteriorId > 0 ? estadoAnteriorId : (int?)null,
                            estadoNuevoId: estadoNuevoId,
                            usuarioId: usuarioId,
                            comentario: comment,
                            correlationId: correlationId,
                            iteracion: null,
                            rol: userRole
                        ));
                    }
                }
            }
            else if (entry.Entity is Postulacion postulacion)
            {
                var estadoIdProp = entry.Property("EstadoPipelineId");
                if (estadoIdProp.IsModified)
                {
                    int estadoAnteriorId = (int)estadoIdProp.OriginalValue!;
                    int estadoNuevoId = (int)estadoIdProp.CurrentValue!;

                    if (estadoAnteriorId != estadoNuevoId)
                    {
                        stateHistories.Add(new StateHistory(
                            entidad: "Postulacion",
                            entidadId: postulacion.Id,
                            estadoAnteriorId: estadoAnteriorId > 0 ? estadoAnteriorId : (int?)null,
                            estadoNuevoId: estadoNuevoId,
                            usuarioId: usuarioId,
                            comentario: comment,
                            correlationId: correlationId,
                            iteracion: null,
                            rol: userRole
                        ));
                    }
                }
            }
            else if (entry.Entity is PerfilCargo perfil)
            {
                var estadoIdProp = entry.Property("EstadoId");
                if (entry.State == EntityState.Added)
                {
                    int estadoNuevoId = (int)estadoIdProp.CurrentValue!;
                    var history = new StateHistory(
                        entidad: "PerfilCargo",
                        entidadId: perfil.Id,
                        estadoAnteriorId: null,
                        estadoNuevoId: estadoNuevoId,
                        usuarioId: usuarioId,
                        comentario: comment,
                        correlationId: correlationId,
                        iteracion: perfil.Version,
                        rol: userRole
                    );
                    _pendingAddedAudits.Add((perfil, history));
                }
                else if (entry.State == EntityState.Modified && estadoIdProp.IsModified)
                {
                    int estadoAnteriorId = (int)estadoIdProp.OriginalValue!;
                    int estadoNuevoId = (int)estadoIdProp.CurrentValue!;
                    if (estadoAnteriorId != estadoNuevoId)
                    {
                        stateHistories.Add(new StateHistory(
                            entidad: "PerfilCargo",
                            entidadId: perfil.Id,
                            estadoAnteriorId: estadoAnteriorId > 0 ? estadoAnteriorId : (int?)null,
                            estadoNuevoId: estadoNuevoId,
                            usuarioId: usuarioId,
                            comentario: comment,
                            correlationId: correlationId,
                            iteracion: perfil.Version,
                            rol: userRole
                        ));
                    }
                }
            }
        }

        if (stateHistories.Any())
        {
            await dbContext.Set<StateHistory>().AddRangeAsync(stateHistories, cancellationToken);
        }
    }

    private async Task PublishDomainEvents(DbContext? dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext == null) return;

        var domainEvents = new List<IDomainEvent>();

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (entry.Entity is Entity entity)
            {
                domainEvents.AddRange(entity.GetDomainEvents());
                entity.ClearDomainEvents();
            }
            else
            {
                var type = entry.Entity.GetType();
                var baseType = type.BaseType;
                while (baseType != null)
                {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Entity<>))
                    {
                        var getEventsMethod = baseType.GetMethod("GetDomainEvents");
                        var clearEventsMethod = baseType.GetMethod("ClearDomainEvents");
                        if (getEventsMethod != null && clearEventsMethod != null)
                        {
                            var events = getEventsMethod.Invoke(entry.Entity, null) as IEnumerable<IDomainEvent>;
                            if (events != null)
                            {
                                domainEvents.AddRange(events);
                            }
                            clearEventsMethod.Invoke(entry.Entity, null);
                        }
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
        }

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);
            if (notification != null)
            {
                await _publisher.Publish(notification, cancellationToken);
            }
        }
    }
}
