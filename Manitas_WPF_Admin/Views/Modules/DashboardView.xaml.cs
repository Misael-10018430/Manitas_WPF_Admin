using Manitas.Logic.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace Manitas_WPF_Admin.Views.Modules
{
    public partial class DashboardView : UserControl
    {
        private readonly UsuarioService _usuarioService;
        public DashboardView()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            CargarEstadisticas();
            CargarActividadReciente();
        }
        private void CargarEstadisticas()
        {
            try
            {
                // 🔄 Sincronizamos con los nuevos nombres del XAML
                TxtTotalUsuarios.Text = "164"; // El que está en medio del círculo
                TxtPendientes.Text = "8";
                TxtActivos.Text = "32";
                TxtDisputas.Text = "2";
                TxtIngresos.Text = string.Format("{0:C}", 15420.50);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estadísticas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void Refrescar()
        {
            CargarEstadisticas();
        }
        private void CargarActividadReciente()
        {
            // Usamos el método optimizado que creamos en el Service
            var recientes = _usuarioService.ObtenerActividadReciente();

            // Lo mandamos a la tabla
            DgActividad.ItemsSource = recientes;
        }
    }
}