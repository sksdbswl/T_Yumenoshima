using UnityEngine;

public static class BuilderLayers
{
    public static readonly int LAYER_SNAP     = LayerMask.NameToLayer("SnapPoint");
    public static readonly int LAYER_GROUND   = LayerMask.NameToLayer("Ground");
    public static readonly int LAYER_TILE     = LayerMask.NameToLayer("Tile");
    public static readonly int LAYER_ROAD     = LayerMask.NameToLayer("Road");
    public static readonly int LAYER_DECO     = LayerMask.NameToLayer("Deco");
    public static readonly int LAYER_BUILDING = LayerMask.NameToLayer("Building");

    // 배치 가능한 표면들만 (Ground, Tile)
    public static readonly int MASK_SURFACE_PLACEMENT =
        (1 << LAYER_GROUND) | (1 << LAYER_TILE);

    // 스냅 전용
    public static readonly int MASK_SNAP = 1 << LAYER_SNAP;

    // Raycast시 스냅 제외하고 표면만 맞추기
    public static readonly int MASK_RAYCAST_PLACEMENT = MASK_SURFACE_PLACEMENT;

    public static void SetLayerRecursive(Transform transform, int layer)
    {
        transform.gameObject.layer = layer;
    }
    
    // 각 Role에 따른 Layer 지정
    public static int LayerFromRole(PlaceableRole role)
    {
        switch (role)
        {
            case PlaceableRole.Road:     return LAYER_ROAD;
            case PlaceableRole.Tile:     return LAYER_TILE;
            case PlaceableRole.Deco:     return LAYER_DECO;
            case PlaceableRole.Building: return LAYER_BUILDING;
            default:                     return LAYER_GROUND;
        }
    }
}