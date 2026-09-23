using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Specifications;

public class CatalogosActivosSpecification : Specification<Catalogo>
{
    public CatalogosActivosSpecification()
        : base(c => !c.IsDeleted)
    {
        AddInclude(c => c.Parametros);
    }
}
