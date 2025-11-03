using System.Collections.Generic;
using UnityEngine;

namespace REIW.LoneGarden
{
    public partial class HousingObject
    {
        private const int SnapCount = 6;
        public Collider[] SnapPoints => snapPoints;
        public static HousingObject SnappedObject { get; private set; }
        private readonly Collider[] snapPoints = new Collider[SnapCount];
        private readonly Dictionary<HousingObject, HashSet<int>> activeCollisions = new();

        private bool isSnapped;
        private Transform snapParents;

        public void SnapTo(int mySide, int otherSide, HousingObject otherObject)
        {
            if (!activeCollisions.TryGetValue(otherObject, out HashSet<int> sides))
            {
                sides = new HashSet<int>();
                activeCollisions[otherObject] = sides;
            }

            sides.Add(mySide);
            if (sides.Count == 1)
            {
                ExecuteSnap(mySide, otherSide, otherObject);
            }
        }

        public void SetupAllSnaps()
        {
            if (snapParents != null)
            {
                return;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name != "Snaps") continue;
                snapParents = transform.GetChild(i);
                break;
            }

            if (snapParents == null)
            {
                Debug.LogError($"HousingObject {Cache.Name}에 'Snaps' 하위 개체가 없습니다.");
                return;
            }

            for (int i = 0; i < SnapCount && i < snapParents.childCount; i++)
            {
                Transform child = snapParents.GetChild(i);
                child.gameObject.layer = HousingLayers.SnapIndex;
                Collider cap = child.GetComponent<Collider>()
                               ?? child.gameObject.AddComponent<Collider>();
                cap.isTrigger = true;
                snapPoints[i] = cap;

                SnapDetector det = child.gameObject.AddComponent<SnapDetector>();
                det.Initialize(this, i);

                child.localRotation = i switch
                {
                    0 => Quaternion.Euler(0f, -90f, 0f),
                    1 => Quaternion.Euler(0f, 90f, 0f),
                    2 => Quaternion.Euler(90f, 0f, 0f),
                    3 => Quaternion.Euler(-90f, 0f, 0f),
                    4 => Quaternion.Euler(0f, 180f, 0f),
                    5 => Quaternion.identity,
                    _ => child.localRotation
                };
            }

            UpdateSnapPointPositions();
        }

        private void UpdateSnapPointPositions()
        {
            Bounds bounds = ObjColliders[0].bounds;
            Vector3 localMin = transform.InverseTransformPoint(bounds.min);
            Vector3 localMax = transform.InverseTransformPoint(bounds.max);

            float midX = (localMin.x + localMax.x) * 0.5f;
            float midY = (localMin.y + localMax.y) * 0.5f;
            float midZ = (localMin.z + localMax.z) * 0.5f;

            snapPoints[0].transform.localPosition = new Vector3(localMin.x, midY, midZ);
            snapPoints[1].transform.localPosition = new Vector3(localMax.x, midY, midZ);
            snapPoints[2].transform.localPosition = new Vector3(midX, localMin.y, midZ);
            snapPoints[3].transform.localPosition = new Vector3(midX, localMax.y, midZ);
            snapPoints[4].transform.localPosition = new Vector3(midX, midY, localMin.z);
            snapPoints[5].transform.localPosition = new Vector3(midX, midY, localMax.z);
        }

        internal void OnSnapEnter(int mySide, int otherSide, HousingObject otherObject)
        {
            Bounds bounds = ObjColliders[0].bounds;
            Bounds myB = new Bounds(transform.TransformPoint(bounds.center), bounds.size);
            Bounds otherB = new Bounds(
                otherObject.transform.TransformPoint(otherObject.ObjColliders[0].bounds.center),
                otherObject.ObjColliders[0].bounds.size);

            const float eps = 0.001f;
            if (Vector3.Distance(myB.center, otherB.center) < eps &&
                Vector3.Distance(myB.size, otherB.size) < eps)
                return;

            if (!HousingEditor.Instance.HousingHUD.IsSnapEditing
                || SnappedObject != null
                || HousingEditor.CurrentObject != this
                || Cache.Name != otherObject.Cache.Name)
                return;

            if (!activeCollisions.TryGetValue(otherObject, out HashSet<int> sides))
            {
                sides = new HashSet<int>();
                activeCollisions[otherObject] = sides;
            }

            sides.Add(mySide);
            if (sides.Count == 1)
            {
                ExecuteSnap(mySide, otherSide, otherObject);
            }
        }

        internal void OnSnapExit(int mySide, int _, HousingObject otherObject)
        {
            if (activeCollisions.TryGetValue(otherObject, out HashSet<int> sides))
            {
                sides.Remove(mySide);
                if (sides.Count == 0)
                {
                    activeCollisions.Remove(otherObject);
                }
            }

            if (isSnapped && SnappedObject == this)
            {
                ResetSnap();
            }
        }

        private void ExecuteSnap(int mySide, int otherSide, HousingObject otherObject)
        {
            isSnapped = true;
            isDragging = false;
            SnappedObject = this;

            Transform myPoint = snapPoints[mySide].transform;
            Transform otherPoint = otherObject.snapPoints[otherSide].transform;
            Vector3 targetPos = transform.position + (otherPoint.position - myPoint.position);

            targetPos.y = Mathf.Max(targetPos.y, 0f);
            transform.position = targetPos;

            IsInterrupt = false;
            bottomMesh.sharedMaterial = floorMaterial1;
        }

        private void ResetSnap()
        {
            isSnapped = false;
            SnappedObject = null;
        }
    }
}