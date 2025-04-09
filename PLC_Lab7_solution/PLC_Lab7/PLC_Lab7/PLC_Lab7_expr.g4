grammar PLC_Lab7_expr;

/** The start rule; begin parsing here. */
prog: (stmt ';')+;

stmt: decl                              # declaration
    | expr                              # expression
    | doWhileStmt                       # doWhileStatement
    ;

doWhileStmt: 'do' '{' stmtBlock '}' 'while' '(' expr ')' # doWhile
    ;

stmtBlock: (stmt ';')*                  # statementBlock
    ;

decl: type ID (',' ID)*                 # variableDecl
    ;

type: 'int'                             #intType
    | 'float'                           #floatType
    | 'string'                          #stringType
    | 'bool'                            #boolType
    ;

expr: ID '=' expr                       # assign
    | expr op=('>'|'<'|'>='|'<=') expr  # comparison
    | expr op=('=='|'!=') expr          # equality
    | expr op=('+'|'-') expr            # add
    | expr op=('*'|'/'|'%') expr        # mul
    | INT                               # int
    | FLOAT                             # float
    | STRING                            # string
    | BOOL                              # bool
    | OCT                               # oct
    | HEXA                              # hexa
    | ID                                # var
    | '(' expr ')'                      # par
    ;

BOOL: 'true' | 'false';
ID : [a-zA-Z]+ ;        
INT : '0' | [1-9][0-9]* ;          
FLOAT : [0-9]+ '.' [0-9]+ ; 
STRING: '"' ( ~["\r\n\\] | '\\' . )* '"' ;  // String in double quotes with escape support
OCT : '0'[0-7]* ;
HEXA : '0x'[0-9a-fA-F]+ ;
WS : [ \t\r\n]+ -> skip ;                   // clean whitespace
