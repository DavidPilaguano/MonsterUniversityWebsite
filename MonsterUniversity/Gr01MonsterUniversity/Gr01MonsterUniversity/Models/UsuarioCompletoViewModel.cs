using System;
using System.ComponentModel.DataAnnotations;

namespace Gr01MonsterUniversity.Models
{
    public class UsuarioCompletoViewModel
    {
        // ==========================================
        // PEEEMP_EMPLE (Datos Personales y Contacto)
        // ==========================================
        [Required(ErrorMessage = "El código es obligatorio")]
        [Display(Name = "Código")]
        public string PEEEMP_CODIGO { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria")]
        [Display(Name = "Cédula")]
        public string PEEEMP_CEDULA { get; set; }

        [Display(Name = "Nombres")]
        public string PEEEMP_NOMBRES { get; set; }

        [Display(Name = "Apellidos")]
        public string PEEEMP_APELLIDOS { get; set; }

        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string PEEEMP_EMAIL { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? PEEEMP_FECHANAC { get; set; } // Agregado para Tab Demográficos

        public string PEEEMP_DIRECCION { get; set; } // Agregado para Tab Contacto

        public string PEEEMP_TELEFONO { get; set; }  // Agregado para Tab Contacto

        public string PEEEMP_CELULAR { get; set; }

        [Range(0, 20, ErrorMessage = "Número de hijos no válido")]
        public int? PEEEMP_HIJOS { get; set; }       // Agregado para Tab Demográficos

        public string PEPSEX_CODIGO { get; set; }

        public string PEESC_CODIGO { get; set; }

        // public string PEEMP_FOTO { get; set; } // Se mantiene fuera por estabilidad de binding

        // ==========================================
        // XEUSU_USUAR (Seguridad y Cuenta)
        // ==========================================
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string XEUSU_PASWD { get; set; }

        [Display(Name = "Estado de Cuenta")]
        public string XEEST_CODIGO { get; set; }     // Agregado para manejar Activo/Inactivo

        [Display(Name = "Pie de Firma")]
        public string XEUSU_PIEFIR { get; set; }

        // ==========================================
        // XEUXP_USUPE (Perfil y Roles)
        // ==========================================
        [Required(ErrorMessage = "Debe asignar un perfil")]
        public string XEPER_CODIGO { get; set; }

        // Propiedad extra para mostrar el nombre del perfil en el Index o Reportes
        public string XEPER_DESCRI { get; set; }
    }
}