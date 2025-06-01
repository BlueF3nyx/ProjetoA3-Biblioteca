
namespace BibliotecaAPP.Views
{
    public partial class CadastroMembroPage : ContentPage
    {
        public CadastroMembroPage()
        {
            InitializeComponent();
        }

        private async void OnSalvarMembroClicked(object sender, EventArgs e)
        {
            string nome = NomeEntry.Text;
            string cpf = CpfEntry.Text;
            string telefone = TelefoneEntry.Text;
            string email = EmailEntry.Text;

            // Validação simples
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(cpf) ||
                string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            // Aqui futuramente você chamará o repositório para salvar no banco
            await DisplayAlert("Sucesso", $"Membro cadastrado:\n\nNome: {nome}\nCPF: {cpf}\nTelefone: {telefone}\nEmail: {email}", "OK");

            // Limpar campos
            NomeEntry.Text = "";
            CpfEntry.Text = "";
            TelefoneEntry.Text = "";
            EmailEntry.Text = "";
        }
    }
}
