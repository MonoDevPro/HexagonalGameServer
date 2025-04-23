using Server.Domain.Entities;
using Server.Domain.Enum;
using Server.Domain.Repositories;
using Server.Domain.Services;
using Server.Domain.ValueObjects;
using Server.Domain.ValueObjects.Primitives;

namespace Server.Application.Services;

public class CharacterService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IGameEventPublisher _eventPublisher;

    public CharacterService(
        ICharacterRepository characterRepository,
        IGameEventPublisher eventPublisher)
    {
        _characterRepository = characterRepository;
        _eventPublisher = eventPublisher;
    }

    // Operações básicas de CRUD
    public async Task<Character?> GetCharacterByIdAsync(long id)
    {
        return await _characterRepository.GetByIdAsync(id);
    }

    public async Task<Character?> GetCharacterByNameAsync(string name)
    {
        return await _characterRepository.GetByNameAsync(name);
    }

    public async Task<IEnumerable<Character>> GetAllCharactersAsync()
    {
        return await _characterRepository.GetAllAsync();
    }

    // Criação de personagem
    public async Task<Character> CreateCharacterAsync(
        string name,
        Stats stats,
        Vital vital,
        BoundingBox boundingBox,
        Direction direction,
        int floorIndex)
    {
        if (await _characterRepository.ExistsAsync(name))
            throw new InvalidOperationException($"Character with name '{name}' already exists.");

        var character = new Character(
            name,
            stats,
            vital,
            boundingBox,
            direction,
            CharacterState.Idle,
            floorIndex);

        await _characterRepository.AddAsync(character);
        await PublishDomainEventsAsync(character);

        return character;
    }

    // Movimento
    public async Task MoveCharacterAsync(long characterId, Direction direction)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.Move(direction);
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    public async Task StopCharacterMovementAsync(long characterId)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.StopMoving();
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    public async Task ChangeFloorAsync(long characterId, int newFloorIndex)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.ChangeFloor(newFloorIndex);
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    // Combate
    public async Task AttackAsync(long attackerId, long targetId)
    {
        var attacker = await GetCharacterOrThrowAsync(attackerId);
        var target = await GetCharacterOrThrowAsync(targetId);
        
        attacker.Attack(target);
        
        await _characterRepository.UpdateAsync(attacker);
        await _characterRepository.UpdateAsync(target);
        
        await PublishDomainEventsAsync(attacker);
        await PublishDomainEventsAsync(target);
    }

    public async Task TakeDamageAsync(long characterId, double amount)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.TakeDamage(amount);
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    public async Task HealAsync(long characterId, double amount)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.Heal(amount);
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    public async Task UseManaAsync(long characterId, double amount)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.UseMana(amount);
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    public async Task RestoreManaAsync(long characterId, double amount)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.RestoreMana(amount);
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    public async Task KillCharacterAsync(long characterId)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.Die();
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    public async Task ReviveCharacterAsync(long characterId, double healthPercentage = 0.1)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.Revive(healthPercentage);
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    // Progressão de personagem
    public async Task IncreaseStatsAsync(long characterId, Stats statsIncrease)
    {
        var character = await GetCharacterOrThrowAsync(characterId);
        
        character.IncreaseStats(statsIncrease);
        
        await _characterRepository.UpdateAsync(character);
        await PublishDomainEventsAsync(character);
    }

    // Método auxiliar para buscar personagem ou lançar exceção
    private async Task<Character> GetCharacterOrThrowAsync(long characterId)
    {
        var character = await _characterRepository.GetByIdAsync(characterId);
        if (character == null)
            throw new InvalidOperationException($"Character with ID {characterId} not found.");
            
        return character;
    }

    // Método auxiliar para publicar eventos de domínio
    private async Task PublishDomainEventsAsync(Character character)
    {
        var domainEvents = character.GetDomainEvents();
        
        foreach (var domainEvent in domainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent);
        }
        
        character.ClearDomainEvents();
    }
}