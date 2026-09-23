# KB_Observability — Estrategia de Observabilidad, Monitoreo y Diagnóstico
## Sistema Inteligente de Reclutamiento — Nacional Seguros

> **Versión:** 1.0.0 | **Estado:** PENDIENTE DE VALIDACIÓN
> **Última actualización:** 2026-06-20
> **Propietario:** NacionalSeguros_ProjectAuditor / Lead DevOps Engineer
> **Audiencia:** Backend (.NET 8), Frontend (Angular), n8n, QA, Seguridad, Operaciones TI, Soporte L1/L2

---

## Control de Versiones Documentales

| Versión | Fecha | Autor | Rol | Cambios Realizados |
| :---: | :---: | :---: | :---: | :--- |
| `1.0.0` | 2026-06-20 | Antigravity | Technical Writer / DevOps Engineer | Creación del estándar de observabilidad, catálogo de logs/métricas, diseño de trazabilidad e incidentes. |

---

## Fuentes Oficiales

Esta KB se rige obligatoriamente por los siguientes documentos del proyecto:

| Documento | Rol en esta KB |
| :--- | :--- |
| `CONSTITUCION_PROYECTO.md` | Restricciones de observabilidad, salud de APIs y logs estructurados |
| `ANALISIS_FUNCIONAL.md` | Flujos y procesos críticos de negocio que requieren monitoreo |
| `DISEÑO_ERD.md` | Esquemas físicos de tablas de logs (`AuditLog`, `AuditDetail`, `AgentExecution`) |
| `DICCIONARIO_DATOS.md` | Columnas y campos obligatorios de telemetría de IA y base de datos |
| `ARQUITECTURA_BACKEND.md` | Estándares de logging estructurado JSON en ASP.NET Core |
| `ARQUITECTURA_N8N.md` | Telemetría de tokens y observabilidad de errores en n8n workflows |
| `POLITICAS_DE_SEGURIDAD.md` | Registro de accesos de autenticación/MFA e inmutabilidad de bitácoras |
| `AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md` | Requisitos de trazabilidad del CorrelationId extremo a extremo |
| [KB_StateMachine](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_StateMachine.md) | Control de tiempos de SLA e historial de transiciones de estados |
| [KB_MasterData](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_MasterData.md) | Parametrización global de alertas y escalamientos del sistema |
| [KB_SLA_Governance](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_SLA_Governance.md) | Acuerdos de nivel de servicio (SLAs) operacionales por módulo |

---

# Estrategia de Observabilidad

La estrategia de observabilidad del Sistema Inteligente de Reclutamiento (SIR) se compone de cuatro dimensiones complementarias para asegurar la salud técnica y funcional de la plataforma:

```
┌─────────────────────────────────────────────────────────┐
│                      OBSERVABILIDAD                     │
├───────────────────────────┬─────────────────────────────┤
│  1. Observabilidad Técnica │  2. Observabilidad Funcional│
│  (CPU, RAM, DB, APIs)     │  (Estados, SLAs, Procesos)  │
├───────────────────────────┼─────────────────────────────┤
│  3. Observabilidad Operat.│  4. Observabilidad Negocio  │
│  (Logs, Errores, Fallas)  │  (Costos IA, KPIs RRHH)     │
└───────────────────────────┴─────────────────────────────┘
```

1. **Observabilidad Técnica:** Monitoreo del estado físico de los servidores, latencia de base de datos SQL Server 2022, uso de recursos (CPU, RAM) de la API .NET 8 y conectividad con n8n.
2. **Observabilidad Funcional:** Seguimiento de las máquinas de estado por dominio. Mide la velocidad de las solicitudes en el pipeline, detecta bloqueos en las transiciones y audita los SLAs operacionales.
3. **Observabilidad Operativa:** Diagnóstico rápido de errores mediante logs JSON estructurados. Permite identificar fallos en las integraciones externas (WhatsApp, SMTP, Outlook) y audita intentos de accesos no autorizados.
4. **Observabilidad de Negocio:** Telemetría de la Inteligencia Artificial. Mide el consumo financiero de tokens de IA por agente, latencia de LLMs y efectividad del sourcing/matching.

