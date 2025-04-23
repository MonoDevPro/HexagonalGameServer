using System;
using CharacterState = Server.Domain.Enum.CharacterState;

namespace Server.Domain.Events;

/// <summary>
/// Evento disparado quando um personagem morre (transita para o estado Dead).
/// Contém informações adicionais específicas sobre a morte do personagem.
/// </summary>
public class CharacterDiedEvent : CharacterStateChangedEvent
{
    /// <summary>
    /// Causa da morte do personagem
    /// </summary>
    public string CauseOfDeath { get; }
    
    /// <summary>
    /// ID do responsável pela morte (se houver)
    /// </summary>
    public long? KillerId { get; }
    
    /// <summary>
    /// Localização onde o personagem morreu
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Cria uma nova instância do evento de morte de personagem.
    /// </summary>
    /// <param name="previousState">Estado anterior do personagem</param>
    /// <param name="characterId">ID do personagem</param>
    /// <param name="causeOfDeath">Causa da morte</param>
    /// <param name="killerId">ID do responsável pela morte (opcional)</param>
    /// <param name="location">Localização onde o personagem morreu</param>
    public CharacterDiedEvent(
        CharacterState previousState,
        long characterId,
        string name,
        string causeOfDeath,
        long? killerId = null,
        string location = "Unknown") 
        : base(previousState, CharacterState.Dead, characterId, causeOfDeath)
    {
        CauseOfDeath = causeOfDeath;
        KillerId = killerId;
        Location = location;

        CharacterId = characterId;
        Name = name;
    }

    public long CharacterId { get; }
    public string Name { get; }
}