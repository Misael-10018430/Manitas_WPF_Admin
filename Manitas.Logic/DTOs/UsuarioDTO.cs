using System;

namespace Manitas.Logic.DTOs
{
    public class UsuarioDTO
    {
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
            get => string.IsNullOrWhiteSpace(_fotoPerfilUrl) ? null : _fotoPerfilUrl;
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
    }
}