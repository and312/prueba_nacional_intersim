# Plan de Proyecto, Gobernanza y Planificación Estratégica (Roadmap)

Este documento establece el roadmap de fases, la estructura de desglose del trabajo (WBS), la gobernanza de sprints y los mecanismos de control de riesgos y alcance para el **Sistema Inteligente de Reclutamiento para Nacional Seguros** (Fase 1). Cumple estrictamente con la [Constitución del Proyecto](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/CONSTITUCION_PROYECTO.md) y las directrices del [Auditor de Calidad](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md).

---

## 1. Misión del Project Manager

* **Garantizar el Alcance de la Fase 1:** Evitar desvíos ("scope creep") y asegurar la entrega oportuna de los módulos prioritarios.
* **Orquestación Interdisciplinaria:** Coordinar los entregables de los arquitectos de Backend, SQL, Frontend, n8n, Seguridad y QA.
* **Control de Calidad y Trazabilidad:** Monitorear que ningún entregable pase a producción sin su respectivo reporte de auditoría y pruebas de seguridad aprobados.

---

## 2. Roadmap General (Fase 1)

El proyecto se ejecutará en 5 fases lógicas organizadas secuencialmente:

```
┌──────────────────────┐
│ 1. Descubrimiento   │ (Diseño Conceptual, Arquitectura Base y ERD Lógico)
└──────────┬───────────┘
           ▼
┌──────────────────────┐
│ 2. Diseño            │ (Mockups Frontend, Contratos REST OpenAPI, Diseños n8n)
└──────────┬───────────┘
           ▼
┌──────────────────────┐
│ 3. Construcción      │ (Codificación de API, Componentes Angular, Workflows y SQL)
└──────────┬───────────┘
           ▼
┌──────────────────────┐
│ 4. QA y Seguridad    │ (Unit Testing 80%, API testing, PenTesting, UAT por roles)
└──────────┬───────────┘
           ▼
┌──────────────────────┐
│ 5. Despliegue        │ (Contenedores a DEV, QA y Pase Final a PROD Auditado)
└──────────────────────┘
```

---

## 3. Estructura de Desglose del Trabajo (WBS / EDT)

### WBS 1: Inicialización y Gobernanza
* **1.1** Aprobación del Project Constitution y Manual de Estándares.
* **1.2** Configuración del Repositorio de Código y Ambientes Iniciales de DEV.
* **1.3** Definición del Modelo ERD de la Base de Datos Transaccional.

### WBS 2: Sprint 1 (Core Seguridad y Parametrización)
* **2.1** Implementación de base de datos (`Usuarios`, `Roles`, `Parametros`, `Prompts`).
* **2.2** APIs de Autenticación (JWT) y Control de Acceso (RBAC).
* **2.3** Módulo de Parametrización (CRUD de prompts versionados, plantillas y SLAs).

### WBS 3: Sprint 2 (Solicitudes y Perfiles de Vacantes)
* **3.1** Kanban de Solicitudes (Borrador, Validación, Aprobación).
* **3.2** Generación de Perfiles de Cargo (Integración .NET ──> n8n ──> Proveedor de LLM).
* **3.3** Registro obligatorio de `AuditLogs` y `AgentExecutions` en el flujo.

### WBS 4: Sprint 3 (Vacantes y Postulantes)
* **4.1** Gestión de Ficha de Vacante (Banda Salarial protegida, Estados y SLAs).
* **4.2** Carga de Expedientes de Postulantes (CV parsing, matching y scoring de IA).
* **4.3** Proceso manual de toma de decisiones (Aprobación/Rechazo por RRHH).

### WBS 5: Sprint 4 (Agenda, Integraciones y Cierre)
* **5.1** Calendario de Entrevistas integrado con recordatorios por WhatsApp/Email.
* **5.2** Ejecución de Pruebas de QA Unitarias (80%), Integración y Security Checks.
* **5.3** Auditoría de Cumplimiento final y Pase a Producción.

---

## 4. Gestión de Riesgos Críticos

| Riesgo | Categoría | Nivel | Mitigación |
| :--- | :--- | :--- | :--- |
| **Almacenamiento o Fuga de PII en n8n/LLM** | Seguridad | **Crítico** | Enviar payloads anonimizados con identificadores de negocio temporales a LLM externos. |
| **Omisión del Control Humano (Automatización Completa)** | Operativo | **Alto** | Diseñar bloqueos de estado en base de datos que exijan firma digital o confirmación de usuario antes del pase. |
| **Deuda Técnica por Desacople de APIs** | Arquitectura | **Medio** | Diseñar primeros contratos OpenAPI (Swagger) y mockups de datos antes de iniciar codificación frontend. |

---

## 5. Control y Gestión del Cambio

Cualquier cambio solicitado en los requerimientos del proyecto se procesará evaluando:
1. **Desviación en Cronograma:** Impacto en días en la ruta crítica del roadmap.
2. **Impacto en Arquitectura:** Si altera el flujo seguro `Angular ──> .NET ──> SQL Server` o requiere nuevos servicios externos.
3. **Costo de Implementación:** Horas de desarrollo y pruebas adicionales requeridas.
