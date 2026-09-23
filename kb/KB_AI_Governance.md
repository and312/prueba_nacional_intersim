# KB_AI_Governance — Marco de Gobernanza de Inteligencia Artificial
## Sistema Inteligente de Reclutamiento — Nacional Seguros

> **Versión:** 1.0.0 | **Estado:** PENDIENTE DE VALIDACIÓN
> **Última actualización:** 2026-06-20
> **Propietario:** NacionalSeguros_ProjectAuditor / CISO
> **Audiencia:** Backend (.NET 8), Workflows (n8n), Frontend (Angular), QA, Auditoría, Comité de Riesgos

---

## Control de Versiones Documentales

| Versión | Fecha | Autor | Rol | Cambios Realizados |
| :---: | :---: | :---: | :---: | :--- |
| `1.0.0` | 2026-06-20 | Antigravity | Technical Writer / CISO | Definición del marco regulatorio de IA, políticas de explicabilidad, control de prompts, modelos y supervisión humana. |

---

## Fuentes Oficiales

Esta KB se rige obligatoriamente por los siguientes documentos del proyecto:

| Documento | Rol en esta KB |
| :--- | :--- |
| `CONSTITUCION_PROYECTO.md` | Restricciones de toma de decisiones autónomas por la IA |
| `ANALISIS_FUNCIONAL.md` | Casos de uso interactivos y de asistencia de la IA |
| `DISEÑO_ERD.md` | Estructuras físicas del log de IA (`AgentExecution`, `AgentRecommendation`) |
| `ARQUITECTURA_N8N.md` | Flujos de comunicación de prompts y telemetría de tokens en callbacks |
| `POLITICAS_DE_SEGURIDAD.md` | Anonimización obligatoria de datos PII antes de enviar a LLMs |
| `AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md` | Estándares de inmutabilidad, no repudio y trazabilidad |
| [KB_StateMachine](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_StateMachine.md) | Transiciones críticas sujetas a validación humana |
| [KB_MasterData](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_MasterData.md) | Catálogos parametrizables de agentes y prompts |
| [KB_CatalogoAgentes](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_CatalogoAgentes.md) | Fichas técnicas, contratos y alcances individuales de agentes |

---

# 1. Gobierno de Prompts

Para impedir la alteración arbitraria de las instrucciones de la Inteligencia Artificial y asegurar la consistencia del sistema, todos los prompts de Nacional Seguros se catalogan de forma centralizada en la base de datos `SIR_NacionalSeguros` (tablas `Prompt` y `PromptVersion`) y se gobiernan según las siguientes reglas:

### 1.1 Estructura del Catálogo de Prompts
Cada prompt registrado en la base de datos debe contener los siguientes campos obligatorios:
* **PromptId:** Identificador físico único.
* **Nombre:** Código semántico único (ej. `PR_MATCH_CV`).
* **Objetivo:** Descripción del resultado de negocio esperado del prompt.
* **Versión:** Número de versión correlativo (`VersionNumber`).
* **Estado:** Estado de ciclo de vida (`Activo = 1` o `Inactivo = 0`).
* **Responsable:** Rol propietario facultado para autorizar modificaciones.
* **Fecha de Aprobación:** Timestamp UTC del momento en que el prompt fue aprobado por QA y Seguridad.

---

### 1.2 Catálogo Oficial de Prompts (Estado de Ciclo de Vida)

```
[Prompts Productivos] ── (Inactivación) ──> [Prompts Obsoletos]
        ▲
        │ (Aprobación y Paso a Prod)
[Prompts Experimentales]
```

#### A) Prompts Productivos (Vigentes en Producción)
Prompts activos consumidos en tiempo de ejecución por los workflows n8n:

| PromptId | Nombre | Versión | Objetivo | Responsable | Fecha Aprobación |
| :---: | :--- | :---: | :--- | :--- | :---: |
| **PR-01** | `PR_SOL_VALIDATE` | `1.0.0` | Analizar consistencia y viabilidad de la solicitud. | Jefe de Reclutamiento | 2026-06-18 |
| **PR-02** | `PR_PERF_BUILD` | `1.0.0` | Estructurar borrador de profesiograma del cargo. | Jefe de Compensación | 2026-06-18 |
| **PR-03** | `PR_SRC_CANALES` | `1.0.0` | Sugerir canales de sourcing según vacante. | Reclutador Sénior | 2026-06-19 |
| **PR-04** | `PR_MAT_CV_PERFIL` | `1.0.0` | Comparar semánticamente el CV contra perfil. | Jefe de Reclutamiento | 2026-06-19 |
| **PR-05** | `PR_SCO_POSTULANTE`| `1.0.0` | Calcular nota de idoneidad y justificación. | Líder de RRHH | 2026-06-20 |
| **PR-06** | `PR_COO_RECORDATOR`| `1.0.0` | Entablar flujo conversacional de agenda. | Coordinador de RRHH | 2026-06-20 |
| **PR-07** | `PR_ANA_KPI_SLAS` | `1.0.0` | Consolidar métricas semanales y redactar informe. | Gerente de RRHH | 2026-06-20 |

#### B) Prompts Experimentales (Ambiente de Pruebas / QA)
Prompts en evaluación de calidad por el equipo de ingeniería de IA:

| PromptId | Nombre | Versión | Objetivo de la Experimentación | Responsable Técnico | Estado |
| :---: | :--- | :---: | :--- | :--- | :---: |
| **PR-EXP-01**| `PR_SCO_POSTULANTE`| `1.1.0-RC1` | Incorporar análisis de fit cultural de entrevistas. | Lead AI Engineer | En Pruebas |
| **PR-EXP-02**| `PR_MAT_CV_PERFIL` | `1.0.8-BETA`| Evaluar mejoras en detección de sinónimos de skills. | AI Engineer | En Pruebas |

#### C) Prompts Obsoletos (Histórico Inactivo)
Prompts archivados que no deben ser llamados bajo ninguna circunstancia por n8n:

| PromptId | Nombre | Versión | Motivo de Obsolescencia | Reemplazado Por | Fecha Baja |
| :---: | :--- | :---: | :--- | :--- | :---: |
| **PR-OBS-01**| `PR_MAT_CV_V0` | `0.9.0` | Falta de controles de privacidad y fuga de PII. | `PR_MAT_CV_PERFIL` v1.0.0| 2026-06-17 |

---

# 2. Gobierno de Modelos LLM

Nacional Seguros limita el uso de modelos de Inteligencia Artificial exclusivamente a proveedores y configuraciones autorizadas para garantizar la latencia, costos y soberanía de los datos.

| Proveedor / API | Modelo Utilizado | Versión del Modelo | Uso Autorizado | Restricciones de Seguridad |
| :--- | :--- | :--- | :--- | :--- |
| **Proveedor LLM Enterprise (Cloud)** | **Modelo de lenguaje (Avanzado)** | `Configurado por la organización` | Matching semántico curricular (`AGE-04`), Scoring analítico (`AGE-05`) y Redacción de Informes (`AGE-07`). | Requiere conexiones seguras HTTPS TLS 1.3 con clave en secrets. Datos enviados **no** se usan para entrenamiento. |
| **Proveedor LLM Enterprise (Cloud)** | **Modelo de lenguaje (Rápido)** | `Configurado por la organización` | Estructuración inicial rápida (`AGE-01`), sugerencias de sourcing (`AGE-03`) y flujo conversacional de WhatsApp (`AGE-06`). | Limitar payloads grandes para optimizar costos de tokens. |
| **Local / Enterprise** | **Modelos Locales** (Opcional Fase 2) | — | Reservado para búsquedas vectoriales a nivel de base de datos internas. | Ningún dato transaccional del negocio puede salir de la red local sin cifrar. |

---

# 3. Gobierno de Agentes

El gobierno de los agentes delimita los objetivos, alcances y dependencias para evitar la delegación excesiva de control.