---

# Logging (Bitácoras Estructuradas)

Para cumplir con las `POLITICAS_DE_SEGURIDAD.md`, todos los logs del sistema se emiten en **formato estructurado JSON** a través de consolas y archivos inmutables.

### Catálogo de Logs Oficiales

| ID | Log | Evento Registrado | Origen | Severidad | Formato | Retención | Responsable |
| :---: | :--- | :--- | :--- | :---: | :--- | :---: | :--- |
| **L-01** | **Aplicación** | Inicio de servicios, depuración general. | .NET Core | Info | JSON | 30 días | Lead Dev |
| **L-02** | **Workflows** | Trigger de workflows n8n, llamadas a LLMs. | n8n | Info | JSON | 6 meses | n8n Designer |
| **L-03** | **Integración**| Peticiones y respuestas externas (ej. Outlook API). | HttpClient | Warn / Error | JSON | 1 año | Lead Dev |
| **L-04** | **Autenticación**| Login exitoso, fallas de contraseña, MFA. | Identity | Info / Warn | JSON | 5 años | CISO |
| **L-05** | **Autorización** | Denegación de acceso RBAC a endpoints. | Security | Warn | JSON | 5 años | CISO |
| **L-06** | **Auditoría** | Cambios en datos maestros y tablas transaccionales.| SQL Trigger | Info | SQL (AuditLog) | 5 años (Inmutable)| DBA |
| **L-07** | **Errores** | Excepciones no controladas y fallas en runtime. | Middleware | Error | JSON | 1 año | Lead Dev |
| **L-08** | **Seguridad** | Ataques SQL Injection detectados, IPs bloqueadas. | WAF / API | Critical | JSON | 5 años | CISO |

### Configuración y Sanitización de Logs con Serilog
Para evitar la fuga accidental de credenciales o información confidencial de los candidatos en los logs del servidor (incluso en trazas de error de nivel Debug), el motor de Serilog se configura en `Program.cs` inyectando un enriquecedor de sanitización y aplicando expresiones regulares precompiladas:

1. **Patrones Regex de Enmascaramiento:**
   - **Tokens JWT:** `bearer\s+[a-zA-Z0-9\-\._~\+\/]+=*` y `TokenJwt\s*:\s*\"[^\"]+\"`
   - **Contraseñas / Secretos:** `password=\w+`, `clave=\w+` y `client_secret=\w+`
   - **Datos Confidenciales:** Los campos JSON `BandaSalarialMin`, `BandaSalarialMax`, `PretensionSalarial` y `ScoreCoincidencia` son reemplazados por `'*****'` en el payload antes de escribir el evento en el log.
2. **Sanitización de Excepciones de Base de Datos:**
   - El enriquecedor de Serilog analiza las excepciones de tipo `SqlException` o `DbUpdateException`.
   - Remueve de manera preventiva los valores de parámetros contenidos en las sentencias SQL fallidas y oculta la cadena de conexión (`ConnectionString`) de la pila de llamadas (StackTrace) de la excepción, registrando únicamente el código de error nativo de SQL Server (ej: error 50000 para dependencias jerárquicas circulares o 2627 para violación de índice único).

---

# Catálogo Oficial de Métricas

Las métricas cuantifican el rendimiento y disponibilidad del sistema para activar alarmas ante anomalías:

---

## 1. Métricas Técnicas (Backend, BD e APIs)

* **MET-TEC-01: Latencia de la API REST**
  * **Descripción:** Tiempo promedio de respuesta de los endpoints HTTP expuestos.
  * **Fórmula:** $\text{Latencia} = \frac{\sum \text{Tiempo Respuesta Endpoints}}{\text{Total Peticiones}}$
  * **Unidad:** Milisegundos (ms) | **Frecuencia:** Tiempo Real | **Umbral:** $< 300\text{ ms}$ en promedio.
* **MET-TEC-02: Tasa de Errores HTTP**
  * **Descripción:** Porcentaje de respuestas HTTP 5xx respecto al total de peticiones.
  * **Fórmula:** $\text{Tasa Errores} = \left( \frac{\text{Peticiones con Código 5xx}}{\text{Total Peticiones}} \right) \times 100$
  * **Unidad:** Porcentaje (%) | **Frecuencia:** Tiempo Real | **Umbral:** $< 0.5\%$.
