-- ========================================================
-- 10_MAINTENANCE/16_add_parametro_orden.sql
-- Agregar columna Orden a la tabla Parametros para soportar
-- ordenamiento personalizado dinámico de campos requeridos.
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR)
-- ========================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE SIR_NacionalSeguros;
GO

PRINT 'Agregando columna Orden a la tabla Parametros...';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Parametros') AND name = 'Orden')
BEGIN
    ALTER TABLE dbo.Parametros ADD Orden INT NOT NULL CONSTRAINT DF_Parametros_Orden DEFAULT 0;
END
GO

PRINT 'Actualizando el orden para los parámetros de completitud del ARES Resumidor...';

DECLARE @CatId INT = (SELECT CatalogoId FROM dbo.Catalogos WHERE Codigo = 'CAT-REQ-FIELDS');

IF @CatId IS NOT NULL
BEGIN
    UPDATE dbo.Parametros SET Orden = 1 WHERE CatalogoId = @CatId AND Codigo = 'cargo';
    UPDATE dbo.Parametros SET Orden = 2 WHERE CatalogoId = @CatId AND Codigo = 'area';
    UPDATE dbo.Parametros SET Orden = 3 WHERE CatalogoId = @CatId AND Codigo = 'modalidad';
    UPDATE dbo.Parametros SET Orden = 4 WHERE CatalogoId = @CatId AND Codigo = 'funciones';
    UPDATE dbo.Parametros SET Orden = 5 WHERE CatalogoId = @CatId AND Codigo = 'skills';
    UPDATE dbo.Parametros SET Orden = 6 WHERE CatalogoId = @CatId AND Codigo = 'jornada';
    UPDATE dbo.Parametros SET Orden = 7 WHERE CatalogoId = @CatId AND Codigo = 'tipoSolicitud';
    UPDATE dbo.Parametros SET Orden = 8 WHERE CatalogoId = @CatId AND Codigo = 'motivo';
    UPDATE dbo.Parametros SET Orden = 9 WHERE CatalogoId = @CatId AND Codigo = 'ubicacion';
    UPDATE dbo.Parametros SET Orden = 10 WHERE CatalogoId = @CatId AND Codigo = 'seniority';
    UPDATE dbo.Parametros SET Orden = 11 WHERE CatalogoId = @CatId AND Codigo = 'prioridad';
    UPDATE dbo.Parametros SET Orden = 12 WHERE CatalogoId = @CatId AND Codigo = 'fechaIdeal';
    UPDATE dbo.Parametros SET Orden = 13 WHERE CatalogoId = @CatId AND Codigo = 'remuneracionOfrecida';
END
GO

PRINT 'Columna Orden agregada y configurada exitosamente.';
GO
