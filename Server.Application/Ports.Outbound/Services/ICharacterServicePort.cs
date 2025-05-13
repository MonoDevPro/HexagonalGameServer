// filepath: /home/filipe/Desenvolvimento/NOVOSERVER/HexagonalGameServer/Server.Application/Ports.Outbound/ICharacterService.cs

using Server.Domain.Entities;
using Server.Domain.Policies;
using Server.Domain.ValueObjects.Character;

namespace Server.Application.Ports.Outbound.Services;

/// <summary>
/// Interface para o serviço de personagem (port outbound)
/// </summary>
public interface ICharacterServicePort
{
    // Operações básicas de CRUD
    Task<Character?> GetCharacterByIdAsync(AccountAuthentication authentication, long id);
    Task<Character?> GetCharacterByNameAsync(AccountAuthentication authentication, string name);
    Task<IEnumerable<Character>> GetAllCharactersAsync(AccountAuthentication authentication);
    Task<Character> CreateCharacterAsync(AccountAuthentication authentication, CharacterCreationOptions options);
    
    // Outros serviços que utilizam a entidade, como uma movimentação ,etc....
}