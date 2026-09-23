# KB_04_IA_Agents

## Descripción

Esta Knowledge Base define la arquitectura, responsabilidades, comportamiento, restricciones y gobierno de los agentes de Inteligencia Artificial del Sistema Inteligente de Reclutamiento para Nacional Seguros.

Los agentes son asistentes especializados que apoyan el proceso de reclutamiento.

Los agentes NO reemplazan la toma de decisiones humanas.

Toda recomendación generada por IA debe ser revisada y validada por usuarios autorizados.

---

# Principios de IA

La IA debe:

* Asistir.
* Recomendar.
* Analizar.
* Clasificar.
* Resumir.
* Priorizar.

La IA NO debe:

* Contratar.
* Aprobar.
* Rechazar automáticamente.
* Modificar datos críticos sin autorización.
* Tomar decisiones finales.

---

# Arquitectura de IA

Frontend Angular

↓

Backend .NET

↓

n8n

↓

Proveedor LLM

↓

Respuesta

↓

Backend .NET

↓

SQL Server

---

# Restricciones

Los agentes:

* No acceden directamente a SQL Server.
* No modifican tablas de negocio.
* No modifican estados críticos.
* No toman decisiones finales.

Toda ejecución debe pasar por APIs.

---

# Agente de Solicitud

## Objetivo

Asistir al solicitante en la construcción de requerimientos de personal.

---

## Inputs

* Área
* Cargo
* Motivo
* Funciones
* Necesidades del área

---

## Outputs

* Solicitud estructurada
* Información faltante
* Recomendaciones

---

## Restricciones

No puede aprobar solicitudes.

---

# Agente de Perfil

## Objetivo

Construir borradores de perfil.

---

## Inputs

* Solicitud
* Manual de funciones
* Historial de perfiles

---

## Outputs

* Funciones
* Skills
* Keywords
* Fit cultural
* Red Flags
* Recomendaciones

---

## Restricciones

No puede aprobar perfiles.

---

# Agente de Sourcing

## Objetivo

Recomendar estrategia de búsqueda.

---

## Inputs

* Perfil
* Seniority
* Ubicación
* Mercado

---

## Outputs

* Canales recomendados
* Estrategia de búsqueda
* Justificación

---

## Restricciones

No publica automáticamente.

---

# Agente de Matching

## Objetivo

Comparar postulantes contra perfiles.

---

## Inputs

* Perfil
* CV
* Historial del postulante

---

## Outputs

* Coincidencias
* Brechas
* Recomendaciones

---

## Restricciones

No descarta candidatos.

---

# Agente de Scoring

## Objetivo

Generar score explicable.

---

## Inputs

* Perfil
* CV
* Matching
* Psicotécnicos

---

## Outputs

* Score
* Factores
* Justificación

---

## Restricciones

No selecciona candidatos.

---

# Agente de Coordinación

## Objetivo

Gestionar comunicaciones operativas.

---

## Inputs

* Agenda
* Entrevistas
* Plantillas

---

## Outputs

* Recordatorios
* Confirmaciones
* Notificaciones

---

## Restricciones

No modifica estados sin autorización.

---

# Agente Analítico

## Objetivo

Generar análisis y métricas.

---

## Inputs

* Vacantes
* Postulantes
* Entrevistas
* SLA

---

## Outputs

* KPIs
* Tendencias
* Recomendaciones

---

## Restricciones

No modifica datos.

---

# Gestión de Prompts

Todos los prompts:

* Son propiedad de Nacional Seguros.
* Deben almacenarse en SQL Server.
* Deben versionarse.
* Deben auditarse.
* Deben documentarse.

---

# Modelo de Prompt

Todo prompt debe tener:

* Nombre
* Descripción
* Versión
* Estado
* Fecha creación
* Fecha modificación
* Autor
* Prompt System
* Prompt User
* Variables

---

# PromptVersion

Registrar:

* VersionNumber
* Fecha
* Autor
* Cambios realizados
* Estado

---

# AgentExecution

Toda ejecución IA debe registrar:

* Id
* AgentName
* PromptVersion
* Usuario
* FechaInicio
* FechaFin
* Duración
* Input
* Output
* Resultado
* Error

---

# Trazabilidad

Toda ejecución debe permitir responder:

* Qué agente ejecutó.
* Quién lo ejecutó.
* Cuándo se ejecutó.
* Qué información recibió.
* Qué resultado produjo.
* Qué prompt utilizó.

---

# Costos IA

Registrar:

* Modelo utilizado
* Tokens Input
* Tokens Output
* Costo estimado
* Tiempo de respuesta

---

# Observabilidad

Monitorear:

* Errores
* Latencia
* Reintentos
* Tokens
* Costos
* Disponibilidad

---

# Seguridad

Prohibido enviar al LLM:

* Contraseñas
* Tokens
* Credenciales
* Información financiera sensible

Aplicar anonimización cuando corresponda.

---

# Integración con n8n

Todo workflow debe definir:

* Objetivo
* Trigger
* Inputs
* Outputs
* Errores
* Reintentos
* Auditoría

---

# Validación Final

Antes de aprobar cualquier agente verificar:

* Cumple PRD.
* Cumple seguridad.
* Cumple auditoría.
* Cumple trazabilidad.
* Cumple explicabilidad.
* Cumple gobierno IA.

Si existe riesgo de automatización indebida debe rechazarse la implementación.
