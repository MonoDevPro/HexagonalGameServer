using NetworkHexagonal.Infrastructure.DependencyInjection;
using Server.Application.DependencyInjection;
using Server.Infrastructure.DependencyInjection;
using Server.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Services.AddLogging(configure => configure.AddConsole());

// Camadas da arquitetura hexagonal
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// Configuração específica do Worker
builder.Services.AddWorkerServices(builder.Configuration);

// Constrói e executa o host
var host = builder.Build();
host.Run();
