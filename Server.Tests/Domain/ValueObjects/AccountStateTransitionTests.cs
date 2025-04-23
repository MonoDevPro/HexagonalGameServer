using Server.Domain.Enum;
using Server.Domain.ValueObjects;
using Xunit;

namespace Server.Tests.Domain.ValueObjects;

public class AccountStateTransitionTests
{
    [Fact]
    public void IsValidTransition_SameState_ReturnsTrue()
    {
        // Arrange
        var currentState = AccountState.Activated;
        var targetState = AccountState.Activated;
        
        // Act
        var result = AccountStateTransition.IsValidTransition(currentState, targetState);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(AccountState.Created, AccountState.Activated)]
    [InlineData(AccountState.Created, AccountState.Locked)]
    [InlineData(AccountState.Created, AccountState.Suspended)]
    [InlineData(AccountState.Created, AccountState.Banned)]
    [InlineData(AccountState.Created, AccountState.Deleted)]
    public void IsValidTransition_FromCreated_ReturnsTrue(AccountState currentState, AccountState targetState)
    {
        // Act
        var result = AccountStateTransition.IsValidTransition(currentState, targetState);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(AccountState.Activated, AccountState.Locked)]
    [InlineData(AccountState.Activated, AccountState.Suspended)]
    [InlineData(AccountState.Activated, AccountState.Banned)]
    [InlineData(AccountState.Activated, AccountState.Deleted)]
    public void IsValidTransition_FromActivated_ReturnsTrue(AccountState currentState, AccountState targetState)
    {
        // Act
        var result = AccountStateTransition.IsValidTransition(currentState, targetState);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(AccountState.Locked, AccountState.Activated)]
    [InlineData(AccountState.Locked, AccountState.Suspended)]
    [InlineData(AccountState.Locked, AccountState.Banned)]
    [InlineData(AccountState.Locked, AccountState.Deleted)]
    public void IsValidTransition_FromLocked_ReturnsTrue(AccountState currentState, AccountState targetState)
    {
        // Act
        var result = AccountStateTransition.IsValidTransition(currentState, targetState);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(AccountState.Suspended, AccountState.Activated)]
    [InlineData(AccountState.Suspended, AccountState.Locked)]
    [InlineData(AccountState.Suspended, AccountState.Banned)]
    [InlineData(AccountState.Suspended, AccountState.Deleted)]
    public void IsValidTransition_FromSuspended_ReturnsTrue(AccountState currentState, AccountState targetState)
    {
        // Act
        var result = AccountStateTransition.IsValidTransition(currentState, targetState);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(AccountState.Banned, AccountState.Activated)]
    [InlineData(AccountState.Banned, AccountState.Deleted)]
    public void IsValidTransition_FromBanned_ReturnsTrue(AccountState currentState, AccountState targetState)
    {
        // Act
        var result = AccountStateTransition.IsValidTransition(currentState, targetState);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(AccountState.Deleted, AccountState.Created)]
    [InlineData(AccountState.Deleted, AccountState.Activated)]
    [InlineData(AccountState.Deleted, AccountState.Locked)]
    [InlineData(AccountState.Deleted, AccountState.Suspended)]
    [InlineData(AccountState.Deleted, AccountState.Banned)]
    public void IsValidTransition_FromDeleted_ReturnsFalse(AccountState currentState, AccountState targetState)
    {
        // Act
        var result = AccountStateTransition.IsValidTransition(currentState, targetState);
        
        // Assert
        Assert.False(result);
    }
    
    [Theory]
    [InlineData(AccountState.Created)]
    [InlineData(AccountState.Activated)]
    [InlineData(AccountState.Locked)]
    [InlineData(AccountState.Suspended)]
    [InlineData(AccountState.Banned)]
    [InlineData(AccountState.Deleted)]
    public void GetPossibleTransitions_ReturnsCorrectTransitions(AccountState state)
    {
        // Act
        var possibleTransitions = AccountStateTransition.GetPossibleTransitions(state);
        
        // Assert
        // Verificar se o conjunto não é nulo
        Assert.NotNull(possibleTransitions);
        
        // Verificar se o conjunto contém os estados esperados com base nas regras de transição
        switch (state)
        {
            case AccountState.Created:
                Assert.Equal(5, possibleTransitions.Count);
                Assert.Contains(AccountState.Activated, possibleTransitions);
                Assert.Contains(AccountState.Locked, possibleTransitions);
                Assert.Contains(AccountState.Suspended, possibleTransitions);
                Assert.Contains(AccountState.Banned, possibleTransitions);
                Assert.Contains(AccountState.Deleted, possibleTransitions);
                break;
                
            case AccountState.Activated:
                Assert.Equal(4, possibleTransitions.Count);
                Assert.Contains(AccountState.Locked, possibleTransitions);
                Assert.Contains(AccountState.Suspended, possibleTransitions);
                Assert.Contains(AccountState.Banned, possibleTransitions);
                Assert.Contains(AccountState.Deleted, possibleTransitions);
                break;
                
            case AccountState.Locked:
                Assert.Equal(4, possibleTransitions.Count);
                Assert.Contains(AccountState.Activated, possibleTransitions);
                Assert.Contains(AccountState.Suspended, possibleTransitions);
                Assert.Contains(AccountState.Banned, possibleTransitions);
                Assert.Contains(AccountState.Deleted, possibleTransitions);
                break;
                
            case AccountState.Suspended:
                Assert.Equal(4, possibleTransitions.Count);
                Assert.Contains(AccountState.Activated, possibleTransitions);
                Assert.Contains(AccountState.Locked, possibleTransitions);
                Assert.Contains(AccountState.Banned, possibleTransitions);
                Assert.Contains(AccountState.Deleted, possibleTransitions);
                break;
                
            case AccountState.Banned:
                Assert.Equal(2, possibleTransitions.Count);
                Assert.Contains(AccountState.Activated, possibleTransitions);
                Assert.Contains(AccountState.Deleted, possibleTransitions);
                break;
                
            case AccountState.Deleted:
                Assert.Empty(possibleTransitions);
                break;
        }
    }
}