using Andy.Agentic;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80);

    try
    {
        serverOptions.ListenAnyIP(443, configure => configure.UseHttps());
    }
    catch (InvalidOperationException)
    {
        Console.WriteLine("Warning: HTTPS not configured - no valid certificate found. Running HTTP only.");
    }
});

var startup = new Startup(builder.Configuration, builder.Environment);

#pragma warning disable SKEXP0001 // Le type est utilisé à des fins d’évaluation uniquement et est susceptible d’être modifié ou supprimé dans les futures mises à jour. Supprimez ce diagnostic pour continuer.
startup.ConfigureApplicationServices(builder.Services);
#pragma warning restore SKEXP0001 // Le type est utilisé à des fins d’évaluation uniquement et est susceptible d’être modifié ou supprimé dans les futures mises à jour. Supprimez ce diagnostic pour continuer.

var app = builder.Build();

startup.ConfigureApplication(app);

app.Run();
