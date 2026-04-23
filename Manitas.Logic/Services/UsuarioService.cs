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

        // helper privado para construir URLs
        private string BuildUrl(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa)) return null;

            if (rutaRelativa.StartsWith("/App_Data/"))
                return BaseUrl + "/Archivo/Servir?ruta=" + Uri.EscapeDataString(rutaRelativa);

            return BaseUrl + rutaRelativa;
        }
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
                        ServiciosCompletados = esManitas
                            ? (perfilManitas.servicios_completados_total) : 0,
                        CalificacionesNegativas = esManitas
                            ? (perfilManitas.calificaciones_neg_consecutivas) : 0,
                        FotoPerfilUrl = esManitas ? BuildUrl(perfilManitas.foto_perfil_url) : null,
                        IneFrenteUrl = esManitas ? BuildUrl(perfilManitas.ine_frente_url) : null,
                        IneReversoUrl = esManitas ? BuildUrl(perfilManitas.ine_reverso_url) : null,
                        DocumentoExtraUrl = esManitas ? BuildUrl(perfilManitas.comprobante_url) : null
                    };
                }).ToList();
            }
        }

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
    }
}