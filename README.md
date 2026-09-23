# 🏢 SIR - Sistema de Inteligencia y Reclutamiento (Nacional Seguros)

Bienvenido al repositorio oficial del **Sistema de Inteligencia y Reclutamiento (SIR)** para **Nacional Seguros**.
Este proyecto contiene la solución completa compuesta por la **API Backend en .NET 8**, el **Frontend Web en Angular 19**, scripts de Base de Datos y las configuraciones de orquestación con **Docker**.

---

## 🏗️ Arquitectura del Sistema

La solución está estructurada bajo estándares de arquitectura limpia (*Clean Architecture*) y separación de responsabilidades:

```text
nacional-seguros-sir/
├── docker-compose.yml              # Orquestador unificado Docker
├── Dockerfile                      # Build multi-stage para Backend .NET 8
├── nacionalseguros.nginx.conf      # Configuración de Reverse Proxy Nginx para Host
├── NacionalSeguros.sln             # Solución C# .NET 8
├── README.md                       # Manual de instalación y arquitectura
│
├── frontend/                       # 🌐 FRONTEND (Angular 19 + TailwindCSS)
│   ├── Dockerfile                  # Build multi-stage (Node 22 + Nginx Alpine)
│   ├── nginx.conf                  # Router interno de Nginx SPA
│   ├── package.json
│   └── src/                        # Componentes, servicios y páginas
│
├── src/                            # ⚙️ BACKEND (.NET 8 Clean Architecture)
│   ├── NacionalSeguros.Api/        # Controladores, Middlewares, Webhooks y Swagger
│   ├── NacionalSeguros.Application/# Casos de uso, DTOs, Comandos y CQRS
│   ├── NacionalSeguros.Contracts/  # Contratos e Interfaces de API
│   ├── NacionalSeguros.Domain/     # Entidades de dominio y Reglas de Negocio
│   ├── NacionalSeguros.Infrastructure/# Integraciones con AI, Notificaciones e IO
│   ├── NacionalSeguros.Persistence/# EF Core 9 (SQL Server 2022) y Repositorios
│   └── NacionalSeguros.Shared/     # Utilitarios y primitivas compartidas
│
├── sql/                            # 🗄️ BASE DE DATOS
│   └── (Scripts DDL/DML, vistas, procedimientos almacenados y seeds)
│
└── docs/postman/                   # 🧪 PRUEBAS Y COLECCIONES POSTMAN
    ├── SIR.postman_collection.json
    └── Entornos (QA, Producción, Local)
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
