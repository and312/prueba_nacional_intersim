# Módulo 01 – Seguridad (SIR Backend)

Este módulo contiene el sistema completo de Autenticación, Autorización y Auditoría del **Sistema Inteligente de Reclutamiento (SIR)** para Nacional Seguros.

---

## 1. Arquitectura y Componentes
Implementado bajo los principios de **Clean Architecture**, **Domain-Driven Design (DDD)** y **CQRS (MediatR)**:

- **NacionalSeguros.Domain:**
  - `Usuario`: Agregado raíz que controla el estado del usuario local y el directorio corporativo.
  - `Rol` & `Permiso`: Modelos RBAC y claims granulares.
  - `Sesion`: Almacén de Refresh Tokens activos en la tabla `Sesiones`.
  - `Specification`: Primitivas de consultas filtradas (`UsuarioConRolesSpecification`).
  - `DomainEvents`: Trazabilidad a través de eventos (`UsuarioCreadoEvent`).
  
- **NacionalSeguros.Contracts:**
  - Contratos de DTOs tipados estrictamente según la especificación OpenAPI (Login, MFA, Password Change, User CRUD).

- **NacionalSeguros.Application:**
  - Casos de uso (`Commands` / `Queries`) para todas las acciones de autenticación e incidentes.
  - Validadores automáticos mediante **FluentValidation** (Password Policy, Email rules).
  - Pipeline behavior de validación (`ValidationBehavior.cs`).

- **NacionalSeguros.Infrastructure:**
  - `PasswordHasher`: Hashing PBKDF2 nativo en .NET 8 con sal única y 100k iteraciones.
  - `JwtService`: Generación de JWT firmados y validación segura.
  - `MfaService`: Implementación nativa de **TOTP RFC 6238** (HMAC-SHA1) con ventana de tolerancia de 90 segundos.
  - `ActiveDirectoryService`: Cliente LDAP integrado para inicio híbrido corporativo.
  - `AuditService`: Registro automatizado inmutable en `AuditLogs` (Ledger table).

- **NacionalSeguros.Persistence:**
  - Mapeos Fluent API.
  - `UsuarioRepository`, `SesionRepository`, `RolRepository` y `PermisoRepository` con Entity Framework Core 9.

- **NacionalSeguros.Api:**
  - `AuthController` & `UserController` exponiendo endpoints.
  - `AuditMiddleware`: Captura automática de mutaciones de datos (POST, PUT, DELETE).
  - `ExceptionHandlingMiddleware`: Sanitización fina de errores e inyección del `CorrelationId`.

---

## 2. Configuración (`appsettings.json`)

Configure los siguientes parámetros en su archivo de configuración o Secrets Manager:

```json
{
  "Jwt": {
    "Secret": "SU_JWT_SECRET_COMPARTIDO_DE_AL_MENOS_32_BYTES",
    "Issuer": "NacionalSeguros.SIR",
    "Audience": "NacionalSeguros.SIR.Clients"
  },
  "ActiveDirectory": {
    "Simulate": true,
    "LdapServer": "nacionalvida.corp",
    "LdapPort": 389
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SIR_NacionalSeguros;User Id=sa;Password=SuPasswordSQL2022!;TrustServerCertificate=True;"
  }
}
```

> [!TIP]
> **Modo Simulación AD:** Si `"ActiveDirectory:Simulate"` está en `true`, se aceptará cualquier credencial del directorio corporativo de al menos 8 caracteres para agilizar pruebas en desarrollo local sin requerir un Domain Controller LDAP.

---

## 3. Seguridad Clave Implementada

1. **Prevención de Inyección SQL (CWE-89):** Todo el acceso a datos se procesa parametrizado mediante EF Core 9 y Dapper.
2. **Rotación de Refresh Tokens (RTR):** Los tokens de refresco son de un solo uso. Si se detecta un intento de reuso de un token inactivo (posible robo), la API anula automáticamente todas las sesiones del usuario afectado (`TOKEN_REUSE_DETECTED`) y retorna HTTP 403.
3. **MFA Obligatorio:** TOTP obligatorio para Administradores y Recursos Humanos. El login inicial responde `mfaRequerido: true`.
4. **Sanitización de Datos de Logs y Ledger:** `AuditService` detecta campos salariales o contraseñas en payloads de base de datos antes de registrar las auditorías y enmascara su contenido como `[DATOS ENMASCARADOS POR SEGURIDAD]`.

---

## 4. Compilación y Pruebas

Para validar el módulo completo, ejecute desde la raíz del proyecto en PowerShell:

```powershell
# Restaurar dependencias
dotnet restore

# Compilar proyecto en modo Release sin warnings tratados como errores activos
dotnet build

# Ejecutar el arnés de pruebas de seguridad
dotnet test
```
