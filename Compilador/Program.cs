using System;
using System.IO;

namespace Compilador
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // 1. Ler o arquivo
                string codigo = File.ReadAllText("correto.php.txt");
                Console.WriteLine("Código lido com sucesso!");

                // 2. Análise Léxica
                var lexico = new Lexico(codigo);
                var tokens = lexico.Scan();
                Console.WriteLine($"Análise Léxica concluída! {tokens.Count} tokens gerados.");

                // 3. Análise Sintática
                Console.WriteLine("Iniciando Análise Sintática...");
                var sintatico = new Sintatico(tokens);
                sintatico.Analisar();

                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("SUCESSO: O código é sintaticamente válido!");
                Console.WriteLine("------------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nERRO FATAL:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }
    }
}