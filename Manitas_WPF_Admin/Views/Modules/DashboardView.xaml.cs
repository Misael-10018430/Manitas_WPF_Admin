using System;
using System.Windows;
using System.Windows.Controls;
using Manitas.Logic.Services; // Asegúrate de tener esta referencia

namespace Manitas_WPF_Admin.Views.Modules
{
    public partial class DashboardView : UserControl
    {
        // 1. Instanciamos el servicio (como hiciste en el Login)
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
                // En un escenario real, aquí llamarías a métodos de tu Service
                // Ejemplo: TxtManitasActivos.Text = _usuarioService.ContarManitasActivos().ToString();

                // Por ahora, mantenemos estos valores pero prepárate para conectarlos:
                TxtManitasActivos.Text = "124";
                TxtSolicitudes.Text = "8";
                TxtEnCurso.Text = "32";
                TxtDisputas.Text = "2";
                TxtComisiones.Text = string.Format("{0:C}", 15420.50); // Formato de moneda profesional
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