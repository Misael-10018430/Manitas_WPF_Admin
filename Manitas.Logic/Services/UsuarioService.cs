using System;
using System.Collections.Generic;
using System.Linq;
using Manitas.Data.Models;
using Manitas.Logic.DTOs;
using BCrypt.Net;

namespace Manitas.Logic.Services
{
    public class UsuarioService
    {
        private const string BaseUrl = "http://localhost:44355";
        private string BuildUrl(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa)) return null;
            if (rutaRelativa.StartsWith("https://") || rutaRelativa.StartsWith("http://"))
                return rutaRelativa;
            if (rutaRelativa.StartsWith("/App_Data/"))
                return BaseUrl + "/Archivo/Servir?ruta=" + Uri.EscapeDataString(rutaRelativa);
            return BaseUrl + rutaRelativa;
        }
        // helper privado para construir URLs
        //Funcionalidad : Construye la URL completa para acceder a un recurso, manejando rutas relativas y casos especiales como archivos en App_Data, lo que facilita la gestión de recursos y mejora la experiencia del usuario al mostrar imágenes o documentos relacionados con el perfil del usuario.
        // private string BuildUrl(string rutaRelativa)
        // {
        //    if (string.IsNullOrWhiteSpace(rutaRelativa)) return null;

        //    if (rutaRelativa.StartsWith("/App_Data/"))
        //        return BaseUrl + "/Archivo/Servir?ruta=" + Uri.EscapeDataString(rutaRelativa);

        //    return BaseUrl + rutaRelativa;
        //  }
        //Funcionalidad: Autentica a un usuario verificando su correo y contraseña, asegurando que solo los usuarios con roles internos activos puedan acceder al sistema, lo que mejora la seguridad de la aplicación al restringir el acceso a funciones administrativas solo a usuarios autorizados.
        public UsuarioDTO Autenticar(string correo, string password)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var usuarioEncontrado = db.usuarios
                    .FirstOrDefault(u => u.correo == correo && u.activo == true);
                if (usuarioEncontrado == null) return null;
                // Verifica si el hash es BCrypt o texto plano (legacy)
                bool passwordValida = false;
                string hash = usuarioEncontrado.contrasena_hash ?? "";
                if (hash.StartsWith("$2a$") || hash.StartsWith("$2b$"))
                    passwordValida = BCrypt.Net.BCrypt.Verify(password, hash);
                else
                    passwordValida = hash == password; // legacy temporal
                if (!passwordValida) return null;
                // Solo roles internos pueden entrar al WPF Admin
                var rol = usuarioEncontrado.usuario_roles
                    .FirstOrDefault(r => r.activo == true);

                string nombreRol = rol?.role?.nombre ?? "";
                bool esInterno = nombreRol == "administrador" ||
                                 nombreRol == "agente_operativo" ||
                                 nombreRol == "agente_disputas";

                if (!esInterno) return null;
                return new UsuarioDTO
                {
                    Id = usuarioEncontrado.id,
                    Correo = usuarioEncontrado.correo,
                    NombreCompleto = usuarioEncontrado.nombre_completo,
                    RolNombre = nombreRol
                };
            }
        }
        /// <summary>
        /// Obtiene los usuarios que ya tienen el rol de 'Manita' con su info completa
        /// </summary>
        /// 
        // Funcionalidad: Recupera una lista de usuarios con el rol de 'Manita' desde la base de datos, incluyendo detalles como su nombre completo, correo, teléfono, oficio, experiencia y enlaces a sus fotos y documentos, lo que permite a los administradores gestionar y revisar la información de los manitas registrados en la plataforma de manera eficiente.
        public List<UsuarioDTO> ObtenerManitas()
        {
            List<UsuarioDTO> listaDTO = new List<UsuarioDTO>();
            using (var db = new Manitas_DBPilotoEntities())
            {
                var manitasBD = db.usuarios
                    .Where(u => u.activo == true &&
                                u.usuario_roles.Any(r => r.role.nombre == "Manita"))
                    .ToList();
                foreach (var u in manitasBD)
                {
                    var perfil = u.perfiles_manitas.FirstOrDefault();
                    var registroServicio = perfil?.manitas_servicios.FirstOrDefault();
                    listaDTO.Add(new UsuarioDTO
                    {
                        Id = u.id,
                        NombreCompleto = u.nombre_completo,
                        Correo = u.correo,
                        Telefono = u.telefono,
                        OficioDescripcion = registroServicio?.tipos_servicio?.nombre ?? "Oficio no asignado",
                        Apodo = perfil?.apodo,
                        AniosExperiencia = perfil?.anios_experiencia,
                        DisponibilidadHorario = perfil?.disponibilidad_texto,
                        ServiciosCompletados = perfil?.servicios_completados_total ?? 0,
                        CalificacionesNegativas = perfil?.calificaciones_neg_consecutivas ?? 0,
                        FotoPerfilUrl = BuildUrl(perfil?.foto_perfil_url),
                        IneFrenteUrl = BuildUrl(perfil?.ine_frente_url),
                        IneReversoUrl = BuildUrl(perfil?.ine_reverso_url),
                        DocumentoExtraUrl = BuildUrl(perfil?.comprobante_url)
                    });
                }
            }
            return listaDTO;
        }
        /// <summary>
        /// Cambia el rol de un usuario a 'Manita' en la base de datos SQL
        /// </summary>
        /// 
        // Funcionalidad: Actualiza el rol de un usuario específico a 'Manita' en la base de datos, activando su cuenta y cambiando su estado a "Activo
        public bool AprobarUsuarioComoManita(Guid usuarioId)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var relacionActual = db.usuario_roles.FirstOrDefault(ur => ur.usuario_id == usuarioId);
                var rolManita = db.roles.FirstOrDefault(r => r.nombre == "manitas");
                if (relacionActual != null && rolManita != null)
                {
                    relacionActual.rol_id = rolManita.id;
                    relacionActual.activo = true;
                    var perfil = db.perfiles_manitas.FirstOrDefault(p => p.usuario_id == usuarioId);
                    if (perfil != null)
                        perfil.estado = "Activo";

                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// Desactiva a un usuario en la base de datos (Borrado Lógico)
        /// </summary>
        /// 
        // Funcionalidad: Desactiva a un usuario específico en la base de datos mediante un borrado lógico, estableciendo su relación de rol como inactiva, lo que impide que el usuario acceda a la plataforma sin eliminar completamente su información, permitiendo así una posible reactivación futura o conservación de datos para auditorías.
        public bool RechazarUsuario(Guid usuarioId)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var relacion = db.usuario_roles.FirstOrDefault(ur => ur.usuario_id == usuarioId);
                if (relacion != null)
                {
                    relacion.activo = false;
                    return db.SaveChanges() > 0;
                }
                return false;
            }
        }
        // Funcionalidad: Actualiza el estado de un usuario con rol de 'Manita' en la base de datos, permitiendo establecer un nuevo estado (como "Rechazado" o "En revisión") y registrar un motivo de rechazo, lo que facilita la gestión de las solicitudes de manitas y proporciona transparencia sobre las decisiones tomadas respecto a su aprobación o rechazo en la plataforma.
        public bool ActualizarEstadoManitas(Guid usuarioId, string nuevoEstado, string motivo)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var perfil = db.perfiles_manitas.FirstOrDefault(p => p.usuario_id == usuarioId);
                if (perfil != null)
                {
                    perfil.estado = nuevoEstado;
                    perfil.motivo_rechazo = motivo;
                    return db.SaveChanges() > 0;
                }
                return false;
            }
        }
        // Funcionalidad: Actualiza el estado activo de un usuario específico en la base de datos, permitiendo activar o desactivar su cuenta según sea necesario, lo que facilita la gestión de usuarios y el control de acceso a la plataforma sin eliminar completamente su información.
        public void ActualizarEstadoUsuario(Guid usuarioId, bool activo)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var relacion = db.usuario_roles.FirstOrDefault(ur => ur.usuario_id == usuarioId);
                if (relacion != null)
                {
                    relacion.activo = activo;
                    db.SaveChanges();
                }
            }
        }
        // Funcionalidad: Obtiene una lista de los usuarios más recientes con roles de 'cliente' o 'manitas' desde la base de datos, ordenados por fecha de registro, lo que permite a los administradores visualizar rápidamente las nuevas incorporaciones a la plataforma y gestionar su información de manera eficiente.
        public List<UsuarioDTO> ObtenerActividadReciente()
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                return db.usuarios
                    .Where(u => u.usuario_roles.Any(r =>
                        r.role.nombre == "cliente" ||
                        r.role.nombre == "manitas"))
                    .OrderByDescending(u => u.fecha_registro)
                    .Take(6)
                    .AsEnumerable()
                    .Select(u => new UsuarioDTO
                    {
                        NombreCompleto = u.nombre_completo,
                        RolNombre = u.usuario_roles.FirstOrDefault()?.role?.nombre ?? "Sin Rol",
                        Telefono = u.telefono,
                        FotoPerfilUrl = u.perfiles_manitas.FirstOrDefault()?.foto_perfil_url,
                        IsActivo = u.usuario_roles.FirstOrDefault().activo == true
                    }).ToList();
            }
        }
        // Funcionalidad: Recupera una lista de usuarios con roles
        public List<UsuarioDTO> ObtenerUsuariosGlobal(string busqueda, string filtroRol)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                // Solo clientes y manitas — excluye roles internos
                var query = db.usuarios.Where(u => u.usuario_roles.Any(r =>
                    r.role.nombre == "cliente" ||
                    r.role.nombre == "manitas"));

                return query.ToList().Select(u =>
                {
                    var rol = u.usuario_roles.FirstOrDefault();
                    var perfilManitas = u.perfiles_manitas.FirstOrDefault();
                    bool esManitas = perfilManitas != null;

                    return new UsuarioDTO
                    {
                        Id = u.id,
                        NombreCompleto = u.nombre_completo,
                        Correo = u.correo,
                        Telefono = u.telefono,
                        FechaRegistro = u.fecha_registro,
                        IsActivo = rol?.activo == true,
                        Estado = rol?.activo == true ? "Activo" : "Inactivo",
                        RolNombre = esManitas
                            ? (rol?.role?.nombre ?? "manitas")
                            : "Cliente",
                        OficioNombre = esManitas
                            ? (perfilManitas.manitas_servicios.FirstOrDefault()?.tipos_servicio?.nombre
                               ?? "Oficio no seleccionado")
                            : "Cliente",
                        OficioDescripcion = esManitas
                            ? (perfilManitas.descripcion ?? "Sin descripción")
                            : null,
                        Apodo = esManitas ? perfilManitas.apodo : null,
                        AniosExperiencia = esManitas ? perfilManitas.anios_experiencia : null,
                        DisponibilidadHorario = esManitas ? perfilManitas.disponibilidad_texto : null,
                        // ✅ Después
                        ServiciosCompletados = esManitas
                        ? db.servicios.Count(s => s.manitas_id == u.id && s.estado == "completado") : 0,
                        CalificacionesNegativas = esManitas
                        ? db.calificaciones_manitas.Count(c => c.manitas_id == u.id && c.es_negativa == true) : 0,
                        FotoPerfilUrl = esManitas ? BuildUrl(perfilManitas.foto_perfil_url) : null,
                        IneFrenteUrl = esManitas ? BuildUrl(perfilManitas.ine_frente_url) : null,
                        IneReversoUrl = esManitas ? BuildUrl(perfilManitas.ine_reverso_url) : null,
                        DocumentoExtraUrl = esManitas ? BuildUrl(perfilManitas.comprobante_url) : null
                    };
                }).ToList();
            }
        }
        // Funcionalidad: Obtiene una lista de usuarios que tienen solicitudes pendientes para convertirse en 'Manita', incluyendo detalles relevantes como su nombre completo, correo, teléfono, oficio solicitado, experiencia y enlaces a sus fotos y documentos, lo que permite a los administradores revisar y gestionar
        public List<UsuarioDTO> ObtenerSolicitudesPendientes()
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                return db.usuarios
                .Where(u => u.perfiles_manitas.Any(p => p.estado == "en_solicitud"))
                .ToList()
                .Select(u =>
                {
        var perfil = u.perfiles_manitas.FirstOrDefault();

        return new UsuarioDTO
        {
            Id = u.id,
            NombreCompleto = u.nombre_completo,
            Telefono = u.telefono,
            Correo = u.correo,
            RolNombre = "Manita Pendiente",
            OficioNombre = perfil?.manitas_servicios.FirstOrDefault()?.tipos_servicio?.nombre
                           ?? "Oficio no seleccionado",
            OficioDescripcion = perfil?.descripcion ?? "Sin descripción",
            Apodo = perfil?.apodo,
            AniosExperiencia = perfil?.anios_experiencia,
            DisponibilidadHorario = perfil?.disponibilidad_texto,
            ServiciosCompletados = perfil?.servicios_completados_total ?? 0,
            CalificacionesNegativas = perfil?.calificaciones_neg_consecutivas ?? 0,
            FotoPerfilUrl = BuildUrl(perfil?.foto_perfil_url),
            IneFrenteUrl = BuildUrl(perfil?.ine_frente_url),
            IneReversoUrl = BuildUrl(perfil?.ine_reverso_url),
            DocumentoExtraUrl = BuildUrl(perfil?.comprobante_url)
        };
    }).ToList();
            }
        }
        // Funcionalidad: Cambia el estado activo de un usuario específico en la base de datos, permitiendo activar o desactivar su cuenta según sea necesario, lo que facilita la gestión de usuarios y el control de acceso a la plataforma sin eliminar completamente su información.
        public bool CambiarEstatusUsuario(Guid usuarioId, bool nuevoEstado)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var relacion = db.usuario_roles.FirstOrDefault(ur => ur.usuario_id == usuarioId);
                if (relacion != null)
                {
                    relacion.activo = nuevoEstado;
                    return db.SaveChanges() > 0;
                }
                return false;
            }
        }
        // Funcionalidad: Obtiene una lista de usuarios con el rol de 'cliente' que no

        public List<UsuarioDTO> ObtenerClientes(string busqueda = "")
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var query = db.usuarios.Where(u =>
                    u.usuario_roles.Any(r => r.role.nombre == "cliente") &&
                    !u.perfiles_manitas.Any()); // Solo clientes puros, sin perfil manitas

                if (!string.IsNullOrEmpty(busqueda))
                    query = query.Where(u => u.nombre_completo.Contains(busqueda) ||
                                             u.correo.Contains(busqueda));
                return query.ToList().Select(u =>
                {
                    var rol = u.usuario_roles.FirstOrDefault();
                    var perfilCliente = u.perfiles_clientes.FirstOrDefault();
                    return new UsuarioDTO
                    {
                        Id = u.id,
                        NombreCompleto = u.nombre_completo,
                        Correo = u.correo,
                        Telefono = u.telefono,
                        FechaRegistro = u.fecha_registro,
                        IsActivo = rol?.activo ?? false,
                        FotoPerfilUrl = null, // clientes no tienen foto en esta tabla
                                              // Campos de perfiles_clientes
                        ServiciosCompletados = perfilCliente?.inasistencias_consecutivas ?? 0,
                        CalificacionesNegativas = perfilCliente?.suspensiones_en_6_meses ?? 0,
                        Estado = perfilCliente?.estado ?? (rol?.activo == true ? "Activo" : "Inactivo")
                    };
                }).ToList();
            }
        }
        // Funcionalidad: Obtiene una lista de usuarios con roles internos (administrador, agente operativo, agente de disputas) desde la base de datos, permitiendo filtrar por nombre completo o correo, lo que facilita a los administradores gestionar y revisar la información de los usuarios que tienen acceso a funciones administrativas en la plataforma.
        public List<UsuarioDTO> ObtenerUsuariosSistema(string busqueda)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var query = db.usuarios.Where(u => u.usuario_roles.Any(r =>
                    r.activo && (
                        r.role.nombre == "administrador" ||
                        r.role.nombre == "agente_operativo" ||
                        r.role.nombre == "agente_disputas"
                    )
                ));
                if (!string.IsNullOrEmpty(busqueda))
                {
                    string b = busqueda.ToLower();
                    query = query.Where(u => u.nombre_completo.ToLower().Contains(b) ||
                                             u.correo.ToLower().Contains(b));
                }
                return query.ToList().Select(u => new UsuarioDTO
                {
                    Id = u.id,
                    Username = u.correo,
                    NombreCompleto = u.nombre_completo,
                    Correo = u.correo,
                    RolNombre = u.usuario_roles.FirstOrDefault(r => r.activo)?.role?.nombre ?? "Sin Rol",
                    IsActivo = u.usuario_roles.FirstOrDefault(r => r.activo)?.activo ?? false,
                    UltimaConexion = u.fecha_registro
                }).ToList();
            }
        }
        // Funcionalidad: Actualiza la contraseña de un usuario específico en la base de datos, aplicando un hash seguro utilizando BCrypt, lo que mejora la seguridad de las cuentas de usuario al almacenar contraseñas de manera segura y protegerlas contra accesos no autorizados.
        public bool ActualizarPassword(Guid usuarioId, string nuevaPassword)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var user = db.usuarios.Find(usuarioId);
                if (user == null) return false;
                user.contrasena_hash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
                return db.SaveChanges() > 0;
            }
        }
        // Funcionalidad: Actualiza el rol de un usuario específico en la base de datos, permitiendo cambiar su rol a uno nuevo (como "administrador", "agente_operativo" o "agente_disputas"), lo que facilita la gestión de permisos y accesos de los usuarios dentro de la plataforma según sus responsabilidades y funciones asignadas.
        public bool ActualizarRolSistema(Guid usuarioId, string nuevoRolNombre)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var rol = db.roles.FirstOrDefault(r => r.nombre.ToLower() == nuevoRolNombre.ToLower());
                var relacionActual = db.usuario_roles.FirstOrDefault(ur => ur.usuario_id == usuarioId && ur.activo);
                if (rol == null || relacionActual == null) return false;
                relacionActual.rol_id = rol.id;
                return db.SaveChanges() > 0;
            }
        }
        // Funcionalidad: Crea un nuevo usuario con un rol específico en la base de datos, asegurando que el proceso de creación y asignación de rol se realice de manera atómica mediante una transacción, lo que garantiza la integridad de los datos y evita inconsistencias en caso de errores durante la operación.
        public bool CrearUsuarioStaff(usuario nuevoU, string nombreRol)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.usuarios.Add(nuevoU);
                        db.SaveChanges();

                        var rol = db.roles.FirstOrDefault(r => r.nombre.ToLower() == nombreRol.ToLower());
                        if (rol != null)
                        {
                            db.usuario_roles.Add(new usuario_roles
                            {
                                id = Guid.NewGuid(),
                                usuario_id = nuevoU.id,
                                rol_id = rol.id,
                                activo = true,
                                fecha_asignacion = DateTime.Now
                            });
                            db.SaveChanges();
                            transaction.Commit();
                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }

        }
        // Funcionalidad: Verifica si un correo electrónico ya existe en la base de datos, lo que ayuda a prevenir la creación de cuentas duplicadas y mejora la experiencia del usuario al proporcionar retroalimentación inmediata sobre la disponibilidad del correo durante el proceso de registro.
        public bool CorreoYaExiste(string correo)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                return db.usuarios.Any(u => u.correo.ToLower() == correo.ToLower().Trim());
            }
        }
        // Funcionalidad: Elimina un usuario específico de la base de datos, asegurando que solo los usuarios sin roles administrativos puedan
        public bool EliminarUsuarioInterno(Guid usuarioId)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var rol = db.usuario_roles.FirstOrDefault(ur => ur.usuario_id == usuarioId);
                if (rol == null) return false;
                // No permite eliminar administradores
                if (rol.role.nombre == "administrador") return false;
                db.usuario_roles.Remove(rol);
                var usuario = db.usuarios.Find(usuarioId);
                if (usuario != null) db.usuarios.Remove(usuario);
                return db.SaveChanges() > 0;
            }
        }
    }
}