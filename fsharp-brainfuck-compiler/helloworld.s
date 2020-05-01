.data
.text
.global main

main:
    pushq   %rbp
    movq    %rsp, %rbp
    subq    $30016, %rsp
    movq    $0, -30016(%rbp)
    movq    $0, -30008(%rbp)
    leaq    -30000(%rbp), %rax
    movl    $29984, %edx
    movl    $0, %esi
    movq    %rax, %rdi
    call    memset
    leaq    -30016(%rbp), %rax
    movq    %rax, -8(%rbp)
    .L1:
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    testb   %al, %al
    je      .L2
    call    getchar
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    .L3:
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    testb   %al, %al
    je      .L4
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    jmp     .L3
    .L4:
    call    getchar
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    call    getchar
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    call    getchar
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    call    getchar
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    addl    $1, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    call    getchar
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    subl    $1, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    call    getchar
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    call    getchar
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    .L5:
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    testb   %al, %al
    je      .L6
    jmp     .L5
    .L6:
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    jmp     .L1
    .L2:
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    addl    $8, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    .L7:
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    testb   %al, %al
    je      .L8
    addq    $1, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    addl    $4, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    .L9:
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    testb   %al, %al
    je      .L10
    movq    -8(%rbp), %rax
    addq    $1, %rax
    movzbl  (%rax), %eax
    leal    2(%rax), %edx
    movq    -8(%rbp), %rax
    addq    $1, %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    addq    $2, %rax
    movzbl  (%rax), %eax
    leal    3(%rax), %edx
    movq    -8(%rbp), %rax
    addq    $2, %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    addq    $3, %rax
    movzbl  (%rax), %eax
    leal    3(%rax), %edx
    movq    -8(%rbp), %rax
    addq    $3, %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    addq    $4, %rax
    movzbl  (%rax), %eax
    leal    1(%rax), %edx
    movq    -8(%rbp), %rax
    addq    $4, %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    subl    $1, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    jmp     .L9
    .L10:
    movq    -8(%rbp), %rax
    addq    $1, %rax
    movzbl  (%rax), %eax
    leal    1(%rax), %edx
    movq    -8(%rbp), %rax
    addq    $1, %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    addq    $2, %rax
    movzbl  (%rax), %eax
    leal    1(%rax), %edx
    movq    -8(%rbp), %rax
    addq    $2, %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    addq    $3, %rax
    movzbl  (%rax), %eax
    leal    -1(%rax), %edx
    movq    -8(%rbp), %rax
    addq    $3, %rax
    movb    %dl, (%rax)
    addq    $5, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    addl    $1, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    .L11:
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    testb   %al, %al
    je      .L12
    subq    $1, -8(%rbp)
    jmp     .L11
    .L12:
    subq    $1, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    subl    $1, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    jmp     .L7
    .L8:
    addq    $2, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    addq    $1, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    subl    $3, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    addl    $7, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    addl    $3, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    addq    $2, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    subq    $1, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    subl    $1, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    subq    $1, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    addl    $3, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    subl    $6, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    subl    $8, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    addq    $2, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    addl    $1, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    addq    $1, -8(%rbp)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    addl    $2, %eax
    movl    %eax, %edx
    movq    -8(%rbp), %rax
    movb    %dl, (%rax)
    movq    -8(%rbp), %rax
    movzbl  (%rax), %eax
    movsbl  %al, %eax
    movl    %eax, %edi
    call    putchar
    movl $0, %eax
    leave
    ret
