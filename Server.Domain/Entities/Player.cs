using System;
using System.Collections.Generic;
using Server.Domain.Events;
using Server.Domain.Events.Player;

namespace Server.Domain.Entities;

/// <summary>
/// Representa um jogador conectado ao servidor - uma sessão ativa de jogo.
/// Diferente de Account (que representa a conta do usuário) e Character (que representa um personagem no jogo),
/// Player é o conceito que conecta a camada de rede com a sessão de jogador em andamento.
/// </summary>
public class Player : Entity
{
    /// <summary>
    /// ID de conexão na camada de rede
    /// </summary>
    public int ConnectionId { get; private set; }
    
    /// <summary>
    /// Conta associada ao jogador (pode ser null antes da autenticação)
    /// </summary>
    public Account? Account { get; private set; }
    
    /// <summary>
    /// Personagem atualmente selecionado (pode ser null se estiver apenas logado na conta)
    /// </summary>
    public Character? SelectedCharacter { get; private set; }
    
    /// <summary>
    /// Data e hora da conexão
    /// </summary>
    public DateTime ConnectedAt { get; private set; }
    
    /// <summary>
    /// Último momento de atividade
    /// </summary>
    public DateTime LastActivity { get; private set; }
    
    /// <summary>
    /// Latência atual em milissegundos
    /// </summary>
    public int CurrentLatency { get; private set; }
    
    /// <summary>
    /// Estado da autenticação do player
    /// </summary>
    public bool IsAuthenticated => Account != null;
    
    /// <summary>
    /// Estado de seleção de personagem
    /// </summary>
    public bool HasSelectedCharacter => SelectedCharacter != null;

    // Construtor para ORM
    protected Player() { }

    /// <summary>
    /// Cria um novo jogador associado a uma conexão
    /// </summary>
    /// <param name="connectionId">ID da conexão de rede</param>
    public Player(int connectionId)
    {
        ConnectionId = connectionId;
        ConnectedAt = DateTime.UtcNow;
        LastActivity = ConnectedAt;
        
        AddDomainEvent(new PlayerConnectedEvent(connectionId));
    }

    /// <summary>
    /// Autentica o jogador associando uma conta
    /// </summary>
    /// <param name="account">Conta autenticada</param>
    /// <returns>Se a autenticação foi bem-sucedida</returns>
    public bool Authenticate(Account account)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));
            
        // Verifica se a conta está em um estado válido para autenticação
        if (account.State == Enum.AccountState.Banned || 
            account.State == Enum.AccountState.Deleted ||
            account.State == Enum.AccountState.Locked || 
            account.State == Enum.AccountState.Suspended)
        {
            AddDomainEvent(new PlayerAccountLoginFailedEvent(
                ConnectionId, 
                account.Username, 
                $"Conta em estado inválido: {account.State}")
            );
            return false;
        }

        Account = account;
        UpdateActivity();
        
        AddDomainEvent(new PlayerAccountLoginSuccessEvent(ConnectionId, account.Username));
        return true;
    }

    /// <summary>
    /// Seleciona um personagem para jogar
    /// </summary>
    /// <param name="character">Personagem a ser selecionado</param>
    /// <returns>Se a seleção foi bem-sucedida</returns>
    public bool SelectCharacter(Character character)
    {
        if (!IsAuthenticated)
        {
            AddDomainEvent(new PlayerCharacterSelectFailedEvent(
                ConnectionId,
                character?.Id ?? 0,
                "Não autenticado",
                "Jogador precisa estar autenticado para selecionar um personagem"
            ));
            return false;
        }
        
        // Verificar se o personagem pertence à conta
        if (Account!.GetCharacterById(character.Id) == null)
        {
            AddDomainEvent(new PlayerCharacterSelectFailedEvent(
                ConnectionId,
                character.Id,
                Account.Username,
                "Personagem não pertence a esta conta"
            ));
            return false;
        }

        SelectedCharacter = character;
        UpdateActivity();
        
        AddDomainEvent(new PlayerCharacterSelectSuccessEvent(
            ConnectionId,
            character.Id,
            character.Name,
            Account.Username
        ));
        
        // Evento para informar que um personagem entrou no mundo
        AddDomainEvent(new PlayerCharacterEnteredWorldEvent(
            ConnectionId,
            character.Id,
            character.Name,
            character.FloorIndex,
            character.BoundingBox.X,
            character.BoundingBox.Y
        ));
        
        return true;
    }

    /// <summary>
    /// Desseleciona o personagem atual, voltando para a tela de seleção de personagens
    /// </summary>
    public void UnselectCharacter()
    {
        if (SelectedCharacter != null)
        {
            Character previousCharacter = SelectedCharacter;
            
            // Evento de saída do mundo
            AddDomainEvent(new PlayerCharacterLeftWorldEvent(
                ConnectionId,
                previousCharacter.Id,
                previousCharacter.Name,
                "Desseleção de personagem"
            ));
            
            SelectedCharacter = null;
            UpdateActivity();
        }
    }

    /// <summary>
    /// Desconecta o jogador
    /// </summary>
    /// <param name="reason">Motivo da desconexão</param>
    public void Disconnect(string reason = "Desconexão normal")
    {
        // Se estava com personagem selecionado, emitir evento de saída do mundo
        if (SelectedCharacter != null)
        {
            AddDomainEvent(new PlayerCharacterLeftWorldEvent(
                ConnectionId,
                SelectedCharacter.Id,
                SelectedCharacter.Name,
                reason
            ));
        }
        
        // Evento de desconexão
        AddDomainEvent(new PlayerDisconnectedEvent(ConnectionId, reason));
    }

    /// <summary>
    /// Atualiza a latência do jogador
    /// </summary>
    /// <param name="latencyMs">Latência em milissegundos</param>
    public void UpdateLatency(int latencyMs)
    {
        CurrentLatency = latencyMs;
        UpdateActivity();
    }

    /// <summary>
    /// Atualiza o timestamp da última atividade
    /// </summary>
    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }
}