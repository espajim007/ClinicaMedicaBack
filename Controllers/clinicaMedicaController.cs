using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using AnalisisIClinicaMedicaBack.Provider;
using AnalisisIClinicaMedicaBack.Models;
using MySql.Data.MySqlClient.Memcached;
using AnalisisIClinicaMedicaBack.Requests;

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

        [HttpGet("usuarios")]
        public IActionResult GetUsuarios()
        {
            try
            {
                var query = "SELECT * FROM usuario";
                var resultado = db.ExecuteQuery(query);
                var usuarios = resultado.AsEnumerable().Select(row => new usuario
                {
                    id_usuario = Convert.ToInt32(row["id_usuario"]),
                    id_rol = Convert.ToInt32(row["id_rol"]),
                    nombre = row["nombre"].ToString(),
                    contrasenia = row["contrasenia"].ToString(),
                    email = row["email"].ToString(),
                }).ToList();
                return Ok(usuarios);
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
                // Aquí realizamos la lógica para autenticar al usuario
                var query = $"SELECT * FROM usuario WHERE email = '{sesion.correo}' AND contrasenia = '{sesion.contrasenia}'";
                var resultado = db.ExecuteQuery(query);

                if (resultado.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    // Si el usuario no existe o las credenciales son incorrectas, devolvemos un Unauthorized
                    return Unauthorized();
                }

                // Si las credenciales son correctas, construimos un objeto usuario con los datos del DataRow
                var usuario = new usuario
                {
                    id_usuario = Convert.ToInt32(resultado.Rows[0]["id_usuario"]),
                    id_rol = Convert.ToInt32(resultado.Rows[0]["id_rol"]),
                    email = resultado.Rows[0]["email"].ToString(),
                    nombre = resultado.Rows[0]["nombre"].ToString()
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
                var queryValidador = $"SELECT id_usuario FROM usuario WHERE email = '{registro.correo}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO usuario (id_rol, nombre, contrasenia, email) VALUES (2,'{registro.nombre}', '{registro.contrasenia}', '{registro.correo}')";
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


