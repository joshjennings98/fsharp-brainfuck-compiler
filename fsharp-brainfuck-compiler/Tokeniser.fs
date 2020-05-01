(*
Module for taking a string and turning it into a list of tokens.
*)

module Tokeniser


open Types


let tokenise (input : string) : Instruction list =
    let validChars = 
        ['['; ']';'>'; '<'; '+'; '-'; '.'; ',']
    
    let filterByList (list : 'a list) (input : 'a list) : 'a list = 
        List.filter (fun el1 -> (List.exists (fun el2 -> el2 = el1) list)) input
    
    let toInstruction (ch : char) : Instruction =
        match ch with
        | '[' -> LB
        | ']' -> RB
        | '>' -> IncPtr
        | '<' -> DecPtr
        | '+' -> IncLoc
        | '-' -> DecLoc
        | '.' -> Write
        | ',' -> Get
        | _ -> failwithf "Shouldn't happen since all other characters are filtered out prior to this branch being evaluated."
    
    input 
    |> Seq.toList 
    |> filterByList validChars 
    |> List.map toInstruction