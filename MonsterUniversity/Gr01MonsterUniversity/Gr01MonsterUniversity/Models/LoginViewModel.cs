using System.ComponentModel.DataAnnotations;

namespace Gr01MonsterUniversity.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Requerido")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "Requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}