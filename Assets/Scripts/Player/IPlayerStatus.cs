public interface IPlayerStatus
{
    float CurrentHp { get; }
    float MaxHp { get; }

    float CurrentStamina { get; }
    float MaxStamina { get; }

    Const.EEmotion CurrentEmotion { get; }
    Const.EStatusEffect CurrentStatusEffects { get; }

    void TakeDamage(float amount);
    void Heal(float amount);

    void UseStamina(float amount);
    void RecoverStamina(float amount);

    void ChangeEmotion(Const.EEmotion emotion);

    void ApplyStatusEffect(Const.EStatusEffect effect);
    void RemoveStatusEffect(Const.EStatusEffect effect);
    bool HasStatusEffect(Const.EStatusEffect effect);
}