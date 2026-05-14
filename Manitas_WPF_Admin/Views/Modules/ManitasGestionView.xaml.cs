using Manitas.Logic.DTOs;
using Manitas.Logic.Security;
using Manitas.Logic.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;                
using System.Windows.Controls;
using System.Windows.Media;         
using System.Windows.Media.Animation; 
namespace Manitas_WPF_Admin.Views.Modules
{
    public partial class ManitasGestionView : UserControl
    {
        public ObservableCollection<UsuarioDTO> ListaManitas { get; set; }
        private readonly UsuarioService _usuarioService;
        //Funcionalidad de esta vista: mostrar una lista de los usuarios que tienen el rol de 'Manita' y permitir gestionar sus detalles.
        public ManitasGestionView()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            ListaManitas = new ObservableCollection<UsuarioDTO>();
            DgManitas.ItemsSource = ListaManitas;
            CargarDatosDesdeBD();
            ScvDetalles.PreviewMouseWheel += (s, e) =>
            {
                var scv = (ScrollViewer)s;
                scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
                e.Handled = true;
            };
        }
        // En esta sección se encuentran los métodos relacionados con la carga de datos desde la base de datos y la funcionalidad de búsqueda en la lista de Manitas, lo que permite a los administradores encontrar rápidamente perfiles específicos y gestionar sus detalles de manera eficiente.
        #region
        private void CargarDatosDesdeBD()
        {
            try
            {
                var manitasDesdeDb = _usuarioService.ObtenerSolicitudesPendientes();

                ListaManitas.Clear();
                foreach (var m in manitasDesdeDb)
                {
                    ListaManitas.Add(m);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar: " + ex.Message);
            }
        }
        // Funcionalidad de búsqueda en tiempo real: filtra la lista de Manitas a medida que el usuario escribe en el cuadro de búsqueda, mostrando solo aquellos perfiles que coinciden con el nombre o correo ingresados.
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
        // En esta sección se encuentran los métodos relacionados con la visualización de detalles de cada Manita, así como las acciones de aprobación y rechazo. Al hacer clic en "Ver Detalles", se muestra un panel lateral con información detallada del perfil seleccionado, y se ofrecen opciones para aprobar o rechazar la solicitud, con animaciones suaves para mejorar la experiencia del usuario.
        private void BtnVerDetalles_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var manita = btn?.DataContext as UsuarioDTO;

            if (manita != null)
            {
                PnlDetalles.Visibility = Visibility.Visible;
                PnlDetalles.DataContext = manita;
                ColTabla.Width = new GridLength(1.3, GridUnitType.Star);
                ColSpacer.Width = new GridLength(20);
                ColDetalle.Width = new GridLength(1.7, GridUnitType.Star);

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
        // Funcionalidad de cierre del panel de detalles: al hacer clic en "Cerrar Detalles", se ejecuta una animación que desliza el panel hacia la derecha y luego lo oculta, restableciendo el diseño de la tabla para mostrar la lista completa de Manitas.
        private void BtnCerrarDetalle_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation slideAnim = new DoubleAnimation
            {
                To = 450,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            slideAnim.Completed += (s, ev) =>
            {
                PnlDetalles.Visibility = Visibility.Collapsed;
                ColTabla.Width = new GridLength(1, GridUnitType.Star);
                ColSpacer.Width = new GridLength(0);
                ColDetalle.Width = new GridLength(0);
                DgManitas.SelectedItem = null;
            };
            TransDetalle.BeginAnimation(TranslateTransform.XProperty, slideAnim);
        }
        // Funcionalidad de aprobación de perfiles: al hacer clic en "Aprobar", se muestra un mensaje de confirmación, y si el administrador confirma, se actualiza el estado del usuario en la base de datos para marcarlo como aprobado, permitiéndole recibir trabajos en la plataforma web.
        private void BtnAprobar_Click(object sender, RoutedEventArgs e)
        {
            if (!SesionUsuario.EsAdmin())
            {
                MessageBox.Show("No tienes permisos para validar perfiles de Manitas.", "Acceso Restringido", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            var manita = PnlDetalles.DataContext as UsuarioDTO;
            if (manita == null) return;
            var resultado = MessageBox.Show($"¿Confirmas la aprobación de {manita.NombreCompleto}? Al hacerlo, podrá recibir trabajos en la plataforma web.",
                "Validación de Perfil", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resultado == MessageBoxResult.Yes)
            {
                bool exito = _usuarioService.AprobarUsuarioComoManita(manita.Id);
                if (exito)
                {
                    MessageBox.Show("¡Perfil validado! El Manitas ahora está activo en el sistema global.");
                    CargarDatosDesdeBD();
                    BtnCerrarDetalle_Click(null, null);
                }
            }
        }
        // Funcionalidad de rechazo de perfiles: al hacer clic en "Rechazar", se despliega un panel para ingresar el motivo del rechazo, y al confirmar, se actualiza el estado del usuario en la base de datos para marcarlo como rechazado, informando al usuario del motivo a través de un mensaje claro.
        private void BtnRechazar_Click(object sender, RoutedEventArgs e)
        {
            if (!Manitas.Logic.Security.SesionUsuario.EsAdmin())
            {
                MessageBox.Show("No tienes permisos suficientes para rechazar solicitudes de registro.",
                                "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            if (PnlRechazo.Visibility == Visibility.Collapsed)
            {
                PnlRechazo.Visibility = Visibility.Visible;
                TxtMotivoRechazo.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtMotivoRechazo.Text))
            {
                MessageBox.Show("Por favor, indica el motivo del rechazo para informar al usuario.",
                                "Motivo Obligatorio", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var manita = PnlDetalles.DataContext as UsuarioDTO;
            if (manita == null)
            {
                MessageBox.Show("Error: No se pudo identificar al usuario seleccionado.");
                return;
            }
            try
            {
                bool exito = _usuarioService.ActualizarEstadoManitas(manita.Id, "rechazado", TxtMotivoRechazo.Text);
                if (exito)
                {
                    MessageBox.Show($"La solicitud de {manita.NombreCompleto} ha sido rechazada con éxito.",
                                    "Proceso Completado", MessageBoxButton.OK, MessageBoxImage.Information);

                    CargarDatosDesdeBD();
                    BtnCerrarDetalle_Click(null, null);
                    TxtMotivoRechazo.Clear();
                    PnlRechazo.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error técnico al rechazar: {ex.Message}", "Error");
            }
        }
        // Funcionalidad de selección de Manitas: al seleccionar un perfil en la tabla, se despliega automáticamente el panel de detalles con la información del usuario seleccionado, permitiendo a los administradores revisar rápidamente los detalles y tomar decisiones de gestión sin necesidad de hacer clic adicionalmente en "Ver Detalles".
        #endregion
        private void DgManitas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var manita = DgManitas.SelectedItem as UsuarioDTO;
            if (manita != null)
            {
                ScvDetalles.ScrollToHome();
                PnlDetalles.Visibility = Visibility.Visible;
                PnlDetalles.DataContext = manita;

                // Panel más ancho: tabla ocupa menos, detalle ocupa más
                ColTabla.Width = new GridLength(1.3, GridUnitType.Star);
                ColSpacer.Width = new GridLength(20);
                ColDetalle.Width = new GridLength(1.7, GridUnitType.Star);

                if (PnlRechazo != null) PnlRechazo.Visibility = Visibility.Collapsed;

                var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                TransDetalle.BeginAnimation(TranslateTransform.XProperty, anim);
            }
        }
    }
}