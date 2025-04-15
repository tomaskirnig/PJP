using Antlr4.Runtime.Misc;
using System;
using PLC_Lab7;

namespace PLC_Lab7
{
    public class EvalVisitor : PLC_Lab7_exprBaseVisitor<object>
    {
        // Internal TypeChecker instance that collects type errors.
        private TypeChecker typeChecker = new TypeChecker();
        public TypeChecker TypeChecker => typeChecker;

        // Literal nodes simply convert the literal text.
        public override object VisitInt(PLC_Lab7_exprParser.IntContext context)
        {
            return Convert.ToInt32(context.INT().GetText(), 10);
        }

        public override object VisitFloat(PLC_Lab7_exprParser.FloatContext context)
        {
            return float.Parse(context.FLOAT().GetText());
        }

        public override object VisitString(PLC_Lab7_exprParser.StringContext context)
        {
            string text = context.STRING().GetText();
            // Remove the surrounding quotation marks.
            return text.Substring(1, text.Length - 2);
        }

        public override object VisitBool(PLC_Lab7_exprParser.BoolContext context)
        {
            string boolText = context.BOOL().GetText();
            return boolText.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        // Variable access: get the stored value.
        public override object VisitVar(PLC_Lab7_exprParser.VarContext context)
        {
            string varName = context.ID().GetText();
            return typeChecker.GetVariable(varName, context.ID().Symbol);
        }

        // Variable declaration
        public override object VisitVariableDecl(PLC_Lab7_exprParser.VariableDeclContext context)
        {
            var typeNode = context.type();
            if (typeNode == null)
                throw new Exception("Type information is missing in the variable declaration.");

            string typeStr = Visit(typeNode)?.ToString();
            if (typeStr == null)
                throw new Exception("Failed to determine the type of the variable.");

            // Determine the declared type based on the type string.
            TypeChecker.DataType dt = TypeChecker.DataType.Int;
            if (typeStr == "float")
                dt = TypeChecker.DataType.Float;
            else if (typeStr == "string")
                dt = TypeChecker.DataType.String;
            else if (typeStr == "bool")
                dt = TypeChecker.DataType.Bool;

            // Declare each variable in the declaration.
            foreach (var id in context.ID())
            {
                typeChecker.DeclareVariable(id.GetText(), dt, id.Symbol);
            }
            return 0; // Výchozí návratová hodnota místo null
        }

        // Assignment
        public override object VisitAssign(PLC_Lab7_exprParser.AssignContext context)
        {
            string varName = context.ID().GetText();
            object value = Visit(context.expr());
            typeChecker.AssignVariable(varName, value, context.ID().Symbol);
            return value; 
        }

        // Binary addition / concatenation
        public override object VisitAddConcat(PLC_Lab7_exprParser.AddConcatContext context)
        {
            var left = Visit(context.expr(0));
            var right = Visit(context.expr(1));

            // Ochrana proti null hodnotám
            if (left == null || right == null)
            {
                string pos = $"{context.op.Line}:{context.op.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}One or both operands are null in {context.op.Text} operation.");
                return 0; // Výchozí hodnota int
            }

            try
            {
                var leftType = typeChecker.DetermineExpressionType(left);
                var rightType = typeChecker.DetermineExpressionType(right);

                string pos = $"{context.op.Line}:{context.op.Column} - ";
                Console.WriteLine($"DEBUG: {context.op.Text} - left: {left} ({leftType}), right: {right} ({rightType})");

                if (context.op.Text == "+")
                {
                    // Pokud jsou oba operandy stejného typu
                    if (leftType == rightType)
                    {
                        if (leftType == TypeChecker.DataType.Int)
                        {
                            return (int)left + (int)right; // Skutečné sčítání intů
                        }
                        else if (leftType == TypeChecker.DataType.Float)
                        {
                            return (float)left + (float)right; // Skutečné sčítání floatů
                        }
                        else if (leftType == TypeChecker.DataType.String)
                        {
                            return (string)left + (string)right; // Skutečná konkatenace stringů
                        }
                    }
                    else
                    {

                        // Vraťte hodnotu podle vyššího typu (ale už bude zaznamenána chyba)
                        // Tím zajistíme, že případný výsledek operace má alespoň správný typ
                        if (leftType == TypeChecker.DataType.Float || rightType == TypeChecker.DataType.Float)
                            return 0.0f;
                        // Detailní chybová zpráva s pozicí
                        typeChecker.TypeErrors.Add($"{pos}Type error in addition: incompatible types {leftType} and {rightType}.");
                        return 0;
                    }
                }
                // For the minus operator, ensure both operands are numeric.
                else if (context.op.Text == "-")
                {
                    if (leftType == rightType)
                    {
                        if (leftType == TypeChecker.DataType.Int)
                        {
                            return (int)left - (int)right; // Skutečné odčítání intů
                        }
                        else if (leftType == TypeChecker.DataType.Float)
                        {
                            return (float)left - (float)right; // Skutečné odčítání floatů
                        }
                    }
                    // Míchání typů - opět chceme hlásit chyby
                    else
                    {
                        typeChecker.TypeErrors.Add($"{pos}Type error in subtraction: incompatible types {leftType} and {rightType}.");
                        if (leftType == TypeChecker.DataType.Float || rightType == TypeChecker.DataType.Float)
                            return 0.0f;
                        return 0;
                    }
                }
                // For the string concatenation with '.' operator
                else if (context.op.Text == ".")
                {
                    if (leftType == TypeChecker.DataType.String && rightType == TypeChecker.DataType.String)
                    {
                        return (string)left + (string)right; // Skutečná konkatenace stringů
                    }
                    else
                    {
                        typeChecker.TypeErrors.Add($"{pos}Type error in string concatenation: operands must be strings.");
                        return ""; // Výchozí hodnota string
                    }
                }

                // Pokud se nedostaneme do žádné podmínky výše
                typeChecker.TypeErrors.Add($"{pos}Unsupported operator or operand types: {context.op.Text}");
                return 0;
            }
            catch (Exception ex)
            {
                string pos = $"{context.op.Line}:{context.op.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Error in {context.op.Text} operation: {ex.Message}");
                return 0;
            }
        }

