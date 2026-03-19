using System;

public enum DSDialogueActionTrigger { OnEnter, OnExit, OnDialogueEnd }
public enum DSDialogueActionType { SetNpcStoryStage, SetQuestState, SetFlag, CallMethod }

[Serializable]
public class DSDialogueActionData
{
    public DSDialogueActionTrigger trigger = DSDialogueActionTrigger.OnExit;
    public DSDialogueActionType type;

    public string npcId;
    public int npcStoryStage;

    public string questId;
    public QuestState questState;

    public string flag;

    public string receiverType; // 호출 대상 타입
    public string methodName;   // 호출할 메서드 이름
    
    public QuestMetaData questMeta = new QuestMetaData();
}