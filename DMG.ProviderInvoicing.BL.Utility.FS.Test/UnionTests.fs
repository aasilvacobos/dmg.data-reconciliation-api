module DMG.ProviderInvoicing.BL.Utility.Test.UnionTests

open NUnit.Framework
open FsUnitTyped

type TestType = | Case1 | Case2 | Case3

[<Test>]
let ``WHEN a union case is converted to a string and then back to a case THEN it should be equal to the original case`` () =
    let unionCache = Union.getCases<TestType> |> Seq.cache
    let case = TestType.Case1
    let case' = case
                |> Union.fromCaseToString
                |> Union.tryCreateCase<TestType> unionCache
    shouldEqual (Some case) case'

[<Test>]
let ``WHEN a string is converted to a union case and then back to a string THEN it should be equal to the original string`` () =
    let unionCache = Union.getCases<TestType> |> Seq.cache
    let caseStr = "Case1"
    let caseStr' =
        match Union.tryCreateCase<TestType> unionCache caseStr with
        | Some case -> case |> Union.fromCaseToString
        | None -> emptyString
    shouldEqual caseStr caseStr'

[<Test>]
let ``WHEN a union is converted to sequence of strings THEN it should be equal to a sequence of each case converted to string`` () =
    let unionAsStrings = Union.getCases<TestType>
                         |> Seq.cache
                         |> Union.asStrings
    shouldEqual (["Case1"; "Case2"; "Case3"] |> Seq.ofList) unionAsStrings 