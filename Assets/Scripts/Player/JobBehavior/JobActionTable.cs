
using System;
using System.Collections.Generic;

public static class JobActionTable
{
    private static readonly Dictionary<Const.JobType, Func<IJobBehavior>> _map
        = new()
        {
            { Const.JobType.Firefighter, () => new FirefighterBehavior() },
            { Const.JobType.Police, () => new PoliceBehavior() },
            { Const.JobType.Doctor, () => new DoctorBehavior() },
            { Const.JobType.Citizen, () => new CitizenBehavior() }
        };

    public static IJobBehavior Create(Const.JobType jobType)
    {
        if (_map.TryGetValue(jobType, out var creator))
            return creator();
        
        return new CitizenBehavior();
    }
    
    // public static IJobBehavior Create(Const.JobType jobType)
    // {
    //     switch (jobType)
    //     {
    //         case Const.JobType.Citizen:
    //             //return new FarmerBehavior();
    //
    //         case Const.JobType.Police:
    //             return new PoliceBehavior();
    //
    //         case Const.JobType.Doctor:
    //             return new DoctorBehavior();
    //
    //         case Const.JobType.Firefighter:
    //         default:
    //             return new FirefighterBehavior();
    //     }
    // }
    
    private static readonly Dictionary<Const.JobType, HashSet<Const.JobType>> jobActions = new()
    {
        { Const.JobType.None, new HashSet<Const.JobType>() },
        { Const.JobType.Firefighter, new HashSet<Const.JobType> { Const.JobType.Firefighter } },
        { Const.JobType.Police, new HashSet<Const.JobType> { Const.JobType.Police } },
        { Const.JobType.Doctor, new HashSet<Const.JobType> { Const.JobType.Doctor } },
    };

    public static bool CanDo(Const.JobType job)
    {
        return jobActions.TryGetValue(job, out var actions) && actions.Contains(job);
    }
}