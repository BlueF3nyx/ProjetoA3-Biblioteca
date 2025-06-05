
using BibliotecaAPP.Data;
using BibliotecaAPP.Models;
using System.Net.Mail;
namespace BibliotecaAPP.Views
{
    public partial class CadastroMembroPage : ContentPage
    {
        private readonly IMembroRepository _membroRepository;
        private List<Membro> membros;
        private Membro? membroEmEdicao;
        public CadastroMembroPage()
        {
            InitializeComponent();
            _membroRepository = new MembroRepository();
            membros = new List<Membro>();
            membroEmEdicao = null;
            _ = CarregarMembrosAsync();
        }
        private async Task CarregarMembrosAsync()
        {
            try
            {
                membros = await _membroRepository.ObterTodosAsync();
                MembrosListView.ItemsSource = membros;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao carregar membros: {ex.Message}", "OK");
            }
        }
        private async void OnSalvarMembroClicked(object sender, EventArgs e)
        {
            string? nome = NomeEntry.Text?.Trim();
            string? cpf = CpfEntry.Text?.Trim();
            string? telefone = TelefoneEntry.Text?.Trim();
            string? email = EmailEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(cpf) ||
              string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }
            try
            {
                var mail = new MailAddress(email);
            }
            catch
            {
                await DisplayAlert("Erro", "Email inválido.", "OK");
                return;
            }
            SalvarButton.IsEnabled = false;
            try
            {
                if (membroEmEdicao == null)
                {
                    var novoMembro = new Membro
                    {
                        Nome = nome,
                        CPF = cpf,
                        Telefone = telefone,
                        Email = email
                    };
                    await _membroRepository.AdicionarAsync(novoMembro);
                    await DisplayAlert("Sucesso", "Membro cadastrado com sucesso!", "OK");
                }
                else
                {
                    membroEmEdicao.Nome = nome;
                    membroEmEdicao.CPF = cpf;
                    membroEmEdicao.Telefone = telefone;
                    membroEmEdicao.Email = email;
                    await _membroRepository.AtualizarAsync(membroEmEdicao);
                    await DisplayAlert("Sucesso", "Membro atualizado com sucesso!", "OK");
                    membroEmEdicao = null;
                    CancelarEdicaoButton.IsVisible = false;
                }
                LimparFormulario();
                await CarregarMembrosAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao salvar membro: {ex.Message}", "OK");
            }
            finally
            {
                SalvarButton.IsEnabled = true;
            }
        }
        private void LimparFormulario()
        {
            NomeEntry.Text = "";
            CpfEntry.Text = "";
            TelefoneEntry.Text = "";
            EmailEntry.Text = "";
        }
        private void OnEditarMembroClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var membroSelecionado = button?.BindingContext as Membro;
            if (membroSelecionado == null) return;
            membroEmEdicao = membroSelecionado;
            NomeEntry.Text = membroEmEdicao.Nome;
            CpfEntry.Text = membroEmEdicao.CPF;
            TelefoneEntry.Text = membroEmEdicao.Telefone;
            EmailEntry.Text = membroEmEdicao.Email;
            CancelarEdicaoButton.IsVisible = true;
        }
        private void OnCancelarEdicaoClicked(object sender, EventArgs e)
        {
            membroEmEdicao = null;
            LimparFormulario();
            CancelarEdicaoButton.IsVisible = false;
        }
        private async void OnExcluirMembroClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var membroSelecionado = button?.BindingContext as Membro;
            if (membroSelecionado == null) return;
            bool confirmarExclusao = await DisplayAlert("Confirmar Exclusão",
                            $"Tem certeza que deseja excluir o membro {membroSelecionado.Nome}?",
                            "Sim", "Não");
            if (!confirmarExclusao)
            {
                return;
            }
            try
            {
                
                var emprestimoRepository = new EmprestimoRepository();
                bool temEmprestimos = await emprestimoRepository.MembroTemEmprestimosAsync(membroSelecionado.ID);
                if (temEmprestimos)
                {
                    await DisplayAlert("Erro de Exclusão",
                             "Não é possível excluir este membro porque ele possui empréstimos registrados no sistema (ativos ou históricos).",
                             "OK");
                }
                else
                {
                    await _membroRepository.ExcluirAsync(membroSelecionado.ID);
                    await DisplayAlert("Sucesso", "Membro excluído com sucesso!", "OK");
                    await CarregarMembrosAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao excluir membro: {ex.Message}", "OK");
            }
        }
        private void OnMembroSelecionado(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            ((ListView)sender).SelectedItem = null;
        }
    }
}
