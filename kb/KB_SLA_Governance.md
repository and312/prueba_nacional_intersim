# KB_SLA_Governance — Gobierno de SLAs, Alertas y Escalamientos
## Sistema Inteligente de Reclutamiento — Nacional Seguros

> **Versión:** 1.0.0 | **Estado:** PENDIENTE DE VALIDACIÓN
> **Última actualización:** 2026-06-20
> **Propietario:** NacionalSeguros_ProjectAuditor / Lead Operations Manager
> **Audiencia:** Backend (.NET 8), Workflows (n8n), Frontend (Angular), QA, Auditoría, Liderazgo Operativo

---

## Control de Versiones Documentales

| Versión | Fecha | Autor | Rol | Cambios Realizados |
| :---: | :---: | :---: | :---: | :--- |
| `1.0.0` | 2026-06-20 | Antigravity | Technical Writer / SLA Specialist | Creación de la especificación de SLAs por módulo, matriz de alertas y escalamientos jerárquicos. |

---

## Fuentes Oficiales

Esta KB se rige obligatoriamente por los siguientes documentos del proyecto:

| Documento | Rol en esta KB |
| :--- | :--- |
| `CONSTITUCION_PROYECTO.md` | Directrices de control operativo, auditoría y observabilidad |
| `ANALISIS_FUNCIONAL.md` | Flujos de negocio y plazos máximos por rol transaccional |
| `DISEÑO_ERD.md` | Estructura de las tablas `SLA` y `SLAExecution` en la base de datos |
| `DICCIONARIO_DATOS.md` | Campos y nulidades del contexto de reportería de tiempos y métricas |
| `ARQUITECTURA_BACKEND.md` | Servicios de control de tiempo y APIs de consulta de SLAs |
| `ARQUITECTURA_N8N.md` | Orquestación de cron jobs de alertas y recordatorios a usuarios |
| `POLITICAS_DE_SEGURIDAD.md` | RBAC para escalamientos y firmas digitales de responsables |
| `AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md` | Trazabilidad del CorrelationId en el log de alertas y vencimientos |
| [KB_StateMachine](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_StateMachine.md) | Estados y eventos de transiciones que marcan inicio/fin de SLAs |
| [KB_MasterData](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_MasterData.md) | Configuración jerárquica de herencias y sobrescrituras de parámetros |
| [KB_CatalogoWorkflows](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/kb/KB_CatalogoWorkflows.md) | Workflow de control de SLAs (`WF-08-MonitoreoSLA`) en n8n |

---

## Principios Rectores del Gobierno de SLAs

1. **Monitoreo No Bloqueante:** El vencimiento de un SLA nunca detiene el flujo transaccional del negocio. En su lugar, el sistema cambia la metadata, emite alertas y deriva tareas, pero permite continuar el proceso para evitar cuellos de botella irrecuperables.
2. **Registro Histórico Obligatorio:** Cada cambio, pausa, cumplimiento o vencimiento de un SLA de negocio debe registrarse inmutablemente en la tabla `SLAExecution`, asociándolo al `CorrelationId` de la transacción que originó el cambio.
3. **Idempotencia de Alertas:** El motor de alertas de n8n no debe duplicar notificaciones para un mismo hito de vencimiento. Todo envío de alerta requiere verificación previa de no existencia del registro en la tabla `AuditLog` o `SLAExecution`.
4. **Supervisión Jerárquica:** El escalamiento de un proceso atascado sigue de forma estricta la estructura de roles (RBAC) definida en la organización. Ningún escalamiento salta de nivel sin registrar el responsable anterior y la justificación.
5. **Cero Valores Duros:** Todos los límites de tiempo en días u horas deben consumirse dinámicamente de la tabla `SLA` a través de la API del backend, permitiendo ajustes rápidos por el Administrador.

---

# Catálogo Oficial de SLAs del Sistema

A continuación se detallan los Acuerdos de Nivel de Servicio (SLA) vigentes para cada módulo de la Fase 1 del proyecto:

---

## 1. Módulo: Solicitudes de Personal

* **SLA-SOL-01: Creación de Solicitud**
  * **Objetivo:** Plazo máximo para que un solicitante complete una solicitud en borrador antes de archivarla.
  * **Límite de Tiempo:** 5 días calendario (120 horas).
  * **Estado de Inicio:** `SOL-01 BORRADOR`.
  * **Estado de Fin:** `SOL-02 ENVIADA` (o `SOL-07 CANCELADA`).
