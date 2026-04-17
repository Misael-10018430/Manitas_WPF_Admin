using Manitas.Logic.DTOs;             // ✨ QUITA EL ERROR DE UsuarioDTO
using Manitas.Logic.Services;         // ✨ QUITA EL ERROR DE UsuarioService
using System;
using System.Collections.ObjectModel; // ✨ QUITA EL ERROR DE ObservableCollection
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
            // ESTO DEBE IR ANTES DE INITIALIZECOMPONENT
            _usuarioService = new UsuarioService();
            ListaUsuarios = new ObservableCollection<UsuarioDTO>();

            InitializeComponent(); // <--- Aquí es donde se "enciende" el XAML

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
            // 🛡️ ESTA ES LA LÍNEA MÁGICA:
            // Si la tabla es nula (aún no se crea), salimos de la función sin filtrar
            if (DgUsuarios == null) return;

            // 1. Obtener los valores de los filtros
            string busqueda = TxtBusqueda?.Text.ToLower().Trim() ?? "";
            string rolSeleccionado = (CbRol?.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Todos los Miembros";

            // 2. Filtrar la lista que viene de la BD
            var filtrados = ListaUsuarios.Where(u => {
                // Filtro por texto (Nombre o Correo)
                bool coincideTexto = string.IsNullOrEmpty(busqueda) ||
                                     u.NombreCompleto.ToLower().Contains(busqueda) ||
                                     u.Correo.ToLower().Contains(busqueda);

                // Filtro por Rol
                bool coincideRol = true;
                if (rolSeleccionado == "Manitas") coincideRol = u.RolNombre.ToLower().Contains("manita");
                else if (rolSeleccionado == "Clientes") coincideRol = u.RolNombre.ToLower().Contains("cliente");

                return coincideTexto && coincideRol;
            }).ToList();

            // 3. Actualizar la tabla
            DgUsuarios.ItemsSource = filtrados;
        }
        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var seleccionado = DgUsuarios.SelectedItem as UsuarioDTO;

            if (seleccionado != null)
            {
                ColDetalle.Width = new GridLength(350);
                PanelDetalle.Visibility = Visibility.Visible;

                // 1. Llenar los datos de texto
                TxtNombreDetalle.Text = seleccionado.NombreCompleto;
                TxtRolDetalle.Text = seleccionado.RolNombre?.ToUpper();
                if (TxtTelefonoDetalle != null)
                    TxtTelefonoDetalle.Text = seleccionado.Telefono ?? "Sin registro";

                // 📸 2. LÓGICA DE IMAGEN ESTRICTA: Solo muestra si hay URL real
                try
                {
                    if (!string.IsNullOrEmpty(seleccionado.FotoPerfilUrl))
                    {
                        ImgPerfil.Source = new BitmapImage(new Uri(seleccionado.FotoPerfilUrl, UriKind.RelativeOrAbsolute));
                    }
                    else
                    {
                        ImgPerfil.Source = null; // ❌ NO MOSTRAR NADA si no hay foto
                    }

                    if (!string.IsNullOrEmpty(seleccionado.IneFrenteUrl))
                        ImgIneFrente.Source = new BitmapImage(new Uri(seleccionado.IneFrenteUrl, UriKind.RelativeOrAbsolute));
                    else
                        ImgIneFrente.Source = null;
                }
                catch
                {
                    ImgPerfil.Source = null;
                }
            }
        }

        // 📂 3. Acción para Ver el Expediente (El documento PDF/Imagen extra)
        private void BtnVerExpediente_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = DgUsuarios.SelectedItem as UsuarioDTO;

            // Verificamos que haya un documento que mostrar
            if (seleccionado != null && !string.IsNullOrEmpty(seleccionado.DocumentoExtraUrl))
            {
                try
                {
                    // Abre la URL o archivo en el programa predeterminado de Windows
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
        private void BtnCerrarDetalle_Click(object sender, RoutedEventArgs e)
        {
            // Ocultar el panel y regresar la columna a 0
            ColDetalle.Width = new GridLength(0);
            PanelDetalle.Visibility = Visibility.Collapsed;
            DgUsuarios.SelectedItem = null; // Quitamos la selección de la tabla
        }
    }
}