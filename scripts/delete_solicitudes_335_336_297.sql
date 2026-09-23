SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
BEGIN TRANSACTION;

BEGIN TRY
    -- Definir los SolicitudId a eliminar
    DECLARE @TargetSolicitudes TABLE (SolicitudId INT PRIMARY KEY);
    INSERT INTO @TargetSolicitudes (SolicitudId) VALUES (335), (336), (297);

    -- Tablas temporales para almacenar IDs intermedios
    DECLARE @TargetPerfilesCargo TABLE (PerfilCargoId INT PRIMARY KEY);
    INSERT INTO @TargetPerfilesCargo (PerfilCargoId)
    SELECT PerfilCargoId FROM PerfilesCargo WHERE SolicitudId IN (SELECT SolicitudId FROM @TargetSolicitudes);

    DECLARE @TargetPerfilSecciones TABLE (PerfilSeccionId INT PRIMARY KEY);
    INSERT INTO @TargetPerfilSecciones (PerfilSeccionId)
    SELECT PerfilSeccionId FROM PerfilSecciones WHERE PerfilCargoId IN (SELECT PerfilCargoId FROM @TargetPerfilesCargo);

    DECLARE @TargetPerfilEstructurado TABLE (PerfilEstructuradoId INT PRIMARY KEY);
    INSERT INTO @TargetPerfilEstructurado (PerfilEstructuradoId)
    SELECT PerfilEstructuradoId FROM PerfilEstructurado WHERE SolicitudId IN (SELECT SolicitudId FROM @TargetSolicitudes);

    DECLARE @TargetMatchingEjecuciones TABLE (MatchingEjecucionId INT PRIMARY KEY);
    INSERT INTO @TargetMatchingEjecuciones (MatchingEjecucionId)
    SELECT MatchingEjecucionId FROM MatchingEjecuciones 
    WHERE PerfilCargoId IN (SELECT PerfilCargoId FROM @TargetPerfilesCargo)
       OR PerfilEstructuradoId IN (SELECT PerfilEstructuradoId FROM @TargetPerfilEstructurado);

    DECLARE @TargetVacantes TABLE (VacanteId INT PRIMARY KEY);
    INSERT INTO @TargetVacantes (VacanteId)
    SELECT VacanteId FROM Vacantes WHERE SolicitudId IN (SELECT SolicitudId FROM @TargetSolicitudes);

    -- 1. Hijos de MatchingEjecuciones
    DELETE FROM EstrategiaExternas WHERE MatchingEjecucionId IN (SELECT MatchingEjecucionId FROM @TargetMatchingEjecuciones);
    DELETE FROM EstrategiaInternas WHERE MatchingEjecucionId IN (SELECT MatchingEjecucionId FROM @TargetMatchingEjecuciones);
    DELETE FROM MatchingResultados WHERE MatchingEjecucionId IN (SELECT MatchingEjecucionId FROM @TargetMatchingEjecuciones);
    DELETE FROM MatchingEjecuciones WHERE MatchingEjecucionId IN (SELECT MatchingEjecucionId FROM @TargetMatchingEjecuciones);

    -- 2. Hijos de Vacantes
    DELETE FROM Contrataciones WHERE VacanteId IN (SELECT VacanteId FROM @TargetVacantes);
    DELETE FROM Entrevistas WHERE VacanteId IN (SELECT VacanteId FROM @TargetVacantes);
    DELETE FROM Matchings WHERE VacanteId IN (SELECT VacanteId FROM @TargetVacantes);
    DELETE FROM Ofertas WHERE VacanteId IN (SELECT VacanteId FROM @TargetVacantes);
    DELETE FROM Postulaciones WHERE VacanteId IN (SELECT VacanteId FROM @TargetVacantes);
    DELETE FROM Scorings WHERE VacanteId IN (SELECT VacanteId FROM @TargetVacantes);
    DELETE FROM Vacantes WHERE VacanteId IN (SELECT VacanteId FROM @TargetVacantes);

    -- 3. Hijos de PerfilSecciones y PerfilesCargo
    DELETE FROM PerfilAuditoria 
    WHERE PerfilCargoId IN (SELECT PerfilCargoId FROM @TargetPerfilesCargo)
       OR PerfilSeccionId IN (SELECT PerfilSeccionId FROM @TargetPerfilSecciones);

    DELETE FROM PerfilObservaciones WHERE PerfilCargoId IN (SELECT PerfilCargoId FROM @TargetPerfilesCargo);
    DELETE FROM PerfilSecciones WHERE PerfilCargoId IN (SELECT PerfilCargoId FROM @TargetPerfilesCargo);
    DELETE FROM ResumenEjecutivos WHERE PerfilCargoId IN (SELECT PerfilCargoId FROM @TargetPerfilesCargo);
    DELETE FROM PostulantesInternos WHERE PerfilCargoId IN (SELECT PerfilCargoId FROM @TargetPerfilesCargo);
    DELETE FROM PostulantesExternos WHERE PerfilCargoId IN (SELECT PerfilCargoId FROM @TargetPerfilesCargo);
    DELETE FROM PerfilesCargo WHERE SolicitudId IN (SELECT SolicitudId FROM @TargetSolicitudes);

    -- 4. Otros hijos directos de Solicitudes
    DELETE FROM PerfilEstructurado WHERE SolicitudId IN (SELECT SolicitudId FROM @TargetSolicitudes);
    DELETE FROM SolicitudComentarios WHERE SolicitudId IN (SELECT SolicitudId FROM @TargetSolicitudes);
    DELETE FROM SolicitudDocumentos WHERE SolicitudId IN (SELECT SolicitudId FROM @TargetSolicitudes);
    DELETE FROM SolicitudResumenes WHERE SolicitudId IN (SELECT SolicitudId FROM @TargetSolicitudes);

    -- 5. Tabla Principal: Solicitudes
    DELETE FROM Solicitudes WHERE SolicitudId IN (SELECT SolicitudId FROM @TargetSolicitudes);

    PRINT 'Eliminacion completada exitosamente.';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;
