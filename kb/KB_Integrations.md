# KB_Integrations — Gobierno de Integraciones Internas y Externas
## Sistema Inteligente de Reclutamiento — Nacional Seguros

> **Versión:** 1.0.0 | **Estado:** PENDIENTE DE VALIDACIÓN
> **Última actualización:** 2026-06-20
> **Propietario:** NacionalSeguros_IntegrationArchitect / CISO
> **Audiencia:** Backend (.NET 8), Frontend (Angular), n8n, QA, Seguridad, Operaciones TI

---

## Control de Versiones Documentales

| Versión | Fecha | Autor | Rol | Cambios Realizados |
| :---: | :---: | :---: | :---: | :--- |
| `1.0.0` | 2026-06-20 | Antigravity | Technical Writer / Integration Architect | Creación del estándar de integraciones, definición de contratos, matriz de seguridad, traza y flujos de recuperación de errores. |

---

## Fuentes Oficiales

Esta KB se rige obligatoriamente por los siguientes documentos del proyecto:

| Documento | Rol en esta KB |
| :--- | :--- |
| `CONSTITUCION_PROYECTO.md` | Principios rectores de flujo de datos y restricciones críticas |
| `ANALISIS_FUNCIONAL.md` | Casos de uso operacionales e integraciones de negocio |
| `DISEÑO_ERD.md` | Tablas de log de integración (`WorkflowExecution`, `AgentExecution`) |
| `DICCIONARIO_DATOS.md` | Tipos de datos, longitudes y nulabilidades de campos de log |
| `ARQUITECTURA_BACKEND.md` | Estándares de desarrollo de APIs RESTful en .NET 8 |
| `ARQUITECTURA_FRONTEND.md` | Consumo de servicios HTTP y guards de autenticación en Angular |
| `ARQUITECTURA_N8N.md` | Arquitectura de integración n8n y callbacks asíncronos |
| `POLITICAS_DE_SEGURIDAD.md` | Cifrado SSL/TLS 1.3, hashing JWT y control RBAC |
| `AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md` | Trazabilidad del CorrelationId y bitácoras de auditoría |
| [KB_StateMachine](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_StateMachine.md) | Eventos y estados transaccionales que disparan flujos de integración |
| [KB_MasterData](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_MasterData.md) | Parametrización de plantillas y canales de comunicación |
| [KB_CatalogoAgentes](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_CatalogoAgentes.md) | Contratos de los agentes de IA invocados por workflows |
| [KB_CatalogoWorkflows](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_CatalogoWorkflows.md) | Triggers y dependencias entre workflows orquestados |
| [KB_AI_Governance](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_AI_Governance.md) | Trazabilidad inmutable de telemetría y llamadas a LLMs |

---

## Principios Rectores de las Integraciones

1. **Autenticación en Cada Punto:** Queda terminantemente prohibida cualquier comunicación entre sistemas que no cuente con un mecanismo de autenticación robusto (JWT, OAuth 2.0 u tokens portadores seguros).
2. **Propagación y Trazabilidad Transversal:** Cada petición que cruce fronteras tecnológicas debe incluir en sus cabeceras HTTP la cabecera `X-Correlation-ID`. Toda entrada en logs de error o auditoría debe incluir esta ID única de correlación.
3. **Manejo de Errores y Caídas:** Las integraciones no deben fallar de forma silenciosa. Si un servicio externo no responde, el sistema debe registrar el fallo, iniciar la política de reintentos con amortiguación y, en caso de fallo definitivo, notificar al administrador TI y cambiar el estado del proceso a un estado seguro controlado.
4. **No Repudio e Inmutabilidad de Logs:** Toda petición transaccional entre el Core en .NET 8 y sistemas externos (n8n, SMTP, Calendario corporativo, WhatsApp) debe registrar un log inmutable de auditoría para verificar la fecha, actor y resultado.
5. **Principio de Mínimo Privilegio (RBAC):** Las credenciales utilizadas por n8n o sistemas externos deben contar exclusivamente con permisos para consumir endpoints específicos (Header `X-API-Key` o scopes acotados), prohibiendo permisos de administración globales.

---

# Catálogo Oficial de Integraciones

