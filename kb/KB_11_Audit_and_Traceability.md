# KB_11_Audit_and_Traceability

## Descripción

Esta Knowledge Base define los estándares de auditoría, trazabilidad, bitácoras, historial, evidencia, seguimiento y reconstrucción de procesos para el Sistema Inteligente de Reclutamiento de Nacional Seguros.

La trazabilidad completa es un requisito obligatorio del proyecto.

Toda acción realizada por usuarios, agentes IA, integraciones y workflows debe poder reconstruirse posteriormente.

---

# Principios

Toda acción debe ser:

* Auditable
* Trazable
* Consultable
* Explicable
* Inmutable
* Reconstruible

---

# Objetivos

Permitir responder:

* Qué ocurrió.
* Quién lo hizo.
* Cuándo ocurrió.
* Desde qué canal ocurrió.
* Qué cambió.
* Qué agente participó.
* Qué workflow participó.
* Qué decisión se tomó.
* Qué evidencia la respalda.

---

# Componentes de Trazabilidad

La solución deberá implementar:

* AuditLog
* AuditDetail
* StateHistory
* Timeline
* AgentExecution
* IntegrationLog
* NotificationLog
* ErrorLog

---

# AuditLog

Registro principal de auditoría.

Campos mínimos:

* AuditId
* FechaHoraUTC
* UsuarioId
* UsuarioNombre
* Rol
* Modulo
* Entidad
* EntidadId
* Accion
* EstadoAnterior
* EstadoNuevo
* Canal
* Observacion
* CorrelationId

---

# Acciones Auditables

Registrar obligatoriamente:

* Crear
* Editar
* Eliminar lógico
* Aprobar
* Rechazar
* Publicar
* Despublicar
* Contratar
* Descartar
* Reprogramar
* Parametrizar

---

# StateHistory

Registrar toda transición de estado.

Campos mínimos:

* StateHistoryId
* Entidad
* EntidadId
* EstadoAnterior
* EstadoNuevo
* Usuario
* Fecha
* Comentario

---

# Timeline

Toda entidad principal deberá tener una línea de tiempo.

Aplica para:

* Solicitudes
* Perfiles
* Vacantes
* Postulantes
* Entrevistas

---

# Eventos Timeline

Registrar:

* Creación
* Modificación
* Cambio de estado
* Aprobación
* Rechazo
* Notificación
* Ejecución IA
* Error
* Integración

---

# AgentExecution

Toda ejecución IA debe registrarse.

Campos mínimos:

* ExecutionId
* AgentName
* PromptVersion
* Usuario
* FechaInicio
* FechaFin
* Duracion
* Input
* Output
* Resultado
* Error
* WorkflowOrigen

---

# Prompt Traceability

Debe registrarse:

* Prompt utilizado
* Versión
* Fecha
* Autor
* Resultado

---

# Workflow Traceability

Todo workflow n8n debe registrar:

* WorkflowName
* Versión
* Trigger
* Usuario
* Inicio
* Fin
* Estado
* Resultado

---

# IntegrationLog

Registrar integraciones con:

* Correo
* WhatsApp
* Calendario
* Sistema Salar
* APIs externas

Campos mínimos:

* Fecha
* SistemaOrigen
* SistemaDestino
* Operación
* Resultado
* Error

---

# NotificationLog

Registrar:

* Correo enviado
* WhatsApp enviado
* Destinatario
* Plantilla utilizada
* Resultado

---

# ErrorLog

Registrar:

* Fecha
* Sistema
* Módulo
* Error
* StackTrace resumido
* Usuario
* CorrelationId

---

# CorrelationId

Toda operación debe generar un CorrelationId.

Debe propagarse a:

* Frontend
* Backend
* SQL
* n8n
* IA
* Integraciones

---

# Evidencia

Toda decisión importante debe almacenar evidencia.

Ejemplos:

* Aprobación de perfil
* Aprobación de vacante
* Oferta
* Contratación
* Rechazo

---

# Auditoría de Datos Sensibles

Registrar acceso a:

* Banda salarial
* Pretensión salarial
* Scoring
* Resultados psicotécnicos
* Evaluaciones
* Matching

---

# Consultas de Auditoría

El sistema debe permitir filtrar por:

* Usuario
* Rol
* Fecha
* Módulo
* Entidad
* Vacante
* Postulante
* Workflow
* Agente

---

# Reportes de Auditoría

Generar reportes de:

* Cambios de estado
* Accesos sensibles
* Ejecuciones IA
* Errores
* Aprobaciones
* Integraciones

---

# Seguridad de Auditoría

Los registros de auditoría:

* No pueden eliminarse.
* No pueden modificarse.
* Deben mantenerse históricos.

---

# Observabilidad

Toda auditoría debe integrarse con:

* Logs
* Métricas
* Alertas
* Dashboards

---

# Gobierno IA

Toda recomendación IA debe ser rastreable.

Debe poder identificarse:

* Agente
* Prompt
* Versión
* Input
* Output
* Usuario responsable

---

# Checklist de Validación

Antes de aprobar cualquier desarrollo verificar:

* Existe AuditLog.
* Existe StateHistory.
* Existe Timeline.
* Existe CorrelationId.
* Existe evidencia.
* Existe trazabilidad completa.

---

# Resultado Esperado

Nacional Seguros debe poder reconstruir cualquier proceso de reclutamiento desde la solicitud inicial hasta la contratación final, incluyendo usuarios, agentes IA, workflows, decisiones, aprobaciones, cambios de estado e integraciones involucradas.
