using System;
using System.Collections.Generic;

namespace Lox
{
    public class Scanner
    {
        private readonly string _source;
        private readonly IList<Token> _tokens = new List<Token>();

        private bool IsAtEnd() => _current >= _source.Length;

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
        private int _column = 1;

        private static readonly IDictionary<string, TokenType> _keywords = new Dictionary<string, TokenType> {
            { "and", TokenType.AND },
            { "class", TokenType.CLASS },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "fun", TokenType.FUN },
            { "if", TokenType.IF },
            { "nil", TokenType.NIL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super", TokenType.SUPER },
            { "this", TokenType.THIS },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "while", TokenType.WHILE }
        };

        public Scanner(string source)
        {
            _source = source;
        }

        public IList<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line, _column));
            return _tokens;
        }

        private void ScanToken()
        {
            var currentChar = Advance();
            switch (currentChar)
            {
                case '(':
                    AddToken(TokenType.LEFT_PAREN);
                    break;
                case ')':
                    AddToken(TokenType.RIGHT_PAREN);
                    break;
                case '{':
                    AddToken(TokenType.LEFT_BRACE);
                    break;
                case '}':
                    AddToken(TokenType.RIGHT_BRACE);
                    break;
                case ',':
                    AddToken(TokenType.COMMA);
                    break;
                case '.':
                    AddToken(TokenType.DOT);
                    break;
                case '-':
                    AddToken(TokenType.MINUS);
                    break;
                case '+':
                    AddToken(TokenType.PLUS);
                    break;
                case ';':
                    AddToken(TokenType.SEMICOLON);
                    break;
                case '*':
                    AddToken(TokenType.STAR);
                    break;
                case '!':
                    AddToken(Match('=') ?
                        TokenType.BANG_EQUAL :
                        TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ?
                        TokenType.EQUAL_EQUAL :
                        TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ?
                        TokenType.LESS_EQUAL :
                        TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ?
                        TokenType.GREATER_EQUAL :
                        TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // If it is a comment, we want to consume the entire line
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    NextLine();
                    break;
                case '"':
                    String();
                    break;
                default:
                    if (IsDigit(currentChar))
                    {
                        Number();
                    }
                    else if (IsAlpha(currentChar))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lox.Error(_line, _column - 1, "Unexpected character.");
                    }
                    break;
            }
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsDigit(char c)
        {
            return Char.IsDigit(c);
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            var identifierValue = _source.Substring(_start, _current - _start);
            var tokenType = _keywords.ContainsKey(identifierValue) ? _keywords[identifierValue] : TokenType.IDENTIFIER;
            AddToken(tokenType);
        }

        private void Number()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            var numberValue = Convert.ToDouble(_source.Substring(_start, _current - _start));
            AddToken(TokenType.NUMBER, numberValue);
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') NextLine();
                Advance();
            }

            if (IsAtEnd())
            {
                Lox.Error(_line, _column, "Unterminated string.");
                return;
            }

            Advance();

            // Length is difference between _current and _start minus the two quotes
            var length = (_current - _start) - 2;
            var stringLiteralValue = _source.Substring(_start + 1, length);
            AddToken(TokenType.STRING, stringLiteralValue);
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            Advance();
            return true;
        }

        private char Advance()
        {
            _current++;
            _column++;
            return _source[_current - 1];
        }

        private void NextLine()
        {
            _line++;
            _column = 1;
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            var text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line, _column - 1));
        }
    }
}
