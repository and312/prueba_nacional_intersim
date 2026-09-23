# KB_23_PerfilCargo_Sprint_2026 — Gobierno del Módulo de Perfiles (IMPLEMENTADO)

Esta Knowledge Base documenta la persistencia, flujos de estados, control documental y APIs para la gestión de Perfiles dentro del SIR.

---

## 1. Mapeo Físico de Datos Maestros

### Entidades y Tablas Reutilizadas
*   **`PerfilCargo` (`PerfilesCargo`)**: Cabecera principal del perfil.
    *   *Propiedades*: `PerfilCargoId` (PK), `SolicitudId` (FK, UNIQUE), `Cargo`, `Descripcion`, `Version`, `EstadoId` (FK), `PdfUrl`, `JsonOriginalIA`, `JsonActual`, `Activo`, `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`, `IsDeleted`.
*   **`SolicitudDocumento` (`SolicitudDocumentos`)**: Historial de archivos físicos (PDFs). Se filtran bajo `"RESUMEN_EJECUTIVO_PDF"` y `"PERFIL_ESTRUCTURADO_PDF"`.
    *   *Índice Único*: Configurado para la combinación `SolicitudId` + `TipoDocumento` (`UQ_SolicitudDocumentos_Solicitud_Tipo`) para garantizar que exista como máximo un PDF por tipo.
*   **`StateHistory` (`StateHistories`)**: Historial de estados transversal (utiliza `Entidad = 'Perfil'`, `EntidadId = PerfilCargo.Id`).

### Entidades y Tablas Nuevas
*   **`ResumenEjecutivo` (`ResumenEjecutivos`)**: Almacena de forma física estructurada los campos del resumen ejecutivo generados por la IA o corregidos por RRHH.
    *   *Propiedades*: `ResumenId` (PK), `PerfilCargoId` (FK, UNIQUE), `Resumen` (NVARCHAR), `ObjetivoCargo` (NVARCHAR), `FuncionesPrincipales` (NVARCHAR, JSON array), `RequisitosMinimos` (NVARCHAR, JSON array), `FormacionExperiencia` (NVARCHAR), `HardSkills` (NVARCHAR, JSON array), `SoftSkills` (NVARCHAR, JSON array), `Modalidad` (NVARCHAR), `Ubicacion` (NVARCHAR), `BandaSalarial` (NVARCHAR), `CriteriosEvaluacion` (NVARCHAR), `CaracteristicasClave` (NVARCHAR), `ValoracionPerfil` (NVARCHAR), `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`.
*   **`TipoObservacion` (`TiposObservacion`)**: Catálogo parametrizable de tipos de observaciones del perfil.
    *   *Propiedades*: `TipoObservacionId` (PK), `Codigo` (VARCHAR, UNIQUE), `Nombre` (VARCHAR), `Descripcion` (VARCHAR), `Estado` (VARCHAR), `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`, `IsDeleted`.
*   **`PerfilObservacion` (`PerfilObservaciones`)**: Observaciones registradas por el solicitante enlazadas al catálogo de tipos.
    *   *Propiedades*: `PerfilObservacionId` (PK), `PerfilCargoId` (FK), `TipoObservacionId` (FK), `Comentario` (NVARCHAR), `UsuarioSolicitanteId` (FK), `NumeroIteracion` (INT), `EstadoObservacion` (`Pendiente`/`Atendida`), `CreatedDate`, `AtendidaPorUsuarioId` (FK), `FechaAtencion` (DATETIME).

### Perfil Estructurado (Vista Compuesta)
*   **No requiere tabla nueva**. Es una vista de solo lectura construida dinámicamente consultando los datos existentes de `Solicitudes` y sus relaciones (`Usuarios`, `Areas`, `Regionales`, `TiposSolicitud`, `ModalidadesTrabajo`) del Sprint 2.
*   **`PerfilSecciones`** no se utiliza en este flujo.

---

## 2. Estrategia de Control Documental (Reemplazo de PDFs)

*   Se restringe la persistencia de PDFs a **un único archivo vigente por tipo** para cada solicitud.
*   **Lógica de Carga (`RegistrarDocumentoCommandHandler`)**:
    *   Busca coincidencia en `SolicitudDocumentos` por: `SolicitudId` + `TipoDocumento`.
    *   Si existe: Se reemplaza físicamente el archivo en almacenamiento y se actualizan los metadatos del registro existente (`FileName`, `StoragePath`, `PublicUrl`, `GeneradoPor`, `CorrelationId`), manteniendo intacto el `CreatedDate` original.
    *   Si no existe: Se inserta un nuevo registro.

---

## 3. Resolución del Usuario en WhatsApp

*   Identificación de número móvil entrante.
*   Conversión a formato normalizado: `var normalizedIdentifier = identifier.StartsWith("+") ? identifier : "+591" + identifier;`
*   Búsqueda de sesión en `WSSessions` para rastreo temporal.
*   Resolución del solicitante cruzando el número normalizado con el campo `Usuario.Telefono`.

---

## 4. Máquina de Estados del Perfil

| Código de Estado | Descripción Funcional | Actor Autorizado |
| :--- | :--- | :--- |
| **`PERF-PEN-GEN`** | Pendiente de generación | Sistema / Backend |
| **`PERF-REV-RRHH`** | En revisión RRHH | Automatización / LLM |
| **`PERF-RES-GEN`** | Resumen ejecutivo generado | RRHH |
| **`PERF-REV-AREA`** | En revisión área solicitante | Sistema (Al cargar PDFs) |
| **`PERF-OBS-AREA`** | Observado por área solicitante | Solicitante (WhatsApp / Web) |
| **`PERF-COR-RRHH`** | En corrección RRHH | RRHH (Al atender observación) |
| **`PERF-APR-AREA`** | Aprobado por área solicitante | Solicitante (WhatsApp) |
| **`PERF-APR-FIN`** | Perfil aprobado final | RRHH |

---

## 5. Endpoints de Integración y Frontend

### Endpoints para Catálogo de Observaciones (Administrativos)
*   `GET    /api/v1/tipos-observacion`: Lista todos los tipos de observaciones.
*   `POST   /api/v1/tipos-observacion`: Registra un tipo nuevo. (Solo Admin)
*   `PUT    /api/v1/tipos-observacion/{id}`: Modifica los campos básicos. (Solo Admin)
*   `PATCH  /api/v1/tipos-observacion/{id}/activar`: Reactiva el tipo. (Solo Admin)
*   `PATCH  /api/v1/tipos-observacion/{id}/inactivar`: Cambia el estado a inactivo. (Solo Admin)

### Endpoints para Automatización (n8n / API Interna)
*   `GET /api/internal/vacancy-requests/perfiles?solicitanteId={id}&telefono={number}`: Consulta perfiles del solicitante.
*   `GET /api/internal/perfiles/{id}/detalle-completo`: Detalle compuesto y `resumenEjecutivoRol`.
*   `PUT /api/internal/perfiles/{perfilId}/resumen`: Registra o actualiza el resumen ejecutivo físico.
*   `POST /api/internal/perfiles/{perfilId}/observaciones`: Registro de observaciones desde WhatsApp.
*   `POST /api/internal/perfiles/{perfilId}/aprobar-solicitante`: Aprobación del solicitante desde WhatsApp.
*   `POST /api/v1/solicitudes/{solicitudId}/documentos`: Registra o sobrescribe el PDF.
*   `GET /api/internal/solicitudes/{solicitudId}/documentos/{tipo}/descarga`: Descarga el PDF vigente.
*   `GET /api/internal/perfiles-estructurados/{solicitudId}`: Obtiene el perfil estructurado filtrando por SolicitudId (n8n).
*   `PUT /api/internal/perfiles-estructurados/{solicitudId}`: Guarda o actualiza el perfil estructurado realizando un **Upsert** (n8n).
*   `GET /api/internal/perfiles/solicitud/{solicitudId}/resumen`: Obtiene el resumen ejecutivo por SolicitudId (n8n).
*   `PUT /api/internal/perfiles/solicitud/{solicitudId}/resumen`: Guarda o actualiza el resumen ejecutivo realizando un **Upsert** (n8n).

### Endpoints para Frontend (Backoffice)
*   `GET /api/v1/perfiles`: Listado con filtro de visibilidad por rol y propietario (resuelve colisiones de ruta).
*   `GET /api/v1/perfiles/{id}`: Detalle compuesto de cabecera, `resumenEjecutivo`, `perfilEstructurado` (de solo lectura de Solicitudes) y `observaciones`.
*   `PUT /api/v1/perfiles/{id}/resumen`: Actualización del resumen estructurado por RRHH/Administrador.
*   `POST /api/v1/perfiles/{id}/observaciones`: Registro de observaciones por el Área Solicitante.
*   `POST /api/v1/perfiles/{id}/aprobar-solicitante`: Aprobación del solicitante.
*   `POST /api/v1/perfiles/{id}/atender-observaciones`: Marcado de observaciones atendidas por RRHH/Administrador.
*   `POST /api/v1/perfiles/{id}/enviar-area`: Despacho del perfil de RRHH a Área Solicitante.
*   `POST /api/v1/perfiles/{id}/aprobar-final`: Cierre final por RRHH/Administrador.
*   `GET /api/v1/perfiles-estructurados`: Listado general de perfiles estructurados (Accesible para Solicitante, RRHH, Administrador).
*   `GET /api/v1/perfiles-estructurados/{id}`: Detalle del perfil estructurado por ID.
*   `GET /api/v1/perfiles-estructurados/solicitud/{solicitudId}`: Detalle del perfil estructurado por SolicitudId.
*   `POST /api/v1/perfiles-estructurados`: Crea perfil estructurado (Solo RRHH, Administrador).
*   `PUT /api/v1/perfiles-estructurados/{id}`: Actualiza perfil estructurado (Solo RRHH, Administrador).

### Flujo de Estados e Integraciones Cruzadas
1. **Webhook de Aprobación**: Al transitar una solicitud al estado `SOL-APR` (Aprobada), el backend dispara asíncronamente un webhook saliente hacia `https://nacional-seguros-dev.isia.cloud/webhook/perfil_vacante`.
2. **Upsert y Transición**: n8n procesa la solicitud y llama a `PUT /api/internal/perfiles-estructurados/{solicitudId}`. La primera vez que se realiza este Upsert (creación), el handler cambia automáticamente el estado del `PerfilCargo` asociado a `"PERF-REV-RRHH"` (En revisión RRHH) y registra el cambio en `dbo.StateHistory`. Las actualizaciones subsiguientes omiten la transición para no entorpecer los flujos manuales.

---

## 6. Arquitectura de Despliegue en Servidor de Desarrollo (`2.25.133.206`)

Para garantizar alta disponibilidad y facilidad de acceso, el servidor tiene desplegado el backend en dos puertos con diferentes propósitos, ambos conectados a la base de datos de desarrollo activa:

### A. Base de Datos Activa
*   **Servidor**: `2.25.133.206,1433`
*   **Base de datos**: `SIR_NacionalSeguros` (contiene las 60 tablas con nombres físicos en español y la migración final del Sprint de Perfiles).
*   *Nota*: La base de datos `nacional_seguros` contiene tablas con nombres en inglés (`VacancyRequests`, etc.) pertenecientes a versiones previas.

### B. Enrutamiento en Nginx (`/etc/nginx/sites-available/`)
*   **Frontend**: Desplegado en `/var/www/nacional-intersim/frontend/`.
*   **Proxy `/api/` (Consumo Frontend)**:
    *   Redirige a `http://localhost:5000`.
    *   Gestionado por el servicio de sistema **`sir-backend.service`** (ejecución nativa .NET 8 Kestrel en `/var/www/nacional-intersim/backend/`).
