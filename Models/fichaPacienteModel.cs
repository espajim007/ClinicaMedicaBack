﻿namespace AnalisisIClinicaMedicaBack.Models
{
    public class fichaPacienteModel
    {
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
        public DateOnly? fecha_nacimiento { get; set; }
        public int? telefono { get; set; }
        public string? correo_electronico { get; set; }
        public string? NIT { get; set; }
        public string? observaciones { get; set; }
    }
}