
using BibliotecaAppBase.Models;

namespace BibliotecaAPP.Views
{
    public partial class CadastroLivroPage : ContentPage
    {
        public CadastroLivroPage()
        {
            InitializeComponent();
        }

        private async void OnSalvarLivroClicked(object sender, EventArgs e)
        {
            string titulo = TituloEntry.Text;
            string autor = AutorEntry.Text;
            string categoria = CategoriaEntry.Text;
            string? disponibilidade = DisponibilidadePicker.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(titulo) ||
                string.IsNullOrWhiteSpace(autor) ||
                string.IsNullOrWhiteSpace(categoria) ||
                string.IsNullOrWhiteSpace(disponibilidade))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            var novoLivro = new Livro
            {
                Titulo = titulo,
                Autor = autor,
                Categoria = categoria,
                Disponibilidade = disponibilidade
            };

            // Aqui futuramente você chamará algo como:
            // await _livroRepository.AdicionarAsync(novoLivro);

            await DisplayAlert("Sucesso", $"Livro '{novoLivro.Titulo}' cadastrado com sucesso!", "OK");

            // Limpar os campos
            TituloEntry.Text = string.Empty;
            AutorEntry.Text = string.Empty;
            CategoriaEntry.Text = string.Empty;
            DisponibilidadePicker.SelectedIndex = -1;
        }
    }
}
