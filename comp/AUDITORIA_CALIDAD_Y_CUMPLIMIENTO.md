# Especificación de Auditoría de Calidad y Cumplimiento

Este documento establece la matriz de validación, métricas de evaluación y estructura de reportes de auditoría que se aplicarán a cualquier diseño, código, base de datos, API, pantalla o workflow para el **Sistema Inteligente de Reclutamiento de Nacional Seguros**. Cumple íntegramente con la [Constitución del Proyecto](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/CONSTITUCION_PROYECTO.md).

---

## 1. Misión del Auditor de Calidad

* **Monitorear y Validar:** Revisar que todos los entregables respeten los lineamientos técnicos de arquitectura, seguridad, base de datos e IA.
* **Detectar Riesgos Tempranos:** Identificar desviaciones de diseño, código espagueti, deuda técnica, dependencias circulares y riesgos de seguridad.
* **Asegurar Cumplimiento:** Impedir la aprobación de cualquier desarrollo que intente desviar la arquitectura transaccional autorizada o delegar decisiones finales a la IA sin validación humana.

---

## 2. Matriz de Validación de Calidad (Checklist)

### 2.1 Arquitectura e Integridad (Puntaje 0-100)
* **[ ]** Angular 17+ y .NET 8 se utilizan como tecnologías oficiales.
* **[ ]** n8n y otros canales externos (WhatsApp, email) actúan únicamente como orquestadores o canales y **no** realizan transacciones en base de datos.
* **[ ]** Arquitectura limpia de 4 capas aplicada sin referencias cruzadas ni dependencias circulares.
* **[ ]** Implementación estricta de SOLID, DRY y KISS.

### 2.2 Cumplimiento Funcional del PRD (Puntaje 0-100)
* **[ ]** Cobertura total de los módulos obligatorios (Solicitudes, Perfiles, Vacantes, Postulantes, Agenda, Publicaciones, Analítica, Parametrización, Auditoría, Administración).
* **[ ]** Cobertura total de los roles mínimos del sistema.

### 2.3 Seguridad y Privacidad OWASP (Puntaje 0-100)
* **[ ]** Protección contra SQL Injection (uso de EF Core parametrizado o Dapper con variables parametrizadas).
* **[ ]** Sanitización de inputs contra XSS y prevención de CSRF.
* **[ ]** Autenticación mediante JWT, con expiración corta, Refresh Tokens y uso de cookies seguras (HttpOnly) o storage protegido.
* **[ ]** Rutas en Angular y endpoints en .NET protegidos mediante Roles y Claims (RBAC).
* **[ ]** Cero credenciales o connection strings hardcodeadas en repositorios de código.

### 2.4 Diseño de Base de Datos SQL Server 2022 (Puntaje 0-100)
* **[ ]** Toda tabla contiene las columnas de auditoría estándar (`CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`, `IsDeleted`).
* **[ ]** Integridad referencial fuerte (PK, FK obligatorias). No se permiten relaciones lógicas "en código" sin FK física.
* **[ ]** Índices creados para optimizar búsquedas comunes y filtrados para registros eliminados lógicamente (`IsDeleted = 0`).

### 2.5 APIs REST y Contratos (Puntaje 0-100)
* **[ ]** OpenAPI/Swagger documenta todos los endpoints expuestos.
* **[ ]** Formato estandarizado de respuesta para éxitos y errores (con `CorrelationId` y logs en JSON).
* **[ ]** Validación de DTOs en el pipeline de la API (ej. FluentValidation).

### 2.6 Workflows y Gobernanza IA (Puntaje 0-100)
* **[ ]** Prompts guardados y versionados en SQL Server, nunca estáticos dentro de n8n.
* **[ ]** Registro obligatorio de cada llamada a IA en `AgentExecutions` (con cálculo de costos y consumo de tokens).
* **[ ]** La IA asiste y sugiere, pero **no toma decisiones transaccionales finales**. Toda confirmación crítica pasa por un usuario del sistema.

---

## 3. Matriz de Clasificación de Riesgo Operativo

* 🔴 **Riesgo ALTO:** Fallo de seguridad crítico (OWASP), omisión de auditoría en tablas transaccionales, acceso directo de n8n o IA a SQL Server, omisión de aprobación humana para decisiones del sistema.
* 🟡 **Riesgo MEDIO:** Deuda técnica moderada, falta de índices en tablas de alta transaccionalidad, logs no estructurados, validaciones incompletas en DTOs.
* 🟢 **Riesgo BAJO:** Ajustes menores de formato en DTOs, comentarios faltantes, optimizaciones opcionales de UI/UX.

---

## 4. Plantilla Estandarizada para Reportes de Auditoría

Cada revisión formal de un diseño o código por parte del Auditor de Calidad se entregará bajo el siguiente esquema de respuesta:

### Resumen Ejecutivo
*(Breve descripción del artefacto evaluado y la conclusión de la auditoría).*

### Hallazgos
* **Hallazgo 1 (Código/Estructura):** Descripción y ubicación.
* **Hallazgo 2 (Seguridad):** Descripción y ubicación.

### Riesgos
* **Riesgo 1:** Nivel de riesgo (Bajo/Medio/Alto) e impacto en el sistema.

### Incumplimientos
* **Incumplimiento 1:** Regla violada del `CONSTITUCION_PROYECTO.md` o del PRD.

### Recomendaciones
* **Recomendación 1:** Corrección técnica sugerida paso a paso.

### Métricas de Evaluación
* **Cumplimiento Arquitectónico:** [0-100]
* **Cumplimiento PRD:** [0-100]
* **Seguridad:** [0-100]
* **Mantenibilidad:** [0-100]
* **Escalabilidad:** [0-100]
* **Riesgo Operativo:** [Bajo / Medio / Alto]

### Nivel de Aprobación
* **[ ] Aprobado:** El diseño cumple al 100% las directrices.
* **[ ] Aprobado con observaciones:** Requiere correcciones menores antes de producción, pero no bloquea el desarrollo.
* **[ ] Requiere correcciones:** Cambios obligatorios antes de proceder con el desarrollo o integración.
* **[ ] Rechazado:** El diseño infringe reglas fundamentales y requiere reestructuración total.
