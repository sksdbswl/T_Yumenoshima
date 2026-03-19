using UnityEngine;

public class NpcEmotionManager : BasePoolManager<Const.EEmotion>
{
    private static NpcEmotionManager _instance;
    public static NpcEmotionManager Instance 
    {
        get 
        {
            if (_instance == null) _instance = FindFirstObjectByType<NpcEmotionManager>();
            return _instance;
        }
    }

    protected override void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // BasePoolManager의 Initialize() 실행
        base.Awake(); 
    }
    
    public EmotionIcon ShowEmotion(Const.EEmotion emotion, Transform target, Vector3 offset = default)
    {
        if (emotion == Const.EEmotion.None) return null;

        EmotionIcon icon = Pop<EmotionIcon>(emotion);
        
        if (icon != null)
            icon.EmotionPlay(target, offset);
        
        return icon;
    }
    
    public void ReturnEmotion(EmotionIcon icon)
    {
        Push(icon);
    }
}