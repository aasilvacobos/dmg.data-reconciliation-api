module DMG.ProviderInvoicing.BL.Utility.Test.WrappedStringTests

open System
open NUnit.Framework
open FsUnitTyped

// see Scott W. blog for description of this pattern
// https://fsharpforfunandprofit.com/posts/designing-with-types-more-semantic-types/

type MyWrappedStringType = private MyWrappedStringType of string with
    member internal this.Value = let (MyWrappedStringType s) = this in s
    interface IWrappedString with
        member this.Value = this.Value

module MyWrappedString =
    let private ctor = MyWrappedStringType
    let validString = "valid"
    let invalidString = "chicken"
    let validWrappedString () = ctor validString
    
    // todo add validation on the type when needed
    let isValid str = match str with               
                      | "chicken" -> false
                      | _ -> true
    let isValidWithException (str: String) : bool = Exception("bad chicken") |> raise
    
    let value (wrappedValue: IWrappedString) = wrappedValue.Value // redirect to interface property
    let create str = WrappedString.create id(*canonicalize*) ctor str
    let tryCreate str  = WrappedString.createCore id(*canonicalize*) isValid ctor str
    let tryCreate2 str = WrappedString.createCore id(*canonicalize*) isValidWithException ctor str
    let tryCreateOption str = WrappedString.tryCreateOption id(*canonicalize*) isValid ctor str

[<Test>]
let ``WHEN WrappedString.tryCreateOption is called with empty string THEN it returns Successful None`` () =
    shouldEqual (MyWrappedString.tryCreateOption emptyString) (Ok None)

[<Test>]
let ``WHEN WrappedString.tryCreateOption is called with white space THEN it returns Successful None`` () =
    shouldEqual (MyWrappedString.tryCreateOption " ") (Ok None)

[<Test>]
let ``WHEN WrappedString.tryCreateOption is called with null THEN it returns Successful None`` () =
    shouldEqual (MyWrappedString.tryCreateOption null) (Ok None) 

[<Test>]
let ``WHEN WrappedString.tryCreateOption is called with invalid (formatted) string THEN it returns Error of message`` () =
    shouldEqual (MyWrappedString.tryCreateOption MyWrappedString.invalidString) (Error $"MyWrappedString has invalid format.")

[<Test>]
let ``WHEN WrappedString.tryCreateOption is called with non-null, non-whitespace, non-empty string value THEN it returns Successful Some wrapped value`` () =
    shouldEqual (MyWrappedString.tryCreateOption MyWrappedString.validString) (() |> MyWrappedString.validWrappedString |> Some |> Ok) 

[<Test>]
let ``Wrapped string create should handle invalid data`` () =
    match MyWrappedString.tryCreate2 "chicken" with
    | ResultCreateString.ErrorInvalidFormat-> Assert.Pass()
    | ResultCreateString.ErrorException ex -> ex.Message |> shouldEqual "bad chicken"
    | _ -> Assert.Fail("Should not get here")
    ()
    
[<Test>]
let ``Wrapped string create should fail when exception happens in validation`` () =
    match MyWrappedString.tryCreate "chicken" with
    | ResultCreateString.ErrorInvalidFormat-> Assert.Pass()
    | _ -> Assert.Fail("Should not get here")
    ()    
        
[<Test>]
let ``Wrapped string create should handle null`` () =
    try
        MyWrappedString.create null |> ignore
    with
        | ex ->
            let msg = ex.Message
            msg |> shouldEqual "MyWrappedString has invalid null string value. (Parameter 'str')"
            ()

[<Test>]
let ``Wrapped string create should work with valid string`` () =
    let input = "abc"
    let res = MyWrappedString.create input
    input |> shouldEqual (MyWrappedString.value res)
   
[<Test>]    
let ``Wrapped string create should work with empty string`` () =
    let input = ""    
    try
        MyWrappedString.create input |> ignore
    with
        | ex ->
            let msg = ex.Message
            msg |> shouldEqual "MyWrappedString has invalid empty string value. (Parameter 'str')"
            ()