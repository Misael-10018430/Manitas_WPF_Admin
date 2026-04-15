using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Manitas.Logic.DTOs;
using Manitas.Logic.Services;

namespace Manitas_WPF_Admin.Views.Modules
{
    public partial class ManitasGestionView : UserControl
    {
        // Colección principal ligada al DataGrid
        public ObservableCollection<UsuarioDTO> ListaManitas { get; set; }
        private readonly UsuarioService _usuarioService;

        public ManitasGestionView()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            ListaManitas = new ObservableCollection<UsuarioDTO>();

            // Vinculamos la lista al DataGrid del XAML
            DgManitas.ItemsSource = ListaManitas;

            // Cargamos los datos reales desde la base de datos al iniciar
            CargarDatosDesdeBD();
        }

        #region 🛠️ Carga de Datos y Filtros

        private void CargarDatosDesdeBD()
        {
            try
            {
                // 1. Esto intenta traer los datos de tu SQL Server
                var manitasDesdeDb = _usuarioService.ObtenerManitas();
                ListaManitas.Clear();

                foreach (var m in manitasDesdeDb)
                {
                    ListaManitas.Add(m);
                }

                // ══════════════════════════════════════════════════════════════════
                // 🚀 AQUÍ PONES EL DATO DE PRUEBA (Hardcodeado)
                // ══════════════════════════════════════════════════════════════════
                if (ListaManitas.Count == 0) // Solo si la base de datos está vacía
                {
                    ListaManitas.Add(new UsuarioDTO
                    {
                        Id = Guid.NewGuid(),
                        NombreCompleto = "Misael (Modo Prueba)",
                        Correo = "misael@test.com",
                        OficioNombre = "Desarrollador WPF",
                        Ubicacion = "Villahermosa, Tabasco",
                        RolNombre = "Manita"
                    });
                }
                // ══════════════════════════════════════════════════════════════════
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

        #region 🎭 Lógica del Panel de Detalles (ZARVIS Drawer)

        /// <summary>
        /// Abre el panel lateral con animación y carga los datos del manita seleccionado
        /// </summary>
        private void BtnVerDetalles_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var manita = btn?.DataContext as UsuarioDTO;

            if (manita != null)
            {
                // 1. Asignar textos (Asegúrate que los nombres coincidan con el XAML)
                DetalleNombre.Text = manita.NombreCompleto;
                DetalleCorreo.Text = manita.Correo;
                DetalleUbicacion.Text = manita.Ubicacion;
                DetalleOficio.Text = manita.OficioNombre; // O el campo que tengas para el oficio

                // 2. Hacer visible el panel
                PnlDetalles.Visibility = Visibility.Visible;

                // 3. Ejecutar animación de entrada (Slide de derecha a izquierda)
                DoubleAnimation slideAnim = new DoubleAnimation
                {
                    From = 450, // Ancho del panel
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
            if (DataContext is UsuarioDTO manitaSeleccionado)
            {
                bool exito = _usuarioService.AprobarUsuarioComoManita(manitaSeleccionado.Id);

                if (exito)
                {
                    MessageBox.Show("¡Manita aprobado con éxito!");
                    CargarDatosDesdeBD();
                }
            }
        }
        private void BtnRechazar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var manita = btn?.DataContext as UsuarioDTO;

            if (manita != null)
            {
                var result = MessageBox.Show($"¿Estás seguro de rechazar la solicitud de {manita.NombreCompleto}? Esta acción lo desactivará del sistema.",
                                           "Advertencia de Rechazo",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (_usuarioService.RechazarUsuario(manita.Id))
                    {
                        BtnCerrarDetalle_Click(null, null);
                        CargarDatosDesdeBD();
                        MessageBox.Show("Solicitud rechazada y registro desactivado.", "Proceso Completado");
                    }
                }
            }
        }
        #endregion
    }
}