using NetworkCommon.DTOs;
using NetworkCommon.DTOs.Primitives;
using Server.Domain.Entities;
using Server.Domain.Entities.Primitives;
using Server.Domain.ValueObjects;

namespace Server.Application.Ports.Outbound.Mapping;

/// <summary>
/// Porta para mapeamento entre entidades de domínio e DTOs
/// </summary>
public interface IDtoMapper
{
    /// <summary>
    /// Mapeia um personagem para seu DTO correspondente
    /// </summary>
    CharacterDto MapToDto(Character character);
    
    /// <summary>
    /// Mapeia uma conta para seu DTO correspondente
    /// </summary>
    AccountDto MapToDto(Account account);
    
    /// <summary>
    /// Mapeia estatísticas para seu DTO correspondente
    /// </summary>
    StatsDto MapToDto(Stats stats);
    
    /// <summary>
    /// Mapeia atributos vitais para seu DTO correspondente
    /// </summary>
    VitalsDto MapToDto(Vitals vitals);
    
    /// <summary>
    /// Mapeia uma bounding box para seu DTO correspondente
    /// </summary>
    BoundingBoxDto MapToDto(PositionBox positionBox);
    
    /// <summary>
    /// Cria um CharacterDto com informações mínimas
    /// </summary>
    CharacterDto CreateMinimalCharacterDto(string name, long id = 0);
}