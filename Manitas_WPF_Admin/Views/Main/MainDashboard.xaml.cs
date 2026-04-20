using Manitas.Logic.DTOs;
using Manitas.Logic.Security;
using Manitas_WPF_Admin.Views.Account;
using Manitas_WPF_Admin.Views.Modules;
using System;
using System.Windows;
namespace Manitas_WPF_Admin.Views.Main
{
    public partial class MainDashboard : Window
    {
        public UsuarioDTO UsuarioSesion { get; set; }
        public MainDashboard()
        {
            InitializeComponent();

        }
        public MainDashboard(UsuarioDTO usuario) : this()
        {
            this.UsuarioSesion = usuario;
            SesionUsuario.UsuarioActual = usuario;
            this.DataContext = usuario;
            ConfigurarMenuPorRol();
            EstablecerSaludo();
            CargarModuloDashboard();
        }
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            CargarModuloDashboard();
        }
        private void BtnManitas_Click(object sender, RoutedEventArgs e)
        {
            TxtModuloActual.Text = "Gestión de Manitas";
            MainFrame.Navigate(new ManitasGestionView());
        }
        /// <summary>
        /// Oculta o muestra botones del menú según el rango del usuario
        /// </summary>
        private void ConfigurarMenuPorRol()
        {
            if (!SesionUsuario.EsAdmin())
            {
                if (RbUsuarios != null) RbUsuarios.Visibility = Visibility.Collapsed;
            }
        }
        private void BtnUsuariosSistema_Click(object sender, RoutedEventArgs e)
        {
            if (SesionUsuario.EsAdmin())
            {
                TxtModuloActual.Text = "Gestión de Usuarios del Sistema";
                MainFrame.Navigate(new UsuariosSistemaView());
            }
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("¿Seguro que quieres cerrar sesión?", "Cerrar Sesión",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SesionUsuario.Logout();
                LoginView login = new LoginView();
                login.Show();
                this.Close();
            }
        }
        private void CargarModuloDashboard()
        {
            TxtModuloActual.Text = "Dashboard Principal";
            MainFrame.Navigate(new DashboardView());
        }
        private void BtnUsuariosGlobal_Click(object sender, RoutedEventArgs e)
        {
            TxtModuloActual.Text = "Directorio Global de Usuarios";
            MainFrame.Navigate(new UsuariosGlobalView());
        }
        private void EstablecerSaludo()
        {
            int hora = DateTime.Now.Hour;
            string saludo;

            if (hora >= 6 && hora < 12) saludo = "Buenos días";
            else if (hora >= 12 && hora < 19) saludo = "Buenas tardes";
            else saludo = "Buenas noches";
            TxtNombreAdmin.Text = $"{saludo}, {UsuarioSesion.NombreCompleto}";
        }
        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GestionClientesView());

            TxtModuloActual.Text = "Administración de Cuentas de Clientes";
        }
    }
}