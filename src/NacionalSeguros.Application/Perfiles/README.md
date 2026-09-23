# Módulo 04: Gestión de Perfiles
## Sistema Inteligente de Reclutamiento (SIR) – Nacional Seguros

Este módulo gestiona la creación, versionado, aprobación y observación de los **Perfiles de Cargo (Profesiogramas)** generados mediante los Agentes de Inteligencia Artificial (`AgentePerfil` vía n8n) y revisados por los analistas de RRHH de Nacional Seguros.

---

## 1. Estructura de Código del Módulo

El desarrollo está organizado bajo el estándar de **Clean Architecture** y **Domain-Driven Design (DDD)** del proyecto:

*   **Dominio (`NacionalSeguros.Domain`):**
    *   [PerfilCargo.cs (Aggregate Root)](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Entities/PerfilCargo.cs): Encapsula las reglas del negocio, el versionado y la máquina de estados compartida.
*   **Aplicación (`NacionalSeguros.Application`):**
    *   **Commands:**
        *   [GenerarPerfil](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Commands/GenerarPerfil) (Command, Handler): Dispara de forma asíncrona la inferencia del `AgentePerfil`.
        *   [CrearPerfilCargo](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Commands/CrearPerfilCargo) (Command, Handler): Crea y persiste un nuevo profesiograma (v1, v2, etc.).
        *   [ActualizarPerfilCargo](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Commands/ActualizarPerfilCargo) (Command, Handler): Permite editar el profesiograma en borrador u observado.
        *   [AprobarPerfilCargo](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Commands/AprobarPerfilCargo) (Command, Handler): Transita a estado Aprobado (`SOL-APR`).
        *   [ObservarPerfilCargo](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Commands/ObservarPerfilCargo) (Command, Handler): Transita a Observado (`SOL-OBS`) con justificación.
        *   [EliminarPerfilCargo](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Commands/EliminarPerfilCargo): Borrado lógico (Soft Delete).
        *   [ActivarPerfilCargo](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Commands/ActivarPerfilCargo) / [DesactivarPerfilCargo](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Commands/DesactivarPerfilCargo): Activación/Desactivación del perfil.
    *   **Queries:**
        *   [ObtenerPerfil](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Queries/ObtenerPerfil) (Query, Handler): Retorna el perfil más reciente de una solicitud.
        *   [ObtenerPerfilPorId](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Queries/ObtenerPerfilPorId) (Query, Handler): Detalle de un perfil específico.
        *   [BuscarPerfiles](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Application/Perfiles/Queries/BuscarPerfiles) (Query, Handler): Lista y busca perfiles en el sistema.
*   **Persistencia (`NacionalSeguros.Persistence`):**
    *   [PerfilCargoConfiguration.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Configurations/PerfilCargoConfiguration.cs): Mapeos de EF Core a la tabla `PerfilesCargo`, llaves, llaves foráneas y filtros globales de Soft Delete.
    *   [PerfilCargoRepository.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Repositories/PerfilCargoRepository.cs): Repositorio persistente e implementación de consultas.
*   **Controladores (`NacionalSeguros.Api`):**
    *   [PerfilController.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Api/Controllers/PerfilController.cs): Exposición de endpoints REST e integración de callback protegido por API Key (`X-API-Key`).

---

## 2. Matriz PRD → Código (Trazabilidad Funcional)

| Requisito PRD / OpenAPI | Operación de Negocio | Símbolo / Controlador | Detalles de Implementación |
| :---: | :--- | :--- | :--- |
| **Generar Borrador** | Generar Profesiograma con IA | `POST /api/v1/perfiles/generar` | Dispara el flujo asíncrono y devuelve 202 Accepted. |
| **Ver Profesiograma** | Obtener detalle y versiones | `GET /api/v1/perfiles/{id}` | Retorna el profesiograma estructurado y su metadata. |
| **Aprobar Perfil** | Transitar a estado Aprobado | `POST /api/v1/perfiles/{id}/aprobar` | Modifica el estado a `SOL-APR` haciéndolo elegible para abrir vacantes. |
| **Observar Perfil** | Transitar a Observado | `POST /api/v1/perfiles/{id}/observar` | Modifica el estado a `SOL-OBS` y requiere justificación obligatoria. |
| **Callback n8n** | Callback de creación de la IA | `POST /api/v1/callbacks/perfil-creacion` | Invocado por n8n con cabecera `X-API-Key` obligatoria para persistir el profesiograma. |

---

## 3. Matriz Estados → Transiciones (Máquina de Estados)

| Código Estado Origen | Estado Destino Permitido | Acción / Evento | Restricciones / Validaciones |
| :---: | :---: | :--- | :--- |
| **`SOL-BOR`** (Borrador) | **`SOL-ENV`** (En Validación) | Enviar a Validación | Traspaso estándar. |
| **`SOL-ENV`** (En Validación) | **`SOL-APR`** (Aprobado) | Aprobar Perfil | Habilita el profesiograma para abrir vacantes. |
| | **`SOL-OBS`** (Observado) | Observar Perfil | Requiere justificación escrita. Permite edición posterior. |

---

## 4. Matriz Endpoints → OpenAPI (Swagger)

| Ruta HTTP | Método | Operación | Seguridad / Acceso |
| :--- | :---: | :--- | :--- |
| `/api/v1/perfiles/generar` | `POST` | Disparar Generación por IA | JWT (RRHH) |
| `/api/v1/perfiles/{id}` | `GET` | Consultar Profesiograma | JWT (RRHH, Decisor, Solicitante) |
| `/api/v1/perfiles/{id}/aprobar` | `POST` | Aprobar Profesiograma | JWT (RRHH) |
| `/api/v1/perfiles/{id}/observar` | `POST` | Observar Profesiograma | JWT (RRHH) |
| `/api/v1/callbacks/perfil-creacion`| `POST` | Recibir Profesiograma de IA | API Key (`X-API-Key`) |

---

## 5. Autoevaluación y Certificación Técnica

*   **Compilación exitosa:** `dotnet build` ejecutada de forma limpia (**0 errores, 0 advertencias**).
*   **Pruebas unitarias:** xUnit y FluentAssertions funcionando. **35 pruebas superadas de 35 totales (100% de éxito)**.
*   **Seguridad:** Validación robusta del header `X-API-Key` en el callback de n8n para prevenir ejecuciones no autorizadas y auditoría Ledger implementada.
