grammar PLC_Lab7_expr;

/** The start rule; begin parsing here. */
prog: (stmt ';')+;

stmt: decl                              # declaration
    | expr                              # expression
    | doWhileStmt                       # doWhile
    | ifStmt                            # if
    | whileStmt                         # while
    | block                             # blockStmt
    |                                   # empty
    ;

doWhileStmt: 'do' '{' stmtBlock '}' 'while' '(' expr ')' # doWhileStatement
    ;

stmtBlock: (stmt ';')*                  # statementBlock
    ;

block: '{' stmtBlock '}'                # blockContainer
    ;

ifStmt: 'if' '(' expr ')' stmt ('else' stmt)? # ifStatement
    ;

whileStmt: 'while' '(' expr ')' stmt    # whileStatement
    ;

decl: type ID (',' ID)*                 # variableDecl
    ;

type: 'int'                             # intType
    | 'float'                           # floatType
    | 'string'                          # stringType
    | 'bool'                            # boolType
    ;

/** Expression rules ordered by precedence (lowest to highest) */
expr: ID '=' expr                       # assign        // Right-associative
    | expr '||' expr                    # logicalOr     // Left-associative
    | expr '&&' expr                    # logicalAnd    // Left-associative
    | expr op=('=='|'!=') expr          # equality      // Left-associative
    | expr op=('>'|'<'|'>='|'<=') expr  # comparison    // Left-associative
    | expr op=('+'|'-'|'.') expr        # addConcat     // Left-associative 
    | expr op=('*'|'/'|'%') expr        # mul           // Left-associative
    | '!' expr                          # logicalNot    // Unary
    | '-' expr                          # unaryMinus    // Unary
    | INT                               # int
    | FLOAT                             # float
    | STRING                            # string
    | BOOL                              # bool
    | OCT                               # oct
    | HEXA                              # hexa
    | ID                                # var
    | '(' expr ')'                      # par
    ;

/** Lexer rules */
BOOL: 'true' | 'false';
ID : [a-zA-Z]+[0-9]* ;        
INT : '0' | [1-9][0-9]* ;          
FLOAT : [0-9]+ '.' [0-9]+ ; 
STRING: '"' ( ~["\r\n\\] | '\\' . )* '"' ;  // String in double quotes with escape support
OCT : '0'[0-7]* ;
HEXA : '0x'[0-9a-fA-F]+ ;

/** Comment rules */
SINGLE_LINE_COMMENT: '//' ~[\r\n]* -> skip;
MULTI_LINE_COMMENT: '/*' .*? '*/' -> skip;
WS : [ \t\r\n]+ -> skip ;                   // Clean whitespace
