# Registro Técnico de Incidente — Bloqueo de Interfaz al Enviar Perfil al Área Solicitante

## Problema

Al intentar enviar un perfil al Área Solicitante desde la interfaz de Recursos Humanos (RRHH), si ocurre una falla o demora en la generación de los documentos (resumen ejecutivo o perfil estructurado), el modal de confirmación se bloquea indefinidamente mostrando el estado **"Procesando..."**, con los botones deshabilitados, y no se actualiza la interfaz, forzando al usuario a recargar la página.

---

## Causa Raíz

1.  **Doble Estado de Carga Desacoplado**:
    -   El componente `DetallePerfilComponent` administra su propio estado de carga `isLoading` (mediante un signal de Angular).
    -   El componente común `DialogoConfirmacionComponent` administra internamente una variable primitive local `isLoading = false` (booleano común).
2.  **Invariabilidad en Errores en el Diálogo**:
    -   En `DialogoConfirmacionComponent.ts`, cuando el observable devuelto por `onConfirmar` lanza un error en su flujo, el manejador `error` de la suscripción del diálogo únicamente realiza un `console.error` pero **no cierra el modal ni actualiza la variable del diálogo**.
    -   Aunque el operador `finalize` de RxJS cambia `this.isLoading = false` en el diálogo, al tratarse de una variable primitiva y no un Signal, y ocurrir dentro de una llamada asíncrona bloqueada por un `alert()` síncrono del navegador, la detección de cambios de Angular no se gatilla para redibujar el diálogo.
    -   Esto produce que el botón del diálogo quede eternamente congelado con el spinner y la leyenda "Procesando...".
3.  **Ejecución Secuencial Ineficiente (Frontend)**:
    -   El flujo frontend en `enviarAlArea()` ejecutaba los webhooks de n8n en serie mediante operadores `concatMap`:
        1.  `guardarResumen` (API)
        2.  `generateExecutiveSummaryPdf` (A4 Webhook)
        3.  `generateStructuredProfilePdf` (A5 Webhook)
        4.  `notifySolicitanteProfileUnderReview` (Notificador2 Webhook)
        5.  `enviarPerfilArea` (API - Transición a `PERF-REV-AREA`)
        6.  `obtenerPerfilPorId` (API - Recarga)
    -   Si el webhook A4 de n8n fallaba o superaba el timeout de 15 segundos, se lanzaba un error mediante `throw err`. Al abortar la tubería (`pipe`), la llamada a `enviarPerfilArea` **nunca se ejecutaba**. Como consecuencia, el perfil nunca cambiaba su estado a `PERF-REV-AREA` y la transición en base de datos quedaba inconclusa.

---

## Evidencia

-   **Endpoint Backend Afectado**: `POST /api/v1/perfiles/{id}/enviar-area` (nunca llegaba a invocarse al fallar los pasos anteriores de la serie).
-   **Webhooks de n8n Afectados**:
    -   `A4_Genera_PDF_resumen_ejecutivo_del_rol` (provocaba el timeout o fallo simulado en el screenshot).
    -   `ares-generador-pdf-perfil-estructurado`
-   **Respuesta HTTP**: `504 Gateway Timeout` o fallo de red en los webhooks.
-   **Variables de Loading Involucradas**:
    -   `isLoading` en `DialogoConfirmacionComponent` (no se actualizaba en la UI).
    -   `isLoading` en `DetallePerfilComponent` (se liberaba correctamente, pero el modal bloqueaba la vista).
-   **Ubicación del Código**:
    -   [dialogo-confirmacion.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/core/components/dialogo-confirmacion/dialogo-confirmacion.component.ts#L43-L78)
    -   [detalle-perfil.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/detalle-perfil/detalle-perfil.component.ts#L627-L677)

---

## Solución Aplicada

### 1. Desbloqueo Reactivo de la Interfaz (Frontend)
-   Convertimos la propiedad `isLoading` de `DialogoConfirmacionComponent` en un **Angular Signal** (`isLoading = signal(false)`).
-   Actualizamos el HTML de la directiva del diálogo para consumir la señal reactiva (`isLoading()`), garantizando que Angular redibuje los botones y deshabilite/habilite los controles instantáneamente al terminar el stream de datos con éxito o error.
-   Ahora, al fallar la generación, la interfaz se desbloquea por completo y permite al usuario reintentar la operación o cerrar el modal para navegar libremente.

### 2. Ejecución de Webhooks en Paralelo
-   Modificamos el flujo en `detalle-perfil.component.ts` para ejecutar las peticiones de generación de PDF (`generateExecutiveSummaryPdf` y `generateStructuredProfilePdf`) y la notificación por WhatsApp (`notifySolicitanteProfileUnderReview`) en paralelo utilizando el operador **`forkJoin`** de RxJS.
-   Esto disminuye el tiempo de carga drásticamente (de un máximo potencial de 45 segundos en serie a un máximo de 15 segundos en paralelo).
-   Si la generación de PDFs obligatorios falla, la operación se cancela de forma controlada sin persistir un estado inconsistente, pero la UI se desbloquea permitiendo reintentar de inmediato.
-   Si el webhook de notificaciones falla, su error es capturado (`catchError`) y se retorna un observable con éxito vacío (`of('')`) para no bloquear el flujo de envío del perfil.

---

## Pruebas de Escenarios Realizadas

### Caso 1: Todos los procesos responden correctamente
-   El modal muestra "Procesando...".
-   Los webhooks A4, A5 y Notificación se disparan en paralelo y finalizan con éxito.
-   Se llama a `enviarPerfilArea` en el backend, la cual cambia el estado en base de datos a `PERF-REV-AREA`.
-   El modal se cierra automáticamente, la vista se recarga y el perfil se muestra actualizado con éxito.

### Caso 2: Falla el resumen ejecutivo (Webhook A4)
-   Se ejecuta en paralelo. Al fallar A4, la alerta "No se pudo generar el resumen ejecutivo." es mostrada.
-   El error cancela la transición en el backend. El estado del perfil permanece intacto en base de datos.
-   El modal **deja de mostrar "Procesando..."**, los botones se vuelven a habilitar y el usuario puede elegir hacer clic en "Enviar al Área Solicitante" para reintentar o en "Cancelar" para cerrar el modal.

### Caso 3: Webhook sin respuesta (Timeout)
-   Se dispara la petición y transcurren 15 segundos sin respuesta de n8n.
-   Se activa el operador `timeout(15000)` del frontend, abortando la petición con un error de timeout.
-   La interfaz se desbloquea limpiamente para reintento y no se genera duplicación de llamadas.

### Caso 4: Doble clic
-   Al hacer clic en el botón de confirmación, `isLoading` del diálogo cambia a `true`, inhabilitando y desactivando inmediatamente los botones del DOM para prevenir envíos duplicados o duplicidad de trazabilidad en base de datos.
