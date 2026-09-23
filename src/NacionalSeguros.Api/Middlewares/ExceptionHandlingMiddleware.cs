using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentValidation;
using NacionalSeguros.Shared.Exceptions;
using NacionalSeguros.Contracts.Security;

namespace NacionalSeguros.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        string correlationId = context.Items.TryGetValue("X-Correlation-ID", out var cid) ? cid?.ToString() ?? string.Empty : string.Empty;

        _logger.LogError(exception, "Ocurrió una excepción en el pipeline de la API. CorrelationId: {CorrelationId}", correlationId);

        context.Response.ContentType = "application/json";

        int statusCode = StatusCodes.Status500InternalServerError;
        string detail = "Ha ocurrido un error inesperado en el servidor.";
        string errorCode = "SERVER_ERROR";
        string responseDetail = exception.Message;

        if (exception is DomainException domainException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            detail = domainException.Message;
            errorCode = domainException.Code;
        }
        else if (exception is ValidationException validationException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            detail = string.Join(" ", validationException.Errors.Select(e => e.ErrorMessage));
            errorCode = "VALIDATION_ERROR";
        }
        else if (exception.GetType().Name == "SqlException" || exception.InnerException?.GetType().Name == "SqlException")
        {
            // Sanitización de excepciones de base de datos para prevenir fuga de metadatos (CWE-209)
            statusCode = StatusCodes.Status500InternalServerError;
            detail = "Ocurrió un error al procesar la solicitud en la base de datos. Los detalles técnicos han sido sanitizados por seguridad.";
            errorCode = "DATABASE_ERROR";
            responseDetail = "Los detalles técnicos han sido sanitizados por seguridad.";
        }
        else if (exception is BaseException baseException)
        {
            statusCode = StatusCodes.Status500InternalServerError;
            detail = baseException.Message;
            errorCode = "BASE_ERROR";
        }

        context.Response.StatusCode = statusCode;

        var responsePayload = new ApiErrorDto
        {
            Code = errorCode,
            Message = detail,
            Detail = responseDetail,
            CorrelationId = correlationId
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = null }; // Mantener PascalCase como pide el DTO o camelCase según convención
        var json = JsonSerializer.Serialize(responsePayload, options);
        await context.Response.WriteAsync(json);
    }
}
