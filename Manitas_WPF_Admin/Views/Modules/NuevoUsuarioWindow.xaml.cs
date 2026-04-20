using Manitas.Logic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Manitas.Data.Models;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace Manitas_WPF_Admin.Views.Modules
{
    /// <summary>
    /// Lógica de interacción para NuevoUsuarioWindow.xaml
    /// </summary>
    public partial class NuevoUsuarioWindow : Window
    {
        private readonly UsuarioService _service = new UsuarioService();

        public NuevoUsuarioWindow()
        {
            InitializeComponent();
        }
        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNombre.Text) ||
                string.IsNullOrWhiteSpace(TxtCorreo.Text) ||
                string.IsNullOrWhiteSpace(TxtPass.Password))
            {
                MessageBox.Show("Por favor, llena todos los campos obligatorios.");
                return;
            }
            if (CboRol.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecciona un rol para el nuevo miembro.");
                return;
            }
            var nuevo = new usuario
            {
                id = Guid.NewGuid(),
                nombre_completo = TxtNombre.Text,
                correo = TxtCorreo.Text,
                contrasena_hash = TxtPass.Password,
                fecha_registro = DateTime.Now,
                activo = true
            };
            string rolSeleccionado = (CboRol.SelectedItem as ComboBoxItem).Content.ToString();
            if (_service.CrearUsuarioStaff(nuevo, rolSeleccionado))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Hubo un problema al guardar en la base de datos. Verifica la conexión.");
            }
        }
    }
}
