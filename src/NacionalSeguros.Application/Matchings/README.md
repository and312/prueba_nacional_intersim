# Módulo 07: Motor de Matching Inteligente (IA)
## Sistema Inteligente de Reclutamiento (SIR) – Nacional Seguros

Este módulo implementa el motor de comparación de perfiles (Matching) y de evaluación de idoneidad técnica/experiencia (Scoring) utilizando inteligencia artificial de manera completamente desacoplada y configurable.

---

## 1. Diseño y Abstracción de IA

La arquitectura del motor de matching se rige bajo los siguientes principios:
*   **Independencia de Proveedor (`IIAProvider`):** Se definió una interfaz en la capa de dominio [IIAProvider.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Domain/Services/IAProvider.cs). Esto permite intercambiar el motor de IA subyacente (OpenAI, Azure OpenAI, Google Vertex AI, Anthropic, Ollama, etc.) únicamente modificando el registro en el contenedor de dependencias, sin alterar la lógica de negocio ni el dominio.
*   **Simulación (`MockIAProvider`):** Para entornos locales y pruebas unitarias/integración, se incluye [MockIAProvider.cs](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/src/NacionalSeguros.Persistence/Services/MockIAProvider.cs) que emula las inferencias devolviendo resultados JSON estables y consumos de tokens controlados sin requerir conexión a internet ni API Keys.
*   **Trazabilidad de Inferencias (`AgentExecutions`):** Para cumplir con las políticas financieras y de observabilidad, cada matching o scoring ejecutado registra de forma obligatoria una fila en la tabla ledger `AgentExecutions`, almacenando duración, tokens consumidos, costo financiero en USD y prompts correspondientes.

---

## 2. Ejecución de Pruebas

Para validar el funcionamiento del motor de matching y telemetría de IA:

```powershell
# Compilar todos los proyectos
dotnet build

# Ejecutar la suite de pruebas
dotnet test
```
