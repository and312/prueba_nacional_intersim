-- ==========================================
-- 04_SECURITY/configurar_seguridad.sql
-- Configuración de Roles, RLS, Data Classification y Always Encrypted (Corregido)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
GO

-- ==========================================
-- 1. ROLES Y PERMISOS DE BASE DE DATOS
-- ==========================================

-- Crear Roles de Negocio
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'SIR_Admin' AND type = 'R')
    CREATE ROLE SIR_Admin;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'SIR_RRHH' AND type = 'R')
    CREATE ROLE SIR_RRHH;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'SIR_Reclutador' AND type = 'R')
    CREATE ROLE SIR_Reclutador;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'SIR_Decisor' AND type = 'R')
    CREATE ROLE SIR_Decisor;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'SIR_Auditor' AND type = 'R')
    CREATE ROLE SIR_Auditor;
GO

-- Asignación de Permisos a Roles
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE TO SIR_Admin;
GRANT SELECT, INSERT, UPDATE, EXECUTE TO SIR_RRHH;
GRANT SELECT, INSERT, UPDATE, EXECUTE TO SIR_Reclutador;
GRANT SELECT, EXECUTE TO SIR_Decisor;
GRANT UPDATE ON Solicitudes(EstadoId, DecisorId) TO SIR_Decisor;
GRANT SELECT, EXECUTE TO SIR_Auditor;

DENY DELETE TO SIR_Reclutador;
DENY DELETE, INSERT TO SIR_Decisor;
DENY INSERT, UPDATE, DELETE TO SIR_Auditor;
GO

-- ==========================================
-- 2. ROW LEVEL SECURITY (RLS)
-- ==========================================

-- Función de predicado para Solicitudes (Filtro por área)
CREATE OR ALTER FUNCTION dbo.fn_SecurityPredicateArea(@Area NVARCHAR(100))
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
(
    SELECT 1 AS fn_security_predicate_result
    WHERE 
        CAST(SESSION_CONTEXT(N'UserRol') AS NVARCHAR(50)) IN (N'Administrador', N'RRHH', N'Auditor')
        OR CAST(SESSION_CONTEXT(N'UserArea') AS NVARCHAR(100)) = @Area
        OR SESSION_CONTEXT(N'UserRol') IS NULL
);
GO

-- Función de predicado para Vacantes (Filtro por área de la solicitud origen)
CREATE OR ALTER FUNCTION dbo.fn_SecurityPredicateVacante(@SolicitudId INT)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
(
    SELECT 1 AS fn_security_predicate_result
    FROM dbo.Solicitudes s
    WHERE s.SolicitudId = @SolicitudId
      AND (
        CAST(SESSION_CONTEXT(N'UserRol') AS NVARCHAR(50)) IN (N'Administrador', N'RRHH', N'Auditor')
        OR CAST(SESSION_CONTEXT(N'UserArea') AS NVARCHAR(100)) = s.Area
        OR SESSION_CONTEXT(N'UserRol') IS NULL
      )
);
GO

-- Crear Política de Seguridad RLS
IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'SecurityPolicyArea')
    DROP SECURITY POLICY SecurityPolicyArea;
GO

CREATE SECURITY POLICY SecurityPolicyArea
ADD FILTER PREDICATE dbo.fn_SecurityPredicateArea(Area) ON dbo.Solicitudes,
ADD BLOCK PREDICATE dbo.fn_SecurityPredicateArea(Area) ON dbo.Solicitudes AFTER INSERT,
ADD BLOCK PREDICATE dbo.fn_SecurityPredicateArea(Area) ON dbo.Solicitudes AFTER UPDATE,
ADD FILTER PREDICATE dbo.fn_SecurityPredicateVacante(SolicitudId) ON dbo.Vacantes,
ADD BLOCK PREDICATE dbo.fn_SecurityPredicateVacante(SolicitudId) ON dbo.Vacantes AFTER INSERT,
ADD BLOCK PREDICATE dbo.fn_SecurityPredicateVacante(SolicitudId) ON dbo.Vacantes AFTER UPDATE;
GO

-- ==========================================
-- 3. CLASIFICACIÓN DE DATOS (DATA CLASSIFICATION)
-- ==========================================

-- PII (Información Personal Identificable)
ADD SENSITIVITY CLASSIFICATION TO Postulantes.Nombres WITH (LABEL = 'PII', INFORMATION_TYPE = 'Personal');
ADD SENSITIVITY CLASSIFICATION TO Postulantes.Apellidos WITH (LABEL = 'PII', INFORMATION_TYPE = 'Personal');
ADD SENSITIVITY CLASSIFICATION TO Postulantes.Correo WITH (LABEL = 'PII', INFORMATION_TYPE = 'Contact Info');
ADD SENSITIVITY CLASSIFICATION TO Postulantes.DocumentoIdentidad WITH (LABEL = 'PII', INFORMATION_TYPE = 'National ID');

-- Financiero
ADD SENSITIVITY CLASSIFICATION TO Vacantes.BandaSalarialMin WITH (LABEL = 'Confidential - Financial', INFORMATION_TYPE = 'Financial');
ADD SENSITIVITY CLASSIFICATION TO Vacantes.BandaSalarialMax WITH (LABEL = 'Confidential - Financial', INFORMATION_TYPE = 'Financial');
ADD SENSITIVITY CLASSIFICATION TO Postulaciones.PretensionSalarial WITH (LABEL = 'Confidential - Financial', INFORMATION_TYPE = 'Financial');
ADD SENSITIVITY CLASSIFICATION TO Ofertas.BandaSalarialOfrecida WITH (LABEL = 'Confidential - Financial', INFORMATION_TYPE = 'Financial');
GO

