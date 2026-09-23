# KB_03_Architecture_Standards

## Descripción

Esta Knowledge Base define los estándares de arquitectura, diseño, desarrollo y construcción que deben aplicarse obligatoriamente en el Sistema Inteligente de Reclutamiento para Nacional Seguros.

Todos los componentes desarrollados deberán respetar estos lineamientos.

---

# Principios Arquitectónicos

La solución debe priorizar:

* Escalabilidad
* Mantenibilidad
* Seguridad
* Trazabilidad
* Auditoría
* Observabilidad
* Desacoplamiento
* Reutilización

---

# Arquitectura General

Arquitectura objetivo:

Frontend Angular

↓

Backend .NET 8

↓

SQL Server 2022

↓

Integraciones

↓

n8n

↓

LLMs

---

# Arquitectura Limpia

Aplicar Clean Architecture.

Capas obligatorias:

## Domain

Contiene:

* Entidades
* Value Objects
* Enumeraciones
* Reglas de negocio

No debe depender de ninguna otra capa.

---

## Application

Contiene:

* Casos de uso
* Servicios de aplicación
* DTOs
* Interfaces

---

## Infrastructure

Contiene:

* SQL Server
* Repositorios
* Integraciones
* Correo
* WhatsApp
* n8n
* Archivos

---

## Presentation

Contiene:

* APIs REST
* Controllers
* Middleware
* Validaciones

---

# Principios SOLID

Aplicar siempre:

* Single Responsibility Principle
* Open Closed Principle
* Liskov Substitution Principle
* Interface Segregation Principle
* Dependency Inversion Principle

---

# Domain Driven Design

Aplicar:

* Bounded Contexts
* Entities
* Value Objects
* Domain Services
* Aggregates

---

# Bounded Contexts

## Seguridad

Usuarios.
Roles.
Permisos.

---

## Solicitudes

Gestión de requerimientos.

---

## Perfiles

Construcción de perfiles.

---

## Vacantes

Administración de vacantes.

---

## Postulantes

Gestión de candidatos.

---

## Agenda

Entrevistas y seguimiento.

---

## Auditoría

Eventos y trazabilidad.

---

## Parametrización

Configuraciones.

---

## IA

Agentes y prompts.

---

# Backend Standards

Tecnología:

.NET 8

Patrones:

* Repository Pattern
* Unit Of Work
* Dependency Injection

Toda lógica de negocio debe residir en Application o Domain.

Prohibido:

* Lógica de negocio en Controllers.
* SQL embebido en Controllers.
* Dependencias directas entre módulos.

---

# API Standards

Toda API debe:

* Ser REST.
* Estar documentada en Swagger.
* Tener versionado.
* Utilizar DTOs.
* Manejar errores estandarizados.

Formato de respuesta:

Success

Error

ValidationError

BusinessError

---

# Angular Standards

Arquitectura:

Feature Based.

Separar:

* Pages
* Components
* Services
* Models
* Guards
* Interceptors

Aplicar:

* Lazy Loading
* Reactive Forms
* Route Guards

---

# SQL Server Standards

Motor:

SQL Server 2022

Aplicar:

* PK
* FK
* Índices
* Constraints
* Auditoría

Evitar:

* SQL dinámico inseguro
* Tablas sin claves
* Duplicidad de datos

---

# Naming Standards

## Tablas

Singular.

Ejemplo:

Usuario
Rol
Vacante
Postulante

---

## APIs

Formato:

api/v1/recurso

Ejemplo:

api/v1/vacantes

---

## DTOs

Sufijos:

CreateDto
UpdateDto
ResponseDto

---

## Servicios

Sufijo:

Service

---

## Repositorios

Sufijo:

Repository

---

# Integración con n8n

n8n es una capa de automatización.

No debe contener:

* Reglas críticas de negocio.
* Persistencia principal.
* Decisiones finales.

Toda integración debe realizarse mediante APIs.

---

# Gestión de Errores

Implementar:

* Global Exception Handler
* CorrelationId
* Logging estructurado

---

# Observabilidad

Implementar:

* Health Checks
* Logs JSON
* Métricas
* Alertas

Registrar:

* APIs
* Integraciones
* IA
* Workflows

---

# Escalabilidad

Diseñar para soportar:

* Incremento de usuarios
* Incremento de vacantes
* Incremento de postulantes
* Incremento de integraciones

---

# Reglas de Revisión

Antes de aprobar cualquier diseño verificar:

* Cumple Clean Architecture.
* Cumple SOLID.
* Cumple DDD.
* Cumple Seguridad.
* Cumple Auditoría.
* Cumple Escalabilidad.
* Cumple Mantenibilidad.

---

# Resultado Esperado

Todo artefacto generado deberá seguir estos estándares para garantizar uniformidad técnica en todo el proyecto.
