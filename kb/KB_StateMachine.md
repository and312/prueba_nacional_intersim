# KB_StateMachine — Motor de Estados Oficial
## Sistema Inteligente de Reclutamiento — Nacional Seguros

> **Versión:** 1.0.0 | **Estado:** BORRADOR PENDIENTE DE VALIDACIÓN
> **Última actualización:** 2026-06-20
> **Propietario:** NacionalSeguros_StateMachineArchitect
> **Audiencia:** Backend (.NET 8), Frontend (Angular), SQL Server, n8n, QA, Arquitectura

---

## Fuentes Oficiales

Esta KB se rige obligatoriamente por:

| Documento                            | Rol en esta KB                                                 |
|--------------------------------------|----------------------------------------------------------------|
| `CONSTITUCION_PROYECTO.md`           | Principios rectores y restricciones absolutas                  |
| `ANALISIS_FUNCIONAL.md`              | Flujos funcionales validados por dominio                       |
| `DISEÑO_ERD.md`                      | Modelo de datos que soporta los estados                        |
| `DICCIONARIO_DATOS.md`               | Definición canónica de campos de estado                        |
| `ARQUITECTURA_BACKEND.md`            | Implementación del motor de estados en .NET 8                  |
| `ARQUITECTURA_N8N.md`                | Workflows activados por transiciones                           |
| `POLITICAS_DE_SEGURIDAD.md`          | RBAC, roles autorizados por transición                         |
| `AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md`| StateHistory, AuditLog, CorrelationId                          |
| PRD completo (KB_01, KB_02)          | Requisitos funcionales y reglas de negocio                     |

---

## Principios Rectores del Motor de Estados

1. **Separación estricta** — Cada dominio tiene su propia máquina de estados independiente.
2. **Transiciones controladas** — Ninguna transición puede ocurrir sin evento disparador, rol autorizado y validaciones previas.
3. **Trazabilidad total** — Toda transición genera `StateHistory` + `AuditLog` + timestamp UTC.
4. **Sin hardcoding** — Los estados se almacenan en catálogo parametrizable, no en código.
5. **Compatibilidad n8n** — Toda transición emite un evento de dominio consumible por n8n.
6. **RBAC obligatorio** — Toda transición valida rol y permiso antes de ejecutarse.

---

## Convenciones de Esta KB

### Tipos de Estado

| Tipo       | Descripción                                      | Símbolo |
|------------|--------------------------------------------------|---------|
| Inicial    | Estado de entrada al dominio                     | ⬛       |
| Intermedio | Estado de proceso activo                         | 🔵      |
| Final OK   | Estado de cierre exitoso                         | ✅      |
| Final NOK  | Estado de cierre negativo                        | ❌      |
| Bloqueado  | Estado en espera de acción externa               | ⏸️       |

### Estructura de Transición

Cada transición documenta:

| Campo               | Descripción                                        |
|---------------------|----------------------------------------------------|
| Origen              | Estado actual de la entidad                        |
| Destino             | Estado resultante tras la transición               |
| Evento              | Acción o condición que dispara la transición       |
| Rol autorizado      | Rol(es) que pueden ejecutar la transición          |
| Validaciones previas| Condiciones obligatorias antes de transitar        |
| Acciones posteriores| Efectos obligatorios tras la transición            |
| StateHistory        | ✅ Siempre / ❌ No aplica                           |
| AuditLog            | ✅ Siempre / ❌ No aplica                           |
| Evento n8n          | Nombre del evento de dominio emitido               |

---

## 1. DOMINIO: SOLICITUDES

### 1.1 Catálogo de Estados

| Código | Nombre              | Tipo        | Descripción                                                      |
|--------|---------------------|-------------|------------------------------------------------------------------|
| SOL-01 | BORRADOR            | ⬛ Inicial  | Solicitud creada pero no enviada para revisión                   |
| SOL-02 | ENVIADA             | 🔵 Proceso  | Solicitud enviada a RRHH para revisión inicial                   |
| SOL-03 | EN_REVISION         | 🔵 Proceso  | RRHH está evaluando la solicitud                                 |
| SOL-04 | OBSERVADA           | ⏸️ Bloqueada | Solicitud devuelta al solicitante con observaciones              |
| SOL-05 | APROBADA            | ✅ Final OK | Solicitud aprobada — genera Perfil automáticamente               |
| SOL-06 | RECHAZADA           | ❌ Final NOK| Solicitud rechazada con motivo obligatorio                       |
| SOL-07 | CANCELADA           | ❌ Final NOK| Solicitud cancelada por el solicitante o RRHH                    |
| SOL-08 | REABIERTA           | 🔵 Proceso  | Solicitud rechazada/cancelada reabierta con justificación        |

**Estado inicial:** `SOL-01 BORRADOR`
**Estados finales:** `SOL-05 APROBADA`, `SOL-06 RECHAZADA`, `SOL-07 CANCELADA`
**Estado reabierto:** `SOL-08 REABIERTA` → retoma flujo desde `SOL-02 ENVIADA`

---

### 1.2 Matriz de Transiciones — Solicitudes

