# Plan de Implementación: Módulo 02 - Catálogos Maestros
## Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros

Este documento presenta el plan detallado para la implementación del **Módulo 02 – Catálogos Maestros**, asegurando consistencia con los requerimientos funcionales, el diseño de la base de datos de Nacional Seguros (SQL Server 2022) y los principios de Clean Architecture, DDD, CQRS y observabilidad.

---

## User Review Required

> [!IMPORTANT]
> - **Prevención de Dependencias Circulares:** Implementaremos validaciones defensivas a nivel de repositorio y comando para evitar ciclos infinitos en el árbol jerárquico de parámetros (ej. que una Unidad apunte a una Gerencia y viceversa).
> - **Estrategia de Caché:** Dado que los datos maestros tienen una tasa de cambio extremadamente baja, se implementará un servicio de caché en memoria (`IMemoryCache`) para optimizar el endpoint de consulta `/config/catalogos` y evitar accesos redundantes a base de datos. La caché se invalidará ante cualquier mutación de catálogo o parámetro.
> - **Auditoría Ledger Transversal:** Toda operación de mutación (Creación, Edición, Eliminación) de Catálogos y Parámetros registrará una traza inmutable en `AuditLogs` (Ledger table), capturando el `CorrelationId` de la transacción. Las lecturas no se auditarán para evitar sobrecarga.

---

## Open Questions

> [!NOTE]
> - **Restricción de Borrado Físico:** De acuerdo con la directriz de Soft Delete transversal, no se realizarán sentencias `DELETE` físicas en la base de datos para `Catalogos` o `Parametros`. Se utilizará la columna `IsDeleted` (Soft Delete) configurada mediante Query Filters automáticos en EF Core.
> - **Entidad Estado:** La tabla física `Estados` no contiene columnas de auditoría estándar (`CreatedBy`, `CreatedDate`, `IsDeleted`). Por lo tanto, la entidad `Estado` y su configuración de EF se implementarán sin heredar ni mapear campos de auditoría ni soft-delete.

---

## Proposed Changes

### 1. Capa Domain (`NacionalSeguros.Domain`)
Definiremos las entidades, enumeraciones, especificaciones y eventos de dominio del módulo.

* #### [NEW] [Catalogo.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Catalogo.cs)
  Entidad agregada raíz mapeando a la tabla `Catalogos`. Hereda de `Entity<int>`. Contiene Nombre, Codigo y campos de auditoría estándar (`CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`, `DeletedBy`, `DeletedDate`, `IsDeleted`). Expone la colección de navegación `IReadOnlyCollection<Parametro> Parametros`.
* #### [NEW] [Parametro.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Parametro.cs)
  Entidad mapeando a la tabla `Parametros`. Hereda de `Entity<int>`. Contiene `CatalogoId`, `Codigo`, `Valor`, `ParametroIdPadre` y campos de auditoría estándar. Define navegación `Catalogo`, `Parametro? Padre` y la colección de hijos `IReadOnlyCollection<Parametro> Hijos`.
* #### [NEW] [Estado.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Estado.cs)
  Entidad de solo lectura mapeando a la tabla `Estados`. Hereda de `Entity<int>`. Contiene `Codigo`, `Nombre`, `Entidad` y `SLAId` (FK opcional).
* #### [NEW] [Events.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Events/CatalogoEvents.cs)
  Mapea los eventos del dominio para desacoplamiento y auditoría asíncrona: `CatalogoCreadoEvent`, `ParametroCreadoEvent`, `CatalogoActualizadoEvent`, `ParametroActualizadoEvent`, `CatalogoEliminadoEvent`, `ParametroEliminadoEvent`.
* #### [NEW] [Specifications.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Specifications/CatalogosSpecifications.cs)
  Especificaciones para encapsular consultas lógicas:
  - `CatalogoConParametrosSpecification`: Carga un catálogo junto con sus parámetros activos.
  - `ParametrosActivosByCatalogoSpecification`: Obtiene los parámetros activos de un catálogo específico.
* #### [NEW] [ICatalogoRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/ICatalogoRepository.cs)
  Definición de repositorio para operaciones sobre `Catalogo`.
* #### [NEW] [IParametroRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/IParametroRepository.cs)
  Definición de repositorio para `Parametro`, incluyendo el contrato `HasCircularDependencyAsync`.
