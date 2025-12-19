using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DS.Utilities
{
    using Data;
    using Data.Save;
    using Elements;
    using ScriptableObjects;
    using Windows;
    using Enumerations;

    public static class DSIOUtility
    {
        private static DSGraphView graphView;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<DSNode> nodes;
        private static List<DSGroup> groups;

        private static Dictionary<string, DSDialogueGroupSO> createdDialogueGroups;
        private static Dictionary<string, DSDialogueSO> createdDialogues;

        private static Dictionary<string, DSGroup> loadedGroups;
        private static Dictionary<string, DSNode> loadedNodes;

        public static void Initialize(DSGraphView dsGraphView, string graphName)
        {
            graphView = dsGraphView;

            graphFileName = graphName;
            containerFolderPath = $"Assets/Dialog/DialogueSystem/Dialogues/{graphName}";

            nodes = new List<DSNode>();
            groups = new List<DSGroup>();

            createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
            createdDialogues = new Dictionary<string, DSDialogueSO>();

            loadedGroups = new Dictionary<string, DSGroup>();
            loadedNodes = new Dictionary<string, DSNode>();
        }

        public static void Save()
        {
            CreateDefaultFolders();

            GetElementsFromGraphView();

            DSGraphSaveDataSO graphData =
                CreateAsset<DSGraphSaveDataSO>("Assets/Dialog/Editor/DialogueSystem/Graphs", $"{graphFileName}Graph");
            graphData.Initialize(graphFileName);

            DSDialogueContainerSO dialogueContainer = CreateAsset<DSDialogueContainerSO>(containerFolderPath, graphFileName);
            dialogueContainer.Initialize(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        // =========================
        // GROUPS
        // =========================

        private static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();

            foreach (DSGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(DSGroup group, DSGraphSaveDataSO graphData)
        {
            DSGroupSaveData groupData = new DSGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position,

                GroupType = group.GroupType,
                NpcId = group.NpcId,
                QuestId = group.QuestId
            };

            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder($"{containerFolderPath}/Groups", groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            DSDialogueGroupSO dialogueGroup =
                CreateAsset<DSDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

            dialogueGroup.Initialize(groupName);
            dialogueGroup.SetMeta(group.GroupType, group.NpcId, group.QuestId);

            createdDialogueGroups.Add(group.ID, dialogueGroup);
            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());

            SaveAsset(dialogueGroup);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        // =========================
        // NODES
        // =========================

        private static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (DSNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);
                    continue;
                }

                ungroupedNodeNames.Add(node.DialogueName);
            }

            UpdateDialoguesChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void SaveNodeToGraph(DSNode node, DSGraphSaveDataSO graphData)
        {
            List<DSChoiceSaveData> choices = CloneNodeChoices(node.Choices);

            DSNodeSaveData nodeData = new DSNodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position,
                AnimationClip = node.NpcAnimationClip,

                // ✅ Actions 저장 (Graph에)
                Actions = CloneActions(node.Actions)
            };

            graphData.Nodes.Add(nodeData);
        }

        private static void SaveNodeToScriptableObject(DSNode node, DSDialogueContainerSO dialogueContainer)
        {
            DSDialogueSO dialogue;

            if (node.Group != null)
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);
                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);
                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }

            dialogue.Initialize(
                node.DialogueName,
                node.Text,
                ConvertNodeChoicesToDialogueChoices(node.Choices),
                node.DialogueType,
                node.IsStartingNode(),
                node.NpcAnimationClip
            );

            // ✅ 그룹 메타를 DialogueSO에 복사 (GroupType/NpcId/QuestId)
            if (node.Group != null)
            {
                var groupSO = createdDialogueGroups[node.Group.ID];
                dialogue.SetGroupMeta(groupSO.GroupType, groupSO.NpcId, groupSO.QuestId);
            }
            else
            {
                // ungrouped 정책: 필요하면 None/Empty로 세팅
                // (DialogueGroupType에 None이 없으면 아래 줄은 제거해도 됨)
                // dialogue.SetGroupMeta(DialogueGroupType.None, "", "");
            }

            // ✅ Actions 저장 (SO에)
            dialogue.Actions = CloneActions(node.Actions);

            createdDialogues.Add(node.ID, dialogue);
            SaveAsset(dialogue);
        }

        private static List<DSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSDialogueChoiceData> dialogueChoices = new List<DSDialogueChoiceData>();

            foreach (DSChoiceSaveData nodeChoice in nodeChoices)
            {
                DSDialogueChoiceData choiceData = new DSDialogueChoiceData()
                {
                    Text = nodeChoice.Text
                };

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        private static void UpdateDialoguesChoicesConnections()
        {
            foreach (DSNode node in nodes)
            {
                DSDialogueSO dialogue = createdDialogues[node.ID];

                for (int choiceIndex = 0; choiceIndex < node.Choices.Count; ++choiceIndex)
                {
                    DSChoiceSaveData nodeChoice = node.Choices[choiceIndex];

                    if (string.IsNullOrEmpty(nodeChoice.NodeID))
                        continue;

                    dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[nodeChoice.NodeID];
                    SaveAsset(dialogue);
                }
            }
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        // =========================
        // LOAD
        // =========================

        public static void Load()
        {
            DSGraphSaveDataSO graphData = LoadAsset<DSGraphSaveDataSO>("Assets/Dialog/Editor/DialogueSystem/Graphs", graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"Assets/Dialog/Editor/DialogueSystem/Graphs/{graphFileName}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );

                return;
            }

            DSEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadGroups(List<DSGroupSaveData> groups)
        {
            foreach (DSGroupSaveData groupData in groups)
            {
                DSGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;

                group.GroupType = groupData.GroupType;
                group.NpcId = groupData.NpcId;
                group.QuestId = groupData.QuestId;

                group.ApplyMeta(groupData.GroupType, groupData.NpcId, groupData.QuestId);
                loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(List<DSNodeSaveData> nodes)
        {
            foreach (DSNodeSaveData nodeData in nodes)
            {
                List<DSChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);

                DSNode node = graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);

                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;
                node.NpcAnimationClip = nodeData.AnimationClip;

                // ✅ Actions 로드 (Graph -> Node)
                node.Actions = CloneActions(nodeData.Actions);

                node.Draw();
                graphView.AddElement(node);

                loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID))
                    continue;

                DSGroup group = loadedGroups[nodeData.GroupID];
                node.Group = group;
                group.AddElement(node);
            }
        }

        private static void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, DSNode> loadedNode in loadedNodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    DSChoiceSaveData choiceData = (DSChoiceSaveData)choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.NodeID))
                        continue;

                    DSNode nextNode = loadedNodes[choiceData.NodeID];
                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                    graphView.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        // =========================
        // FOLDERS / ASSETS
        // =========================

        private static void CreateDefaultFolders()
        {
            CreateFolder("Assets/Dialog/Editor/DialogueSystem", "Graphs");

            CreateFolder("Assets/Dialog", "DialogueSystem");
            CreateFolder("Assets/Dialog/DialogueSystem", "Dialogues");

            CreateFolder("Assets/Dialog/DialogueSystem/Dialogues", graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "Dialogues");
        }

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(DSGroup);

            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is DSNode node)
                {
                    nodes.Add(node);
                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    DSGroup group = (DSGroup)graphElement;
                    groups.Add(group);
                    return;
                }
            });
        }

        public static void CreateFolder(string parentFolderPath, string newFolderName)
        {
            if (AssetDatabase.IsValidFolder($"{parentFolderPath}/{newFolderName}"))
                return;

            AssetDatabase.CreateFolder(parentFolderPath, newFolderName);
        }

        public static void RemoveFolder(string path)
        {
            FileUtil.DeleteFileOrDirectory($"{path}.meta");
            FileUtil.DeleteFileOrDirectory($"{path}/");
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        public static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        // =========================
        // CLONE HELPERS
        // =========================

        private static List<DSChoiceSaveData> CloneNodeChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSChoiceSaveData> choices = new List<DSChoiceSaveData>();

            if (nodeChoices == null) return choices;

            foreach (DSChoiceSaveData choice in nodeChoices)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };

                choices.Add(choiceData);
            }

            return choices;
        }

        private static List<DSDialogueActionData> CloneActions(List<DSDialogueActionData> src)
        {
            if (src == null) return new List<DSDialogueActionData>();

            var list = new List<DSDialogueActionData>(src.Count);

            for (int i = 0; i < src.Count; i++)
            {
                var a = src[i];
                if (a == null) continue;

                list.Add(new DSDialogueActionData
                {
                    trigger = a.trigger,
                    type = a.type,
                    npcId = a.npcId,
                    npcStoryStage = a.npcStoryStage,
                    questId = a.questId,
                    questState = a.questState,
                    flag = a.flag
                });
            }

            return list;
        }
    }
}
