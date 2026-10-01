ALTER TABLE Usuarios ALTER COLUMN Password varchar(255) NULL;
GO
UPDATE Usuarios
SET Password = '$pbkdf2-sha512$210000$/DzdQalk8A3JsyEGgiB4EA==$Zaqe0tmMj/b4W4Zqnt2JbjYnJibmIncjaL7K9rOJO/CZNxu4r0pSQu14m/UbfKz/hxadwr/pnDbeNkGOkVhxvA=='
WHERE UserName = 'Pablo'
  AND (Password IS NULL OR Password NOT LIKE '$pbkdf2%');
GO
INSERT INTO Usuarios (UserName, Password, Email, IDRol, Estado)
SELECT 'admin', '$pbkdf2-sha512$210000$HPUBR/6XDHImnoYfPX9MNQ==$aKiVGL5iXuDfFPfU519IWiIeyuJkThP8PN5rYu8Ww3vIOP3iN255iELXeen0XGcfSMEGTr0w8hDk41TIsaP66Q==', 'admin@sistema.com', R.IDRol, 1
FROM Roles R
WHERE R.Nombre = 'SuperAdmin'
  AND NOT EXISTS (SELECT 1 FROM Usuarios U WHERE U.UserName = 'admin');
GO
SELECT IDUsuario, UserName, Password, Email, IDRol, Estado
FROM Usuarios
ORDER BY IDUsuario;
GO