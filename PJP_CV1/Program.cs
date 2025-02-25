using System;
using System.Collections.Generic;

enum TokenType { Number, Plus, Minus, Mul, Div, LParen, RParen, End }

class Token
{
    public TokenType Type;
    public int Value;
    public Token(TokenType type, int value = 0) { Type = type; Value = value; }
}

class Parser
{
    List<Token> tokens;
    int pos;
    public Parser(List<Token> tokens) { this.tokens = tokens; pos = 0; }
    public Token Curr => tokens[pos];
    void Eat(TokenType t) { if (Curr.Type == t) pos++; else throw new Exception(); }

    public int ParseExpr()
    {
        int x = ParseTerm();
        while (Curr.Type == TokenType.Plus || Curr.Type == TokenType.Minus)
            x = Curr.Type == TokenType.Plus ? x + (EatAndParse(TokenType.Plus)) : x - (EatAndParse(TokenType.Minus));
        return x;
    }
    int EatAndParse(TokenType t) { Eat(t); return ParseTerm(); }

    int ParseTerm()
    {
        int x = ParseFactor();
        while (Curr.Type == TokenType.Mul || Curr.Type == TokenType.Div)
            x = Curr.Type == TokenType.Mul ? x * (EatAndParse(TokenType.Mul)) : x / (EatAndParse(TokenType.Div));
        return x;
    }

    int ParseFactor()
    {
        if (Curr.Type == TokenType.Number) { int v = Curr.Value; Eat(TokenType.Number); return v; }
        if (Curr.Type == TokenType.LParen) { Eat(TokenType.LParen); int v = ParseExpr(); Eat(TokenType.RParen); return v; }
        throw new Exception();
    }
}

class Program
{
    static List<Token> Tokenize(string s)
    {
        var tokens = new List<Token>();
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (char.IsWhiteSpace(c)) continue;
            if (char.IsDigit(c))
            {
                int v = 0;
                while (i < s.Length && char.IsDigit(s[i]))
                    v = v * 10 + (s[i++] - '0');
                tokens.Add(new Token(TokenType.Number, v));
                i--;
            }
            else
            {
                tokens.Add(c switch
                {
                    '+' => new Token(TokenType.Plus),
                    '-' => new Token(TokenType.Minus),
                    '*' => new Token(TokenType.Mul),
                    '/' => new Token(TokenType.Div),
                    '(' => new Token(TokenType.LParen),
                    ')' => new Token(TokenType.RParen),
                    _ => throw new Exception()
                });
            }
        }
        tokens.Add(new Token(TokenType.End));
        return tokens;
    }
    static void Main()
    {
        Console.WriteLine("Enter number of lines: ");
        int T = int.Parse(Console.ReadLine());

        for (int i = 0; i < T; i++)
        {
            Console.WriteLine($"Enter expression ({i+1}.): ");
            string line = Console.ReadLine();
            try
            {
                var tokens = Tokenize(line);
                var parser = new Parser(tokens);
                int result = parser.ParseExpr();
                if (parser.Curr.Type != TokenType.End) throw new Exception();
                Console.WriteLine(result);
            }
            catch { Console.WriteLine("ERROR"); }
        }
    }
}
