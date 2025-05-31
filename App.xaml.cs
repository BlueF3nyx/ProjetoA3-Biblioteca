using BibliotecaAPP.Views;

namespace BibliotecaAppBase;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new LoginPage());
    }
}