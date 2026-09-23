-- ============================================================================
-- SCRIPT MAESTRO DE INSTALACIÓN - SISTEMA INTELIGENTE DE RECLUTAMIENTO (SIR)
-- NACIONAL SEGUROS
--
-- INSTRUCCIONES DE EJECUCIÓN (Modo SQLCMD en SSMS o a través de sqlcmd utility):
-- 1. Abra SQL Server Management Studio (SSMS).
-- 2. Conéctese a la instancia de SQL Server 2022.
-- 3. Habilite el Modo SQLCMD (Menú Consulta > Modo SQLCMD).
-- 4. Ejecute este script.
-- ============================================================================

:setvar DatabaseName "SIR_NacionalSeguros"

PRINT '==================================================';
PRINT 'INICIANDO INSTALACIÓN DE LA BASE DE DATOS SIR';
PRINT '==================================================';
GO

-- 00. Crear Base de Datos, Filegroups y Query Store
PRINT 'Ejecutando: 00_DATABASE/crear_db.sql...';
:r ./00_DATABASE/crear_db.sql
GO

-- 01. Crear Estructura de Tablas del Sistema
PRINT 'Ejecutando: 01_TABLES/crear_tablas.sql...';
:r ./01_TABLES/crear_tablas.sql
GO

-- 02. Aplicar Restricciones de Integridad y Claves Foráneas
PRINT 'Ejecutando: 02_CONSTRAINTS/crear_constraints.sql...';
:r ./02_CONSTRAINTS/crear_constraints.sql
GO

-- 03. Crear Índices de Cobertura y Optimización
PRINT 'Ejecutando: 03_INDEXES/crear_indices.sql...';
:r ./03_INDEXES/crear_indices.sql
GO

-- 04. Configurar Seguridad RLS, Clasificación y Always Encrypted
PRINT 'Ejecutando: 04_SECURITY/configurar_seguridad.sql...';
:r ./04_SECURITY/configurar_seguridad.sql
GO

-- 05. Crear Funciones de Negocio y Utilidades
PRINT 'Ejecutando: 06_FUNCTIONS/crear_funciones.sql...';
:r ./06_FUNCTIONS/crear_funciones.sql
GO

-- 06. Crear Vistas de Dashboard y Operativas
PRINT 'Ejecutando: 05_VIEWS/crear_vistas.sql...';
:r ./05_VIEWS/crear_vistas.sql
GO

-- 07. Crear Procedimientos Almacenados
PRINT 'Ejecutando: 07_STORED_PROCEDURES/crear_sps.sql...';
:r ./07_STORED_PROCEDURES/crear_sps.sql
GO

-- 08. Crear Triggers de Jerarquía e Integridad
PRINT 'Ejecutando: 08_TRIGGERS/crear_triggers.sql...';
:r ./08_TRIGGERS/crear_triggers.sql
GO

-- 09. Cargar Datos Semilla Obligatorios
PRINT 'Ejecutando: 09_SEED_DATA/semilla.sql...';
:r ./09_SEED_DATA/semilla.sql
GO

-- 10. Crear Procedimientos y Tareas de Mantenimiento
PRINT 'Ejecutando: 10_MAINTENANCE/mantenimiento.sql...';
:r ./10_MAINTENANCE/mantenimiento.sql
GO

PRINT '==================================================';
PRINT 'INSTALACIÓN DE BASE DE DATOS COMPLETADA CON ÉXITO';
PRINT '==================================================';
GO
