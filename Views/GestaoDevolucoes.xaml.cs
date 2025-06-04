using BibliotecaAPP.Data;
using BibliotecaAPP.Models;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace BibliotecaAPP.Views
{
    public partial class GestaoDevolucoes : ContentPage
    {
        private readonly IMembroRepository _membroRepository;
        private readonly IEmprestimoRepository _emprestimoRepository;
        private List<Membro> membros;
        private EmprestimoDetalhado? emprestimoSelecionado;

        public GestaoDevolucoes()
        {
            InitializeComponent();

            // Inicializar repositórios usando suas classes existentes
            _membroRepository = new MembroRepository();
            _emprestimoRepository = new EmprestimoRepository();
            membros = new List<Membro>();

            // Inicializar a tela
            InicializarTela();

            // Carregar membros
            _ = CarregarMembrosAsync();
        }

        private void InicializarTela()
        {
            // Estado inicial da tela
            lblTituloLivro.Text = "Selecione um membro";
            lblNomeMembro.Text = "Aguardando seleção...";
            entryDataDevolucao.Text = "--/--/----";
            lblAtraso.Text = "Sem informações";
            lblAtraso.TextColor = Colors.Gray;
            frameAtraso.BackgroundColor = Color.FromArgb("#F0F0F0");

            // Limpar campos
            estadoLivroPicker.SelectedIndex = -1;
            justificativaEditor.Text = "";
            chkPago.IsChecked = false;
            chkIsentar.IsChecked = false;
        }

        private async Task CarregarMembrosAsync()
        {
            try
            {
                // Carregar membros do banco usando seu repository
                membros = await _membroRepository.ObterTodosAsync();
                membroPicker.ItemsSource = membros;

                // Configurar exibição do nome no picker
                membroPicker.ItemDisplayBinding = new Binding("Nome");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao carregar membros: {ex.Message}", "OK");
            }
        }

        private async void OnMembroSelecionadoChanged(object sender, EventArgs e)
        {
            var membroSelecionado = membroPicker.SelectedItem as Membro;
            if (membroSelecionado == null)
            {
                InicializarTela();
                return;
            }

            try
            {
                // Mostrar que está carregando
                lblTituloLivro.Text = "Carregando...";
                lblNomeMembro.Text = "Buscando empréstimos...";

                var emprestimosAtivos = await _emprestimoRepository.ObterEmprestimosAtivosPorMembroAsync(membroSelecionado.ID);

                if (emprestimosAtivos.Count == 0)
                {
                    // Nenhum empréstimo ativo
                    lblTituloLivro.Text = "Nenhum empréstimo ativo";
                    lblNomeMembro.Text = $"{membroSelecionado.Nome} (ID: {membroSelecionado.ID:D5})";
                    entryDataDevolucao.Text = "--/--/----";
                    lblAtraso.Text = "Sem empréstimos";
                    lblAtraso.TextColor = Colors.Green;
                    frameAtraso.BackgroundColor = Color.FromArgb("#E8F5E8");

                    emprestimoSelecionado = null;
                    await DisplayAlert("Informação", "Este membro não possui empréstimos ativos.", "OK");
                    return;
                }

                // Se há apenas um empréstimo, seleciona automaticamente
                if (emprestimosAtivos.Count == 1)
                {
                    emprestimoSelecionado = emprestimosAtivos[0];
                    ExibirInformacoesEmprestimo();
                }
                else
                {
                    // Múltiplos empréstimos - mostrar lista para escolha
                    var opcoes = emprestimosAtivos.Select(e => $"{e.TituloLivro} (Vence: {e.DataDevolucao:dd/MM/yyyy})").ToArray();
                    var escolha = await DisplayActionSheet("Este membro tem múltiplos empréstimos. Selecione o livro:", "Cancelar", null, opcoes);

                    if (escolha != "Cancelar" && escolha != null)
                    {
                        var emprestimoEscolhido = emprestimosAtivos[Array.IndexOf(opcoes, escolha)];
                        emprestimoSelecionado = emprestimoEscolhido;
                        ExibirInformacoesEmprestimo();
                    }
                    else
                    {
                        InicializarTela();
                        membroPicker.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao carregar empréstimos: {ex.Message}", "OK");
                InicializarTela();
            }
        }

        private void ExibirInformacoesEmprestimo()
        {
            if (emprestimoSelecionado == null) return;

            // Atualizar informações na tela
            lblTituloLivro.Text = emprestimoSelecionado.TituloLivro;
            lblNomeMembro.Text = $"{emprestimoSelecionado.NomeMembro} (ID: {emprestimoSelecionado.MembroId:D5})";
            entryDataDevolucao.Text = emprestimoSelecionado.DataDevolucao.ToString("dd/MM/yyyy");

            // Configurar status de atraso
            if (emprestimoSelecionado.DiasAtraso > 0)
            {
                lblAtraso.Text = $"Atraso: {emprestimoSelecionado.DiasAtraso} dias";
                lblAtraso.TextColor = Color.FromArgb("#B85C00"); // Laranja
                frameAtraso.BackgroundColor = Color.FromArgb("#FFF4E5"); // Fundo laranja claro
            }
            else
            {
                lblAtraso.Text = "No prazo";
                lblAtraso.TextColor = Color.FromArgb("#28A745"); // Verde
                frameAtraso.BackgroundColor = Color.FromArgb("#E8F5E8"); // Fundo verde claro
            }

            // Mostrar informações detalhadas
            var detalhes = $"📚 Livro: {emprestimoSelecionado.TituloLivro}\n" +
                          $"👤 Membro: {emprestimoSelecionado.NomeMembro}\n" +
                          $"📅 Empréstimo: {emprestimoSelecionado.DataEmprestimo:dd/MM/yyyy}\n" +
                          $"🗓️ Devolução: {emprestimoSelecionado.DataDevolucao:dd/MM/yyyy}\n" +
                          $"⏰ Status: {emprestimoSelecionado.StatusAtraso}";

            if (emprestimoSelecionado.DiasAtraso > 0)
            {
                detalhes += $"\n💰 Multa: R$ {emprestimoSelecionado.ValorMulta:F2}";
            }

            DisplayAlert("Detalhes do Empréstimo", detalhes, "OK");
        }

        private async void OnConfirmarDevolucaoClicked(object sender, System.EventArgs e)
        {
            if (emprestimoSelecionado == null)
            {
                await DisplayAlert("Erro", "Nenhum empréstimo selecionado para devolução.", "OK");
                return;
            }

            var membroSelecionado = membroPicker.SelectedItem as Membro;
            var estadoLivro = estadoLivroPicker.SelectedItem as string;
            var justificativa = justificativaEditor.Text ?? "";

            if (membroSelecionado == null)
            {
                await DisplayAlert("Erro", "Por favor, selecione um membro.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(estadoLivro))
            {
                await DisplayAlert("Erro", "Por favor, selecione o estado do livro.", "OK");
                return;
            }

            // Verificar multa se houver atraso
            if (emprestimoSelecionado.DiasAtraso > 0 && !chkPago.IsChecked && !chkIsentar.IsChecked)
            {
                await DisplayAlert("Multa Pendente",
                    $"Este empréstimo possui uma multa de R$ {emprestimoSelecionado.ValorMulta:F2} " +
                    $"por {emprestimoSelecionado.DiasAtraso} dias de atraso.\n\n" +
                    "Para prosseguir, marque uma das opções: 'Pago' ou 'Isentar Multa'", "OK");
                return;
            }

            // Confirmação final
            var statusMulta = "";
            if (emprestimoSelecionado.DiasAtraso > 0)
            {
                statusMulta = chkPago.IsChecked ? "\n💰 Multa: PAGA" : "\n💰 Multa: ISENTA";
            }

            var confirmacao = $"Confirmar devolução?\n\n" +
                             $" Livro: {emprestimoSelecionado.TituloLivro}\n" +
                             $" Membro: {membroSelecionado.Nome}\n" +
                             $" Estado: {estadoLivro}{statusMulta}";

            if (!string.IsNullOrWhiteSpace(justificativa))
            {
                confirmacao += $"\n Justificativa: {justificativa}";
            }

            bool confirmar = await DisplayAlert("Confirmar Devolução", confirmacao, "Sim", "Não");
            if (!confirmar) return;

            try
            {
                bool sucesso = await _emprestimoRepository.RealizarDevolucaoAsync(
                    emprestimoSelecionado.EmprestimoId,
                    estadoLivro,
                    justificativa);

                if (sucesso)
                {
                    await DisplayAlert(" Sucesso", "Devolução realizada com sucesso!", "OK");
                    LimparFormulario();
                }
                else
                {
                    await DisplayAlert(" Erro", "Falha ao realizar a devolução.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Erro", $"Erro ao processar devolução:\n{ex.Message}", "OK");
            }
        }

        private void LimparFormulario()
        {
            membroPicker.SelectedIndex = -1;
            estadoLivroPicker.SelectedIndex = -1;
            justificativaEditor.Text = "";
            chkPago.IsChecked = false;
            chkIsentar.IsChecked = false;

            InicializarTela();
            emprestimoSelecionado = null;
        }

        private void OnCancelarClicked(object sender, System.EventArgs e)
        {
            LimparFormulario();
        }
    }
}
