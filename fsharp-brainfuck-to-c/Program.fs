(*
This is a program for taking brainfuck code and turning it into assembly language.
*)

open System
open Tokeniser
open Optimiser
open Compiler

[<EntryPoint>]
let main argv =
    
    let input, saveToFile, optimiseBool, size = 
        match Array.length argv with
        | 1 -> 
            Console.Write("Input brainfuck string: ")
            Console.ReadLine(), false, true, 30000
        | 2 -> 
            if IO.File.Exists argv.[1]
            then 
                IO.File.ReadAllText argv.[1], true, true, 30000
            else 
                failwithf "%s does not exist.\nYou must either provide an input brainfuck file (without extension) or no file at all for the interactive version.\n" argv.[1]
        | 3 -> 
            if IO.File.Exists argv.[1]
            then 
                let optimise, size =
                    match argv.[2] with
                    | "no-optimise" ->
                         false, 30000
                    | x when fst (System.Double.TryParse(x)) -> 
                        true, (int x)
                    | _ -> 
                        true, 30000
                
                IO.File.ReadAllText argv.[1], true, optimise, size
            else 
                failwithf "%s does not exist.\nYou must either provide an input brainfuck file (without extension) or no file at all for the interactive version.\n" argv.[1]
        | 4 ->
            if IO.File.Exists argv.[1]
            then 
                let optimise, size =
                    match argv.[2], argv.[3] with
                    | x, y when x = "no-optimise" && fst (System.Double.TryParse(y)) -> 
                        if (int y) > 15
                        then true, (int y)
                        else failwithf "Minimum array size is 16.\n"
                    | y, x when x = "no-optimise" && fst (System.Double.TryParse(y)) ->
                        if (int y) > 15
                        then true, (int y)
                        else failwithf "Minimum array size is 16.\n"
                    | _ -> 
                        failwithf "If providing 3 arguments, you must first provide the filename and then whether to optimise and the size of the array.\n"
                
                IO.File.ReadAllText argv.[1], true, optimise, size
            else 
                failwithf "%s does not exist.\nYou must either provide an input brainfuck file (without extension) or no file at all for the interactive version.\n" argv.[1]
        | _ -> 
            failwithf "You must either provide an input brainfuck file (without extension) or no file at all for the interactive version.\n"

    input
    |> tokenise
    |> optimise optimiseBool
    |> compile size
    |> function
        | Ok code ->
            match saveToFile with
            | true ->
                code
                |> fun asm -> 
                    IO.File.WriteAllLines (@"" + argv.[1].Substring(0, argv.[1].Length-3) + ".s", [asm])
                printfn "Compiled %s to %s.s" argv.[1] (argv.[1].Substring(0, argv.[1].Length-3))
            | false ->
                Console.Write("\n")
                printfn "%A" code
        | Error er -> 
            er 
            |> string 
            |> failwithf "Error: %A"    
    
    0 // return an integer exit code
