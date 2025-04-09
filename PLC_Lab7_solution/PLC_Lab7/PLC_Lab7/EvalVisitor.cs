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

        public override object VisitAdd([NotNull] PLC_Lab7_exprParser.AddContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            if (context.op.Text.Equals("+"))
            {
                return typeChecker.Add(left, right, context.op);
            }
            else
            {
                return typeChecker.Subtract(left, right, context.op);
            }
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
            return typeChecker.GetVariable(varName, context.ID().Symbol);
        }

        public override object VisitAssign([NotNull] PLC_Lab7_exprParser.AssignContext context)
        {
            string varName = context.ID().GetText();
            object value = Visit(context.expr());
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
                // Print only expression results, not declarations
                if (lastValue != null && stmt is PLC_Lab7_exprParser.ExpressionContext)
                {
                    Console.WriteLine(lastValue);
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
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

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
    }
}
