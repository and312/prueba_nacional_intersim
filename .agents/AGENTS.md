# Reglas del Proyecto - Sistema de Reclutamiento Inteligente (SIR)

## Regla Arquitectónica: Gobierno del Diseño Frontend

### Objetivo
Garantizar que todas las interfaces del Sistema Inteligente de Reclutamiento para Nacional Seguros mantengan una experiencia de usuario uniforme, consistente y alineada con la identidad visual corporativa.

---

### Fuente Oficial de Diseño
El proyecto de diseño mantenido en **Stitch**, accesible mediante **Stitch MCP**, constituye la única fuente oficial de diseño visual del sistema.

Todos los agentes, desarrolladores y herramientas de generación de código deberán utilizar exclusivamente dicho proyecto como referencia para la construcción del Frontend.

---

### Regla Obligatoria
Antes de generar cualquier componente, pantalla, layout o módulo del Frontend, el agente deberá:
1. Consultar Stitch MCP.
2. Obtener el diseño correspondiente.
3. Obtener los Design Tokens.
4. Obtener los componentes utilizados.
5. Obtener la estructura del layout.
6. Obtener las reglas responsive.

Solamente después de completar estos pasos podrá generar el código Angular.

---

### Prohibiciones
Está estrictamente prohibido:
* Inventar interfaces.
* Modificar colores.
* Cambiar tipografía.
* Cambiar espaciados.
* Crear variantes visuales.
* Cambiar layouts.
* Rediseñar componentes.
* Crear componentes equivalentes cuando ya existan en Stitch.
* Aplicar estilos distintos a los definidos en Stitch.

Si una pantalla o componente no existe en Stitch, primero deberá diseñarse y aprobarse allí antes de ser implementado.

---

### Tecnología Obligatoria
Toda implementación del Frontend deberá utilizar exclusivamente:
* Angular 22
* TypeScript
* Standalone Components
* Angular Signals
* Angular Control Flow
* SCSS
* Angular Material
* Angular CDK

No se permite utilizar React, Vue, Blazor u otros frameworks para el Frontend del proyecto.

---

### Librería Corporativa
Todos los componentes deberán construirse utilizando la librería corporativa:
**NacionalSeguros.UI**

La librería será la implementación técnica del Design System definido en Stitch. No podrán existir componentes duplicados ni implementaciones paralelas.

---

### Flujo Obligatorio de Desarrollo
Toda generación de interfaces deberá seguir el siguiente flujo:
1. Consultar Stitch MCP.
2. Validar existencia del diseño.
3. Identificar componentes reutilizables.
4. Obtener Design Tokens.
5. Construir utilizando la librería NacionalSeguros.UI.
6. Generar código Angular 22.
7. Validar coincidencia visual con Stitch.

No se permitirá alterar este flujo.

---

### Validación
Antes de aprobar cualquier pantalla deberá verificarse que:
* Coincide visualmente con Stitch.
* Utiliza únicamente componentes oficiales.
* Respeta la identidad corporativa.
* Mantiene consistencia con el resto del sistema.
* No introduce estilos personalizados innecesarios.

---

### Prioridad
En caso de conflicto entre el código generado y el diseño disponible en Stitch, **prevalecerá siempre el diseño definido en Stitch**. El código deberá adaptarse al diseño, nunca el diseño al código.

==============================================================================

## SISTEMA DE DISEÑO ÚNICO PARA TODO EL FRONTEND

### Objetivo
A partir de este momento, **todas las páginas que se generen deberán seguir exactamente el mismo Design System**, reutilizando componentes, estilos, layouts y patrones visuales. No se debe diseñar cada página desde cero; cada nueva pantalla deberá parecer parte del mismo sistema.

---

### Diseño General
Utilizar una apariencia corporativa moderna (Inspiración: Microsoft Dynamics 365, Azure Portal, Atlassian Jira Cloud, GitHub Enterprise, SAP Fiori). Priorizar:
* Simplicidad
* Consistencia
* Productividad
* Legibilidad

---

### Layout Unificado
Todas las páginas deben utilizar exactamente:
* Mismo Header
* Mismo Sidebar
* Mismo Footer
* Mismo Breadcrumb
* Mismo ancho de contenido
* Mismo espaciado
* Misma separación entre tarjetas
* Misma tipografía
* Mismos iconos
* Mismo sistema de colores

Nunca modificar el Layout entre módulos.

---

### Componentes Reutilizables Obligatorios
Todos los módulos deberán reutilizar los mismos componentes del Design System:
* `AppCard`, `AppTable`, `AppForm`, `AppButton`, `AppToolbar`, `AppSearch`, `AppFilter`, `AppPagination`, `AppDialog`, `AppConfirmDialog`, `AppSnackbar`, `AppLoading`, `AppBreadcrumb`, `AppStatusChip`, `AppAvatar`, `AppEmptyState`, `AppPageHeader`, `AppSection`.

No crear variantes ni componentes nuevos si ya existe uno que resuelva el mismo problema.

---

### Tablas
Todas las tablas deberán tener exactamente:
* Toolbar superior con Título, Botón Nuevo, Buscador, Filtros, Exportar y Refresh.
* Tabla Material con Paginador, Cantidad de registros, Acciones, y Estado mediante Chips.
* Diseño 100% responsive.

---

### Formularios
Todos los formularios deberán utilizar:
* Cards y Grid Layout.
* Reactive Forms con validaciones y mensajes de error debajo de cada campo.
* Botones alineados (Cancelar, Guardar, Guardar y continuar).

---

### Iconografía y Tipografía
* Utilizar exclusivamente **Material Icons**.
* Mantener exactamente los mismos tamaños, pesos y estilos para títulos, subtítulos y textos en todo el sistema.
