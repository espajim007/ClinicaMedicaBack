namespace AnalisisIClinicaMedicaBack.Models
{
    public class citaModel
    {
        public int? id_cita { get; set; }
        public int? expediente_id_expediente { get; set; }
        public int? medico_id_medico { get; set; }
        public int? id_estado_cita { get; set; }
        public string fecha { get; set; }
        public string hora { get; set; }
        public int? id_empleado { get; set; }
    }
}
