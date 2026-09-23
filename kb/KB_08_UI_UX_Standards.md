# KB_08_UI_UX_Standards

## Descripción

Esta Knowledge Base define los estándares de experiencia de usuario (UX), interfaz de usuario (UI), accesibilidad, navegación y diseño visual del Sistema Inteligente de Reclutamiento para Nacional Seguros.

Todas las pantallas, componentes, formularios, dashboards y módulos deben seguir estas directrices.

---

# Objetivos UX

La interfaz debe priorizar:

* Simplicidad
* Productividad
* Claridad
* Velocidad operativa
* Trazabilidad
* Accesibilidad
* Consistencia

---

# Principios de Diseño

Aplicar:

* Menos clics
* Menos navegación innecesaria
* Información contextual
* Diseño orientado a tareas
* Diseño por roles

Evitar:

* Pantallas sobrecargadas
* Formularios excesivamente largos
* Información duplicada
* Navegación confusa

---

# Diseño Responsivo

Obligatorio soportar:

* Desktop
* Laptop
* Tablet

Diseño Mobile:

Opcional para Fase 1.

---

# Roles

La experiencia debe adaptarse según:

* RRHH
* Reclutador
* Solicitante
* Decisor
* Administrador
* Auditor

Cada rol visualizará únicamente las opciones permitidas.

---

# Navegación Principal

Menú lateral obligatorio.

Módulos:

* Inicio
* Solicitudes
* Vacantes
* Postulantes
* Agenda
* Publicaciones
* Analítica
* Parametrización
* Auditoría
* Administración

---

# Dashboard Inicio

Mostrar:

* Solicitudes pendientes
* Vacantes activas
* Entrevistas del día
* SLA próximos a vencer
* Actividades recientes
* Alertas importantes

---

# Formularios

Aplicar:

* Validación en tiempo real
* Mensajes claros
* Agrupación lógica

Mostrar:

* Campo obligatorio
* Campo opcional
* Error de validación

---

# Validaciones

Mostrar mensajes amigables.

Ejemplo:

Correcto:

"Debe ingresar el cargo solicitado."

Incorrecto:

"Campo inválido."

---

# Botones

Utilizar acciones consistentes.

Ejemplos:

Guardar

Guardar y Aprobar

Cancelar

Eliminar

Exportar

Enviar

---

# Tablas

Todas las tablas deben soportar:

* Búsqueda
* Filtros
* Ordenamiento
* Exportación
* Paginación

---

# Kanban

Implementar para:

Solicitudes

Vacantes

Postulantes

---

# Estados Visuales

Mostrar colores consistentes.

Ejemplo:

Pendiente

En Proceso

Aprobado

Rechazado

Completado

Los colores deben ser configurables.

---

# Timeline

Implementar timeline para:

* Solicitudes
* Vacantes
* Postulantes

Mostrar:

* Fecha
* Usuario
* Acción
* Estado

---

# Ficha de Vacante

Debe contener:

* Información General
* Perfil
* Publicaciones
* Postulantes
* Entrevistas
* Bitácora

Utilizar pestañas.

---

# Expediente del Postulante

Debe contener:

* Datos personales
* CV
* Documentos
* Matching
* Scoring
* Entrevistas
* Historial

---

# Agenda

Vistas:

* Día
* Semana
* Mes

Mostrar:

* Entrevistas
* Eventos
* Recordatorios

---

# Analítica

Utilizar:

* Cards KPI
* Gráficos
* Tendencias
* Comparativas

KPIs mínimos:

* Tiempo de cobertura
* Vacantes por área
* SLA
* Éxito a 3 meses
* Efectividad por canal

---

# Parametrización

Organizar por categorías:

* Roles
* Permisos
* Estados
* Canales
* SLA
* Prompts
* Plantillas

---

# Auditoría

Permitir filtros por:

* Usuario
* Fecha
* Módulo
* Entidad
* Acción

---

# Accesibilidad

Cumplir:

* Navegación por teclado
* Contraste adecuado
* Etiquetas accesibles
* Lectores de pantalla cuando aplique

