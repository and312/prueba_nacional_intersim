# KB_12_Reporting_and_KPI

## Descripción

Esta Knowledge Base define los indicadores, métricas, dashboards, reportes operativos, reportes ejecutivos, monitoreo de SLA y evaluación de desempeño del Sistema Inteligente de Reclutamiento para Nacional Seguros.

Todo indicador debe ser trazable, auditable y basado en información oficial proveniente de SQL Server.

---

# Objetivos

La analítica debe permitir:

* Medir eficiencia operativa.
* Medir calidad de contratación.
* Medir desempeño de reclutamiento.
* Medir desempeño de agentes IA.
* Medir cumplimiento de SLA.
* Detectar cuellos de botella.
* Apoyar decisiones ejecutivas.

---

# Principios

Todos los indicadores deben ser:

* Auditables
* Explicables
* Reproducibles
* Trazables
* Configurables

---

# Tipos de Reportes

## Operativos

Orientados a RRHH y reclutadores.

---

## Tácticos

Orientados a líderes y responsables de área.

---

## Ejecutivos

Orientados a gerencia.

---

## Auditoría

Orientados a control interno y cumplimiento.

---

# Dashboard Principal

Mostrar:

* Solicitudes activas
* Vacantes activas
* Vacantes vencidas
* Entrevistas pendientes
* SLA vencidos
* Alertas operativas
* Contrataciones del mes

---

# KPI de Solicitudes

## Tiempo de Aprobación

Tiempo entre:

SolicitudCreada

y

SolicitudAprobada

---

## Solicitudes por Área

Cantidad de solicitudes por área.

---

## Solicitudes por Prioridad

* Alta
* Media
* Baja

---

## Solicitudes Rechazadas

Cantidad y porcentaje.

---

# KPI de Vacantes

## Tiempo de Cobertura

Tiempo desde:

VacantePublicada

hasta

Contratado

---

## Vacantes por Área

Agrupación por área.

---

## Vacantes por Ubicación

Agrupación geográfica.

---

## Vacantes Cerradas

Cantidad por período.

---

## Vacantes Canceladas

Cantidad por período.

---

# KPI de Postulantes

## Postulantes por Vacante

Promedio.

---

## Tasa de Conversión

Por fase del pipeline.

---

## Postulantes Descartados

Cantidad y motivo.

---

## Tiempo Promedio por Fase

Captación

Screening

Psicotécnica

Entrevista

Oferta

---

# KPI de Reclutamiento

## Productividad por Reclutador

* Vacantes gestionadas
* Postulantes evaluados
* Contrataciones logradas

---

## Tiempo de Gestión

Por reclutador.

---

## Efectividad

Contratados vs evaluados.

---

# KPI de Canales

## Efectividad por Canal

Canales:

* Portal Institucional
* Referidos
* Correo Interno
* Periódico

---

## Conversión por Canal

Postulantes → Contratados

---

## Tiempo por Canal

Promedio de cobertura.

---

# KPI de Entrevistas

## Entrevistas Programadas

Cantidad.

---

## Entrevistas Confirmadas

Cantidad.

---

## Entrevistas Reprogramadas

Cantidad.

---

## Entrevistas Canceladas

Cantidad.

---

# KPI de Calidad de Contratación

## Éxito a 3 Meses

Porcentaje de contratados con evaluación positiva a 3 meses.

---

## Rotación Temprana

Cantidad de bajas antes de 3 meses.

---

## Evaluación Post Ingreso

Promedio por área.

---

# KPI de SLA

## SLA Solicitudes

Cumplidos vs vencidos.

---

## SLA Vacantes

Cumplidos vs vencidos.

---

## SLA Postulantes

Cumplidos vs vencidos.

---

## SLA Entrevistas

Cumplidos vs vencidos.

---

# KPI de IA

## Ejecuciones por Agente

* Agente Solicitud
* Agente Perfil
* Agente Matching
* Agente Scoring
* Agente Coordinación
* Agente Analítico

---

## Tiempo Promedio de Respuesta

Por agente.

---

## Errores por Agente

Cantidad.

---

## Costos por Agente

Tokens y costo estimado.

---

## Precisión del Matching

Métrica configurable.

---

## Precisión del Scoring

Métrica configurable.

---

# KPI de Workflows

## Ejecuciones n8n

Cantidad.

---

## Fallos de Workflow

Cantidad.

---

## Reintentos

Cantidad.

---

## Tiempo de Ejecución

Promedio.

---

# KPI de Seguridad

## Accesos Sensibles

Cantidad.

---

## Accesos Denegados

Cantidad.

---

## Incidentes

Cantidad por período.

---

# KPI de Auditoría

## Cambios de Estado

Cantidad.

---

## Aprobaciones

Cantidad.

---

## Rechazos

Cantidad.

---

## Acciones por Usuario

Cantidad.

---

# Reportes Obligatorios

## Reporte Ejecutivo

Mensual.

---

## Reporte de Reclutamiento

Semanal.

---

## Reporte de SLA

Diario.

---

## Reporte de Auditoría

Mensual.

---

## Reporte de IA

Mensual.

---

# Visualizaciones

Utilizar:

* KPI Cards
* Tablas
* Barras
* Líneas
* Embudos
* Tendencias

---

# Exportación

Todos los reportes deben permitir:

* Excel
* PDF

---

# Seguridad

Los reportes deben respetar:

* Roles
* Permisos
* Accesos

---

# Trazabilidad

Todo KPI debe indicar:

* Fuente de datos
* Fecha de cálculo
* Fórmula utilizada

---

# Validación Final

Antes de aprobar cualquier dashboard verificar:

* Cumple PRD.
* Cumple Seguridad.
* Cumple Auditoría.
* Cumple Trazabilidad.
* Cumple Gobierno de Datos.

Ningún KPI puede basarse en datos no auditables.

Toda métrica debe poder reconstruirse desde las entidades transaccionales del sistema.
