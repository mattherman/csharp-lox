# Lox Grammar
```
program        → declaration* EOF ;

declaration    → classDecl
               | funDecl
               | varDecl
               | statement ;
classDecl      → "class" IDENTIFIER ( "<" IDENTIFIER )? "{" function* "}" ;
funDecl        → "fun" function ;
function  → IDENTIFIER lambda ;
parameters     → IDENTIFIER ( "," IDENTIFIER )* ;

varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;

statement      → exprStmt
               | ifStmt
               | printStmt
               | returnStmt
               | whileStmt
               | block;

block          → "{" declaration* "}" ;
ifStmt         → "if" "(" expression ")" statement ( "else" statement)? ;
whileStmt      → "while" "(" expression ")" statement ;
returnStmt     → "return" expression? ";" ;
forStmt        → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement ;
exprStmt       → expression ";" ;
printStmt      → "print" expression ";" ;

expression     → assignment ;
lambda         → "(" parameters? ")" block ;
assignment     → ( call "." )? IDENTIFER "=" assignment
               | logic_or ;
logic_or       → logic_and ( "or" logic_and )* ;
logic_and      → equality ( "and" equality )* ;
equality       → comparison ( ( "!=" | "==" ) comparison )* ;
comparison     → addition ( ( ">" | ">=" | "<" | "<=" ) addition )* ;
addition       → multiplication ( ( "-" | "+" ) multiplication )* ;
multiplication → unary ( ( "/" | "*" ) unary )* ;
unary          → ( "!" | "-" ) unary
               | call ;
call           → primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
arguments      → expression ( "," expression )* ;
primary        →  "false" | "true" | "nil" | "this"
               | NUMBER | STRING | IDENTIFIER | "(" expression ")" 
               | "super" "." IDENTIFIER | "fun" lambda ;
```
