namespace AnalisisIClinicaMedicaBack.Models
{
    public class contacto_emergenciaModel
    {
        public int? id_contacto_emergencia { get; set; }
        public int? id_relacion_paciente { get; set; }
        public int? id_genero { get; set; }
        public string? primer_nombre { get; set; }
        public string? segundo_nombre { get; set; }
        public string? primer_apellido { get; set; }
        public string? segundo_apellido { get; set; }
        public int? telefono { get; set; }
    }
}
