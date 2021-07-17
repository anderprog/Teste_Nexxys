using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiValidacao.Models
{
    public class Usuario
    {
        [Key]
        public string Login { get; set; }
        public string Senha { get; set; }

    }
}
