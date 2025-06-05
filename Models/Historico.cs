using System.ComponentModel;
namespace BibliotecaAPP.Models
{
    public class Historico : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int IdLivro { get; set; }
        public int IdMembro { get; set; }
        public string? TituloLivro { get; set; }
        public string? NomeMembro { get; set; }
        public DateTime DataEmprestimo { get; set; }
        public DateTime DataDevolucaoPrevista { get; set; }
        public DateTime? DataDevolucaoReal { get; set; }

        private string? _status;
        public string? Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusFormatado));
                OnPropertyChanged(nameof(StatusCorFundo));
                OnPropertyChanged(nameof(StatusCorTexto));
                OnPropertyChanged(nameof(FoiDevolvido));
            }
        }

        // Propriedades para a UI
        public string StatusFormatado => Status switch
        {
            "Emprestado" => " EMPRESTADO",
            "Devolvido" => " DEVOLVIDO",
            "Pendente" => " PENDENTE",
            "Atrasado" => " ATRASADO",
            _ => " INDEFINIDO"
        };

        public Color StatusCorFundo => Status switch
        {
            "Emprestado" => Color.FromArgb("#E3F2FD"),
            "Devolvido" => Color.FromArgb("#E8F5E8"),
            "Pendente" => Color.FromArgb("#FFF3E0"),
            "Atrasado" => Color.FromArgb("#FFEBEE"),
            _ => Color.FromArgb("#F5F5F5")
        };

        public Color StatusCorTexto => Status switch
        {
            "Emprestado" => Color.FromArgb("#1976D2"),
            "Devolvido" => Color.FromArgb("#388E3C"),
            "Pendente" => Color.FromArgb("#FFF3E0"),      
            "Atrasado" => Color.FromArgb("#FFEBEE"),
            _ => Color.FromArgb("#616161")
        };

        public bool FoiDevolvido => DataDevolucaoReal.HasValue;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
