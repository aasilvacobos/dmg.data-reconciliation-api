module KafkaLib

open System
open System.IO
open System.Collections.Generic
open System.Linq
open System.Text.RegularExpressions

open Confluent.Kafka
open Confluent.Kafka.Admin
open Google.Protobuf

open Uuid
open FSharpPlus
open Focal.Core

let getSecurityProtocol hostAndPort =
    let isPlainText = Regex(":9092$").Match(hostAndPort).Success

    if isPlainText then
        SecurityProtocol.Plaintext
    else
        SecurityProtocol.Ssl

let private readNumberOfPartitions (topicName: string) (hostAndPort: string) : int =
    Confluent
        .Kafka
        .AdminClientBuilder(
            AdminClientConfig(
                BootstrapServers = hostAndPort,
                SecurityProtocol = getSecurityProtocol hostAndPort
            )
        )
        .Build()
        .GetMetadata(TimeSpan.FromSeconds 20)
        .Topics.SingleOrDefault(fun i -> i.Topic = topicName)
        .Partitions
        .Count

let rawReadFromTopic host topic = seq {
    let cConfig = new ConsumerConfig()
    cConfig.BootstrapServers <- host
    cConfig.GroupId <- UUID.New().ToString()
    cConfig.AutoOffsetReset <- AutoOffsetReset.Earliest
    cConfig.SecurityProtocol <- getSecurityProtocol host
    use consumer = ConsumerBuilder<byte[], byte[]>(cConfig).Build()
    consumer.Subscribe([topic])

    let number_of_partitions =
        readNumberOfPartitions topic host

    let maxOffsetLookup =
        Seq.init number_of_partitions id
        |> Seq.map (fun i -> i,(consumer.QueryWatermarkOffsets (TopicPartition(topic, i), TimeSpan.FromMilliseconds 5000)).High.Value - 1L)
        |> dict

    let mutable completedPartitions =
        Array.init number_of_partitions (fun _ -> false)

    let mutable result = consumer.Consume()
    let mutable offset = result.Offset.Value
    let key = result.Message.Key
    let value = result.Message.Value
    let outVal = if value <> null then Some value else None
    yield (key, outVal)
    while completedPartitions |> Array.exists(fun x->not x) do
        result <- consumer.Consume()

        if maxOffsetLookup[result.TopicPartition.Partition.Value] = result.Offset.Value then
            completedPartitions[result.TopicPartition.Partition.Value] <- true
        offset <- result.Offset.Value
        let key = result.Message.Key
        let value = result.Message.Value
        let outVal = if value <> null then Some value else None
        yield (key, outVal)
    consumer.Close() }
    

let rawReadFromTopicWithSnp host topic = seq {
    let cConfig = new ConsumerConfig()
    cConfig.BootstrapServers <- host
    cConfig.GroupId <- UUID.New().ToString()
    cConfig.AutoOffsetReset <- AutoOffsetReset.Earliest
    cConfig.SecurityProtocol <- getSecurityProtocol host
    use consumer = ConsumerBuilder<byte[], byte[]>(cConfig).Build()
    consumer.Subscribe([topic])

    let number_of_partitions =
        readNumberOfPartitions topic host

    let maxOffsetLookup =
        Seq.init number_of_partitions id
        |> Seq.map (fun i -> i,(consumer.QueryWatermarkOffsets (TopicPartition(topic, i), TimeSpan.FromMilliseconds 5000)).High.Value - 1L)
        |> dict

    let mutable completedPartitions =
        Array.init number_of_partitions (fun _ -> false)

    let mutable result = consumer.Consume()
    let mutable offset = result.Offset.Value
    let key = result.Message.Key
    let value = result.Message.Value
    let outVal = if value <> null then Some value else None
    yield (key, outVal, (result.Topic, result.TopicPartition, result.Offset))
    while completedPartitions |> Array.exists(fun x->not x) do
        result <- consumer.Consume()

        if maxOffsetLookup[result.TopicPartition.Partition.Value] = result.Offset.Value then
            completedPartitions[result.TopicPartition.Partition.Value] <- true
        offset <- result.Offset.Value
        let key = result.Message.Key
        let value = result.Message.Value
        let outVal = if value <> null then Some value else None
        yield (key, outVal, (result.Topic, result.TopicPartition, result.Offset))
    consumer.Close() }

