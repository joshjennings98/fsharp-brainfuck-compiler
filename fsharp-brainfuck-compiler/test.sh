# Compile BF to ASM
dotnet run fsharp-brainfuck-to-c.fsproj helloworld.bf

# Link and assemble the assembler file
gcc -no-pie helloworld.s -o helloworld

# Run the program
./helloworld