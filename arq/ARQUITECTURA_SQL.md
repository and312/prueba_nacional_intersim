# Base de Conocimiento de Arquitectura de Base de Datos: SQL Server 2022

Este documento representa la base de conocimiento para la arquitectura de base de datos, convenciones y especificaciones del **Sistema Inteligente de Reclutamiento (SIR)** de **Nacional Seguros**. 

La especificación técnica completa, detallada y certificada del modelo físico, columnas, constraints, índices, vistas, stored procedures y estrategias de rendimiento/seguridad se encuentra en el documento maestro del proyecto:
👉 **[DISEÑO_FISICO_SQL.md](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/DISEÑO_FISICO_SQL.md)**

---

## Resumen Ejecutivo de la Arquitectura de Base de Datos
La base de datos está diseñada bajo estándares empresariales rigurosos utilizando:
* **Microsoft SQL Server 2022 Standard** con **Nivel de Compatibilidad (Compatibility Level) 160**.
* **ORM Entity Framework Core 9** y **.NET 8** para persistencia transaccional y **Dapper** para consultas masivas de analítica.
* **SQL Server Ledger** nativo para tablas inmutables de auditoría (`AuditLogs`), transiciones de estados (`StateHistory`) y ejecuciones de IA (`AgentExecutions`), evitando alteración física de registros.
* **Always Encrypted con Enclaves VBS** para el cifrado transparente de bandas salariales, pretensiones y scores de IA.
* **Cálculo de SLAs Hábiles:** Tabla física `Feriados` y algoritmo de exclusión de feriados (recurrentes/no recurrentes) y fines de semana.
* **Integridad Jerárquica:** Trigger recursivo `trg_Parametro_PreventCircular` como última línea de defensa contra bucles jerárquicos.
* **Particionamiento Mensual:** Por rango de fechas para las tablas de logs y auditoría masivas.

Para detalles de los esquemas de tablas, constraints, índices, vistas, stored procedures y plan de respaldos, por favor consulte el documento maestro:
👉 **[DISEÑO_FISICO_SQL.md](file:///c:/Users/DELL%20XPS/Desktop/INTERSIM/nacional/DISEÑO_FISICO_SQL.md)**
