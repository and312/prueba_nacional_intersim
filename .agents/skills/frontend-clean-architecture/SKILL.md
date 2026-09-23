---
name: frontend-clean-architecture
description: "Estándares y reglas de arquitectura limpia para el Frontend Angular 19 de SIR Nacional Seguros. Contenedores técnicos en inglés (components, services, models, constants, utils), nombres de archivo en español (kebab-case), patrón Singleton en servicios (@Injectable providedIn root), reactividad con Angular Signals y sintaxis moderna de control flow (@if, @for)."
---

# Reglas de Arquitectura e Invariantes del Frontend (Angular 19 SPA)

Esta skill especifica las reglas obligatorias de diseño, estructura de archivos y patrones de desarrollo acordadas con el equipo para el proyecto **SIR Nacional Seguros Frontend**.

## 1. Convención de Estructura de Directorios (Contenedores Técnicos)

Dentro de cada módulo o sub-feature en `src/app/features/[nombre-modulo]/`, los contenedores técnicos DEBEN estar escritos en **inglés**:

- `components/`: Componentes de UI (ej: `profile-table`, `observaciones-perfil`).
- `services/`: Servicios de estado e integración de la feature.
- `models/`: Interfaces, tipos TypeScript, enums y DTOs de la feature.
- `constants/`: Enumeraciones, catálogos y archivos de configuración.
- `utils/`: Helpers y funciones puras de utilidad.

## 2. Convención de Nomenclatura de Archivos (Nombres en Español)

- **Kebab-case en Español**: Los nombres de los archivos se mantienen en español respetando el dominio del negocio creado por el equipo (ej: `observaciones-perfil.component.ts`, `configuracion-estados-perfil.ts`, `perfil.model.ts`).
- **Sufijos por Tipo**:
  - Componentes: `[nombre-espanol].component.ts` / `.html` / `.scss`
  - Servicios: `[nombre-espanol].service.ts`
  - Modelos: `[nombre-espanol].model.ts` o `.enum.ts`
  - Constantes/Configuración: `[nombre-espanol].config.ts` o `.ts`

## 3. Patrón Singleton & Servicios de Estado

- Todos los servicios deben incluir `@Injectable({ providedIn: 'root' })` para garantizar una única instancia viva durante la navegación SPA.
- Evitar proveer servicios en el arreglo `providers: [...]` de componentes individuales salvo que sea estrictamente necesario un alcance aislado.

## 4. Control Flow y Reactividad (Angular 19)

- **Signals**: Utilizar `signal()`, `computed()` y `effect()` para manejar el estado reactivo en componentes.
- **Control Flow**: Utilizar la sintaxis nativa `@if`, `@for`, `@switch` en lugar de las directivas legacy `*ngIf`, `*ngFor`.
- **Directiva Track en Listas**: Todo bucle `@for` debe incluir la directiva `track` con un identificador único (ej: `@for (item of items(); track item.id)`).

## 5. Garantía Cero Rupturas (Zero-Breakage)

- Antes de finalizar cualquier refactorización, verificar que la compilación pase limpiamente (`npm run build`).
- No realizar `git push` ni comits remotos a GitHub; toda gestión de Git se realiza manualmente por el usuario.
