grammar PLC_Lab7_expr;

/** The start rule; begin parsing here. */
prog: (stmt ';')+;

stmt: decl                              # declaration
    | expr                              # expression
    | doWhileStmt                       # doWhileStatement
    |                                   # emptyStatement
    ;

doWhileStmt: 'do' '{' stmtBlock '}' 'while' '(' expr ')' # doWhile
    ;

stmtBlock: (stmt ';')*                  # statementBlock
    ;

decl: type ID (',' ID)*                 # variableDecl
    ;

type: 'int'                             # intType
    | 'float'                           # floatType
    | 'string'                          # stringType
    | 'bool'                            # boolType
    ;

expr: ID '=' expr                       # assign
    | expr op=('>'|'<'|'>='|'<=') expr  # comparison
    | expr op=('=='|'!=') expr          # equality
    | expr ('.' expr)+                  # strConcat
    | expr op=('+'|'-') expr            # add
    | expr op=('*'|'/'|'%') expr        # mul
    | expr op=('&&'|'||') expr          # logicalAndOr
    | '!' expr                          # logicalNot
    | '-' expr                          # unaryMinus
    | INT                               # int
    | FLOAT                             # float
    | STRING                            # string
    | BOOL                              # bool
    | OCT                               # oct
    | HEXA                              # hexa
    | ID                                # var
    | '(' expr ')'                      # par
    ;
 
// Rest of lexer rules
BOOL: 'true' | 'false';
ID : [a-zA-Z]+[0-9]* ;        
INT : '0' | [1-9][0-9]* ;          
FLOAT : [0-9]+ '.' [0-9]+ ; 
STRING: '"' ( ~["\r\n\\] | '\\' . )* '"' ;  // String in double quotes with escape support
OCT : '0'[0-7]* ;
HEXA : '0x'[0-9a-fA-F]+ ;

// Comment rules
SINGLE_LINE_COMMENT: '//' ~[\r\n]* -> skip;
MULTI_LINE_COMMENT: '/*' .*? '*/' -> skip;
WS : [ \t\r\n]+ -> skip ;                   // clean whitespace