* **MET-TEC-03: Conexiones Activas de Base de Datos**
  * **Descripción:** Número de hilos simultáneos conectados a la base de datos `SIR_NacionalSeguros`.
  * **Fórmula:** Recuento directo de la DMV `sys.dm_exec_sessions`.
  * **Unidad:** Conexiones | **Frecuencia:** 1 minuto | **Umbral:** $< 150$ conexiones.

---

## 2. Métricas de Automatización e IA (n8n y LLMs)

* **MET-IA-01: Costo Acumulado de Tokens**
  * **Descripción:** Costo estimado acumulado en USD por llamadas al LLM.
  * **Fórmula:** Suma de `CostoEstimado` en la tabla `AgentExecution`.
  * **Unidad:** Dólares (USD) | **Frecuencia:** 1 hora | **Umbral:** $< \$10.00\text{ USD}$ diarios (Alerta preventiva).
* **MET-IA-02: Latencia de Inferencia LLM**
  * **Descripción:** Tiempo de respuesta de las APIs del LLM integradas.
  * **Fórmula:** Promedio del campo `DuracionMs` en `AgentExecution`.
  * **Unidad:** Milisegundos (ms) | **Frecuencia:** Tiempo Real | **Umbral:** $< 5000\text{ ms}$ por inferencia.

---

## 3. Métricas de Negocio y SLAs Operativos

* **MET-OPS-01: Cumplimiento de SLA**
  * **Descripción:** Porcentaje de tareas críticas cerradas dentro del tiempo del SLA.
  * **Fórmula:** $\left( \frac{\text{SLAExecution con Cumplido = 1}}{\text{Total SLAExecution Cerrados}} \right) \times 100$
  * **Unidad:** Porcentaje (%) | **Frecuencia:** 24 horas | **Umbral:** $\ge 90\%$.
* **MET-OPS-02: Tiempo Promedio de Cobertura**
  * **Descripción:** Promedio de días para cubrir una vacante desde su apertura.
  * **Fórmula:** Promedio de $\text{FechaCierre} - \text{FechaApertura}$ en la tabla `Vacante`.
  * **Unidad:** Días | **Frecuencia:** Semanal | **Umbral:** $< 30$ días.

---

# Matriz de Monitoreo Técnico

El monitoreo inspecciona continuamente las fuentes de datos y ejecuta acciones correctivas automáticas ante desviaciones de umbrales.

```
[Monitoreo Activo] ──> Detecta Desviación ──> Dispara Alerta ──> Ejecuta Acción Correctiva
```

| Componente | Fuente de Datos | Frecuencia | Umbral Crítico | Acción Correctiva Automática |
| :--- | :--- | :---: | :--- | :--- |
| **Disponibilidad** | Health Check (`/health`) | 30 segundos | 3 fallos consecutivos | Notificación crítica a TI; reinicio de contenedor app. |
| **Rendimiento** | CPU/Memoria del Servidor | 1 minuto | CPU $> 85\%$ por 5 min | Escalado horizontal del servicio web (Autoscaling). |
| **BBDD** | DMV de SQL Server | 5 minutos | Transacciones bloqueadas $> 5$ | Terminar (`KILL`) la sesión bloqueante más antigua. |
| **Seguridad** | Intentos fallidos de login | 1 minuto | $> 5$ intentos fallidos / IP | Bloqueo temporal de la IP de origen en el Firewall por 1 hora. |
| **Integraciones**| Peticiones HTTP a n8n | Tiempo Real | Código 504 (Timeout) | Activar el circuito de descarte (DLQ) en .NET. |

---

# Matriz de Alertas Técnicas

Las alertas técnicas notifican de manera automática al personal de operaciones a través de los canales adecuados.

