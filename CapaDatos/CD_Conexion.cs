using System;
using System.Configuration;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CD_Conexion
    {
        public const string NOMBRE_CONEXION = "DB_Sistema";
        public const string SERVIDOR_POR_DEFECTO = @".\SQLEXPRESS";
        public const string BASE_DATOS_POR_DEFECTO = "DB_Sistema";

        private readonly string _cadenaConexion;

        public CD_Conexion()
        {
            _cadenaConexion = ObtenerCadenaConexion();
        }

        public CD_Conexion(string cadenaConexion)
        {
            if (string.IsNullOrEmpty(cadenaConexion))
                throw new ArgumentException("Data Source=.;Initial Catalog=DB_Sistema;Integrated Security=True;TrustServerCertificate=True");

            _cadenaConexion = cadenaConexion;
        }

        public string CadenaConexion
        {
            get { return _cadenaConexion; }
        }

        public SqlConnection ObtenerConexion()
        {
            try
            {
                SqlConnection conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                return conexion;
            }
            catch (SqlException excepcion)
            {
                throw new CDExcepcion("No se pudo establecer la conexión con la base " + NOMBRE_CONEXION + ".", excepcion);
            }
            catch (InvalidOperationException excepcion)
            {
                throw new CDExcepcion("La cadena de conexión de la base " + NOMBRE_CONEXION + " no es válida.", excepcion);
            }
        }

        public static string ObtenerCadenaConexion()
        {
            ConnectionStringSettings configuracion = ConfigurationManager.ConnectionStrings[NOMBRE_CONEXION];

            if (configuracion != null && !string.IsNullOrEmpty(configuracion.ConnectionString))
                return configuracion.ConnectionString;

            return ConstruirCadenaConexion(
                LeerConfiguracion("Servidor", SERVIDOR_POR_DEFECTO),
                LeerConfiguracion("BaseDatos", BASE_DATOS_POR_DEFECTO),
                LeerConfiguracion("Usuario", string.Empty),
                LeerConfiguracion("Clave", string.Empty));
        }

        public static string ConstruirCadenaConexion(string servidor, string baseDatos, string usuario, string clave)
        {
            SqlConnectionStringBuilder constructor = new SqlConnectionStringBuilder();
            constructor.DataSource = servidor;
            constructor.InitialCatalog = baseDatos;
            constructor.ConnectTimeout = 15;
            constructor.IntegratedSecurity = string.IsNullOrEmpty(usuario) && string.IsNullOrEmpty(clave);

            if (!constructor.IntegratedSecurity)
            {
                constructor.UserID = usuario;
                constructor.Password = clave;
            }

            return constructor.ConnectionString;
        }

        private static string LeerConfiguracion(string clave, string valorPorDefecto)
        {
            string valor = ConfigurationManager.AppSettings[clave];
            return string.IsNullOrEmpty(valor) ? valorPorDefecto : valor;
        }
    }
}