using System;
using Microsoft.Maui.Controls;

namespace BibliotecaAPP.Views
{
    public partial class RelatoriosPage : ContentPage
    {
        public RelatoriosPage()
        {
            InitializeComponent();

            // Pode definir valores padrão para os DatePickers, por exemplo:
            dataInicioPicker.Date = DateTime.Today.AddMonths(-1);  // 1 mês atrás
            dataFimPicker.Date = DateTime.Today;                   // Hoje
        }

        private void OnGerarRelatorioClicked(object sender, EventArgs e)
        {
            // Pega os filtros selecionados
            DateTime dataInicio = dataInicioPicker.Date;
            DateTime dataFim = dataFimPicker.Date;
            string tipoRelatorio = tipoRelatorioPicker.SelectedItem as string;

            if (string.IsNullOrEmpty(tipoRelatorio))
            {
                DisplayAlert("Atenção", "Por favor, selecione o tipo de relatório.", "OK");
                return;
            }

            if (dataInicio > dataFim)
            {
                DisplayAlert("Atenção", "A data de início não pode ser maior que a data final.", "OK");
                return;
            }

            // Aqui você pode fazer a chamada para buscar os dados do relatório
            // Vou simular um texto simples para exibir no Label
            relatorioLabel.Text = $"Relatório: {tipoRelatorio}\n" +
                                 $"Período: {dataInicio:dd/MM/yyyy} até {dataFim:dd/MM/yyyy}\n\n" +
                                 "Aqui aparecerão os dados do relatório...";

            // Se quiser, pode colocar lógica para mostrar um spinner, carregar dados, etc.
        }
    }
}
