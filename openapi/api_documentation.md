# Documentación Complementaria de APIs: Nacional Seguros (Fase 1)

Este documento complementa el contrato [openapi.yaml](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/openapi/openapi.yaml) y detalla las matrices de acceso, catálogos de DTOs, flujos de estados, errores e integración asíncrona con n8n del **Sistema Inteligente de Reclutamiento de Nacional Seguros**.

---

## 1. Inventario Completo de Endpoints

La API expone los siguientes endpoints estructurados bajo la versión `/api/v1/`:

| Método | Ruta | Descripción | Autenticación | Roles Autorizados (RBAC) |
| :---: | :--- | :--- | :---: | :--- |
| **POST** | `/auth/login` | Login híbrido local y AD. | No | Todos |
| **POST** | `/auth/logout` | Cierre de sesión e invalidación del token. | JWT | Todos |
| **POST** | `/auth/refresh` | Renovación de token (RTR). | No | Todos (Vía Cookie / Payload) |
| **POST** | `/auth/mfa` | Verificación de OTP. | No | Todos |
| **POST** | `/auth/change-password` | Cambio de contraseña local. | JWT | Todos (Excepto AD) |
| **GET** | `/usuarios` | Listar usuarios parametrizados. | JWT | Administrador |
| **POST** | `/usuarios` | Crear usuario. | JWT | Administrador |
| **GET** | `/usuarios/{id}` | Detalles de usuario. | JWT | Administrador |
| **PUT** | `/usuarios/{id}` | Actualizar datos básicos de usuario. | JWT | Administrador |
| **DELETE**| `/usuarios/{id}` | Inactivación lógica del usuario. | JWT | Administrador |
| **GET** | `/usuarios/{id}/roles` | Obtener roles del usuario. | JWT | Administrador |
| **PUT** | `/usuarios/{id}/roles` | Sobrescribir roles (RBAC). | JWT | Administrador |
| **POST** | `/solicitudes` | Crear solicitud de personal. | JWT | Solicitante, RRHH |
| **GET** | `/solicitudes` | Listar solicitudes con filtros. | JWT | Solicitante, Decisor, RRHH |
| **GET** | `/solicitudes/{id}` | Detalle de solicitud. | JWT | Solicitante, Decisor, RRHH |
| **PUT** | `/solicitudes/{id}` | Editar solicitud en borrador. | JWT | Solicitante, RRHH |
| **POST** | `/solicitudes/{id}/aprobar` | Aprobar solicitud. Transita a 'Aprobada'. | JWT | Decisor, RRHH |
| **POST** | `/solicitudes/{id}/rechazar`| Rechazar solicitud. Transita a 'Rechazada'. | JWT | Decisor, RRHH |
| **GET** | `/solicitudes/{id}/historial`| Consultar historial de estados. | JWT | Solicitante, Decisor, RRHH |
| **GET** | `/solicitudes/{solicitudId}/perfil`| Obtener perfil más reciente de una solicitud. | JWT | RRHH, Decisor, Solicitante |
| **POST** | `/perfiles/generar` | Disparar generación asíncrona por IA. | JWT | RRHH |
| **GET** | `/perfiles` | Buscar y listar perfiles con filtros. | JWT | RRHH, Decisor, Solicitante |
| **GET** | `/perfiles/{id}` | Consultar perfil y sus versiones. | JWT | RRHH, Decisor, Solicitante |
| **POST** | `/perfiles/{id}/aprobar`| Aprobar profesiograma. | JWT | RRHH |
| **POST** | `/perfiles/{id}/observar`| Observar y devolver perfil a edición. | JWT | RRHH |
| **POST** | `/vacantes` | Abrir vacante a partir de solicitud. | JWT | RRHH |
| **GET** | `/vacantes` | Consultar listado de vacantes activas. | JWT | RRHH, Reclutador, Decisor |
| **POST** | `/vacantes/{id}/publicar`| Publicar vacante en LinkedIn y portales. | JWT | Reclutador, RRHH |
| **POST** | `/vacantes/{id}/cerrar` | Cerrar vacante y seleccionar ganador. | JWT | RRHH |
| **POST** | `/vacantes/{id}/cancelar`| Cancelar vacante. Requiere código de motivo.| JWT | RRHH, Decisor |
| **PUT** | `/vacantes/{id}` | Actualizar datos de vacante. | JWT | RRHH |
| **POST** | `/vacantes/{id}/pausar`| Pausar vacante. Requiere justificación. | JWT | RRHH, Reclutador |
| **POST** | `/vacantes/{id}/reanudar`| Reanudar vacante. Requiere justificación. | JWT | RRHH, Reclutador |
| **POST** | `/postulantes/registro` | Recibir postulación e iniciar parseo de CV. | No | Todos (Público / Externo) |
| **GET** | `/postulantes` | Listar postulantes asignados a vacante. | JWT | RRHH, Reclutador |
| **GET** | `/postulantes/{id}` | Consultar expediente del postulante. | JWT | RRHH, Reclutador |
| **PUT** | `/postulantes/{id}/estado` | Transitar postulante en pipeline. | JWT | RRHH, Reclutador |
| **POST** | `/entrevistas/programar`| Programar cita de evaluación y Teams. | JWT | Reclutador |
| **POST** | `/entrevistas/{id}/confirmar`| Confirmar asistencia de entrevista. | No | Postulante, n8n Callback |
| **POST** | `/entrevistas/{id}/reprogramar`| Reprogramar entrevista (máx 3 veces). | JWT | Reclutador, Postulante |
| **POST** | `/entrevistas/{id}/cancelar`| Cancelar entrevista y liberar horario. | JWT | Reclutador |
| **POST** | `/ofertas/generar` | Registrar propuesta de oferta. | JWT | RRHH |
| **POST** | `/ofertas/{id}/aprobar` | Aprobar oferta económica. | JWT | Decisor |
| **POST** | `/ofertas/{id}/enviar` | Enviar propuesta al postulante. | JWT | RRHH |
| **POST** | `/ofertas/{id}/aceptar` | Registrar aceptación de oferta. | JWT | RRHH, Reclutador |
| **POST** | `/ofertas/{id}/rechazar`| Registrar rechazo de oferta. | JWT | RRHH, Reclutador |
| **POST** | `/contrataciones` | Registrar inicio de contratación. | JWT | RRHH |
| **POST** | `/contrataciones/{id}/finalizar`| Cerrar contratación y dar de alta. | JWT | RRHH |
| **GET** | `/agenda` | Listar eventos de calendario del mes. | JWT | Reclutador, RRHH |
| **GET** | `/reportes/dashboard/ejecutivo`| Dashboard ejecutivo de KPIs de Dirección. | JWT | Decisor, RRHH, Auditor |
| **GET** | `/reportes/dashboard/reclutamiento`| Dashboard de KPIs del pipeline. | JWT | RRHH, Reclutador |
| **GET** | `/reportes/dashboard/sla` | KPIs y heatmap de cumplimiento de SLAs. | JWT | RRHH, Decisor, Auditor |
| **GET** | `/auditoria/logs` | Consultar log estructurado de auditoría. | JWT | Auditor, Administrador |
| **GET** | `/config/catalogos` | Cargar dinámicamente un catálogo. | JWT | Todos |
| **POST** | `/callbacks/solicitud-consistencia`| Callback de validación de solicitud. | API Key | n8n (Idempotente) |
| **POST** | `/callbacks/perfil-creacion`| Callback de carga de profesiograma. | API Key | n8n (Idempotente) |
| **POST** | `/callbacks/cv-parsing` | Callback de estructuración de CV. | API Key | n8n (Idempotente) |
| **POST** | `/callbacks/scoring` | Callback de notas del postulante. | API Key | n8n (Idempotente) |

