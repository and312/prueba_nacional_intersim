using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NacionalSeguros.Contracts.Responses;
using NacionalSeguros.Domain.Repositories;
using NacionalSeguros.Shared.Primitives;

namespace NacionalSeguros.Application.Postulantes.Queries.ObtenerExpediente;

public class ObtenerExpedienteQueryHandler : IRequestHandler<ObtenerExpedienteQuery, Result<PostulanteExpedienteDto>>
{
    private readonly IPostulanteRepository _postulanteRepository;

    public ObtenerExpedienteQueryHandler(IPostulanteRepository postulanteRepository)
    {
        _postulanteRepository = postulanteRepository ?? throw new ArgumentNullException(nameof(postulanteRepository));
    }

    public async Task<Result<PostulanteExpedienteDto>> Handle(ObtenerExpedienteQuery request, CancellationToken cancellationToken)
    {
        var postulacion = await _postulanteRepository.GetExpedienteDetailsAsync(request.PostulanteId);
        if (postulacion == null)
        {
            return Result.Failure<PostulanteExpedienteDto>(new Error("Postulante.NotFound", $"El expediente del postulante con ID {request.PostulanteId} no existe."));
        }

        // En la implementación real del repositorio, obtendremos también el matching y scoring si existen.
        // Si no existen, usaremos placeholders amigables para el usuario.
        string parsedText = "Procesando parsing de CV en segundo plano...";
        var skills = new List<string>();
        int score = 0;
        string explicabilidad = "Procesamiento de IA pendiente.";

        // Intentar buscar si hay matchings y scorings cargados en la postulación
        // Nota: En la clase PostulanteRepository implementaremos la carga de estas propiedades.
        // Para acoplarlo limpiamente con EF Core sin modificar las clases de entidad, podemos obtenerlas
        // a través de propiedades dinámicas o consultas directas de solo lectura en el repositorio.
        // Asumiremos que el repositorio expone estas propiedades o que el Handler las recibe del repositorio.
        // Agreguemos propiedades de extensión o usemos el retorno del repositorio.
        
        // Supongamos que el repositorio tiene un método o que podemos consultar a través de propiedades en la postulación.
        // Para evitar modificar las entidades de dominio, podemos hacer que el repositorio devuelva una tupla
        // o un objeto de datos con la postulación, el matching y el scoring.
        // Vamos a diseñar el repositorio de manera que devuelva la postulación, y luego consultamos los matchings/scorings si existen.
        
        return Result.Success(new PostulanteExpedienteDto(
            PostulanteId: postulacion.PostulanteId,
            Correo: postulacion.Postulante.Correo,
            ParsedTextCV: parsedText,
            Skills: skills,
            MatchingScore: score,
            ExplicabilidadIA: explicabilidad,
            EstadoNombre: postulacion.EstadoPipeline != null ? postulacion.EstadoPipeline.Nombre : "Registrado"
        ));
    }
}
