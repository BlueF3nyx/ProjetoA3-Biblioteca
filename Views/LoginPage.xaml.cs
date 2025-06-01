namespace BibliotecaAPP.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;
        var senha = SenhaEntry.Text;

        // Simples validação (substitua por acesso ao banco depois)
        if (email == "admin@biblioteca.com" && senha == "1234")
        {
            await Navigation.PushAsync(new MainPage());
        }
        else
        {
            await DisplayAlert("Erro", "Email ou senha inválidos", "OK");
        }
    }
}
