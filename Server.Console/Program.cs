using Microsoft.Extensions.DependencyInjection;
using NetworkHexagonal.Core.Application.Ports.Inbound;
using NetworkHexagonal.Core.Application.Ports.Outbound;
using NetworkHexagonal.Infrastructure.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkHexagonal.Core.Domain.Events.Network;

namespace Server.Console;

public class Program
{
    private static IServiceProvider _provider;
    
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("Iniciando cliente de rede...");
        
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddSingleton<INetworkConfiguration>(a =>
        {
            var config = new NetworkConfiguration
            {
                UpdateIntervalMs = 15,
                DisconnectTimeoutMs = 5000,
                ConnectionKey = "HexagonalGameServer",
                UseUnsyncedEvents = false // Eventos processados apenas via Update()
            };
            return config;
        });

        // Adiciona serviços de rede
        serviceCollection.AddNetworking();
        serviceCollection.AddLogging(a => 
        {
            a.AddConsole();
            a.SetMinimumLevel(LogLevel.Debug);
        });

        _provider = serviceCollection.BuildServiceProvider();
        
        // Obtém os serviços necessários
        var clientApp = _provider.GetRequiredService<IClientNetworkApp>();
        var packetRegistry = _provider.GetRequiredService<IPacketRegistry>();
        var eventBus = _provider.GetRequiredService<INetworkEventBus>();
        
        // Registra handlers de eventos
        eventBus.Subscribe<ConnectionEvent>(OnConnectionEvent);
        eventBus.Subscribe<DisconnectionEvent>(OnDisconnectEvent);
        
        // Registra handlers de pacotes
        // packetRegistry.RegisterPacketHandler<SeuTipoDePacote>(HandlerDoSeuPacote);
        
        
        var cancelationTokenSource = new System.Threading.CancellationTokenSource();

        await Task.Run(async () =>
        {
            while (cancelationTokenSource.Token.IsCancellationRequested)
            {
                clientApp.Update();
                await Task.Delay(15, cancelationTokenSource.Token);
            }
            
            clientApp.Disconnect();
        }, cancelationTokenSource.Token);
        
        // Conecta ao servidor
        System.Console.WriteLine("Tentando conectar ao servidor...");
        var connected = await clientApp.ConnectAsync("127.0.0.1", 9050);
        
        if (connected.Success)
        {
            System.Console.WriteLine("Conectado com sucesso!");
        }
        else
        {
            System.Console.WriteLine("Falha ao conectar ao servidor!");
        }
        
        System.Console.WriteLine("Pressione qualquer tecla para sair...");
        System.Console.ReadKey();
    }
    
    private static void OnConnectionEvent(ConnectionEvent evt)
    {
        System.Console.WriteLine($"Evento de conexão: {evt.PeerId}");
    }
    
    private static void OnDisconnectEvent(DisconnectionEvent evt)
    {
        System.Console.WriteLine($"Evento de desconexão: {evt.Reason}");
    }
}