* **SLA-SOL-02: Revisión de Consistencia (IA)**
  * **Objetivo:** Plazo máximo de procesamiento del Agente de Solicitud en n8n.
  * **Límite de Tiempo:** 2 minutos.
  * **Estado de Inicio:** `SOL-02 ENVIADA`.
  * **Estado de Fin:** `SOL-03 EN_REVISION` (o `SOL-04 OBSERVADA`).
* **SLA-SOL-03: Aprobación / Decisión Funcional**
  * **Objetivo:** Plazo máximo para que el Gerente/Decisor apruebe o rechace la solicitud consistente.
  * **Límite de Tiempo:** 3 días hábiles (72 horas).
  * **Estado de Inicio:** `SOL-03 EN_REVISION`.
  * **Estado de Fin:** `SOL-05 APROBADA` (o `SOL-06 RECHAZADA` / `SOL-07 CANCELADA`).

---

## 2. Módulo: Perfiles de Cargo

* **SLA-PERF-01: Generación de Profesiograma (IA)**
  * **Objetivo:** Plazo para que el Agente de Perfil retorne el JSON estructurado.
  * **Límite de Tiempo:** 5 minutos.
  * **Estado de Inicio:** Transición de Solicitud a `APROBADA` (crea perfil en `PERF-01 BORRADOR`).
  * **Estado de Fin:** `PERF-02 EN_REVISION`.
* **SLA-PERF-02: Revisión y Ajustes por RRHH**
  * **Objetivo:** Plazo para que el analista complete la edición manual del borrador.
  * **Límite de Tiempo:** 2 días hábiles (48 horas).
  * **Estado de Inicio:** `PERF-02 EN_REVISION`.
  * **Estado de Fin:** `PERF-04 APROBADO` (o `PERF-03 OBSERVADO` / `PERF-05 RECHAZADO`).

---

## 3. Módulo: Gestión de Vacantes

* **SLA-VAC-01: Publicación en Canales**
  * **Objetivo:** Plazo para que el reclutador configure los canales y publique la vacante tras aprobarse el perfil.
  * **Límite de Tiempo:** 24 horas hábiles.
  * **Estado de Inicio:** `VAC-01 BORRADOR` (creada automáticamente tras aprobar perfil).
  * **Estado de Fin:** `VAC-02 PUBLICADA`.
* **SLA-VAC-02: Cierre de Vacante (Cobertura)**
  * **Objetivo:** Plazo máximo operativo para cubrir la vacante (candidato contratado).
  * **Límite de Tiempo:** 30 días calendario (configurable por tipo de vacante).
  * **Estado de Inicio:** `VAC-02 PUBLICADA` / `VAC-03 EN_PROCESO`.
  * **Estado de Fin:** `VAC-05 CUBIERTA` (o `VAC-06 CERRADA` / `VAC-07 CANCELADA`).

---

## 4. Módulo: Pipeline de Reclutamiento

* **SLA-POST-01: Evaluación Curricular y Matching (IA)**
  * **Objetivo:** Plazo para que el Agente de Matching y Scoring analicen el currículum de un postulante.
  * **Límite de Tiempo:** 10 minutos.
  * **Estado de Inicio:** `POST-01 CAPTADO` (CV cargado).
  * **Estado de Fin:** `POST-02 EN_REVISION_CV` (Score registrado).
* **SLA-POST-02: Preselección Manual**
  * **Objetivo:** Plazo para que el reclutador evalúe la recomendación de la IA y contacte al candidato.
  * **Límite de Tiempo:** 48 horas hábiles.
  * **Estado de Inicio:** `POST-02 EN_REVISION_CV`.
  * **Estado de Fin:** `POST-04 EN_CONTACTO` (o `POST-09 DESCARTADO`).

---

## 5. Módulo: Coordinación de Entrevistas

* **SLA-ENT-01: Programación Conversacional**
  * **Objetivo:** Plazo para que el Agente de Coordinación logre acordar una fecha con el candidato por WhatsApp.
  * **Límite de Tiempo:** 24 horas hábiles (Wait Node en n8n).
  * **Estado de Inicio:** `POST-05 ENTREVISTA_PEND`.
  * **Estado de Fin:** `ENT-01 AGENDADA`.
