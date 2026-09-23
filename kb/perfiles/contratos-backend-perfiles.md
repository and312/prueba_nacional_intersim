# Contratos y Expectativas de Backend para Perfiles

**Estado de Implementación**: Mapeado / TODOs en Frontend  
**Última Actualización**: 12/07/2026

A continuación se detallan las especificaciones técnicas del API Rest y de integración asíncrona que el backend debe proveer para dar soporte al módulo de perfiles de vacante.

---

## 1. Endpoints REST API Requeridos

### 1. Obtener lista de perfiles
*   **Contrato**: `GET /api/perfiles`
*   **Parámetros**: Filtros opcionales (`estado`, `areaId`, `cargo`, `limite`, `pagina`).
*   **Respuesta**: Array paginado de perfiles con metadatos de control (Código, Cargo, Estado, Última actualización).

### 2. Obtener detalle de perfil
*   **Contrato**: `GET /api/perfiles/{idPerfil}`
*   **Respuesta**: Objeto completo `DetallePerfil` conteniendo la estructura de las 12 secciones y el catálogo de campos con sus valores, orígenes y editabilidad.

### 3. Obtener resumen ejecutivo
*   **Contrato**: `GET /api/perfiles/{idPerfil}/resumen-ejecutivo`
*   **Respuesta**: Objeto con los 13 bloques estructurados del resumen y el estado del procesamiento asíncrono IA (`GENERADO`, `GENERANDO`, etc.).

### 4. Obtener trazabilidad (bitácora)
*   **Contrato**: `GET /api/perfiles/{idPerfil}/trazabilidad`
*   **Respuesta**: Array de eventos cronológicos del timeline.

### 5. Obtener documentos adjuntos y PDF
*   **Contrato**: `GET /api/perfiles/{idPerfil}/documentos`
*   **Respuesta**: Enlaces de descarga firmados de los PDFs oficiales generados para el profesiograma.

### 6. Obtener catálogo de tipos de observación activos
*   **Contrato**: `GET /api/tipos-observacion?activo=true`
*   **Respuesta**: Catálogo filtrado de tipos de observaciones activos.

### 7. Obtener observaciones de un perfil
*   **Contrato**: `GET /api/perfiles/{idPerfil}/observaciones`
*   **Respuesta**: Array de observaciones globales registradas.

### 8. Registrar nueva observación
*   **Contrato**: `POST /api/perfiles/{idPerfil}/observaciones`
*   **Payload**:
    ```json
    {
      "idTipoObservacion": 3,
      "comentario": "El comentario aclaratorio ingresado..."
    }
    ```
*   **Respuesta**: Registro de la observación creada con su ID y fecha.

### 9. Resolver observación (Checklist RRHH)
*   **Contrato**: `PATCH /api/perfiles/{idPerfil}/observaciones/{idObservacionPerfil}/resolver`
*   **Respuesta**: Objeto de la observación con estado `RESUELTA`, resolutor y fecha.

### 10. Forzar generación/regeneración del resumen ejecutivo
*   **Contrato**: `POST /api/perfiles/{idPerfil}/resumen-ejecutivo/generar`
*   **Respuesta**: Confirmación de encolamiento de la tarea asíncrona.

---

## 2. Seguridad y Contexto de Usuario
> [!IMPORTANT]
> El frontend **no debe enviar el rol del usuario de forma explícita** en los payloads de las peticiones HTTP para autorizar acciones.
*   **Procedimiento**: El backend intercepta el token de seguridad corporativo (Bearer JWT), extrae la identidad del usuario conectado, valida en base de datos sus roles y áreas asignadas, y deniega o autoriza la petición.
*   **Regla de Negocio**: Evitar la suplantación de identidad o elevación de privilegios modificando variables del lado del cliente.

---

## 3. Integración Asíncrona (Automatización y WhatsApp)
Los motores externos de mensajería (n8n, integraciones de WhatsApp) que notifican a los tomadores de decisiones sobre perfiles observados deben recibir del backend un payload estructurado de observaciones que **no dependa de secciones**:

```json
{
  "perfilId": 5,
  "codigoPerfil": "PERF-005",
  "cargo": "Jefe de Cuentas",
  "estadoPerfil": "Observada",
  "observaciones": [
    {
      "idObservacionPerfil": 1,
      "idTipoObservacion": 1,
      "nombreTipo": "Información incompleta",
      "comentario": "Falta incluir la herramienta Salesforce que es indispensable para el cargo.",
      "estado": "PENDIENTE"
    }
  ]
}
```
Esto garantiza la consistencia del flujo conversacional sin requerir mapeos complejos de acordeones del frontend en sistemas de mensajería.
