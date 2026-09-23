# Estándar de Documentación Técnica y Manuales

Este documento establece las directrices de documentación oficial para el **Sistema Inteligente de Reclutamiento de Nacional Seguros**. Define la estructura para diagramas de arquitectura C4 (Contexto, Contenedores, Componentes), Diccionarios de Datos, catálogos OpenAPI/Swagger, Manuales Técnicos y de Usuario, y bitácoras de control de versiones. Cumple en su totalidad con la [Constitución del Proyecto](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/CONSTITUCION_PROYECTO.md) y las directrices del [Auditor de Calidad](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md).

---

## 1. Misión del Technical Writer

* **Centralizar y Unificar el Conocimiento:** Evitar silos de información y garantizar la comprensibilidad técnica del sistema.
* **Garantizar la Trazabilidad Documental:** Asegurar que todo cambio de software, base de datos o workflow de n8n quede reflejado en la documentación y diagramas correspondientes de forma inmediata.
* **Simplificar la Transferencia de Conocimiento:** Proveer manuales técnicos, operativos y de usuario con pasos claros y accionables que faciliten la inducción de nuevos desarrolladores o usuarios operativos.

---

## 2. Estándar de Diagramación C4 (Mermaid)

Para asegurar la legibilidad, todos los diagramas de arquitectura del proyecto se representarán en formato de texto Mermaid dentro del repositorio:

### 2.1 C4 Nivel 1: Diagrama de Contexto
Establece las interacciones externas primarias de la plataforma con los usuarios y canales externos autorizados (WhatsApp API, Servidor SMTP, etc.).

### 2.2 C4 Nivel 2: Diagrama de Contenedores
Visualiza las fronteras tecnológicas del sistema: Angular (Frontend), API en .NET 8 (Backend), n8n (Automatización de IA) y SQL Server 2022 (Persistencia Transaccional).

### 2.3 C4 Nivel 3: Diagrama de Componentes
Detalla el desacoplamiento interno de las capas, por ejemplo, los controladores REST, los servicios de aplicación, los manejadores de casos de uso (MediatR Handlers), repositorios y adaptadores de infraestructura.

---

## 3. Diccionario de Datos y OpenAPI

### Estructura de Diccionario de Datos SQL
Por cada tabla creada se documenta:
* **Nombre de la Tabla:** Identificador físico.
* **Descripción Funcional:** Propósito y lógica de negocio asociada.
* **Campos:** Nombre del campo, tipo de dato de SQL Server, nulabilidad (`NULL`/`NOT NULL`), rol de llave (`PK`/`FK`), relación y descripción técnica.

### Catálogo de APIs REST (Swagger)
Cada endpoint expuesto por el backend .NET debe incluir en Swagger:
* Ruta y Método HTTP (ej. `POST /api/v1/solicitudes`).
* Request Body / Query Params (esquema del DTO).
* Response Body en caso de éxito (código `200 OK` / `201 Created`).
* Estructura del error estándar (código `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `500 Internal Server Error`).
* Requerimiento de seguridad (política JWT requerida).

---

## 4. Manuales Obligatorios del Sistema

1. **Manual Técnico del Desarrollador:**
   * Requisitos de entorno de desarrollo (SDK .NET 8, Angular CLI, motor de contenedores local compatible).
   * Pasos detallados para clonación, restauración de paquetes y ejecución en local.
   * Procedimiento para la ejecución de pruebas unitarias y de cobertura.
2. **Manual de Operaciones y Troubleshooting (Soporte L1/L2):**
   * Configuración de observabilidad (cómo leer los logs JSON estructurados).
   * Solución de incidentes recurrentes (ej. pérdida de conexión a n8n, fallo del LLM, renovación de tokens JWT).
   * Procedimiento de restauración de backups SQL Server ante desastres.
3. **Manual de Usuario por Roles:**
   * Sección específica para cada rol operativo (RRHH, Solicitante, Decisor, Reclutador, Administrador, Auditor) con capturas de pantalla, explicaciones de campos y flujos de negocio paso a paso.

---

## 5. Control de Versiones Documentales

Todo documento técnico o manual funcional incluirá al inicio la siguiente bitácora de control:

| Versión | Fecha | Autor | Rol | Cambios Realizados |
| :---: | :---: | :---: | :---: | :--- |
| `1.0.0` | 2026-06-17 | Antigravity | Technical Writer | Creación del estándar inicial y estructura de documentación. |
