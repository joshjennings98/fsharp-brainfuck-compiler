(*
This is a program for taking brainfuck code and turning it into C.

This code is purposefully written quite verbosely since brainfuck compilers have been made that are only 100 - 200 bytes so there is no point in making it super small.

Todo:
- Account for wrapping of bytes.
- Add compile arguments for brainfuck such as array size.
- Add file input and output rather than just terminal input.
- Add more optimisations.
*)

open System
open Tokeniser
open Optimiser
open Compiler

[<EntryPoint>]
let main argv =
    
    Console.Write("Input brainfuck string: ")
    let input = Console.ReadLine()
    Console.Write("\n")
    
    let test = 
        input
        |> tokenise
        |> optimise true
        |> compile
        |> function
            | Ok x -> x
            | Error er -> er |> string |> (+) "Error: "
    printf "%A \n" test
    
    System.Console.ReadKey() |> ignore
    0 // return an integer exit code
