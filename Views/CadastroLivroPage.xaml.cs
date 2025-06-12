
using BibliotecaAPP.Data;
using BibliotecaAPP.Models;
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
        _ = CarregarLivrosAsync(); 
    }

    private async Task CarregarLivrosAsync()
    {
        try
        {
            var lista = await _livroRepository.ObterTodosAsync();
            livros.Clear();
            foreach (var livro in lista)
            {
                livros.Add(livro);
            }

            // Controlar mensagem de lista vazia
            if (EmptyStateLabel != null)
            {
                EmptyStateLabel.IsVisible = !livros.Any();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar livros: {ex.Message}", "OK");
        }
    }

    private async void OnSalvarLivroClicked(object sender, EventArgs e)
    {
        try
        {
            // Validação
            if (string.IsNullOrWhiteSpace(TituloEntry.Text) || string.IsNullOrWhiteSpace(AutorEntry.Text))
            {
                await DisplayAlert("Erro", "📝 Preencha os campos obrigatórios (Título e Autor)!", "OK");
                return;
            }

            // Desabilitar botão durante operação
            SalvarButton.IsEnabled = false;
            SalvarButton.Text = livroSelecionado == null ? "Salvando..." : "Atualizando...";

            var disponibilidade = DisponibilidadePicker.SelectedItem?.ToString() ?? "Disponível";

            if (livroSelecionado == null)
            {
                //  ADICIONAR NOVO LIVRO
                var novoLivro = new Livro
                {
                    Titulo = TituloEntry.Text.Trim(),
                    Autor = AutorEntry.Text.Trim(),
                    Categoria = CategoriaEntry.Text?.Trim() ?? "",
                    Disponibilidade = disponibilidade
                };

                await _livroRepository.AdicionarAsync(novoLivro);
                await DisplayAlert("Sucesso", $"✅ Livro '{novoLivro.Titulo}' adicionado com sucesso!", "OK");
            }
            else
            {
                //  ATUALIZAR LIVRO EXISTENTE
                livroSelecionado.Titulo = TituloEntry.Text.Trim();
                livroSelecionado.Autor = AutorEntry.Text.Trim();
                livroSelecionado.Categoria = CategoriaEntry.Text?.Trim() ?? "";
                livroSelecionado.Disponibilidade = disponibilidade;

                await _livroRepository.AtualizarAsync(livroSelecionado);
                await DisplayAlert("Sucesso", $"✅ Livro '{livroSelecionado.Titulo}' atualizado com sucesso!", "OK");
            }

            // Recarregar lista e limpar formulário
            await CarregarLivrosAsync();
            LimparFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"❌ Erro ao salvar livro: {ex.Message}", "OK");
        }
        finally
        {
            // Restaurar botão
            SalvarButton.IsEnabled = true;
            SalvarButton.Text = livroSelecionado == null ? "Salvar Livro" : "Atualizar Livro";
        }
    }

    [Obsolete]
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

            // SCROLL PARA O TOPO DO FORMULÁRIO
            _ = Task.Run(async () =>
            {
                await Task.Delay(100);
                Device.BeginInvokeOnMainThread(() =>
                {
                    TituloEntry.Focus();
                });
            });
        }
    }

    private async void OnExcluirClicked(object sender, EventArgs e)
    {
        try
        {
            var button = (Button)sender;
            var livro = (Livro)button.CommandParameter;

            if (livro == null) return;

            // Confirmar exclusão
            bool confirmar = await DisplayAlert("⚠️ Confirmar Exclusão",
                $"Tem certeza que deseja excluir o livro:\n\n📚 {livro.Titulo}\n✍️ {livro.Autor}\n📂 {livro.Categoria}?",
                "Sim, Excluir", "Cancelar");

            if (!confirmar) return;

            // Mostrar loading
            button.IsEnabled = false;
            button.Text = "⏳";

            // Tentar excluir
            var (sucesso, mensagem) = await _livroRepository.ExcluirAsync(livro.ID);

            if (sucesso)
            {
                await DisplayAlert("Sucesso", $"✅ {mensagem}", "OK");

               
                livros.Remove(livro);

                // Se estava editando este livro, limpar formulário
                if (livroSelecionado?.ID == livro.ID)
                {
                    LimparFormulario();
                }

                // Atualizar estado vazio
                if (EmptyStateLabel != null)
                {
                    EmptyStateLabel.IsVisible = !livros.Any();
                }
            }
            else
            {
                await DisplayAlert("Erro", $"❌ {mensagem}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"❌ Erro inesperado: {ex.Message}", "OK");
        }
        finally
        {
            // Restaurar botão
            if (sender is Button btn)
            {
                btn.IsEnabled = true;
                btn.Text = "🗑️";
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
        DisponibilidadePicker.SelectedIndex = 0; 
        SalvarButton.Text = "Salvar Livro";
        CancelarEdicaoButton.IsVisible = false;
    }

    private async void OnLivroSelecionado(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem != null)
        {
            var selectedBook = e.SelectedItem as Livro;

            
            await DisplayAlert(" Detalhes do Livro",
                $" Título: {selectedBook?.Titulo}\n" +
                $" Autor: {selectedBook?.Autor}\n" +
                $" Categoria: {selectedBook?.Categoria}\n" +
                $" Status: {selectedBook?.Disponibilidade}", "OK");

            // Deselecionar item
            ((ListView)sender).SelectedItem = null;
        }
    }

<<<<<<< HEAD
    
=======
    //MÉTODO PARA ATUALIZAR QUANDO A PÁGINA APARECER
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CarregarLivrosAsync();
    }
>>>>>>> 35b5fec31e6ac91e96ddd5a8e7b1eb0a2eb2d6c9
}
