SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE SIR_NacionalSeguros;
GO

-- Establecer el contexto de sesión para omitir el RLS durante la siembra de datos
EXEC sp_set_session_context 'UserRol', 'Administrador';
GO

-- 1. Insertar usuarios de prueba adicionales si no existen
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Correo = 'rrhh@nacionalseguros.com.bo')
BEGIN
    INSERT INTO Usuarios (Nombre, Correo, ClaveHash, TipoAutenticacion, Estado, MfaHabilitado, CreatedBy)
    VALUES (N'Roberto Méndez (RRHH)', N'rrhh@nacionalseguros.com.bo', N'5f884ba895a75cfc2c4fa84bb6461a293a525fcf93cd426d0a7a030190dc42d5', N'Local', N'Activo', 0, N'SYSTEM');
    
    DECLARE @NewRrhhId INT = (SELECT UsuarioId FROM Usuarios WHERE Correo = 'rrhh@nacionalseguros.com.bo');
    INSERT INTO UsuarioRoles (UsuarioId, RolId) VALUES (@NewRrhhId, 2); -- Rol RRHH
END

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Correo = 'reclutador@nacionalseguros.com.bo')
BEGIN
    INSERT INTO Usuarios (Nombre, Correo, ClaveHash, TipoAutenticacion, Estado, MfaHabilitado, CreatedBy)
    VALUES (N'Carla Rojas (Reclutador)', N'reclutador@nacionalseguros.com.bo', N'5f884ba895a75cfc2c4fa84bb6461a293a525fcf93cd426d0a7a030190dc42d5', N'Local', N'Activo', 0, N'SYSTEM');
    
    DECLARE @NewRecId INT = (SELECT UsuarioId FROM Usuarios WHERE Correo = 'reclutador@nacionalseguros.com.bo');
    INSERT INTO UsuarioRoles (UsuarioId, RolId) VALUES (@NewRecId, 3); -- Rol Reclutador
END
GO

-- 2. Limpiar e insertar solicitudes reales de prueba que coincidan con las de Stitch
DELETE FROM Solicitudes;
GO

-- Declarar y obtener IDs de usuarios
DECLARE @RrhhId INT = (SELECT UsuarioId FROM Usuarios WHERE Correo = 'rrhh@nacionalseguros.com.bo');
DECLARE @RecId INT = (SELECT UsuarioId FROM Usuarios WHERE Correo = 'reclutador@nacionalseguros.com.bo');

-- Insertar SOL-0041 (Analista Actuarial)
INSERT INTO Solicitudes (Cargo, Area, SolicitanteId, DecisorId, Modalidad, Seniority, Prioridad, FechaIdeal, Funciones, Skills, EstadoId, CreatedBy, CreatedDate, IsDeleted)
VALUES (
    N'Analista Actuarial', 
    N'Finanzas', 
    @RrhhId, 
    NULL, 
    N'Presencial', 
    N'Senior', 
    N'Alta', 
    '2026-11-01', 
    N'Análisis actuarial, valuación de reservas, reportes financieros.', 
    N'C#, .NET 8, SQL Server, Excel Avanzado', 
    2, -- SOL-ENV (En revisión RRHH)
    N'rrhh@nacionalseguros.com.bo', 
    GETDATE(), 
    0
);

-- Insertar SOL-0042 (Ejecutivo Comercial)
INSERT INTO Solicitudes (Cargo, Area, SolicitanteId, DecisorId, Modalidad, Seniority, Prioridad, FechaIdeal, Funciones, Skills, EstadoId, CreatedBy, CreatedDate, IsDeleted)
VALUES (
    N'Ejecutivo Comercial', 
    N'Comercial', 
    @RrhhId, 
    NULL, 
    N'Hibrido', 
    N'SemiSenior', 
    N'Media', 
    '2026-11-01', 
    N'Venta de seguros, prospección de clientes, atención de cartera.', 
    N'Ventas, Negociación, Relacionamiento comercial', 
    4, -- SOL-OBS (Con observaciones)
    N'rrhh@nacionalseguros.com.bo', 
    GETDATE(), 
    0
);

-- Insertar SOL-0043 (Asistente Administrativo)
INSERT INTO Solicitudes (Cargo, Area, SolicitanteId, DecisorId, Modalidad, Seniority, Prioridad, FechaIdeal, Funciones, Skills, EstadoId, CreatedBy, CreatedDate, IsDeleted)
VALUES (
    N'Asistente Administrativo', 
    N'Operaciones', 
    @RecId, 
    NULL, 
    N'Presencial', 
    N'Junior', 
    N'Baja', 
    '2026-11-01', 
    N'Apoyo administrativo general al área; Gestión, foliación y orden de archivos; Atención y canalización de llamadas.', 
    N'Excel intermedio, Organización, Comunicación asertiva', 
    3, -- SOL-APR (Aprobada / Aprobada para perfil)
    N'reclutador@nacionalseguros.com.bo', 
    GETDATE(), 
    0
);

PRINT 'Datos de prueba insertados correctamente.';
GO
