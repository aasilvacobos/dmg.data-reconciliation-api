using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using static DMG.ProviderInvoicing.IO.SorConcentrator.Common.MemoryCacheStatistics;

namespace DMG.ProviderInvoicing.IO.SorConcentrator;

/// <summary>
/// API consumed by other I/O adapters to retrieve user from the SOR Concentrator.
/// </summary>
internal static class SystemOfRecordUser
{
    private static DualLayerMemoryCacheWrapper dualLayerMemoryCacheWrapper = DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursUser(), Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursUser()*4);

    private static async Task<Either<ErrorMessage, DT.Domain.User>> GetByIdCoreAsync(UserId userId) =>
        (await SorConcentratorClient.GetByIdAsync<Dmg.Usersor.UserState>(userId.Value, SorEntityName.User))
        .Bind(x => UserMessageMapper.TryToEntity(x.CurrentValue).ToEither(ErrorMessage.ResourceNotFound))
        .MapLeft(errorMessage =>
            errorMessage switch
            {
                ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.UserNotFound, // convert generic error message to entity specific message
                _ => errorMessage
            });

    internal static Task<Either<ErrorMessage, DT.Domain.User>> GetByIdAsync(UserId userId) =>
        dualLayerMemoryCacheWrapper.GetOrUpdateAsync<Guid, ErrorMessage, DT.Domain.User>(userId.Value, () => GetByIdCoreAsync(userId));

    internal static MemoryCacheCounts GetCacheCount() =>
        MemoryCacheStatisticsMapper.FromDualLayerMemoryWrapperCacheCounts(MemoryCacheEntity.User, dualLayerMemoryCacheWrapper.GetCounts());
}