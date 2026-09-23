-- ==========================================
-- 09_SEED_DATA/semilla.sql
-- Inserción de Datos Semilla Obligatorios (Corregido)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
GO

PRINT 'Iniciando carga de datos semilla...';

-- 1. Roles
INSERT INTO Roles (Nombre, Descripcion, CreatedBy)
VALUES 
(N'Administrador', N'Acceso total a configuraciones y auditoría del sistema.', N'SYSTEM'),
(N'RRHH', N'Gestión operativa de vacantes, perfiles y selección.', N'SYSTEM'),
(N'Reclutador', N'Gestión operativa de candidatos y programación de entrevistas.', N'SYSTEM'),
(N'Decisor', N'Líderes de área que aprueban solicitudes y ofertas.', N'SYSTEM'),
(N'Auditor', N'Acceso de solo lectura a logs y métricas de gobernanza.', N'SYSTEM'),
(N'Solicitante', N'Inicio y seguimiento de solicitudes de personal de su área.', N'SYSTEM');
GO

-- 2. Permisos
INSERT INTO Permisos (Codigo, Nombre, CreatedBy)
VALUES 
(N'solicitudes.crear', N'Crear Solicitudes de Personal', N'SYSTEM'),
(N'solicitudes.aprobar', N'Aprobar o Rechazar Solicitudes', N'SYSTEM'),
(N'perfiles.editar', N'Editar Profesiogramas de Cargo', N'SYSTEM'),
(N'vacantes.publicar', N'Publicar Vacantes Activas', N'SYSTEM'),
(N'postulantes.evaluar', N'Ver Scoring y Realizar Evaluaciones', N'SYSTEM'),
(N'ofertas.crear', N'Crear y Enviar Ofertas de Contrato', N'SYSTEM'),
(N'config.slas', N'Modificar Tiempos de SLAs', N'SYSTEM'),
(N'auditoria.ver', N'Ver Logs de Auditoría y Ledger', N'SYSTEM');
GO

-- 3. Mapeo RolPermisos
-- Admin tiene todos
INSERT INTO RolPermisos (RolId, PermisoId)
SELECT 1, PermisoId FROM Permisos;

-- RRHH
INSERT INTO RolPermisos (RolId, PermisoId)
SELECT 2, PermisoId FROM Permisos WHERE Codigo IN ('solicitudes.crear', 'perfiles.editar', 'vacantes.publicar', 'postulantes.evaluar', 'ofertas.crear');

-- Reclutador
INSERT INTO RolPermisos (RolId, PermisoId)
SELECT 3, PermisoId FROM Permisos WHERE Codigo IN ('solicitudes.crear', 'perfiles.editar', 'postulantes.evaluar');

-- Decisor
INSERT INTO RolPermisos (RolId, PermisoId)
SELECT 4, PermisoId FROM Permisos WHERE Codigo IN ('solicitudes.aprobar');

-- Auditor
INSERT INTO RolPermisos (RolId, PermisoId)
SELECT 5, PermisoId FROM Permisos WHERE Codigo IN ('auditoria.ver');

-- Solicitante
INSERT INTO RolPermisos (RolId, PermisoId)
SELECT (SELECT RolId FROM Roles WHERE Nombre = 'Solicitante'), PermisoId FROM Permisos WHERE Codigo IN ('solicitudes.crear');
GO

-- 4. Usuario Administrador Inicial
INSERT INTO Usuarios (Nombre, Correo, ClaveHash, TipoAutenticacion, Estado, MfaHabilitado, CreatedBy)
VALUES (N'Administrador General', N'admin@nacionalseguros.com.bo', N'5f884ba895a75cfc2c4fa84bb6461a293a525fcf93cd426d0a7a030190dc42d5', N'Local', N'Activo', 0, N'SYSTEM');

-- Asignar rol Administrador al usuario inicial
INSERT INTO UsuarioRoles (UsuarioId, RolId)
VALUES (1, 1);
GO

-- 5. SLAs
INSERT INTO SLAs (Nombre, DiasMaximos, Modulo, CreatedBy)
VALUES 
(N'SLA-SOL-01', 3, N'Solicitudes', N'SYSTEM'),
(N'SLA-VAC-01', 2, N'Vacantes', N'SYSTEM'),
(N'SLA-POS-01', 5, N'Filtro Curricular', N'SYSTEM'),
(N'SLA-POS-02', 7, N'Entrevistas', N'SYSTEM'),
(N'SLA-POS-03', 2, N'Respuesta Oferta', N'SYSTEM');
GO

