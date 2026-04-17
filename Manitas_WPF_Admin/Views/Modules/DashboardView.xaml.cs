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
                TxtTotalUsuarios.Text = "164"; 
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
            var recientes = _usuarioService.ObtenerActividadReciente();
            DgActividad.ItemsSource = recientes;
        }
    }
}