using System;
using System.Collections.Generic;

namespace Compilador
{
    public class Lexico
    {
        private readonly string _codigo;
        private readonly List<Token> _tokens = new List<Token>();
        private int _inicio = 0;
        private int _atual = 0;
        private int _linha = 1;

        public List<string> Erros { get; } = new List<string>();

        private static readonly Dictionary<string, TipoToken> _palavrasReservadas = new Dictionary<string, TipoToken>
        {
            { "if", TipoToken.IF },
            { "else", TipoToken.ELSE },
            { "while", TipoToken.WHILE },
            { "echo", TipoToken.ECHO },
            { "function", TipoToken.FUNCTION },
            { "PHP_EOL", TipoToken.PHP_EOL },
            { "floatval", TipoToken.FLOATVAL },
            { "readline", TipoToken.READLINE }
        };

        public Lexico(string codigo)
        {
            _codigo = codigo;
        }

        public List<Token> Scan()
        {
            while (!IsAtEnd())
            {
                _inicio = _atual;
                ScanToken();
            }

            _tokens.Add(new Token(TipoToken.EOF, "", _linha));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Avancar();

            switch (c)
            {
                case '(': AddToken(TipoToken.LPAREN); break;
                case ')': AddToken(TipoToken.RPAREN); break;
                case '{': AddToken(TipoToken.LBRACE); break;
                case '}': AddToken(TipoToken.RBRACE); break;
                case ';': AddToken(TipoToken.SEMICOLON); break;
                case ',': AddToken(TipoToken.COMMA); break;
                case '.': AddToken(TipoToken.DOT); break;
                case '+': AddToken(TipoToken.PLUS); break;
                case '-': AddToken(TipoToken.MINUS); break;
                case '*': AddToken(TipoToken.MULT); break;

                case '=':
                    AddToken(Match('=') ? TipoToken.EQ : TipoToken.ASSIGN);
                    break;
                case '!':
                    AddToken(Match('=') ? TipoToken.NEQ : TipoToken.UNKNOWN);
                    break;
                case '<':
                    if (Match('?'))
                    {
                        if (_atual + 3 <= _codigo.Length)
                        {
                            string aux = _codigo.Substring(_atual, 3);

                            if (aux == "php")
                            {
                                _atual += 3;
                                AddToken(TipoToken.PROGRAM_OPEN);
                            }

                            else
                            {
                                Erros.Add($"Linha {_linha}: Esperado 'php' após '<?'");
                            }
                        }
                        else
                        {
                            Erros.Add($"Linha {_linha}: Arquivo terminou inesperadamente após '<?'");
                        }
                    }
                    else
                    {
                        AddToken(Match('=') ? TipoToken.LE : TipoToken.LT);
                    }
                    break;
                case '>':
                    AddToken(Match('=') ? TipoToken.GE : TipoToken.GT);
                    break;
                case '?':
                    if (Match('>')) AddToken(TipoToken.PROGRAM_CLOSE);
                    else Erros.Add($"Linha {_linha}: Caractere '?' inesperado.");
                    break;

                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd()) Avancar();
                    }
                    else if (Match('*'))
                    {
                        while (!IsAtEnd())
                        {
                            if (Peek() == '*' && PeekNext() == '/')
                            {
                                Avancar();
                                Avancar();
                                break;
                            }
                            Avancar();
                        }
                    }
                    else
                    {
                        AddToken(TipoToken.DIV);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n':
                    _linha++;
                    break;

                case '$':
                    Variavel();
                    break;

                default:
                    if (IsDigit(c))
                    {
                        LerNumero();
                    }
                    else if (IsAlpha(c))
                    {
                        LerIdentificador();
                    }
                    else
                    {
                        Erros.Add($"Linha {_linha}: Caractere inválido '{c}'");
                    }
                    break;
            }
        }


        private void Variavel()
        {
            while (IsAlphaNumeric(Peek())) Avancar();

            AddToken(TipoToken.VAR_NAME);
        }

        private void LerIdentificador()
        {
            while (IsAlphaNumeric(Peek())) Avancar();

            string texto = _codigo.Substring(_inicio, _atual - _inicio);

            TipoToken tipo = _palavrasReservadas.ContainsKey(texto) ? _palavrasReservadas[texto] : TipoToken.FUNC_NAME;
            AddToken(tipo);
        }

        private void LerNumero()
        {
            while (IsDigit(Peek())) Avancar();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Avancar();
                while (IsDigit(Peek())) Avancar();
            }

            AddToken(TipoToken.NUMBER);
        }

        private void BlockComment()
        {
            while (!IsAtEnd() && !(Peek() == '*' && PeekNext() == '/'))
            {
                if (Peek() == '\n') _linha++;
                Avancar();
            }

            if (IsAtEnd())
            {
                Erros.Add($"Linha {_linha}: Comentário não fechado.");
                return;
            }

            Avancar();
            Avancar();
        }

        private bool IsAtEnd() => _atual >= _codigo.Length;

        private char Avancar()
        {
            return _codigo[_atual++];
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _codigo[_atual];
        }

        private char PeekNext()
        {
            if (_atual + 1 >= _codigo.Length) return '\0';
            return _codigo[_atual + 1];
        }

        private bool Match(char esperado)
        {
            if (IsAtEnd()) return false;
            if (_codigo[_atual] != esperado) return false;

            _atual++;
            return true;
        }

        private void AddToken(TipoToken tipo)
        {
            string texto = _codigo.Substring(_inicio, _atual - _inicio);
            _tokens.Add(new Token(tipo, texto, _linha));
        }

        private bool IsDigit(char c) => c >= '0' && c <= '9';

        private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
    }
}