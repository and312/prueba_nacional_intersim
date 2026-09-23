# KB_10_Workflow_Orchestration

## Descripción

Esta Knowledge Base define los estándares de orquestación, automatización, integración, eventos, SLA, excepciones y gobierno de workflows para el Sistema Inteligente de Reclutamiento de Nacional Seguros.

Toda automatización deberá implementarse mediante workflows gobernados y auditables.

n8n es el motor oficial de orquestación.

---

# Principios de Orquestación

Todo workflow debe ser:

* Auditable
* Trazable
* Recuperable
* Escalable
* Observable
* Versionable

---

# Responsabilidades de n8n

n8n puede:

* Orquestar procesos
* Coordinar agentes IA
* Enviar correos
* Enviar WhatsApp
* Ejecutar reglas
* Gestionar recordatorios
* Gestionar SLA
* Coordinar aprobaciones

n8n NO puede:

* Ser fuente oficial de datos
* Reemplazar SQL Server
* Tomar decisiones finales
* Aprobar procesos críticos
* Contratar candidatos

---

# Arquitectura

Frontend Angular

↓

Backend .NET

↓

SQL Server

↓

Eventos

↓

n8n

↓

Agentes IA

↓

Integraciones

---

# Eventos de Dominio

Todo cambio importante debe generar eventos.

---

## Solicitudes

SolicitudCreada

SolicitudValidada

SolicitudObservada

SolicitudAprobada

SolicitudRechazada

SolicitudConvertidaAVacante

---

## Perfiles

PerfilGenerado

PerfilEditado

PerfilValidado

PerfilAprobado

PerfilRechazado

---

## Vacantes

VacanteCreada

VacantePublicada

VacanteActualizada

VacanteCerrada

---

## Postulantes

PostulanteRegistrado

PostulantePreseleccionado

PostulanteEvaluado

PostulanteDescartado

PostulanteContratado

---

## Entrevistas

EntrevistaProgramada

EntrevistaConfirmada

EntrevistaReprogramada

EntrevistaCancelada

---

# Diseño de Workflows

Todo workflow debe contener:

* Objetivo
* Trigger
* Inputs
* Validaciones
* Acciones
* Outputs
* Manejo de errores
* Auditoría

---

# Naming Standards

Formato:

Modulo_Proceso_Version

Ejemplos:

Solicitud_Validacion_V1

Perfil_Generacion_V1

Matching_Candidato_V1

Scoring_Postulante_V1

Agenda_Entrevistas_V1

---

# Triggers Permitidos

Webhook

Evento de Dominio

Cron

API

Aprobación Humana

Cambio de Estado

---

# Workflows Base

## WF-01 Solicitud

Objetivo:

Gestionar creación y validación de solicitudes.

---

## WF-02 Perfil

Objetivo:

Generar borrador de perfil mediante IA.

---

## WF-03 Aprobación de Perfil

Objetivo:

Gestionar aprobación humana.

---

## WF-04 Estrategia de Búsqueda

Objetivo:

Generar estrategia de sourcing.

---

## WF-05 Matching

Objetivo:

Comparar postulantes con perfil.

---

## WF-06 Scoring

Objetivo:

Generar score explicable.

---

## WF-07 Agenda

Objetivo:

Coordinar entrevistas.

---

## WF-08 Notificaciones

Objetivo:

Gestionar correos y WhatsApp.

---

## WF-09 SLA

Objetivo:

Controlar tiempos y vencimientos.

---

## WF-10 Analítica

Objetivo:

Generar indicadores.

---

# Aprobaciones Humanas

Obligatorias para:

* Perfil aprobado
* Estrategia aprobada
* Shortlist
* Oferta
* Contratación

n8n no puede saltar aprobaciones.

---

# Gestión de Errores

Todo workflow debe:

* Capturar excepciones
* Registrar errores
* Generar alertas
* Permitir reintentos

---

# Reintentos

Definir:

* Cantidad máxima
* Tiempo entre intentos
* Escalamiento

---

# Dead Letter Queue

Cuando un proceso falle definitivamente:

* Registrar error
* Notificar responsable
* Registrar evidencia
* Mantener trazabilidad

---

# SLA

Todo workflow debe soportar:

* Tiempo objetivo
* Tiempo máximo
* Estado SLA
* Alertas

---

# Alertas

Generar alertas por:

* Error técnico
* SLA vencido
* Aprobación pendiente
* Entrevista pendiente
* Integración caída

---

# Integraciones

Integraciones oficiales:

* Backend .NET
* SQL Server vía API
* Correo
* WhatsApp
* Calendario
* Sistema Salar
* Repositorio documental

---

# Seguridad

No almacenar:

* Contraseñas
* Tokens
* Credenciales

Utilizar:

* Variables seguras
* Secret Manager
* Credenciales protegidas

---

# AgentExecution

Toda ejecución IA debe registrar:

* Workflow
* Agente
* Usuario
* PromptVersion
* Fecha
* Resultado
* Error
* Duración

---

# Observabilidad

Registrar:

* Inicio
* Fin
* Tiempo ejecución
* Error
* Reintentos

---

# Auditoría

Todo workflow debe registrar:

* Usuario
* Acción
* Entidad
* Estado anterior
* Estado nuevo
* Fecha

---

# Versionado

Todo workflow debe:

* Tener versión
* Tener historial
* Tener autor
* Tener fecha de modificación

---

# Checklist de Validación

Antes de aprobar un workflow verificar:

* Cumple PRD
* Cumple Seguridad
* Cumple Auditoría
* Cumple Trazabilidad
* Cumple SLA
* Cumple Gobierno IA

---

# Resultado Esperado

Toda automatización debe ser controlada, observable, auditable y alineada al modelo de gobierno de Nacional Seguros.

La automatización nunca debe sacrificar trazabilidad, auditoría o seguridad.
