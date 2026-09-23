# Estrategia DevOps y Operación del Sistema

Este documento establece los lineamientos para la configuración de ambientes, pipelines de CI/CD, contenedorización, gestión de secretos, observabilidad (Logs/Métricas/Alertas), esquemas de backups y políticas de recuperación ante desastres para el **Sistema Inteligente de Reclutamiento de Nacional Seguros**. Cumple íntegramente con la [Constitución del Proyecto](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/CONSTITUCION_PROYECTO.md) y las directrices del [Auditor de Calidad](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md).

---

## 1. Ambientes de Despliegue

La plataforma se despliega en tres ambientes aislados lógicamente para garantizar la estabilidad operativa:

1. **Desarrollo (DEV):** Integración continua automática. Utilizado por el equipo técnico. Datos de prueba mockup.
2. **Aseguramiento de Calidad (QA):** Despliegue semiautomático tras pasar las pruebas unitarias y de calidad de código. Utilizado para pruebas de aceptación de usuario (UAT) y auditorías de seguridad.
3. **Producción (PROD):** Despliegue controlado y altamente disponible. Requiere aprobaciones explícitas de los decisores y gestores de cambio.

---

## 2. Contenedorización

Cada componente se empaqueta en imágenes de contenedor optimizadas y seguras, reduciendo la superficie de ataque (uso de imágenes base `distroless` o `alpine` de confianza):

### 2.1 Backend (.NET 8 API) - `Dockerfile`
* Construcción multietapa (Multi-stage build).
* Ejecución con un usuario no root (`appuser`) para evitar escalamiento de privilegios.

### 2.2 Frontend (Angular SPA) - `Dockerfile`
* Etapa 1: Compilación de la SPA usando Node.js.
* Etapa 2: Servido con Nginx optimizado, configurando cabeceras de seguridad HTTP (HSTS, Content Security Policy, X-Frame-Options).

### 2.3 n8n - `Dockerfile`
* Imagen oficial de n8n parametrizada exclusivamente mediante variables de entorno inyectadas para evitar credenciales persistentes en el contenedor.

---

## 3. Estrategia CI/CD (SemVer 2.0.0)

Se implementarán pipelines en el orquestador de despliegue de integración y entrega continua (CI/CD) bajo las siguientes directrices:

### 3.1 Flujo del Pipeline de Desarrollo (DEV)
```
[Git Push / PR] ──> [Build Angular / .NET] ──> [Unit Tests & Coverage] ──> [Static Security Scan] ──> [Deploy DEV]
```
* **Security Scan:** Análisis estático de seguridad (SAST) para buscar secretos hardcodeados o vulnerabilidades OWASP comunes.

### 3.2 Flujo del Pipeline de Calidad (QA)
```
[Merge to main/release] ──> [Integration Tests] ──> [API Functional Tests] ──> [Dynamic Security Scan] ──> [Deploy QA]
```

### 3.3 Flujo del Pipeline de Producción (PROD)
```
[Manual Approval Decisor] ──> [Release Tag (Major.Minor.Patch)] ──> [Zero-Downtime Deploy] ──> [Smoke Tests]
```
* **Mecanismo de Rollback:** En caso de fallo en los *Smoke Tests* automáticos post-despliegue, el pipeline realizará un rollback automático a la versión estable previa inmediatamente, notificando y bloqueando la rama afectada.

---

## 4. Gestión de Secretos y Configuración

* **Cero Hardcoding:** Todas las credenciales, cadenas de conexión SQL y API keys se almacenan en el proveedor corporativo de gestión de secretos.
* **Inyección en Tiempo de Ejecución:** Los secretos se inyectan como variables de entorno seguras directamente en los contenedores en tiempo de despliegue, nunca almacenados en archivos del repositorio.

---

## 5. Observabilidad

* **Logs Estructurados:** Backend, n8n y Base de Datos emiten logs en formato JSON estructurado conteniendo `CorrelationId`, `Timestamp`, `LogLevel`, `Usuario`, `Clase/Componente` y `Mensaje`.
* **Health Checks:** Implementación del endpoint `/health` en la API .NET y en el frontend Angular para verificar la salud de las conexiones a base de datos, APIs y n8n en tiempo real.
* **Alertas Críticas:** Se configuran alertas (vía Slack/Teams y correo) ante fallos en:
  * Conexión a base de datos.
  * Workflows fallidos de n8n.
  * Tiempos de respuesta de llamadas a LLM que excedan los 10 segundos.
  * Errores 5xx persistentes en la API de .NET.

---

## 6. Backups y Continuidad Operativa

* **Frecuencia de Backups (SQL Server 2022):**
  * **Diario:** Backup incremental almacenado en almacenamiento en la nube cifrado (con políticas de retención de 30 días).
  * **Semanal:** Backup completo con retención de 3 meses.
  * **Mensual:** Backup completo con retención de 1 año (para fines de auditoría).
* **Parámetros de Recuperación:**
  * **RPO (Recovery Point Objective):** Máximo 1 hora de pérdida de datos.
  * **RTO (Recovery Time Objective):** Máximo 4 horas de inactividad para recuperar el servicio completo en caso de desastre.
* **Pruebas de Recuperación:** Se ejecutarán pruebas de restauración periódicas semestrales en un ambiente aislado para validar la integridad de las copias de seguridad.
