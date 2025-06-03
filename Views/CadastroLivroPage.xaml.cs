using BibliotecaAPP.Data;
using BibliotecaAppBase.Models;
using System.Collections.ObjectModel;

namespace BibliotecaAPP.Views;

public partial class CadastroLivroPage : ContentPage
{
    private readonly ILivroRepository _livroRepository;
    private ObservableCollection<Livro> livros;
    private Livro livroSelecionado = null;

    public CadastroLivroPage()
    {
        InitializeComponent();

        _livroRepository = new LivroRepository();

        livros = new ObservableCollection<Livro>();
        LivrosListView.ItemsSource = livros;

        CarregarLivrosAsync();
    }

    private async Task CarregarLivrosAsync()
    {
        var lista = await _livroRepository.ObterTodosAsync();
        livros.Clear();

        foreach (var livro in lista)
        {
            livros.Add(livro);
        }
    }

    private async void OnSalvarLivroClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TituloEntry.Text) || string.IsNullOrWhiteSpace(AutorEntry.Text))
        {
            await DisplayAlert("Erro", "Preencha os campos obrigatórios!", "OK");
            return;
        }

        var disponibilidade = DisponibilidadePicker.SelectedItem?.ToString() ?? "Disponível";

        if (livroSelecionado == null)
        {
            var novoLivro = new Livro
            {
                Titulo = TituloEntry.Text,
                Autor = AutorEntry.Text,
                Categoria = CategoriaEntry.Text,
                Disponibilidade = disponibilidade
            };

            await _livroRepository.AdicionarAsync(novoLivro);
            await DisplayAlert("Sucesso", "Livro adicionado.", "OK");
        }
        else
        {
            livroSelecionado.Titulo = TituloEntry.Text;
            livroSelecionado.Autor = AutorEntry.Text;
            livroSelecionado.Categoria = CategoriaEntry.Text;
            livroSelecionado.Disponibilidade = disponibilidade;

            await _livroRepository.AtualizarAsync(livroSelecionado);
            await DisplayAlert("Sucesso", "Livro atualizado.", "OK");
        }

        await CarregarLivrosAsync();
        LimparFormulario();
    }

    private void OnEditarLivroClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var livro = button?.BindingContext as Livro;

        if (livro != null)
        {
            livroSelecionado = livro;

            TituloEntry.Text = livro.Titulo;
            AutorEntry.Text = livro.Autor;
            CategoriaEntry.Text = livro.Categoria;
            DisponibilidadePicker.SelectedItem = livro.Disponibilidade;

            SalvarButton.Text = "Atualizar Livro";
            CancelarEdicaoButton.IsVisible = true;
        }
    }

    private async void OnExcluirLivroClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var livro = button?.BindingContext as Livro;

        if (livro != null)
        {
            bool confirm = await DisplayAlert("Confirmação", $"Deseja excluir o livro '{livro.Titulo}'?", "Sim", "Não");
            if (confirm)
            {
                await _livroRepository.ExcluirAsync(livro.ID);
                await DisplayAlert("Sucesso", "Livro excluído.", "OK");

                if (livro == livroSelecionado)
                {
                    LimparFormulario();
                }

                await CarregarLivrosAsync();
            }
        }
    }

    private void OnCancelarEdicaoClicked(object sender, EventArgs e)
    {
        LimparFormulario();
    }

    private void LimparFormulario()
    {
        livroSelecionado = null;

        TituloEntry.Text = "";
        AutorEntry.Text = "";
        CategoriaEntry.Text = "";
        DisponibilidadePicker.SelectedIndex = -1;

        SalvarButton.Text = "Salvar Livro";
        CancelarEdicaoButton.IsVisible = false;
    }
    private void OnLivroSelecionado(object sender, SelectedItemChangedEventArgs e)
    {
        // Handle the event when a book is selected  
        if (e.SelectedItem != null)
        {
            // Example: Display the selected book's title  
            var selectedBook = e.SelectedItem as Livro;
            DisplayAlert("Livro Selecionado", $"Título: {selectedBook?.Titulo}", "OK");

            // Optionally, deselect the item  
            ((ListView)sender).SelectedItem = null;
        }
    }
}
