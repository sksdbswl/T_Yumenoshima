#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using TMPro;

namespace REIW
{
    public class DynamicFontAssetAutoClear : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths)
            {
                if (AssetDatabase.LoadAssetAtPath(path, typeof(TMP_FontAsset)) is TMP_FontAsset fontAsset)
                {
                    if (fontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic)
                    {
                        //Debug.Log("Clearing font asset data at " + path);
                        fontAsset.ClearFontAssetData(setAtlasSizeToZero: true);
                    }
                }
            }

            return paths;
        }
        
    }
}

#endif
