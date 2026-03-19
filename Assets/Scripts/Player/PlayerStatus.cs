using UnityEngine;

public sealed class PlayerStatus : MonoBehaviour, IPlayerStatus
{
    [Header("Asset")]
    [SerializeField] private int Money = 10000000;
    
    [Header("HP")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float currentHp = 100f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;

    [Header("Emotion / Status")]
    [SerializeField] private Const.EEmotion currentEmotion = Const.EEmotion.Neutral;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    public Const.EEmotion CurrentEmotion => currentEmotion;

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        currentHp = Mathf.Max(0f, currentHp - amount);
     
        // animator.SetTrigger("Attack"); 

        if (currentHp <= 0f)
        {
            OnDead();
        }
    }
    
    public void TakeSteal(int amount)
    {
        if (amount <= 0f) return;

        Money -= amount;

        if (Money <= 0f)
        {
           Debug.Log("더 이상 가진 자산이 없습니다.");
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }

    public void UseStamina(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Max(0f, currentStamina - amount);
    }

    public void RecoverStamina(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
    }

    public void ChangeEmotion(Const.EEmotion emotion)
    {
        currentEmotion = emotion;
    }

    private void OnDead()
    {
        Debug.Log("Player Dead");
    }
}