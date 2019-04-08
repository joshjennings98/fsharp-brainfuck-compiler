module BrainfuckCompiler
(*
To Do: 
- Probably account for wrapping of bytes
- Implement compilation for: [ ] . ,
*)

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

let tokenise (input : string) : Token list =
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
    | (i, count) :: t when i = x && x <> LBracket && x <> RBracket -> (i, count + 1) :: t 
    | t -> (x, 1) :: t
  List.foldBack concat inputList []

let compile (input : Token list) : string =
  let mutable pointer = 0
  let compileInstruction (token : Token) = 
    match token with
    | (LBracket, i) -> failwithf "Not yet implemented."
    | (RBracket, i) -> failwithf "Not yet implemented."
    | (IncrementPointer, i) -> 
      pointer <- i + pointer 
      sprintf "; Increment pointer by %A. Pointer now at %A. \nLDR R0, =0x%A \n" i pointer (pointer.ToString("X"))
    | (DecrementPointer, i) -> 
      pointer <- pointer - i 
      sprintf "; Decrement pointer by %A. Pointer now at %A. \nLDR R0, =0x%A \n" i pointer (pointer.ToString("X"))
    | (IncrementCell, i) -> 
      sprintf "; Increment cell at memory location 0x%A by %A. \nLDR R1, [R0] \nADD R1, R1, 0x%A \nSTR R1, [R0] \n" (pointer.ToString("X")) i (i.ToString("X")) //Check what memory locations go up in
    | (DecrementCell, i) -> 
      sprintf "; Decrement cell at memory location 0x%A by %A. \nLDR R1, [R0] \nSUB R1, R1, 0x%A \nSTR R1, [R0] \n" (pointer.ToString("X")) i (i.ToString("X")) //Check what memory locations go up in
    | (WriteChar, i) -> failwithf "Not yet implemented."
    | (GetChar, i) -> failwithf "Not yet implemented."
    | _ ->failwithf "Will never happen."
  input 
  |> List.map (fun el -> compileInstruction el)
  |> List.reduce (+)


//printf "%A" (">>>>>>>+++<<-----" |> tokenise |> compile)

  
