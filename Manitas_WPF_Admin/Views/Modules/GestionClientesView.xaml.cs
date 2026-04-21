using Manitas.Logic.DTOs;
using Manitas.Logic.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Manitas_WPF_Admin.Views.Modules
{
    public partial class GestionClientesView : UserControl
    {
        private UsuarioService _service = new UsuarioService();

        public GestionClientesView()
        {
            InitializeComponent();
            CargarClientes();
        }

        private void CargarClientes()
        {
            string filtro = txtBusqueda?.Text ?? "";
            dgClientes.ItemsSource = _service.ObtenerClientes(filtro);
        }

        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClientes.SelectedItem is UsuarioDTO cliente)
            {
                PanelExpediente.DataContext = cliente;
                PanelExpediente.Visibility = Visibility.Visible;

                ColTabla.Width = new GridLength(1.3, GridUnitType.Star);
                ColSpacer.Width = new GridLength(20);
                ColDetalle.Width = new GridLength(1.7, GridUnitType.Star);

                var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                TransExpediente.BeginAnimation(TranslateTransform.XProperty, anim);
            }
        }

        private void BtnCerrarPanel_Click(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation
            {
                To = 450,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            anim.Completed += (s, ev) =>
            {
                PanelExpediente.Visibility = Visibility.Collapsed;
                ColTabla.Width = new GridLength(1, GridUnitType.Star);
                ColSpacer.Width = new GridLength(0);
                ColDetalle.Width = new GridLength(0);
                dgClientes.SelectedItem = null;
            };
            TransExpediente.BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private void txtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarClientes();
        }

        private void BtnCambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            if (!Manitas.Logic.Security.SesionUsuario.EsAdmin())
            {
                MessageBox.Show("Acceso denegado. Solo los administradores pueden activar o desactivar cuentas de clientes.",
                                "Restricción de Seguridad", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            var cliente = PanelExpediente.DataContext as UsuarioDTO;
            if (cliente == null) return;

            bool nuevoEstado = !cliente.IsActivo;
            try
            {
                _service.ActualizarEstadoUsuario(cliente.Id, nuevoEstado);
                MessageBox.Show($"Usuario {cliente.NombreCompleto} actualizado correctamente.",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hubo un problema al actualizar: {ex.Message}", "Error");
            }
        }
    }
}