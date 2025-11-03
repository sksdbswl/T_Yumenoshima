using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace REIW.LoneGarden
{
    public partial class HousingObject : MonoBehaviour
    {
        public ObjectData Cache { get; private set; }
        [SerializeField] private Material floorMaterial1;
        [SerializeField] private Material floorMaterial2;
        [SerializeField] private GameObject[] wallObjects; // Wall일 경우에만 할당!!

        private void Awake()
        {
            meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
            ObjColliders = GetComponentsInChildren<Collider>(true);
        }

        private void Start()
        {
            SetupAllSnaps();
        }

        public void SetCache(string itemName, Type type, ushort category, ushort kind, uint serial, ulong dbId, float wallIdx)
        {
            Cache = new ObjectData(category, kind, serial, dbId, Guid.NewGuid().ToString("N"),
                itemName, type, transform.position, transform.rotation.eulerAngles.y, wallIdx);

            if (type == Type.BuildingWall && wallObjects != null && wallObjects.Length >= 4)
            {
                wallObjects[(int)wallIdx].SetActive(true);
            }

            HousingLayers.SetLayerRecursive(gameObject.transform,
                Cache.LayerIndexFromCategory(),
                HousingLayers.SnapIndex
            );

            const float epsilon = 0.001f;
            transform.SetParent(HousingManager.Instance.transform, worldPositionStays: false);

            Vector3 pos = transform.localPosition;
            pos.x += Random.Range(-epsilon, epsilon);
            pos.y += Random.Range(-epsilon, epsilon);
            pos.z += Random.Range(-epsilon, epsilon);
            transform.localPosition = pos;
            transform.localScale = Vector3.one * (1f + Random.Range(-epsilon, epsilon));
            transform.SetParent(HousingManager.Instance.transform);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) && !isFocusing && HousingEditor.CurrentObject == this) RequestCameraFocus();
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z)) TryUndo();
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y)) TryRedo();

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Keypad0)) JsonHelper.ClearJsonAsync().Forget();
#endif
        }

        // 드래그 중 위치 업데이트
        private void LateUpdate()
        {
            if (!isDragging || isFocusing || HousingEditor.CurrentObject != this) return;
            Ray ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
            if (!TryGetPlaneIntersection(ray, out float enter)) return;

            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 nextPosition = ApplyOffsetAndClamp(hitPoint);
            ApplyGridAndVerticalConstraints(nextPosition);
        }
    }
}