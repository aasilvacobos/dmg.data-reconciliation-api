namespace DMG.ProviderInvoicing.DT.Domain.Rule;

/// Rules and calculations related to the job 
public static class JobRule
{
    public static bool IsProviderNotResponding(Job job) =>
        IsProviderNotResponding(job.JobWorkState);

    public static bool IsProviderNotResponding(JobWorkState jobWorkState) =>
        jobWorkState == JobWorkState.JobProviderNotResponding;
    
    /// Determine if costing is defined on job
    public static bool IsCostingDefined(Job job) =>
        CostingRule.TryChooseCosting(job.Costings).IsSome;
}