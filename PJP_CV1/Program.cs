using System;
using System.Collections.Generic;

class Program
{
    private static int pos;
    private static string expression;
    private static List<int> results;

    static void Main()
    {
        pos = 0;
        Console.Write("Enter number of lines: ");
        string count = Console.ReadLine();

        for (int i = Int32.Parse(count); i > 0; i--)
        {
            expression = Console.ReadLine();
            expression = expression.Replace(" ", "");
            results.Add(Evaluate(expression));
        }
    }

    static int Evaluate(string str)
    {
        int results = 0;
        string tmp = "";
        while (pos < expression.Length)
        {
            if (str[pos] > '0' && str[pos] < '9')
            {
                tmp.Append(str[pos]);
                pos++;
            }
            else if (str[pos] == '*' || str[pos] == '/')
            {

            }
            else if (str[pos] == '+' || str[pos] == '-')
            {

            }
            else if (str[pos] == '(')
            {
                Evaluate(str.Substring(pos, str.IndexOf(')') - pos);
            }
            else if (str[pos] == ')')
            {
                pos++;
                return results;
            }
        }
    }
}
