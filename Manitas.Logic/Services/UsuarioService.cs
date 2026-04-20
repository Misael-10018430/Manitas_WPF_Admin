using System;
using System.Collections.Generic;
using System.Linq;
using Manitas.Data.Models;
using Manitas.Logic.DTOs;
namespace Manitas.Logic.Services
{
    public class UsuarioService
    {
        /// <summary>
        /// Valida las credenciales del administrador en el Login
        /// </summary>
        public UsuarioDTO Autenticar(string correo, string password)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var usuarioEncontrado = db.usuarios
                    .FirstOrDefault(u => u.correo == correo && u.contrasena_hash == password && u.activo == true);
                if (usuarioEncontrado != null)
                {
                    return new UsuarioDTO
                    {
                        Id = usuarioEncontrado.id,
                        Correo = usuarioEncontrado.correo,
                        NombreCompleto = usuarioEncontrado.nombre_completo,
                        RolNombre = usuarioEncontrado.usuario_roles.FirstOrDefault()?.role.nombre ?? "Sin Rol"
                    };
                }
                return null;
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
                        OficioDescripcion = registroServicio?.tipos_servicio?.nombre ?? "Oficio no asignado"
                    });
                }
            }
            return listaDTO;
        }
        /// <summary>
        /// Cambia el rol de un usuario a 'Manita' en la base de datos SQL
        /// </summary>
        /// 
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
                    {
                        perfil.estado = "Activo"; 
                    }

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
        public List<UsuarioDTO> ObtenerActividadReciente()
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                return db.usuarios
                    .Where(u => u.usuario_roles.Any(r => r.role.nombre != "administrador" && r.role.nombre != "encargado"))
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
        // Rutas de respaldo para cuando el usuario no ha subido fotos
        private readonly string _noImage = "pack://application:,,,/Manitas_WPF_Admin;component/Assets/Images/no-image.png";
        private readonly string _noDoc = "pack://application:,,,/Manitas_WPF_Admin;component/Assets/Images/no-document.png";
        public List<UsuarioDTO> ObtenerUsuariosGlobal(string busqueda, string filtroRol)
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                var query = db.usuarios.Where(u => u.usuario_roles.Any(r => r.role.nombre != "administrador"));
                return query.ToList().Select(u => {
                    var perfil = u.perfiles_manitas.FirstOrDefault();
                    var rol = u.usuario_roles.FirstOrDefault();
                    return new UsuarioDTO
                    {
                        Id = u.id,
                        NombreCompleto = u.nombre_completo,
                        Correo = u.correo,
                        Telefono = u.telefono,
                        RolNombre = rol?.role?.nombre ?? "Sin Rol",
                        Estado = rol?.activo == true ? "Activo" : "Inactivo",
                        IsActivo = rol?.activo == true,
                        FechaRegistro = u.fecha_registro,
                        OficioDescripcion = perfil?.descripcion ?? "Sin oficio",
                        OficioNombre = perfil?.manitas_servicios.FirstOrDefault()?.tipos_servicio?.nombre
                                       ?? (u.usuario_roles.Any(r => r.role.nombre.ToLower().Contains("manita"))
                                           ? "Oficio no seleccionado"
                                           : "Cliente"),
                        FotoPerfilUrl = string.IsNullOrWhiteSpace(perfil?.foto_perfil_url) ? _noImage : perfil.foto_perfil_url,
                        IneFrenteUrl = string.IsNullOrWhiteSpace(perfil?.ine_frente_url) ? _noImage : perfil.ine_frente_url,
                        IneReversoUrl = string.IsNullOrWhiteSpace(perfil?.ine_reverso_url) ? _noImage : perfil.ine_reverso_url,
                        DocumentoExtraUrl = string.IsNullOrWhiteSpace(perfil?.comprobante_url) ? _noDoc : perfil.comprobante_url
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
                    .Select(u => {
                        var perfil = u.perfiles_manitas.FirstOrDefault();

                        return new UsuarioDTO
                        {
                            Id = u.id,
                            NombreCompleto = u.nombre_completo,
                            Telefono = u.telefono,
                            Correo = u.correo,
                            RolNombre = "Manita Pendiente",
                            OficioNombre = perfil?.manitas_servicios.FirstOrDefault()?.tipos_servicio?.nombre ?? "Oficio no seleccionado",
                            OficioDescripcion = perfil?.descripcion ?? "Sin descripción",
                            FotoPerfilUrl = string.IsNullOrWhiteSpace(perfil?.foto_perfil_url) ? _noImage : perfil.foto_perfil_url,
                            IneFrenteUrl = string.IsNullOrWhiteSpace(perfil?.ine_frente_url) ? _noImage : perfil.ine_frente_url,
                            IneReversoUrl = string.IsNullOrWhiteSpace(perfil?.ine_reverso_url) ? _noImage : perfil.ine_reverso_url,
                            DocumentoExtraUrl = string.IsNullOrWhiteSpace(perfil?.comprobante_url) ? _noDoc : perfil.comprobante_url
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
    }
}