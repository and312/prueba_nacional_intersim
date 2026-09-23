using System.Collections.Generic;
using System.Threading.Tasks;
using NacionalSeguros.Domain.Entities;

namespace NacionalSeguros.Domain.Repositories;

public interface IPostulanteRepository
{
    Task<Postulante?> GetByIdAsync(int id);
    Task<Postulante?> GetByCorreoAsync(string correo);
    Task<Postulacion?> GetPostulacionByIdsAsync(int postulanteId, int vacanteId);
    Task<Estado?> GetEstadoByCodigoAsync(string codigo);
    Task<Estado?> GetEstadoByIdAsync(int id);
    Task AddPostulanteAsync(Postulante postulante);
    Task AddPostulacionAsync(Postulacion postulacion);
    void UpdatePostulante(Postulante postulante);
    void UpdatePostulacion(Postulacion postulacion);
    Task<IEnumerable<Postulacion>> ListPostulacionesAsync(int? vacanteId);
    Task<Postulacion?> GetExpedienteDetailsAsync(int postulanteId);
}
