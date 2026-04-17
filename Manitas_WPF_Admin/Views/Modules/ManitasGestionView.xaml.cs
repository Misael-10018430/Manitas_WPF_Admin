using System;
using System.Linq;
using System.Windows;                // ✨ Quita el error de Visibility
using System.Windows.Controls;
using System.Windows.Media;          // ✨ Quita el error de TranslateTransform
using System.Windows.Media.Animation; // ✨ Quita el error de DoubleAnimation y CubicEase
using System.Collections.ObjectModel;
using Manitas.Logic.DTOs;
using Manitas.Logic.Services;
namespace Manitas_WPF_Admin.Views.Modules
{
    public partial class ManitasGestionView : UserControl
    {
        public ObservableCollection<UsuarioDTO> ListaManitas { get; set; }
        private readonly UsuarioService _usuarioService;
        public ManitasGestionView()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            ListaManitas = new ObservableCollection<UsuarioDTO>();
            DgManitas.ItemsSource = ListaManitas;
            CargarDatosDesdeBD();
        }
        #region
        private void CargarDatosDesdeBD()
        {
            try
            {
                var manitasDesdeDb = _usuarioService.ObtenerManitas();
                ListaManitas.Clear();
                foreach (var m in manitasDesdeDb)
                {
                    ListaManitas.Add(m);
                }
                if (ListaManitas.Count == 0) 
                {
                    ListaManitas.Add(new UsuarioDTO
                    {
                        Id = Guid.NewGuid(),
                        NombreCompleto = "Misael (Modo Prueba)",
                        Correo = "misael@test.com",
                        Telefono = "9933123456",             // ✅ USA TELEFONO
                        OficioDescripcion = "Desarrollador WPF", // ✅ CAMBIÓ DE NOMBRE
                        RolNombre = "Manita"
                        // 🗑️ BORRA LA LÍNEA DE UBICACION COMPLETAMENTE
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar: " + ex.Message);
            }
        }
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = TxtBuscar.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(filtro))
            {
                DgManitas.ItemsSource = ListaManitas;
            }
            else
            {
                var filtrados = ListaManitas.Where(m =>
                    m.NombreCompleto.ToLower().Contains(filtro) ||
                    m.Correo.ToLower().Contains(filtro)
                ).ToList();

                DgManitas.ItemsSource = filtrados;
            }
        }
        #endregion
        #region
        /// <summary>
        /// Abre el panel lateral con animación y carga los datos del manita seleccionado
        /// </summary>
        private void BtnVerDetalles_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var manita = btn?.DataContext as UsuarioDTO;

            if (manita != null)
            {
                // ✅ Sincronizamos con los nombres reales del DTO y del XAML
                DetalleNombre.Text = manita.NombreCompleto;
                DetalleCorreo.Text = manita.Correo;
                // DetalleUbicacion.Text = manita.Ubicacion; // 🗑️ BORRADO: Ya no existe
                DetalleOficio.Text = manita.OficioDescripcion; // ✅ Nombre correcto

                PnlDetalles.Visibility = Visibility.Visible;
                PnlDetalles.DataContext = manita; // ✨ Esto auto-rellena las fotos

                // Animación de entrada
                DoubleAnimation slideAnim = new DoubleAnimation
                {
                    From = 450,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.4),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                TransDetalle.BeginAnimation(TranslateTransform.XProperty, slideAnim);
            }
        }
        /// <summary>
        /// Cierra el panel lateral con animación de salida
        /// </summary>
        private void BtnCerrarDetalle_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation slideAnim = new DoubleAnimation
            {
                To = 450,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            slideAnim.Completed += (s, ev) => PnlDetalles.Visibility = Visibility.Collapsed;

            TransDetalle.BeginAnimation(TranslateTransform.XProperty, slideAnim);
        }
        private void BtnAprobar_Click(object sender, RoutedEventArgs e)
        {
            var manita = PnlDetalles.DataContext as UsuarioDTO;
            if (manita == null) return;

            var resultado = MessageBox.Show($"¿Confirmas la aprobación de {manita.NombreCompleto}?",
                                            "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                // Llamamos al servicio para actualizar en la DB
                bool exito = _usuarioService.ActualizarEstadoManitas(manita.Id, "activo", null);

                if (exito)
                {
                    MessageBox.Show("El perfil ha sido activado correctamente.");
                    CargarDatosDesdeBD(); // Refrescamos la lista
                    BtnCerrarDetalle_Click(null, null); // Cerramos el panel
                }
            }
        }
        private void BtnRechazar_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Si el panel no es visible, lo mostramos
            if (PnlRechazo.Visibility == Visibility.Collapsed)
            {
                PnlRechazo.Visibility = Visibility.Visible;
                TxtMotivoRechazo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtMotivoRechazo.Text))
            {
                MessageBox.Show("Por favor, indica el motivo del rechazo para informar al usuario.");
                return;
            }

            var manita = PnlDetalles.DataContext as UsuarioDTO;
            // ✅ Asegúrate de que este método exista en tu UsuarioService
            bool exito = _usuarioService.ActualizarEstadoManitas(manita.Id, "rechazado", TxtMotivoRechazo.Text);

            if (exito)
            {
                MessageBox.Show("Solicitud rechazada. Se ha notificado al usuario.");
                CargarDatosDesdeBD();
                BtnCerrarDetalle_Click(null, null);
            }
        }
        #endregion
    }
}