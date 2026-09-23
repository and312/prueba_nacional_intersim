# Frontend MVP de Integración - SIR Nacional Seguros
## Sistema Inteligente de Reclutamiento (SIR)

Este proyecto es el **Frontend MVP de Integración** desarrollado en **Angular 19** y **Angular Material** para validar el funcionamiento end-to-end con el backend .NET 8 y SQL Server 2022.

---

## 1. Requisitos Previos

*   **Node.js:** Versión 20.x o superior.
*   **Angular CLI:** Versión 19.x.
*   **Backend .NET 8:** Debe estar ejecutándose en `http://localhost:5000` (o configurado en `environment.ts`).

---

## 2. Estructura de la Aplicación

El proyecto se encuentra organizado bajo las directrices modernas de desarrollo en Angular utilizando **Standalone Components**:

*   **`src/app/core/`**:
    *   `services/auth.service.ts`: Maneja el token JWT, sesión y roles (RBAC).
    *   `services/api.service.ts`: Cliente de API unificado que consume los servicios de solicitudes, perfiles, vacantes, postulantes y matching.
    *   `guards/auth.guard.ts`: Protección de rutas y validación de accesos por roles.
    *   `interceptors/auth.interceptor.ts`: Interceptor HTTP que inyecta automáticamente el token Bearer y un `X-Correlation-ID` único.
*   **`src/app/layouts/`**:
    *   `layout/`: Contiene el contenedor principal de navegación (`MatSidenav`, `MatToolbar` y footer).
*   **`src/app/features/`**:
    *   `login/`: Pantalla de inicio de sesión.
    *   `dashboard/`: Pantalla principal con métricas operativas del SIR.
    *   `catalogos/`: Consulta de tablas maestras.
    *   `solicitudes/`: CRUD y aprobación de solicitudes de personal.
    *   `perfiles/`: Visualización de profesiogramas generados por IA.
    *   `vacantes/`: CRUD y transiciones de estado de vacantes.
    *   `postulantes/`: Registro de postulantes (con carga de CV) y expediente integrado con el análisis de **Matching IA**.

---

## 3. Instrucciones de Ejecución

Para iniciar el frontend en modo de desarrollo:

```powershell
# 1. Posicionarse en el directorio del frontend
cd c:\Users\DELL XPS\Desktop\INTERSIM\nacional\frontend

# 2. Levantar el servidor de desarrollo de Angular
npm run start
```

La aplicación estará disponible en `http://localhost:4200/`.

---

## 4. Usuarios de Prueba (Simulación de Roles RBAC)

Para probar los diferentes niveles de acceso, inicie sesión con los siguientes correos (cualquier contraseña superior a 6 caracteres):

1.  **Administrador:** `admin@nacionalseguros.com.bo` (Acceso completo)
2.  **Recursos Humanos (RRHH):** `rrhh@nacionalseguros.com.bo` (Permiso para aprobar solicitudes)
3.  **Reclutador:** `reclutador@nacionalseguros.com.bo` (Gestión de vacantes y postulantes)
