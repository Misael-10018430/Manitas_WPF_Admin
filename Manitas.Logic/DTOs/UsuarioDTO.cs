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
        public string FotoPerfilUrl { get; set; }
        public string IneFrenteUrl { get; set; }
        public string IneReversoUrl { get; set; }
        public string DocumentoExtraUrl { get; set; }
        public bool IsActivo { get; set; } 
        public string EstadoTexto => IsActivo ? "Activo" : "Inactivo";
        public string EstadoColor => IsActivo ? "#22C55E" : "#94A3B8"; 
    }
}