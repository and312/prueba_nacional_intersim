# Base de Conocimiento de Arquitectura Frontend: Angular Enterprise

Este documento representa la base de conocimiento para la arquitectura del frontend del **Sistema Inteligente de Reclutamiento (SIR)** de **Nacional Seguros**. 

La especificación técnica completa, detallada y certificada se encuentra en el documento maestro del proyecto:
👉 **[ARQUITECTURA_FRONTEND_ANGULAR.md](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/arq/ARQUITECTURA_FRONTEND_ANGULAR.md)**

---

## Resumen Ejecutivo de la Arquitectura
El frontend está diseñado bajo estándares corporativos rigurosos utilizando:
* **Angular 20+** y **TypeScript**.
* **Standalone Components** y **Angular Material** para UI consistente.
* **Angular Signals** para el control de estado reactivo local síncrono.
* **RxJS** para el control de flujos de datos asíncronos y eventos en tiempo real.
* **Autenticación Híbrida (Local JWT + Proveedor corporativo de identidad)** con doble factor (MFA) interactivo y soporte de código QR.
* **Refresh Token Rotation (RTR)** seguro con almacenamiento en `sessionStorage`.
* **CDK Drag & Drop** para el Kanban de Postulantes con semaforización basada en SLAs.

Para detalles de enrutamiento, matriz de roles, estructura física de carpetas y catálogo de componentes y modelos, por favor consulte el documento maestro:
👉 **[ARQUITECTURA_FRONTEND_ANGULAR.md](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/arq/ARQUITECTURA_FRONTEND_ANGULAR.md)**
