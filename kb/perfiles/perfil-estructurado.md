# Perfil Estructurado (Profesiograma)

**Estado de Implementación**: Implementado en Frontend / Campos Reactivos  
**Última Actualización**: 12/07/2026

La estructura detallada del cargo se divide en 12 secciones normalizadas presentadas mediante acordeones colapsables en la interfaz de usuario.

---

## 1. Las 12 Secciones del Profesiograma
1.  **Datos generales del cargo**: Identificación básica (Misión, reportes, jerarquía).
2.  **Objetivo principal del cargo**: Propósito fundamental de la posición en la empresa.
3.  **Perfil requerido**: Requisitos formales del postulante (Edad, estado civil sugerido, etc.).
4.  **Conocimientos técnicos requeridos**: Habilidades cognitivas y saberes específicos (ej. Legislación de seguros).
5.  **Herramientas y sistemas**: Software e infraestructura de trabajo (ej. SAP, Salesforce).
6.  **Funciones principales del cargo**: Responsabilidades recurrentes del puesto.
7.  **Competencias clave**: Habilidades blandas y actitudinales requeridas por la cultura organizacional.
8.  **Indicadores de éxito del cargo**: Métricas de rendimiento y KPI (Key Performance Indicators).
9.  **Perfil ideal del candidato**: Rasgos distintivos deseables adicionales.
10. **Filtros clave para selección**: Criterios indispensables no negociables de filtrado.
11. **Matriz sugerida de ponderación**: Distribución de porcentajes de evaluación para el proceso.
12. **Perfil tipo de alto ajuste**: Resumen del candidato estrella óptimo.

---

## 2. Origen de los Campos
Cada campo del profesiograma cuenta con un metadato de procedencia para auditar la construcción del perfil:

*   `SOLICITUD_VACANTE`: Datos inmutables importados directamente desde la solicitud aprobada (ej. Título del cargo, Salario base, Área).
*   `AGENTE_IA`: Propuestas de texto o listas generadas algorítmicamente por el Agente de Perfiles.
*   `DATO_HISTORICO`: Elementos recuperados de vacantes pasadas equivalentes.
*   `AJUSTADO_RRHH`: Modificaciones explícitas introducidas por el analista de RRHH.

---

## 3. Reglas de Edición y Bloqueo

### Bloqueo de Campos de la Solicitud (`SOLICITUD_VACANTE`)
> [!IMPORTANT]
> Los campos con origen `SOLICITUD_VACANTE` están **completamente bloqueados para edición** en todo momento del ciclo de vida del perfil, tanto para RRHH como para el Área Solicitante.
*   **Representación Visual**: Deben renderizar un icono de candado (`lock`) y la aclaración "Solicitud aprobada".
*   **Razón de Negocio**: Garantizar que el profesiograma no distorsione el presupuesto, jerarquía y necesidades aprobadas originalmente en la solicitud.

### Edición para Recursos Humanos (RRHH)
RRHH puede modificar los campos enriquecidos (procedentes de `AGENTE_IA` o `DATO_HISTORICO`) únicamente en los estados `EnRevisionRRHHPE` y `Observada` presionando el botón "Editar perfil".
*   **Comportamiento del Formulario**:
    *   **Modo Lectura**: Muestra la información limpia en formato de ficha, sin cajas de texto deshabilitadas que saturen la pantalla.
    *   **Modo Edición**: Los campos editables (por ejemplo, competencias, herramientas) se transforman dinámicamente en inputs, textareas o controles de ingreso reactivos.

### Edición para el Área Solicitante
El Área Solicitante visualiza el perfil estructurado **siempre en modo lectura** (100% de los campos y secciones bloqueadas). No tiene capacidades de modificación directa sobre la grilla. Su feedback se canaliza exclusivamente por medio del módulo global de observaciones.
