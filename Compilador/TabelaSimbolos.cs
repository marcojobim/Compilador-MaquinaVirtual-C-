using System;
using System.Collections.Generic;

namespace Compilador
{
    public class Simbolo
    {
        public string? Nome { get; set; }
        public string? Tipo { get; set; }
        public string? Categoria { get; set; }
        public string? Escopo { get; set; }
        public int Endereco { get; set; } = -1;
        public string? Label { get; set; }
    }

    public class TabelaSimbolos
    {
        private Dictionary<string, Simbolo> _tabela = new Dictionary<string, Simbolo>();
        private int _contadorEndereco = 0;

        public void Declarar(string nome, string tipo, string categoria, string escopo)
        {
            string chave = $"{escopo}:{nome}";

            if (_tabela.ContainsKey(chave))
            {
                Atualizar(nome, escopo, tipo);
                return;
            }

            int endereco = -1;

            if (categoria != "funcao")
            {
                endereco = _contadorEndereco++;
            }

            var simbolo = new Simbolo
            {
                Nome = nome,
                Tipo = tipo,
                Categoria = categoria,
                Escopo = escopo,
                Endereco = endereco
            };

            _tabela[chave] = simbolo;
        }

        public void Atualizar(string nome, string escopo, string? novoTipo = null)
        {
            string chaveLocal = $"{escopo}:{nome}";
            if (_tabela.ContainsKey(chaveLocal))
            {
                if (novoTipo != null) _tabela[chaveLocal].Tipo = novoTipo;
                return;
            }

            string chaveGlobal = $"global:{nome}";
            if (_tabela.ContainsKey(chaveGlobal))
            {
                if (novoTipo != null) _tabela[chaveGlobal].Tipo = novoTipo;
                return;
            }

            throw new Exception($"Erro Semântico: Tentativa de atribuir valor a variável não declarada '{nome}' no escopo '{escopo}'.");
        }

        public Simbolo Recuperar(string nome, string escopoAtual)
        {
            string chaveLocal = $"{escopoAtual}:{nome}";
            if (_tabela.ContainsKey(chaveLocal))
            {
                return _tabela[chaveLocal];
            }

            if (escopoAtual != "global")
            {
                string chaveGlobal = $"global:{nome}";
                if (_tabela.ContainsKey(chaveGlobal))
                {
                    return _tabela[chaveGlobal];
                }
            }

            throw new Exception($"Erro Semântico: O símbolo '{nome}' não foi declarado (Escopo: {escopoAtual}).");
        }
        public int ContarVariaveisEscopo(string nomeEscopo)
        {
            int count = 0;
            foreach (var s in _tabela.Values)
            {
                if (s.Escopo == nomeEscopo && (s.Categoria == "variavel" || s.Categoria == "parametro"))
                    count++;
            }
            return count;
        }

        public int ContarApenasVariaveisLocais(string escopo)
        {
            int count = 0;
            foreach (var s in _tabela.Values)
            {
                if (s.Escopo == escopo && s.Categoria == "variavel")
                {
                    count++;
                }
            }
            return count;
        }

        public List<Simbolo> ObterParametrosEscopo(string escopo)
        {
            var parametros = new List<Simbolo>();
            foreach (var s in _tabela.Values)
            {
                if (s.Escopo == escopo && s.Categoria == "parametro")
                {
                    parametros.Add(s);
                }
            }
            parametros.Sort((a, b) => a.Endereco.CompareTo(b.Endereco));
            return parametros;
        }
    }
}