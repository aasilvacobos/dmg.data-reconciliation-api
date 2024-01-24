using LanguageExt;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatistics;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.IO.SorConcentrator.Common;

internal static class MemoryCache
{
    private static MemoryCacheCounts GetCount(MemoryCacheEntity item)
    {
        switch (item)
        {
            case MemoryCacheEntity.CatalogItem:
                return SystemOfRecordCatalogItem.GetCacheCount();
            case MemoryCacheEntity.Customer:
                return SystemOfRecordCustomer.GetCacheCount();
            case MemoryCacheEntity.Property:
                return SystemOfRecordProperty.GetCacheCount();
            case MemoryCacheEntity.ProviderOrg:
                return SystemOfRecordProviderOrg.GetCacheCount();
            case MemoryCacheEntity.ServiceLine:
                return SystemOfRecordServiceLine.GetCacheCount();
            case MemoryCacheEntity.ServiceType:
                return SystemOfRecordServiceType.GetCacheCount();
            case MemoryCacheEntity.User:
            default:
                return SystemOfRecordUser.GetCacheCount();
        }
    }

    private static Lst<MemoryCacheCounts> GetSpecificCounts(Lst<MemoryCacheEntity> memoryCacheTypes) =>
        memoryCacheTypes.Map(GetCount).ToList().Freeze();
    
    internal static Lst<MemoryCacheCounts> GetAllCounts() =>
        MemoryCache.GetSpecificCounts(List<MemoryCacheEntity>(
            MemoryCacheEntity.CatalogItem,
            MemoryCacheEntity.Customer,
            MemoryCacheEntity.Property,
            MemoryCacheEntity.ProviderOrg,
            MemoryCacheEntity.ServiceLine,
            MemoryCacheEntity.ServiceType,
            MemoryCacheEntity.User));
}