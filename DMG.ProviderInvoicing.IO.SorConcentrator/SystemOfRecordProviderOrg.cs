using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatistics;

namespace DMG.ProviderInvoicing.IO.SorConcentrator;

public static class SystemOfRecordProviderOrg
{
    private static readonly DualLayerMemoryCacheWrapper DualLayerMemoryCacheWrapper = 
        DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(
            Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursProviderOrg(),
            Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursProviderOrg() * 4);

    private static async Task<Either<ErrorMessage, DT.Domain.ProviderOrg>> GetByIdCoreAsync(ProviderOrgId providerOrgId) =>
        (await SorConcentratorClient.GetByIdAsync<Dmg.Providers.V1.ProviderOrg>(providerOrgId.Value, SorEntityName.ProviderOrg))
        .Map(providerOrgMessage =>  ProviderOrgMessageMapper.ToEntity(providerOrgMessage, providerOrgId))
        .MapLeft(errorMessage =>
            errorMessage switch
            {
                ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.ProviderOrgNotFound, // convert generic error message to entity specific message
                _ => errorMessage
            });

    internal static Task<Either<ErrorMessage, DT.Domain.ProviderOrg>> GetByIdAsync(ProviderOrgId providerOrgId) =>
        DualLayerMemoryCacheWrapper.GetOrUpdateAsync(providerOrgId.Value, () => GetByIdCoreAsync(providerOrgId));

    internal static MemoryCacheCounts GetCacheCount() =>
        MemoryCacheStatisticsMapper.FromDualLayerMemoryWrapperCacheCounts(MemoryCacheEntity.ProviderOrg, DualLayerMemoryCacheWrapper.GetCounts());
}