*   **Proxy `/backend/` (Consumo Automatizaciones/n8n)**:
    *   Redirige a `http://127.0.0.1:5210`.
    *   Gestionado por el contenedor Docker **`nacional-seguros-backend`** (ejecución contenerizada en `/opt/docker/nacionalseguros/`).

---

## 7. Incompatibilidades e Incidencias del Backend Identificadas

Durante la integración del Sprint 3 se identificaron las siguientes incompatibilidades y errores del lado del backend:

### Colisión de Rutas (AmbiguousMatchException en /api/v1/perfiles)
El backend cuenta con dos endpoints que mapean exactamente la misma ruta HTTP GET `/api/v1/perfiles` bajo los siguientes controladores:
1.  `NacionalSeguros.Api.Controllers.PerfilController.BuscarPerfiles` (Ruta: `api/v1/perfiles`, parámetro query `q`)
2.  `NacionalSeguros.Api.Controllers.PerfilesController.Listar` (Ruta: `api/v1/perfiles`, sin parámetros)

Al realizar cualquier consulta GET a `/api/v1/perfiles`, el motor de enrutamiento de ASP.NET Core lanza una excepción de ambigüedad (`Microsoft.AspNetCore.Routing.Matching.AmbiguousMatchException: The request matched multiple endpoints`), lo que resulta en un error HTTP `500 Internal Server Error` persistente.

*Acción recomendada para backend*: Eliminar o renombrar el endpoint obsoleto `BuscarPerfiles` de `PerfilController` (que ya cuenta con la directiva `[ApiExplorerSettings(IgnoreApi = true)]` para ocultarse de Swagger) o ajustar su plantilla de enrutamiento.

---

## 8. Gobierno Visual y Control de Estados en Frontend (Sprint 3 - Fase 2)

Para mejorar la experiencia de usuario y simplificar la comprensión del flujo de negocio, se implementó una capa de traducción y normalización de estados visuales y funcionales en el frontend, ocultando transiciones técnicas internas.

### A. Estados de Negocio y Mapeo Visual

| Código Real (Backend) | Nombre Funcional (Backend) | Estado Visual del Usuario (Frontend) | Propósito / Tipo |
| :--- | :--- | :--- | :--- |
| `PERF-PEN-GEN` | Pendiente de generación | **Procesando** | Transitorio (Solo Lectura) |
| `PERF-REV-RRHH` | En revisión RRHH | **En revisión de Recursos Humanos** | Activo (Editable RRHH) |
| `PERF-RES-GEN` | Resumen ejecutivo generado | **En revisión del Área Solicitante** | Transitorio |
| `PERF-REV-AREA` | En revisión área solicitante | **En revisión del Área Solicitante** | Activo (Editable Área) |
| `PERF-OBS-AREA` | Observado por área solicitante | **Observado por el Área Solicitante** | Activo (Editable RRHH) |
| `PERF-COR-RRHH` | En corrección RRHH | **Observado por el Área Solicitante** | Activo (Editable RRHH) |
| `PERF-APR-AREA` | Aprobado por área solicitante | **Aprobado por el Área Solicitante** | Activo (Solo Lectura) |
| `PERF-APR-FIN` | Perfil aprobado final | **Perfil aprobado final** | Final (Solo Lectura) |
| *Otro / Desconocido* | *N/A* | **Estado no identificado** | Control de Fallback |

### B. Matriz de Permisos por Rol y Estado

1.  **RRHH / Administrador**:
    *   **Edición**: Habilitada en los estados `PERF-REV-RRHH`, `PERF-OBS-AREA` y `PERF-COR-RRHH`.
    *   **Acciones**:
        *   `PERF-REV-RRHH` $\rightarrow$ Enviar al Área Solicitante.
        *   `PERF-OBS-AREA` / `PERF-COR-RRHH` $\rightarrow$ Enviar correcciones.
        *   `PERF-APR-AREA` $\rightarrow$ Realizar aprobación final.
        *   `PERF-APR-FIN` / `PERF-PEN-GEN` / `PERF-RES-GEN` $\rightarrow$ Solo consulta.
2.  **Área Solicitante (Propietario del Área)**:
    *   **Edición**: Siempre en modo **Solo Lectura** (el Perfil estructurado y Resumen ejecutivo no tienen inputs ni botones de edición habilitados).
    *   **Acciones**:
        *   `PERF-REV-AREA` / `PERF-RES-GEN` $\rightarrow$ Botones de **Aprobar perfil** y **Enviar observaciones** disponibles.
        *   Otros estados $\rightarrow$ Vista de solo consulta.

### C. Archivos Frontend Modificados y Rol de cada uno

*   [profile-status.config.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/constants/profile-status.config.ts): Centraliza las etiquetas visibles (`label`), iconos y CSS de los badges de estado.
*   [profile-filters.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/components/profile-filters/profile-filters.component.ts): Limita las opciones mostradas en el select de estados del listado a las 5 etapas funcionales.
*   [profiles-list.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/services/profiles-list.service.ts):
    *   Ajusta el filtrado lógico en el listado para agrupar estados compuestos.
    *   Recalcula dinámicamente las métricas de KPIs del listado según el rol y área.
*   [profile-table.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/components/profile-table/profile-table.component.ts): Asigna las etiquetas de acción oportunas (ej. `"Continuar corrección"`, `"Ver observaciones"`, etc.) según el rol y estado real del perfil.
*   [detalle-perfil.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/detalle-perfil/detalle-perfil.component.ts): Controla la lógica de permisos dinámicos, la visualización de botones de acción en la cabecera (excluyendo controles de edición cuando el tab es "Resumen ejecutivo"), e inicia de forma automática el formulario estructurado en modo edición si se accedió por la ruta de acción `/revisar` o `/corregir`.

---

## 9. Integración de Servicios Reales y Transiciones de Estados (Sprint 3 - Fase 3)

Se integraron las llamadas reales al backend utilizando `ApiService` para la ejecución de transiciones de negocio del perfil de cargo, eliminando simulaciones locales de estado en el detalle del perfil.

### A. Endpoints Reales Utilizados
*   **Envío al Área**: `apiService.enviarPerfilArea(id)` (POST `/api/v1/perfiles/{id}/enviar-area`). Invocado desde `enviarAlArea()` y `enviarCorreccionesAlArea()`.
*   **Atención de Observaciones**: `apiService.atenderObservaciones(id)` (POST `/api/v1/perfiles/{id}/atender-observaciones`). Invocado por RRHH al hacer clic en `"Atender observaciones"` en el estado `PERF-OBS-AREA` para transitar el perfil a `PERF-COR-RRHH`.
*   **Aprobación del Solicitante**: `apiService.aprobarSolicitante(id)` (POST `/api/v1/perfiles/{id}/aprobar-solicitante`). Invocado por el Área Solicitante en el diálogo de confirmación de aprobación.
*   **Aprobación Final**: `apiService.aprobarFinal(id)` (POST `/api/v1/perfiles/{id}/aprobar-final`). Invocado por RRHH en la aprobación final definitiva.

### B. Ciclo de Vida del Cambio de Estado
Tras cualquier acción exitosa de transición, el frontend:
1.  Invoca el endpoint real de la API.
2.  Llama a `cargarDatos()`, que refesca los datos haciendo un `GET /api/v1/perfiles/{id}`.
3.  Actualiza el estado y los controles dinámicos de forma exclusiva a partir de la respuesta real devuelta por el servidor.

### C. Integración de Observaciones Reales
Se conectó `ObservacionesPerfilService` directamente a los endpoints de backend:
*   `obtenerTiposObservacionActivos()` $\rightarrow$ Consume `apiService.getTiposObservacion(true)` y lo mapea al modelo `TipoObservacion` local.
*   `obtenerObservacionesPerfil(idPerfil)` $\rightarrow$ Obtiene las observaciones directamente del perfil compuesto haciendo `apiService.getPerfilById(idPerfil)`.
*   `agregarObservacionPerfil(...)` $\rightarrow$ Registra una observación en el backend con `apiService.registrarObservacion(idPerfil, idTipoObservacion, comentario)`.

### D. Traducción Dinámica Basada en Rol
Para cumplir con las restricciones de enmascaramiento visual:
*   El componente `ProfileStatusBadgeComponent` recibe el rol activo del usuario (`[rol]="userRole"` o `[rol]="rol"`).
*   Si el rol es `AreaSol` y el estado real del perfil es `PERF-OBS-AREA` o `PERF-COR-RRHH`, se traduce visualmente a `"En revisión de Recursos Humanos"` (con su correspondiente diseño de insignia `"badge-review"`).
*   Para RRHH y Administrador, dichos estados se muestran en su denominación técnica original: `"Observado por el Área Solicitante"`.


## 10. Representación Visual del Perfil Estructurado y Guardado Secuencial (Sprint 3 - Ajuste Controlado)

Se completó y corrigió la visualización del Perfil estructurado del cargo alineándolo al diseño físico de 12 acordeones y controlando la edición de campos.

### A. Alineación de los 12 Acordeones Visuales
Se rediseñó el mapeo dinámico en `PerfilesService.mapEstructuradoToSecciones` para ajustarlo exactamente a las especificaciones del cliente:
1.  **Datos generales del cargo**: Nombre del cargo (SV), Área solicitante (SV), Regional/ciudad (SV), Reporta a (RRHH), Tipo de posición (SV), Modalidad (SV), Banda salarial (RRHH). (Se eliminó visualmente el campo duplicado de Gerencia).
2.  **Objetivo principal del cargo**: Objetivo principal (SV).
3.  **Perfil requerido**: Formación académica (SV), Experiencia requerida (SV - combina experiencia mínima, indispensable y valorada).
4.  **Conocimientos técnicos requeridos**: Conocimientos técnicos (SV).
5.  **Herramientas y sistemas**: Herramientas y sistemas (RRHH - editable).
6.  **Funciones principales del cargo**: Funciones principales (RRHH - editable).
7.  **Competencias clave**: Competencias clave (SV).
8.  **Indicadores de éxito del cargo**: Indicadores de éxito (RRHH - editable).
9.  **Perfil ideal del candidato**: Descripción del perfil ideal (RRHH - editable).
10. **Filtros clave para selección**: Criterios excluyentes sugeridos (SV), Criterios deseables (SV).
11. **Matriz sugerida de ponderación**: Matriz sugerida (RRHH - editable).
12. **Perfil tipo de alto ajuste**: Perfil tipo de alto ajuste (RRHH - editable).

### B. Mapeo Resiliente a Estructuras del Backend
El mapeo de datos soporta de manera nativa y transparente dos estructuras del DTO devueltas por el servidor según la fase de generación del perfil:
*   **Fase No Generada (Datos iniciales de Solicitud)**: Lee de las propiedades `perfilEstructurado.datosGenerales`, `perfilEstructurado.perfilRequerido` y `perfilEstructurado.condicionesVacante`.
*   **Fase Generada (Datos consolidados)**: Lee de las propiedades `perfilEstructurado.datosGeneralesCargo`, `perfilEstructurado.perfilRequerido`, `perfilEstructurado.herramientasSistemas`, `perfilEstructurado.conocimientosTecnicosRequeridos`, `perfilEstructurado.funcionesPrincipalesCargo`, `perfilEstructurado.competenciasClave`, `perfilEstructurado.indicadoresExitoCargo`, `perfilEstructurado.matrizPonderacion`, `perfilEstructurado.perfilIdealCandidato` y `perfilEstructurado.perfilTipoAltoAjuste`.

