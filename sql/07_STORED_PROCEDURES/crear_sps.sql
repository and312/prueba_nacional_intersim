-- ==========================================
-- 07_STORED_PROCEDURES/crear_sps.sql
-- Creación de Procedimientos Almacenados (Corregido)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
GO

-- ==========================================
-- 1. sp_Seguridad_RegistrarSesion
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_Seguridad_RegistrarSesion
    @UsuarioId INT,
    @RefreshToken NVARCHAR(256),
    @FechaExpiracion DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Sesiones (UsuarioId, RefreshToken, FechaExpiracion, Activa, CreatedDate)
    VALUES (@UsuarioId, @RefreshToken, @FechaExpiracion, 1, GETUTCDATE());
END;
GO

-- ==========================================
-- 2. sp_Seguridad_InvalidarSesionesUsuario
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_Seguridad_InvalidarSesionesUsuario
    @UsuarioId INT,
    @CorrelationId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Sesiones
    SET Activa = 0
    WHERE UsuarioId = @UsuarioId AND Activa = 1;
    
    INSERT INTO IntegrationLogs (Fecha, SistemaExterno, EndpointUrl, MetodoHttp, Resultado, ErrorMessage, CorrelationId)
    VALUES (GETUTCDATE(), N'InternalSecurity', N'/api/v1/usuarios/' + CAST(@UsuarioId AS NVARCHAR(10)) + N'/logout-all', N'POST', N'Exitoso', N'Sesiones invalidadas de forma manual o preventiva', ISNULL(@CorrelationId, NEWID()));
END;
GO

-- ==========================================
-- 3. sp_Reclutamiento_TransitarEstadoPostulacion (Corregido)
-- Transiciona el estado de la postulación de forma atómica y auditable (Ledger)
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_Reclutamiento_TransitarEstadoPostulacion
    @PostulacionId INT,
    @NuevoEstadoId INT,
    @UsuarioId INT,
    @Comentario NVARCHAR(500),
    @CorrelationId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @EstadoAnteriorId INT;
        DECLARE @PostulanteId INT;
        DECLARE @VacanteId INT;
        
        SELECT 
            @EstadoAnteriorId = EstadoPipelineId,
            @PostulanteId = PostulanteId,
            @VacanteId = VacanteId
        FROM Postulaciones 
        WHERE PostulacionId = @PostulacionId;

        IF @EstadoAnteriorId IS NULL
        BEGIN
            THROW 50001, 'La postulación especificada no existe.', 1;
        END

        -- Actualizar estado de la postulación
        UPDATE Postulaciones
        SET EstadoPipelineId = @NuevoEstadoId,
            ModifiedBy = (SELECT Nombre FROM Usuarios WHERE UsuarioId = @UsuarioId),
            ModifiedDate = GETUTCDATE()
        WHERE PostulacionId = @PostulacionId;

        -- Registrar transacción de estado en la tabla inmutable Ledger
        INSERT INTO StateHistory (Entidad, EntidadId, EstadoAnteriorId, EstadoNuevoId, UsuarioId, Fecha, Comentario, CorrelationId)
        VALUES (N'Postulacion', @PostulacionId, @EstadoAnteriorId, @NuevoEstadoId, @UsuarioId, GETUTCDATE(), @Comentario, @CorrelationId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- ==========================================
-- 4. sp_Auditoria_ConsultarEjecucionesAgente
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_Auditoria_ConsultarEjecucionesAgente
    @AgenteId INT = NULL,
    @CorrelationId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ExecutionId,
        AgenteId,
        PromptVersionId,
        UsuarioId,
        FechaInicio,
        FechaFin,
        DuracionMs,
        InputJson,
        OutputJson,
        ResultadoStatus,
        TokensInput,
        TokensOutput,
        CostoEstimado,
        CorrelationId
    FROM AgentExecutions
    WHERE 
        (@AgenteId IS NULL OR AgenteId = @AgenteId)
        AND (@CorrelationId IS NULL OR CorrelationId = @CorrelationId)
    ORDER BY FechaInicio DESC;
END;
GO

-- ==========================================
-- 5. sp_SLA_CalcularFechaLimite
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_SLA_CalcularFechaLimite
    @SLAId INT,
    @FechaInicio DATETIME2(7),
    @FechaLimiteCalculada DATETIME2(7) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DiasMaximos INT;
    SELECT @DiasMaximos = DiasMaximos FROM SLAs WHERE SLAId = @SLAId;

    SET @FechaLimiteCalculada = dbo.fn_CalcularFechaLimiteSLA(@FechaInicio, ISNULL(@DiasMaximos, 0));
END;
GO

-- ==========================================
-- 6. sp_Reporte_CargarSnapshotMetricas
-- ==========================================
CREATE OR ALTER PROCEDURE dbo.sp_Reporte_CargarSnapshotMetricas
    @FechaReferencia DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @FechaReferencia IS NULL
        SET @FechaReferencia = CAST(GETUTCDATE() AS DATE);

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Costo Tokens Acumulado
        INSERT INTO MetricSnapshot (MetricaNombre, FechaHora, Valor, AgrupacionClave, AgrupacionValor)
        VALUES (N'SLA_CostoTokensAcumuladoUsd', GETUTCDATE(), 
                (SELECT ISNULL(SUM(CostoEstimado), 0.0) FROM AgentExecutions), NULL, NULL);

        -- 2. Cumplimiento SLA por Módulo
        INSERT INTO MetricSnapshot (MetricaNombre, FechaHora, Valor, AgrupacionClave, AgrupacionValor)
        SELECT 
            N'SLA_CumplimientoPorcentaje', 
            GETUTCDATE(),
            CAST(SUM(CASE WHEN Cumplido = 1 THEN 100.0 ELSE 0.0 END) / COUNT(1) AS DECIMAL(18,4)),
            N'Modulo',
            s.Modulo
        FROM SLAExecutions se
        INNER JOIN SLAs s ON se.SLAId = s.SLAId
        WHERE se.Cumplido IS NOT NULL AND se.IsDeleted = 0
        GROUP BY s.Modulo;

        -- 3. Total Postulaciones por Origen (Corregido)
        INSERT INTO MetricSnapshot (MetricaNombre, FechaHora, Valor, AgrupacionClave, AgrupacionValor)
        SELECT 
            N'TotalPostulacionesPorOrigen',
            GETUTCDATE(),
            CAST(COUNT(1) AS DECIMAL(18,4)),
            N'Origen',
            p.Origen
        FROM Postulaciones pos
        INNER JOIN Postulantes p ON pos.PostulanteId = p.PostulanteId
        WHERE pos.IsDeleted = 0 AND p.IsDeleted = 0
        GROUP BY p.Origen;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
