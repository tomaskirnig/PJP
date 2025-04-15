using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Lab7
{
    public class EvalVisitor : PLC_Lab7_exprBaseVisitor<object>
    {
        private TypeChecker typeChecker = new();
        public override object VisitInt([NotNull] PLC_Lab7_exprParser.IntContext context)
        {
            return Convert.ToInt32(context.INT().GetText(), 10);
        }

        public override object VisitFloat([NotNull] PLC_Lab7_exprParser.FloatContext context)
        {
            return float.Parse(context.FLOAT().GetText());
        }

        public override object VisitHexa([NotNull] PLC_Lab7_exprParser.HexaContext context)
        {
            string hexText = context.HEXA().GetText();
            // Remove the "0x" prefix before parsing
            if (hexText.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hexText = hexText.Substring(2);
            }
            return Convert.ToInt32(hexText, 16);
        }

        public override object VisitOct([NotNull] PLC_Lab7_exprParser.OctContext context)
        {
            string octText = context.OCT().GetText();
            // Remove the leading "0" if present
            if (octText.StartsWith("0") && octText.Length > 1)
            {
                octText = octText.Substring(1);
            }
            return Convert.ToInt32(octText, 8);
        }

        public override object VisitPar([NotNull] PLC_Lab7_exprParser.ParContext context)
        {
            return Visit(context.expr());
        }

        public override object VisitString([NotNull] PLC_Lab7_exprParser.StringContext context)
        {
            string text = context.STRING().GetText();
            return text.Substring(1, text.Length - 2);
        }

        public override object VisitMul([NotNull] PLC_Lab7_exprParser.MulContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            if (context.op.Text.Equals("*"))
            {
                return typeChecker.Multiply(left, right, context.op);
            }
            else if (context.op.Text.Equals("/"))
            {
                return typeChecker.Divide(left, right, context.op);
            }
            else // "%"
            {
                return typeChecker.Modulo(left, right, context.op);
            }
        }

        public override object VisitVar([NotNull] PLC_Lab7_exprParser.VarContext context)
        {
            string varName = context.ID().GetText();

            // For debugging - print info about the variable being accessed
            //Console.WriteLine($"Debug: Accessing variable {varName}, type: {typeChecker.GetVariableType(varName)}");

            // Check the parent context to understand usage
            //var parent = context.Parent;
            //if (parent != null)
            //{
            //    Console.WriteLine($"Debug: Used in context: {parent.GetType().Name}");
            //}

            return typeChecker.GetVariable(varName, context.ID().Symbol);
        }

        public override object VisitAssign([NotNull] PLC_Lab7_exprParser.AssignContext context)
        {
            string varName = context.ID().GetText();

            // Get the target variable type
            //TypeChecker.DataType targetType = typeChecker.GetVariableType(varName);

            // Evaluate the expression
            object value = Visit(context.expr());

            // Use DetermineExpressionType to check compatibility
            //TypeChecker.DataType exprType = typeChecker.DetermineExpressionType(value);

            // Debug information
            //Console.WriteLine($"Debug: Assignment - Variable: {varName} (type: {targetType}), " + $"Expression result: {value} (type: {exprType})");

            // Assign with proper type conversion
            return typeChecker.AssignVariable(varName, value, context.ID().Symbol);
        }

        public override object VisitVariableDecl([NotNull] PLC_Lab7_exprParser.VariableDeclContext context)
        {
            var typeNode = context.type();
            if (typeNode == null)
                throw new Exception("Type information is missing in the variable declaration.");

            string typeStr = Visit(typeNode)?.ToString();
            if (typeStr == null)
                throw new Exception("Failed to determine the type of the variable.");

            TypeChecker.DataType dataType = TypeChecker.DataType.Int;

            if (typeStr == "float")
                dataType = TypeChecker.DataType.Float;
            else if (typeStr == "string")
                dataType = TypeChecker.DataType.String;
            else if (typeStr == "bool")
                dataType = TypeChecker.DataType.Bool;

            // Processing all variables in the declaration
            foreach (var idNode in context.ID())
            {
                string varName = idNode.GetText();
                typeChecker.DeclareVariable(varName, dataType, idNode.Symbol);
            }

            return null;
        }

        public override object VisitIntType([NotNull] PLC_Lab7_exprParser.IntTypeContext context)
        {
            return "int";
        }

        public override object VisitFloatType([NotNull] PLC_Lab7_exprParser.FloatTypeContext context)
        {
            return "float";
        }

        public override object VisitStringType([NotNull] PLC_Lab7_exprParser.StringTypeContext context)
        {
            return "string";
        }

        public override object VisitProg([NotNull] PLC_Lab7_exprParser.ProgContext context)
        {
            object lastValue = null;
            foreach (var stmt in context.stmt())
            {
                lastValue = Visit(stmt);

                // Print only standalone expressions (not assignments or declarations)
                if (lastValue != null && stmt is PLC_Lab7_exprParser.ExpressionContext exprContext)
                {
                    var expr = exprContext.expr();

                    // Check if it's a standalone expression (not an assignment)
                    if (!(expr is PLC_Lab7_exprParser.AssignContext))
                    {
                        Console.WriteLine(lastValue);
                    }
                }
            }
            return lastValue;
        }

        public override object VisitExpression([NotNull] PLC_Lab7_exprParser.ExpressionContext context)
        {
            return Visit(context.expr());
        }

        public override object VisitDeclaration([NotNull] PLC_Lab7_exprParser.DeclarationContext context)
        {
            return Visit(context.decl());
        }
        public override object VisitEquality([NotNull] PLC_Lab7_exprParser.EqualityContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (context.op.Text.Equals("=="))
            {
                return typeChecker.Equal(left, right, context.op);
            }
            else // "!="
            {
                return typeChecker.NotEqual(left, right, context.op);
            }
        }
        public override object VisitBool([NotNull] PLC_Lab7_exprParser.BoolContext context)
        {
            string boolText = context.BOOL().GetText();
            return boolText.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public override object VisitBoolType([NotNull] PLC_Lab7_exprParser.BoolTypeContext context)
        {
            return "bool";
        }
        public override object VisitComparison([NotNull] PLC_Lab7_exprParser.ComparisonContext context)
        {
            // Evaluate operands
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            // Use DetermineExpressionType to get actual types
            //TypeChecker.DataType leftType = typeChecker.DetermineExpressionType(left);
            //TypeChecker.DataType rightType = typeChecker.DetermineExpressionType(right);

            // Debug info
            //Console.WriteLine($"Debug: Comparison - Left: {left} (type: {leftType}), " + $"Right: {right} (type: {rightType}), Op: {context.op.Text}");

            // Perform the comparison
            switch (context.op.Text)
            {
                case ">":
                    return typeChecker.GreaterThan(left, right, context.op);
                case "<":
                    return typeChecker.LessThan(left, right, context.op);
                case ">=":
                    return typeChecker.GreaterThanOrEqual(left, right, context.op);
                case "<=":
                    return typeChecker.LessThanOrEqual(left, right, context.op);
                default:
                    throw new Exception($"Unexpected comparison operator: {context.op.Text}");
            }
        }

        public override object VisitLogicalNot([NotNull] PLC_Lab7_exprParser.LogicalNotContext context)
        {
            var value = Visit(context.expr());
            return typeChecker.LogicalNot(value);
        }

        // Helper method to determine if a value evaluates to true
        private bool IsTrue(object value)
        {
            if (value is bool boolValue)
                return boolValue;

            if (value is int intValue)
                return intValue != 0;

            if (value is float floatValue)
                return floatValue != 0;

            if (value is string stringValue)
                return !string.IsNullOrEmpty(stringValue);

            return false;
        }
        public override object VisitUnaryMinus([NotNull] PLC_Lab7_exprParser.UnaryMinusContext context)
        {
            var value = Visit(context.expr());
            return typeChecker.UnaryMinus(value);
        }

        public override object VisitAddConcat([NotNull] PLC_Lab7_exprParser.AddConcatContext context)
        {
            var left = Visit(context.expr(0));
            var right = Visit(context.expr(1));

            switch (context.op.Text)
            {
                case "+":
                    return typeChecker.Add(left, right, context.op);
                case "-":
                    return typeChecker.Subtract(left, right, context.op);
                case ".":
                    return typeChecker.StringConcat(left, right, context.op);
                default:
                    throw new Exception($"Unexpected operator: {context.op.Text}");
            }
        }
        public override object VisitLogicalOr([NotNull] PLC_Lab7_exprParser.LogicalOrContext context)
        {
            var left = Visit(context.expr(0));

            if (IsTrue(left))
                return true;

            var right = Visit(context.expr(1));
            return typeChecker.LogicalOr(left, right);
        }

        public override object VisitLogicalAnd([NotNull] PLC_Lab7_exprParser.LogicalAndContext context)
        {
            var left = Visit(context.expr(0));

            if (!IsTrue(left))
                return false;

            var right = Visit(context.expr(1));
            return typeChecker.LogicalAnd(left, right);
        }

        public override object VisitStatementBlock([NotNull] PLC_Lab7_exprParser.StatementBlockContext context)
        {
            object lastValue = null;

            foreach (var stmt in context.stmt())
            {
                lastValue = Visit(stmt);
            }

            return lastValue;
        }

        public override object VisitIfStatement([NotNull] PLC_Lab7_exprParser.IfStatementContext  context)
        {
            var condition = Visit(context.expr());
            if (!(bool)typeChecker.ConvertValueToType(condition, TypeChecker.DataType.Bool))
            {
                throw new Exception("Condition in 'if' statement must evaluate to a boolean.");
            }

            if ((bool)condition)
            {
                return Visit(context.stmt(0)); // Execute the 'if' block
            }
            else if (context.stmt().Length > 1)
            {
                return Visit(context.stmt(1)); // Execute the 'else' block (if present)
            }

            return null;
        }

        public override object VisitWhileStatement([NotNull] PLC_Lab7_exprParser.WhileStatementContext context)
        {
            while (true)
            {
                var condition = Visit(context.expr());
                if (!(bool)typeChecker.ConvertValueToType(condition, TypeChecker.DataType.Bool))
                {
                    throw new Exception("Condition in 'while' statement must evaluate to a boolean.");
                }

                if (!(bool)condition)
                {
                    break; // Exit the loop if the condition is false
                }

                Visit(context.stmt());
            }

            return null;
        }
    }
}
