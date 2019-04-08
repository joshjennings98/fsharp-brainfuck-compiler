module BrainfuckCompiler
(*
To Do: 
- Probably account for wrapping of bytes
- Implement compilation for getting inputs and outputs
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

type Token = Instruction * int

let filterByList (list : 'a list) (input : 'a list) : 'a list = 
    List.filter (fun el1 -> (List.exists (fun el2 -> el2 = el1) list)) input

let tokenise (input : string) : Result<Token list, SyntaxError> =
  let mutable openBrackets = 0
  let validChars = ['['; ']';'>'; '<'; '+'; '-'; '.'; ',']
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
  let inputList = 
    input 
    |> Seq.toList 
    |> filterByList validChars 
    |> List.map toInstruction
  let concat (x : Instruction) = function
    | (i, count) :: t when i = x && i <> LBracket && i <> RBracket -> (i, count + 1) :: t 
    | t -> (x, 1) :: t
  let tokenList = List.foldBack concat inputList []
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
      sprintf "; If byte in cell %A = 0 then goto to instruction after 'EXIT%A' \nENTER%A \nLDR R1, R0 \nCMP R1, #0 \nBLE EXIT%A \n\n" pointer loopNumber loopNumber loopNumber
    | (RBracket, i) -> 
      let currentLoopValue = loopStack.Head
      loopStack <- loopStack.Tail
      sprintf "; Goto to 'ENTER%A' \nB ENTER%A \nEXIT%A \n\n" currentLoopValue currentLoopValue currentLoopValue
    | (IncrementPointer, i) -> 
      pointer <- i + pointer 
      sprintf "; Increment pointer by %A. Pointer now at 0x%A. \nLDR R0, =0x%A \n\n" i ((256 * pointer).ToString("X")) ((256 * pointer).ToString("X"))
    | (DecrementPointer, i) -> 
      pointer <- pointer - i 
      sprintf "; Decrement pointer by %A. Pointer now at 0x%A. \nLDR R0, =0x%A \n\n" i ((256 * pointer).ToString("X")) ((256 * pointer).ToString("X"))
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
  "[>>>>[<<[>>>>]>>>+++<<---<<<]-]>-"
  |> tokenise 
  |> compile

printf "%A" test

  
