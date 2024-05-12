using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using AnalisisIClinicaMedicaBack.Provider;
using AnalisisIClinicaMedicaBack.Models;
using MySql.Data.MySqlClient.Memcached;
using AnalisisIClinicaMedicaBack.Requests;
using Microsoft.Win32;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;


namespace AnalisisIClinicaMedicaBack.Controllers
{

    public class Progra
    {
        private readonly DatabaseProvider db;

        public Progra(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ConnectionString");
            db = new DatabaseProvider(connectionString);
        }


        /*public void Existente()
        {
            try
            {
                var query = "SELECT id_usuario, contrasenia FROM usuario";
                var resultado = db.ExecuteQuery(query);

                foreach (DataRow row in resultado.Rows)
                {
                    int idUsuario = Convert.ToInt32(row["id_usuario"]);
                    string contraseniaActual = row["contrasenia"].ToString();

                    string encriptar = EncriptarContraseña(contraseniaActual);

                    var queryActualizar = $"UPDATE usuario SET contrasenia = '{encriptar}' WHERE id_usuario = {idUsuario}";
                    db.ExecuteQuery(queryActualizar);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al encriptar las contraseñas: {ex.Message}");
            }
        }*/

        public string EncriptarContraseña(string contraseña)
        {
            // Generar un salt aleatorio
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            // Concatenar la contraseña con el salt
            byte[] contraseniaConSalt = Encoding.UTF8.GetBytes(contraseña).Concat(salt).ToArray();

            // Calcular el hash utilizando SHA-256
            using (var sha256 = SHA256.Create())
            {
                byte[] hashContraseña = sha256.ComputeHash(contraseniaConSalt);

                // Construir la cadena encriptada concatenando el salt y el hash
                byte[] hashConcatenado = new byte[salt.Length + hashContraseña.Length];
                Array.Copy(salt, 0, hashConcatenado, 0, salt.Length);
                Array.Copy(hashContraseña, 0, hashConcatenado, salt.Length, hashContraseña.Length);

                return Convert.ToBase64String(hashConcatenado);
            }
        }


    }

    [Route("api/[controller]")]
    [ApiController]
    public class clinicaMedicaController : ControllerBase
    {
        private readonly DatabaseProvider db;
        private readonly Progra progra;

        public clinicaMedicaController(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ConnectionString");
            db = new DatabaseProvider(connectionString);
            progra = new Progra(configuration);
        }

        //Catalogos

        [HttpGet("catalogos/usuarios")]
        public IActionResult GetUsuarios()
        {
            try
            {
                var query = @"SELECT id_usuario, id_rol , nombre, email, contrasenia,estado
                     FROM usuario 
                     ORDER BY id_usuario";
                var resultado = db.ExecuteQuery(query);
                var usuarios = resultado.AsEnumerable().Select(row => new usuario
                {
                    id_usuario = Convert.ToInt32(row["id_usuario"]),
                    id_rol = Convert.ToInt32(row["id_rol"]),
                    nombre = row["nombre"].ToString(),
                    email = row["email"].ToString(),
                    contrasenia = progra.EncriptarContraseña(row["contrasenia"].ToString()), // Aquí obtienes la contraseña
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
                $"estado = '{editarUsuario.estado}' WHERE id_usuario = {editarUsuario.id_usuario}";
                var actualizar = db.ExecuteQuery(queryActualizar);
                 

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpDelete("catalogos/eliminar-usuario/{idUsuario}")]
public IActionResult EliminarUsuario(int idUsuario)
{
    try
    {
        // Aquí realizas la lógica para eliminar el usuario de la base de datos
        var queryEliminar = $"DELETE FROM usuario WHERE id_usuario = {idUsuario}";
        db.ExecuteQuery(queryEliminar);

        return Ok("Usuario eliminado correctamente");
    }
    catch (Exception ex)
    {
        // En caso de error, devuelves un BadRequest con el mensaje de error
        return BadRequest($"Error al eliminar el usuario: {ex.Message}");
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
                                                                                                            $"'{nuevoUsuario.contrasenia}', '{nuevoUsuario.email}', 1)";
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

                if (resultado.Rows.Count == 0)
                {
                    // Si no se encuentra ningún usuario con las credenciales proporcionadas, devolver un Unauthorized
                    return Unauthorized("Credenciales incorrectas o usuario inexistente");
                }

                // Construir el objeto usuario con los datos obtenidos de la base de datos
                var usuario = new usuario
                {
                    id_usuario = Convert.ToInt32(resultado.Rows[0]["id_usuario"]),
                    id_rol = Convert.ToInt32(resultado.Rows[0]["id_rol"]),
                    email = resultado.Rows[0]["email"].ToString(),
                    nombre = resultado.Rows[0]["nombre"].ToString(),
                    estado = Convert.ToBoolean(resultado.Rows[0]["estado"])
                };

                // Devolver el usuario autenticado
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest($"Error al autenticar al usuario: {ex.Message}");
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
                    var queryInsertar = $"INSERT INTO usuario (id_rol, nombre, contrasenia, email,estado) VALUES ( 2,'{registro.nombre}', '{registro.contrasenia}', '{registro.email}',1)";
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


