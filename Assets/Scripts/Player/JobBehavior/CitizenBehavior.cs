using Cysharp.Threading.Tasks;

public class CitizenBehavior : IJobBehavior
{
    private IJobBehavior _jobBehaviorImplementation;
    public Const.JobType JobType => Const.JobType.Citizen;
    public bool CanInteract(Player player, IInteractable target)
    {
        return _jobBehaviorImplementation.CanInteract(player, target);
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