```mermaid
graph LR
    %% Gobernanza de Agentes
    AGE01["AGE-01: AgenteSolicitud"] -->|Valida| AGE02["AGE-02: AgentePerfil"]
    AGE02 -->|Soporta| AGE04["AGE-04: AgenteMatching"]
    AGE04 -->|Alimenta| AGE05["AGE-05: AgenteScoring"]
```

### Reglas de Gobierno por Agente:
1. ** AGE-01: AgenteSolicitud:**
   * **Objetivo:** Validar viabilidad funcional de requerimientos.
   * **Alcance:** Exclusivo para la fase inicial de Solicitud de Personal. No tiene visibilidad sobre postulantes o contratos.
   * **Responsable Funcional:** Líder del área solicitante.
2. **AGE-04: AgenteMatching:**
   * **Objetivo:** Medir afinidad curricular desvinculada de datos personales.
   * **Alcance:** Módulo de Postulantes (Screening).
   * **Dependencias:** Requiere el profesiograma aprobado en `PerfilCargo` para realizar la comparación.

---

# 4. Matriz de Trazabilidad Transversal

Para cumplir con las auditorías de calidad de InterSIM, cada interacción de IA debe registrar de forma granular una traza unificada mediante la inyección y propagación del `CorrelationId`:

```
[Request API .NET 8]  ──>  Inyecta CorrelationId  ──>  [Workflow n8n]
                                                              │
                                                              ▼
[Base de Datos SQL]  <──  Escribe AgentExecution  <──  [Callback .NET 8]
```

### Campos Registrados en la Traza de Ejecución:
* **Input Recibido:** Payload JSON exacto anonimizado enviado a n8n.
* **Output Generado:** Respuesta estructurada y justificación devuelta por la IA.
* **Prompt Utilizado:** Relación física al ID de versión exacto en `PromptVersion`.
* **Modelo Utilizado:** Modelo del LLM consultado (ej. Modelo Avanzado).
* **Usuario Solicitante:** ID del usuario administrativo o sistema que inició la llamada.
* **Fecha y Hora:** Timestamp exacto en UTC.
* **Workflow Ejecutado:** Nombre del proceso orquestado en n8n (`WorkflowName`).

---

# 5. Especificación del Log `AgentExecution`

### 5.1 Estructura en Base de Datos
El log de ejecuciones se persiste físicamente en la tabla `AgentExecution` con restricciones *append-only* (inmutable):
* **Retención de Datos:**
  * **Hot Storage (Base Transaccional):** 1 año calendario para consultas operativas rápidas desde el frontend.
  * **Cold Storage (Auditoría/Backups):** 5 años archivado para peritajes legales o auditorías de cumplimiento de Nacional Seguros.
* **Auditoría:** La tabla no admite operaciones `UPDATE` o `DELETE`. Cualquier intento de borrado lógico o físico genera una alerta inmediata en los logs de base de datos.
* **Consulta y RBAC:** El acceso a la tabla de ejecuciones está restringido estrictamente a usuarios con el rol `Auditor` y `Administrador`. El frontend no expone endpoints de consulta de logs a reclutadores estándar para resguardar la seguridad.

---

# 6. Política de Explicabilidad de IA

Toda recomendación o resultado calculado por un agente de Inteligencia Artificial que sea desplegado a un usuario de Nacional Seguros debe cumplir de forma obligatoria con los siguientes criterios de explicabilidad explicados en lenguaje natural:

1. **Razón de la Recomendación:** Explicar detalladamente por qué el candidato es o no apto, o por qué la solicitud fue observada (ej. *"El candidato cuenta con experiencia con bases de datos pero carece de C#"*).
2. **Factores Considerados:** Detallar qué variables influyeron en la nota (ej. *Experiencia curricular: 40%, Prueba Técnica: 40%, Entrevista: 20%*).
3. **Nivel de Confianza:** Porcentaje de certeza devuelto por la IA (`NivelConfianza` o `ScoreCoincidencia`).
4. **Evidencias Utilizadas:** Referencias textuales directas del currículum o del profesiograma que justifican la decisión (ej. *"Línea 12 del CV: Desarrollador .NET Senior en Banco Unión"*).

