namespace BibliotecaAPP.Models { 
    // Usado para exibir informações detalhadas de um empréstimo na UI.

    public class EmprestimoDetalhado
    {
        
        public int EmprestimoId { get; set; }
        public int LivroId { get; set; } 
        public int MembroId { get; set; } 
        public DateTime DataEmprestimo { get; set; }
        public DateTime DataDevolucaoPrevista { get; set; }
        public DateTime? DataDevolucaoReal { get; set; }

        public string? Status { get; set; }
        
        public string? TituloLivro { get; set; }
        public string? NomeMembro { get; set; }

        // O status de exibição (Atrasado, Pendente, Emprestado, Devolvido)
        public string StatusExibicao
        {
            get
            {
                if (DataDevolucaoReal.HasValue && DataDevolucaoReal.Value != DateTime.MinValue)
                    return "Devolvido";

                var hoje = DateTime.Now.Date;
                var previsao = DataDevolucaoPrevista.Date;

                if (hoje > previsao)
                    return "Atrasado";

                if ((previsao - hoje).Days >= 0 && (previsao - hoje).Days <= 1)
                    return "Pendente";

                return "Emprestado";
            }
        }

        // Propriedade calculada para os dias de atraso 
        public int DiasAtraso
        {
            get
            {
                if (DataDevolucaoReal.HasValue && DataDevolucaoReal.Value != DateTime.MinValue)
                    return 0; // Não há atraso se já foi devolvido

                var hoje = DateTime.Now.Date;
                var previsao = DataDevolucaoPrevista.Date;

                if (hoje > previsao)
                {
                    return (hoje - previsao).Days;
                }

                return 0; // Não está atrasado
            }
        }

        
    }
}
