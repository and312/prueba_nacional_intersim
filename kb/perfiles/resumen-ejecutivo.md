# Resumen Ejecutivo del Perfil

**Estado de Implementación**: Implementado en Frontend / Renderizado de Insumos  
**Última Actualización**: 12/07/2026

El **Resumen Ejecutivo** es una síntesis ejecutiva generada a partir de los datos consolidados en las 12 secciones del perfil estructurado. Ofrece una lectura fluida e inmediata de la vacante.

---

## 1. Reglas Generales
*   **Solo Lectura**: El resumen ejecutivo es 100% de solo lectura en todo momento, tanto para RRHH como para el Área Solicitante. No cuenta con controles de edición manual directa.
*   **Regeneración**: El resumen se regenera automáticamente cuando RRHH guarda cambios o ajustes estructurales en el profesiograma y pasa al estado `ResumenEjecutivoGenerado`.
*   **Estados de Generación**:
    *   `PENDIENTE_GENERACION`: El perfil estructurado aún no ha sido procesado por el agente IA.
    *   `GENERANDO`: El agente de IA está procesando los textos estructurados (bloqueo visual con spinner).
    *   `GENERADO`: Resumen listo y visible en la pestaña.
    *   `ERROR_GENERACION`: Fallo al conectar con el servicio LLM.

---

## 2. Contenido Estructurado
El componente renderiza los siguientes bloques de datos:

1.  **Resumen ejecutivo del rol**: Resumen en lenguaje natural de la posición.
2.  **Objetivo del cargo**: Misión del puesto.
3.  **Funciones principales**: Lista sintética de responsabilidades clave.
4.  **Requisitos mínimos**: Criterios excluyentes.
5.  **Formación y experiencia**: Títulos requeridos y años en el rubro.
6.  **Hard skills (Habilidades técnicas)**: Conocimientos de herramientas y metodologías.
7.  **Soft skills (Competencias blandas)**: Actitudes y comportamiento esperado.
8.  **Modalidad**: Presencial, híbrido o remoto.
9.  **Ubicación**: Sede física de trabajo (ej. Santa Cruz, La Paz).
10. **Salario o banda salarial**: Rango presupuestado.
11. **Criterios de evaluación**: Pautas sugeridas de ponderación.
12. **Características clave**: Rasgos actitudinales indispensables.
13. **Valoración del perfil**: Diagnóstico de viabilidad de la vacante.

> [!WARNING]
> La **jornada laboral** (horarios, turnos) **no debe incluirse** en el Resumen Ejecutivo, ya que es un parámetro puramente administrativo de la solicitud y no aporta valor estratégico al profesiograma de búsqueda.

---

## 3. Tolerancia a Información Faltante
La interfaz del resumen ejecutivo está construida con un diseño flexible y auto-ajustable:
*   **Ocultación de Bloques Opcionales**: Si una variable opcional no cuenta con información en el perfil estructurado, el bloque correspondiente se oculta del HTML.
*   **No mostrar valores técnicos nulos**: Está estrictamente prohibido renderizar textos literales como `null`, `undefined` o dejar la sección en blanco con etiquetas vacías.
*   **Sin Alturas Fijas**: Para evitar desbordamientos u huecos grises en pantallas responsive, los contenedores utilizan layouts dinámicos (flex/grid con auto-height).

---

## 4. Valoración del Perfil y Diagnóstico
La valoración técnica del profesiograma recopila indicadores clave para predecir la complejidad de la búsqueda:
*   **Criticidad del cargo**: Nivel de impacto del puesto.
*   **Coherencia interna**: Alineación entre salario, requisitos y responsabilidades.
*   **Riesgo de cobertura**: Estimación del tiempo y disponibilidad de talento en el mercado laboral.
*   **Comentario del agente**: Feedback automático del Agente de Perfiles.

> [!IMPORTANT]
> El frontend **no calcula** ni infiere estas métricas mediante fórmulas o reglas locales. Los datos de valoración provienen calculados directamente del backend tras el análisis del Agente de Perfiles.

---

## 5. Integración con el Backend (Sprint 2026)

*   **Endpoint de Consulta**: `GET /api/v1/perfiles/{id}`
*   **Método Frontend**: `obtenerResumenPorPerfilId(perfilId)` en `DatosResumenEjecutivoMockService` (que llama a `ApiService.getPerfilById(perfilId)`).
*   **Propiedad del DTO**: `resumenEjecutivo?: ResumenEjecutivo;` en `PerfilDetail`.
*   **Mapeo de Campos Backend → Vista**:
    *   `resumen` → Resumen ejecutivo del rol (`resumenRol`)
    *   `objetivoCargo` → Objetivo del cargo (`objetivoCargo`)
    *   `funcionesPrincipales` → Funciones principales (`funcionesPrincipales`)
    *   `requisitosMinimos` → Requisitos mínimos (`requisitosMinimos`)
    *   `formacionExperiencia` → Formación y experiencia (`formacionAcademica`)
    *   `hardSkills` → Habilidades técnicas (`habilidadesTecnicas`)
    *   `softSkills` → Habilidades blandas (`habilidadesBlandas`)
    *   `modalidad` → Modalidad (`modalidad`)
    *   `ubicacion` → Ubicación (`ubicacion`)
    *   `bandaSalarial` → Banda salarial (`salario`)
    *   `criteriosEvaluacion` → Criterios de evaluación (`criteriosEvaluacion`)
    *   `caracteristicasClave` → Características clave (`caracteristicasClave`)
    *   `valoracionPerfil` → Valoración del perfil (`valoracion`)
*   **Relación Solicitud-Perfil**: Consolidada en una sola llamada. El nombre y el código de la solicitud se extraen directamente del perfil devuelto (`perfil.cargo` y `perfil.codigoSolicitud`) para registrar dinámicamente la Solicitud de Vacante como insumo real.
*   **Comportamiento sin Resumen**: Si `resumenEjecutivo` es `null`, vacío o inexistente, se retorna el estado `EstadoGeneracionResumen.PendienteGeneracion`, mostrando visualmente:
    *   *Título*: "Resumen ejecutivo en proceso"
    *   *Texto*: "El resumen ejecutivo estará disponible una vez que Recursos Humanos revise y envíe el perfil al Área Solicitante."
*   **Manejo de Carga y Cambio de Perfil**: El componente `ResumenEjecutivoPerfilComponent` implementa `OnChanges` para interceptar cambios del `@Input() perfilId`. Al cambiar de perfil, limpia el estado previo (`this.resumen.set(null)`) y vuelve a consultar, evitando mezcla de datos.

