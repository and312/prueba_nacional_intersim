namespace NacionalSeguros.Contracts.Catalogos;

public record MasterDataCreateRequest(string Codigo, string Nombre, string? Descripcion);

public record MasterDataUpdateRequest(string Nombre, string? Descripcion, string Estado);

public record MasterDataResponse(int Id, string Codigo, string Nombre, string? Descripcion, string Estado, bool IsDeleted);

public record CargoCreateRequest(int AreaCargoId, string Codigo, string Nombre, string? Descripcion);

public record CargoUpdateRequest(int AreaCargoId, string Nombre, string? Descripcion, string Estado);

public record CargoDataResponse(int Id, int AreaCargoId, string AreaCargoNombre, string Codigo, string Nombre, string? Descripcion, string Estado, bool IsDeleted);

