using System;
using Manitas.Logic.DTOs;
namespace Manitas.Logic.Security 
{
    /// <summary>
    /// Clase estática para persistir los datos del usuario logueado 
    /// en toda la ejecución de la aplicación.
    /// </summary>
    public static class SesionUsuario
    {
        public static UsuarioDTO UsuarioActual { get; set; }
        public static bool EsAdmin()
            => UsuarioActual?.RolNombre == "administrador" || UsuarioActual?.RolNombre == "Super Admin";
        public static void Logout()
        {
            UsuarioActual = null;
        }
    }
}