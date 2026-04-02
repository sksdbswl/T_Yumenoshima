using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuilderUI : UIBase
{
    [Header("Build Store Settings")]
    [SerializeField] private GameObject builderStoreUI;
    [SerializeField] private RectTransform builderItemContent;
    [SerializeField] private GameObject builderItemPrefab;

    private void Awake()
    {
        LoadHousingShop();
    }
    
    public void Load()
    {
        PlacementManager.Singleton.Load();
    }
    
    public void Save()
    {
        PlacementManager.Singleton.Save();
    }
    
    public void Clear()
    {
        OnPlacementReset();
    }
    
    public void OnPlacementReset()
    {
        PlacementManager.Singleton.ClearAll();

        var objects = FindObjectsOfType<PlaceableInteraction>();
        foreach (var obj in objects)
            Destroy(obj.gameObject);

        PlacementSystem placement = FindObjectOfType<PlacementSystem>();
        placement.RebuildFromSave(PlacementManager.Singleton.PlacedObjects);
    }

    public void OpenStore()
    {
        builderStoreUI.SetActive(true);
    }
    
    public void CloseStore()
    {
        builderStoreUI.SetActive(false);
        PlacementManager.Singleton.placementSystem.enabled = false;
    }
    
    private void LoadHousingShop()
    {
        var placement = PlacementManager.Singleton.placementSystem.CatalogTable;
        
        foreach (var item in placement.Items)
        {
            var place = Instantiate(builderItemPrefab, builderItemContent.transform);
            var icon = place.GetComponentInChildren<Image>();
            var name = place.GetComponentInChildren<TextMeshProUGUI>();

            icon.sprite = item.Icon;
            name.text = item.DisplayName;
            place.gameObject.SetActive(true);
            
            place.GetComponent<Button>().onClick.AddListener(() =>
            {
                BuilderPlacement(item.BuilderId);
            });
        }
    }
    
    private void BuilderPlacement(int buildingId = 0)
    {
        Debug.Log($"BuilderPlacement: {buildingId}");
        
        PlacementManager.Singleton.placementSystem.SelectCatalogBuilderID(buildingId);
        PlacementManager.Singleton.placementSystem.enabled = true;
    }
}
