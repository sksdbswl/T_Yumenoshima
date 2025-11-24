public interface IInteractable
{
    void CheckInteract(int stage);
    void BeginInteract(Player player);
    void ContinueInteract(Player player); 
    void EndInteract(Player player);
}