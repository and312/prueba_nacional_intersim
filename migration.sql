SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- 0. Ajustar restricción CK_Estados_Entidad para permitir la entidad 'Perfil'
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Estados_Entidad' AND parent_object_id = OBJECT_ID('dbo.Estados'))
    BEGIN
        ALTER TABLE dbo.Estados DROP CONSTRAINT CK_Estados_Entidad;
        ALTER TABLE dbo.Estados ADD CONSTRAINT CK_Estados_Entidad CHECK ([Entidad]='Perfil' OR [Entidad]='Oferta' OR [Entidad]='Entrevista' OR [Entidad]='Postulante' OR [Entidad]='Vacante' OR [Entidad]='Solicitud');
    END

    -- 1. Crear catálogo dbo.TiposObservacion si no existe
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TiposObservacion' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        CREATE TABLE dbo.TiposObservacion (
            TipoObservacionId INT IDENTITY(1,1) NOT NULL,
            Codigo VARCHAR(50) NOT NULL,
            Nombre VARCHAR(150) NOT NULL,
            Descripcion NVARCHAR(1000) NULL,
            Estado VARCHAR(20) NOT NULL CONSTRAINT DF_TiposObservacion_Estado DEFAULT 'Activo',
            CreatedBy VARCHAR(100) NOT NULL,
            CreatedDate DATETIME NOT NULL CONSTRAINT DF_TiposObservacion_CreatedDate DEFAULT GETUTCDATE(),
            ModifiedBy VARCHAR(100) NULL,
            ModifiedDate DATETIME NULL,
            DeletedBy VARCHAR(100) NULL,
            DeletedDate DATETIME NULL,
            IsDeleted BIT NOT NULL CONSTRAINT DF_TiposObservacion_IsDeleted DEFAULT 0,
            CONSTRAINT PK_TiposObservacion PRIMARY KEY (TipoObservacionId),
            CONSTRAINT UQ_TiposObservacion_Codigo UNIQUE (Codigo)
        );
    END

    -- 2. Crear tabla dbo.PerfilObservaciones si no existe
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PerfilObservaciones' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        CREATE TABLE dbo.PerfilObservaciones (
            PerfilObservacionId INT IDENTITY(1,1) NOT NULL,
            PerfilCargoId INT NOT NULL,
            TipoObservacionId INT NOT NULL,
            Comentario NVARCHAR(MAX) NOT NULL,
            UsuarioSolicitanteId INT NOT NULL,
            NumeroIteracion INT NOT NULL,
            EstadoObservacion VARCHAR(20) NOT NULL,
            CreatedDate DATETIME NOT NULL CONSTRAINT DF_PerfilObservaciones_CreatedDate DEFAULT GETUTCDATE(),
            AtendidaPorUsuarioId INT NULL,
            FechaAtencion DATETIME NULL,
            CONSTRAINT PK_PerfilObservaciones PRIMARY KEY (PerfilObservacionId)
        );
    END

    -- 3. Crear tabla dbo.ResumenEjecutivos si no existe (Relación 1-1 con PerfilesCargo)
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ResumenEjecutivos' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        CREATE TABLE dbo.ResumenEjecutivos (
            ResumenId INT IDENTITY(1,1) NOT NULL,
            PerfilCargoId INT NOT NULL,
            Resumen NVARCHAR(MAX) NOT NULL,
            ObjetivoCargo NVARCHAR(MAX) NOT NULL,
            FuncionesPrincipales NVARCHAR(MAX) NOT NULL,
            RequisitosMinimos NVARCHAR(MAX) NOT NULL,
            FormacionExperiencia NVARCHAR(MAX) NOT NULL,
            HardSkills NVARCHAR(MAX) NOT NULL,
            SoftSkills NVARCHAR(MAX) NOT NULL,
            Modalidad NVARCHAR(200) NOT NULL,
            Ubicacion NVARCHAR(200) NOT NULL,
            BandaSalarial NVARCHAR(200) NOT NULL,
            CriteriosEvaluacion NVARCHAR(MAX) NOT NULL,
            CaracteristicasClave NVARCHAR(MAX) NOT NULL,
            ValoracionPerfil NVARCHAR(MAX) NOT NULL,
            CreatedBy VARCHAR(100) NOT NULL,
            CreatedDate DATETIME NOT NULL CONSTRAINT DF_ResumenEjecutivos_CreatedDate DEFAULT GETUTCDATE(),
            ModifiedBy VARCHAR(100) NULL,
            ModifiedDate DATETIME NULL,
            CONSTRAINT PK_ResumenEjecutivos PRIMARY KEY (ResumenId),
            CONSTRAINT UQ_ResumenEjecutivos_PerfilCargo UNIQUE (PerfilCargoId)
        );
    END

    -- 4. Crear Llaves Foráneas por separado si no existen
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PerfilObservaciones_PerfilesCargo' AND parent_object_id = OBJECT_ID('dbo.PerfilObservaciones'))
    BEGIN
        ALTER TABLE dbo.PerfilObservaciones 
        ADD CONSTRAINT FK_PerfilObservaciones_PerfilesCargo 
        FOREIGN KEY (PerfilCargoId) REFERENCES dbo.PerfilesCargo (PerfilCargoId);
    END

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PerfilObservaciones_TiposObservacion' AND parent_object_id = OBJECT_ID('dbo.PerfilObservaciones'))
    BEGIN
        ALTER TABLE dbo.PerfilObservaciones 
        ADD CONSTRAINT FK_PerfilObservaciones_TiposObservacion 
        FOREIGN KEY (TipoObservacionId) REFERENCES dbo.TiposObservacion (TipoObservacionId);
    END

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PerfilObservaciones_Usuarios_Solicitante' AND parent_object_id = OBJECT_ID('dbo.PerfilObservaciones'))
    BEGIN
        ALTER TABLE dbo.PerfilObservaciones 
        ADD CONSTRAINT FK_PerfilObservaciones_Usuarios_Solicitante 
        FOREIGN KEY (UsuarioSolicitanteId) REFERENCES dbo.Usuarios (UsuarioId);
    END

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PerfilObservaciones_Usuarios_AtendidaPor' AND parent_object_id = OBJECT_ID('dbo.PerfilObservaciones'))
    BEGIN
        ALTER TABLE dbo.PerfilObservaciones 
        ADD CONSTRAINT FK_PerfilObservaciones_Usuarios_AtendidaPor 
        FOREIGN KEY (AtendidaPorUsuarioId) REFERENCES dbo.Usuarios (UsuarioId);
    END

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ResumenEjecutivos_PerfilesCargo' AND parent_object_id = OBJECT_ID('dbo.ResumenEjecutivos'))
    BEGIN
        ALTER TABLE dbo.ResumenEjecutivos 
        ADD CONSTRAINT FK_ResumenEjecutivos_PerfilesCargo 
        FOREIGN KEY (PerfilCargoId) REFERENCES dbo.PerfilesCargo (PerfilCargoId);
    END

    -- 5. Sembrar Estados del Perfil Faltantes
    IF NOT EXISTS (SELECT 1 FROM dbo.Estados WHERE Codigo = 'PERF-PEN-GEN')
        INSERT INTO dbo.Estados (Codigo, Nombre, Entidad, SLAId) VALUES ('PERF-PEN-GEN', 'Pendiente de generación', 'Perfil', NULL);

    IF NOT EXISTS (SELECT 1 FROM dbo.Estados WHERE Codigo = 'PERF-REV-RRHH')
        INSERT INTO dbo.Estados (Codigo, Nombre, Entidad, SLAId) VALUES ('PERF-REV-RRHH', 'En revisión RRHH', 'Perfil', NULL);

    IF NOT EXISTS (SELECT 1 FROM dbo.Estados WHERE Codigo = 'PERF-RES-GEN')
        INSERT INTO dbo.Estados (Codigo, Nombre, Entidad, SLAId) VALUES ('PERF-RES-GEN', 'Resumen ejecutivo generado', 'Perfil', NULL);

    IF NOT EXISTS (SELECT 1 FROM dbo.Estados WHERE Codigo = 'PERF-REV-AREA')
        INSERT INTO dbo.Estados (Codigo, Nombre, Entidad, SLAId) VALUES ('PERF-REV-AREA', 'En revisión área solicitante', 'Perfil', NULL);

    IF NOT EXISTS (SELECT 1 FROM dbo.Estados WHERE Codigo = 'PERF-OBS-AREA')
        INSERT INTO dbo.Estados (Codigo, Nombre, Entidad, SLAId) VALUES ('PERF-OBS-AREA', 'Observado por área solicitante', 'Perfil', NULL);

    IF NOT EXISTS (SELECT 1 FROM dbo.Estados WHERE Codigo = 'PERF-COR-RRHH')
        INSERT INTO dbo.Estados (Codigo, Nombre, Entidad, SLAId) VALUES ('PERF-COR-RRHH', 'En corrección RRHH', 'Perfil', NULL);

    IF NOT EXISTS (SELECT 1 FROM dbo.Estados WHERE Codigo = 'PERF-APR-AREA')
        INSERT INTO dbo.Estados (Codigo, Nombre, Entidad, SLAId) VALUES ('PERF-APR-AREA', 'Aprobado por área solicitante', 'Perfil', NULL);

    IF NOT EXISTS (SELECT 1 FROM dbo.Estados WHERE Codigo = 'PERF-APR-FIN')
        INSERT INTO dbo.Estados (Codigo, Nombre, Entidad, SLAId) VALUES ('PERF-APR-FIN', 'Perfil aprobado final', 'Perfil', NULL);

    -- 6. Crear índice único en dbo.SolicitudDocumentos si no existe
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes 
        WHERE object_id = OBJECT_ID('dbo.SolicitudDocumentos') AND name = 'UQ_SolicitudDocumentos_Solicitud_Tipo'
    )
    BEGIN
        -- Eliminar los 4 duplicados antiguos detectados
        DELETE FROM dbo.SolicitudDocumentos WHERE DocumentoId IN (9, 11, 6, 29);

        DECLARE @Duplicados INT = 0;
        SELECT @Duplicados = COUNT(*) FROM (
            SELECT SolicitudId, TipoDocumento FROM dbo.SolicitudDocumentos 
            GROUP BY SolicitudId, TipoDocumento HAVING COUNT(*) > 1
        ) AS t;

        IF @Duplicados > 0
        BEGIN
            RAISERROR('Error: Existen registros duplicados de SolicitudId + TipoDocumento en dbo.SolicitudDocumentos. No es posible crear la restricción única.', 16, 1);
        END
        ELSE
        BEGIN
            CREATE UNIQUE INDEX UQ_SolicitudDocumentos_Solicitud_Tipo 
            ON dbo.SolicitudDocumentos (SolicitudId, TipoDocumento);
        END
    END

    COMMIT TRANSACTION;
    PRINT 'Transacción confirmada con éxito.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 
    BEGIN
        ROLLBACK TRANSACTION;
    END
    PRINT 'Error detectado en la migración SQL. Transacción abortada.';
    ;THROW;
END CATCH
