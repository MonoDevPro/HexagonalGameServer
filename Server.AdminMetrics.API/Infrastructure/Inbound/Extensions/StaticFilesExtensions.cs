using Microsoft.Extensions.FileProviders;

namespace Server.AdminMetrics.API.Infrastructure.Inbound.Extensions;

/// <summary>
/// Extensões para configuração dos recursos estáticos da interface web
/// </summary>
public static class StaticFilesExtensions
{
    /// <summary>
    /// Configura o servidor para disponibilizar os arquivos estáticos da interface gráfica web
    /// </summary>
    /// <param name="app">Aplicação web</param>
    /// <param name="env">Ambiente de hospedagem</param>
    /// <returns>A aplicação web configurada</returns>
    public static WebApplication UseWebInterface(this WebApplication app, IWebHostEnvironment env)
    {
        // Obtém o caminho para a pasta wwwroot dentro da Infraestrutura
        // Corrigindo o caminho para a estrutura correta Http/Inbound/wwwroot
        var path = Path.Combine(AppContext.BaseDirectory, "Server.AdminMetrics.API", "Infrastructure", "Inbound", "wwwroot");
        
        // Se estamos em ambiente de desenvolvimento, tentamos usar o caminho relativo ao projeto
        if (env.IsDevelopment())
        {
            var devPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "Server.AdminMetrics.API", "Infrastructure", "Inbound", "wwwroot"));
            if (Directory.Exists(devPath))
            {
                path = devPath;
            }
        }

        // Verifica se a pasta existe
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        // Log do caminho para debug
        Console.WriteLine($"Servindo arquivos estáticos de: {path}");

        // Configura middleware para servir arquivos estáticos
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(path),
            RequestPath = ""
        });

        // Configura middleware para usar arquivos padrão (index.html)
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            DefaultFileNames = new List<string> { "index.html" },
            FileProvider = new PhysicalFileProvider(path),
            RequestPath = ""
        });

        return app;
    }
}
