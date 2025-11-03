using UnityEngine;

namespace REIW
{
    public static class LODGroupUtility
    {
        public static int GetCurrentLODIndex(LODGroup lodGroup, Camera camera)
        {
            if (lodGroup == null || camera == null)
                return -1;

            var lods = lodGroup.GetLODs();
            float relativeHeight = GetRelativeScreenHeight(lodGroup, camera);

            for (int i = 0; i < lods.Length; i++)
            {
                if (relativeHeight >= lods[i].screenRelativeTransitionHeight)
                    return i;
            }

            return lods.Length; // this mean : Culled
        }

        private static float GetRelativeScreenHeight(LODGroup lodGroup, Camera camera)
        {
            float distance = Vector3.Distance(camera.transform.position, lodGroup.transform.position);
            float objectSize = lodGroup.size;
            float screenHeight = 2.0f * Mathf.Tan(0.5f * camera.fieldOfView * Mathf.Deg2Rad) * distance;
            return objectSize / screenHeight;
        }
    }
}
