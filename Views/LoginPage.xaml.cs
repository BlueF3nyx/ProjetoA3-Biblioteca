using BibliotecaAPP.Data;

namespace BibliotecaAPP.Views;

public partial class LoginPage : ContentPage
{
    private readonly IFuncionarioRepository _repo;

    public LoginPage()
    {
        InitializeComponent();

        _repo = new FuncionarioRepository();
    }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // Resetar mensagem de erro
        MensagemErroLabel.IsVisible = false;
        MensagemErroLabel.Text = string.Empty;

        string email = EmailEntry.Text?.Trim() ?? string.Empty;
        string senha = SenhaEntry.Text ?? string.Empty;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
        {
            MensagemErroLabel.Text = "Por favor, preencha email e senha.";
            MensagemErroLabel.IsVisible = true;
            return;
        }

        var funcionario = await _repo.AutenticarAsync(email, senha);

        if (funcionario != null)
        {
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
        else
        {
            MensagemErroLabel.Text = "Email ou senha inválidos.";
            MensagemErroLabel.IsVisible = true;
        }
    }
}
