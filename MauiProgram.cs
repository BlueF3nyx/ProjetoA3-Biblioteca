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

        // Registrar serviços (repositórios)
        builder.Services.AddSingleton<IMembroRepository, MembroRepository>();
        builder.Services.AddSingleton<ILivroRepository, LivroRepository>();
        builder.Services.AddSingleton<IEmprestimoRepository, EmprestimoRepository>();

        builder.Services.AddTransient<RegistroEmprestimoPage>();
        builder.Services.AddTransient<RelatoriosPage>();
        
        builder.Services.AddTransient<GestaoDevolucoes>();

        var app = builder.Build();

        
        App.Services = app.Services;

        return app;
    }
}
