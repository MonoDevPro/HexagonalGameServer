namespace NetworkCommon.DTOs.Enum;

/// <summary>
/// Enum que representa os estados possíveis de um personagem
/// </summary>
public enum CharacterStateDto : byte
{
    /// <summary>
    /// Personagem em estado parado/ocioso
    /// </summary>
    Idle,
    
    /// <summary>
    /// Personagem em movimento/andando
    /// </summary>
    Walking,
    
    /// <summary>
    /// Personagem executando um ataque
    /// </summary>
    Attacking,
    
    /// <summary>
    /// Personagem morto
    /// </summary>
    Dead,
}