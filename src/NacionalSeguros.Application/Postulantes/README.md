# Módulo 06: Gestión de Postulantes
## Sistema Inteligente de Reclutamiento (SIR) – Nacional Seguros

Este módulo gestiona la administración de candidatos, el registro de postulaciones a vacantes activas, el pipeline de selección del postulante y la consolidación de su expediente de evaluación (matching y scoring de IA).

---

## 1. Estructura de Código del Módulo

El desarrollo está organizado bajo el estándar de **Clean Architecture** y **Domain-Driven Design (DDD)** del proyecto:

*   **Dominio (`NacionalSeguros.Domain`):**
    *   [Postulante.cs (Aggregate Root)](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Postulante.cs): Información personal básica del candidato.
    *   [Postulacion.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/Postulacion.cs): Representa la postulación del candidato a una vacante y la máquina de estados en el pipeline de selección.
    *   [IPostulanteRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Repositories/IPostulanteRepository.cs): Interfaz del repositorio de acceso a datos para postulantes y postulaciones.
    *   [PostulanteEvents.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Events/PostulanteEvents.cs): Eventos de dominio (`PostulanteRegistradoEvent`, `PostulanteEstadoTransitadoEvent`).
*   **Contratos (`NacionalSeguros.Contracts`):**
    *   [PostulanteRequests.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Requests/PostulanteRequests.cs): DTO de petición para cambios de estado.
    *   [PostulanteResponseDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Responses/PostulanteResponseDto.cs): DTO de respuesta básica para listados.
    *   [PostulanteExpedienteDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Responses/PostulanteExpedienteDto.cs): DTO detallado para el expediente del candidato.
    *   [PagedPostulantesResponseDto.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Contracts/Responses/PagedPostulantesResponseDto.cs): DTO de respuesta paginada.
*   **Aplicación (`NacionalSeguros.Application`):**
    *   **Commands:**
        *   `RegistrarPostulante` (Command, Handler, Validator): Registra un postulante con placeholders (para satisfacer columnas NOT NULL de base de datos) y crea su postulación en estado `POS-REG` (Registrado).
        *   `ActualizarPostulante` (Command, Handler): Permite actualizar los nombres, apellidos y documento de identidad del postulante tras el procesamiento asíncrono del CV.
        *   `TransitarPostulante` (Command, Handler): Transita el estado del postulante en el pipeline.
    *   **Queries:**
        *   `ObtenerExpediente` (Query, Handler): Consulta el expediente consolidado.
        *   `ListarPostulantes` (Query, Handler): Obtiene la lista de postulantes de forma paginada y filtrada.
    *   **Mappings:**
        *   `PostulanteMappingProfile`: Configuración de AutoMapper.
*   **Persistencia (`NacionalSeguros.Persistence`):**
    *   [PostulanteConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/PostulanteConfiguration.cs) / [PostulacionConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/PostulacionConfiguration.cs): Mapeos Fluent API, relaciones físicas y filtros de Soft Delete.
    *   [PostulanteRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Repositories/PostulanteRepository.cs): Implementación física del repositorio.
*   **Controladores (`NacionalSeguros.Api`):**
    *   [PostulanteController.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/PostulanteController.cs): Exposición de endpoints REST con soporte de multipart/form-data.

---

## 2. Compilación y Pruebas

Para compilar y validar la suite de pruebas del módulo de postulantes:

```powershell
# Compilar todos los proyectos
dotnet build

# Ejecutar el arnés completo de pruebas (incluyendo tests de postulantes)
dotnet test
```