* **SLA-ENT-02: Confirmación de Cita**
  * **Objetivo:** Plazo para que entrevistador y candidato confirmen asistencia previa.
  * **Límite de Tiempo:** 12 horas antes de la cita.
  * **Estado de Inicio:** `ENT-01 AGENDADA`.
  * **Estado de Fin:** `ENT-02 CONFIRMADA` (o `ENT-03 REPROGRAMADA`).

---

## 6. Módulo: Oferta Salarial

* **SLA-OFE-01: Emisión de Oferta**
  * **Objetivo:** Plazo para generar y enviar la oferta económica tras finalizar las evaluaciones técnicas.
  * **Límite de Tiempo:** 48 horas hábiles.
  * **Estado de Inicio:** Postulante en `EN_EVALUACION` con notas cerradas.
  * **Estado de Fin:** `OFE-02 ENVIADA`.
* **SLA-OFE-02: Respuesta del Candidato (Aceptación / Rechazo)**
  * **Objetivo:** Plazo para que el postulante acepte o rechace la oferta digital.
  * **Límite de Tiempo:** 48 horas hábiles (Wait Node).
  * **Estado de Inicio:** `OFE-02 ENVIADA`.
  * **Estado de Fin:** `OFE-04 ACEPTADA` (o `OFE-05 RECHAZADA` / `OFE-06 EXPIRADA`).

---

# Matriz y Umbrales de Alertas Operativas

El control de tiempo se basa en un esquema de 4 alertas progresivas monitoreadas de forma asíncrona por el workflow `WF-08-MonitoreoSLA`:

```
| 0% (Inicio) ────────── 50% ────────── 80% ────────── 100% (Límite) ────────── 120% (Incumplimiento Crítico)
                         │             │              │                       │
                  Preventiva     Próximo Vencer    Incumplimiento          Crítica (Escalamiento)
```

| Nivel de Alerta | Porcentaje de Tiempo Transcurrido | Canal de Envío | Destinatario | Acción del Sistema |
| :--- | :---: | :--- | :--- | :--- |
| **1. Preventiva** | **50% del SLA** | Email / Notif Web | Responsable Asignado | Envía un recordatorio amistoso indicando los días restantes. |
| **2. Vencimiento Próximo**| **80% del SLA** | Email / Notif Web / WA | Responsable Asignado | Emite alerta con etiqueta "Urgente" y prioridad alta en el dashboard. |
| **3. Incumplimiento** | **100% del SLA** | Email / WA / Teams | Responsable + Líder | Envía reporte de incumplimiento del SLA y marca la vacante en rojo. |
| **4. Crítica (Escalamiento)**| **120% del SLA** | Correo Directivo / WA | Gerencia / Director | Dispara el workflow de escalamiento asignando el caso al superior. |

---

# Matriz de Responsables y Reglas de Escalamiento

Cuando se supera el **120% del tiempo de un SLA** sin resolución, el sistema inicia las reglas de escalamiento automático para reasignar tareas o disparar notificaciones de nivel ejecutivo.

```
[Responsable Inicial: Operativo] ──> [120% SLA Vencido] ──> [Responsable Secundario: Gerente] ──> [150% SLA Vencido] ──> [Responsable Final: Director]
```

| Proceso / SLA | Responsable Inicial | Responsable Secundario (120%) | Responsable Final (150%) | Regla de Escalamiento Operativo |
| :--- | :--- | :--- | :--- | :--- |
| **SLA-SOL-03 (Decisión)**| Decisor del Área | Gerente del Área | VP de Talento Humano | Reasigna la firma de la solicitud al Gerente. Envía alerta WA. |
| **SLA-PERF-02 (RRHH)** | Analista de RRHH | Jefe de Reclutamiento | Gerente de RRHH | Deriva la edición del profesiograma al superior. Alerta en dashboard. |
| **SLA-VAC-02 (Cobertura)**| Reclutador Asignado | Gerente de RRHH | VP de Talento Humano | Alerta de desvío presupuestario e informe a la vicepresidencia. |
| **SLA-ENT-02 (Confirmar)**| Reclutador Asignado | Jefe de Reclutamiento | Gerente de RRHH | Cancela entrevista y reasigna para agendamiento manual. |
| **SLA-OFE-02 (Aceptar)** | Candidato / Postulante | Reclutador Asignado | Gerente de RRHH | El workflow expira la oferta y notifica al reclutador para llamar. |

---

# Indicadores de Desempeño Operativo (KPIs)

