# Trazabilidad Histórica del Perfil (Bitácora)

**Estado de Implementación**: Implementado en Frontend / Reutilizado de Solicitudes  
**Última Actualización**: 12/07/2026

La trazabilidad del perfil de vacante registra cronológicamente todos los hitos y cambios de estado del profesiograma.

---

## 1. Principios de Trazabilidad
*   **Solo Lectura**: El timeline o bitácora es meramente informativo y de solo lectura para todos los roles autorizados. No permite adición manual de eventos ni modificaciones.
*   **Inmutabilidad**: Toda transición de estado gatillada en el sistema debe persistir un registro inalterable en la base de datos de auditoría.
*   **Reutilización**: Se hereda la misma estructura visual (Timeline vertical) y de componentes utilizada exitosamente en el módulo de solicitudes para mantener la coherencia UX.

---

## 2. Modelo del Evento de Trazabilidad
Cada registro en la bitácora de trazabilidad recopila los siguientes campos obligatorios provistos por el backend:

*   **Estado Origen**: Estado en el que se encontraba la entidad antes de la transición (ej. `EnRevisionRRHHPE` o `null` para la creación).
*   **Estado Destino**: Estado al cual transitó la entidad tras la acción (ej. `EnRevisionAreaSol`).
*   **Fecha y Hora**: Estampa de tiempo exacta del servidor.
*   **Actor**: Nombre completo del usuario que inició la acción.
*   **Rol**: Rol organizativo del usuario (ej. `RRHH`, `AreaSol`, `Administrador`).
*   **Tipo de Actor**: Define si fue una acción humana o automática (ej. `HUMANO`, `SISTEMA`, `AGENTE_IA`).
*   **Iteración**: Número secuencial del ciclo de correcciones (ej. Iteración 1, Iteración 2).
*   **Comentario**: Texto aclaratorio o justificación introducida por el usuario al realizar el cambio.

---

## 3. Regla Crítica del Frontend
> [!IMPORTANT]
> El frontend **no debe inferir ni construir** eventos de trazabilidad de manera sintética basándose únicamente en el estado actual del perfil.
*   **Causa**: La bitácora debe ser fiel reflejo de la base de datos.
*   **Comportamiento**: El componente realiza una llamada limpia `GET /api/perfiles/{idPerfil}/trazabilidad` y renderiza la lista ordenada de eventos devuelta por el servidor sin alteración.
