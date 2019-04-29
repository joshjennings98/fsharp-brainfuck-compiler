(*
Module for compiling a list of tokens into C.
*)

module Compiler

open Types

let compile (input : Result<Token list, SyntaxError>) : Result<string, SyntaxError> =
  let mutable pointer = 0
  let mutable indentation = 1
  let compileInstruction (token : Token) = 
    match token with
    | (LBracket, i) -> 
      indentation <- indentation + 1
      sprintf "%Awhile (*ptr) { \n" (List.init (indentation - 1) (fun el -> "\t") |> List.reduce (+))
    | (RBracket, i) -> 
      indentation <- indentation - 1
      sprintf "%A} \n" (List.init (indentation + 1) (fun el -> "\t") |> List.reduce (+))
    | (IncrementPointer, i) -> 
      pointer <- pointer + i
      sprintf "%Aptr += %A; \n" (List.init (indentation) (fun el -> "\t") |> List.reduce (+)) i 
    | (DecrementPointer, i) -> 
      pointer <- pointer - i
      sprintf "%Aptr -= %A; \n" (List.init (indentation) (fun el -> "\t") |> List.reduce (+)) i 
    | (IncrementCell, i) -> 
      sprintf "%Aptr[%A] += %A; \n" (List.init (indentation) (fun el -> "\t") |> List.reduce (+)) pointer i
    | (DecrementCell, i) -> 
      sprintf "%Aptr[%A] -= %A; \n" (List.init (indentation) (fun el -> "\t") |> List.reduce (+)) pointer i
    | (WriteChar, i) -> sprintf "%Aputchar(*ptr); \n" (List.init (indentation) (fun el -> "\t") |> List.reduce (+))
    | (GetChar, i) -> sprintf "%A*ptr = getchar(); \n" (List.init (indentation) (fun el -> "\t") |> List.reduce (+))
    | (Set x, y) -> sprintf "%Aptr[%A] = %A \n" (List.init (indentation) (fun el -> "\t") |> List.reduce (+)) (pointer + y - 1) x
    | (Add (x, y), z) -> sprintf "%Aptr[%A] += %A \n" (List.init (indentation) (fun el -> "\t") |> List.reduce (+)) (pointer + x) y
    | (Sub (x, y), z) -> sprintf "%Aptr[%A] -= %A \n" (List.init (indentation) (fun el -> "\t") |> List.reduce (+)) (pointer + x) y
  match input with
  | Ok x ->
    x 
    |> List.map (fun el -> compileInstruction el)
    |> List.append ["#include <stdio.h> \n\n"; "#define SIZE 30000 \n\n"; "int main(void) { \n"; "\tchar array[SIZE] = {0}; \n"; "\tchar *ptr = array; \n\n"]
    |> List.reduce (+)
    |> fun el -> el + "\n\treturn 0; \n} \n"
    |> Ok
  | Error e -> e |> Error