| Alerta | Evento Disparador | Severidad | Canal de Envío | Responsable | Acción Requerida |
| :--- | :--- | :---: | :--- | :--- | :--- |
| **Informativa** | Generación de Backup de BD completo. | 🟢 Bajo | Email | DBA | Ninguna. Archivar reporte. |
| **Preventiva** | Espacio en disco del servidor $< 20\%$. | 🟡 Medio | Notif Web / Slack | Administrador TI | Depuración de archivos temporales e incrementar disco. |
| **Advertencia** | Latencia de base de datos $> 1000\text{ ms}$. | 🟠 Alto | Email / Teams | DBA | Revisar fragmentación de índices y consultas colgadas. |
| **Advertencia** | LLM HTTP 429 (Too Many Requests). | 🟠 Alto | Email / Slack | Lead DevOps / IA | Validar tasa de consumo y tiempos de espera en cola del semáforo. |
| **Crítica** | API de n8n no responde (Error 502/504).| 🔴 Crítico | WA / SMS | Lead DevOps | Reiniciar el servicio de n8n; validar conectividad de red. |
| **Crítica** | Circuito Abierto de Integración (Polly Circuit Breaker). | 🔴 Crítico | WA / Teams | Lead DevOps | Investigar caída del servicio externo (n8n o AD) y restaurar estabilidad. |

---

# Matriz de Dashboards Operativos y Ejecutivos

Los dashboards del sistema visualizan los datos estructurados en tiempo real para diferentes audiencias organizacionales:

| Dashboard | Indicadores Clave Mostrados | Actualización | Usuarios Autorizados (RBAC) | Fuente de Datos |
| :--- | :--- | :---: | :--- | :--- |
| **Operaciones** | Estado de servidores, tasa de errores de APIs, latencia de base de datos. | 1 minuto | Administrador, DevOps | API `/health`, DMV SQL |
| **Reclutamiento** | Vacantes abiertas, postulantes en pipeline por estado, rendimiento de fuentes. | 15 minutos | RRHH, Reclutador | Vistas SQL (`vw_vacante_pipeline`) |
| **Tecnología / IA** | Ejecuciones de agentes, costo de tokens en USD, latencia del LLM. | 5 minutos | Administrador, Lead Architect | Tabla `AgentExecution` |
| **Seguridad** | Logins erróneos, bloqueos de IP, auditoría de accesos privilegios. | Tiempo Real | CISO, Auditor | Tabla `HistorialAcceso` |
| **Cumplimiento SLA** | Tasa de SLAs vencidos por área, alarmas de vencimiento próximo activas. | 10 minutos | Gerente RRHH, Directores | Tabla `SLAExecution` |
| **Dirección** | KPIs consolidados (tiempo promedio de cobertura, tasa de aceptación de ofertas), embudo de conversión de reclutamiento, alertas de SLAs (SLA Heatmap), costo acumulado de tokens IA en USD, trazabilidad de accesos (Local vs. AD) y alertas de seguridad. | 24 horas | Decisor, RRHH, Auditor | Tabla `MetricSnapshot`, `AgentExecution`, `HistorialAcceso`, `SLAExecution` |

---

# Trazabilidad Extremo a Extremo (Correlation ID)

Para diagnosticar problemas en un sistema asíncrono y desacoplado, se establece la propagación estricta de la clave `CorrelationId` en todas las capas tecnológicas:

```
[Angular UI]
   │  (Genera CorrelationId en Interceptor HTTP)
   ▼
[API Backend .NET 8]
   │  (Propaga CorrelationId en Headers HTTP y escribe a AuditLog)
   ▼
[Workflows n8n]
   │  (Recibe CorrelationId, registra en AgentExecution y llama a IA)
   ▼
[Sistemas Externos] (Guardan CorrelationId en cabeceras de comunicación)
```

### Componentes de la Traza:
1. **Correlation ID:** UUID único generado por el frontend de Angular al iniciar una acción del usuario o por el backend .NET al recibir peticiones externas. Viaja en las cabeceras HTTP como `X-Correlation-ID`.
2. **Request ID:** Identificador único de la petición HTTP local generado por el servidor de .NET 8.
3. **Workflow ID:** ID de ejecución de n8n (`WorkflowExecutionId`) asociada al correlation.
4. **Transaction ID:** ID de transacción del Unit of Work en base de datos.
5. **Audit ID:** Clave primaria inmutable en `AuditLog` para auditorías de no repudio.

