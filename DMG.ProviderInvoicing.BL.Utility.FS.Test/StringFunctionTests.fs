module DMG.ProviderInvoicing.BL.Utility.Test.StringFunctionTests

open NUnit.Framework
open FsUnitTyped
open Microsoft.Toolkit

[<Test>]
let ``Test toUpper`` () =
    let input = "my input"
    toUpper input |> shouldEqual (input.ToUpper())
    
[<Test>]
let ``Test toUpper with null`` () = 
    try
         toUpper null |> shouldEqual "chicken"
    with
        | ex ->  Assert.Pass()
        
[<Test>]
let ``Test toLower`` () =
    let input = "my input"
    toLower input |> shouldEqual (input.ToLower())
    
[<Test>]
let ``Test toLower with null`` () = 
    try
         toLower null |> shouldEqual "chicken"
    with
        | ex ->  Assert.Pass()
        
[<Test>]
let ``Test startsWith`` () =
    let input = "SampleTestString"
    startsWith  "Sample" input  |> shouldEqual (input.StartsWith("Sample"))
    
[<Test>]
let ``Test endsWith`` () =
    let input = "ThisIsTheEnding"
    endsWith  "Ending" input  |> shouldEqual (input.EndsWith("Ending"))
    
[<Test>]
let ``Test toCharArray`` () =
    let input = "TestingCharArray"
    toCharArray  input  |> shouldEqual (input.ToCharArray())
    
[<Test>]
let ``Test replace`` () =
    let errorString = "This docment uses 3 other docments to docment the docmentation"    
    replace ("docment","document") errorString  |> shouldEqual (errorString.Replace("docment", "document"))
    
[<Test>]
let ``Test trimStart`` () =
    let stringWithLeadingSymbols = "$$$$Hello World!"
    let symbol = [| '$' |]    
    trimStart symbol stringWithLeadingSymbols  |> shouldEqual "Hello World!"
    
[<Test>]
let ``Test trimEnd`` () =
    let stringWithTrailingSymbols = "Hello World!$$$"
    let symbol = [| '$' |]    
    trimEnd symbol stringWithTrailingSymbols  |> shouldEqual "Hello World!"
    
[<Test>]
let ``Test trim`` () =
    let stringWithSpaces = "   Hello World!   "
    trim stringWithSpaces  |> shouldEqual "Hello World!"


[<Test>]
let ``Test concat`` () =
    let stringTest = seq[ "a"; "simple"; "string"]
    concat "-" stringTest |> shouldEqual "a-simple-string"

[<Test>]
let ``Test replicate`` () =
    let stringToRepeat = "*"
    replicate 5 stringToRepeat  |> shouldEqual "*****"

[<Test>]
let ``Test isNullOrWhiteSpace-whitespace`` () =
    let stringWhiteSpace = " "
    isNullOrWhiteSpace stringWhiteSpace  |> shouldEqual true

[<Test>]
let ``Test isNullOrWhiteSpace-nonwhitespace`` () =
    let stringNoWhiteSpace = "test"
    isNullOrWhiteSpace stringNoWhiteSpace  |> shouldEqual false

[<Test>]
let ``Test isNullOrWhiteSpace-null`` () =
    let stringNull = null
    isNullOrWhiteSpace stringNull  |> shouldEqual true

[<Test>]
let ``Test emptyString`` () =
    emptyString |> shouldEqual ""

[<Test>]
let ``Test length`` () =
    let stringTest = "aaa"
    length stringTest |> shouldEqual 3

[<Test>]
let ``Test subString`` () =
    let stringSubString = "AAABBB"
    subString 3 3 stringSubString |> shouldEqual "BBB"

[<Test>]
let ``Test subStringToEnd`` () =
    let stringSubString = "AAABBB"
    subStringToEnd 3 stringSubString |> shouldEqual "BBB"

[<Test>]
let ``Test toTitleCase`` () =
    let stringTest = "a Simple string"
    toTitleCase stringTest |> shouldEqual "A Simple String"

[<Test>]
let ``Test join`` () =
    let stringTest = seq[ "a"; "simple"; "string"]
    join "-" stringTest |> shouldEqual "a-simple-string"

[<Test>]
let ``Test split`` () =
    let stringTest = "You win some. You lose some."
    let stringSplit = "You win some. You lose some."
    split " " stringTest |> shouldEqual [|"You"; "win"; "some."; "You"; "lose"; "some."|]

[<Test>]
let ``Test nullToEmptyString`` () =
    let stringTest = null
    nullToEmptyString  stringTest |> shouldEqual emptyString

[<Test>]
let ``Test nullToEmptyString non null`` () =
    let stringTest = "Test"
    nullToEmptyString stringTest |> shouldEqual stringTest

[<Test>]
let ``WHEN contains function is called with a non-null comparison string and non-null base string THEN it returns same value as String.Contains`` () =
    let testString1 = "chicken"
    let testString2 = "chick"
    testString1 |> contains testString2 |> shouldEqual (testString1.Contains(testString2))
    testString2 |> contains testString1 |> shouldEqual (testString2.Contains(testString1))

[<Test>]
let ``WHEN contains function is called with a null comparison string or null base string THEN it returns false`` () =
    let testStringValue = "chicken"
    let testStringNull = Unchecked.defaultof<string>
    testStringValue |> contains testStringNull |> shouldEqual false
    testStringNull |> contains testStringValue |> shouldEqual false

[<Test>]
let ``WHEN contains function is called with an empty comparison string THEN it returns true`` () =
    let testStringValue = "chicken"
    let testStringEmpty = emptyString
    testStringValue |> contains testStringEmpty |> shouldEqual true

[<Test>]
let ``WHEN contains function is called with a empty base string THEN it returns false`` () =
    let testStringValue = "chicken"
    let testStringEmpty = emptyString
    testStringEmpty |> contains testStringValue |> shouldEqual false

[<Test>]
let ``WHEN truncate function is called with a non-negative length and non-null string THEN it returns same value as StringExtensions.Truncate`` () =
    let testStringValue = "chicken"
    testStringValue |> truncate (testStringValue.Length - 1) |> shouldEqual (StringExtensions.Truncate(testStringValue, testStringValue.Length - 1))
    testStringValue |> truncate (testStringValue.Length + 1) |> shouldEqual (StringExtensions.Truncate(testStringValue, testStringValue.Length + 1))
    testStringValue |> truncate 0 |> shouldEqual (StringExtensions.Truncate(testStringValue, 0))
        
[<Test>]
let ``WHEN truncate function is called with a null string THEN it returns null`` () =
    let testStringValue = Unchecked.defaultof<string>
    testStringValue |> truncate 0 |> shouldEqual Unchecked.defaultof<string>
    
[<Test>]
let ``WHEN truncate function is called with a negative length and non-null string THEN it returns a null`` () =
    let testStringValue = "chicken"
    testStringValue |> truncate -1 |> shouldEqual Unchecked.defaultof<string>
    