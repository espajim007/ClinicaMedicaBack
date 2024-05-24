namespace AnalisisIClinicaMedicaBack.Requests
{
    public class editarFichapacienteRequest
    {
        //Tabla ficha_paciente
        public int? id_expediente { get; set; }
        public int? paciente_id_paciente { get; set; }
        public int? aseguradora_id_aseguradora { get; set; }
        public int? id_contacto_emergencia { get; set; }
        public int? id_direccion { get; set; }
        public int? id_tipo_sangre { get; set; }
        public int? id_ocupacion { get; set; }
        public int? genero_idgenero { get; set; }
        public int? id_estado_civil { get; set; }
        public string? primer_nombre { get; set; }
        public string? segundo_nombre { get; set; }
        public string? primer_apellido { get; set; }
        public string? segundo_apellido { get; set; }
        public long? identif { get; set; }
        public string? fecha_nacimiento { get; set; }
        public int? telefono { get; set; }
        public string? correo_electronico { get; set; }
        public string? nit_fac { get; set; }
        public string? observaciones { get; set; }
        //Tabla direccion
        public int? id_municipio { get; set; }
        public string? calle { get; set; }
        public string? avenida { get; set; }
        public string? zona_barrio { get; set; }
        public string? residencial_colonia { get; set; }
        public string? numero_vivienda { get; set; }
        public string? indicacion_extra { get; set; }
        //Tabla contacto
        public int? id_relacion_paciente { get; set; }
        public int? genero_contacto { get; set; }
        public string? primer_nombre_contacoe { get; set; }
        public string? segundo_nombre_contactoe { get; set; }
        public string? primer_apellido_contactoe { get; set; }
        public string? segundo_apellido_contactoe { get; set; }
        public int? telefono_contactoe { get; set; }
        
        
        
    }
}
