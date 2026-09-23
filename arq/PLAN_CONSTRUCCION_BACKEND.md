# Plan Maestro de Construcción del Backend .NET 8
## Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros

**Autores:** NacionalSeguros_BackendArchitect | NacionalSeguros_TechnicalLead | NacionalSeguros_SolutionArchitect | NacionalSeguros_ProjectManager | NacionalSeguros_ProjectAuditor  
**Fecha:** 2026-06-25  
**Estado:** **APROBADO PARA CONSTRUCCIÓN DEL BACKEND**

---

## 1. Objetivos

### 1.1 Objetivo General
Definir el marco de trabajo, estándares, orden de desarrollo y el catálogo detallado de artefactos técnicos para la construcción física del **Backend .NET 8** del **Sistema Inteligente de Reclutamiento (SIR)** de **Nacional Seguros**, asegurando un código modular, seguro, escalable y 100% compatible con los diagramas ERD, especificaciones OpenAPI y los flujos asíncronos de n8n aprobados.

### 1.2 Objetivos Específicos
*   **Establecer la Estructura Clean Architecture:** Crear los 6 proyectos de la solución y configurar las dependencias unidireccionales de forma estricta.
*   **Mapear el Dominio y Persistencia (EF Core 9 / Dapper):** Definir el DbContext, mapeos fluidos para SQL Server 2022 y repositorios.
*   **Implementar CQRS y MediatR:** Estructurar el procesamiento de comandos (escritura) y consultas (lectura) de forma desacoplada y con validación en el pipeline.
*   **Configurar Controles de Seguridad:** Implementar el middleware de JWT, autenticación LDAP para el servicio de directorio de identidad corporativo, y el interceptor de cifrado Always Encrypted.
*   **Habilitar la Observabilidad e Idempotencia:** Desarrollar los middlewares de CorrelationId, excepciones sanitizadas y el filtro de idempotencia para callbacks de n8n.

---

## 2. Estrategia de Arquitectura y Desarrollo

El backend se construirá utilizando los siguientes pilares de diseño de software empresarial:

*   **Desarrollo Modular e Incremental:** El desarrollo se divide en módulos funcionales independientes. No se avanza al desarrollo de un módulo de negocio superior si no se han completado y certificado las capas base correspondientes.
*   **Clean Architecture:** Separación rígida de responsabilidades en capas. El dominio (`NacionalSeguros.Domain`) es el núcleo puro y no posee dependencias externas. La lógica de negocio reside en `Application`, los adaptadores de tecnologías en `Persistence` e `Infrastructure`, y la exposición en `Api`.
*   **Domain-Driven Design (DDD):** Modelado orientado al negocio mediante Entidades con identidad, Value Objects inmutables, Aggregates para consistencia transaccional y Eventos de Dominio para desacoplamiento asíncrono.
*   **CQRS (Command Query Responsibility Segregation):** Separación de operaciones de escritura (Commands que mutan el estado y disparan eventos de dominio) y operaciones de lectura (Queries optimizadas para alto rendimiento que consultan vistas o ejecutan Dapper sin lógica de negocio compleja).
*   **Pipeline de MediatR:** Orquestación central de peticiones. Se inyectan comportamientos cruzados (Validation Behavior, Logging Behavior, Transaction Behavior) en el pipeline de ejecución antes de llegar al handler.
*   **Integración Continua (CI):** Compilación limpia obligatoria ante cada Pull Request, análisis SAST (SonarQube) y cobertura de pruebas unitarias mayor al 80%.

---

## 3. Orden de Implementación y Justificación Técnica

A continuación se define el orden secuencial de desarrollo del backend:

```
[Solución Base] ──► [Infraestructura & DB] ──► [Seguridad] ──► [Catálogos] ──► [Flujo de Reclutamiento 5-13] ──► [SLAs & Analytics] ──► [Tuning]
```

1.  **Solución Base:** Creación del árbol de proyectos y configuración de las dependencias e importaciones NuGet básicas. *Justificación: Establece el esqueleto del software.*
2.  **Configuración de Infraestructura:** Setup de `DbContext`, migraciones iniciales de EF Core 9 y Dapper en SQL Server 2022. *Justificación: Habilita el acceso y conectividad de datos.*
3.  **Seguridad:** Middleware JWT, LDAP del servicio de directorio de identidad corporativo e inyección criptográfica de Always Encrypted. *Justificación: Bloquea todos los endpoints de forma temprana.*
4.  **Catálogos Maestros:** Implementación de la parametrización de catálogos y árbol jerárquico. *Justificación: Datos base necesarios para asociar estados, seniorities y prioridades en las tablas principales.*
5.  **Solicitudes:** Módulo de entrada de requisiciones de personal. *Justificación: Inicia el ciclo de vida del proceso de reclutamiento.*
6.  **Perfiles:** Definición de profesiogramas de cargos. *Justificación: Se derivan directamente de la solicitud aprobada.*
7.  **Vacantes:** Creación del proceso operativo con cifrado Always Encrypted de salarios. *Justificación: Requiere un perfil de cargo aprobado.*
8.  **Postulantes:** Captación de candidatos y mapeo M:N de postulaciones. *Justificación: Se postulan directamente sobre las vacantes creadas.*
9.  **Matching IA:** Procesamiento asíncrono curricular contra profesiograma. *Justificación: Requiere postulantes registrados y vacantes configuradas.*
10. **Scoring IA:** Evaluación explicable y asignación de ranking. *Justificación: Corre sobre la terna filtrada por el matching curricular.*
11. **Entrevistas:** Coordinación de agendas y llamadas Teams. *Justificación: Se agenda solo para candidatos seleccionados en el ranking.*
12. **Ofertas:** Propuesta económica final cruzando bandas y pretensiones salariales ( Always Encrypted). *Justificación: Se emite tras las entrevistas.*
13. **Contrataciones:** Cierre del pipeline y alta del empleado. *Justificación: Cierre del proceso tras aceptación de la oferta.*
14. **SLA:** Motor de auditoría y cálculo de vencimientos por día hábil. *Justificación: Requiere transiciones y tiempos de todos los módulos anteriores.*
15. **Observabilidad:** Telemetría de APIs, interceptores de auditoría y Ledger. *Justificación: Habilita la trazabilidad integral de logs de la aplicación.*
16. **Reportes:** Exportaciones y snapshots analíticos. *Justificación: Consume datos históricos de todos los módulos.*
17. **Integraciones:** Adaptadores finales de mensajería y APIs externas. *Justificación: Canales y conectores que apoyan el flujo de notificaciones.*
18. **Optimización:** Afinamiento de Query Store, índices y compresión de logs. *Justificación: Ajustes de rendimiento finales sobre la solución armada.*

