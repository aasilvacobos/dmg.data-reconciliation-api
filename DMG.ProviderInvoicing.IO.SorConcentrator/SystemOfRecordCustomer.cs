using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatistics;

namespace DMG.ProviderInvoicing.IO.SorConcentrator;

/// <summary>
/// API consumed by other I/O adapters to retrieve customer from the SOR.
/// </summary>
internal static class SystemOfRecordCustomer
{
    private static DualLayerMemoryCacheWrapper dualLayerMemoryCacheWrapper = DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer(), Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer()*4);

    private static async Task<Either<ErrorMessage, DT.Domain.Customer>> GetByIdCoreAsync(CustomerId customerId) =>
        (await SorConcentratorClient.GetByIdAsync<DMG.Proto.Customers.Customer>(customerId.Value, SorEntityName.Customer))
        .Map(CustomerMessageMapper.ToEntity)
        .MapLeft(errorMessage =>
            errorMessage switch
            {
                ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.CustomerNotFound, // convert generic error message to entity specific message
                _ => errorMessage
            });

    internal static Task<Either<ErrorMessage, DT.Domain.Customer>> GetByIdAsync(CustomerId customerId) =>
        dualLayerMemoryCacheWrapper.GetOrUpdateAsync<Guid, ErrorMessage, DT.Domain.Customer>(customerId.Value, () => GetByIdCoreAsync(customerId));

    internal static MemoryCacheCounts GetCacheCount() =>
        MemoryCacheStatisticsMapper.FromDualLayerMemoryWrapperCacheCounts(MemoryCacheEntity.Customer, dualLayerMemoryCacheWrapper.GetCounts());
}