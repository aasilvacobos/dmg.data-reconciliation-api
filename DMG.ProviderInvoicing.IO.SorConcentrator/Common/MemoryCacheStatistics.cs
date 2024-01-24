using DMG.ProviderInvoicing.BL.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatistics;

namespace DMG.ProviderInvoicing.IO.SorConcentrator.Common;

public static class MemoryCacheStatistics
{
    public record MemoryCacheCounts(MemoryCacheEntity MemoryCacheEntity, int ShortCacheCount, int LongCacheCount);
    //public record MemoryCacheSizes(NonEmptyText CacheName, long ShortCacheSize, long LongCacheSize);
}

public static class MemoryCacheStatisticsMapper
{
    public static MemoryCacheCounts FromDualLayerMemoryWrapperCacheCounts(MemoryCacheEntity memoryCacheEntity, DualLayerMemoryCacheWrapper.Counts counts) =>
        new MemoryCacheCounts(memoryCacheEntity, counts.ShortCacheCount, counts.LongCacheCount);

    //public static MemoryCacheSizes FromDualLayerMemoryWrapperCacheSizes(NonEmptyText cacheName, DualLayerMemoryCacheWrapper.Sizes sizes) =>
    //  new MemoryCacheSizes(cacheName, sizes.ShortCacheSize, sizes.LongCacheSize);
}

public enum MemoryCacheEntity
{
    CatalogItem,
    Customer,
    Property,
    ProviderOrg,
    ServiceLine,
    ServiceType,
    User,
    ProviderServiceAgreement
}
