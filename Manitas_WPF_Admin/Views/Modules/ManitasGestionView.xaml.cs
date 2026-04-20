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
        private void BtnVerDetalles_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var manita = btn?.DataContext as UsuarioDTO;

            if (manita != null)
            {
                ColDetalle.Width = new GridLength(450);
                PnlDetalles.Visibility = Visibility.Visible;
                PnlDetalles.DataContext = manita;
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
                ColDetalle.Width = new GridLength(0); 
            };
            TransDetalle.BeginAnimation(TranslateTransform.XProperty, slideAnim);
        }
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
        #endregion
        private void DgManitas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var manita = DgManitas.SelectedItem as UsuarioDTO;
            if (manita != null)
            {
                PnlDetalles.Visibility = Visibility.Visible;
                ColTabla.Width = new GridLength(2, GridUnitType.Star);
                ColSpacer.Width = new GridLength(30);
                ColDetalle.Width = new GridLength(1.2, GridUnitType.Star);
                if (PnlRechazo != null) PnlRechazo.Visibility = Visibility.Collapsed;
                var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
                TransDetalle.BeginAnimation(TranslateTransform.XProperty, anim);
            }
        }
    }
}