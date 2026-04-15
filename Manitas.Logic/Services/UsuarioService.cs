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
                        Correo = u.correo,
                        NombreCompleto = u.nombre_completo,
                        RolNombre = "Manita",
                        Ubicacion = perfil?.estado ?? "Sin ubicación",
                        OficioNombre = registroServicio?.tipos_servicio?.nombre ?? "Oficio pendiente",
                        RutaIdentificacion = perfil?.ine_frente_url ?? ""
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
            try
            {
                using (var db = new Manitas_DBPilotoEntities())
                {
                    var usuario = db.usuarios.FirstOrDefault(u => u.id == usuarioId);
                    if (usuario != null)
                    {
                        var rolManita = db.roles.FirstOrDefault(r => r.nombre == "Manita");
                        if (rolManita != null)
                        {
                            var relacionActual = db.usuario_roles.FirstOrDefault(ur => ur.usuario_id == usuarioId);
                            if (relacionActual != null)
                            {
                                relacionActual.rol_id = rolManita.id;
                            }
                            else
                            {
                                db.usuario_roles.Add(new usuario_roles
                                {
                                    usuario_id = usuarioId,
                                    rol_id = rolManita.id
                                });
                            }
                            db.SaveChanges();
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en la persistencia de datos: " + ex.Message);
            }
        }
        /// <summary>
        /// Desactiva a un usuario en la base de datos (Borrado Lógico)
        /// </summary>
        public bool RechazarUsuario(Guid usuarioId)
        {
            try
            {
                using (var db = new Manitas_DBPilotoEntities())
                {
                    var usuario = db.usuarios.FirstOrDefault(u => u.id == usuarioId);

                    if (usuario != null)
                    {
                        usuario.activo = false;

                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al procesar el rechazo en SQL: " + ex.Message);
            }
        }
    }
}