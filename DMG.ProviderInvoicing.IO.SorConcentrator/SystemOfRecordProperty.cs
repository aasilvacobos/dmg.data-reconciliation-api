using DMG.ProviderInvoicing.DT.Domain;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using DMG.ProviderInvoicing.IO.LookupItems;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatistics;

namespace DMG.ProviderInvoicing.IO.SorConcentrator;

/// <summary>
/// API consumed by other I/O adapters to retrieve property from the SOR Concentrator.
/// </summary>
internal static class SystemOfRecordProperty
{
    private static DualLayerMemoryCacheWrapper dualLayerMemoryCacheWrapper = DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer(), Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursCustomer()*4);

    private static async Task<Either<ErrorMessage, DT.Domain.Property>> GetByIdCoreAsync(PropertyId propertyId)
    {
        var propertyMessageEither = await SorConcentratorClient.GetByIdAsync<DMG.Proto.Properties.Property>(propertyId.Value, SorEntityName.Property);

        // Find the servicing address while still in message format
        var propertyAddressServicingMessageOption =
            propertyMessageEither
                .ToOption()
                .Bind(property => PropertyMessageMapper.TryFindPropertyAddressServicingMessage(property.PropertyAddresses));

        // build servicing address entity
        var propertyAddressServicingOption =
            propertyAddressServicingMessageOption
                .Map(PropertyMessageMapper.ToEntity);

        // build property entity
        var propertyEither =
            propertyMessageEither
                .Map(x => PropertyMessageMapper.ToEntity(x, propertyAddressServicingOption))
                .MapLeft(errorMessage =>
                    errorMessage switch
                    {
                        ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.PropertyNotFound, // convert generic error message to entity specific message
                        _ => errorMessage
                    });

        return propertyEither;
    }

    internal static Task<Either<ErrorMessage, DT.Domain.Property>> GetByIdAsync(PropertyId propertyId) =>
        dualLayerMemoryCacheWrapper.GetOrUpdateAsync<Guid, ErrorMessage, DT.Domain.Property>(propertyId.Value, () => GetByIdCoreAsync(propertyId));

    internal static MemoryCacheCounts GetCacheCount() =>
        MemoryCacheStatisticsMapper.FromDualLayerMemoryWrapperCacheCounts(MemoryCacheEntity.Property, dualLayerMemoryCacheWrapper.GetCounts());
}