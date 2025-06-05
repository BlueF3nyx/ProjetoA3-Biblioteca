using BibliotecaAPP.Data;
using BibliotecaAPP.Models;
using BibliotecaAppBase.Models;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace BibliotecaAPP.Views
{
    public partial class GestaoDevolucoes : ContentPage
    {
        private readonly IMembroRepository _membroRepository;
        private readonly IEmprestimoRepository _emprestimoRepository;
        private readonly ILivroRepository _livroRepository;
        private List<Membro> membros;
        private EmprestimoDetalhado? emprestimoSelecionado;
        private Livro? livroSelecionado;
       
         public GestaoDevolucoes()
        {
            InitializeComponent();

            membros = new List<Membro>();
            _membroRepository = new MembroRepository(); // ou como você cria normalmente
            _emprestimoRepository = new EmprestimoRepository();
            _livroRepository = new LivroRepository();

            InicializarTela();
            _ = CarregarMembrosAsync();
        }

        private void InicializarTela()
        {
            lblTituloLivro.Text = "Selecione um membro";
            lblNomeMembro.Text = "Aguardando seleção...";
            entryDataDevolucao.Text = "--/--/----";
            lblAtraso.Text = "Sem informações";
            lblAtraso.TextColor = Colors.Gray;
            frameAtraso.BackgroundColor = Color.FromArgb("#F0F0F0");

            estadoLivroPicker.SelectedIndex = -1;
            justificativaEditor.Text = "";
        }

        private async Task CarregarMembrosAsync()
        {
            try
            {
                // Obter todos os membros primeiro
                var todosMembros = await _membroRepository.ObterTodosAsync();

                // Lista para armazenar membros que têm empréstimos ativos
                var membrosComEmprestimos = new List<Membro>();

                // Verificar quais membros têm empréstimos ativos
                foreach (var membro in todosMembros)
                {
                    var emprestimosAtivos = await _emprestimoRepository.ObterEmprestimosAtivosPorMembroAsync(membro.ID);
                    if (emprestimosAtivos.Count > 0)
                    {
                        membrosComEmprestimos.Add(membro);
                    }
                }

                membros = membrosComEmprestimos;

                membroPicker.ItemsSource = membros;
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
                lblTituloLivro.Text = "Carregando...";
                lblNomeMembro.Text = "Buscando empréstimos...";

                // Usar o método específico para obter empréstimos ativos do membro
                var emprestimosAtivos = await _emprestimoRepository.ObterEmprestimosAtivosPorMembroAsync(membroSelecionado.ID);

                if (emprestimosAtivos.Count == 0)
                {
                    lblTituloLivro.Text = "Nenhum empréstimo ativo";
                    lblNomeMembro.Text = $"{membroSelecionado.Nome}";
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
                    await ExibirInformacoesEmprestimo();
                }
                else
                {
                    // Múltiplos empréstimos - usar os dados do EmprestimoDetalhado
                    var opcoes = new List<string>();

                    foreach (var emp in emprestimosAtivos)
                    {
                        var titulo = emp.TituloLivro ?? "Livro não encontrado";
                        var dataVencimento = emp.DataDevolucao.ToString("dd/MM/yyyy");
                        opcoes.Add($"{titulo} (Vence: {dataVencimento})");
                    }

                    var escolha = await DisplayActionSheet(
                        "Este membro tem múltiplos empréstimos. Selecione o livro:",
                        "Cancelar",
                        null,
                        opcoes.ToArray());

                    if (escolha != "Cancelar" && escolha != null)
                    {
                        var index = opcoes.IndexOf(escolha);
                        emprestimoSelecionado = emprestimosAtivos[index];
                        await ExibirInformacoesEmprestimo();
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

        private async Task ExibirInformacoesEmprestimo()
        {
            if (emprestimoSelecionado == null) return;

            try
            {
                // Buscar informações do livro na lista de todos os livros
                var todosLivros = await _livroRepository.ObterTodosAsync();
                livroSelecionado = todosLivros.FirstOrDefault(l => l.ID == emprestimoSelecionado.LivroId);

                if (livroSelecionado == null)
                {
                    await DisplayAlert("Erro", "Erro ao carregar informações do livro.", "OK");
                    return;
                }

                // Atualizar informações na tela
                lblTituloLivro.Text = emprestimoSelecionado.TituloLivro ?? livroSelecionado.Titulo;
                lblNomeMembro.Text = emprestimoSelecionado.NomeMembro ?? "Nome não disponível";

                // Exibir data de devolução
                entryDataDevolucao.Text = emprestimoSelecionado.DataDevolucao.ToString("dd/MM/yyyy");

                // Usar a propriedade calculada DiasAtraso
                var diasAtraso = emprestimoSelecionado.DiasAtraso;

                if (diasAtraso > 0)
                {
                    lblAtraso.Text = $"Atraso: {diasAtraso} dias";
                    lblAtraso.TextColor = Color.FromArgb("#B85C00");
                    frameAtraso.BackgroundColor = Color.FromArgb("#FFF4E5");
                }
                else
                {
                    lblAtraso.Text = "No prazo";
                    lblAtraso.TextColor = Color.FromArgb("#28A745");
                    frameAtraso.BackgroundColor = Color.FromArgb("#E8F5E8");
                }

                // Mostrar informações detalhadas
                var detalhes = $"Livro: {emprestimoSelecionado.TituloLivro}\n" +
                              $"Membro: {emprestimoSelecionado.NomeMembro}\n" +
                              $"Empréstimo: {emprestimoSelecionado.DataEmprestimo.ToString("dd/MM/yyyy")}\n" +
                              $"Devolução: {emprestimoSelecionado.DataDevolucao.ToString("dd/MM/yyyy")}\n" +
                              $"Status: {emprestimoSelecionado.Status}";

                if (diasAtraso > 0)
                {
                    detalhes += $"\nAtraso: {diasAtraso} dias";
                }
                else
                {
                    detalhes += "\nSituação: No prazo";
                }

                await DisplayAlert("Detalhes do Empréstimo", detalhes, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao exibir informações: {ex.Message}", "OK");
            }
        }

        private async void OnConfirmarDevolucaoClicked(object sender, EventArgs e)
        {
            if (emprestimoSelecionado == null || livroSelecionado == null)
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

            // Confirmação final
            var confirmacao = $"Confirmar devolução?\n\n" +
                             $"Livro: {emprestimoSelecionado.TituloLivro}\n" +
                             $"Membro: {emprestimoSelecionado.NomeMembro}\n" +
                             $"Estado: {estadoLivro}";

            if (!string.IsNullOrWhiteSpace(justificativa))
            {
                confirmacao += $"\nJustificativa: {justificativa}";
            }

            bool confirmar = await DisplayAlert("Confirmar Devolução", confirmacao, "Sim", "Não");
            if (!confirmar) return;

            try
            {
                // Usar o método específico para realizar devolução
                bool sucesso = await _emprestimoRepository.RealizarDevolucaoAsync(emprestimoSelecionado.EmprestimoId, estadoLivro, justificativa);

                if (sucesso)
                {
                    await DisplayAlert("Sucesso", "Devolução realizada com sucesso!", "OK");
                    LimparFormulario();

                    // Recarregar membros
                    await CarregarMembrosAsync();
                }
                else
                {
                    await DisplayAlert("Erro", "Falha ao processar devolução.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao processar devolução:\n{ex.Message}", "OK");
            }
        }

        private void LimparFormulario()
        {
            membroPicker.SelectedIndex = -1;
            estadoLivroPicker.SelectedIndex = -1;
            justificativaEditor.Text = "";
            InicializarTela();
            emprestimoSelecionado = null;
            livroSelecionado = null;
        }

        private void OnCancelarClicked(object sender, EventArgs e)
        {
            LimparFormulario();
        }
    }
}
