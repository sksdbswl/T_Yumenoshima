// using Unity.Collections;
//
// namespace REIW
// {
//     [System.Serializable]
//     public abstract class ContentIdTable
//     {
//         [ReadOnly] public ulong ContentIDFull;
//         [ReadOnly] public ushort ContentIDCategory;
//         [ReadOnly] public ushort ContentIDKind;
//         [ReadOnly] public uint ContentIDSerial;
//         [ReadOnly] public string ContentIDString;
//
//         public ContentIDValue ContentId => new()
//         {
//             m_Category = ContentIDCategory,
//             m_KIND = ContentIDKind,
//             m_Serial = ContentIDSerial
//         };
//
//         public void SetContentIDString(string value)
//         {
//             LogUtil.Log("SetContentIDString: " + value);
//             
//             if (string.IsNullOrEmpty(value)) return;
//             ContentIDString = value;
//             ContentIDFull = ReNetworkUtility.GetContentIDFromValue(ContentIDString);
//             ContentIDCategory = (ushort)ReNetworkUtility.GetEnumCategory(ContentIDFull);
//             ContentIDKind = (ushort)ReNetworkUtility.GetKind(ContentIDFull);
//             ContentIDSerial = ReNetworkUtility.GetSerial(ContentIDFull);
//         }
//
//         public virtual void SetData() {}
//     }
// }