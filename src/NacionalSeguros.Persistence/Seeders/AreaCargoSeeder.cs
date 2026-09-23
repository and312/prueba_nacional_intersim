using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NacionalSeguros.Domain.Entities;
using NacionalSeguros.Persistence.Context;

namespace NacionalSeguros.Persistence.Seeders;

public static class AreaCargoSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await EnsureTablesExistAsync(context);

        if (await context.AreasCargo.AnyAsync())
        {
            return; // Ya fue sembrado
        }

        var data = GetInitialData();

        foreach (var group in data)
        {
            string areaNombre = group.Key;
            string areaCodigo = "AREA_" + areaNombre.ToUpper().Replace(" ", "_").Replace("/", "_").Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U").Replace("Ñ", "N");
            if (areaCodigo.Length > 45) areaCodigo = areaCodigo.Substring(0, 45);

            var areaCargo = new AreaCargo(areaCodigo, areaNombre, $"Área de cargo: {areaNombre}");
            areaCargo.SetAuditoriaCreacion("SystemSeed");
            context.AreasCargo.Add(areaCargo);
            await context.SaveChangesAsync();

            int cargoIndex = 1;
            foreach (var cargoNombre in group.Value.Distinct())
            {
                string cargoCodigo = $"CARGO_{areaCargo.Id}_{cargoIndex++}";
                var cargo = new Cargo(areaCargo.Id, cargoCodigo, cargoNombre, $"Cargo: {cargoNombre}");
                cargo.SetAuditoriaCreacion("SystemSeed");
                context.Cargos.Add(cargo);
            }
            await context.SaveChangesAsync();
        }
    }

    private static Dictionary<string, List<string>> GetInitialData()
    {
        return new Dictionary<string, List<string>>
        {
            ["VPE"] = new List<string>
            {
                "CHOFER",
                "ASISTENTE DE PRESIDENCIA Y DIRECTORIO",
                "PRESIDENTE",
                "MENSAJERO",
                "ASESOR DE DIRECTORIO"
            },
            ["RRHH"] = new List<string>
            {
                "AUXILIAR DE SELECCION DE RRHH",
                "ANALISTA DE COMPENSACION TOTAL",
                "ANALISTA DE PLANIFICACIÓN DE RRHH",
                "SUB GERENTE CORPORATIVO DE RRHH",
                "ANALISTA DE TALENTO Y DESARROLLO PROFESIONAL",
                "RESPONSABLE DE RECURSOS HUMANOS",
                "AUXILIAR DE RRHH",
                "ANALISTA DE VALORACIÓN DE PERSONAS",
                "AUXILIAR DE CONTROL DE VACACIONES",
                "JEFE CORPORATIVO DE REMUNERACIONES",
                "ASISTENTE DE REMUNERACIONES"
            },
            ["FINANZAS"] = new List<string>
            {
                "AUXILIAR DE FINANZAS",
                "CONTROLLER",
                "ANALISTA DE FINANZAS",
                "GERENTE CORPORATIVO DE FINANZAS",
                "ANALISTA DE TESORERÍA",
                "AUXILIAR DE TESORERIA",
                "JEFE CORPORATIVO DE INVERSIONES",
                "JEFE CORPORATIVO DE TESORERÍA",
                "AUXILIAR DE TESORERÍA"
            },
            ["ADM/CONTABILIDAD"] = new List<string>
            {
                "SUB GERENTE CORP. DE ADMINISTRACION Y CONTABILIDAD",
                "ANALISTA CONTABLE",
                "CONTADOR"
            },
            ["PROYECTO CUSTODIA"] = new List<string>
            {
                "ASESOR LEGAL",
                "GESTOR ADMINISTRATIVO DE BIENES"
            },
            ["DESARROLLO DE NEGOCIOS"] = new List<string>
            {
                "TECNICO ESPECIALISTA EN DESARROLLO DE NEGOCIOS",
                "ASESOR DE MARKETING DIGITAL",
                "SUB GERENTE DE ASISTENCIA Y SERVICIOS",
                "EJECUTIVO DE MARKETING",
                "EJECUTIVO DE PRODUCTOS Y ASISTENCIAS",
                "JEFE CORPORATIVO DE PRODUCTOS Y ESTRATEGIA",
                "ANALISTA DE PRODUCT MARKETING",
                "TECNICO ESPECIALISTA EN MARKETING DE SEGUROS DE VIDA"
            },
            ["LEGAL"] = new List<string>
            {
                "ASESOR LEGAL SOCIETARIO",
                "JEFE NACIONAL LEGAL DE FIANZAS",
                "ASISTENTE LEGAL",
                "ASESOR LEGAL FIANZAS",
                "GERENTE LEGAL CORPORATIVO",
                "ASISTENTE LEGAL DE FIANZAS",
                "ASESOR JURÍDICO LEGAL"
            },
            ["ADQUISICIONES Y SERVICIOS"] = new List<string>
            {
                "AUXILIAR DE SERVICIOS GENERALES",
                "AUXILIAR DE ACTIVOS FIJOS",
                "SUPERVISOR DE MANTENIMIENTO",
                "SUB GERENTE CORPORATIVO DE SERVICIOS GENERALES"
            },
            ["PROYECTOS"] = new List<string>
            {
                "RESPONSABLE CORPORATIVO DE PROYECTOS"
            },
            ["CALIDAD"] = new List<string>
            {
                "ANALISTA DE CALIDAD",
                "SUB GERENTE CORPORATIVO DE PLANIFICACIÓN",
                "JEFE DE SISTEMAS DE GESTIÓN Y MEJORA CONTINUA",
                "AUXILIAR DE CALIDAD"
            },
            ["INTELIGENCIA DE NEGOCIOS"] = new List<string>
            {
                "ANALISTA DE DATOS",
                "ASISTENTE DE GESTIÓN DE PROYECTOS DE INNOVACIÓN",
                "SUB GERENTE CORPORATIVO DE INNOVACIÓN Y ESTRATEGIA"
            },
            ["FIANZAS Y CAUCIONES"] = new List<string>
            {
                "ASISTENTE DE EMISIÓN",
                "OFICIAL DE FIANZAS Y CAUCIONES",
                "GERENTE NACIONAL DE FIANZAS Y CAUCIONES",
                "SUB GERENTE NACIONAL COMERCIAL DE FIANZAS Y CAUCION"
            },
            ["ATENCIÓN AL CLIENTE"] = new List<string>
            {
                "JEFE REGIONAL DE ATENCIÓN AL CLIENTE",
                "EJECUTIVO DE ATENCIÓN AL CLIENTE",
                "EJECUTIVO DE UNIDAD TÉCNICA DE CONTROL",
                "ASISTENTE ADMINISTRATIVO DE ATENCIÓN AL CLIENTE",
                "SUPERVISOR REGIONAL DE ATENCION AL CLIENTE",
                "SUPERVISOR DE ATENCIÓN AL CLIENTE",
                "SUB GERENTE NACIONAL DE ATENCIÓN AL CLIENTE",
                "SUB GERENTE DE ATENCIÓN AL CLIENTE VIDA Y SALUD",
                "ANALISTA NACIONAL DE ATENCIÓN AL CLIENTE",
                "JEFE NACIONAL DE SINIESTROS VIDA"
            },
            ["COMERCIAL CORPORATIVO"] = new List<string>
            {
                "AUXILIAR DE LICITACIONES",
                "EJECUTIVO COMERCIAL CORPORATIVO",
                "SUB GERENTE REGIONAL COMERCIAL",
                "JEFE COMERCIAL CORPORATIVO",
                "ASISTENTE COMERCIAL",
                "EJECUTIVO DE LICITACIONES",
                "RESPONSABLE DE LICITACIONES",
                "SUPERVISOR COMERCIAL"
            },
            ["COBRANZAS"] = new List<string>
            {
                "AUXILIAR DE COBRANZAS",
                "COBRADOR",
                "SUB GERENTE NACIONAL DE COBRANZAS",
                "SUPERVISOR DE COBRANZAS",
                "EJECUTIVO DE UNIMAC",
                "AUXILIAR DE UNIMAC",
                "JEFE NACIONAL DE UNIMAC"
            },
            ["AUDITORÍA"] = new List<string>
            {
                "ASISTENTE DE AUDITORIA INTERNA",
                "ANALISTA DE AUDITORIA INTERNA",
                "AUDITOR INTERNO"
            },
            ["GERENCIA"] = new List<string>
            {
                "GERENTE REGIONAL",
                "SUB GERENTE REGIONAL",
                "GERENTE GENERAL",
                "GERENTE REGIONAL SUCURSAL SUCRE",
                "GERENTE NACIONAL DE OPERACIONES",
                "OFICIAL DE SEGURIDAD DE LA INFORMACIÓN",
                "DIRECTOR",
                "RESPONSABLE ADMINISTRATIVO"
            },
            ["FUERZA DE VENTAS"] = new List<string>
            {
                "ASISTENTE COMERCIAL",
                "JEFE COMERCIAL",
                "TECNICO ASESOR COMERCIAL EN SEGUROS",
                "RESPONSABLE COMERCIAL",
                "JEFE REGIONAL DE VENTAS",
                "CAJERO RECEPCIONISTA",
                "ANALISTA COMERCIAL",
                "SUPERVISOR DE VENTAS"
            },
            ["TÉCNICA RIESGO"] = new List<string>
            {
                "ANALISTA DE RIESGO",
                "JEFE NACIONAL DE RIESGO"
            },
            ["ADM/FINANCIERO"] = new List<string>
            {
                "CAJERO",
                "AUXILIAR CONTABLE",
                "ANALISTA ADMINISTRATIVO",
                "ASISTENTE ADMINISTRATIVO",
                "MENSAJERO",
                "SUPERVISOR CONTABLE",
                "ASISTENTE DE RECEPCIÓN Y ATENCIÓN AL CLIENTE",
                "ANALISTA CONTABLE",
                "JEFE ADMINISTRATIVO CONTABLE",
                "CONTADOR",
                "SUPERVISOR ADMINISTRATIVO",
                "GERENTE NACIONAL ADMINISTRATIVO FINANCIERO",
                "RESPONSABLE ADMINISTRATIVO",
                "JEFE NACIONAL ADMINISTRATIVO FINANCIERO"
            },
            ["TECNOLOGÍA"] = new List<string>
            {
                "DESARROLLADOR SENIOR",
                "ADMINISTRADOR DE PROYECTO DE SOFTWARE",
                "ANALISTA DE ASEGURAMIENTO DE CALIDAD",
                "JEFE DE INFRAESTRUCTURA",
                "JEFE DE DESARROLLO Y PROYECTOS DE SOFTWARE"
            },
            ["EXPERIENCIA AL CLIENTE"] = new List<string>
            {
                "JEFE DE EXPERIENCIA AL CLIENTE",
                "EJECUTIVO DE EXPERIENCIA AL CLIENTE",
                "GERENTE NACIONAL DE CLIENTES",
                "ANALISTA DE EXPERIENCIA AL CLIENTE",
                "RECEPCIONISTA",
                "GERENTE DE EXPERIENCIA AL CLIENTE",
                "EJECUTIVO DE PLATAFORMA ATENCIÓN AL CLIENTE"
            },
            ["TÉCNICA PRODUCCIÓN"] = new List<string>
            {
                "ASISTENTE DE EMISIÓN, CONTROL Y CALIDAD",
                "EJECUTIVO DE PRODUCCIÓN",
                "SUPERVISOR DE EMISIÓN, CONTROL Y CALIDAD"
            },
            ["TÉCNICA RECUPERO"] = new List<string>
            {
                "JEFE NACIONAL DE RECUPEROS",
                "ANALISTA DE RECUPERO"
            },
            ["TÉCNICA"] = new List<string>
            {
                "ANALISTA DE SUSCRIPCIÓN",
                "ANALISTA TÉCNICO",
                "ASESOR TÉCNICO",
                "ASISTENTE ACTUARIAL",
                "SUPERVISOR REGIONAL TÉCNICO",
                "ASISTENTE TÉCNICO",
                "GERENTE NACIONAL TÉCNICO",
                "ANALISTA DE RIESGO",
                "EJECUTIVO DE SUSCRIPCIÓN",
                "JEFE REGIONAL TÉCNICO",
                "ASISTENTE TECNICO"
            },
            ["UIF"] = new List<string>
            {
                "ENCARGADO UIF"
            },
            ["MONITOREO E INVESTIGACIÓN"] = new List<string>
            {
                "ANALISTA DE MONITOREO E INVESTIGACIÓN"
            },
            ["ARCHIVO"] = new List<string>
            {
                "AUXILIAR DE ARCHIVO",
                "SUPERVISOR DE ARCHIVOS"
            },
            ["TÉCNICA REASEGUROS"] = new List<string>
            {
                "ASISTENTE DE REASEGUROS",
                "ANALISTA DE REASEGUROS",
                "JEFE NACIONAL DE REASEGUROS Y SUSCRIPCION",
                "SUB GERENTE NACIONAL DE REASEGUROS Y SUSCRIPCION",
                "SUPERVISOR DE REASEGUROS"
            },
            ["TÉCNICA RIESGOS"] = new List<string>
            {
                "JEFE NACIONAL DE RIESGOS"
            },
            ["MASIVOS"] = new List<string>
            {
                "GERENTE NACIONAL DE BANCA SEGUROS Y MASIVOS",
                "JEFE NACIONAL DE TELEMERCADEO",
                "EJECUTIVO DE BANCA SEGUROS",
                "JEFE COMERCIAL DE SEGUROS MASIVOS",
                "SUPERVISOR DE BANCA SEGUROS Y COLECTIVOS",
                "ASISTENTE DE BANCA SEGUROS Y COLECTIVOS",
                "EJECUTIVO COMERCIAL DE SEGUROS MASIVOS",
                "SUB GERENTE NACIONAL DE BANCA SEGUROS"
            },
            ["FORMACIÓN DE AGENTES"] = new List<string>
            {
                "ASISTENTE DE FORMACIÓN Y CAPTACIÓN DE AGENTES"
            },
            ["TÉCNICA MÉDICOS"] = new List<string>
            {
                "MÉDICO AUDITOR",
                "DIRECTOR MÉDICO"
            },
            ["TÉCNICA REDES MÉDICAS"] = new List<string>
            {
                "ANALISTA DE REDES MEDICAS",
                "SUB GERENTE NACIONAL DE REDES MÉDICAS"
            },
            ["ASISTENCIA EJECUTIVA"] = new List<string>
            {
                "ASISTENTE DE GERENCIA"
            }
        };
    }

    private static async Task EnsureTablesExistAsync(ApplicationDbContext context)
    {
        string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AreasCargo')
BEGIN
    CREATE TABLE dbo.AreasCargo (
        AreaCargoId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Codigo NVARCHAR(50) NOT NULL,
        Nombre NVARCHAR(150) NOT NULL,
        Descripcion NVARCHAR(250) NULL,
        Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_AreasCargo_Estado DEFAULT ('Activo'),
        CreatedBy NVARCHAR(100) NOT NULL CONSTRAINT DF_AreasCargo_CreatedBy DEFAULT ('SYSTEM'),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_AreasCargo_CreatedDate DEFAULT (GETUTCDATE()),
        ModifiedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,
        DeletedDate DATETIME2 NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_AreasCargo_IsDeleted DEFAULT (0),
        CONSTRAINT UQ_AreasCargo_Codigo UNIQUE (Codigo)
    );
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cargos')
BEGIN
    CREATE TABLE dbo.Cargos (
        CargoId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AreaCargoId INT NOT NULL,
        Codigo NVARCHAR(50) NOT NULL,
        Nombre NVARCHAR(200) NOT NULL,
        Descripcion NVARCHAR(250) NULL,
        Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Cargos_Estado DEFAULT ('Activo'),
        CreatedBy NVARCHAR(100) NOT NULL CONSTRAINT DF_Cargos_CreatedBy DEFAULT ('SYSTEM'),
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Cargos_CreatedDate DEFAULT (GETUTCDATE()),
        ModifiedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,
        DeletedDate DATETIME2 NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Cargos_IsDeleted DEFAULT (0),
        CONSTRAINT FK_Cargos_AreasCargo_AreaCargoId FOREIGN KEY (AreaCargoId) REFERENCES dbo.AreasCargo(AreaCargoId),
        CONSTRAINT UQ_Cargos_Codigo UNIQUE (Codigo)
    );
END;
";
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}

