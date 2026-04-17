using System;
using System.Windows;
using Manitas.Logic.DTOs;
using Manitas_WPF_Admin.Views.Modules;
using Manitas_WPF_Admin.Views.Account;
namespace Manitas_WPF_Admin.Views.Main
{
    public partial class MainDashboard : Window
    {
        public UsuarioDTO UsuarioSesion { get; set; }
        /// <summary>
        /// Constructor principal que recibe al usuario autenticado
        /// </summary>
        public MainDashboard(UsuarioDTO usuario)
        {
            InitializeComponent();
            this.UsuarioSesion = usuario;
            TxtNombreAdminSidebar.Text = usuario.NombreCompleto;
            TxtRolAdmin.Text = usuario.RolNombre;
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
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show("¿Estás seguro de que deseas cerrar sesión?",
                                          "Cerrar Sesión",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
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
            TxtNombreAdmin.Text = $"{saludo}, {TxtNombreAdminSidebar.Text}";
        }
    }
}