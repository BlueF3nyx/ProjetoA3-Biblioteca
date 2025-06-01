using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPP.Models
{
    public class Emprestimo
    {
        public int? Id { get; set; }
        public int? IdLivro { get; set; }
        public int? IdMembro { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public string? Status { get; set; }  // Pode ser "Ativo", "Devolvido", "Atrasado", etc.
    }

}
