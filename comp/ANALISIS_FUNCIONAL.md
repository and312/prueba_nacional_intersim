# Guía de Análisis Funcional y Product Ownership

Este documento establece las directrices para la estructuración de Requerimientos, Historias de Usuario (User Stories), Casos de Uso, Criterios de Aceptación (Gherkin) y Reglas de Negocio para el **Sistema Inteligente de Reclutamiento de Nacional Seguros**. Cumple con la [Constitución del Proyecto](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/CONSTITUCION_PROYECTO.md) y las directrices del [Auditor de Calidad](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md).

---

## 1. Misión del Analista Funcional

* **Traducción de Negocio:** Convertir las necesidades de la gerencia de Recursos Humanos y de los líderes solicitantes en especificaciones técnicas claras y estructuradas.
* **Gobernanza del Alcance:** Garantizar que todas las historias de usuario implementen el control humano y la auditoría explícita requerida por la constitución.
* **Definición de Pruebas de Aceptación:** Establecer criterios claros que los desarrolladores y el equipo de QA puedan usar para verificar el éxito de la entrega.

---

## 2. Plantilla Estructurada de Historias de Usuario

Todas las historias de usuario que se generen en el proyecto deberán redactarse bajo la siguiente sintaxis:

```markdown
### US-[Número]: [Nombre de la Historia de Usuario]

**Como** [Rol del Sistema (ej. RRHH, Solicitante, Decisor, Reclutador, Administrador)]
**Quiero** [Acción u Objetivo funcional a realizar en la plataforma]
**Para** [El beneficio de negocio, justificación o valor agregado obtenido]

#### Reglas de Negocio Asociadas
* **Actor:** Quien ejecuta el evento.
* **Evento:** La acción iniciadora.
* **Condición:** Los prerrequisitos lógicos para procesar la acción.
* **Resultado:** El efecto exitoso (ej. cambio de estado, persistencia, notificación).
* **Excepción:** Qué ocurre si falla una condición (ej. mensajes de error, logueo de auditoría).

#### Criterios de Aceptación (Gherkin)
* **Escenario 1: [Nombre del escenario exitoso]**
  * **GIVEN** [Estado inicial del sistema y condiciones]
  * **WHEN** [El actor ejecuta la acción]
  * **THEN** [El sistema realiza los cambios, genera auditoría y muestra el resultado esperado]
* **Escenario 2: [Nombre de escenario alterno o fallo]**
  * **GIVEN** [Estado inicial]
  * **WHEN** [El actor ejecuta la acción incorrecta o con datos inválidos]
  * **THEN** [El sistema bloquea la transacción, genera el ErrorLog y muestra el mensaje de error correspondiente]
```

---

## 3. Plantilla de Casos de Uso

Para describir flujos complejos que involucran APIs, interfaces y orquestaciones con n8n, se definirá el siguiente formato de Caso de Uso:

* **Nombre:** Nombre del caso de uso.
* **Actor Principal:** Rol que lo ejecuta.
* **Precondiciones:** Estado requerido del sistema.
* **Flujo Principal:** Paso a paso numerado de la ejecución feliz.
* **Flujos Alternos / Excepciones:** Desviaciones o flujos de error.
* **Postcondiciones:** Estado final en el que queda el sistema y los registros de auditoría obligatorios (`AuditLog`, `AgentExecution`).

---

## 4. Estructura de Entregables por Funcionalidad

Cuando se solicite detallar una funcionalidad del sistema, el entregable constará de:
1. **Descripción Funcional:** Resumen del alcance y comportamiento esperado de la funcionalidad.
2. **User Stories:** Redactadas en formato estándar con sus respectivos criterios de aceptación.
3. **Reglas de Negocio:** Detallando actores, eventos, condiciones, resultados y excepciones.
4. **Casos de Uso:** Flujos principales y alternativos.
5. **Dependencias:** Listado de otras funcionalidades o servicios externos necesarios.
6. **Riesgos:** Identificación de posibles cuellos de botella en la operación o implementación.
7. **Prioridad:** Clasificación de la tarea (Alta, Media, Baja) para la planeación del sprint.
