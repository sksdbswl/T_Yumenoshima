using TMPro;
using UnityEngine;

public class QuestUI : UIBase
{
    // [SerializeField] private RectTransform root;
    // [SerializeField] private TextMeshProUGUI questText;

    private void Awake()
    {
        var accepted = PlayerDialogueProgress.Singleton.GetAcceptedQuests();
    
        for (int i = 0; i < accepted.Count; i++)
        {
            var entry = accepted[i];
            var data = GameManager.Singleton.GetQuestData(entry.questId);
    
            Debug.Log($"퀘스트 이름: {data.questName}");
        }
    }
    
    public void SetQuestText(string text)
    {
        //questText.text = text;
    }
}