using UnityEngine;

namespace REIW.LoneGarden
{
    public partial class HousingObject
    {
        public Collider[] ObjColliders { get; private set; }
        private MeshRenderer[] meshRenderers;
        private MeshRenderer bottomMesh;

        public void CreateGroundPlane()
        {
            const float padding = 0.1f;
            Collider targetCollider = ObjColliders[0];

            // 맞춤용 레이어를 타겟으로 잡으면 표시자가 이상하게 나온다.
            foreach (Collider col in ObjColliders)
            {
                if (col.gameObject.activeSelf &&
                    col.gameObject.layer != Layer.LAYER_SNAP_POINT &&
                    col.gameObject.layer != Layer.LAYER_IGNORE_RAYCAST)
                {
                    targetCollider = col;
                }
            }

            Bounds bounds = targetCollider.bounds;
            float objectHeight = bounds.size.y;
            float thickness = objectHeight * 0.1f;

            float width = bounds.size.x + padding;
            float height = thickness;
            float depth = bounds.size.z + padding;

            GameObject groundPlane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groundPlane.name = "GroundPlane";
            groundPlane.transform.SetParent(transform, false);
            groundPlane.transform.localScale = new Vector3(width, height, depth);

            float yPos = bounds.min.y + thickness * 0.5f;
            Vector3 worldPos = new Vector3(bounds.center.x, yPos, bounds.center.z);
            groundPlane.transform.localPosition = transform.InverseTransformPoint(worldPos);

            MeshRenderer rend = groundPlane.GetComponent<MeshRenderer>();
            rend.sharedMaterial = floorMaterial1;

            Destroy(groundPlane.GetComponent<Collider>());
            groundPlane.SetActive(false);

            bottomMesh = rend;
        }

        private void SetRigidBody(bool attach)
        {
            if (attach)
            {
                Rigidbody rb = gameObject.TryGetComponent(out Rigidbody existingRb) 
                    ? existingRb 
                    : gameObject.AddComponent<Rigidbody>();

                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null) Destroy(rb);
            }
        }
    }
}