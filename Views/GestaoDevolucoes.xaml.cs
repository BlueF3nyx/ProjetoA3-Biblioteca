using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace BibliotecaAPP.Views
{
    public partial class GestaoDevolucoes : ContentPage
    {
        // Classe simples para representar um membro
        public class Membro
        {
            public int? Id { get; set; }
            public string? Nome { get; set; }
            public override string? ToString() => Nome;  // Para mostrar no Picker
        }

        // Lista de membros simulada
        private List<Membro> membros = new List<Membro>
        {
            new Membro { Id = 1, Nome = "João da Silva" },
            new Membro { Id = 2, Nome = "Maria Oliveira" },
            new Membro { Id = 3, Nome = "Carlos Souza" }
        };

        public GestaoDevolucoes()
        {
            InitializeComponent();

            // Popula o Picker de membros
            membroPicker.ItemsSource = membros;

            // Opcional: Seleciona o primeiro membro por padrão
            if (membros.Count > 0)
                membroPicker.SelectedIndex = 0;

            // Opcional: Se quiser popular estadoLivroPicker via código, faça aqui
            // estadoLivroPicker.ItemsSource = new List<string> { "Bom estado", "Danificado", "Perdido" };
        }

        private void OnConfirmarDevolucaoClicked(object sender, System.EventArgs e)
        {
            var membroSelecionado = membroPicker.SelectedItem as Membro;
            var estadoLivro = estadoLivroPicker.SelectedItem as string;
            var justificativa = justificativaEditor.Text ?? "";
            var multaPago = chkPago.IsChecked;
            var multaIsentar = chkIsentar.IsChecked;

            if (membroSelecionado == null)
            {
                DisplayAlert("Erro", "Por favor, selecione um membro.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(estadoLivro))
            {
                DisplayAlert("Erro", "Por favor, selecione o estado do livro.", "OK");
                return;
            }

            // Aqui você pode salvar no banco, enviar dados, etc.
            // Por enquanto, só mostrar um resumo

            string mensagem =
                $"Membro: {membroSelecionado.Nome}\n" +
                $"Estado do Livro: {estadoLivro}\n" +
                $"Justificativa: {justificativa}\n" +
                $"Multa Pago: {multaPago}\n" +
                $"Isentar Multa: {multaIsentar}";

            DisplayAlert("Devolução Confirmada", mensagem, "OK");
        }

        private void OnCancelarClicked(object sender, System.EventArgs e)
        {
            // Limpa os campos ou volta para a tela anterior
            membroPicker.SelectedIndex = -1;
            estadoLivroPicker.SelectedIndex = -1;
            justificativaEditor.Text = "";
            chkPago.IsChecked = false;
            chkIsentar.IsChecked = false;
        }
    }
}
