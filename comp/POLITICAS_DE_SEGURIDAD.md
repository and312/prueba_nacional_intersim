# Políticas de Seguridad de la Información y Cumplimiento (OWASP)

Este documento define la arquitectura de seguridad, la gestión de identidades, la protección de datos sensibles, la seguridad en APIs, la gobernanza de accesos en base de datos y n8n, y los escaneos DevSecOps obligatorios para el **Sistema Inteligente de Reclutamiento de Nacional Seguros**. Cumple de forma absoluta con la [Constitución del Proyecto](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/CONSTITUCION_PROYECTO.md) y las directrices del [Auditor de Calidad](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/AUDITORIA_CALIDAD_Y_CUMPLIMIENTO.md).

---

## 1. Controles Clave Contra OWASP Top 10

Para mitigar los riesgos más comunes en el desarrollo de software web, se implementarán los siguientes controles obligatorios:

### 1.1 Inyección (SQL Injection)
* **Control:** Prohibido el uso de consultas SQL concatenadas dinámicamente. Toda interacción con SQL Server 2022 se realiza a través de LINQ parametrizado en Entity Framework Core o consultas parametrizadas explícitas mediante Dapper.

### 1.2 Pérdida de Autenticación y Autorización (Broken Authentication & Access Control)
* **Control:** El login inicial requiere validación segura en el Backend. Se admite la autenticación local e híbrida delegada al servicio de directorio corporativo de identidad (LDAP/OIDC). Tras una autenticación exitosa, se emite un JWT firmado digitalmente mediante un algoritmo robusto (ej. HS256/RS256).
* **Control:** El backend inyecta los claims de roles y permisos del usuario en el token. Cada Endpoint expuesto en la API cuenta con la anotación `[Authorize(Roles = "...")]` o políticas customizadas. El frontend valida estos roles mediante guardias de ruta y la directiva `*hasRole`.
* **Control (Directorio Corporativo):** Para usuarios autenticados mediante el servicio de directorio corporativo (LDAP/OIDC), la base de datos local omite almacenar contraseñas (`ClaveHash` como nullable). Las políticas de complejidad de claves, expiración y MFA son delegadas directamente a la configuración e infraestructura de identidad corporativa de la organización.
* **Control (Rotación de Refresh Tokens - RTR):** Toda emisión de JWT asocia un Refresh Token de un solo uso en la tabla `Sesion`. Al solicitar renovación de sesión, el token utilizado se marca inmediatamente como inactivo (`Activa = 0`) y se emite uno nuevo. Si se detecta un intento de reutilización de un Refresh Token ya invalidado, la API suspenderá inmediatamente todas las sesiones activas del usuario por sospecha de secuestro de sesión.

### 1.3 Exposición de Datos Sensibles (Cryptographic Failures)
* **Control:** Toda comunicación viaja cifrada mediante HTTPS con soporte exclusivo para TLS 1.3. Las contraseñas locales se almacenan cifradas en base de datos utilizando algoritmos con salting (BCrypt o Argon2id).
* **Control:** Las connection strings, credenciales del servidor LDAP y claves del servicio de directorio se inyectan en tiempo de ejecución a través del proveedor corporativo de gestión de secretos o variables de entorno seguras. Nunca en código duro.
* **Control (Gobernanza de Always Encrypted):** La encriptación a nivel de columna para datos salariales y scoring se realiza utilizando Always Encrypted. La llave maestra de columna (CMK) se almacena y administra de manera centralizada en el proveedor corporativo de gestión de secretos. Las llaves de cifrado de columna (CEK) se encriptan utilizando la CMK. Se establece una política de rotación obligatoria de la CMK cada 12 meses, ejecutada por el Administrador de Seguridad (CISO) y auditada en los logs del gestor de secretos.
* **Control (Respaldo y Recuperación de CMK):**
  - Para garantizar la continuidad del negocio y evitar la pérdida irreversible de datos, la CMK cuenta con opciones de borrado lógico y protección contra purga habilitadas obligatoriamente en el proveedor corporativo de gestión de secretos.
  - Se debe realizar un respaldo periódico e inmutable de la CMK utilizando los comandos criptográficos firmados provistos por la herramienta de administración del gestor de secretos:
    ```powershell
    # Comando de ejemplo para respaldo del proveedor de llaves
    Backup-KeyStoreKey -StoreName "secrets-store-prod" -Name "SIR-CMK-MasterKey" -OutputFile "C:\Secrets\SIR-CMK-MasterKey.bak"
    ```
  - En caso de contingencia o desastre, la restauración se realiza mediante:
    ```powershell
    # Comando de ejemplo para restauración del proveedor de llaves
    Restore-KeyStoreKey -StoreName "secrets-store-prod-recovery" -InputFile "C:\Secrets\SIR-CMK-MasterKey.bak"
    ```
  - Se restringen los permisos en el proveedor de secretos usando control de acceso basado en roles (RBAC): el pool del backend de .NET 8 tiene asignado el rol mínimo de operaciones criptográficas de descifrar/cifrar, y solo el Oficial de Seguridad (CISO) posee privilegios de administración completa (creación/rotación/respaldo).

