# 🏢 SIR - Sistema Inteligente de Reclutamiento (Nacional Seguros)

Bienvenido al repositorio oficial del **Sistema Inteligente de Reclutamiento (SIR)** para **Nacional Seguros**.  
Este proyecto contiene la solución completa compuesta por la **API Backend en .NET 8 (`ns-sir-be`)**, el **Frontend Web en Angular 19/22 (`ns-sir-app`)**, scripts de Base de Datos y las configuraciones de orquestación con **Docker**.

---

## 🏗️ Arquitectura del Sistema y Estructura del Repositorio

La solución está estructurada bajo estándares de arquitectura limpia (*Clean Architecture*), separación de responsabilidades y gobernanza técnica mediante **`AGENTS.md`**:

```text
prueba_nacional_intersim/
├── AGENTS.md                       # 🤖 Guía Maestra para IA/Devs (ns-sir-be - Reglas y Prioridad de Fuentes)
├── README.md                       # 📖 Manual de Instalación y Arquitectura del Sistema
├── Dockerfile                      # Build multi-stage para Backend .NET 8
├── docker-compose.yml              # Orquestador unificado Docker
├── NacionalSeguros.sln             # Solución C# .NET 8
│
├── database/                       # 🗄️ BASE DE DATOS (Scripts SQL DDL/DML, Vistas, Procedures y Seeds)
│   └── migration.sql
│
├── deploy/                         # 🚀 DESPLIEGUE Y PROXIES (Nginx Reverse Proxy host)
│   └── nacionalseguros.nginx.conf
│
├── docs/                           # 📚 DOCUMENTACIÓN TÉCNICA Y REGLAS DE NEGOCIO
│   ├── api/                        # Colecciones Postman y Contratos OpenAPI / Swagger
│   ├── architecture/               # Decisiones Técnicas (Clean Architecture, DDD, CQRS)
│   ├── features/                   # Requerimientos Funcionales y Casos de Uso del PRD
│   └── rules/                      # Reglas de Negocio (Permisos, Estados, RLS, Auditoría)
│
├── frontend/                       # 🌐 FRONTEND (ns-sir-app - Angular 19/22 Standalone + TailwindCSS)
│   ├── AGENTS.md                   # 🤖 Guía Técnica de Frontend para IA/Devs (Stack y Patrones RxJS)
│   ├── Dockerfile                  # Build multi-stage (Node + Nginx Alpine)
│   ├── nginx.conf                  # Router interno SPA Nginx
│   ├── package.json
│   ├── tailwind.config.js
│   └── src/                        # Componentes, Servicios, Guards y Vistas
│
├── src/                            # ⚙️ BACKEND (.NET 8 Clean Architecture)
│   ├── NacionalSeguros.Api/        # Controladores REST, Middlewares, Webhooks y Swagger
│   ├── NacionalSeguros.Application/# CQRS Comandos/Queries, DTOs y Handlers MediatR
│   ├── NacionalSeguros.Contracts/  # Interfaces y Contratos de la API
│   ├── NacionalSeguros.Domain/     # Entidades de Dominio, Valuaciones y Reglas de Negocio
│   ├── NacionalSeguros.Infrastructure/# Integraciones con AI, Notificaciones e IO
│   ├── NacionalSeguros.Persistence/# EF Core 9 (SQL Server 2022) y Repositorios
│   └── NacionalSeguros.Shared/     # Utilitarios y Kernel compartido
│
└── tests/                          # 🧪 PRUEBAS UNITARIAS E INTEGRACIÓN
    └── NacionalSeguros.Tests/      # Suite de Pruebas xUnit / Moq / FluentAssertions
```

---

## 🚀 Despliegue con Docker Compose

### Requisitos Previos en el Servidor
* Docker Engine 24.0+ y Docker Compose v2+
* Nginx (opcional como Reverse Proxy principal en el servidor host)
* SQL Server 2022 y Redis 7+ activos en el host o red de Docker.

### Pasos de Despliegue

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/and312/prueba_nacional_intersim.git
   cd prueba_nacional_intersim
   ```

2. Compilar y levantar los contenedores con Docker Compose:
   ```bash
   docker compose up -d --build
   ```

3. Verificar el estado de los contenedores:
   ```bash
   docker ps
   ```

---

## 🌐 Endpoints y Documentación Interactiva (Swagger)

* **Swagger UI (Documentación API):** `http://<IP_SERVIDOR>:8080/swagger`
* **Frontend Angular:** `http://<IP_SERVIDOR>:8000/`
* **API Base Path:** `http://<IP_SERVIDOR>:8080/api/v1/`
