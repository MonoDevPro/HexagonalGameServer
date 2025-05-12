using Server.Domain.Enums;

namespace Server.Domain.ValueObjects.Account;

/// <summary>
/// Opções para a criação de uma nova conta de usuário
/// </summary>
public class AccountCreationOptions
{
    // Propriedades obrigatórias
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!; // Senha não-hasheada
    
    // Propriedades com valores padrão
    public AccountState InitialState { get; set; } = AccountState.Created;
    
    // Validação básica
    public bool Validate(out List<string> validationErrors)
    {
        validationErrors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Username))
            validationErrors.Add("Username cannot be empty");
            
        if (string.IsNullOrWhiteSpace(Password))
            validationErrors.Add("Password cannot be empty");
            
        // if (RequireEmailVerification && string.IsNullOrWhiteSpace(Email))
        //     validationErrors.Add("Email is required when email verification is enabled");
        
        // Validações adicionais (ex: complexidade de senha, formato de email, etc.)
        if (!string.IsNullOrWhiteSpace(Username) && Username.Length < 3)
            validationErrors.Add("Username must be at least 3 characters");
            
        if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 8)
            validationErrors.Add("Password must be at least 8 characters");
            
        return validationErrors.Count == 0;
    }

    /// <summary>
    /// Valida as opções e lança uma exceção se forem inválidas
    /// </summary>
    public void ValidateAndThrow()
    {
        if (!Validate(out var errors))
        {
            throw new ArgumentException($"Invalid character creation options: {string.Join(", ", errors)}");
        }
    }
}