### 1.4 Scripting en Sitios Cruzados (XSS) y CSRF
* **Control:** Angular cuenta con sanitización nativa de plantillas contra XSS. Para cualquier binding directo de HTML se debe emplear un sanitizador controlado del lado del servidor.
* **Control:** Implementación de tokens CSRF en formularios de escritura y configuración de cookies con atributos `SameSite=Strict` y `Secure`.

---

## 2. Clasificación de Datos Sensibles

Se cataloga como información altamente confidencial que requiere cifrado y políticas de acceso estrictas:
* **Datos Salariales:** Bandas salariales de la vacante, pretensiones salariales del postulante.
* **Evaluaciones y Pruebas:** Resultados de exámenes psicotécnicos, notas de entrevistas, scoring y justificación de coincidencia (matching) generada por la IA.
* **Datos Personales (PII):** Números de documento, direcciones y contactos de postulantes.

---

## 3. Seguridad Específica en Integración con n8n e IA

* **Llamadas Seguras a n8n:** Los webhooks de n8n que reciben peticiones del backend requieren autenticación por API Key en las cabeceras (`X-API-Key`).
* **Protección de Datos al Enviar a LLMs:** Queda terminantemente prohibido enviar datos personales directos (ej. nombres completos, direcciones exactas o números de teléfono de postulantes) en los prompts hacia los LLM externos. Al invocar a los agentes de matching o scoring, el backend debe anonimizar los datos enviando identificadores temporales o datos puramente profesionales (Skills, experiencia, educación).
* **No Hardcodeo de Claves en n8n:** Las API keys del LLM se configuran en el manejador de credenciales de n8n, nunca visibles en el workflow de automatización.

---

## 4. Estrategia DevSecOps y Análisis de Código

El pipeline de compilación e integración continua (CI) debe ejecutar en cada Pull Request:
1. **Dependency Scan (ej. Snyk / OWASP Dependency-Check):** Para validar que no se importen librerías de terceros con vulnerabilidades críticas conocidas.
2. **Static Application Security Testing (SAST):** Análisis del código fuente buscando malas prácticas de codificación o fugas de secretos.
3. **Secret Scanning:** Bloquear de inmediato el pipeline si se detecta alguna clave privada, connection string o token en texto plano en los archivos de la confirmación (git commit).

---

## 5. Sanitización de Logs y Excepciones (Evitar Fuga de Información)

Para garantizar la confidencialidad de la información y prevenir la exposición involuntaria de datos PII o credenciales en los archivos de log del servidor y en las respuestas de error de la API, se implementan los siguientes controles de sanitización:

### 5.1 Enriquecedor de Sanitización en Serilog (Filtro Regex)
El motor de logging estructurado (Serilog) debe interceptar todos los mensajes y propiedades de eventos registrados, aplicando filtros basados en expresiones regulares para enmascarar con la etiqueta `[REDACTED]` cualquier dato sensible en tránsito:
* **Tokens de acceso (JWT):** `bearer\s+[a-zA-Z0-9\-\._~\+\/]+=*` y `TokenJwt\s*:\s*\"[^\"]+\"`
* **Credenciales de usuarios:** `password=\w+`, `clave=\w+` y `client_secret=\w+`
* **Campos con anotación `[SensitiveData]`:** Todo JSON de auditoría en `AuditLog` o `AuditDetail` que contenga campos como `BandaSalarialMin`, `BandaSalarialMax`, `PretensionSalarial` o `ScoreCoincidencia` debe ser enmascarado por el interceptor de EF Core antes de ser serializado a log.

### 5.2 Middleware de Excepciones Sanitizado
El middleware global de control de excepciones (`ExceptionHandlingMiddleware`) debe interceptar excepciones de infraestructura de bajo nivel (especialmente las del motor SQL Server 2022 y el servicio de directorio corporativo):
* **Ocultación de Connection Strings:** Se prohíbe mostrar metadatos de conexión, nombres de bases de datos, contraseñas de DB o direcciones IP del servidor SQL en los logs y respuestas HTTP.
* **Supresión de Parámetros SQL:** Si una consulta SQL Server falla debido a violaciones de constraints o timeouts, el mensaje de excepción se sanitiza eliminando los valores de los parámetros encolados en la consulta antes de registrar la traza del error.
* **Payload de Error Estándar para Clientes:** En entornos productivos, el detalle de error (`Detail`) debe ser genérico, asociándose únicamente al identificador de correlación `CorrelationId` para auditoría interna, evitando la fuga de metadatos del servidor.