### C. Estrategia de Guardado Secuencial (Concurrencia de Versiones)
Debido a que el backend implementa un control estricto de concurrencia optimista y versionamiento del perfil (donde cada actualización exitosa a una sección desactiva el registro actual y genera una nueva versión con un `PerfilId` incremental diferente):
1.  **Ejecución en Serie (No Paralela)**: Las secciones modificadas por RRHH se guardan secuencialmente una a la vez utilizando llamadas ordenadas en cadena a `apiService.actualizarPerfilSeccion(...)`.
2.  **Propagación de Versión Activa**: Cada respuesta exitosa de guardado retorna la nueva cabecera activa y su ID actualizado (`res.perfilId`). El componente de frontend intercepta este ID y lo utiliza inmediatamente como el identificador dinámico de destino para la llamada de la siguiente sección en la cola.
3.  **Actualización de URL**: Al finalizar todo el guardado en cadena, se actualiza el URL del navegador (`/perfiles/{nuevoId}`) utilizando `replaceUrl: true` para mantener la consistencia al recargar o refrescar.

### D. Tabla de Secciones Técnicas y Payloads Reales

| Campo Visual | Nro. Sección Backend | Tipo Contenido Backend | Estructura / Ejemplo de Payload de Contenido |
| :--- | :---: | :--- | :--- |
| **Reporta a / Banda salarial** | **3** | Plain string | `"Reporta a: Gerente de Tecnología\nBanda Salarial/Presupuesto: 15000 BOB"` |
| **Funciones principales** | **7** | JSON Array string | `"[\"Diseñar arquitecturas\", \"Realizar revisiones de código\"]"` |
| **Perfil ideal** | **10** | JSON Array string | `"[\"Profesional autónomo y de alto nivel de auto-aprendizaje\"]"` |
| **Herramientas / KPIs** | **12** | JSON Object string | `"{\"Herramientas\":[\"Visual Studio\",\"Docker\"],\"Kpis\":[\"Despliegues sin errores\"],\"Riesgos\":[],\"ObservacionesIA\":\"\"}"` |
| **Matriz de ponderación** | **8** | JSON Array string | `"[\"Matriz: 40% técnica, 40% experiencia, 20% cultura\"]"` |
| **Perfil tipo alto ajuste** | **11** | JSON Array string | `"[\"Ingeniero Backend Senior con más de 5 años de trayectoria\"]"` |

*Nota*: Cada petición HTTP PUT envía a `/api/v1/perfiles/{id}/secciones/{numSeccion}` un cuerpo con el siguiente formato:
```json
{
  "contenido": "[Contenido formateado según la tabla anterior]",
  "motivo": "Ajuste del Perfil estructurado desde Backoffice"
}
```

### E. Detección de Deltas para Guardados Innecesarios
Antes de iniciar la serie de peticiones HTTP, el frontend realiza una validación comparativa local entre el valor de edición actual del formulario y el valor cargado inicialmente en el perfil (recuperado de `getInitialValue(fieldId)`):
*   Los arreglos multilínea (herramientas, kpis, funciones) se comparan extrayendo, recortando espacios (`trim()`) y descartando líneas vacías mediante la función auxiliar `areListsDifferent(val1, val2)`.
*   Si una sección técnica no presenta diferencias con respecto a los datos cargados originalmente de base de datos, es excluida de la lista de envíos.
*   Si la lista total de cambios es vacía (`updates.length === 0`), se notifica al usuario que *"No existen cambios para guardar"* y se cancela la secuencia sin efectuar llamadas HTTP.

### F. Comportamiento Ante Fallo Parcial
Si ocurre un error de comunicación o rechazo del servidor en alguna de las llamadas de la serie de secciones:
1.  **Parada Inmediata**: La recursión secuencial se interrumpe y se descartan las llamadas pendientes de las siguientes secciones en cola.
2.  **Alineación de Versión**: Se actualiza la propiedad de ID física local `this.perfilId` y la URL del navegador al identificador de la última versión exitosa del perfil (`currentId`) utilizando `{ replaceUrl: true }`.
3.  **Refresco Forzado**: Se realiza un GET `/api/v1/perfiles/{lastSuccessfulId}` a través de `cargarDatos()` para sincronizar los datos de la UI con la última versión guardada físicamente en el servidor.
4.  **Notificación**: Se muestra una alerta nativa indicando el fallo parcial, el nombre amigable de la sección que falló y la redirección a la última versión segura.

### G. Limitaciones No Probadas
Debido a la ausencia de un navegador interactivo en esta fase final por problemas de MCP locales, las llamadas secuenciales reales PUT a los endpoints `/api/v1/perfiles/{id}/secciones/{numeroSeccion}` y la navegación recursiva no se probaron dinámicamente frente al servidor de desarrollo con interacciones de usuario, por lo que su correcto funcionamiento final queda sujeto a pruebas manuales en el despliegue de desarrollo.


## 11. Auditoría y Verificación de Integración del Perfil Estructurado (Sprint 3)

Esta sección documenta formalmente la auditoría estática y el estado de la integración real del Perfil estructurado con el backend.

### A. Cadena de Lectura de Datos
El flujo de inicialización y renderizado de un perfil estructurado sigue la siguiente cadena de llamadas 100% conectada a la API real del backend:
1.  **Listado de perfiles** (`profile-table.component.html`): Al pulsar en "Revisar perfil" o "Corregir perfil", se emite la acción en base al identificador real e inmutable `profile.id` (el `PerfilCargoId` primario autoincremental de la base de datos).
2.  **Ruta Angular**: Redirección a `/perfiles/:id` o `/perfiles/:id/revisar`.
3.  **Carga del Detalle** (`detalle-perfil.component.ts`):
    *   Lee el ID numérico de la ruta: `this.perfilId = Number(this.route.snapshot.paramMap.get('id'));`
    *   Invoca: `this.perfilesService.obtenerPerfilPorId(this.perfilId)`
4.  **Servicio de Perfiles** (`perfiles.service.ts`):
    *   Método: `obtenerPerfilPorId(id)`
    *   Delegación a API: `this.apiService.getPerfilById(id)` (Llama al endpoint GET `/api/v1/perfiles/{id}`).
5.  **Mapeador Resiliente** (`mapDetailToFrontend(p)`):
    *   Traduce el DTO de respuesta `PerfilDetail` y lo convierte a `DetallePerfil`.
    *   Puebla los campos en el array `secciones` (12 acordeones estructurados) usando el método `mapEstructuradoToSecciones(p)` que soporta tanto perfiles generados como solicitudes de vacante no consolidadas.

### B. Tabla Completa de Mapeo Real (DTO -> 12 Secciones Visuales)

| Sección Visual | Campo | Propiedad del DTO Backend (`PerfilDetail`) | Tipo de Dato en el DTO | Transformación Aplicada | Origen Real de Datos |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **1. Datos generales** | Cargo | `p.cargo` o `est.datosGeneralesCargo.cargo` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Área | `p.areaSolicitante` o `est.datosGeneralesCargo.area` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Regional/ciudad | `est.datosGeneralesCargo.regional` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Reporta a | `est.datosGeneralesCargo.reportaA` o `reporta_a` | string | `parseStringOrArray()` | `AJUSTADO_RRHH` (Editable) |
| **1. Datos generales** | Tipo posición | `est.condicionesVacante.seniority` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Modalidad | `est.condicionesVacante.modalidadTrabajo` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Banda salarial | `resumenEjecutivo.bandaSalarial` | string | `parseStringOrArray()` | `AJUSTADO_RRHH` (Editable) |
| **2. Objetivo** | Objetivo principal | `est.objetivoPrincipalCargo` o `pr.objetivoCargo` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **3. Perfil requerido** | Formación académica| `est.perfilRequerido.estudios` o `pr.formacionAcademica` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **3. Perfil requerido** | Experiencia requerida| `est.perfilRequerido.anosExperiencia` | string / number | Traducido a *"X años de experiencia"* | `SOLICITUD_VACANTE` |
| **4. Conocimientos** | Conocimientos técnicos| `est.conocimientosTecnicosRequeridos` | string[] / string | `splitTextToList()` | `SOLICITUD_VACANTE` |
| **5. Herramientas** | Herramientas y sistemas| `est.herramientasSistemas` | string[] / object | Desglose de claves de objeto o texto | `AJUSTADO_RRHH` (Editable) |
| **6. Funciones** | Funciones principales | `est.funcionesPrincipalesCargo` | string[] / string | `splitTextToList()` | `AJUSTADO_RRHH` (Editable) |
| **7. Competencias** | Competencias clave | `est.competenciasClave` | string[] / string | `splitTextToList()` | `SOLICITUD_VACANTE` |
| **8. Indicadores** | Indicadores de éxito | `est.indicadoresExitoCargo` | string[] / string | `splitTextToList()` | `AJUSTADO_RRHH` (Editable) |
| **9. Perfil ideal** | Perfil ideal | `est.perfilIdealCandidato` | string[] / string | `parseStringOrArray()` | `AJUSTADO_RRHH` (Editable) |
| **10. Filtros clave** | Criterios excluyentes | `est.filtrosClaveSeleccion` | string[] / object | Desglose de claves de objeto o texto | `SOLICITUD_VACANTE` |
| **10. Filtros clave** | Criterios deseables | `est.perfilRequerido.criteriosDeseables` | string[] / string | `splitTextToList()` | `SOLICITUD_VACANTE` |
| **11. Matriz** | Matriz ponderación | `est.matrizPonderacion` | array / string | Formateado a *"criterio: peso%"* | `AJUSTADO_RRHH` (Editable) |
| **12. Perfil alto ajuste**| Perfil tipo alto ajuste | `est.perfilTipoAltoAjuste` | string[] / string | `parseStringOrArray()` | `AJUSTADO_RRHH` (Editable) |

### C. Servicios de Lectura y Guardado Utilizados
*   **Lectura**: `apiService.getPerfilById(id)` (GET `/api/v1/perfiles/{id}`).
*   **Guardado**: `apiService.actualizarPerfilSeccion(perfilId, numeroSeccion, { contenido, motivo })` (PUT `/api/v1/perfiles/{id}/secciones/{numeroSeccion}`).
*   **Versionamiento**: Confirmado en el handler del backend. Cada guardado de sección devuelve un nuevo `perfilId` (clave primaria incremental del nuevo registro inmutable) y actualiza `versionActiva`.

### D. Regla de Guardado Incremental y Recarga Posterior
1.  El formulario reactivo se mapea con los campos iniciales del perfil mediante `getInitialValue(campoId)`.
2.  Al hacer clic en "Guardar ajustes", el frontend compara dinámicamente los valores actuales con los iniciales mediante delta check.
3.  Solo las secciones técnicas con cambios detectados reales se agregan a la cola de peticiones.
4.  Si la cola está vacía, no se ejecuta ninguna llamada HTTP.
5.  Si hay cambios, se ejecutan en serie secuencial enviando la petición del nuevo ID de perfil versión (`currentId`) devuelto en el DTO de respuesta anterior.
6.  Al terminar la secuencia completa, la URL del navegador se reescribe de forma transparente con el ID final vigente (`replaceUrl: true`) y se ejecuta `cargarDatos()` que hace un GET del nuevo estado del perfil persistido, cerrando el modo de edición y asegurando que la UI dibuje exclusivamente la información retornada del servidor.

### F. Resolución del Bug en Detección de Cambios de Formulario (Form dirty check)
Anteriormente, el botón `Guardar ajustes` se mantenía inactivo o la vista de edición no reaccionaba a los cambios del usuario porque la propiedad `FormGroup.dirty` de Angular es una propiedad ordinaria y no un Signal. Al estar contenida dentro del computado `permisos()`, Angular no detectaba su cambio, por lo que el botón nunca cambiaba su estado de deshabilitación de forma reactiva.
*   **Solución**: Se implementó el Signal reactivo `formTieneCambios = signal(false)` en `detalle-perfil.component.ts`.
*   Al inicializarse el formulario, se realiza una suscripción dinámica:
    ```typescript
    this.perfilForm.valueChanges.subscribe(() => {
      this.formTieneCambios.set(this.perfilForm.dirty);
    });
    ```
