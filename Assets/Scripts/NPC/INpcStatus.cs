using UnityEngine;

public interface INpcStatus
{
    float CurrentHp { get; }
    float MaxHp { get; }

    Const.EEmotion CurrentEmotion { get; }
    Const.EStatusEffect CurrentStatusEffects { get; }
    Transform GetTransform();
    void TakeDamage(float amount);
    void Heal(float amount);

    void ChangeEmotion(Const.EEmotion emotion);

    void ApplyStatusEffect(Const.EStatusEffect effect);
    void RemoveStatusEffect(Const.EStatusEffect effect);
    bool HasStatusEffect(Const.EStatusEffect effect);
}