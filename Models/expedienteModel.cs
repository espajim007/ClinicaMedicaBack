namespace AnalisisIClinicaMedicaBack.Models
{
    public class expedienteModel
    {
        public int? id_expediente { get; set; }
        public int? paciente_id_paciente { get; set; }
        public DateOnly? fecha_creacion { get; set; }
    }
}