---

# Gestión de Incidentes Técnicos

Cuando se activa una **alerta crítica**, el equipo de operaciones inicia el protocolo de gestión de incidentes:

```
[1. Detección de Alerta] ──> [2. Clasificación (L1/L2/L3)] ──> [3. Mitigación/Resolución] ──> [4. Cierre y Post-Mortem]
```

### 1. Clasificación y Severidad del Incidente:
* **Severidad 1 (Crítico):** Pérdida completa de funcionalidad de producción (ej. base de datos inaccesible, API .NET caída). SLA de resolución: $< 2$ horas.
* **Severidad 2 (Alto):** Caída de un módulo no crítico o fallo temporal de IA (ej. n8n no responde pero el sitio web funciona). SLA de resolución: $< 8$ horas.
* **Severidad 3 (Medio):** Fallo cosmético en vistas o lentitud aceptable en reportería. SLA de resolución: $< 24$ horas.

### 2. Protocolo de Resolución y Cierre:
1. **Mitigación:** Aplicar acciones inmediatas (ej. reiniciar servicios, activar failover de BD).
2. **Diagnóstico:** Buscar la causa raíz filtrando los logs inmutables usando el `CorrelationId` del incidente.
3. **Resolución:** Implementar el parche definitivo.
4. **Cierre:** Registrar la hora de conclusión y completar el informe *Post-Mortem* de errores.

---

# Riesgos Detectados y Mitigaciones

| ID | Riesgo | Severidad | Impacto | Control y Mitigación Propuesta |
| :---: | :--- | :---: | :--- | :--- |
| **R-OBS-01** | **Logs Sin Estructura (Texto Plano)** | 🟠 Medio | Pérdida de capacidad de análisis de logs automatizado por herramientas de monitoreo. | Configurar Serilog en .NET 8 para forzar el formateo en JSON de todas las salidas de consola y archivos. |
| **R-OBS-02** | **Fuga de Secretos en Logs** | 🔴 Alto | Connection strings o tokens expuestos en la traza de excepciones de la API en producción. | Implementar un filtro sanitizador en el Logger que reemplace cualquier coincidencia de patrones sensibles (ej. 'password', 'token') por asteriscos. |
| **R-OBS-03** | **Corte de Traza en n8n** | 🔴 Alto | n8n no propagando el `CorrelationId` al llamar al callback de .NET, rompiendo la trazabilidad. | El middleware del callback del backend validará obligatoriamente la presencia de la UUID en la cabecera, rechazando peticiones sin ella. |
| **R-OBS-04** | **Llenado de Disco por Logs Infinitos** | 🟠 Medio | Colapso del servidor por espacio insuficiente de almacenamiento debido a logs de depuración (Debug). | Configurar rolling files diarios y establecer el nivel de log por defecto en `Information` para producción. |

---

# Recomendaciones Técnicas

1. **Configurar Alertas de SLA en n8n:** Utilizar el workflow `WF-08-MonitoreoSLA` para enviar de forma programada los vencimientos de SLAs a un canal central en Teams o Slack de RRHH.
2. **Implementar Health Checks robustos:** No retornar únicamente un JSON vacío en el endpoint `/health`. Validar internamente la conexión física a SQL Server (`SELECT 1`) y la disponibilidad del webhook de n8n.
3. **Cifrado de Datos de Telemetría:** Asegurar que los tokens e inputs sensibles de la IA persistidos en la tabla `AgentExecution` se mantengan protegidos y con acceso de lectura restingido.

---

## Cumplimiento de Lineamientos del Proyecto
**Resultado de la Aprobación:** `APPROVED`

### Justificación del Resultado:
* **Observabilidad Integral:** Se describen las 4 capas de observabilidad (Técnica, Funcional, Operativa y de Negocio).
* **Logging Estructurado:** Se define el catálogo de logs forzando el formato JSON estructurado exigido.
* **Trazabilidad Extremo a Extremo:** Se establece el CorrelationId unificado propagado desde Angular hasta las APIs de integraciones externas.
* **Incidentes y SLAs:** Se incorpora la matriz de alertas progresivas de SLAs y el protocolo de escalamiento e incidentes.
