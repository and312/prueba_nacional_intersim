using System.Threading;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Services;

namespace NacionalSeguros.Persistence.Services;

public class MockIAProvider : IIAProvider
{
    public Task<IAInferenceResult> ProcessInferenceAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        // Retornar un resultado de inferencia simulado e idéntico para propósitos de prueba y consistencia
        string jsonOutput = @"{
            ""Score"": 85.50,
            ""Coincidencias"": ""El candidato tiene una solida experiencia en desarrollo con .NET 8, C# y arquitecturas limpias, coincidiendo en un 90% con los requisitos tecnicos de la vacante."",
            ""Brechas"": ""Se identifica falta de experiencia liderando equipos de trabajo directamente, aunque posee capacidades tecnicas sobresalientes.""
        }";

        var result = new IAInferenceResult(
            OutputText: jsonOutput,
            TokensInput: 1250,
            TokensOutput: 420,
            CostoUSD: 0.0189m
        );

        return Task.FromResult(result);
    }
}
