using System;
namespace Manitas.Logic.DTOs
{
    public class UsuarioDTO
    {
        public Guid Id { get; set; }
        public string Correo { get; set; }
        public string NombreCompleto { get; set; }
        public string RolNombre { get; set; }
        public string Ubicacion { get; set; }
        public string OficioNombre { get; set; }
        public string RutaIdentificacion { get; set; }
    }
}