        // For other expression nodes (e.g. parentheses) simply forward the visit.
        public override object VisitPar(PLC_Lab7_exprParser.ParContext context)
        {
            return Visit(context.expr());
        }

        // General expression node.
        public override object VisitExpression(PLC_Lab7_exprParser.ExpressionContext context)
        {
            return Visit(context.expr());
        }

        // Declaration wrapper node.
        public override object VisitDeclaration(PLC_Lab7_exprParser.DeclarationContext context)
        {
            return Visit(context.decl());
        }

        // Program: traverse all statements.
        public override object VisitProg(PLC_Lab7_exprParser.ProgContext context)
        {
            object last = null;
            foreach (var stmt in context.stmt())
            {
                last = Visit(stmt);
            }
            return last;
        }

        // Type nodes - return the name of the type
        public override object VisitIntType(PLC_Lab7_exprParser.IntTypeContext context)
        {
            return "int";
        }

        public override object VisitFloatType(PLC_Lab7_exprParser.FloatTypeContext context)
        {
            return "float";
        }

        public override object VisitStringType(PLC_Lab7_exprParser.StringTypeContext context)
        {
            return "string";
        }

        public override object VisitBoolType(PLC_Lab7_exprParser.BoolTypeContext context)
        {
            return "bool";
        }

        // Multiplication, division and modulo operations
        public override object VisitMul(PLC_Lab7_exprParser.MulContext context)
        {
            var left = Visit(context.expr(0));
            var right = Visit(context.expr(1));

            // Ochrana proti null hodnotám
            if (left == null || right == null)
            {
                string pos = $"{context.op.Line}:{context.op.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}One or both operands are null in {context.op.Text} operation.");
                return 0; // Výchozí hodnota int
            }

