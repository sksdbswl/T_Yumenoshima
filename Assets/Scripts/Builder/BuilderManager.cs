namespace REIW.LoneGarden
{
    public class BuilderManager
    {
        public static BuilderManager Instance { get; private set; }
        
        private void Start()
        {
            // LoadBuilderEditor();
        }
        
        //todo::LoadBuilderEditor => 배치된 내용 불러오기
        // public void LoadBuilderEditor()
        // {
        //     isEditMode = true;
        //     LoadingUI.Loading(loadingTask: async (progress) =>
        //     {
        //         AllHousingSetup(true);
        //         editorButton.gameObject.SetActive(false);
        //         progress(0f);
        //         await LoadHousingEditorAsync();
        //         progress(1f);
        //     }).Forget();
        // }
        
        //todo::UnloadBuilderEditor => 배치된 내용 해제
        // public void UnloadBuilderEditor()
        // {
        //     isEditMode = false;
        //     LoadingUI.Loading(loadingTask: async (progress) =>
        //     {
        //         progress(0f);
        //         await UnloadHousingEditorAsync();
        //         progress(1f);
        //         editorButton.gameObject.SetActive(true);
        //         AllHousingSetup(false);
        //     }).Forget();
        // }
        
        
        //todo:: SaveToJson => 배치 내용 저장
        // public void SaveToJson()
        // {
        //     List<ObjectData> cacheList = new List<ObjectData>();
        //     foreach (HousingObject housingObject in HousingObjects.Values)
        //     {
        //         ObjectData cache = housingObject.Cache;
        //         cacheList.Add(cache);
        //         ReNetworkClient.Singleton.GetGameServerClient().REQ_FIELD_SUBJECT_RELOCATE(cache.Category, cache.KIND, cache.Serial,
        //             cache.DatabaseId, cache.Position, new Vector3(0, cache.Direction.y, 0)
        //         );
        //     }
        //
        //     JsonHelper.SaveToJsonAsync(cacheList).Forget();
        // }
    }
}