-- ==========================================
-- 08_TRIGGERS/crear_triggers.sql
-- Creación de Triggers de Integridad y Auditoría (Corregido)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
GO

-- ==========================================
-- 1. trg_Parametro_PreventCircular
-- Trigger recursivo para prevenir dependencias circulares en la tabla jerárquica Parametros
-- ==========================================
CREATE OR ALTER TRIGGER dbo.trg_Parametro_PreventCircular
ON dbo.Parametros
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @HasCycle BIT = 0;

    -- CTE recursiva en el nivel superior para reconstruir ancestros y detectar ciclos
    WITH Ancestors AS (
        SELECT 
            i.ParametroId AS StartParametroId,
            i.ParametroIdPadre AS CurrentPadreId,
            1 AS Depth
        FROM inserted i
        WHERE i.ParametroIdPadre IS NOT NULL

        UNION ALL

        SELECT 
            a.StartParametroId,
            p.ParametroIdPadre,
            a.Depth + 1
        FROM dbo.Parametros p
        INNER JOIN Ancestors a ON p.ParametroId = a.CurrentPadreId
        WHERE p.ParametroIdPadre IS NOT NULL AND a.Depth < 100
    )
    SELECT TOP 1 @HasCycle = 1 
    FROM Ancestors 
    WHERE StartParametroId = CurrentPadreId;

    IF @HasCycle = 1
    BEGIN
        RAISERROR ('ERROR CRÍTICO DDL: Se detectó una dependencia circular en la jerarquía de Parámetros. Operación abortada.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- ==========================================
-- NOTA: Los triggers trg_Solicitudes_AuditStateHistory, trg_Vacantes_AuditStateHistory y trg_Postulaciones_AuditStateHistory
-- han sido eliminados de la base de datos y su lógica fue migrada al backend en C# (PublishDomainEventsInterceptor.cs)
-- para evitar problemas de compatibilidad con @@ROWCOUNT en Entity Framework Core 9.
-- ==========================================
