namespace DMG.ProviderInvoicing.DT.Service

open System
open DMG.ProviderInvoicing.DT.Domain

[<AutoOpen>]
module internal MessageUtil =
    let normalizeString str = 
        str
        |> Option.ofObj
        |> Option.defaultValue emptyString // prevents a null value DMG.ProviderInvoicing.DT.Service.FS.MessageUtil