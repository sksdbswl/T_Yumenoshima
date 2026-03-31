public interface IPlayerStatus
{
    Const.JobType CurrentJobType { get; } 
    float CurrentHp { get; }
    float MaxHp { get; }

    float CurrentStamina { get; }
    float MaxStamina { get; }

    Const.EEmotion CurrentEmotion { get; }

    void TakeDamage(float amount);
    void Heal(float amount);

    void UseStamina(float amount);
    void RecoverStamina(float amount);

    void ChangeEmotion(Const.EEmotion emotion);
    void ChangeJob(Const.JobType job);
}