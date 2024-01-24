namespace global

open System
open System.Collections.Concurrent

module Cache =
    /// Memoize function with a timeout in seconds. A caller identifier parameter is necessary
    /// to uniquely identify the function and its respective argument value.
    let memoizeWithTimeout cacheTimeSeconds (callerIdentifier: string) (fn: 'a -> 'b) =
        let cacheTimes = ConcurrentDictionary<string, DateTime>()
        let cache = ConcurrentDictionary<'a, 'b>()

        fun (key: 'a) ->
            match cacheTimes.TryGetValue callerIdentifier with
            | true, time when time < DateTime.UtcNow.AddSeconds(-cacheTimeSeconds) -> cache.TryRemove(key) |> ignore
            | _ -> ()

            cache.GetOrAdd(
                key,
                fun key ->
                    cacheTimes.AddOrUpdate(callerIdentifier, DateTime.UtcNow, (fun _ _ -> DateTime.UtcNow))
                    |> ignore

                    fn (key)
            )

    let memoize (fn: 'a -> 'b) =
        let cache = ConcurrentDictionary<'a, 'b>()

        (fun key ->
            match cache.TryGetValue key with
            | true, value -> value
            | false, _ ->
                let value = fn key
                cache.TryAdd(key, value) |> ignore
                value)
