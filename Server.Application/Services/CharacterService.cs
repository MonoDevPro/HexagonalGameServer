using Server.Application.Ports.Outbound;
using Server.Application.Ports.Outbound.Messaging;
using Server.Application.Ports.Outbound.Persistence;
using Server.Domain.Entities;
using Server.Domain.Entities.Primitives;
using Server.Domain.Enums;
using Server.Domain.Policies;
using Server.Domain.ValueObjects;
using Server.Domain.ValueObjects.Character;

namespace Server.Application.Services;

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IGameEventPublisher _eventPublisher;

    // Os repositórios aqui injetados são de cenário real, de persistencia em banco de dados.
    public CharacterService(
        ICharacterRepository characterRepository,
        IGameEventPublisher eventPublisher)
    {
        _characterRepository = characterRepository;
        _eventPublisher = eventPublisher;
    }

    // Operações básicas de CRUD
    public async Task<Character?> GetCharacterByIdAsync(AccountAuthentication authentication, long id)
    {
        if (!authentication.CheckValidation())
            return null;
        
        return await _characterRepository.GetByIdAsync(id);
    }

    public async Task<Character?> GetCharacterByNameAsync(AccountAuthentication authentication, string name)
    {
        if (!authentication.CheckValidation())
            return null;
        
        return await _characterRepository.GetByNameAsync(name);
    }

    public async Task<IEnumerable<Character>> GetAllCharactersAsync(AccountAuthentication authentication)
    {
        if (!authentication.CheckValidation())
            return [];
        
        return await _characterRepository.GetAllAsync();
    }

    // Criação de personagem
    public async Task<Character> CreateCharacterAsync(AccountAuthentication authentication, CharacterCreationOptions options)
    {
        if (await _characterRepository.ExistsAsync(options.Name))
            throw new InvalidOperationException($"Character with name '{options.Name}' already exists.");

        var character = new Character(options);
        
        await _characterRepository.AddAsync(character);
        await PublishDomainEventsAsync(character);

        return character;
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