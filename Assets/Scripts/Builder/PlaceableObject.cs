using UnityEngine;


[System.Serializable]
public class PlacedObjectData
{
    public int id;              
    public PlaceableRole role;
    public float gridX;
    public float gridZ;
    public float rotationY;
}

public class PlaceableObject : MonoBehaviour
{
    public int HomeId { get; private set; }
    public PlaceableRole Role { get; private set; }
    public PlaceableItem SourceItem { get; private set; }
    
    public void Initialize(PlaceableRole role, PlaceableItem item, Vector3 position, bool force = false)
    {
        Role = role;
        SourceItem = item;

        PlacedObjectData data = new PlacedObjectData();

        data.id = PlayerProgress.GenerateBuilderId(); 
        data.role = role;
        data.gridX = position.x;
        data.gridZ = position.z;
        data.rotationY = transform.eulerAngles.y;

        HomeId = data.id;

        int layer = BuilderLayers.LayerFromRole(role);
        BuilderLayers.SetLayerRecursive(transform, layer);

        PlaceableRegistry.Singleton.Register(this, HomeId);

        if (force)
            PlacementSaveManager.Singleton.RegisterPlacedObject(data);
    }
    
    // public void Initialize(PlaceableRole role, PlaceableItem item, Vector3 position, bool force = false)
    // {
    //     Role = role;
    //     SourceItem = item;
    //
    //     PlacedObjectData data = new PlacedObjectData();
    //     
    //     data.id = PlayerProgress.GenerateBuilderId(); 
    //     data.role = role;
    //     data.gridX = position.x;
    //     data.gridZ = position.z;
    //     data.rotationY = transform.eulerAngles.y;
    //     
    //     int layer = BuilderLayers.LayerFromRole(role);
    //     BuilderLayers.SetLayerRecursive(transform, layer);
    //
    //     if (force)
    //         PlacementSaveManager.Singleton.RegisterPlacedObject(data);
    // }
}