-- 6. Estados (Máquinas de Estado)
-- Solicitudes (SOL-)
DECLARE @SlaSol01Id INT = (SELECT SLAId FROM SLAs WHERE Nombre = 'SLA-SOL-01');
INSERT INTO Estados (Codigo, Nombre, Entidad, SLAId) VALUES
(N'SOL-REG', N'Solicitud Registrada', N'Solicitud', NULL),
(N'SOL-PEN', N'Solicitud Pendiente', N'Solicitud', NULL),
(N'SOL-BOR', N'Borrador', N'Solicitud', NULL),
(N'SOL-ENV', N'Solicitud Enviada a RRHH', N'Solicitud', @SlaSol01Id),
(N'SOL-APR', N'Solicitud Aprobada', N'Solicitud', NULL),
(N'SOL-OBS', N'Solicitud Observada', N'Solicitud', NULL),
(N'SOL-RECH', N'Solicitud Rechazada', N'Solicitud', NULL),
(N'SOL-CAN', N'Cancelada', N'Solicitud', NULL),
(N'SOL-CONV', N'Convertida a Vacante', N'Solicitud', NULL),
(N'SOL-COR', N'Corregida', N'Solicitud', NULL);

-- Vacantes (VAC-)
DECLARE @SlaVac01Id INT = (SELECT SLAId FROM SLAs WHERE Nombre = 'SLA-VAC-01');
DECLARE @SlaPos01Id INT = (SELECT SLAId FROM SLAs WHERE Nombre = 'SLA-POS-01');
DECLARE @SlaPos02Id INT = (SELECT SLAId FROM SLAs WHERE Nombre = 'SLA-POS-02');
DECLARE @SlaPos03Id INT = (SELECT SLAId FROM SLAs WHERE Nombre = 'SLA-POS-03');

INSERT INTO Estados (Codigo, Nombre, Entidad, SLAId) VALUES
(N'VAC-CRE', N'Creada', N'Vacante', NULL),
(N'VAC-PUB', N'Publicada', N'Vacante', @SlaVac01Id),
(N'VAC-CAP', N'Captación', N'Vacante', @SlaPos01Id),
(N'VAC-SHR', N'Shortlist', N'Vacante', @SlaPos02Id),
(N'VAC-OFE', N'Oferta', N'Vacante', @SlaPos03Id),
(N'VAC-CON', N'Contratada', N'Vacante', NULL),
(N'VAC-CER', N'Cerrada', N'Vacante', NULL);

-- Postulantes (POS-)
INSERT INTO Estados (Codigo, Nombre, Entidad, SLAId) VALUES
(N'POS-REG', N'Registrado', N'Postulante', NULL),
(N'POS-CAP', N'Captado', N'Postulante', @SlaPos01Id),
(N'POS-PRE', N'Preseleccionado', N'Postulante', NULL),
(N'POS-PSI', N'Psicotécnica', N'Postulante', NULL),
(N'POS-RRHH', N'Entrevista RRHH', N'Postulante', @SlaPos02Id),
(N'POS-TEC', N'Entrevista Técnica', N'Postulante', @SlaPos02Id),
(N'POS-SHR', N'Shortlist', N'Postulante', NULL),
(N'POS-OFE', N'Oferta', N'Postulante', @SlaPos03Id),
(N'POS-CON', N'Contratado', N'Postulante', NULL),
(N'POS-DES', N'Descartado', N'Postulante', NULL),
(N'POS-RET', N'Retirado', N'Postulante', NULL);
GO

-- 7. Catálogos
INSERT INTO Catalogos (Nombre, Codigo, CreatedBy) VALUES
(N'Modalidades de Trabajo', N'CAT-MOD', N'SYSTEM'),
(N'Seniorities de Cargo', N'CAT-SEN', N'SYSTEM'),
(N'Prioridades de Solicitud', N'CAT-PRI', N'SYSTEM'),
(N'Fuentes de Postulantes', N'CAT-SRE', N'SYSTEM'),
(N'Tipos de Agentes de IA', N'CAT-AGE', N'SYSTEM');
GO

-- 8. Parámetros (Detalles de Catálogos con Cargas Dinámicas)
-- CAT-MOD (Modalidades)
DECLARE @CatModId INT = (SELECT CatalogoId FROM Catalogos WHERE Codigo = 'CAT-MOD');
INSERT INTO Parametros (CatalogoId, Codigo, Valor, ParametroIdPadre, CreatedBy) VALUES
(@CatModId, N'Presencial', N'Presencial (Oficinas Centrales)', NULL, N'SYSTEM'),
(@CatModId, N'Teletrabajo', N'100% Trabajo Remoto', NULL, N'SYSTEM'),
(@CatModId, N'Hibrido', N'Esquema Mixto Remoto/Físico', NULL, N'SYSTEM');

-- CAT-SEN (Seniorities)
DECLARE @CatSenId INT = (SELECT CatalogoId FROM Catalogos WHERE Codigo = 'CAT-SEN');
INSERT INTO Parametros (CatalogoId, Codigo, Valor, ParametroIdPadre, CreatedBy) VALUES
(@CatSenId, N'Junior', N'Nivel Inicial / Menos de 2 años', NULL, N'SYSTEM'),
(@CatSenId, N'SemiSenior', N'Nivel Intermedio / 2 a 5 años', NULL, N'SYSTEM'),
(@CatSenId, N'Senior', N'Nivel Avanzado / Más de 5 años', NULL, N'SYSTEM');