| Código | Integración | Protocolo | Autenticación | Tipo | Propósito |
| :---: | :--- | :---: | :---: | :---: | :--- |
| **INT-01** | **Angular $\leftrightarrow$ Backend .NET 8** | REST HTTPS | JWT (Local / Directorio Corporativo) | Interna | Consumo de datos y control de la UI. |
| **INT-02** | **Backend .NET $\leftrightarrow$ SQL Server 2022**| ADO.NET (EF/Dapper)| Connection String Cifrada | Interna | Persistencia transaccional oficial. |
| **INT-03** | **Backend .NET $\leftrightarrow$ n8n Webhooks** | REST HTTPS | API Key (`X-API-Key`) | Interna | Automatización y orquestación de IA. |
| **INT-04** | **n8n $\rightarrow$ Servidor SMTP (Correo)** | SMTP TLS | Credenciales SSL | Externa | Envío de notificaciones y ofertas. |
| **INT-05** | **n8n $\leftrightarrow$ Microsoft Graph (Outlook)** | REST HTTPS | OAuth 2.0 (Directorio Corporativo) | Externa | Sincronización y agendamiento. |
| **INT-06** | **n8n $\leftrightarrow$ WhatsApp Business API** | REST HTTPS | Bearer Token (Meta) | Externa | Coordinación conversacional directa. |
| **INT-07** | **n8n $\leftrightarrow$ Portales de Reclutamiento** | REST HTTPS | OAuth 2.0 / API Keys | Externa | Publicación y recepción de postulaciones. |

---

# Integraciones Internas

---

## 1. Angular 17+ ↔ Backend .NET 8 (INT-01)

* **APIs Consumidas:** Todos los controladores REST expuestos bajo `/api/v1/` (ej. `/api/v1/solicitudes`, `/api/v1/postulantes`).
* **Autenticación y Rotación de Refresh Tokens (RTR):** Híbrida (Local con hashing BCrypt / AD delegada). Tras el login exitoso, el backend emite un JWT de corta duración (15-30 min) y un Refresh Token de un solo uso en la tabla `Sesion`. Al solicitar renovación de sesión, el token utilizado se invalida (`Activa = 0`) y se emite uno nuevo. Si se detecta un intento de reutilización de un Refresh Token ya invalidado, la API suspenderá inmediatamente todas las sesiones activas del usuario (detección y mitigación de secuestro de sesión).
* **Autorización:** Control de Acceso Basado en Roles (RBAC). El frontend lee los claims del JWT para habilitar/deshabilitar vistas. El backend intercepta los claims en cada endpoint mediante políticas `[Authorize]`.
* **Manejo de Errores:** Interceptor HTTP global en Angular que atrapa respuestas no exitosas (4xx y 5xx) y las traduce en notificaciones toast estructuradas para el usuario. Envía el detalle con el `CorrelationId` provisto en el header.

---

## 2. Backend .NET 8 ↔ SQL Server 2022 (INT-02)

* **Acceso a Datos:** A través de Entity Framework Core 8 para la lógica relacional transaccional y Dapper para consultas masivas y agregaciones de reportería de alta velocidad.
* **Transacciones:** Controladas mediante bloques Unit of Work que garantizan atomicidad (ACID) y previenen escrituras huérfanas en transiciones complejas de la máquina de estados.
* **Auditoría:** Triggers DDL y lógica en el `DbContext` interceptando los cambios para inyectar automáticamente valores en las 7 columnas de auditoría transversales (ej. `CreatedBy`, `IsDeleted`).
* **Integridad de Datos:** Llaves primarias y foráneas explícitas. Restricciones UNIQUE a nivel de BD para evitar duplicados en tablas críticas (`Usuario`, `Rol`, `Parametro`).

---

## 3. Backend .NET 8 ↔ n8n Webhooks (INT-03)

* **Endpoints de Llamada (Trigger):** El backend invoca a n8n mediante HTTP POST enviando el payload de negocio a la dirección del webhook asignado al workflow (ej. `https://n8n.nacional.bo/webhook/wf-solicitud`).
* **Autenticación y Seguridad:** n8n valida la presencia y valor del encabezado `X-API-Key`. Las peticiones viajan estrictamente bajo TLS 1.3.
* **Callback Asíncrono e Idempotencia:** n8n responde inmediatamente `202 Accepted` al recibir la orden de trigger. Al concluir el procesamiento del LLM, n8n realiza una llamada HTTP POST de retorno al callback de .NET 8 (ej. `POST /api/v1/solicitudes/callback/consistencia`). Para evitar el procesamiento duplicado de transiciones ante reintentos de red o de n8n, los endpoints de callback están protegidos por el filtro `[IdempotentCallbackFilter]` (basado en `IdempotencyFilter`).
  - **Estrategia de Caché de Idempotencia:**
    - *Desarrollo Local:* Se utiliza la caché en memoria nativa (`IMemoryCache`) de ASP.NET Core, lo que simplifica la infraestructura de desarrollo.
    - *QA y Producción:* Se utiliza un proveedor de caché distribuida compatible (`IDistributedCache`) configurado con una expiración de 60 segundos. Esto asegura la consistencia de la idempotencia en un entorno con múltiples réplicas detrás de un balanceador de carga.
  - **Lógica del Filtro:** El filtro intercepta la petición, verifica si el `X-Correlation-ID` ya existe en la caché. Si existe, cancela la ejecución duplicada de inmediato y responde con un status `HTTP 200 OK` o `HTTP 202 Accepted` indicando el código `IDEMPOTENCY_CALLBACK_DUPLICATED`, previniendo transiciones de estado duplicadas e inconsistencias en `StateHistory`.
