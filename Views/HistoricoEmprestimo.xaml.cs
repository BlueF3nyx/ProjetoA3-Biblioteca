using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BibliotecaAPP.Models;
using BibliotecaAPP.Data;

namespace BibliotecaAPP.Views
{
    public partial class HistoricoEmprestimo : ContentPage, INotifyPropertyChanged
    {
        private readonly IEmprestimoRepository _emprestimoRepository;
        private ObservableCollection<Historico> _todosEmprestimos;
        private ObservableCollection<Historico> _emprestimosFiltrados;

        public ObservableCollection<Historico> EmprestimosFiltrados
        {
            get => _emprestimosFiltrados;
            set { _emprestimosFiltrados = value; OnPropertyChanged(); }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public HistoricoEmprestimo()
        {
            InitializeComponent();
            _emprestimoRepository = new EmprestimoRepository(); 
            _todosEmprestimos = new ObservableCollection<Historico>();
            _emprestimosFiltrados = new ObservableCollection<Historico>();
            BindingContext = this;
            StatusPicker.SelectedIndex = 0; 
            PeriodoPicker.SelectedIndex = 0; 
            _ = CarregarHistoricoAsync(); 
        }

        private async Task CarregarHistoricoAsync()
        {
            try
            {
                TotalEmprestimosLabel.Text = "Carregando histórico...";
               
                var emprestimos = await _emprestimoRepository.ObterTodosComDetalhesAsync();

                
                var historicoItems = emprestimos.Select(e => new Historico
                {
                    
                    Id = e.EmprestimoId, 
                    IdLivro = e.LivroId, 
                    IdMembro = e.MembroId, 
                    TituloLivro = e.TituloLivro ?? "Livro não encontrado",
                    NomeMembro = e.NomeMembro ?? "Membro não encontrado",
                    DataEmprestimo = e.DataEmprestimo,
                    DataDevolucaoPrevista = e.DataDevolucaoPrevista,
                    DataDevolucaoReal = e.DataDevolucaoReal,
                    
                    Status = DeterminarStatus(e)
                }).OrderByDescending(h => h.DataEmprestimo).ToList();

                _todosEmprestimos.Clear();
                foreach (var item in historicoItems)
                {
                    _todosEmprestimos.Add(item);
                }
                AplicarFiltros(); // Aplica os filtros iniciais após carregar

                TotalEmprestimosLabel.Text = _todosEmprestimos.Count > 0
                    ? $"Total: {_todosEmprestimos.Count} empréstimos"
                    : "Nenhum empréstimo encontrado";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar: {ex.Message}", "OK");
                _todosEmprestimos.Clear();
                EmprestimosFiltrados.Clear();
                TotalEmprestimosLabel.Text = "Erro ao carregar dados";
            }
        }

        
        private string DeterminarStatus(EmprestimoDetalhado emprestimo)
        {
            
            if (emprestimo.DataDevolucaoReal.HasValue && emprestimo.DataDevolucaoReal.Value != DateTime.MinValue)
                return "Devolvido";

            
            var hoje = DateTime.Now.Date;

            
            if (emprestimo.DataDevolucaoPrevista == DateTime.MinValue)
            {
                
                return "Emprestado (Sem Previsão)"; 
            }

            var previsao = emprestimo.DataDevolucaoPrevista.Date;

            if (hoje > previsao)
                return "Atrasado"; 

            
            if ((previsao - hoje).Days >= 0 && (previsao - hoje).Days <= 1)
                return "Pendente"; 


            
            return "Emprestado"; 
        }

        private void OnStatusFiltroChanged(object sender, EventArgs e) => AplicarFiltros();
        private void OnPeriodoFiltroChanged(object sender, EventArgs e) => AplicarFiltros();

        private void OnLimparFiltrosClicked(object sender, EventArgs e)
        {
            StatusPicker.SelectedIndex = 0;
            PeriodoPicker.SelectedIndex = 0;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var filtrados = _todosEmprestimos.AsEnumerable();
            var status = StatusPicker.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(status) && status != "Todos")
            {
                // Filtra pelo status calculado pelo método DeterminarStatus
                filtrados = filtrados.Where(e => e.Status == status);
            }

            var periodo = PeriodoPicker.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(periodo) && periodo != "Todos")
            {
                var dataLimite = ObterDataLimitePeriodo(periodo);
                if (dataLimite.HasValue)
                {
                    // Filtra pela data de empréstimo
                    filtrados = filtrados.Where(e => e.DataEmprestimo.Date >= dataLimite.Value.Date);
                }
            }

            EmprestimosFiltrados.Clear();
            // Ordena novamente após filtrar
            foreach (var item in filtrados.OrderByDescending(e => e.DataEmprestimo))
            {
                EmprestimosFiltrados.Add(item);
            }

            var totalFiltrados = EmprestimosFiltrados.Count;
            var totalGeral = _todosEmprestimos.Count;
            TotalEmprestimosLabel.Text = totalFiltrados == totalGeral
                ? (totalGeral > 0 ? $"Total: {totalGeral} empréstimos" : "Nenhum empréstimo cadastrado")
                : $"Mostrando: {totalFiltrados} de {totalGeral} empréstimos";
        }

        private DateTime? ObterDataLimitePeriodo(string periodo)
        {
            var hoje = DateTime.Now.Date;
            return periodo switch
            {
                "Última semana" => hoje.AddDays(-7),
                "Último mês" => hoje.AddMonths(-1),
                "Últimos 3 meses" => hoje.AddMonths(-3),
                "Este ano" => new DateTime(hoje.Year, 1, 1),
                _ => null // "Todos" ou outro valor
            };
        }

        // Método público para atualizar o histórico de fora (ex: após um empréstimo ser adicionado)
        public async Task AtualizarHistoricoAsync()
        {
            await CarregarHistoricoAsync();
        }

        // Carrega o histórico sempre que a página aparece
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarHistoricoAsync();
        }

        
    }
}
