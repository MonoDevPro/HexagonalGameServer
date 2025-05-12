namespace Server.Domain.Entities.Primitives;

public class Vitals : Entity, IEquatable<Vitals>
{
    // Propriedades com validações para garantir valores dentro de limites
    private double _health;
    public double Health
    {
        get => _health;
        set => _health = Math.Clamp(value, 0, MaxHealth);
    }

    private double _maxHealth;
    public double MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = value <= 0 ? 1 : value;
    }

    private double _mana;
    public double Mana
    {
        get => _mana;
        set => _mana = Math.Clamp(value, 0, MaxMana);
    }

    private double _maxMana;
    public double MaxMana
    {
        get => _maxMana;
        set => _maxMana = value <= 0 ? 1 : value;
    }

    // Propriedades derivadas úteis para o jogo
    public double HealthPercentage => Health / MaxHealth * 100;
    public double ManaPercentage => Mana / MaxMana * 100;
    public bool IsDead => Health <= 0;
    public bool IsFullHealth => Health >= MaxHealth;
    public bool IsFullMana => Mana >= MaxMana;

    // Construtor padrão para ORM
    protected Vitals() { }

    public Vitals(double health, double maxHealth, double mana, double maxMana)
    {
        // Definir primeiro os máximos para que as validações dos valores atuais funcionem corretamente
        MaxHealth = maxHealth;
        MaxMana = maxMana;
        Health = health;
        Mana = mana;
    }

    // Métodos para gerenciar os atributos vitais
    public void ApplyDamage(double damage)
    {
        if (damage < 0) return; // Não aceitar dano negativo
        Health = Math.Max(0, Health - damage);
    }

    public void Heal(double amount)
    {
        if (amount < 0) return; // Não aceitar cura negativa
        Health = Math.Min(MaxHealth, Health + amount);
    }

    public void UseMana(double amount)
    {
        if (amount < 0) return; // Não aceitar consumo negativo
        Mana = Math.Max(0, Mana - amount);
    }

    public void RestoreMana(double amount)
    {
        if (amount < 0) return; // Não aceitar restauração negativa
        Mana = Math.Min(MaxMana, Mana + amount);
    }

    public bool HasEnoughMana(double requiredMana)
    {
        return Mana >= requiredMana;
    }

    public void IncreaseMaxHealth(double amount)
    {
        if (amount <= 0) return;
        double oldMaxHealth = MaxHealth;
        MaxHealth += amount;
        
        // Aumenta a vida atual proporcionalmente (mantendo a porcentagem)
        Health = (Health / oldMaxHealth) * MaxHealth;
    }

    public void IncreaseMaxMana(double amount)
    {
        if (amount <= 0) return;
        double oldMaxMana = MaxMana;
        MaxMana += amount;
        
        // Aumenta a mana atual proporcionalmente (mantendo a porcentagem)
        Mana = (Mana / oldMaxMana) * MaxMana;
    }

    // Clone para criar uma cópia independente
    public Vitals Clone()
    {
        return new Vitals(Health, MaxHealth, Mana, MaxMana);
    }

    public bool Equals(Vitals? other)
    {
        if (other is null) return false;
        return Health.Equals(other.Health) && 
               MaxHealth.Equals(other.MaxHealth) && 
               Mana.Equals(other.Mana) && 
               MaxMana.Equals(other.MaxMana);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Health, MaxHealth, Mana, MaxMana);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Vitals);
    }
    
    public override string ToString()
    {
        return $"HP:{Health:F1}/{MaxHealth:F1} MP:{Mana:F1}/{MaxMana:F1}";
    }
}