*   La propiedad `puedeGuardar` del computado ahora depende directamente del Signal `formTieneCambios()`, asegurando una actualización visual reactiva e inmediata en cuanto el usuario modifica cualquier campo.
*   Se añadieron salvaguardas durante el guardado para deshabilitar los botones de Guardar y Cancelar si `isLoading()` es `true`, evitando llamadas concurrentes duplicadas.

### G. Flujo de Estados y Correcciones en PERF-OBS-AREA (Observado por el Área Solicitante)
De acuerdo a las reglas de negocio del backend, Recursos Humanos no puede modificar ni guardar cambios directamente si el perfil se encuentra en el estado `PERF-OBS-AREA` (código visual de frontend: `EstadoPerfil.Observada`).
*   **Corrección**: Se ajustó la condición `canEditState` en `cargarDatos()` para excluir `EstadoPerfil.Observada` del inicio automático del modo de edición.
*   **Flujo Restablecido**: En `PERF-OBS-AREA`, el profesional de RRHH visualiza el panel de observaciones del área y dispone del botón de acción `"Atender observaciones"`. Al pulsarlo, se llama a la API real `/atender-observaciones`, la cual cambia el estado del perfil en base de datos a `PERF-COR-RRHH` (código visual: `EstadoPerfil.Corregida`). Solo una vez que el perfil se encuentra en estado `Corregida`, se habilita el botón `"Editar perfil"`, permitiendo aplicar las correcciones estructuradas y guardarlas secuencialmente con éxito.

### H. Configuración del Botón "Enviar al Área Solicitante"
Para unificar los flujos de envío tras revisiones iniciales y correcciones de observaciones, se integró el botón `"Enviar al Área Solicitante"` (ID: `'enviarArea'`) en los estados clave de revisión:
*   **Estados Permitidos**:
    *   `EstadoPerfil.EnRevisionRRHHPE` (En revisión RRHH)
    *   `EstadoPerfil.ResumenEjecutivoGenerado` (Resumen ejecutivo generado)
    *   `EstadoPerfil.Corregida` (En corrección RRHH)
*   **Diálogo de Confirmación**: Al presionar el botón, se despliega el componente de diálogo `DialogoConfirmacionComponent` con la siguiente parametrización técnica:
    *   `titulo`: `"Enviar al Área Solicitante"`
    *   `descripcion`: `"¿Deseas enviar este perfil al Área Solicitante? Una vez enviado, el área solicitante podrá revisar la información y aprobar u observar el perfil."`
    *   `icono`: `'send'`
    *   `variante`: `'advertencia'`
    *   `requiereCheckbox`: `false`
    *   `textoBotonPrincipal`: `"Enviar al Área Solicitante"`
    *   `textoBotonCancelar`: `"Cancelar"`
*   **Ejecución**: Tras confirmar en el diálogo, se invoca `apiService.enviarPerfilArea(this.perfilId)` (POST `/api/v1/perfiles/{id}/enviar-area`), el cual actualiza el estado en backend a `PERF-REV-AREA` ("En revisión área solicitante") y añade la trazabilidad histórica de envío correspondiente.

### I. Automatización de PDFs mediante Webhooks (Integración)
Antes de realizar la transición de estado al Área Solicitante, se deben generar automáticamente los PDFs correspondientes. Para ello, se realiza la invocación encadenada de dos webhooks de automatización desde el cliente:

1.  **A4 - Generador PDF resumen ejecutivo del rol**:
    *   **URL**: `https://nacional-seguros-dev.isia.cloud/webhook/A4_Genera_PDF_resumen_ejecutivo_del_rol`
2.  **A5 - Generador PDF del perfil estructurado completo**:
    *   **URL**: `https://nacional-seguros-dev.isia.cloud/webhook/ares-generador-pdf-perfil-estructurado`
*   [detalle-perfil.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/detalle-perfil/detalle-perfil.component.ts): Controla la lógica de permisos dinámicos, la visualización de botones de acción en la cabecera (excluyendo controles de edición cuando el tab es "Resumen ejecutivo"), e inicia de forma automática el formulario estructurado en modo edición si se accedió por la ruta de acción `/revisar` o `/corregir`.

---

## 9. Integración de Servicios Reales y Transiciones de Estados (Sprint 3 - Fase 3)

Se integraron las llamadas reales al backend utilizando `ApiService` para la ejecución de transiciones de negocio del perfil de cargo, eliminando simulaciones locales de estado en el detalle del perfil.

### A. Endpoints Reales Utilizados
*   **Envío al Área**: `apiService.enviarPerfilArea(id)` (POST `/api/v1/perfiles/{id}/enviar-area`). Invocado desde `enviarAlArea()` y `enviarCorreccionesAlArea()`.
*   **Atención de Observaciones**: `apiService.atenderObservaciones(id)` (POST `/api/v1/perfiles/{id}/atender-observaciones`). Invocado por RRHH al hacer clic en `"Atender observaciones"` en el estado `PERF-OBS-AREA` para transitar el perfil a `PERF-COR-RRHH`.
*   **Aprobación del Solicitante**: `apiService.aprobarSolicitante(id)` (POST `/api/v1/perfiles/{id}/aprobar-solicitante`). Invocado por el Área Solicitante en el diálogo de confirmación de aprobación.
*   **Aprobación Final**: `apiService.aprobarFinal(id)` (POST `/api/v1/perfiles/{id}/aprobar-final`). Invocado por RRHH en la aprobación final definitiva.

### B. Ciclo de Vida del Cambio de Estado
Tras cualquier acción exitosa de transición, el frontend:
1.  Invoca el endpoint real de la API.
2.  Llama a `cargarDatos()`, que refesca los datos haciendo un `GET /api/v1/perfiles/{id}`.
3.  Actualiza el estado y los controles dinámicos de forma exclusiva a partir de la respuesta real devuelta por el servidor.

### C. Integración de Observaciones Reales
Se conectó `ObservacionesPerfilService` directamente a los endpoints de backend:
*   `obtenerTiposObservacionActivos()` $\rightarrow$ Consume `apiService.getTiposObservacion(true)` y lo mapea al modelo `TipoObservacion` local.
*   `obtenerObservacionesPerfil(idPerfil)` $\rightarrow$ Obtiene las observaciones directamente del perfil compuesto haciendo `apiService.getPerfilById(idPerfil)`.
*   `agregarObservacionPerfil(...)` $\rightarrow$ Registra una observación en el backend con `apiService.registrarObservacion(idPerfil, idTipoObservacion, comentario)`.

### D. Traducción Dinámica Basada en Rol
Para cumplir con las restricciones de enmascaramiento visual:
*   El componente `ProfileStatusBadgeComponent` recibe el rol activo del usuario (`[rol]="userRole"` o `[rol]="rol"`).
*   Si el rol es `AreaSol` y el estado real del perfil es `PERF-OBS-AREA` o `PERF-COR-RRHH`, se traduce visualmente a `"En revisión de Recursos Humanos"` (con su correspondiente diseño de insignia `"badge-review"`).
*   Para RRHH y Administrador, dichos estados se muestran en su denominación técnica original: `"Observado por el Área Solicitante"`.


## 10. Representación Visual del Perfil Estructurado y Guardado Secuencial (Sprint 3 - Ajuste Controlado)

Se completó y corrigió la visualización del Perfil estructurado del cargo alineándolo al diseño físico de 12 acordeones y controlando la edición de campos.

### A. Alineación de los 12 Acordeones Visuales
Se rediseñó el mapeo dinámico en `PerfilesService.mapEstructuradoToSecciones` para ajustarlo exactamente a las especificaciones del cliente:
1.  **Datos generales del cargo**: Nombre del cargo (SV), Área solicitante (SV), Regional/ciudad (SV), Reporta a (RRHH), Tipo de posición (SV), Modalidad (SV), Banda salarial (RRHH). (Se eliminó visualmente el campo duplicado de Gerencia).
2.  **Objetivo principal del cargo**: Objetivo principal (SV).
3.  **Perfil requerido**: Formación académica (SV), Experiencia requerida (SV - combina experiencia mínima, indispensable y valorada).
4.  **Conocimientos técnicos requeridos**: Conocimientos técnicos (SV).
5.  **Herramientas y sistemas**: Herramientas y sistemas (RRHH - editable).
6.  **Funciones principales del cargo**: Funciones principales (RRHH - editable).
7.  **Competencias clave**: Competencias clave (SV).
8.  **Indicadores de éxito del cargo**: Indicadores de éxito (RRHH - editable).
9.  **Perfil ideal del candidato**: Descripción del perfil ideal (RRHH - editable).
10. **Filtros clave para selección**: Criterios excluyentes sugeridos (SV), Criterios deseables (SV).
11. **Matriz sugerida de ponderación**: Matriz sugerida (RRHH - editable).
12. **Perfil tipo de alto ajuste**: Perfil tipo de alto ajuste (RRHH - editable).

### B. Mapeo Resiliente a Estructuras del Backend
El mapeo de datos soporta de manera nativa y transparente dos estructuras del DTO devueltas por el servidor según la fase de generación del perfil:
*   **Fase No Generada (Datos iniciales de Solicitud)**: Lee de las propiedades `perfilEstructurado.datosGenerales`, `perfilEstructurado.perfilRequerido` y `perfilEstructurado.condicionesVacante`.
*   **Fase Generada (Datos consolidados)**: Lee de las propiedades `perfilEstructurado.datosGeneralesCargo`, `perfilEstructurado.perfilRequerido`, `perfilEstructurado.herramientasSistemas`, `perfilEstructurado.conocimientosTecnicosRequeridos`, `perfilEstructurado.funcionesPrincipalesCargo`, `perfilEstructurado.competenciasClave`, `perfilEstructurado.indicadoresExitoCargo`, `perfilEstructurado.matrizPonderacion`, `perfilEstructurado.perfilIdealCandidato` y `perfilEstructurado.perfilTipoAltoAjuste`.

### C. Estrategia de Guardado Secuencial (Concurrencia de Versiones)
Debido a que el backend implementa un control estricto de concurrencia optimista y versionamiento del perfil (donde cada actualización exitosa a una sección desactiva el registro actual y genera una nueva versión con un `PerfilId` incremental diferente):
1.  **Ejecución en Serie (No Paralela)**: Las secciones modificadas por RRHH se guardan secuencialmente una a la vez utilizando llamadas ordenadas en cadena a `apiService.actualizarPerfilSeccion(...)`.
2.  **Propagación de Versión Activa**: Cada respuesta exitosa de guardado retorna la nueva cabecera activa y su ID actualizado (`res.perfilId`). El componente de frontend intercepta este ID y lo utiliza inmediatamente como el identificador dinámico de destino para la llamada de la siguiente sección en la cola.
3.  **Actualización de URL**: Al finalizar todo el guardado en cadena, se actualiza el URL del navegador (`/perfiles/{nuevoId}`) utilizando `replaceUrl: true` para mantener la consistencia al recargar o refrescar.

### D. Tabla de Secciones Técnicas y Payloads Reales

| Campo Visual | Nro. Sección Backend | Tipo Contenido Backend | Estructura / Ejemplo de Payload de Contenido |
| :--- | :---: | :--- | :--- |
| **Reporta a / Banda salarial** | **3** | Plain string | `"Reporta a: Gerente de Tecnología\nBanda Salarial/Presupuesto: 15000 BOB"` |
| **Funciones principales** | **7** | JSON Array string | `"[\"Diseñar arquitecturas\", \"Realizar revisiones de código\"]"` |
| **Perfil ideal** | **10** | JSON Array string | `"[\"Profesional autónomo y de alto nivel de auto-aprendizaje\"]"` |
| **Herramientas / KPIs** | **12** | JSON Object string | `"{\"Herramientas\":[\"Visual Studio\",\"Docker\"],\"Kpis\":[\"Despliegues sin errores\"],\"Riesgos\":[],\"ObservacionesIA\":\"\"}"` |
| **Matriz de ponderación** | **8** | JSON Array string | `"[\"Matriz: 40% técnica, 40% experiencia, 20% cultura\"]"` |
| **Perfil tipo alto ajuste** | **11** | JSON Array string | `"[\"Ingeniero Backend Senior con más de 5 años de trayectoria\"]"` |

