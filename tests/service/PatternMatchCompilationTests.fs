module FSharp.Compiler.Service.Tests.PatternMatchCompilationTests

open FsUnit
open NUnit.Framework
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Diagnostics
open FSharp.Compiler.EditorServices
open FSharp.Compiler.IO
open FSharp.Compiler.Syntax


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Wrong type 01 - Match`` () =
    let _, checkResults = getParseAndCheckResults """
match () with
| "" -> ()
| x -> let y = () in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(3,2--3,4): This expression was expected to have type 'unit' but here has type 'string'"
    ]

[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Wrong type 02 - Binding`` () =
    let _, checkResults = getParseAndCheckResults """
let ("": unit), (x: int) = let y = () in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(2,5--2,7): This expression was expected to have type 'unit' but here has type 'string'"
        "(2,41--2,43): This expression was expected to have type 'unit * int' but here has type 'unit'"
        "(2,4--2,24): Incomplete pattern matches on this expression."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Attributes 01 `` () =
    let _, checkResults = getParseAndCheckResults """
match () with
| [<CompiledName("Foo")>] x -> let y = () in ()
"""
    assertHasSymbolUsages ["x"; "y"; "CompiledNameAttribute"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(3,2--3,25): Attributes are not allowed within patterns"
        "(3,4--3,16): This attribute is not valid for use on this language element"
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Optional val 01 `` () =
    let _, checkResults = getParseAndCheckResults """
match () with
| ?x -> let y = () in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(3,2--3,4): Optional arguments are only permitted on type members"
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Null 01`` () =
    let _, checkResults = getParseAndCheckResults """
match 1, 2 with
| null -> let y = () in ()
"""
    assertHasSymbolUsages ["y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(3,2--3,6): The type '(int * int)' does not have 'null' as a proper value"
        "(2,6--2,10): Incomplete pattern matches on this expression. For example, the value '( some-non-null-value )' may indicate a case not covered by the pattern(s)."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Union case 01 - Missing field`` () =
    let _, checkResults = getParseAndCheckResults """
type U =
    | A
    | B of int * int * int

match A with
| B (x, _) -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(7,2--7,10): This union case expects 3 arguments in tupled form"        
        "(6,6--6,7): Incomplete pattern matches on this expression. For example, the value 'A' may indicate a case not covered by the pattern(s)."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Union case 02 - Extra args`` () =
    let _, checkResults = getParseAndCheckResults """
type U =
    | A
    | B of int

match A with
| B (_, _, x) -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(7,5--7,12): This expression was expected to have type 'int' but here has type ''a * 'b * 'c'"
        "(6,6--6,7): Incomplete pattern matches on this expression."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Union case 03 - Extra args`` () =
    let _, checkResults = getParseAndCheckResults """
type U =
    | A
    | B of int * int

match A with
| B (_, _, x) -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(7,11--7,12): This constructor is applied to 3 argument(s) but expects 2"
        "(6,6--6,7): Incomplete pattern matches on this expression. For example, the value 'A' may indicate a case not covered by the pattern(s)."
    ]

