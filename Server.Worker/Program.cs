using Microsoft.AspNetCore.Builder;
using Server.Infrastructure.Configuration;
using Server.Worker.Extensions;
using Server.Infrastructure.Inbound.Http.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Services.AddLogging(configure => configure.AddConsole());

// Camadas da arquitetura hexagonal
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

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
