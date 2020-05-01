(*
Module for defining all the types that are used.
*)

module Types


type SyntaxError =
    | ``Unclosed brackets``


type Instruction =
    | LB
    | RB
    | IncPtr
    | DecPtr
    | IncLoc
    | DecLoc
    | Write
    | Get
    | Add of int * int //Add second int to (pointer location + first int)
    | Sub of int * int //Subtract second int to (pointer location + first int)
    | Set of int //Set cell at pointer to first int


type Token = Instruction * int