# KB_13_Agent_Contracts

## Descripción

Esta Knowledge Base define los contratos funcionales, operativos y técnicos de todos los agentes de Inteligencia Artificial del Sistema Inteligente de Reclutamiento para Nacional Seguros.

Cada agente debe tener responsabilidades claramente delimitadas, entradas definidas, salidas auditables y comportamiento controlado.

Los agentes son asistentes especializados.

Los agentes no sustituyen la decisión humana.

---

# Principios Generales

Todo agente debe ser:

* Auditable
* Trazable
* Explicable
* Reproducible
* Versionable
* Observable

---

# Estructura Obligatoria de un Agente

Todo agente debe definir:

* Nombre
* Objetivo
* Inputs
* Outputs
* Reglas
* Restricciones
* Prompt
* Variables
* Eventos
* Workflow Asociado
* AgentExecution
* Métricas

---

# AGENTE SOLICITUD

## Nombre

AgenteSolicitud

---

## Objetivo

Asistir en la estructuración y validación inicial de solicitudes de personal.

---

## Inputs

* Área
* Cargo
* Motivo
* Prioridad
* Funciones
* Requerimientos

---

## Outputs

* Solicitud estructurada
* Campos faltantes
* Observaciones
* Recomendaciones

---

## Restricciones

No puede aprobar solicitudes.

No puede crear vacantes.

---

## Eventos

SolicitudAnalizada

SolicitudObservada

---

## Workflow

Solicitud_Analisis_V1

---

# AGENTE PERFIL

## Nombre

AgentePerfil

---

## Objetivo

Generar borradores de perfiles de cargo.

---

## Inputs

* Solicitud aprobada
* Historial de perfiles
* Manuales de funciones

---

## Outputs

* Perfil propuesto
* Funciones
* Skills
* Keywords
* Fit cultural
* Red Flags

---

## Restricciones

No puede aprobar perfiles.

---

## Eventos

PerfilGenerado

PerfilRecomendado

---

## Workflow

Perfil_Generacion_V1

---

# AGENTE SOURCING

## Nombre

AgenteSourcing

---

## Objetivo

Definir estrategia de búsqueda de candidatos.

---

## Inputs

* Perfil aprobado
* Mercado
* Seniority
* Ubicación

---

## Outputs

* Canales recomendados
* Estrategia recomendada
* Justificación

---

## Restricciones

No publica automáticamente.

---

## Eventos

EstrategiaGenerada

---

## Workflow

Sourcing_Estrategia_V1

---

# AGENTE MATCHING

## Nombre

AgenteMatching

---

## Objetivo

Comparar postulantes contra perfiles.

---

## Inputs

* Perfil
* CV
* Expediente
* Historial

---

## Outputs

* Coincidencias
* Brechas
* ScoreMatching
* Observaciones

---

## Restricciones

No descarta postulantes.

---

## Eventos

MatchingCalculado

---

## Workflow

Matching_Postulante_V1

---

# AGENTE SCORING

## Nombre

AgenteScoring

---

## Objetivo

Generar scoring explicable.

---

## Inputs

* Perfil
* Matching
* Experiencia
* Formación
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

## Eventos

ScoringCalculado

---

## Workflow

Scoring_Postulante_V1

---

# AGENTE COORDINACIÓN

## Nombre

AgenteCoordinacion

---

## Objetivo

Coordinar entrevistas y seguimiento.

---

## Inputs

* Agenda
* Entrevistas
* Disponibilidad
* Plantillas

---

## Outputs

* Invitaciones
* Recordatorios
* Confirmaciones

---

## Restricciones

No modifica resultados de entrevistas.

---

## Eventos

EntrevistaProgramada

EntrevistaConfirmada

---

## Workflow

Agenda_Entrevistas_V1

---

# AGENTE ANALÍTICO

## Nombre

AgenteAnalitico

---

## Objetivo

Generar indicador y análisis.

---

## Inputs

* Vacantes
* Postulantes
* Entrevistas
* SLA
* Auditoría

---

## Outputs

* KPIs
* Tendencias
* Hallazgos
* Recomendaciones

---

## Restricciones

No modifica datos.

---

## Eventos

ReporteGenerado

---

## Workflow

Analitica_General_V1

---

# PROMPT CONTRACT

Todo agente debe utilizar:

Prompt

PromptVersion

PromptExecution

---

# PromptVersion

Campos mínimos:

* PromptVersionId
* AgentName
* Version
* Fecha
* Autor
* Estado

---

# AgentExecution

Campos mínimos:

* ExecutionId
* AgentName
* PromptVersion
* Workflow
* Usuario
* FechaInicio
* FechaFin
* Duracion
* Input
* Output
* Resultado
* Error

---

# Gestión de Errores

Todo agente debe:

* Capturar errores
* Registrar errores
* Notificar errores
* Permitir reintentos

---

# Observabilidad

Registrar:

* Tiempo respuesta
* Tokens entrada
* Tokens salida
* Costos
* Errores
* Reintentos

---

# Métricas

Cada agente debe generar:

* Cantidad de ejecuciones
* Tiempo promedio
* Tasa de error
* Costo promedio
* Aprobaciones generadas
* Rechazos generados

---

# Seguridad

Prohibido enviar al LLM:

* Contraseñas
* Tokens
* Credenciales
* Secretos

Aplicar anonimización cuando corresponda.

---

# Gobierno IA

Toda recomendación debe indicar:

* Agente
* Versión
* Fecha
* Nivel de confianza
* Justificación

---

# Checklist de Validación

Antes de aprobar un agente verificar:

* Tiene objetivo definido.
* Tiene inputs definidos.
* Tiene outputs definidos.
* Tiene restricciones.
* Tiene AgentExecution.
* Tiene auditoría.
* Tiene observabilidad.
* Tiene métricas.
* Tiene workflow asociado.

---

# Resultado Esperado

Todos los agentes del Sistema Inteligente de Reclutamiento deberán operar bajo contratos explícitos, auditables y versionados, garantizando trazabilidad completa, control operativo y cumplimiento de los lineamientos de Nacional Seguros.
