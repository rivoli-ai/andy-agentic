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

startup.ConfigureApplicationServices(builder.Services);

var app = builder.Build();

startup.ConfigureApplication(app);

app.Run();
