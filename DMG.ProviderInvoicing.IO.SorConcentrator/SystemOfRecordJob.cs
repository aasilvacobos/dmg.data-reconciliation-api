using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using LanguageExt;
using static LanguageExt.Prelude;
using Dmg.Work.V1;

namespace DMG.ProviderInvoicing.IO.SorConcentrator; 

/// <summary>
/// API consumed by other I/O adapters to retrieve job data from the SOR Concentrator.
/// </summary>
internal static class SystemOfRecordJob {
    private static async Task<Either<ErrorMessage, Work>> GetJobWorkMessageByIdAsync(JobWorkId jobWorkId) => 
        await SorConcentratorClient.GetByIdAsync<Work>(jobWorkId.Value, SorEntityName.Work)
            .Map(x => x.MapLeft(errorMessage =>
                errorMessage switch
                {
                    ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.JobNotFound, // convert generic error message to job specific message 
                    _ => errorMessage
                }));

    internal static async Task<Either<ErrorMessage, Job>> GetJobByIdAsync(JobWorkId jobWorkId) =>
        await GetJobWorkMessageByIdAsync(jobWorkId)
            .MapAsync(WorkMessageMapper.ToEntity)
            .MapAsync(job =>
            {
                if (!job.Costings.Exists(costing => costing.RateType == RateType.Regular)) IoAdapterLogger.Error($"Job {job.JobWorkId.Value} retrieved with no Regular costing defined.");
                return job;
            });

    private static Either<ErrorMessage, Work> GetJobWorkMessageById(JobWorkId jobWorkId) =>
       SorConcentratorClient.GetById<Work>(jobWorkId.Value, SorEntityName.Work)
        .MapLeft(errorMessage =>
               errorMessage switch
               {
                   ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.JobNotFound, // convert generic error message to job specific message 
                   _ => errorMessage
               });

    internal static Either<ErrorMessage, Job> GetJobById(JobWorkId jobWorkId) =>
       GetJobWorkMessageById(jobWorkId)
           .Map(WorkMessageMapper.ToEntity)
           .Map(job =>
           {
               if (!job.Costings.Exists(costing => costing.RateType == RateType.Regular)) IoAdapterLogger.Error($"Job {job.JobWorkId.Value} retrieved with no Regular costing defined.");
               return job;
           });
}