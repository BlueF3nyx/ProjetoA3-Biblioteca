using BibliotecaAppBase.Models;
using BibliotecaAPP.Data;  
using System;

namespace BibliotecaAPP.Views
{
    public partial class CadastroLivroPage : ContentPage
    {
        private readonly ILivroRepository _livroRepository;

        public CadastroLivroPage()
        {
            InitializeComponent();

            // Instancie o repositório (ou injete via construtor se preferir)
            _livroRepository = new LivroRepository();
        }

        private async void OnSalvarLivroClicked(object sender, EventArgs e)
        {
            string titulo = TituloEntry.Text;
            string autor = AutorEntry.Text;
            string categoria = CategoriaEntry.Text;
            string? disponibilidadeSelecionada = DisponibilidadePicker.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(titulo) ||
                string.IsNullOrWhiteSpace(autor) ||
                string.IsNullOrWhiteSpace(categoria) ||
                string.IsNullOrWhiteSpace(disponibilidadeSelecionada))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            // Converte "Disponível" => "Sim", "Emprestado" => "Não"
            string disponibilidadeParaSalvar = disponibilidadeSelecionada == "Disponível" ? "Sim" : "Não";

            var novoLivro = new Livro
            {
                Titulo = titulo,
                Autor = autor,
                Categoria = categoria,
                Disponibilidade = disponibilidadeParaSalvar
            };

            // Aqui você chama o repositório para salvar no banco
            await _livroRepository.AdicionarAsync(novoLivro);

            await DisplayAlert("Sucesso", $"Livro '{novoLivro.Titulo}' cadastrado com sucesso!", "OK");

            // Limpar os campos
            TituloEntry.Text = string.Empty;
            AutorEntry.Text = string.Empty;
            CategoriaEntry.Text = string.Empty;
            DisponibilidadePicker.SelectedIndex = -1;
        }

    }
}
