using System;

namespace NacionalSeguros.Domain.Entities;

public class ApiKeyPermiso
{
    protected ApiKeyPermiso()
    {
    }

    public ApiKeyPermiso(long apiKeyId, int permisoId)
    {
        ApiKeyId = apiKeyId;
        PermisoId = permisoId;
    }

    public long ApiKeyId { get; private set; }
    public int PermisoId { get; private set; }

    // Navigation
    public ApiKey ApiKey { get; private set; } = null!;
    public Permiso Permiso { get; private set; } = null!;
}
