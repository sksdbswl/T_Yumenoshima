using UnityEngine;

public class Player : MonoBehaviour
{
    bool isTalking = false;
    NpcInteraction currentNpc;
    [SerializeField] private DialogTyper typer;
    

    // Update is called once per frame
    void Update()
    {
        if (isTalking && Input.GetKeyDown(KeyCode.Space))
        {
            currentNpc.TryTalk(currentNpc.npcSO,typer);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NpcInteraction>())
        {
            var npc = other.GetComponent<NpcInteraction>();
            Debug.Log(npc.npcSO.Name);

            if (!isTalking)
            {
                isTalking = true; 
            }
        }
    }
}
