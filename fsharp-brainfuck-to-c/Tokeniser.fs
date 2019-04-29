(*
Module for taking a string and turning it into a list of tokens.
*)

module Tokeniser

open Types

let tokenise (input : string) : Instruction list =
  let mutable openBrackets = 0
  let validChars = ['['; ']';'>'; '<'; '+'; '-'; '.'; ',']
  let filterByList (list : 'a list) (input : 'a list) : 'a list = 
    List.filter (fun el1 -> (List.exists (fun el2 -> el2 = el1) list)) input
  let toInstruction (x : char) : Instruction =
    match x with
    | '[' -> LBracket
    | ']' -> RBracket
    | '>' -> IncrementPointer
    | '<' -> DecrementPointer
    | '+' -> IncrementCell
    | '-' -> DecrementCell
    | '.' -> WriteChar
    | ',' -> GetChar
    | _ -> failwithf "Won't happen since all other characters are filtered out prior to this being called."
  input 
  |> Seq.toList 
  |> filterByList validChars 
  |> List.map toInstruction