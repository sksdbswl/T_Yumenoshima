// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
//
// namespace REIW.LoneGarden
// {
//     public partial class HousingObject : IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
//     {
//         private bool isFocusing;
//         private bool isDragging;
//         private float holdTime;
//         private float _lastFreeRotationY;
//         private Vector3 offset;
//         private Coroutine holdCoroutine;
//
//         private readonly Stack<ObjectData> _undoStack = new();
//         private readonly Stack<ObjectData> _redoStack = new();
//
//         private const float FreeRotationThreshold = 10f;
//
//         private bool CanUndo => _undoStack.Count > 1;
//         private bool CanRedo => _redoStack.Count > 0;
//         private bool IsInterrupt { get; set; }
//
//         public void SetEditMode(bool isEditMode)
//         {
//             HousingLayers.SetLayerRecursive(gameObject.transform,
//                 isEditMode ? HousingLayers.CurrEditIndex : Cache.LayerIndexFromCategory(),
//                 HousingLayers.SnapIndex
//             );
//
//             PhysicsRaycaster raycaster = Camera.main!.GetComponent<PhysicsRaycaster>();
//             raycaster.eventMask = isEditMode
//                 ? HousingLayers.CurrEditMask
//                 : HousingLayers.AllItemsMask;
//
//             IsInterrupt = false;
//             bottomMesh.sharedMaterial = floorMaterial1;
//
//             if (isEditMode)
//             {
//                 foreach (Collider col in ObjColliders) col.isTrigger = true;
//                 bottomMesh.gameObject.SetActive(true);
//                 RecordState();
//                 SetRigidBody(true);
//
//                 _undoStack.Clear();
//                 _redoStack.Clear();
//                 RequestCameraFocus();
//             }
//             else
//             {
//                 FinalizePlacement();
//             }
//         }
//
//         private void FinalizePlacement()
//         {
//             foreach (MeshRenderer mr in meshRenderers) mr.enabled = true;
//             _undoStack.Clear();
//             _redoStack.Clear();
//             SnappedObject = null;
//             isSnapped = false;
//             bottomMesh.gameObject.SetActive(false);
//             SetRigidBody(false);
//         }
//
//         private IEnumerator HoldToAdjust()
//         {
//             holdTime = 0f;
//             while (holdTime < 0.3f)
//             {
//                 holdTime += Time.deltaTime;
//                 yield return null;
//             }
//
//             HousingEditor.Instance.BeginEdit(this);
//             holdCoroutine = null;
//         }
//
//         private void StartDrag(PointerEventData eventData)
//         {
//             isDragging = true;
//
//             Ray ray = Camera.main!.ScreenPointToRay(eventData.position);
//             Plane plane = HousingEditor.Instance.IsVerticalEditing
//                 ? new Plane(-Camera.main.transform.forward, transform.position)
//                 : new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
//
//             if (plane.Raycast(ray, out float enter))
//             {
//                 Vector3 hitPoint = ray.GetPoint(enter);
//                 offset = transform.position - hitPoint;
//             }
//         }
//
//         private bool TryGetPlaneIntersection(Ray ray, out float enter)
//         {
//             Plane plane = HousingEditor.Instance.IsVerticalEditing
//                 ? new Plane(-Camera.main!.transform.forward, transform.position)
//                 : new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
//
//             return plane.Raycast(ray, out enter);
//         }
//
//         private Vector3 ApplyOffsetAndClamp(Vector3 hitPoint)
//         {
//             Vector3 raw = hitPoint + offset;
//             raw.y = Mathf.Max(raw.y, 0f);
//             return raw;
//         }
//
//         private void ApplyGridAndVerticalConstraints(Vector3 position)
//         {
//             if (HousingEditor.Instance.IsVerticalEditing)
//             {
//                 float newY = HousingEditor.Instance.IsGridEditing ? Snap(position.y) : position.y;
//                 position = new Vector3(transform.position.x, newY, transform.position.z);
//             }
//             else
//             {
//                 float newX = HousingEditor.Instance.IsGridEditing ? Snap(position.x) : position.x;
//                 float newZ = HousingEditor.Instance.IsGridEditing ? Snap(position.z) : position.z;
//                 position = new Vector3(newX, transform.position.y, newZ);
//             }
//
//             HousingEditor.Instance.HousingHUD.HideRotateButtons();
//             transform.position = position;
//             return;
//
//             float Snap(float v) => Mathf.Round(v * 10f) / 10f;
//         }
//
//         public void Rotate(bool isLeft)
//         {
//             Vector3 rotation = transform.eulerAngles;
//             if (HousingEditor.Instance.IsGridEditing)
//             {
//                 rotation.y += isLeft ? 10f : -10f;
//                 rotation.y = Mathf.Round(rotation.y / 10f) * 10f;
//             }
//             else
//             {
//                 rotation.y += isLeft ? 1.75f : -1.75f;
//                 float diff = Mathf.DeltaAngle(_lastFreeRotationY, rotation.y);
//                 if (Mathf.Abs(diff) >= FreeRotationThreshold)
//                 {
//                     _lastFreeRotationY = rotation.y;
//                 }
//             }
//
//             transform.eulerAngles = rotation;
//         }
//
//         private void TryUndo()
//         {
//             if (!CanUndo) return;
//
//             ObjectData current = _undoStack.Pop();
//             _redoStack.Push(current);
//
//             ObjectData previous = _undoStack.Peek();
//             RestoreState(previous);
//         }
//
//         private void TryRedo()
//         {
//             if (!CanRedo) return;
//
//             ObjectData next = _redoStack.Pop();
//             _undoStack.Push(next);
//
//             RestoreState(next);
//         }
//
//         public void RecordState()
//         {
//             ObjectData snapshot = Cache.Clone();
//             snapshot.UpdateTransform(transform.position, transform.eulerAngles.y);
//             _undoStack.Push(snapshot);
//             _redoStack.Clear();
//
//             Cache = snapshot;
//         }
//
//         private void RestoreState(ObjectData data)
//         {
//             transform.position = data.Position;
//             transform.eulerAngles = new Vector3(0f, data.Direction.y, 0f);
//             Cache = data;
//         }
//
//         private void RequestCameraFocus()
//         {
//             if (meshRenderers.Length == 0) return;
//             isFocusing = true;
//             Bounds b = meshRenderers[0].bounds;
//             for (int i = 1; i < meshRenderers.Length; i++) b.Encapsulate(meshRenderers[i].bounds);
//             HousingEditor.Instance.ObserverCam.FocusOnBounds(b, () => { isFocusing = false; });
//         }
//
//         private void OnTriggerStay(Collider other)
//         {
//             if (!HousingEditor.Instance) return;
//             if (!HousingEditor.Instance.IsEditMode || HousingEditor.CurrentObject != this) return;
//             if (other.gameObject.layer == HousingLayers.GroundIndex) return;
//             if (isSnapped) return;
//
//             IsInterrupt = true;
//             bottomMesh.sharedMaterial = floorMaterial2;
//         }
//
//         private void OnTriggerExit(Collider other)
//         {
//             if (!HousingEditor.Instance) return;
//             if (!HousingEditor.Instance.IsEditMode || HousingEditor.CurrentObject != this) return;
//             if (other.gameObject.layer == HousingLayers.GroundIndex) return;
//
//             IsInterrupt = false;
//             bottomMesh.sharedMaterial = floorMaterial1;
//         }
//
//         public void OnPointerDown(PointerEventData eventData)
//         {
//             if (eventData.button != PointerEventData.InputButton.Left) return;
//             if (isFocusing || isDragging) return;
//
//             if (HousingEditor.CurrentObject != this)
//             {
//                 holdCoroutine = StartCoroutine(HoldToAdjust());
//                 return;
//             }
//
//             StartDrag(eventData);
//         }
//
//         public void OnPointerUp(PointerEventData eventData)
//         {
//             if (eventData.button != PointerEventData.InputButton.Left) return;
//
//             // 홀드 코루틴이 돌고 있으면 취소
//             if (holdCoroutine != null)
//             {
//                 StopCoroutine(holdCoroutine);
//                 holdCoroutine = null;
//             }
//
//             // 드래그 끝
//             RecordState();
//             HousingEditor.Instance.HousingHUD.ShowRotateButtons();
//             isDragging = false;
//             holdTime = 0f;
//         }
//
//         public void OnPointerEnter(PointerEventData eventData)
//         {
//             if (!HousingEditor.Instance.IsEditMode && SnappedObject == null)
//             {
//                 bottomMesh.gameObject.SetActive(true);
//             }
//         }
//
//         public void OnPointerExit(PointerEventData eventData)
//         {
//             if (!HousingEditor.Instance.IsEditMode && SnappedObject == null)
//             {
//                 bottomMesh.gameObject.SetActive(false);
//             }
//         }
//
//         public void ShowInitialCameraFacingWall()
//         {
//             if (Cache.Type != Type.BuildingWall || wallObjects == null || wallObjects.Length < 1)
//                 return;
//
//             Camera cam = Camera.main;
//             if (cam == null) return;
//
//             float minDist = float.MaxValue;
//             int bestIdx = 0;
//
//             for (int i = 0; i < wallObjects.Length; i++)
//             {
//                 if (wallObjects[i] == null) continue;
//                 Vector3 worldPos = wallObjects[i].transform.position;
//                 float d = (cam.transform.position - worldPos).sqrMagnitude;
//                 if (d < minDist)
//                 {
//                     minDist = d;
//                     bestIdx = i;
//                 }
//             }
//
//             Cache.SetWallIdx(bestIdx);
//             for (int i = 0; i < wallObjects.Length; i++)
//             {
//                 wallObjects[i].SetActive(i == bestIdx);
//             }
//         }
//     }
// }