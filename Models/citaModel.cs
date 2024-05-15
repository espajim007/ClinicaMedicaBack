namespace AnalisisIClinicaMedicaBack.Models
{
    public class citaModel
    {
        public int? id_cita { get; set; }
        public int? expediente_id_expediente { get; set; }
        public int? medico_id_medico { get; set; }
        public int? id_estado_cita { get; set; }
        public DateOnly? fecha { get; set; }
        public TimeOnly? hora { get; set; }
    }
}
