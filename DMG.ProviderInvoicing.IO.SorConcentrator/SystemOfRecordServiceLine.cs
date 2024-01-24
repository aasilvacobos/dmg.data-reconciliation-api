using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using LanguageExt;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatistics;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.IO.SorConcentrator;

/// <summary>
/// API consumed by other I/O adapters to retrieve service type from the SOR Concentrator.
/// </summary>
internal static class SystemOfRecordServiceLine
{
    private static DualLayerMemoryCacheWrapper dualLayerMemoryCacheWrapper = DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer(), Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer()*4);

    private static async Task<Either<ErrorMessage, DT.Domain.ServiceLine>> GetByIdCoreAsync(ServiceLineId serviceLineId) =>
        (await SorConcentratorClient.GetByIdAsync<DMG.DataServices.ServiceLine>(serviceLineId.Value, SorEntityName.ServiceLine))
        .Map(ServiceLineMessageMapper.ToEntity)
        .MapLeft(errorMessage =>
            errorMessage switch
            {
                ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.ServiceLineNotFound, // convert generic error message to entity specific message 
                _ => errorMessage
            });

    internal static Task<Either<ErrorMessage, DT.Domain.ServiceLine>> GetByIdAsync(ServiceLineId serviceLineId) =>
        dualLayerMemoryCacheWrapper.GetOrUpdateAsync<Guid, ErrorMessage, DT.Domain.ServiceLine>(serviceLineId.Value, () => GetByIdCoreAsync(serviceLineId));

    internal static MemoryCacheCounts GetCacheCount() =>
        MemoryCacheStatisticsMapper.FromDualLayerMemoryWrapperCacheCounts(MemoryCacheEntity.ServiceLine, dualLayerMemoryCacheWrapper.GetCounts());
}