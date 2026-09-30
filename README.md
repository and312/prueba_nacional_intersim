# SIR — Sistema Inteligente de Reclutamiento
### Nacional Seguros · Intersim

---

## 1. Descripción

El **SIR (Sistema Inteligente de Reclutamiento)** es la plataforma de gestión de procesos de reclutamiento de Nacional Seguros. Permite gestionar solicitudes de vacante, perfiles de cargo, postulantes (internos y externos), matching de candidatos y generación de documentos (perfiles estructurados, resúmenes ejecutivos) asistida por Inteligencia Artificial.

El sistema opera en dos canales:
- **Backoffice Web**: interfaz Angular para el equipo de RRHH.
- **WhatsApp / n8n**: automatización de flujos de aprobación, generación de perfiles y notificaciones mediante n8n.

---

## 2. Arquitectura y Tecnologías

### Backend
| Tecnología | Versión | Rol |
|:-----------|:-------:|:----|
| **.NET / ASP.NET Core** | 8.0 | API REST principal |
| **Entity Framework Core** | 8.x | ORM / Migraciones |
| **SQL Server** | 2022 | Base de datos relacional |
| **Redis** | 7.x | Caché y sesiones |
| **Serilog** | - | Logging estructurado |

### Frontend
| Tecnología | Versión | Rol |
|:-----------|:-------:|:----|
| **Angular** | 22.x | SPA del Backoffice |
| **Angular Material** | 22.x | UI Components |
| **Angular CDK** | 22.x | Utilities de UI |
| **TypeScript** | 6.0.x | Lenguaje |

### Integración / Automatización
| Tecnología | Rol |
|:-----------|:----|
| **n8n** | Motor de workflows / automatización WhatsApp |
| **LDAP / Active Directory** | Autenticación corporativa (simulable en desarrollo) |
| **JWT** | Autenticación de sesiones Backoffice |

### Patrón Arquitectónico — Clean Architecture

```
┌──────────────────────────────────────────┐
│              Frontend (Angular 22)       │
└─────────────────┬────────────────────────┘
                  │  HTTP REST /api/v1
┌─────────────────▼────────────────────────┐
│          Backend (ASP.NET Core 8)        │
│  ┌─────────┐ ┌─────────────┐ ┌────────┐ │
│  │   Api   │ │ Application │ │ Domain │ │
│  └─────────┘ └─────────────┘ └────────┘ │
│  ┌─────────────────┐ ┌─────────────────┐ │
│  │   Persistence   │ │  Infrastructure │ │
│  │   (EF Core)     │ │ (Redis, AD, ..) │ │
│  └─────────────────┘ └─────────────────┘ │
└──────────────┬───────────────────────────┘
        ┌──────┴──────┐
   ┌────▼────┐   ┌────▼────┐
   │SQL Server│   │  Redis  │
   └──────────┘   └─────────┘
```

---

## 3. Estructura del Proyecto

```
nacional-intersim/
├── src/
│   ├── NacionalSeguros.Api/              # Controllers, Program.cs, Middlewares
│   │   ├── Controllers/
│   │   │   ├── InternalApiController.cs  # Endpoints para n8n
│   │   │   ├── SolicitudesController.cs
│   │   │   ├── PerfilesController.cs
│   │   │   └── ...
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   ├── NacionalSeguros.Application/      # CQRS, Commands, Queries, Handlers
│   ├── NacionalSeguros.Domain/           # Entidades, Interfaces, Value Objects
│   ├── NacionalSeguros.Persistence/      # DbContext, Configurations, Migrations
│   │   ├── Context/ApplicationDbContext.cs
│   │   ├── Configurations/               # IEntityTypeConfiguration por entidad
│   │   └── Migrations/
│   ├── NacionalSeguros.Infrastructure/   # Redis, AD, servicios externos
│   ├── NacionalSeguros.Contracts/        # DTOs, modelos de contrato
│   └── NacionalSeguros.Shared/           # Utilidades comunes
│
├── frontend/                             # Angular 22 Backoffice
│   ├── src/
│   │   ├── app/
│   │   └── environments/
│   ├── proxy.conf.json                   # Proxy dev → backend :5000
│   └── package.json
│
├── sql/                                  # Scripts de instalación de la BD
│   ├── instalar_sir.sql                  # ← SCRIPT MAESTRO (ejecutar este)
│   ├── 00_DATABASE/                      # Creación de la base de datos
│   ├── 01_TABLES/                        # Creación de tablas
│   ├── 02_CONSTRAINTS/                   # Foreign keys, constraints
│   ├── 03_INDEXES/                       # Índices de cobertura
│   ├── 04_SECURITY/                      # Seguridad, usuarios SQL
│   ├── 05_VIEWS/                         # Vistas de negocio
│   ├── 06_FUNCTIONS/                     # Funciones SQL
│   ├── 07_STORED_PROCEDURES/             # Procedimientos almacenados
│   ├── 08_TRIGGERS/                      # Triggers
│   └── 09_SEED_DATA/                     # Datos semilla obligatorios
│
├── tests/                                # Tests unitarios e integración
├── Dockerfile                            # Docker del backend
├── docker-compose.yml                    # Orquestación completa
└── NacionalSeguros.sln
```

