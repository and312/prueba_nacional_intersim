# Estrategia de Aseguramiento de Calidad y Testing (QA)

Este documento establece los niveles de pruebas, la cobertura mínima, los escenarios de prueba obligatorios, la plantilla estandarizada de casos de prueba y las estrategias de validación de seguridad y auditoría para el **Sistema Inteligente de Reclutamiento de Nacional Seguros**. Cumple rigurosamente con la [Constitución del Proyecto](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/CONSTITUCION_PROYECTO.md) y las directrices del [Auditor de Calidad](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md).

---

## 1. Niveles y Cobertura de Testing

Para garantizar la fiabilidad del sistema en cada despliegue, se definen los siguientes niveles de pruebas:

1. **Unit Testing (Pruebas Unitarias):**
   * **Objetivo:** Validar la lógica aislada de servicios, casos de uso (MediatR Handlers) y entidades de dominio.
   * **Cobertura Mínima de Código:** **80%** obligatorio en la capa de Backend (`Application` y `Domain`) y componentes del Frontend.
   * **Frameworks:** xUnit / NSubstitute para Backend, Jasmine / Karma para Angular.
2. **Integration Testing (Pruebas de Integración):**
   * **Objetivo:** Validar la correcta comunicación entre componentes internos y externos (Backend ↔ SQL Server LocalDB/TestDB, Backend ↔ n8n API, Backend ↔ SMTP/WhatsApp).
3. **API Testing:**
   * **Objetivo:** Validar endpoints de la API (Request/Response, códigos de estado HTTP correctos, encabezados JWT y validación automática de datos con FluentValidation).
4. **Functional Testing (Pruebas Funcionales):**
   * **Objetivo:** Validar el comportamiento punta a punta (End-to-End) de las historias de usuario y flujos completos de negocio.
5. **UAT (Pruebas de Aceptación del Usuario):**
   * **Objetivo:** Validaciones de usabilidad y procesos operativos ejecutadas directamente por los stakeholders clave (RRHH, Solicitantes, Decisores, Reclutadores, Auditores).

---

## 2. Validación de Seguridad y Auditoría (QA Mandatario)

El equipo de control de calidad (QA) debe validar activamente los siguientes aspectos de seguridad e integridad en cada ciclo:

* **Privilegio Mínimo (RBAC):** Verificar que los endpoints restringidos retornen `403 Forbidden` o `401 Unauthorized` cuando son accedidos con JWT de roles no autorizados (ej. un *Solicitante* intentando acceder a la pantalla de *Auditoría*).
* **Validación de Cifrado (HTTPS):** Garantizar que todas las llamadas de red se realicen exclusivamente sobre HTTPS TLS 1.3.
* **Trazabilidad en Base de Datos:** Comprobar en base de datos que tras cada acción de inserción/modificación/borrado lógico se actualicen adecuadamente las columnas de auditoría y se genere un registro en `AuditLogs`.
* **Trazabilidad de IA (`AgentExecution`):** Validar que toda llamada a n8n para generación de perfiles, matching o scoring cree un registro en `AgentExecutions` detallando modelo, duración, entrada, salida y costo.

---

## 3. Plantilla Estandarizada para Casos de Prueba

Todos los casos de prueba redactados para el proyecto se estructuran bajo el siguiente formato:

| Campo | Descripción |
| :--- | :--- |
| **ID** | Identificador único del caso de prueba (ej. `TC_SOL_01`, `TC_SEC_05`). |
| **Nombre** | Título descriptivo y conciso de la prueba. |
| **Objetivo** | Qué funcionalidad, regla o excepción se está validando. |
| **Precondiciones** | Estado inicial del sistema y datos previos requeridos. |
| **Pasos de Ejecución** | Flujo secuencial y detallado de acciones del usuario o llamadas API. |
| **Resultado Esperado** | Comportamiento, cambio de estado, logs de auditoría y respuestas esperadas. |
| **Resultado Obtenido** | Detalle de lo sucedido durante la ejecución real de la prueba. |
| **Estado** | `Pasa` (Pass), `Falla` (Fail), `Bloqueado` (Blocked) o `No Ejecutado`. |

---

## 4. Escenarios de Prueba Obligatorios por Módulo

### 4.1 Módulo de Solicitudes (Kanban)
* **Escenario Exitoso:** Solicitante crea una solicitud en estado *Borrador*, la envía a validación, y el Decisor la aprueba pasando a estado *Aprobada*.
* **Escenario Negativo (Seguridad):** Solicitante intenta aprobar su propia solicitud de personal (Debe retornar error de permisos).
* **Escenario Límite:** Solicitante envía datos vacíos o campos con caracteres de longitud no permitida.

### 4.2 Módulo de Perfiles (IA Asistido)
* **Escenario de IA (Co-Piloto):** Reclutador solicita borrador de perfil, se ejecuta workflow en n8n, se devuelve callback, Reclutador edita el texto en Angular y lo aprueba guardándolo en base de datos.
* **Escenario de Auditoría de IA:** Verificar que el prompt versión X utilizado quede enlazado en `AgentExecutions.PromptVersionId`.

### 4.3 Módulo de Postulantes (Pipeline)
* **Escenario de Matching:** Reclutador sube un CV de postulante, la IA realiza matching y retorna un Score explicable. Comprobar que no se descarte al candidato automáticamente independientemente del puntaje obtenido.
