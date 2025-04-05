﻿using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace PLC_Lab7
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var fileName = "input2.txt";
            Console.WriteLine("Parsing: " + fileName);
            var inputFile = new StreamReader(fileName);
            AntlrInputStream input = new AntlrInputStream(inputFile);
            PLC_Lab7_exprLexer lexer = new PLC_Lab7_exprLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            PLC_Lab7_exprParser parser = new PLC_Lab7_exprParser(tokens);

            parser.AddErrorListener(new VerboseListener());

            IParseTree tree = parser.prog();

            if (parser.NumberOfSyntaxErrors == 0)
            {
                //Console.WriteLine(tree.ToStringTree(parser));

                new EvalVisitor().Visit(tree);
            }
        }
    }
}