---

## 4. Requisitos Previos

### Para desarrollo local

| Herramienta | Versión mínima | Descarga |
|:------------|:--------------:|:---------|
| .NET SDK | **8.0** | https://dotnet.microsoft.com/download/dotnet/8 |
| Node.js | **20 LTS** | https://nodejs.org |
| Angular CLI | **22.x** | `npm install -g @angular/cli@22` |
| SQL Server | **2019 / 2022** | https://www.microsoft.com/sql-server |
| Redis | **7.x** | https://redis.io / WSL2 |
| Git | Cualquiera | https://git-scm.com |

### Para despliegue con Docker

| Herramienta | Versión mínima |
|:------------|:--------------:|
| Docker Desktop | 4.x |
| Docker Compose | v2 |

> El proyecto incluye `global.json` que fija el SDK en `8.0.100` con `rollForward: latestFeature`.

---

## 5. Configuración de Base de Datos

### ¿Cómo obtiene la BD un desarrollador nuevo?

Hay dos caminos:

#### 🟢 Camino A — Instalación desde cero (entorno local limpio)

El proyecto incluye un sistema completo de scripts SQL en `sql/`:

1. Tener SQL Server instalado y corriendo.
2. Abrir **SSMS** y conectarse a la instancia.
3. Activar **Modo SQLCMD**: `Menú Consulta > Modo SQLCMD`.
4. Abrir y ejecutar `sql/instalar_sir.sql`.

Esto crea automáticamente en orden:
- La base de datos `SIR_NacionalSeguros`
- Toda la estructura (tablas, constraints, índices, vistas, funciones, triggers)
- Los **datos semilla obligatorios** (catálogos, estados, roles, parámetros)

#### 🔵 Camino B — Conectarse al servidor compartido (recomendado para el equipo)

Si existe un SQL Server de desarrollo compartido:
1. Editar `appsettings.Development.json` con el connection string del servidor.
2. No se necesita instalar SQL Server localmente.

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=IP_SERVIDOR,1433;Database=SIR_NacionalSeguros;User Id=usr_ns;Password=TuPassword;TrustServerCertificate=True;"
  }
}
```

### Crear el usuario SQL para la aplicación (Camino A)

```sql
CREATE LOGIN usr_ns WITH PASSWORD = 'TuPasswordAqui';
USE SIR_NacionalSeguros;
CREATE USER usr_ns FOR LOGIN usr_ns;
ALTER ROLE db_datareader ADD MEMBER usr_ns;
ALTER ROLE db_datawriter ADD MEMBER usr_ns;
-- Solo en desarrollo (para que EF Core aplique migraciones):
ALTER ROLE db_ddladmin ADD MEMBER usr_ns;
```

### Migraciones de EF Core (alternativa al script SQL)

```bash
dotnet ef database update \
  --project src/NacionalSeguros.Persistence \
  --startup-project src/NacionalSeguros.Api
