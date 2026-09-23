USE SIR_NacionalSeguros;
GO

-- 1. Create Gerencias table if it does not exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Gerencias]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Gerencias] (
        [GerenciaId] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Activo] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedBy] NVARCHAR(100) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 2. Populate Gerencias from existing distinct Gerencias in Areas, plus defaults
INSERT INTO [dbo].[Gerencias] (Nombre, CreatedBy, CreatedDate)
SELECT DISTINCT Gerencia, 'Migration', GETDATE()
FROM Areas
WHERE Gerencia IS NOT NULL AND Gerencia <> '' AND Gerencia NOT IN (SELECT Nombre FROM Gerencias);

-- Ensure default ones exist
IF NOT EXISTS (SELECT 1 FROM Gerencias WHERE Nombre = N'Gerencia de Tecnología')
    INSERT INTO Gerencias (Nombre, CreatedBy) VALUES (N'Gerencia de Tecnología', 'Migration');
IF NOT EXISTS (SELECT 1 FROM Gerencias WHERE Nombre = N'Gerencia de Capital Humano')
    INSERT INTO Gerencias (Nombre, CreatedBy) VALUES (N'Gerencia de Capital Humano', 'Migration');
IF NOT EXISTS (SELECT 1 FROM Gerencias WHERE Nombre = N'Gerencia de Finanzas')
    INSERT INTO Gerencias (Nombre, CreatedBy) VALUES (N'Gerencia de Finanzas', 'Migration');
GO

-- 3. Add GerenciaId to Areas (allow null temporarily to migrate)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Areas]') AND name = N'GerenciaId')
BEGIN
    ALTER TABLE [dbo].[Areas] ADD [GerenciaId] INT NULL;
END
GO

-- 4. Update Areas set GerenciaId pointing to the matching Gerencia
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Areas]') AND name = N'Gerencia')
BEGIN
    EXEC sp_executesql N'
    UPDATE a
    SET a.GerenciaId = g.GerenciaId
    FROM Areas a
    JOIN Gerencias g ON a.Gerencia = g.Nombre;
    ';
END

-- For any that didn't match, assign the first one
DECLARE @DefaultGerenciaId INT = (SELECT TOP 1 GerenciaId FROM Gerencias);
UPDATE Areas
SET GerenciaId = @DefaultGerenciaId
WHERE GerenciaId IS NULL;
GO

-- 5. Make GerenciaId NOT NULL and add Foreign Key constraint
ALTER TABLE [dbo].[Areas] ALTER COLUMN [GerenciaId] INT NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Areas_Gerencias]'))
BEGIN
    ALTER TABLE [dbo].[Areas] ADD CONSTRAINT [FK_Areas_Gerencias] 
    FOREIGN KEY ([GerenciaId]) REFERENCES [dbo].[Gerencias] ([GerenciaId]);
END
GO

-- 6. Drop the old string column Gerencia from Areas
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Areas]') AND name = N'Gerencia')
BEGIN
    ALTER TABLE [dbo].[Areas] DROP COLUMN [Gerencia];
END
GO

PRINT 'Migración de base de datos para Gerencias completada con éxito.';
