using UnityEngine;

namespace REIW.LoneGarden
{
    [CreateAssetMenu(menuName = "LoneGarden/Placeable Catalog", fileName = "PlaceableTable")]
    public class PlaceableCatalogTable : ScriptableObject
    {
        public PlaceableItem[] Items; 
    }
}