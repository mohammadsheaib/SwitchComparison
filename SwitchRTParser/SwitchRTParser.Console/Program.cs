using System;
using System.IO;
using System.Text.Json;
using SwitchRTParser.Lib;

namespace SwitchRTParser.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SwitchRTParser.Console <input_file_path> <output_file_path>");
                return;
            }

            string inputPath = args[0];
            string outputPath = args[1];

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Error: Input file '{inputPath}' does not exist.");
                return;
            }

            try
            {
                Console.WriteLine($"Reading input file: {inputPath}");
                string input = File.ReadAllText(inputPath);

                Console.WriteLine("Parsing RTs...");
                var rts = RTsParser.Parse(input);

                Console.WriteLine($"Found {rts.Count} RTs.");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(rts, options);

                Console.WriteLine($"Writing results to: {outputPath}");
                File.WriteAllText(outputPath, json);

                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
