namespace AnalisisIClinicaMedicaBack.Requests
{
    public class agregaryeditarMedicoRequest
    {
        //Tabla medico
        public int? id_medico { get; set; }
        public int? colegiado { get; set; }
        public int? id_empleado { get; set; }
        public int? especialidad_id_especialidad { get; set; } // para intermedia catalogo medico
        public int? id_especialidad { get; set; }
        public int? id_genero { get; set; }
        public int? id_estado_civil { get; set; }
        public string? primer_nombre { get; set; }
        public string? segundo_nombre { get; set; }
        public string? primer_apellido { get; set; }
        public string? segundo_apellido { get; set; }
        public string? DPI { get; set; }
        public DateTime? fecha_nacimiento { get; set; }
        public int? telefono { get; set; }
        public string? correo_electronico { get; set; }
        public DateTime? fecha_contratacion { get; set; }
        public int? id_direccion { get; set; }
        public int? id_departamento { get; set; }
        public int? id_municipio { get; set; }
        public string? calle { get; set; }
        public string? avenida { get; set; }
        public string? zona_barrio { get; set; }
        public string? residencial_colonia { get; set; }
        public string? numero_vivienda { get; set; }
        public string? indicacion_extra { get; set; }
    }
}