* #### [NEW] [IEstadoRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/IEstadoRepository.cs)
  Definición de repositorio para `Estado`.

---

### 2. Capa Contracts (`NacionalSeguros.Contracts`)
DTOs puros de entrada/salida para la API de catálogos.

* #### [NEW] [CatalogoRequests.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Catalogos/CatalogoRequests.cs)
  Define `CrearCatalogoRequest` y `ActualizarCatalogoRequest`.
* #### [NEW] [ParametroRequests.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Catalogos/ParametroRequests.cs)
  Define `CrearParametroRequest` y `ActualizarParametroRequest`.
* #### [NEW] [CatalogoResponse.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Catalogos/CatalogoResponse.cs)
  DTO de respuesta con datos del catálogo.
* #### [NEW] [ParametroResponse.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Catalogos/ParametroResponse.cs)
  DTO de respuesta con datos del parámetro.
* #### [NEW] [EstadoResponse.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Catalogos/EstadoResponse.cs)
  DTO de respuesta con datos del estado.

---

### 3. Capa Application (`NacionalSeguros.Application`)
Casos de uso MediatR, interfaces lógicas de negocio, validaciones FluentValidation y perfiles AutoMapper.

* #### [NEW] [ICatalogoCacheService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Abstractions/Cache/ICatalogoCacheService.cs)
  Interfaz para la invalidación y obtención de parámetros cacheados.
* #### [NEW] [CatalogosMappingProfile.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Common/CatalogosMappingProfile.cs)
  Mapeos AutoMapper para Módulo 02.
* #### [NEW] [CrearCatalogoCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/CrearCatalogo/CrearCatalogoCommand.cs) / [CrearCatalogoCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/CrearCatalogo/CrearCatalogoCommandHandler.cs)
  Command y Handler para crear un catálogo.
* #### [NEW] [ActualizarCatalogoCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/ActualizarCatalogo/ActualizarCatalogoCommand.cs) / [ActualizarCatalogoCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/ActualizarCatalogo/ActualizarCatalogoCommandHandler.cs)
  Command y Handler para editar un catálogo.
* #### [NEW] [EliminarCatalogoCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/EliminarCatalogo/EliminarCatalogoCommand.cs) / [EliminarCatalogoCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/EliminarCatalogo/EliminarCatalogoCommandHandler.cs)
  Command y Handler para la inactivación lógica (soft-delete) de un catálogo y todos sus parámetros relacionados.
* #### [NEW] [CrearParametroCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/CrearParametro/CrearParametroCommand.cs) / [CrearParametroCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/CrearParametro/CrearParametroCommandHandler.cs)
  Command y Handler para agregar un parámetro. Valida dependencias jerárquicas y previene recursiones. Invalida caché.
* #### [NEW] [ActualizarParametroCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/ActualizarParametro/ActualizarParametroCommand.cs) / [ActualizarParametroCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/ActualizarParametro/ActualizarParametroCommandHandler.cs)
  Command y Handler para editar un parámetro. Valida recursión jerárquica e invalida caché.
* #### [NEW] [EliminarParametroCommand.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/EliminarParametro/EliminarParametroCommand.cs) / [EliminarParametroCommandHandler.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Commands/EliminarParametro/EliminarParametroCommandHandler.cs)
  Command y Handler para inactivación lógica de un parámetro e hijos recursivamente. Invalida caché.
* #### [NEW] [Queries.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Queries/)
  Implementación de queries CQRS:
  - `ObtenerCatalogosQuery` / `ObtenerCatalogosQueryHandler`
  - `ObtenerCatalogoPorIdQuery` / `ObtenerCatalogoPorIdQueryHandler`
  - `ObtenerDetallesQuery` / `ObtenerDetallesQueryHandler`
  - `BuscarCatalogosQuery` / `BuscarCatalogosQueryHandler`
* #### [NEW] [Validators.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Catalogos/Validators/)
  FluentValidation para cada comando, asegurando fortaleza en la nomenclatura de códigos y unicidad semántica.

---

### 4. Capa Persistence (`NacionalSeguros.Persistence`)
Mapeo de Entity Framework Core 9 y repositorios.

* #### [MODIFY] [ApplicationDbContext.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Context/ApplicationDbContext.cs)
  Registrar `DbSet<Catalogo>`, `DbSet<Parametro>` y `DbSet<Estado>`.
