using BibliotecaAPP.Data;
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
        
        if (App.Services == null)
        {
            throw new InvalidOperationException("Service provider is not initialized.");
        }

        var page = ActivatorUtilities.CreateInstance<RegistroEmprestimoPage>(App.Services);
        await Navigation.PushAsync(page);
    }

    private void OnGestaoDevolucoesClicked(object sender, EventArgs e)
    {
        
        if (App.Services == null)
        {
            throw new InvalidOperationException("Service provider is not initialized.");
        }

        // Retrieve the required dependencies from the service provider
        var membroRepository = App.Services.GetRequiredService<IMembroRepository>();
        var emprestimoRepository = App.Services.GetRequiredService<IEmprestimoRepository>();

        // Pass the dependencies to the GestaoDevolucoes constructor
        var page = new GestaoDevolucoes(membroRepository, emprestimoRepository);
        Navigation.PushAsync(page);
    }

    private void OnHistoricoEmprestimosClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new HistoricoEmprestimo());
    }

    private void OnRelatoriosClicked(object sender, EventArgs e)
    {
        if (App.Services == null)
        {
            throw new InvalidOperationException("Service provider is not initialized.");
        }

        var emprestimoRepository = App.Services.GetRequiredService<IEmprestimoRepository>();
        var page = new RelatoriosPage(emprestimoRepository);
        Navigation.PushAsync(page);
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        
        if (Application.Current == null)
        {
            throw new InvalidOperationException("");
        }

        
        Application.Current.MainPage = new LoginPage();
    }
}
