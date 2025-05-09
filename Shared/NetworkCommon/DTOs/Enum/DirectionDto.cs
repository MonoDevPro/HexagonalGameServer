namespace NetworkCommon.DTOs.Enum;

/// <summary>
/// Enum que representa as direções possíveis para um personagem
/// </summary>
public enum DirectionDto : byte
{
    /// <summary>
    /// Direção para cima
    /// </summary>
    Up,
    
    /// <summary>
    /// Direção para baixo
    /// </summary>
    Down,
    
    /// <summary>
    /// Direção para a esquerda
    /// </summary>
    Left,
    
    /// <summary>
    /// Direção para a direita
    /// </summary>
    Right,
    
    /// <summary>
    /// Direção para cima e esquerda (diagonal)
    /// </summary>
    UpLeft,
    
    /// <summary>
    /// Direção para cima e direita (diagonal)
    /// </summary>
    UpRight,
    
    /// <summary>
    /// Direção para baixo e esquerda (diagonal)
    /// </summary>
    DownLeft,
    
    /// <summary>
    /// Direção para baixo e direita (diagonal)
    /// </summary>
    DownRight,
}