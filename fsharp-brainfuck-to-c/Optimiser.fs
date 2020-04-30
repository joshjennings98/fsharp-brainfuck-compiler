(*
Module for applying various optimisations to the initial token list. See README for details.
*)

module Optimiser


open Types


let optimise (optimiseCode : bool) (input : Instruction list) : Result<Token list, SyntaxError> =
    let concatIncrDecr (x : Instruction) = function
        | (i, count) :: t when i = x && i <> LB && i <> RB && i <> Write && i <> Get -> (i, count + 1) :: t 
        | t -> (x, 1) :: t
    
    let mergeSuccessiveIncrDecr (list : Token list) = 
        let rec loop (acc : Token list) (list : Token list) : Token list =
            match list with
            | x1::x2::xs ->
                let instr1, instr2, val1, val2 =
                    fst x1, fst x2, snd x1, snd x2

                match instr1, instr2 with
                | IncLoc, DecLoc -> 
                    (IncLoc, val1 - val2)::xs
                    |> loop acc 

                | DecLoc, IncLoc -> 
                    (IncLoc, val2 - val1)::xs
                    |> loop acc 

                | IncPtr, DecPtr -> 
                    (IncPtr, val1 - val2)::xs
                    |> loop acc 
                    
                | DecPtr, IncPtr -> 
                    (IncPtr, val2 - val1)::xs
                    |> loop acc 

                | _ -> 
                    loop (x1::acc) (x2::xs)
            | x::xs -> 
                loop (x::acc) xs
            | _ -> 
                List.rev acc

        loop [] list
    
    let simplifyAddSub (list : Token list) = 
        let rec loop acc list =
            match list with 
            
            | x1::x2::x3::xs -> 
                let instr1, instr2, instr3, val1, val2, val3 =
                    fst x1, fst x2, fst x3, snd x1, snd x2, snd x3

                match instr1, instr2, instr3 with
                | IncPtr, IncLoc, DecPtr ->
                    let add1 = 
                        Add (val1, val2), 1
                    let newToken = 
                        IncPtr, val1 - val3
                    loop acc (add1::newToken::xs)

                | DecPtr, IncLoc, IncPtr ->
                    let add1 = 
                        Add (-1 * val1, val2), 1
                    let newToken =
                        IncPtr, val3 - val1
                    loop acc (add1::newToken::xs)

                | IncPtr, IncLoc, IncPtr ->
                    let add1 = 
                        Add (val1, val2), 1
                    let newToken =
                        IncPtr, val3 + val1
                    loop acc (add1::newToken::xs)

                | DecPtr, IncLoc, DecPtr ->
                    let add1 = 
                        Add (-1 * val1, val2), 1
                    let newToken =
                        DecPtr, val3 + val1
                    loop acc (add1::newToken::xs)

                | LB, IncLoc, RB ->
                    loop acc ((Set 0, 1)::xs)
                | IncPtr, DecLoc, DecPtr ->
                    let sub1 = 
                        Sub (val1, val2), 1
                    let newToken =
                        IncPtr, val1 - val3
                    loop acc (sub1::newToken::xs)   

                | DecPtr, DecLoc, IncPtr ->
                    let sub1 = 
                        Add (-1 * val1, val2), 1
                    let newToken =
                        IncPtr, val3 - val1
                    loop acc (sub1::newToken::xs)

                | IncPtr, DecLoc, IncPtr ->
                    let sub1 = 
                        Sub (val1, val2), 1
                    let newToken =
                        IncPtr, val3 + val1
                    loop acc (sub1::newToken::xs)

                | DecPtr, DecLoc, DecPtr ->
                    let sub1 = 
                        Add (-1 * val1, val2), 1
                    let newToken =
                        DecPtr, val3 + val1
                    loop acc (sub1::newToken::xs)

                | LB, DecLoc, RB ->
                    let set =
                        (Set 0, 1)
                    loop acc (set::xs)   

                | _ -> 
                    loop (x1::acc) (x2::x3::xs)
            | x::xs -> 
                loop (x::acc) xs
            | _ -> 
                List.rev acc
                
        loop [] list
    
    let removeBeforeSet (list : Token list) = 
        let rec loop acc list =
            match list with
            | x1::x2::xs ->
                let instr1, instr2, val2 =
                    fst x1, fst x2, snd x2

                match instr1, instr2 with
                | IncLoc, Set i -> 
                    let newToken =
                        Set i, val2
                    loop acc (newToken::xs)

                | DecLoc, Set i -> 
                    let newToken =
                        Set i, val2
                    loop acc (newToken::xs)

                | _ -> 
                    loop (x1::acc) (x2::xs)
            | x::xs -> 
                loop (x::acc) xs
            | _ -> 
                List.rev acc
                
        loop [] list
    
    let fixIncrDecr (x : Token) = //This function isn't really neccessary but it makes it look nicer when its tokenised
        match x with
        | (IncLoc, i) when i < 0 -> DecLoc, -1 * i
        | (DecLoc, i) when i < 0 -> IncLoc, -1 * i
        | (IncPtr, i) when i < 0 -> DecPtr, -1 * i
        | (DecPtr, i) when i < 0 -> IncPtr, -1 * i
        | (Add(x, y), z) when y < 0 -> Sub(x, y * -1), z
        | (Sub(x, y), z) when y < 0 -> Add(x, y * -1), z
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
                 |> List.filter (fun el -> snd el <> 0)
             | false -> 
                 input
                 |> List.map (fun el -> el, 1)
    
    if (List.fold (fun acc el -> 
        match fst el with 
        | LB -> acc + 1 
        | RB -> acc - 1
        | _ -> acc
    ) 0 tokenList) = 0
    then 
        Ok tokenList
    else Error ``Unclosed brackets``