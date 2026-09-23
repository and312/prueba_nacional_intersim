-- ==========================================
-- 06_FUNCTIONS/crear_funciones.sql
-- Creación de Funciones de Negocio y Utilidades (Optimizado Set-Based)
-- Proyecto: Sistema Inteligente de Reclutamiento (SIR) - Nacional Seguros
-- Motor: SQL Server 2022 Standard Edition
-- ==========================================

USE SIR_NacionalSeguros;
GO

-- ==========================================
-- 1. fn_ObtenerDiasHabiles
-- Calcula la cantidad de días hábiles entre dos fechas, 
-- excluyendo fines de semana y feriados (fijos o recurrentes) de forma set-based.
-- ==========================================
CREATE OR ALTER FUNCTION dbo.fn_ObtenerDiasHabiles(
    @FechaInicio DATE, 
    @FechaFin DATE
)
RETURNS INT
AS
BEGIN
    IF @FechaInicio IS NULL OR @FechaFin IS NULL OR @FechaInicio > @FechaFin
        RETURN 0;

    DECLARE @DiasHabiles INT;

    -- Generación de secuencia de fechas orientada a conjuntos (Set-Based)
    -- usando cross joins en sys.all_objects
    WITH Numbers AS (
        SELECT TOP (DATEDIFF(day, @FechaInicio, @FechaFin) + 1)
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS N
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    , Dates AS (
        SELECT DATEADD(day, N, @FechaInicio) AS ActualDate
        FROM Numbers
    )
    SELECT @DiasHabiles = COUNT(1)
    FROM Dates
    WHERE (DATEPART(dw, ActualDate) + @@DATEFIRST - 1) % 7 NOT IN (0, 6)
      AND NOT EXISTS (
          SELECT 1 FROM Feriados f
          WHERE f.IsDeleted = 0 AND (
              f.Fecha = ActualDate
              OR (f.EsRecurrente = 1 
                  AND DATEPART(month, f.Fecha) = DATEPART(month, ActualDate) 
                  AND DATEPART(day, f.Fecha) = DATEPART(day, ActualDate))
          )
      );

    RETURN ISNULL(@DiasHabiles, 0);
END;
GO

-- ==========================================
-- 2. fn_CalcularFechaLimiteSLA
-- Calcula la fecha de vencimiento sumando días hábiles a una fecha de inicio.
-- Excluye fines de semana y feriados utilizando cálculo orientado a conjuntos.
-- ==========================================
CREATE OR ALTER FUNCTION dbo.fn_CalcularFechaLimiteSLA(
    @FechaInicio DATETIME2, 
    @DiasMaximos INT
)
RETURNS DATETIME2
AS
BEGIN
    IF @FechaInicio IS NULL OR @DiasMaximos <= 0
        RETURN @FechaInicio;

    DECLARE @FechaLimite DATETIME2;

    -- Generar suficiente rango de días calendario consecutivos (estimado de 5 veces el límite)
    WITH Numbers AS (
        SELECT TOP (ISNULL(@DiasMaximos, 0) * 5 + 10)
            ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    , Dates AS (
        SELECT DATEADD(day, N, CAST(@FechaInicio AS DATE)) AS ActualDate
        FROM Numbers
    )
    , BusinessDates AS (
        SELECT ActualDate,
               ROW_NUMBER() OVER (ORDER BY ActualDate) AS BusinessDaySeq
        FROM Dates
        WHERE (DATEPART(dw, ActualDate) + @@DATEFIRST - 1) % 7 NOT IN (0, 6)
          AND NOT EXISTS (
              SELECT 1 FROM Feriados f
              WHERE f.IsDeleted = 0 AND (
                  f.Fecha = ActualDate
                  OR (f.EsRecurrente = 1 
                      AND DATEPART(month, f.Fecha) = DATEPART(month, ActualDate) 
                      AND DATEPART(day, f.Fecha) = DATEPART(day, ActualDate))
              )
          )
    )
    SELECT @FechaLimite = MIN(ActualDate)
    FROM BusinessDates
    WHERE BusinessDaySeq = @DiasMaximos;

    -- Si se calculó una fecha, reinyectar el componente de hora (Time) original con precisión exacta
    IF @FechaLimite IS NOT NULL
    BEGIN
        SET @FechaLimite = DATEADD(nanosecond, DATEPART(nanosecond, @FechaInicio), 
                           DATEADD(second, DATEPART(second, @FechaInicio), 
                           DATEADD(minute, DATEPART(minute, @FechaInicio), 
                           DATEADD(hour, DATEPART(hour, @FechaInicio), CAST(@FechaLimite AS DATETIME2(7))))));
    END
    ELSE
    BEGIN
        SET @FechaLimite = @FechaInicio;
    END

    RETURN @FechaLimite;
END;
GO

-- ==========================================
-- 3. fn_ObtenerCostoIAEnPeriodo
-- Obtiene el costo total en USD acumulado por inferencias de IA en un rango.
-- ==========================================
CREATE OR ALTER FUNCTION dbo.fn_ObtenerCostoIAEnPeriodo(
    @FechaInicio DATETIME2, 
    @FechaFin DATETIME2
)
RETURNS DECIMAL(10,5)
AS
BEGIN
    DECLARE @Costo DECIMAL(10,5);
    SELECT @Costo = ISNULL(SUM(CostoEstimado), 0.0)
    FROM AgentExecutions
    WHERE FechaInicio >= @FechaInicio AND FechaFin <= @FechaFin;
    RETURN @Costo;
END;
GO
