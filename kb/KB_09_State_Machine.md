# KB_09_State_Machine

## Descripción

Esta Knowledge Base define el modelo oficial de estados, transiciones, validaciones, aprobaciones y reglas de negocio para el Sistema Inteligente de Reclutamiento de Nacional Seguros.

Toda transición deberá ser auditable, trazable y gobernada.

No se permiten cambios directos de estado fuera del motor de estados.

---

# Principios

Toda entidad con ciclo de vida debe:

* Tener estados definidos.
* Tener transiciones permitidas.
* Registrar historial.
* Registrar responsable.
* Registrar fecha.
* Registrar observación.
* Registrar canal.
* Registrar aprobaciones cuando aplique.

---

# Motores de Estado

Existen tres motores independientes:

1. Solicitudes
2. Vacantes
3. Postulantes

No deben mezclarse.

---

# Máquina de Estados de Solicitud

## Estados

Borrador

EnValidacion

Observada

Aprobada

Rechazada

ConvertidaAVacante

Cancelada

---

## Transiciones Permitidas

Borrador → EnValidacion

EnValidacion → Observada

EnValidacion → Aprobada

EnValidacion → Rechazada

Observada → EnValidacion

Aprobada → ConvertidaAVacante

CualquierEstado → Cancelada

---

## Restricciones

No puede convertirse en vacante si no está aprobada.

Toda aprobación debe registrar:

* Usuario
* Rol
* Fecha
* Comentario

---

# Máquina de Estados de Vacante

## Estados

Creada

PendientePublicacion

Publicada

Captacion

Screening

Psicotecnica

Entrevistas

Shortlist

Oferta

Contratada

Cerrada

Cancelada

---

## Transiciones Permitidas

Creada → PendientePublicacion

PendientePublicacion → Publicada

Publicada → Captacion

Captacion → Screening

Screening → Psicotecnica

Psicotecnica → Entrevistas

Entrevistas → Shortlist

Shortlist → Oferta

Oferta → Contratada

Contratada → Cerrada

CualquierEstado → Cancelada

---

## Restricciones

No puede publicarse sin perfil aprobado.

No puede cerrarse sin resultado final.

Toda transición debe quedar auditada.

---

# Máquina de Estados de Postulante

## Estados

Registrado

Captado

Preseleccionado

Psicotecnica

EntrevistaRRHH

EntrevistaTecnica

Shortlist

Oferta

Contratado

Descartado

Retirado

---

## Transiciones Permitidas

Registrado → Captado

Captado → Preseleccionado

Preseleccionado → Psicotecnica

Psicotecnica → EntrevistaRRHH

EntrevistaRRHH → EntrevistaTecnica

EntrevistaTecnica → Shortlist

Shortlist → Oferta

Oferta → Contratado

CualquierEstado → Descartado

CualquierEstado → Retirado

---

## Restricciones

Ningún postulante puede contratarse sin pasar por oferta.

Todo descarte debe registrar motivo.

---

# Historial de Estados

Toda transición genera:

StateHistory

Campos:

* StateHistoryId
* Entidad
* EntidadId
* EstadoAnterior
* EstadoNuevo
* Usuario
* Rol
* Fecha
* Comentario
* Canal

---

# Aprobaciones

Estados críticos requieren aprobación humana.

Ejemplos:

* Aprobación de perfil.
* Estrategia de búsqueda.
* Shortlist.
* Oferta.
* Contratación.

---

# SLA por Estado

El sistema deberá permitir configurar SLA por:

* Solicitud
* Vacante
* Postulante

Cada estado puede tener:

* Tiempo objetivo
* Tiempo máximo
* Alertas
* Escalamientos

---

# Alertas

Generar alertas para:

* Sin respuesta.
* SLA vencido.
* Estado bloqueado.
* Pendiente de aprobación.
* Entrevista no confirmada.

---

# Integración con n8n

Todo cambio de estado debe generar eventos:

SolicitudEstadoCambiado

VacanteEstadoCambiado

PostulanteEstadoCambiado

Estos eventos podrán activar workflows.

---

# Auditoría

Toda transición debe registrarse en:

AuditLog

StateHistory

Timeline

---

# Validación Final

Antes de aprobar una transición verificar:

* Rol autorizado.
* Estado válido.
* Datos requeridos completos.
* Reglas de negocio cumplidas.
* Auditoría registrada.

No se permiten cambios directos de estado mediante SQL o actualizaciones manuales.
