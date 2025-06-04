using System.Windows.Input;
using BibliotecaAPP;
using BibliotecaAPP.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class LoginViewModel : ObservableObject
{
    private readonly IFuncionarioRepository _repo;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string senha;

    [ObservableProperty]
    private string mensagemErro;

    public ICommand LoginCommand { get; }

    public LoginViewModel(IFuncionarioRepository repo)
    {
        _repo = repo;
        LoginCommand = new AsyncRelayCommand(ExecutarLogin);
    }

    private async Task ExecutarLogin()
    {
        var funcionario = await _repo.AutenticarAsync(Email!, Senha!);

        if (funcionario != null)
        {
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
        else
        {
            MensagemErro = "Email ou senha inválidos.";
        }
    }

}
