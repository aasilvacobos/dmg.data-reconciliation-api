using DMG.Framework.DotNet.Grpc.Client;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.IO.Host;
using Grpc.Core;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.Framework.DotNet.Grpc.Client.Configuration;
using System.Xml.Linq;
using Grpc.Net.Client;
using System;
//using Grpc.Health.V1;
using LanguageExt.Pipes;
//using static Grpc.Health.V1.Health;

namespace DMG.ProviderInvoicing.IO.Utility.Rpc;

public static class RpcUtility
{
    public static ErrorMessage BuildUnhandledRpcErrorMessage(RpcException ex, string rpcName, string messageSupplement)
    {
        UtilityLogger.Error(ErrorMessage.NewUndefinedError($"RPC error message not handled in {rpcName}. Message: {ex.Message}. RPC Status code: {ex.StatusCode.ToString()}. {messageSupplement}").ToText());
        // message that we expect UI to see:
        var userErrorMessage = ErrorMessage.NewUndefinedError($"Message: {ex.Message}. Status Code: {ex.StatusCode.ToString()}. {messageSupplement}");
        return userErrorMessage;
    }

    public static void TestConnection<T>(string ioAdapterName, Func<Option<T>> functionToGetClient, Action<T> actionToTestConnection)
    {
        Option<T> client = functionToGetClient();

        client.Match(x =>
        {
            try
            {
                actionToTestConnection(x);
                UtilityLogger.Info($"{ioAdapterName} connection test successful.");
            }
            catch (RpcException ex)
            {
                switch (ex.StatusCode)
                {
                    case StatusCode.Unimplemented:
                        UtilityLogger.Info($"{ioAdapterName} connection test connected but received an unimplemented exception.");
                        break;
                    default:
                        UtilityLogger.Emergency($"{ioAdapterName} connection test failure. {ex.Message}");
                        break;
                }
            }
            catch (Exception ex)
            {
                UtilityLogger.Emergency($"{ioAdapterName} connection test failure. {ex.Message}");
            }
        },
        () => UtilityLogger.Emergency($"{ioAdapterName} could not create the API client."));
    }

    //TODO The Ingress rules need to be fixed before we can do this.  Currently this pings a random services because the ingress rules are not working correctly.
    //public static async void TestConnection(string ioAdapterName)
    //{
    //    try
    //    {
    //        var configuration = HostConfiguration.GetConfigRoot();
    //        var grpcClientConfigsOption =  Optional(configuration.GetClientConfigs("Grpc:Clients"));
    //        var grpcClientConfigOption = grpcClientConfigsOption.Select(config => config.Single(c => string.Equals(c.Name, ioAdapterName, StringComparison.OrdinalIgnoreCase)));
    //        var urlOption = grpcClientConfigOption.Select(config => config.Uri);
    //        var grpcChannelOption = urlOption.Select(url => GrpcChannel.ForAddress(url));
    //        var healthClientOption = grpcChannelOption.Select(grpcChannel => new Health.HealthClient(grpcChannel));
    //        var healthCheckResponseOption = healthClientOption.Select(PreformHealthCheck);
    //        healthCheckResponseOption.Match(x => Logger.Info($"{ioAdapterName} connection test successful. {x.Status}"),
    //                                        () => Logger.Emergency($"{ioAdapterName} connection test failure."));
    //    }
    //    catch (RpcException ex)
    //    {
    //        switch (ex.StatusCode)
    //        {
    //            case StatusCode.Unimplemented:
    //                Logger.Info($"{ioAdapterName} connection test connected but received an unimplemented exception.");
    //                break;
    //            default:
    //                Logger.Emergency($"{ioAdapterName} connection test failure. {ex.Message}");
    //                break;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Logger.Emergency($"{ioAdapterName} connection test failure. {ex.Message}");
    //    }
    //}

    //private static HealthCheckResponse PreformHealthCheck(HealthClient healthClient)
    //{
    //    return healthClient.Check(new HealthCheckRequest());
    //}
}