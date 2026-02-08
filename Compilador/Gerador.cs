using System;
using System.Collections.Generic;
using System.IO;

namespace Compilador
{
    public static class Gerador
    {
        private static List<string> _instrucoes = new List<string>();
        private static int _labelCounter = 1;

        public static void Resetar()
        {
            _instrucoes.Clear();
            _labelCounter = 1;
        }

        public static void Adicionar(string instrucao)
        {
            _instrucoes.Add(instrucao);
        }

        public static string GerarLabel()
        {
            return $"LB_{_labelCounter++}";
        }

        public static void MarcarLabel(string label)
        {
            _instrucoes.Add($"{label} NADA");
        }

        public static void SalvarArquivo(string caminho)
        {
            var codigoFinal = new List<string>();
            var mapaLabels = new Dictionary<string, int>();

            int linhaReal = 0;
            foreach (var instr in _instrucoes)
            {
                if (instr.StartsWith("LB_"))
                {
                    string labelNome = instr.Split(' ')[0];
                    mapaLabels[labelNome] = linhaReal;
                }
                else
                {
                    linhaReal++;
                }
            }

            foreach (var instr in _instrucoes)
            {
                if (instr.StartsWith("LB_")) continue;

                string[] partes = instr.Split(' ');
                string comando = partes[0];

                if (partes.Length > 1)
                {
                    string argumento = partes[1];

                    if (mapaLabels.ContainsKey(argumento))
                    {
                        int numeroLinha = mapaLabels[argumento];
                        codigoFinal.Add($"{comando} {numeroLinha}");
                    }
                    else
                    {
                        codigoFinal.Add(instr);
                    }
                }
                else
                {
                    codigoFinal.Add(instr);
                }
            }

            File.WriteAllLines(caminho, codigoFinal);
        }
    }
}