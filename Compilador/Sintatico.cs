using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;

namespace Compilador
{
    public class Sintatico
    {
        private readonly List<Token> _tokens;
        private int _atual = 0;

        private TabelaSimbolos _tabela = new TabelaSimbolos();
        private string _escopoAtual = "global";

        public Sintatico(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Token TokenAtual => _tokens[_atual];

        private void Match(TipoToken tipoEsperado)
        {
            if (TokenAtual.Tipo == tipoEsperado)
            {
                _atual++;
            }
            else
            {
                throw new Exception(
                    $"Erro Sintático na linha {TokenAtual.Linha}: " +
                    $"Esperado '{tipoEsperado}', mas encontrado '{TokenAtual.Tipo}' ('{TokenAtual.Valor}')"
                );
            }
        }

        public void Analisar()
        {
            Programa();
            if (TokenAtual.Tipo != TipoToken.EOF)
            {
                throw new Exception($"Erro: Tokens extras no final do arquivo: {TokenAtual.Valor}");
            }
        }

        //Regras da gramática

        // <programa> -> <?php <corpo> ?>
        private void Programa()
        {
            Gerador.Resetar();
            Gerador.Adicionar("INPP");

            Match(TipoToken.PROGRAM_OPEN);
            Corpo();
            Match(TipoToken.PROGRAM_CLOSE);

            Gerador.Adicionar("PARA");
            Gerador.SalvarArquivo("teste.txt");
        }

        // <corpo> -> <dc> <comandos>
        private void Corpo()
        {
            Dc();
            Comandos();
        }

        // <dc> -> <dc_v> <mais_dc> | <dc_f> <mais_dc_f> | λ
        private void Dc()
        {
            if (TokenAtual.Tipo == TipoToken.VAR_NAME)
            {
                Dc_v();
                Mais_dc();
            }
            else if (TokenAtual.Tipo == TipoToken.FUNCTION)
            {
                Dc_f();
                Mais_dc_f();
            }
        }

        // <mais_dc> -> <dc> | λ
        private void Mais_dc()
        {
            if (TokenAtual.Tipo == TipoToken.VAR_NAME || TokenAtual.Tipo == TipoToken.FUNCTION)
            {
                Dc();
            }
        }

        // <mais_dc_f> -> <dc_f> | λ
        private void Mais_dc_f()
        {
            if (TokenAtual.Tipo == TipoToken.FUNCTION)
            {
                Dc_f();
            }
        }

        // <dc_v> -> $ident <atribuicao_opcional>
        private void Dc_v()
        {
            string nome = TokenAtual.Valor;
            Match(TipoToken.VAR_NAME);

            bool existe = false;
            try
            {
                var sim = _tabela.Recuperar(nome, _escopoAtual);
                if (sim != null) existe = true;
            }
            catch
            {
                existe = false;
            }

            if (!existe)
            {
                _tabela.Declarar(nome, "real", "variavel", _escopoAtual);
                Gerador.Adicionar("ALME 1");
            }

            var simbolo = _tabela.Recuperar(nome, _escopoAtual);
            Atribuicao_opcional(simbolo);
        }

        // <atribuicao_opcional> -> = <expressao> ; | ;
        private void Atribuicao_opcional(Simbolo simbolo)
        {
            if (TokenAtual.Tipo == TipoToken.ASSIGN)
            {
                Match(TipoToken.ASSIGN);
                Expressao();
                Gerador.Adicionar($"ARMZ {simbolo.Endereco}");
                Match(TipoToken.SEMICOLON);
            }
            else
            {
                Match(TipoToken.SEMICOLON);
            }
        }

        // <dc_f> -> function ident <parametros> { <corpo_f> }
        private void Dc_f()
        {
            Match(TipoToken.FUNCTION);
            string nomeFuncao = TokenAtual.Valor;

            string labelFim = Gerador.GerarLabel();
            Gerador.Adicionar($"DSVI {labelFim}");

            string labelFuncao = Gerador.GerarLabel();
            Gerador.MarcarLabel(labelFuncao);

            _tabela.Declarar(nomeFuncao, "void", "funcao", "global");
            var simboloFuncao = _tabela.Recuperar(nomeFuncao, "global");
            simboloFuncao.Label = labelFuncao;

            Match(TipoToken.FUNC_NAME);

            _escopoAtual = nomeFuncao;

            Parametros();
            Match(TipoToken.LBRACE);
            var paramsDoEscopo = _tabela.ObterParametrosEscopo(nomeFuncao);

            for (int i = paramsDoEscopo.Count - 1; i >= 0; i--)
            {
                Gerador.Adicionar($"ARMZ {paramsDoEscopo[i].Endereco}");
            }

            Corpo_f();
            Match(TipoToken.RBRACE);

            int totalParaDesempilhar = _tabela.ContarApenasVariaveisLocais(nomeFuncao);

            if (totalParaDesempilhar > 0)
            {
                Gerador.Adicionar($"DESM {totalParaDesempilhar}");
            }

            Gerador.Adicionar("RTPR");

            Gerador.MarcarLabel(labelFim);
            _escopoAtual = "global";
        }

        // <parametros> -> ( <lista_par> ) | ( )
        private void Parametros()
        {
            Match(TipoToken.LPAREN);
            if (TokenAtual.Tipo == TipoToken.VAR_NAME)
            {
                Lista_par();
            }
            Match(TipoToken.RPAREN);
        }

        // <lista_par> -> $ident <mais_par>
        private void Lista_par()
        {
            string nomeParam = TokenAtual.Valor;
            _tabela.Declarar(nomeParam, "var", "parametro", _escopoAtual);
            Match(TipoToken.VAR_NAME);

            Mais_par();
        }

        // <mais_par> -> , $ident <mais_par> | λ
        private void Mais_par()
        {
            if (TokenAtual.Tipo == TipoToken.COMMA)
            {
                Match(TipoToken.COMMA);
                string nomeParam = TokenAtual.Valor;
                _tabela.Declarar(nomeParam, "var", "parametro", _escopoAtual);
                Match(TipoToken.VAR_NAME);
                Mais_par();
            }
        }

        // <corpo_f> -> <dc_loc> <comandos>
        private void Corpo_f()
        {
            Dc_loc();
            Comandos();
        }

        // <dc_loc> -> <dc_v> <mais_dcloc> | λ
        private void Dc_loc()
        {
            if (TokenAtual.Tipo == TipoToken.VAR_NAME)
            {
                Dc_v();
                Mais_dcloc();
            }
        }

        // <mais_dcloc> -> <dc_loc> | λ
        private void Mais_dcloc()
        {
            if (TokenAtual.Tipo == TipoToken.VAR_NAME)
            {
                Dc_loc();
            }
        }

        // <lista_arg> -> ( <argumentos> ) | ( )
        private void Lista_arg()
        {
            Match(TipoToken.LPAREN);

            if (TokenAtual.Tipo != TipoToken.RPAREN)
            {
                GerarCodigoArgumento();

                while (TokenAtual.Tipo == TipoToken.COMMA)
                {
                    Match(TipoToken.COMMA);
                    GerarCodigoArgumento();
                }
            }

            Match(TipoToken.RPAREN);
        }

        private void GerarCodigoArgumento()
        {
            if (TokenAtual.Tipo == TipoToken.VAR_NAME)
            {
                string nome = TokenAtual.Valor;
                var simbolo = _tabela.Recuperar(nome, _escopoAtual);

                Match(TipoToken.VAR_NAME);

                Gerador.Adicionar($"PARAM {simbolo.Endereco}");
            }
            else
            {
                Expressao();
            }
        }
        // <argumentos> -> <expressao> <mais_ident>
        private void Argumentos()
        {
            Expressao();
            Mais_ident();
        }

        // <mais_ident> -> , <expressao> <mais_ident> | λ
        private void Mais_ident()
        {
            if (TokenAtual.Tipo == TipoToken.COMMA)
            {
                Match(TipoToken.COMMA);
                Expressao();
                Mais_ident();
            }
        }

        // <comandos> -> <comando> <mais_comandos>
        private void Comandos()
        {
            Comando();
            Mais_comandos();
        }

        // <mais_comandos> -> <comandos> | λ
        private void Mais_comandos()
        {
            if (TokenAtual.Tipo == TipoToken.ECHO ||
                TokenAtual.Tipo == TipoToken.IF ||
                TokenAtual.Tipo == TipoToken.WHILE ||
                TokenAtual.Tipo == TipoToken.VAR_NAME ||
                TokenAtual.Tipo == TipoToken.FUNC_NAME)

            {
                Comandos();
            }
        }

        // <comando>
        private void Comando()
        {
            if (TokenAtual.Tipo == TipoToken.ECHO)
            {
                // echo $ident . PHP_EOL;
                Match(TipoToken.ECHO);
                string nome = TokenAtual.Valor;
                var simbolo = _tabela.Recuperar(nome, _escopoAtual);
                Gerador.Adicionar($"CRVL {simbolo.Endereco}");
                Match(TipoToken.VAR_NAME);
                Gerador.Adicionar("IMPR");
                Match(TipoToken.DOT);
                Match(TipoToken.PHP_EOL);
                Match(TipoToken.SEMICOLON);
            }
            else if (TokenAtual.Tipo == TipoToken.IF)
            {
                // if ( <condicao> ) { <comandos> } <pfalsa>
                Match(TipoToken.IF);
                Match(TipoToken.LPAREN);
                string labelElse = Gerador.GerarLabel();
                string labelFim = Gerador.GerarLabel();
                Condicao();
                Match(TipoToken.RPAREN);
                Gerador.Adicionar($"DSVF {labelElse}");
                Match(TipoToken.LBRACE);
                Comandos();
                Match(TipoToken.RBRACE);
                Gerador.Adicionar($"DSVI {labelFim}");
                Gerador.MarcarLabel(labelElse);
                Pfalsa();
                Gerador.MarcarLabel(labelFim);
            }
            else if (TokenAtual.Tipo == TipoToken.WHILE)
            {
                // while ( <condicao> ) { <comandos> }
                Match(TipoToken.WHILE);
                Match(TipoToken.LPAREN);
                string labelInicio = Gerador.GerarLabel();
                string labelFim = Gerador.GerarLabel();
                Gerador.MarcarLabel(labelInicio);
                Condicao();
                Match(TipoToken.RPAREN);
                Gerador.Adicionar($"DSVF {labelFim}");
                Match(TipoToken.LBRACE);
                Comandos();
                Match(TipoToken.RBRACE);
                Gerador.Adicionar($"DSVI {labelInicio}");
                Gerador.MarcarLabel(labelFim);
            }
            else if (TokenAtual.Tipo == TipoToken.VAR_NAME)
            {
                // $ident <restoIdent> ;
                string nome = TokenAtual.Valor;
                var simbolo = _tabela.Recuperar(nome, _escopoAtual);
                Match(TipoToken.VAR_NAME);
                RestoIdent(simbolo);
                Match(TipoToken.SEMICOLON);
            }
            else if (TokenAtual.Tipo == TipoToken.FUNC_NAME)
            {
                string nomeFuncao = TokenAtual.Valor;
                var simbolo = _tabela.Recuperar(nomeFuncao, "global");

                // ident <lista_arg> ;
                Match(TipoToken.FUNC_NAME);

                string labelRetorno = Gerador.GerarLabel();
                Gerador.Adicionar($"PUSHER {labelRetorno}");

                Lista_arg();
                Match(TipoToken.SEMICOLON);

                Gerador.Adicionar($"CHPR {simbolo.Label}");
                Gerador.MarcarLabel(labelRetorno);
            }
            else
            {
                throw new Exception($"Comando inválido: {TokenAtual.Valor}");
            }
        }

        // <restoIdent> -> = <expressao> | <lista_arg>
        private void RestoIdent(Simbolo alvo)
        {
            if (TokenAtual.Tipo == TipoToken.ASSIGN)
            {
                Match(TipoToken.ASSIGN);
                Expressao();
                Gerador.Adicionar($"ARMZ {alvo.Endereco}");
            }
            else if (TokenAtual.Tipo == TipoToken.LPAREN)
            {
                string labelRetorno = Gerador.GerarLabel();
                Gerador.Adicionar($"PUSHER {labelRetorno}");
                Lista_arg();
                Gerador.Adicionar($"CHPR {alvo.Label}");
                Gerador.MarcarLabel(labelRetorno);
            }
            else
            {
                throw new Exception("Esperado '=' ou '(' após variável no início do comando.");
            }
        }

        // <condicao> -> <expressao> <relacao> <expressao>
        private void Condicao()
        {
            Expressao();
            string op = Relacao();
            Expressao();
            switch (op)
            {
                case "==": Gerador.Adicionar("CPIG"); break;
                case ">": Gerador.Adicionar("CPMA"); break;
                case "<": Gerador.Adicionar("CPME"); break;
                case ">=": Gerador.Adicionar("CMAI"); break;
                case "<=": Gerador.Adicionar("CPMI"); break;
                case "<>":
                case "!=": Gerador.Adicionar("CDES"); break;
                default:
                    throw new Exception($"Operador relacional desconhecido: {op}");
            }
        }

        // <relacao> -> == | != | >= | <= | > | <
        private string Relacao()
        {
            string operador = TokenAtual.Valor;
            if (TokenAtual.Tipo >= TipoToken.EQ && TokenAtual.Tipo <= TipoToken.LE)
            {
                Match(TokenAtual.Tipo);
            }
            else
            {
                throw new Exception($"Esperado operador relacional, encontrado: {TokenAtual.Valor}");
            }

            return operador;
        }

        // <expressao> -> <termo> <outros_termos> | floatval(readline());
        private void Expressao()
        {
            if (TokenAtual.Tipo == TipoToken.FLOATVAL)
            {
                Match(TipoToken.FLOATVAL);
                Match(TipoToken.LPAREN);
                Match(TipoToken.READLINE);
                Match(TipoToken.LPAREN);
                Match(TipoToken.RPAREN);
                Gerador.Adicionar("LEIT");
                Match(TipoToken.RPAREN);
            }
            else
            {
                Termo();
                Outros_termos();
            }
        }

        // <termo> -> <op_un> <fator> <mais_fatores>
        private void Termo()
        {
            Op_un();
            Fator();
            Mais_fatores();
        }

        // <op_un> -> - | λ
        private void Op_un()
        {
            if (TokenAtual.Tipo == TipoToken.MINUS)
            {
                Match(TipoToken.MINUS);
            }
        }

        // <fator> -> $ident | numero_real | ( <expressao> )
        private void Fator()
        {
            if (TokenAtual.Tipo == TipoToken.VAR_NAME)
            {
                string nome = TokenAtual.Valor;
                _tabela.Recuperar(nome, _escopoAtual);

                var simbolo = _tabela.Recuperar(nome, _escopoAtual);
                Gerador.Adicionar($"CRVL {simbolo.Endereco}");
                Match(TipoToken.VAR_NAME);
            }
            else if (TokenAtual.Tipo == TipoToken.NUMBER)
            {
                string valor = TokenAtual.Valor;
                Gerador.Adicionar($"CRCT {valor}");
                Match(TipoToken.NUMBER);
            }
            else if (TokenAtual.Tipo == TipoToken.LPAREN)
            {
                Match(TipoToken.LPAREN);
                Expressao();
                Match(TipoToken.RPAREN);
            }
            else
            {
                throw new Exception($"Fator inválido: {TokenAtual.Valor}");
            }
        }

        // <outros_termos> -> <op_ad> <termo> <outros_termos> | λ
        private void Outros_termos()
        {
            if (TokenAtual.Tipo == TipoToken.PLUS)
            {
                Match(TipoToken.PLUS);
                Termo();

                Gerador.Adicionar("SOMA");

                Outros_termos();
            }
            else if (TokenAtual.Tipo == TipoToken.MINUS)
            {
                Match(TipoToken.MINUS);
                Termo();

                Gerador.Adicionar("SUBT");

                Outros_termos();
            }
        }

        // <op_ad> -> + | -
        private void Op_ad()
        {
            if (TokenAtual.Tipo == TipoToken.PLUS) Match(TipoToken.PLUS);
            else Match(TipoToken.MINUS);
        }

        // <mais_fatores> -> <op_mul> <fator> <mais_fatores> | λ
        private void Mais_fatores()
        {
            if (TokenAtual.Tipo == TipoToken.MULT)
            {
                Match(TipoToken.MULT);
                Fator();
                Gerador.Adicionar("MULT");
                Mais_fatores();
            }
            else if (TokenAtual.Tipo == TipoToken.DIV)
            {
                Match(TipoToken.DIV);
                Fator();
                Gerador.Adicionar("DIVI");
                Mais_fatores();
            }
        }

        // <op_mul> -> * | /
        private void Op_mul()
        {
            if (TokenAtual.Tipo == TipoToken.MULT) Match(TipoToken.MULT);
            else Match(TipoToken.DIV);
        }

        // <pfalsa> -> else { <comandos> } | λ
        private void Pfalsa()
        {
            if (TokenAtual.Tipo == TipoToken.ELSE)
            {
                Match(TipoToken.ELSE);
                Match(TipoToken.LBRACE);
                Comandos();
                Match(TipoToken.RBRACE);
            }
        }
        private bool IsStartOfExpression()
        {
            return TokenAtual.Tipo == TipoToken.MINUS ||
                   TokenAtual.Tipo == TipoToken.LPAREN ||
                   TokenAtual.Tipo == TipoToken.VAR_NAME ||
                   TokenAtual.Tipo == TipoToken.NUMBER ||
                   TokenAtual.Tipo == TipoToken.FLOATVAL;
        }
    }
}