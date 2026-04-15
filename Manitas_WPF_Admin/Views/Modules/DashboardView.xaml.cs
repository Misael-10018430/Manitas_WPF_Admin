using System;
using System.Windows;
using System.Windows.Controls;
using Manitas.Logic.Services;
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
        }
        private void CargarEstadisticas()
        {
            try
            {
                TxtManitasActivos.Text = "124";
                TxtSolicitudes.Text = "8";
                TxtEnCurso.Text = "32";
                TxtDisputas.Text = "2";
                TxtComisiones.Text = string.Format("{0:C}", 15420.50);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la base de datos: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void Refrescar()
        {
            CargarEstadisticas();
        }
    }
}