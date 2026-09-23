# Tasks: Reordenamiento de Columna PerfilSessionDataJson

- [x] **1. Reversión Migración** — Revertir y remover la migración AddPerfilSessionDataJsonToWSSessions
- [x] **2. Configuración** — Modificar `WSSessionConfiguration.cs` para agregar HasColumnOrder
- [x] **3. Regeneración** — Generar la migración y depurar los cambios ajenos
- [x] **4. Aplicación BD** — Ejecutar script SQL de reordenamiento en servidor y aplicar migración
- [x] **5. Verificación** — Compilar y ejecutar pruebas unitarias (`dotnet test`)
