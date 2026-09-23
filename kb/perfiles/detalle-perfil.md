# Pantalla de Detalle del Perfil

**Estado de Implementación**: Implementado en Frontend / Navegación Fluida  
**Última Actualización**: 12/07/2026

La pantalla de detalle del perfil de vacante centraliza toda la información, la edición interactiva de campos y las acciones de transición de estado.

---

## 1. Estructura de la Pantalla

La interfaz está dividida en las siguientes zonas funcionales:

### 1. Encabezado de Datos (Header)
Muestra la información de control y metadatos generales del perfil:
*   **Código del perfil**: Identificador (ej. `PERF-005`).
*   **Solicitud origen**: Código cliqueable hacia la solicitud de vacante.
*   **Cargo / Puesto**: Título del perfil.
*   **Área Solicitante**: Departamento asociado.
*   **Versión**: Número incremental de versión del profesiograma.
*   **Estado**: Badge de estado actual.
*   **Última actualización**: Fecha del último cambio registrado.

### 2. Acciones Superiores (Botones de Control)
Alineados a la derecha del encabezado en una sola fila en pantallas de escritorio, con distribución flexible para evitar saltos desproporcionados en móviles o tablets. Se calculan dinámicamente según el rol del usuario conectado, el estado del perfil y la pestaña seleccionada.
*   **No hacer**: No se debe mostrar el botón redundante "Ir al perfil estructurado", ya que la navegación natural mediante pestañas es directa.

### 3. Pestañas de Navegación (Tabs)
*   **Pestaña 1: Resumen ejecutivo**: Pestaña activa por defecto. Presenta una síntesis en lenguaje natural e insumos del perfil.
*   **Pestaña 2: Perfil estructurado**: Muestra los acordeones con los campos detallados del profesiograma para lectura o edición.
*   **Regla Crítica**: Ambos tabs permanecen habilitados y navegables en todo momento una vez generado el resumen ejecutivo. El usuario puede cambiar libremente entre ellos sin perder cambios locales ni bloquear la UI.

### 4. Panel Lateral de Apoyo (Sidebar - 25% de ancho)
Contiene widgets dinámicos según el tab y rol del usuario:
*   **Widget Insumos utilizados**: Exclusivo de la pestaña de Resumen ejecutivo. Muestra la lista de documentos y fuentes con las que se estructuró el perfil.
*   **Widget Estado de Proceso**: Muestra el progreso del profesiograma.
*   **Widget Observaciones Recibidas (Checklist)**: Exclusivo del Perfil estructurado para RRHH/Admin cuando el perfil se encuentra en estado `Observada` o `EnRevisionRRHHPE`. Permite auditar y resolver las correcciones pendientes.
*   **Widget Trazabilidad (Bitácora)**: Disponible en ambos tabs y visible para todos los roles de forma cronológica.

---

## 2. Acciones del Flujo por Rol y Pestaña

### En la pestaña "Resumen ejecutivo"
*   **RRHH / Administrador**:
    *   `verPdf`: Ver PDF.
    *   `aprobarFinal`: Aprobar perfil final (habilitado si el estado es `Aprobada`).
*   **Área Solicitante**:
    *   `verPdf`: Ver PDF.
    *   `aprobarArea`: Aprobar perfil.
    *   `enviarObservaciones`: Enviar observaciones (habilitado si hay al menos una observación agregada).

### En la pestaña "Perfil estructurado"
*   **RRHH / Administrador** (Sin modo edición activo):
    *   `iniciarEdicion`: Editar perfil (habilita controles de formulario reactivo en campos editables).
    *   `enviarArea`: Enviar al Área Solicitante (si está en `EnRevisionRRHHPE`).
    *   `enviarCorrecciones`: Enviar correcciones (si está en `Observada`).
    *   `aprobarFinal`: Aprobar perfil final (si está en `Aprobada`).
    *   `verPdf`: Ver PDF.
*   **RRHH / Administrador** (Con modo edición activo):
    *   `guardarAjustes`: Guardar ajustes (persiste cambios locales).
    *   `cancelarEdicion`: Cancelar (revierte cambios locales y apaga controles de edición).
*   **Área Solicitante**:
    *   `aprobarArea`: Aprobar perfil.
    *   `enviarObservaciones`: Enviar observaciones (habilitado si hay observaciones registradas).
    *   `verPdf`: Ver PDF.
