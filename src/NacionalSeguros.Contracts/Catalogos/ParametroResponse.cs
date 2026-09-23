using System;

namespace NacionalSeguros.Contracts.Catalogos;

public record ParametroResponse(int ParametroId, int CatalogoId, string Codigo, string Valor, int? ParametroIdPadre, string CreatedBy, DateTime CreatedDate);
