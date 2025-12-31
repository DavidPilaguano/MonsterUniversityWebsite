using System.Collections.Generic;

namespace Gr01MonsterUniversity.Models
{
    public class AsignacionPerfilesViewModel
    {
        // Cambiado a string para coincidir con varchar(5) de la DB
        public string SelectedPerfilId { get; set; }
        public List<string> UsuariosDisponibles { get; set; }
        public List<string> UsuariosAsignados { get; set; }

        public AsignacionPerfilesViewModel()
        {
            UsuariosDisponibles = new List<string>();
            UsuariosAsignados = new List<string>();
        }
    }
}