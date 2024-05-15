﻿using Microsoft.AspNetCore.Http;
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
using Microsoft.VisualBasic;
using System.Collections.Generic;


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

        private static readonly byte[] Salt = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 }; // Salt fijo

        public string EncriptarContraseña(string contraseña)
        {
            // Concatenar la contraseña con el salt fijo
            byte[] contraseniaConSalt = Encoding.UTF8.GetBytes(contraseña).Concat(Salt).ToArray();

            // Calcular el hash utilizando SHA-256
            using (var sha256 = SHA256.Create())
            {
                byte[] hashContraseña = sha256.ComputeHash(contraseniaConSalt);

                return Convert.ToBase64String(hashContraseña);
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

       

        [HttpPost("sesion")]
        public IActionResult sesion([FromBody] Sesion sesion)
        {
            try
            {

                var query = $"SELECT * FROM usuario WHERE email = '{sesion.correo}' AND contrasenia = '{progra.EncriptarContraseña(sesion.contrasenia)}' AND estado = TRUE";
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
                    var queryInsertar = $"INSERT INTO usuario (id_rol, nombre, contrasenia, email,estado) VALUES ( 2,'{registro.nombre}', '{progra.EncriptarContraseña(registro.contrasenia)}', '{registro.email}',1)";
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

        //Catalogos
        //---------------------------------------ASEGURADORA
        [HttpGet("catalogos/aseguradora")]
        public IActionResult GetAseguradora()
        {
            try
            {
                var query = @"SELECT id_aseguradora, nombre, copago, telefono, Correo
                     FROM aseguradora 
                     ORDER BY id_aseguradora";
                var resultado = db.ExecuteQuery(query);
                var aseguradora = resultado.AsEnumerable().Select(row => new aseguradoraModel
                {
                    id_aseguradora = Convert.ToInt32(row["id_aseguradora"]),
                    nombre = row["nombre"].ToString(),
                    copago = Convert.ToDouble(row["copago"]),
                    telefono = Convert.ToInt32(row["telefono"]),
                    Correo = row["Correo"].ToString(),
                }).ToList();
                return Ok(aseguradora);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        //---------------------------------------CITA
        [HttpGet("catalogos/cita")]
        public IActionResult GetCita()
        {
            try
            {
                var query = @"SELECT id_cita, expediente_id_expediente, medico_id_medico, id_estado_cita, fecha, hora
                     FROM cita 
                     ORDER BY id_cita";
                var resultado = db.ExecuteQuery(query);
                var citas = resultado.AsEnumerable().Select(row => new citaModel
                {
                    id_cita = Convert.ToInt32(row["id_cita"]),
                    expediente_id_expediente = Convert.ToInt32(row["expediente_id_expediente"]),
                    medico_id_medico = Convert.ToInt32(row["medico_id_medico"]),
                    id_estado_cita = Convert.ToInt32(row["id_estado_cita"]),
                    fecha = ((DateTime)row["fecha"]).Date,
                    hora = TimeSpan.Parse(row["hora"].ToString())
                }).ToList();
                return Ok(citas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //---------------------------------------contactoEmergencia
        [HttpGet("catalogos/contactoEmergencia")]
        public IActionResult GetContactoEmergencia()
        {
            try
            {
                var query = @"SELECT id_contacto_emergencia, id_relacion_paciente, id_genero, primer_nombre, segundo_nombre, primer_apellido, segundo_apellido, telefono
                     FROM contacto_emergencia 
                     ORDER BY id_contacto_emergencia";
                var resultado = db.ExecuteQuery(query);
                var contactosEmergencias = resultado.AsEnumerable().Select(row => new contacto_emergenciaModel
                {
                    id_contacto_emergencia = Convert.ToInt32(row["id_contacto_emergencia"]),
                    id_relacion_paciente = Convert.ToInt32(row["id_relacion_paciente"]),
                    id_genero = Convert.ToInt32(row["id_genero"]),
                    primer_nombre = row["primer_nombre"].ToString(),
                    segundo_nombre = row["segundo_nombre"].ToString(),
                    primer_apellido = row["primer_apellido"].ToString(),
                    segundo_apellido = row["segundo_apellido"].ToString(),
                    telefono = Convert.ToInt32(row["telefono"])
                }).ToList();
                return Ok(contactosEmergencias);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //---------------------------------------direccion
        [HttpGet("catalogos/direccion")]
        public IActionResult GetDireccion()
        {
            try
            {
                var query = @"SELECT id_direccion, id_municipio, calle, avenida, zona_barrio, residencial_colonia, numero_vivienda, indicacion_extra
                     FROM direccion 
                     ORDER BY id_direccion";
                var resultado = db.ExecuteQuery(query);
                var direcciones = resultado.AsEnumerable().Select(row => new direccionModel
                {
                    id_direccion = Convert.ToInt32(row["id_direccion"]),
                    id_municipio = Convert.ToInt32(row["id_municipio"]),
                    calle = row["calle"].ToString(),
                    avenida = row["avenida"].ToString(),
                    zona_barrio = row["zona_barrio"].ToString(),
                    residencial_colonia = row["residencial_colonia"].ToString(),
                    numero_vivienda = row["numero_vivienda"].ToString(),
                    indicacion_extra = row["indicacion_extra"].ToString()
                }).ToList();
                return Ok(direcciones);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //---------------------------------------expediente
        [HttpGet("catalogos/expediente")]
        public IActionResult GetExpediente()
        {
            try
            {
                var query = @"SELECT id_expediente, paciente_id_paciente, fecha_creacion
                     FROM expediente 
                     ORDER BY id_expediente";
                var resultado = db.ExecuteQuery(query);
                var expedientes = resultado.AsEnumerable().Select(row => new expedienteModel
                {
                    id_expediente = Convert.ToInt32(row["id_expediente"]),
                    paciente_id_paciente = Convert.ToInt32(row["paciente_id_paciente"]),
                    fecha_creacion = ((DateTime)row["fecha_creacion"]).Date,
                }).ToList();
                return Ok(expedientes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //---------------------------------------fichaPaciente
        [HttpGet("catalogos/fichaPaciente")]
        public IActionResult GetFichaPaciente()
        {
            try
            {
                var query = @"SELECT id_ficha_paciente, aseguradora_id_aseguradora, id_contacto_emergencia, id_direccion,
                id_tipo_sangre, id_ocupacion, genero_idgenero, id_estado_civil, primer_nombre, segundo_nombre, primer_apellido, 
                segundo_apellido, DPI, fecha_nacimiento, telefono, correo_electronico, NIT, observaciones
                     FROM ficha_paciente 
                     ORDER BY id_ficha_paciente";
                var resultado = db.ExecuteQuery(query);
                var fichas = resultado.AsEnumerable().Select(row => new fichaPacienteModel
                {
                    id_ficha_paciente = Convert.ToInt32(row["id_ficha_paciente"]),
                    aseguradora_id_aseguradora = Convert.ToInt32(row["aseguradora_id_aseguradora"]),
                    id_contacto_emergencia = Convert.ToInt32(row["id_contacto_emergencia"]),
                    id_direccion = Convert.ToInt32(row["id_direccion"]),
                    id_tipo_sangre = Convert.ToInt32(row["id_tipo_sangre"]),
                    id_ocupacion = Convert.ToInt32(row["id_ocupacion"]),
                    genero_idgenero = Convert.ToInt32(row["genero_idgenero"]),
                    id_estado_civil = Convert.ToInt32(row["id_estado_civil"]),
                    primer_nombre = row["primer_nombre"].ToString(),
                    segundo_nombre = row["segundo_nombre"].ToString(),
                    primer_apellido = row["primer_apellido"].ToString(),
                    segundo_apellido = row["segundo_apellido"].ToString(),
                    DPI = row["DPI"].ToString(),
                    fecha_nacimiento = ((DateTime)row["fecha_nacimiento"]).Date,
                    telefono = Convert.ToInt32(row["telefono"]),
                    correo_electronico = row["correo_electronico"].ToString(),
                    NIT = row["NIT"].ToString(),
                    observaciones = row["observaciones"].ToString()
                }).ToList();
                return Ok(fichas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        //-------------------------------------------USUARIOS

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
                    contrasenia = row["contrasenia"].ToString(), // Aquí obtienes la contraseña
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
                var queryActualizar = $"UPDATE usuario SET id_rol = '{editarUsuario.id_rol}',nombre ='{editarUsuario.nombre}',email ='{editarUsuario.email}' " +
                $" WHERE id_usuario = {editarUsuario.id_usuario}";
                var actualizar = db.ExecuteQuery(queryActualizar);


                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpDelete("catalogos/cambio-estado-usuario/{idUsuario}")]
        public IActionResult CambioEstadoUsuario(int idUsuario)
        {
            try
            {
                // Aquí realizas la lógica para eliminar el usuario de la base de datos
                var query = $"UPDATE usuario SET estado = CASE " +
                    $"                                    WHEN estado = 1 THEN 0 " +
                    $"                                    ELSE 1 END WHERE id_usuario = {idUsuario}";


                db.ExecuteQuery(query);

                return Ok("Estado Cambiado Correctamente");
            }
            catch (Exception ex)
            {
                // En caso de error, devuelves un BadRequest con el mensaje de error
                return BadRequest($"Error al cambiar el estado  : {ex.Message}");
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
                                                                                                            $"'{progra.EncriptarContraseña(nuevoUsuario.contrasenia)}', '{nuevoUsuario.email}', 1)";
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
        //---------------------------------------ROLES
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

        //-----------------------------DEPARTAMENTO
        [HttpGet("catalogos/departamento")]
        public IActionResult GetDepartamento()
        {
            try
            {
                var query = @"SELECT id_departamento, nombre
                     FROM departamento 
                     ORDER BY id_deparamento";
                var resultado = db.ExecuteQuery(query);
                var departamentos = resultado.AsEnumerable().Select(row => new departamentoModel
                {
                    id_departamento = Convert.ToInt32(row["id_deparamento"]),
                    nombre = row["nombre"].ToString()
                }).ToList();
                return Ok(departamentos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("catalogos/editar-departamento")]
        public IActionResult editarDepartamento([FromBody] departamentoRequest editarDep)
        {
            try
            {
                var queryActualizar = $"UPDATE departamento SET nombre = '{editarDep.nombre}' " +
                $" WHERE id_departamento = {editarDep.id_departamento}";
                var actualizar = db.ExecuteQuery(queryActualizar);


                return Ok();
            }
            catch (Exception ex)
            { 
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nuevo-departamento")]
        public IActionResult nuevoDepartamento([FromBody] departamentoRequest nuevoDeo)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT nombre FROM departamento WHERE email = '{nuevoDeo.nombre}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO departamento (nombre) VALUES ( '{nuevoDeo.nombre}'";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado ese departamento");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //-----------------------------Municipio
        [HttpGet("catalogos/municipio")]
        public IActionResult GetMunicipio()
        {
            try
            {
                var query = @"SELECT id_municipio,id_departamento, nombre
                     FROM municipio 
                     ORDER BY id_municipio";
                var resultado = db.ExecuteQuery(query);
                var municipios = resultado.AsEnumerable().Select(row => new municipioModel
                {
                    id_municipio = Convert.ToInt32(row["id_municipio"]),
                    id_departamento = Convert.ToInt32(row["id_deparamento"]),
                    nombre = row["nombre"].ToString()
                }).ToList();
                return Ok(municipios);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("catalogos/editar-municipio")]
        public IActionResult EditarMunicipio([FromBody] municipioRequest editarMuni)
        {
            try
            {
                var queryActualizar = $"UPDATE municipio SET id_departamento = '{editarMuni.id_departamento}', nombre = '{editarMuni.nombre}'" +
                $" WHERE id_municipio = {editarMuni.id_municipio}";
                var actualizar = db.ExecuteQuery(queryActualizar);


                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nuevo-municipio")]
        public IActionResult nuevoMunicipio([FromBody] municipioRequest nuevoMuni)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT nombre FROM departamento WHERE email = '{nuevoMuni.nombre}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO municipio (nombre) VALUES ( '{nuevoMuni.nombre}'";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado ese departamento");
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


