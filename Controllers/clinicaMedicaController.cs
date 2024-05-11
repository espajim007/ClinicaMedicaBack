using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using AnalisisIClinicaMedicaBack.Provider;
using AnalisisIClinicaMedicaBack.Models;
using MySql.Data.MySqlClient.Memcached;
using AnalisisIClinicaMedicaBack.Requests;
using Microsoft.Win32;

namespace AnalisisIClinicaMedicaBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class clinicaMedicaController : ControllerBase
    {
        private readonly DatabaseProvider db;

        public clinicaMedicaController(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ConnectionString");
            db = new DatabaseProvider(connectionString);
        }
        


        //Catalogos

        [HttpGet("catalogos/usuarios")]
        public IActionResult GetUsuarios()
        {
            try
            {
                var query = @"SELECT a.id_usuario, b.nombre AS rol, a.nombre, a.email, a.estado
                             FROM usuario a
                            INNER JOIN rol b ON a.id_rol = b.id_rol
                            ORDER BY a.id_usuario";
                var resultado = db.ExecuteQuery(query);
                var usuarios = resultado.AsEnumerable().Select(row => new usuario
                {
                    id_usuario = Convert.ToInt32(row["id_usuario"]),
                    rol = row["rol"].ToString(),
                    nombre = row["nombre"].ToString(),
                    email = row["email"].ToString(),
                    estado = Convert.ToBoolean(row["estado"])
                }).ToList();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {

                return BadRequest(ex);
            }
            
        }

        [HttpPost("catalogos/editar-usuario")]
        public IActionResult editarUsuario([FromBody] registro_usuario editarUsuario)
        {
            try
            {

                    var queryActualizar = $"UPDATE usuario SET id_rol = '{editarUsuario.id_rol}',nombre ='{editarUsuario.nombre}',email ='{editarUsuario.email}', " +
                        $"estado ='{editarUsuario.estado}' WHERE id_usuario = {editarUsuario.id_usuario}";
                    var actualizar = db.ExecuteQuery(queryActualizar);
                    return Ok();

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nuevo-usuario")]
        public IActionResult nuevoUsuario([FromBody] registro_usuario nuevoUsuario)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT email FROM usuario WHERE email = '{nuevoUsuario.email}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO usuario (id_rol, nombre, contrasenia, email, estado) VALUES ( '{nuevoUsuario.id_rol}','{nuevoUsuario.nombre}', " +
                                                                                                            $"'{nuevoUsuario.contrasenia}', '{nuevoUsuario.email}', 0)";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado ese correo");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpGet("catalogos/roles")]
        public IActionResult GetRoles()
        {
            try
            {
                var query = @"SELECT *
                             FROM rol";
                var resultado = db.ExecuteQuery(query);
                var roles = resultado.AsEnumerable().Select(row => new rol
                {
                    id_rol = Convert.ToInt32(row["id_rol"]),
                    nombre = row["nombre"].ToString(),
                }).ToList();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [HttpPost("sesion")]
        public IActionResult sesion([FromBody] Sesion sesion)
        {
            try
            {

                var query = $"SELECT * FROM usuario WHERE email = '{sesion.correo}' AND contrasenia = '{sesion.contrasenia}' AND estado = TRUE";
                var resultado = db.ExecuteQuery(query);

                if (resultado.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    // Si el usuario no existe o las credenciales son incorrectas, devolvemos un Unauthorized
                    return Unauthorized("Credenciales incorrectas o usuario inexistente");
                }

                // Si las credenciales son correctas, construimos un objeto usuario con los datos del DataRow
                var usuario = new usuario
                {
                    id_usuario = Convert.ToInt32(resultado.Rows[0]["id_usuario"]),
                    rol = resultado.Rows[0]["id_rol"].ToString(),
                    email = resultado.Rows[0]["email"].ToString(),
                    nombre = resultado.Rows[0]["nombre"].ToString(),
                    estado = Convert.ToBoolean(resultado.Rows[0]["estado"])
                };

                // En este ejemplo, devolvemos un Ok con el usuario autenticado
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                // En caso de error, devolvemos un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("registro")]
        public IActionResult Register([FromBody] registro_usuario registro)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT id_usuario FROM usuario WHERE email = '{registro.email}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO usuario (id_rol, nombre, contrasenia, email) VALUES ( 2,'{registro.nombre}', '{registro.contrasenia}', '{registro.email}')";
                    db.ExecuteQuery(queryInsertar);

                    // Registro exitoso, devolver un Ok
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("El usuario ya está registrado");
                }
               
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }
    }



}


