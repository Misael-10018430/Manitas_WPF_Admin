using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Manitas.Logic.DTOs;
using Manitas.Logic.Services;
using Manitas.Logic.Security;
namespace Manitas_WPF_Admin.Views.Modules
{
    public partial class UsuariosSistemaView : UserControl
    {
        private readonly UsuarioService _service;
        public ObservableCollection<UsuarioDTO> ListaStaff { get; set; }
        public UsuariosSistemaView()
        {
            InitializeComponent();
            _service = new UsuarioService();
            ListaStaff = new ObservableCollection<UsuarioDTO>();
            DgStaff.ItemsSource = ListaStaff;
            AplicarSeguridad();
            CargarStaff();
        }
        private void CargarStaff()
        {
            try
            {
                var staff = _service.ObtenerUsuariosSistema(TxtBusqueda.Text);

                ListaStaff.Clear();
                foreach (var u in staff)
                {
                    ListaStaff.Add(u);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar staff: " + ex.Message);
            }
        }
        private void DgStaff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var seleccionado = DgStaff.SelectedItem as UsuarioDTO;
            if (seleccionado != null)
            {
                PanelDetalle.DataContext = seleccionado;
                PanelDetalle.Visibility = Visibility.Visible;
                ColDetalle.Width = new GridLength(350); 
            }
        }
        private void BtnCerrarDetalle_Click(object sender, RoutedEventArgs e)
        {
            PanelDetalle.Visibility = Visibility.Collapsed;
            ColDetalle.Width = new GridLength(0);
            DgStaff.SelectedItem = null;
        }
        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarStaff();
        }
        private void BtnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            NuevoUsuarioWindow ventana = new NuevoUsuarioWindow();
            if (ventana.ShowDialog() == true)
            {
                CargarStaff();
            }
        }
        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = DgStaff.SelectedItem as UsuarioDTO;
            if (seleccionado == null) return;

            var result = MessageBox.Show($"¿Deseas resetear la contraseña de {seleccionado.NombreCompleto}?",
                                        "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (_service.ActualizarPassword(seleccionado.Id, "123456"))
                {
                    MessageBox.Show("Contraseña reseteada a: 123456");
                }
            }
        }
        private void BtnCambiarRol_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = DgStaff.SelectedItem as UsuarioDTO;
            if (seleccionado == null) return;

            // Ventana simple para elegir el nuevo rol
            var opciones = new[] { "administrador", "agente_operativo", "agente_disputas" };
            string rolActual = seleccionado.RolNombre;

            // Cicla al siguiente rol como lógica simple
            int indexActual = Array.IndexOf(opciones, rolActual);
            string nuevoRol = opciones[(indexActual + 1) % opciones.Length];

            var confirm = MessageBox.Show(
                $"¿Cambiar el rol de {seleccionado.NombreCompleto} a '{nuevoRol}'?",
                "Cambiar Rol", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                if (_service.ActualizarRolSistema(seleccionado.Id, nuevoRol))
                {
                    CargarStaff();
                    MessageBox.Show("Rol actualizado correctamente.");
                }
            }
        }
        private void AplicarSeguridad()
        {
            if (SesionUsuario.UsuarioActual != null)
            {
                string rol = SesionUsuario.UsuarioActual.RolNombre.ToLower();

                if (rol == "agente_operativo" || rol == "agente_disputas")
                {
                    BtnNuevoUsuario.Visibility = Visibility.Collapsed;
                    BtnResetPassword.Visibility = Visibility.Collapsed;
                    BtnCambiarRol.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}