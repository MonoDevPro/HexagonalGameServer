using System;
using System.Collections.Generic;
using Server.Domain.Enums;
using Server.Domain.Events;
using Server.Domain.Events.Player;
using Server.Domain.Events.Player.Account;
using Server.Domain.Events.Player.Character;
using Server.Domain.Events.Player.Connection;
using Server.Domain.Policies;

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
    /// Nome de usuário do jogador
    /// </summary>
    public string Username => AccountAuthentication?.Username ?? String.Empty;

    /// <summary>
    /// Estado da autenticação do player
    /// </summary>
    public bool IsAuthenticated => AccountAuthentication != null && 
                                   AccountAuthentication.CheckValidation();
    
    /// <summary>
    /// Estado de seleção de personagem
    /// </summary>
    public bool HasSelectedCharacter => SelectedCharacterId != -1;
    
    /// <summary>
    /// Data e hora da conexão
    /// </summary>
    public DateTime ConnectedAt { get; private set; }
    
    /// <summary>
    /// Último momento de atividade
    /// </summary>
    public DateTime LastActivity { get; private set; }
    
    public AccountAuthentication? AccountAuthentication { get; private set; }
    
    private Character? _selectedCharacter;
    private long SelectedCharacterId => _selectedCharacter?.Id ?? -1;

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
    /// Autentica o jogador com um nome de usuário e verifica o estado da conta
    /// </summary>
    /// <param name="authentication">Autenticação do serviço de contas</param>
    public bool Authenticate(AccountAuthentication authentication)
    {
        // Verifica se a conta está em um estado válido para autenticação
        if (authentication.AccountState == AccountState.Banned || 
            authentication.AccountState == AccountState.Deleted ||
            authentication.AccountState == AccountState.Locked || 
            authentication.AccountState == AccountState.Suspended)
        {
            AddDomainEvent(new PlayerAccountLoginFailedEvent(
                ConnectionId, 
                authentication.Username, 
                $"Conta em estado inválido: {authentication.AccountState}")
            );
            return false;
        }

        AccountAuthentication = authentication;

        UpdateActivity();
        
        AddDomainEvent(new PlayerAccountLoginSuccessEvent(ConnectionId, Username));
        return true;
    }

    /// <summary>
    /// Realiza o logout do jogador, removendo o personagem selecionado
    /// </summary>
    public void Logout()
    {
        if (!HasSelectedCharacter)
            return;

        Character previousCharacter = _selectedCharacter!;
        _selectedCharacter = null;
        
        // Emitir evento de logout do personagem
        AddDomainEvent(new PlayerCharacterLeftWorldEvent(
            ConnectionId,
            previousCharacter.Id,
            previousCharacter.Name,
            "Logout do personagem"
        ));
    }

    /// <summary>
    /// Seleciona um personagem para jogar
    /// </summary>
    /// <param name="character">personagem a ser selecionado</param>
    /// <returns>Se a seleção foi bem-sucedida</returns>
    public bool SelectCharacter(Character character)
    {
        if (!IsAuthenticated)
        {
            AddDomainEvent(new PlayerCharacterSelectFailedEvent(
                ConnectionId,
                character.Id,
                "Não autenticado",
                "Jogador precisa estar autenticado para selecionar um personagem"
            ));
            return false;
        }

        _selectedCharacter = character;

        UpdateActivity();
        
        AddDomainEvent(new PlayerCharacterSelectSuccessEvent(
            ConnectionId,
            character.Id,
            character.Name,
            Username
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
        if (HasSelectedCharacter)
        {
            Character previousCharacter = _selectedCharacter!;
            
            // Evento de saída do mundo
            AddDomainEvent(new PlayerCharacterLeftWorldEvent(
                ConnectionId,
                previousCharacter.Id,
                previousCharacter.Name,
                "Desseleção de personagem"
            ));
            
            _selectedCharacter = null;
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
        if (HasSelectedCharacter)
        {
            AddDomainEvent(new PlayerCharacterLeftWorldEvent(
                ConnectionId,
                SelectedCharacterId,
                _selectedCharacter!.Name,
                reason
            ));
        }
        
        // Evento de desconexão
        AddDomainEvent(new PlayerDisconnectedEvent(ConnectionId, reason));
    }

    /// <summary>
    /// Atualiza o timestamp da última atividade
    /// </summary>
    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Envia uma mensagem de chat
    /// </summary>
    /// <param name="message">Conteúdo da mensagem</param>
    /// <param name="type">Tipo de chat</param>
    /// <param name="recipientName">Nome do destinatário (para sussuros)</param>
    /// <returns>Se a mensagem foi enviada com sucesso</returns>
    public bool SendChatMessage(string message)
    {
        if (!IsAuthenticated || !HasSelectedCharacter)
            return false;

        AddDomainEvent(new PlayerChatEvent(
            ConnectionId,
            Username,
            HasSelectedCharacter ? _selectedCharacter!.Name : string.Empty,
            message
        ));
        
        UpdateActivity();
        return true;
    }

    /// <summary>
    /// Cria um personagem para este jogador
    /// </summary>
    /// <param name="characterName">Nome do personagem</param>
    /// <returns>Evento indicando sucesso ou falha na criação</returns>
    public void CreateCharacter(string characterName) 
    {
        if (!IsAuthenticated)
        {
            AddDomainEvent(new PlayerCharacterCreationFailedEvent(
                ConnectionId,
                "",  // Username vazio, pois não está autenticado
                characterName,
                "Jogador não autenticado"
            ));
            return;
        }
        
        AddDomainEvent(new PlayerCharacterCreationSuccessEvent(
            ConnectionId, 
            Username, 
            characterName
        ));
        
        UpdateActivity();
    }
}