using TMPro;
using UnityEngine;

public class Dashboard : SingletonBase<Dashboard>
{ 
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI job;
    [SerializeField] private TextMeshProUGUI quest;
    
    private void Awake()
    {
        
    }

    private void Start()
    {
        var accepted = PlayerDialogueProgress.Singleton.GetAcceptedQuests();
    
        for (int i = 0; i < accepted.Count; i++)
        {
            var entry = accepted[i];
            var data = GameManager.Singleton.GetQuestData(entry.questId);
    
            Debug.Log($"퀘스트 이름: {data.questName}");
            
            quest.text = data.questName;
        }

        Init();
    }

    public void Init()
    {
        job.text = player._playerStatus.CurrentJobType.ToString();
    }
}