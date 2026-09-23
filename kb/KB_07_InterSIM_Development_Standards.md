# KB_07_InterSIM_Development_Standards

## Descripción

Esta Knowledge Base contiene los estándares oficiales de desarrollo utilizados por InterSIM para la construcción de soluciones empresariales.

Todos los componentes del Sistema Inteligente de Reclutamiento para Nacional Seguros deberán cumplir estos estándares para garantizar uniformidad, mantenibilidad, seguridad y calidad.

---

# Principios Generales

Todo desarrollo debe priorizar:

* Legibilidad
* Mantenibilidad
* Escalabilidad
* Reutilización
* Seguridad
* Auditoría
* Trazabilidad

---

# Principios de Desarrollo

Aplicar siempre:

* SOLID
* DRY
* KISS
* YAGNI
* Separation of Concerns
* Clean Code

---

# Convenciones .NET

## Arquitectura

Aplicar:

* Clean Architecture
* DDD
* Repository Pattern
* Dependency Injection

---

## Estructura

Domain

Application

Infrastructure

Presentation

---

## Controllers

Responsabilidades:

* Validar requests
* Invocar casos de uso
* Retornar respuestas

Prohibido:

* Lógica de negocio
* SQL
* Integraciones directas

---

## Services

Sufijo:

Service

Ejemplo:

VacanteService

PostulanteService

AgendaService

---

## Repositories

Sufijo:

Repository

Ejemplo:

VacanteRepository

PostulanteRepository

---

## DTOs

Sufijos:

CreateDto

UpdateDto

ResponseDto

FilterDto

Ejemplo:

VacanteCreateDto

VacanteUpdateDto

VacanteResponseDto

---

## Commands

Sufijo:

Command

Ejemplo:

CreateVacanteCommand

---

## Queries

Sufijo:

Query

Ejemplo:

GetVacanteByIdQuery

---

# API Standards

Formato:

api/v1/recurso

Ejemplos:

api/v1/vacantes

api/v1/postulantes

api/v1/solicitudes

---

# Respuestas API

Formato estándar:

Success

ValidationError

BusinessError

SystemError

---

# Error Response

Campos mínimos:

* CorrelationId
* Code
* Message
* Details

---

# Angular Standards

## Arquitectura

Feature Based

Separar:

* Pages
* Components
* Services
* Models
* Guards
* Interceptors

---

## Componentes

Sufijo:

Component

Ejemplo:

VacanteListComponent

VacanteDetailComponent

---

## Services

Sufijo:

Service

Ejemplo:

VacanteService

---

## Models

Sufijo:

Model

Ejemplo:

VacanteModel

---

## Forms

Utilizar:

Reactive Forms

---

## Routing

Aplicar:

Lazy Loading

Role Guards

---

# SQL Standards

## Tablas

Utilizar singular.

Ejemplo:

Usuario

Vacante

Postulante

Solicitud

---

## Claves Primarias

Formato:

EntidadId

Ejemplo:

VacanteId

PostulanteId

UsuarioId

---

## Procedimientos

Prefijo:

sp_

Ejemplo:

sp_Vacante_Insertar

sp_Postulante_Actualizar

---

## Vistas

Prefijo:

vw_

Ejemplo:

vw_VacantesActivas

---

## Funciones

Prefijo:

fn_

Ejemplo:

fn_CalcularScore

---

# Naming Standards

Utilizar nombres descriptivos.

Evitar:

tmp1

data2

valor3

---

# Logging Standards

Todos los logs deben registrar:

* Fecha
* Usuario
* Acción
* CorrelationId
* Resultado

---

# CorrelationId

Toda solicitud debe generar un CorrelationId único.

Debe propagarse a:

* APIs
* Integraciones
* n8n
* Logs

---

# n8n Standards

## Workflows

Nombre:

Modulo_Proceso_Version

Ejemplos:

Perfil_Generacion_V1

Matching_Candidato_V1

Scoring_Postulante_V1

---

## Nodos

Utilizar nombres descriptivos.

Ejemplo:

ObtenerPerfil

CalcularMatching

RegistrarAgentExecution

---

## Errores

Todo workflow debe:

* Capturar errores
* Registrar errores
* Generar trazabilidad

---

## Reintentos

Definir:

* Número máximo
* Tiempo de espera

---

# Git Standards

Modelo:

Git Flow

---

# Branches

main

develop

feature/*

release/*

hotfix/*

---

# Commits

Formato:

Tipo: Descripción

Ejemplos:

feat: crear gestión de vacantes

fix: corregir cálculo de scoring

refactor: optimizar consulta de postulantes

---

# Documentación

Todo componente debe documentar:

* Objetivo
* Dependencias
* Entradas
* Salidas

---

# Testing Standards

Cobertura mínima recomendada:

80%

Aplicar:

* Unit Tests
* Integration Tests
* API Tests

---

# Seguridad

Prohibido:

* Credenciales hardcodeadas
* Secrets en código
* Passwords visibles

Utilizar:

* Variables seguras
* Secret Manager

---

# Revisión de Código

Todo código debe cumplir:

* SOLID
* Clean Code
* Seguridad
* Auditoría
* Trazabilidad

---

# Validación Final

Antes de aprobar cualquier desarrollo verificar:

* Cumple PRD.
* Cumple Architecture Standards.
* Cumple Security Compliance.
* Cumple Naming Standards.
* Cumple Logging Standards.
* Cumple Testing Standards.

Toda excepción debe documentarse y justificarse técnicamente.
