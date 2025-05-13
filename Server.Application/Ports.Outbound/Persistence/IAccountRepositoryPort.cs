using Server.Domain.Entities;

namespace Server.Application.Ports.Outbound.Persistence;

public interface IAccountRepositoryPort
{
    Task<Account?> GetByIdAsync(long id);
    Task<Account?> GetByUsernameAsync(string username);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(Account account);
    Task<bool> ExistsAsync(string username);
}