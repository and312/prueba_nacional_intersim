USE SIR_NacionalSeguros;
GO

-- 1. Fix Roles
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã­', 'í') WHERE Descripcion LIKE '%Ã­%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã³', 'ó') WHERE Descripcion LIKE '%Ã³%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã©', 'é') WHERE Descripcion LIKE '%Ã©%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã¡', 'á') WHERE Descripcion LIKE '%Ã¡%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ãº', 'ú') WHERE Descripcion LIKE '%Ãº%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã±', 'ñ') WHERE Descripcion LIKE '%Ã±%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã', 'Í') WHERE Descripcion LIKE '%Ã%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã', 'Ó') WHERE Descripcion LIKE '%Ã%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã', 'É') WHERE Descripcion LIKE '%Ã%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã', 'Á') WHERE Descripcion LIKE '%Ã%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã', 'Ú') WHERE Descripcion LIKE '%Ã%';
UPDATE Roles SET Descripcion = REPLACE(Descripcion, 'Ã', 'Ñ') WHERE Descripcion LIKE '%Ã%';

UPDATE Roles SET Nombre = REPLACE(Nombre, 'Ã­', 'í') WHERE Nombre LIKE '%Ã­%';
UPDATE Roles SET Nombre = REPLACE(Nombre, 'Ã³', 'ó') WHERE Nombre LIKE '%Ã³%';
UPDATE Roles SET Nombre = REPLACE(Nombre, 'Ã©', 'é') WHERE Nombre LIKE '%Ã©%';
UPDATE Roles SET Nombre = REPLACE(Nombre, 'Ã¡', 'á') WHERE Nombre LIKE '%Ã¡%';

-- 2. Fix Estados
UPDATE Estados SET Nombre = REPLACE(Nombre, 'Ã­', 'í') WHERE Nombre LIKE '%Ã­%';
UPDATE Estados SET Nombre = REPLACE(Nombre, 'Ã³', 'ó') WHERE Nombre LIKE '%Ã³%';
UPDATE Estados SET Nombre = REPLACE(Nombre, 'Ã©', 'é') WHERE Nombre LIKE '%Ã©%';
UPDATE Estados SET Nombre = REPLACE(Nombre, 'Ã¡', 'á') WHERE Nombre LIKE '%Ã¡%';
UPDATE Estados SET Nombre = REPLACE(Nombre, 'Ãº', 'ú') WHERE Nombre LIKE '%Ãº%';
UPDATE Estados SET Nombre = REPLACE(Nombre, 'Ã±', 'ñ') WHERE Nombre LIKE '%Ã±%';

-- 3. Fix Areas
UPDATE Areas SET Nombre = REPLACE(Nombre, 'Ã­', 'í') WHERE Nombre LIKE '%Ã­%';
UPDATE Areas SET Nombre = REPLACE(Nombre, 'Ã³', 'ó') WHERE Nombre LIKE '%Ã³%';
UPDATE Areas SET Nombre = REPLACE(Nombre, 'Ã©', 'é') WHERE Nombre LIKE '%Ã©%';
UPDATE Areas SET Nombre = REPLACE(Nombre, 'Ã¡', 'á') WHERE Nombre LIKE '%Ã¡%';
UPDATE Areas SET Nombre = REPLACE(Nombre, 'Ãº', 'ú') WHERE Nombre LIKE '%Ãº%';
UPDATE Areas SET Nombre = REPLACE(Nombre, 'Ã±', 'ñ') WHERE Nombre LIKE '%Ã±%';

UPDATE Areas SET Gerencia = REPLACE(Gerencia, 'Ã­', 'í') WHERE Gerencia LIKE '%Ã­%';
UPDATE Areas SET Gerencia = REPLACE(Gerencia, 'Ã³', 'ó') WHERE Gerencia LIKE '%Ã³%';
UPDATE Areas SET Gerencia = REPLACE(Gerencia, 'Ã©', 'é') WHERE Gerencia LIKE '%Ã©%';
UPDATE Areas SET Gerencia = REPLACE(Gerencia, 'Ã¡', 'á') WHERE Gerencia LIKE '%Ã¡%';
UPDATE Areas SET Gerencia = REPLACE(Gerencia, 'Ãº', 'ú') WHERE Gerencia LIKE '%Ãº%';
UPDATE Areas SET Gerencia = REPLACE(Gerencia, 'Ã±', 'ñ') WHERE Gerencia LIKE '%Ã±%';

-- 4. Fix Parametros
UPDATE Parametros SET Valor = REPLACE(Valor, 'Ã­', 'í') WHERE Valor LIKE '%Ã­%';
UPDATE Parametros SET Valor = REPLACE(Valor, 'Ã³', 'ó') WHERE Valor LIKE '%Ã³%';
UPDATE Parametros SET Valor = REPLACE(Valor, 'Ã©', 'é') WHERE Valor LIKE '%Ã©%';
UPDATE Parametros SET Valor = REPLACE(Valor, 'Ã¡', 'á') WHERE Valor LIKE '%Ã¡%';
UPDATE Parametros SET Valor = REPLACE(Valor, 'Ãº', 'ú') WHERE Valor LIKE '%Ãº%';
UPDATE Parametros SET Valor = REPLACE(Valor, 'Ã±', 'ñ') WHERE Valor LIKE '%Ã±%';