---

## 2. Catálogo de DTOs (objeto de transferencia de datos)

### 2.1 DTOs de Autenticación
* **`LoginRequestDto`:**
  * `correo` (NVARCHAR(100), Requerido, Formato email, No nulo).
  * `clave` (NVARCHAR(100), Requerido, No nulo).
  * `tipoAutenticacion` (NVARCHAR(50), Requerido, Valores permitidos: `Local`, `ActiveDirectory`).
* **`LoginResponseDto`:**
  * `token` (NVARCHAR(MAX), JWT firmado de acceso).
  * `expiraEnSegundos` (INT, Tiempo de validez).
  * `usuario` (`UsuarioResponseDto`).
  * `mfaRequerido` (BIT, True si requiere segundo factor).
* **`TokenRefreshRequestDto`:**
  * `tokenExpirado` (NVARCHAR(MAX), JWT expirado).
  * `refreshToken` (NVARCHAR(100), Token de refresco de un solo uso).

### 2.2 DTOs de Solicitudes
* **`SolicitudCreateDto`:**
  * `cargoCodigo` (NVARCHAR(50), Requerido, FK a catálogo Cargos).
  * `areaCodigo` (NVARCHAR(50), Requerido, FK a catálogo Áreas).
  * `gerenciaCodigo` (NVARCHAR(50), Requerido, FK a catálogo Gerencias).
  * `unidadesCodigo` (NVARCHAR(50), Requerido, FK a catálogo Unidades).
  * `tipoVacanteCodigo` (NVARCHAR(50), Requerido, Valores: `VAC_NUEVA`, `VAC_REEMPLAZO`).
  * `remuneracionOfrecida` (DECIMAL(18,2), Requerido, Mayor a 0).
  * `justificacion` (NVARCHAR(500), Requerido, Mínimo 10 caracteres).