let readFromTopic host topic decode = 
    rawReadFromTopic host topic
    |> Seq.map (fun (k, vo) -> 
        try
            (UUID.New k, Option.map decode vo)
        with
        | e -> 
            printfn "Failed to read bytes: (%s, %A)" (Convert.ToHexString k) (Option.map (fun (arr: byte array) -> Convert.ToHexString arr) vo)
            raise e)



let readStateTopic host topic decode =
    readFromTopic host topic decode
    |> Seq.map (fun (kResult, vOpt) -> (Result.map(fun (k:UUID) -> k.GetString) kResult, vOpt))
    |> Seq.fold (fun accResult (kResult, vOpt)  -> 
        kResult
        |> Result.bind (fun k ->
            accResult
            |> Result.map (fun acc -> 
                match vOpt with
                | Some v -> Map.add k v acc
                | None -> Map.remove k acc))) (Ok Map.empty)



type TopicType =
    | EventTopic
    | StateTopic

type ConfigTopic =
    { 
        topic_name: string
        topic_type: TopicType
    }

type Config =
    {
        bootstrap_servers: string
        topics: ConfigTopic array
    }

let setupKafka
    (config: Config)
    =

    let topics =
        config.topics
        |> Array.map (fun topic ->
            let topic_configuration =
                match topic.topic_type with
                | EventTopic -> dict []
                | StateTopic -> dict [ "cleanup.policy", "compact" ]

            //! need a replicationFactor of 2s when wirting to a hosted version of Kafka
            TopicSpecification(
                Name = topic.topic_name,
                ReplicationFactor = 1s,
                NumPartitions = 1,
                Configs = new Dictionary<string, string>(topic_configuration)
            ))

    task {
        use admin_client =
            AdminClientBuilder(new AdminClientConfig(BootstrapServers = config.bootstrap_servers, SecurityProtocol = getSecurityProtocol config.bootstrap_servers))
                .Build()

        try
            let! result = admin_client.CreateTopicsAsync(topics, CreateTopicsOptions())

            return Ok result

        with
        | :? CreateTopicsException as cte ->
            return Result.Error cte
    }

let deleteTopics (bootstrap_servers: string) (topic_names: string array)
    =
    task {
        try
            use admin_client =
                AdminClientBuilder(new AdminClientConfig(BootstrapServers = bootstrap_servers, SecurityProtocol = getSecurityProtocol bootstrap_servers))
                    .Build()
            let! result = admin_client.DeleteTopicsAsync(topic_names, DeleteTopicsOptions())

            return Ok result

        with
        | :? DeleteTopicsException as cte -> 
            printfn "%s" cte.Message
            return Result.Error cte
    }


let listTopics (bootstrap_servers: string) =
    use admin_client =
        AdminClientBuilder(new AdminClientConfig(BootstrapServers = bootstrap_servers, SecurityProtocol = getSecurityProtocol bootstrap_servers))
            .Build()
    let result = admin_client.GetMetadata(TimeSpan.FromSeconds(20))
    result.Topics |> Seq.toList |> List.map (fun x -> x.Topic) |> List.sort

let topicExists (bootstrap_servers: string) topicName =
    use adminClient =
        AdminClientBuilder(new AdminClientConfig(BootstrapServers = bootstrap_servers))
            .Build()
    let meta = adminClient.GetMetadata(TimeSpan.FromSeconds(20))
    meta.Topics.Exists(fun x -> x.Topic = topicName)


let printMetadata bootstrapServers =
    use adminClient =
        AdminClientBuilder(AdminClientConfig(BootstrapServers = bootstrapServers, SecurityProtocol = getSecurityProtocol bootstrapServers))
            .Build()

    let meta = adminClient.GetMetadata(TimeSpan.FromSeconds(20))
    Console.WriteLine($"{meta.OriginatingBrokerId} {meta.OriginatingBrokerName}")

    for broker in meta.Brokers do
        Console.WriteLine($"Broker: {broker.BrokerId} {broker.Host}:{broker.Port}")

    for topic in meta.Topics do
        Console.WriteLine($"Topic: {topic.Topic} {topic.Error} ")
        
        for partition in topic.Partitions do
            let replicas =
                partition.Replicas
                |> Array.map (fun x -> x.ToString())
                |> String.concat ","

            let in_sync_replicas =
                partition.InSyncReplicas
                |> Array.map (fun x -> x.ToString())
                |> String.concat ","

            Console.WriteLine($"  Partition: {partition.PartitionId}")
            Console.WriteLine($"    Replicas: {replicas}")
            Console.WriteLine($"    InSyncReplicas: {in_sync_replicas}")

