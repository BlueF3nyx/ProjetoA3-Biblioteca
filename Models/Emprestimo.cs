namespace BibliotecaAPP.Models
{


    public class Emprestimo
    {
        public int ID { get; set; }
        public int IdLivro { get; set; }
        public int IdMembro { get; set; }
        public DateTime DataEmprestimo { get; set; }
        public DateTime DataDevolucaoPrevista { get; set; }
        public DateTime? DataDevolucaoReal { get; set; }
        public string? Status { get; set; }

        
        public string? TituloLivro { get; set; }
        public string OverdueStatusText
        {
            get
            {
                if (DataDevolucaoReal.HasValue)
                {
                    return "Devolvido";
                }
                else if (DataDevolucaoPrevista < DateTime.Today)
                {
                    TimeSpan atraso = DateTime.Today - DataDevolucaoPrevista;
                    return $"Atrasado ({atraso.Days} dias)";
                }
                else if (DataDevolucaoPrevista == DateTime.Today)
                {
                    return "Vence Hoje";
                }
                else
                {
                    TimeSpan restante = DataDevolucaoPrevista - DateTime.Today;
                    // Trata a pluralização para "dia" ou "dias"
                    string diasTexto = restante.Days == 1 ? "dia" : "dias";
                    return $"Faltam {restante.Days} {diasTexto}";
                }
            }
        }

    }
}
