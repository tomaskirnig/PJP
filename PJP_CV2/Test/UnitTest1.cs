using System.Collections.Generic;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.TestHost;
namespace CV2;

public class ParserTests
{
    [Fact]
    public void Parse_ShouldTokenizeNumbersCorrectly()
    {
        // Arrange
        string input = "123 456";

        // Act
        List<Token> tokens = CV2.Program.Parse(input);

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Num, tokens[0].type);
        Assert.Equal("123", tokens[0].value);
        Assert.Equal(TokenType.Num, tokens[1].type);
        Assert.Equal("456", tokens[1].value);
    }

    [Fact]
    public void Parse_ShouldTokenizeOperatorsCorrectly()
    {
        string input = "+ - * /";
        List<Token> tokens = Program.Parse(input);

        Assert.Equal(4, tokens.Count);
        Assert.Equal(TokenType.Op, tokens[0].type);
        Assert.Equal("+", tokens[0].value);
        Assert.Equal(TokenType.Op, tokens[1].type);
        Assert.Equal("-", tokens[1].value);
        Assert.Equal(TokenType.Op, tokens[2].type);
        Assert.Equal("*", tokens[2].value);
        Assert.Equal(TokenType.Op, tokens[3].type);
        Assert.Equal("/", tokens[3].value);
    }

    [Fact]
    public void Parse_ShouldTokenizeParensAndSemicolonCorrectly()
    {
        string input = "( ) ;";
        List<Token> tokens = Program.Parse(input);

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.LParen, tokens[0].type);
        Assert.Null(tokens[0].value);
        Assert.Equal(TokenType.RParen, tokens[1].type);
        Assert.Null(tokens[1].value);
        Assert.Equal(TokenType.Semicolon, tokens[2].type);
        Assert.Null(tokens[2].value);
    }

    [Fact]
    public void Parse_ShouldTokenizeModDivKeywordsCorrectly()
    {
        string input = "mod div";
        List<Token> tokens = Program.Parse(input);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Mod, tokens[0].type);
        Assert.Null(tokens[0].value);
        Assert.Equal(TokenType.Div, tokens[1].type);
        Assert.Null(tokens[1].value);
    }

    [Fact]
    public void Parse_ShouldTokenizeIdentifiersCorrectly()
    {
        string input = "variable anotherVar";
        List<Token> tokens = Program.Parse(input);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Id, tokens[0].type);
        Assert.Equal("variable", tokens[0].value);
        Assert.Equal(TokenType.Id, tokens[1].type);
        Assert.Equal("anotherVar", tokens[1].value);
    }

    [Fact]
    public void Parse_ShouldIgnoreComments()
    {
        string input = "123 // This is a comment \n 456";
        List<Token> tokens = Program.Parse(input);

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Num, tokens[0].type);
        Assert.Equal("123", tokens[0].value);
        Assert.Equal(TokenType.Num, tokens[1].type);
        Assert.Equal("456", tokens[1].value);
    }
}