let writeToTopic bootstrapServer (topicName: string) kvPairs =
    let mutable config = new ProducerConfig()
    config.BootstrapServers <- bootstrapServer
    config.ClientId <- System.Net.Dns.GetHostName()
    config.SecurityProtocol <- getSecurityProtocol bootstrapServer
    use mutable producer: IProducer<byte array, byte array> = (new ProducerBuilder<byte array,byte array>(config)).Build()
    let mutable i = 0

    for (k, vo) in kvPairs do
        let mutable producerMessage = new Message<byte array, byte array>(Key=k)
        match vo with
        | Some v ->
            producerMessage.Value <- v
        | _ -> ()
        producer.Produce(topicName, producerMessage, fun _ -> ())
        i <- i + 1
        if (i % 10000 = 0) then 
            producer.Flush()
    producer.Flush()

let resetTopic bootstrapServer topic =
    task {
        if topicExists bootstrapServer topic.topic_name then
            printfn "deleting topic '%s' on '%s'" topic.topic_name bootstrapServer
            let! result = deleteTopics bootstrapServer [| topic.topic_name |] 
            match result with
            | Ok _ -> ()
            | Error err -> raise err
        printfn "Setting up topic '%s' on '%s'" topic.topic_name bootstrapServer
        let! result = setupKafka ({ bootstrap_servers=bootstrapServer;  topics=[| topic |] })
        match result with
            | Ok _ -> ()
            | Error err -> raise err
    }

let backupToFile bootstrapServer topicName filePath =
    let kvPairs = rawReadFromTopic bootstrapServer topicName
    use writer = new StreamWriter(filePath, append=false)
    for (k, vo) in kvPairs do
        writer.WriteLine $"{Convert.ToHexString k},{vo |> Option.map Convert.ToHexString |> Option.defaultValue String.Empty}"

let backupToFileWithTimestamp bootstrapServer topicName filePath =
    let now = DateTime.Now
    backupToFile bootstrapServer topicName (filePath + $"-{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}")

let readFromFile filePath =
    task {
        let! input = File.ReadAllLinesAsync filePath 
        return
            input
            |> Array.map (fun s -> s.Split(','))
            |> Array.map (fun s -> (s[0], if String.IsNullOrWhiteSpace s[1] then None else Some s[1]))
            |> Array.map (fun (k, vo) -> (Convert.FromHexString k, vo |> Option.map Convert.FromHexString)) }

let restoreFromFile bootstrapServer topic filePath =
    task {
        let! kvPairs = readFromFile filePath
        do! resetTopic bootstrapServer topic
        printfn "transferring messages for topic %s" topic.topic_name
        writeToTopic bootstrapServer topic.topic_name kvPairs
        printfn "all messages copied for topic '%s'" topic.topic_name
    }

let loadFromFile bootstrapServer topic filePath =
    task {
        printfn "Load to topic %s, from file %s." topic.topic_name filePath
        let! kvPairs = readFromFile filePath
        printfn "Transferring messages for topic %s" topic.topic_name
        writeToTopic bootstrapServer topic.topic_name kvPairs
        printfn "All messages Loaded for topic '%s'" topic.topic_name
    }

let backupTopicsToFolder bootstrapServer topics dirPath =
    if Directory.Exists dirPath then
        printfn $"Found existing directory at '{dirPath}'. Deleting..."
        Directory.Delete(dirPath, true)
    Directory.CreateDirectory dirPath |> ignore
    for topic in topics do
        backupToFile bootstrapServer topic (Path.Combine(dirPath, $"{topic}.csv"))

let restoreTopicsFromFolder bootstrapServer (topics: ConfigTopic seq) dirPath =
    task {
        for topic in topics do
            let filePath = Path.Combine(dirPath, $"{topic.topic_name}.csv") 
            do! restoreFromFile bootstrapServer topic filePath |> Async.AwaitTask
    }

