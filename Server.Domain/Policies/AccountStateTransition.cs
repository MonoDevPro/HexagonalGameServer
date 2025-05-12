using Server.Domain.Enums;

namespace Server.Domain.Policies;

/// <summary>
/// Classe que define e valida as transições possíveis entre estados de conta
/// </summary>
public class AccountStateTransition
{
    // Define o mapa de transições válidas para cada estado
    private static readonly Dictionary<AccountState, IReadOnlySet<AccountState>> AllowedTransitions = new()
    {
        // De Created, pode ir para Activated, Locked, Suspended, Banned ou Deleted
        [AccountState.Created] = new HashSet<AccountState> 
        { 
            AccountState.Activated, 
            AccountState.Locked, 
            AccountState.Suspended, 
            AccountState.Banned, 
            AccountState.Deleted 
        },
        
        // De Activated, pode ir para Locked, Suspended, Banned ou Deleted
        [AccountState.Activated] = new HashSet<AccountState> 
        { 
            AccountState.Locked, 
            AccountState.Suspended, 
            AccountState.Banned, 
            AccountState.Deleted 
        },
        
        // De Locked, pode ir para Activated, Suspended, Banned ou Deleted
        [AccountState.Locked] = new HashSet<AccountState> 
        { 
            AccountState.Activated, 
            AccountState.Suspended, 
            AccountState.Banned, 
            AccountState.Deleted 
        },
        
        // De Suspended, pode ir para Activated, Locked, Banned ou Deleted
        [AccountState.Suspended] = new HashSet<AccountState> 
        { 
            AccountState.Activated, 
            AccountState.Locked, 
            AccountState.Banned, 
            AccountState.Deleted 
        },
        
        // De Banned, pode ir apenas para Activated ou Deleted
        [AccountState.Banned] = new HashSet<AccountState> 
        { 
            AccountState.Activated, 
            AccountState.Deleted 
        },
        
        // De Deleted, não pode ir para nenhum outro estado
        [AccountState.Deleted] = new HashSet<AccountState>()
    };

    /// <summary>
    /// Verifica se uma transição de estado é válida
    /// </summary>
    /// <param name="currentState">Estado atual da conta</param>
    /// <param name="targetState">Estado alvo para o qual se deseja transitar</param>
    /// <returns>Verdadeiro se a transição for válida, falso caso contrário</returns>
    public static bool IsValidTransition(AccountState currentState, AccountState targetState)
    {
        // Transição para o mesmo estado é sempre válida
        if (currentState == targetState)
            return true;
            
        // Verificar no mapa de transições
        if (AllowedTransitions.TryGetValue(currentState, out var allowedTargetStates))
        {
            return allowedTargetStates.Contains(targetState);
        }
        
        // Estado atual não está no mapa (não deveria acontecer)
        return false;
    }
    
    /// <summary>
    /// Obtém todos os estados possíveis para os quais uma conta pode transitar a partir do estado atual
    /// </summary>
    /// <param name="currentState">Estado atual da conta</param>
    /// <returns>Conjunto com todos os estados possíveis</returns>
    public static IReadOnlySet<AccountState> GetPossibleTransitions(AccountState currentState)
    {
        if (AllowedTransitions.TryGetValue(currentState, out var allowedTargetStates))
        {
            return allowedTargetStates;
        }
        
        return new HashSet<AccountState>();
    }
}