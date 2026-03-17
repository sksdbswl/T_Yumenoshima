using UnityEngine;

public class StoryRewardManager : SingletonBase<StoryRewardManager>
{
    private static string GetRewardKey(int npcId, int npcStoryStage)
        => $"reward_{npcId}_{npcStoryStage}";

    public bool HasReceivedReward(int npcId, int npcStoryStage)
        => PlayerPrefs.GetInt(GetRewardKey(npcId, npcStoryStage), 0) == 1;

    private void MarkReceived(int npcId, int npcStoryStage)
    {
        PlayerPrefs.SetInt(GetRewardKey(npcId, npcStoryStage), 1);
        PlayerPrefs.Save();
    }

    public void TryGrantStoryReward(int npcId, int npcStoryStage)
    {
        if (HasReceivedReward(npcId, npcStoryStage))
            return;

        var reward = RewardRepository.Singleton.GetReward(npcId, npcStoryStage);
        if (reward == null)
        {
            // 이 스테이지는 보상 없는 스테이지
            return;
        }

        Debug.Log($"[StoryReward] Grant npc={npcId}, stage={npcStoryStage}, gold={reward.Gold}, item={reward.ItemId}");

        // 실제 보상 적용 (예시)
        // PlayerInventory.AddGold(reward.Gold);
        // PlayerInventory.AddItem(reward.ItemId, reward.ItemCount);
        // PlayerStatus.AddExp(reward.Exp);

        MarkReceived(npcId, npcStoryStage);
    }
}