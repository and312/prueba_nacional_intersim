# KB_05_Security_Compliance

## Descripción

Esta Knowledge Base define las políticas de seguridad, auditoría, cumplimiento, protección de datos, gestión de accesos y gobierno tecnológico del Sistema Inteligente de Reclutamiento para Nacional Seguros.

Todos los componentes de la solución deben cumplir obligatoriamente con estas directrices.

---

# Objetivos de Seguridad

La solución debe garantizar:

* Confidencialidad
* Integridad
* Disponibilidad
* Trazabilidad
* No Repudio
* Gobierno de Datos

---

# Principios Generales

Aplicar:

* Zero Trust
* Least Privilege
* Defense in Depth
* Secure by Design
* Privacy by Design

---

# OWASP Top 10

Toda solución debe prevenir:

## A01 - Broken Access Control

Validar:

* Roles
* Permisos
* Accesos

---

## A02 - Cryptographic Failures

Aplicar:

* TLS 1.3
* HTTPS
* Hashing seguro
* Cifrado de datos sensibles

---

## A03 - Injection

Prevenir:

* SQL Injection
* Command Injection
* LDAP Injection

Utilizar:

* Queries parametrizadas
* ORM seguro

---

## A04 - Insecure Design

Aplicar:

* Revisión arquitectónica
* Threat Modeling

---

## A05 - Security Misconfiguration

Validar:

* Configuración de ambientes
* Configuración de servidores
* Configuración de APIs

---

## A06 - Vulnerable Components

Monitorear:

* Dependencias
* Librerías
* Frameworks

---

## A07 - Authentication Failures

Aplicar:

* MFA
* JWT
* Expiración de sesión

---

## A08 - Software Integrity Failures

Controlar:

* CI/CD
* Versionado
* Integridad de despliegues

---

## A09 - Logging Failures

Garantizar:

* Logs
* Auditoría
* Monitoreo

---

## A10 - SSRF

Validar:

* URLs externas
* Integraciones

---

# Autenticación

Método oficial:

JWT

---

# Requisitos JWT

Validar:

* Firma
* Emisor
* Audiencia
* Expiración

---

# MFA

Obligatorio para:

* Administradores
* RRHH
* Usuarios con acceso sensible

---

# Autorización

Aplicar:

RBAC

Roles mínimos:

* Administrador
* RRHH
* Reclutador
* Solicitante
* Decisor
* Auditor

---

# Datos Sensibles

Se consideran sensibles:

* Banda salarial
* Pretensión salarial
* Scoring
* Matching
* Resultados psicotécnicos
* Observaciones internas
* Evaluaciones
* Resultados IA

---

# Protección de Datos

Aplicar:

* Cifrado en tránsito
* Cifrado en reposo
* Control de acceso
* Auditoría de acceso

---

# Gestión de Secretos

Utilizar:

* Almacén de secretos corporativo
* Secret Manager
* Variables Seguras

Prohibido:

* Passwords hardcodeados
* API Keys hardcodeadas
* Connection Strings visibles

---

# Seguridad Backend

Aplicar:

* JWT
* Policies
* Authorization Handlers
* Middleware de seguridad

---

# Seguridad Frontend

Aplicar:

* Route Guards
* Role Guards
* Session Management

No almacenar:

* Tokens en URLs
* Información sensible en Local Storage

---

# Seguridad SQL Server

Aplicar:

* Roles
* Permisos
* Auditoría
* Cifrado

Prohibido:

* SQL Dinámico inseguro
* Usuarios con privilegios excesivos

---

# Seguridad n8n

Aplicar:

* Secrets protegidos
* Variables seguras
* Acceso restringido

Prohibido:

* Credenciales en nodos
* Tokens visibles

---

# Seguridad IA

Validar:

* Versionado de prompts
* Auditoría de prompts
* Auditoría de respuestas

Prohibido enviar al modelo:

* Contraseñas
* Tokens
* Credenciales
* Secretos

---

# Auditoría

Toda acción relevante debe registrarse.

AuditLog mínimo:

* Usuario
* Rol
* Fecha
* Acción
* Módulo
* Estado anterior
* Estado nuevo
* Canal
* Observación

---

# AgentExecution

Toda ejecución IA debe registrar:

* AgentName
* PromptVersion
* Usuario
* Fecha
* Input
* Output
* Duración
* Estado
* Error

---

# Logging

Aplicar:

Logs estructurados JSON.

Registrar:

* APIs
* Integraciones
* Errores
* Seguridad
* IA
* n8n

---

# Observabilidad

Implementar:

* Métricas
* Dashboards
* Alertas
* Health Checks

---

# DevSecOps

Pipeline obligatorio:

* Build
* Unit Test
* Security Scan
* Dependency Scan
* Secret Scan
* Deploy

---

# Ambientes

Separación obligatoria:

DEV

QA

PROD

Prohibido compartir:

* Credenciales
* Bases de datos
* Secretos

---

# Gestión de Incidentes

Clasificación:

* Baja
* Media
* Alta
* Crítica

Toda incidencia debe:

* Registrarse
* Auditarse
* Documentarse

---

# Validación Final

Todo diseño debe cumplir:

* OWASP Top 10
* JWT
* RBAC
* MFA
* Auditoría
* Trazabilidad
* DevSecOps
* Protección de Datos

Si existe conflicto entre funcionalidad y seguridad, prevalece la seguridad.

Si existe conflicto entre automatización y auditoría, prevalece la auditoría.

Si existe conflicto entre velocidad y protección de datos, prevalece la protección de datos.