[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Union case 04 - Extra args`` () =
    let _, checkResults = getParseAndCheckResults """
type U =
    | A
    | B of int

match A with
| A x -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(7,2--7,5): This union case does not take arguments"
        "(6,6--6,7): Incomplete pattern matches on this expression. For example, the value 'B (_)' may indicate a case not covered by the pattern(s)."
    ]

[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Union case 05 - Single arg, no errors`` () =
    let _, checkResults = getParseAndCheckResults """
type U =
    | A
    | B of int

match A with
| B x -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(6,6--6,7): Incomplete pattern matches on this expression. For example, the value 'A' may indicate a case not covered by the pattern(s)."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Union case 06 - Named args - Wrong field name`` () =
    let _, checkResults = getParseAndCheckResults """
type U =
    | A
    | B of field: int

match A with
| B (name = x) -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(7,5--7,9): The union case 'B' does not have a field named 'name'."
        "(6,6--6,7): Incomplete pattern matches on this expression. For example, the value 'A' may indicate a case not covered by the pattern(s)."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Union case 07 - Named args - Name used twice`` () =
    let _, checkResults = getParseAndCheckResults """
type U =
    | A
    | B of field: int * int

match A with
| B (field = x; field = z) -> let y = x + z + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"; "z"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(7,16--7,21): Union case/exception field 'field' cannot be used more than once."
        "(6,6--6,7): Incomplete pattern matches on this expression. For example, the value 'A' may indicate a case not covered by the pattern(s)."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Union case 08 - Multiple tupled args`` () =
    let _, checkResults = getParseAndCheckResults """
type U =
    | A
    | B of field: int * int

match A with
| B x z -> let y = x + z + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"; "z"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(7,2--7,7): This union case expects 2 arguments in tupled form"
        "(6,6--6,7): Incomplete pattern matches on this expression. For example, the value 'A' may indicate a case not covered by the pattern(s)."
    ]


[<Test>]
let ``Union case 09 - Single arg`` () =
    let _, checkResults = getParseAndCheckResults """
match None with
| None -> ()
| Some (x, z) -> let y = x + z + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"; "z"] checkResults
    dumpErrors checkResults |> shouldEqual [
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Active pattern 01 - Named args`` () =
    let _, checkResults = getParseAndCheckResults """
let (|Foo|) x = x

match 1 with
| Foo (field = x) -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(5,2--5,17): Foo is an active pattern and cannot be treated as a discriminated union case with named fields."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Literal 01 - Args - F#`` () =
    let _, checkResults = getParseAndCheckResults """
let [<Literal>] Foo = 1

match 1 with
| Foo x -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(5,2--5,7): This literal pattern does not take arguments"
        "(4,6--4,7): Incomplete pattern matches on this expression. For example, the value '0' may indicate a case not covered by the pattern(s)."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Literal 02 - Args - IL`` () =
    let _, checkResults = getParseAndCheckResults """
open System.Diagnostics

match TraceLevel.Off with
| TraceLevel.Off x -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(5,2--5,18): This literal pattern does not take arguments"
        "(4,6--4,20): Incomplete pattern matches on this expression. For example, the value 'TraceLevel.Error' may indicate a case not covered by the pattern(s)."
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Caseless DU`` () =
    let _, checkResults = getParseAndCheckResults """
type DU = Case of int

let f du =
    match du with
    | Case -> ()

let dowork () =
    f (Case 1)
    0 // return an integer exit code"""
    assertHasSymbolUsages ["DU"; "dowork"; "du"; "f"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(6,6--6,10): This constructor is applied to 0 argument(s) but expects 1"
    ]
    
[<Test>]
let ``Or 01 - No errors`` () =
    let _, checkResults = getParseAndCheckResults """
match 1 with
| x | x -> let y = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"] checkResults
    dumpErrors checkResults |> shouldEqual []


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Or 02 - Different names`` () =
    let _, checkResults = getParseAndCheckResults """
match 1 with
| x | z -> let y = x + z + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"; "z"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(3,2--3,7): The two sides of this 'or' pattern bind different sets of variables"
    ]


[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``Or 03 - Different names and types`` () =
    let _, checkResults = getParseAndCheckResults """
type U =
    | A
    | B of int * string

match A with
| B (x, y) | B (a, x) -> let z = x + 1 in ()
"""
    assertHasSymbolUsages ["x"; "y"; "z"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(7,2--7,21): The two sides of this 'or' pattern bind different sets of variables"
        "(7,19--7,20): This expression was expected to have type 'int' but here has type 'string'"
        "(6,6--6,7): Incomplete pattern matches on this expression. For example, the value 'A' may indicate a case not covered by the pattern(s)."
    ]
    
[<Test>]
let ``As 01 - names and wildcards`` () =
    let _, checkResults = getParseAndCheckResults """
match 1 with
| _ as w -> let x = w + 1 in ()

match 2 with
| y as _ -> let z = y + 1 in ()

match 3 with
| a as b -> let c = a + b in ()
"""
    assertHasSymbolUsages ["a"; "b"; "c"; "w"; "x"; "y"; "z"] checkResults
    dumpErrors checkResults |> shouldEqual []
    
    
[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``As 02 - type testing`` () =
    let _, checkResults = getParseAndCheckResults """
let (|Id|) = id
match box 1 with
| :? int as a -> let b = a + 1 in ()
| :? uint as (c & d) -> let e = c + d + 2u in ()
| :? int64 as Id f -> let g = f + 3L in ()
| :? uint64 as Id h & i -> let z = h + 4UL + i in () // (:? uint64 as Id h) & (i : obj)
| :? int8 as Id j as k -> let y = j + 5y + k in () // Only the first "as" will have the derived type
"""
    assertHasSymbolUsages (List.map string ['a'..'k']) checkResults
    dumpErrors checkResults |> shouldEqual [
        "(7,45--7,46): The type 'obj' does not match the type 'uint64'"
        "(7,43--7,44): The type 'obj' does not match the type 'uint64'"
        "(8,43--8,44): The type 'obj' does not match the type 'int8'"
        "(8,41--8,42): The type 'obj' does not match the type 'int8'"
        "(3,6--3,11): Incomplete pattern matches on this expression. For example, the value '( some-other-subtype )' may indicate a case not covered by the pattern(s)."
    ]
    
[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``As 03 - impossible type testing`` () =
    let _, checkResults = getParseAndCheckResults """
match Unchecked.defaultof<System.ValueType> with
| :? System.Enum as (:? System.ConsoleKey as a) -> let b = a + enum 1 in ()
| :? System.Enum as (:? System.ConsoleKey as c) -> let d = c + enum 1 in ()
| :? System.Enum as (:? int as x) -> let w = x + 1 in ()
| :? string as y -> let z = y + "" in ()
| _ -> ()
"""
    assertHasSymbolUsages ["a"; "b"; "c"; "d"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(5,21--5,27): Type constraint mismatch. The type 'int' is not compatible with type 'System.Enum' "
    ]

[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``As 04 - duplicate type testing`` () =
    let _, checkResults = getParseAndCheckResults """
match Unchecked.defaultof<System.ValueType> with
| :? System.Enum as (a & b) -> let c = a = b in ()
| :? System.Enum as (:? System.ConsoleKey as (d & e)) -> let f = d + e + enum 1 in ()
| g -> ()
"""
    assertHasSymbolUsages ["a"; "b"; "c"; "d"; "e"; "f"; "g"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(4,2--4,85): This rule will never be matched"
    ]

[<Test>]
#if !NETCOREAPP
[<Ignore("These tests weren't running on desktop and this test fails")>]
#endif
let ``As 05 - inferred type testing`` () =
    let _, checkResults = getParseAndCheckResults """
match Unchecked.defaultof<obj> with
| :? _ as a -> let _ = a in ()

match Unchecked.defaultof<int> with
| :? _ as z -> let _ = z in ()
"""
    assertHasSymbolUsages ["a"] checkResults
    dumpErrors checkResults |> shouldEqual [
        "(2,6--2,25): Incomplete pattern matches on this expression. For example, the value '( some-other-subtype )' may indicate a case not covered by the pattern(s)."
        "(6,2--6,6): The type 'int' does not have any proper subtypes and cannot be used as the source of a type test or runtime coercion."
    ]

[<Test>]
let ``As 06 - completeness`` () =
    let _, checkResults = getParseAndCheckResults """
match Unchecked.defaultof<bool> with
| true as a -> if a then ()
| b as false -> if not b then ()

match Unchecked.defaultof<bool> with
| c as true as d -> if c && d then ()
| e as (f as false) -> if e || f then ()

match Unchecked.defaultof<bool> with
| (g & h) as (i as true) as (_ as j) -> if g && h && i && j then ()
| k & l as (m as (false as n)) as (o as _) -> if k || l || m || n || o then ()
"""
    assertHasSymbolUsages (List.map string ['a'..'o']) checkResults
    dumpErrors checkResults |> shouldEqual []

[<Test>]
let ``As 07 - completeness`` () =
    let _, checkResults = getParseAndCheckResults """
match Unchecked.defaultof<bool> with
| true as a -> if a then ()
| b as false -> if not b then ()

match Unchecked.defaultof<bool> with
| c as true as d -> if c && d then ()
| e as (f as false) -> if e || f then ()

match Unchecked.defaultof<bool> with
| (g & h) as (i as true) as (_ as j) -> if g && h && i && j then ()
| k & l as (m as (false as n)) as (o as _) -> if k || l || m || n || o then ()
"""
    assertHasSymbolUsages (List.map string ['a'..'o']) checkResults
    dumpErrors checkResults |> shouldEqual []