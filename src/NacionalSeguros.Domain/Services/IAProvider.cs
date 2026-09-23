using System.Threading;
using System.Threading.Tasks;

namespace NacionalSeguros.Domain.Services;

public record IAInferenceResult(
    string OutputText,
    int TokensInput,
    int TokensOutput,
    decimal CostoUSD);

public interface IIAProvider
{
    Task<IAInferenceResult> ProcessInferenceAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);
}
