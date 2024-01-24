using DMG.ProviderInvoicing.DT.Domain;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatisticsMapper;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatistics;

namespace DMG.ProviderInvoicing.IO.SorConcentrator;

/// <summary>
/// API consumed by other I/O adapters to retrieve catalog item from the SOR.
/// </summary>
internal static class SystemOfRecordCatalogItem
{
    private static DualLayerMemoryCacheWrapper dualLayerMemoryCacheWrapper = DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer(), Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer()*4);

    private static async Task<Either<ErrorMessage, DT.Domain.CatalogItem>> GetByIdCoreAsync(CatalogItemId catalogItemId) =>
        (await SorConcentratorClient.GetByIdAsync<DMG.ItemCatalog.CatalogItem>(catalogItemId.Value, SorEntityName.CatalogItem))
        .Map(CatalogItemMessageMapper.ToEntity)
        .MapLeft(errorMessage =>
            errorMessage switch
            {
                ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.CatalogItemNotFound, // convert generic error message to entity specific message
                _ => errorMessage
            });

    internal static Task<Either<ErrorMessage, DT.Domain.CatalogItem>> GetByIdAsync(CatalogItemId catalogItemId) =>
        dualLayerMemoryCacheWrapper.GetOrUpdateAsync<Guid, ErrorMessage, DT.Domain.CatalogItem>(catalogItemId.Value, () => GetByIdCoreAsync(catalogItemId));

    internal static Task<Lst<Either<ErrorMessage, DT.Domain.CatalogItem>>> GetByIdsAsync(Lst<CatalogItemId> catalogItemIds) =>
        catalogItemIds
            .Map(GetByIdAsync)
            .Traverse(either => either); // converts Lst<Task> to Task<Lst>
    
    internal static MemoryCacheCounts GetCacheCount() =>
        MemoryCacheStatisticsMapper.FromDualLayerMemoryWrapperCacheCounts(MemoryCacheEntity.CatalogItem, dualLayerMemoryCacheWrapper.GetCounts());
}