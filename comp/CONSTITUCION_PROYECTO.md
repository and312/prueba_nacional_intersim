# Constitución del Proyecto: Sistema Inteligente de Reclutamiento para Nacional Seguros

Este documento define la arquitectura, seguridad, base de datos, trazabilidad, agentes de IA, observabilidad y experiencia de usuario obligatorios para el proyecto "Sistema Inteligente de Reclutamiento para Nacional Seguros" (Fase 1).

---

## 1. Contexto del Proyecto

* **Objetivo:** Centralizar, estandarizar y hacer trazable el proceso de reclutamiento desde la solicitud de vacante hasta la contratación.
* **Fase Actual:** Fase 1.
* **Alcance:**
  * Solicitudes
  * Generación de perfiles
  * Validación de perfiles
  * Gestión de vacantes
  * Gestión de postulantes
  * Agenda
  * Auditoría
  * Parametrización
  * Roles y permisos
  * Integraciones
  * IA asistida
* **Fuera de Alcance:**
  * Contratación automática
  * Decisiones automáticas sin intervención humana
  * Procesos transaccionales fuera de la plataforma

---

## 2. Reglas de Arquitectura Obligatorias

### Tecnologías Autorizadas
* **Frontend:** Angular
* **Backend:** .NET 8 o superior
* **Base de Datos:** SQL Server 2022 o superior
* **Motor de Agentes:** n8n
* **Canales de Comunicación:** WhatsApp, Correo electrónico

### Restricciones Críticas de Flujo de Datos
1. **SQL Server** es la única fuente oficial de información.
2. **WhatsApp, Correo electrónico y n8n** no son sistemas transaccionales.
3. Toda operación debe pasar obligatoriamente por las **APIs del Backend (.NET)**.
4. Los **agentes de IA** nunca acceden directamente a las tablas de negocio.
5. Los **agentes de IA** no pueden tomar decisiones finales.
6. Toda decisión crítica requiere **aprobación humana**.

### Arquitecturas Permitidas

#### Flujo Estándar
```
[Frontend Angular] ──> [API .NET] ──> [SQL Server]
```

#### Flujo con Agentes / Automatizaciones
```
[Frontend Angular] ──> [API .NET] ──> [n8n] ──> [API .NET] ──> [SQL Server]
```

### Arquitecturas Prohibidas
* ❌ `Frontend ──> n8n ──> SQL Server`
* ❌ `Frontend ──> SQL Server`
* ❌ `n8n ──> SQL Server directamente`

---

## 3. Estándares de Desarrollo

