using Cysharp.Threading.Tasks;


/// <summary>
/// Player 외의 상호작용 오브젝트가 보유하는 인터페이스
/// </summary>
public interface IInteractable
{
    void CheckInteract(int stage);
    UniTask BeginInteract(Player player);
    void EndInteract(Player player);
}