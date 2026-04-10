using Cysharp.Threading.Tasks;

public interface IJobBehavior
{
    Const.JobType JobType { get; }
    bool CanInteract(Player player, IInteractable target);
    UniTask Execute(Player player, IInteractable target);
}