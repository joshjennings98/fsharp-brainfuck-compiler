(*
Module for compiling a list of tokens into C.
*)

module Compiler


open Types


let compile (input : Result<Token list, SyntaxError>) : Result<string, SyntaxError> =
    let mutable ptr = 0
    let mutable ctr = 1
    let mutable nesting = 0
    let mutable labels = []

    let compileLB (_ : unit) = 
        nesting <- nesting + 2
        labels <- List.append labels [ctr; ctr+1]
        ctr <- ctr + 2
        [
            ".L" + (string labels.[nesting-2]) + ":\n";
            "movq    -8(%rbp), %rax\n";
            "movzbl  (%rax), %eax\n";
            "testb   %al, %al\n";
            "je      .L" + (string labels.[nesting-1]) + "\n";
        ]

    let compileRB (_ : unit) = 
        nesting <- nesting - 2
        let s = [
            "jmp     .L" + (string labels.[nesting]) + "\n";
            ".L" + (string labels.[nesting+1]) + ":\n";
        ]
        labels <- List.filter (fun el -> el <> labels.[nesting] && el <> labels.[nesting+1]) labels
        s


    let compileIncPtr (i : int) =
        ptr <- ptr + i
        [
            "addq    $" + (string i) + ", -8(%rbp)\n"
        ]

    let compileDecPtr (i : int) =
        ptr <- ptr - i
        [
            "subq    $" + (string i) + ", -8(%rbp)\n"
        ]

    let compileIncLoc (i : int) =
        [
            "movq    -8(%rbp), %rax\n";
            "movzbl  (%rax), %eax\n";
            "addl    $"+ (string i) + ", %eax\n";
            "movl    %eax, %edx\n";
            "movq    -8(%rbp), %rax\n";
            "movb    %dl, (%rax)\n";
        ]

    let compileDecLoc (i : int) =
        [
            "movq    -8(%rbp), %rax\n";
            "movzbl  (%rax), %eax\n";
            "subl    $"+ (string i) + ", %eax\n";
            "movl    %eax, %edx\n";
            "movq    -8(%rbp), %rax\n";
            "movb    %dl, (%rax)\n";
        ]

    let compileAdd (i : int) (loc : int) =
        [
            "movq    -8(%rbp), %rax\n";
            "addq    $" + (string loc) + ", %rax\n";
            "movzbl  (%rax), %eax\n";
            "leal    "+ (string i) + "(%rax), %edx\n";
            "movq    -8(%rbp), %rax\n";
            "addq    $" + (string loc) + ", %rax\n";
            "movb    %dl, (%rax)\n";
        ]

    let compileSub (i : int) (loc : int) =
        [
            "movq    -8(%rbp), %rax\n";
            "addq    $" + (string loc) + ", %rax\n";
            "movzbl  (%rax), %eax\n";
            "leal    -"+ (string i) + "(%rax), %edx\n";
            "movq    -8(%rbp), %rax\n";
            "addq    $" + (string loc) + ", %rax\n";
            "movb    %dl, (%rax)\n";
        ]

    let compileSet (i : int) (loc : int) =
        [
            "movq    -8(%rbp), %rax\n";
            "addq    $" + (string loc) + ", %rax\n";
            "movb    $" + (string i) + ", (%rax)\n";
        ]

    let compilePut =
        [
            "movq    -8(%rbp), %rax\n";
            "movzbl  (%rax), %eax\n";
            "movsbl  %al, %eax\n";
            "movl    %eax, %edi\n";
            "call    putchar\n";
        ]

    let compileGet = 
        [
            "call    getchar\n";
            "movl    %eax, %edx\n";
            "movq    -8(%rbp), %rax\n";
            "movb    %dl, (%rax)\n";
        ]

    let compileInstruction (token : Token) = 
        match token with
        | (LB, _) -> 
            compileLB()
        | (RB, _) -> 
            compileRB()
        | (IncPtr, i) -> 
            compileIncPtr i
        | (DecPtr, i) -> 
            compileDecPtr i
        | (IncLoc, i) ->
            compileIncLoc i
        | (DecLoc, i) -> 
            compileDecLoc i
        | (Write, _) -> 
            compilePut
        | (Get, _) -> 
            compileGet
        | (Set x, y) -> 
            compileSet x (ptr + y - 1)
        | (Add (x, y), _) -> 
            compileAdd y (ptr + x - 1)
        | (Sub (x, y), _) -> 
            compileSub y (ptr + x - 1)

    let headerStuff = [
        ".data\n";
        ".text\n";
        ".global main\n\n";
        "main:\n";
        "    pushq   %rbp\n";
        "    movq    %rsp, %rbp\n";
        "    subq    $30016, %rsp\n";
        "    movq    $0, -30016(%rbp)\n";
        "    movq    $0, -30008(%rbp)\n";
        "    leaq    -30000(%rbp), %rax\n";
        "    movl    $29984, %edx\n";
        "    movl    $0, %esi\n";
        "    movq    %rax, %rdi\n";
        "    call    memset\n";
        "    leaq    -30016(%rbp), %rax\n";
        "    movq    %rax, -8(%rbp)\n";
    ]

    match input with
    | Ok x ->
        x 
        |> List.collect compileInstruction
        |> List.map (fun s -> "    " + s)
        |> List.append headerStuff
        |> List.reduce (+)
        |> fun s -> s + "    movl $0, %eax\n    leave\n    ret"
        |> Ok
    | Error e -> e |> Error

