# Módulo de Parametrización de Catálogos

**Última Actualización**: 12/07/2026  
**Estado**: Módulo completamente corregido e integrado con datos mock  
**Pendiente**: Integración definitiva con backend y formularios de Solicitud  

---

## 1. Propósito del Módulo
El **Módulo de Parametrización** permite administrar los catálogos operativos utilizados de forma transversal en el flujo de solicitudes de vacante, estructuración de perfiles y procesos de reclutamiento. Esto elimina la necesidad de cablear (*hardcode*) valores fijos en el código de las vistas, permitiendo que la organización se adapte dinámicamente a cambios en sedes, modalidades y clasificaciones de negocio.

---

## 2. Seguridad y Acceso
*   **Rol Autorizado**: Únicamente accesible por usuarios con el rol `'Administrador'`.
*   **Protección de Ruta**: Gobernado en `app.routes.ts` mediante `authGuard` y restricción de datos `data: { roles: ['Administrador'] }`.
*   **Ruta Oficial**: `/configuracion/parametrizacion`
*   **Entrada en Sidebar**: Ubicada en la sección administrativa (visible solo para Administrador).

---

## 3. Catálogos Iniciales Administrados
El módulo gestiona inicialmente los siguientes cuatro catálogos parametrizables:

1.  **Observaciones**: Representa los tipos de observación disponibles en el catálogo para registrar observaciones dentro del profesiograma del perfil (por ejemplo: "Información incompleta", "Revisar criterio").
    *   *Nota Crítica*: Representa la clasificación parametrizable, **no** el listado transaccional de observaciones individuales guardadas en un perfil.
2.  **Regionales**: Sedes corporativas o geográficas de cobertura.
3.  **Tipos de solicitud**: Tipos de requisición de personal.
4.  **Modalidades**: Modalidades de jornada de trabajo.

---

## 4. Modelos de Datos Utilizados
Se definieron las siguientes interfaces y tipos TypeScript:

```typescript
export interface Observacion {
  idObservacion: number;
  nombre: string;
  activo: boolean;
}

export interface Regional {
  idRegional: number;
  nombre: string;
  activo: boolean;
}

export interface TipoSolicitud {
  idTipoSolicitud: number;
  nombre: string;
  activo: boolean;
}

export interface Modalidad {
  idModalidad: number;
  nombre: string;
  activo: boolean;
}

export type ClaveCatalogoParametrizacion =
  | 'OBSERVACIONES'
  | 'REGIONALES'
  | 'TIPOS_SOLICITUD'
  | 'MODALIDADES';

export interface ConfiguracionCatalogoParametrizacion {
  clave: ClaveCatalogoParametrizacion;
  titulo: string;
  descripcion: string;
  icono: string;
}
```

---

## 5. Estructura Modular en Frontend
Se implementó siguiendo el lineamiento de feature encapsulado:

```text
src/app/features/parametrizacion/
  ├── paginas/
  │    └── inicio-parametrizacion/             <-- Contenedor principal de tarjetas
  ├── componentes/
  │    ├── tarjeta-parametrizacion/            <-- Tarjeta reutilizable por catálogo
  │    ├── formulario-catalogo/                <-- Formulario reactivo para crear/editar
  │    └── modal-mantenimiento-catalogo/       <-- Modal de mantenimiento con grilla
  ├── modelos/                                 <-- Tipado estricto
  ├── servicios/                               <-- Servicios aislados con BehaviorSubject
  └── constantes/
       └── catalogos-parametrizacion.ts        <-- Configuración estática de tarjetas
```

---

## 6. Servicios y Ubicación de Mocks
Los servicios se modularizaron por separado para evitar inflar el servicio monolítico `ApiService`. Cuentan con retrasos simulados (`delay`) para emular latencia de red:
*   [observaciones.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/parametrizacion/servicios/observaciones.service.ts)
*   [regionales.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/parametrizacion/servicios/regionales.service.ts)
*   [tipos-solicitud.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/parametrizacion/servicios/tipos-solicitud.service.ts)
*   [modalidades.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/parametrizacion/servicios/modalidades.service.ts)

---

## 7. Reglas de Negocio Implementadas

### Diferencias entre Inactivar y Eliminar
*   **Inactivar (Baja Lógica)**: 
    *   *Comportamiento*: Conserva el registro en el sistema de manera que las referencias históricas (solicitudes y perfiles existentes) sigan renderizando el valor correcto. Sin embargo, evita que sea seleccionado en nuevos procesos o solicitudes.
    *   *Facilidad*: Se puede realizar en cualquier momento.
*   **Eliminar (Eliminación Física)**:
    *   *Comportamiento*: Remueve el registro permanentemente del catálogo y de la base de datos.
    *   *Restricción*: Solo se permite si el registro no tiene ninguna relación transaccional activa (no está en uso). No se puede deshacer.

### Reglas de Eliminación Controlada
*   **Validación de Relaciones**: Un registro con `puedeEliminar === false` o `cantidadUsos > 0` no puede ser eliminado físicamente. El sistema bloquea el flujo destructivo y despliega un diálogo explicativo: *"No es posible eliminar este registro porque ya está siendo utilizado en procesos existentes. Puede inactivarlo para evitar su uso en nuevos registros."*
*   **Confirmación Destructiva**: Si el registro no está relacionado en ningún proceso, al hacer clic en el botón de eliminar, el sistema solicita confirmación mediante un modal de confirmación en dos fases: *"¿Está seguro de eliminar ‘{nombre}’? Esta acción no se puede deshacer."*
*   **Reactivación**: Un registro inactivo que no esté en uso puede seguir siendo activado o eliminado indistintamente.

