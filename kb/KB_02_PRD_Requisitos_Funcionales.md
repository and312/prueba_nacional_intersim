# KB_02_PRD_Requisitos_Funcionales

## Descripción

Esta Knowledge Base contiene todos los requisitos funcionales, no funcionales, historias de usuario y reglas de negocio del Sistema Inteligente de Reclutamiento para Nacional Seguros.

Todo diseño, API, pantalla, entidad, workflow y agente debe alinearse con estos requerimientos.

---

# Requisitos Funcionales

## RF-01 Registro de Solicitud

El sistema debe permitir registrar solicitudes de personal mediante un formulario estructurado dividido en cuatro secciones que diferencian claramente la información automática del sistema de los campos obligatorios y opcionales:

### Estructura y Campos del Formulario

#### 1. Datos Generales de la Solicitud
*   **Datos Automáticos del Sistema** (de solo lectura, fondo neutro con indicador de candado discreto):
    *   **Fecha de solicitud**: Fecha actual del sistema en formato local (`dd/MM/yyyy`).
    *   **Área solicitante**: Nombre de área obtenido del usuario autenticado.
    *   **Solicitante responsable**: Nombre completo obtenido de la sesión.
    *   **Cargo del solicitante**: Puesto del usuario obtenido del sistema.
*   **Campos Obligatorios de Llenado**:
    *   **Cargo requerido** (texto libre, mínimo 3 caracteres, obligatorio).
    *   **Regional** (lista desplegable parametrizable de catalogación activa, obligatorio).
    *   **Cantidad de vacantes** (numérico entero, obligatorio, mínimo 1).
    *   **Tipo de solicitud** (lista desplegable parametrizable de catalogación activa, obligatorio).
    *   **Motivo de la vacante** (área de texto libre, obligatorio, mínimo 10 caracteres).

#### 2. Perfil Requerido
*   **Información Obligatoria**:
    *   **Objetivo principal del cargo** (área de texto obligatorio).
    *   **Experiencia mínima requerida** (área de texto obligatorio).
    *   **Conocimientos técnicos requeridos** (área de texto obligatorio).
    *   **Funciones principales** (área de texto obligatorio, con texto de ayuda "ingresar una función por línea").
*   **Información Recomendada** (todos opcionales con badge "Opcional"):
    *   **Formación académica requerida** (área de texto opcional).
    *   **Experiencia específica indispensable** (área de texto opcional).
    *   **Herramientas o sistemas requeridos** (área de texto opcional).
    *   **Competencias clave del perfil** (área de texto opcional).
    *   **Criterios excluyentes** (área de texto opcional).
    *   **Criterios deseables** (área de texto opcional).

#### 3. Condiciones de la Vacante
*   **Campos Obligatorios**:
    *   **Modalidad de trabajo** (lista desplegable parametrizable de catalogación activa, obligatorio).
    *   **Seniority** (lista desplegable, obligatorio).
    *   **Prioridad** (lista desplegable, obligatorio).
*   **Campo Recomendado**:
    *   **Disponibilidad requerida** (área de texto opcional para turnos, viajes, etc.).

#### 4. Resumen de la Solicitud
*   Muestra un panel compacto y reactivo al final del formulario resumiendo los datos principales. Muestra `"No especificado"` cuando el dato no se ha ingresado, previniendo visualizaciones nulas.

### Mapeo al Payload (`SolicitudCreateRequest`)
*   `cargo`: Cargo requerido.
*   `regionalId`: Identificador numérico de la regional activa elegida.
*   `cantidadVacantes`: Cantidad numérica entera.
*   `tipoSolicitudId`: Identificador numérico del tipo de solicitud.
*   `motivo`: Detalle del motivo de la vacante.
*   `objetivoCargo`: Objetivo del cargo.
*   `formacionAcademica`: Opcional.
*   `experienciaMinima`: Experiencia mínima.
*   `experienciaIndispensable`: Opcional.
*   `conocimientosTecnicos`: Conocimientos requeridos.
*   `herramientasSistemas`: Opcional.
*   `competenciasClave`: Opcional.
*   `criteriosExcluyentes`: Opcional.
*   `criteriosDeseables`: Opcional.
*   `funciones`: Funciones principales.
*   `modalidadTrabajoId`: Identificador numérico de la modalidad activa elegida.
*   `disponibilidadRequerida`: Opcional.
*   `seniority`: Seniority seleccionado.
*   `prioridad`: Prioridad seleccionada.
*   `usuarioId`: Identificador único del solicitante obtenido automáticamente de la sesión (`getUserId()`).

