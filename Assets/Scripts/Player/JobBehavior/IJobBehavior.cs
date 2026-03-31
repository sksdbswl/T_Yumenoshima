using Cysharp.Threading.Tasks;

public interface IJobBehavior
{
    Const.JobType JobType { get; }
    bool CanInteract(IInteractable target);
    UniTask Execute(Player player, IInteractable target);
}