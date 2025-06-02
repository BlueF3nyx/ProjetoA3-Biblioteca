using BibliotecaAPP.Data;
using BibliotecaAPP.Models;
using BibliotecaAppBase.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace BibliotecaAPP.Views
{
    public partial class RegistroEmprestimoPage : ContentPage
    {
        private readonly IMembroRepository _membroRepository;
        private readonly ILivroRepository _livroRepository;
        private readonly IEmprestimoRepository _emprestimoRepository;

        // Usamos essa lista para exibir os livros adicionados com duração
        private ObservableCollection<(Livro Livro, int Duracao)> livrosAdicionados = new();

        public RegistroEmprestimoPage(IMembroRepository membroRepo, ILivroRepository livroRepo, IEmprestimoRepository emprestimoRepo)
        {
            InitializeComponent();

            _membroRepository = membroRepo;
            _livroRepository = livroRepo;
            _emprestimoRepository = emprestimoRepo;

            LivrosCollectionView.ItemsSource = livrosAdicionados;

            DatePickerEmprestimo.Date = DateTime.Today;
            CalcularDataDevolucao();

            DatePickerEmprestimo.DateSelected += (s, e) => CalcularDataDevolucao();
            TextBoxDuracao.TextChanged += (s, e) => CalcularDataDevolucao();

            LoadDataAsync();
        }
       
        private async Task LoadDataAsync()
        {
            try
            {
                var membros = await _membroRepository.ObterTodosAsync();
                ComboBoxMembros.ItemsSource = membros;
                ComboBoxMembros.ItemDisplayBinding = new Binding("Nome");

                var livros = await _livroRepository.ObterTodosAsync();
                ComboBoxLivros.ItemsSource = livros;
                ComboBoxLivros.ItemDisplayBinding = new Binding("Titulo");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Erro ao carregar dados: " + ex.Message, "OK");
            }
        }

        private void CalcularDataDevolucao()
        {
            if (int.TryParse(TextBoxDuracao.Text, out int duracao))
                DatePickerDevolucao.Date = DatePickerEmprestimo.Date.AddDays(duracao);
            else
                DatePickerDevolucao.Date = DatePickerEmprestimo.Date;
        }

        private void AdicionarLivro_Click(object sender, EventArgs e)
        {
            if (ComboBoxLivros.SelectedItem == null || !int.TryParse(TextBoxDuracao.Text, out int duracao))
            {
                DisplayAlert("Erro", "Selecione um livro e informe uma duração válida.", "OK");
                return;
            }

            var livroSelecionado = (Livro)ComboBoxLivros.SelectedItem;

            livrosAdicionados.Add((livroSelecionado, duracao));

            // Atualiza a CollectionView manualmente (se necessário)
            LivrosCollectionView.ItemsSource = null;
            LivrosCollectionView.ItemsSource = livrosAdicionados;

            ComboBoxLivros.SelectedItem = null;
            TextBoxDuracao.Text = string.Empty;
        }

        private async void Confirmar_Click(object sender, EventArgs e)
        {
            if (ComboBoxMembros.SelectedItem == null)
            {
                await DisplayAlert("Erro", "Selecione um membro.", "OK");
                return;
            }

            if (livrosAdicionados.Count == 0)
            {
                await DisplayAlert("Erro", "Adicione pelo menos um livro.", "OK");
                return;
            }

            var membroSelecionado = (Membro)ComboBoxMembros.SelectedItem;

            try
            {
                foreach (var (livro, duracao) in livrosAdicionados)
                {
                    var emprestimo = new Emprestimo
                    {
                        IdLivro = livro.ID,
                        IdMembro = membroSelecionado.ID,
                        DataEmprestimo = DatePickerEmprestimo.Date,
                        DataDevolucao = DatePickerEmprestimo.Date.AddDays(duracao),
                        Status = "Ativo"
                    };

                    await _emprestimoRepository.AdicionarAsync(emprestimo);
                }

                await DisplayAlert("Sucesso", $"Empréstimo registrado para {membroSelecionado.Nome} com {livrosAdicionados.Count} livro(s).", "OK");

                ComboBoxMembros.SelectedItem = null;
                livrosAdicionados.Clear();

                // Atualizar lista da CollectionView
                LivrosCollectionView.ItemsSource = null;
                LivrosCollectionView.ItemsSource = livrosAdicionados;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Falha ao registrar empréstimo: " + ex.Message, "OK");
            }
        }

        private async void Cancelar_Click(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
