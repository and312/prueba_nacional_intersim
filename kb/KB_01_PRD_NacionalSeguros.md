# KB_01_PRD_NacionalSeguros

## Descripción

Esta Knowledge Base contiene el contexto funcional oficial del Sistema Inteligente de Reclutamiento para Nacional Seguros.

Toda funcionalidad, arquitectura, base de datos, API, workflow n8n, agente IA, pantalla o integración deberá alinearse con este conocimiento.

---

# Resumen Ejecutivo

Nacional Seguros requiere una plataforma integral de reclutamiento asistida por Inteligencia Artificial que permita gestionar el proceso completo desde la solicitud de vacante hasta la contratación y evaluación post ingreso.

La solución debe centralizar la operación, garantizar trazabilidad, auditoría, seguridad y escalabilidad, incorporando agentes IA que asistan el proceso sin reemplazar la validación humana.

---

# Objetivo General

Centralizar, estandarizar y hacer trazable el proceso de reclutamiento de punta a punta, integrando:

* Solicitudes de personal.
* Construcción de perfiles.
* Gestión de vacantes.
* Gestión de postulantes.
* Coordinación de entrevistas.
* Analítica.
* Evaluación post ingreso.

Apoyando la operación mediante agentes IA especializados.

---

# Alcance Fase 1

La Fase 1 incluye:

## Solicitudes

* Registro de solicitud.
* Validación.
* Aprobación.
* Rechazo.
* Historial.

---

## Perfiles

* Generación asistida por IA.
* Edición.
* Versionado.
* Validación.
* Aprobación.

---

## Vacantes

* Creación.
* Gestión.
* Seguimiento.
* Estados.
* Bitácora.

---

## Postulantes

* Registro.
* Expediente.
* Documentos.
* Matching.
* Scoring.
* Pipeline.

---

## Agenda

* Entrevistas.
* Reprogramaciones.
* Confirmaciones.
* Seguimiento.

---

## Parametrización

* Roles.
* Permisos.
* Estados.
* SLA.
* Plantillas.
* Prompts.

---

## Auditoría

* AuditLog.
* Historiales.
* Evidencias.
* AgentExecution.

---

# Principios del Sistema

La plataforma debe garantizar:

* Seguridad.
* Trazabilidad.
* Auditoría.
* Escalabilidad.
* Observabilidad.
* Explicabilidad de IA.
* Mantenibilidad.

---

# Roles

## RRHH

Gestiona todo el proceso de reclutamiento.

---

## Solicitante

Solicita personal y aprueba perfiles.

---

## Decisor

Valida perfiles y candidatos.

---

## Reclutador

Opera vacantes y postulantes.

---

## Administrador

Configura el sistema.

---

## Auditor

Consulta trazabilidad y evidencias.

---

# Módulos Funcionales

## Inicio

Dashboard principal.

---

## Solicitudes

Gestión de solicitudes.

Estados:

* Borrador
* En Validación
* Aprobada
* Rechazada

---

## Perfiles

Construcción y aprobación de perfiles.

---

## Vacantes

Gestión integral de vacantes.

---

## Postulantes

Pipeline de reclutamiento.

Estados mínimos:

* Captado
* Screening
* Psicotécnico
* Entrevista RRHH
* Entrevista Técnica
* Terna
* Contratado
* Descartado

---

## Agenda

Calendario de entrevistas.

---

## Publicaciones

Gestión de canales de reclutamiento.

---

## Analítica

KPIs operativos y ejecutivos.

---

## Parametrización

Configuración del sistema.

---

## Auditoría

Consulta de eventos y trazabilidad.

---

# Agentes IA

## Agente Solicitud

Asiste en la estructuración del requerimiento.

---

## Agente Perfil

Genera perfiles de cargo.

---

## Agente Sourcing

Sugiere estrategias de búsqueda.

---

## Agente Matching

Compara CV contra perfil.

---

## Agente Scoring

Calcula score explicable.

---

## Agente Coordinación

Gestiona entrevistas y recordatorios.

---

## Agente Analítico

Genera indicadores y análisis.

---

# Reglas de Negocio

* Toda vacante debe tener perfil aprobado.
* Ningún agente IA puede aprobar perfiles.
* Ningún agente IA puede contratar candidatos.
* Toda acción relevante debe ser auditable.
* Toda ejecución IA debe ser trazable.
* Toda modificación crítica debe registrar historial.
* Toda integración debe registrar errores y reintentos.
* Toda decisión crítica requiere intervención humana.

---

# KPIs Esperados

* Tiempo de cobertura.
* Vacantes por área.
* Vacantes por urgencia.
* Éxito a 3 meses.
* SLA cumplidos.
* Efectividad por canal.
* Tiempo promedio por fase.
* Productividad de reclutamiento.

---

# Restricciones

* WhatsApp es canal de comunicación.
* Correo es canal de comunicación.
* Ningún canal es sistema transaccional.
* Los agentes IA son asistentes.
* SQL Server es la fuente oficial de información.
* Toda operación debe ser auditable.
* Toda operation debe ser trazable.

---

# Resultado Esperado

Todo diseño generado para este proyecto debe alinearse con esta base de conocimiento y respetar el alcance, reglas de negocio, objetivos y restricciones aquí definidas.
