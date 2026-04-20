using Manitas.Logic.Security;
using Manitas.Logic.Services;
using System;
using System.Windows;
using System.Windows.Input;
namespace Manitas_WPF_Admin.Views.Account
{
    public partial class LoginView : Window
    {
        private readonly UsuarioService _usuarioService;
        public LoginView()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.Source is System.Windows.Controls.TextBox ||
                    e.Source is System.Windows.Controls.PasswordBox ||
                    e.Source is System.Windows.Controls.Button)
                    return;

                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };
        }
        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            EjecutarLogin();
        }
        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                EjecutarLogin();
        }
        private void EjecutarLogin()
        {
            OcultarError();
            string correo = TxtCorreo.Text.Trim();
            string password = TxtPassword.Password;
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(password))
            {
                MostrarError("Por favor, completa todos los campos.");
                return;
            }
            BtnIngresar.IsEnabled = false;
            BtnIngresar.Content = "Verificando...";
            try
            {
                var usuario = _usuarioService.Autenticar(correo, password);

                if (usuario != null)
                {
                    SesionUsuario.Logout();
                    SesionUsuario.UsuarioActual = usuario;
                    var dashboard = new Manitas_WPF_Admin.Views.Main.MainDashboard(usuario);
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    MostrarError("Correo o contraseña incorrectos.");
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error de conexión: {ex.Message}");
            }
            finally
            {
                BtnIngresar.IsEnabled = true;
                BtnIngresar.Content = "Ingresar";
            }
        }
        private void MostrarError(string mensaje)
        {
            TxtError.Text = mensaje;
            PanelError.Visibility = Visibility.Visible;
        }
        private void OcultarError()
        {
            PanelError.Visibility = Visibility.Collapsed;
        }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}