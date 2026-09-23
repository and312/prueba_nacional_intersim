using System;

namespace NacionalSeguros.Contracts.Catalogos;

public record EstadoResponse(int EstadoId, string Codigo, string Nombre, string Entidad, int? SLAId);
