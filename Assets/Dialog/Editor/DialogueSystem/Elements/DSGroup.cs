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
        public string QuestId = "";
        
        private EnumField typeField;
        private TextField npcIdField;
        private TextField questIdField;
        
        public DSGroup(string groupTitle, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();

            title = groupTitle;
            OldTitle = groupTitle;

            SetPosition(new Rect(position, Vector2.zero));

            defaultBorderColor = contentContainer.style.borderBottomColor.value;
            defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
            
            AddMetaUI();
            RefreshMetaUIVisibility();
            
            ApplyMeta(GroupType, NpcId, QuestId);
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
            // Group 기본 헤더(titleContainer)에 작은 UI 붙이기
            typeField = new EnumField(GroupType);
            typeField.style.minWidth = 110;
            typeField.RegisterValueChangedCallback(evt =>
            {
                GroupType = (DialogueGroupType)evt.newValue;
                RefreshMetaUIVisibility();
            });

            npcIdField = new TextField("NpcId") { value = NpcId };
            npcIdField.style.minWidth = 150;
            npcIdField.RegisterValueChangedCallback(evt => NpcId = evt.newValue);

            questIdField = new TextField("QuestId") { value = QuestId };
            questIdField.style.minWidth = 150;
            questIdField.RegisterValueChangedCallback(evt => QuestId = evt.newValue);

            // 헤더에 추가 (그룹 타이틀 아래쪽에 붙음)
            headerContainer.Add(typeField);
            headerContainer.Add(npcIdField);
            headerContainer.Add(questIdField);
        }

        private void RefreshMetaUIVisibility()
        {
            // 타입별로 필요한 필드만 노출
            bool showNpc = GroupType == DialogueGroupType.NpcStory;
            bool showQuest = GroupType == DialogueGroupType.Quest;

            npcIdField.style.display = showNpc ? DisplayStyle.Flex : DisplayStyle.None;
            questIdField.style.display = showQuest ? DisplayStyle.Flex : DisplayStyle.None;

            // 타입 바뀌면 반대쪽 ID는 비워두는 게 안전
            if (showNpc) QuestId = "";
            if (showQuest) NpcId = "";
            if (GroupType == DialogueGroupType.MainStory || GroupType == DialogueGroupType.Daily)
            {
                NpcId = "";
                QuestId = "";
            }

            // UI 값도 동기화
            npcIdField.SetValueWithoutNotify(NpcId);
            questIdField.SetValueWithoutNotify(QuestId);
        }
        
        public void ApplyMeta(DialogueGroupType type, string npcId, string questId)
        {
            GroupType = type;
            NpcId = npcId ?? "";
            QuestId = questId ?? "";

            SyncMetaUI();
        }

        public void SyncMetaUI()
        {
            if (typeField == null || npcIdField == null || questIdField == null)
                return;

            // UI 값 동기화
            typeField.SetValueWithoutNotify(GroupType);
            npcIdField.SetValueWithoutNotify(NpcId);
            questIdField.SetValueWithoutNotify(QuestId);

            // 표시/숨김만 처리 (값 초기화는 여기서 하지 않는 게 안전)
            bool showNpc = GroupType == DialogueGroupType.NpcStory;
            bool showQuest = GroupType == DialogueGroupType.Quest;

            npcIdField.style.display = showNpc ? DisplayStyle.Flex : DisplayStyle.None;
            questIdField.style.display = showQuest ? DisplayStyle.Flex : DisplayStyle.None;
        }

    }
}