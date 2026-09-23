# Plan de Implementación: Módulo 01 - Seguridad
## Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros

Este documento presenta el plan detallado para la implementación del **Módulo 01 – Seguridad**, asegurando consistencia con los requerimientos funcionales, el diseño de la base de datos de Nacional Seguros y los estándares de seguridad OWASP y Clean Architecture.

---

## User Review Required

> [!IMPORTANT]
> - **Autenticación Híbrida (Local vs Directorio Corporativo):** La base de datos tiene una restricción estricta (`CK_Usuarios_ClaveHash_AD`):
>   - Si `TipoAutenticacion` es `Local`, `ClaveHash` no debe ser nulo.
>   - Si `TipoAutenticacion` es `ActiveDirectory`, `ClaveHash` debe ser nulo.
>   Nuestra lógica de persistencia y servicios garantizará esta regla.
> - **Rotación de Refresh Tokens (RTR):** Implementaremos RTR en la tabla `Sesiones`. Al expirar un token o solicitar refresco, el token anterior pasa a `Activa = 0`.
>   > [!WARNING]
>   > Si se intenta reusar un token inactivo, se anularán inmediatamente todas las sesiones del usuario (`Activa = 0`) para mitigar el robo de tokens (`TOKEN_REUSE_DETECTED`).
> - **TOTP MFA:** Se requerirá MFA para los roles administrativos y de recursos humanos. Implementaremos la generación de claves TOTP y la validación en C# de forma nativa sin librerías externas mediante `HMACSHA1` para evitar dependencias innecesarias y cumplir estrictamente las directrices de seguridad.
> - **Sanitización de Errores de BD en Middleware:** El middleware de excepciones capturará cualquier `SqlException` o error de concurrencia y devolverá una estructura `ApiErrorDto` limpia con un `CorrelationId`, impidiendo fugas de metadatos del motor SQL Server 2022.

---

## Open Questions

- Ninguno. Seguiremos estrictamente el esquema de base de datos definido en [crear_tablas.sql](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/sql/01_TABLES/crear_tablas.sql) y [crear_constraints.sql](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/sql/02_CONSTRAINTS/crear_constraints.sql).

---

## Proposed Changes

### 1. Capa Domain
Definiremos las entidades, enumeraciones, especificaciones y eventos de dominio del módulo.

* **[NEW] [Entity.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Primitives/Entity.cs):** Extender con una versión genérica `Entity<TId>` para dar soporte a claves numéricas (`INT` y `BIGINT`) requeridas por el ERD físico.
* **[NEW] [TipoAutenticacion.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Enums/TipoAutenticacion.cs):** Enum con valores `Local` y `ActiveDirectory`.
* **[NEW] [UsuarioEstado.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Enums/UsuarioEstado.cs):** Enum con valores `Activo` e `Inactivo`.
* **[NEW] [Usuario.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Usuario.cs):** Entidad agregada raíz con propiedades asociadas a la tabla `Usuarios` (Nombre, Correo, ClaveHash, TipoAutenticacion, ActiveDirectoryId, Estado, MfaHabilitado, MfaSecreto).
* **[NEW] [Rol.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Rol.cs):** Entidad `Rol` mapeando a la tabla `Roles` (Nombre, Descripcion, IsDeleted).
* **[NEW] [Permiso.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Permiso.cs):** Entidad `Permiso` mapeando a la tabla `Permisos` (Codigo, Nombre, IsDeleted).
* **[NEW] [Sesion.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Sesion.cs):** Entidad `Sesion` mapeando a la tabla `Sesiones` (UsuarioId, RefreshToken, FechaExpiracion, Activa).
* **[NEW] [UsuarioCreadoEvent.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Events/UsuarioCreadoEvent.cs):** Evento disparado tras crear un usuario.
* **[NEW] [UsuarioConRolesSpecification.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Specifications/UsuarioConRolesSpecification.cs):** Especificación para cargar usuario con sus roles y permisos relacionados.

---

### 2. Capa Contracts
DTOs puros de entrada/salida para la API de seguridad según la especificación OpenAPI.