*Nota*: Cada petición HTTP PUT envía a `/api/v1/perfiles/{id}/secciones/{numSeccion}` un cuerpo con el siguiente formato:
```json
{
  "contenido": "[Contenido formateado según la tabla anterior]",
  "motivo": "Ajuste del Perfil estructurado desde Backoffice"
}
```

### E. Detección de Deltas para Guardados Innecesarios
Antes de iniciar la serie de peticiones HTTP, el frontend realiza una validación comparativa local entre el valor de edición actual del formulario y el valor cargado inicialmente en el perfil (recuperado de `getInitialValue(fieldId)`):
*   Los arreglos multilínea (herramientas, kpis, funciones) se comparan extrayendo, recortando espacios (`trim()`) y descartando líneas vacías mediante la función auxiliar `areListsDifferent(val1, val2)`.
*   Si una sección técnica no presenta diferencias con respecto a los datos cargados originalmente de base de datos, es excluida de la lista de envíos.
*   Si la lista total de cambios es vacía (`updates.length === 0`), se notifica al usuario que *"No existen cambios para guardar"* y se cancela la secuencia sin efectuar llamadas HTTP.

### F. Comportamiento Ante Fallo Parcial
Si ocurre un error de comunicación o rechazo del servidor en alguna de las llamadas de la serie de secciones:
1.  **Parada Inmediata**: La recursión secuencial se interrumpe y se descartan las llamadas pendientes de las siguientes secciones en cola.
2.  **Alineación de Versión**: Se actualiza la propiedad de ID física local `this.perfilId` y la URL del navegador al identificador de la última versión exitosa del perfil (`currentId`) utilizando `{ replaceUrl: true }`.
3.  **Refresco Forzado**: Se realiza un GET `/api/v1/perfiles/{lastSuccessfulId}` a través de `cargarDatos()` para sincronizar los datos de la UI con la última versión guardada físicamente en el servidor.
4.  **Notificación**: Se muestra una alerta nativa indicando el fallo parcial, el nombre amigable de la sección que falló y la redirección a la última versión segura.

### G. Limitaciones No Probadas
Debido a la ausencia de un navegador interactivo en esta fase final por problemas de MCP locales, las llamadas secuenciales reales PUT a los endpoints `/api/v1/perfiles/{id}/secciones/{numeroSeccion}` y la navegación recursiva no se probaron dinámicamente frente al servidor de desarrollo con interacciones de usuario, por lo que su correcto funcionamiento final queda sujeto a pruebas manuales en el despliegue de desarrollo.


## 11. Auditoría y Verificación de Integración del Perfil Estructurado (Sprint 3)

Esta sección documenta formalmente la auditoría estática y el estado de la integración real del Perfil estructurado con el backend.

### A. Cadena de Lectura de Datos
El flujo de inicialización y renderizado de un perfil estructurado sigue la siguiente cadena de llamadas 100% conectada a la API real del backend:
1.  **Listado de perfiles** (`profile-table.component.html`): Al pulsar en "Revisar perfil" o "Corregir perfil", se emite la acción en base al identificador real e inmutable `profile.id` (el `PerfilCargoId` primario autoincremental de la base de datos).
2.  **Ruta Angular**: Redirección a `/perfiles/:id` o `/perfiles/:id/revisar`.
3.  **Carga del Detalle** (`detalle-perfil.component.ts`):
    *   Lee el ID numérico de la ruta: `this.perfilId = Number(this.route.snapshot.paramMap.get('id'));`
    *   Invoca: `this.perfilesService.obtenerPerfilPorId(this.perfilId)`
4.  **Servicio de Perfiles** (`perfiles.service.ts`):
    *   Método: `obtenerPerfilPorId(id)`
    *   Delegación a API: `this.apiService.getPerfilById(id)` (Llama al endpoint GET `/api/v1/perfiles/{id}`).
5.  **Mapeador Resiliente** (`mapDetailToFrontend(p)`):
    *   Traduce el DTO de respuesta `PerfilDetail` y lo convierte a `DetallePerfil`.
    *   Puebla los campos en el array `secciones` (12 acordeones estructurados) usando el método `mapEstructuradoToSecciones(p)` que soporta tanto perfiles generados como solicitudes de vacante no consolidadas.

### B. Tabla Completa de Mapeo Real (DTO -> 12 Secciones Visuales)

| Sección Visual | Campo | Propiedad del DTO Backend (`PerfilDetail`) | Tipo de Dato en el DTO | Transformación Aplicada | Origen Real de Datos |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **1. Datos generales** | Cargo | `p.cargo` o `est.datosGeneralesCargo.cargo` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Área | `p.areaSolicitante` o `est.datosGeneralesCargo.area` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Regional/ciudad | `est.datosGeneralesCargo.regional` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Reporta a | `est.datosGeneralesCargo.reportaA` o `reporta_a` | string | `parseStringOrArray()` | `AJUSTADO_RRHH` (Editable) |
| **1. Datos generales** | Tipo posición | `est.condicionesVacante.seniority` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Modalidad | `est.condicionesVacante.modalidadTrabajo` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **1. Datos generales** | Banda salarial | `resumenEjecutivo.bandaSalarial` | string | `parseStringOrArray()` | `AJUSTADO_RRHH` (Editable) |
| **2. Objetivo** | Objetivo principal | `est.objetivoPrincipalCargo` o `pr.objetivoCargo` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **3. Perfil requerido** | Formación académica| `est.perfilRequerido.estudios` o `pr.formacionAcademica` | string | `parseStringOrArray()` | `SOLICITUD_VACANTE` |
| **3. Perfil requerido** | Experiencia requerida| `est.perfilRequerido.anosExperiencia` | string / number | Traducido a *"X años de experiencia"* | `SOLICITUD_VACANTE` |
| **4. Conocimientos** | Conocimientos técnicos| `est.conocimientosTecnicosRequeridos` | string[] / string | `splitTextToList()` | `SOLICITUD_VACANTE` |
| **5. Herramientas** | Herramientas y sistemas| `est.herramientasSistemas` | string[] / object | Desglose de claves de objeto o texto | `AJUSTADO_RRHH` (Editable) |
| **6. Funciones** | Funciones principales | `est.funcionesPrincipalesCargo` | string[] / string | `splitTextToList()` | `AJUSTADO_RRHH` (Editable) |
| **7. Competencias** | Competencias clave | `est.competenciasClave` | string[] / string | `splitTextToList()` | `SOLICITUD_VACANTE` |
| **8. Indicadores** | Indicadores de éxito | `est.indicadoresExitoCargo` | string[] / string | `splitTextToList()` | `AJUSTADO_RRHH` (Editable) |
| **9. Perfil ideal** | Perfil ideal | `est.perfilIdealCandidato` | string[] / string | `parseStringOrArray()` | `AJUSTADO_RRHH` (Editable) |
| **10. Filtros clave** | Criterios excluyentes | `est.filtrosClaveSeleccion` | string[] / object | Desglose de claves de objeto o texto | `SOLICITUD_VACANTE` |
| **10. Filtros clave** | Criterios deseables | `est.perfilRequerido.criteriosDeseables` | string[] / string | `splitTextToList()` | `SOLICITUD_VACANTE` |
| **11. Matriz** | Matriz ponderación | `est.matrizPonderacion` | array / string | Formateado a *"criterio: peso%"* | `AJUSTADO_RRHH` (Editable) |
| **12. Perfil alto ajuste**| Perfil tipo alto ajuste | `est.perfilTipoAltoAjuste` | string[] / string | `parseStringOrArray()` | `AJUSTADO_RRHH` (Editable) |

### C. Servicios de Lectura y Guardado Utilizados
*   **Lectura**: `apiService.getPerfilById(id)` (GET `/api/v1/perfiles/{id}`).
*   **Guardado**: `apiService.actualizarPerfilSeccion(perfilId, numeroSeccion, { contenido, motivo })` (PUT `/api/v1/perfiles/{id}/secciones/{numeroSeccion}`).
*   **Versionamiento**: Confirmado en el handler del backend. Cada guardado de sección devuelve un nuevo `perfilId` (clave primaria incremental del nuevo registro inmutable) y actualiza `versionActiva`.

### D. Regla de Guardado Incremental y Recarga Posterior
1.  El formulario reactivo se mapea con los campos iniciales del perfil mediante `getInitialValue(campoId)`.
2.  Al hacer clic en "Guardar ajustes", el frontend compara dinámicamente los valores actuales con los iniciales mediante delta check.
3.  Solo las secciones técnicas con cambios detectados reales se agregan a la cola de peticiones.
4.  Si la cola está vacía, no se ejecuta ninguna llamada HTTP.
5.  Si hay cambios, se ejecutan en serie secuencial enviando la petición del nuevo ID de perfil versión (`currentId`) devuelto en el DTO de respuesta anterior.
6.  Al terminar la secuencia completa, la URL del navegador se reescribe de forma transparente con el ID final vigente (`replaceUrl: true`) y se ejecuta `cargarDatos()` que hace un GET del nuevo estado del perfil persistido, cerrando el modo de edición y asegurando que la UI dibuje exclusivamente la información retornada del servidor.

### F. Resolución del Bug en Detección de Cambios de Formulario (Form dirty check)
Anteriormente, el botón `Guardar ajustes` se mantenía inactivo o la vista de edición no reaccionaba a los cambios del usuario porque la propiedad `FormGroup.dirty` de Angular es una propiedad ordinaria y no un Signal. Al estar contenida dentro del computado `permisos()`, Angular no detectaba su cambio, por lo que el botón nunca cambiaba su estado de deshabilitación de forma reactiva.
*   **Solución**: Se implementó el Signal reactivo `formTieneCambios = signal(false)` en `detalle-perfil.component.ts`.
*   Al inicializarse el formulario, se realiza una suscripción dinámica:
    ```typescript
    this.perfilForm.valueChanges.subscribe(() => {
      this.formTieneCambios.set(this.perfilForm.dirty);
    });
    ```
*   La propiedad `puedeGuardar` del computado ahora depende directamente del Signal `formTieneCambios()`, asegurando una actualización visual reactiva e inmediata en cuanto el usuario modifica cualquier campo.
*   Se añadieron salvaguardas durante el guardado para deshabilitar los botones de Guardar y Cancelar si `isLoading()` es `true`, evitando llamadas concurrentes duplicadas.

### G. Flujo de Estados y Correcciones en PERF-OBS-AREA (Observado por el Área Solicitante)
De acuerdo a las reglas de negocio del backend, Recursos Humanos no puede modificar ni guardar cambios directamente si el perfil se encuentra en el estado `PERF-OBS-AREA` (código visual de frontend: `EstadoPerfil.Observada`).
*   **Corrección**: Se ajustó la condición `canEditState` en `cargarDatos()` para excluir `EstadoPerfil.Observada` del inicio automático del modo de edición.
*   **Flujo Restablecido**: En `PERF-OBS-AREA`, el profesional de RRHH visualiza el panel de observaciones del área y dispone del botón de acción `"Atender observaciones"`. Al pulsarlo, se llama a la API real `/atender-observaciones`, la cual cambia el estado del perfil en base de datos a `PERF-COR-RRHH` (código visual: `EstadoPerfil.Corregida`). Solo una vez que el perfil se encuentra en estado `Corregida`, se habilita el botón `"Editar perfil"`, permitiendo aplicar las correcciones estructuradas y guardarlas secuencialmente con éxito.

