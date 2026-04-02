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

    [HideInInspector] public bool _isOpen;

    public void SetStore(bool isOpen)
    {
        _isOpen = isOpen;
        builderStoreUI.SetActive(isOpen);

        // Placement 제어
        PlacementManager.Singleton.placementSystem.enabled = isOpen;
    }
    
    // public void OpenStore()
    // {
    //     builderStoreUI.SetActive(true);
    // }
    //
    // public void CloseStore()
    // {
    //     builderStoreUI.SetActive(false);
    //     PlacementManager.Singleton.placementSystem.enabled = false;
    // }
    
    private void LoadHousingShop()
    {
        var placement = PlacementManager.Singleton.placementSystem.CatalogTable;
        
        foreach (var item in placement.Items)
        {
            var place = Instantiate(builderItemPrefab, builderItemContent.transform);
            var builderItem = place.GetComponent<BuilderItem>();

            builderItem.Initialize(item);
            
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