Para evaluar la eficiencia operativa y detectar cuellos de botella, la base de datos almacena en `MetricSnapshot` los siguientes agregados calculados por el Agente Analítico (`AGE-07`):

1. **Cumplimiento de SLA (%):**
   $$\text{Cumplimiento} = \left( \frac{\text{Total Procesos Cerrados dentro del SLA}}{\text{Total Procesos Cerrados}} \right) \times 100$$
2. **Tasa de Incumplimiento (%):**
   $$\text{Incumplimiento} = \left( \frac{\text{Total Procesos Cerrados fuera del SLA}}{\text{Total Procesos Cerrados}} \right) \times 100$$
3. **Tiempo Promedio de Cobertura (Days):** Promedio de días transcurridos desde `SOL-05 APROBADA` hasta `CON-06 COMPLETADA`.
4. **Tiempos Límites (Máximo / Mínimo):** Detección de los casos extremos (el proceso más rápido y el más demorado por departamento).

---

# Integración con Auditoría y Trazabilidad

Conforme a las `POLITICAS_DE_SEGURIDAD.md`, toda alerta o escalamiento debe registrar una traza inmutable en base de datos.

### Estructura de Registro del Log de SLA (`SLAExecution`)

```sql
-- Representación física de la auditoría en base de datos
SELECT 
    SLAExecutionId,
    SLAId,
    Entidad,       -- 'Solicitud', 'Vacante', 'Postulante'
    EntidadId,
    EstadoId,
    FechaInicio,   -- Timestamp UTC de entrada al estado
    FechaLimite,   -- Calculada sumando DiasMaximos a FechaInicio
    FechaFin,      -- Timestamp UTC de salida
    Cumplido,      -- BIT (1: Sí, 0: No, NULL: En Proceso)
    ModifiedBy,    -- Usuario que resolvió o escaló el proceso
    CorrelationId  -- UUID propagado en la traza
FROM SLAExecution;
```

---

# Gestión de Feriados y Cálculo de Tiempos Hábiles

Para evitar falsos vencimientos de los SLAs durante fines de semana y días festivos declarados oficiales, el backend de .NET 8 centraliza la lógica de tiempos a través del servicio `SLAExecutionService` y la base de datos SQL Server 2022.

### Tabla Física `Feriado` en Base de Datos
Toda fecha correspondiente a feriados nacionales o sectoriales se almacena de forma estructurada para el cálculo matemático de días hábiles:

```sql
CREATE TABLE Feriado (
    FeriadoId INT IDENTITY(1,1) CONSTRAINT PK_Feriado PRIMARY KEY,
    Fecha DATE NOT NULL CONSTRAINT UQ_Feriado_Fecha UNIQUE,
    Descripcion NVARCHAR(150) NOT NULL,
    EsRecurrente BIT NOT NULL CONSTRAINT DF_Feriado_EsRecurrente DEFAULT 0,
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL CONSTRAINT DF_Feriado_CreatedDate DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2(7) NULL,
    DeletedBy NVARCHAR(100) NULL,
    DeletedDate DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Feriado_IsDeleted DEFAULT 0
);
```

### Algoritmo de Cálculo de Fecha Límite (`SLAExecutionService`)
Al iniciar una nueva etapa o transición de estado, el servicio calcula la `FechaLimite` excluyendo días no laborables de la siguiente forma:
1. **Paso de Fecha Inicial:** Se toma `FechaInicio` (en formato UTC) y los días máximos permitidos (`SLA.DiasMaximos`).
2. **Exclusión de Fines de Semana:** El algoritmo avanza día a día sumando tiempo. Si el día corresponde a un sábado o domingo, se ignora en la resta de días pendientes.
3. **Exclusión de Feriados:** En cada paso del ciclo, se consulta en la caché (Caché Distribuida o `IMemoryCache` con carga desde la tabla `Feriado`) si la fecha coincide con un registro activo (`IsDeleted = 0`) en la tabla `Feriado`. De ser así, se ignora en la resta de días pendientes.
   * Si `EsRecurrente = 1`, se compara únicamente el día y mes del año en curso.
   * Si `EsRecurrente = 0`, se compara la fecha exacta (`año-mes-día`).
4. **Almacenamiento de la Fecha Límite:** La fecha final calculada se persiste en `SLAExecution.FechaLimite` en formato UTC.

---

