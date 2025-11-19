public interface IInteractable
{
    void BeginInteract(Player player);
    void ContinueInteract(Player player); 
    void EndInteract(Player player);
}