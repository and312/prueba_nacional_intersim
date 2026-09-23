# Lineamientos de Arquitectura Frontend Modular

**Estado de Implementación**: Vigente y Obligatorio  
**Última Actualización**: 12/07/2026

Para asegurar la mantenibilidad a largo plazo del frontend Angular del proyecto Nacional Seguros y evitar regresiones o colapsos operativos, todo desarrollo debe regirse por las siguientes directrices arquitectónicas:

---

## 1. Integración Progresiva (No Refactorización Masiva)
*   **Enfoque**: Integrarse progresivamente con la arquitectura actual respetando rutas, layouts, autenticación, sidebar, header, guards e interceptores existentes.
*   **Regla**: Queda estrictamente prohibida la refactorización integral o masiva de módulos adyacentes para "limpiar" código viejo si esto paraliza el flujo de entrega de valor al negocio. El código nuevo convive de forma respetuosa con los patrones establecidos.

---

## 2. Modularidad y Componentización
*   **Responsabilidad Única**: Cada vista compleja (como el Detalle del Perfil) debe fragmentarse en componentes independientes y auto-contenidos de grano fino (ej. `encabezado-perfil`, `pestanas-perfil`, `observaciones-recibidas-perfil`).
*   **Servicios Dedicados**: Evitar el crecimiento desmedido del servicio principal `PerfilesService` o `ApiService`. La lógica específica de componentes complejos debe delegarse a servicios satélites (ej. `ObservacionesPerfilService`).
*   **Standalone Components**: Todos los nuevos componentes y servicios deben declararse como `standalone: true`, importando únicamente los módulos mínimos indispensables en su decorador.

---

## 3. Tipado Estricto y TypeScript
*   **Prohibición de `any`**: Está prohibido el uso del tipo `any` en modelos, firmas de métodos, propiedades y respuestas de observables. Toda estructura de datos debe contar con su correspondiente `interface` o `type`.
*   **isolatedModules**: Para cumplir con la configuración estricta de transpila del proyecto, los re-exports de interfaces o tipos en barriles (index) deben realizarse explícitamente mediante `export type` en lugar de `export`.

---

## 4. Diseño y UI/UX Standards
*   **Sin Librerías Invasivas**: Mantener la estética premium corporativa heredada. No se debe introducir Bootstrap, TailwindCSS o PrimeNG si no están instalados por defecto. El diseño se maquetea con Vanilla CSS encapsulado y Angular Material/CDK existente.
*   **No Duplicación de Vistas por Rol**: No se deben duplicar componentes HTML enteros para renderizar la pantalla del Área Solicitante y de RRHH. Se utiliza el mismo componente estructural, controlando la visibilidad de bloques o la editabilidad de controles mediante directivas condicionales y condicionales lógicos del rol.
*   **Modo Lectura vs Edición**: Evitar renderizar todos los datos en cajas de texto deshabilitadas. El modo lectura muestra fichas limpias con estilos semánticos y tipografía corporativa. El modo edición transforma sólo los campos autorizados en controles interactivos de formulario reactivo.

---

## 5. Manejo de Estados Visuales de Carga
Toda interacción asíncrona contra el servidor o agente de IA debe gestionar limpiamente los tres estados críticos de interfaz:
1.  **Carga (Loading)**: Spinner o Shimmer animado para evitar clicks duplicados del usuario.
2.  **Error**: Pantallas mitigadas de fallo con mensaje amigable y botón de reintento (`retry`).
3.  **Vacío (Empty State)**: Mensajes ilustrados con iconos de material symbols cuando un listado no contiene registros.
