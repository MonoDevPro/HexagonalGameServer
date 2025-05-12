// filepath: /home/filipe/Desenvolvimento/NOVOSERVER/HexagonalGameServer/Server.Application/Ports.Outbound/IAccountService.cs

using Server.Application.Services;
using Server.Domain.Entities;
using Server.Domain.Enums;
using Server.Domain.Policies;
using Server.Domain.ValueObjects;
using Server.Domain.ValueObjects.Account;

namespace Server.Application.Ports.Outbound;

/// <summary>
/// Interface para o serviço de conta (port outbound) 
/// </summary>
public interface IAccountService
{
    Task<bool> CreateAccountAsync(AccountCreationOptions options);
    Task<AccountAuthentication?> AuthenticateAsync(string username, string password);
    Task<bool> ChangePasswordAsync(AccountAuthentication authentication, string newPassword);
    
    // Gerenciamento de estado da conta
    Task<bool> ActivateAccountAsync(AccountAuthentication authentication);
    Task<bool> BanAccountAsync(AccountAuthentication authentication, string reason);
    Task<bool> LockAccountAsync(AccountAuthentication authentication);
    Task<bool> SuspendAccountAsync(AccountAuthentication authentication, TimeSpan duration, string reason);
    Task<bool> DeleteAccountAsync(AccountAuthentication authentication);
    Task<IReadOnlySet<AccountState>> GetPossibleStateTransitionsAsync(AccountAuthentication authentication);
    Task<AccountState> GetAccountStateAsync(AccountAuthentication authentication);
    
    // Gerenciamento de personagens
    Task<IReadOnlyCollection<Character>> GetCharactersAsync(AccountAuthentication authentication);
    Task<bool> AttachCharacterAsync(AccountAuthentication authentication, long characterId);
    Task<bool> DetachCharacterAsync(AccountAuthentication authentication, long characterId);
    Task<Character> GetCharacterByIdAsync(AccountAuthentication authentication, long characterId);
    Task<Character> GetCharacterByNameAsync(AccountAuthentication authentication, string characterName);
}