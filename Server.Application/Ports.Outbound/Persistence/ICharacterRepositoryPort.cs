using Server.Domain.Entities;

namespace Server.Application.Ports.Outbound.Persistence;

public interface ICharacterRepositoryPort
{
    Task<Character?> GetByIdAsync(long id);
    Task<Character?> GetByNameAsync(string name);
    Task<IEnumerable<Character>> GetAllAsync();
    Task AddAsync(Character character);
    Task UpdateAsync(Character character);
    Task DeleteAsync(Character character);
    Task<bool> ExistsAsync(string name);
    Task<bool> ExistsAsync(long id);
}