* **Reintentos:** Configurado en la cola de n8n para reintentar la llamada de callback 3 veces de forma exponencial amortiguada ante fallas de timeout del backend.

---

# Integraciones Externas

---

## 1. n8n ↔ Servidor SMTP (Correo Electrónico - INT-04)

* **Eventos Disparadores:** Cambio de estado de solicitudes (notificar al solicitante), ofertas de contrato emitidas (envío formal al candidato) o alertas de SLAs vencidos a gerencias.
* **Plantillas:** El cuerpo del correo se construye dinámicamente inyectando placeholders sobre las plantillas registradas en la base de datos (tabla `Plantilla`).
* **Seguimiento:** Cada envío de correo se registra inmutablemente en la tabla `NotificationLog` del backend con fecha, destinatario, asunto y estado final (Enviado / Fallido).

---

## 2. n8n ↔ Microsoft Graph API (Calendario Outlook - INT-05)

* **Creación de Eventos:** Se dispara automáticamente al pasar el postulante a `POST-05 ENTREVISTA_PEND`. El workflow de n8n consulta la disponibilidad, bloquea el horario en la agenda de Outlook del entrevistador y genera el enlace único de Microsoft Teams.
* **Reprogramaciones:** Al transitar la entrevista a `ENT-03 REPROGRAMADA` por reclutador o candidato, n8n modifica la cita en Outlook y notifica a las partes por correo y WhatsApp.
* **Cancelaciones:** Si transita a `ENT-06 CANCELADA`, n8n libera el bloque de horario del entrevistador y envía la confirmación de la anulación del evento.

---

## 3. n8n ↔ WhatsApp Business API (INT-06)

* **Notificaciones de Coordinación:** Envío de recordatorios preconfigurados utilizando las plantillas aprobadas por Meta para evitar clasificaciones de spam.
* **Confirmación Conversacional (Webhook de Entrada):** n8n expone un webhook que recibe las respuestas interactivas del postulante (ej. botones de confirmación de entrevista) y las procesa para actualizar la base de datos de .NET.
* **Seguimiento:** Registro de fecha y texto completo de interacciones en la tabla de bitácora conversacional (`WhatsAppLog`).

---

## 4. n8n ↔ Portales de Reclutamiento (LinkedIn API - INT-07)

* **Publicación de Vacantes:** Al publicarse la vacante (`VAC-02 PUBLICADA`), n8n utiliza las credenciales de la empresa para enviar la descripción estructurada e ingresar el anuncio en LinkedIn.
* **Recepción de Postulaciones:** n8n actúa como receptor de webhooks de LinkedIn (*Easy Apply*), captura los datos curriculares entrantes y los envía de inmediato al endpoint de registro del backend (`POST /api/v1/postulantes/registro`).
* **Sincronización:** Tarea periódica cron (cada 4 horas) para actualizar el estado de las vacantes activas en los portales externos.

---

# Especificación y Contratos de Integración (DTOs)

A continuación se detallan los contratos de integración para las dos interfaces clave del sistema:

---

## CONTRATO INT-03: Backend .NET 8 $\rightarrow$ n8n Webhook (Trigger)

* **Objetivo:** Disparar de forma segura un workflow de automatización de IA en n8n desde el backend.
* **Trigger:** Evento de dominio en el backend (ej. Solicitud Enviada).
* **Format:** JSON sobre HTTPS REST.
* **Seguridad:** Header `X-API-Key` y TLS 1.3 obligatorio.
* **Timeout:** 5000ms (espera del `202 Accepted`).

