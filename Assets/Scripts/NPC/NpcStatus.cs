using UnityEngine;
using UnityEngine.AI;

public sealed class NpcStatus : MonoBehaviour, INpcStatus
{
    [Header("Asset")]
    [SerializeField] private int Money = 10000000;
    
    [Header("HP")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float currentHp = 100f;

    [Header("Emotion / Status")]
    [SerializeField] private Const.EEmotion currentEmotion = Const.EEmotion.Neutral;
    [SerializeField] private Const.EStatusEffect currentStatusEffects = Const.EStatusEffect.None;
    
    private NavMeshAgent agent;
    private INpcStatus _npcStatusImplementation;
    
    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;

    public Const.EEmotion CurrentEmotion => currentEmotion;
    public Const.EStatusEffect CurrentStatusEffects => currentStatusEffects;

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
        agent = GetComponent<NavMeshAgent>();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        currentHp = Mathf.Max(0f, currentHp - amount);

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
    
    public void ChangeEmotion(Const.EEmotion emotion)
    {
        currentEmotion = emotion;

        if (agent != null)
        {
            if (emotion == Const.EEmotion.Tired)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            else
            {
                agent.isStopped = false;
            }
        }
    }
    
    public void ApplyStatusEffect(Const.EStatusEffect effect)
    {
        currentStatusEffects |= effect;
    }

    public void RemoveStatusEffect(Const.EStatusEffect effect)
    {
        currentStatusEffects &= ~effect;
    }

    public bool HasStatusEffect(Const.EStatusEffect effect)
    {
        return (currentStatusEffects & effect) == effect;
    }

    private void OnDead()
    {
        Debug.Log("Player Dead");
    }
}