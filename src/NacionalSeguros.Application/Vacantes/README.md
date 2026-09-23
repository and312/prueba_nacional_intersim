# Módulo 05: Gestión de Vacantes
## Sistema Inteligente de Reclutamiento (SIR) – Nacional Seguros

Este módulo gestiona la creación, publicación, cierre y cancelación de las **Vacantes** en el sistema, integrándose con las solicitudes y los perfiles de cargo aprobados.

---

## 1. Estructura de Código del Módulo

El desarrollo está organizado bajo el estándar de **Clean Architecture** y **Domain-Driven Design (DDD)** del proyecto:

*   **Dominio (`NacionalSeguros.Domain`):**
    *   [Vacante.cs (Aggregate Root)](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Vacante.cs): Encapsula las reglas de negocio, validaciones y la máquina de estados de las vacantes.
    *   [IVacanteRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/IVacanteRepository.cs): Definición de los métodos de acceso a datos para Vacantes.
    *   [VacanteEvents.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Events/VacanteEvents.cs): Eventos de dominio (`VacanteCreadaEvent`, `VacantePublicadaEvent`, `VacanteCerradaEvent`).
*   **Contratos (`NacionalSeguros.Contracts`):**
    *   [VacanteRequests.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Requests/VacanteRequests.cs): DTO de petición `VacanteCreateDto`.
    *   [VacanteResponseDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Responses/VacanteResponseDto.cs): DTO de respuesta estructurada.
    *   [PagedVacantesResponseDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Responses/PagedVacantesResponseDto.cs): DTO de respuesta paginada.
*   **Aplicación (`NacionalSeguros.Application`):**
    *   **Commands:**
        *   `CrearVacante` (Command, Handler, Validator): Registra una vacante en estado `VAC-CRE` basándose en una solicitud y perfil aprobados.
        *   `PublicarVacante` (Command, Handler): Transita la vacante a `VAC-PUB` y dispara la lógica de sourcing externa.
        *   `CerrarVacante` (Command, Handler): Transita la vacante a `VAC-CON` (Contratada) al seleccionarse un postulante.
        *   `CancelarVacante` (Command, Handler): Transita la vacante a `VAC-CER` (Cerrada/Cancelada) requiriendo un código de motivo.
    *   **Queries:**
        *   `ObtenerVacantePorId` (Query, Handler): Consulta el detalle de una vacante por ID.
        *   `BuscarVacantes` (Query, Handler): Lista vacantes con filtros y paginación.
    *   **Mappings:**
        *   `VacanteMappingProfile`: Configuración de AutoMapper.
*   **Persistencia (`NacionalSeguros.Persistence`):**
    *   [VacanteConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/VacanteConfiguration.cs): Mapeo de EF Core a la tabla `Vacantes`, llaves foráneas y filtros automáticos de Soft Delete.
    *   [VacanteRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Repositories/VacanteRepository.cs): Repositorio persistente e implementación de consultas.
*   **Controladores (`NacionalSeguros.Api`):**
    *   [VacanteController.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/VacanteController.cs): Exposición de los endpoints REST con seguridad JWT y roles RBAC (RRHH, Reclutador, Decisor).

---

## 2. Matriz de Estados y Transiciones

| Código Estado Origen | Estado Destino Permitido | Acción / Evento | Restricciones / Validaciones |
| :---: | :---: | :--- | :--- |
| **`VAC-CRE`** (Creada) | **`VAC-PUB`** (Publicada) | Publicar Vacante | Requiere que el perfil de cargo esté en estado Aprobado. |
| **`VAC-PUB`** (Publicada) | **`VAC-CON`** (Contratada) | Cerrar Vacante | Posición cubierta con candidato. Detiene el SLA. |
| | **`VAC-CER`** (Cerrada) | Cancelar Vacante | Cierre sin cubrir. Requiere registrar código de motivo. |

---

## 3. Compilación y Pruebas

Para compilar y validar la suite de pruebas del módulo de vacantes:

```powershell
# Compilar todos los proyectos
dotnet build

# Ejecutar el arnés completo de pruebas (incluyendo tests de vacantes)
dotnet test
```
