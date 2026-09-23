using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;

namespace NacionalSeguros.Api.Filters;

public class IdempotentCallbackFilter : IAsyncActionFilter
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly IDistributedCache _cache;

    public IdempotentCallbackFilter(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationIdValues) ||
            string.IsNullOrEmpty(correlationIdValues.ToString()))
        {
            context.Result = new BadRequestObjectResult(new { Message = $"La cabecera {CorrelationIdHeader} es requerida para garantizar la idempotencia de la solicitud." });
            return;
        }

        string correlationId = correlationIdValues.ToString();
        string cacheKey = $"idempotency:{correlationId}";

        var cachedValue = await _cache.GetStringAsync(cacheKey);
        if (cachedValue is not null)
        {
            // Retorna un Ok descriptivo para evitar reprocesamiento redundante
            context.Result = new OkObjectResult(new { Message = "Callback ya procesado (Idempotente).", CorrelationId = correlationId });
            return;
        }

        var executedContext = await next();

        if (executedContext.Exception is null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            };
            await _cache.SetStringAsync(cacheKey, "PROCESSED", options);
        }
    }
}
