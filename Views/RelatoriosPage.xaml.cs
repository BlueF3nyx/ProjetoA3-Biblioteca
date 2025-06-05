// No arquivo RelatoriosPage.xaml.cs
using System;
using Microsoft.Maui.Controls;
using BibliotecaAPP.Core.Data; // Importe o namespace do seu repositório
using BibliotecaAPP.Core.Models; // Importe o namespace dos seus modelos
using System.Collections.Generic; // Para List
using System.Text; // Para StringBuilder

namespace BibliotecaAPP.Views
{
    public partial class RelatoriosPage : ContentPage
    {
        private readonly IEmprestimoRepository _emprestimoRepository; // Campo para o repositório

        // Construtor que aceita o repositório (usando injeção de dependência)
        public RelatoriosPage(IEmprestimoRepository emprestimoRepository)
        {
            InitializeComponent();
            _emprestimoRepository = emprestimoRepository; // Atribui a instância injetada

            // Pode definir valores padrão para os DatePickers, por exemplo:
            dataInicioPicker.Date = DateTime.Today.AddMonths(-1); // 1 mês atrás
            dataFimPicker.Date = DateTime.Today; // Hoje
        }

        // Torne o método async
        private async void OnGerarRelatorioClicked(object sender, EventArgs e)
        {
            // Pega os filtros selecionados
            DateTime dataInicio = dataInicioPicker.Date;
            DateTime dataFim = dataFimPicker.Date;
            string tipoRelatorio = tipoRelatorioPicker.SelectedItem as string;

            if (string.IsNullOrEmpty(tipoRelatorio))
            {
                await DisplayAlert("Atenção", "Por favor, selecione o tipo de relatório.", "OK");
                return;
            }

            if (dataInicio > dataFim)
            {
                await DisplayAlert("Atenção", "A data de início não pode ser maior que a data final.", "OK");
                return;
            }

            // Limpa a área de relatório e mostra uma mensagem de carregamento (opcional)
            relatorioLabel.Text = "Gerando relatório...";
            // Pode adicionar um ActivityIndicator aqui se quiser

            try
            {
                // ✅ Chama o método do repositório para obter os dados
                List<EmprestimoDetalhado> dadosRelatorio = await _emprestimoRepository.ObterRelatorioEmprestimosAsync(dataInicio, dataFim, tipoRelatorio);

                // ✅ Processa os resultados e formata para exibição
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Relatório: {tipoRelatorio}");
                sb.AppendLine($"Período: {dataInicio:dd/MM/yyyy} até {dataFim:dd/MM/yyyy}");
                sb.AppendLine($"Total encontrado: {dadosRelatorio.Count}");
                sb.AppendLine("----------------------------------------");

                if (dadosRelatorio.Count == 0)
                {
                    sb.AppendLine("Nenhum registro encontrado para os filtros selecionados.");
                }
                else
                {
                    foreach (var item in dadosRelatorio)
                    {
                        sb.AppendLine($"Livro: {item.TituloLivro}");
                        sb.AppendLine($"Membro: {item.NomeMembro}");
                        sb.AppendLine($"Empréstimo: {item.DataEmprestimo:dd/MM/yyyy}");
                        sb.AppendLine($"Prev. Devolução: {item.DataDevolucaoPrevista:dd/MM/yyyy}");
                        // Exibe a DataDevolucaoReal se existir
                        if (item.DataDevolucaoReal.HasValue && item.DataDevolucaoReal.Value != DateTime.MinValue)
                        {
                            sb.AppendLine($"Devolução Real: {item.DataDevolucaoReal.Value:dd/MM/yyyy}");
                        }
                        // Usa a propriedade calculada para exibir o status atual
                        sb.AppendLine($"Status: {item.StatusExibicao}");
                        // Exibe dias de atraso se aplicável
                        if (item.DiasAtraso > 0)
                        {
                            sb.AppendLine($"Dias Atraso: {item.DiasAtraso}");
                        }
                        sb.AppendLine("---"); // Separador entre os itens
                    }
                }

                // Exibe o relatório formatado no Label
                relatorioLabel.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                // Trata erros de banco de dados ou outros
                await DisplayAlert("Erro", $"Ocorreu um erro ao gerar o relatório: {ex.Message}", "OK");
                relatorioLabel.Text = "Erro ao gerar relatório."; // Limpa ou indica erro na UI
            }
            finally
            {
                // Esconde o ActivityIndicator se estiver usando
            }
        }
    }
}
