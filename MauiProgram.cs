using BibliotecaAPP.Data;
using BibliotecaAPP.Views;
using BibliotecaAppBase;
using Microsoft.Extensions.Logging;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Registrar serviços e páginas
        builder.Services.AddSingleton<IMembroRepository, MembroRepository>();
        builder.Services.AddSingleton<ILivroRepository, LivroRepository>();
        builder.Services.AddSingleton<IEmprestimoRepository, EmprestimoRepository>();

        builder.Services.AddTransient<RegistroEmprestimoPage>();

        var app = builder.Build();

        App.Services = app.Services;  // Aqui você seta o IServiceProvider estático no App

        return app;
    }
}
