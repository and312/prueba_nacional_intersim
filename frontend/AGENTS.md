# AGENTS.md — Identificador: ns-sir-app

## Build & Run

```bash
npm install        # Instalación de dependencias (Angular Material, RxJS, TailwindCSS)
npm start          # Servidor de desarrollo local (ng serve)
npm run build      # Compilación de producción (ng build)
npm test           # Ejecución de pruebas unitarias (Karma + Jasmine)
```

---

## Pila Tecnológica (Frontend - ns-sir-app)

* **Framework:** Angular 19 / 22 (Componentes **Standalone únicamente**, sin NgModules).
* **Lenguaje:** TypeScript 5.x / 6.x.
* **Estilos:** TailwindCSS v3 con tema personalizado + Angular Material v22.
* **Manejo de Estado:** Sin NgRx ni librerías pesadas de estado global — Servicios reactivos ligeros usando raw RxJS (`BehaviorSubject` / `ReplaySubject` / Signals).
* **Internacionalización:** Transloco v8 (archivos de traducción en `public/i18n/`).
* **Visualizaciones:** ApexCharts (`ng-apexcharts`) y Editor Enriquecido Quill (`ngx-quill`).

---

## Identidad y Estructura del Proyecto

* **Estructura Modular (`src/app/features/`):**
  * `auth / login`: Autenticación, MFA TOTP, guards y gestión de sesión JWT.
  * `solicitudes`: Formulario y tablero Kanban interactivo para solicitudes de personal.
  * `perfiles`: Creación, edición, consulta y resumen estructurado de Perfiles de Cargo.
  * `vacantes`: Gestión y publicación de vacantes.
  * `postulantes`: Clasificación y perfilado de candidatos internos y externos.
  * `estrategias / matching`: Asignación de reglas y scoring.
  * `catalogos / parametrizacion`: Gestión de listas maestras (Áreas, Gerencias, Cargos, Roles).

---

## Reglas de Desarrollo Frontend

1. Usar únicamente componentes Standalone e imports explícitos.
2. Mantener la lógica de negocio y llamadas HTTP encapsuladas en `Services`, dejando los componentes centrados en la interfaz UI.
3. No mutar el estado de los componentes directamente desde la plantilla HTML; utilizar manejadores de eventos explícitos.
4. Garantizar diseño responsive adaptado a la paleta de colores y componentes de Nacional Seguros.
