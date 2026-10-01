using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CD_Usuario
    {
        private readonly CD_Conexion _conexion;

        public CD_Usuario()
        {
            _conexion = new CD_Conexion();
        }

        public CD_Usuario(CD_Conexion conexion)
        {
            _conexion = conexion;
        }

        public UsuarioDTO LoginUsuario(string userName)
        {
            const string CONSULTA = @"SELECT
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
WHERE U.UserName = @UserName";

            using (SqlConnection conexion = _conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(CONSULTA, conexion))
            {
                comando.Parameters.Add("@UserName", SqlDbType.VarChar, 50).Value = userName;

                try
                {
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (!lector.Read())
                            return null;

                        return new UsuarioDTO
                        {
                            IDUsuario = Convert.ToInt32(lector["IDUsuario"]),
                            UserName = Convert.ToString(lector["UserName"]),
                            Password = Convert.ToString(lector["Password"]),
                            Email = Convert.ToString(lector["Email"]),
                            IDRol = Convert.ToInt32(lector["IDRol"]),
                            Estado = lector["Estado"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(lector["Estado"]),
                            NombreRol = Convert.ToString(lector["NombreRol"])
                        };
                    }
                }
                catch (SqlException excepcion)
                {
                    throw new CDExcepcion("No se pudo consultar la información de los usuarios.", excepcion);
                }
            }
        }

        public bool ActualizarPassword(int idUsuario, string passwordHash)
        {
            const string CONSULTA = @"UPDATE Usuarios
SET Password = @Password
WHERE IDUsuario = @IDUsuario";

            using (SqlConnection conexion = _conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(CONSULTA, conexion))
            {
                comando.Parameters.Add("@Password", SqlDbType.VarChar, 255).Value = passwordHash;
                comando.Parameters.Add("@IDUsuario", SqlDbType.Int).Value = idUsuario;

                try
                {
                    return comando.ExecuteNonQuery() > 0;
                }
                catch (SqlException excepcion)
                {
                    throw new CDExcepcion("No se pudo actualizar la contraseña del usuario.", excepcion);
                }
            }
        }
    }
}