namespace AnalisisIClinicaMedicaBack.Models
{
    public class empleadoModel
    {
        public int? id_empleado { get; set; }
        public int? id_direccion { get; set; }
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
    }
}