---

# Componentes Reutilizables

Diseñar componentes estándar para:

* Tablas
* Formularios
* Kanban
* Timeline
* Dashboard Cards
* Confirmaciones:
  * **Diálogo de Confirmación (`app-dialogo-confirmacion`)**: Ubicado en `src/app/core/components/dialogo-confirmacion/`. Componente genérico Standalone para aprobaciones, eliminaciones o avisos con control de estados. Soporta títulos, descripciones, íconos decorativos según variante, tarjetas de resumen compacto, checkboxes obligatorios de confirmación y estados de carga (`isLoading` con animación spinner).
* Adjuntos
* Modales
* Alertas

---

# UX para IA

Los agentes IA deben mostrarse como asistentes.

Ejemplos:

"Generar Perfil"

"Analizar Matching"

"Calcular Score"

"Sugerir Estrategia"

Nunca mostrar IA como autoridad de decisión.

---

# Mensajes del Sistema

Todos los mensajes deben ser:

* Claros
* Cortos
* Accionables

Evitar mensajes técnicos.

---

# Carga y Espera

Mostrar:

* Loading
* Progress
* Confirmaciones

Para procesos IA mostrar:

* Estado de ejecución
* Tiempo estimado
* Resultado

---

# Diseño Visual

Estilo:

* Corporativo
* Profesional
* Limpio
* Moderno

Priorizar funcionalidad sobre efectos visuales.

### Prevención de Traducción en Íconos (Google Translate)
*   **Problema**: Los navegadores (como Chrome) traducen las ligaduras de texto interno de los íconos de Material Symbols / Material Icons (ej. `verified` -> `VERIFICADO`, `settings` -> `CONFIGURACIÓN`). Esto corrompe visualmente la interfaz mostrando texto gigante en lugar del glifo.
*   **Solución Global (Aplicada)**:
    Dado que la aplicación Nacional Seguros está diseñada exclusivamente para usarse en español, se configuró una directiva global en el archivo de entrada raíz `src/index.html` para inhibir por completo cualquier traducción automática del navegador:
    1.  Establecer la etiqueta raíz: `<html lang="es" translate="no">`.
    2.  Establecer el metadato en la cabecera `<head>`: `<meta name="google" content="notranslate">`.
*   **Impacto y Regla de Desarrollo**:
    *   Esta configuración protege de forma nativa y global a la totalidad de los componentes de la aplicación.
    *   No es obligatorio agregar manualmente la clase `notranslate` ni el atributo `translate="no"` a cada etiqueta de ícono individual en el futuro.
    *   Las protecciones locales preexistentes (`notranslate`, `translate="no"`) pueden mantenerse seguras en la base de código.
    *   Cualquier ícono nuevo o componente futuro se beneficia automáticamente de esta protección global.

### Directrices Visuales y Responsive en Formularios de Solicitudes
*   **Distribución en Cuadrícula Responsive**:
    *   Los campos obligatorios del perfil se organizan en **dos columnas en escritorio** y vuelven a **una columna en móvil o tablets angostas (900px)** para reducir la longitud vertical.
    *   Los campos opcionales del perfil se agrupan en pares lado a lado en escritorio, y se apilan verticalmente en resoluciones móviles.
    *   **Prevención de Desbordamiento (Grid Seguro)**: Toda grilla de dos columnas debe declarar `grid-template-columns: repeat(2, minmax(0, 1fr))` para evitar que los placeholders o textos de ayuda forcen la expansión horizontal de la columna.
    *   **Control del Ancho (Box-Sizing)**: Todos los inputs, selects y textareas deben declarar de forma local `box-sizing: border-box` y `max-width: 100%` con `width: 100%` para asegurar que el padding y borde no sumen ancho extra al control, previniendo desbordamientos del contenedor.
*   **Optimización de Textareas**:
    *   Para optimizar la densidad de información y evitar scrolls innecesarios, se definen alturas máximas/mínimas controladas según el propósito del textarea:
        *   Propósito General y Funciones: `110px`.
        *   Requisitos obligatorios técnicos/experiencia: `95px`.
        *   Campos opcionales/recomendados: `85px`.
        *   Disponibilidad y condiciones específicas: `90px`.
    *   Todos los textareas deben permitir redimensionamiento vertical exclusivamente (`resize: vertical`).
