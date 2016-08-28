using System;
using System.Collections.Generic;
using System.IO;

namespace WordChain
{
    public class Program
    {
        public static IEnumerable<String> FindChain(String inputsPath, String dictPath)
        {
            string fromWord, toWord;
            using (var reader = new StreamReader(inputsPath))
            {
                fromWord = reader.ReadLine();
                toWord = reader.ReadLine();
            }

            var dict = File.ReadLines(dictPath);

            WordChainer wc = new WordChainer(dict);
            return wc.FindChain(fromWord, toWord);
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }
            
            var inputFile = args[0];
            var dictFile = args[1];

            foreach (var word in FindChain(inputFile, dictFile))
            {
                Console.WriteLine(word);
            }

        }

        private static void PrintUsage()
        {
            Console.WriteLine(@"Make an elephant from a fly.
Secify two files as input, 
    First should contain start and finish word, each one on a separate line
    Second contains dictionary used for transformation, one word per line");
        }
    }
}
