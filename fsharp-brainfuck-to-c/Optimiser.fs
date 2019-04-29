(*
Module for applying various optimisations to the initial token list. See README for details.
*)

module Optimiser

open Types

let optimise (optimiseCode : bool) (input : Instruction list) : Result<Token list, SyntaxError> =
  let concatIncrDecr (x : Instruction) = function
    | (i, count) :: t when i = x && i <> LBracket && i <> RBracket -> (i, count + 1) :: t 
    | t -> (x, 1) :: t
  let mergeSuccessiveIncrDecr (list : Token list) = 
    let rec loop acc list =
      match list with
      | x1::x2::xs ->
        match (x1 |> fst), (x2 |> fst) with
        | IncrementCell, DecrementCell -> loop acc ((IncrementCell, (x1 |> snd) - (x2 |> snd))::xs)
        | DecrementCell, IncrementCell -> loop acc ((IncrementCell, (x2 |> snd) - (x1 |> snd))::xs)
        | IncrementPointer, DecrementPointer -> loop acc ((IncrementPointer, (x1 |> snd) - (x2 |> snd))::xs)
        | DecrementPointer, IncrementPointer -> loop acc ((IncrementPointer, (x2 |> snd) - (x1 |> snd))::xs)
        | _ -> loop (x1::acc) (x2::xs)
      | x::xs -> loop (x::acc) xs
      | _ -> acc |> List.rev
    loop [] list
  let simplifyAddSub (list : Token list) = 
    let rec loop acc list =
      match list with 
      | x1::x2::x3::xs -> 
        match (x1 |> fst), (x2 |> fst), (x3 |> fst) with
        | IncrementPointer, IncrementCell, DecrementPointer ->
          let addition = Add ((x1 |> snd),(x2 |> snd))
          loop acc ((addition, 1)::(IncrementPointer, (x1 |> snd) - (x3 |> snd))::xs)  
        | DecrementPointer, IncrementCell, IncrementPointer ->
          let addition = Add (-1 * (x1 |> snd),(x2 |> snd))
          loop acc ((addition, 1)::(IncrementPointer, (x3 |> snd) - (x1 |> snd))::xs)
        | IncrementPointer, IncrementCell, IncrementPointer ->
          let addition = Add ((x1 |> snd),(x2 |> snd))
          loop acc ((addition, 1)::(IncrementPointer, (x3 |> snd) + (x1 |> snd))::xs)
        | DecrementPointer, IncrementCell, DecrementPointer ->
          let addition = Add (-1 * (x1 |> snd),(x2 |> snd))
          loop acc ((addition, 1)::(DecrementPointer, (x3 |> snd) + (x1 |> snd))::xs)
        | LBracket, IncrementCell, RBracket ->
          loop acc ((Set 0, 1)::xs)
        | IncrementPointer, DecrementCell, DecrementPointer ->
          let subtraction = Sub ((x1 |> snd),(x2 |> snd))
          loop acc ((subtraction, 1)::(IncrementPointer, (x1 |> snd) - (x3 |> snd))::xs)  
        | DecrementPointer, DecrementCell, IncrementPointer ->
          let subtraction = Add (-1 * (x1 |> snd),(x2 |> snd))
          loop acc ((subtraction, 1)::(IncrementPointer, (x3 |> snd) - (x1 |> snd))::xs)
        | IncrementPointer, DecrementCell, IncrementPointer ->
          let subtraction = Add ((x1 |> snd),(x2 |> snd))
          loop acc ((subtraction, 1)::(IncrementPointer, (x3 |> snd) + (x1 |> snd))::xs)
        | DecrementPointer, DecrementCell, DecrementPointer ->
          let subtraction = Add (-1 * (x1 |> snd),(x2 |> snd))
          loop acc ((subtraction, 1)::(DecrementPointer, (x3 |> snd) + (x1 |> snd))::xs)
        | LBracket, DecrementCell, RBracket ->
          loop acc ((Set 0, 1)::xs)        
        | _ -> loop (x1::acc) (x2::x3::xs)
      | x::xs -> loop (x::acc) xs
      | _ -> acc |> List.rev
    loop [] list
  let removeBeforeSet (list : Token list) = 
    let rec loop acc list =
      match list with
      | x1::x2::xs ->
        match (x1 |> fst), (x2 |> fst) with
        | IncrementCell, Set i -> loop acc ((Set i, (x2 |> snd))::xs)
        | DecrementCell, Set i -> loop acc ((Set i, (x2 |> snd))::xs)
        | _ -> loop (x1::acc) (x2::xs)
      | x::xs -> loop (x::acc) xs
      | _ -> acc |> List.rev
    loop [] list
  
  let fixIncrDecr (x : Token) = //This function isn't really neccessary but it makes it look nicer when its tokenised
    match x with
    | (IncrementCell, i) when i < 0 -> (DecrementCell, -1 * i)
    | (DecrementCell, i) when i < 0 -> (IncrementCell, -1 * i)
    | (IncrementPointer, i) when i < 0 -> (DecrementPointer, -1 * i)
    | (DecrementPointer, i) when i < 0 -> (IncrementPointer, -1 * i)
    | (Add(x, y), z) when y < 0 -> (Sub(x, y * -1), z)
    | (Sub(x, y), z) when y < 0 -> (Add(x, y * -1), z)
    | _ -> x
  let tokenList = 
    []
    |> fun el ->
       match optimiseCode with
       | true -> 
         el
         |> List.foldBack concatIncrDecr input
         |> mergeSuccessiveIncrDecr
         |> simplifyAddSub
         |> removeBeforeSet
         |> List.map fixIncrDecr
         |> List.filter (fun el -> (el |> snd) <> 0)
       | false -> 
         input
         |> List.map (fun el -> (el, 1))
  if (List.fold (fun acc el -> 
    match (el |> fst) with 
    | LBracket -> acc + 1 
    | RBracket -> acc - 1
    | _ -> acc
  ) 0 tokenList) = 0
  then tokenList |> Ok
  else ``Unclosed brackets`` |> Error