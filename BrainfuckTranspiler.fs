module BrainfuckTranspiler

(*
This code is purposefully written quite verbosely since brainfuck compilers have been made that are only 100 - 200 bytes so there is no point in making it super small.

Todo:
- Account for wrapping of bytes
- Add compile arguments for brainfuck such as array size.
- Turn into something actually usable without just modifying the test variable at the bottom of the file
- Add more optimisations.
*)

//module Types

type SyntaxError =
  | ``Unclosed brackets``

type Instruction =
  | LBracket
  | RBracket
  | IncrementPointer
  | DecrementPointer
  | IncrementCell
  | DecrementCell
  | WriteChar
  | GetChar
  | Add of int * int //Add second int to (pointer location + first int)
  | Sub of int * int //Subtract second int to (pointer location + first int)
  | Set of int //Set cell at pointer to first int

type Token = Instruction * int

let optimiseCode = true

//module Tokeniser

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

//module Optimiser

let optimise (input : Instruction list) : Result<Token list, SyntaxError> =
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

//module Compiler

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

//main()

let test = 
  ">[-+---]"
  |> tokenise
  |> optimise
  |> compile

printf "%A \n" test
  
