# AGENTS.md — Identificador: ns-sir-be

## Propósito
Este repositorio contiene la solución Backend (.NET 8 Clean Architecture) y la documentación maestra del **Sistema Inteligente de Reclutamiento (SIR)** para **Nacional Seguros**. Antes de modificar código, consultar la documentación y seguir estrictamente las decisiones técnicas aquí establecidas.

---

## Prioridad de Fuentes

1. **`docs/rules/**`**: Reglas vigentes del sistema. Tienen prioridad absoluta para decisiones de dominio, permisos, seguridad RLS, trazabilidad y transiciones de estado.
2. **`docs/features/**`**: Requerimientos funcionales, flujos de usuario, criterios de aceptación y comportamiento esperado por módulo (Perfiles, Solicitudes, Vacantes, Postulantes).
3. **`docs/api/**`**: Contratos, especificaciones OpenAPI / Swagger, endpoints y payloads esperados.
4. **`docs/architecture/**`**: Decisiones técnicas transversales (Clean Architecture, DDD, CQRS, MediatR, FluentValidation).
5. **`docs/roadmaps/**`**: Planes futuros de incrementos y líneas base.

---

## Reglas para Implementación

* **No asumir reglas de negocio no documentadas:** Ante duda sobre transiciones de estado (ej: aprobación de solicitudes o perfiles), consultar `docs/rules/`.
* **Seguridad y Permisos:** No permitir acciones sin validar explícitamente Rol, Área, Permiso y Estado del Agregado en el Handler correspondiente.
* **Trazabilidad de Eventos:** Registrar eventos de auditoría cuando una acción cambie estado, actualice información sensible, responda observaciones o modifique la persistencia.
* **Acceso a Base de Datos:** No realizar escrituras directas desde scripts o automatizaciones; todas las operaciones de datos deben canalizarse mediante EF Core / Repositorios del Backend y migraciones declarativas.
* **Consistencia:** Mantener estricta consistencia entre los DTOs de Backend, contratos OpenAPI, componentes Frontend y la documentación funcional.

---

## Pila Tecnológica (Backend - ns-sir-be)

* **Framework:** .NET 8 Web API
* **Arquitectura:** Clean Architecture + Domain-Driven Design (DDD) + CQRS (MediatR)
* **Persistencia:** EF Core 9 sobre SQL Server 2022 + Dapper para consultas de lectura optimizadas
* **Seguridad:** JWT Bearer, PBKDF2 Hashing, MFA TOTP (dos fases), Interceptores Row-Level Security (RLS)
* **Validación:** FluentValidation integrado en el pipeline de MediatR
* **Pruebas:** xUnit / Moq / FluentAssertions en `tests/NacionalSeguros.Tests`
