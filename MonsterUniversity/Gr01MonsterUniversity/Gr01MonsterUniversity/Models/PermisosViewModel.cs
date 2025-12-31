using System.Collections.Generic;

namespace Gr01MonsterUniversity.Models
{
    public class PermisosViewModel
    {
        public string CodigoPerfil { get; set; }
        public string NombrePerfil { get; set; }
        public List<OpcionSeleccionable> Opciones { get; set; }
    }
}