* **[NEW] [LoginRequestDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/LoginRequestDto.cs):** Request con correo, clave y tipo de autenticación.
* **[NEW] [LoginResponseDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/LoginResponseDto.cs):** Response con token JWT, tiempo de expiración, información del usuario y bandera `mfaRequerido`.
* **[NEW] [TokenRefreshRequestDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/TokenRefreshRequestDto.cs):** Request con JWT expirado y Refresh Token.
* **[NEW] [MfaVerifyRequestDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/MfaVerifyRequestDto.cs):** Request para verificar código TOTP OTP.
* **[NEW] [ChangePasswordRequestDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/ChangePasswordRequestDto.cs):** Request para cambiar contraseña local.
* **[NEW] [UsuarioCreateDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/UsuarioCreateDto.cs):** Request para registrar usuario.
* **[NEW] [UsuarioUpdateDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/UsuarioUpdateDto.cs):** Request para editar usuario.
* **[NEW] [UsuarioResponseDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/UsuarioResponseDto.cs):** Datos básicos expuestos del usuario.
* **[NEW] [RolResponseDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/RolResponseDto.cs):** DTO de respuesta para roles.
* **[NEW] [PagedUsuariosResponseDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Security/PagedUsuariosResponseDto.cs):** DTO paginado.

---

### 3. Capa Application
Casos de uso MediatR, interfaces lógicas de negocio, validaciones FluentValidation y perfiles AutoMapper.

* **[NEW] [IPasswordHasher.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Abstractions/Security/IPasswordHasher.cs):** Interfaz para hashing y verificación.
* **[NEW] [IJwtService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Abstractions/Security/IJwtService.cs):** Interfaz para generación y lectura de JWT.
* **[NEW] [IActiveDirectoryService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Abstractions/Security/IActiveDirectoryService.cs):** Interfaz para autenticación corporativa.
* **[NEW] [IMfaService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Abstractions/Security/IMfaService.cs):** Interfaz para TOTP MFA.
* **[NEW] [IEmailService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Abstractions/Notifications/IEmailService.cs):** Interfaz para notificaciones de seguridad.
* **[NEW] [IAuditService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Abstractions/Audit/IAuditService.cs):** Interfaz para inserción inmutable en `AuditLogs` (Ledger).
* **[NEW] [LoginCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/Login/LoginCommand.cs) / [LoginCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/Login/LoginCommandHandler.cs):** Valida credenciales locales o AD. Gestiona si requiere MFA. Si no requiere MFA, genera sesión y tokens.
* **[NEW] [LogoutCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/Logout/LogoutCommand.cs) / [LogoutCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/Logout/LogoutCommandHandler.cs):** Inactiva la sesión de forma física o lógica (`Activa = 0`).
* **[NEW] [RefreshTokenCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/RefreshToken/RefreshTokenCommand.cs) / [RefreshTokenCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/RefreshToken/RefreshTokenCommandHandler.cs):** Lógica RTR. Inactiva el token anterior, valida reuso y emite nuevos tokens.
* **[NEW] [CambiarPasswordCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/CambiarPassword/CambiarPasswordCommand.cs) / [CambiarPasswordCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/CambiarPassword/CambiarPasswordCommandHandler.cs):** Cambia clave de usuario validando la clave anterior (Solo local).
* **[NEW] [ConfigurarMfaCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/ConfigurarMfa/ConfigurarMfaCommand.cs) / [ConfigurarMfaCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/ConfigurarMfa/ConfigurarMfaCommandHandler.cs):** Habilita/genera el secreto TOTP para un usuario administrativo/RRHH.
* **[NEW] [VerificarMfaCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/VerificarMfa/VerificarMfaCommand.cs) / [VerificarMfaCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Commands/VerificarMfa/VerificarMfaCommandHandler.cs):** Valida el OTP para finalizar el inicio de sesión.
* **[NEW] [ObtenerUsuarioActualQuery.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Queries/ObtenerUsuarioActual/ObtenerUsuarioActualQuery.cs) / [ObtenerUsuarioActualQueryHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Queries/ObtenerUsuarioActual/ObtenerUsuarioActualQueryHandler.cs):** Retorna el usuario autenticado.
* **[NEW] [ObtenerPermisosQuery.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Queries/ObtenerPermisos/ObtenerPermisosQuery.cs) / [ObtenerPermisosQueryHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Queries/ObtenerPermisos/ObtenerPermisosQueryHandler.cs):** Lista permisos disponibles.
* **[NEW] [ObtenerRolesQuery.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Queries/ObtenerRoles/ObtenerRolesQuery.cs) / [ObtenerRolesQueryHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Queries/ObtenerRoles/ObtenerRolesQueryHandler.cs):** Lista roles disponibles.
* **[NEW] [Validators.cs]:** Validadores FluentValidation para cada comando de seguridad, aplicando la política de fortaleza de contraseñas (mínimo 8 caracteres, validación sintáctica de correo).
* **[NEW] [SecurityMappingProfile.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Security/Common/SecurityMappingProfile.cs):** Configuración de mapeo AutoMapper.

---

### 4. Capa Infrastructure
Implementación de los servicios de criptografía, tokens y auditoría.

