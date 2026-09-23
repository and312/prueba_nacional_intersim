using System;
using System.Data.Common;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NacionalSeguros.Persistence.Interceptors;

public class SetSessionContextInterceptor : DbConnectionInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SetSessionContextInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetSessionContextAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        SetSessionContext(connection);
        base.ConnectionOpened(connection, eventData);
    }

    private void SetSessionContext(DbConnection connection)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        string userMail = "system@nacionalseguros.com.bo";
        string userRol = "Administrador"; // Default to Admin for tests / CLI
        string userArea = "";
        string correlationId = Guid.NewGuid().ToString();

        if (httpContext != null)
        {
            var userClaims = httpContext.User;
            userMail = userClaims?.FindFirst(ClaimTypes.Email)?.Value 
                       ?? userClaims?.FindFirst("email")?.Value 
                       ?? userMail;
            userRol = userClaims?.FindFirst(ClaimTypes.Role)?.Value 
                      ?? userClaims?.FindFirst("role")?.Value 
                      ?? userRol;
            userArea = userClaims?.FindFirst("area")?.Value 
                       ?? (httpContext.Request.Headers.ContainsKey("X-User-Area") ? httpContext.Request.Headers["X-User-Area"].ToString() : null) 
                       ?? userArea;
            correlationId = httpContext.Items["X-Correlation-ID"]?.ToString() ?? correlationId;
        }

        if (string.IsNullOrEmpty(userArea) && !string.IsNullOrEmpty(userMail))
        {
            string emailLower = userMail.ToLower();
            if (emailLower.StartsWith("admin")) userArea = "Tecnología (TI)";
            else if (emailLower.StartsWith("rrhh")) userArea = "Recursos Humanos";
            else if (emailLower.StartsWith("gerente")) userArea = "Gerencia General";
            else if (emailLower.StartsWith("comercial")) userArea = "Comercial";
            else if (emailLower.StartsWith("finanzas") || emailLower.StartsWith("alexander")) userArea = "Finanzas";
        }

        Console.WriteLine($"[SetSessionContext Sync] Mail: '{userMail}', Rol: '{userRol}', Area: '{userArea}', CorrelationId: '{correlationId}'");

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            EXEC sp_set_session_context 'UserMail', @UserMail;
            EXEC sp_set_session_context 'UserRol', @UserRol;
            EXEC sp_set_session_context 'UserArea', @UserArea;
            EXEC sp_set_session_context 'CorrelationId', @CorrelationId;
        ";

        var pMail = cmd.CreateParameter();
        pMail.ParameterName = "@UserMail";
        pMail.Value = userMail ?? (object)DBNull.Value;
        cmd.Parameters.Add(pMail);

        var pRol = cmd.CreateParameter();
        pRol.ParameterName = "@UserRol";
        pRol.Value = userRol ?? (object)DBNull.Value;
        cmd.Parameters.Add(pRol);

        var pArea = cmd.CreateParameter();
        pArea.ParameterName = "@UserArea";
        pArea.Value = userArea ?? (object)DBNull.Value;
        cmd.Parameters.Add(pArea);

        var pCid = cmd.CreateParameter();
        pCid.ParameterName = "@CorrelationId";
        pCid.Value = Guid.TryParse(correlationId, out var cid) ? cid : Guid.NewGuid();
        cmd.Parameters.Add(pCid);

        cmd.ExecuteNonQuery();
    }

    private async Task SetSessionContextAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        string userMail = "system@nacionalseguros.com.bo";
        string userRol = "Administrador";
        string userArea = "";
        string correlationId = Guid.NewGuid().ToString();

        if (httpContext != null)
        {
            var userClaims = httpContext.User;
            userMail = userClaims?.FindFirst(ClaimTypes.Email)?.Value 
                       ?? userClaims?.FindFirst("email")?.Value 
                       ?? userMail;
            userRol = userClaims?.FindFirst(ClaimTypes.Role)?.Value 
                      ?? userClaims?.FindFirst("role")?.Value 
                      ?? userRol;
            userArea = userClaims?.FindFirst("area")?.Value 
                       ?? (httpContext.Request.Headers.ContainsKey("X-User-Area") ? httpContext.Request.Headers["X-User-Area"].ToString() : null) 
                       ?? userArea;
            correlationId = httpContext.Items["X-Correlation-ID"]?.ToString() ?? correlationId;
        }

        if (string.IsNullOrEmpty(userArea) && !string.IsNullOrEmpty(userMail))
        {
            string emailLower = userMail.ToLower();
            if (emailLower.StartsWith("admin")) userArea = "Tecnología (TI)";
            else if (emailLower.StartsWith("rrhh")) userArea = "Recursos Humanos";
            else if (emailLower.StartsWith("gerente")) userArea = "Gerencia General";
            else if (emailLower.StartsWith("comercial")) userArea = "Comercial";
            else if (emailLower.StartsWith("finanzas") || emailLower.StartsWith("alexander")) userArea = "Finanzas";
        }

        Console.WriteLine($"[SetSessionContext Async] Mail: '{userMail}', Rol: '{userRol}', Area: '{userArea}', CorrelationId: '{correlationId}'");

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            EXEC sp_set_session_context 'UserMail', @UserMail;
            EXEC sp_set_session_context 'UserRol', @UserRol;
            EXEC sp_set_session_context 'UserArea', @UserArea;
            EXEC sp_set_session_context 'CorrelationId', @CorrelationId;
        ";

        var pMail = cmd.CreateParameter();
        pMail.ParameterName = "@UserMail";
        pMail.Value = userMail ?? (object)DBNull.Value;
        cmd.Parameters.Add(pMail);

        var pRol = cmd.CreateParameter();
        pRol.ParameterName = "@UserRol";
        pRol.Value = userRol ?? (object)DBNull.Value;
        cmd.Parameters.Add(pRol);

        var pArea = cmd.CreateParameter();
        pArea.ParameterName = "@UserArea";
        pArea.Value = userArea ?? (object)DBNull.Value;
        cmd.Parameters.Add(pArea);

        var pCid = cmd.CreateParameter();
        pCid.ParameterName = "@CorrelationId";
        pCid.Value = Guid.TryParse(correlationId, out var cid) ? cid : Guid.NewGuid();
        cmd.Parameters.Add(pCid);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