            try
            {
                var leftType = typeChecker.DetermineExpressionType(left);
                var rightType = typeChecker.DetermineExpressionType(right);

                // Check if both operands are numeric
                if ((leftType == TypeChecker.DataType.Int || leftType == TypeChecker.DataType.Float) &&
                    (rightType == TypeChecker.DataType.Int || rightType == TypeChecker.DataType.Float))
                {
                    // Determine result type (float if either operand is float)
                    if (leftType == TypeChecker.DataType.Float || rightType == TypeChecker.DataType.Float)
                        return 0.0f;
                    return 0;
                }
                else
                {
                    string pos = $"{context.op.Line}:{context.op.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Type error in {context.op.Text} operation: operands must be numeric.");
                    return 0; // Výchozí hodnota místo null
                }
            }
            catch (Exception ex)
            {
                string pos = $"{context.op.Line}:{context.op.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Error in {context.op.Text} operation: {ex.Message}");
                return 0; // Výchozí hodnota při chybě
            }
        }

        // Logical operations (AND, OR)
        public override object VisitLogicalAnd(PLC_Lab7_exprParser.LogicalAndContext context)
        {
            var left = Visit(context.expr(0));
            var right = Visit(context.expr(1));

            // Ochrana proti null hodnotám
            if (left == null || right == null)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}One or both operands are null in logical AND operation.");
                return false; // Výchozí hodnota bool
            }

            try
            {
                var leftType = typeChecker.DetermineExpressionType(left);
                var rightType = typeChecker.DetermineExpressionType(right);

                if (leftType == TypeChecker.DataType.Bool && rightType == TypeChecker.DataType.Bool)
                {
                    return false; // Dummy boolean result
                }
                else
                {
                    string pos = $"{context.start.Line}:{context.start.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Type error in logical AND: operands must be boolean.");
                    return false; // Výchozí hodnota bool místo null
                }
            }
            catch (Exception ex)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Error in logical AND operation: {ex.Message}");
                return false; // Výchozí hodnota při chybě
            }
        }

        public override object VisitLogicalOr(PLC_Lab7_exprParser.LogicalOrContext context)
        {
            var left = Visit(context.expr(0));
            var right = Visit(context.expr(1));

            // Ochrana proti null hodnotám
            if (left == null || right == null)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}One or both operands are null in logical OR operation.");
                return false; // Výchozí hodnota bool
            }

            try
            {
                var leftType = typeChecker.DetermineExpressionType(left);
                var rightType = typeChecker.DetermineExpressionType(right);

                if (leftType == TypeChecker.DataType.Bool && rightType == TypeChecker.DataType.Bool)
                {
                    return false; // Dummy boolean result
                }
                else
                {
                    string pos = $"{context.start.Line}:{context.start.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Type error in logical OR: operands must be boolean.");
                    return false; // Výchozí hodnota bool místo null
                }
            }
            catch (Exception ex)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Error in logical OR operation: {ex.Message}");
                return false; // Výchozí hodnota při chybě
            }
        }

        public override object VisitLogicalNot(PLC_Lab7_exprParser.LogicalNotContext context)
        {
            var expr = Visit(context.expr());

            // Ochrana proti null hodnotě
            if (expr == null)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Operand is null in logical NOT operation.");
                return false; // Výchozí hodnota bool
            }

            try
            {
                var exprType = typeChecker.DetermineExpressionType(expr);

                if (exprType == TypeChecker.DataType.Bool)
                {
                    return false; // Dummy boolean result
                }
                else
                {
                    string pos = $"{context.start.Line}:{context.start.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Type error in logical NOT: operand must be boolean.");
                    return false; // Výchozí hodnota bool místo null
                }
            }
            catch (Exception ex)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Error in logical NOT operation: {ex.Message}");
                return false; // Výchozí hodnota při chybě
            }
        }

        // Comparison operations (>, <, >=, <=)
        public override object VisitComparison(PLC_Lab7_exprParser.ComparisonContext context)
        {
            var left = Visit(context.expr(0));
            var right = Visit(context.expr(1));

            // Ochrana proti null hodnotám
            if (left == null || right == null)
            {
                string pos = $"{context.op.Line}:{context.op.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}One or both operands are null in {context.op.Text} comparison.");
                return false; // Výchozí hodnota bool
            }

            try
            {
                var leftType = typeChecker.DetermineExpressionType(left);
                var rightType = typeChecker.DetermineExpressionType(right);

                // Only numeric types can be compared with these operators
                if ((leftType == TypeChecker.DataType.Int || leftType == TypeChecker.DataType.Float) &&
                    (rightType == TypeChecker.DataType.Int || rightType == TypeChecker.DataType.Float))
                {
                    return false; // Dummy boolean result
                }
                else
                {
                    string pos = $"{context.op.Line}:{context.op.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Type error in {context.op.Text} comparison: operands must be numeric.");
                    return false; // Výchozí hodnota bool místo null
                }
            }
            catch (Exception ex)
            {
                string pos = $"{context.op.Line}:{context.op.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Error in {context.op.Text} comparison: {ex.Message}");
                return false; // Výchozí hodnota při chybě
            }
        }

        // Equality operations (==, !=)
        public override object VisitEquality(PLC_Lab7_exprParser.EqualityContext context)
        {
            var left = Visit(context.expr(0));
            var right = Visit(context.expr(1));

            // Ochrana proti null hodnotám
            if (left == null || right == null)
            {
                string pos = $"{context.op.Line}:{context.op.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}One or both operands are null in {context.op.Text} comparison.");
                return false; // Výchozí hodnota bool
            }

            try
            {
                var leftType = typeChecker.DetermineExpressionType(left);
                var rightType = typeChecker.DetermineExpressionType(right);

                // Same types can be compared for equality
                if (leftType == rightType)
                {
                    return false; // Dummy boolean result
                }
                // Allow comparison between int and float
                else if ((leftType == TypeChecker.DataType.Int && rightType == TypeChecker.DataType.Float) ||
                         (leftType == TypeChecker.DataType.Float && rightType == TypeChecker.DataType.Int))
                {
                    return false;
                }
                else
                {
                    string pos = $"{context.op.Line}:{context.op.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Type error in {context.op.Text} comparison: incompatible types.");
                    return false; // Výchozí hodnota bool místo null
                }
            }
            catch (Exception ex)
            {
                string pos = $"{context.op.Line}:{context.op.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Error in {context.op.Text} comparison: {ex.Message}");
                return false; // Výchozí hodnota při chybě
            }
        }

        // Unary minus operation
        public override object VisitUnaryMinus(PLC_Lab7_exprParser.UnaryMinusContext context)
        {
            var expr = Visit(context.expr());

            // Ochrana proti null hodnotě
            if (expr == null)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Operand is null in unary minus operation.");
                return 0; // Výchozí hodnota int
            }

            try
            {
                var exprType = typeChecker.DetermineExpressionType(expr);

                if (exprType == TypeChecker.DataType.Int)
                {
                    return 0; // Dummy int result
                }
                else if (exprType == TypeChecker.DataType.Float)
                {
                    return 0.0f; // Dummy float result
                }
                else
                {
                    string pos = $"{context.start.Line}:{context.start.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Type error in unary minus: operand must be numeric.");
                    return 0; // Výchozí hodnota int místo null
                }
            }
            catch (Exception ex)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Error in unary minus operation: {ex.Message}");
                return 0; // Výchozí hodnota při chybě
            }
        }

        // Octal and hexadecimal literals
        public override object VisitOct(PLC_Lab7_exprParser.OctContext context)
        {
            string octStr = context.OCT().GetText();
            // Convert octal string to integer
            return Convert.ToInt32(octStr, 8);
        }

        public override object VisitHexa(PLC_Lab7_exprParser.HexaContext context)
        {
            string hexStr = context.HEXA().GetText();
            // Remove '0x' prefix and convert
            return Convert.ToInt32(hexStr.Substring(2), 16);
        }

        // Control flow statements
        public override object VisitIfStatement(PLC_Lab7_exprParser.IfStatementContext context)
        {
            var condition = Visit(context.expr());

            // Ochrana proti null hodnotě
            if (condition == null)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Condition is null in if statement.");
            }
            else
            {
                try
                {
                    var condType = typeChecker.DetermineExpressionType(condition);

                    if (condType != TypeChecker.DataType.Bool)
                    {
                        string pos = $"{context.start.Line}:{context.start.Column} - ";
                        typeChecker.TypeErrors.Add($"{pos}Type error in if statement: condition must be boolean.");
                    }
                }
                catch (Exception ex)
                {
                    string pos = $"{context.start.Line}:{context.start.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Error in if statement condition: {ex.Message}");
                }
            }

            // Visit the 'then' branch
            Visit(context.stmt(0));

            // Visit the 'else' branch if present
            if (context.stmt(1) != null)
            {
                Visit(context.stmt(1));
            }

            return 0; // Výchozí návratová hodnota místo null
        }

        public override object VisitWhileStatement(PLC_Lab7_exprParser.WhileStatementContext context)
        {
            var condition = Visit(context.expr());

            // Ochrana proti null hodnotě
            if (condition == null)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Condition is null in while statement.");
            }
            else
            {
                try
                {
                    var condType = typeChecker.DetermineExpressionType(condition);

                    if (condType != TypeChecker.DataType.Bool)
                    {
                        string pos = $"{context.start.Line}:{context.start.Column} - ";
                        typeChecker.TypeErrors.Add($"{pos}Type error in while statement: condition must be boolean.");
                    }
                }
                catch (Exception ex)
                {
                    string pos = $"{context.start.Line}:{context.start.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Error in while statement condition: {ex.Message}");
                }
            }

            Visit(context.stmt());
            return 0; // Výchozí návratová hodnota místo null
        }

        public override object VisitDoWhileStatement(PLC_Lab7_exprParser.DoWhileStatementContext context)
        {
            Visit(context.stmtBlock());

            var condition = Visit(context.expr());

            // Ochrana proti null hodnotě
            if (condition == null)
            {
                string pos = $"{context.start.Line}:{context.start.Column} - ";
                typeChecker.TypeErrors.Add($"{pos}Condition is null in do-while statement.");
            }
            else
            {
                try
                {
                    var condType = typeChecker.DetermineExpressionType(condition);

                    if (condType != TypeChecker.DataType.Bool)
                    {
                        string pos = $"{context.start.Line}:{context.start.Column} - ";
                        typeChecker.TypeErrors.Add($"{pos}Type error in do-while statement: condition must be boolean.");
                    }
                }
                catch (Exception ex)
                {
                    string pos = $"{context.start.Line}:{context.start.Column} - ";
                    typeChecker.TypeErrors.Add($"{pos}Error in do-while statement condition: {ex.Message}");
                }
            }

            return 0; // Výchozí návratová hodnota místo null
        }

        // Block handling
        public override object VisitBlockContainer(PLC_Lab7_exprParser.BlockContainerContext context)
        {
            return Visit(context.stmtBlock());
        }

        public override object VisitStatementBlock(PLC_Lab7_exprParser.StatementBlockContext context)
        {
            object last = null;
            foreach (var stmt in context.stmt())
            {
                last = Visit(stmt);
            }
            return last;
        }

        // Additional statement types
        public override object VisitBlockStmt(PLC_Lab7_exprParser.BlockStmtContext context)
        {
            return Visit(context.block());
        }

        public override object VisitIf(PLC_Lab7_exprParser.IfContext context)
        {
            return Visit(context.ifStmt());
        }

        public override object VisitWhile(PLC_Lab7_exprParser.WhileContext context)
        {
            return Visit(context.whileStmt());
        }

        public override object VisitDoWhile(PLC_Lab7_exprParser.DoWhileContext context)
        {
            return Visit(context.doWhileStmt());
        }

        // Empty statement
        public override object VisitEmpty(PLC_Lab7_exprParser.EmptyContext context)
        {
            return 0; // Výchozí návratová hodnota místo null
        }
    }
}
