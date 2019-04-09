module BrainfuckCompiler

(*
This code is purposefully written quite verbosely since brainfuck compilers have been made that are only 100 - 200 bytes so there is no point in making it super small, may as well make it readable.

To Do: 
- Probably account for wrapping of bytes
- Implement compilation for getting inputs and outputs
- Add pointer initialisation so it begins after the start of where the code is stored
- Add extra stuff needed for arm assembly like import I/O stuff?
- Add compile arguments for brainfuck such as cell size, wrapping options, etc.
- Turn into something actually usable without just modifying the test variable at the bottom of the file
- Do a write up in the readme
- Maybe convert tuples to 'of int'
*)

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
    |> List.foldBack concatIncrDecr input
    |> mergeSuccessiveIncrDecr
    |> simplifyAddSub
    |> removeBeforeSet
    |> List.map fixIncrDecr
    |> List.filter (fun el -> (el |> snd) <> 0)
  if (List.fold (fun acc el -> 
    match (el |> fst) with 
    | LBracket -> acc + 1 
    | RBracket -> acc - 1
    | _ -> acc
  ) 0 tokenList) = 0
  then tokenList |> Ok
  else ``Unclosed brackets`` |> Error




let compile (input : Result<Token list, SyntaxError>) : Result<string, SyntaxError> =
  let mutable pointer = 0
  let mutable loopNumber = 0
  let mutable loopStack = []
  let compileInstruction (token : Token) = 
    match token with
    | (LBracket, i) -> 
      loopNumber <- loopNumber + 1
      loopStack <- [loopNumber] @ loopStack
      sprintf "; If byte in cell %A = 0 then goto to instruction after 'EXIT%A' \nENTER%A \nLDR R1, [R0] \nCMP R1, #0 \nBLE EXIT%A \n\n" pointer loopNumber loopNumber loopNumber
    | (RBracket, i) -> 
      let currentLoopValue = loopStack.Head
      loopStack <- loopStack.Tail
      sprintf "; Goto to 'ENTER%A' \nB ENTER%A \nEXIT%A \n\n" currentLoopValue currentLoopValue currentLoopValue
    | (IncrementPointer, i) -> 
      pointer <- i + pointer 
      sprintf "; Increment pointer by %A. Pointer now at 0x%A. \nLDR R0, 0x%A \n\n" i ((256 * pointer).ToString("X")) ((256 * pointer).ToString("X"))
    | (DecrementPointer, i) -> 
      pointer <- pointer - i 
      sprintf "; Decrement pointer by %A. Pointer now at 0x%A. \nLDR R0, 0x%A \n\n" i ((256 * pointer).ToString("X")) ((256 * pointer).ToString("X"))
    | (IncrementCell, i) -> 
      sprintf "; Increment cell %A by %A. \nLDR R1, [R0] \nADD R1, R1, #0x%A \nSTR R1, [R0] \n\n" pointer i (i.ToString("X")) //Check what memory locations go up in
    | (DecrementCell, i) -> 
      sprintf "; Decrement cell %A by %A. \nLDR R1, [R0] \nSUB R1, R1, #0x%A \nSTR R1, [R0] \n\n" pointer i (i.ToString("X")) //Check what memory locations go up in
    | (WriteChar, i) -> failwithf "Not yet implemented."
    | (GetChar, i) -> failwithf "Not yet implemented."
  match input with
  | Ok x ->
    x 
    |> List.map (fun el -> compileInstruction el)
    |> List.reduce (+)
    |> Ok
  | Error e -> e |> Error

let test = 
  ">>><<<<<<++---[---]-->"
  |> tokenise
  |> optimise 
  //|> compile

printf "%A \n" test
  