| # | Origen       | Destino      | Evento Disparador          | Rol Autorizado              | Validaciones Previas                                      | Acciones Posteriores                                               | StateHistory | AuditLog | Evento n8n                    |
|---|-------------|-------------|----------------------------|-----------------------------|-----------------------------------------------------------|--------------------------------------------------------------------|-------------|---------|-------------------------------|
| 1 | BORRADOR    | ENVIADA     | Solicitante envía          | Solicitante, Jefe de Área   | Campos obligatorios completos; perfil de cargo existente  | Notificar a RRHH; asignar revisor                                  | ✅          | ✅      | `solicitud.enviada`           |
| 2 | ENVIADA     | EN_REVISION | RRHH inicia revisión       | Analista RRHH               | Solicitud en estado ENVIADA                               | Registrar revisor y timestamp de inicio                            | ✅          | ✅      | `solicitud.en_revision`       |
| 3 | EN_REVISION | OBSERVADA   | RRHH devuelve observación  | Analista RRHH               | Observación con descripción obligatoria                   | Notificar solicitante; registrar observaciones en detalle          | ✅          | ✅      | `solicitud.observada`         |
| 4 | OBSERVADA   | ENVIADA     | Solicitante corrige y reenvía | Solicitante, Jefe de Área | Observaciones atendidas; campos corregidos                | Notificar a RRHH; limpiar observaciones anteriores                 | ✅          | ✅      | `solicitud.reenviada`         |
| 5 | EN_REVISION | APROBADA    | RRHH aprueba               | Analista RRHH, Gerente RRHH | Sin observaciones pendientes; validación de presupuesto   | Crear Perfil automáticamente; notificar solicitante; activar SLA   | ✅          | ✅      | `solicitud.aprobada`          |
| 6 | EN_REVISION | RECHAZADA   | RRHH rechaza               | Analista RRHH, Gerente RRHH | Motivo de rechazo obligatorio                             | Notificar solicitante con motivo; cerrar SLA                       | ✅          | ✅      | `solicitud.rechazada`         |
| 7 | BORRADOR    | CANCELADA   | Solicitante cancela        | Solicitante, Jefe de Área   | Motivo de cancelación obligatorio                         | Registrar cancelación; notificar si tenía revisor asignado         | ✅          | ✅      | `solicitud.cancelada`         |
| 8 | ENVIADA     | CANCELADA   | Solicitante cancela        | Solicitante, Jefe de Área   | Motivo de cancelación obligatorio                         | Notificar revisor asignado; liberar asignación                     | ✅          | ✅      | `solicitud.cancelada`         |
| 9 | RECHAZADA   | REABIERTA   | RRHH o solicitante reabre  | Gerente RRHH, Director      | Justificación obligatoria; aprobación de nivel superior   | Notificar partes; reiniciar SLA; transitar a ENVIADA               | ✅          | ✅      | `solicitud.reabierta`         |
|10 | CANCELADA   | REABIERTA   | Solicitante reabre         | Gerente RRHH, Director      | Justificación obligatoria; validación de vigencia         | Notificar RRHH; reiniciar flujo desde ENVIADA                      | ✅          | ✅      | `solicitud.reabierta`         |

### 1.3 Transiciones Prohibidas — Solicitudes

| Origen      | Destino     | Motivo de Prohibición                                              |
|-------------|-------------|-------------------------------------------------------------------|
| BORRADOR    | APROBADA    | Salto de estado — debe pasar por revisión                         |
| BORRADOR    | EN_REVISION | Salto de estado — debe enviarse primero                           |
| APROBADA    | cualquiera  | Estado final — no puede transitar                                 |
| RECHAZADA   | EN_REVISION | Salto de estado — debe reabrir primero                            |
| CANCELADA   | ENVIADA     | Salto de estado — debe reabrir primero                            |

### 1.4 Reglas de Negocio — Solicitudes

- **RN-SOL-01:** Una solicitud solo puede enviarse si el perfil de cargo referenciado existe y está activo.
- **RN-SOL-02:** El rechazo siempre requiere motivo textual registrado en `AuditLog`.
- **RN-SOL-03:** La aprobación de una solicitud genera automáticamente un Perfil en estado `BORRADOR`.
- **RN-SOL-04:** La reapertura requiere aprobación de nivel jerárquico superior al que rechazó/canceló.
- **RN-SOL-05:** El SLA se activa al pasar a `EN_REVISION` y se cierra al llegar a estado final.

---

## 2. DOMINIO: PERFILES

### 2.1 Catálogo de Estados

| Código  | Nombre       | Tipo        | Descripción                                                        |
|---------|-------------|-------------|--------------------------------------------------------------------|
| PERF-01 | BORRADOR     | ⬛ Inicial  | Perfil creado automáticamente desde solicitud aprobada             |
| PERF-02 | EN_REVISION  | 🔵 Proceso  | RRHH está completando o revisando el perfil                        |
| PERF-03 | OBSERVADO    | ⏸️ Bloqueado | Perfil devuelto con observaciones para corrección                  |
| PERF-04 | APROBADO     | ✅ Final OK | Perfil aprobado — habilita creación de vacante                     |
| PERF-05 | RECHAZADO    | ❌ Final NOK| Perfil rechazado con motivo obligatorio                            |
| PERF-06 | INACTIVO     | ❌ Final NOK| Perfil desactivado por cambio organizacional o decisión de negocio |

**Estado inicial:** `PERF-01 BORRADOR`
**Estados finales:** `PERF-04 APROBADO`, `PERF-05 RECHAZADO`, `PERF-06 INACTIVO`

---

### 2.2 Matriz de Transiciones — Perfiles

