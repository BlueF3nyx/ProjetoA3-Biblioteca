using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using BibliotecaAPP.Models;
using BibliotecaAPP.Data;
using System.Runtime.CompilerServices;

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
                return "Devolvido"; // ✅ Verde

            
            var hoje = DateTime.Now.Date;

            
            if (emprestimo.DataDevolucaoPrevista == DateTime.MinValue)
            {
                
                return "Emprestado (Sem Previsão)"; // Ou outro status padrão
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

        
        private async Task CriarDadosDeTesteParaCores()
        {
            try
            {
                
                var emprestimosExistentes = await _emprestimoRepository.ObterTodosAsync();
                if (emprestimosExistentes.Any())
                {
                    await DisplayAlert("ℹ️ Info", "Dados de teste já existem! Limpe o banco para recriar.", "OK");
                    return;
                }

               
                var livroRepo = new LivroRepository(); 
                var membroRepo = new MembroRepository(); 
                var livros = await livroRepo.ObterTodosAsync(); 
                var membros = await membroRepo.ObterTodosAsync(); 

                if (!livros.Any())
                {
                    await DisplayAlert("❌ Erro", "Cadastre pelo menos 1 livro primeiro!", "OK");
                    return;
                }
                if (!membros.Any())
                {
                    await DisplayAlert("❌ Erro", "Cadastre pelo menos 1 membro primeiro!", "OK");
                    return;
                }

                
                var livroId = livros.First().ID; 
                var membroId = membros.First().ID; 

              
                var demos = new[]
                {
                    
                    new Emprestimo 
                    {
                        IdLivro = livroId,
                        IdMembro = membroId,
                        DataEmprestimo = DateTime.Now.AddDays(-30), // Emprestado há 30 dias
                        DataDevolucaoPrevista = DateTime.Now.AddDays(-5), // Previsão era há 5 dias
                        DataDevolucaoReal = null, // Ainda não devolvido
                        Status = "Ativo" // Status no banco (pode ser 'Ativo' ou 'Atrasado' dependendo da sua lógica de persistência)
                    },
                    // 🧡 PENDENTE (DataDevolucaoPrevista hoje ou amanhã, DataDevolucaoReal é NULL)
                    new Emprestimo
                    {
                        IdLivro = livroId,
                        IdMembro = membroId,
                        DataEmprestimo = DateTime.Now.AddDays(-7), // Emprestado há 7 dias
                        DataDevolucaoPrevista = DateTime.Now.Date.AddDays(1), // Previsão para amanhã (use .Date para comparar apenas a data)
                        DataDevolucaoReal = null, // Ainda não devolvido
                        Status = "Ativo" // Status no banco
                    },
                     // 📖 EMPRESTADO (DataDevolucaoPrevista no futuro, DataDevolucaoReal é NULL)
                    new Emprestimo
                    {
                        IdLivro = livroId,
                        IdMembro = membroId,
                        DataEmprestimo = DateTime.Now.AddDays(-3), // Emprestado há 3 dias
                        DataDevolucaoPrevista = DateTime.Now.AddDays(15), // Previsão para daqui 15 dias
                        DataDevolucaoReal = null, // Ainda não devolvido
                        Status = "Ativo" // Status no banco
                    },
                    // ✅ DEVOLVIDO (DataDevolucaoReal tem valor)
                    new Emprestimo
                    {
                        IdLivro = livroId,
                        IdMembro = membroId,
                        DataEmprestimo = DateTime.Now.AddDays(-10), // Emprestado há 10 dias
                        DataDevolucaoPrevista = DateTime.Now.AddDays(-3), // Previsão era há 3 dias (pode ser antes ou depois da real)
                        DataDevolucaoReal = DateTime.Now.AddDays(-1), // Devolvido há 1 dia
                        Status = "Devolvido" // Status no banco
                    }
                };
                foreach (var demo in demos)
                {
                    // ✅ Adiciona os empréstimos de teste usando o repositório
                    await _emprestimoRepository.AdicionarAsync(demo); // Assumindo que AdicionarAsync recebe Emprestimo
                }
                await DisplayAlert("✅ Sucesso", "Dados de teste criados com sucesso!", "OK");
                await CarregarHistoricoAsync(); // Recarrega a lista para mostrar os novos dados
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Erro", $"Erro ao criar dados de teste: {ex.Message}", "OK");
            }
        }

        // ✅ Evento Click do botão de teste (adicione este botão no seu XAML)
        private async void OnCriarDemoClicked(object sender, EventArgs e)
        {
            await CriarDadosDeTesteParaCores();
        }
    }
}
