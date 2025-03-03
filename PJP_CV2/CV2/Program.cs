using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

namespace CV2
{
    public enum TokenType { Num, Op, Mod, Div, LParen, RParen, Semicolon, Id }
    public class Token
    {
        public TokenType type;
        public string? value;

        public Token(TokenType type, string value)
        {
            this.type = type;
            this.value = value;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter lines: ");
            string input = Console.ReadLine();
            foreach (var token in Parse(input))
            {
                Console.WriteLine($"{token.type}: {token.value ?? ""}");
            }
        }

        public static List<Token> Parse(string expr)
        {
            List<Token> res = new List<Token>();
            int pos = 0;

            while (pos < expr.Length)
            {
                if (char.IsWhiteSpace(expr[pos])) 
                {
                    pos++;
                    continue; 
                }
                if (char.IsDigit(expr[pos]))
                {
                    int num = 0;
                    while (pos < expr.Length && char.IsDigit(expr[pos]))
                    {
                        num = num * 10 + (expr[pos] - '0');
                        pos++;
                    }

                    res.Add(new Token(TokenType.Num, num.ToString()));
                    //pos--;
                }
                else
                {
                    if (expr[pos] == '/')
                    {
                        if (expr[pos + 1] == '/')
                        {
                            while (pos < expr.Length && expr[pos] != '\n')
                            {
                                pos++;
                            }
                        }
                        else
                        {
                            res.Add(new Token(TokenType.Op, "/"));
                        }
                    }else if (new[] { '+', '-', '*' }.Contains(expr[pos]))
                    {
                        res.Add(new Token(TokenType.Op, expr[pos].ToString()));
                    }else if (new[] { '(', ')', ';'}.Contains(expr[pos]))
                    {
                        res.Add(expr[pos] switch
                        {
                            '(' => new Token(TokenType.LParen, null),
                            ')' => new Token(TokenType.RParen, null),
                            ';' => new Token(TokenType.Semicolon, null),
                            _ => throw new InvalidDataException("Not expected char - (, ), ; - switch")
                        });
                    }
                    else if (char.IsLetter(expr[pos]))
                    {
                        int cntr = 0;
                        StringBuilder tmp = new StringBuilder();

                        while(pos < expr.Length && (char.IsLetter(expr[pos]) || char.IsNumber(expr[pos])))
                        {
                            tmp.Append(expr[pos]);
                            cntr++;
                            pos++;
                        }

                        if ("mod" == tmp.ToString())
                        {
                            res.Add(new Token(TokenType.Mod, null));   
                        }else if ("div" == tmp.ToString()) {
                            res.Add(new Token(TokenType.Div, null));
                        }
                        else
                        {
                            res.Add(new Token(TokenType.Id, tmp.ToString()));
                        }
                    }
                }
                pos++;
            }

            return res;
        }
    }
}
