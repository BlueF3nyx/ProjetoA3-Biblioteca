using System.ComponentModel;
namespace BibliotecaAPP.Core.Models
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


        public bool FoiDevolvido => DataDevolucaoReal.HasValue;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
