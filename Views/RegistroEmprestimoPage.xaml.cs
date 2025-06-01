using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using BibliotecaAPP.Models;

namespace BibliotecaAPP.Views
{
    public partial class RegistroEmprestimoPage : ContentPage
    {
        // Classe para representar os itens na lista de emprestimos
        public class EmprestimoItem
        {
            public string Livro { get; set; }
            public int Duracao { get; set; }
            public DateTime DataEmprestimo { get; set; }
            public DateTime DataDevolucao { get; set; }
        }

        // Coleção observável para os itens
        public ObservableCollection<EmprestimoItem> ItensEmprestimo { get; } = new ObservableCollection<EmprestimoItem>();

        public RegistroEmprestimoPage()
        {
            InitializeComponent();
            
            // Definir a fonte de dados para a CollectionView
            LivrosCollectionView.ItemsSource = ItensEmprestimo;
            
            // Configurar data inicial para hoje
            DatePickerEmprestimo.Date = DateTime.Today;
            
            // Calcular data de devolucao
            CalcularDataDevolucao();
            
            // Eventos para calcular a data de devolucao quando valores mudarem
            DatePickerEmprestimo.DateSelected += (s, e) => CalcularDataDevolucao();
            TextBoxDuracao.TextChanged += (s, e) => CalcularDataDevolucao();
        }

        private void CalcularDataDevolucao()
        {
            if (int.TryParse(TextBoxDuracao.Text, out int duracao))
            {
                DatePickerDevolucao.Date = DatePickerEmprestimo.Date.AddDays(duracao);
            }
            else
            {
                DatePickerDevolucao.Date = DatePickerEmprestimo.Date;
            }
        }

        private void AdicionarLivro_Click(object sender, EventArgs e)
        {
            // Validar campos
            if (ComboBoxLivros.SelectedItem == null || 
                string.IsNullOrWhiteSpace(TextBoxDuracao.Text) ||
                !int.TryParse(TextBoxDuracao.Text, out int duracao))
            {
                DisplayAlert("Erro", "Selecione um livro e informe uma duração válida.", "OK");
                return;
            }

            // Adicionar novo item a colecao
            ItensEmprestimo.Add(new EmprestimoItem
            {
                Livro = ComboBoxLivros.SelectedItem.ToString(),
                Duracao = duracao,
                DataEmprestimo = DatePickerEmprestimo.Date,
                DataDevolucao = DatePickerDevolucao.Date
            });

            // Limpar seleção e campos
            ComboBoxLivros.SelectedItem = null;
            TextBoxDuracao.Text = string.Empty;
        }

        private async void Confirmar_Click(object sender, EventArgs e)
        {
            // Validar campos
            if (ComboBoxMembros.SelectedItem == null)
            {
                await DisplayAlert("Erro", "Selecione um membro.", "OK");
                return;
            }

            if (ItensEmprestimo.Count == 0)
            {
                await DisplayAlert("Erro", "Adicione pelo menos um livro.", "OK");
                return;
            }

            // Aqui voce implementaria a ligica para salvar no banco de dados
            string membro = ComboBoxMembros.SelectedItem.ToString();
            
            // Exemplo de mensagem de sucesso
            await DisplayAlert("Sucesso", $"Empréstimo registrado para {membro} com {ItensEmprestimo.Count} livro(s).", "OK");
            
            // Limpar todos os campos
            ComboBoxMembros.SelectedItem = null;
            ItensEmprestimo.Clear();
        }

        private async void Cancelar_Click(object sender, EventArgs e)
        {
            // Voltar para a pagina anterior
            await Navigation.PopAsync();
        }
    }
}