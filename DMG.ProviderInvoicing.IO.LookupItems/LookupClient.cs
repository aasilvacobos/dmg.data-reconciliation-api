using DMG.DataServices;
using DMG.Framework.DotNet.Grpc.Client;
using DMG.ProviderInvoicing.IO.Host;
using Grpc.Core;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using LanguageExt;
using static LanguageExt.Prelude;
using LookupDataSetCore = DMG.ProviderInvoicing.DT.Domain.LookupDataSetCore;
using DMG.ProviderInvoicing.IO.Logging;

namespace DMG.ProviderInvoicing.IO.LookupItems;

/// <summary>
/// The wrapper for the gRPC client for lookup items.
/// </summary>
public static class LookupClient
{
    private static readonly Map<int, DualLayerMemoryCacheWrapper> MemoryCaches = Map<int, DualLayerMemoryCacheWrapper>();
    private const string IoAdapterName = @"Lookup";

    static LookupClient()
    {
        foreach (var item in Enum.GetValues(typeof(LookupDataSetType)))
        {
            switch (item)
            {
                case LookupDataSetType.Index:
                    //we didn't want to cache index
                    break;
                case LookupDataSetType.State:
                    MemoryCaches = MemoryCaches.Add((int)item, DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(
                    Host.HostConfiguration.GetLookupCacheTimeoutInHoursState(),
                    Host.HostConfiguration.GetLookupCacheTimeoutInHoursState() * 30));
                    break;

                case LookupDataSetType.PaymentTerm:
                    MemoryCaches = MemoryCaches.Add((int)item, DualLayerMemoryCacheWrapper.CreateWithTimeoutInHours(
                    Host.HostConfiguration.GetLookupCacheTimeoutInHoursPaymentTerm(),
                    Host.HostConfiguration.GetLookupCacheTimeoutInHoursPaymentTerm() * 30));
                    break;
                default:
                    //additionally we could create a default cache here so we wouldn't have to lookup each time below
                    Logger.Error($"Missing configuration for lookup cache type '{item}' and will not be cached.");
                    break;
            }
        }
    }



    private static LookupDataSetId ToLookupDataSetId(LookupDataSetType lookupDataSetType) =>
        new LookupDataSetId((int)lookupDataSetType);

    private static Option<LookupDataSetRpc.LookupDataSetRpcClient> TryGetClient()
    {
        try
        {
            return Optional(GrpcClientBuilder.CreateClient<LookupDataSetRpc.LookupDataSetRpcClient>(HostConfiguration.GetConfigRoot(), HostConfiguration.GetLookupItemsServiceName()));
        }
        catch (Exception ex)
        {
            IoAdapterLogger.Exception(ex, "There was an error creating the lookup items RPC client.");
            return Option<LookupDataSetRpc.LookupDataSetRpcClient>.None;
        }
    }

    /// <summary>
    /// Function to test the connection to the gRPC service.
    /// </summary>
    /// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
    /// TODO rework this to actually ping the service instead of retrieving the state data set
    internal static bool Ping()
    {
        try
        {
            Option<LookupDataSetRpc.LookupDataSetRpcClient>  lookupItemsRpcClient = TryGetClient();

            return lookupItemsRpcClient.Match(x =>
            {
                x.GetLookupDataSet(new GetLookupDataSetRequest
                {
                    DataSetsRequested = { new DataSetRequest { DataSetId = 1 } }
                });
                IoAdapterLogger.Info("LookupItems ping test successful.");

                return true;
            },
            () =>
            {
                IoAdapterLogger.Emergency("The look up items RPC client is not available.");

                return false;
            });
        }
        catch (RpcException rEx)
        {
            switch (rEx.StatusCode)
            {
                case StatusCode.Unimplemented:
                    IoAdapterLogger.Info("LookupItems ping test connected but received an unimplemented exception. Fulfillment appears to be up.");
                    return true;
                default:
                    IoAdapterLogger.Emergency($"LookupItems ping test failure. {rEx.Message}");
                    break;
            }

            return false;
        }
        catch (Exception ex)
        {
            IoAdapterLogger.Emergency($"LookupItems ping test failure. {ex.Message}");
            return false;
        }
    }

