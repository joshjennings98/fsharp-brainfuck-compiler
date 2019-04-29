(*
This is a program for taking brainfuck code and turning it into C.

This code is purposefully written quite verbosely since brainfuck compilers have been made that are only 100 - 200 bytes so there is no point in making it super small.

Todo:
- Account for wrapping of bytes
- Add compile arguments for brainfuck such as array size.
- Add proper input.
- Add more optimisations.
*)

open Tokeniser
open Optimiser
open Compiler

[<EntryPoint>]
let main argv =
    
    let test = 
        ">[-+---]" //Edit this to change desired input
        |> tokenise
        |> optimise true
        |> compile
    printf "%A \n" test

    0 // return an integer exit code
