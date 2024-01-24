namespace DMG.ProviderInvoicing.IO.Host

open System
open System.Collections.Concurrent
open DMG.ProviderInvoicing.DT.Service

module internal CacheIO =
    /// Memoize non-deterministic function with a timeout in seconds. A caller identifier parameter is necessary
    /// to uniquely identify the function and its respective argument value.
    ///
    /// Constraining memoized function to HostSetting argument to fast track the time out functionality.
    /// This can be made more generic if necessary.
    let memoizeWithTimeOut timeOutInSeconds (fn: HostEnvironmentVariable -> 'b) =
        let cacheTimes = ConcurrentDictionary<HostEnvironmentVariable, DateTime>()
        let cache = ConcurrentDictionary<HostEnvironmentVariable, 'b>()

        fun (key: HostEnvironmentVariable) ->
            match cacheTimes.TryGetValue key with
            | true, time when time < DateTime.UtcNow.AddSeconds(-timeOutInSeconds) -> cache.TryRemove(key) |> ignore
            | _ -> ()

            cache.GetOrAdd(
                key,
                fun key ->
                    cacheTimes.AddOrUpdate(key, DateTime.UtcNow, (fun _ _ -> DateTime.UtcNow))
                    |> ignore

                    fn (key)
            )

    /// Memoize a non-deterministic function.
    let memoize (fn: 'a -> 'b) =
        let cache = ConcurrentDictionary<'a, 'b>()

        fun (key: 'a) ->
            match cache.TryGetValue key with
            | true, value -> value
            | false, _ ->
                let value = fn key
                cache.TryAdd(key, value) |> ignore
                //#if DEBUG
//                printfn $"CacheIO.memoize memoized %A{key} %A{value}"
//#endif
                value
