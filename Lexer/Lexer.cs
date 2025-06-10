using Fractals.Diagnostics;

namespace Fractals.Lexer;

public class Lexer
{
    private readonly FileInfo _fileInfo;
    private readonly StreamReader _streamReader;
    
    private char _current;
    private int _characterPosition = -1;
    private int _line = 1;
    private int _column = -1;
    
    public Lexer(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
        _streamReader = new StreamReader(_fileInfo.OpenRead());
    }

    private bool IsWindowsEndOfLine => 
        (_current is '\n' && Peek() is '\r') ||
        (_current is '\r' && Peek() is '\n');
    
    private bool IsLinuxEndOfLine => _current is '\n';
    
    private bool IsMaxOsEndOfLine => _current is '\r';
    
    private char Peek()
    {
        var peek = _streamReader.Peek();
        return peek > -1 ? (char)peek : '\0';
    }

    private bool MoveNext()
    {
        if (!_streamReader.EndOfStream)
        {
            _characterPosition++;
            _column++; // will be reset when a new line is reached
        }
        
        var result = _streamReader.Read();
        _current = (char)result;
        return result > -1; 
    }
    
    private SpanMeta GetSpanMeta(string? text) => new(text, _fileInfo.FullName, _characterPosition, _line, _column);
    private SpanMeta GetSpanMeta(int line, string? text) => new(text, _fileInfo.FullName, _characterPosition, line, _column);
    
    public List<Token> Run()
    {
        var tokens = new List<Token>();

        MoveNext(); // move to the first character
        
        while (!_streamReader.EndOfStream)
        {
            if (ConsumeToken() is { } token)
            {
                tokens.Add(token);
            }
        }

        if (ConsumeToken() is { } lastToken)
        {
            tokens.Add(lastToken);
        }

        tokens.Add(new Token(TokenType.EndOfFile, GetSpanMeta(null)));
        
        return tokens;
    }

    private Token? ConsumeToken()
    {
        if (IsWindowsEndOfLine)
        {
            MoveNext();
            MoveNext();
            var line = _line;
            _line++;
            _column = 0;
            return new Token(TokenType.EndOfLine, GetSpanMeta(line, null));
        }
        
        if (IsLinuxEndOfLine || IsMaxOsEndOfLine)
        {
            MoveNext();
            var line = _line;
            _line++;
            _column = 0;
            return new Token(TokenType.EndOfLine, GetSpanMeta(line, null));
        }

        if (char.IsWhiteSpace(_current))
        {
            return ConsumeWhitespace();
        }

        if (char.IsLetter(_current) || _current is '_')
        {
            return ConsumeIdentifier();
        }

        if (_current is '"')
        {
            return ConsumeStringLiteral();
        }

        if (char.IsDigit(_current) || (_current is '.' && char.IsDigit(Peek())))
        {
            return ConsumeNumber();
        }

        if (_current is '=' && Peek() is '=')
        {
            MoveNext();
            MoveNext();
            return new Token(TokenType.Equals, GetSpanMeta(null));
        }
        
        if (_current is '!' && Peek() is '=')
        {
            MoveNext();
            MoveNext();
            return new Token(TokenType.NotEquals, GetSpanMeta(null));
        }
        
        if (_current is '>' && Peek() is '=')
        {
            MoveNext();
            MoveNext();
            return new Token(TokenType.GreaterThanOrEqual, GetSpanMeta(null));
        }
        
        if (_current is '<' && Peek() is '=')
        {
            MoveNext();
            MoveNext();
            return new Token(TokenType.LessThanOrEqual, GetSpanMeta(null));
        }
        
        if (_current is '-' && Peek() is '>')
        {
            MoveNext();
            MoveNext();
            return new Token(TokenType.Assignment, GetSpanMeta(null));
        }

        return TryConsumeSingleCharacterToken();
    }

