#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public abstract class JsonImporterSOEditor<TAsset> : Editor where TAsset : ScriptableObject
{
    /// <summary>
    /// SO 스크립트의 List 타입 요소 필드 이름
    /// </summary>
    protected abstract string ListName { get; }

    /// <summary>
    /// ContentID로 쓸 요소의 이름 (Key가 될 string 값), 없으면 공백
    /// </summary>
    protected virtual string ContentIdName => ""; // Optional

    /// <summary>
    /// JSON파일 내부의 배열 이름, 예시: "Items":[`의 Items 
    /// </summary>
    protected virtual string JsonArrayKey => "Items";

    /// <summary>
    /// 해당 Json 파일의 경로 (스크립트 자체에서 하드코딩해두면 편할 것...)
    /// </summary>
    protected virtual string JsonPath => "Assets/01_REIW/Anothers/Game Data Convert/RawData/";

    /// <summary>
    /// (옵션) JSON → DTO → 요소타입 으로 매핑하고 싶을 때 DTO 요소 타입을 지정.
    /// 지정하지 않으면 요소타입(TElement)로 직접 역직렬화 시도.
    /// </summary>
    protected virtual Type ImportDtoElementType => null;

    private SerializedProperty _listProp;
    private string _rootArrayKey;
    private string _jsonPath;
    private string _contentIDName;
    private string _listName;

    protected virtual void OnEnable()
    {
        _listProp = serializedObject.FindProperty(ListName);
        _rootArrayKey = JsonArrayKey;
        _jsonPath = JsonPath;
        _contentIDName = ContentIdName;
        _listName = ListName;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _contentIDName = EditorGUILayout.TextField(new GUIContent("ContentID (Optional)"), _contentIDName);
        _listName = EditorGUILayout.TextField(new GUIContent("SO 클래스의 List 이름"), _listName);
        _rootArrayKey = EditorGUILayout.TextField(new GUIContent("파싱할 Json의 키 값"), _rootArrayKey);
        _jsonPath = EditorGUILayout.TextField(new GUIContent("불러올 Json 파일 경로"), _jsonPath);
        EditorGUILayout.Space(8);

        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_jsonPath)))
            {
                if (GUILayout.Button("UPDATE", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    ApplyJsonFromPathSafe(_jsonPath);
                }
            }

            using (new EditorGUI.DisabledScope(_listProp == null))
            {
                if (GUILayout.Button("DISCARD", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    foreach (var t in targets)
                    {
                        if (t is TAsset so)
                        {
                            var (field, elemType) = ResolveListFieldAndElementType(so.GetType(), ListName);
                            if (field == null || elemType == null)
                            {
                                Debug.LogError($"'{ListName}' 타입 해석 실패");
                                continue;
                            }

                            var empty = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType));
                            Undo.RecordObject(so, "Clear Items");
                            SetListOnAsset(so, empty);
                            EditorUtility.SetDirty(so);
                        }
                    }

                    AssetDatabase.SaveAssets();
                }
            }

            if (GUILayout.Button("RESET", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                _rootArrayKey = JsonArrayKey;
                _jsonPath = JsonPath;
                _contentIDName = ContentIdName;
                _listName = ListName;
                GUI.FocusControl(null);
            }

            GUILayout.FlexibleSpace();
        }

        // 리스트 바인딩 미스 방지
        if (_listProp == null || !_listProp.isArray)
        {
            EditorGUILayout.HelpBox($"필드 '{ListName}' 를 찾을 수 없거나 배열/리스트가 아닙니다.", MessageType.Error);
        }
        else
        {
            EditorGUILayout.PropertyField(_listProp, includeChildren: true);
        }

        EditorGUILayout.Space(8);

        serializedObject.ApplyModifiedProperties();
    }

    private void TryApplyJson(string json)
    {
        foreach (var t in targets)
        {
            if (t is TAsset so)
            {
                try
                {
                    ApplyJsonToAsset(so, json, _rootArrayKey);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"JSON 파싱 실패: {ex.Message}");
                }
            }
        }
    }

    // === 핵심 로직 ===
    private void ApplyJsonToAsset(TAsset asset, string json, string rootArrayKey)
    {
        if (_listProp == null)
            throw new InvalidOperationException($"'{ListName}' 리스트를 찾지 못했습니다.");

        var (listField, elemType) = ResolveListFieldAndElementType(asset.GetType(), ListName);
        if (listField == null || elemType == null)
            throw new InvalidOperationException($"'{ListName}' 필드 타입 해석 실패 (List<T>/T[] 필요).");

        string normalized = NormalizeToWrapperItems(json, rootArrayKey);

        IList newList;

        if (ImportDtoElementType != null)
        {
            // 1) DTO로 역직렬화
            var dtoWrapperType = typeof(ArrayWrapper<>).MakeGenericType(ImportDtoElementType);
            var dtoWrapper = JsonUtility.FromJson(normalized, dtoWrapperType);
            if (dtoWrapper == null) throw new Exception("JsonUtility.FromJson 결과가 null (DTO)");

            var dtoItemsField = dtoWrapperType.GetField("Items", BindingFlags.Instance | BindingFlags.Public);
            if (dtoItemsField == null) throw new Exception("DTO Wrapper.Items 필드를 찾지 못했습니다.");

            var dtoListObj = dtoItemsField.GetValue(dtoWrapper);
            if (!(dtoListObj is IList dtoItems)) throw new Exception("DTO Wrapper.Items 타입이 IList가 아닙니다.");

            // 2) DTO -> 요소타입 매핑
            newList = CreateListOf(elemType, dtoItems.Count);
            
            foreach (var dto in dtoItems)
            {
                var mapped = MapDtoToElement(dto, elemType);
                // if (ContentIdName != "" && mapped is ContentIdTable contentId)
                // {
                //     string id = TryGetStringMember(dto, ContentIdName) ?? TryGetStringMember(mapped, ContentIdName);
                //     if (!string.IsNullOrEmpty(id)) contentId.SetContentIDString(id);
                //     contentId.SetData();
                // }

                newList.Add(mapped);
            }
        }
        else
        {
            // 요소타입으로 직접 역직렬화
            var wrapperType = typeof(ArrayWrapper<>).MakeGenericType(elemType);
            var wrapper = JsonUtility.FromJson(normalized, wrapperType);
            if (wrapper == null) throw new Exception("JsonUtility.FromJson 결과가 null");

            var itemsField = wrapperType.GetField("Items", BindingFlags.Instance | BindingFlags.Public);
            if (itemsField == null) throw new Exception("Wrapper.Items 필드를 찾지 못했습니다.");

            var listObj = itemsField.GetValue(wrapper);
            if (!(listObj is IList items)) throw new Exception("Wrapper.Items 타입이 IList가 아닙니다.");

            newList = CreateListOf(elemType, items.Count);
            foreach (var it in items)
            {                 
                // if (it is ContentIdTable idTarget && !string.IsNullOrEmpty(idTarget.ContentIDString))
                // {
                //     idTarget.SetContentIDString(idTarget.ContentIDString);
                // }

                newList.Add(it);
            }
        }

        Undo.RecordObject(asset, $"Fill {ListName} from JSON");
        SetListOnAsset(asset, newList);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        Debug.Log($"{typeof(TAsset).Name}.{ListName} 채움 완료: {newList.Count}개 항목");
    }

    /// <summary>
    /// DTO → 요소타입으로 매핑. 기본 구현은 같은 이름의 필드/프로퍼티를 찾아 값 복사.
    /// 필요하면 파생 클래스에서 오버라이드.
    /// </summary>
    private static object MapDtoToElement(object dto, Type elemType)
    {
        var dst = Activator.CreateInstance(elemType);

        // 소스(필드/프로퍼티)
        var sFields = dto.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
        var sProps = dto.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

        // 타겟(필드/프로퍼티)
        var tFields = elemType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var tProps = elemType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        // 필드 이름 매핑
        foreach (var sf in sFields)
        {
            var tf = Array.Find(tFields, f => f.Name == sf.Name);
            if (tf != null && !tf.IsInitOnly)
            {
                var val = sf.GetValue(dto);
                TryAssign(tf.FieldType, val, v => tf.SetValue(dst, v));
                continue;
            }

            var tp = Array.Find(tProps, p => p.Name == sf.Name && p.CanWrite);
            if (tp != null)
            {
                var val = sf.GetValue(dto);
                TryAssign(tp.PropertyType, val, v => tp.SetValue(dst, v));
            }
        }

        // 프로퍼티 이름 매핑도 지원 (DTO가 프로퍼티일 수도 있으니)
        foreach (var sp in sProps)
        {
            if (!sp.CanRead) continue;
            var tp = Array.Find(tProps, p => p.Name == sp.Name && p.CanWrite);
            if (tp != null)
            {
                var val = sp.GetValue(dto);
                TryAssign(tp.PropertyType, val, v => tp.SetValue(dst, v));
                continue;
            }

            var tf = Array.Find(tFields, f => f.Name == sp.Name);
            if (tf != null && !tf.IsInitOnly)
            {
                var val = sp.GetValue(dto);
                TryAssign(tf.FieldType, val, v => tf.SetValue(dst, v));
            }
        }

        return dst;
    }

    private static void TryAssign(Type targetType, object value, Action<object> setter)
    {
        if (value == null)
        {
            // 값형은 기본값으로, 참조형은 null 그대로
            setter(targetType.IsValueType ? Activator.CreateInstance(targetType) : null);
            return;
        }

        var vType = value.GetType();
        if (targetType.IsAssignableFrom(vType))
        {
            setter(value);
            return;
        }

        try
        {
            // 간단 형변환 (예: int → enum, string → enum 등)
            if (targetType.IsEnum)
            {
                setter(vType == typeof(string)
                    ? Enum.Parse(targetType, (string)value)
                    : Enum.ToObject(targetType, Convert.ChangeType(value, Enum.GetUnderlyingType(targetType))));
                return;
            }

            setter(Convert.ChangeType(value, targetType));
        }
        catch
        {
            // 변환 불가하면 무시
        }
    }

    private static (FieldInfo field, Type elementType) ResolveListFieldAndElementType(Type assetType,
        string listFieldName)
    {
        var f = assetType.GetField(listFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f == null) return (null, null);
        var t = f.FieldType;

        if (t.IsArray) return (f, t.GetElementType());
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) return (f, t.GetGenericArguments()[0]);
        return (null, null);
    }

    private static IList CreateListOf(Type elemType, int capacity)
    {
        var listType = typeof(List<>).MakeGenericType(elemType);
        return (IList)Activator.CreateInstance(listType, capacity);
    }

    private void SetListOnAsset(TAsset asset, IList list)
    {
        var f = asset.GetType().GetField(ListName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f == null) throw new InvalidOperationException($"'{ListName}' 필드를 찾지 못했습니다.");
        f.SetValue(asset, list);
    }

    // JSON이 [ ... ] 형태면 그대로 감싸고, { ... } 객체면 지정 키에서 배열을 추출하여 감쌉니다.
    private static string NormalizeToWrapperItems(string json, string rootArrayKey)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("빈 JSON");
        if (LooksLikeArray(json)) return $"{{\"Items\":{json}}}";

        // 객체에서 키를 찾아 배열 부분만 추출
        if (TryExtractArrayByKey(json, rootArrayKey, out string arrayPayload)) return $"{{\"Items\":{arrayPayload}}}";

        throw new FormatException($"루트가 배열이 아니고, 객체에서 키 '{rootArrayKey}' 의 배열을 찾지 못했습니다.");
    }

    private static bool LooksLikeArray(string json)
    {
        foreach (char c in json)
        {
            if (!char.IsWhiteSpace(c)) return c == '[';
        }

        return false;
    }

    // 따옴표 토글 순서 수정: 문자열 바깥에서만 키 시도 → 마지막에 토글
    private static bool TryExtractArrayByKey(string json, string key, out string arrayPayload)
    {
        arrayPayload = null;
        if (string.IsNullOrEmpty(key)) return false;

        bool inStr = false;
        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            bool escaped = i > 0 && json[i - 1] == '\\';

            // 문자열 바깥에서만 키 토큰 시도
            if (!inStr && c == '"' && TryMatchString(json, i, key, out int afterQuote))
            {
                int colon = SkipWsToChar(json, afterQuote, ':');
                if (colon < 0) continue;

                int arrStart = SkipWsToChar(json, colon + 1, '[');
                if (arrStart < 0) continue;

                int depth = 0;
                bool inStr2 = false;
                for (int j = arrStart; j < json.Length; j++)
                {
                    char cj = json[j];
                    bool esc2 = j > 0 && json[j - 1] == '\\';

                    if (cj == '"' && !esc2) inStr2 = !inStr2;
                    if (inStr2) continue;

                    if (cj == '[') depth++;
                    else if (cj == ']')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            arrayPayload = json.Substring(arrStart, j - arrStart + 1);
                            return true;
                        }
                    }
                }
            }

            // 따옴표 토글은 마지막에
            if (c == '"' && !escaped) inStr = !inStr;
        }

        return false;
    }

    private static bool TryMatchString(string s, int quoteStart, string target, out int afterClosingQuote)
    {
        afterClosingQuote = -1;
        if (quoteStart >= s.Length || s[quoteStart] != '"') return false;
        int end = quoteStart + 1;
        var sb = new StringBuilder();
        while (end < s.Length)
        {
            char c = s[end++];
            if (c == '"' && s[end - 2] != '\\') break;
            sb.Append(c);
        }

        afterClosingQuote = end;
        return sb.ToString() == target;
    }

    private static int SkipWsToChar(string s, int start, char ch)
    {
        for (int i = start; i < s.Length; i++)
        {
            char c = s[i];
            if (!char.IsWhiteSpace(c)) return c == ch ? i : -1;
        }

        return -1;
    }

    private static string TryGetStringMember(object obj, string name)
    {
        if (obj == null || string.IsNullOrEmpty(name)) return null;

        var t = obj.GetType();
        // 필드
        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(string)) return (string)f.GetValue(obj);

        // 프로퍼티
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanRead && p.PropertyType == typeof(string)) return (string)p.GetValue(obj);

        return null;
    }

    private void ApplyJsonFromPathSafe(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.LogWarning("파일 경로가 비어 있습니다.");
            return;
        }

        // 확장자 체크 (강제는 아니지만 실수 방지)
        if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            if (!EditorUtility.DisplayDialog("확인", "선택한 파일이 .json 확장자가 아닙니다. 계속하시겠습니까?", "예", "아니오"))
                return;
        }

        if (!File.Exists(path))
        {
            Debug.LogError($"파일을 찾을 수 없습니다: {path}");
            return;
        }

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            TryApplyJson(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"파일 읽기 실패: {ex.Message}");
        }
    }
}

[Serializable]
internal class ArrayWrapper<T>
{
    public List<T> Items;
}

#endif