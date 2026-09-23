# Gestión Global de Observaciones del Perfil

**Estado de Implementación**: Implementado en Frontend / Desacoplado de Secciones  
**Última Actualización**: 12/07/2026

---

## 1. El Cambio Funcional: Desacoplamiento de Secciones
> [!IMPORTANT]
> Se ha eliminado por completo la dependencia entre las observaciones y las secciones individuales del profesiograma. Las observaciones ahora son **globales** al perfil completo.

### Modelo Anterior (Obsoleto)
Las observaciones estaban ligadas a un `seccionId` y `seccionNombre`. Esto provocaba que:
*   Los acordeones de secciones se pintaran de rojo y tuvieran badges de alerta.
*   El usuario se viera obligado a asignar cada observación a un acordeón específico.
*   Se mostraran cajas de alerta amarillas dentro del detalle de campos del acordeón.

### Modelo Actual (Global y Modular)
Las observaciones se registran sobre el profesiograma en su conjunto.
*   **Sin marcas en acordeones**: Las secciones y acordeones del perfil estructurado permanecen limpios y libres de alertas visuales de observación.
*   **Independencia**: Las propiedades `seccionId` y `seccionNombre` han sido eliminadas por completo del formulario y del flujo lógico.

---

## 2. Modelo de Base de Datos y Entidades

### Tabla/Catálogo: `TiposObservacion`
Representa el catálogo parametrizable de clasificaciones de observaciones disponibles:
*   `IdTipoObservacion` (int, PK): Identificador secuencial.
*   `Nombre` (nvarchar): Etiqueta descriptiva (ej. "Información incompleta", "Ajustar contenido", "Revisar criterio").
*   `Activo` (bit): Define si está disponible para nuevas observaciones.

### Tabla/Entidad: `ObservacionPerfil`
Representa el registro formal de la observación agregada:
*   `IdObservacionPerfil` (int, PK): Secuencial único.
*   `IdTipoObservacion` (int, FK): Relación con catálogo de tipo.
*   `IdPerfil` (int, FK): Relación con el perfil de vacante.
*   `Comentario` (nvarchar): Detalle de la corrección solicitada.
*   `Estado` (varchar/enum): `PENDIENTE` o `RESUELTA`.
*   `IdUsuarioSolicitante` (int, FK): Usuario del Área Solicitante que registró la observación.
*   `IdUsuarioRRHH` (int, FK, Nullable): Analista de RRHH que resolvió la observación.
*   `FechaRegistro` (datetime): Estampa temporal de creación.
*   `FechaResolucion` (datetime, Nullable): Estampa temporal de resolución.

---

## 3. Comportamiento y Flujos en la Interfaz de Usuario

### Vista del Área Solicitante (`app-observaciones-perfil`)
*   **Formulario**: Se compone únicamente de un selector de "Tipo de observación" (carga dinámicamente tipos donde `activo === true`) y un campo de texto "Comentario aclaratorio" (valida un mínimo de 5 caracteres). Se eliminó el selector de sección afectada.
*   **Lista de agregadas**: Muestra las observaciones asociadas al perfil. Si la observación está en estado `PENDIENTE` y no se ha enviado el lote a RRHH, se permite su eliminación mediante un botón de papelera.
*   **Visualización de resueltas**: Muestra claramente si RRHH ya resolvió la observación mediante un badge verde `RESUELTA`, indicando el nombre del analista y la fecha de resolución, permitiendo la auditoría por el cliente interno.

### Vista de Recursos Humanos (`app-observaciones-recibidas-perfil`)
*   **Checklist lateral independiente**: Ubicado en el panel derecho de la pestaña del perfil estructurado cuando el perfil se encuentra en estado `Observada` o `EnRevisionRRHHPE`.
*   **Acción de resolver**: RRHH puede auditar la lista de observaciones pendientes. Al marcar el checkbox de un ítem, el sistema ejecuta de forma asíncrona la resolución. El estado cambia localmente a `RESUELTA`, asignando el analista autenticado y la fecha del sistema.
*   **No eliminación**: Las observaciones resueltas **no desaparecen** de la lista; mitigan su estilo visual y muestran el badge de resuelto para mantener la trazabilidad de los cambios durante toda la sesión.