* **[NEW] [PasswordHasher.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/Security/PasswordHasher.cs):** Algoritmo de hashing PBKDF2 nativo (`Rfc2898DeriveBytes`) con sal única por usuario y 100,000 iteraciones.
* **[NEW] [JwtService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/Security/JwtService.cs):** Genera e inspecciona JWT firmados usando la llave simétrica inyectada de forma dinámica en `Program.cs`.
* **[NEW] [MfaService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/Security/MfaService.cs):** Implementación nativa de RFC 6238 TOTP en C# mediante `HMACSHA1`. Generación de llaves Base32 e interpolación a URL de aprovisionamiento de Authenticator (QR).
* **[NEW] [ActiveDirectoryService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/Security/ActiveDirectoryService.cs):** Cliente LDAP nativo o simulador robusto en desarrollo que valida credenciales del directorio corporativo.
* **[NEW] [EmailService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/Notifications/EmailService.cs):** Envío ficticio de notificaciones/OTP con logs estructurados para no depender de pasarelas SMTP en desarrollo.
* **[NEW] [AuditService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/Audit/AuditService.cs):** Registra eventos de seguridad directamente en la tabla Ledger `AuditLogs`.

---

### 5. Capa Persistence
Mapeo de base de datos de EF Core 9 y soporte de consultas seguras.

* **[NEW] [Configurations/](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/):**
  - `UsuarioConfiguration.cs`: Mapeo de `Usuarios`, índices únicos, longitud de campos.
  - `RolConfiguration.cs`: Mapeo de `Roles`.
  - `PermisoConfiguration.cs`: Mapeo de `Permisos`.
  - `SesionConfiguration.cs`: Mapeo de `Sesiones`.
  - `AuditLogConfiguration.cs`: Mapeo de la tabla Ledger `AuditLogs` de solo inserción.

---

### 6. Capa API
Controladores HTTP que exponen la especificación oficial y middlewares de seguridad.

* **[NEW] [AuthController.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/AuthController.cs):** Controladores para login, logout, refresh, mfa y cambio de contraseña.
* **[NEW] [UserController.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/UserController.cs):** Gestión de usuarios (CRUD), asignación de roles y visualización de permisos.
* **[NEW] [AuditMiddleware.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Middlewares/AuditMiddleware.cs):** Middleware que intercepta solicitudes de mutación de datos (POST, PUT, DELETE) y genera registros en `AuditLogs` de forma automatizada.
* **[MODIFY] [Program.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Program.cs):** Registrar inyección de dependencias para los nuevos servicios e integrar los validadores de MediatR.

---

## Verification Plan

### Automated Tests
Se construirán pruebas automatizadas xUnit en `NacionalSeguros.Tests` para validar el funcionamiento del módulo:
- **`dotnet build`:** Garantizar que la solución compila con cero errores y warnings.
- **`dotnet test`:** Ejecutar el arnés de pruebas unitarias y de integración del módulo de seguridad.

Pruebas específicas a implementar:
1. **Password Hashing:** Verificar que dos contraseñas idénticas producen hashes distintos (debido a la sal aleatoria) y que la verificación es 100% exitosa.
2. **JWT Generation & Claims:** Verificar que el JWT generado contiene los roles del usuario, sus permisos y la fecha de expiración configurada.
3. **MFA TOTP Validation:** Probar que el generador TOTP valida códigos correctos y rechaza códigos expirados o incorrectos.
4. **RTR Session Rotation:** Validar el flujo de refresco de tokens exitoso y comprobar que un intento de reuso inactiva todas las sesiones del usuario y lanza una excepción de negocio.
5. **Restricción del Directorio Corporativo:** Validar que la base de datos (y EF Core) rechazan usuarios del directorio corporativo con clave local asignada (y viceversa).

### Manual Verification
- Levantar la API en localhost (`127.0.0.1:5000` / `https://127.0.0.1:5001`) y probar la autenticación y MFA utilizando una herramienta como Swagger o Postman.
- Validar que las transiciones de estado de sesión persisten correctamente en la tabla `Sesiones` de la base de datos local.

### Security Validation (Mandatory Checklist)
1. **Zero Hardcoded Secrets:** Confirmar que la firma JWT utiliza la resolución dinámica del token implementada en el Módulo 00.
2. **Data Sanitization:** Asegurar que ninguna consulta a la base de datos concatena strings para evitar inyección SQL (CWE-89).
3. **No Sensitive Logging:** Verificar que los payloads de log omitan la clave del usuario o los secretos MFA para evitar fuga de credenciales.
4. **Localhost Binding:** El servidor de desarrollo escuchará estrictamente en `localhost` / `127.0.0.1` y no en `0.0.0.0` (CWE-200 compliance).
