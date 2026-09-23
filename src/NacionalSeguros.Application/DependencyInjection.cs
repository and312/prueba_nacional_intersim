using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NacionalSeguros.Application.Behaviors;

namespace NacionalSeguros.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Registrar todos los validadores de FluentValidation del ensamblado
        services.AddValidatorsFromAssembly(assembly);

        // Registrar el pipeline de validación para MediatR
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
