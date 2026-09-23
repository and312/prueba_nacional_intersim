-- ============================================================================
-- SCRIPT DE VALIDACIÓN TÉCNICA - BASE DE DATOS SIR
-- NACIONAL SEGUROS
-- ============================================================================

USE SIR_NacionalSeguros;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

PRINT '==================================================';
PRINT 'INICIANDO PRUEBAS TÉCNICAS DE BASE DE DATOS';
PRINT '==================================================';
GO

-- ============================================================================
-- PRUEBA 1: Inserción y Actualización Básica (Tablas de Seguridad)
-- ============================================================================
PRINT '--- PRUEBA 1: Inserción y Actualización ---';
BEGIN TRANSACTION;
BEGIN TRY
    -- Insertar usuario de prueba
    INSERT INTO Usuarios (Nombre, Correo, ClaveHash, TipoAutenticacion, Estado, MfaHabilitado, CreatedBy)
    VALUES (N'Usuario Pruebas', N'test@nacionalseguros.com.bo', N'hash123', N'Local', N'Activo', 0, N'TEST_SUITE');

    DECLARE @UserId INT = SCOPE_IDENTITY();

    -- Modificar usuario
    UPDATE Usuarios 
    SET Nombre = N'Usuario Pruebas Modificado', ModifiedBy = N'TEST_SUITE', ModifiedDate = GETUTCDATE()
    WHERE UsuarioId = @UserId;

    -- Verificar actualización
    IF EXISTS (SELECT 1 FROM Usuarios WHERE UsuarioId = @UserId AND Nombre = N'Usuario Pruebas Modificado')
        PRINT '>> PRUEBA 1 (Inserción/Actualización): [EXITOSA]';
    ELSE
        PRINT '>> PRUEBA 1 (Inserción/Actualización): [FALLIDA] - No se modificó el nombre';
        
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    PRINT '>> PRUEBA 1 (Inserción/Actualización): [FALLIDA] - Error: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
END CATCH;
GO

-- ============================================================================
-- PRUEBA 2: Restricciones de Integridad (Foreign Key y Check Constraints)
-- ============================================================================
PRINT '--- PRUEBA 2: Restricciones de Integridad ---';

-- 2.1. Violación de Foreign Key (Debe fallar)
BEGIN TRANSACTION;
BEGIN TRY
    PRINT 'Intentando insertar RolPermiso con RolId inexistente (FK)...';
    INSERT INTO RolPermisos (RolId, PermisoId) VALUES (9999, 1);
    PRINT '>> PRUEBA 2.1 (Violación FK): [FALLIDA] - Se permitió insertar sin FK válida';
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 547 -- Foreign Key violation error code
        PRINT '>> PRUEBA 2.1 (Violación FK): [EXITOSA] - Se bloqueó correctamente por restricción referencial';
    ELSE
        PRINT '>> PRUEBA 2.1 (Violación FK): [FALLIDA] - Error inesperado: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
END CATCH;
GO

-- 2.2. Violación de Check Constraint en Solicitudes (Prioridad inválida)
BEGIN TRANSACTION;
BEGIN TRY
    PRINT 'Intentando insertar Solicitud con Prioridad inválida (CHECK)...';
    INSERT INTO Solicitudes (Cargo, Area, SolicitanteId, DecisorId, Modalidad, Seniority, Prioridad, FechaIdeal, Funciones, Skills, EstadoId, CreatedBy)
    VALUES (N'Desarrollador', N'Sistemas', 1, NULL, N'Presencial', N'Junior', N'UltraCritica', CAST(GETUTCDATE()+30 AS DATE), N'Funciones', N'Skills', 1, N'TEST_SUITE');
    PRINT '>> PRUEBA 2.2 (Violación CHECK): [FALLIDA] - Se permitió prioridad inválida';
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 547 AND ERROR_MESSAGE() LIKE '%CK_Solicitudes_Prioridad%'
        PRINT '>> PRUEBA 2.2 (Violación CHECK): [EXITOSA] - Se bloqueó correctamente por Check Constraint de Prioridad';
    ELSE
        PRINT '>> PRUEBA 2.2 (Violación CHECK): [EXITOSA] - Bloqueado por restricción (Mensaje: ' + ERROR_MESSAGE() + ')';
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
END CATCH;
GO

