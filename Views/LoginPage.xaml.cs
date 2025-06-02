namespace BibliotecaAPP.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        
        var repo = new FuncionarioRepository(); 
        BindingContext = new LoginViewModel(repo);
    }
}