| # | Origen      | Destino     | Evento Disparador              | Rol Autorizado              | Validaciones Previas                                     | Acciones Posteriores                                     | StateHistory | AuditLog | Evento n8n               |
|---|------------|------------|--------------------------------|-----------------------------|----------------------------------------------------------|----------------------------------------------------------|-------------|---------|--------------------------|
| 1 | BORRADOR   | EN_REVISION | Analista inicia completado    | Analista RRHH               | Solicitud asociada en estado APROBADA                    | Registrar analista responsable y timestamp               | ✅          | ✅      | `perfil.en_revision`     |
| 2 | EN_REVISION | OBSERVADO  | Revisor devuelve perfil       | Gerente RRHH                | Observación con descripción obligatoria                  | Notificar analista; registrar observaciones              | ✅          | ✅      | `perfil.observado`       |
| 3 | OBSERVADO  | EN_REVISION | Analista corrige y reenvía    | Analista RRHH               | Observaciones atendidas y documentadas                   | Notificar revisor; limpiar flag de observación           | ✅          | ✅      | `perfil.reenviado`       |
| 4 | EN_REVISION | APROBADO   | Gerente aprueba perfil        | Gerente RRHH, Director RRHH | Todos los campos obligatorios completos; sin observaciones | Habilitar creación de vacante; notificar solicitante   | ✅          | ✅      | `perfil.aprobado`        |
| 5 | EN_REVISION | RECHAZADO  | Gerente rechaza perfil        | Gerente RRHH, Director RRHH | Motivo de rechazo obligatorio                            | Notificar; registrar motivo; cerrar SLA                  | ✅          | ✅      | `perfil.rechazado`       |
| 6 | APROBADO   | INACTIVO   | Desactivación organizacional  | Director RRHH, Admin        | Motivo obligatorio; sin vacantes activas asociadas       | Inactivar vacantes asociadas si aplica; notificar        | ✅          | ✅      | `perfil.inactivado`      |

### 2.3 Reglas de Negocio — Perfiles

- **RN-PERF-01:** Un perfil solo puede aprobarse si todos los campos del diccionario de datos están completos.
- **RN-PERF-02:** La aprobación del perfil es prerequisito para crear una vacante.
- **RN-PERF-03:** Un perfil INACTIVO no puede asociarse a nuevas vacantes.
- **RN-PERF-04:** Un perfil puede tener múltiples versiones; la activa es siempre la de mayor versión aprobada.

---

## 3. DOMINIO: VACANTES

### 3.1 Catálogo de Estados

| Código | Nombre        | Tipo        | Descripción                                                          |
|--------|--------------|-------------|----------------------------------------------------------------------|
| VAC-01 | BORRADOR      | ⬛ Inicial  | Vacante creada a partir de perfil aprobado                           |
| VAC-02 | PUBLICADA     | 🔵 Proceso  | Vacante publicada en canales internos y/o externos                   |
| VAC-03 | EN_PROCESO    | 🔵 Proceso  | Vacante con postulantes activos en pipeline                          |
| VAC-04 | PAUSADA       | ⏸️ Bloqueada | Vacante temporalmente suspendida                                     |
| VAC-05 | CUBIERTA      | ✅ Final OK | Vacante cubierta exitosamente — postulante contratado                |
| VAC-06 | CERRADA       | ❌ Final NOK| Vacante cerrada sin cubrir (sin candidatos o decisión de negocio)    |
| VAC-07 | CANCELADA     | ❌ Final NOK| Vacante cancelada con motivo obligatorio                             |
| VAC-08 | REABIERTA     | 🔵 Proceso  | Vacante cerrada o cancelada reabierta con justificación              |

**Estado inicial:** `VAC-01 BORRADOR`
**Estados finales:** `VAC-05 CUBIERTA`, `VAC-06 CERRADA`, `VAC-07 CANCELADA`

---

### 3.2 Matriz de Transiciones — Vacantes

| # | Origen      | Destino     | Evento Disparador                | Rol Autorizado           | Validaciones Previas                                       | Acciones Posteriores                                          | StateHistory | AuditLog | Evento n8n                   |
|---|------------|------------|----------------------------------|--------------------------|------------------------------------------------------------|---------------------------------------------------------------|-------------|---------|------------------------------|
| 1 | BORRADOR   | PUBLICADA  | Reclutador publica vacante       | Reclutador, Analista RRHH| Perfil APROBADO; canales seleccionados; fecha límite definida | Publicar en canales; activar SLA; iniciar sourcing IA      | ✅          | ✅      | `vacante.publicada`          |
| 2 | PUBLICADA  | EN_PROCESO | Primer postulante recibido       | Sistema automático       | Al menos un postulante en pipeline                         | Notificar reclutador; activar agente Sourcing                 | ✅          | ✅      | `vacante.en_proceso`         |
| 3 | EN_PROCESO | PAUSADA    | Reclutador pausa proceso         | Reclutador, Gerente RRHH | Motivo de pausa obligatorio; notificación a postulantes activos | Notificar postulantes activos; suspender SLA             | ✅          | ✅      | `vacante.pausada`            |
| 4 | PAUSADA    | EN_PROCESO | Reclutador reanuda proceso       | Reclutador, Gerente RRHH | Motivo de reanudación obligatorio                          | Notificar postulantes; reactivar SLA                          | ✅          | ✅      | `vacante.reanudada`          |
| 5 | EN_PROCESO | CUBIERTA   | Postulante acepta oferta         | Sistema / Reclutador     | Oferta aceptada; contratación iniciada                     | Cerrar pipeline; descartar otros postulantes; notificar       | ✅          | ✅      | `vacante.cubierta`           |
| 6 | EN_PROCESO | CERRADA    | Reclutador cierra sin cubrir     | Gerente RRHH, Director   | Motivo de cierre obligatorio                               | Notificar solicitante; archivar postulantes; cerrar SLA       | ✅          | ✅      | `vacante.cerrada`            |
| 7 | PUBLICADA  | CANCELADA  | Cancelación antes de postulantes | Gerente RRHH, Director   | Motivo obligatorio; sin postulantes activos o notificación confirmada | Despublicar; notificar; cerrar SLA               | ✅          | ✅      | `vacante.cancelada`          |
| 8 | EN_PROCESO | CANCELADA  | Cancelación con postulantes      | Director RRHH            | Motivo obligatorio; notificación obligatoria a todos los postulantes | Notificar postulantes; descarte masivo con motivo     | ✅          | ✅      | `vacante.cancelada`          |
| 9 | CERRADA    | REABIERTA  | Reapertura por necesidad         | Director RRHH            | Justificación obligatoria; perfil aún vigente              | Notificar; reiniciar SLA; transitar a PUBLICADA               | ✅          | ✅      | `vacante.reabierta`          |
|10 | CANCELADA  | REABIERTA  | Reapertura de cancelada          | Director RRHH            | Justificación obligatoria; aprobación superior             | Notificar; reiniciar SLA; transitar a BORRADOR                | ✅          | ✅      | `vacante.reabierta`          |

