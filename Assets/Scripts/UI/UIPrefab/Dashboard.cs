using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dashboard : SingletonBase<Dashboard>
{ 
    [SerializeField] private Player player;
  
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI job;
    [SerializeField] private TextMeshProUGUI quest;
    
    [Header("Func")]
    [SerializeField] private Button Housing;
    [SerializeField] private GameObject HousingUI;
    [SerializeField] private GameObject HousingItemPrefab;
    [SerializeField] private RectTransform HousingParent;
    
    bool isHousingUIActive = false;
    
    private void Awake()
    {
        Housing.onClick.AddListener(OnActiveHousing);
    }

    private void Start()
    {
        var accepted = PlayerDialogueProgress.Singleton.GetAcceptedQuests();
    
        for (int i = 0; i < accepted.Count; i++)
        {
            var entry = accepted[i];
            var data = GameManager.Singleton.GetQuestData(entry.questId);
    
            Debug.Log($"퀘스트 이름: {data.questName}");
            
            quest.text = data.questName;
        }

        Init();
        LoadHousingShop();
    }

    public void Init()
    {
        job.text = player._playerStatus.CurrentJobType.ToString();
    }

    private void LoadHousingShop()
    {
        var placement = PlacementManager.Singleton.placementSystem.CatalogTable;
        
        // TODO :: 이미 배치된 빌딩의 경우 비활성화 처리 필요
        foreach (var place in placement.Items)
        {
            var h_prefab = Instantiate(HousingItemPrefab, HousingParent.transform);
            var image = h_prefab.GetComponentInChildren<Image>();
            var text = h_prefab.GetComponentInChildren<TextMeshProUGUI>();
            image.sprite = place.Icon;
            text.text = place.DisplayName;
            h_prefab.SetActive(true);

            h_prefab.GetComponent<Button>().onClick.AddListener(() =>
            {
                BuilderPlacement(place.BuilderId);
            });
        }
    }

    private void BuilderPlacement(int buildingId = 0)
    {
        Debug.Log($"BuilderPlacement: {buildingId}");
        
        PlacementManager.Singleton.placementSystem.SelectCatalogIndex(buildingId);
        PlacementManager.Singleton.OnPlacementEdit();
       
        OnActiveHousing();
    }

    private void OnActiveHousing()
    {
        isHousingUIActive = !isHousingUIActive;
        HousingUI.SetActive(isHousingUIActive);
        
        if (isHousingUIActive)
        {
            PlacementManager.Singleton.placementSystem.enabled = false;
        }
    }
}