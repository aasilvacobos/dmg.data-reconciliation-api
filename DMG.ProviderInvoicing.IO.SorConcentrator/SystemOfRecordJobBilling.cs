using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Domain.Map;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.IO.SorConcentrator;

/// <summary>
/// API consumed by other I/O adapters to retrieve job billing from the SOR.
/// </summary>
internal static class SystemOfRecordJobBilling
{
    internal static async Task<Either<ErrorMessage, DT.Domain.JobBillingGross>> GetByIdAsync(JobBillingId jobBillingId) =>
        await SorConcentratorClient.GetByIdAsync<Dmg.Work.Billing.V1.JobBillingData>(jobBillingId.Value, SorEntityName.JobBilling)
            .MapAsync(JobBillingGrossMessageMapper.ToEntity);
    
    /// Retrieve the JobBillingGross from Fulfillment and return as JobBillingDecorated 
    internal static async Task<Either<ErrorMessage, JobBillingDecorated>> GetJobBillingDecoratedByIdAsync(JobBillingId jobBillingId) {
        // Retrieve job billing gross 
        var jobBillingGrossEither = await GetByIdAsync(jobBillingId);  
        
        // In order to build the JobBillingDecorated, a successful retrieve of jobWork is also required
        var jobBillingDecoratedEither = 
            await jobBillingGrossEither.BindAsync(async jobBillingGross =>
            {
                var jobEither = await SystemOfRecordJob.GetJobByIdAsync(jobBillingGross.JobWorkId);
                return jobEither
                    .Map(job => JobBillingGrossMapper.ToJobBillingDecorated(jobBillingGross, job));
            });
        
        return jobBillingDecoratedEither;
    }
}