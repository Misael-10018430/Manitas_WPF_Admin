using System;

namespace Manitas.Logic.DTOs // <--- Verifica que el nombre de la carpeta sea "DTOs"
{
    public class UsuarioDTO
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }        // 📱 Dato Real
        public string RolNombre { get; set; }
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Datos de Manitas
        public string OficioDescripcion { get; set; } // Lo que describen en la web
        public string FotoPerfilUrl { get; set; }
        public string IneFrenteUrl { get; set; }
        public string IneReversoUrl { get; set; }
        public string DocumentoExtraUrl { get; set; }
    }
}