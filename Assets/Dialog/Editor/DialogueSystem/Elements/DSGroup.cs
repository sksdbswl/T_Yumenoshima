using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    public class DSGroup : Group
    {
        public string ID { get; set; }
        public string OldTitle { get; set; }

        private Color defaultBorderColor;
        private float defaultBorderWidth;

        public DialogueGroupType GroupType = DialogueGroupType.NpcStory;
        public string NpcId = "";

        private EnumField typeField;
        private TextField npcIdField;

        public DSGroup(string groupTitle, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();

            title = groupTitle;
            OldTitle = groupTitle;

            SetPosition(new Rect(position, Vector2.zero));

            defaultBorderColor = contentContainer.style.borderBottomColor.value;
            defaultBorderWidth = contentContainer.style.borderBottomWidth.value;

            AddMetaUI();
            SyncMetaUI();
        }

        public void SetErrorStyle(Color color)
        {
            contentContainer.style.borderBottomColor = color;
            contentContainer.style.borderBottomWidth = 2f;
        }

        public void ResetStyle()
        {
            contentContainer.style.borderBottomColor = defaultBorderColor;
            contentContainer.style.borderBottomWidth = defaultBorderWidth;
        }

        private void AddMetaUI()
        {
            typeField = new EnumField(GroupType);
            typeField.style.minWidth = 110;
            typeField.RegisterValueChangedCallback(evt =>
            {
                GroupType = (DialogueGroupType)evt.newValue;
                SyncMetaUI(); // 값 초기화 X, 표시만
            });

            npcIdField = new TextField("NpcId") { value = NpcId };
            npcIdField.style.minWidth = 160;
            npcIdField.RegisterValueChangedCallback(evt => NpcId = evt.newValue);

            headerContainer.Add(typeField);
            headerContainer.Add(npcIdField);
        }

        public void ApplyMeta(DialogueGroupType type, string npcId)
        {
            GroupType = type;
            NpcId = npcId ?? "";
            SyncMetaUI();
        }

        public void SyncMetaUI()
        {
            if (typeField == null || npcIdField == null) return;

            typeField.SetValueWithoutNotify(GroupType);
            npcIdField.SetValueWithoutNotify(NpcId);

            // ✅ NpcStory / Quest 는 npcId 필요
            bool showNpc = (GroupType == DialogueGroupType.NpcStory) || (GroupType == DialogueGroupType.Quest);
            npcIdField.style.display = showNpc ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