### 3.3 Reglas de Negocio — Vacantes

- **RN-VAC-01:** Una vacante solo puede publicarse si el perfil asociado está en estado `APROBADO`.
- **RN-VAC-02:** El cierre de una vacante como `CUBIERTA` requiere que exista una contratación asociada confirmada.
- **RN-VAC-03:** La cancelación con postulantes activos requiere notificación confirmada a cada postulante.
- **RN-VAC-04:** Una vacante `PAUSADA` no acepta nuevos postulantes pero mantiene los activos.

---

## 4. DOMINIO: POSTULANTES (PIPELINE)

### 4.1 Catálogo de Estados

| Código  | Nombre              | Tipo        | Descripción                                                              |
|---------|---------------------|-------------|--------------------------------------------------------------------------|
| POST-01 | CAPTADO             | ⬛ Inicial  | Postulante registrado y vinculado a una vacante                          |
| POST-02 | EN_REVISION_CV      | 🔵 Proceso  | CV en revisión por reclutador o agente IA de matching                    |
| POST-03 | PRESELECCIONADO     | 🔵 Proceso  | Postulante pasó filtro curricular; pendiente de contacto inicial         |
| POST-04 | EN_CONTACTO         | 🔵 Proceso  | Reclutador contactó al postulante; pendiente de respuesta                |
| POST-05 | ENTREVISTA_PEND     | 🔵 Proceso  | Entrevista agendada o pendiente de agendar                               |
| POST-06 | EN_EVALUACION       | 🔵 Proceso  | Postulante en proceso de entrevistas activo                              |
| POST-07 | OFERTA_ENVIADA      | 🔵 Proceso  | Oferta económica enviada al postulante                                   |
| POST-08 | CONTRATADO          | ✅ Final OK | Postulante aceptó oferta y proceso de contratación iniciado              |
| POST-09 | DESCARTADO          | ❌ Final NOK| Postulante descartado con motivo obligatorio                             |
| POST-10 | RETIRADO            | ❌ Final NOK| Postulante se retiró voluntariamente del proceso                         |
| POST-11 | EN_RESERVA          | ⏸️ Especial | Postulante no seleccionado para esta vacante pero guardado para futuras  |

**Estado inicial:** `POST-01 CAPTADO`
**Estados finales:** `POST-08 CONTRATADO`, `POST-09 DESCARTADO`, `POST-10 RETIRADO`
**Estado especial:** `POST-11 EN_RESERVA` (no es final — puede reactivarse)

---

### 4.2 Matriz de Transiciones — Postulantes

| # | Origen           | Destino          | Evento Disparador                    | Rol Autorizado          | Validaciones Previas                                          | Acciones Posteriores                                           | StateHistory | AuditLog | Evento n8n                        |
|---|-----------------|-----------------|--------------------------------------|-------------------------|---------------------------------------------------------------|----------------------------------------------------------------|-------------|---------|-----------------------------------|
| 1 | CAPTADO         | EN_REVISION_CV  | Sistema inicia revisión IA           | Sistema / Reclutador    | Vacante en estado PUBLICADA o EN_PROCESO                      | Invocar agente Matching; registrar AgentExecution              | ✅          | ✅      | `postulante.en_revision_cv`       |
| 2 | EN_REVISION_CV  | PRESELECCIONADO | Pasa filtro IA o manual              | Sistema / Reclutador    | Score ≥ umbral configurado O aprobación manual del reclutador | Notificar reclutador; registrar score en expediente            | ✅          | ✅      | `postulante.preseleccionado`      |
| 3 | EN_REVISION_CV  | DESCARTADO      | No pasa filtro                       | Sistema / Reclutador    | Motivo de descarte obligatorio                                | Notificar postulante (si aplica política); registrar motivo    | ✅          | ✅      | `postulante.descartado`           |
| 4 | PRESELECCIONADO | EN_CONTACTO     | Reclutador inicia contacto           | Reclutador              | Datos de contacto del postulante disponibles                  | Registrar canal y timestamp de contacto                        | ✅          | ✅      | `postulante.en_contacto`          |
| 5 | EN_CONTACTO     | ENTREVISTA_PEND | Postulante confirma interés          | Reclutador              | Respuesta positiva del postulante registrada                  | Agendar entrevista inicial; crear registro de Entrevista       | ✅          | ✅      | `postulante.entrevista_pendiente` |
| 6 | EN_CONTACTO     | DESCARTADO      | Sin respuesta o descarte por contacto| Reclutador              | Motivo obligatorio (sin respuesta, no interesado, etc.)       | Notificar cierre; registrar motivo                             | ✅          | ✅      | `postulante.descartado`           |
| 7 | ENTREVISTA_PEND | EN_EVALUACION   | Entrevista realizada                 | Reclutador              | Entrevista en estado REALIZADA                                | Actualizar expediente; continuar pipeline según resultado      | ✅          | ✅      | `postulante.en_evaluacion`        |
| 8 | EN_EVALUACION   | OFERTA_ENVIADA  | Aprobado en todas las etapas         | Gerente RRHH, Director  | Todas las entrevistas en estado APROBADA                      | Generar oferta; notificar postulante; activar SLA de respuesta | ✅          | ✅      | `postulante.oferta_enviada`       |
| 9 | EN_EVALUACION   | DESCARTADO      | No aprobado en evaluación            | Reclutador, Gerente RRHH| Motivo de descarte obligatorio                                | Notificar postulante; registrar motivo; cerrar evaluaciones    | ✅          | ✅      | `postulante.descartado`           |
|10 | OFERTA_ENVIADA  | CONTRATADO      | Postulante acepta oferta             | Sistema / Reclutador    | Oferta en estado ACEPTADA                                     | Iniciar contratación; cubrir vacante; notificar RRHH           | ✅          | ✅      | `postulante.contratado`           |
|11 | OFERTA_ENVIADA  | DESCARTADO      | Postulante rechaza oferta            | Sistema / Reclutador    | Motivo de rechazo registrado                                  | Reactivar pipeline de vacante; notificar equipo               | ✅          | ✅      | `postulante.descartado`           |
|12 | cualquiera      | RETIRADO        | Postulante se retira voluntariamente | Reclutador / Postulante | Confirmación del retiro voluntario                            | Registrar motivo si lo proporciona; notificar; cerrar          | ✅          | ✅      | `postulante.retirado`             |
|13 | DESCARTADO      | EN_RESERVA      | Reclutador marca para reserva        | Reclutador, Gerente RRHH| Consentimiento del postulante para banco de talento            | Registrar en banco de talento; notificar postulante            | ✅          | ✅      | `postulante.en_reserva`           |