```

> ⚠️ Las migraciones crean la estructura pero **no incluyen datos semilla**. Ejecutar `sql/09_SEED_DATA/semilla.sql` después.

---

## 6. Configuración del Backend

Archivo: `src/NacionalSeguros.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SIR_NacionalSeguros;User Id=usr_ns;Password=TuPassword;TrustServerCertificate=True;"
  },
  "Redis": {
    "ConnectionString": "127.0.0.1:6379"
  },
  "Jwt": {
    "Issuer": "NacionalSeguros.SIR",
    "Audience": "NacionalSeguros.SIR.Clients",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7,
    "Secret": "CAMBIAR_CLAVE_SEGURA_MINIMO_32_CHARS!"
  },
  "Ldap": {
    "Url": "ldaps://tu-servidor-ad:636",
    "Domain": "tudominio.com"
  },
  "ActiveDirectory": {
    "Simulate": true
  },
  "InternalApi": {
    "ApiKeys": [
      {
        "Nombre": "Workflow RRHH",
        "Key": "CAMBIAR_API_KEY_N8N_RRHH",
        "Workflow": "WorkflowRRHH",
        "Permisos": "solicitudes.crear,solicitudes.editar,..."
      },
      {
        "Nombre": "Workflow IA",
        "Key": "CAMBIAR_API_KEY_N8N_IA",
        "Workflow": "WorkflowIA",
        "Permisos": "perfiles.generar,perfiles.callback,perfiles.read,perfiles.write"
      }
    ]
  },
  "Webhooks": {
    "AresResumidorUrl": "https://tu-n8n.cloud/webhook/ares-resumidor",
    "EnviarAreaWebhookUrl": "https://tu-n8n.cloud/webhook/enviar_area",
    "PerfilVacanteResumenUrl": "https://tu-n8n.cloud/webhook/perfil_vacante_resumen"
  },
  "FrontendBaseUrl": "http://localhost:4200"
}
```

---

## 7. Configuración del Frontend

### Proxy de desarrollo: `frontend/proxy.conf.json`

```json
{
  "/api/v1": {
    "target": "http://localhost:5000",
    "secure": false,
    "changeOrigin": true
  }
}
```

Todas las llamadas a `/api/v1/*` se redirigen al backend en `localhost:5000`.

### Environments

```typescript
// frontend/src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: '/api/v1'
};

// frontend/src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://nacionalseguros.intersim.cloud/api/v1'
};
```

---

## 8. Variables y Archivos de Configuración

| Archivo | Entorno | Contenido |
|:--------|:-------:|:---------|
| `appsettings.json` | Todos | Estructura base y valores por defecto |
| `appsettings.Development.json` | Desarrollo | Overrides locales (BD, flags) |
| `appsettings.Production.json` | Producción | **No en repo** — se configura en el servidor |
| `frontend/proxy.conf.json` | Dev | Proxy Angular CLI → backend local |
| `frontend/src/environments/environment.ts` | Dev | URL de la API en desarrollo |
| `frontend/src/environments/environment.prod.ts` | Prod | URL de la API en producción |
| `docker-compose.yml` | Docker | Variables de entorno para contenedores |

### Variables de entorno para Docker / Servidor de producción

```bash
ConnectionStrings__DefaultConnection="Server=sql-host,1433;Database=SIR_NacionalSeguros;..."
Redis__ConnectionString="redis-host:6379"
Jwt__Secret="CLAVE_JWT_SEGURA_MINIMO_32_CHARS"
ActiveDirectory__Simulate="false"
InternalApi__ApiKeys__0__Key="clave-n8n-rrhh"
InternalApi__ApiKeys__1__Key="clave-n8n-ia"
```

---

## 9. Ejecución Local

### Backend

```bash
# 1. Clonar el repositorio
git clone <url-del-repo>
cd nacional-intersim

# 2. Restaurar paquetes NuGet
dotnet restore

# 3. Configurar connection string en appsettings.Development.json

# 4. (Opcional) Aplicar migraciones si la BD es nueva
dotnet ef database update \
  --project src/NacionalSeguros.Persistence \
  --startup-project src/NacionalSeguros.Api

# 5. Ejecutar
dotnet run --project src/NacionalSeguros.Api
# Escucha en:  http://localhost:5000
# Swagger en:  http://localhost:5000/swagger
```

### Frontend

```bash
cd frontend

# 1. Instalar dependencias
npm install

# 2. Ejecutar servidor de desarrollo
npm start
# Disponible en: http://localhost:4200
```

---

## 10. Compilación y Despliegue

### Compilar el Backend

```bash
dotnet publish src/NacionalSeguros.Api/NacionalSeguros.Api.csproj \
  -c Release \
  -o ./publish \
  /p:UseAppHost=false
```

### Compilar el Frontend

```bash
cd frontend
npm run build
# Genera los archivos en: frontend/dist/
```

### Despliegue con Docker Compose

```bash
# Desde la raíz del proyecto
docker compose up -d --build

# Servicios:
#  - Backend:  http://host:8080
#  - Frontend: http://host:8000
```

Las variables de entorno se pueden inyectar con un archivo `.env` en la raíz:

```env
# .env — NO subir al repositorio
DB_CONNECTION=Server=tu-sql,1433;Database=SIR_NacionalSeguros;User Id=usr_ns;Password=Pass;TrustServerCertificate=True;
REDIS_CONNECTION=redis-host:6379
```

### Actualización rápida del backend en servidor

```bash
# 1. Subir los archivos modificados al servidor
# 2. En el servidor, reconstruir solo el API:
docker compose -f /opt/docker/docker-compose.yml up -d --build nacionalseguros.api
```
