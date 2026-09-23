-- ==========================================
-- 10_MAINTENANCE/mantenimiento.sql
-- Procedimientos de Mantenimiento de Índices, Estadísticas y Compresión
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
GO

-- ==========================================
-- 1. sp_Mantenimiento_OptimizarIndices
-- Reorganiza o reconstruye índices según su nivel de fragmentación.
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_Mantenimiento_OptimizarIndices
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SchemaName NVARCHAR(128);
    DECLARE @TableName NVARCHAR(128);
    DECLARE @IndexName NVARCHAR(128);
    DECLARE @Fragmentation FLOAT;
    DECLARE @SqlCommand NVARCHAR(MAX);

    -- Cursor para obtener índices con fragmentación > 10%
    DECLARE IndexCursor CURSOR FOR
    SELECT 
        s.name AS SchemaName,
        o.name AS TableName,
        i.name AS IndexName,
        ps.avg_fragmentation_in_percent AS Fragmentation
    FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
    INNER JOIN sys.indexes i ON ps.object_id = i.object_id AND ps.index_id = i.index_id
    INNER JOIN sys.objects o ON i.object_id = o.object_id
    INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
    WHERE ps.avg_fragmentation_in_percent > 10.0
      AND i.name IS NOT NULL
      AND o.type = 'U';

    OPEN IndexCursor;
    FETCH NEXT FROM IndexCursor INTO @SchemaName, @TableName, @IndexName, @Fragmentation;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Reconstrucción completa si fragmentación > 30%
        IF @Fragmentation > 30.0
        BEGIN
            SET @SqlCommand = N'ALTER INDEX [' + @IndexName + N'] ON [' + @SchemaName + N'].[' + @TableName + N'] REBUILD WITH (FILLFACTOR = 90);';
            PRINT 'Reconstruyendo índice: ' + @IndexName + ' en ' + @TableName + ' (Fragmentación: ' + CAST(@Fragmentation AS NVARCHAR(10)) + '%)';
        END
        -- Reorganización si fragmentación entre 10% y 30%
        ELSE
        BEGIN
            SET @SqlCommand = N'ALTER INDEX [' + @IndexName + N'] ON [' + @SchemaName + N'].[' + @TableName + N'] REORGANIZE;';
            PRINT 'Reorganizando índice: ' + @IndexName + ' en ' + @TableName + ' (Fragmentación: ' + CAST(@Fragmentation AS NVARCHAR(10)) + '%)';
        END

        BEGIN TRY
            EXEC sp_executesql @SqlCommand;
        END TRY
        BEGIN CATCH
            PRINT 'ERROR ejecutando: ' + @SqlCommand + ' - Mensaje: ' + ERROR_MESSAGE();
        END CATCH;

        FETCH NEXT FROM IndexCursor INTO @SchemaName, @TableName, @IndexName, @Fragmentation;
    END

    CLOSE IndexCursor;
    DEALLOCATE IndexCursor;
END;
GO

-- ==========================================
-- 2. sp_Mantenimiento_ActualizarEstadisticas
-- Actualiza las estadísticas de todas las tablas de usuario con FULLSCAN
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_Mantenimiento_ActualizarEstadisticas
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TableName NVARCHAR(256);
    DECLARE @SqlCommand NVARCHAR(MAX);

    DECLARE TableCursor CURSOR FOR
    SELECT N'[' + s.name + N'].[' + t.name + N']'
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.type = 'U';

    OPEN TableCursor;
    FETCH NEXT FROM TableCursor INTO @TableName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @SqlCommand = N'UPDATE STATISTICS ' + @TableName + N' WITH FULLSCAN;';
        PRINT 'Actualizando estadísticas de: ' + @TableName;
        
        BEGIN TRY
            EXEC sp_executesql @SqlCommand;
        END TRY
        BEGIN CATCH
            PRINT 'ERROR actualizando estadísticas en: ' + @TableName + ' - ' + ERROR_MESSAGE();
        END CATCH;

        FETCH NEXT FROM TableCursor INTO @TableName;
    END

    CLOSE TableCursor;
    DEALLOCATE TableCursor;
END;
GO

-- ==========================================
-- 3. sp_Mantenimiento_ComprimirDatosHistoricos
-- Aplica compresión PAGE en tablas Ledger e históricas para optimizar I/O
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_Mantenimiento_ComprimirDatosHistoricos
AS
BEGIN
    SET NOCOUNT ON;
    
    -- NOTA: Las tablas Ledger no se pueden truncar ni eliminar filas, 
    -- por lo que la compresión PAGE es la mejor forma de mitigar el crecimiento en disco.
    
    BEGIN TRY
        PRINT 'Comprimiendo tabla AuditLogs...';
        ALTER TABLE AuditLogs REBUILD WITH (DATA_COMPRESSION = PAGE);

        PRINT 'Comprimiendo tabla StateHistory...';
        ALTER TABLE StateHistory REBUILD WITH (DATA_COMPRESSION = PAGE);

        PRINT 'Comprimiendo tabla AgentExecutions...';
        ALTER TABLE AgentExecutions REBUILD WITH (DATA_COMPRESSION = PAGE);

        PRINT 'Comprimiendo tabla IntegrationLogs...';
        ALTER TABLE IntegrationLogs REBUILD WITH (DATA_COMPRESSION = PAGE);
        
        PRINT 'Compresión finalizada con éxito.';
    END TRY
    BEGIN CATCH
        PRINT 'ERROR aplicando compresión PAGE: ' + ERROR_MESSAGE();
    END CATCH;
END;
GO

-- ==========================================
-- 4. PLANTILLA DE SQL AGENT JOBS (RECOMENDADOS)
-- ==========================================
/*
-- Se sugiere programar en SQL Server Agent:
-- 1. Job Diario (Fuera de horario de oficina):
--    EXEC SIR_NacionalSeguros.dbo.sp_Mantenimiento_ActualizarEstadisticas;
--
-- 2. Job Semanal (Fines de semana):
--    EXEC SIR_NacionalSeguros.dbo.sp_Mantenimiento_OptimizarIndices;
--    EXEC SIR_NacionalSeguros.dbo.sp_Mantenimiento_ComprimirDatosHistoricos;
*/
GO