### 4.3 Reglas de Negocio — Postulantes

- **RN-POST-01:** Todo descarte debe registrar motivo de descarte del catálogo maestro (no texto libre sin categoría).
- **RN-POST-02:** El scoring IA debe quedar registrado en `AgentExecution` con versión de prompt y modelo utilizado.
- **RN-POST-03:** Un postulante solo puede pasar a `CONTRATADO` si la oferta asociada está en estado `ACEPTADA`.
- **RN-POST-04:** Un postulante en `EN_RESERVA` puede ser reactivado en un nuevo proceso sin reiniciar desde cero.
- **RN-POST-05:** El umbral de score para preselección es configurable en el catálogo maestro (no hardcodeado).

---

## 5. DOMINIO: ENTREVISTAS

### 5.1 Catálogo de Estados

| Código  | Nombre         | Tipo        | Descripción                                                         |
|---------|---------------|-------------|---------------------------------------------------------------------|
| ENT-01  | AGENDADA       | ⬛ Inicial  | Entrevista programada con fecha, hora, lugar y tipo definidos       |
| ENT-02  | CONFIRMADA     | 🔵 Proceso  | Entrevista confirmada por el entrevistador y el postulante          |
| ENT-03  | REPROGRAMADA   | 🔵 Proceso  | Entrevista movida de fecha/hora (con motivo obligatorio)            |
| ENT-04  | REALIZADA      | 🔵 Proceso  | Entrevista ejecutada; pendiente de evaluación                       |
| ENT-05  | EVALUADA       | ✅ Final OK | Entrevista con resultado registrado (APROBADA / RECHAZADA)          |
| ENT-06  | CANCELADA      | ❌ Final NOK| Entrevista cancelada por cualquiera de las partes                   |
| ENT-07  | NO_PRESENTADO  | ❌ Final NOK| Postulante no se presentó sin justificación previa                  |

**Estado inicial:** `ENT-01 AGENDADA`
**Estados finales:** `ENT-05 EVALUADA`, `ENT-06 CANCELADA`, `ENT-07 NO_PRESENTADO`

---

### 5.2 Matriz de Transiciones — Entrevistas

| # | Origen        | Destino       | Evento Disparador                  | Rol Autorizado               | Validaciones Previas                                     | Acciones Posteriores                                         | StateHistory | AuditLog | Evento n8n                         |
|---|--------------|--------------|-------------------------------------|------------------------------|----------------------------------------------------------|--------------------------------------------------------------|-------------|---------|-------------------------------------|
| 1 | AGENDADA     | CONFIRMADA   | Confirmación de partes              | Reclutador / Sistema         | Fecha futura; entrevistador disponible; postulante notificado | Enviar recordatorio automático; bloquear agenda           | ✅          | ✅      | `entrevista.confirmada`             |
| 2 | CONFIRMADA   | REPROGRAMADA | Cambio de fecha solicitado          | Reclutador, Entrevistador    | Motivo obligatorio; nueva fecha futura disponible         | Notificar a todas las partes; actualizar agenda              | ✅          | ✅      | `entrevista.reprogramada`           |
| 3 | REPROGRAMADA | CONFIRMADA   | Reconfirmación en nueva fecha       | Reclutador / Sistema         | Nueva fecha confirmada por entrevistador                  | Notificar partes; enviar recordatorio                        | ✅          | ✅      | `entrevista.confirmada`             |
| 4 | CONFIRMADA   | REALIZADA    | Entrevista ejecutada                | Entrevistador / Sistema      | Fecha de entrevista alcanzada; asistencia confirmada      | Habilitar formulario de evaluación; notificar reclutador     | ✅          | ✅      | `entrevista.realizada`              |
| 5 | REALIZADA    | EVALUADA     | Entrevistador registra resultado    | Entrevistador                | Resultado (APROBADO/RECHAZADO) y comentarios obligatorios | Actualizar expediente postulante; activar siguiente etapa    | ✅          | ✅      | `entrevista.evaluada`               |
| 6 | AGENDADA     | CANCELADA    | Cancelación antes de confirmación   | Reclutador, Entrevistador    | Motivo obligatorio                                        | Notificar partes; liberar agenda; registrar motivo           | ✅          | ✅      | `entrevista.cancelada`              |
| 7 | CONFIRMADA   | CANCELADA    | Cancelación tras confirmación       | Reclutador, Entrevistador    | Motivo obligatorio; notificación a postulante obligatoria | Notificar postulante; liberar agenda; ofrecer reprogramar    | ✅          | ✅      | `entrevista.cancelada`              |
| 8 | CONFIRMADA   | NO_PRESENTADO| Postulante no asiste                | Sistema / Reclutador         | Tiempo de gracia superado sin aviso                       | Registrar inasistencia; notificar equipo; evaluar descarte   | ✅          | ✅      | `entrevista.no_presentado`          |

