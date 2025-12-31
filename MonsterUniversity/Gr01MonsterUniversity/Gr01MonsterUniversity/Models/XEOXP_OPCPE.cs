using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Agrega este para el Order

namespace Gr01MonsterUniversity.Models
{
    public partial class XEOXP_OPCPE
    {
        [Key]
        [Column(Order = 0)] // Define el orden de la llave compuesta
        public int XEPER_CODIGO { get; set; }

        [Key]
        [Column(Order = 1)] // Define el orden de la llave compuesta
        public string XEOPC_CODIGO { get; set; }
    }
}