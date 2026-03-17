using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public interface IInteractable
{
    void CheckInteract(int stage);
    UniTask BeginInteract(Player player);
    void EndInteract(Player player);
}