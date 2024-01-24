using DMG.ProviderInvoicing.DT.Domain;
using DMG.Framework.DotNet.Grpc.Client;
using Google.Protobuf;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.SORConcentrator;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.IO.Host;

namespace DMG.ProviderInvoicing.IO.SorConcentrator.Common;

internal static class SorConcentratorClient
{
    private static readonly object LockObject = new object();

    private static Option<GetByIdRpc.GetByIdRpcClient> GetClient() 
    { 
        try
        {
            return Optional(GrpcClientBuilder.CreateClient<GetByIdRpc.GetByIdRpcClient>(Host.HostConfiguration.GetConfigRoot(), HostConfiguration.GetSorConcentratorApiServiceName()));
        }
        catch(Exception ex)
        {
            IoAdapterLogger.Exception(ex, "There was an error creating the SOR Concentrator Client.");
            return None;
        }
    }

    //TODO Once the ingress rules are fixed we can move to this type of connection test
    //public static void TestConnection() =>
    //    Utility.Rpc.RpcUtility.TestConnection(HostConfiguration.GetSorConcentratorApiServiceName());

    internal static async Task TestConnectionsAsync()
    {
        IoAdapterLogger.Info("Testing SOR Concentrator topic connections...");
        await TestTopicConnectionAsync<Dmg.Work.V1.Work>(SorEntityName.Work);
        await TestTopicConnectionAsync<Dmg.Work.Billing.V1.JobBillingData>(SorEntityName.JobBilling);
        await TestTopicConnectionAsync<DMG.TicketBilling.TicketBilling>(SorEntityName.TicketBilling);
        await TestTopicConnectionAsync<Dmg.Tickets.V1.Ticket>(SorEntityName.Ticket);
        await TestTopicConnectionAsync<DMG.Proto.Customers.Customer>(SorEntityName.Customer);
        await TestTopicConnectionAsync<DMG.Proto.Properties.Property>(SorEntityName.Property);
        await TestTopicConnectionAsync<Dmg.Providers.V1.ProviderOrg>(SorEntityName.ProviderOrg);
        await TestTopicConnectionAsync<Dmg.Usersor.User>(SorEntityName.User);
        await TestTopicConnectionAsync<DMG.DataServices.ServiceLine>(SorEntityName.ServiceLine);
        await TestTopicConnectionAsync<DMG.DataServices.ServiceType>(SorEntityName.ServiceType);
        await TestTopicConnectionAsync<DMG.ItemCatalog.CatalogItem>(SorEntityName.CatalogItem);
    }

    internal static async void TestConnections() =>
        await TestConnectionsAsync();
    
    private static async Task TestTopicConnectionAsync<TProtoBufMessage>(SorEntityName sorEntityName) where TProtoBufMessage : class, IMessage, new()
    {
        (await GetByIdAsync<TProtoBufMessage>(Guid.Empty, sorEntityName))
            .IfLeft( // A Right (success) should never happen since we are sending an empty Guid.
                errorMessage =>
                {
                    var topicName = SorConcentratorConfiguration.GetSorTopicName(sorEntityName).Value;
                    Action logAction =
                        errorMessage is ErrorMessage.SorConcentratorTopicConnectionFailure
                            ? () => IoAdapterLogger.Emergency($"SOR Concentrator topic {topicName} connection test failure. {errorMessage.ToText()}")
                            : () => IoAdapterLogger.Info($"SOR Concentrator topic {topicName} connection test successful.");
                    logAction.Invoke();
                });
    }