### 5.3 Reglas de Negocio — Entrevistas

- **RN-ENT-01:** El tipo de entrevista (inicial, técnica, gerencial) es obligatorio y proviene del catálogo maestro.
- **RN-ENT-02:** El resultado de la entrevista (APROBADO/RECHAZADO/EN_ESPERA) debe ser registrado dentro del SLA definido.
- **RN-ENT-03:** Tres reprogramaciones consecutivas generan alerta automática al gerente.
- **RN-ENT-04:** Un `NO_PRESENTADO` sin justificación válida en 24h genera descarte automático sugerido.

---

## 6. DOMINIO: OFERTAS

### 6.1 Catálogo de Estados

| Código | Nombre     | Tipo        | Descripción                                                          |
|--------|-----------|-------------|----------------------------------------------------------------------|
| OFE-01 | GENERADA   | ⬛ Inicial  | Oferta económica creada y pendiente de envío                         |
| OFE-02 | ENVIADA    | 🔵 Proceso  | Oferta enviada al postulante; pendiente de respuesta                 |
| OFE-03 | EN_NEGOC   | 🔵 Proceso  | Postulante solicitó negociación de condiciones                       |
| OFE-04 | ACEPTADA   | ✅ Final OK | Postulante aceptó la oferta; activa proceso de contratación          |
| OFE-05 | RECHAZADA  | ❌ Final NOK| Postulante rechazó la oferta con motivo obligatorio                  |
| OFE-06 | EXPIRADA   | ❌ Final NOK| Oferta no respondida dentro del plazo definido                       |
| OFE-07 | REVOCADA   | ❌ Final NOK| Oferta retirada por la empresa antes de respuesta del postulante     |

**Estado inicial:** `OFE-01 GENERADA`
**Estados finales:** `OFE-04 ACEPTADA`, `OFE-05 RECHAZADA`, `OFE-06 EXPIRADA`, `OFE-07 REVOCADA`

---

### 6.2 Matriz de Transiciones — Ofertas

| # | Origen     | Destino    | Evento Disparador               | Rol Autorizado            | Validaciones Previas                                       | Acciones Posteriores                                          | StateHistory | AuditLog | Evento n8n               |
|---|-----------|-----------|----------------------------------|---------------------------|-------------------------------------------------------------|---------------------------------------------------------------|-------------|---------|--------------------------|
| 1 | GENERADA  | ENVIADA   | Reclutador envía oferta          | Reclutador, Gerente RRHH  | Postulante en estado OFERTA_ENVIADA; aprobación salarial    | Notificar postulante; iniciar SLA de respuesta                | ✅          | ✅      | `oferta.enviada`         |
| 2 | ENVIADA   | EN_NEGOC  | Postulante solicita negociación  | Reclutador / Sistema      | Solicitud de negociación dentro del plazo vigente           | Notificar gerente; registrar contrapropuesta                  | ✅          | ✅      | `oferta.en_negociacion`  |
| 3 | EN_NEGOC  | ENVIADA   | Nueva oferta emitida             | Gerente RRHH, Director    | Nueva propuesta aprobada; reinicio de SLA de respuesta      | Notificar postulante con nueva oferta; registrar versión      | ✅          | ✅      | `oferta.enviada`         |
| 4 | ENVIADA   | ACEPTADA  | Postulante acepta                | Reclutador / Sistema      | Respuesta dentro del plazo; confirmación escrita            | Iniciar contratación; marcar vacante como CUBIERTA            | ✅          | ✅      | `oferta.aceptada`        |
| 5 | EN_NEGOC  | ACEPTADA  | Postulante acepta tras negociar  | Reclutador / Sistema      | Acuerdo documentado; confirmación escrita                   | Iniciar contratación; cerrar negociación                      | ✅          | ✅      | `oferta.aceptada`        |
| 6 | ENVIADA   | RECHAZADA | Postulante rechaza               | Reclutador / Sistema      | Motivo de rechazo obligatorio                               | Notificar equipo; reactivar búsqueda si aplica                | ✅          | ✅      | `oferta.rechazada`       |
| 7 | EN_NEGOC  | RECHAZADA | Negociación fracasa              | Reclutador / Sistema      | Motivo de fracaso obligatorio                               | Notificar equipo; documentar límites de negociación           | ✅          | ✅      | `oferta.rechazada`       |
| 8 | ENVIADA   | EXPIRADA  | SLA de respuesta vencido         | Sistema automático        | Plazo de respuesta superado sin acción del postulante       | Notificar equipo; sugerir descarte o extensión de plazo       | ✅          | ✅      | `oferta.expirada`        |
| 9 | ENVIADA   | REVOCADA  | Empresa retira la oferta         | Director RRHH, Legal      | Motivo legal u organizacional obligatorio                   | Notificar postulante formalmente; documentar motivo           | ✅          | ✅      | `oferta.revocada`        |

