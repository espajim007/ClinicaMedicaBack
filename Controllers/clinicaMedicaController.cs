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

        [HttpPost("catalogos/editar-aseguradora")]
        public IActionResult EditarAseguradora([FromBody] aseguradoraModel aseguradora)
        {
            try
            {
                var queryActualizar = $"UPDATE aseguradora SET nombre = '{aseguradora.nombre}', copago = '{aseguradora.copago}', telefono = '{aseguradora.telefono}'" +
                    $", Correo = '{aseguradora.Correo}'" +
                $" WHERE id_aseguradora = {aseguradora.id_aseguradora}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nueva-aseguradora")]
        public IActionResult NuevaAseguradora([FromBody] aseguradoraModel aseguradora)
        {
            try
            {
                    var queryInsertar = $"INSERT INTO aseguradora (nombre,copago,telefono, Correo) " +
                    $"VALUES ( '{aseguradora.nombre}', '{aseguradora.copago}', '{aseguradora.telefono}', '{aseguradora.Correo}')";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
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

        [HttpPost("catalogos/editar-cita")]
        public IActionResult EditarCita([FromBody] citaModel cita)
        {
            try
            {
                var queryActualizar = $"UPDATE cita SET expediente_id_expediente = '{cita.expediente_id_expediente}', medico_id_medico = '{cita.medico_id_medico}'" +
                    $", id_estado_cita = '{cita.id_estado_cita}'" +
                $" WHERE id_cita = {cita.id_cita}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        //---------------------------------------contactoEmergencia
        [HttpGet("catalogos/contacto-emergencia")]
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

        [HttpPost("catalogos/editar-contacto-emergencia")]
        public IActionResult EditarContactoEmergencia([FromBody] contacto_emergenciaModel contacto)
        {
            try
            {
                var queryActualizar = $"UPDATE contacto_emergencia SET id_relacion_paciente = '{contacto.id_relacion_paciente}', id_genero = '{contacto.id_genero}'" +
                    $", primer_nombre = '{contacto.primer_nombre}', segundo_nombre = '{contacto.segundo_nombre}', primer_apellido = '{contacto.primer_apellido}'," +
                    $" segundo_apellido = '{contacto.segundo_apellido}',telefono = '{contacto.telefono}'" +
                $" WHERE id_contacto_emergencia = {contacto.id_contacto_emergencia}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nuevo-contacto-emergencia")]
        public IActionResult nuevoContactoEmergencia([FromBody] contacto_emergenciaModel contacto)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT telefono FROM contacto_emergencia WHERE telefono = '{contacto.telefono}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO contacto_emergencia (id_relacion_paciente, id_genero, primer_nombre, segundo_nombre, primer_apellido, segundo_apellido, telefono) VALUES" +
                        $" ( '{contacto.id_relacion_paciente}',{contacto.id_genero}',{contacto.primer_nombre}',{contacto.segundo_nombre}',{contacto.primer_apellido}'" +
                        $",{contacto.segundo_apellido}',{contacto.telefono}')";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado este contacto");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
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

        [HttpPost("catalogos/editar-direccion")]
        public IActionResult Editardireccion([FromBody] direccionModel direccion)
        {
            try
            {
                var queryActualizar = $"UPDATE direccion SET id_municipio = '{direccion.id_municipio}', calle = '{direccion.calle}'" +
                    $", avenida = '{direccion.avenida}', zona_barrio = '{direccion.zona_barrio}', residencial_colonia = '{direccion.residencial_colonia}'," +
                    $" numero_vivienda = '{direccion.numero_vivienda}',indicacion_extra = '{direccion.indicacion_extra}'" +
                $" WHERE id_direccion = {direccion.id_direccion}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nueva-direccion")]
        public IActionResult NuevaDireccion([FromBody] direccionModel direccion)
        {
            try
            {
                    var queryInsertar = $"INSERT INTO direccion (id_municipio, calle, avenida, zona_barrio, residencial_colonia, numero_vivienda, indicacion_extra) VALUES" +
                        $" ( '{direccion.id_municipio}',{direccion.calle}',{direccion.avenida}',{direccion.zona_barrio}',{direccion.residencial_colonia}')," +
                        $"'{direccion.numero_vivienda}',{direccion.indicacion_extra}'";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
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

        [HttpPost("catalogos/editar-expediente")]
        public IActionResult EditarExpediente([FromBody] expedienteModel expediente)
        {
            try
            {
                var queryActualizar = $"UPDATE expediente SET paciente_id_paciente = '{expediente.paciente_id_paciente}'" +
                $" WHERE id_expediente = {expediente.id_expediente}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nuevo-Expediente")]
        public IActionResult NuevoExpediente([FromBody] expedienteModel expediente)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT id_expediente FROM expediente WHERE email = '{expediente.paciente_id_paciente}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO expediente ( paciente_id_paciente, fecha_creacion) VALUES ( '{expediente.paciente_id_paciente}', GETDATE())";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado ese paciente");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //---------------------------------------fichaPaciente
        [HttpGet("catalogos/ficha-paciente")]
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

        [HttpPost("catalogos/editar-ficha-paciente")]
        public IActionResult EditarFichaPaciente([FromBody] fichaPacienteModel fichaPaciente)
        {
            try
            {
                var queryActualizar = $"UPDATE ficha_paciente SET aseguradora_id_aseguradora = '{fichaPaciente.aseguradora_id_aseguradora}', id_contacto_emergencia = '{fichaPaciente.id_contacto_emergencia}', " +
                $" id_direccion = '{fichaPaciente.id_direccion}', id_tipo_sangre = '{fichaPaciente.id_tipo_sangre}', id_ocupacion = '{fichaPaciente.id_ocupacion}', genero_idgenero = '{fichaPaciente.genero_idgenero}'," +
                $" id_estado_civil = '{fichaPaciente.id_estado_civil}', primer_nombre = '{fichaPaciente.primer_nombre}', segundo_nombre = '{fichaPaciente.segundo_nombre}'," +
                $" primer_apellido = '{fichaPaciente.primer_apellido}', segundo_apellido = '{fichaPaciente.segundo_apellido}', DPI = '{fichaPaciente.DPI}', fecha_nacimiento = '{fichaPaciente.fecha_nacimiento}', " +
                $" telefono = '{fichaPaciente.telefono}', correo_electronico = '{fichaPaciente.correo_electronico}', NIT = '{fichaPaciente.NIT}', observaciones = '{fichaPaciente.observaciones}'" +
                $" WHERE id_ficha_paciente = {fichaPaciente.id_ficha_paciente}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nueva-ficha-paciente")]
        public IActionResult NuevoExpediente([FromBody] fichaPacienteModel ficha)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT DPI FROM ficha_paciente WHERE DPI = '{ficha.DPI}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO ficha_paciente ( aseguradora_id_aseguradora, id_contacto_emergencia, id_direccion, id_tipo_sangre, id_ocupacion, genero_idgenero, " +
                        $"id_estado_civil, primer_nombre, segundo_nombre, primer_apellido, segundo_apellido, DPI, fecha_nacimiento, telefono, correo_electronico, NIT, observaciones) " +
                        $"VALUES ( '{ficha.aseguradora_id_aseguradora}', '{ficha.id_contacto_emergencia}', '{ficha.id_direccion}', '{ficha.id_tipo_sangre}', '{ficha.id_ocupacion}', " +
                        $"'{ficha.genero_idgenero}', '{ficha.id_estado_civil}'" +
                        $", '{ficha.primer_nombre}', '{ficha.segundo_nombre}', '{ficha.primer_apellido}', '{ficha.segundo_apellido}', '{ficha.DPI}', '{ficha.fecha_nacimiento}'," +
                        $", '{ficha.telefono}', '{ficha.correo_electronico}', '{ficha.NIT}', '{ficha.observaciones}')";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado ese paciente");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //-------------------------------------------empleado

        [HttpGet("catalogos/empleados")]
        public IActionResult GetEmpleados()
        {
            try
            {
                var query = @"SELECT id_empleado, id_direccion, id_genero, id_estado_civil, 
                                    primer_nombre, segundo_nombre, primer_apellido, segundo_apellido, 
                                    DPI, fecha_nacimiento, telefono, correo_electronico, fecha_contratacion
                              FROM empleado 
                              ORDER BY id_empleado";
                var resultado = db.ExecuteQuery(query);
                var empleados = resultado.AsEnumerable().Select(row => new empleadoModel
                {
                    id_empleado = Convert.ToInt32(row["id_empleado"]),
                    id_direccion = Convert.ToInt32(row["id_direccion"]),
                    id_genero = Convert.ToInt32(row["id_genero"]),
                    id_estado_civil = Convert.ToInt32(row["id_estado_civil"]),
                    primer_nombre = row["primer_nombre"]?.ToString(),
                    segundo_nombre = row["segundo_nombre"]?.ToString(),
                    primer_apellido = row["primer_apellido"]?.ToString(),
                    segundo_apellido = row["segundo_apellido"]?.ToString(),
                    DPI = row["DPI"]?.ToString(),
                    fecha_nacimiento = ((DateTime)row["fecha_nacimiento"]).Date,
                    telefono = row["telefono"] as int?,
                    correo_electronico = row["correo_electronico"]?.ToString(),
                    fecha_contratacion = ((DateTime)row["fecha_contratacion"]).Date,

                }).ToList();
                return Ok(empleados);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("catalogos/editar-empleado")]
        public IActionResult EditarEmpleado([FromBody] empleadoModel empleado)
        {
            try
            {
                var queryActualizar = $"UPDATE empleado SET id_direccion = '{empleado.id_direccion}', id_genero = '{empleado.id_genero}', id_estado_civil = '{empleado.id_estado_civil}', primer_nombre = '{empleado.primer_nombre}'," +
                $" segundo_nombre = '{empleado.segundo_nombre}', primer_apellido = '{empleado.primer_apellido}', segundo_apellido = '{empleado.segundo_apellido}', DPI = '{empleado.DPI}', fecha_nacimiento = '{empleado.fecha_nacimiento}'," +
                $" telefono = '{empleado.telefono}', correo_electronico = '{empleado.correo_electronico}', fecha_contratacion = '{empleado.fecha_contratacion}'" +
                $" WHERE id_empleado = {empleado.id_empleado}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nuevo-empleado")]
        public IActionResult NuevoEmpleado([FromBody] empleadoModel empleado)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT DPI FROM empleado WHERE DPI = '{empleado.DPI}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO empleado ( id_direccion, id_genero, id_estado_civil, primer_nombre, segundo_nombre, primer_apellido," +
                        $" segundo_apellido, DPI, fecha_nacimiento, telefono, correo_electronico, fecha_contratacion) " +
                        $"VALUES ( '{empleado.id_direccion}', '{empleado.id_genero}', '{empleado.id_estado_civil}', '{empleado.primer_nombre}', '{empleado.segundo_nombre}', " +
                        $", '{empleado.primer_apellido}', '{empleado.segundo_apellido}', '{empleado.DPI}', '{empleado.fecha_nacimiento}', '{empleado.telefono}', " +
                        $", '{empleado.correo_electronico}', '{empleado.fecha_contratacion}')";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado ese empleado");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //-------------------------------------------especialidad

        [HttpGet("catalogos/especialidad")]
        public IActionResult GetEspecialidad()
        {
            try
            {
                var query = @"SELECT id_especialidad, nombre, descipcion
                     FROM especialidad 
                     ORDER BY id_especialidad";
                var resultado = db.ExecuteQuery(query);
                var especialidades = resultado.AsEnumerable().Select(row => new especialidadModel
                {
                    id_especialidad = Convert.ToInt32(row["id_especialidad"]),
                    nombre = row["nombre"]?.ToString(),
                    descipcion = row["descipcion"]?.ToString()
                }).ToList();
                return Ok(especialidades);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("catalogos/editar-especialidad")]
        public IActionResult EditarEspecialidad([FromBody] especialidadModel especialidad)
        {
            try
            {
                var queryActualizar = $"UPDATE especialidad SET nombre = '{especialidad.nombre}', descipcion = '{especialidad.descipcion}'" +
                $" WHERE id_especialidad = {especialidad.id_especialidad}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nueva-especialidad")]
        public IActionResult NuevaEspecialidad([FromBody] especialidadModel especialidad)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT  nombre FROM especialidad WHERE nombre = '{especialidad.nombre}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO especialidad ( nombre, descipcion) " +
                        $"VALUES ( '{especialidad.nombre}', '{especialidad.descipcion}')";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado esta especialidad");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //-------------------------------------------estadoCita

        [HttpGet("catalogos/estado-cita")]
        public IActionResult GetEstadoCita()
        {
            try
            {
                var query = @"SELECT id_estado_cita, nombre
                     FROM estado_cita 
                     ORDER BY id_estado_cita";
                var resultado = db.ExecuteQuery(query);
                var estadoCitas = resultado.AsEnumerable().Select(row => new estadoCitaModel
                {
                    id_estado_cita = Convert.ToInt32(row["id_estado_cita"]),
                    nombre = row["nombre"]?.ToString(),
                }).ToList();
                return Ok(estadoCitas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("catalogos/editar-estado-cita")]
        public IActionResult EditarEstadoCita([FromBody] estadoCitaModel estado)
        {
            try
            {
                var queryActualizar = $"UPDATE estado_cita SET nombre = '{estado.nombre}' " +
                $" WHERE id_estado_cita = {estado.id_estado_cita}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nueva-estado-cita")]
        public IActionResult NuevaEstadoCita([FromBody] estadoCitaModel estado)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT  nombre FROM estado_cita WHERE nombre = '{estado.nombre}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO estado_cita (nombre) " +
                        $"VALUES ( '{estado.nombre}')";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado este estado");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //-------------------------------------------estadoCivil

        [HttpGet("catalogos/estado-civil")]
        public IActionResult GetEstadoCivil()
        {
            try
            {
                var query = @"SELECT id_estado_civil, nombre
                     FROM estado_civil 
                     ORDER BY id_estado_civil";
                var resultado = db.ExecuteQuery(query);
                var estadosCiviles = resultado.AsEnumerable().Select(row => new estadoCivilModel
                {
                    id_estado_civil = Convert.ToInt32(row["id_estado_civil"]),
                    nombre = row["nombre"]?.ToString(),
                }).ToList();
                return Ok(estadosCiviles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("catalogos/editar-estado-civil")]
        public IActionResult EditarEstadoCivil([FromBody] estadoCivilModel estado)
        {
            try
            {
                var queryActualizar = $"UPDATE estado_civil SET nombre = '{estado.nombre}' " +
                $" WHERE id_estado_civil = {estado.id_estado_civil}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nueva-estado-civil")]
        public IActionResult NuevaEstadoCivil([FromBody] estadoCivilModel estado)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT  nombre FROM estado_civil WHERE nombre = '{estado.nombre}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO estado_civil (nombre) " +
                        $"VALUES ( '{estado.nombre}')";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado este estado");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //-------------------------------------------genero

        [HttpGet("catalogos/genero")]
        public IActionResult GetGenero()
        {
            try
            {
                var query = @"SELECT idgenero, genero
                     FROM genero 
                     ORDER BY idgenero";
                var resultado = db.ExecuteQuery(query);
                var generos = resultado.AsEnumerable().Select(row => new generoModel
                {
                    idgenero = Convert.ToInt32(row["idgenero"]),
                    genero = row["genero"]?.ToString(),
                }).ToList();
                return Ok(generos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //-------------------------------------------medicoEspecialidad

        [HttpGet("catalogos/medico-especialidad")]
        public IActionResult GetMedicoEspacialidad()
        {
            try
            {
                var query = @"SELECT id_medico_especialidad, id_medico, especialidad_id_especialidad
                      FROM medico_especialidad 
                      ORDER BY id_medico";
                var resultado = db.ExecuteQuery(query);
                var especialidades = resultado.AsEnumerable().Select(row => new medicoEspecialidadModel
                {
                    id_medico_especialidad = Convert.ToInt32(row["id_medico_especialidad"]),
                    id_medico = Convert.ToInt32(row["id_medico"]),
                    especialidad_id_especialidad = Convert.ToInt32(row["especialidad_id_especialidad"])
                }).ToList();
                return Ok(especialidades);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("catalogos/editar-medico-especialidad")]
        public IActionResult EditarMedicoEspecialiad([FromBody] medicoEspecialidadModel medicoEspecialidad)
        {
            try
            {
                var queryActualizar = $"UPDATE medico_especialidad SET id_medico = '{medicoEspecialidad.id_medico}', especialidad_id_especialidad = '{medicoEspecialidad.id_medico_especialidad}' " +
                $" WHERE id_medico_especialidad = {medicoEspecialidad.id_medico_especialidad}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nueva-medico-especialidad")]
        public IActionResult NuevoMedicoEspecialiad([FromBody] medicoEspecialidadModel medicoEspecialidad)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT id_medico FROM medico_especialidad WHERE id_medico = '{medicoEspecialidad.id_medico}' AND especialidad_id_especialidad = '{medicoEspecialidad.id_medico_especialidad}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO medico_especialidad (id_medico,id_medico_especialidad ) " +
                        $"VALUES ( '{medicoEspecialidad.id_medico}',{medicoEspecialidad.id_medico_especialidad})";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado ");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //-------------------------------------------medico

        [HttpGet("catalogos/medico")]
        public IActionResult GetMedico()
        {
            try
            {
                var query = @"SELECT id_medico, colegiado, id_empleado
                      FROM medico
                      ORDER BY id_medico";
                var resultado = db.ExecuteQuery(query);
                var medicos = resultado.AsEnumerable().Select(row => new medicoModel
                {
                    id_medico = Convert.ToInt32(row["id_medico"]),
                    colegiado = Convert.ToInt32(row["colegiado"]),
                    id_empleado = Convert.ToInt32(row["id_empleado"])
                }).ToList();
                return Ok(medicos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("catalogos/editar-medico")]
        public IActionResult EditarMedico([FromBody] medicoModel medico)
        {
            try
            {
                var queryActualizar = $"UPDATE medico SET colegiado = '{medico.colegiado}', id_empleado = '{medico.id_empleado}' " +
                $" WHERE id_medico = {medico.id_medico}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nuevo-medico")]
        public IActionResult NuevoMedico([FromBody] medicoModel medico)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT id_medico FROM medico WHERE id_medico = '{medico.colegiado}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO medico (colegiado,id_empleado ) " +
                        $"VALUES ( '{medico.colegiado}',{medico.id_empleado})";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado este medico");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //-------------------------------------------ocupacion

        [HttpGet("catalogos/ocupacion")]
        public IActionResult GetOcupacion()
        {
            try
            {
                var query = @"SELECT id_ocupacion, nombre
                      FROM ocupacion
                      ORDER BY id_ocupacion";
                var resultado = db.ExecuteQuery(query);
                var ocupaciones = resultado.AsEnumerable().Select(row => new ocupacionModel
                {
                    id_ocupacion = Convert.ToInt32(row["id_ocupacion"]),
                    nombre = row["nombre"].ToString(),
                }).ToList();
                return Ok(ocupaciones);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("catalogos/editar-ocupacion")]
        public IActionResult EditarOcupacion([FromBody] ocupacionModel ocupacion)
        {
            try
            {
                var queryActualizar = $"UPDATE ocupacion SET nombre = '{ocupacion.nombre}' " +
                $" WHERE id_ocupacion = {ocupacion.id_ocupacion}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nueva-ocupacion")]
        public IActionResult NuevaOcupacion([FromBody] ocupacionModel ocupacion)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT nombre FROM ocupacion WHERE id_medico = '{ocupacion.nombre}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO ocupacion (nombre) " +
                        $"VALUES ( '{ocupacion.nombre}')";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado ");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }

        //-------------------------------------------tipoSangre

        [HttpGet("catalogos/tipo-sangre")]
        public IActionResult GetTipoSangre()
        {
            try
            {
                var query = @"SELECT id_tipo_sangre, Nombre
                      FROM tipo_sangre
                      ORDER BY id_tipo_sangre";
                var resultado = db.ExecuteQuery(query);
                var tiposSangre = resultado.AsEnumerable().Select(row => new tipoSangreModel
                {
                    id_tipo_sangre = Convert.ToInt32(row["id_tipo_sangre"]),
                    Nombre = row["Nombre"].ToString(),
                }).ToList();
                return Ok(tiposSangre);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("catalogos/editar-tipo-sangre")]
        public IActionResult EditarTipoSangre([FromBody] tipoSangreModel sangre)
        {
            try
            {
                var queryActualizar = $"UPDATE tipo_sangre SET nombre = '{sangre.Nombre}' " +
                $" WHERE id_tipo_sangre = {sangre.id_tipo_sangre}";
                var actualizar = db.ExecuteQuery(queryActualizar);

                return Ok();
            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("catalogos/nueva-tipo-sangre")]
        public IActionResult NuevoTipoSangre([FromBody] tipoSangreModel sangre)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT Nombre FROM tipo_sangre WHERE id_medico = '{sangre.Nombre}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO tipo_sangre (Nombre) " +
                        $"VALUES ( '{sangre.Nombre}')";
                    db.ExecuteQuery(queryInsertar);
                    return Ok();
                }
                else
                {
                    // El usuario ya existe, devolver un BadRequest
                    return BadRequest("Ya esta registrado ");
                }

            }
            catch (Exception ex)
            {
                // En caso de error, devolver un BadRequest con el mensaje de error
                return BadRequest(ex.Message);
            }
        }


        //---------------------------------------------------------------------------------------------------------------------------
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
        public IActionResult editarDepartamento([FromBody] departamentoModel editarDep)
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
        public IActionResult nuevoDepartamento([FromBody] departamentoModel nuevoDeo)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT nombre FROM departamento WHERE email = '{nuevoDeo.nombre}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO departamento (nombre) VALUES ( '{nuevoDeo.nombre}')";
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
        public IActionResult EditarMunicipio([FromBody] municipioModel editarMuni)
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
        public IActionResult nuevoMunicipio([FromBody] municipioModel nuevoMuni)
        {
            try
            {
                // Verificar si el usuario ya existe en la base de datos
                var queryValidador = $"SELECT nombre FROM municipio WHERE email = '{nuevoMuni.nombre}'";
                var resultadoValidador = db.ExecuteQuery(queryValidador);

                if (resultadoValidador.Rows.Count == 0) // si no coincide con nada, el usuario no existe y por eso en la ejecucion del query devuelve 0 filas
                {
                    var queryInsertar = $"INSERT INTO municipio (nombre) VALUES ( '{nuevoMuni.nombre}')";
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











        // AQUI SE ACABAN LOS CATALOGOS DE PROCESO DE CITA

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
        
    }
}