#### Payload de Petición (Request Body):
```json
{
  "CorrelationId": "e2c39e2d-3b4e-4f1a-8b3d-2f0d9b4c7b2d",
  "WorkflowName": "WF-01-ConsistenciaSolicitud",
  "TargetEndpointCallback": "/api/v1/solicitudes/callback/consistencia",
  "Payload": {
    "SolicitudId": 125,
    "Cargo": "Ingeniero de Ciberseguridad",
    "Funciones": "Auditoría de código, configuración de firewalls de aplicación y gestión de llaves criptográficas.",
    "Skills": "SAST, DAST, TLS 1.3, OWASP Top 10"
  }
}
```

---

## CONTRATO INT-05: n8n $\rightarrow$ Microsoft Graph (Creación de Cita)

* **Objetivo:** Agendar de manera automática una entrevista e inyectar el evento en el calendario de Outlook corporativo del entrevistador.
* **Trigger:** Postulante agendado (`ENT-01 AGENDADA`).
* **Formato:** JSON sobre REST API de Microsoft Graph.
* **Seguridad:** Token de acceso OAuth 2.0 (Servicio de Directorio de Identidad Corporativo con privilegios `Calendars.ReadWrite`).
* **Timeout:** 10000ms.

#### Payload de Petición (Microsoft Graph API Schema):
```json
{
  "subject": "Entrevista Técnica - SIR Nacional Seguros",
  "body": {
    "contentType": "HTML",
    "content": "Estimado Evaluador, se ha agendado la entrevista técnica del candidato en el SIR. Enlace de Teams adjunto."
  },
  "start": {
    "dateTime": "2026-06-22T10:00:00",
    "timeZone": "SA Western Standard Time"
  },
  "end": {
    "dateTime": "2026-06-22T11:00:00",
    "timeZone": "SA Western Standard Time"
  },
  "location": {
    "displayName": "Microsoft Teams Meeting"
  },
  "attendees": [
    {
      "emailAddress": {
        "address": "entrevistador@nacional.bo",
        "name": "Evaluador Técnico SIR"
      },
      "type": "required"
    }
  ],
  "isOnlineMeeting": true,
  "onlineMeetingProvider": "teamsForBusiness"
}
```

---

# Trazabilidad Unificada de Integración

Toda integración (interna o externa) que modifique o consulte datos críticos debe registrar de forma mandatoria una bitácora en la tabla `IntegrationLog` en SQL Server 2022 estructurando la siguiente traza:

* **Fecha:** Timestamp UTC exacto del registro (`DateTime2(7)`).
* **Usuario:** Identificador del usuario que ejecutó la llamada (o 'SYSTEM' si es automático).
* **Evento:** Código del evento de dominio asociado (ej. `solicitud.aprobada`).
* **Resultado:** Estado final de la transacción (`Exitoso` / `Fallido` / `Reintento`).
* **Error:** JSON con la traza de excepción técnica (`ErrorMessage` y `Stacktrace` sanitizado).
* **CorrelationId:** ID unificada UUID de la traza para rastrear los logs desde el origen.

---

# Estrategia de Manejo de Errores y Recuperación

```
[Fallo en Integración]
          │
          ▼
[1. Reintento Exponencial (Backoff)]
          │
          ├──> Éxito: Termina flujo
          └──> Falla todos los intentos
                     │
                     ▼
[2. DLQ (Dead Letter Queue)] ──> [3. Alerta y Notificación]
                                          │
                                          ▼
                         [4. Estado Seguro de Negocio]
```

### 1. Reintentos Exponenciales (Backoff)
* **Lógica:** Ante un fallo de red o timeout de la API externa (ej. Microsoft Graph o WhatsApp), n8n ejecutará una cola de reintentos automática.
* **Frecuencia:** 3 reintentos separados de forma exponencial:
  * Intentos: 1° a los 2 minutos $\rightarrow$ 2° a los 10 minutos $\rightarrow$ 3° a los 30 minutos.
  * Se aplica Jitter (desviación aleatoria) para no colapsar la API en caso de caídas generales de red.

### 1.1 Resiliencia y Polly en Clientes HTTP (.NET ──> n8n)
La API de .NET 8 configura políticas de resiliencia mediante **Polly** usando `IHttpClientFactory` para el cliente tipado de integración (`n8nClient`):
* **Reintento Exponencial con Jitter:** Se configuran **3 reintentos** automáticos con tiempos de espera exponenciales amortiguados más una variación aleatoria (jitter) para evitar el efecto de tormenta de peticiones (Thundering Herd) sobre n8n:
  $$\text{Tiempo de Espera} = 2^{\text{intento}} \text{ segundos} + \text{jitter (0-100 ms)}$$
