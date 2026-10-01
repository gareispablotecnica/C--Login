# Login-Csharp

Sistema de autenticación de usuarios en **C# Windows Forms** con **SQL Server**, **ADO.NET** y **arquitectura por capas**.
Permite iniciar sesión, validar credenciales contra hashes seguros, obtener el **rol** del usuario y abrir el formulario principal.

---

## Arquitectura

```
FrmLogin / FrmPrincipal  (CapaPresentacion)
            ↓
        CapaLogica        (CL_Usuario, Usuario, PasswordHasher)
            ↓
        CapaDatos         (CD_Usuario, CD_Conexion)
            ↓
         SQL Server        (DB_Sistema → Usuarios + Roles)
```

- La capa de presentación **no** contiene `SqlConnection`, `SqlCommand`, `SqlDataReader` ni consultas SQL.
- Toda consulta SQL vive en `CapaDatos`.
- `CapaLogica` no depende de ADO.NET: se comunica con la base a través de las clases de `CapaDatos`.

---

## Tecnologías

| Elemento | Detalle |
|---|---|
| Lenguaje | C# |
| Interfaz | Windows Forms |
| Framework | .NET Framework 4.7.2 |
| Base de datos | SQL Server Express (`DB_Sistema`) |
| Acceso a datos | ADO.NET (`System.Data.SqlClient`) |
| Seguridad | PBKDF2-HMAC-SHA512, salt aleatorio, comparación en tiempo constante |

---

## Estructura de la solución

```
Login-Csharp/
├── Sistemas.slnx
├── CapaPresentacion/
│   ├── FrmLogin.cs / FrmLogin.Designer.cs / FrmLogin.resx
│   ├── FrmPrincipal.cs / FrmPrincipal.Designer.cs
│   ├── Program.cs
│   └── App.config                     (cadena de conexión y appSettings)
├── CapaLogica/
│   ├── CL_Usuario.cs                  (lógica de autenticación)
│   ├── Usuario.cs                     (modelo del usuario autenticado)
│   ├── PasswordHasher.cs              (hash y verificación de contraseñas)
│   └── ResultadoAutenticacion.cs      (resultado controlado del login)
├── CapaDatos/
│   ├── CD_Conexion.cs                 (administración de la conexión)
│   ├── CD_Usuario.cs                  (consultas sobre Usuarios y Roles)
│   ├── UsuarioDTO.cs                  (transferencia de datos)
│   └── CDExcepcion.cs                 (excepción propia de la capa de datos)
└── Scripts/
    ├── MigrarPasswords.sql            (migración de contraseñas y usuario admin)
    └── VerificarBaseDatos.sql         (verificación de estructura y datos)
```

Referencias configuradas:

```
CapaPresentacion → CapaLogica → CapaDatos → SQL Server
```

---

## Base de datos

Se utiliza **exclusivamente** la base existente `DB_Sistema`. No se creó ninguna base, tabla ni rol adicional.

### Tablas

**Usuarios**

| Columna | Tipo | Nulos | Descripción |
|---|---|---|---|
| IDUsuario | int | No | Clave primaria |
| UserName | varchar(50) | Sí | Nombre de acceso |
| Password | varchar(255) | Sí | Hash de la contraseña |
| Email | varchar(50) | Sí | Correo del usuario |
| IDRol | int | Sí | Clave foránea hacia `Roles.IDRol` |
| Estado | bit | Sí | 1 = activo, 0 = inactivo |

**Roles**

| Columna | Tipo | Nulos | Descripción |
|---|---|---|---|
| IDRol | int | No | Clave primaria |
| Nombre | varchar(20) | No | Nombre del rol |

Relación: `Usuarios.IDRol → Roles.IDRol` (`FK_Usuarios_Roles`).

Roles existentes: **Admin**, **Cliente**, **SuperAdmin**.

> Único cambio de estructura realizado: `Usuarios.Password` pasó de `varchar(50)` a `varchar(255)`,
> necesario para poder almacenar el hash de 135 caracteres. No se modificó ningún otro campo,
> ni se crean tablas nuevas.

### Consulta de login

```sql
SELECT
    U.IDUsuario,
    U.UserName,
    U.Password,
    U.Email,
    U.IDRol,
    U.Estado,
    R.Nombre AS NombreRol
FROM Usuarios U
INNER JOIN Roles R
    ON U.IDRol = R.IDRol
WHERE U.UserName = @UserName
```

El valor ingresado por el usuario viaja siempre como **parámetro `@UserName`**, nunca concatenado en la consulta.

---

## Configuración de la conexión

La cadena de conexión se modifica en `CapaPresentacion/App.config` (se copia al `.exe.config` al compilar):

```xml
<connectionStrings>
    <add name="DB_Sistema"
         connectionString="Server=.\SQLEXPRESS;Database=DB_Sistema;Trusted_Connection=True;MultipleActiveResultSets=True;Connect Timeout=15"
         providerName="System.Data.SqlClient" />
</connectionStrings>
<appSettings>
    <add key="Servidor" value=".\SQLEXPRESS" />
    <add key="BaseDatos" value="DB_Sistema" />
    <add key="Usuario" value="" />
    <add key="Clave" value="" />
</appSettings>
```

`CD_Conexion` funciona en dos modos:

