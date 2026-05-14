using System;
namespace Manitas.Logic.DTOs
{
    public class UsuarioDTO
    {
        //Funcionalidad de este DTO: estar en la sesión del usuario logueado, por lo que se pueden agregar propiedades adicionales que no estén en la base de datos pero que sean útiles para la aplicación.
        public string Username { get; set; }
        //Se pueden agregar propiedades adicionales como Apodo, AniosExperiencia, DisponibilidadHorario, etc. para mostrar en el perfil del usuario o en la lista de usuarios.
        public DateTime? UltimaConexion { get; set; }
        //Propiedades básicas del usuario
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string RolNombre { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string OficioDescripcion { get; set; }
        public string OficioNombre { get; set; }
        //Propiedad para indicar si el usuario está activo o inactivo, que se puede usar para mostrar un estado visual en la interfaz de usuario.
        public bool IsActivo { get; set; }
        private string _fotoPerfilUrl;
        private string _ineFrenteUrl;
        private string _ineReversoUrl;
        private string _documentoExtraUrl;
        //Propiedades para las URLs de las imágenes, con lógica para retornar null si no hay una URL válida, lo que facilita la lógica en la interfaz de usuario para mostrar imágenes predeterminadas o mensajes cuando no hay una imagen disponible.
        public string FotoPerfilUrl
        {
            get => string.IsNullOrWhiteSpace(_fotoPerfilUrl) ? null : _fotoPerfilUrl;
            set => _fotoPerfilUrl = value;
        }
        // Las siguientes propiedades siguen la misma lógica para retornar null si no hay una URL válida, lo que ayuda a mantener la consistencia en la forma en que se manejan las imágenes en la aplicación.
        public string IneFrenteUrl
        {
            get => string.IsNullOrWhiteSpace(_ineFrenteUrl) ? null : _ineFrenteUrl;
            set => _ineFrenteUrl = value;
        }
        // La propiedad IneReversoUrl también sigue la misma lógica para manejar la URL de la imagen del reverso del INE, lo que permite a la interfaz de usuario mostrar una imagen predeterminada o un mensaje cuando no hay una imagen disponible.
        public string IneReversoUrl
        {
            get => string.IsNullOrWhiteSpace(_ineReversoUrl) ? null : _ineReversoUrl;
            set => _ineReversoUrl = value;
        }
        // La propiedad DocumentoExtraUrl sigue la misma lógica para manejar la URL de cualquier documento adicional que el usuario pueda haber subido, lo que facilita la gestión de documentos en la aplicación y mejora la experiencia del usuario al mostrar información relevante de manera clara.
        public string DocumentoExtraUrl
        {
            get => string.IsNullOrWhiteSpace(_documentoExtraUrl) ? null : _documentoExtraUrl;
            set => _documentoExtraUrl = value;
        }
        // Propiedad calculada para mostrar el estado del usuario como texto, lo que facilita la comprensión del estado del usuario en la interfaz de usuario sin tener que interpretar un valor booleano.
        public string EstadoTexto => IsActivo ? "Activo" : "Inactivo";
        public string EstadoColor => IsActivo ? "#22C55E" : "#94A3B8";
        // Propiedad calculada para mostrar las iniciales del usuario, lo que puede ser útil para mostrar un avatar con las iniciales del usuario cuando no hay una foto de perfil disponible, mejorando la experiencia visual en la interfaz de usuario.
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
        // Propiedad calculada para asignar un color basado en el rol del usuario, lo que puede ser útil para diferenciar visualmente a los usuarios según su rol en la interfaz de usuario, mejorando la usabilidad y la experiencia del usuario.
        public string ColorRol
        {
            get
            {
                string rol = RolNombre?.ToLower() ?? "";
                if (rol == "administrador") return "#EF4444";
                if (rol == "agente_operativo") return "#F59E0B";
                if (rol == "agente_disputas") return "#10B981";
                return "#6366F1";
            }
        }
        // Propiedad calculada para determinar si el usuario tiene una foto de perfil válida, lo que puede ser útil para mostrar una imagen predeterminada o un mensaje cuando no hay una foto de perfil disponible, mejorando la experiencia del usuario al proporcionar información visual clara sobre la disponibilidad de la foto de perfil.
        public bool TieneFotoValida =>
        !string.IsNullOrWhiteSpace(_fotoPerfilUrl) &&
        !_fotoPerfilUrl.Contains("no-image") &&
        !_fotoPerfilUrl.StartsWith("pack://");
        public string Apodo { get; set; }
        public int? AniosExperiencia { get; set; }
        public string DisponibilidadHorario { get; set; }
        public int ServiciosCompletados { get; set; }
        public int CalificacionesNegativas { get; set; }
        // Propiedad calculada para mostrar los años de experiencia como texto, lo que puede ser útil para mostrar esta información de manera clara y legible en la interfaz de usuario, mejorando la comprensión del nivel de experiencia del usuario.
        public string AniosExperienciaTexto => AniosExperiencia.HasValue
            ? $"{AniosExperiencia} año(s) de experiencia"
            : "Experiencia no especificada";
        // Propiedad calculada para mostrar la disponibilidad de horario como texto, lo que puede ser útil para mostrar esta información de manera clara y legible en la interfaz de usuario, mejorando la comprensión de la disponibilidad del usuario.
        public int InasistenciasConsecutivas { get; set; }
        public int SuspensionesEnSeisMeses { get; set; }

    }
}