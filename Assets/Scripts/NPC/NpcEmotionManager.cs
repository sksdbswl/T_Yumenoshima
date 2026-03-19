using UnityEngine;

public class NpcEmotionManager : SingletonBase<NpcEmotionManager>
{
    [Header("Emotion Object")]
    [SerializeField] private GameObject exclamation;
    [SerializeField] private GameObject tired;

    public void SetEmotion(Const.EEmotion emotion)
    {
        // 전부 끄기
        exclamation.SetActive(false);
        tired.SetActive(false);

        switch (emotion)
        {
            case Const.EEmotion.Exclamation:
                exclamation.SetActive(true);
                break;

            case Const.EEmotion.Tired:
                tired.SetActive(true);
                break;

            case Const.EEmotion.None:
                break;
        }
    }
}