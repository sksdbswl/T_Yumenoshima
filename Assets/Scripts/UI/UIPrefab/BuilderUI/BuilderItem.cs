using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuilderItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI text;

    public void Initialize( PlaceableItem item)
    {
        icon.sprite = item.Icon;
        text.text = item.DisplayName;
        this.gameObject.SetActive(true);
    }
}