### H. Configuración del Botón "Enviar al Área Solicitante"
Para unificar los flujos de envío tras revisiones iniciales y correcciones de observaciones, se integró el botón `"Enviar al Área Solicitante"` (ID: `'enviarArea'`) en los estados clave de revisión:
*   **Estados Permitidos**:
    *   `EstadoPerfil.EnRevisionRRHHPE` (En revisión RRHH)
    *   `EstadoPerfil.ResumenEjecutivoGenerado` (Resumen ejecutivo generado)
    *   `EstadoPerfil.Corregida` (En corrección RRHH)
*   **Diálogo de Confirmación**: Al presionar el botón, se despliega el componente de diálogo `DialogoConfirmacionComponent` con la siguiente parametrización técnica:
    *   `titulo`: `"Enviar al Área Solicitante"`
    *   `descripcion`: `"¿Deseas enviar este perfil al Área Solicitante? Una vez enviado, el área solicitante podrá revisar la información y aprobar u observar el perfil."`
    *   `icono`: `'send'`
    *   `variante`: `'advertencia'`
    *   `requiereCheckbox`: `false`
    *   `textoBotonPrincipal`: `"Enviar al Área Solicitante"`
    *   `textoBotonCancelar`: `"Cancelar"`

### I. Automatización de PDFs mediante Webhooks (Integración)
Se deben generar automáticamente los PDFs correspondientes antes de enviar.

#### Payload Común Enviado
```json
{
  "perfilId": 12,
  "codigoPerfil": "PRF-SOL-2026-0204",
  "solicitudId": 34,
  "codigoSolicitud": "SOL-2026-0204",
  "estadoActual": "PERF-REV-RRHH",
  "accion": "ENVIAR_AREA_SOLICITANTE",
  "usuarioAccion": {
    "id": "1",
    "nombre": "Analista de RRHH",
    "rol": "RRHH"
  },
  "fechaAccion": "2026-07-15T01:17:00.000Z"
}
```

#### Secuencia de Ejecución (Orden Final de Llamadas)
1.  El usuario RRHH presiona "Enviar al Área Solicitante" y confirma la acción en el modal.
2.  El modal reutilizable `DialogoConfirmacionComponent` intercepta el click y establece `this.isLoading = true` (spinner `"Procesando..."` interno).
3.  El callback `onConfirmar()` retorna una tubería de observables encadenados con `concatMap`, centralizando el ciclo de vida en la suscripción única del diálogo.
4.  **Validación y Guardado de Resumen Ejecutivo (Condicional)**:
    *   Se consulta el DTO del perfil `apiService.getPerfilById(perfilId)`.
    *   Si el perfil se encuentra en estado inicial `PERF-REV-RRHH`, se ejecuta la llamada a `apiService.guardarResumen(perfilId, updatePayload)` con los datos del perfil estructurado actualizados. Esto realiza la transición de estado requerida en base de datos a `PERF-RES-GEN` ("Resumen ejecutivo generado").
5.  Se ejecuta el webhook **A4** (PDF Resumen Ejecutivo) con un `timeout(15000)`.
6.  Se ejecuta el webhook **A5** (PDF Perfil estructurado completo) con un `timeout(15000)`.
7.  Se ejecuta `apiService.enviarPerfilArea(perfilId)` (que ahora es admitido exitosamente dado que el perfil se encuentra en `PERF-RES-GEN` o `PERF-COR-RRHH`).
8.  Se ejecuta la recarga `perfilesService.obtenerPerfilPorId(perfilId)`.
9.  En caso de éxito, el diálogo cierra automáticamente el modal (`dialogRef.close(true)`) tras finalizar la tubería.

#### Manejo de Errores y Causa Real del Bloqueo / 400 Bad Request
*   **Causa del 400 Bad Request en secciones**: El backend de actualización de secciones (`ActualizarPerfilSeccionCommandHandler`) valida estrictamente que el estado del perfil sea `"EnRevisionRRHH"` o `"PerfilCorregidoRRHH"` (códigos de estado de Solicitudes, ID 31 o 32). Al intentar guardar secciones en perfiles cuyo estado fuera `"PERF-REV-RRHH"` (código de estado de Perfiles, ID 1039), se retornaba un 400.
*   **Causa del 400 Bad Request en Enviar Área**: El comando `EnviarAreaPerfilCommand` del controlador plural (`PerfilesController.cs` / `POST /api/v1/perfiles/{id}/enviar-area`) valida que el perfil esté en estado `"PERF-RES-GEN"` o `"PERF-COR-RRHH"`. Si el perfil se enviaba directamente estando en `"PERF-REV-RRHH"`, se rechazaba con 400 Bad Request.
*   **Solución en Frontend**:
    1.  Se refactorizó `onConfirmar()` para retornar un único pipeline reactivo encadenado con operadores `concatMap`, `tap` y `finalize`.
    2.  Si el estado del perfil es `"PERF-REV-RRHH"`, se antepone secuencialmente la petición `guardarResumen(...)` antes de los webhooks y de `/enviar-area`, lo cual desplaza automáticamente el estado en base de datos a `"PERF-RES-GEN"`.
    3.  Se añadió el operador `timeout(15000)` a las llamadas de A4 y A5. Si un webhook supera los 15 segundos en `pending`, se interrumpe y se lanza un error controlado.
    4.  Se implementó una extracción dinámica de errores del backend (`err?.error?.message || err?.error?.Message`) para notificar al usuario el motivo preciso del rechazo.
    5.  Se implementó `finalize()` en el observable dentro de `DialogoConfirmacionComponent` para garantizar que la variable interna `this.isLoading` que controla `"Procesando..."` se restablezca a `false` en caso de error, liberando el diálogo para que el usuario pueda corregir, reintentar o cancelar.
    6.  Se implementó `finalize()` en el pipeline del componente padre para asegurar que la señal de carga del detalle del perfil `isLoading.set(false)` siempre se restablezca.

#### Restricciones tras el Envío (Estado: `PERF-REV-AREA`)
Una vez que el perfil transita a `PERF-REV-AREA` ("En revisión del Área Solicitante"), se aplican las siguientes reglas de solo lectura para RRHH:
*   Se ocultan las acciones de `"Editar perfil"`, `"Guardar ajustes"`, `"Cancelar"` y `"Enviar al Área Solicitante"`.
*   El Perfil estructurado queda completamente bloqueado para modificaciones.
*   El Área Solicitante mantiene habilitadas sus acciones correspondientes de `"Aprobar perfil"` y `"Enviar observaciones"`.

---

## 12. Acceso al Detalle y Aprobación Final para RRHH (Estado: `PERF-APR-AREA`)

Se resolvió la integración del estado `PERF-APR-AREA` ("Aprobado por el Área Solicitante") para permitir que el rol RRHH/Administrador pueda visualizar de forma completa el perfil en solo lectura y efectuar la aprobación final desde la vista de detalle.

### A. Diagnóstico de la Causa Raíz
*   **Problema original**: Al hacer clic en la acción de la tabla del listado de perfiles para un perfil en estado `"Aprobado por el Área Solicitante"`, el sistema intentaba invocar directamente el endpoint de aprobación final (`aprobarPerfilFinal()`) a través del servicio local de listado, sin navegar a la ruta del detalle. Esto bloqueaba la posibilidad de que RRHH revisara visualmente la información estructurada y el resumen del perfil antes de realizar la acción definitiva.

### B. Solución y Enrutamiento Aplicados
1.  **Cambio de Acción en Listado**:
    *   En [profile-table.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/components/profile-table/profile-table.component.ts), se cambió la etiqueta del botón de la fila para `ProfileStatus.Aprobada` a `"Revisar y aprobar final"`, asignándole la acción `'revisar_aprobar_final'`.
    *   En [perfiles-list.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/perfiles-list/perfiles-list.component.ts), al recibir la acción `'revisar_aprobar_final'`, el frontend redirige al usuario utilizando la ruta del detalle existente: `/perfiles/{id}`.
2.  **Permisos y Visualización en Detalle**:
    *   Cuando el perfil está en `PERF-APR-AREA`, se muestran las pestañas de `"Resumen ejecutivo"` y `"Perfil estructurado"` bloqueadas en modo **Solo Lectura** (los controles de `"Editar perfil"`, `"Guardar ajustes"`, `"Cancelar"`, `"Enviar al Área Solicitante"` y los paneles de observaciones están ocultos).
    *   Se muestra de forma destacada el botón `"Aprobar perfil final"`.
3.  **Flujo y Servicio de Aprobación Final**:
    *   Al hacer clic en `"Aprobar perfil final"`, se despliega el modal de confirmación `DialogoConfirmacionComponent` con la advertencia de que la acción es definitiva.
    *   Se consume el servicio real `apiService.aprobarFinal(perfilId)` (que ejecuta un HTTP `POST /api/v1/perfiles/{perfilId}/aprobar-final`).
    *   Una vez que el backend responde de forma exitosa confirmando el estado `"PERF-APR-FIN"`, se recarga la información, se cierra el diálogo, se despliega una notificación de éxito y la pantalla entera queda bloqueada en modo de solo lectura definitivo (ocultando el botón de aprobación).
4.  **Manejo de Errores**:
    *   Si el backend rechaza la transacción con un error, se detiene el spinner del modal, manteniendo el diálogo abierto y permitiendo al usuario "Cancelar" o "Reintentar".

### C. Archivos Modificados
*   [profile-table.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/components/profile-table/profile-table.component.ts)
*   [perfiles-list.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/perfiles-list/perfiles-list.component.ts)
*   [detalle-perfil.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/detalle-perfil/detalle-perfil.component.ts)
*   [KB_23_PerfilCargo_Sprint_2026.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_23_PerfilCargo_Sprint_2026.md)

---

## 13. Diagnóstico y Corrección — Guardado del Perfil Estructurado (Sprint 3 - Cierre)

Se diagnosticaron y corrigieron las causas de rechazo del backend al intentar guardar las secciones técnicas editadas del Perfil estructurado, logrando la persistencia exitosa para los estados válidos de RRHH.

### A. Diagnóstico de la Causa Raíz
1.  **Divergencia de Códigos de Estado en Backend**: El comando de actualización de sección (`ActualizarPerfilSeccionCommandHandler.cs`) comprobaba de manera estricta que el código de estado del perfil fuera `"EnRevisionRRHH"` (ID 31) o `"PerfilCorregidoRRHH"` (ID 32). Sin embargo, al guardar secciones sobre perfiles que iniciaron mediante automatizaciones o transiciones estructuradas, la entidad tiene el estado `"PERF-REV-RRHH"` (ID 1039) o `"PERF-COR-RRHH"` (ID 1043). Al no coincidir los códigos, el backend rechazaba la acción devolviendo el error `PerfilCargo.InvalidState`.
2.  **Omisión de Restricción de Roles en Endpoint**: El endpoint `HttpPut("perfiles/{id:int}/secciones/{numeroSeccion:int}")` en `PerfilController.cs` carecía de restricciones de rol, utilizando una directiva general `[Authorize]`.

### B. Correcciones Aplicadas
1.  **Backend (ActualizarPerfilSeccionCommandHandler.cs)**:
    *   Se amplió la validación de estados para contemplar la coexistencia de ambos flujos en paralelo. El condicional ahora permite la modificación en los 4 códigos de estado válidos de revisión de Recursos Humanos:
        ```csharp
        if (latestPerfil.Estado?.Codigo != "EnRevisionRRHH" && 
            latestPerfil.Estado?.Codigo != "PerfilCorregidoRRHH" && 
            latestPerfil.Estado?.Codigo != "PERF-REV-RRHH" && 
            latestPerfil.Estado?.Codigo != "PERF-COR-RRHH")
        ```
