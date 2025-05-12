using NetworkCommon.DTOs;
using NetworkCommon.DTOs.Enum;
using NetworkCommon.DTOs.Primitives;
using Server.Application.Ports.Outbound.Mapping;
using Server.Domain.Entities;
using Server.Domain.Entities.Primitives;
using Server.Domain.Enums;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure.Outbound.Mapping;

/// <summary>
/// Serviço de mapeamento entre entidades de domínio e DTOs
/// </summary>
public class DtoMapper : IDtoMapper
{
    /// <inheritdoc />
    public CharacterDto MapToDto(Character character)
    {
        if (character == null)
            return new CharacterDto();

        return new CharacterDto
        {
            Name = character.Name,
            DirectionDto = MapDirectionToDto(character.Direction),
            //StateDto = (CharacterStateDto)character., // Enums idênticos
            FloorIndex = character.FloorIndex,
            StatsDto = MapToDto(character.Stats),
            VitalsDto = MapToDto(character.Vital),
            BoundingBoxDto = MapToDto(character.BoundingBox)
        };
    }

    /// <inheritdoc />
    public AccountDto MapToDto(Account account)
    {
        if (account == null)
            return new AccountDto();

        return new AccountDto
        {
            Username = account.Username,
        };
    }

    /// <inheritdoc />
    public StatsDto MapToDto(Stats stats)
    {
        if (stats == null)
            return new StatsDto();

        return new StatsDto
        {
            Strength = (int)stats.Strength,
            Defense = (int)stats.Defense,
            Agility = (int)stats.Agility
        };
    }

    /// <inheritdoc />
    public VitalsDto MapToDto(Vitals vitals)
    {
        if (vitals == null)
            return new VitalsDto();

        return new VitalsDto
        {
            Health = (int)vitals.Health,
            MaxHealth = (int)vitals.MaxHealth,
            Mana = (int)vitals.Mana,
            MaxMana = (int)vitals.MaxMana
        };
    }

    /// <inheritdoc />
    public BoundingBoxDto MapToDto(PositionBox positionBox)
    {
        if (positionBox == null)
            return new BoundingBoxDto();

        return new BoundingBoxDto
        {
            X = (int)positionBox.X,
            Y = (int)positionBox.Y,
            Width = (int)positionBox.Width,
            Height = (int)positionBox.Height
        };
    }

    /// <inheritdoc />
    public CharacterDto CreateMinimalCharacterDto(string name, long id = 0)
    {
        return new CharacterDto
        {
            Name = name,
            DirectionDto = DirectionDto.Down, // Valor padrão
            FloorIndex = 0,
            BoundingBoxDto = new BoundingBoxDto(),
            StatsDto = new StatsDto(),
            VitalsDto = new VitalsDto()
        };
    }

    /// <summary>
    /// Converte um Direction do domínio para um DirectionDto
    /// </summary>
    private DirectionDto MapDirectionToDto(Direction direction)
    {
        return direction switch
        {
            Direction.North => DirectionDto.Up,
            Direction.South => DirectionDto.Down,
            Direction.East => DirectionDto.Right,
            Direction.West => DirectionDto.Left,
            Direction.NorthEast => DirectionDto.UpRight,
            Direction.NorthWest => DirectionDto.UpLeft,
            Direction.SouthEast => DirectionDto.DownRight,
            Direction.SouthWest => DirectionDto.DownLeft,
            _ => DirectionDto.Down // Valor padrão
        };
    }
}