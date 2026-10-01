using System;
using System.Windows.Forms;
using CapaLogica;

namespace CapaPresentacion
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            IniciarSesion();
        }

        private void txtUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtContraseña.Focus();
            }
        }

        private void txtContraseña_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                IniciarSesion();
            }
        }

        private void IniciarSesion()
        {
            string userName = txtUsuario.Text.Trim();
            string password = txtContraseña.Text;

            if (userName.Length == 0)
            {
                MostrarMensaje(CL_Usuario.MENSAJE_SIN_USUARIO, txtUsuario);
                return;
            }

            if (password.Length == 0)
            {
                MostrarMensaje(CL_Usuario.MENSAJE_SIN_PASSWORD, txtContraseña);
                return;
            }

            btnIngresar.Enabled = false;

            try
            {
                CL_Usuario logica = new CL_Usuario();
                ResultadoAutenticacion resultado = logica.LoginUsuario(userName, password);

                if (!resultado.Exito || resultado.Usuario == null)
                {
                    txtContraseña.Clear();
                    MostrarMensaje(resultado.Mensaje, txtUsuario);
                    return;
                }

                FrmPrincipal principal = new FrmPrincipal(this, resultado.Usuario);
                this.Hide();
                principal.Show();
            }
            catch (Exception)
            {
                txtContraseña.Clear();
                MostrarMensaje("Ocurrió un error inesperado. Intente nuevamente.", txtUsuario);
            }
            finally
            {
                btnIngresar.Enabled = true;
            }
        }

        private static void MostrarMensaje(string mensaje, Control controlFoco)
        {
            MessageBox.Show(mensaje, "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            controlFoco.Focus();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            DialogResult Resultado = MessageBox.Show(
                "¿Estas seguro que desea Cerrar Sesión?",
                "Cerrar Aplicación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (Resultado == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btnMostrarContraseña_Click(object sender, EventArgs e)
        {
            if (txtContraseña.UseSystemPasswordChar == true)
            {
                txtContraseña.UseSystemPasswordChar = false;
            }
            else
            {
                txtContraseña.UseSystemPasswordChar = true;
            }
        }
    }
}