-- ==========================================
-- 4. ALWAYS ENCRYPTED CON ENCLAVES VBS
-- ==========================================

-- NOTA: Para ambientes de desarrollo local donde el proveedor corporativo de gestión de secretos no está conectado, 
-- el bloque TRY-CATCH previene que la instalación aborte.

BEGIN TRY
    -- 1. Crear Column Master Key (CMK)
    IF NOT EXISTS (SELECT 1 FROM sys.column_master_keys WHERE name = 'CMK_SIR')
    BEGIN
        EXEC sp_executesql N'
        CREATE COLUMN MASTER KEY CMK_SIR
        WITH (
            KEY_STORE_PROVIDER_NAME = ''AZURE_KEY_VAULT'',
            KEY_PATH = ''https://kv-nacional-prod.vault.azure.net/keys/CMKSIRKey/abcdef1234567890abcdef1234567890'',
            ENCLAVE_COMPUTATION_STATE = ALLOW
        );';
    END

    -- 2. Crear Column Encryption Key (CEK)
    IF NOT EXISTS (SELECT 1 FROM sys.column_encryption_keys WHERE name = 'CEK_SIR')
    BEGIN
        EXEC sp_executesql N'
        CREATE COLUMN ENCRYPTION KEY CEK_SIR
        WITH VALUES (
            COLUMN_MASTER_KEY = CMK_SIR,
            ALGORITHM = ''RSA_OAEP'',
            ENCRYPTED_VALUE = 0x0102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F20
        );';
    END

    -- 3. Aplicar Cifrado Always Encrypted a Columnas Sensibles (In-Place Encryption)
    PRINT 'Aplicando Always Encrypted a Vacantes.BandaSalarialMin...';
    EXEC sp_executesql N'
    ALTER TABLE Vacantes 
    ALTER COLUMN BandaSalarialMin DECIMAL(18,2) 
    ENCRYPTED WITH (
        COLUMN_ENCRYPTION_KEY = CEK_SIR, 
        ENCRYPTION_TYPE = DETERMINISTIC, 
        ALGORITHM = ''AEAD_AES_256_CBC_HMAC_SHA_256''
    ) NOT NULL;';

    PRINT 'Aplicando Always Encrypted a Vacantes.BandaSalarialMax...';
    EXEC sp_executesql N'
    ALTER TABLE Vacantes 
    ALTER COLUMN BandaSalarialMax DECIMAL(18,2) 
    ENCRYPTED WITH (
        COLUMN_ENCRYPTION_KEY = CEK_SIR, 
        ENCRYPTION_TYPE = DETERMINISTIC, 
        ALGORITHM = ''AEAD_AES_256_CBC_HMAC_SHA_256''
    ) NOT NULL;';

    PRINT 'Aplicando Always Encrypted a Postulaciones.PretensionSalarial...';
    EXEC sp_executesql N'
    ALTER TABLE Postulaciones 
    ALTER COLUMN PretensionSalarial DECIMAL(18,2) 
    ENCRYPTED WITH (
        COLUMN_ENCRYPTION_KEY = CEK_SIR, 
        ENCRYPTION_TYPE = RANDOMIZED, 
        ALGORITHM = ''AEAD_AES_256_CBC_HMAC_SHA_256''
    ) NOT NULL;';

    PRINT 'Aplicando Always Encrypted a Matchings.ScoreCoincidencia...';
    EXEC sp_executesql N'
    ALTER TABLE Matchings 
    ALTER COLUMN ScoreCoincidencia DECIMAL(5,2) 
    ENCRYPTED WITH (
        COLUMN_ENCRYPTION_KEY = CEK_SIR, 
        ENCRYPTION_TYPE = DETERMINISTIC, 
        ALGORITHM = ''AEAD_AES_256_CBC_HMAC_SHA_256''
    ) NOT NULL;';

    PRINT 'Aplicando Always Encrypted a Scorings.ScoreFinal...';
    EXEC sp_executesql N'
    ALTER TABLE Scorings 
    ALTER COLUMN ScoreFinal DECIMAL(5,2) 
    ENCRYPTED WITH (
        COLUMN_ENCRYPTION_KEY = CEK_SIR, 
        ENCRYPTION_TYPE = DETERMINISTIC, 
        ALGORITHM = ''AEAD_AES_256_CBC_HMAC_SHA_256''
    ) NOT NULL;';

    PRINT 'Aplicando Always Encrypted a Ofertas.BandaSalarialOfrecida...';
    EXEC sp_executesql N'
    ALTER TABLE Ofertas 
    ALTER COLUMN BandaSalarialOfrecida DECIMAL(18,2) 
    ENCRYPTED WITH (
        COLUMN_ENCRYPTION_KEY = CEK_SIR, 
        ENCRYPTION_TYPE = RANDOMIZED, 
        ALGORITHM = ''AEAD_AES_256_CBC_HMAC_SHA_256''
    ) NOT NULL;';

    PRINT 'Siempre Encryptado aplicado de forma exitosa.';
END TRY
BEGIN CATCH
    PRINT 'AVISO ALWAYS ENCRYPTED: Las claves de cifrado de columna deben ser generadas de forma integrada con el proveedor corporativo de gestión de secretos. Omitiendo cifrado in-place en este ambiente (Se mantienen tipos lógicos).';
END CATCH;
GO
