using BCrypt.Net;
using Manitas.Logic.Services;
using Manitas.Data.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Manitas_WPF_Admin.Views.Modules
{
    public partial class NuevoUsuarioWindow : Window
    {
        private readonly UsuarioService _service = new UsuarioService();

        public NuevoUsuarioWindow()
        {
            InitializeComponent();
            // Selecciona el primer item por defecto
            if (CboRol.Items.Count > 0)
                (CboRol.Items[0] as ComboBoxItem).IsSelected = true;
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
                MessageBox.Show("Por favor, selecciona un rol.");
                return;
            }

            // Validar que el correo no esté registrado
            if (_service.CorreoYaExiste(TxtCorreo.Text))
            {
                MessageBox.Show("Este correo electrónico ya está registrado en el sistema. Usa uno diferente.",
                                "Correo duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string rolSeleccionado = (CboRol.SelectedItem as ComboBoxItem).Content.ToString();

            var nuevo = new usuario
            {
                id = Guid.NewGuid(),
                nombre_completo = TxtNombre.Text.Trim(),
                correo = TxtCorreo.Text.Trim(),
                contrasena_hash = BCrypt.Net.BCrypt.HashPassword(TxtPass.Password),
                telefono = "0000000000",
                fecha_registro = DateTime.Now,
                activo = true,
                correo_verificado = true,
                terminos_aceptados = true
            };

            try
            {
                if (_service.CrearUsuarioStaff(nuevo, rolSeleccionado))
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se encontró el rol seleccionado en la base de datos.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error detallado: " + ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}