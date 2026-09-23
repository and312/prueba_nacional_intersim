-- ==========================================
-- 05_VIEWS/crear_vistas.sql
-- Creación de Vistas Lógicas y de Reporte (Corregido)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
GO

-- ==========================================
-- 1. vw_DashboardEjecutivo
-- ==========================================
CREATE OR ALTER VIEW dbo.vw_DashboardEjecutivo
AS
SELECT 
    (SELECT COUNT(1) FROM Solicitudes WHERE IsDeleted = 0) AS TotalSolicitudes,
    (SELECT COUNT(1) FROM Vacantes v 
     INNER JOIN Estados e ON v.EstadoId = e.EstadoId 
     WHERE v.IsDeleted = 0 AND e.Codigo IN ('VAC-PUB', 'VAC-CAP')) AS TotalVacantesActivas,
    (SELECT COUNT(1) FROM Postulantes WHERE IsDeleted = 0) AS TotalPostulantes,
    (SELECT ISNULL(AVG(DATEDIFF(day, FechaApertura, FechaCierre)), 0) 
     FROM Vacantes 
     WHERE IsDeleted = 0 AND FechaCierre IS NOT NULL) AS TiempoPromedioCoberturaDias,
    (SELECT ISNULL(SUM(CostoEstimado), 0.0) FROM AgentExecutions) AS CostoTokensAcumuladoUsd;
GO

-- ==========================================
-- 2. vw_DashboardSLA
-- ==========================================
CREATE OR ALTER VIEW dbo.vw_DashboardSLA
AS
SELECT 
    se.SLAExecutionId,
    s.Nombre AS NombreSLA,
    s.Modulo,
    se.Entidad,
    se.EntidadId,
    e.Nombre AS EstadoNombre,
    se.FechaInicio,
    se.FechaLimite,
    se.FechaFin,
    DATEDIFF(hour, se.FechaInicio, ISNULL(se.FechaFin, GETUTCDATE())) AS TranscurridoHoras,
    CASE 
        WHEN se.Cumplido = 1 THEN 'Verde (Cumplido)'
        WHEN se.FechaFin IS NULL AND GETUTCDATE() > se.FechaLimite THEN 'Rojo (Vencido)'
        WHEN se.FechaFin IS NULL AND DATEDIFF(hour, GETUTCDATE(), se.FechaLimite) <= 24 THEN 'Naranja (Crítico)'
        WHEN se.FechaFin IS NULL AND DATEDIFF(hour, GETUTCDATE(), se.FechaLimite) <= 72 THEN 'Amarillo (Prevención)'
        ELSE 'Verde (A Tiempo)'
    END AS EstadoAlerta,
    se.Cumplido,
    se.CorrelationId
FROM SLAExecutions se
INNER JOIN SLAs s ON se.SLAId = s.SLAId
INNER JOIN Estados e ON se.EstadoId = e.EstadoId
WHERE se.IsDeleted = 0;
GO

-- ==========================================
-- 3. vw_DashboardReclutamiento
-- ==========================================
CREATE OR ALTER VIEW dbo.vw_DashboardReclutamiento
AS
SELECT 
    v.VacanteId,
    pc.Cargo AS Cargo,
    sol.Area AS Area,
    DATEDIFF(day, v.FechaApertura, ISNULL(v.FechaCierre, GETUTCDATE())) AS DiasAbierta,
    (SELECT COUNT(1) FROM Postulaciones pos WHERE pos.VacanteId = v.VacanteId AND pos.IsDeleted = 0) AS CantidadCandidatos,
    (SELECT COUNT(1) 
     FROM Postulaciones pos
     INNER JOIN Estados e ON pos.EstadoPipelineId = e.EstadoId
     WHERE pos.VacanteId = v.VacanteId AND pos.IsDeleted = 0 AND e.Codigo IN ('POS-SHR', 'POS-OFE')) AS CandidatosEnTerna,
    e_vac.Nombre AS EstadoVacante
FROM Vacantes v
INNER JOIN PerfilesCargo pc ON v.PerfilCargoId = pc.PerfilCargoId
INNER JOIN Solicitudes sol ON v.SolicitudId = sol.SolicitudId
INNER JOIN Estados e_vac ON v.EstadoId = e_vac.EstadoId
WHERE v.IsDeleted = 0;
GO

-- ==========================================
-- 4. vw_DashboardIA
-- ==========================================
CREATE OR ALTER VIEW dbo.vw_DashboardIA
AS
SELECT 
    a.Nombre AS AgenteNombre,
    pv.VersionNumber AS VersionPrompt,
    COUNT(ae.ExecutionId) AS TotalEjecuciones,
    AVG(ae.TokensInput) AS TokensInputPromedio,
    AVG(ae.TokensOutput) AS TokensOutputPromedio,
    SUM(ae.CostoEstimado) AS CostoTotalUsd,
    AVG(ae.DuracionMs) AS LatenciaPromedioMs
FROM AgentExecutions ae
INNER JOIN Agentes a ON ae.AgenteId = a.AgenteId
INNER JOIN PromptVersions pv ON ae.PromptVersionId = pv.PromptVersionId
GROUP BY a.Nombre, pv.VersionNumber;
GO

-- ==========================================
-- 5. vw_PostulantePipeline
-- ==========================================
CREATE OR ALTER VIEW dbo.vw_PostulantePipeline
AS
SELECT 
    p.PostulanteId,
    pos.PostulacionId,
    CONCAT(p.Nombres, ' ', p.Apellidos) AS NombreCompleto,
    v.VacanteId,
    pc.Cargo AS Cargo,
    pos.EstadoPipelineId,
    e.Codigo AS EstadoPipelineCodigo,
    e.Nombre AS EstadoPipelineNombre,
    s.ScoreFinal AS ScoreFinalCifrado,
    CASE 
        WHEN pos.EstadoPipelineId IN (SELECT EstadoId FROM Estados WHERE Codigo IN ('POS-DES', 'POS-RET')) THEN 'Gris'
        ELSE 'Verde'
    END AS ColorSLA
FROM Postulaciones pos
INNER JOIN Postulantes p ON pos.PostulanteId = p.PostulanteId
INNER JOIN Estados e ON pos.EstadoPipelineId = e.EstadoId
LEFT JOIN Scorings s ON pos.PostulanteId = s.PostulanteId AND pos.VacanteId = s.VacanteId
LEFT JOIN Vacantes v ON pos.VacanteId = v.VacanteId
LEFT JOIN PerfilesCargo pc ON v.PerfilCargoId = pc.PerfilCargoId
WHERE pos.IsDeleted = 0 AND p.IsDeleted = 0;
GO

-- ==========================================
-- 6. vw_Auditoria
-- ==========================================
CREATE OR ALTER VIEW dbo.vw_Auditoria
AS
SELECT 
    AuditId,
    FechaHoraUTC,
    UsuarioNombre,
    Rol,
    Modulo,
    Entidad,
    EntidadId,
    Accion,
    Canal,
    CorrelationId
FROM AuditLogs;
GO
