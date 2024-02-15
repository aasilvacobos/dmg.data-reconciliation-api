module ReconLibrary 

open KafkaLib
open System
open FsToolkit.ErrorHandling

// Uncomment one of these to select an environment

//let host = "b-1.kafka-sandbox.6k2r3u.c9.kafka.us-east-1.amazonaws.com:9094" // SANDBOX
//let host = "b-1.kafka-test-001.3998ks.c3.kafka.us-east-1.amazonaws.com:9094"// TEST
let host = "b-1.kafka-stage.nikt1t.c20.kafka.us-east-1.amazonaws.com:9094"  // STAGING
//let host = "b-1.kafka-prod.0n8vuh.c23.kafka.us-east-1.amazonaws.com:9094"   // PROD

let topic = "dmg.provider-billing.state"
let visitTopic = "fulfilment.workvisit.state"

let directory =
    match int System.Environment.OSVersion.Platform with
    | 4 | 128 -> "./"
    | 6       -> "./"
    | _       -> "c:/temp/"

let inline readRecords<'T> (parser: byte [] -> 'T) (getKey: 'T -> Guid option) topic =
    let parser = Option.map parser
    let contents =
        rawReadFromTopic host topic
        |> Seq.choose (fun (_,v) -> option {
            try
                let! o = parser v
                let! k = getKey o
                return (k,o)
            with _ -> return! None})


    let uniqueRecords =
        Collections.Generic.Dictionary<_, _>()

    for key,value in contents do
        uniqueRecords[key] <- value

    uniqueRecords

type ProviderBillingProjection = DMG.ProviderInvoicing.DT.Domain.ProviderBilling

module ProviderBillingProjection =
    open DMG.ProviderInvoicing.IO.ProviderBilling.Database
    open DMG.ProviderInvoicing.DT.Domain
    let api = new Reader.ProviderBillingDatabaseApi("put connection string here")
    let loadAsync =
        ProviderBillingId
        >> api.RetrieveByIdAsync
        >> Task.map LanguageExt.FSharp.ToFSharp
        >> TaskResult.mapError string

    let getAllLines<'a> (getLines: ProviderBillingVisit -> LanguageExt.Lst<'a>) (p: ProviderBillingProjection) =
        p.Visit.Visits
        |> LanguageExt.FSharp.ToFSharp
        |> List.collect (getLines >> LanguageExt.FSharp.ToFSharp)


type Visit = Dmg.Work.Visit.V1.Visit

module Visit =
    let parseId (v:Visit) =
        match Guid.TryParse v.VisitId with
        | false, _ -> v.VisitId |> sprintf "Bad GUID value for visit id: %s" |> Error
        | true, pbid -> Ok pbid


type ProviderBillingState = DMG.Proto.ProviderBilling.State.ProviderBillingState

