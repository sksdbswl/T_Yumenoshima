using System;
using System.Collections.Generic;
using System.Linq;
using DS.ScriptableObjects;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    using Data.Save;
    using Enumerations;
    using Utilities;
    using Windows;

    /// <summary>
    /// 실제 Editor에 그려질 노드 정보들
    /// </summary>
    public class DSNode : Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; } // 다이얼로그에서 사용될 각 노드의 이름
        public List<DSChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }
        public AnimationClip NpcAnimationClip { get; set; }
        
        public List<DSDialogueActionData> Actions { get; set; } = new();
        
        public int StageId { get; set; }

        public DSGroup Group { get; set; }
        
        protected DSGraphView graphView;
        private Color defaultBackgroundColor;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }

        public virtual void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();

            DialogueName = nodeName;
            Choices = new List<DSChoiceSaveData>();
            Text = "Dialogue text.";

            SetPosition(new Rect(position, Vector2.zero));

            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {
            /* TITLE CONTAINER */

            TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName, null, callback =>
            {
                TextField target = (TextField) callback.target;

                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName))
                    {
                        ++graphView.NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName))
                    {
                        --graphView.NameErrorsAmount;
                    }
                }

                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);

                    DialogueName = target.value;

                    graphView.AddUngroupedNode(this);

                    return;
                }

                DSGroup currentGroup = Group;

                graphView.RemoveGroupedNode(this, Group);

                DialogueName = target.value;

                graphView.AddGroupedNode(this, currentGroup);
            });

            dialogueNameTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );

            titleContainer.Insert(0, dialogueNameTextField);

            /* INPUT CONTAINER */

            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);

            /* EXTENSION CONTAINER */

            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = DSElementUtility.CreateTextArea(Text, null, callback => Text = callback.newValue);

            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            textFoldout.Add(textTextField);

            customDataContainer.Add(textFoldout);

            
            /* Animation Clip */
            
            ObjectField animField = DSElementUtility.CreateObjectField<AnimationClip>(
                "NPC Animation",
                NpcAnimationClip,
                evt =>
                {
                    NpcAnimationClip = evt.newValue as AnimationClip;
                }
            );
            
            /* ACTIONS */
            Foldout actionsFoldout = DSElementUtility.CreateFoldout("Actions");

            Button addActionBtn = new Button(() =>
            {
                Actions.Add(new DSDialogueActionData());
                RefreshActionsUI(actionsFoldout);
            })
            {
                text = "+ Add Action"
            };
            
            IntegerField stageField = new IntegerField("StageId")
            {
                value = StageId
            };
            stageField.RegisterValueChangedCallback(evt => StageId = evt.newValue);

            customDataContainer.Add(stageField);

            actionsFoldout.Add(addActionBtn);
            RefreshActionsUI(actionsFoldout);

            customDataContainer.Add(actionsFoldout);
            
            // 스타일이 필요하면 클래스 추가도 가능
            //animField.AddToClassList("ds-node__object-field");

            customDataContainer.Add(animField);
            
            extensionContainer.Add(customDataContainer);
        }

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port) inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
        
        private void RefreshActionsUI(Foldout foldout)
        {
            // foldout[0] = add button 이므로 1부터 삭제
            while (foldout.childCount > 1)
                foldout.RemoveAt(1);

            for (int i = 0; i < Actions.Count; i++)
            {
                int index = i;
                var action = Actions[index];

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Column;
                row.style.marginBottom = 6;
                row.style.paddingLeft = 6;

                // 1) Trigger
                var triggerField = new EnumField("Trigger", action.trigger);
                triggerField.RegisterValueChangedCallback(e =>
                {
                    action.trigger = (DSDialogueActionTrigger)e.newValue;
                });

                // 2) Type
                var typeField = new EnumField("Type", action.type);
                typeField.RegisterValueChangedCallback(e =>
                {
                    action.type = (DSDialogueActionType)e.newValue;
                    RefreshActionsUI(foldout); // 타입 바뀌면 파라미터 UI 다시 그림
                });

                row.Add(triggerField);
                row.Add(typeField);

                // 3) Type별 파라미터
                row.Add(BuildActionParamsUI(action));

                // 4) Remove Button
                var removeBtn = new Button(() =>
                {
                    Actions.RemoveAt(index);
                    RefreshActionsUI(foldout);
                })
                {
                    text = "Remove"
                };

                row.Add(removeBtn);

                foldout.Add(row);
            }
        }
        private VisualElement BuildActionParamsUI(DSDialogueActionData action)
        {
            var box = new VisualElement();
            box.style.flexDirection = FlexDirection.Column;
            box.style.marginTop = 4;

            switch (action.type)
            {
                case DSDialogueActionType.SetNpcStoryStage:
                {
                    var npcId = new TextField("NpcId") { value = action.npcId };
                    npcId.RegisterValueChangedCallback(e => action.npcId = e.newValue);

                    var stage = new IntegerField("StoryID") { value = action.npcStoryStage };
                    stage.RegisterValueChangedCallback(e => action.npcStoryStage = e.newValue);

                    box.Add(npcId);
                    box.Add(stage);
                    break;
                }
                
                case DSDialogueActionType.SetQuestState:
                {
                    if (action.questMeta == null)
                        action.questMeta = new QuestMetaData();

                    var state = new EnumField("QuestState", action.questState);
                    state.RegisterValueChangedCallback(e => action.questState = (QuestState)e.newValue);

                    box.Add(state);
                    box.Add(CreateQuestMetaFields(action));
                    break;
                }

                case DSDialogueActionType.SetFlag:
                {
                    var flag = new TextField("Flag") { value = action.flag };
                    flag.RegisterValueChangedCallback(e => action.flag = e.newValue);

                    box.Add(flag);
                    break;
                }
                
                case DSDialogueActionType.CallMethod:
                {
                    var receiverField = new TextField("ReceiverType") { value = action.receiverType };
                    receiverField.RegisterValueChangedCallback(e => action.receiverType = e.newValue);

                    var methodField = new TextField("MethodName") { value = action.methodName };
                    methodField.RegisterValueChangedCallback(e => action.methodName = e.newValue);

                    box.Add(receiverField);
                    box.Add(methodField);
                    
                    break;
                }
            }
            
            return box;
        }
        
        private VisualElement CreateQuestMetaFields(DSDialogueActionData action)
        {
            if (action.questMeta == null)
                action.questMeta = new QuestMetaData();

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.marginTop = 4;
            container.style.marginBottom = 4;

            var questIdField = new TextField("QuestId")
            {
                value = action.questMeta.questId ?? action.questId ?? ""
            };
            questIdField.RegisterValueChangedCallback(evt =>
            {
                action.questMeta.questId = evt.newValue;
                action.questId = evt.newValue; // 기존 questId와 동기화
            });
            container.Add(questIdField);

            var questNameField = new TextField("QuestName")
            {
                value = action.questMeta.questName ?? ""
            };
            questNameField.RegisterValueChangedCallback(evt =>
            {
                action.questMeta.questName = evt.newValue;
            });
            container.Add(questNameField);

            var descriptionField = new TextField("Description")
            {
                value = action.questMeta.description ?? "",
                multiline = true
            };
            descriptionField.RegisterValueChangedCallback(evt =>
            {
                action.questMeta.description = evt.newValue;
            });
            container.Add(descriptionField);

            var moneyField = new IntegerField("Money")
            {
                value = action.questMeta.money
            };
            moneyField.RegisterValueChangedCallback(evt =>
            {
                action.questMeta.money = evt.newValue;
            });
            container.Add(moneyField);

            var expField = new IntegerField("Exp")
            {
                value = action.questMeta.exp
            };
            expField.RegisterValueChangedCallback(evt =>
            {
                action.questMeta.exp = evt.newValue;
            });
            container.Add(expField);

            var cleanlinessField = new IntegerField("Cleanliness")
            {
                value = action.questMeta.cleanliness
            };
            cleanlinessField.RegisterValueChangedCallback(evt =>
            {
                action.questMeta.cleanliness = evt.newValue;
            });
            container.Add(cleanlinessField);

            return container;
        }
    }
}