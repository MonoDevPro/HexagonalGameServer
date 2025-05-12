using Server.Domain.Enums;
using Server.Domain.ValueObjects;
using Server.Domain.ValueObjects.Account;

namespace Server.Application.Factories;

/// <summary>
/// Factory para criar opções de criação de conta com configurações pré-definidas
/// </summary>
public static class AccountTemplateFactory
{
    /// <summary>
    /// Cria opções padrão para uma conta de jogador
    /// </summary>
    public static AccountCreationOptions CreatePlayerAccount(string username, string password)
    {
        return new AccountCreationOptions
        {
            Username = username,
            Password = password,
            InitialState = AccountState.Created,
        };
    }
    
    // /// <summary>
    // /// Cria opções para uma conta de moderador
    // /// </summary>
    // public static AccountCreationOptions CreateModeratorAccount(
    //     string username, string password, string email)
    // {
    //     var options = CreateVerifiedPlayerAccount(username, password, email);
    //     options.Metadata["Role"] = "Moderator";
    //     options.Metadata["CanModerate"] = true;
    //     return options;
    // }
    
    // /// <summary>
    // /// Cria opções para uma conta de administrador
    // /// </summary>
    // public static AccountCreationOptions CreateAdministratorAccount(
    //     string username, string password, string email)
    // {
    //     var options = CreateVerifiedPlayerAccount(username, password, email);
    //     options.Metadata["Role"] = "Administrator";
    //     options.Metadata["CanModerate"] = true;
    //     options.Metadata["CanAdminister"] = true;
    //     options.InitialState = AccountState.Activated; // Contas de admin já são ativadas
    //     return options;
    // }
}