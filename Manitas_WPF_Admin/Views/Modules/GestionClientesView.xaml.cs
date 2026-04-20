using Manitas.Logic.DTOs;
using Manitas.Logic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace Manitas_WPF_Admin.Views.Modules
{
    /// <summary>
    /// Lógica de interacción para GestionClientesView.xaml
    /// </summary>
    public partial class GestionClientesView : UserControl
    {
        public GestionClientesView()
        {
            InitializeComponent();
            CargarClientes(); 
        }
        private UsuarioService _service = new UsuarioService();
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
                ColumnExpediente.Width = new GridLength(350); 
            }
        }
        private void BtnCerrarPanel_Click(object sender, RoutedEventArgs e)
        {
            PanelExpediente.Visibility = Visibility.Collapsed;
            ColumnExpediente.Width = new GridLength(0); 
            dgClientes.SelectedItem = null;
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
                                "Restricción de Seguridad",
                                MessageBoxButton.OK,
                                MessageBoxImage.Stop);
                return; 
            }
            var cliente = (UsuarioDTO)PanelExpediente.DataContext;
            if (cliente == null) return;
            bool nuevoEstado = !cliente.IsActivo;
            try
            {
                _service.ActualizarEstadoUsuario(cliente.Id, nuevoEstado);

                MessageBox.Show($"Usuario {cliente.NombreCompleto} actualizado correctamente.",
                                "Éxito",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                CargarClientes(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hubo un problema al actualizar: {ex.Message}", "Error");
            }
        }
    }
}
