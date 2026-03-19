using UnityEngine;

public class EmotionIcon : PoolObject<Const.EEmotion>
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0f);
    [SerializeField] private bool billboard = true;
    [SerializeField] private Const.EEmotion emotionType;

    private Transform followTarget;
    private Vector3 followOffset;
    private bool isPlaying;
    
    public override Const.EEmotion Key => emotionType;
    
    public override void OnPop()
    {
        base.OnPop();

        isPlaying = false;
        followTarget = null;
    }
    
    public override void OnPush()
    {
        base.OnPush();
        isPlaying = false;
        followTarget = null;
    }
    
    public void EmotionPlay(Transform target, Vector3 offset = default)
    {
        followTarget = target;
        followOffset = offset;
        isPlaying = true;
        UpdateTransform();
    }
    
    private void UpdateTransform()
    {
        if (followTarget == null) return;
        transform.position = followTarget.position + followOffset;
    }

    private void Update()
    {
        if (!isPlaying || followTarget == null) return;
        UpdateTransform();
    }

    private void LateUpdate()
    {
        if (!isPlaying || Camera.main == null) return;

        Vector3 euler = transform.eulerAngles;
        euler.x = 0f;
        euler.z = 0f;
        euler.y = Camera.main.transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(euler);
    }
}