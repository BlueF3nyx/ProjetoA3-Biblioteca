using BibliotecaAPP.Data;
using BibliotecaAPP.Models;
using BibliotecaAppBase.Models;

namespace BibliotecaAPP.Views
{
    public partial class RegistroEmprestimoPage : ContentPage
    {
        private readonly ILivroRepository _livroRepository;
        private readonly IMembroRepository _membroRepository;
        private readonly IEmprestimoRepository _emprestimoRepository;

        private List<Livro> _livrosDisponiveis = new();
        private List<Membro> _membrosAtivos = new();

        public RegistroEmprestimoPage()
        {
            InitializeComponent();
            _livroRepository = new LivroRepository();
            _membroRepository = new MembroRepository();
            _emprestimoRepository = new EmprestimoRepository();

            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            DatePickerEmprestimo.Date = DateTime.Now;
            TextBoxDuracao.Text = "15";
            CalcularDataDevolucao();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarDados();
        }

        private async Task CarregarDados()
        {
            try
            {
                // Carregar todos os membros (sem filtro de Status se não existir)
                _membrosAtivos = await _membroRepository.ObterTodosAsync();

                // Carregar livros disponíveis
                _livrosDisponiveis = (await _livroRepository.ObterTodosAsync())
                    .Where(l => l.Disponibilidade == "Disponível").ToList();

                AtualizarPickers();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            }
        }

        private void AtualizarPickers()
        {
            ComboBoxMembros.ItemsSource = _membrosAtivos.Select(m => $"{m.Nome} - {m.Email}").ToList();
            ComboBoxLivros.ItemsSource = _livrosDisponiveis.Select(l => $"{l.Titulo} - {l.Autor}").ToList();
        }

        private void ComboBoxMembros_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBoxMembros.SelectedIndex >= 0)
            {
                var membro = _membrosAtivos[ComboBoxMembros.SelectedIndex];
                LabelMembroSelecionado.Text = membro.Nome;
                _ = VerificarStatusMembro(membro);
            }
            else
            {
                LabelMembroSelecionado.Text = "Nenhum membro selecionado";
            }
            AtualizarBotaoConfirmar();
        }

        private void ComboBoxLivros_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBoxLivros.SelectedIndex >= 0)
            {
                var livro = _livrosDisponiveis[ComboBoxLivros.SelectedIndex];
                LabelLivroSelecionado.Text = livro.Titulo;
            }
            else
            {
                LabelLivroSelecionado.Text = "Nenhum livro selecionado";
            }
            AtualizarBotaoConfirmar();
        }

        

        private async Task VerificarStatusMembro(Membro membro) // Updated type reference
        {
            try
            {
                var emprestimosAtivos = await _emprestimoRepository.ObterEmprestimosAtivosPorMembroAsync(membro.ID);

                if (emprestimosAtivos.Any(e => e.DiasAtraso > 0))
                {
                    await DisplayAlert("Aviso", "Membro possui livros em atraso.", "OK");
                    BtnConfirmar.IsEnabled = false;
                    return;
                }

                if (emprestimosAtivos.Count >= 3)
                {
                    await DisplayAlert("Aviso", "Membro atingiu limite de 3 livros.", "OK");
                    BtnConfirmar.IsEnabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar status: {ex.Message}");
            }
        }

        private void PeriodoRapido_Click(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string diasStr)
            {
                TextBoxDuracao.Text = diasStr;
                CalcularDataDevolucao();
            }
        }

        private void TextBoxDuracao_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcularDataDevolucao();
        }

        private void DatePickerEmprestimo_DateSelected(object sender, DateChangedEventArgs e)
        {
            CalcularDataDevolucao();
        }

        private void CalcularDataDevolucao()
        {
            if (int.TryParse(TextBoxDuracao.Text, out int dias) && dias > 0 && dias <= 90)
            {
                DatePickerDevolucao.Date = DatePickerEmprestimo.Date.AddDays(dias);
                LabelPeriodoSelecionado.Text = $"{dias} dia{(dias > 1 ? "s" : "")} - até {DatePickerDevolucao.Date:dd/MM/yyyy}";
            }
            else
            {
                LabelPeriodoSelecionado.Text = "Período inválido (1-90 dias)";
            }
            AtualizarBotaoConfirmar();
        }

        private void AtualizarBotaoConfirmar()
        {
            bool membroSelecionado = ComboBoxMembros.SelectedIndex >= 0;
            bool livroSelecionado = ComboBoxLivros.SelectedIndex >= 0;
            bool periodoValido = int.TryParse(TextBoxDuracao.Text, out int dias) && dias > 0 && dias <= 90;

            bool podeConfirmar = membroSelecionado && livroSelecionado && periodoValido;

            BtnConfirmar.IsEnabled = podeConfirmar;
            BtnConfirmar.BackgroundColor = podeConfirmar ?
                Color.FromArgb("#28A745") : Color.FromArgb("#6C757D");
        }

        private async void Cancelar_Click(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Cancelar", "Deseja cancelar e voltar?", "Sim", "Não");
            if (confirmar)
            {
                await Navigation.PopAsync();
            }
        }

        private async void Confirmar_Click(object sender, EventArgs e)
        {
            if (ComboBoxLivros.SelectedIndex >= 0 && ComboBoxMembros.SelectedIndex >= 0)
            {
                var livro = _livrosDisponiveis[ComboBoxLivros.SelectedIndex];
                var membro = _membrosAtivos[ComboBoxMembros.SelectedIndex];

                var emprestimo = new Emprestimo
                {
                    IdLivro = livro.ID,
                    IdMembro = membro.ID,
                    DataEmprestimo = DatePickerEmprestimo.Date,
                    DataDevolucaoReal = DatePickerDevolucao.Date
                };

                try
                {
                    var resultado = await _emprestimoRepository.AdicionarAsync(emprestimo);

                    if (resultado > 0)
                    {
                        livro.Disponibilidade = "Emprestado";
                        await _livroRepository.AtualizarAsync(livro);

                        await DisplayAlert("Sucesso", "Empréstimo realizado com sucesso!", "OK");

                        // Voltar para a tela anterior
                        await Navigation.PopAsync();
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Erro ao confirmar empréstimo: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Erro", "Selecione um livro e um membro antes de confirmar.", "OK");
            }
        }


        // Eventos para as setas dos pickers
        private void Picker_Focused(object sender, FocusEventArgs e)
        {
            // Implementar se necessário
        }

        private void Picker_Unfocused(object sender, FocusEventArgs e)
        {
            // Implementar se necessário
        }
    }
}
