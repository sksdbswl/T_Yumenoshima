using UnityEngine;

public interface INpcStatus
{
    float CurrentHp { get; }
    float MaxHp { get; }
    
    bool IsAnomaly { get; set; }
    
    Const.EEmotion CurrentEmotion { get; }
    Transform GetTransform();
    void TakeDamage(float amount);
    void Heal(float amount);

    void ChangeEmotion(Npc npc, Const.EEmotion emotion);
}