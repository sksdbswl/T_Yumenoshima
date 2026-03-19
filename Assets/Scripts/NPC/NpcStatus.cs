using UnityEngine;
using UnityEngine.AI;

public sealed class NpcStatus : MonoBehaviour, INpcStatus
{
    [Header("Asset")]
    [SerializeField] private int Money = 10000000;
    
    [Header("HP")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float currentHp = 100f;

    [Header("Emotion")]
    [SerializeField] private Const.EEmotion currentEmotion = Const.EEmotion.Neutral;
    
    private NavMeshAgent agent;
    private INpcStatus _npcStatusImplementation;
    
    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsAnomaly { get; set; }

    public Const.EEmotion CurrentEmotion => currentEmotion;

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
    
    public void ChangeEmotion(Npc npc, Const.EEmotion emotion)
    {
        currentEmotion = emotion;
        
        if (emotion == Const.EEmotion.Tired)
        {
            agent.isStopped = true;
            npc.executor.isAnomaly = true;
        }
        else
        {
            agent.isStopped = false;
        }
    }
    
    private void OnDead()
    {
        Debug.Log("Player Dead");
    }
}