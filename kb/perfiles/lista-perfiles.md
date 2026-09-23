# Vista de Lista de Perfiles (Dashboard)

**Estado de Implementación**: Implementado en Frontend  
**Última Actualización**: 12/07/2026

La pantalla principal del módulo de perfiles se adapta visualmente según el rol del usuario conectado para priorizar su carga de trabajo.

---

## 1. Interfaz de Recursos Humanos (RRHH)

### Tarjetas de Resumen (Métricas)
*   **Total en Proceso**: Cantidad total de perfiles de vacante activos en el sistema.
*   **En revisión RRHH**: Perfiles en estado `EnRevisionRRHHPE` o `Corregida` que esperan acción metodológica.
*   **Observados por corregir**: Cantidad de perfiles devueltos por el Área Solicitante en estado `Observada`.
*   **Pendientes de aprobación final**: Perfiles que ya fueron aprobados por el cliente interno y esperan el control final de RRHH (`Aprobada`).

### Filtros de Búsqueda Recomendados
*   **Estado del perfil**: Selección múltiple o individual de los estados del ciclo de vida.
*   **Área solicitante**: Dropdown de catálogo de las áreas de la corporación.
*   **Acción requerida**: Filtro rápido para aislar registros que requieren intervención directa del usuario.
*   **Fecha de última actualización**: Rango de fechas para control de SLA.

### Grilla de Datos (Columnas)
1.  **Código**: Identificador único formateado del profesiograma (ej. `PERF-001`).
2.  **Solicitud origen**: Código de la solicitud de vacante asociada (ej. `SOL-101`).
3.  **Cargo**: Nombre del puesto de trabajo.
4.  **Área solicitante**: Departamento de la empresa del cual procede la vacante (ej. *Comercial*, *Tecnología*).
5.  **Estado**: Badge visual que representa el estado del perfil con colores consistentes.
6.  **Última actualización**: Fecha y hora del último movimiento o guardado.
7.  **Acciones**: Enlace directo a la vista de detalle.

---

## 2. Interfaz del Área Solicitante (Cliente Interno)

### Tarjetas de Resumen (Métricas)
*   **Pendientes de revisión**: Perfiles en estado `EnRevisionAreaSol` que requieren validación urgente del área.
*   **Observados por el área**: Perfiles enviados de vuelta a RRHH que están siendo corregidos (`Observada`).
*   **Aprobados por el área**: Perfiles cuya aprobación ya fue dada por el área y esperan la firma final de RRHH.
*   **Perfiles finalizados**: Historial de profesiogramas cerrados en estado `PerfilAprobadoFinal`.

### Filtros de Búsqueda
*   **Estado del perfil**: Estados visibles aplicables al área.
*   **Acción requerida**: Registros pendientes de validación/aprobación.
*   **Fecha de última actualización**: Rango temporal de cambios.

### Grilla de Datos (Columnas)
1.  **Código**: Identificador único del profesiograma.
2.  **Solicitud origen**: Código de la solicitud de vacante asociada.
3.  **Cargo**: Nombre del puesto.
4.  **Estado**: Estado actual del perfil.
5.  **Última actualización**: Fecha y hora.
6.  **Acciones**: Enlace directo a la vista de detalle.

> [!IMPORTANT]
> Para el Área Solicitante se **omite por completo** la columna "Área", ya que la lista se encuentra filtrada implícitamente por el departamento al cual pertenece el usuario logueado.
