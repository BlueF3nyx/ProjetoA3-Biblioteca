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

        //  ObservableCollection pública para binding automático
        public ObservableCollection<string> LivrosAdicionadosTexto { get; set; } = new();

        //  Lista interna para controlar os dados
        private List<(Livro Livro, int Duracao, DateTime DataDevolucao)> livrosAdicionados = new();

        public RegistroEmprestimoPage(IMembroRepository membroRepo, ILivroRepository livroRepo, IEmprestimoRepository emprestimoRepo)
        {
            InitializeComponent();
            _membroRepository = membroRepo;
            _livroRepository = livroRepo;
            _emprestimoRepository = emprestimoRepo;

            //  Binding correto
            LivrosCollectionView.ItemsSource = LivrosAdicionadosTexto;

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

                //  Adicionar evento para atualizar label do membro
                ComboBoxMembros.SelectedIndexChanged += (s, e) => AtualizarMembroSelecionado();

                //  Carregar apenas livros disponíveis
                var livros = await _livroRepository.ObterTodosAsync();
                var livrosDisponiveis = livros.Where(l => l.Disponibilidade == "Disponível").ToList();
                ComboBoxLivros.ItemsSource = livrosDisponiveis;
                ComboBoxLivros.ItemDisplayBinding = new Binding("Titulo");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Erro ao carregar dados: " + ex.Message, "OK");
            }
        }

        //  Método para atualizar o label do membro selecionado
        private void AtualizarMembroSelecionado()
        {
            if (ComboBoxMembros.SelectedItem != null)
            {
                var membro = (Membro)ComboBoxMembros.SelectedItem;
                LabelMembroSelecionado.Text = membro.Nome;
            }
            else
            {
                LabelMembroSelecionado.Text = "Nenhum membro selecionado";
            }
        }

        private void CalcularDataDevolucao()
        {
            if (int.TryParse(TextBoxDuracao.Text, out int duracao))
                DatePickerDevolucao.Date = DatePickerEmprestimo.Date.AddDays(duracao);
            else
                DatePickerDevolucao.Date = DatePickerEmprestimo.Date;
        }

        private async void AdicionarLivro_Click(object sender, EventArgs e)
        {
            if (ComboBoxLivros.SelectedItem == null || !int.TryParse(TextBoxDuracao.Text, out int duracao))
            {
                await DisplayAlert("Erro", "Selecione um livro e informe uma duração válida.", "OK");
                return;
            }

            var livroSelecionado = (Livro)ComboBoxLivros.SelectedItem;

            //  Verificar se o livro já foi adicionado
            if (livrosAdicionados.Any(x => x.Livro.ID == livroSelecionado.ID))
            {
                await DisplayAlert("Erro", "Este livro já foi adicionado à lista.", "OK");
                return;
            }

            var dataDevolucao = DatePickerEmprestimo.Date.AddDays(duracao);

            //  Adicionar à lista interna
            livrosAdicionados.Add((livroSelecionado, duracao, dataDevolucao));

            // Adicionar texto formatado à ObservableCollection
            string textoLivro = $"{livroSelecionado.Titulo} - {duracao} dias (até {dataDevolucao:dd/MM/yyyy})";
            LivrosAdicionadosTexto.Add(textoLivro);

            //  Limpar seleção
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
                foreach (var (livro, duracao, dataDevolucao) in livrosAdicionados)
                {
                    var emprestimo = new Emprestimo
                    {
                        IdLivro = livro.ID,
                        IdMembro = membroSelecionado.ID,
                        DataEmprestimo = DatePickerEmprestimo.Date,
                        DataDevolucao = dataDevolucao,
                        Status = "Ativo"
                    };

                    //  Salvar empréstimo
                    await _emprestimoRepository.AdicionarAsync(emprestimo);

                    //  CRUCIAL: Atualizar status do livro para "Emprestado"
                    livro.Disponibilidade = "Emprestado";
                    await _livroRepository.AtualizarAsync(livro);
                }

                await DisplayAlert("Sucesso", $"Empréstimo registrado com sucesso!\nMembro: {membroSelecionado.Nome}\nLivros: {livrosAdicionados.Count}", "OK");

                //  Limpar formulário
                ComboBoxMembros.SelectedItem = null;
                LabelMembroSelecionado.Text = "Nenhum membro selecionado"; //  Limpar label também
                livrosAdicionados.Clear();
                LivrosAdicionadosTexto.Clear();

                //  Recarregar livros disponíveis
                await LoadDataAsync();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Falha ao registrar empréstimo: " + ex.Message, "OK");
            }
        }

        // Removido o método Cancelar_Click já que não há mais botão cancelar
    }
}
