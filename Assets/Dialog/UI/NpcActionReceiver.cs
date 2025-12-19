using Dialog.DialogueSystem.Scripts.Data;
using UnityEngine;

namespace Dialog.UI
{
    public class NpcActionReceiver : MonoBehaviour
    {
        [DialogueAction]
        public void GiveReward()
        {
            Debug.Log("Reward given!");
        }

        [DialogueAction]
        public void StartQuest()
        {
            Debug.Log("Quest started!");
        }
    }

}