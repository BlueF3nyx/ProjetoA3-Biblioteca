using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPP.Models
{
    public class Historico
    {
        public string? Id_Livro { get; set; }
        public string? Id_Membro { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public string? Status { get; set; }
        public Color? StatusColor { get; set; }
    }
}
