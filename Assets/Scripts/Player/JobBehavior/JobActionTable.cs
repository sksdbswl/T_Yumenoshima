
using System;
using System.Collections.Generic;

public static class JobActionTable
{
    private static readonly Dictionary<Const.JobType, Func<IJobBehavior>> _jobMap
        = new()
        {
            { Const.JobType.Firefighter, () => new FirefighterBehavior() },
            { Const.JobType.Police, () => new PoliceBehavior() },
            { Const.JobType.Doctor, () => new DoctorBehavior() },
            { Const.JobType.Citizen, () => new CitizenBehavior() }
        };

    public static IJobBehavior Create(Const.JobType jobType)
    {
        if (_jobMap.TryGetValue(jobType, out var creator))
            return creator();
        
        return new CitizenBehavior();
    }
}