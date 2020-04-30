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
    
    let input, saveToFile = 
        match Array.length argv with
        | 1 -> 
            Console.Write("Input brainfuck string: ")
            Console.ReadLine(), false
        | 2 -> 
            if IO.File.Exists (argv.[1] + ".bf")
            then IO.File.ReadAllText (argv.[1] + ".bf"), true
            else failwithf "%s.bf does not exist.\nYou must either provide an input brainfuck file (without extension) or no file at all for the interactive version.\n" argv.[1]
        | _ -> failwithf "You must either provide an input brainfuck file (without extension) or no file at all for the interactive version.\n"
    
    input
    |> tokenise
    |> optimise true
    |> compile
    |> function
        | Ok code ->
            match saveToFile with
            | true ->
                code
                |> fun asm -> 
                    IO.File.WriteAllLines (@"" + argv.[1] + ".s", [asm])
                printfn "Compiled %s.bf to %s.s" argv.[1] argv.[1]
            | false ->
                Console.Write("\n")
                printfn "%A" code
        | Error er -> 
            er 
            |> string 
            |> failwithf "Error: %A"    
    

    0 // return an integer exit code