### Validaciones de Creación y Edición
*   **Nombre Obligatorio**: Longitud mínima de 3 y máxima de 100 caracteres.
*   **Limpieza de Espacios**: Trimeo automático antes de persistir o validar duplicados.
*   **Prevención de Duplicados**: Validación local previa a nivel de frontend ignorando mayúsculas y minúsculas (ej. "LA PAZ" y "La Paz" se consideran duplicados).

---

## 8. Diseño y Responsive
*   **Desktop**: Grid de 4 columnas de tarjetas. Modal centrado de ancho máximo 680px y scroll vertical interno exclusivo para evitar scroll global de fondo.
*   **Tablet**: Grid de 2 columnas de tarjetas.
*   **Móvil**: Tarjetas apiladas en 1 columna. Modal ocupa el total de ancho visible y la tabla se adapta de forma fluida.

---

## 9. Contratos de Backend Pendientes
El backend deberá implementar un controlador REST genérico o independiente para los catálogos exponiendo las siguientes firmas:
*   `GET /api/parametros/{catalogo}`: Listado de registros.
*   `POST /api/parametros/{catalogo}`: Creación de registros.
*   `PUT /api/parametros/{catalogo}/{id}`: Edición de nombre.
*   `PATCH /api/parametros/{catalogo}/{id}/estado`: Alta/baja lógica.
*   `DELETE /api/parametros/{catalogo}/{id}`: Eliminación física.

### Comportamiento y Códigos HTTP de Eliminación
*   **Éxito (204 No Content)**: Si el registro no posee relaciones transactivas, se elimina permanentemente.
*   **Conflicto (409 Conflict)**: Si el registro está siendo utilizado en algún proceso, el backend debe retornar:
    ```json
    {
      "codigo": "CATALOGO_EN_USO",
      "mensaje": "No se puede eliminar el registro porque tiene relaciones activas.",
      "cantidadUsos": 4
    }
    ```

---

## 10. Próxima Etapa
1.  **Integración de API**: Conectar los 4 servicios de parametrización reemplazando la inyección `BehaviorSubject` por llamadas `http.get` y `http.post`.
2.  **Integración en Solicitudes**: Modificar la pantalla de `NuevaSolicitudComponent` para reemplazar las opciones fijas en HTML por combos dinámicos consultados al backend.

---

## 11. Archivos Modificados y Creados
- **Rutas y Sidebar**:
  - `src/app/app.routes.ts`: Agregada la ruta `/configuracion/parametrizacion`.
  - `src/app/layouts/layout/layout.component.html`: Agregado el botón en el sidebar para el Administrador.
- **Modelos**:
  - `src/app/features/parametrizacion/modelos/observacion.model.ts`
  - `src/app/features/parametrizacion/modelos/regional.model.ts`
  - `src/app/features/parametrizacion/modelos/tipo-solicitud.model.ts`
  - `src/app/features/parametrizacion/modelos/modalidad.model.ts`
- **Servicios de Parametrización**:
  - `src/app/features/parametrizacion/servicios/observaciones.service.ts`
  - `src/app/features/parametrizacion/servicios/regionales.service.ts`
  - `src/app/features/parametrizacion/servicios/tipos-solicitud.service.ts`
  - `src/app/features/parametrizacion/servicios/modalidades.service.ts`
- **Componentes Visuales**:
  - `src/app/features/parametrizacion/paginas/inicio-parametrizacion/inicio-parametrizacion.component.html`
  - `src/app/features/parametrizacion/paginas/inicio-parametrizacion/inicio-parametrizacion.component.ts`
  - `src/app/features/parametrizacion/componentes/modal-mantenimiento-catalogo/modal-mantenimiento-catalogo.component.ts`
  - `src/app/features/parametrizacion/componentes/modal-mantenimiento-catalogo/modal-mantenimiento-catalogo.component.html`
  - `src/app/features/parametrizacion/componentes/modal-mantenimiento-catalogo/modal-mantenimiento-catalogo.component.scss`

---

## 12. Diagnóstico de la Corrección de Integración (12/07/2026)
*   **Causa del Problema**: En la primera versión de `cargarTodos()`, se agrupaban las consultas en un `forkJoin` esperando que emitieran y completaran. Sin embargo, los servicios retornaban `datosMock.asObservable()` directo del `BehaviorSubject`. Dado que los `Subject` calientes nunca completan de forma automática, `forkJoin` quedaba colgado indefinidamente. Por ende, la vista de carga no finalizaba nunca y las tarjetas no se renderizaban.
*   **Solución Aplicada**: Se ajustó la firma del método `listar()` de los 4 servicios para que emitan el valor actual envuelto en `of()` y apliquen un retardo controlado (`delay`). Al utilizar `of()`, la secuencia emite y completa inmediatamente, permitiendo que el `forkJoin` unifique los catálogos y active el renderizado de la grilla de tarjetas.
*   **Ajuste del Icono y Título**: Se retiró el icono ornamental decorativo con apariencia no corporativa y se sustituyó por el Material Symbol `settings_suggest` en el color e intensidades de la marca. El encabezado de la página se adaptó a la clase `admin-page` y `admin-header` para que combine con el resto de módulos del panel administrativo de Nacional Seguros.
*   **Verificación**: Se verificó el correcto funcionamiento de las 4 tarjetas (Observaciones, Regionales, Tipos de solicitud y Modalidades) y la apertura de cada modal individual de edición y altas.

