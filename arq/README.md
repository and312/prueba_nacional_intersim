# Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
## Módulo 00: Solución Base .NET 8 / C# 12

Este repositorio contiene la estructura fundacional del Backend del **Sistema Inteligente de Reclutamiento (SIR)** de Nacional Seguros. Diseñado bajo los principios de **Clean Architecture**, **Domain-Driven Design (DDD)** y **CQRS**, utilizando **.NET 8** y **Entity Framework Core 9**.

---

## 1. Estructura de la Solución (NacionalSeguros.sln)

La solución está organizada en 8 proyectos estructurados jerárquicamente para evitar dependencias circulares y mantener el núcleo de dominio aislado de la infraestructura técnica:

```
NacionalSeguros.sln
├── src/
│   ├── NacionalSeguros.Shared/         - Tipos primitivos, patrón Result, excepciones globales.
│   ├── NacionalSeguros.Domain/         - Entidades puras, Value Objects, Domain Events e interfaces de repositorios.
│   ├── NacionalSeguros.Contracts/      - DTOs puros de Requests y Responses (contratos de API).
│   ├── NacionalSeguros.Application/    - Casos de uso, Commands, Queries, Validadores (FluentValidation), behaviors.
│   ├── NacionalSeguros.Persistence/    - Acceso físico a SQL Server 2022 mediante EF Core 9 y Dapper.
│   ├── NacionalSeguros.Infrastructure/ - Servicios externos (JWT, Capa de Caché Distribuida, Directorio de Identidad Corporativo, Polly, etc.).
│   └── NacionalSeguros.Api/            - Presentación: Controladores ASP.NET Core, middlewares, Swagger.
└── tests/
    └── NacionalSeguros.Tests/          - Pruebas unitarias (xUnit, FluentAssertions, Moq).
```

### Tabla de Referencias de Proyectos

*   **Domain** depende únicamente de **Shared**.
*   **Contracts** depende únicamente de **Shared**.
*   **Application** depende de **Domain**, **Contracts** y **Shared**.
*   **Persistence** depende de **Domain**, **Application** y **Shared**.
*   **Infrastructure** depende de **Application**, **Domain** y **Shared**.
*   **Api** depende de **Application**, **Persistence**, **Infrastructure** y **Contracts**.
*   **Tests** depende de **Api** (e indirectamente de todos los demás proyectos).

---

## 2. Tecnologías y Configuración Integrada

La solución base tiene pre-configuradas las siguientes tecnologías clave:

*   **Entity Framework Core 9:** Configurado con `ApplicationDbContext` y soporte para inyección automática de configuraciones de Fluent API en el ensamblado de persistencia.
*   **Dapper:** Listo para usarse junto a EF Core para consultas de alto rendimiento (Queries en CQRS).
*   **Caché distribuida:** Configurada para almacenamiento en caché e implementación del filtro de idempotencia.
*   **Seguridad y JWT (CWE-798 & CWE-287):** Configuración de tokens JWT con validación estricta de firma, emisor, audiencia y tiempo de expiración. Incorpora una resolución segura del Secret Key: si no se encuentra configurado en Variables de Entorno o Secrets, genera automáticamente una clave criptográfica segura y emite una advertencia de desarrollo en los logs.
*   **Serilog Structured Logging:** Configurado para registrar trazas JSON enriquecidas con el `CorrelationId`.
*   **Correlation ID:** Middleware global que propaga el encabezado `X-Correlation-ID` en las solicitudes y respuestas HTTP para permitir la trazabilidad completa.
*   **Manejo Global de Excepciones (CWE-209):** Middleware que intercepta y sanitiza las excepciones técnicas de SQL Server (`SqlException`) y responde con un formato limpio compatible con RFC 7807, impidiendo la filtración de metadatos de base de datos a los usuarios finales.
*   **OpenTelemetry:** Configurado para recolectar y exportar trazas automáticas de solicitudes HTTP entrantes y salientes.
*   **Health Checks:** Endpoint `/health` pre-configurado para monitorear el estado de la conexión a SQL Server y la capa de caché distribuida.
*   **Swagger / OpenAPI:** Autogeneración de documentación interactiva protegida con seguridad Bearer JWT.

---

## 3. Requisitos Previos

Asegúrese de tener instalados los siguientes componentes:
1.  **.NET 8.0 SDK** (o superior compatible).
2.  **Motor de contenedores local compatible** (opcional, para levantar SQL Server 2022 y la capa de caché distribuida mediante la orquestación de contenedores local).

---

## 4. Guía de Compilación y Ejecución

### 4.1 Compilación
Para compilar la solución completa y comprobar que no existan errores ni advertencias, ejecute el siguiente comando desde la raíz del proyecto:

```powershell
dotnet build
```

### 4.2 Ejecución de Pruebas Unitarias
Para correr todas las pruebas automatizadas del sistema:

```powershell
dotnet test
```

### 4.3 Inicializar Servicios de Infraestructura Local
Si desea levantar una instancia local de SQL Server 2022 y la capa de caché distribuida en contenedores locales:

```powershell
[comando de contenedores] up -d
```

### 4.4 Ejecución de la API
Para iniciar la API en modo de desarrollo local (el Swagger estará disponible en `http://localhost:5000/swagger` o `https://localhost:5001/swagger`):

```powershell
dotnet run --project src/NacionalSeguros.Api/NacionalSeguros.Api.csproj
```