    /// <summary>
    /// Load an SOR record from the concentrator.
    /// </summary>
    /// <param name="id">the primary key of the SOR record</param>
    /// <param name="sorEntityName">identifier for entity</param>
    /// <returns>None if the record isn't found</returns>
    internal static Task<Either<ErrorMessage, TProtoBufMessage>> GetByIdAsync<TProtoBufMessage>(Guid id, SorEntityName sorEntityName) where TProtoBufMessage : class, IMessage, new()
    {
        var sorTopicName = SorConcentratorConfiguration.GetSorTopicName(sorEntityName)!;
        var sorName = sorTopicName.Value!;   // F# can't return a C# (non) nullable reference type 

        Option<GetByIdRpc.GetByIdRpcClient> client = GetClient();

        return client.MatchAsync(async x =>
        {
            try
            {
                //SorConcentratorLogger.Debug($"Calling SOR Concentrator for {sorName}/{id}");

                // build the request and load the object
                var request = new GetByIdRequest { Id = id.ToString(), SorName = sorName };
                var response = await x.GetByIdAsync(request);

                // if we got a response and it contains an object
                if (response is { Success: true })
                {
                    IoAdapterLogger.Debug($"Retrieved SOR Concentrator entity: sorName:{sorName}, id:{id}");
                    var protoBufMessage = new TProtoBufMessage();
                    protoBufMessage.MergeFrom(response.ObjectAsBytes);
                    var verifyBytes = protoBufMessage.ToByteString();
                    if (!verifyBytes.Equals(response.ObjectAsBytes))
                        IoAdapterLogger.Warning($"Invalid protobuf bytes {sorEntityName}/{sorName}/{id}");

                    return Right<ErrorMessage, TProtoBufMessage>(protoBufMessage!);
                }
                else
                {
                    var errorMessage = ErrorMessage.NewSorConcentratorEntityNotFound(id, sorName);
                    if (id != Guid.Empty)
                        IoAdapterLogger.Error(errorMessage.ToText());
                    return Left(errorMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ErrorMessage.NewSorConcentratorTopicConnectionFailure(sorName, ex.Message);
                IoAdapterLogger.Exception(ex, errorMessage.ToText());
                return Left(errorMessage);
            }
        },
        () => Left(ErrorMessage.SorConcentratorClientNotFound));
    }

    /// <summary>
    /// Load an SOR record from the concentrator.
    /// </summary>
    /// <param name="id">the primary key of the SOR record</param>
    /// <param name="sorEntityName">identifier for entity</param>
    /// <returns>None if the record isn't found</returns>
    internal static Either<ErrorMessage, TProtoBufMessage> GetById<TProtoBufMessage>(Guid id, SorEntityName sorEntityName) where TProtoBufMessage : class, IMessage, new()
    {
        var sorTopicName = SorConcentratorConfiguration.GetSorTopicName(sorEntityName)!;
        var sorName = sorTopicName.Value!;   // F# can't return a C# (non) nullable reference type 

        Option<GetByIdRpc.GetByIdRpcClient> client = GetClient();

        return client.Match(x =>
        {
            try
            {
                //SorConcentratorLogger.Debug($"Calling SOR Concentrator for {sorName}/{id}");

                // build the request and load the object
                var request = new GetByIdRequest { Id = id.ToString(), SorName = sorName };
                var response = x.GetById(request);

                // if we got a response and it contains an object
                if (response is { Success: true })
                {
                    IoAdapterLogger.Debug($"Retrieved SOR Concentrator entity: sorName:{sorName}, id:{id}");
                    var protoBufMessage = new TProtoBufMessage();
                    protoBufMessage.MergeFrom(response.ObjectAsBytes);
                    var verifyBytes = protoBufMessage.ToByteString();
                    if (!verifyBytes.Equals(response.ObjectAsBytes))
                        IoAdapterLogger.Warning($"Invalid protobuf bytes {sorEntityName}/{sorName}/{id}");

                    return Right<ErrorMessage, TProtoBufMessage>(protoBufMessage!);
                }
                else
                {
                    var errorMessage = ErrorMessage.NewSorConcentratorEntityNotFound(id, sorName);
                    if (id != Guid.Empty)
                        IoAdapterLogger.Error(errorMessage.ToText());
                    return Left(errorMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ErrorMessage.NewSorConcentratorTopicConnectionFailure(sorName, ex.Message);
                IoAdapterLogger.Exception(ex, errorMessage.ToText());
                return Left(errorMessage);
            }
        },
        () => Left(ErrorMessage.SorConcentratorClientNotFound));
    }
}