---

# 7. Política de Supervisión Humana (Human-in-the-Loop)

La Inteligencia Artificial opera bajo el rol de asistente consultivo. Las decisiones transaccionales y de cambio de estados críticos son exclusivas del personal de Nacional Seguros.

### Matriz de Decisiones y Control Humano:

| Evento / Acción | Rol de la IA | Decisión Final del Humano | Canal de Confirmación |
| :--- | :--- | :--- | :--- |
| **Aprobación de Solicitud** | AGE-01 analiza consistencia. | El **Decisor** aprueba o rechaza manualmente. | Frontend Angular |
| **Aprobación de Profesiograma** | AGE-02 genera borrador. | El **Analista de RRHH** edita y aprueba versión. | Frontend Angular |
| **Descarte Curricular** | AGE-04 y AGE-05 sugieren descarte. | El **Reclutador** confirma descarte registrando motivo. | Pipeline (Kanban) |
| **Coordinación de Agenda** | AGE-06 consulta disponibilidad. | El **Postulante** elige fecha y el **Reclutador** valida. | WhatsApp / Calendario |
| **Contratación Final** | Ninguno. Fuera de alcance de la IA. | El **Gerente de RRHH** firma el contrato físico/digital. | Sistema ERP / Legal |

---

# 8. Controles de Seguridad y Privacidad

Para mitigar riesgos y cumplir con la directrices OWASP Top 10 y normativas locales:
* **RBAC Estricto:** Los webhooks de n8n no procesan peticiones si no provienen de la API de .NET con un token JWT válido que contenga los claims autorizados para inyectar transiciones.
* **Protección de Datos Sensibles (PII):** Los datos personales confidenciales (como pretensiones salariales o scoring) se almacenan utilizando *Always Encrypted* en SQL Server 2022. Las llaves de cifrado de columna (CEK) y la llave maestra de columna (CMK) se administran centralmente en el almacén de secretos corporativo. n8n nunca recibe valores descifrados.
* **Gobernanza de Claves y Rotación:** Se establece la rotación mandatoria de la CMK en el almacén de secretos corporativo cada 12 meses, ejecutada de manera coordinada por el Oficial de Seguridad (CISO) y auditada en los registros de acceso.
* **Auditoría Transversal:** Cada interacción escribe en la tabla `AuditLog` detallando el usuario que autorizó o rechazó la recomendación del agente de IA, cerrando el ciclo de no repudio.

---

# 9. Control de Cuotas, Throttling y Concurrencia (Modelos de Lenguaje)

Para evitar interrupciones de servicio por denegación de solicitudes (errores HTTP 429 - Too Many Requests) causadas por picos de tráfico en procesos de análisis masivo de postulantes, el backend de .NET 8 implementa una política rígida de control de tráfico y concurrencia.

### 9.1 Token Bucket Rate Limiting (Middleware)
Se configura un middleware nativo de limitación de tasa de solicitudes en la API de .NET 8 que controla el tráfico dirigido a los endpoints de integraciones y n8n:
* **Capacidad del Balde:** Se limita la tasa a un máximo de **50 peticiones por minuto** para peticiones externas o de callbacks de integración.
* **Comportamiento ante Exceso:** Si el cliente (o el workflow de n8n) supera la cuota permitida de llamadas, la API retorna inmediatamente un código de estado `HTTP 429 Too Many Requests` con la cabecera `Retry-After`.

### 9.2 Control de Concurrencia con `SemaphoreSlim`
El backend de .NET 8 protege la API de inferencia del LLM controlando las peticiones salientes simultáneas desde la capa de aplicación:
* **Semáforo Semántico:** Los Handlers de MediatR para las llamadas del Agente de Matching (`AGE-04`) y Agente de Scoring (`AGE-05`) utilizan un semáforo estático de clase:
  ```csharp
  private static readonly SemaphoreSlim _vertexSemaphore = new SemaphoreSlim(10, 10);
  ```