---

## 4. Especificación Detallada de los Módulos del Backend

A continuación se define la estructura técnica detallada para cada uno de los 18 módulos del Backend:

---

### Módulo 1: Solución Base
*   **Objetivo:** Setup inicial de la solución .sln y dependencias.
*   **Dependencias:** Ninguna (Infraestructura de desarrollo).
*   **Entidades / Value Objects / DTOs:** Ninguno.
*   **Commands / Queries / Handlers:** Ninguno.
*   **Validators / Repositories / Interfaces / Services:** Setup de `IApplicationDbContext` y base de `UnitOfWork`.
*   **Controllers / Endpoints OpenAPI:** Ninguno.
*   **Stored Procedures / n8n / Agentes IA:** Ninguno.
*   **Eventos de Dominio:** Ninguno.
*   **Casos de Prueba Requeridos:** Pruebas arquitectónicas con `NetArchTest` para verificar que la capa de dominio no dependa de persistencia ni API.
*   **Riesgos Técnicos:** Dependencias circulares entre capas en el setup inicial de Visual Studio.
*   **Criterios de Aceptación:** La solución compila limpiamente con las referencias de proyectos orientadas al centro (Clean Architecture).

---

### Módulo 2: Configuración de Infraestructura y Base de Datos
*   **Objetivo:** Configuración de Entity Framework Core 9, Dapper y conectividad con SQL Server 2022.
*   **Dependencias:** Módulo 1.
*   **Entidades:** Configuración base de entidades en el `ApplicationDbContext`.
*   **Value Objects:** Ninguno.
*   **DTOs:** `DbConnectionStatusResponse`.
*   **Commands:** Ninguno.
*   **Queries:** `CheckDatabaseHealthQuery`.
*   **Handlers:** `CheckDatabaseHealthQueryHandler` (ejecuta un query rápido Dapper `SELECT 1`).
*   **Validators:** Ninguno.
*   **Repositories:** Configuración del `RepositoryBase<T>`.
*   **Interfaces:** `IApplicationDbContext`, `IUnitOfWork`.
*   **Services:** `DbHealthCheckService`.
*   **Controllers:** `SystemHealthController`.
*   **Endpoints OpenAPI:** `GET /api/v1/health/db`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Ninguno.
*   **Eventos de Dominio:** Ninguno.
*   **Casos de Prueba:** `DbConnection_ReturnsTrue_OnValidConfig`.
*   **Riesgos Técnicos:** Error al levantar migraciones automáticas en SQL Server con esquemas de filegroups diferentes en DEV.
*   **Criterios de Aceptación:** El endpoint de healthcheck retorna `200 OK` y el estado de la base de datos es activo.

---

