using Server.Domain.Entities.Primitives;
using Server.Domain.Enums;

namespace Server.Domain.ValueObjects.Character;

public class CharacterCreationOptions
{
    // Propriedades obrigatórias
    public string Name { get; set; } = string.Empty;
    
    // Propriedades com valores padrão
    public Stats Stats { get; set; } = new (strength: 10, defense: 10, agility: 10);
    public Vitals Vitals { get; set; } = new (health: 100, maxHealth: 100, mana: 50, maxMana: 50);
    public PositionBox PositionBox { get; set; } = new (0, 0, 32, 32);
    public Direction Direction { get; set; } = Direction.South;
    public CharacterState State { get; set; } = CharacterState.Idle;
    public int FloorIndex { get; set; } = 1;

    /// <summary>
    /// Valida se as opções de criação de personagem são válidas
    /// </summary>
    /// <param name="validationErrors">Lista de erros de validação, se houver</param>
    /// <returns>True se todas as validações passarem, False caso contrário</returns>
    public bool Validate(out List<string> validationErrors)
    {
        validationErrors = [];
        
        // Validação do nome
        if (string.IsNullOrWhiteSpace(Name))
        {
            validationErrors.Add("Character name cannot be empty");
        }
        else if (Name.Length < 3)
        {
            validationErrors.Add("Character name must be at least 3 characters long");
        }
        else if (Name.Length > 20)
        {
            validationErrors.Add("Character name cannot exceed 20 characters");
        }
        
        // Stats
        if (Stats.Strength < 1 || Stats.Strength > 100)
            validationErrors.Add("Strength must be between 1 and 100");
                
        if (Stats.Agility < 1 || Stats.Agility > 100)
            validationErrors.Add("Agility must be between 1 and 100");
                
        if (Stats.Defense < 1 || Stats.Defense > 100)
            validationErrors.Add("Defense must be between 1 and 100");
        
        // Vitals
        if (Vitals.Health < 0)
            validationErrors.Add("Health cannot be negative");
                
        if (Vitals.MaxHealth <= 0)
            validationErrors.Add("Max health must be greater than zero");
                
        if (Vitals.Health > Vitals.MaxHealth)
            validationErrors.Add("Health cannot exceed max health");
                
        if (Vitals.Mana < 0)
            validationErrors.Add("Mana cannot be negative");
                
        if (Vitals.MaxMana < 0)
            validationErrors.Add("Max mana cannot be negative");
                
        if (Vitals.Mana > Vitals.MaxMana)
            validationErrors.Add("Mana cannot exceed max mana");
        
        // Se necessário, adicionar validações específicas para as dimensões
        if (PositionBox.Width <= 0 || PositionBox.Height <= 0)
            validationErrors.Add("Bounding box dimensions must be positive");
        
        // Validação de FloorIndex
        if (FloorIndex < 0)
        {
            validationErrors.Add("Floor index cannot be negative");
        }
        
        // Retorna true se não houver erros
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