1. Si existe la entrada `DB_Sistema` en `connectionStrings`, se utiliza esa cadena.
2. Si no existe, se arma la cadena con `SqlConnectionStringBuilder` usando los `appSettings`
   (Autenticación de Windows cuando `Usuario` y `Clave` están vacíos, o SQL Server si se informan).

También se puede pasar una cadena explícita: `new CD_Conexion(cadenaConexion)`.

> En `System.Data.SqlClient` de .NET Framework la clave correcta es **`Connect Timeout`** (con espacio).
> `ConnectTimeout` sin espacio provoca un error de palabra clave no admitida.

---

## Seguridad de contraseñas

- Algoritmo: **PBKDF2 con HMAC-SHA512**.
- Salt aleatorio de 16 bytes por usuario.
- 210.000 iteraciones.
- Hash de 64 bytes.
- Formato almacenado: `$pbkdf2-sha512$210000$<saltBase64>$<hashBase64>`.
- Comparación en **tiempo constante** para evitar ataques de temporización.
- Cuando el usuario no existe se ejecuta una verificación con un hash uniforme, de modo que el
  tiempo de respuesta no revela si el usuario está registrado.
- Las contraseñas **nunca** se guardan en texto plano ni se cifran de forma reversible.

### Migración de contraseñas existentes

Los usuarios que tenían la contraseña en texto plano se migran de forma controlada, sin eliminar usuarios:

1. `Scripts/MigrarPasswords.sql` convierte a hash las contraseñas existentes (es idempotente: solo
   actualiza las que todavía no son hash) y crea el usuario `admin` con el rol `SuperAdmin`.
2. Además, el login aplica una **migración transparente**: si el valor almacenado no tiene formato de
   hash, `PasswordHasher.VerifyPassword` lo valida como contraseña heredada y, si el inicio de sesión
   es exitoso, `CL_Usuario` la reemplaza automáticamente por el hash PBKDF2.

---

## Flujo de login

```
Usuario + Contraseña
        ↓
FrmLogin (validaciones de interfaz)
        ↓
CL_Usuario.LoginUsuario(userName, password)
        ↓
CD_Usuario.LoginUsuario(userName)   → SQL Server → DB_Sistema → Usuarios + Roles
        ↓
CL_Usuario: verifica estado activo y PasswordHasher.VerifyPassword
        ↓
ResultadoAutenticacion (Exito, Mensaje, Usuario)
        ↓
FrmPrincipal con los datos del usuario autenticado
```

### Mensajes al usuario

| Situación | Mensaje |
|---|---|
| Usuario vacío | `Debe ingresar el usuario.` |
| Contraseña vacía | `Debe ingresar la contraseña.` |
| Credenciales inválidas o usuario inexistente | `Usuario o contraseña incorrectos.` |
| Usuario sin estado activo | `El usuario se encuentra inactivo.` |
| Servidor o base inaccesible | `No se pudo establecer conexión con la base de datos.` |
| Error inesperado | `Ocurrió un error inesperado. Intente nuevamente.` |

Nunca se muestran al usuario la cadena de conexión, el SQL, el stack trace ni nombres internos de clases.

---

## Usuario autenticado

Al iniciar sesión correctamente se conserva el usuario en `FrmPrincipal.UsuarioActual`:

```csharp
FrmPrincipal principal = new FrmPrincipal(this, resultado.Usuario);

principal.UsuarioActual.IDUsuario;
principal.UsuarioActual.UserName;
principal.UsuarioActual.Email;
principal.UsuarioActual.IDRol;
principal.UsuarioActual.NombreRol;
principal.UsuarioActual.Estado;
```

Estos datos dejan el sistema preparado para implementar perfiles y permisos por rol en el futuro.

---

## Compilación y ejecución

Requisitos: Visual Studio (o Build Tools) con el targeting pack de **.NET Framework 4.7.2**
y una instancia de **SQL Server Express** con la base `DB_Sistema`.

Desde Visual Studio: abrir `Sistemas.slnx`, compilar y ejecutar el proyecto `CapaPresentacion`.

Desde consola:

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" Sistemas.slnx -t:Build -restore -p:Configuration=Debug
```

El ejecutable queda en `CapaPresentacion\bin\Debug\CapaPresentacion.exe`
junto a `CapaLogica.dll` y `CapaDatos.dll`.

---

## Usuarios de prueba

| Usuario | Contraseña | Rol | Estado |
|---|---|---|---|
| `admin` | `123456` | SuperAdmin | Activo |
| `Pablo` | `1234` | Admin | Activo |

Para verificar la base de datos:

```powershell
sqlcmd -S ".\SQLEXPRESS" -E -C -d "DB_Sistema" -i "Scripts\VerificarBaseDatos.sql"
```

---

## Verificaciones realizadas

- Compilación de la solución completa en `Debug` y `Release` sin errores.
- Inicio de sesión correcto de `admin` y `Pablo`, con obtención de rol y estado.
- Contraseña incorrecta, usuario inexistente, usuario inactivo y campos vacíos.
- Servidor inaccesible (mensaje amigable, sin datos sensibles).
- Conexión usando `connectionStrings` y usando solo `appSettings`.
- Migración automática de contraseña en texto plano tras un login exitoso.
- Prueba de la aplicación real: apertura de `FrmPrincipal`, cierre de sesión y mensaje de credenciales inválidas.