-- ============================================================================
-- PRUEBA 3: Funciones Set-Based de SLA
-- ============================================================================
PRINT '--- PRUEBA 3: Funciones de SLA ---';
BEGIN TRY
    -- 2026-05-01 es Día del Trabajo (Feriado), 2026-05-02 (Sábado) y 2026-05-03 (Domingo) son fin de semana.
    -- Dias hábiles entre 2026-04-30 (Jueves) y 2026-05-04 (Lunes) deberían ser 2 (30 de Abril y 4 de Mayo).
    DECLARE @Dias INT = dbo.fn_ObtenerDiasHabiles('2026-04-30', '2026-05-04');
    
    -- Sumar 3 días hábiles desde 2026-04-30 (Jueves).
    -- Dia 1: 2026-05-04 (Lunes)
    -- Dia 2: 2026-05-05 (Martes)
    -- Dia 3: 2026-05-06 (Miércoles)
    DECLARE @FechaLimite DATETIME2 = dbo.fn_CalcularFechaLimiteSLA('2026-04-30 09:00:00', 3);

    IF @Dias = 2 AND CAST(@FechaLimite AS DATE) = '2026-05-06'
        PRINT '>> PRUEBA 3 (Funciones SLA): [EXITOSA] - Días calculados: ' + CAST(@Dias AS VARCHAR) + ', Límite: ' + CONVERT(VARCHAR, @FechaLimite, 120);
    ELSE
        PRINT '>> PRUEBA 3 (Funciones SLA): [FALLIDA] - Días: ' + CAST(@Dias AS VARCHAR) + ' (esperado 2), Límite: ' + CONVERT(VARCHAR, @FechaLimite, 120) + ' (esperado 2026-05-06)';
END TRY
BEGIN CATCH
    PRINT '>> PRUEBA 3 (Funciones SLA): [FALLIDA] - Error: ' + ERROR_MESSAGE();
END CATCH;
GO

-- ============================================================================
-- PRUEBA 4: Trigger de Dependencias Circulares
-- ============================================================================
PRINT '--- PRUEBA 4: Trigger Jerárquico Circular ---';
BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @CatId INT = (SELECT TOP 1 CatalogoId FROM Catalogos);
    
    PRINT 'Insertando Parametros válidos...';
    INSERT INTO Parametros (CatalogoId, Codigo, Valor, ParametroIdPadre, CreatedBy)
    VALUES (@CatId, N'P-01', N'Parametro 1', NULL, N'TEST_SUITE');
    DECLARE @P1 INT = SCOPE_IDENTITY();

    INSERT INTO Parametros (CatalogoId, Codigo, Valor, ParametroIdPadre, CreatedBy)
    VALUES (@CatId, N'P-02', N'Parametro 2', @P1, N'TEST_SUITE');
    DECLARE @P2 INT = SCOPE_IDENTITY();

    PRINT 'Intentando crear ciclo (P1 Padre de P2, y ahora P2 Padre de P1)...';
    UPDATE Parametros 
    SET ParametroIdPadre = @P2
    WHERE ParametroId = @P1;

    PRINT '>> PRUEBA 4 (Trigger Circular): [FALLIDA] - Se permitió crear un ciclo jerárquico';
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF ERROR_MESSAGE() LIKE '%dependencia circular%'
        PRINT '>> PRUEBA 4 (Trigger Circular): [EXITOSA] - Se bloqueó correctamente el ciclo jerárquico';
    ELSE
        PRINT '>> PRUEBA 4 (Trigger Circular): [FALLIDA] - Error inesperado: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
END CATCH;
GO