* #### [NEW] [Configurations/CatalogoConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/CatalogoConfiguration.cs)
  Mapeo fluido de `Catalogos`, configurando soft-delete automático (`HasQueryFilter(c => !c.IsDeleted)`).
* #### [NEW] [Configurations/ParametroConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/ParametroConfiguration.cs)
  Mapeo fluido de `Parametros`, configurando la relación recursiva (`ParametroIdPadre` -> `ParametroId`) y soft-delete filter.
* #### [NEW] [Configurations/EstadoConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/EstadoConfiguration.cs)
  Mapeo fluido de `Estados` (sin soft-delete y sin columnas de auditoría).
* #### [NEW] [Repositories/](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Repositories/)
  Implementaciones físicas de repositorios:
  - `CatalogoRepository.cs`
  - `ParametroRepository.cs` (incluye la recursión iterativa segura contra ciclos infinitos)
  - `EstadoRepository.cs`
* #### [MODIFY] [DependencyInjection.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/DependencyInjection.cs)
  Registrar los nuevos repositorios para inyección.

---

### 5. Capa Infrastructure (`NacionalSeguros.Infrastructure`)
Implementación de los servicios de caché en memoria.

* #### [NEW] [CatalogoCacheService.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/Cache/CatalogoCacheService.cs)
  Implementación usando `IMemoryCache` de .NET para agilizar la obtención de lookups dinámicos.
* #### [MODIFY] [DependencyInjection.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Infrastructure/DependencyInjection.cs)
  Registrar la inyección de `IMemoryCache` y `ICatalogoCacheService`.

---

### 6. Capa API (`NacionalSeguros.Api`)
Controlador REST que expone los recursos y mantiene el contrato con OpenAPI/Swagger.

* #### [NEW] [CatalogosController.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/CatalogosController.cs)
  Expone endpoints HTTP:
  - `GET /api/v1/config/catalogos?catalogoNombre=XXX` (OpenAPI de lookups dinámicos, usa caché)
  - `GET /api/v1/catalogos` (listar catálogos con filtros de búsqueda y paginación)
  - `GET /api/v1/catalogos/{id}` (obtener por ID)
  - `POST /api/v1/catalogos` (crear catálogo)
  - `PUT /api/v1/catalogos/{id}` (actualizar catálogo)
  - `DELETE /api/v1/catalogos/{id}` (inactivar catálogo)
  - `GET /api/v1/catalogos/{catalogoId}/detalles` (obtener detalles / parámetros)
  - `POST /api/v1/catalogos/{catalogoId}/detalles` (crear detalle / parámetro)
  - `PUT /api/v1/catalogos/detalles/{id}` (actualizar detalle / parámetro)
  - `DELETE /api/v1/catalogos/detalles/{id}` (eliminar detalle / parámetro)
  - `GET /api/v1/config/slas` (consultar SLAs parametrizados)

---

## Verification Plan

### Automated Tests
Se agregarán pruebas automatizadas xUnit en `NacionalSeguros.Tests` para validar el funcionamiento del módulo:
- **`dotnet build`:** Garantizar que la solución compila con cero errores y warnings.
- **`dotnet test`:** Validar el comportamiento de las dependencias circulares, el soft-delete recursivo y el almacenamiento en caché.

Pruebas específicas a implementar:
1. **Circular Dependency Verification:** Intentar crear un parámetro que apunte a sí mismo o que forme un ciclo jerárquico cerrado (ej. A -> B -> C -> A), verificando que la base de datos o el handler lance `CIRCULAR_DEPENDENCY_DETECTED`.
2. **Caching & Invalidation:** Probar que la obtención secuencial de parámetros lee del caché en memoria y que la edición de un parámetro invalida automáticamente la caché para reflejar el cambio.
3. **Soft Delete Cascade:** Comprobar que al inactivar lógicamente un catálogo o parámetro padre, todos sus parámetros dependientes se marcan como `IsDeleted = true`.
4. **Validation Rules:** Verificar que códigos vacíos o duplicados en catálogos y parámetros sean rechazados por FluentValidation.

### Manual Verification
- Invocar los endpoints `/api/v1/config/catalogos` y comprobar la respuesta DTO estructurada.
- Verificar a nivel de base de datos que las modificaciones en los catálogos y parámetros insertan filas correctas en la tabla `AuditLogs`.
