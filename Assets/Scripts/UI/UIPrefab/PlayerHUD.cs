using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : UIBase
{
    [SerializeField] private Image iconImage;

    [Header("Effect Icons")]
    [SerializeField] private Sprite getIcon;
    [SerializeField] private Sprite catchIcon;
    [SerializeField] private Sprite changeIcon;
    [SerializeField] private Sprite gachaIcon;

    [Header("Follow Setting")]
    [SerializeField] private Vector3 headOffset = new Vector3(0f, 2.0f, 0f);

    private Coroutine hideRoutine;
    private Coroutine followRoutine;

    public void ShowEffect(Const.EEffect effect, Player player)
    {
        if (effect == Const.EEffect.None || player == null)
            return;

        Sprite icon = null;

        switch (effect)
        {
            case Const.EEffect.Get:
                icon = getIcon;
                break;
            case Const.EEffect.Catch:
                icon = catchIcon;
                break;
            case Const.EEffect.Change:
                icon = changeIcon;
                break;
            case Const.EEffect.Gacha:
                icon = gachaIcon;
                break;
        }

        if (icon == null)
        {
            Debug.LogWarning($"No icon for effect: {effect}");
            return;
        }

        Show(icon, player, 1.5f);
    }

    private void Show(Sprite icon, Player player, float duration = 1.5f)
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        if (followRoutine != null)
            StopCoroutine(followRoutine);

        iconImage.enabled = true;
        iconImage.sprite = icon;
        gameObject.SetActive(true);

        UpdatePosition(player);
        followRoutine = StartCoroutine(FollowPlayer(player));

        hideRoutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator FollowPlayer(Player player)
    {
        while (player != null && gameObject.activeSelf)
        {
            UpdatePosition(player);
            yield return null;
        }
    }

    private void UpdatePosition(Player player)
    {
        Vector3 worldPos = player.transform.position + headOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        iconImage.transform.position = screenPos;
    }

    private IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
            followRoutine = null;
        }

        iconImage.enabled = false;
        gameObject.SetActive(false);
        hideRoutine = null;
    }
}