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
internal static class SystemOfRecordServiceType
{
    private static DualLayerMemoryCacheWrapper dualLayerMemoryCacheWrapper = DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer(), Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer()*4);

    private static async Task<Either<ErrorMessage, DT.Domain.ServiceType>> GetByIdCoreAsync(ServiceTypeId serviceTypeId) =>
        (await SorConcentratorClient.GetByIdAsync<DMG.DataServices.ServiceType>(serviceTypeId.Value, SorEntityName.ServiceType))
        .Map(ServiceTypeMessageMapper.ToEntity)
        .MapLeft(errorMessage =>
            errorMessage switch
            {
                ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.ServiceTypeNotFound, // convert generic error message to entity specific message 
                _ => errorMessage
            });

    internal static Task<Either<ErrorMessage, DT.Domain.ServiceType>> GetByIdAsync(ServiceTypeId serviceTypeId) =>
        dualLayerMemoryCacheWrapper.GetOrUpdateAsync<Guid, ErrorMessage, DT.Domain.ServiceType>(serviceTypeId.Value, () => GetByIdCoreAsync(serviceTypeId));

    internal static MemoryCacheCounts GetCacheCount() =>
        MemoryCacheStatisticsMapper.FromDualLayerMemoryWrapperCacheCounts(MemoryCacheEntity.ServiceType, dualLayerMemoryCacheWrapper.GetCounts());
}