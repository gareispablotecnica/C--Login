SELECT t.name AS Tabla, c.name AS Columna, ty.name AS TipoDato, c.max_length AS Longitud, c.is_nullable AS PermiteNulos
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN ('Usuarios', 'Roles')
ORDER BY t.name, c.column_id;
GO
SELECT fk.name AS NombreFK,
       OBJECT_NAME(fk.parent_object_id) AS TablaPadre,
       COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnaPadre,
       OBJECT_NAME(fk.referenced_object_id) AS TablaHija,
       COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ColumnaHija
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
ORDER BY fk.name;
GO
SELECT IDRol, Nombre
FROM Roles
ORDER BY IDRol;
GO
SELECT U.IDUsuario,
       U.UserName,
       U.Email,
       U.IDRol,
       R.Nombre AS NombreRol,
       U.Estado,
       CASE WHEN U.Password LIKE '$pbkdf2%' THEN 'HASH' ELSE 'TEXTO_PLANO' END AS TipoPassword
FROM Usuarios U
INNER JOIN Roles R ON U.IDRol = R.IDRol
ORDER BY U.IDUsuario;
GO