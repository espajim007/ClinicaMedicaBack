namespace AnalisisIClinicaMedicaBack.Requests
{
    public class editarFichapacienteRequest
    {
        //Tabla ficha_paciente
        public int? id_ficha_paciente { get; set; }
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
        public string? DPI { get; set; }
        public DateTime? fecha_nacimiento { get; set; }
        public int? telefono { get; set; }
        public string? correo_electronico { get; set; }
        public string? NIT { get; set; }
        public string? observaciones { get; set; }
        //Tabla aseguradora
        public string? nombre_aseguradora { get; set; }
        public double? copago { get; set; }
        public int? telefono_aseguradora { get; set; }
        public string? Correo_aseguradora { get; set; }
        //Tabla contacto de emergencia
        public int? id_relacion_paciente { get; set; }
        public string? primer_nombre_contacoe { get; set; }
        public string? segundo_nombre_contactoe { get; set; }
        public string? primer_apellido_contactoe { get; set; }
        public string? segundo_apellido_contactoe { get; set; }
        public int? telefono_contactoe { get; set; }
        //Tabla relacion paciente
        public string? relacion { get; set; }
        //Tabla genero contacto de emergencia
        public string? genero_contactoe { get; set; }
        //Tabla direccion
        public int? id_municipio { get; set; }
        public string? calle { get; set; }
        public string? avenida { get; set; }
        public string? zona_barrio { get; set; }
        public string? residencial_colonia { get; set; }
        public string? numero_vivienda { get; set; }
        public string? indicacion_extra { get; set; }
        //Tabla municipio
        public int? id_departamento { get; set; }
        public string? nombre_municipio { get; set; }
        //Tabla departamento
        public string? nombre_departamento { get; set; }
        //Tabla tipo de sangre
        public string? Nombre_tipo_sangre { get; set; }
        //Tabla ocupacion
        public string? nombre_ocupacion { get; set; }
        //Tabla genero paciente
        public string? genero_paciente { get; set; }
        //Tabla estado civil
        public string? nombre_estado_civil { get; set; }
    }
}