### Módulo 3: Seguridad (JWT, Identidad y Always Encrypted)
*   **Objetivo:** Asegurar las APIs, autenticar usuarios e inyectar llaves de encriptación Always Encrypted.
*   **Dependencias:** Módulos 1, 2.
*   **Entidades:** `Usuario`, `Rol`, `Permiso`, `UsuarioRol`, `RolPermiso`, `Sesion`.
*   **Value Objects:** `Email`.
*   **DTOs:** `LoginRequest`, `LoginResponse`, `TokenRefreshRequest`.
*   **Commands:** `AutenticarUsuarioCommand`, `RefrescarTokenCommand`, `InvalidarSesionesUsuarioCommand`.
*   **Queries:** `ObtenerPermisosUsuarioQuery`.
*   **Handlers:** `AutenticarUsuarioCommandHandler` (con bifurcación Local/Directorio Corporativo), `RefrescarTokenCommandHandler` (con lógica RTR y invalidación masiva por reuso), `InvalidarSesionesUsuarioCommandHandler`.
*   **Validators:** `LoginRequestValidator` (valida estructura de correo).
*   **Repositories:** `IUsuarioRepository`, `ISesionRepository`.
*   **Interfaces:** `ITokenService`, `IActiveDirectoryService`.
*   **Services:** `JwtTokenService`, `LdapActiveDirectoryService`.
*   **Controllers:** `AuthController`.
*   **Endpoints OpenAPI:** `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `POST /api/v1/auth/logout`.
*   **Stored Procedures:** `sp_Seguridad_RegistrarSesion`, `sp_Seguridad_InvalidarSesionesUsuario`.
*   **n8n / Agentes IA:** Ninguno.
*   **Eventos de Dominio:** `SesionIniciadaEvent`, `TokenReusedDetectedEvent` (dispara invalidación global).
*   **Casos de Prueba:** `RefrescarToken_InvalidatesAllSessions_OnTokenReused`.
*   **Riesgos Técnicos:** Timeout en llamadas LDAP al Directorio de Identidad Corporativo de Nacional Seguros corporativo.
*   **Criterios de Aceptación:** JWT generado exitosamente; la base de datos invalida todas las sesiones de un usuario ante la detección de un token de refresco inactivo.

---

### Módulo 4: Catálogos Maestros y Parámetros
*   **Objetivo:** Gestionar catálogos y validar de forma defensiva la jerarquía de parámetros.
*   **Dependencias:** Módulo 3.
*   **Entidades:** `Catalogo`, `Parametro`, `Estado`.
*   **Value Objects:** Ninguno.
*   **DTOs:** `CatalogoDto`, `ParametroDto`, `CrearParametroRequest`.
*   **Commands:** `CrearParametroCommand`.
*   **Queries:** `ListParametrosByCatalogoQuery`.
*   **Handlers:** `CrearParametroCommandHandler` (valida recursión), `ListParametrosByCatalogoQueryHandler`.
*   **Validators:** `CrearParametroValidator` (valida códigos no nulos).
*   **Repositories:** `IParametroRepository` (expone `ValidateNoCircularDependency`).
*   **Interfaces:** `ICatalogoCacheService`.
*   **Services:** `CatalogoCacheService` (mantiene caché en memoria de la estructura para evitar colisiones).
*   **Controllers:** `CatalogosController`.
*   **Endpoints OpenAPI:** `GET /api/v1/catalogos/{codigo}`, `POST /api/v1/catalogos/parametros`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Ninguno.
*   **Eventos de Dominio:** `ParametroCreadoEvent`.
*   **Casos de Prueba:** `CrearParametro_ThrowsCircularDependencyException_OnCircularHierarchy`.
*   **Riesgos Técnicos:** Sobrecarga de base de datos debido a la validación recursiva del trigger jerárquico.
*   **Criterios de Aceptación:** Impide la creación de parámetros circulares en base de datos y memoria, retornando `400 Bad Request` con código `CIRCULAR_DEPENDENCY_DETECTED`.

---

### Módulo 5: Solicitudes de Personal
*   **Objetivo:** Registro, validación y control de solicitudes de personal en el Kanban.
*   **Dependencias:** Módulo 4.
*   **Entidades:** `Solicitud`, `SolicitudComentario`.
*   **Value Objects:** Ninguno.
*   **DTOs:** `CrearSolicitudRequest`, `SolicitudDetalleResponse`, `TransitarSolicitudRequest`.
*   **Commands:** `CrearSolicitudCommand`, `TransitarEstadoSolicitudCommand`.
*   **Queries:** `GetSolicitudByIdQuery`, `ListSolicitudesKanbanQuery`.
*   **Handlers:** `CrearSolicitudCommandHandler`, `TransitarEstadoSolicitudCommandHandler`.
*   **Validators:** `CrearSolicitudValidator` (remuneración > 0, habilidades no nulas).
*   **Repositories:** `ISolicitudRepository`.
*   **Interfaces:** Ninguna.
*   **Services:** Ninguno.
*   **Controllers:** `SolicitudesController`.
*   **Endpoints OpenAPI:** `POST /api/v1/solicitudes`, `GET /api/v1/solicitudes/{id}`, `POST /api/v1/solicitudes/{id}/transicion`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Callback n8n de validación de consistencia (`WF-01`).
*   **Eventos de Dominio:** `SolicitudCreadaEvent`, `SolicitudEstadoTransitadoEvent`.
*   **Casos de Prueba:** `TransitarSolicitud_WritesStateHistory_OnSuccess`.
*   **Riesgos Técnicos:** Fuga de visibilidad de áreas debido a fallas en el filtro RLS de Solicitudes.
*   **Criterios de Aceptación:** Creación correcta; RLS filtra registros según `SESSION_CONTEXT('UserArea')` del usuario JWT conectado.

---

### Módulo 6: Perfiles de Cargo (Profesiogramas)
*   **Objetivo:** Diseño y versionamiento del perfil de cargo.
*   **Dependencias:** Módulo 5.
*   **Entidades:** `PerfilCargo`.
*   **Value Objects:** Ninguno.
*   **DTOs:** `PerfilCargoResponse`, `ActualizarPerfilRequest`.
*   **Commands:** `GenerarBorradorPerfilCommand`, `AprobarPerfilCommand`.
*   **Queries:** `GetPerfilBySolicitudQuery`.
*   **Handlers:** `GenerarBorradorPerfilCommandHandler` (invoca webhook n8n), `AprobarPerfilCommandHandler`.
*   **Validators:** `AprobarPerfilValidator`.
*   **Repositories:** `IPerfilCargoRepository`.
*   **Interfaces:** `In8nIntegrationService`.
*   **Services:** `n8nIntegrationService`.
*   **Controllers:** `PerfilesController`.
*   **Endpoints OpenAPI:** `POST /api/v1/perfiles/borrador`, `POST /api/v1/perfiles/{id}/aprobar`, `GET /api/v1/perfiles/solicitud/{solicitudId}`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Workflow `WF-02` (Generación automática de profesiograma con AgentePerfil).
*   **Eventos de Dominio:** `PerfilBorradorGeneradoEvent`, `PerfilAprobadoEvent`.
*   **Casos de Prueba:** `GenerarBorrador_InvokesN8nWebhook_WithApiKeyHeader`.
*   **Riesgos Técnicos:** Respuestas nulas o formateadas incorrectamente de la IA al parsear el profesiograma.
*   **Criterios de Aceptación:** Llama asíncronamente a n8n; guarda versión de perfil y se actualiza mediante callback seguro.

---

### Módulo 7: Gestión de Vacantes
*   **Objetivo:** Publicación y control salarial seguro de vacantes.
*   **Dependencias:** Módulo 6.
*   **Entidades:** `Vacante`.
*   **Value Objects:** `BandaSalarial` (cifrado Always Encrypted).
*   **DTOs:** `CrearVacanteRequest`, `VacanteCardResponse`.
*   **Commands:** `CrearVacanteCommand`, `CerrarVacanteCommand`.
*   **Queries:** `ListVacantesActivasQuery`.
*   **Handlers:** `CrearVacanteCommandHandler` (recibe salarios en claro, los persiste cifrados), `CerrarVacanteCommandHandler`.
*   **Validators:** `CrearVacanteValidator` (BandaMin <= BandaMax).
*   **Repositories:** `IVacanteRepository`.
*   **Interfaces:** Ninguna.
*   **Services:** Ninguno.
*   **Controllers:** `VacantesController`.
*   **Endpoints OpenAPI:** `POST /api/v1/vacantes`, `GET /api/v1/vacantes/activas`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Workflow `WF-03` (Publicación automática en portales externos).
*   **Eventos de Dominio:** `VacanteCreadaEvent`, `VacanteCerradaEvent`.
*   **Casos de Prueba:** `CrearVacante_EncryptsSalaryColumns_InDatabase`.
*   **Riesgos Técnicos:** Error del driver de base de datos al realizar consultas si el certificado de AE no coincide.
*   **Criterios de Aceptación:** Registro exitoso; bandas salariales escritas como `VARBINARY` cifrado en base de datos.

---

### Módulo 8: Postulantes y Postulaciones (M:N)
*   **Objetivo:** Registro de candidatos y vinculación a vacantes activas.
*   **Dependencias:** Módulo 7.
*   **Entidades:** `Postulante`, `Postulacion`.
*   **Value Objects:** `DocumentoIdentidad`.
*   **DTOs:** `RegistrarPostulanteRequest`, `PostulacionStatusResponse`.
*   **Commands:** `RegistrarPostulacionCommand`.
*   **Queries:** `GetPostulantePipelineQuery`.
*   **Handlers:** `RegistrarPostulacionCommandHandler` (valida no duplicidad de postulación por vacante), `GetPostulantePipelineQueryHandler`.
*   **Validators:** `RegistrarPostulanteValidator` (correo estructurado, documento no nulo).
*   **Repositories:** `IPostulanteRepository`, `IPostulacionRepository`.
*   **Interfaces:** `IBlobStorageService`.
*   **Services:** `AzureBlobStorageService` (proveedor de almacenamiento de objetos corporativo; guarda CVs en formato PDF/Word seguro).
*   **Controllers:** `PostulantesController`.
*   **Endpoints OpenAPI:** `POST /api/v1/postulantes`, `GET /api/v1/postulaciones/pipeline`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Callback n8n de recepción de CV (`WF-04`).
*   **Eventos de Dominio:** `PostulanteRegistradoEvent`, `PostulacionCreadaEvent`.
*   **Casos de Prueba:** `RegistrarPostulacion_ThrowsException_OnDuplicateApplication`.
*   **Riesgos Técnicos:** Inyección de archivos maliciosos en la carga del CV (PDF/Word).
*   **Criterios de Aceptación:** Valida extensión y tamaño; guarda en almacenamiento seguro; crea postulación mapeada al pipeline de estados.

---

### Módulo 9: Matching Inteligente (IA)
*   **Objetivo:** Comparación automática de CVs contra profesiogramas mediante IA asíncrona.
*   **Dependencias:** Módulo 8.
*   **Entidades:** `Matching`.
*   **Value Objects:** `ScoreCoincidencia` ( Always Encrypted).
*   **DTOs:** `MatchingCallbackRequest`, `MatchingResultResponse`.
*   **Commands:** `ProcesarMatchingAsincronoCommand`, `RegistrarResultadoMatchingCommand`.
*   **Queries:** `GetMatchingByPostulanteVacanteQuery`.
*   **Handlers:** `ProcesarMatchingAsincronoCommandHandler`, `RegistrarResultadoMatchingCommandHandler`.
*   **Validators:** `MatchingCallbackValidator` (CorrelationId y score obligatorios).
*   **Repositories:** `IMatchingRepository`.
*   **Interfaces:** `In8nIntegrationService`.
*   **Services:** `n8nIntegrationService` (envía carga útil sanitizada sin PII al webhook).
*   **Controllers:** `CallbacksController`.
*   **Endpoints OpenAPI:** `POST /api/v1/callbacks/matching`, `GET /api/v1/matching/postulaciones/{id}`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Workflow `WF-05` (Matching Curricular asíncrono con AgenteMatching).
*   **Eventos de Dominio:** `MatchingCompletadoEvent`.
*   **Casos de Prueba:** `CallbacksMatching_IsIdempotent_OnDuplicateWebhookCalls`.
*   **Riesgos Técnicos:** Webhooks duplicados o fuera de orden que alteren el pipeline del candidato.
*   **Criterios de Aceptación:** Idempotencia activa (caché de 60 segundos por CorrelationId); escribe resultados cifrados en `Matchings`.

---

### Módulo 10: Scoring Explicable (IA)
*   **Objetivo:** Clasificación cualitativa y justificación de idoneidad.
*   **Dependencias:** Módulo 9.
*   **Entidades:** `Scoring`.
*   **Value Objects:** `ScoreFinal` (cifrado Always Encrypted).
*   **DTOs:** `ScoringCallbackRequest`, `ScoringDetailResponse`.
*   **Commands:** `ProcesarScoringCommand`, `RegistrarResultadoScoringCommand`.
*   **Queries:** `GetScoringDetailQuery`.
*   **Handlers:** `ProcesarScoringCommandHandler`, `RegistrarResultadoScoringCommandHandler`.
*   **Validators:** `ScoringCallbackValidator`.
*   **Repositories:** `IScoringRepository`.
*   **Interfaces:** Ninguna.
*   **Services:** Ninguno.
*   **Controllers:** `CallbacksController`.
*   **Endpoints OpenAPI:** `POST /api/v1/callbacks/scoring`, `GET /api/v1/scoring/postulantes/{id}`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Workflow `WF-06` (Evaluación e idoneidad de candidatos con AgenteScoring).
*   **Eventos de Dominio:** `ScoringCompletadoEvent`.
*   **Casos de Prueba:** `Scoring_CalculatesFinalWeightedScore_Correctly`.
*   **Riesgos Técnicos:** Exposición de datos personales del postulante hacia LLMs externos.
*   **Criterios de Aceptación:** Anonimiza el perfil profesional en el DTO enviado; persiste la justificación cualitativa y el score cifrado.

---

### Módulo 11: Coordinación de Entrevistas
*   **Objetivo:** Agenda de citas y aprovisionamiento de salas Teams.
*   **Dependencias:** Módulos 8, 10.
*   **Entidades:** `Entrevista`.
*   **Value Objects:** Ninguno.
*   **DTOs:** `ProgramarEntrevistaRequest`, `EntrevistaDetalleResponse`.
*   **Commands:** `ProgramarEntrevistaCommand`, `ReprogramarEntrevistaCommand`, `CancelarEntrevistaCommand`.
*   **Queries:** `ListEntrevistasByVacanteQuery`.
*   **Handlers:** `ProgramarEntrevistaCommandHandler` (invoca Graph API), `ReprogramarEntrevistaCommandHandler` (valida límite de 3 reintentos).
*   **Validators:** `ProgramarEntrevistaValidator` (FechaHora en UTC a futuro).
*   **Repositories:** `IEntrevistaRepository`.
*   **Interfaces:** `IMicrosoftGraphService`.
*   **Services:** `MicrosoftGraphService` (conector con Exchange/Teams).
*   **Controllers:** `EntrevistasController`.
*   **Endpoints OpenAPI:** `POST /api/v1/entrevistas`, `POST /api/v1/entrevistas/{id}/reprogramar`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Workflow `WF-07` (Envío automático de links de Teams y notificaciones por WhatsApp/Correo).
*   **Eventos de Dominio:** `EntrevistaProgramadaEvent`, `EntrevistaReprogramadaEvent`.
*   **Casos de Prueba:** `ReprogramarEntrevista_Fails_OnFourthAttempt`.
*   **Riesgos Técnicos:** Colisiones de agenda en Exchange del entrevistador.
*   **Criterios de Aceptación:** Crea link de Teams; notifica por correo; bloquea reprogramaciones si supera el límite en el trigger/SLA.

---

### Módulo 12: Emisión de Ofertas
*   **Objetivo:** Formulación y firma de la propuesta salarial económica.
*   **Dependencias:** Módulos 7, 11.
*   **Entidades:** `Ofertas`.
*   **Value Objects:** `BandaSalarialOfrecida` ( Always Encrypted).
*   **DTOs:** `CrearOfertaRequest`, `OfertaStatusResponse`.
*   **Commands:** `CrearOfertaCommand`, `RegistrarRespuestaOfertaCommand`.
*   **Queries:** `GetOfertaByIdQuery`.
*   **Handlers:** `CrearOfertaCommandHandler` (valida que la oferta no exceda la banda máxima de la vacante), `RegistrarRespuestaOfertaCommandHandler`.
*   **Validators:** `CrearOfertaValidator`.
*   **Repositories:** `IOfertaRepository`.
*   **Interfaces:** Ninguna.
*   **Services:** Ninguno.
*   **Controllers:** `OfertasController`.
*   **Endpoints OpenAPI:** `POST /api/v1/ofertas`, `POST /api/v1/ofertas/{id}/responder`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Workflow `WF-08` (Notificación y recolección de firma de la oferta).
*   **Eventos de Dominio:** `OfertaCreadaEvent`, `OfertaFirmadaEvent`, `OfertaRechazadaEvent`.
*   **Casos de Prueba:** `CrearOferta_ThrowsException_IfSalaryExceedsBandaMax`.
*   **Riesgos Técnicos:** Pérdida de llaves criptográficas al validar la oferta en el middleware.
*   **Criterios de Aceptación:** La oferta de salario se persiste encriptada; valida bandas salariales contra la vacante origen de forma segura.

---

### Módulo 13: Contrataciones e Incorporación
*   **Objetivo:** Alta del empleado en el sistema y cierre de vacante.
*   **Dependencias:** Módulo 12.
*   **Entidades:** `Contrataciones`.
*   **Value Objects:** Ninguno.
*   **DTOs:** `RegistrarContratacionRequest`, `ContratacionStatusResponse`.
*   **Commands:** `RegistrarContratacionCommand`.
*   **Queries:** `GetContratacionByIdQuery`.
*   **Handlers:** `RegistrarContratacionCommandHandler` (cierra de forma atómica la vacante y transita al postulante a contratado).
*   **Validators:** `RegistrarContratacionValidator` (FechaIngreso no nula).
*   **Repositories:** `IContratacionRepository`.
*   **Interfaces:** `ISystemIntegratorService`.
*   **Services:** `ERPIntegrationService` (conector REST para el alta en sistemas corporativos de Nacional Seguros).
*   **Controllers:** `ContratacionesController`.
*   **Endpoints OpenAPI:** `POST /api/v1/contrataciones`, `GET /api/v1/contrataciones/{id}`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Workflow `WF-09` (Automatización de alta del usuario e inducción en la empresa).
*   **Eventos de Dominio:** `ContratacionCompletadaEvent`.
*   **Casos de Prueba:** `RegistrarContratacion_ClosesVacante_Atomically`.
*   **Riesgos Técnicos:** Timeout en llamadas remotas REST al ERP durante el guardado de la transacción.
*   **Criterios de Aceptación:** Cierra la vacante (`VAC-CON`); registra al postulante como contratado (`POS-CON`); dispara evento de alta.

---

### Módulo 14: Control de SLAs
*   **Objetivo:** Medición y control de tiempos hábiles de respuesta en el SIR.
*   **Dependencias:** Módulos 4, 13.
*   **Entidades:** `SLA`, `SLAExecution`, `SLAAlert`, `SLAEscalation`, `Feriado`.
*   **Value Objects:** Ninguno.
*   **DTOs:** `SLAExecutionResponse`, `CrearFeriadoRequest`.
*   **Commands:** `IniciarSLAExecutionCommand`, `RegistrarFinSLACommand`, `CrearFeriadoCommand`.
*   **Queries:** `GetSLADeviationListQuery`.
*   **Handlers:** `IniciarSLAExecutionCommandHandler`, `RegistrarFinSLACommandHandler`, `CrearFeriadoCommandHandler`.
*   **Validators:** `CrearFeriadoValidator`.
*   **Repositories:** `ISLAExecutionRepository`, `IFeriadoRepository`.
*   **Interfaces:** `ISlaAlertService`.
*   **Services:** `SlaAlertService` (envía alertas ante proximidad de vencimiento).
*   **Controllers:** `SlaController`.
*   **Endpoints OpenAPI:** `GET /api/v1/sla/desvios`, `POST /api/v1/sla/feriados`.
*   **Stored Procedures:** `sp_SLA_CalcularFechaLimite`.
*   **n8n / Agentes IA:** Workflow `WF-10` (Enrutamiento de alertas y escalaciones por correo/Teams).
*   **Eventos de Dominio:** `SLAExecutionIniciadoEvent`, `SLAVencidoEvent` (dispara escalación).
*   **Casos de Prueba:** `CalcularSLA_ExcludesHolidaysAndWeekends_Correctly`.
*   **Riesgos Técnicos:** Alta latencia si se evalúa día a día el vencimiento en queries masivos.
*   **Criterios de Aceptación:** Utiliza funciones de cálculo orientadas a conjuntos (Set-Based) excluyendo sábados, domingos y feriados nacionales de Bolivia.

---

### Módulo 15: Dashboards Lógicos
*   **Objetivo:** Consolidar métricas globales agregadas para directores y analistas.
*   **Dependencias:** Módulos 13, 14.
*   **Entidades:** Ninguna (Lectura directa).
*   **Value Objects:** Ninguno.
*   **DTOs:** `DashboardEjecutivoResponse`, `DashboardSLAResponse`.
*   **Commands:** Ninguno.
*   **Queries:** `GetDashboardEjecutivoQuery`, `GetDashboardSLAQuery`, `GetDashboardIAQuery`.
*   **Handlers:** `GetDashboardEjecutivoQueryHandler`, `GetDashboardSLAQueryHandler`, `GetDashboardIAQueryHandler`.
*   **Validators:** Ninguno.
*   **Repositories:** `IDashboardQueryRepository` (implementado con Dapper para velocidad).
*   **Interfaces:** Ninguna.
*   **Services:** Ninguno.
*   **Controllers:** `DashboardsController`.
*   **Endpoints OpenAPI:** `GET /api/v1/dashboards/ejecutivo`, `GET /api/v1/dashboards/sla`, `GET /api/v1/dashboards/ia`.
*   **Stored Procedures:** Ninguno.
*   **n8n / Agentes IA:** Dashboard de costos alimentado por telemetría de tokens consumidos en `AgentExecutions`.
*   **Eventos de Dominio:** Ninguno.
*   **Casos de Prueba:** `GetDashboardEjecutivo_ReturnsAggregateMetrics_Fast`.
*   **Riesgos Técnicos:** Bloqueos de lectura (Locking) en tablas altamente transaccionales.
*   **Criterios de Aceptación:** Lecturas sin tracking de EF Core (`AsNoTracking()`) o queries directos con Dapper apuntando a la tabla consolidada `MetricSnapshot`.

---

### Módulo 16: Reportes y Snapshots
*   **Objetivo:** Generación y exportación semanal de métricas de cobertura y costos de IA.
*   **Dependencias:** Módulo 15.
*   **Entidades:** `MetricSnapshot`.
*   **Value Objects:** Ninguno.
*   **DTOs:** `ExportReportRequest`, `MetricSnapshotDto`.
*   **Commands:** `CargarSnapshotMetricasCommand`.
*   **Queries:** `ListMetricSnapshotsQuery`.
*   **Handlers:** `CargarSnapshotMetricasCommandHandler`, `ListMetricSnapshotsQueryHandler`.
*   **Validators:** Ninguno.
*   **Repositories:** `IMetricSnapshotRepository`.
*   **Interfaces:** `IExcelExportService`.
*   **Services:** `ExcelExportService` (genera libros estructurados).
*   **Controllers:** `ReportesController`.
*   **Endpoints OpenAPI:** `POST /api/v1/reportes/snapshot`, `GET /api/v1/reportes/exportar`.
*   **Stored Procedures:** `sp_Reporte_CargarSnapshotMetricas`.
*   **n8n / Agentes IA:** AgenteAnalitico (`AGE-07`) lee los snapshots para redactar reportes automatizados.
*   **Eventos de Dominio:** `SnapshotMetricasCargadoEvent`.
*   **Casos de Prueba:** `CargarSnapshotMetricas_ExecutesSucceeds_InDatabase`.
*   **Riesgos Técnicos:** Error por timeout al compilar reportes de millones de filas de logs.
*   **Criterios de Aceptación:** Genera e inserta correctamente registros de snapshots en `MetricSnapshot` mediante el Stored Procedure optimizado.

---

### Módulo 17: Observabilidad y Auditoría Centralizada
*   **Objetivo:** Interceptar cambios en base de datos e inmutar registros en Ledger.
*   **Dependencias:** Módulos 3, 14.
*   **Entidades:** `AuditLogs`, `StateHistory`, `AgentExecutions`, `IntegrationLogs`, `NotificationLogs`, `WorkflowExecutions`.
*   **Value Objects:** Ninguno.
*   **DTOs:** `AuditLogResponseDto`.
*   **Commands:** `RegistrarAuditLogCommand`, `RegistrarAgentExecutionCommand`.
*   **Queries:** `ListAuditLogsQuery`.
*   **Handlers:** `RegistrarAuditLogCommandHandler`, `RegistrarAgentExecutionCommandHandler`, `ListAuditLogsQueryHandler`.
*   **Validators:** Ninguno.
*   **Repositories:** `IAuditLogRepository`, `IAgentExecutionRepository`.
*   **Interfaces:** `ICurrentUserService`.
*   **Services:** `CurrentUserService` (extrae los claims del JWT).
*   **Controllers:** `AuditoriaController`.
*   **Endpoints OpenAPI:** `GET /api/v1/auditoria/logs`, `GET /api/v1/auditoria/agentes`.
*   **Stored Procedures:** `sp_Auditoria_ConsultarEjecucionesAgente`.
*   **n8n / Agentes IA:** Auditoría inmutable de telemetría y costos de LLMs (`AgentExecutions`).
*   **Eventos de Dominio:** Ninguno.
*   **Casos de Prueba:** `AuditInterceptor_SanitizesSensitiveData_ReplacingWithAsterisks`.
*   **Riesgos Técnicos:** Exposición de salarios en claro dentro de los campos `EstadoAnterior` / `EstadoNuevo` de la bitácora.
*   **Criterios de Aceptación:** `AuditInterceptor` sanitiza campos con atributo `[SensitiveData]` reemplazando valores por `'*****'`. Las tablas Ledger están configuradas en filegroups separados.

---

### Módulo 18: Optimización y Hardening
*   **Objetivo:** Afinamiento de rendimiento, Query Store, índices y compresión de logs.
*   **Dependencias:** Módulos 1 a 17.
*   **Entidades:** Ninguna.
*   **Value Objects:** Ninguno.
*   **DTOs:** Ninguno.
*   **Commands:** `EjecutarCompresionTablasCommand`, `OptimizarIndicesCommand`.
*   **Queries:** Ninguno.
*   **Handlers:** `EjecutarCompresionTablasCommandHandler`, `OptimizarIndicesCommandHandler`.
*   **Validators:** Ninguno.
*   **Repositories:** Ninguno.
*   **Interfaces:** Ninguna.
*   **Services:** `DatabaseMaintenanceService` (ejecuta rutinas semanales).
*   **Controllers:** `MaintenanceController` (restringido a rol `Administrador`).
*   **Endpoints OpenAPI:** `POST /api/v1/maintenance/optimize`.
*   **Stored Procedures:** `sp_Mantenimiento_OptimizarIndices`, `sp_Mantenimiento_ActualizarEstadisticas`, `sp_Mantenimiento_ComprimirDatosHistoricos`.
*   **n8n / Agentes IA:** Ninguno.
*   **Eventos de Dominio:** Ninguno.
*   **Casos de Prueba:** `OptimizeMaintenance_RunsSucceeds_InDatabase`.
*   **Riesgos Técnicos:** Bloqueos en producción por mantenimiento en horas pico.
*   **Criterios de Aceptación:** Ejecución exitosa de rutinas de mantenimiento; estadísticas actualizadas con `FULLSCAN`; compresión PAGE en tablas Ledger activada.

---

## 5. Estándares de Desarrollo del Backend

Para garantizar la homogeneidad y mantenimiento de la base de código, se definen las siguientes directrices obligatorias:

### 5.1 Convenciones de Nombres
*   **Clases y Métodos:** `PascalCase` (ej: `UsuarioService`, `SaveChangesAsync`).
*   **Variables Locales y Parámetros:** `camelCase` (ej: `usuarioId`, `correoElectronico`).
*   **Interfaces:** Prefijo `I` en `PascalCase` (ej: `IUsuarioRepository`).
*   **Tablas de SQL Server (en C#):** Singulares en el modelo de entidades, pero mapeadas en plural a la persistencia (`.ToTable("Usuarios")`).

### 5.2 Manejo Global de Excepciones
*   El `ExceptionHandlingMiddleware` intercepta todos los errores de la aplicación.
*   **Sanitización Defensiva:** Si la excepción es de base de datos (`SqlException`), se suprime el mensaje de error técnico del JSON saliente y del log físico (reemplazando cadenas de conexión y parámetros) para evitar la filtración de metadatos del servidor.

### 5.3 Validaciones en el Pipeline de MediatR
*   Toda petición de mutación (`Command`) pasa por un `ValidationBehavior` inyectado en el pipeline.
*   FluentValidation evalúa las reglas. Si existen fallas, aborta la ejecución antes de llegar al Handler y lanza una `ValidationException`, que el middleware expone como `400 Bad Request` estructurado.

### 5.4 Control Transaccional en Base de Datos
*   El pipeline de MediatR implementa un `TransactionBehavior` que ejecuta un `Commit` si el handler termina con éxito o un `Rollback` inmediato ante cualquier excepción física o de negocio.

---

## 6. Estrategia de Pruebas del Backend

*   **Pruebas Unitarias:**
    *   *Mocks:* Utilizar `NSubstitute` para simular dependencias de persistencia (`DbContext`) y servicios de infraestructura (`n8nClient`).
    *   *Aseveraciones:* Utilizar `FluentAssertions` para validaciones legibles en español.
*   **Pruebas de Integración (Base de Datos):**
    *   *Estrategia:* Utilizar `Testcontainers` para arrancar dinámicamente contenedores compatibles con SQL Server 2022.
    *   *Objetivo:* Validar la aplicación de RLS, cifrado Always Encrypted in-place, y disparadores de dependencias circulares.
*   **Cobertura Mínima:** Cobertura global del **80%** de código en las capas de `Application` y `Domain`.

---

## 7. Estrategia DevOps y CI/CD del Backend

*   **GitFlow Estándar:** Desarrollo en `feature/*`, integración en `develop`, pruebas pre-PROD en `release/*` y producción en `main`.
*   **Pipeline CI (Orquestador de despliegue configurado para CI/CD):**
    *   Fase 1: Restaurar dependencias (`dotnet restore`).
    *   Fase 2: Compilar solución (`dotnet build --configuration Release --no-restore`).
    *   Fase 3: Ejecutar pruebas unitarias (`dotnet test --no-build --verbosity normal`).
    *   Fase 4: Análisis estático de vulnerabilidades SAST (SonarQube).
*   **Pipeline CD (Despliegue Continuo):**
    *   Generación automática de imágenes de contenedores del API backend.
    *   Despliegue en IIS o orquestador de contenedores empresarial de Nacional Seguros.
    *   Ejecución de dry-runs T-SQL sobre la base de datos de producción mediante herramientas de migración (`EF Core Migrations Bundle`).

---

## 8. Checklist de Desarrollo y QA del Backend

Un módulo se considera listo para ser promovido a QA y preproducción si y solo si cumple con los siguientes controles de calidad:

```
[Código Compila] ──► [Pruebas Unitarias >80%] ──► [Auditoría Ledger] ──► [Sanitización Completa] ──► [Listo para QA]
```

### 8.1 Checklist de Desarrollo (Tech Lead)
*   [ ] El código compila al 100% sin advertencias severas de compilación.
*   [ ] Todos los endpoints OpenAPI correspondientes responden según el contrato Swagger.
*   [ ] Las consultas de lectura pesadas utilizan explicitamente `.AsNoTracking()`.
*   [ ] Se validó de forma defensiva la no recursividad jerárquica en los catálogos.
*   [ ] Las columnas salariales sensibles están encriptadas y protegidas por Always Encrypted.

### 8.2 Checklist de QA (QA Lead)
*   [ ] Pruebas unitarias completadas con cobertura de código superior al 80%.
*   [ ] Middleware de CorrelationId propaga correctamente el identificador en los headers HTTP.
*   [ ] El middleware de excepciones sanitiza las trazas de SQL Server previniendo fuga de credenciales.
*   [ ] La API de callbacks e integraciones responde con idempotencia ante llamadas duplicadas de n8n.
*   [ ] Las acciones de cambio de estado se escriben de forma inmutable en la tabla Ledger `StateHistory`.

---
