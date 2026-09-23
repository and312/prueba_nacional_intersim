using System;

namespace NacionalSeguros.Contracts.Catalogos;

public record CatalogoResponse(int CatalogoId, string Nombre, string Codigo, string CreatedBy, DateTime CreatedDate);
