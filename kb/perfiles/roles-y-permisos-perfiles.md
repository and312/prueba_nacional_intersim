# Roles y Permisos en el Módulo de Perfiles

**Estado de Implementación**: Centralizado en Frontend y Mocks  
**Última Actualización**: 12/07/2026

---

## 1. Catálogo de Roles Activos
El acceso a la información y las acciones operativas en la cabecera del profesiograma están estrictamente controlados según tres perfiles de usuario:

### 1. Recursos Humanos (RRHH)
*   **Responsabilidad**: Diseñar, ajustar y validar la estructura técnica del cargo.
*   **Acciones Permitidas**:
    *   Consultar la lista general de perfiles (grilla con columna "Área Solicitante").
    *   Editar campos enriquecidos (IA/Históricos) si el estado es `EnRevisionRRHHPE` u `Observada`.
    *   Resolver las observaciones recibidas desde el Área Solicitante (Checklist lateral).
    *   Iniciar transiciones de estado: "Enviar al Área Solicitante", "Enviar correcciones", "Aprobar perfil final" (esta última solo si el perfil está en estado `Aprobada`).
    *   Descargar y visualizar el PDF oficial de la estructura del cargo.
    *   Ver el panel histórico de trazabilidad.

### 2. Área Solicitante
*   **Responsabilidad**: Proveer el feedback del negocio y validar que el perfil responda a sus requisitos prácticos.
*   **Acciones Permitidas**:
    *   Consultar la lista general de perfiles (grilla optimizada, sin columna "Área").
    *   Visualizar el perfil estructurado y el resumen ejecutivo en modo lectura (siempre bloqueados).
    *   Registrar observaciones globales con tipos parametrizados activos.
    *   Enviar observaciones registradas a RRHH (transición al estado `Observada`), habilitado si existe al menos una observación agregada.
    *   Aprobar el perfil (transición al estado `Aprobada`).
    *   Descargar y visualizar el PDF del cargo.
    *   Ver el panel histórico de trazabilidad.
*   **Acciones Prohibidas**:
    *   No puede editar los valores de ningún campo del profesiograma de forma directa.
    *   No puede resolver observaciones.
    *   No puede dar aprobación final (`PerfilAprobadoFinal`).

### 3. Administrador
*   **Responsabilidad**: Soporte operativo y parametrización de catálogos.
*   **Acciones Permitidas**:
    *   Todos los permisos de RRHH.
    *   Capacidades para gestionar catálogos (Tipos de Observación) y ver auditoría técnica avanzada.

---

## 2. Matriz de Permisos de Edición y Control
A continuación se detalla la disponibilidad de las opciones de edición e inicio de flujos por rol y estado actual:

| Rol | Estado Perfil | ¿Puede Editar Campos? | ¿Puede Registrar Obs.? | Acciones de Flujo Disponibles |
| :--- | :--- | :---: | :---: | :--- |
| **RRHH** | EnRevisionRRHHPE | **SÍ** | NO | Enviar al Área Solicitante |
| **RRHH** | Observada | **SÍ** | NO | Enviar correcciones |
| **RRHH** | Aprobada | NO | NO | Aprobar perfil final |
| **RRHH** | PerfilAprobadoFinal | NO | NO | Ninguna (Bloqueado) |
| **AreaSol**| EnRevisionAreaSol | NO | **SÍ** | Aprobar perfil / Enviar observaciones |
| **AreaSol**| Observada / Aprobada | NO | NO | Ninguna (Espera de RRHH) |