module ProviderBillingState =
    open DMG.Proto.ProviderBilling.State

    type Visit = DMG.Proto.ProviderBilling.State.Visit

    module Visit =
        let parseId (v:Visit) =
            match Guid.TryParse v.VisitId with
            | false, _ -> v.VisitId |> sprintf "Bad GUID value for visit id: %s" |> Error
            | true, pbid -> Ok pbid

    let parseId (pb:ProviderBillingState) =
        match System.Guid.TryParse pb.ProviderBillingId with
        | false, _ -> pb.ProviderBillingId |> sprintf "Bad GUID value for provider billing id: %s" |> Error
        | true, pbid -> Ok pbid

    let parseFromByteArray (x: byte []) = ProviderBillingState.Parser.ParseFrom x

    let calculateAmount (pb:ProviderBillingState) =
        let totalAmount =
            match pb.TotalAmount |> Option.ofObj |> Option.map (fun ta -> ta.Amount) with
            | None
            | Some 0L ->
                pb.LineItems
                |> Seq.sumBy (fun li ->
                    li.MoneyAmount
                    |> Option.ofObj
                    |> Option.map (fun ma -> ma.Amount)
                    |> Option.defaultValue 0L)
            | Some a -> a
        (decimal totalAmount) / 1000m

    let isVerified (pb:ProviderBillingState) = pb.State = State.BillingVerified

    module Option =
        let isVerified<'a> =
            Option.bind (fun (pb,totalAmount: 'a) ->
                if isVerified pb then
                    Some (pb, totalAmount)
                else
                    None)

    let writeToFile (records: List<ProviderBillingState * decimal>) =
        let now = DateTime.Now
        use writer = new System.IO.StreamWriter(directory + $"{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-" + typeof<'T>.Name + ".csv", append=false)

        writer.WriteLine "sequenceNumber,ProviderBillingId,ProviderBillingNumber,TotalAmount"
        records
        |> List.iteri (fun (i: int) (pb, totalAmount) ->
            sprintf "%i,%s,%s,%M" i pb.ProviderBillingId pb.ProviderBillingNumber totalAmount
            |> writer.WriteLine)
        printfn "Processing Complete.\n\n\n"
        writer.Flush()

let providerBillings =
    printfn "fetching provider billings from state topic"
    topic
    |> readRecords ProviderBillingState.parseFromByteArray (ProviderBillingState.parseId >> Result.toOption)
    |> fun x -> x.Values
    |> List.ofSeq
    |> List.map (fun pb -> (pb, ProviderBillingState.calculateAmount pb))

// write verfied provider billings to file
// providerBillings
// |> List.choose ProviderBillingState.Option.isVerified
// |> ProviderBillingState.writeToFile



let visitsDict =
    printfn "fetching visits from state topic"
    visitTopic
    |> readRecords Dmg.Work.Visit.V1.Visit.Parser.ParseFrom (Visit.parseId >> Result.toOption)



open ShellProgressBar

let reconToProjection () =
    let length = providerBillings.Length
    let pbar = new ProgressBar(length, "Processing Topic<->Projection Reconciliation", ProgressBarOptions(ProgressCharacter = '─',ProgressBarOnBottom = true))
    let reportProgress (i: int) = pbar.Tick(i);

    let parallelMax i w = Async.Parallel(w,i)

    let now = DateTime.Now
    use writer = new System.IO.StreamWriter(directory + $"{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-" + "Projection.csv", append=false)
    writer.WriteLine "sequenceNumber,ProviderBillingId,ProviderBillingNumber,TotalAmount,ProjectionTotal,Diff,AbsDiff"
    providerBillings
    // |> List.truncate 100
    |> List.mapi (
        fun i (pb: ProviderBillingState,totalAmount: decimal) ->
            task {
                let! projection =
                    pb
                    |> ProviderBillingState.parseId
                    |> Threading.Tasks.Task.FromResult
                    |> TaskResult.bind ProviderBillingProjection.loadAsync
                if i % 10 = 0 then reportProgress i
                return (pb,totalAmount,projection)
            } |> Async.AwaitTask)
    |> parallelMax 10
    |> Async.RunSynchronously
    |> Array.iteri (fun i (pb: ProviderBillingState,totalAmount: decimal, projection: Result<ProviderBillingProjection,string>)  ->
        match projection with
        | Error _ ->
            let diff = totalAmount
            let absDiff = diff |> abs
            (0.00m,diff,absDiff)
        | Ok p ->
            let serviceLines =
                let eventLineItemId =
                    p.Event
                    |> LanguageExt.FSharp.ToFSharp
                    |> Option.bind (fun e -> LanguageExt.FSharp.ToFSharp e.EventLineItemId)
                    |> Option.map (fun eid -> eid.Value)
                    |> Option.defaultValue Guid.Empty
                    |> DMG.ProviderInvoicing.DT.Domain.PerOccurrenceItemId
                p |> ProviderBillingProjection.getAllLines (fun v -> v.ServiceLineItems)
                |> List.filter (fun l -> l.PerOccurrenceItemId <> eventLineItemId)
            let eventTotal =
                p.Event
                |> LanguageExt.FSharp.ToFSharp
                |> Option.map (fun e -> e.Amount)
                |> Option.defaultValue 0m
            let timeAndMaterialLines =
                p |> ProviderBillingProjection.getAllLines (fun v -> v.TimeAndMaterialLineItems)
            let adjustmentTotal =
                let tmAdjustmentAmount =
                    timeAndMaterialLines
                    |> List.choose (fun l -> (LanguageExt.FSharp.ToFSharp l.Adjustment))
                    |> List.sumBy (fun a -> a.Amount)
                serviceLines
                |> List.choose (fun sl -> sl.Adjustment |> LanguageExt.FSharp.ToFSharp)
                |> List.sumBy (fun a -> a.Amount)
                |> (+) tmAdjustmentAmount
            let projectionTotal =
                (serviceLines |> List.sumBy (fun sl -> sl.ServiceRate.Value))
                + (timeAndMaterialLines |> List.sumBy (fun sl -> sl.Amount))
                + adjustmentTotal
                + eventTotal
            let diff = totalAmount - projectionTotal
            let absDiff = diff |> abs
            (projectionTotal,diff, absDiff)
        |> fun  (projectionTotal,diff, absDiff) -> sprintf "%i,%s,%s,%M,%M,%M,%M" i pb.ProviderBillingId pb.ProviderBillingNumber totalAmount projectionTotal diff absDiff
        |> writer.WriteLine
        writer.Flush()
    )

reconToProjection()


let compareVisits() =
    let length = providerBillings.Length
    let pbar = new ProgressBar(length, "Processing Topic<->Projection Reconciliation", ProgressBarOptions(ProgressCharacter = '─',ProgressBarOnBottom = true))
    let reportProgress (i: int) = pbar.Tick(i);

    let parallelMax i w = Async.Parallel(w,i)

    let now = DateTime.Now
    use writer = new System.IO.StreamWriter(directory + $"{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-" + "Visits.csv", append=false)
    //writer.WriteLine "SequenceNumber,ProviderBillingId,ProviderBillingNumber,BillingVisitQtyTotal,BillingVisitCorrectedTotal,LoggedVisitsTotal,VisitsTotal,Diff,AbsDiff"
    writer.WriteLine "SequenceNumber,ProviderBillingId,ProviderBillingNumber,LineItems,BillingLineItems,Mismatch"

    let visits = visitsDict |> Seq.map (|KeyValue|) |> Map.ofSeq
    providerBillings
    |> List.filter (fun (pb,_) -> pb.NonRoutine |> Option.ofNull |> Option.isSome)
    |> List.iteri (fun i (pb,_) ->
        // This accounts for the line items that PB was able to fetch from the logged visits
        let billingVisitQtyTotal =
            pb.LineItems
            |> Seq.filter (fun li -> li.Source = DMG.Proto.ProviderBilling.State.Source.VisitLogs && li.Status <> DMG.Proto.ProviderBilling.State.LineItemStatus.Removed)
            |> Seq.sumBy(fun li ->
               li.DecimalQuantity
               |> Option.ofObj
               |> Option.map (fun x -> x.Units + int64(x.Nanos / 1000000000))
               |> Option.defaultValue 0L
               |> decimal)
        // These are the li from the logged visits + anything corrected by the DMs/Providers
        let billingVisitCorrectedQtyTotal =
            pb.LineItems
            |> Seq.filter (fun li -> li.Status <> DMG.Proto.ProviderBilling.State.LineItemStatus.Removed)
            |> Seq.sumBy(fun li ->
               li.DecimalQuantity
               |> Option.ofObj
               |> Option.map (fun x -> x.Units + int64(x.Nanos / 1000000000))
               |> Option.defaultValue 0L
               |> decimal)
        let billingVisitLineItems =
            pb.LineItems
            |> Seq.filter (fun li -> li.Status <> DMG.Proto.ProviderBilling.State.LineItemStatus.Removed && li.Name <> "Trip Charge")
            |> Seq.sumBy(fun _ -> 1)
        let visitIds =
           pb.Visits |> Seq.choose (ProviderBillingState.Visit.parseId >> Result.toOption)
        let jobIds = pb.JobIds |> List.ofSeq
        let billingLoggedVisitsTotalQty = pb.Visits |> Seq.length |> Decimal
        let visitsFromTopic =
            visitIds
            |> Seq.choose (fun x -> Map.tryFind x visits)
        //This needs to use the transformed quantity
        //https://buf.build/divisions-maintenance-group/dmg-work-proto/docs/main:dmg.work.commons.v1#dmg.work.commons.v1.TransformedQuantity
        //let visitQtyTotal =
        //   visitsFromTopic
        //   |> Seq.sumBy (fun (v:Visit) ->
        //       v.TechnicianSubmittedItems
        //       |> Seq.sumBy (fun tsi ->
        //           tsi.Quantity
        //           |> Option.ofNullable
        //           |> Option.map decimal
        //           |> Option.defaultValue 0m))
        // i.e. How many line item are we expected to fetch from that visit
        let lineItemTotal =
           visitsFromTopic
           |> Seq.sumBy (fun (v:Visit) -> Seq.length v.TechnicianSubmittedItems)
        // To check whether we actually got the same ids on the visits inside the final billing
        let lineItemPerOccurrenceIdsFromVisit =
           visitsFromTopic
           |> Seq.map (fun (v:Visit) ->
               Seq.choose (fun (li: Dmg.Work.Visit.V1.VisitItem) -> if isNull li.ServiceBasedItem then None else Some li.ServiceBasedItem.ServiceBasedItemId) v.TechnicianSubmittedItems)
           |> Seq.concat
           |> Set.ofSeq
        // I'm replacing it with the corrected total to account values manually fixed by the providers/dmg
        //let diff = visitQtyTotal - billingVisitCorrectedQtyTotal
        let diff = billingVisitLineItems - lineItemTotal
        let absDiff = diff |> abs
        let mismatch = diff < 0
        let billingTotal =
            try
                pb.TotalAmount
                |> Option.ofNullable
                |> Option.map decimal
                |> Option.defaultValue 0M
            with
            | _ -> 0M
        if i % 10 = 0 then reportProgress i
        //$"%i{i},%s{pb.ProviderBillingId},%s{pb.ProviderBillingNumber},%M{billingVisitQtyTotal},%M{billingVisitCorrectedQtyTotal},%M{billingLoggedVisitsTotalQty},%M{visitQtyTotal},%M{diff},%M{absDiff}"
        //|> writer.WriteLine
        $"%i{i},%s{pb.ProviderBillingId},%s{pb.ProviderBillingNumber},%i{lineItemTotal},%i{billingVisitLineItems},%b{mismatch}"
        |> writer.WriteLine
        )
    writer.Flush()

compareVisits()


//namespace DMG.DataReconciliation
//    let loadAsyncProviderBilling = ProviderBillingProjection.loadAsync
//    let getAllLinesProviderBilling = ProviderBillingProjection.getAllLines

//    let parseIdVisit = Visit.parseId
//    let parseIdProviderBillingState = ProviderBillingState.parseId
//    let calculateAmountProviderBillingState = ProviderBillingState.calculateAmount
//    let isVerifiedProviderBillingState = ProviderBillingState.isVerified