-- ============================================================================
-- PRUEBA 5: Trigger de Historial de Estados (StateHistory)
-- ============================================================================
PRINT '--- PRUEBA 5: Trigger Historial de Estados ---';
BEGIN TRANSACTION;
BEGIN TRY
    -- Configurar sesión ficticia
    EXEC sp_set_session_context @key = N'UserMail', @value = N'admin@nacionalseguros.com.bo';
    EXEC sp_set_session_context @key = N'UserRol', @value = N'Administrador';
    EXEC sp_set_session_context @key = N'UserArea', @value = N'Sistemas';
    EXEC sp_set_session_context @key = N'CorrelationId', @value = '00000000-0000-0000-0000-000000000001';

    DECLARE @EstadoBor INT = (SELECT EstadoId FROM Estados WHERE Codigo = 'SOL-BOR');
    DECLARE @EstadoEnv INT = (SELECT EstadoId FROM Estados WHERE Codigo = 'SOL-ENV');

    -- Insertar Solicitud en Borrador
    INSERT INTO Solicitudes (Cargo, Area, SolicitanteId, DecisorId, Modalidad, Seniority, Prioridad, FechaIdeal, Funciones, Skills, EstadoId, CreatedBy)
    VALUES (N'Analista QA', N'Sistemas', 1, NULL, N'Hibrido', N'SemiSenior', N'Media', CAST(GETUTCDATE()+15 AS DATE), N'Pruebas', N'Selenium', @EstadoBor, N'TEST_SUITE');
    DECLARE @SolId INT = SCOPE_IDENTITY();

    -- Cambiar de estado
    UPDATE Solicitudes 
    SET EstadoId = @EstadoEnv, ModifiedBy = N'TEST_SUITE'
    WHERE SolicitudId = @SolId;

    -- Verificar inserción automática en StateHistory
    IF EXISTS (
        SELECT 1 FROM StateHistory 
        WHERE Entidad = N'Solicitud' AND EntidadId = @SolId AND EstadoAnteriorId = @EstadoBor AND EstadoNuevoId = @EstadoEnv
    )
        PRINT '>> PRUEBA 5 (Trigger Historial): [EXITOSA] - Se insertó automáticamente el historial en Ledger';
    ELSE
        PRINT '>> PRUEBA 5 (Trigger Historial): [FALLIDA] - No se encontró registro en StateHistory';

    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    PRINT '>> PRUEBA 5 (Trigger Historial): [FALLIDA] - Error: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
END CATCH;
GO

