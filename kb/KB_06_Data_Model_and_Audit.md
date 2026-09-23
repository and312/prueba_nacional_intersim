# KB_06_Data_Model_and_Audit

## Descripción

Esta Knowledge Base define el modelo de datos corporativo, las entidades principales, las reglas de persistencia, auditoría, trazabilidad, versionado y gobierno de datos del Sistema Inteligente de Reclutamiento para Nacional Seguros.

Todos los diseños de base de datos, APIs, workflows, agentes IA y reportes deberán alinearse con este modelo.

---

# Principios de Datos

La información debe garantizar:

* Integridad
* Consistencia
* Trazabilidad
* Auditabilidad
* Disponibilidad
* Escalabilidad

---

# Motor de Base de Datos

Motor Oficial:

SQL Server 2022 o superior

No utilizar como repositorio principal:

* PostgreSQL
* MongoDB
* Supabase
* SQLite

---

# Entidades Principales

## Seguridad

### Usuario

Representa usuarios del sistema.

Campos mínimos:

* UsuarioId
* Nombre
* Correo
* Estado
* FechaCreacion

---

### Rol

Representa perfiles de acceso.

Ejemplos:

* Administrador
* RRHH
* Reclutador
* Solicitante
* Decisor
* Auditor

---

### Permiso

Permisos específicos.

---

### UsuarioRol

Relación Usuario ↔ Rol.

---

### RolPermiso

Relación Rol ↔ Permiso.

---

# Solicitudes

### Solicitud

Representa requerimientos de personal.

Campos mínimos:

* SolicitudId
* Cargo
* Area
* Solicitante
* Prioridad
* Estado
* FechaSolicitud

---

### HistorialSolicitud

Registro de cambios de estado.

---

### AprobacionSolicitud

Registro de aprobaciones.

---

# Perfiles

### PerfilCargo

Perfil oficial de contratación.

Campos:

* PerfilId
* Cargo
* Descripcion
* Version
* Estado

---

### PerfilVersion

Historial de versiones.

---

### Skill

Catálogo de habilidades.

---

### PerfilSkill

Relación Perfil ↔ Skill.

---

# Vacantes

### Vacante

Vacante formal del proceso.

Campos:

* VacanteId
* PerfilId
* Estado
* FechaApertura
* FechaCierre

---

### Publicacion

Publicaciones realizadas.

---

### VacanteCanal

Canales utilizados.

---

# Postulantes

### Postulante

Información principal del candidato.

---

### Expediente

Expediente completo.

---

### Documento

Documentos asociados.

---

### Matching

Resultado de matching.

---

### Scoring

Resultado de scoring.

---

### Evaluacion

Resultados de evaluaciones.

---

### Entrevista

Información de entrevistas.

---

# Agenda

### EventoAgenda

Entrevistas y reuniones.

---

### ParticipanteEvento

Participantes de agenda.

---

# Parametrización

### Estado

Catálogo de estados.

---

### Canal

Canales de reclutamiento.

---

### Plantilla

Plantillas de comunicación.

---

### SLA

Reglas SLA.

---

### Configuracion

Parámetros generales.

---

# Inteligencia Artificial

### Agente

Catálogo de agentes.

---

### Prompt

Prompt principal.

---

### PromptVersion

Versionado obligatorio.

Campos:

* PromptVersionId
* PromptId
* Version
* Estado
* Fecha
* Autor

---

### AgentExecution

Registro obligatorio de ejecuciones.

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

---

# Auditoría

### AuditLog

Registro centralizado.

Campos mínimos:

* AuditId
* Fecha
* Usuario
* Rol
* Modulo
* Accion
* Entidad
* EntidadId
* EstadoAnterior
* EstadoNuevo
* Canal
* Observacion

---

### AuditDetail

Detalle ampliado.

---

### ErrorLog

Errores funcionales y técnicos.

---

# Campos de Auditoría Obligatorios

Toda entidad principal debe incluir:

CreatedBy

CreatedDate

ModifiedBy

ModifiedDate

---

Cuando aplique:

DeletedBy

DeletedDate

IsDeleted

---

# Soft Delete

Aplicar preferentemente:

IsDeleted

No eliminar físicamente información crítica.

---

# Versionado

Versionar obligatoriamente:

* Perfiles
* Prompts
* Configuraciones
* Reglas
* Plantillas

---

# Integridad Referencial

Aplicar:

* PK
* FK
* Unique Constraints
* Check Constraints

No permitir relaciones huérfanas.

---

# Índices

Crear índices para:

* Vacantes
* Postulantes
* Solicitudes
* Auditoría
* AgentExecution

Optimizar consultas de dashboard.

---

# Historiales

Mantener historial para:

* Solicitudes
* Perfiles
* Vacantes
* Postulantes
* Configuración
* Prompts

---

# Retención

No eliminar:

* Auditoría
* AgentExecution
* Historiales

Salvo política explícita de retención.

---

# Reportería

El modelo debe soportar:

* KPIs
* Dashboards
* SLA
* Productividad
* Analítica IA

---

# Validación Final

Todo diseño de datos debe cumplir:

* SQL Server 2022
* Integridad referencial
* Auditoría
* Trazabilidad
* Escalabilidad
* Seguridad

Si existe conflicto entre rendimiento y auditoría, prevalece la auditoría.

Si existe conflicto entre simplicidad y trazabilidad, prevalece la trazabilidad.
