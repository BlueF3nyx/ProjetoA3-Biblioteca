using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPP.Models
{
    
        public class EmprestimoDetalhado
        {
            public int EmprestimoId { get; set; }
            public int MembroId { get; set; }
            public string? NomeMembro { get; set; }
            public int LivroId { get; set; }
            public string? TituloLivro { get; set; }
            public DateTime DataEmprestimo { get; set; }
            public DateTime DataDevolucao { get; set; }
            public string? Status { get; set; }

            // Propriedades calculadas
            public int DiasAtraso => DataDevolucao < DateTime.Now ? (DateTime.Now - DataDevolucao).Days : 0;
            public string StatusAtraso => DiasAtraso > 0 ? "ATRASADO" : "NO PRAZO";
            public decimal ValorMulta => DiasAtraso * 2.00m; // R$ 2,00 por dia
        }
   
}