### Principios y Patrones Obligatorios
* **SOLID** (Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion)
* **Clean Architecture**
* **Domain Driven Design (DDD)**
* **Repository Pattern**
* **Dependency Injection**
* **Separation of Concerns (SoC)**
* **OpenAPI / Swagger** para documentación de APIs
* **DRY** (Don't Repeat Yourself)
* **KISS** (Keep It Simple, Stupid)

### Prácticas Prohibidas
* ❌ Código duplicado
* ❌ Hardcodeo de configuraciones
* ❌ Lógica de negocio en la UI (Frontend)
* ❌ Lógica de negocio en los workflows de n8n
* ❌ Dependencias circulares

---

## 4. Seguridad (OWASP Top 10)

### Validaciones Obligatorias
* SQL Injection
* Cross-Site Scripting (XSS)
* Cross-Site Request Forgery (CSRF)
* Broken Authentication
* Broken Access Control
* Security Misconfiguration

### Medidas Obligatorias
* Autenticación basada en **JWT** (JSON Web Tokens) con soporte para inicio de sesión híbrido (Local y corporativo delegado al servicio de directorio corporativo de identidad vía OIDC o LDAP).
* Control de acceso basado en roles (**RBAC**)
* Conexiones seguras **HTTPS** con **TLS 1.3**
* **Password Hashing** seguro (ej. BCrypt, Argon2, PBKDF2) para cuentas de autenticación local.
* Gestión segura de secretos (ej. un almacén de secretos corporativo o variables de entorno protegidas)
* Auditoría de accesos

### Prohibido
* ❌ Credenciales expuestas en código fuente
* ❌ Cadenas de conexión (Connection strings) hardcodeadas
* ❌ Tokens expuestos
* ❌ Secretos en repositorios de código

---

## 5. Estructura de Base de Datos (SQL Server 2022)

Todas las entidades de base de datos deben contar con:
* Llave Primaria (PK)
* Llaves Foráneas (FK) con integridad referencial explícita
* Índices apropiados para optimización de consultas
* Restricciones (Constraints) de validación
* Trazabilidad y auditoría básica:

### Campos de Auditoría Obligatorios por Tabla
* `CreatedBy` (Usuario que creó el registro)
* `CreatedDate` (Fecha y hora de creación)
* `ModifiedBy` (Usuario que modificó por última vez)
* `ModifiedDate` (Fecha y hora de la última modificación)
* `DeletedBy` (Usuario que realizó borrado lógico, si aplica)
* `DeletedDate` (Fecha y hora del borrado lógico, si aplica)

---

## 6. Trazabilidad y Auditoría (AuditLog)

Toda acción relevante dentro de la plataforma debe registrarse de manera persistente en un log de auditoría. Está prohibido eliminar registros de esta tabla.

### Campos Mínimos del AuditLog
* `Fecha`
* `Usuario`
* `Rol`
* `Acción`
* `Módulo`
* `Canal`
* `Estado anterior`
* `Estado nuevo`
* `Observación`

---

## 7. Reglas para Agentes de IA

### Registro de Ejecuciones (`AgentExecution`)
Cada ejecución de un agente de IA debe quedar registrada con los siguientes campos mínimos:
* `Id` (Identificador único de ejecución)
* `AgentName` (Nombre del agente)
* `ExecutionDate` (Fecha y hora de ejecución)
* `User` (Usuario que disparó o interactuó con el agente)
* `Input` (Entrada provista al agente)
* `Output` (Respuesta generada por el agente)
* `Status` (Estado de la ejecución: Éxito/Fallo)
* `ErrorMessage` (Mensaje de error, en caso de fallo)
* `Duration` (Tiempo de respuesta en milisegundos)

### Propiedad y Gestión de Prompts
* Los prompts son propiedad intelectual exclusiva de Nacional Seguros.
* Todos los prompts deben ser **versionados**, **almacenados**, **auditables** y **recuperables**.

---

## 8. Observabilidad

Es obligatorio implementar herramientas y prácticas de monitoreo:
* **Logs estructurados** en formato JSON.
* **Health Checks** activos para APIs y dependencias.
* **Métricas** de rendimiento.
* **Alertas** configuradas para fallos críticos.
* **Dashboard de monitoreo** centralizado.
* **Registros de:** Errores, excepciones, integraciones, ejecuciones de IA y tiempos de respuesta.

---

## 9. Experiencia de Usuario y Accesos

### Módulos Mínimos en la Interfaz
* Inicio (Dashboard)
* Solicitudes
* Vacantes
* Postulantes
* Agenda
* Publicaciones
* Analítica
* Parametrización
* Auditoría
* Administración

### Roles Mínimos del Sistema
* **RRHH:** Gestión general del proceso de reclutamiento.
* **Solicitante:** Líderes o jefes de área que solicitan cubrir vacantes.
* **Decisor:** Gerentes o directores que aprueban/rechazan en puntos clave.
* **Reclutador:** Ejecutores directos de la búsqueda y preselección.
* **Administrador:** Gestión de configuraciones, roles, seguridad y parámetros.
* **Auditor:** Acceso de solo lectura a los logs de auditoría e historial del sistema.

---

## 10. Documentación Obligatoria por Diseño

Cada propuesta de diseño o implementación técnica debe ir acompañada de:
* Diagramas de Arquitectura C4 (Contexto, Contenedores, Componentes, Código)
* Modelo Entidad-Relación (ERD) actualizado
* Especificación OpenAPI / Swagger
* Diagramas de flujo de procesos
* Casos de uso detallados
* Manual Técnico del Desarrollador
* Manual de Despliegue (Deployment Guide)
