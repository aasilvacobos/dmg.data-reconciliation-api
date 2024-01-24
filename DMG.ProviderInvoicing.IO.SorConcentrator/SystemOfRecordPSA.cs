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
internal static class SystemOfRecordPsa
{
    private static readonly DualLayerMemoryCacheWrapper DualLayerMemoryCacheWrapper = DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursPSA(), Host.HostConfiguration.GetSorConcentratorCacheTimeoutInHoursPSA()*4);

    private static async Task<Either<ErrorMessage, DMG.Proto.ProviderAgreements.V2.Agreement>> GetByIdCoreAsync(PsaId psaId) =>
        (await SorConcentratorClient.GetByIdAsync<DMG.Proto.ProviderAgreements.V2.Agreement>(psaId.Value, SorEntityName.ProviderServiceAgreement))
        .MapLeft(errorMessage =>
            errorMessage switch
            {
                ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.ProviderServiceAgreementNotFound, // convert generic error message to entity specific message
                _ => errorMessage
            });

    internal static Task<Either<ErrorMessage, DMG.Proto.ProviderAgreements.V2.Agreement>> GetByIdAsync(PsaId psaId) =>
        DualLayerMemoryCacheWrapper.GetOrUpdateAsync<Guid, ErrorMessage, DMG.Proto.ProviderAgreements.V2.Agreement>(psaId.Value, () => GetByIdCoreAsync(psaId));

    internal static MemoryCacheCounts GetCacheCount() =>
        MemoryCacheStatisticsMapper.FromDualLayerMemoryWrapperCacheCounts(MemoryCacheEntity.ProviderServiceAgreement, DualLayerMemoryCacheWrapper.GetCounts());
}