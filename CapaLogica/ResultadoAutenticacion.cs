namespace CapaLogica
{
    public class ResultadoAutenticacion
    {
        public bool Exito { get; set; }

        public string Mensaje { get; set; }

        public Usuario Usuario { get; set; }

        public static ResultadoAutenticacion Exitoso(Usuario usuario)
        {
            return new ResultadoAutenticacion
            {
                Exito = true,
                Mensaje = string.Empty,
                Usuario = usuario
            };
        }

        public static ResultadoAutenticacion Fallido(string mensaje)
        {
            return new ResultadoAutenticacion
            {
                Exito = false,
                Mensaje = mensaje,
                Usuario = null
            };
        }
    }
}