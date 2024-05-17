namespace AnalisisIClinicaMedicaBack.Models
{
    public class direccionModel
    {
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
