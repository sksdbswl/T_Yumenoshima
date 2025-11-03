// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.U2D;
//
// namespace REIW
// {
//     public class SpriteAtlasLazyLoader : SingletonBase<SpriteAtlasLazyLoader>
//     {
//         private class AtlasInfo
//         {
//             public SpriteAtlas atlas;
//             public AsyncOperationHandle<SpriteAtlas> handle;
//             public float lastUsedTime;
//         }
//
//         private readonly Dictionary<string, AtlasInfo> loadedAtlases = new();
//         private readonly HashSet<string> loadingTags = new();
//         private bool isInitialized = false;
//         private const float UnloadThreshold = 60f; // 초 단위
//
//         public void Initialize()
//         {
//             if (isInitialized)
//                 return;
//
//             SpriteAtlasManager.atlasRequested += OnAtlasRequested;
//             isInitialized = true;
//         }
//
//         private void OnAtlasRequested(string tag, System.Action<SpriteAtlas> callback)
//         {
//             LogUtil.Log($"OnAtlasRequested Tag : {tag}");
//             if (loadingTags.Contains(tag))
//                 return;
//
//             loadingTags.Add(tag);
//             AssetManager.Singleton.LoadSpriteAtlasAsset(tag, OnCompleted);
//
//             void OnCompleted(AsyncOperationHandle<SpriteAtlas> handle)
//             {
//                 loadingTags.Remove(tag);
//
//                 if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
//                 {
//                     var atlas = handle.Result;
//
//                     loadedAtlases[tag] = new AtlasInfo()
//                     {
//                         atlas = atlas,
//                         handle = handle,
//                         lastUsedTime = Time.unscaledTime,
//                     };
//
//                     callback?.Invoke(handle.Result);
//                 }
//                 else
//                 {
//                     LogUtil.LogError($"Failed to load Sprite Atlas : {tag}");
//                 }
//             };
//         }
//
//         private void Update()
//         {
//             // var now = Time.unscaledTime;
//             // var toUnload = new List<string>();
//             //
//             // foreach (var kvp in loadedAtlases)
//             // {
//             //     var tag = kvp.Key;
//             //     var info = kvp.Value;
//             //
//             //     if (now - info.lastUsedTime >= UnloadThreshold)
//             //     {
//             //         Addressables.Release(info.handle);
//             //         toUnload.Add(tag);
//             //
//             //         LogUtil.Log($"♻️ Unloading unused atlas: {tag}");
//             //     }
//             // }
//             //
//             // foreach (var tag in toUnload)
//             //     loadedAtlases.Remove(tag);
//         }
//
//         public void UnloadAllNow()
//         {
//             foreach (var info in loadedAtlases.Values)
//                 AssetManager.Singleton.ReleaseAsset(info.atlas);
//
//             loadedAtlases.Clear();
//         }
//     }
// }