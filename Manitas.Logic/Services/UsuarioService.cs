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
            try
            {
                using (var db = new Manitas_DBPilotoEntities())
                {
                    var relacionActual = db.usuario_roles.FirstOrDefault(ur => ur.usuario_id == usuarioId);
                    if (relacionActual != null)
                    {
                        var rolManita = db.roles.FirstOrDefault(r => r.nombre == "manitas");
                        if (rolManita != null) relacionActual.rol_id = rolManita.id;
                        relacionActual.activo = true;
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al aprobar: " + ex.Message);
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
                    .Where(u => u.usuario_roles.Any(r => r.role.nombre != "administrador" && r.role.nombre != "encargado")) // 👈 FILTRO CLAVE
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
                var query = db.usuarios.Where(u => u.usuario_roles.Any(r => r.role.nombre != "administrador"));
                return query.ToList().Select(u => new UsuarioDTO 
                {
                    Id = u.id,
                    NombreCompleto = u.nombre_completo,
                    Correo = u.correo,
                    Telefono = u.telefono,
                    RolNombre = u.usuario_roles.FirstOrDefault()?.role?.nombre ?? "Sin Rol",
                    Estado = u.usuario_roles.FirstOrDefault()?.activo == true ? "Activo" : "Inactivo",
                    FechaRegistro = u.fecha_registro,
                    OficioDescripcion = u.perfiles_manitas.FirstOrDefault()?.descripcion ?? "Sin oficio",
                    FotoPerfilUrl = u.perfiles_manitas.FirstOrDefault()?.foto_perfil_url,
                    IneFrenteUrl = u.perfiles_manitas.FirstOrDefault()?.ine_frente_url,
                    IneReversoUrl = u.perfiles_manitas.FirstOrDefault()?.ine_reverso_url,
                    DocumentoExtraUrl = u.perfiles_manitas.FirstOrDefault()?.comprobante_url
                }).ToList();
            }
        }
        public List<UsuarioDTO> ObtenerSolicitudesPendientes()
        {
            using (var db = new Manitas_DBPilotoEntities())
            {
                return db.usuarios
                    .Where(u => u.usuario_roles.Any(r => r.role.nombre == "manitas" && r.activo == false))
                    .AsEnumerable()
                    .Select(u => new UsuarioDTO
                    {
                        Id = u.id, 
                        NombreCompleto = u.nombre_completo,
                        RolNombre = "Manita Pendiente",
                        Telefono = u.telefono,
                        FotoPerfilUrl = u.perfiles_manitas.FirstOrDefault()?.foto_perfil_url,
                        IneFrenteUrl = u.perfiles_manitas.FirstOrDefault()?.ine_frente_url,
                        IneReversoUrl = u.perfiles_manitas.FirstOrDefault()?.ine_reverso_url,
                        DocumentoExtraUrl = u.perfiles_manitas.FirstOrDefault()?.comprobante_url,
                        OficioDescripcion = u.perfiles_manitas.FirstOrDefault()?.descripcion ?? "Pendiente de asignar"
                    }).ToList();
            }
        }
    }
}