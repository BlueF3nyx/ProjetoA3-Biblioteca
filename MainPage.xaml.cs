using BibliotecaAPP.Views;
using BibliotecaAppBase;
using Microsoft.Extensions.DependencyInjection;

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

    private async void OnRegistroEmprestimosClicked(object sender, EventArgs e)
    {
        // Fix: Access the Services property from the App class directly  
        var page = ActivatorUtilities.CreateInstance<RegistroEmprestimoPage>(App.Services);
        await Navigation.PushAsync(page);
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
