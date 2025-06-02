using BibliotecaAPP.Data;
using BibliotecaAPP.Models;

namespace BibliotecaAPP.Views
{
    public partial class CadastroMembroPage : ContentPage
    {
        private readonly IMembroRepository _membroRepository;

        public CadastroMembroPage()
        {
            InitializeComponent();
            _membroRepository = new MembroRepository(); // ideal: injeção de dependência
        }

        private async void OnSalvarMembroClicked(object sender, EventArgs e)
        {
            string nome = NomeEntry.Text;
            string cpf = CpfEntry.Text;
            string telefone = TelefoneEntry.Text;
            string email = EmailEntry.Text;

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(cpf) ||
                string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            var novoMembro = new Membro
            {
                Nome = nome,
                CPF = cpf,
                Telefone = telefone,
                Email = email
            };

            try
            {
                await _membroRepository.AdicionarAsync(novoMembro);
                await DisplayAlert("Sucesso", "Membro cadastrado com sucesso!", "OK");

                NomeEntry.Text = "";
                CpfEntry.Text = "";
                TelefoneEntry.Text = "";
                EmailEntry.Text = "";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao cadastrar membro: {ex.Message}", "OK");
            }
        }
    }
}