### 6.3 Reglas de Negocio — Ofertas

- **RN-OFE-01:** La banda salarial ofertada debe estar dentro del rango aprobado para el perfil. Dato sensible — cifrado en reposo.
- **RN-OFE-02:** El plazo de respuesta de la oferta es configurable en el catálogo maestro (default: 48h hábiles).
- **RN-OFE-03:** Toda negociación salarial debe quedar documentada con historial de versiones en `AuditLog`.
- **RN-OFE-04:** La aceptación de la oferta activa automáticamente el flujo de Contratación.

---

## 7. DOMINIO: CONTRATACIÓN

### 7.1 Catálogo de Estados

| Código | Nombre             | Tipo        | Descripción                                                             |
|--------|-------------------|-------------|-------------------------------------------------------------------------|
| CON-01 | INICIADA           | ⬛ Inicial  | Proceso de contratación iniciado tras aceptación de oferta              |
| CON-02 | DOC_PENDIENTE      | 🔵 Proceso  | Documentación requerida al candidato pendiente de recepción             |
| CON-03 | DOC_EN_REVISION    | 🔵 Proceso  | Documentación recibida y en validación por RRHH                        |
| CON-04 | DOC_OBSERVADA      | ⏸️ Bloqueada| Documentación con observaciones; candidato debe corregir                |
| CON-05 | PENDIENTE_FIRMA    | 🔵 Proceso  | Contrato generado y pendiente de firma                                  |
| CON-06 | COMPLETADA         | ✅ Final OK | Contratación exitosa — empleado dado de alta                            |
| CON-07 | CANCELADA          | ❌ Final NOK| Proceso cancelado por empresa o candidato antes de completarse          |

**Estado inicial:** `CON-01 INICIADA`
**Estados finales:** `CON-06 COMPLETADA`, `CON-07 CANCELADA`

---

### 7.2 Matriz de Transiciones — Contratación

| # | Origen           | Destino          | Evento Disparador                  | Rol Autorizado           | Validaciones Previas                                     | Acciones Posteriores                                         | StateHistory | AuditLog | Evento n8n                       |
|---|-----------------|-----------------|-------------------------------------|--------------------------|----------------------------------------------------------|--------------------------------------------------------------|-------------|---------|----------------------------------|
| 1 | INICIADA        | DOC_PENDIENTE   | Sistema solicita documentación      | Sistema / Analista RRHH  | Oferta en estado ACEPTADA                                | Enviar checklist de documentos al candidato; activar SLA     | ✅          | ✅      | `contratacion.doc_pendiente`     |
| 2 | DOC_PENDIENTE   | DOC_EN_REVISION | Candidato entrega documentación     | Analista RRHH / Sistema  | Documentos mínimos recibidos según checklist             | Notificar analista para revisión; registrar fecha de entrega | ✅          | ✅      | `contratacion.doc_en_revision`   |
| 3 | DOC_EN_REVISION | DOC_OBSERVADA   | Analista detecta observaciones      | Analista RRHH            | Observaciones descritas en detalle                       | Notificar candidato con lista de correcciones; reiniciar SLA | ✅          | ✅      | `contratacion.doc_observada`     |
| 4 | DOC_OBSERVADA   | DOC_EN_REVISION | Candidato subsana documentación     | Analista RRHH / Sistema  | Documentos corregidos recibidos                          | Notificar analista; registrar fecha de subsanación           | ✅          | ✅      | `contratacion.doc_en_revision`   |
| 5 | DOC_EN_REVISION | PENDIENTE_FIRMA | Documentación aprobada              | Analista RRHH            | Todos los documentos válidos y completos                 | Generar contrato; notificar candidato para firma             | ✅          | ✅      | `contratacion.pendiente_firma`   |
| 6 | PENDIENTE_FIRMA | COMPLETADA      | Firma del contrato registrada       | Analista RRHH / Sistema  | Contrato firmado por ambas partes                        | Dar de alta al empleado en sistema; notificar áreas; cerrar vacante | ✅    | ✅      | `contratacion.completada`        |
| 7 | cualquiera      | CANCELADA       | Cancelación por empresa o candidato | Gerente RRHH, Director   | Motivo de cancelación obligatorio                        | Notificar todas las partes; registrar motivo; reactivar vacante si aplica | ✅ | ✅   | `contratacion.cancelada`         |

### 7.3 Reglas de Negocio — Contratación

- **RN-CON-01:** El checklist de documentos requeridos proviene del catálogo maestro según tipo de vacante.
- **RN-CON-02:** La cancelación de la contratación después de la firma requiere intervención de Legal.
- **RN-CON-03:** La contratación completada dispara la integración con el sistema de nómina (si aplica en Fase 2).
- **RN-CON-04:** Toda la documentación sensible del candidato debe cifrarse en reposo.

---

## 8. INTEGRACIÓN CON WORKFLOWS N8N

### Catálogo de Eventos de Dominio por Máquina de Estados

| Dominio       | Evento n8n                        | Workflow Activado | SLA Implicado | Agente IA Involucrado     |
|---------------|-----------------------------------|-------------------|---------------|---------------------------|
| Solicitudes   | `solicitud.enviada`               | WF-SOL-01         | ✅            | Agente Solicitud          |
| Solicitudes   | `solicitud.aprobada`              | WF-SOL-02         | ✅            | —                         |
| Perfiles      | `perfil.aprobado`                 | WF-PERF-01        | ✅            | Agente Perfil             |
| Vacantes      | `vacante.publicada`               | WF-VAC-01         | ✅            | Agente Sourcing           |
| Postulantes   | `postulante.en_revision_cv`       | WF-POST-01        | ✅            | Agente Matching/Scoring   |
| Postulantes   | `postulante.preseleccionado`      | WF-POST-02        | —             | —                         |
| Postulantes   | `postulante.oferta_enviada`       | WF-POST-03        | ✅            | Agente Coordinación       |
| Entrevistas   | `entrevista.confirmada`           | WF-ENT-01         | ✅            | Agente Coordinación       |
| Entrevistas   | `entrevista.no_presentado`        | WF-ENT-02         | —             | —                         |
| Ofertas       | `oferta.expirada`                 | WF-OFE-01         | ✅            | —                         |
| Contratación  | `contratacion.completada`         | WF-CON-01         | ✅            | —                         |

