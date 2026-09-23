USE SIR_NacionalSeguros;
GO

-- Añadir columnas para recuperación de contraseña si no existen
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'ResetPasswordToken')
BEGIN
    ALTER TABLE [dbo].[Usuarios] ADD [ResetPasswordToken] NVARCHAR(200) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND name = N'ResetPasswordTokenExpiration')
BEGIN
    ALTER TABLE [dbo].[Usuarios] ADD [ResetPasswordTokenExpiration] DATETIME2 NULL;
END
GO

PRINT 'Columnas de recuperación de contraseña agregadas a la tabla Usuarios.';
