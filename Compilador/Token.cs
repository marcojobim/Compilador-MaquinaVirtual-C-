namespace Compilador
{
    public enum TipoToken
    {
        // Palavras Reservadas e Estruturais
        PROGRAM_OPEN,  // <?php
        PROGRAM_CLOSE,  // ?>
        FUNCTION,
        ECHO,
        PHP_EOL,
        IF, ELSE, WHILE,
        FLOATVAL,
        READLINE,

        // Símbolos
        LPAREN, RPAREN,
        LBRACE, RBRACE,
        SEMICOLON,
        COMMA,
        DOT,
        ASSIGN,

        // Operadores
        PLUS, MINUS, MULT, DIV,
        EQ, NEQ, GT, LT, GE, LE,

        // Tipos
        VAR_NAME,       // $ident
        FUNC_NAME,      // ident (nome de função)
        NUMBER,         // numero_real

        // Controle
        EOF,
        UNKNOWN   // Erro Léxico
    }

    public class Token
    {
        public TipoToken Tipo { get; set; }
        public string Valor { get; set; }
        public int Linha { get; set; }

        public Token(TipoToken tipo, string valor, int linha)
        {
            Tipo = tipo;
            Valor = valor;
            Linha = linha;
        }
    }
}