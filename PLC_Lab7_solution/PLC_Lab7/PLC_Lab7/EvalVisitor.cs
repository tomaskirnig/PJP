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
        public override object VisitHexa([NotNull] PLC_Lab7_exprParser.HexaContext context)
        {
            return Convert.ToInt32(context.HEXA().GetText(), 16);
        }
        public override object VisitOct([NotNull] PLC_Lab7_exprParser.OctContext context)
        {
            return Convert.ToInt32(context.OCT().GetText(), 8);
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
                return typeChecker.Add(left, right);
            }
            else
            {
                return typeChecker.Subtract(left, right);
            }
        }
        public override object VisitMul([NotNull] PLC_Lab7_exprParser.MulContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            if (context.op.Text.Equals("*"))
            {
                return typeChecker.Multiply(left, right);
            }
            else
            {
                return typeChecker.Divide(left, right);
            }
        }

        public override object VisitVar([NotNull] PLC_Lab7_exprParser.VarContext context)
        {
            string varName = context.ID().GetText();
            return typeChecker.GetVariable(varName);
        }

        public override object VisitAssign([NotNull] PLC_Lab7_exprParser.AssignContext context)
        {
            string varName = context.ID().GetText();
            object value = Visit(context.expr());
            return typeChecker.AssignVariable(varName, value);
        }

        public override object VisitVariableDecl([NotNull] PLC_Lab7_exprParser.VariableDeclContext context)
        {
            string varName = context.ID().GetText();
            string typeStr = Visit(context.type()).ToString();

            TypeChecker.DataType dataType = TypeChecker.DataType.Int;
            if (typeStr == "float")
                dataType = TypeChecker.DataType.Float;
            else if (typeStr == "string")
                dataType = TypeChecker.DataType.String;

            typeChecker.DeclareVariable(varName, dataType);
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

        public override object VisitProg([NotNull] PLC_Lab7_exprParser.ProgContext context)
        {
            object lastValue = null;
            foreach (var stmt in context.stmt())
            {
                lastValue = Visit(stmt);
                if (lastValue != null)
                {
                    // Použití instance typu místo výrazu, který může být null
                    if (stmt is PLC_Lab7_exprParser.ExpressionContext)
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
    }
}