2.  **Backend (PerfilController.cs)**:
    *   Se restringió el endpoint de actualización de secciones especificando los roles autorizados: `[Authorize(Roles = "Administrador,RRHH")]`.
3.  **Frontend (Manejo de Errores no Nativo)**:
    *   Se eliminó el `alert()` nativo en `guardarAjustes()` para errores de red o rechazos.
    *   Se agregó el componente de banner de error `.banner-error-detalle` (color rojo) en el HTML del detalle controlado por la señal `errorMessageDetalle`.
    *   Se implementó una distinción lógica:
        *   Si el error ocurre en la **primera sección en cola** (`index === 0`), se infiere un rechazo de estado o versión inactiva y se muestra: `"No fue posible guardar los cambios porque el perfil no se encuentra en una versión o estado editable. La información será actualizada."`.
        *   Si el error ocurre a mitad de camino (`index > 0`), se notifica de forma no nativa un guardado parcial y se re-direcciona.
    *   En cualquiera de los casos de fallo, se detiene la secuencia, se apaga el spinner `isLoading` y se ejecuta `cargarDatos()` refrescando el detalle con el ID de la última versión activa de la base de datos.
4.  **Frontend (Limpieza de Servicio)**:
    *   Se eliminó el método duplicado `registrarObservacion` en `api.service.ts` que provocaba un error de compilación por firma duplicada.

### C. Archivos Modificados
*   **Backend**:
    *   [ActualizarPerfilSeccionCommandHandler.cs](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/src/NacionalSeguros.Application/Perfiles/Commands/ActualizarPerfilSeccion/ActualizarPerfilSeccionCommandHandler.cs)
    *   [PerfilController.cs](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/src/NacionalSeguros.Api/Controllers/PerfilController.cs)
*   **Frontend**:
    *   [api.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/core/services/api.service.ts)
    *   [detalle-perfil.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/detalle-perfil/detalle-perfil.component.ts)
    *   [detalle-perfil.component.html](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/detalle-perfil/detalle-perfil.component.html)
    *   [detalle-perfil.component.scss](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/detalle-perfil/detalle-perfil.component.scss)
    *   [KB_23_PerfilCargo_Sprint_2026.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_23_PerfilCargo_Sprint_2026.md)

---

## 14. Máquina de Estados y Matriz de Permisos por Rol (Sprint 3 - Final)

Se consolidó y documentó la máquina de estados del Perfil de Cargo, garantizando la total coherencia en las transiciones de estado, permisos por rol, etiquetas visuales personalizadas y validaciones en ambos extremos de la aplicación.

### A. Diagrama de Transiciones de Estado
```mermaid
stateDiagram-v2
    [*] --> PERF-PEN-GEN : Solicitud aprobada
    PERF-PEN-GEN --> PERF-REV-RRHH : Generación IA finalizada
    PERF-REV-RRHH --> PERF-RES-GEN : Guardar/Generar Resumen ejecutivo
    PERF-RES-GEN --> PERF-REV-AREA : Enviar al Área Solicitante
    
    PERF-REV-AREA --> PERF-APR-AREA : Solicitante aprueba
    PERF-REV-AREA --> PERF-OBS-AREA : Solicitante observa
    
    PERF-OBS-AREA --> PERF-COR-RRHH : RRHH atiende observaciones
    PERF-COR-RRHH --> PERF-RES-GEN : RRHH guarda y regenera Resumen
    
    PERF-APR-AREA --> PERF-APR-FIN : RRHH aprueba final
    PERF-APR-FIN --> [*]
```

### B. Mappings y Visibilidad de Estados por Rol
Para no confundir al usuario final con estados técnicos transitorios, el frontend agrupa y renombra visualmente las etapas operativas según el rol del usuario autenticado:

| Código Técnico (DB) | Significado Operativo | Etiqueta Vista por RRHH | Etiqueta Vista por Solicitante |
| :--- | :--- | :--- | :--- |
| **`PERF-PEN-GEN`** | Generación de IA en proceso | Procesando (No editable) | Procesando (No editable) |
| **`PERF-REV-RRHH`** | Revisión Inicial de RRHH | En revisión de Recursos Humanos | En revisión de Recursos Humanos |
| **`PERF-RES-GEN`** | Resumen Ejecutivo consolidado | En revisión de Recursos Humanos | En revisión de Recursos Humanos |
| **`PERF-REV-AREA`** | Pendiente aprobación de Solicitante | En revisión del Área Solicitante | En revisión del Área Solicitante |
| **`PERF-OBS-AREA`** | Perfil Observado | Observado por el Área Solicitante | En revisión de Recursos Humanos |
| **`PERF-COR-RRHH`** | Corrección RRHH post-observación | Observado por el Área Solicitante / En corrección | En revisión de Recursos Humanos |
| **`PERF-APR-AREA`** | Perfil aprobado por Área | Aprobado por el Área Solicitante | Aprobado por el Área Solicitante |
| **`PERF-APR-FIN`** | Proceso finalizado y cerrado | Perfil aprobado final | Perfil aprobado final |

### C. Matriz de Permisos Operacionales por Estado

| Estado Técnico | Permisos Recursos Humanos (RRHH) | Permisos Área Solicitante |
| :--- | :--- | :--- |
| **`PERF-PEN-GEN`** | Solo lectura, barra de procesamiento. | Sin acciones visibles. |
| **`PERF-REV-RRHH`** | Revisar, editar, guardar y enviar al área. | Solo lectura. |
| **`PERF-RES-GEN`** | Estado transitorio de despacho al área. | Sin acciones. |
| **`PERF-REV-AREA`** | Solo lectura. | Revisar, registrar observaciones (observar), aprobar. |
| **`PERF-OBS-AREA`** | Ver observaciones, atender observaciones. | Solo lectura (Seguimiento). |
| **`PERF-COR-RRHH`** | Editar, guardar ajustes y reenviar. | Solo lectura (Seguimiento). |
| **`PERF-APR-AREA`** | Revisar y Aprobación Final. | Solo lectura. |
| **`PERF-APR-FIN`** | Solo lectura definitivo (Sin acciones). | Solo lectura definitivo (Sin acciones). |

### D. Contrato de Endpoints y Validaciones de Transición (Backend)

1.  **Actualizar Perfil Estructurado** (`PUT /api/v1/perfiles/{id}/secciones/{numeroSeccion}`)
    *   **Autorización**: `Administrador,RRHH`
    *   **Estados Permitidos**: `PERF-REV-RRHH`, `PERF-COR-RRHH`, `EnRevisionRRHH`, `PerfilCorregidoRRHH`
2.  **Registrar Observación** (`POST /api/v1/perfiles/{id}/observaciones`)
    *   **Autorización**: `Solicitante` (Debe ser el `SolicitanteId` asignado)
    *   **Estado Inicial**: `PERF-REV-AREA`
    *   **Estado Resultante**: `PERF-OBS-AREA`
3.  **Atender Observación** (`POST /api/v1/perfiles/{id}/atender-observaciones`)
    *   **Autorización**: `Administrador,RRHH`
    *   **Estado Inicial**: `PERF-OBS-AREA`
    *   **Estado Resultante**: `PERF-COR-RRHH` (Resuelve observaciones pendientes a Atendidas)
4.  **Enviar al Área Solicitante** (`POST /api/v1/perfiles/{id}/enviar-area`)
    *   **Autorización**: `Administrador,RRHH`
    *   **Estados Permitidos**: `PERF-RES-GEN`, `PERF-COR-RRHH`
    *   **Estado Resultante**: `PERF-REV-AREA`
5.  **Aprobar por Área Solicitante** (`POST /api/v1/perfiles/{id}/aprobar-solicitante`)
    *   **Autorización**: `Solicitante` (Debe ser el `SolicitanteId` asignado)
    *   **Estado Inicial**: `PERF-REV-AREA`
    *   **Estado Resultante**: `PERF-APR-AREA`
6.  **Aprobar Perfil Final** (`POST /api/v1/perfiles/{id}/aprobar-final`)
    *   **Autorización**: `Administrador,RRHH`
    *   **Estado Inicial**: `PERF-APR-AREA`
    *   **Estado Resultante**: `PERF-APR-FIN`

---

## 15. Corrección de la Validación de Edición de Perfiles por Recursos Humanos (Sprint 3 - Cierre Backend)

Se revisó y refinó la validación del backend que impedía guardar cambios en las secciones del Perfil estructurado para los estados de Recursos Humanos en el nuevo flujo de estados.

### A. Diagnóstico y Causa Raíz
*   **Problema de Validación de Estados**: Previamente, el handler de guardado `ActualizarPerfilSeccionCommandHandler.cs` permitía la edición contemplando estados antiguos (`EnRevisionRRHH` y `PerfilCorregidoRRHH`). Se amplió para aceptar de forma estricta los nuevos códigos de estado unificados: `PERF-REV-RRHH` (En revisión de Recursos Humanos) y `PERF-COR-RRHH` (En corrección por Recursos Humanos), bloqueando cualquier otro estado (ej. `PERF-REV-AREA`, `PERF-APR-AREA`, `PERF-APR-FIN`).
*   **Divergencia en Transición Inicial**: Cuando la automatización de IA creaba el perfil cargo por primera vez (`PERF-PEN-GEN`), el handler de callback `CrearPerfilCargoCommandHandler.cs` asignaba erróneamente el código antiguo `"EnRevisionRRHH"` (ID 31, de tipo solicitud). Se corrigió para que asigne y persista el estado correspondiente del perfil `"PERF-REV-RRHH"` (ID 1039).
*   **Validación de Generación del Perfil**: En `GenerarPerfilCommandHandler.cs`, la validación de perfil preexistente utilizaba comparaciones con IDs de estado hardcodeados (`2` y `3`), que representaban estados de solicitudes y no de perfiles de cargo. Se corrigió para comparar usando el código técnico `PERF-PEN-GEN`.

### B. Reglas y Controles Consolidados
*   **Estados Editables de Perfil**: Únicamente `PERF-REV-RRHH` y `PERF-COR-RRHH`.
*   **Roles Autorizados**: Recursos Humanos (`RRHH`) y `Administrador`.
*   **Comportamiento de Versión Activa (Inmutabilidad)**: Cada guardado de sección crea una nueva fila de `PerfilCargo` con `Version = VersionAnterior + 1` y marca la fila anterior como inactiva (`Activo = False`). El estado se copia exactamente igual al perfil anterior (ej. si era `PERF-REV-RRHH`, la nueva fila activa mantiene `PERF-REV-RRHH`).
*   **Transición Inicial**: `PERF-PEN-GEN` $\rightarrow$ `PERF-REV-RRHH` al finalizar la generación por IA y persistir en la base de datos.

### C. Archivos Modificados
*   [ActualizarPerfilSeccionCommandHandler.cs](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/src/NacionalSeguros.Application/Perfiles/Commands/ActualizarPerfilSeccion/ActualizarPerfilSeccionCommandHandler.cs)
*   [CrearPerfilCargoCommandHandler.cs](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/src/NacionalSeguros.Application/Perfiles/Commands/CrearPerfilCargo/CrearPerfilCargoCommandHandler.cs)
*   [GenerarPerfilCommandHandler.cs](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/src/NacionalSeguros.Application/Perfiles/Commands/GenerarPerfil/GenerarPerfilCommandHandler.cs)
*   [PerfilesController.cs](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/src/NacionalSeguros.Api/Controllers/PerfilesController.cs)

---

## 16. Unificación de Estados, Permisos y Validación de Área en el Frontend (Sprint 3 - Frontend)

Se ha realizado una alineación completa en el frontend para que el estado técnico retornado por el backend (`estadoCodigo`) sirva como única fuente de verdad técnica para las etiquetas de estado, permisos dinámicos, botones de acción en la cabecera y acciones de fila en el listado.

### A. Mapeo Técnico de Estados y Etiquetas Visuales por Rol

