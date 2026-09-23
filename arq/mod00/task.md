# Checklist de Construcción: Módulo 00 - Solución Base

- [x] Crear estructura de directorios y solución `NacionalSeguros.sln`
- [x] Crear los 8 proyectos (.csproj) de la solución
- [x] Agregar referencias de proyectos respetando Clean Architecture
- [x] Crear archivos de configuración global (`Directory.Build.props`, `global.json`, `.editorconfig`, `.gitignore`, `Dockerfile`, `docker-compose.yml`)
- [x] Agregar paquetes NuGet en los respectivos proyectos
- [x] Crear tipos base y primitivas en `NacionalSeguros.Shared`
- [x] Crear base DDD en `NacionalSeguros.Domain`
- [x] Crear `ApplicationDbContext`, `UnitOfWork` y repositorios base en `NacionalSeguros.Persistence`
- [x] Implementar middleware y pipeline behaviors en `NacionalSeguros.Application` y `NacionalSeguros.Api`
- [x] Configurar inyección de dependencias global, JWT, Serilog, Swagger y Health Checks en `NacionalSeguros.Api`
- [x] Compilar y validar la solución (`dotnet build`)
- [x] Crear el README.md y walkthrough de entrega