    private Token ConsumeIdentifier()
    {
        var identifier = string.Empty;
            
        while (char.IsLetterOrDigit(_current) || _current is '_')
        {
            identifier += _current;
            if (!MoveNext()) break;
        }

        var spanMeta = GetSpanMeta(identifier);
        
        return identifier switch
        {
            "true" or "false" => new Token(TokenType.Boolean, spanMeta),
            "int" or "decimal" or "bool" or "str" => new Token(TokenType.BuiltInType, spanMeta),
            "for" => new Token(TokenType.KeywordFor, spanMeta),
            "to" => new Token(TokenType.KeywordTo, spanMeta),
            "while" => new Token(TokenType.KeywordWhile, spanMeta),
            "function" => new Token(TokenType.KeywordFunction, spanMeta),
            "if" => new Token(TokenType.KeywordIf, spanMeta),
            "else" => new Token(TokenType.KeywordElse, spanMeta),
            "import" => new Token(TokenType.KeywordImport, spanMeta),
            "and" => new Token(TokenType.LogicalAnd, spanMeta),
            "or" => new Token(TokenType.LogicalOr, spanMeta),
            "return" => new Token(TokenType.KeywordReturn, spanMeta),
            "new" => new Token(TokenType.KeywordNew, spanMeta),
            "type" => new Token(TokenType.KeywordType, spanMeta),
            "break" => new Token(TokenType.KeywordBreak, spanMeta),
            "continue" => new Token(TokenType.KeywordContinue, spanMeta),
            _ => new Token(TokenType.Identifier, spanMeta)
        };
    }
    
    private Token ConsumeWhitespace()
    {
        var whitespace = string.Empty;
            
        while (char.IsWhiteSpace(_current) && !IsWindowsEndOfLine && !IsLinuxEndOfLine && !IsMaxOsEndOfLine)
        {
            whitespace += _current;
            if (!MoveNext()) break;
        }
        
        return new Token(TokenType.Whitespace, GetSpanMeta(whitespace));
    }

    private Token ConsumeStringLiteral()
    {
        var stringLiteral = string.Empty;
        MoveNext();

        while (_current is not '"' && !IsWindowsEndOfLine && !IsLinuxEndOfLine && !IsMaxOsEndOfLine)
        {
            stringLiteral += _current;
            if (!MoveNext()) break;
        }

        if (_current is '"')
        {
            MoveNext();
        }
        else
        {
            Diagnoser.AddError("Missing end quote", GetSpanMeta(stringLiteral));
        }
        
        return new Token(TokenType.StringLiteral, GetSpanMeta(stringLiteral));
    }

    private Token ConsumeNumber()
    {
        var number = string.Empty;
        var containsDecimalPoint = false;

        while (char.IsDigit(_current) || _current is '.')
        {
            number += _current;

            // ensures that only one decimal point is found in the number
            switch (_current)
            {
                case '.' when !containsDecimalPoint:
                    containsDecimalPoint = true;
                    break;
                case '.' when containsDecimalPoint:
                    Diagnoser.AddError("Too many decimal points.", GetSpanMeta(number));
                    break;
            }

            if (!MoveNext()) break;
        }
        
        return new Token(containsDecimalPoint ? TokenType.Decimal : TokenType.Integer, GetSpanMeta(number));
    }

    private Token? TryConsumeSingleCharacterToken()
    {
        switch (_current)
        {
            case '{':
                MoveNext();
                return new Token(TokenType.OpenBracket, GetSpanMeta(null));
            case '}':
                MoveNext();
                return new Token(TokenType.CloseBracket, GetSpanMeta(null));
            case '(':
                MoveNext();
                return new Token(TokenType.OpenParenthesis, GetSpanMeta(null));
            case ')':
                MoveNext();
                return new Token(TokenType.CloseParenthesis, GetSpanMeta(null));
            case '[':
                MoveNext();
                return new Token(TokenType.OpenSquareBracket, GetSpanMeta(null));
            case ']':
                MoveNext();
                return new Token(TokenType.CloseSquareBracket, GetSpanMeta(null));
            case '.':
                MoveNext();
                return new Token(TokenType.DotAccessor, GetSpanMeta(null));
            case '+':
                MoveNext();
                return new Token(TokenType.Addition, GetSpanMeta(null));
            case '-':
                MoveNext();
                return new Token(TokenType.Subtraction, GetSpanMeta(null));
            case '*':
                MoveNext();
                return new Token(TokenType.Multiplication, GetSpanMeta(null));
            case '/':
                MoveNext();
                return new Token(TokenType.Division, GetSpanMeta(null));
            case '%':
                MoveNext();
                return new Token(TokenType.Modulus, GetSpanMeta(null));
            case '>':
                MoveNext();
                return new Token(TokenType.GreaterThan, GetSpanMeta(null));
            case '<':
                MoveNext();
                return new Token(TokenType.LessThan, GetSpanMeta(null));
            case ':':
                MoveNext();
                return new Token(TokenType.Colon, GetSpanMeta(null));
            case ';':
                MoveNext();
                return new Token(TokenType.Semicolon, GetSpanMeta(null));
            case ',':
                MoveNext();
                return new Token(TokenType.Comma, GetSpanMeta(null));
        }

        MoveNext();
        return null;
    }
}