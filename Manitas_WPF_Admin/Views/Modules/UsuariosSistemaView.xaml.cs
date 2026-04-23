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

                bool esAdmin = SesionUsuario.EsAdmin();
                bool seleccionadoEsAdmin = seleccionado.RolNombre == "administrador";

                // Resetear y cambiar rol: solo admin
                BtnResetPassword.IsEnabled = esAdmin;
                BtnCambiarRol.IsEnabled = esAdmin;

                // Estilo visual para indicar que están deshabilitados
                BtnResetPassword.Opacity = esAdmin ? 1.0 : 0.4;
                BtnCambiarRol.Opacity = esAdmin ? 1.0 : 0.4;

                // Eliminar: solo admin, y solo si el seleccionado NO es admin
                BtnEliminarUsuario.Visibility = (esAdmin && !seleccionadoEsAdmin)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
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
                bool esAdmin = SesionUsuario.EsAdmin();

                // Solo admin puede crear nuevos miembros
                BtnNuevoUsuario.Visibility = esAdmin ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void BtnEliminarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (!SesionUsuario.EsAdmin())
            {
                MessageBox.Show("Solo los administradores pueden eliminar usuarios.",
                                "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            var seleccionado = DgStaff.SelectedItem as UsuarioDTO;
            if (seleccionado == null) return;

            if (seleccionado.RolNombre == "administrador")
            {
                MessageBox.Show("No se puede eliminar a un administrador.",
                                "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Estás seguro de eliminar a {seleccionado.NombreCompleto}?\nEsta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    bool exito = _service.EliminarUsuarioInterno(seleccionado.Id);
                    if (exito)
                    {
                        MessageBox.Show("Usuario eliminado correctamente.",
                                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarStaff();
                        BtnCerrarDetalle_Click(null, null);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar el usuario.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.InnerException?.Message ?? ex.Message);
                }
            }
        }
    }
}