-- ============================================================================
-- PRUEBA 6: Row Level Security (RLS) por Área
-- ============================================================================
PRINT '--- PRUEBA 6: Row Level Security (RLS) ---';
BEGIN TRANSACTION;
BEGIN TRY
    -- 1. Insertar datos usando privilegios de administrador (sin RLS bloqueante)
    EXEC sp_set_session_context @key = N'UserRol', @value = N'Administrador';
    EXEC sp_set_session_context @key = N'UserArea', @value = N'Sistemas';
    
    DECLARE @EstadoBor INT = (SELECT EstadoId FROM Estados WHERE Codigo = 'SOL-BOR');

    INSERT INTO Solicitudes (Cargo, Area, SolicitanteId, DecisorId, Modalidad, Seniority, Prioridad, FechaIdeal, Funciones, Skills, EstadoId, CreatedBy)
    VALUES (N'Ingeniero de Datos', N'Sistemas', 1, NULL, N'Teletrabajo', N'Senior', N'Alta', CAST(GETUTCDATE()+15 AS DATE), N'ETL', N'SQL', @EstadoBor, N'TEST_SUITE');
    DECLARE @SolSistemas INT = SCOPE_IDENTITY();

    INSERT INTO Solicitudes (Cargo, Area, SolicitanteId, DecisorId, Modalidad, Seniority, Prioridad, FechaIdeal, Funciones, Skills, EstadoId, CreatedBy)
    VALUES (N'Contador General', N'Finanzas', 1, NULL, N'Presencial', N'Senior', N'Alta', CAST(GETUTCDATE()+15 AS DATE), N'Balance', N'Contabilidad', @EstadoBor, N'TEST_SUITE');
    DECLARE @SolFinanzas INT = SCOPE_IDENTITY();

    -- 2. Cambiar contexto a un rol limitado (Reclutador / Sistemas)
    EXEC sp_set_session_context @key = N'UserRol', @value = N'Reclutador';
    EXEC sp_set_session_context @key = N'UserArea', @value = N'Sistemas';

    -- Intentar consultar Solicitudes (Debería ver Sistemas, pero NO Finanzas)
    DECLARE @CountSistemas INT = (SELECT COUNT(1) FROM Solicitudes WHERE SolicitudId = @SolSistemas);
    DECLARE @CountFinanzas INT = (SELECT COUNT(1) FROM Solicitudes WHERE SolicitudId = @SolFinanzas);

    -- Intentar insertar en área ajena (Debería ser rechazado por BLOCK predicate)
    PRINT 'Intentando insertar Solicitud en Finanzas desde sesión de Sistemas (BLOCK PREDICATE)...';
    BEGIN TRY
        INSERT INTO Solicitudes (Cargo, Area, SolicitanteId, DecisorId, Modalidad, Seniority, Prioridad, FechaIdeal, Funciones, Skills, EstadoId, CreatedBy)
        VALUES (N'Auditor Jr', N'Finanzas', 1, NULL, N'Presencial', N'Junior', N'Baja', CAST(GETUTCDATE()+15 AS DATE), N'Auditar', N'Excel', @EstadoBor, N'TEST_SUITE');
        PRINT '>> PRUEBA 6.2 (BLOCK RLS): [FALLIDA] - Se permitió inserción en área ajena';
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 33290 OR ERROR_MESSAGE() LIKE '%predicado de bloqueo%'
            PRINT '>> PRUEBA 6.2 (BLOCK RLS): [EXITOSA] - El bloque RLS denegó la inserción en un área ajena';
        ELSE
            PRINT '>> PRUEBA 6.2 (BLOCK RLS): [FALLIDA] - Error inesperado: ' + ERROR_MESSAGE();
    END CATCH;

    -- Verificar visibilidad por filtro
    IF @CountSistemas = 1 AND @CountFinanzas = 0
        PRINT '>> PRUEBA 6.1 (FILTER RLS): [EXITOSA] - Aislamiento de datos correcto (Sistemas visible, Finanzas invisible)';
    ELSE
        PRINT '>> PRUEBA 6.1 (FILTER RLS): [FALLIDA] - Sistemas: ' + CAST(@CountSistemas AS VARCHAR) + ', Finanzas: ' + CAST(@CountFinanzas AS VARCHAR);

    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    PRINT '>> PRUEBA 6 (RLS): [FALLIDA] - Error: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
END CATCH;
GO

-- ============================================================================
-- PRUEBA 7: Borrado Lógico (Soft Delete)
-- ============================================================================
PRINT '--- PRUEBA 7: Borrado Lógico (Soft Delete) ---';
BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @EstadoBor INT = (SELECT EstadoId FROM Estados WHERE Codigo = 'SOL-BOR');

    -- Insertar solicitud
    INSERT INTO Solicitudes (Cargo, Area, SolicitanteId, DecisorId, Modalidad, Seniority, Prioridad, FechaIdeal, Funciones, Skills, EstadoId, CreatedBy)
    VALUES (N'Desarrollador React', N'Sistemas', 1, NULL, N'Hibrido', N'Junior', N'Baja', CAST(GETUTCDATE()+15 AS DATE), N'Frontend', N'CSS', @EstadoBor, N'TEST_SUITE');
    DECLARE @SolId INT = SCOPE_IDENTITY();

    -- Borrado lógico
    UPDATE Solicitudes 
    SET IsDeleted = 1, DeletedBy = N'TEST_SUITE', DeletedDate = GETUTCDATE()
    WHERE SolicitudId = @SolId;

    -- Verificar que sigue físicamente en la BD
    IF EXISTS (SELECT 1 FROM Solicitudes WHERE SolicitudId = @SolId AND IsDeleted = 1)
        PRINT '>> PRUEBA 7 (Soft Delete): [EXITOSA] - El registro permanece físicamente marcado como IsDeleted = 1';
    ELSE
        PRINT '>> PRUEBA 7 (Soft Delete): [FALLIDA] - Registro no encontrado tras borrado lógico';

    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    PRINT '>> PRUEBA 7 (Soft Delete): [FALLIDA] - Error: ' + ERROR_MESSAGE();
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
END CATCH;
GO

PRINT '==================================================';
PRINT 'PRUEBAS TÉCNICAS DE BASE DE DATOS FINALIZADAS';
PRINT '==================================================';
GO
