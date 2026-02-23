using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace VM
{
    class Program
    {
        static List<string> Instrucoes = new List<string>();
        static Stack<double> Pilha = new Stack<double>();
        static double[] Memoria = new double[1000];
        static int PC = 0;

        static void Main(string[] args)
        {
            string caminhoArquivo = args.Length > 0 ? args[0] : "teste.txt";

            if (!File.Exists(caminhoArquivo))
            {
                Console.WriteLine($"[ERRO] Arquivo '{caminhoArquivo}' não encontrado.");
                return;
            }

            Instrucoes.AddRange(File.ReadAllLines(caminhoArquivo));

            Console.WriteLine($"INICIANDO VM ({caminhoArquivo})");

            try
            {
                Executar();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"[CRASH] Erro fatal na linha {PC}: {ex.Message}");
            }
        }

        static void Executar()
        {
            while (PC < Instrucoes.Count)
            {
                string linha = Instrucoes[PC].Trim();

                if (string.IsNullOrWhiteSpace(linha) || linha.StartsWith("#") || linha.StartsWith("//"))
                {
                    PC++;
                    continue;
                }

                var partes = linha.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string comando = partes[0].ToUpper();
                string param = partes.Length > 1 ? partes[1] : "";

                switch (comando)
                {
                    // --- 1. CONTROLE DE PROGRAMA E MEMÓRIA ---
                    case "INPP":
                        PC++;
                        break;

                    case "PARA":
                        return;

                    case "ALME":
                        int qtdAlme = int.Parse(param);
                        for (int i = 0; i < qtdAlme; i++)
                        {
                            Pilha.Push(0.0);
                        }
                        PC++;
                        break;

                    case "DESM":
                        int qtdDesm = int.Parse(param);
                        for (int i = 0; i < qtdDesm; i++)
                        {
                            if (Pilha.Count > 0) Pilha.Pop();
                        }
                        PC++;
                        break;

                    // --- 2. MOVIMENTAÇÃO DE DADOS ---
                    case "CRCT":
                        Pilha.Push(double.Parse(param, CultureInfo.InvariantCulture));
                        PC++;
                        break;

                    case "CRVL":
                        int endCrval = int.Parse(param);
                        Pilha.Push(Memoria[endCrval]);
                        PC++;
                        break;

                    case "ARMZ":
                        int endArmz = int.Parse(param);
                        Memoria[endArmz] = Pilha.Pop();
                        PC++;
                        break;

                    case "PARAM":
                        int endParam = int.Parse(param);
                        Pilha.Push(Memoria[endParam]);
                        PC++;
                        break;

                    // --- 3. ARITMÉTICA (CORRIGIDO AQUI) ---
                    case "SOMA":
                        double bSoma = Pilha.Pop();
                        double aSoma = Pilha.Pop();
                        Pilha.Push(aSoma + bSoma);
                        PC++;
                        break;

                    case "SUBT":
                        double bSub = Pilha.Pop();
                        double aSub = Pilha.Pop();
                        Pilha.Push(aSub - bSub);
                        PC++;
                        break;

                    case "MULT":
                        double bMult = Pilha.Pop();
                        double aMult = Pilha.Pop();
                        Pilha.Push(aMult * bMult);
                        PC++;
                        break;

                    case "DIVI":
                        double bDiv = Pilha.Pop();
                        double aDiv = Pilha.Pop();
                        if (bDiv == 0) throw new DivideByZeroException();
                        Pilha.Push(aDiv / bDiv);
                        PC++;
                        break;

                    case "INVE":
                        double valInve = Pilha.Pop();
                        Pilha.Push(-valInve);
                        PC++;
                        break;

                    // --- 4. LÓGICA E COMPARAÇÃO ---
                    case "CONJ":
                        double bConj = Pilha.Pop();
                        double aConj = Pilha.Pop();
                        Pilha.Push((aConj == 1 && bConj == 1) ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "DISJ":
                        double bDisj = Pilha.Pop();
                        double aDisj = Pilha.Pop();
                        Pilha.Push((aDisj == 1 || bDisj == 1) ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "NEGA":
                        double valNega = Pilha.Pop();
                        Pilha.Push(1.0 - valNega);
                        PC++;
                        break;

                    case "CPME":
                        double bCpme = Pilha.Pop();
                        double aCpme = Pilha.Pop();
                        Pilha.Push(aCpme < bCpme ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "CPMA":
                        double bCpma = Pilha.Pop();
                        double aCpma = Pilha.Pop();
                        Pilha.Push(aCpma > bCpma ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "CPIG":
                        double bCpig = Pilha.Pop();
                        double aCpig = Pilha.Pop();
                        Pilha.Push(aCpig == bCpig ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "CDES":
                        double bCdes = Pilha.Pop();
                        double aCdes = Pilha.Pop();
                        Pilha.Push(aCdes != bCdes ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "CPMI":
                        double bCpmi = Pilha.Pop();
                        double aCpmi = Pilha.Pop();
                        Pilha.Push(aCpmi <= bCpmi ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "CMAI":
                        double bCmai = Pilha.Pop();
                        double aCmai = Pilha.Pop();
                        Pilha.Push(aCmai >= bCmai ? 1.0 : 0.0);
                        PC++;
                        break;

                    // --- 5. DESVIO E PROCEDIMENTOS ---
                    case "DSVI":
                        PC = int.Parse(param);
                        break;

                    case "DSVF":
                        double condicao = Pilha.Pop();
                        if (condicao == 0.0)
                        {
                            PC = int.Parse(param);
                        }
                        else
                        {
                            PC++;
                        }
                        break;

                    case "PUSHER":
                        Pilha.Push(double.Parse(param));
                        PC++;
                        break;

                    case "CHPR":
                        PC = int.Parse(param);
                        break;

                    case "RTPR":
                        int endRetorno = (int)Pilha.Pop();
                        PC = endRetorno;
                        break;

                    // --- 6. E/S ---
                    case "LEIT":
                        Console.Write("DIGITE UM VALOR > ");
                        string entrada = Console.ReadLine() ?? "0";
                        if (double.TryParse(entrada.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double valorLido))
                            Pilha.Push(valorLido);
                        else
                            Pilha.Push(0.0);
                        PC++;
                        break;

                    case "IMPR":
                        Console.WriteLine($"SAÍDA: {Pilha.Pop()}");
                        PC++;
                        break;

                    default:
                        throw new Exception($"Comando desconhecido: {comando}");
                }
            }
        }
    }
}