module DMG.ProviderInvoicing.BL.Utility.Introspection

open Microsoft.FSharp.Quotations.Patterns

/// helper to get type of a Module (static class in CLR) https://stackoverflow.com/a/26621814
/// put the following line inside a module to get the type:     let rec private moduleType = LiftToUtility.getModuleType <@ moduleType @>
let getModuleType =
    function
    | PropertyGet(_, propertyInfo, _) -> propertyInfo.DeclaringType
    | _ -> failwith "Expression is no property."