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
            string caminhoArquivo = args.Length > 0 ? args[0] : "teste.txt"; // Caso não queira passar o arquivo por parâmetro é só alterar o teste.txt para o arquivo desejado

            if (!File.Exists(caminhoArquivo))
            {
                Console.WriteLine($"[ERRO] Arquivo '{caminhoArquivo}' não encontrado.");
                Console.WriteLine("Certifique-se de que o compilador gerou o arquivo na mesma pasta.");
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
                Console.WriteLine($"Comando: {Instrucoes[PC]}");
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
                    case "INPP":
                        PC++;
                        break;

                    case "PARA":
                        return;

                    case "CRCT":
                        Pilha.Push(double.Parse(param, CultureInfo.InvariantCulture));
                        PC++;
                        break;

                    case "CRVL":
                    case "PARAM":
                        int endCrvl = int.Parse(param);
                        Pilha.Push(Memoria[endCrvl]);
                        PC++;
                        break;

                    case "ARMZ":
                        int endArmz = int.Parse(param);
                        Memoria[endArmz] = Pilha.Pop();
                        PC++;
                        break;

                    case "ALME":
                        PC++;
                        break;

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
                        if (bDiv == 0) throw new DivideByZeroException("Divisão por zero");
                        Pilha.Push(aDiv / bDiv);
                        PC++;
                        break;

                    case "CMMA":
                        double bMaior = Pilha.Pop();
                        double aMaior = Pilha.Pop();
                        Pilha.Push(aMaior > bMaior ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "CMME":
                        double bMenor = Pilha.Pop();
                        double aMenor = Pilha.Pop();
                        Pilha.Push(aMenor < bMenor ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "CMAI":
                        double bMaiorI = Pilha.Pop();
                        double aMaiorI = Pilha.Pop();
                        Pilha.Push(aMaiorI >= bMaiorI ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "CMEI":
                        double bMenorI = Pilha.Pop();
                        double aMenorI = Pilha.Pop();
                        Pilha.Push(aMenorI <= bMenorI ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "CPMI":
                        double bIgual = Pilha.Pop();
                        double aIgual = Pilha.Pop();
                        Pilha.Push(aIgual == bIgual ? 1.0 : 0.0);
                        PC++;
                        break;

                    case "DIF":
                        double bDif = Pilha.Pop();
                        double aDif = Pilha.Pop();
                        Pilha.Push(aDif != bDif ? 1.0 : 0.0);
                        PC++;
                        break;

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
                        int enderecoRetorno = (int)Pilha.Pop();
                        PC = enderecoRetorno;
                        break;

                    case "DESM":
                        int n = int.Parse(param);
                        for (int i = 0; i < n; i++)
                        {
                            if (Pilha.Count > 0) Pilha.Pop();
                        }
                        PC++;
                        break;

                    case "LEIT":
                        Console.Write("DIGITE UM VALOR > ");
                        string entrada = Console.ReadLine() ?? "0";

                        if (double.TryParse(entrada.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double valorLido))
                        {
                            Pilha.Push(valorLido);
                        }
                        else
                        {
                            Console.WriteLine("Valor inválido. Usando 0.0");
                            Pilha.Push(0.0);
                        }
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