-- 5. Fix Feriados
UPDATE Feriados SET Descripcion = REPLACE(Descripcion, 'Ã­', 'í') WHERE Descripcion LIKE '%Ã­%';
UPDATE Feriados SET Descripcion = REPLACE(Descripcion, 'Ã³', 'ó') WHERE Descripcion LIKE '%Ã³%';
UPDATE Feriados SET Descripcion = REPLACE(Descripcion, 'Ã©', 'é') WHERE Descripcion LIKE '%Ã©%';
UPDATE Feriados SET Descripcion = REPLACE(Descripcion, 'Ã¡', 'á') WHERE Descripcion LIKE '%Ã¡%';
UPDATE Feriados SET Descripcion = REPLACE(Descripcion, 'Ãº', 'ú') WHERE Descripcion LIKE '%Ãº%';
UPDATE Feriados SET Descripcion = REPLACE(Descripcion, 'Ã±', 'ñ') WHERE Descripcion LIKE '%Ã±%';

-- 6. Fix Agentes
UPDATE Agentes SET Descripcion = REPLACE(Descripcion, 'Ã­', 'í') WHERE Descripcion LIKE '%Ã­%';
UPDATE Agentes SET Descripcion = REPLACE(Descripcion, 'Ã³', 'ó') WHERE Descripcion LIKE '%Ã³%';
UPDATE Agentes SET Descripcion = REPLACE(Descripcion, 'Ã©', 'é') WHERE Descripcion LIKE '%Ã©%';
UPDATE Agentes SET Descripcion = REPLACE(Descripcion, 'Ã¡', 'á') WHERE Descripcion LIKE '%Ã¡%';
UPDATE Agentes SET Descripcion = REPLACE(Descripcion, 'Ãº', 'ú') WHERE Descripcion LIKE '%Ãº%';
UPDATE Agentes SET Descripcion = REPLACE(Descripcion, 'Ã±', 'ñ') WHERE Descripcion LIKE '%Ã±%';

-- 7. Fix Usuarios
UPDATE Usuarios SET Gerencia = REPLACE(Gerencia, 'Ã­', 'í') WHERE Gerencia LIKE '%Ã­%';
UPDATE Usuarios SET Gerencia = REPLACE(Gerencia, 'Ã³', 'ó') WHERE Gerencia LIKE '%Ã³%';
UPDATE Usuarios SET Gerencia = REPLACE(Gerencia, 'Ã©', 'é') WHERE Gerencia LIKE '%Ã©%';
UPDATE Usuarios SET Gerencia = REPLACE(Gerencia, 'Ã¡', 'á') WHERE Gerencia LIKE '%Ã¡%';
UPDATE Usuarios SET Gerencia = REPLACE(Gerencia, 'Ãº', 'ú') WHERE Gerencia LIKE '%Ãº%';
UPDATE Usuarios SET Gerencia = REPLACE(Gerencia, 'Ã±', 'ñ') WHERE Gerencia LIKE '%Ã±%';

UPDATE Usuarios SET Cargo = REPLACE(Cargo, 'Ã­', 'í') WHERE Cargo LIKE '%Ã­%';
UPDATE Usuarios SET Cargo = REPLACE(Cargo, 'Ã³', 'ó') WHERE Cargo LIKE '%Ã³%';
UPDATE Usuarios SET Cargo = REPLACE(Cargo, 'Ã©', 'é') WHERE Cargo LIKE '%Ã©%';
UPDATE Usuarios SET Cargo = REPLACE(Cargo, 'Ã¡', 'á') WHERE Cargo LIKE '%Ã¡%';
UPDATE Usuarios SET Cargo = REPLACE(Cargo, 'Ãº', 'ú') WHERE Cargo LIKE '%Ãº%';
UPDATE Usuarios SET Cargo = REPLACE(Cargo, 'Ã±', 'ñ') WHERE Cargo LIKE '%Ã±%';

UPDATE Usuarios SET Nombres = REPLACE(Nombres, 'Ã­', 'í') WHERE Nombres LIKE '%Ã­%';
UPDATE Usuarios SET Nombres = REPLACE(Nombres, 'Ã³', 'ó') WHERE Nombres LIKE '%Ã³%';
UPDATE Usuarios SET Nombres = REPLACE(Nombres, 'Ã©', 'é') WHERE Nombres LIKE '%Ã©%';
UPDATE Usuarios SET Nombres = REPLACE(Nombres, 'Ã¡', 'á') WHERE Nombres LIKE '%Ã¡%';
UPDATE Usuarios SET Nombres = REPLACE(Nombres, 'Ãº', 'ú') WHERE Nombres LIKE '%Ãº%';
UPDATE Usuarios SET Nombres = REPLACE(Nombres, 'Ã±', 'ñ') WHERE Nombres LIKE '%Ã±%';

UPDATE Usuarios SET Apellidos = REPLACE(Apellidos, 'Ã­', 'í') WHERE Apellidos LIKE '%Ã­%';
UPDATE Usuarios SET Apellidos = REPLACE(Apellidos, 'Ã³', 'ó') WHERE Apellidos LIKE '%Ã³%';
UPDATE Usuarios SET Apellidos = REPLACE(Apellidos, 'Ã©', 'é') WHERE Apellidos LIKE '%Ã©%';
UPDATE Usuarios SET Apellidos = REPLACE(Apellidos, 'Ã¡', 'á') WHERE Apellidos LIKE '%Ã¡%';
UPDATE Usuarios SET Apellidos = REPLACE(Apellidos, 'Ãº', 'ú') WHERE Apellidos LIKE '%Ãº%';
UPDATE Usuarios SET Apellidos = REPLACE(Apellidos, 'Ã±', 'ñ') WHERE Apellidos LIKE '%Ã±%';

GO
PRINT 'Limpieza de codificación realizada exitosamente.';
