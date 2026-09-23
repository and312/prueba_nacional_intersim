# Resumen del Módulo de Perfiles de Vacante

**Estado de Implementación**: Mock/Frontend Conectado  
**Última Actualización**: 12/07/2026

---

## Propósito del Módulo
El **Módulo de Perfiles de Vacante** tiene como objetivo centralizar la definición, ajuste, versionamiento y aprobación del **profesiograma** o perfil del cargo para una vacante específica. Permite que Recursos Humanos (RRHH) y el Área Solicitante colaboren en la validación técnica y estratégica del perfil sugerido, garantizando que esté perfectamente alineado con las necesidades del negocio antes de proceder con la búsqueda de candidatos.

---

## Origen del Perfil
El ciclo de vida de un perfil de vacante comienza de forma estrictamente dependiente de una **Solicitud de Vacante Aprobada** (`SolicitudAprobada`). No se pueden registrar perfiles de manera libre o aislada. El perfil hereda y bloquea los datos fundamentales que fueron formalmente aprobados por la gerencia y las áreas de control en la solicitud original.

---

## Relación con el Agente de Inteligencia Artificial (IA)
Una vez que el perfil entra en el estado `PendienteGeneracionPerfil`, el motor de orquestación (por ejemplo, n8n) dispara un flujo que activa el **Agente IA de Perfiles**. Este agente toma la información de la solicitud y, apoyado en bases de conocimientos de la empresa (RAG) y perfiles históricos similares, genera un profesiograma estructurado y propone un **Resumen Ejecutivo** preliminar.
*   **Nota**: El agente IA actúa como un asistente acelerador, pero **nunca** puede aprobar de forma directa o autónoma el perfil de vacante. La aprobación final siempre es potestad humana.

---

## Relación con los Roles Organizacionales

### Recursos Humanos (RRHH)
Actúa como el tutor y editor metodológico del profesiograma. RRHH refina la sugerencia de la IA, modifica los campos de perfil que se consideren convenientes, responde/resuelve las observaciones aportadas por el negocio, e inicia la fase de revisión del cliente interno.

### Área Solicitante
Representa al cliente interno (por ejemplo, Jefe Comercial, Gerente de Finanzas) que conoce la operación del cargo a detalle. El Área Solicitante valida que la propuesta sea idónea a sus expectativas reales y propone correcciones globales (Observaciones) o aprueba formalmente el perfil.

---

## Relación Futura con la Estrategia de Búsqueda
El perfil de vacante aprobado de forma definitiva (`PerfilAprobadoFinal`) constituye el insumo directo e inmutable para el **Módulo de Vacantes**. A partir de este profesiograma, los agentes de reclutamiento (Matching IA, Scoring IA) estructuran las búsquedas de candidatos, evalúan hojas de vida (CV) contra los campos requeridos y ordenan la shortlist de postulantes.
