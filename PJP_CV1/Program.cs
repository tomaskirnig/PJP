using System;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("Enter number of expressions: ");
        if (!int.TryParse(Console.ReadLine(), out int n))
            return;

        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"Enter expression ({i+1}.): ");
            string expr = Console.ReadLine();
            Console.WriteLine(Parser.Evaluate(expr));
        }
    }
}

public static class Parser
{
    public static string Evaluate(string expression)
    {
        try
        {
            int result = ParseExpression(expression);
            return result.ToString();
        }
        catch (Exception)
        {
            return "ERROR";
        }
    }

    private static int ParseExpression(string s)
    {
        int pos = 0;
        int result = ParseExpression(s, ref pos);
        SkipWhitespace(s, ref pos);
        if (pos < s.Length)
            throw new Exception("Unexpected characters");
        return result;
    }

    // ParseExpression: Expression = Term { ('+' | '-') Term }
    private static int ParseExpression(string s, ref int pos)
    {
        int result = ParseTerm(s, ref pos);
        SkipWhitespace(s, ref pos);
        while (pos < s.Length && (s[pos] == '+' || s[pos] == '-'))
        {
            char op = s[pos];
            pos++; // consume operator
            int term = ParseTerm(s, ref pos);
            result = op == '+' ? result + term : result - term;
            SkipWhitespace(s, ref pos);
        }
        return result;
    }

    // ParseTerm: Term = Factor { ('*' | '/') Factor }
    private static int ParseTerm(string s, ref int pos)
    {
        int result = ParseFactor(s, ref pos);
        SkipWhitespace(s, ref pos);
        while (pos < s.Length && (s[pos] == '*' || s[pos] == '/'))
        {
            char op = s[pos];
            pos++; // consume operator
            int factor = ParseFactor(s, ref pos);
            if (op == '*')
                result *= factor;
            else
            {
                if (factor == 0)
                    throw new Exception("Division by zero");
                result /= factor;
            }
            SkipWhitespace(s, ref pos);
        }
        return result;
    }

    // ParseFactor: Factor = Number | '(' Expression ')'
    private static int ParseFactor(string s, ref int pos)
    {
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length)
            throw new Exception("Unexpected end of input");

        if (s[pos] == '(')
        {
            pos++; // consume '('
            int result = ParseExpression(s, ref pos);
            SkipWhitespace(s, ref pos);
            if (pos >= s.Length || s[pos] != ')')
                throw new Exception("Missing closing parenthesis");
            pos++; // consume ')'
            return result;
        }
        else if (char.IsDigit(s[pos]))
        {
            int start = pos;
            while (pos < s.Length && char.IsDigit(s[pos]))
                pos++;
            string numberStr = s.Substring(start, pos - start);
            if (!int.TryParse(numberStr, out int value))
                throw new Exception("Invalid number");
            return value;
        }
        else
        {
            throw new Exception("Unexpected character: " + s[pos]);
        }
    }

    // Helper method to skip whitespace in the expression.
    private static void SkipWhitespace(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos]))
            pos++;
    }
}