# Reglas de Escalamiento Seguro e Integridad de Usuarios

El proceso de escalamiento y reasignación automática de tareas ante retrasos (120% del SLA) debe garantizar que el nuevo responsable asignado sea un usuario activo en el sistema.

### Validación de Estado de Usuarios (RBAC y Auditoría)
1. **Regla de Validación:** Antes de reasignar una solicitud, vacante o perfil de cargo en el workflow de escalamiento, el backend ejecuta una consulta de comprobación sobre el `UsuarioId` del destinatario secundario/terciario:
   * Debe cumplir obligatoriamente: `Estado = 'Activo'` (que equivale a `IsActive = 1` en la lógica de negocio) e `IsDeleted = 0`.
2. **Mitigación ante Inactividad:** Si el superior jerárquico directo o responsable secundario configurado se encuentra inactivo (`IsActive = 0` o `IsDeleted = 1`), el backend intercepta el flujo de reasignación y desvía la asignación hacia el rol genérico de supervisión del departamento correspondiente (ej. *Jefe de Reclutamiento* o *Gerente de RRHH*) que se encuentre activo.
3. **Bitácora del Desvío:** Todo desvío de escalamiento debido a inactividad del responsable debe registrar un evento específico en `AuditLog` con la acción `ESCALATION_REDIRECTED_INACTIVE_USER`, detallando el ID del usuario inactivo original y el ID del destinatario final asignado.

---

# Riesgos Detectados y Mitigaciones

| ID | Riesgo | Severidad | Impacto | Mitigación Propuesta |
| :---: | :--- | :---: | :--- | :--- |
| **R-SLA-01** | **SLA Inconsistente por Feriados** | 🟠 Medio | Alertas de vencimiento falsas enviadas en días no laborables o feriados. | La API de .NET 8 centraliza la exclusión en `SLAExecutionService` leyendo de la tabla `Feriado` en caché distribuida para evitar sobrecarga del motor SQL. |
| **R-SLA-02** | **Bucle de Alertas por Caída de n8n** | 🔴 Alto | Si n8n no logra marcar la alerta como enviada en BD, puede reenviar emails repetidamente. | Implementar un índice único filtrado en base de datos y validación de bandera `AlertaEnviada` en la transacción. |
| **R-SLA-03** | **Procesos Huérfanos sin SLA** | 🟠 Medio | Etapas que no cuentan con SLA configurado en la base de datos, perdiendo trazabilidad. | Constraint NOT NULL en la tabla `Estado` obligando a que todo estado intermedio tenga asociado un ID de la tabla `SLA`. |
| **R-SLA-04** | **Escalamientos a Responsables Inactivos** | 🟠 Medio | Escalar una tarea de aprobación a un gerente que está marcado como de baja lógica o inactivo. | La lógica del backend valida `IsActive = 1` e `IsDeleted = 0` antes de proceder con el cambio de responsable, redirigiendo el escalamiento a un supervisor alterno activo si falla. |

---

# Recomendaciones de Operación

1. **Dashboard de Semáforo en Angular:** Implementar componentes visuales coloridos (Verde/Amarillo/Rojo) en el frontend del Reclutador para monitorear el tiempo restante de las vacantes en tiempo real.
2. **Pausa Controlada de SLAs:** Permitir pausar temporalmente el conteo del SLA (ej. vacantes en estado `PAUSADA` o postulantes en `DOC_OBSERVADA` esperando documentos). Toda pausa requiere motivo obligatorio registrado en `StateHistory`.
3. **Cálculo de SLAs en el Backend:** No calcular plazos en n8n ni en Angular. Las fechas límite deben ser calculadas de forma centralizada por el Backend de .NET 8 utilizando la hora del servidor en UTC.

---

## Cumplimiento de Lineamientos del Proyecto
**Resultado de la Aprobación:** `APPROVED`

### Justificación del Resultado:
* **Gobernanza de Tiempos:** Se definen los plazos de SLAs y límites exigidos por el PRD para los 6 dominios principales de la Fase 1.
* **Trazabilidad de Alertas:** Se detalla el modelo de logs e inyección de `CorrelationId` sobre las tablas inmutables del sistema.
* **Mitigación de Feriados:** Se incorpora el requerimiento de excluir fines de semana y feriados del cómputo.
* **Escalamientos Seguros:** Se definen reglas de asignación y verificación de usuarios inactivos bajo controles RBAC.