let replicate sourceServer targetServer topics =
    task {
        let topicNames = 
            topics
            |> Seq.map (fun x -> x.topic_name)
            |> Seq.toArray

        for topic in topics do            
            let incoming = rawReadFromTopic sourceServer topic.topic_name
            do! resetTopic targetServer topic 
            printfn "transferring messages for topic %s" topic.topic_name
            writeToTopic targetServer topic.topic_name incoming
            printfn "all messages copied for topic '%s'" topic.topic_name
    }

type R<'a> = Microsoft.FSharp.Core.Result<'a,(string*(exn option))>

let inline compareStreamFsgrpcVsCs<'T1,'T2 when 'T1: (static member Proto : Lazy<FsGrpc.Protobuf.ProtoDef<'T1>>) and 'T1: equality and 'T2 :> IMessage<'T2> and 'T2: (new: unit -> 'T2) and 'T2: equality> (bootstrapServer: string) (topicName: string) : R<'T1 option> seq =
    seq {
        for (k,vo) in rawReadFromTopic bootstrapServer topicName do
            
                match vo with
                | Some v -> 
                    let mutable csVersion = new 'T2() 
                    let (result: R<'T1 option>) = monad' {
                            let! uuid = 
                                match UUID.New k with
                                | Ok uuid -> Ok uuid
                                | Error str -> R.Error ($"UUID parse failed: {str}", None)
                            let! expected = 
                                try (FsGrpc.Protobuf.decode<'T1> v |> Ok) 
                                with | ex -> R.Error ("Failed to decode with FsGrpc", Some ex)
                            do! 
                                try (csVersion.MergeFrom v |> Ok)
                                with | ex -> R.Error ("Failed to decode with C# version", Some ex)
                            let! jsonFromCsVersion = 
                                try (Google.Protobuf.JsonFormatter.Default.Format(csVersion) |> Ok)
                                with | ex -> R.Error ("Failed to serialize to JSON", Some ex)
                            let! actual = 
                                try (FsGrpc.Json.deserialize jsonFromCsVersion |> Ok )
                                with | ex -> R.Error ("Failed to deserialize in FsGrpc", Some ex)
                            do! 
                                if actual = expected then 
                                    Ok () 
                                else 
                                    R.Error ($"JSON Deserialized C# entry did not match FsGrpc entry for ID {uuid.GetString}.\n from proto: {expected}\n\nfrom json of reference implementation: {actual}", None)
                            let! csVersion =
                                try (Text.Json.JsonSerializer.Deserialize<'T2>(FsGrpc.Json.serialize expected) |> Ok)
                                with | ex -> R.Error ("Failed to desierialize with C# version", Some ex)                            
                            let! actual = 
                                try (FsGrpc.Json.deserialize jsonFromCsVersion |> Ok )
                                with | ex -> R.Error ("Failed to deserialize in FsGrpc", Some ex)
                            return!
                                if actual = expected then 
                                    Ok (Some expected) 
                                else 
                                    R.Error ($"JSON Deserialized C# entry did not match FsGrpc entry for ID {uuid.GetString}.\n from proto: {expected}\n\nfrom json of reference implementation: {actual}", None)
                        }
                    yield result
                | None -> ()
    }

let inline compareTopicAcrossEnvironments<'T when 'T: (static member Proto : Lazy<FsGrpc.Protobuf.ProtoDef<'T>>)> (bootstrapServer1: string) (bootstrapServer2: string) (topicName: string) (equalityCompare: 'T -> 'T -> bool) =
    let m1 = rawReadFromTopic bootstrapServer1 topicName |> Map.ofSeq
    let m2 = rawReadFromTopic bootstrapServer2 topicName |> Map.ofSeq
    let allKeys = Set.unionMany (List.map (Map.keys >> Set.ofSeq) [m1; m2]) 
    let tryDecode bytes: Result<'T,byte array> =
        try FsGrpc.Protobuf.decode bytes |> Ok
        with _ -> Result.Error bytes
    seq {
        for rawKey in allKeys do
            let keyResult = 
                try UUID.New rawKey |> Ok
                with _ -> Result.Error rawKey
            let v1: Result<'T,byte array> option = Map.tryFind rawKey m1 |> Option.flatten |> Option.map tryDecode
            let v2: Result<'T,byte array> option = Map.tryFind rawKey m2 |> Option.flatten |> Option.map tryDecode
            match (v1, v2) with
            | (Some (Ok left), Some (Ok right)) ->
                if equalityCompare left right = false then yield (keyResult, v1, v2)
            | _ -> ()
    }