using Manitas.Logic.DTOs;             
using Manitas.Logic.Services;       
using System;
using System.Collections.ObjectModel; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Linq; 
namespace Manitas_WPF_Admin.Views.Modules
{
    public partial class UsuariosGlobalView : UserControl
    {
        private readonly UsuarioService _usuarioService;
        public ObservableCollection<UsuarioDTO> ListaUsuarios { get; set; }
        public UsuariosGlobalView()
        {
            _usuarioService = new UsuarioService();
            ListaUsuarios = new ObservableCollection<UsuarioDTO>();
            InitializeComponent();
            DgUsuarios.ItemsSource = ListaUsuarios;
            CargarUsuarios();
        }
        private void CargarUsuarios()
        {
            if (_usuarioService == null) return;
            string busqueda = TxtBusqueda?.Text ?? "";
            string rol = (CbRol?.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Todos";
            try
            {
                var resultados = _usuarioService.ObtenerUsuariosGlobal(busqueda, rol);
                ListaUsuarios.Clear();
                foreach (var u in resultados)
                {
                    ListaUsuarios.Add(u);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al filtrar: " + ex.Message);
            }
        }
        private void Filtros_Changed(object sender, EventArgs e)
        {
            if (DgUsuarios == null) return;
            string busqueda = TxtBusqueda?.Text.ToLower().Trim() ?? "";
            string rolSeleccionado = (CbRol?.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Todos los Miembros";
            var filtrados = ListaUsuarios.Where(u => {
                bool coincideTexto = string.IsNullOrEmpty(busqueda) ||
                                     u.NombreCompleto.ToLower().Contains(busqueda) ||
                                     u.Correo.ToLower().Contains(busqueda);
                bool coincideRol = true;
                if (rolSeleccionado == "Manitas") coincideRol = u.RolNombre.ToLower().Contains("manita");
                else if (rolSeleccionado == "Clientes") coincideRol = u.RolNombre.ToLower().Contains("cliente");
                return coincideTexto && coincideRol;
            }).ToList();
            DgUsuarios.ItemsSource = filtrados;
        }
        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var usuario = DgUsuarios.SelectedItem as UsuarioDTO;

            if (usuario != null)
            {
                PanelDetalle.DataContext = usuario;
                PanelDetalle.Visibility = Visibility.Visible;
                ColTabla.Width = new GridLength(2, GridUnitType.Star); 
                ColSpacer.Width = new GridLength(30);           
                ColDetalle.Width = new GridLength(1, GridUnitType.Star); 
                SecOficio.Visibility = (usuario.RolNombre?.ToLower() == "manitas")
                                        ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void BtnCerrarDetalle_Click(object sender, RoutedEventArgs e)
        {
            ColTabla.Width = new GridLength(1, GridUnitType.Star);
            ColSpacer.Width = new GridLength(0);
            ColDetalle.Width = new GridLength(0);
            PanelDetalle.Visibility = Visibility.Collapsed;
            DgUsuarios.SelectedItem = null;
        }
        private void BtnVerExpediente_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = DgUsuarios.SelectedItem as UsuarioDTO;
            if (seleccionado != null && !string.IsNullOrEmpty(seleccionado.DocumentoExtraUrl))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(seleccionado.DocumentoExtraUrl) { UseShellExecute = true });
                }
                catch
                {
                    MessageBox.Show("No se pudo abrir el archivo. Verifica la ruta.");
                }
            }
            else
            {
                MessageBox.Show("Este usuario no tiene un comprobante o INE cargado aún.");
            }
        }
        private void BtnCambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            var usuario = DgUsuarios.SelectedItem as UsuarioDTO;

            if (usuario == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario primero.");
                return;
            }
            string accion = usuario.IsActivo ? "SUSPENDER" : "ACTIVAR";
            var resultado = MessageBox.Show($"¿Estás seguro de que deseas {accion} la cuenta de {usuario.NombreCompleto}?",
                                            "Confirmar cambio de estado",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);
            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    bool nuevoEstado = !usuario.IsActivo;

                    bool exito = _usuarioService.CambiarEstatusUsuario(usuario.Id, nuevoEstado);

                    if (exito)
                    {
                        MessageBox.Show($"La cuenta ha sido {(nuevoEstado ? "activada" : "suspendida")} correctamente.");
                        CargarUsuarios();
                        BtnCerrarDetalle_Click(null, null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cambiar el estado: " + ex.Message);
                }
            }
        }
    }
}