* **`SolicitudResponseDto`:**
  * `solicitudId` (INT, Clave primaria).
  * `cargoNombre` (NVARCHAR(100), Resuelto desde catálogo).
  * `estadoNombre` (NVARCHAR(100), Estado actual en la máquina de estados).
  * `remuneracionOfrecida` (DECIMAL(18,2)).
  * `createdBy` (NVARCHAR(100)).
  * `createdDate` (DATETIME2).

---

## 3. Catálogo de Errores Comunes de la API

Toda respuesta de error (4xx y 5xx) implementa el formato estándar inyectando el `CorrelationId`:

```json
{
  "Code": "CODIGO_SEMANTICO",
  "Message": "Mensaje legible para el usuario.",
  "Detail": "Detalles técnicos o de validación específicos.",
  "CorrelationId": "f7a391cb-d3f3-4d6d-b8d9-2ef53ea493b8"
}
```

### Códigos de Error Oficiales:
* **`INVALID_CREDENTIALS` (401 Unauthorized):** Usuario o clave incorrecta en autenticación local.
* **`USER_LOCKED` (401 Unauthorized):** Cuenta inhabilitada temporalmente por excesivos intentos fallidos.
* **`TOKEN_EXPIRED` (401 Unauthorized):** El JWT de acceso ha caducado.
* **`TOKEN_REUSE_DETECTED` (403 Forbidden):** Intento de renovación usando un Refresh Token ya consumido. Provoca la baja de todas las sesiones activas del usuario.
* **`CIRCULAR_DEPENDENCY_DETECTED` (400 Bad Request):** El insert/update de la relación jerárquica de catálogos generó un ciclo recursivo (mitigado por trigger).
* **`INVALID_STATE_TRANSITION` (400 Bad Request):** El cambio de estado solicitado no está permitido por la máquina de estados correspondiente.
* **`SLA_VIOLATED_REPROGRAMMING` (400 Bad Request):** Intento de reprogramación de entrevista que supera los 3 intentos autorizados.
* **`INSUFFICIENT_PERMISSIONS` (403 Forbidden):** El usuario no cuenta con el rol o los claims de permisos requeridos para consumir el recurso.
* **`RESOURCE_NOT_FOUND` (404 Not Found):** El ID del recurso (solicitud, vacante, etc.) no existe en la base de datos.
* **`IDEMPOTENCY_CALLBACK_DUPLICATED` (200 OK / 202 Accepted):** El callback de n8n fue recibido y detectado como repetido en caché por el `IdempotencyFilter`, devolviendo éxito inmediato sin reprocesar.

---

## 4. Matriz Rol ↔ Endpoint (RBAC Matrix)

| Endpoint | Administrador | RRHH | Reclutador | Solicitante | Decisor | Auditor |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| `/usuarios` (CRUD) | **X** | — | — | — | — | — |
| `/solicitudes` (Crear) | **X** | **X** | — | **X** | — | — |
| `/solicitudes/{id}/aprobar`| — | **X** | — | — | **X** | — |
| `/perfiles/generar` | — | **X** | — | — | — | — |
| `/perfiles/{id}/aprobar`| — | **X** | — | — | — | — |
| `/vacantes` (Crear/Cerrar)| — | **X** | — | — | — | — |
| `/vacantes/{id}/publicar`| — | **X** | **X** | — | — | — |
| `/postulantes` (Consultar)| — | **X** | **X** | — | — | — |
| `/entrevistas/programar`| — | — | **X** | — | — | — |
| `/ofertas/generar` | — | **X** | — | — | — | — |
| `/ofertas/{id}/aprobar` | — | — | — | — | **X** | — |
| `/reportes/dashboard/ejecutivo`| — | **X** | — | — | **X** | **X** |
| `/auditoria/logs` | **X** | — | — | — | — | **X** |

