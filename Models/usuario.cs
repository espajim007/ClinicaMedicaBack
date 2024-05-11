namespace AnalisisIClinicaMedicaBack.Models
{
    public class usuario
    {
        public int? id_usuario { get; set; }
        public string rol { get; set; }
        public string nombre { get; set; }
        public string email { get; set; }
        public string? contrasenia { get; set; }
        public bool? estado { get; set; }
    }
}
