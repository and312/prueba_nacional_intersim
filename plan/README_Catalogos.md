# Módulo 02 – Catálogos Maestros (SIR Backend)

Este módulo contiene el motor de gestión de **Catálogos Maestros (lookups dinámicos)**, **Parámetros Jerárquicos** y **Estados/SLAs** de las máquinas de estado del **Sistema Inteligente de Reclutamiento (SIR)** para Nacional Seguros.

---

## 1. Arquitectura y Componentes
Implementado bajo los principios de **Clean Architecture**, **Domain-Driven Design (DDD)** y **CQRS (MediatR)**:

- **NacionalSeguros.Domain:**
  - `Catalogo`: Agregado raíz que define la cabecera lógica de los catálogos maestros (ej. Canales de Reclutamiento, Modalidades de Trabajo).
  - `Parametro`: Entidad con soporte jerárquico recursivo (Padre/Hijos) para representar subdivisiones de negocio (ej. Vicepresidencia -> Gerencia -> Unidad).
  - `Estado`: Catálogo inmutable de solo lectura para las transiciones y SLAs en las máquinas de estado de Solicitudes, Vacantes y Postulantes.
  - `Sla`: Entidad que define los tiempos máximos permitidos (SLAs) para los procesos de reclutamiento por módulo.
  - `Specifications`: Consultas lógicas encapsuladas para cargar relaciones y parámetros activos (`CatalogoConParametrosSpecification`, `ParametrosActivosByCatalogoSpecification`).
  
- **NacionalSeguros.Contracts:**
  - Contratos de DTOs tipados estrictamente según la especificación OpenAPI (`CatalogoRequests`, `ParametroRequests`, `CatalogoResponse`, `ParametroResponse`, `EstadoResponse`, `SlaResponse`).

- **NacionalSeguros.Application:**
  - Casos de uso (`Commands` y `Queries`) para todas las mutaciones y lecturas de catálogos y parámetros.
  - Validadores robustos mediante **FluentValidation** para garantizar que los códigos cumplan con la nomenclatura y unicidad requerida.
  - Pipeline behavior de validación automática.
  - Prevención defensiva a nivel de aplicación para evitar que un parámetro sea asignado como padre de sí mismo.

- **NacionalSeguros.Infrastructure:**
  - `CatalogoCacheService`: Abstracción de caché que encapsula `IMemoryCache` de .NET 8, optimizando lecturas de lookups altamente recurrentes con expiración de 12 horas.
  - `DependencyInjection`: Registro del servicio de caché y su inicialización.

- **NacionalSeguros.Persistence:**
  - Mapeos Fluent API (`CatalogoConfiguration`, `ParametroConfiguration`, `EstadoConfiguration`, `SlaConfiguration`).
  - Filtros de consulta global (`HasQueryFilter`) en EF Core 9 para forzar la exclusión automática de registros borrados lógicamente (`IsDeleted = false`).
  - `CatalogoRepository`, `ParametroRepository`, `EstadoRepository` y `SlaRepository`.
  - Algoritmo defensivo iterativo contra dependencias circulares recursivas (`HasCircularDependencyAsync`) para evitar ciclos infinitos en el árbol.

- **NacionalSeguros.Api:**
  - `CatalogosController` que expone los endpoints en estricto cumplimiento con la interfaz OpenAPI/Swagger del proyecto.

---

## 2. Prevención de Dependencias Circulares
Para evitar que se introduzcan bucles infinitos en el árbol jerárquico de parámetros (ej. A -> B -> C -> A), se implementó un control en dos niveles:
1. **Validación del comando:** Si `request.ParametroIdPadre` coincide con el `Id` del parámetro a actualizar, se aborta de inmediato (`CIRCULAR_DEPENDENCY_DETECTED`).
2. **Validación recursiva en base de datos:** El repositorio `ParametroRepository` realiza un recorrido hacia arriba de los padres del nodo de forma iterativa y segura hasta un límite máximo de 10 niveles, deteniendo y rechazando la inserción/actualización si el ID actual se encuentra en la ruta.

---

## 3. Caché en Memoria e Invalidación Dinámica
El endpoint `/api/v1/config/catalogos?catalogoNombre=XXX` lee directamente de la memoria local a través de `ICatalogoCacheService` para acelerar los tiempos de respuesta.
- **Expiración por defecto:** 12 horas.
- **Invalidación:** Ante cualquier acción de mutación en la API (Crear, Actualizar o Eliminar lógicamente un catálogo o parámetro), se invoca preventivamente el método `InvalidateCacheAsync` usando el código de catálogo respectivo para forzar la recarga en la siguiente consulta.

---

## 4. Auditoría Ledger Transversal
Toda operación de creación, actualización y borrado lógico de catálogos y parámetros registra su estado y cambios (`estadoAnterior` y `estadoNuevo`) de forma inmutable mediante `IAuditService`, asociando el `CorrelationId` transversal de la petición HTTP.

---

## 5. Compilación y Pruebas

Para compilar y validar la suite de pruebas del módulo de catálogos:

```powershell
# Restaurar dependencias de la solución
dotnet restore

# Compilar todos los proyectos
dotnet build

# Ejecutar el arnés completo de pruebas (incluyendo tests de dependencias circulares y caché)
dotnet test
```
