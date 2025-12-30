using System;
using System.Collections.Generic;
using DS.Data;
using DS.Enumerations;
using UnityEngine;
using DS.ScriptableObjects;
using TMPro;
using UnityEngine.UI;

public partial class DialogueManager 
{
   private void TestMethod(DSDialogueSO node)
   {
       Debug.Log($"Reward from dialogue {node.DialogueName}");
   }
}
