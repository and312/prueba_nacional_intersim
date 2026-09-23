using System;
using System.Linq.Expressions;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Domain.Primitives;

namespace NacionalSeguros.Domain.Specifications;

public class ParametrosActivosSpecification : Specification<Parametro>
{
    public ParametrosActivosSpecification(int catalogoId)
        : base(p => !p.IsDeleted && p.CatalogoId == catalogoId)
    {
        AddInclude(p => p.Catalogo);
    }

    public ParametrosActivosSpecification(string catalogoCodigo)
        : base(p => !p.IsDeleted && p.Catalogo.Codigo == catalogoCodigo)
    {
        AddInclude(p => p.Catalogo);
    }
}
