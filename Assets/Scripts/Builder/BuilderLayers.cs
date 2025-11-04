using UnityEngine;

namespace REIW.LoneGarden
{
    public static class BuilderLayers
    {
        // 실제 프로젝트의 Layer 이름과 맞추세요.
        public static readonly int LAYER_SNAP = LayerMask.NameToLayer("SnapPoint");
        public static readonly int LAYER_GROUND = LayerMask.NameToLayer("Ground");
        public static readonly int LAYER_WALL   = LayerMask.NameToLayer("Wall");
        public static readonly int LAYER_PROP   = LayerMask.NameToLayer("BGWall"); 

        public static readonly int MASK_SNAP    = 1 << LAYER_SNAP;
        public static readonly int MASK_ALLITEM = LayerMask.GetMask("Ground", "Wall", "BGWall");

        public static void SetLayerRecursive(Transform tr, int setLayer, int ignoreLayer = -1)
        {
            if (ignoreLayer == -1 || tr.gameObject.layer != ignoreLayer)
                tr.gameObject.layer = setLayer;

            for (int i = 0; i < tr.childCount; i++)
                SetLayerRecursive(tr.GetChild(i), setLayer, ignoreLayer);
        }

        public static int LayerFromRole(PlaceableRole role)
        {
            switch (role)
            {
                case PlaceableRole.Road:     return LAYER_GROUND;
                case PlaceableRole.Building:  return LAYER_WALL;
                default:                      return LAYER_PROP;
            }
        }
    }
}