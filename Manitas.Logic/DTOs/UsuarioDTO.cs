using System;
namespace Manitas.Logic.DTOs
{
    public class UsuarioDTO
    {
        public string Username { get; set; }
        public DateTime? UltimaConexion { get; set; }
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string RolNombre { get; set; }
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string OficioDescripcion { get; set; }
        public string OficioNombre { get; set; }
        public bool IsActivo { get; set; }
        private string _fotoPerfilUrl;
        private string _ineFrenteUrl;
        private string _ineReversoUrl;
        private string _documentoExtraUrl;
        public string FotoPerfilUrl
        {
            get => string.IsNullOrWhiteSpace(_fotoPerfilUrl) ? "no-image" : _fotoPerfilUrl;
            set => _fotoPerfilUrl = value;
        }
        public string IneFrenteUrl
        {
            get => string.IsNullOrWhiteSpace(_ineFrenteUrl) ? null : _ineFrenteUrl;
            set => _ineFrenteUrl = value;
        }
        public string IneReversoUrl
        {
            get => string.IsNullOrWhiteSpace(_ineReversoUrl) ? null : _ineReversoUrl;
            set => _ineReversoUrl = value;
        }
        public string DocumentoExtraUrl
        {
            get => string.IsNullOrWhiteSpace(_documentoExtraUrl) ? null : _documentoExtraUrl;
            set => _documentoExtraUrl = value;
        }
        public string EstadoTexto => IsActivo ? "Activo" : "Inactivo";
        public string EstadoColor => IsActivo ? "#22C55E" : "#94A3B8";
        public string Iniciales
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NombreCompleto)) return "??";
                var partes = NombreCompleto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length == 0) return "??";
                if (partes.Length == 1) return partes[0][0].ToString().ToUpper();
                return (partes[0][0].ToString() + partes[partes.Length - 1][0].ToString()).ToUpper();
            }
        }
        public string ColorRol
        {
            get
            {
                string rol = RolNombre?.ToLower() ?? ""; 
                if (rol == "administrador") return "#EF4444";
                if (rol == "moderador") return "#F59E0B";
                if (rol == "soporte") return "#10B981";
                return "#6366F1";
            }
        }
    }
}