-- CAT-PRI (Prioridades)
DECLARE @CatPriId INT = (SELECT CatalogoId FROM Catalogos WHERE Codigo = 'CAT-PRI');
INSERT INTO Parametros (CatalogoId, Codigo, Valor, ParametroIdPadre, CreatedBy) VALUES
(@CatPriId, N'Baja', N'Prioridad Operativa Estándar', NULL, N'SYSTEM'),
(@CatPriId, N'Media', N'Prioridad Moderada de Negocio', NULL, N'SYSTEM'),
(@CatPriId, N'Alta', N'Prioridad Crítica por Reemplazo', NULL, N'SYSTEM'),
(@CatPriId, N'Critica', N'Urgencia Alta / Sanción o Multa', NULL, N'SYSTEM');

-- CAT-SRE (Fuentes)
DECLARE @CatSreId INT = (SELECT CatalogoId FROM Catalogos WHERE Codigo = 'CAT-SRE');
INSERT INTO Parametros (CatalogoId, Codigo, Valor, ParametroIdPadre, CreatedBy) VALUES
(@CatSreId, N'LinkedIn', N'Postulaciones vía LinkedIn Jobs', NULL, N'SYSTEM'),
(@CatSreId, N'Web', N'Portal de Nacional Seguros', NULL, N'SYSTEM'),
(@CatSreId, N'Referido', N'Referencia por Colaborador', NULL, N'SYSTEM'),
(@CatSreId, N'Manual', N'Registro Manual por Reclutador', NULL, N'SYSTEM');

-- CAT-AGE (Agentes de IA)
DECLARE @CatAgeId INT = (SELECT CatalogoId FROM Catalogos WHERE Codigo = 'CAT-AGE');
INSERT INTO Parametros (CatalogoId, Codigo, Valor, ParametroIdPadre, CreatedBy) VALUES
(@CatAgeId, N'AGE-SOL', N'Agente de Consistencia de Solicitudes', NULL, N'SYSTEM'),
(@CatAgeId, N'AGE-PRF', N'Agente de Creación de Perfiles', NULL, N'SYSTEM'),
(@CatAgeId, N'AGE-SRC', N'Agente de Sourcing de Candidatos', NULL, N'SYSTEM'),
(@CatAgeId, N'AGE-MAT', N'Agente de Matching Curricular', NULL, N'SYSTEM'),
(@CatAgeId, N'AGE-SCO', N'Agente de Scoring Explicable', NULL, N'SYSTEM'),
(@CatAgeId, N'AGE-COR', N'Agente Coordinador de Flujos', NULL, N'SYSTEM'),
(@CatAgeId, N'AGE-ANL', N'Agente Analítico y Reportería', NULL, N'SYSTEM');
GO

-- 9. Agentes de IA
INSERT INTO Agentes (Nombre, Descripcion, CreatedBy)
VALUES 
(N'AgenteSolicitud', N'Valida consistencia y justificación de solicitudes de personal.', N'SYSTEM'),
(N'AgentePerfil', N'Genera y estructura profesiogramas basados en el cargo.', N'SYSTEM'),
(N'AgenteSourcing', N'Identifica y atrae candidatos en canales externos.', N'SYSTEM'),
(N'AgenteMatching', N'Compara el CV del postulante contra el profesiograma.', N'SYSTEM'),
(N'AgenteScoring', N'Calcula idoneidad del candidato y genera justificación.', N'SYSTEM'),
(N'AgenteCoordinador', N'Orquesta las citas y notificaciones del proceso.', N'SYSTEM'),
(N'AgenteAnalitico', N'Genera snapshots de KPI y reportes de cobertura.', N'SYSTEM');
GO

-- 10. Feriados (Nacionales de Bolivia - Recurrentes)
INSERT INTO Feriados (Fecha, Descripcion, EsRecurrente, CreatedBy) VALUES
('2026-01-01', N'Año Nuevo', 1, N'SYSTEM'),
('2026-01-22', N'Día del Estado Plurinacional', 1, N'SYSTEM'),
('2026-05-01', N'Día del Trabajo', 1, N'SYSTEM'),
('2026-06-21', N'Año Nuevo Andino Amazónico', 1, N'SYSTEM'),
('2026-08-06', N'Día de la Independencia de Bolivia', 1, N'SYSTEM'),
('2026-11-02', N'Día de Todos los Santos', 1, N'SYSTEM'),
('2026-12-25', N'Navidad', 1, N'SYSTEM');
GO

PRINT 'Datos semilla cargados con éxito.';
GO