*   **Contenedores Plegables (Details/Summary)**:
    *   Para mantener la limpieza visual, los bloques de información no obligatorios (como "Información recomendada") deben encapsularse dentro de un bloque desplegable nativo `<details>` y `<summary>`. Esto asegura que los controles permanezcan en el DOM para la validación reactiva, mantenga los valores intactos al colapsarse, y no requiera lógica TypeScript de soporte.
*   **Barra de Acciones Adhesiva (Sticky Footer)**:
    *   Los botones de acción principales (`Cancelar` y `Registrar solicitud`) deben ubicarse en una barra persistente con `position: sticky; bottom: 0;`.
    *   Debe contar con un fondo de color sólido coincidente con el fondo de página (`#f4f6f8`) y un borde superior sutil (`#d9e2ec`).
    *   Debe acompañarse con un `padding-bottom: 60px` en el contenedor del formulario para evitar que oculte controles al hacer scroll completo.

### Unificación Visual del Backoffice de Reclutamiento
El Backoffice de Reclutamiento unifica la experiencia visual en el entorno de Nacional Seguros usando como referencia el módulo de **Gestión de perfiles de vacante**. Las pautas obligatorias son:

1. **Tarjetas de Métricas (Dashboard / Módulos)**:
   - Fondo blanco con borde sutil (`1px solid var(--border-subtle, #e2e8f0)`), bordes redondeados (`8px`) y sombra sutil (`0 1px 2px rgba(15, 23, 42, 0.03)`).
   - Animación hover discreta (`transform: translateY(-2px);` con transición de 0.2s).
   - Borde izquierdo coloreado de 4px según categoría:
     - Revisión / Pendiente de Acción: Azul (`#3b82f6`)
     - En Proceso / Enriqueciendo: Naranja/Ámbar (`#f59e0b` o `#f28c28`)
     - Aprobado / Completado: Verde (`#10b981`)
     - Neutral / General: Sin borde de color o gris.
   - Ícono con fondo tenue (8% de opacidad del color de la categoría) y bordes redondeados de 8px.
   - Distribución interna horizontal (flex) alineada al centro.

2. **Chips de Estado de Tablas**:
   - En lugar de puntos de color (`.status-dot`), usar badges o chips estructurados que contengan un glifo (`material-symbols-outlined`) y la etiqueta.
   - Altura mínima de 26px, bordes redondeados completos (`9999px`), fuente semibold (600) y opacidades del 10% en fondo con 15% en borde del color del estado (ej. `badge-review`, `badge-observed`, `badge-approved`, `badge-error`).

3. **Botones de Acción de Fila**:
   - Evitar botones sólidos para acciones secundarias en tablas.
   - Usar botones secundarios con borde fino de 1px del color respectivo de la acción (azul para ver/revisar, naranja para corregir, verde para aprobar, rojo para advertir/rechazar).
   - Fondo transparente que se vuelve color sólido en hover.

4. **Filtros de Búsqueda**:
   - Altura estándar de inputs y selectores a 40px con bordes de 1px sutiles y radio de 6px.
   - El botón "Limpiar filtros" debe usar un borde punteado sutil (`1px dashed #cbd5e1`) y fondo transparente.

5. **Paginación y Pie de Tabla**:
   - Alineación justificada (espacio entre el resumen y las acciones).
   - Selectores de tamaño de página y botones de navegación uniformes con bordes de 1px sutiles y radio de 6px.

---

# Validación Final

Antes de aprobar cualquier pantalla verificar:

* Cumple PRD.
* Cumple UX empresarial.
* Cumple accesibilidad.
* Cumple seguridad.
* Cumple trazabilidad.
* Cumple auditoría.
* Cumple prevención de traducción de íconos.

Toda pantalla debe ayudar al usuario a completar tareas con el menor número de pasos posible.