---

## 5. Matriz Estado ↔ Endpoint (StateMachine Matrix)

Mapeo de las transiciones de estados del negocio y los endpoints de API que las gatillan:

### 5.1 Máquina de Estados: Solicitudes
* **`Borrador` $\rightarrow$ `PendienteAprobacion`:** Gatillado por `POST /api/v1/solicitudes` o `PUT /api/v1/solicitudes/{id}`.
* **`PendienteAprobacion` $\rightarrow$ `Aprobada`:** Gatillado por `POST /api/v1/solicitudes/{id}/aprobar`. Dispara n8n.
* **`PendienteAprobacion` $\rightarrow$ `Rechazada`:** Gatillado por `POST /api/v1/solicitudes/{id}/rechazar`.

### 5.2 Máquina de Estados: Postulantes
* **`Registrado` $\rightarrow$ `Screening`:** Gatillado por `POST /api/v1/postulantes/registro` (tras completarse el parseo de CV).
* **`Screening` $\rightarrow$ `Entrevista`:** Gatillado por `POST /api/v1/entrevistas/programar`.
* **`Entrevista` $\rightarrow$ `Oferta`:** Gatillado por `POST /api/v1/ofertas/generar`.
* **`Oferta` $\rightarrow$ `Contratado`:** Gatillado por `POST /api/v1/contrataciones`.
* **Cualquiera $\rightarrow$ `Descartado`:** Gatillado por `PUT /api/v1/postulantes/{id}/estado` (con motivo de descarte).

---

## 6. Matriz Workflow ↔ Endpoint (n8n Integration)

| Código Workflow | n8n Trigger Endpoint (Llamado por .NET) | Callback .NET Endpoint (Llamado por n8n) | Agente IA Asignado |
| :---: | :--- | :--- | :--- |
| **WF-01** | `POST /webhook/wf-solicitud` | `POST /callbacks/solicitud-consistencia` | `AgenteSolicitud` |
| **WF-02** | `POST /webhook/wf-perfil` | `POST /callbacks/perfil-creacion` | `AgentePerfil` |
| **WF-05** | `POST /webhook/wf-cv-parsing` | `POST /callbacks/cv-parsing` | `AgenteMatching` |
| **WF-06** | `POST /webhook/wf-scoring` | `POST /callbacks/scoring` | `AgenteScoring` |
| **WF-07** | `POST /webhook/wf-agenda` | `POST /entrevistas/{id}/confirmar` | `AgenteCoordinacion` |

---

## 7. Riesgos Detectados y Recomendaciones

### 7.1 Riesgos Detectados:
1. **Timeouts en el Callback de n8n:** Si la inferencia de IA en n8n toma más de 60 segundos, la conexión HTTP puede caerse, activando los reintentos automáticos de n8n y arriesgando transiciones dobles.
2. **Exposición de Salarios en logs de Auditoría:** Si el payload anterior/nuevo en `AuditLog` se registra en texto plano sin filtrar campos sensibles, los Auditores verán las bandas salariales del personal sin poseer privilegios.
3. **Consumo no controlado de Tokens de IA:** El endpoint `/perfiles/generar` expuesto a reclutadores podría ser invocado repetidas veces para una misma solicitud, elevando los costos de los servicios de modelos de lenguaje (LLM).

### 7.2 Recomendaciones de Control:
1. **Configuración de Idempotencia y Jitter:** El middleware `IdempotencyFilter` de la API de .NET 8 debe configurarse con la capa de caché distribuida configurable. La cola de reintentos de n8n debe configurarse con un retraso mínimo de 2 minutos para evitar colisionar con la caché de idempotencia de 60 segundos.
2. **Sanitización de Payloads de Auditoría:** El backend de .NET 8 debe implementar un filtro interceptor en Entity Framework Core que detecte propiedades marcadas con la anotación `[SensitiveData]` (ej. `BandaSalarial`, `SalarioOfrecido`) y reemplace sus valores por asteriscos (`*****`) antes de guardarlos en el campo `EstadoNuevo` / `EstadoAnterior` de la tabla `AuditLogs`.
3. **Bloqueo de Re-Ejecución de Perfiles:** La API debe bloquear llamadas a `/perfiles/generar` si la solicitud correspondiente ya tiene un perfil en estado 'Aprobado' o 'En Proceso' en la máquina de estados.
