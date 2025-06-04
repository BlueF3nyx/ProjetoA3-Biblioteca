using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPP.Models
{
    public class Funcionario
    {
        public int Id { get; set; }

        public string? Nome { get; set; }

        public string? Email { get; set; }

        public string? Senha { get; set; }
    }
}
