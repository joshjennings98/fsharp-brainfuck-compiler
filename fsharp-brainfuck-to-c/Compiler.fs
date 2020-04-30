(*
Module for compiling a list of tokens into C.
*)

module Compiler


open Types


let compile (input : Result<Token list, SyntaxError>) : Result<string, SyntaxError> =
    let mutable ptr = 0

    let mutable nesting = 1

    let compileInstruction (token : Token) = 
        match token with
        | (LB, _) -> 
            nesting <- nesting + 1
            sprintf "%Awhile (*ptr) { \n" (List.init (nesting - 1) (fun el -> "\t") |> List.reduce (+))
        | (RB, _) -> 
            nesting <- nesting - 1
            sprintf "%A} \n" (List.init (nesting + 1) (fun el -> "\t") |> List.reduce (+))
        | (IncPtr, i) -> 
            ptr <- ptr + i
            sprintf "%Aptr += %A; \n" (List.init (nesting) (fun el -> "\t") |> List.reduce (+)) i 
        | (DecPtr, i) -> 
            ptr <- ptr - i
            sprintf "%Aptr -= %A; \n" (List.init (nesting) (fun el -> "\t") |> List.reduce (+)) i 
        | (IncLoc, i) -> 
            sprintf "%Aptr[%A] += %A; \n" (List.init (nesting) (fun el -> "\t") |> List.reduce (+)) ptr i
        | (DecLoc, i) -> 
            sprintf "%Aptr[%A] -= %A; \n" (List.init (nesting) (fun el -> "\t") |> List.reduce (+)) ptr i
        | (Write, _) -> 
            sprintf "%Aputchar(*ptr); \n" (List.init (nesting) (fun el -> "\t") |> List.reduce (+))
        | (Get, _) -> 
            sprintf "%A*ptr = Get(); \n" (List.init (nesting) (fun el -> "\t") |> List.reduce (+))
        | (Set x, y) -> 
            sprintf "%Aptr[%A] = %A \n" (List.init (nesting) (fun el -> "\t") |> List.reduce (+)) (ptr + y - 1) x
        | (Add (x, y), _) -> 
            sprintf "%Aptr[%A] += %A \n" (List.init (nesting) (fun el -> "\t") |> List.reduce (+)) (ptr + x) y
        | (Sub (x, y), _) -> 
            sprintf "%Aptr[%A] -= %A \n" (List.init (nesting) (fun el -> "\t") |> List.reduce (+)) (ptr + x) y
    
    match input with
    | Ok x ->
        x 
        |> List.map compileInstruction
        |> List.append ["#include <stdio.h> \n\n"; "#define SIZE 30000 \n\n"; "int main(void) { \n"; "\tchar array[SIZE] = {0}; \n"; "\tchar *ptr = array; \n\n"]
        |> List.reduce (+)
        |> fun el -> el + "\n\treturn 0; \n} \n"
        |> Ok
    | Error e -> e |> Error