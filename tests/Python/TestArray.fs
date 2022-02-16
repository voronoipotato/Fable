module Fable.Tests.Arrays

open System
open Util.Testing
open Fable.Tests.Util

type ParamArrayTest =
    static member Add([<ParamArray>] xs: int[]) = Array.sum xs

let add (xs: int[]) = ParamArrayTest.Add(xs)

#if FABLE_COMPILER
open Fable.Core

[<Emit("$1.constructor.name == $0")>]
let jsConstructorIs (s: string) (ar: 'T[]) = true
#endif

type ExceptFoo = { Bar:string }

let f (x:obj) (y:obj) (z:obj) = (string x) + (string y) + (string z)

let map f ar = Array.map f ar

type Point =
    { x: int; y: int }
    static member Zero = { x=0; y=0 }
    static member Neg(p: Point) = { x = -p.x; y = -p.y }
    static member (+) (p1, p2) = { x= p1.x + p2.x; y = p1.y + p2.y }

type MyNumber =
    | MyNumber of int
    static member Zero = MyNumber 0
    static member (+) (MyNumber x, MyNumber y) =
        MyNumber(x + y)
    static member DivideByInt (MyNumber x, i: int) =
        MyNumber(x / i)

type MyNumberWrapper =
    { MyNumber: MyNumber }

type Things =
    { MainThing: int
      OtherThing: string }

[<Fact>]
let ``test Pattern matching with arrays works`` () =
    match [||] with [||] -> true | _ -> false
    |> equal true
    match [|1|] with [||] -> 0 | [|x|] -> 1 | _ -> 2
    |> equal 1
    match [|"a";"b"|] with [|"a";"b"|] -> 1 | _ -> 2
    |> equal 1

[<Fact>]
let ``test ParamArrayAttribute works`` () =
    ParamArrayTest.Add(1, 2) |> equal 3

[<Fact>]
let ``test Passing an array to ParamArrayAttribute works`` () =
    ParamArrayTest.Add([|3; 2|]) |> equal 5

[<Fact>]
let ``test Passing an array to ParamArrayAttribute from another function works`` () =
    add [|5;-7|] |> equal -2

[<Fact>]
let ``test Can spread a complex expression`` () =
    let sideEffect (ar: int[]) = ar.[1] <- 5
    ParamArrayTest.Add(let ar = [|1;2;3|] in sideEffect ar; ar)
    |> equal 9

[<Fact>]
let ``test Mapping from values to functions works`` () =
    let a = [| "a"; "b"; "c" |]
    let b = [| 1; 2; 3 |]
    let concaters1 = a |> Array.map (fun x y -> y + x)
    let concaters2 = a |> Array.map (fun x -> (fun y -> y + x))
    let concaters3 = a |> Array.map (fun x -> let f = (fun y -> y + x) in f)
    let concaters4 = a |> Array.map f
    let concaters5 = b |> Array.mapi f
    concaters1.[0] "x" |> equal "xa"
    concaters2.[1] "x" |> equal "xb"
    concaters3.[2] "x" |> equal "xc"
    concaters4.[0] "x" "y" |> equal "axy"
    concaters5.[1] "x" |> equal "12x"
    let f2 = f
    a |> Array.mapi f2 |> Array.item 2 <| "x" |> equal "2cx"

[<Fact>]
let ``test Mapping from typed arrays to non-numeric arrays doesn't coerce values`` ()=
    let xs = map string [|1;2|]
    (box xs.[0]) :? string |> equal true
    let xs2 = Array.map string [|1;2|]
    (box xs2.[1]) :? string |> equal true

[<Fact>]
let ``test Array slice with upper index work`` () =
    let xs = [| 1; 2; 3; 4; 5; 6 |]
    let ys = [| 8; 8; 8; 8; 8; 8; 8; 8; |]
    xs.[..2] |> Array.sum |> equal 6
    xs.[..2] <- ys
    xs |> Array.sum |> equal 39

[<Fact>]
let ``test Array slice with lower index work`` () =
    let xs = [| 1; 2; 3; 4; 5; 6 |]
    let ys = [| 8; 8; 8; 8; 8; 8; 8; 8; |]
    xs.[4..] |> Array.sum |> equal 11
    xs.[4..] <- ys
    xs |> Array.sum |> equal 26

[<Fact>]
let ``test Array slice with both indices work`` () =
    let xs = [| 1; 2; 3; 4; 5; 6 |]
    let ys = [| 8; 8; 8; 8; 8; 8; 8; 8; |]
    xs.[1..3] |> Array.sum |> equal 9
    xs.[1..3] <- ys
    xs |> Array.sum |> equal 36

[<Fact>]
let ``test Array slice with non-numeric arrays work`` () =
    let xs = [|"A";"B";"C";"D"|]
    xs.[1..2] <- [|"X";"X";"X";"X"|]
    equal xs.[2] "X"
    equal xs.[3] "D"

[<Fact>]
let ``test Array literals work`` () =
    let x = [| 1; 2; 3; 4; 5 |]
    equal 5 x.Length

[<Fact>]
let ``test Array indexer getter works`` () =
    let x = [| 1.; 2.; 3.; 4.; 5. |]
    x.[2] |> equal 3.

[<Fact>]
let ``test Array indexer setter works`` () =
    let x = [| 1.; 2.; 3.; 4.; 5. |]
    x.[3] <- 10.
    equal 10. x.[3]

[<Fact>]
let ``test Array getter works`` () =
    let x = [| 1.; 2.; 3.; 4.; 5. |]
    Array.get x 2 |> equal 3.

[<Fact>]
let ``test Array setter works`` () =
    let x = [| 1.; 2.; 3.; 4.; 5. |]
    Array.set x 3 10.
    equal 10. x.[3]

[<Fact>]
let ``test xs.Length works`` () =
    let xs = [| 1.; 2.; 3.; 4. |]
    xs.Length |> equal 4

[<Fact>]
let ``test Array.zeroCreate works`` () =
    let xs = Array.zeroCreate 2
    equal 2 xs.Length
    equal 0 xs.[1]

// See https://github.com/fable-compiler/repl/issues/96
[<Fact>]
let ``test Array.zeroCreate works with KeyValuePair`` () =
    let a = Array.zeroCreate<System.Collections.Generic.KeyValuePair<float,bool>> 3
    equal 0. a.[1].Key
    equal false a.[2].Value

[<Fact>]
let ``test Array.create works`` () =
    let xs = Array.create 2 5
    equal 2 xs.Length
    Array.sum xs |> equal 10

[<Fact>]
let ``test Array.blit works`` () =
    let xs = [| 1..10 |]
    let ys = Array.zeroCreate 20
    Array.blit xs 3 ys 5 4        // [|0; 0; 0; 0; 0; 4; 5; 6; 7; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0|]
    ys.[5] + ys.[6] + ys.[7] + ys.[8] |> equal 22

[<Fact>]
let ``test Array.blit works with non typed arrays`` () =
    let xs = [| 'a'..'h' |] |> Array.map string
    let ys = Array.zeroCreate 20
    Array.blit xs 3 ys 5 4
    ys.[5] + ys.[6] + ys.[7] + ys.[8] |> equal "defg"

[<Fact>]
let ``test Array.copy works`` () =
    let xs = [| 1; 2; 3; 4 |]
    let ys = Array.copy xs
    xs.[0] <- 0                   // Ensure a deep copy
    ys |> Array.sum |> equal 10

[<Fact>]
let ``test Array.distinct works`` () =
    let xs = [| 1; 1; 1; 2; 2; 3; 3 |]
    let ys = xs |> Array.distinct
    ys |> Array.length |> equal 3
    ys |> Array.sum |> equal 6

[<Fact>]
let ``test Array.distinct with tuples works`` () =
    let xs = [|(1, 2); (2, 3); (1, 2)|]
    let ys = xs |> Array.distinct
    ys |> Array.length |> equal 2
    ys |> Array.sumBy fst |> equal 3

[<Fact>]
let ``test Array.distinctBy works`` () =
    let xs = [| 4; 4; 4; 6; 6; 5; 5 |]
    let ys = xs |> Array.distinctBy (fun x -> x % 2)
    ys |> Array.length |> equal 2
    ys |> Array.head >= 4 |> equal true

[<Fact>]
let ``test Array.distinctBy with tuples works`` () =
      let xs = [| 4,1; 4,2; 4,3; 6,4; 6,5; 5,6; 5,7 |]
      let ys = xs |> Array.distinctBy (fun (x,_) -> x % 2)
      ys |> Array.length |> equal 2
      ys |> Array.head |> fst >= 4 |> equal true

// FIXME: this test currently hangs
// [<Fact>]
// let ``test Array distinctBy works on large array`` () =
//     let xs = [| 0 .. 50000 |]
//     let ys =
//         Array.append xs xs
//         |> Array.distinctBy(fun x -> x.ToString())
//     ys |> equal xs

[<Fact>]
let ``test Array.sub works`` () =
    let xs = [|0..99|]
    let ys = Array.sub xs 5 10    // [|5; 6; 7; 8; 9; 10; 11; 12; 13; 14|]
    ys |> Array.sum |> equal 95

[<Fact>]
let ``test Array.fill works`` () =
    let xs = Array.zeroCreate 4   // [|0; 0; 0; 0|]
    Array.fill xs 1 2 3           // [|0; 3; 3; 0|]
    xs |> Array.sum |> equal 6

[<Fact>]
let ``test Array.empty works`` () =
    let xs = Array.empty<int>
    xs.Length |> equal 0

[<Fact>]
let ``test Array.append works`` () =
    let xs1 = [|1; 2; 3; 4|]
    let zs1 = Array.append [|0|] xs1
    zs1.[0] + zs1.[1] |> equal 1
    let xs2 = [|"a"; "b"; "c"|]
    let zs2 = Array.append [|"x";"y"|] xs2
    zs2.[1] + zs2.[3] |> equal "yb"

[<Fact>]
let ``test Array.average works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    Array.average xs
    |> equal 2.5

[<Fact>]
let ``test Array.averageBy works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    Array.averageBy (fun x -> x * 2.) xs
    |> equal 5.

[<Fact>]
let ``test Array.average works with custom types`` () =
    [|MyNumber 1; MyNumber 2; MyNumber 3|] |> Array.average |> equal (MyNumber 2)

[<Fact>]
let ``test Array.averageBy works with custom types`` () =
    [|{ MyNumber = MyNumber 5 }; { MyNumber = MyNumber 4 }; { MyNumber = MyNumber 3 }|]
    |> Array.averageBy (fun x -> x.MyNumber) |> equal (MyNumber 4)

[<Fact>]
let ``test Array.choose with ints works`` () =
    let xs = [| 1; 2; 3; 4; 5; 6; 7; 8; 9; 10 |]
    let result = xs |> Array.choose (fun i ->
        if i % 2 = 1 then Some i
        else None)
    result.Length |> equal 5

[<Fact>]
let ``test Array.choose with longs works`` () =
    let xs = [|1L; 2L; 3L; 4L|]
    let result = xs |> Array.choose (fun x ->
       if x > 2L then Some x
       else None)
    result.[0] + result.[1]
    |> equal 7L

[<Fact>]
let ``test Array.choose must construct array of output type`` () =
    let source = [|1; 3; 5; 7|]
    let target = source |> Array.choose (fun x -> Some { MainThing=x; OtherThing="asd" })
    target.[3].MainThing |> equal 7

[<Fact>]
let ``test Array.collect works`` () =
    let xs = [|[|1|]; [|2|]; [|3|]; [|4|]|]
    let ys = xs |> Array.collect id
    ys.[0] + ys.[1]
    |> equal 3

    let xs1 = [|[|1.; 2.|]; [|3.|]; [|4.; 5.; 6.;|]; [|7.|]|]
    let ys1 = xs1 |> Array.collect id
    ys1.[0] + ys1.[1] + ys1.[2] + ys1.[3] + ys1.[4]
    |> equal 15.

[<Fact>]
let ``test Array.concat works`` () =
    let xs = [|[|1.|]; [|2.|]; [|3.|]; [|4.|]|]
    let ys = xs |> Array.concat
    ys.[0] + ys.[1]
    |> equal 3.

[<Fact>]
let ``test Array.concat works with strings`` () =
    [| [| "One" |]; [| "Two" |] |]
    |> Array.concat
    |> List.ofArray
    |> equal [ "One"; "Two" ]

[<Fact>]
let ``test Array.exists works`` () =
    let xs = [|1u; 2u; 3u; 4u|]
    xs |> Array.exists (fun x -> x = 2u)
    |> equal true

[<Fact>]
let ``test Array.exists2 works`` () =
    let xs = [|1UL; 2UL; 3UL; 4UL|]
    let ys = [|1UL; 2UL; 3UL; 4UL|]
    Array.exists2 (fun x y -> x * y = 16UL) xs ys
    |> equal true

[<Fact>]
let ``test Array.filter works`` () =
    let xs = [|1s; 2s; 3s; 4s|]
    let ys = xs |> Array.filter (fun x -> x > 2s)
    ys.Length |> equal 2

[<Fact>]
let ``test Array.filter with chars works`` () =
    let xs = [|'a'; '2'; 'b'; 'c'|]
    let ys = xs |> Array.filter Char.IsLetter
    ys.Length |> equal 3

[<Fact>]
let ``test Array.find works`` () =
    let xs = [|1us; 2us; 3us; 4us|]
    xs |> Array.find ((=) 2us)
    |> equal 2us

[<Fact>]
let ``test Array.findIndex works`` () =
    let xs = [|1.f; 2.f; 3.f; 4.f|]
    xs |> Array.findIndex ((=) 2.f)
    |> equal 1

//[<Fact>]
//let ``test Array.findBack works`` () =
//    let xs = [|1.; 2.; 3.; 4.|]
//    xs |> Array.find ((>) 4.) |> equal 1.
//    xs |> Array.findBack ((>) 4.) |> equal 3.

// [<Fact>]
// let ``test Array.findIndexBack works`` () =
//     let xs = [|1.; 2.; 3.; 4.|]
//     xs |> Array.findIndex ((>) 4.) |> equal 0
//     xs |> Array.findIndexBack ((>) 4.) |> equal 2

// [<Fact>]
// let ``test Array.tryFindBack works`` () =
//     let xs = [|1.; 2.; 3.; 4.|]
//     xs |> Array.tryFind ((>) 4.) |> equal (Some 1.)
//     xs |> Array.tryFindBack ((>) 4.) |> equal (Some 3.)
//     xs |> Array.tryFindBack ((=) 5.) |> equal None

// [<Fact>]
// let ``test Array.tryFindIndexBack works`` () =
//     let xs = [|1.; 2.; 3.; 4.|]
//     xs |> Array.tryFindIndex ((>) 4.) |> equal (Some 0)
//     xs |> Array.tryFindIndexBack ((>) 4.) |> equal (Some 2)
//     xs |> Array.tryFindIndexBack ((=) 5.) |> equal None

[<Fact>]
let ``test Array.fold works`` () =
    let xs = [|1y; 2y; 3y; 4y|]
    let total = xs |> Array.fold (+) 0y
    total |> equal 10y

[<Fact>]
let ``test Array.fold2 works`` () =
    let xs = [|1uy; 2uy; 3uy; 4uy|]
    let ys = [|1uy; 2uy; 3uy; 4uy|]
    let total = Array.fold2 (fun x y z -> x + y + z) 0uy xs ys
    total |> equal 20uy

[<Fact>]
let ``test Array.foldBack works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    let total = Array.foldBack (fun x acc -> acc - x) xs 0.
    total |> equal -10.

[<Fact>]
let ``test Array.foldBack2 works`` () =
    let xs = [|1; 2; 3; 4|]
    let ys = [|1; 2; 3; 4|]
    let total = Array.foldBack2 (fun x y acc -> x + y - acc) xs ys 0
    total |> equal -4

[<Fact>]
let ``test Array.forall works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    Array.forall ((>) 5.) xs
    |> equal true

[<Fact>]
let ``test Array.forall2 works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    let ys = [|1.; 2.; 3.; 5.|]
    Array.forall2 (=) xs ys
    |> equal false
    Array.forall2 (fun x y -> x <= 4. && y <= 5.) xs ys
    |> equal true

[<Fact>]
let ``test Array.init works`` () =
    let xs = Array.init 4 (float >> sqrt)
    xs.[0] + xs.[1]
    |> equal 1.
    (xs.[2] > 1. && xs.[3] < 2.)
    |> equal true

[<Fact>]
let ``test Array.isEmpty works`` () =
    Array.isEmpty [|"a"|] |> equal false
    Array.isEmpty [||] |> equal true

(*
[<Fact>]
let ``test Array.iter works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    let total = ref 0.
    xs |> Array.iter (fun x ->
       total := !total + x
    )
    !total |> equal 10.

[<Fact>]
let ``test Array.iter2 works`` () =
    let xs = [|1; 2; 3; 4|]
    let mutable total = 0
    Array.iter2 (fun x y ->
       total <- total - x - y
    ) xs xs
    total |> equal -20

[<Fact>]
let ``test Array.iteri works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    let total = ref 0.
    xs |> Array.iteri (fun i x ->
       total := !total + (float i) * x
    )
    !total |> equal 20.

[<Fact>]
let ``test Array.iteri2 works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    let total = ref 0.
    Array.iteri2 (fun i x y ->
       total := !total + (float i) * x + (float i) * y
    ) xs xs
    !total |> equal 40.
*)
[<Fact>]
let ``test Array.length works`` () =
    let xs = [|"a"; "a"; "a"; "a"|]
    Array.length xs |> equal 4

// [<Fact>]
// let ``test Array.countBy works`` () =
//     let xs = [|1; 2; 3; 4|]
//     xs |> Array.countBy (fun x -> x % 2)
//     |> Array.length |> equal 2

[<Fact>]
let ``test Array.map works`` () =
    let xs = [|1.|]
    let ys = xs |> Array.map (fun x -> x * 2.)
    ys.[0] |> equal 2.

[<Fact>]
let ``test Array.map doesn't execute side effects twice`` () = // See #1140
    let mutable c = 0
    let i () = c <- c + 1; c
    [| i (); i (); i () |] |> Array.map (fun x -> x + 1) |> ignore
    equal 3 c

[<Fact>]
let ``test Array.map2 works`` () =
    let xs = [|1.|]
    let ys = [|2.|]
    let zs = Array.map2 (*) xs ys
    zs.[0] |> equal 2.

[<Fact>]
let ``test Array.map3 works`` () =
    let value1 = [|1.|]
    let value2 = [|2.|]
    let value3 = [|3.|]
    let zs = Array.map3 (fun a b c -> a * b * c) value1 value2 value3
    zs.[0] |> equal 6.

[<Fact>]
let ``test Array.mapi works`` () =
    let xs = [|1.; 2.|]
    let ys = xs |> Array.mapi (fun i x -> float i + x)
    ys.[1] |> equal 3.

[<Fact>]
let ``test Array.mapi2 works`` () =
    let xs = [|1.; 2.|]
    let ys = [|2.; 3.|]
    let zs = Array.mapi2 (fun i x y -> float i + x * y) xs ys
    zs.[1] |> equal 7.

[<Fact>]
let ``test Array.mapFold works`` () =
    let xs = [|1y; 2y; 3y; 4y|]
    let result = xs |> Array.mapFold (fun acc x -> (x * 2y, acc + x)) 0y
    fst result |> Array.sum |> equal 20y
    snd result |> equal 10y

[<Fact>]
let ``test Array.mapFoldBack works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    let result = Array.mapFoldBack (fun x acc -> (x * -2., acc - x)) xs 0.
    fst result |> Array.sum |> equal -20.
    snd result |> equal -10.

[<Fact>]
let ``test Array.max works`` () =
    let xs = [|1.; 2.|]
    xs |> Array.max
    |> equal 2.

[<Fact>]
let ``test Array.maxBy works`` () =
    let xs = [|1.; 2.|]
    xs |> Array.maxBy (fun x -> -x)
    |> equal 1.

[<Fact>]
let ``test Array.min works`` () =
    let xs = [|1.; 2.|]
    xs |> Array.min
    |> equal 1.

[<Fact>]
let ``test Array.minBy works`` () =
    let xs = [|1.; 2.|]
    xs |> Array.minBy (fun x -> -x)
    |> equal 2.

[<Fact>]
let ``test Array.ofList works`` () =
    let xs = [1.; 2.]
    let ys = Array.ofList xs
    ys.Length |> equal 2

[<Fact>]
let ``test Array.ofSeq works`` () =
    let xs = seq { yield 1; yield 2 }
    let ys = Array.ofSeq xs
    ys.[0] |> equal 1

[<Fact>]
let ``test Array.partition works`` () =
    let xs = [|1.; 2.; 3.|]
    let ys, zs = xs |> Array.partition (fun x -> x <= 1.)
    equal ys [| 1. |]
    equal zs [| 2.; 3. |]

[<Fact>]
let ``test Array.permute works`` () =
    let xs = [|1.; 2.|]
    let ys = xs |> Array.permute (fun i -> i + 1 - 2 * (i % 2))
    ys.[0] |> equal 2.

[<Fact>]
let ``test Array.pick works`` () =
    let xs = [|1.; 2.|]
    xs |> Array.pick (fun x ->
        match x with
        | 2. -> Some x
        | _ -> None)
    |> equal 2.

[<Fact>]
let ``test Array.range works`` () =
    [|1..5|]
    |> Array.reduce (+)
    |> equal 15
    [|0..2..9|]
    |> Array.reduce (+)
    |> equal 20

[<Fact>]
let ``test Array.reduce works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    xs |> Array.reduce (-)
    |> equal -8.

[<Fact>]
let ``test Array.reduce Array.append works`` () = // See #2372
    let nums =
        [|
            [| 0 |]
            [| 1 |]
        |]
    Array.reduce Array.append nums |> equal [|0; 1|]

    let nums2d =
        [|
            [| [| 0 |] |]
            [| [| 1 |] |]
        |]
    Array.reduce Array.append nums2d
    |> equal [|[|0|]; [|1|]|]

    let strs =
        [|
            [| "a" |]
            [| "b" |]
        |]
    Array.reduce Array.append strs
    |> equal [|"a"; "b"|]

[<Fact>]
let ``test Array.reduceBack works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    xs |> Array.reduceBack (-)
    |> equal -2.

[<Fact>]
let ``test Array.rev works`` () =
    let xs = [|1.; 2.|]
    let ys = xs |> Array.rev
    xs.[0] |> equal 1. // Make sure there is no side effects
    ys.[0] |> equal 2.

[<Fact>]
let ``test Array.scan works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    let ys = xs |> Array.scan (+) 0.
    ys.[2] + ys.[3]
    |> equal 9.

[<Fact>]
let ``test Array.scanBack works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    let ys = Array.scanBack (-) xs 0.
    ys.[2] + ys.[3]
    |> equal 3.

(*
[<Fact>]
let ``test Array.sort works`` () =
    let xs = [|3; 4; 1; -3; 2; 10|]
    let ys = [|"a"; "c"; "B"; "d"|]
    xs |> Array.sort |> Array.take 3 |> Array.sum |> equal 0
    ys |> Array.sort |> Array.item 1 |> equal "a"

[<Fact>]
let ``test Array.sort with tuples works`` () =
    let xs = [|3; 1; 1; -3|]
    let ys = [|"a"; "c"; "B"; "d"|]
    (xs, ys) ||> Array.zip |> Array.sort |> Array.item 1 |> equal (1, "B")
*)
[<Fact>]
let ``test Array.truncate works`` () =
    let xs = [|1.; 2.; 3.; 4.; 5.|]
    xs |> Array.truncate 2
    |> Array.last
    |> equal 2.

    xs.Length |> equal 5 // Make sure there is no side effects

    // Array.truncate shouldn't throw an exception if there're not enough elements
    try xs |> Array.truncate 20 |> Array.length with _ -> -1
    |> equal 5

(*
[<Fact>]
let ``test Array.sortDescending works`` () =
    let xs = [|3; 4; 1; -3; 2; 10|]
    xs |> Array.sortDescending |> Array.take 3 |> Array.sum |> equal 17
    xs.[0] |> equal 3  // Make sure there is no side effects

    let ys = [|"a"; "c"; "B"; "d"|]
    ys |> Array.sortDescending |> Array.item 1 |> equal "c"

[<Fact>]
let ``test Array.sortBy works`` () =
    let xs = [|3.; 4.; 1.; 2.|]
    let ys = xs |> Array.sortBy (fun x -> -x)
    ys.[0] + ys.[1]
    |> equal 7.

[<Fact>]
let ``test Array.sortByDescending works`` () =
    let xs = [|3.; 4.; 1.; 2.|]
    let ys = xs |> Array.sortByDescending (fun x -> -x)
    ys.[0] + ys.[1]
    |> equal 3.

[<Fact>]
let ``test Array.sortWith works`` () =
    let xs = [|3.; 4.; 1.; 2.|]
    let ys = xs |> Array.sortWith (fun x y -> int(x - y))
    ys.[0] + ys.[1]
    |> equal 3.

[<Fact>]
let ``test Array.sortInPlace works`` () =
    let xs = [|3.; 4.; 1.; 2.; 10.|]
    Array.sortInPlace xs
    xs.[0] + xs.[1]
    |> equal 3.

[<Fact>]
let ``test Array.sortInPlaceBy works`` () =
    let xs = [|3.; 4.; 1.; 2.; 10.|]
    Array.sortInPlaceBy (fun x -> -x) xs
    xs.[0] + xs.[1]
    |> equal 14.

[<Fact>]
let ``test Array.sortInPlaceWith works`` () =
    let xs = [|3.; 4.; 1.; 2.; 10.|]
    Array.sortInPlaceWith (fun x y -> int(x - y)) xs
    xs.[0] + xs.[1]
    |> equal 3.
*)

[<Fact>]
let ``test Array.sum works`` () =
    let xs = [|1.; 2.|]
    xs |> Array.sum
    |> equal 3.

[<Fact>]
let ``test Array.sumBy works`` () =
    let xs = [|1.; 2.|]
    xs |> Array.sumBy (fun x -> x * 2.)
    |> equal 6.

[<Fact>]
let ``test Array.sum with non numeric types works`` () =
    let p1 = {x=1; y=10}
    let p2 = {x=2; y=20}
    [|p1; p2|] |> Array.sum |> equal {x=3;y=30}

[<Fact>]
let ``test Array.sumBy with non numeric types works`` () =
    let p1 = {x=1; y=10}
    let p2 = {x=2; y=20}
    [|p1; p2|] |> Array.sumBy Point.Neg |> equal {x = -3; y = -30}

[<Fact>]
let ``test Array.sumBy with numeric projection works`` () =
    let p1 = {x=1; y=10}
    let p2 = {x=2; y=20}
    [|p1; p2|] |> Array.sumBy (fun p -> p.y) |> equal 30

[<Fact>]
let ``test Array.sum with non numeric types works II`` () =
    [|MyNumber 1; MyNumber 2; MyNumber 3|] |> Array.sum |> equal (MyNumber 6)

[<Fact>]
let ``test Array.sumBy with non numeric types works II`` () =
    [|{ MyNumber = MyNumber 5 }; { MyNumber = MyNumber 4 }; { MyNumber = MyNumber 3 }|]
    |> Array.sumBy (fun x -> x.MyNumber) |> equal (MyNumber 12)

[<Fact>]
let ``test Array.toList works`` () =
    let xs = [|1.; 2.|]
    let ys = xs |> Array.toList
    ys.[0] + ys.[1]
    |> equal 3.

[<Fact>]
let ``test Array.toSeq works`` () =
    let xs = [|1.; 2.|]
    let ys = xs |> Array.toSeq
    ys |> Seq.head
    |> equal 1.

[<Fact>]
let ``test Array.tryFind works`` () =
    let xs = [|1.; 2.|]
    xs |> Array.tryFind ((=) 1.)
    |> Option.isSome |> equal true
    xs |> Array.tryFind ((=) 3.)
    |> Option.isSome |> equal false

[<Fact>]
let ``test Array.tryFindIndex works`` () =
    let xs = [|1.; 2.|]
    xs |> Array.tryFindIndex ((=) 2.)
    |> equal (Some 1)
    xs |> Array.tryFindIndex ((=) 3.)
    |> equal None

[<Fact>]
let ``test Array.tryPick works`` () =
    let xs = [|1.; 2.|]
    let r = xs |> Array.tryPick (fun x ->
        match x with
        | 2. -> Some x
        | _ -> None)
    match r with
    | Some x -> x
    | None -> 0.
    |> equal 2.

[<Fact>]
let ``test Array.unzip works`` () =
    let xs = [|1., 2.|]
    let ys, zs = xs |> Array.unzip
    ys.[0] + zs.[0]
    |> equal 3.

[<Fact>]
let ``test Array.unzip3 works`` () =
    let xs = [|1., 2., 3.|]
    let ys, zs, ks = xs |> Array.unzip3
    ys.[0] + zs.[0] + ks.[0]
    |> equal 6.

[<Fact>]
let ``test Array.zip works`` () =
    let xs = [|1.; 2.; 3.|]
    let ys = [|1.; 2.; 3.|]
    let zs = Array.zip xs ys
    let x, y = zs.[0]
    x + y |> equal 2.

[<Fact>]
let ``test Array.zip3 works`` () =
    let xs = [|1.; 2.; 3.|]
    let ys = [|1.; 2.; 3.|]
    let zs = [|1.; 2.; 3.|]
    let ks = Array.zip3 xs ys zs
    let x, y, z = ks.[0]
    x + y + z |> equal 3.

[<Fact>]
let ``test Array as IList indexer has same behaviour`` () =
    let xs = [|1.; 2.; 3.|]
    let ys = xs :> _ System.Collections.Generic.IList
    ys.[0] <- -3.
    ys.[0] + ys.[2]
    |> equal 0.

[<Fact>]
let ``test Array as IList count has same behaviour`` () =
    let xs = [|1.; 2.; 3.|]
    let ys = xs :> _ System.Collections.Generic.IList
    ys.Count |> equal 3

[<Fact>]
let ``test Array as IList Seq.length has same behaviour`` () =
    let xs = [|1.; 2.; 3.|]
    let ys = xs :> _ System.Collections.Generic.IList
    ys |> Seq.length |> equal 3

[<Fact>]
let ``test Mapping with typed arrays doesnt coerce`` () =
    let data = [| 1 .. 12 |]
    let page size page data =
        data
        |> Array.skip ((page-1) * size)
        |> Array.take size
    let test1 =
        [| 1..4 |]
        |> Array.map (fun x -> page 3 x data)
    let test2 =
        [| 1..4 |]
        |> Seq.map (fun x -> page 3 x data)
        |> Array.ofSeq
    test1 |> Array.concat |> Array.sum |> equal 78
    test2 |> Array.concat |> Array.sum |> equal 78

[<Fact>]
let ``test Array.item works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    Array.item 2 xs |> equal 3.

[<Fact>]
let ``test Array.tryItem works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    Array.tryItem 3 xs |> equal (Some 4.)
    Array.tryItem 4 xs |> equal None
    Array.tryItem -1 xs |> equal None

[<Fact>]
let ``test Array.head works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    Array.head xs |> equal 1.

[<Fact>]
let ``test Array.tryHead works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    Array.tryHead xs |> equal (Some 1.)
    Array.tryHead [||] |> equal None

[<Fact>]
let ``test Array.last works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    xs |> Array.last
    |> equal 4.

[<Fact>]
let ``test Array.tryLast works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    Array.tryLast xs |> equal (Some 4.)
    Array.tryLast [||] |> equal None

[<Fact>]
let ``test Array.tail works`` () =
    let xs = [|1.; 2.; 3.; 4.|]
    Array.tail xs |> Array.length |> equal 3

(*
[<Fact>]
let ``test Array.groupBy returns valid array`` () =
    let xs = [|1; 2|]
    let actual = Array.groupBy (fun _ -> true) xs
    let actualKey, actualGroup = actual.[0]
    let worked = actualKey && actualGroup.[0] = 1 && actualGroup.[1] = 2
    worked |> equal true
*)

[<Fact>]
let ``Array.windowed works`` () = // See #1716
    let nums = [| 1.0; 1.5; 2.0; 1.5; 1.0; 1.5 |]
    Array.windowed 3 nums |> equal [|[|1.0; 1.5; 2.0|]; [|1.5; 2.0; 1.5|]; [|2.0; 1.5; 1.0|]; [|1.5; 1.0; 1.5|]|]
    Array.windowed 5 nums |> equal [|[| 1.0; 1.5; 2.0; 1.5; 1.0 |]; [| 1.5; 2.0; 1.5; 1.0; 1.5 |]|]
    Array.windowed 6 nums |> equal [|[| 1.0; 1.5; 2.0; 1.5; 1.0; 1.5 |]|]
    Array.windowed 7 nums |> Array.isEmpty |> equal true
