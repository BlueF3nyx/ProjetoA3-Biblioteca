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
            _membroRepository = new MembroRepository(); // Ideal: injeção de dependência
            membros = new List<Membro>();
            membroEmEdicao = null;

            // Carrega membros assincronamente (sem aguardar no construtor)
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
            string nome = NomeEntry.Text?.Trim();
            string cpf = CpfEntry.Text?.Trim();
            string telefone = TelefoneEntry.Text?.Trim();
            string email = EmailEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(cpf) ||
                string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            // Validação simples de email
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
                    SalvarButton.Text = "Salvar Membro";
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

            SalvarButton.Text = "Atualizar Membro";
            CancelarEdicaoButton.IsVisible = true;
        }

        private async void OnExcluirMembroClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var membroSelecionado = button?.BindingContext as Membro;
            if (membroSelecionado == null) return;

            bool confirm = await DisplayAlert("Confirmação", $"Excluir o membro {membroSelecionado.Nome}?", "Sim", "Não");
            if (!confirm) return;

            try
            {
                await _membroRepository.ExcluirAsync(membroSelecionado.ID);
                await DisplayAlert("Sucesso", "Membro excluído com sucesso!", "OK");
                await CarregarMembrosAsync();

                if (membroEmEdicao != null && membroEmEdicao.ID == membroSelecionado.ID)
                {
                    membroEmEdicao = null;
                    CancelarEdicaoButton.IsVisible = false;
                    SalvarButton.Text = "Salvar Membro";
                    LimparFormulario();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao excluir membro: {ex.Message}", "OK");
            }
        }

        private void OnCancelarEdicaoClicked(object sender, EventArgs e)
        {
            membroEmEdicao = null;
            LimparFormulario();
            SalvarButton.Text = "Salvar Membro";
            CancelarEdicaoButton.IsVisible = false;
        }

        private void OnMembroSelecionado(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            ((ListView)sender).SelectedItem = null;
        }
    }
}