| Código Técnico (DB) | Etiqueta Vista por RRHH | Etiqueta Vista por Solicitante | Estilo Visual del Chip |
| :--- | :--- | :--- | :--- |
| **`PERF-PEN-GEN`** | Generando perfil | Generando perfil | Gris / Pendiente |
| **`PERF-REV-RRHH`** | En revisión RRHH | En revisión RRHH | Azul / En revisión |
| **`PERF-RES-GEN`** | En revisión RRHH | En revisión RRHH | Azul / En revisión |
| **`PERF-REV-AREA`** | En revisión solicitante | En revisión solicitante | Lila / badge-solicitante |
| **`PERF-OBS-AREA`** | Observado por solicitante | En revisión RRHH | Naranja / Observado (RRHH) / Azul (Sol) |
| **`PERF-COR-RRHH`** | Observado por solicitante | En revisión RRHH | Naranja / Observado (RRHH) / Azul (Sol) |
| **`PERF-APR-AREA`** | Aprobado por solicitante | Aprobado por solicitante | Verde / Aprobado |
| **`PERF-APR-FIN`** | Aprobado final | Aprobado final | Verde / Aprobado |

*Nota: Se unificó el estado `Corregida` (`PERF-COR-RRHH`) bajo la etiqueta visual `'Observado por solicitante'` para evitar textos excesivamente largos en la interfaz de RRHH.*

### B. Matriz de Acciones del Listado de Perfiles

Las acciones disponibles en cada fila del listado principal se calculan dinámicamente según el código técnico y el rol del usuario actual:

*   **Para Recursos Humanos (RRHH / Reclutador)**:
    *   `PERF-REV-RRHH` $\rightarrow$ `Revisar perfil`
    *   `PERF-OBS-AREA` $\rightarrow$ `Ver observaciones`
    *   `PERF-COR-RRHH` $\rightarrow$ `Corregir perfil`
    *   `PERF-REV-AREA` $\rightarrow$ `Ver seguimiento`
    *   `PERF-APR-AREA` $\rightarrow$ `Revisar y aprobar final`
    *   `PERF-APR-FIN` $\rightarrow$ `Ver detalle`
*   **Para Área Solicitante (Solicitante / AreaSol)**:
    *   `PERF-REV-AREA` $\rightarrow$ `Revisar perfil`
    *   `PERF-OBS-AREA` / `PERF-COR-RRHH` $\rightarrow$ `Ver seguimiento`
    *   `PERF-APR-AREA` / `PERF-APR-FIN` $\rightarrow$ `Ver detalle`

*Nota: La lista de perfiles se ordena descendentemente por `ultimaActualizacion` para mostrar los más recientes en la parte superior.*

### C. Validación Robusta del Área Solicitante
Para evitar que usuarios del Área Solicitante vean botones de acción o formularios que no les corresponden, se implementó el método `checkUserAreaMatch(perfil)`:
1.  **Comparación Técnica**: Compara el `areaId` de la sesión del usuario (`getUserAreaId()`) con el `areaId` del perfil.
2.  **Mapeo del Perfil**: En el detalle se carga el catálogo de áreas reales (`apiService.getAreas()`) y se resuelve el `areaId` del perfil a partir de su `areaNombre`.
3.  **Fallback Normalizado**: Si los IDs no están disponibles, realiza una comparación insensible a mayúsculas, minúsculas, acentos, espacios redundantes y abreviaciones.

### D. Flujo de Envío de Observaciones
El formulario de observaciones se muestra únicamente si el usuario pertenece al área del perfil y este se encuentra exactamente en `PERF-REV-AREA`. Al hacer clic en "Enviar observaciones":
1.  Se dispara el webhook de notificación de WhatsApp (`notifySolicitanteProfileUnderReview`).
2.  Se recarga la información del perfil del backend (lo que transiciona automáticamente el estado en base de datos a `PERF-OBS-AREA`).
3.  El formulario se oculta de inmediato y la vista del perfil se bloquea en solo lectura.

### E. Archivos Modificados (Frontend)
*   [auth.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/core/services/auth.service.ts)
*   [profile.model.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/models/profile.model.ts)
*   [profiles-list.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/services/profiles-list.service.ts)
*   [perfiles.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/servicios/perfiles.service.ts)
*   [profile-status.config.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/constants/profile-status.config.ts)
*   [configuracion-estados-perfil.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/constantes/configuracion-estados-perfil.ts)
*   [profile-status-badge.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/components/profile-status-badge/profile-status-badge.component.ts)
*   [profile-table.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/components/profile-table/profile-table.component.ts)
*   [profile-table.component.html](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/components/profile-table/profile-table.component.html)
*   [encabezado-perfil.component.html](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/componentes/encabezado-perfil/encabezado-perfil.component.html)
*   [perfiles-list.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/perfiles-list/perfiles-list.component.ts)
*   [detalle-perfil.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/detalle-perfil/detalle-perfil.component.ts)

---

## 10. Actualización: Aprobación del Área Solicitante y Aprobación Final de RRHH (Sprint Julio 2026)

### A. Corrección de la Transición de Estados
*   **Antes**: Al aprobar el perfil, `AprobarSolicitantePerfilCommand` cambiaba automáticamente el estado a `PERF-APR-AREA` y luego realizaba una transición automática e inmediata a `PERF-REV-RRHH` ("En revisión RRHH"). Esto impedía ver el estado de aprobación por el área en la UI y forzaba el estado a revisión nuevamente.
*   **Ahora**: Se eliminó la transición automática a `PERF-REV-RRHH` en el backend. El perfil permanece correctamente en el estado técnico **`PERF-APR-AREA`** tras la aprobación del solicitante.

### B. Mapeo y Visualización de Estados
*   **Etiqueta Central**: El código técnico `PERF-APR-AREA` ahora se mapea centralmente a **"Aprobado por el Área Solicitante"** con un **badge de color verde**.
*   **Archivos de configuración actualizados**:
    *   [profile-status.config.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/constants/profile-status.config.ts)
    *   [configuracion-estados-perfil.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/constantes/configuracion-estados-perfil.ts)
    *   [profile-status-badge.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/components/profile-status-badge/profile-status-badge.component.ts)

### C. Comportamiento en Detalle y Permisos
*   **Área Solicitante**:
    *   **Estado**: `PERF-APR-AREA`
    *   **Vista**: Solo lectura en ambos tabs.
    *   **Acciones ocultadas**: Botón "Aprobar perfil", formulario de registro de observaciones y botón "Enviar observaciones" ocultos por completo.
*   **Recursos Humanos (RRHH / Administrador)**:
    *   **Estado**: `PERF-APR-AREA`
    *   **Vista**: Solo lectura en ambos tabs.
    *   **Acciones habilitadas**: Botón **"Aprobar perfil final"** habilitado de forma activa.
    *   **Acciones ocultadas**: Botones "Editar perfil", "Guardar ajustes", "Cancelar", "Enviar al Área Solicitante" y "Atender observaciones" ocultos.

### D. Cambios en el Listado Principal
*   La columna **Estado** muestra correctamente **"Aprobado por el Área Solicitante"** con badge verde.
*   **Acciones de Tabla**:
    *   Para **RRHH**: Muestra la acción **"Revisar y aprobar final"** (redirige al detalle para procesar el cierre del flujo).
    *   Para **Área Solicitante**: Muestra la acción **"Ver detalle"** (vista de solo lectura).

### E. Habilitación de Aprobación Final en Backend
*   Se modificó el handler `AprobarFinalPerfilCommand` en [PerfilCommands.cs](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/src/NacionalSeguros.Application/Perfiles/Commands/PerfilCommands.cs) para validar y permitir la transición a `PERF-APR-FIN` cuando el perfil esté en el nuevo estado correcto `PERF-APR-AREA`.

### F. Archivos Modificados en este Ajuste
*   **Backend**:
    *   [PerfilCommands.cs](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/src/NacionalSeguros.Application/Perfiles/Commands/PerfilCommands.cs)
*   **Frontend**:
    *   [profile-status.config.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/constants/profile-status.config.ts)
    *   [configuracion-estados-perfil.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/constantes/configuracion-estados-perfil.ts)
    *   [profile-status-badge.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/components/profile-status-badge/profile-status-badge.component.ts)




## 11. Actualización: Trazabilidad Histórica del Perfil de Cargo (Sprint Julio 2026)

### A. Origen de Datos e Integración del Endpoint
*   Se conectó el servicio frontend [PerfilesService](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/servicios/perfiles.service.ts) con el endpoint del API del backend expuesto en `PerfilController.cs` bajo la ruta `GET /api/v1/perfiles/{id}/trazabilidad`.
*   El método `obtenerTrazabilidad(idPerfil)` en `PerfilesService` invoca `getTrazabilidadPerfil(idPerfil)` de `ApiService` y transforma el DTO del backend (`StateHistoryResponseDto[]`) al formato requerido por la interfaz visual (`EventoTrazabilidadPerfil[]`).

### B. Mapeo y Ordenamiento
1.  **Orden Cronológico**: Los eventos se ordenan en orden cronológico ascendente (el evento más antiguo primero en la parte superior y el más reciente al final, en la parte inferior).
2.  **Mapeo de Estados**: Traduce de forma robusta tanto los códigos técnicos de estado (como `PERF-REV-RRHH`, `PERF-COR-RRHH`, `PERF-REV-AREA`) como sus nombres literales en español al enum `EstadoPerfil` para que la UI los muestre con nombres legibles y entendibles.
3.  **Clasificación de Actores**:
    *   `AGENTE_IA`: Asignado cuando el rol es `n8n` / `n8n_automation` o el nombre incluye "bot" o "ia". Muestra el ícono `smart_toy`.
    *   `SISTEMA`: Asignado para roles de sistema (`System`, `Sistema`). Muestra el ícono `settings_suggest`.
    *   `USUARIO`: Asignado para usuarios físicos (como analistas de RRHH o jefes del área solicitante). Muestra el ícono `person`.

### C. Cálculo de Iteraciones
*   La iteración se deriva dinámicamente y de forma controlada a partir de los ciclos reales del perfil:
    *   Se inicializa en `1`.
    *   Se incrementa en `1` cada vez que el perfil entra en estado de corrección (`PERF-COR-RRHH` o `Corregida`), marcando el inicio de un nuevo ciclo de revisión.
    *   Permite conservar cada ciclo de iteración e histórico sin sobrescribir los eventos anteriores del perfil.

### D. Actualización Dinámica
*   Se agregó el input `estado` en [trazabilidad-perfil.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/componentes/trazabilidad-perfil/trazabilidad-perfil.component.ts).
*   En `detalle-perfil.component.html`, se vinculó `<app-trazabilidad-perfil [perfilId]="perfilId" [estado]="perfil()?.estadoCodigo">`.
*   Esto garantiza que al cambiar el estado del perfil (tras guardar, enviar, observar o aprobar), el panel de trazabilidad recargue automáticamente la información actualizada del backend en tiempo real sin tener que agregar eventos locales en memoria.

### E. Archivos Modificados
*   [perfiles.service.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/servicios/perfiles.service.ts) (Implementación del mapeo y consulta real de trazabilidad).
*   [trazabilidad-perfil.component.ts](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/componentes/trazabilidad-perfil/trazabilidad-perfil.component.ts) (Entrada `estado` y trigger de recarga en cambios).
*   [detalle-perfil.component.html](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/frontend/src/app/features/perfiles/detalle-perfil/detalle-perfil.component.html) (Vínculo del input `estado` al componente).
*   [KB_23_PerfilCargo_Sprint_2026.md](file:///c:/Users/Hp/Downloads/Project%20Anigravity/SIR%20nacional-seguros-frontend/nacional-intersim/kb/KB_23_PerfilCargo_Sprint_2026.md) (Este archivo).
