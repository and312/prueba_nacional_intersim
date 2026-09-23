# Base de Conocimiento de Automatización e IA: n8n Enterprise

Este documento representa la base de conocimiento para la arquitectura de workflows en n8n, la gobernanza de Inteligencia Artificial y el catálogo de agentes para el **Sistema Inteligente de Reclutamiento (SIR)** de **Nacional Seguros**. 

La especificación técnica completa, detallada y certificada de todos los flujos de automatización se encuentra en el documento maestro del proyecto:
👉 **[ARQUITECTURA_N8N_WORKFLOWS.md](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/arq/ARQUITECTURA_N8N_WORKFLOWS.md)**

---

## Resumen Ejecutivo de la Arquitectura de Automatización
n8n se concibe única y exclusivamente como un motor de automatización y orquestación de tareas de IA y notificaciones externas:
* **Comunicación Segura:** REST APIs seguras cifradas mediante HTTPS (TLS 1.3) y autenticadas con API Keys (`X-API-Key`).
* **Cero Conexión Directa a Base de Datos:** n8n no tiene conexión directa a SQL Server. Los datos se persisten consumiendo endpoints de callback en el backend de .NET 8.
* **Gobernanza de Prompts:** Queda strictly prohibido hardcodear prompts en los nodos de n8n. Se leen dinámicamente desde el catálogo de base de datos (`PromptVersion`) a través de la API.
* **Resiliencia e Idempotencia:** Implementación del flujo centralizado de error `WF-ERR-01` (Global Error Handler) y reintentos automáticos exponenciales con jitter.
* **Gobernanza de IA (Human-in-the-Loop):** Los agentes actúan como copilotos consultivos y todas las decisiones de cambio de estado crítico requieren aprobación humana explícita.

Para detalles de los diagramas maestros, matrices de triggers/APIs, reintentos y observabilidad detallada, por favor consulte el documento maestro:
👉 **[ARQUITECTURA_N8N_WORKFLOWS.md](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/arq/ARQUITECTURA_N8N_WORKFLOWS.md)**
