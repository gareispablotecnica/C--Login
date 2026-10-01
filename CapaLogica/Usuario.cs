namespace CapaLogica
{
    public class Usuario
    {
        public int IDUsuario { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public int IDRol { get; set; }

        public string NombreRol { get; set; }

        public bool Estado { get; set; }
    }
}