    /// Build RPC request for a specific lookup item data set 
    private static GetLookupDataSetCoreRequest BuildRequest(int lookupDataSetTypeValue) =>
        new ()
        {
            DataSetsRequested =
            {
                new DataSetRequest
                {
                    DataSetId = lookupDataSetTypeValue
                }
            },
        };

    /// Retrieve a LookupDataSetCore found by data set id
    private static Option<DT.Domain.LookupDataSetCore> TryGetLookupDataSetCoreInternal(LookupDataSetId lookupDataSetId)
    {
        var lookupDataSetRpcClientOption = TryGetClient();
        lookupDataSetRpcClientOption.IfNone(() => IoAdapterLogger.Error(ErrorMessage.NewIoAdapterClientNotFound(IoAdapterName).ToText()));

        var responseOption = lookupDataSetRpcClientOption
            .Map(lookupDataSetRpcClient => lookupDataSetRpcClient.GetLookupDataSetCore(BuildRequest(lookupDataSetId.Value)));
        responseOption.IfNone(() => IoAdapterLogger.Error($"Lookup item data set {lookupDataSetId.Value.ToString()} could not be retrieved."));

        var lookupDataSetCoreOption = responseOption
            .Map(x => x.DataSets.Freeze().Map(LookupDataSetCoreMessageMapper.ToEntity))
            .Bind(lookupDataSetCores =>
                lookupDataSetCores.Count > 0
                    ? Option<DT.Domain.LookupDataSetCore>.Some(lookupDataSetCores[0])
                    : Option<DT.Domain.LookupDataSetCore>.None);

        lookupDataSetCoreOption
            .Iter(lookupDataSetCore =>
            {
                var message = $"Retrieved lookup data set {lookupDataSetCore.DataSetId.Value} with {lookupDataSetCore.Items.Count} items.";
                if (lookupDataSetCore.Items.Count > 0)
                    IoAdapterLogger.Info(message);
                else
                    IoAdapterLogger.Error(message);
            });

        return lookupDataSetCoreOption;
    }

    /// Retrieve index of all lookup item data sets
    internal static Option<LookupDataSetCore> TryGetLookupDataSetCoreIndex() =>
        TryGetLookupDataSetCoreInternal(ToLookupDataSetId(LookupDataSetType.Index));

    /// Retrieve a lookup data set
    internal static Option<DT.Domain.LookupDataSetCore> TryGetLookupDataSetCore(LookupDataSetType lookupDataSetType) =>
        lookupDataSetType
            switch
        {
            // redirect 
            LookupDataSetType.Index => TryGetLookupDataSetCoreIndex(),
            LookupDataSetType.State => TryGetValuesFromCache(ToLookupDataSetId(lookupDataSetType)),
            LookupDataSetType.PaymentTerm => TryGetValuesFromCache(ToLookupDataSetId(lookupDataSetType)),
            _ => Option<LookupDataSetCore>.None
        };

    private static Option<LookupDataSetCore> TryGetValuesFromCache(LookupDataSetId lookupDataSetId)
    {
        Option<DualLayerMemoryCacheWrapper> dualLayerMemoryCacheWrappersOptional = MemoryCaches.Find(lookupDataSetId.Value);

        return dualLayerMemoryCacheWrappersOptional.Match(
            dualLayerMemoryCacheWrapper => dualLayerMemoryCacheWrapper.GetOrUpdate(
                lookupDataSetId.Value,
                () => TryGetLookupDataSetCoreInternal(lookupDataSetId)
                        .ToEither(ErrorMessage.NewUndefinedError("This doesn't matter as it gets swallowed below with the conversion to an option.")))
                        .ToOption(),
            () =>
            {
                Logger.Error($"Unable to find memory cache for lookupDateSetId {lookupDataSetId.Value}.");
                return TryGetLookupDataSetCoreInternal(lookupDataSetId)
                        .ToEither(ErrorMessage.NewUndefinedError("This doesn't matter as it gets swallowed below with the conversion to an option."))
                        .ToOption();
            });
    }

    /// Return the value for a name (i.e., key) for an given lookup item data set
    internal static Option<NonEmptyText> TryFindValueByName(LookupDataSetType lookupDataSetType, NonEmptyText name) =>
        TryGetLookupDataSetCore(lookupDataSetType)
            .Bind(lookupDataSetCore => lookupDataSetCore.Items.Find(lookupItemCore => lookupItemCore.Name == name))
            .Map(lookupItemCore => lookupItemCore.Value);
}