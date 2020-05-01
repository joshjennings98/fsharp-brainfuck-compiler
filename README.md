# F# Brainfuck Compiler

An optimising brainfuck to x86 assembly compiler written in F#.

- [Brainfuck Compiler]()
  - [Introduction](#introduction)
  - [Implementation](#implementation)
  - [Installation and Usage](#installation-and-usage)
  - [Acknowledgements etc.](#acknowledgements)

## Introduction

Brainfuck is a very simple programming language that consists of only 8 instructions (`>`, `<`, `+`, `-`, `.`, `,` `[`, and `]`) and a data pointer.

Brainfuck works as follows:

|  Character  |       Meaning      |
|:-----------:|:------------------:|
|     `>`     | increment the data pointer. |
|     `<`     | decrement the data pointer. |
|     `+`     | decrement the byte at the data pointer. |
|     `-`     | increment the byte at the data pointer. |
|     `.`     | output the byte at the data pointer. |
|     `,`     | accept one byte of input, storing its value in the byte at the data pointer. |
|     `[`     | if the byte at the data pointer is zero then jump the pointer to the command after the matching `]` command. |
|     `]`     | if the byte at the data pointer is nonzero then jump the pointer back to the command after the matching `[` command. |

This program optimises the brainfuck program based on several simple optimisations, and compiles into x86 assembler code. Information on how the optimisations work can be seen in the [optimiser part](#optimiserfs) of implementation section.

## Implementation

The program is split up into different modules containing the functions for various parts of the compiler.

### Program.fs

This module is the entrance to the program. This module interprets any input arguments. It then runs the tokeniser, if desired the optimiser,  and compiler. If there is an error then it will be returned here.  If the compilation was successful then the output is saved to a file. 

More information on how to use the program can be seen in the [installation and usage section.](#installation-and-usage)

### Types.fs

The types defined are the `Token` type and the `Instruction` types:
* The `Token` type is a tuple of an `Instruction` and an integer that represents how many times that instruction is called at once.
* The `Instruction` type contains representations for: pointer increment; pointer decrement; cell increment; cell decrement; loop start; loop end; input char; output char; addition; subtraction; and set. The final three are pseudo instructions that allow a simplified output when compiling.

### Tokeniser.fs

This module generates a list of basic tokens from the input string. Brainfuck documentation says that all invalid characters are ignored and so part of the tokenisers job is to filter out all invalid characters.

```fsharp
let validChars = // Used to filter invalid characters
        ['['; ']';'>'; '<'; '+'; '-'; '.'; ',']

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
	| _ -> failwithf "Won't happen since all other characters are filtered out prior to this being evaluated."
```

### Optimiser.fs

This module applies several optimisations to the tokenised list:
* Fusing cell increments and cell decrements. For example, `++++` can be transformed into `p[0] += 4` and `+++--` can be converted into `p[0] += 1`.
* Fusing pointer increments and pointer decrements. For example, `>>` can be transformed into `p += 2` and `><<<` can be converted into `p-= 2`.
* Fusing movements into additions and subtractions. Some brainfuck strings can be interpretted as addition and subtraction since the overall pointer movement is zero. For example `>>---<<` results in the pointer ending up where it started, therefore it can become `p[2] -= 3`. The same can be done with addition.
* Postponing movements. If a string of brainfuck consists of only increments, decrements, inputs, and outputs, then the virtual movement of the pointer can be kept track of then performed in one shot at the end. For example `>+>->` becomes `p[1]+=1; p[2]-=1; p+=3;`.
* Setting values to zero. Due to number wrapping, `[-]` or any loop that only contains arithmetic will eventually go to zero.

Sevaral more optimisations could be implemented:
* Simple loops. If the loop body has no subloops and no inputs or outputs and all the movements add up to 0 and all the increments/decrements at `p[0]` add up to −1, then loop body is being run `p[0]` times. So for all the increments/decrements outside of `p[0]`, some multiple of `p[0]` is being added to that location. For example, `[->+>---<<]` becomes `p[1]+=p[0]; p[2]-=p[0]*3; p[0]=0;`.
* Complex loops. If a location other than the loop counter is used as the source for multiply-add and this location is cleared to 0 within the loop, then the loop can be turned into an if statement.

### Compiler.fs

This module turns the token list into a list of assembly instructions and then outputs it as a string. It keeps track of loops using several mutable variables.

The way that the assembly for each possible instruction was determined was using [this online C decompiler.](https://godbolt.org/)

The 8 brainfuck instructions have direct C equivalents:

|  Brainfuck  |    C equivalent   |
|:-----------:|:-----------------:|
|     `>`     |      `ptr++;`     |
|     `<`     |      `ptr--;`     |
|     `+`     |     `*ptr++;`     |
|     `-`     |     `*ptr--;`     |
|     `.`     |  `putchar(*ptr);` |
|     `,`     | `*ptr=getchar();` |
|     `[`     |  `while (*ptr) {` |
|     `]`     |        `}`        |

Using the online decompiler, I determined what each instruction corresponed to in x86 assembly and then used that in the code.

* The `>` instruction becomes:
```asm
addq    $AMOUNT mod 256, -8(%rbp) ; modulo to act as byte wrapping
```
* The `<` instruction becomes:
```asm
subq    $AMOUNT mod 256, -8(%rbp) ; modulo to act as byte wrapping
```
* The `+` instruction becomes:
```asm
movq    -8(%rbp), %rax
movzbl  (%rax), %eax
addl    $AMOUNT, %eax ; the optimiser combines multiple +
movl    %eax, %edx
movq    -8(%rbp), %rax
movb    %dl, (%rax)
```
* The `-` instruction becomes:
```asm
movq    -8(%rbp), %rax
movzbl  (%rax), %eax
subl    $AMOUNT, %eax ; the optimiser combines multiple -
movl    %eax, %edx
movq    -8(%rbp), %rax
movb    %dl, (%rax)
```
* The `.` instruction becomes:
```asm
movq    -8(%rbp), %rax
movzbl  (%rax), %eax
movsbl  %al, %eax
movl    %eax, %edi
call    putchar
```
* The `,` instruction becomes:
```asm
call    getchar
movl    %eax, %edx
movq    -8(%rbp), %rax
movb    %dl, (%rax)
```
* The `[` instruction becomes:
```asm
.LA
movq    -8(%rbp), %rax
movzbl  (%rax), %eax
testb   %al, %al
je      .LB
```
* The `]` instruction becomes:
```asm
jmp     .LA
.LB
```
* The optimiser gives the ability to add or subtract an `AMOUNT` to a tape location `OFFSET` away from the pointer `PTR`. This is implemented using:
```asm
movq    -8(%rbp), %rax
addq    $PTRaddOFFSET, %rax
movzbl  (%rax), %eax
leal    AMMOUNT(%rax), %edx ; negative amount for subtraction
movq    -8(%rbp), %rax
addq    $PTRaddOFFSET, %rax
movb    %dl, (%rax)
```

The neccesary header and footer information for the assmbler to link the code correctly is also added in this function.

## Installation and Usage

**Prerequisites**

To run this code, you will require the .NET SDK version 2.1. Information on the installation of this can be found on [this page.](https://dotnet.microsoft.com/download)

**Usage**

The command to run the program is:
```bash
dotnet run fsharp-brainfuck-compiler.fsproj
```

You can run this program with several command line arguments:
* If nothing is specified, then it will open in interactive mode where the output will be printed to the terminal instead of a file.
* If only a name is provided, then the program will be compiled to a `.s` file of the same name. The brainfuck will be optimised and the tape size will be set to 30000.
* A name can be provided along with either the tape size (an integer greater than 16) or whether to optimise (`no-optimise` argument).
* A name can be provided along with both the tape size and optimise argument. In this case, it doesn't matter what the order of the last two arguments is.

To compile files, GCC is required. Once you have run the program and got an output file, to link, assemble, and run it:

```bash
gcc -no-pie <file>.s -o <file> && ./<file>
```

An example Brainfuck program is included in `helloworld.bf`. This is well commented and explains what is going on. As well as this, there is a script that will compile and run the included example file in `test.sh`. To run this script run

```bash
sh test.sh
```

## Acknowledgements

Ideas for optimisations came from [https://www.nayuki.io/page/optimizing-brainfuck-compiler](https://www.nayuki.io/page/optimizing-brainfuck-compiler)
