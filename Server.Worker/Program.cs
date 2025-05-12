using Server.Infrastructure.Configuration;
using Server.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Services.AddLogging(configure => configure.AddConsole());

// Camadas da arquitetura hexagonal
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Configuração específica do Worker
builder.Services.AddWorkerServices(builder.Configuration);

// Constrói e executa o host
var host = builder.Build();
host.Run();
