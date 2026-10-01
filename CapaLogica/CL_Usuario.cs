using System;
using CapaDatos;

namespace CapaLogica
{
    public class CL_Usuario
    {
        public const string MENSAJE_SIN_USUARIO = "Debe ingresar el usuario.";
        public const string MENSAJE_SIN_PASSWORD = "Debe ingresar la contraseña.";
        public const string MENSAJE_CREDENCIALES = "Usuario o contraseña incorrectos.";
        public const string MENSAJE_INACTIVO = "El usuario se encuentra inactivo.";
        public const string MENSAJE_SIN_CONEXION = "No se pudo establecer conexión con la base de datos.";

        private const string HASH_COMPARACION_UNIFORME = "$pbkdf2-sha512$210000$m/MWc7dTS0fOyGaNhCLo5g==$3Jj98gmOK40ptA1e08D5Cq/C47wamlemzCZ+Ja/BH3juXhkdV4EnJCG4HlATwaxeZXJ5aikLG3Nr0tNHbY/3wg==";

        private readonly CD_Usuario _cdUsuario;

        public CL_Usuario()
        {
            _cdUsuario = new CD_Usuario();
        }

        public CL_Usuario(CD_Usuario cdUsuario)
        {
            _cdUsuario = cdUsuario;
        }

        public ResultadoAutenticacion LoginUsuario(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || userName.Trim().Length == 0)
                return ResultadoAutenticacion.Fallido(MENSAJE_SIN_USUARIO);

            if (string.IsNullOrEmpty(password))
                return ResultadoAutenticacion.Fallido(MENSAJE_SIN_PASSWORD);

            UsuarioDTO datos;

            try
            {
                datos = _cdUsuario.LoginUsuario(userName.Trim());
            }
            catch (CDExcepcion)
            {
                return ResultadoAutenticacion.Fallido(MENSAJE_SIN_CONEXION);
            }

            if (datos == null)
            {
                PasswordHasher.VerifyPassword(password, HASH_COMPARACION_UNIFORME);
                return ResultadoAutenticacion.Fallido(MENSAJE_CREDENCIALES);
            }

            if (datos.Estado != true)
                return ResultadoAutenticacion.Fallido(MENSAJE_INACTIVO);

            bool requiereMigracion = !PasswordHasher.EsHash(datos.Password);

            if (!PasswordHasher.VerifyPassword(password, datos.Password))
                return ResultadoAutenticacion.Fallido(MENSAJE_CREDENCIALES);

            Usuario usuario = new Usuario();
            usuario.IDUsuario = datos.IDUsuario;
            usuario.UserName = datos.UserName;
            usuario.Password = datos.Password;
            usuario.Email = datos.Email;
            usuario.IDRol = datos.IDRol;
            usuario.NombreRol = datos.NombreRol;
            usuario.Estado = datos.Estado == true;

            if (requiereMigracion)
                MigrarPasswordHash(usuario.IDUsuario, password);

            return ResultadoAutenticacion.Exitoso(usuario);
        }

        private void MigrarPasswordHash(int idUsuario, string password)
        {
            try
            {
                _cdUsuario.ActualizarPassword(idUsuario, PasswordHasher.HashPassword(password));
            }
            catch (CDExcepcion)
            {
            }
        }
    }
}