* **Límite Concurrente:** Se permite un máximo de **10 llamadas concurrentes simultáneas** hacia la API del LLM.
* **Encolamiento en Memoria:** Si ingresa una undécima solicitud de análisis en paralelo, el handler realiza una espera asíncrona no bloqueante (`await _vertexSemaphore.WaitAsync()`), ingresando a una cola de ejecución FIFO (First-In, First-Out) en memoria local hasta que se libere un hilo del semáforo.
* **Timeouts de Cola:** Las peticiones en cola tienen un límite de espera máximo de **30 segundos**. Si una petición no logra entrar al semáforo en ese tiempo, se cancela y se lanza un error de Timeout, liberando los recursos del servidor y registrando el incidente en `ErrorLog`.

---

# Riesgos Detectados y Mitigaciones en Gobernanza IA

| ID | Riesgo | Severidad | Impacto | Control y Mitigación |
| :---: | :--- | :---: | :--- | :--- |
| **R-GOV-01** | **Prompts Sin Versionar** | 🔴 Alto | Cambios en prompts de n8n que rompen el formato de respuesta del parser del backend. | Prohibir el uso de variables locales de prompts en n8n. La API .NET aborta la llamada si no existe una entrada aprobada en `PromptVersion`. |
| **R-GOV-02** | **Descartes Sesgados por IA** | 🔴 Alto | Demandas por discriminación algorítmica automatizada de postulantes. | Deshabilitar transiciones automáticas de descarte en el backend. Toda exclusión requiere justificación humana del reclutador. |
| **R-GOV-03** | **Prompts Duplicados en BD** | 🟡 Bajo | Confusión y duplicación de llamadas a la base de datos consumiendo recursos. | Restricción Unique Key en SQL Server `UQ_Prompt_Nombre` para evitar colisiones. |
| **R-GOV-04** | **Ejecuciones de IA Huérfanas** | 🟠 Medio | Ejecuciones que fallan a mitad de camino dejando registros colgados en n8n sin log transaccional. | Configurar middleware de timeout en .NET 8 que escriba en `AgentExecution` como `ResultadoStatus = 'Fallo'` si no hay callback en 60 segundos. |
| **R-GOV-05** | **Saturación por Cuotas en el LLM** | 🔴 Alto | Denegación de servicio (HTTP 429) por picos de tráfico en análisis de postulantes. | Implementación de `SemaphoreSlim` limitando a 10 llamadas concurrentes y middleware de Token Bucket (50 req/min), redirigiendo a cola asíncrona local. |

---

# Recomendaciones de Gobierno

1. **Establecer Comité de Prompts:** Toda nueva versión de prompts que pretenda subirse a producción debe ser aprobada previamente por el Jefe de Reclutamiento y el Oficial de Seguridad (CISO) en el ambiente de QA.
2. **Alertas automáticas de costo de tokens:** Configurar un workflow de alerta en n8n que notifique si una única ejecución supera el costo de $0.10 USD (posible loop infinito de entrada).
3. **Auditoría periódica de explicabilidad:** Realizar muestreos mensuales de 50 currículums evaluados por el matching de IA para validar que la justificación textual del backend coincida de manera coherente con el currículum original del postulante.

---

## Cumplimiento de Lineamientos del Proyecto
**Resultado de la Evaluación:** `APPROVED`

### Justificación del Resultado:
* **Gobernanza y Versionado:** Todos los prompts se estructuran bajo un catálogo de versionado en base de datos inmutable.
* **Supervisión Humana Obligatoria:** Se establecen los bloqueos y políticas para impedir decisiones transaccionales autónomas por los agentes.
* **Trazabilidad y Explicabilidad:** Se describe el modelo de logs `AgentExecution` e inyección de `CorrelationId` alineado con la seguridad del PRD.
* **Mitigación de PII:** Se ratifican las restricciones de anonimización para cumplir con las directrices de privacidad de la organización.