---
Última actualización: 13/07/2026
Estado: Implementado y Conectado a Endpoints HTTP Reales
Internacionalización:
*   Locale de Angular configurado globalmente en `src/main.ts` mediante `registerLocaleData(localeEsBo)` para evitar excepciones de `Missing locale data` al utilizar la fecha del sistema en `DatePipe` en español.
*   Idioma HTML configurado como `"es"` con atributos `translate="no"` y clase `notranslate` en ligaduras de iconos para prevenir traducciones no deseadas por parte de los navegadores (Chrome/Edge).
Mapeo e Integración:
*   **Regionales**: Consumido de `GET /api/v1/regionales?soloActivos=true`.
*   **Tipos de Solicitud**: Consumido de `GET /api/v1/tipos-solicitud?soloActivos=true`.
*   **Modalidades de Trabajo**: Consumido de `GET /api/v1/modalidades-trabajo?soloActivos=true`.
*   **Identidad**: El `usuarioId` es inyectado desde `AuthService.getUserId()` (proveniente del token/login de backend) en lugar de nombres de texto plano redundantes.

---

## RF-02 Ficha Única de Vacante

Debe existir una ficha única de vacante que centralice:

* Resumen
* Perfil aprobado
* Banda salarial
* Publicaciones
* Postulantes
* Entrevistas
* Acciones
* Bitácora

---

## RF-03 Generación de Perfil

El sistema debe permitir generar perfiles de cargo mediante IA utilizando:

* Manuales de funciones
* Historial de contrataciones
* Información de la solicitud

Debe incluir:

* Funciones
* Skills
* Keywords
* Fit cultural
* Red Flags
* Recomendaciones de sourcing

---

## RF-04 Validación de Perfil

Estados:

* En Validación
* Aprobada
* Rechazada

Acciones:

* Guardar
* Aprobar
* Observar
* Rechazar

Sin aprobación no puede iniciarse reclutamiento.

---

## RF-05 Estrategia de Búsqueda

Canales permitidos:

* Correo Interno
* Referidos
* Portal Institucional
* Periódico

Debe permitir sugerencias IA.

---

## RF-06 Publicación de Vacantes

Permitir:

* Publicar
* Editar
* Despublicar
* Consultar

Registrar trazabilidad.

---

## RF-07 Captación de Postulantes

Permitir registrar:

* Origen
* Fecha
* Canal
* Estado

---

## RF-08 Expediente del Postulante

Debe incluir:

* Datos personales
* CV
* Documentos
* Matching
* Score
* Entrevistas
* Historial
* Conversaciones
* Eventos

---

## RF-09 Matching

Comparar:

Perfil ↔ Postulante

Generar:

* Coincidencias
* Brechas
* Recomendaciones

---

## RF-10 Scoring

Generar score parametrizable.

Factores:

* Skills
* Experiencia
* Formación
* Psicotécnicos

Todo score debe ser explicable.

---

## RF-11 Pipeline de Reclutamiento

Estados mínimos:

* Captado
* Screening
* Psicotécnico
* Entrevista RRHH
* Entrevista Técnica
* Terna
* Contratado
* Descartado

---

## RF-12 Gestión Documental

Permitir:

* Cargar
* Consultar
* Versionar
* Auditar

documentos asociados.

---

## RF-13 Parametrización

Configurar:

* Estados
* Canales
* SLA
* Plantillas
* Reglas
* Prompts

---

## RF-14 Coordinación de Entrevistas

Permitir:

* Programar
* Reprogramar
* Cancelar
* Confirmar

Integrado con Agenda.

---

## RF-15 Agenda

Vista:

* Día
* Semana
* Mes

Sincronización con entrevistas.

---

## RF-16 Notificaciones

Canales:

* Correo
* WhatsApp

Registrar:

* Destinatario
* Fecha
* Estado
* Resultado

---

## RF-17 Dashboard

Mostrar:

* Solicitudes
* Vacantes
* Entrevistas
* Alertas
* SLA

---

## RF-18 Kanban

Vistas:

* Solicitudes
* Vacantes
* Postulantes

---

## RF-19 Auditoría

Registrar:

* Acciones
* Cambios
* Aprobaciones
* Integraciones
* IA

---

## RF-20 Analítica

Generar KPIs:

* Tiempo de cobertura
* Vacantes por área
* Vacantes por urgencia
* Éxito a 3 meses
* Efectividad por canal
* SLA

---

## RF-21 Roles y Permisos

Administrar:

* Roles
* Permisos
* Accesos

---

## RF-22 Login

Permitir:

* Autenticación
* Recuperación de contraseña
* Gestión de sesión

---

# Requisitos No Funcionales

## Seguridad

* JWT
* RBAC
* MFA
* HTTPS
* OWASP

---

## Auditoría

Toda acción debe ser trazable.

---

## Rendimiento

Operaciones críticas deben responder en tiempos adecuados.

---

## Escalabilidad

Soportar crecimiento de:

* Vacantes
* Postulantes
* Eventos
* Integraciones

---

## Mantenibilidad

Arquitectura desacoplada.

---

## Observabilidad

* Logs
* Métricas
* Alertas
* Dashboards

---

# Reglas de Negocio

