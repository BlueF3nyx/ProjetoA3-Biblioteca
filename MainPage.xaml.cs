using BibliotecaAPP.Views;

namespace BibliotecaAPP;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCadastroLivrosClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CadastroLivroPage());
    }

    private void OnCadastroMembrosClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CadastroMembroPage());
    }

    private void OnRegistroEmprestimosClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new RegistroEmprestimoPage());
    }

    private void OnGestaoDevolucoesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new GestaoDevolucoes());
    }

    private void OnHistoricoEmprestimosClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new HistoricoEmprestimo());
    }

    private void OnRelatoriosClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new RelatoriosPage());
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        // Navegar para a tela de login
        Application.Current.MainPage = new LoginPage();
    }
}
