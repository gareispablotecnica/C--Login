using System;
using System.Windows.Forms;
using CapaLogica;

namespace CapaPresentacion
{
    public partial class FrmPrincipal : Form
    {
        private readonly FrmLogin _frmLogin;

        public Usuario UsuarioActual { get; private set; }

        public FrmPrincipal(FrmLogin frmLogin, Usuario usuario)
        {
            InitializeComponent();
            _frmLogin = frmLogin;
            UsuarioActual = usuario;
            MostrarDatosUsuario();
        }

        private void MostrarDatosUsuario()
        {
            lblSaludo.Text = "Bienvenido " + UsuarioActual.UserName;
            lblEmail.Text = "Email: " + UsuarioActual.Email;
            lblRol.Text = "Rol: " + UsuarioActual.NombreRol;
            lblIdentificadores.Text = "IDUsuario: " + UsuarioActual.IDUsuario + "    IDRol: " + UsuarioActual.IDRol;
            lblEstado.Text = "Estado: " + (UsuarioActual.Estado ? "Activo" : "Inactivo");
            this.Text = "Principal - " + UsuarioActual.UserName;
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            this.Close();
            _frmLogin.Show();
            _frmLogin.Activate();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}