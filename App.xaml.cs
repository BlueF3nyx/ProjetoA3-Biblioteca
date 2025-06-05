using BibliotecaAPP;
using BibliotecaAPP.Views;

namespace BibliotecaAppBase;

public partial class App : Application
{
    public static IServiceProvider? Services { get; set; }

    public App(IServiceProvider services)
    {
        InitializeComponent();

        Services = services;

        MainPage = new NavigationPage(new CadastroMembroPage());
    }
}