* Ninguna vacante puede publicarse sin perfil aprobado.
* Ningún postulante puede contratarse sin completar flujo.
* Ningún agente IA puede aprobar.
* Ningún agente IA puede contratar.
* Todo cambio crítico debe quedar auditado.
* Todo score debe ser explicable.
* Todo prompt debe estar versionado.
* Toda ejecución IA debe quedar registrada.

---

# Especificación del Listado de Solicitudes (Lectura)

El listado de solicitudes visualiza y gestiona las solicitudes de vacantes registradas, alineándose con el contrato de la API real.

### Contrato de Lectura (`SolicitudListItem`)
*   `solicitudId`: Identificador único numérico de la solicitud.
*   `codigo`: Código estructurado (ej. `SOL-0012`).
*   `cargo`: Nombre del cargo requerido.
*   `area`: Nombre del área solicitante.
*   `solicitanteNombre`: Nombre completo del usuario solicitante (obtenido mediante join en backend).
*   `createdDate`: Fecha de registro original de la solicitud.
*   `lastModifiedDate`: Fecha de la última modificación (actualización) en la base de datos.
*   `prioridad`: Prioridad seleccionada (`Alta`, `Media`, `Baja`, `Critica`).
*   `estadoNombre`: Nombre del estado actual en el workflow.
*   `estadoCodigo`: Código interno del estado.
*   `tipoSolicitud`: Objeto de catálogo con `id`, `codigo` y `nombre` descriptivo.

### Reglas Visuales y Mapeos del Listado
*   **Solicitante**: Muestra `solicitanteNombre`. Si es nulo o vacío, utiliza el fallback neutro `"No disponible"`. Se retiró el fallback `"Sistema"` para evitar falsas atribuciones de autoría.
*   **Tipo de Solicitud**: Renombrado a *"Tipo de solicitud"*, resolviendo la propiedad `tipoSolicitud?.nombre` descriptiva en lugar de IDs o valores genéricos. Fallback: `"No especificado"`.
*   **Última actualización**: Muestra la fecha formateada utilizando `lastModifiedDate`, cayendo en cascada a `createdDate` si no ha sido modificada.
*   **Retiro de Responsable Actual**: Al confirmarse que el responsable actual no interviene en filtros, ordenamientos ni lógica del listado frontend, se eliminó la columna para optimizar el ancho disponible y mejorar la densidad de información.
*   **Búsqueda en Cliente**: El filtro de buscador se extendió para coincidir localmente sobre `cargo`, `area`, `solicitanteNombre` y `codigo`.
*   **Matriz de Acciones Operativas**: Se centralizó la lógica en la función de TypeScript `obtenerAccion(item)` para determinar dinámicamente el botón de acción según el rol del usuario y el estado de la solicitud:
    *   *Administrador*: Siempre ve la acción *"Ver detalle"*.
    *   *Recursos Humanos (RRHH)*:
        *   Estados `SOL-ENV` (Enviada a RRHH/En Validación/En revisión): Muestra la acción operativa principal *"Revisar"*.
        *   Otros estados (Aprobada, Rechazada, Borrador): Muestra la acción pasiva *"Ver detalle"*.
    *   *Solicitante / Área Solicitante*:
        *   Estados `SOL-REG`, `SOL-PEN` o Borradores: Muestra la acción *"Completar datos"* (hacia edición).
        *   Estado `SOL-OBS` (Observada): Muestra la acción *"Corregir"* (hacia edición).
        *   Estados `SOL-ENV` (Enviada/En revisión): Muestra la acción *"Ver seguimiento"*.
        *   Estados Finales (`SOL-APR` Aprobada, `SOL-RECH` Rechazada): Muestra la acción *"Ver detalle"*.

### Reglas de Autorización y Carga en Edición
*   **Validación de Propietario**: La autorización de edición compara numéricamente el identificador del creador de la solicitud (`solicitanteId`) con el ID del usuario en sesión (`getUserId()`), previniendo comparaciones vulnerables por nombre o email.
*   **Restricción de Estados**: Para el rol Solicitante, solo se permite la edición de solicitudes activas y no terminales (estados `SOL-REG`, `SOL-PEN`, `SOL-OBS`, `Borrador`, `Observada`, `Pendiente`). Las solicitudes en estados finales (`SOL-APR`, `SOL-RECH`, `SOL-ENV`) quedan estrictamente bloqueadas para edición.
*   **Manejo de Carga (Loader)**: Ante cualquier denegación de acceso o error HTTP, se asegura el cierre inmediato del spinner de catálogos (`catalogosCargando = false`) antes de redirigir al usuario al listado, evitando que la pantalla quede bloqueada indefinidamente.

---

# Criterio de Validación

Todo diseño generado deberá mapear explícitamente los RF y RNF afectados.

Ningún desarrollo podrá omitir un requisito aquí definido sin justificación y aprobación formal.