---

## 9. IMPACTO EN APIS .NET

### Endpoints Mínimos por Dominio

| Dominio       | Endpoint                                          | Método | Descripción                              |
|---------------|--------------------------------------------------|--------|------------------------------------------|
| Solicitudes   | `/api/v1/solicitudes/{id}/transicion`             | POST   | Ejecutar transición de estado            |
| Solicitudes   | `/api/v1/solicitudes/{id}/historial`              | GET    | Obtener StateHistory                     |
| Perfiles      | `/api/v1/perfiles/{id}/transicion`                | POST   | Ejecutar transición de estado            |
| Vacantes      | `/api/v1/vacantes/{id}/transicion`                | POST   | Ejecutar transición de estado            |
| Postulantes   | `/api/v1/postulantes/{id}/transicion`             | POST   | Ejecutar transición de estado            |
| Entrevistas   | `/api/v1/entrevistas/{id}/transicion`             | POST   | Ejecutar transición de estado            |
| Ofertas       | `/api/v1/ofertas/{id}/transicion`                 | POST   | Ejecutar transición de estado            |
| Contratación  | `/api/v1/contrataciones/{id}/transicion`          | POST   | Ejecutar transición de estado            |
| Global        | `/api/v1/estados/catalogo/{dominio}`              | GET    | Obtener catálogo de estados por dominio  |
| Global        | `/api/v1/transiciones/validas/{dominio}/{estado}` | GET    | Obtener transiciones válidas desde estado|

---

## 10. IMPACTO EN AUDITORÍA

### Registros Obligatorios por Transición

```
StateHistory (por cada transición):
  - EntityId         : ID de la entidad
  - EntityType       : Dominio (Solicitud, Vacante, etc.)
  - FromState        : Estado origen
  - ToState          : Estado destino
  - Event            : Evento disparador
  - PerformedBy      : Usuario o sistema
  - PerformedAt      : Timestamp UTC
  - CorrelationId    : Propagado desde el request original
  - Channel          : Canal origen (Web, API, n8n, Sistema)
  - Reason           : Motivo si aplica

AuditLog (por cada transición):
  - AuditId          : UUID único
  - Action           : TRANSITION
  - EntityType       : Dominio
  - EntityId         : ID de la entidad
  - OldValue         : Estado origen (JSON)
  - NewValue         : Estado destino (JSON)
  - PerformedBy      : Actor
  - PerformedAt      : Timestamp UTC
  - CorrelationId    : Propagado
  - IpAddress        : IP del actor (cuando aplica)
```

---

## 11. RIESGOS DETECTADOS (VERSIÓN 1.0)

| ID    | Severidad | Dominio       | Riesgo                                                              | Recomendación                                         |
|-------|-----------|---------------|---------------------------------------------------------------------|-------------------------------------------------------|
| R-001 | 🔴 Alto   | Global        | Umbrales de scoring hardcodeados en código                          | Mover a catálogo maestro parametrizable               |
| R-002 | 🔴 Alto   | Postulantes   | Descarte sin motivo de catálogo (texto libre sin categoría)         | Implementar catálogo de motivos de descarte           |
| R-003 | 🟠 Medio  | Entrevistas   | Sin límite de reprogramaciones puede generar ciclos infinitos       | Implementar máximo 3 reprogramaciones con alerta      |
| R-004 | 🟠 Medio  | Ofertas       | Banda salarial sin cifrado en reposo                                | Aplicar cifrado según POLITICAS_DE_SEGURIDAD.md       |
| R-005 | 🟡 Bajo   | Contratación  | Checklist de documentos hardcodeado                                 | Mover a catálogo maestro por tipo de vacante          |
| R-006 | 🟡 Bajo   | Global        | Sin validación de CorrelationId en llamadas entre sistemas          | Implementar middleware de propagación de CorrelationId|

---

## 12. RECOMENDACIONES

1. **Implementar motor de estados centralizado** en el Backend como servicio independiente (`StateMachineService`) que valide estados, ejecute transiciones y emita eventos.
2. **Parametrizar todos los estados en base de datos** usando el catálogo maestro gobernado por `NacionalSeguros_MasterDataArchitect`.
3. **Implementar middleware de CorrelationId** en .NET 8 para propagación automática en todas las capas.
4. **Configurar DLQ en n8n** para todos los workflows de transición de estado.
5. **Aplicar cifrado en reposo** para campos sensibles (banda salarial, documentos de contratación).
6. **Definir umbrales de SLA por estado** en el catálogo maestro, no en código.

---

## Resultado de Validación

> ⚠️ **ESTADO:** BORRADOR — Pendiente de validación por `NacionalSeguros_StateMachineArchitect` y `NacionalSeguros_ProjectAuditor`
>
> Esta KB debe ser revisada contra el `ANALISIS_FUNCIONAL.md`, `DISEÑO_ERD.md` y el PRD completo antes de emitir resultado de cumplimiento.

**Resultado esperado tras validación:** `APPROVED` | `APPROVED WITH OBSERVATIONS` | `REJECTED`
