using Microsoft.AspNetCore.Builder;
using Server.AdminMetrics.API.Infrastructure.Configuration;
using Server.AdminMetrics.API.Infrastructure.Inbound.Extensions;
using Server.Infrastructure.Configuration;
using Server.Worker.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Services.AddLogging(configure => configure.AddConsole());

// Camadas da arquitetura hexagonal
builder.Services.AddServerApplication(builder.Configuration);
builder.Services.AddServerInfrastructure(builder.Configuration);
builder.Services.AddServerHttpServices();

// Configuração específica do Worker
builder.Services.AddWorkerServices(builder.Configuration);

// Constrói o aplicativo
var app = builder.Build();

// Configuração do pipeline de requisição HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configuração para servir arquivos estáticos da interface web
app.UseWebInterface(app.Environment);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Executa o aplicativo
app.Run();
