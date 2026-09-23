# Pendientes y Decisiones Abiertas del Módulo de Perfiles

**Estado de Implementación**: Por Validar con Product Owner  
**Última Actualización**: 12/07/2026

A continuación se listan las decisiones técnicas, funcionales y de negocio abiertas que requieren aclaración o validación por parte del Product Owner o del equipo de backend para la puesta en producción.

---

## 1. Decisiones Funcionales y Reglas de Negocio

### 1. Estados Definitivos de las Observaciones
*   *Pregunta*: Para la primera versión se implementaron los estados `PENDIENTE` y `RESUELTA`. ¿Es necesario incorporar formalmente el estado `REABIERTA` si RRHH realiza una mala corrección y el Área Solicitante rechaza la corrección en una segunda iteración?
*   *Impacto*: Afecta al flujo del checklist de RRHH y a los badges visuales.

### 2. Flujo de Reapertura de Observaciones
*   *Pregunta*: Si el perfil es observado por segunda vez, ¿las observaciones previamente resueltas deben mantenerse bloqueadas en modo lectura o pueden reabrirse de forma selectiva?
*   *Impacto*: Requiere endpoint de backend para transitar el estado de la observación individual de vuelta a `PENDIENTE`.

### 3. Permisos Específicos del Administrador
*   *Pregunta*: ¿El rol de Administrador contará con una pantalla especial para parametrizar el catálogo de `TiposObservacion` (añadir nuevos tipos o desactivarlos), o se manejará inicialmente directo por base de datos?
*   *Impacto*: Definición de nuevas vistas en el módulo de configuración.

### 4. Estructura y Generación del PDF
*   *Pregunta*: ¿El PDF descargable unificará en un solo documento el "Resumen ejecutivo" y el "Perfil estructurado", o se mantendrán como descargas independientes?
*   *Impacto*: Formateador de PDF en backend y botonería superior.

### 5. Momento de Generación del Resumen
*   *Pregunta*: ¿El Resumen Ejecutivo se debe generar inmediatamente al pasar a `EnRevisionRRHHPE` por primera vez, o solo después de que RRHH guarde sus primeros ajustes metodológicos?
*   *Impacto*: Disparadores en n8n/backend.

---

## 2. Decisiones Técnicas e Integración de Backend

### 1. Versionado del Resumen Ejecutivo
*   *Pregunta*: Si RRHH edita campos del estructurado, ¿el resumen ejecutivo viejo se borra inmediatamente de base de datos pasando a `PENDIENTE_GENERACION`, o se conserva la versión anterior del texto para consulta del usuario mientras se procesa la nueva versión en segundo plano?
*   *Impacto*: Experiencia de usuario (spinner bloqueante vs visualización diferida).

### 2. Contrato de Acciones Permitidas
*   *Pregunta*: ¿El backend proveerá una lista dinámica de acciones disponibles en la respuesta de `GET /api/perfiles/{id}` (ej. `acciones: ["aprobar", "observar"]`), o el frontend calculará las acciones basándose únicamente en el estado plano?
*   *Recomendación*: Que el backend envíe las acciones permitidas para centralizar las reglas de negocio y permisos en el servidor.

### 3. Campos Definitivos Editables por RRHH
*   *Pregunta*: ¿La editabilidad de los campos del profesiograma (origen `AGENTE_IA`) es configurable por base de datos de manera dinámica por tipo de vacante, o se mantendrá cableada según el origen estático?
*   *Impacto*: Lógica del cargador dinámico del formulario reactivo en el frontend.