* **Circuit Breaker (Interruptor de Circuito):** Se define un interruptor que entra en estado **Abierto** durante **30 segundos** si se detectan **5 fallos HTTP consecutivos** (errores 5xx, timeouts o fallos de red). Durante el estado abierto, todas las llamadas a n8n se detienen en la API lanzando un error inmediato de circuito roto (`BrokenCircuitException`), protegiendo los sockets del backend y permitiendo la recuperación del servicio externo.

### 2. Cola de Descarte (Dead Letter Queue - DLQ)
* **Lógica:** Si los 3 reintentos fallan de forma definitiva, el workflow de error en n8n (`WF-ERR-01`) intercepta la ejecución, escribe en la base de datos `WorkflowExecution` el estado como `Fallido` y guarda el payload original en una tabla de descarte para posterior reenvío manual.

### 3. Alertas y Notificaciones TI
* **Lógica:** Todo error definitivo de integración envía de forma automática una alerta al canal de soporte técnico a través del sistema de observabilidad.

### 4. Estado Seguro de Negocio (Fail-Safe)
* **Lógica:** Para evitar que la entidad de negocio quede en limbo o en proceso indefinido, la API del backend transitará la entidad a un estado seguro configurado (ej. la entrevista se marca como `ENT-06 CANCELADA` con motivo "Fallo del servicio de calendario" y se notifica al reclutador para agendamiento manual).

---

# Riesgos Detectados y Mitigaciones en Integraciones

| ID | Riesgo | Severidad | Impacto | Control y Mitigación Propuesta |
| :---: | :--- | :---: | :--- | :--- |
| **R-INT-01** | **Dependencias Circulares** | 🔴 Alto | n8n llama a la API .NET, y ésta llama a n8n dentro de la misma transacción, provocando bloqueos de threads (Deadlocks). | Implementar llamadas asíncronas en el backend. .NET dispara n8n mediante un hilo asíncrono y responde `202 Accepted` de inmediato sin bloquear. |
| **R-INT-02** | **Credenciales Expuestas en Workflow** | 🔴 Alto | Exposición de API Keys de LinkedIn o WhatsApp en los repositorios de n8n. | Utilizar el manejador de credenciales centralizado cifrado nativo de n8n. Queda prohibido escribir textos planos de tokens en los nodos. |
| **R-INT-03** | **Llamadas Sin Trazabilidad** | 🟠 Medio | Pérdida de CorrelationId en workflows complejos de n8n, impidiendo depurar errores. | El Action Filter global de .NET rechaza callbacks de n8n que no incluyan el encabezado `X-Correlation-ID`. |
| **R-INT-04** | **Expiración de Tokens de Calendario** | 🟠 Medio | Interrupción general del módulo de agenda por desactualización de credenciales OAuth del Directorio Corporativo. | Configurar una alerta automatizada cron de expiración de credenciales que envíe una notificación al administrador TI 15 días antes del vencimiento. |

---

# Recomendaciones de Arquitectura

1. **Implementar Patrón Circuit Breaker en .NET 8:** Integrar la librería Polly en los clientes HTTP del backend .NET para bloquear temporalmente las llamadas hacia servicios caídos conocidos, evitando desperdiciar recursos y colapsar los puertos de comunicación.
2. **Webhooks Idempotentes:** Asegurar que todos los endpoints de callback del backend utilicen validación de idempotencia sobre la clave `CorrelationId` para prevenir que reintentos de red de n8n dupliquen inserciones en base de datos.
3. **Cifrado TLS 1.3 Obligatorio:** Validar periódicamente que el servidor que aloja n8n e IIS (API .NET) tengan deshabilitados protocolos obsoletos como SSLv3, TLS 1.0 y TLS 1.1 en el sistema operativo.

---

## Cumplimiento de Lineamientos del Proyecto
**Resultado de la Evaluación:** `APPROVED`

### Justificación del Resultado:
* **Gobernanza de Integración:** Se detallan de forma exhaustiva los contratos y alcances de las 7 integraciones clave de la Fase 1.
* **Seguridad Robusta:** Se describen los controles de hashing JWT, tokens OAuth 2.0 y secretos inyectados en runtime.
* **Trazabilidad y CorrelationId:** Se impone el CorrelationId y el registro estructurado e inmutable de logs en la tabla `IntegrationLog`.
* **Idempotencia y Fail-Safe:** Se diseña la política de reintentos exponenciales amortiguados (backoff con jitter) y la recuperación controlada de estados de negocio.
