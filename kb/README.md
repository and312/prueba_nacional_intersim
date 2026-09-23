# Índice de la Base de Conocimiento (Knowledge Base - KB)

Bienvenido a la documentación oficial de la base de conocimiento (KB) del **Sistema Inteligente de Reclutamiento (SIR)** de **Nacional Seguros**. Aquí encontrará toda la especificación técnica, funcional y de arquitectura del proyecto.

---

## 📂 Estructura General de la KB

### 📋 Especificaciones y Requisitos (PRD)
*   [KB_01_PRD_NacionalSeguros.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_01_PRD_NacionalSeguros.md): Contexto funcional oficial y alcance general del sistema.
*   [KB_02_PRD_Requisitos_Funcionales.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_02_PRD_Requisitos_Funcionales.md): Listado y detalle de los Requisitos Funcionales oficiales (RF-01 a RF-16).

### 🏛️ Arquitectura y Estándares Técnicos
*   [KB_03_Architecture_Standards.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_03_Architecture_Standards.md): Estándares técnicos, patrones (Clean Architecture, DDD) y directrices generales de desarrollo.
*   [KB_07_InterSIM_Development_Standards.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_07_InterSIM_Development_Standards.md): Convenciones de nomenclatura, codificación y estructura del backend .NET 8.
*   [KB_08_UI_UX_Standards.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_08_UI_UX_Standards.md): Lineamientos visuales, colores, tipografía e interacciones de la interfaz de usuario.
*   [KB_Observability.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_Observability.md): Estándares de monitoreo, telemetría e inyección de logs.

### 🤖 Agentes de Inteligencia Artificial (IA) y Gobernanza
*   [KB_04_IA_Agents.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_04_IA_Agents.md): Catálogo y responsabilidades de los agentes IA en el flujo de selección.
*   [KB_AI_Governance.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_AI_Governance.md): Directrices éticas, auditoría, límites de coste y latencia de los modelos LLM.
*   [KB_CatalogoAgentes.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_CatalogoAgentes.md): Parámetros avanzados, prompts base y configuración operativa de cada agente.

### ⚙️ Máquina de Estados y Orquestación
*   [KB_09_State_Machine.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_09_State_Machine.md): Ciclo de vida y transiciones permitidas para solicitudes, vacantes y postulantes.
*   [KB_StateMachine.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_StateMachine.md): Especificación detallada física del motor de estados en base de datos.
*   [KB_10_Workflow_Orchestration.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_10_Workflow_Orchestration.md): Orquestación asíncrona mediante workflows (n8n, Azure Logic Apps).
*   [KB_CatalogoWorkflows.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_CatalogoWorkflows.md): Registro oficial y catálogo detallado de flujos de automatización.

### 🔒 Datos, Auditoría y Seguridad
*   [KB_05_Security_Compliance.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_05_Security_Compliance.md): Políticas de seguridad, tokens, RLS y hashing de contraseñas.
*   [KB_06_Data_Model_and_Audit.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_06_Data_Model_and_Audit.md): Esquema de base de datos relacional y diseño de tablas de auditoría.
*   [KB_11_Audit_and_Traceability.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_11_Audit_and_Traceability.md): Mecanismos inmutables de registro de logs de auditoría y timelines.

### 🔗 Integraciones y SLAs
*   [KB_Integrations.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_Integrations.md): Protocolos de conexión contra APIs de bolsa de trabajo y WhatsApp.
*   [KB_13_Agent_Contracts.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_13_Agent_Contracts.md): Esquemas de payloads JSON y mensajería conversacional.
*   [KB_SLA_Governance.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_SLA_Governance.md): Tiempos límite, escalamientos y alertas automáticas.
*   [KB_MasterData.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_MasterData.md): Datos corporativos maestros y catálogos globales de referencia.
*   [KB_12_Reporting_and_KPI.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_12_Reporting_and_KPI.md): KPIs e informes requeridos por la gerencia.

---

## 🗂️ Módulos de Aplicación Nuevos (Fase 1.5)

### 📌 Módulo de Perfiles de Vacante (`kb/perfiles/`)
*   [resumen-modulo-perfiles.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/resumen-modulo-perfiles.md): Propósito funcional del módulo, relación con solicitudes y estrategia de búsqueda.
*   [flujo-estados-perfiles.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/flujo-estados-perfiles.md): Ciclo de vida del profesiograma, transiciones y la regla crítica de aprobación del área.
*   [roles-y-permisos-perfiles.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/roles-y-permisos-perfiles.md): Matriz de facultades operativas para RRHH, Área Solicitante y Administrador.
*   [lista-perfiles.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/lista-perfiles.md): Especificación del Dashboard, métricas y grillas por rol.
*   [detalle-perfil.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/detalle-perfil.md): Anatomía de la vista de detalle y lógica de botones dinámicos en la cabecera.
*   [perfil-estructurado.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/perfil-estructurado.md): Las 12 secciones del profesiograma, reglas de edición y origen de campos.
*   [resumen-ejecutivo.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/resumen-ejecutivo.md): Síntesis de lenguaje natural generada por IA, valoración técnica e insumos.
*   [observaciones-perfil.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/observaciones-perfil.md): Detalle del desacoplamiento de secciones, checklist de resolución y nuevo modelo de datos global.
*   [trazabilidad-perfil.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/trazabilidad-perfil.md): Historial cronológico inmutable provisto por backend.
*   [contratos-backend-perfiles.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/contratos-backend-perfiles.md): API REST, paso de tokens seguros y mensajería asíncrona para WhatsApp/n8n.
*   [pendientes-y-decisiones.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/pendientes-y-decisiones.md): Registro de decisiones de negocio pendientes por validar con el Product Owner.
*   [parametrizacion-modulo.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/perfiles/parametrizacion-modulo.md): Propósito funcional, modelos, estructura modular y estado del nuevo módulo de parametrización.

### 🌐 Desarrollo Frontend (`kb/frontend/`)
*   [lineamientos-frontend-modular.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/frontend/lineamientos-frontend-modular.md): Reglas de desarrollo progresivo en Angular, standalone components, tipado estricto de TypeScript e inmutabilidad de estilos.
