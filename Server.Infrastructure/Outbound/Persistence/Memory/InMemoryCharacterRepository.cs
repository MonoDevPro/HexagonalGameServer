using Server.Application.Ports.Outbound.Persistence;
using Server.Domain.Entities;

namespace Server.Infrastructure.Outbound.Persistence.Memory;

public class InMemoryCharacterRepository : ICharacterRepositoryPort
{
    private readonly Dictionary<long, Character> _characters = new();
    private readonly Dictionary<string, Character> _charactersByName = new(StringComparer.OrdinalIgnoreCase);
    private long _nextId = 1;

    public Task<Character?> GetByIdAsync(long id)
    {
        _characters.TryGetValue(id, out var character);
        return Task.FromResult(character);
    }

    public Task<Character?> GetByNameAsync(string name)
    {
        _charactersByName.TryGetValue(name, out var character);
        return Task.FromResult(character);
    }

    public Task<IEnumerable<Character>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Character>>(_characters.Values.ToList());
    }

    public Task AddAsync(Character character)
    {
        // Simular geração de ID
        var field = typeof(Entity).GetField("Id", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic);
            
        if (field != null)
        {
            field.SetValue(character, _nextId++);
        }

        _characters[character.Id] = character;
        _charactersByName[character.Name] = character;
        
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Character character)
    {
        if (_characters.ContainsKey(character.Id))
        {
            _characters[character.Id] = character;
            
            // Atualizar o dicionário por nome caso o nome tenha mudado
            var oldName = _charactersByName.FirstOrDefault(x => x.Value.Id == character.Id).Key;
            if (oldName != null && oldName != character.Name)
            {
                _charactersByName.Remove(oldName);
            }
            
            _charactersByName[character.Name] = character;
        }
        
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Character character)
    {
        _characters.Remove(character.Id);
        _charactersByName.Remove(character.Name);
        
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string name)
    {
        return Task.FromResult(_charactersByName.ContainsKey(name));
    }

    public Task<bool> ExistsAsync(long id)
    {
        return Task.FromResult(_characters.ContainsKey(id));
    }
}