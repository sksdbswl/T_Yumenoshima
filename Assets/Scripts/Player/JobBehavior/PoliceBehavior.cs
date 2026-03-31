using Cysharp.Threading.Tasks;

public class PoliceBehavior : IJobBehavior
{
    private IJobBehavior _jobBehaviorImplementation;
    public Const.JobType JobType => Const.JobType.Police;
    public bool CanInteract(IInteractable target)
    {
        return _jobBehaviorImplementation.CanInteract(target);
    }

    public async UniTask Execute(Player player, IInteractable target)
    {
        _jobBehaviorImplementation.Execute(player, target);
        await UniTask.Delay(500);
    }

    public bool CanInteract()
    {
        throw new System.NotImplementedException();
    }

    public void Execute(Player player)
    {
        throw new System.NotImplementedException();
    }
}