using SwitchRTParser.Lib;
using System.Text;
using System.Text.Json;

namespace ParsersTester
{
    internal class Program
    {
        private const string OutputDirectory = @"C:\RTParserOutput";

        private static readonly Dictionary<string, Func<string, object>> Parsers =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "RT", input => RTsParser.Parse(input) },
                { "RTANA", input => RTANAParser.Parse(input) },
                { "CNACLD", input => CNACLDParser.Parse(input) },
                { "CNACLR", input => CNACLRParser.Parse(input) }
            };

        static int Main(string[] args)
        {
            try
            {
                string parserName = "CNACLD";
                string inputFilePath = @"C:\Users\User\Desktop\SwitchComparison\LST CNACLD - ALL PREFIXes.txt";

                if (!Parsers.TryGetValue(parserName, out var parser))
                {
                    Console.WriteLine($"Unknown parser: {parserName}");
                    PrintAvailableParsers();
                    return 1;
                }

                if (!File.Exists(inputFilePath))
                {
                    Console.WriteLine($"File not found: {inputFilePath}");
                    return 1;
                }

                Console.WriteLine($"Using parser: {parserName}");
                Console.WriteLine("Reading input file...");

                string inputText = File.ReadAllText(inputFilePath, Encoding.UTF8);

                Console.WriteLine("Parsing...");
                object result = parser(inputText);

                Directory.CreateDirectory(OutputDirectory);

                string outputFileName =
                    $"{parserName}_{Path.GetFileNameWithoutExtension(inputFilePath)}_{DateTime.Now:yyyyMMddHHmmss}.json";

                string outputFilePath = Path.Combine(OutputDirectory, outputFileName);

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonOutput = JsonSerializer.Serialize(result, jsonOptions);

                File.WriteAllText(outputFilePath, jsonOutput, Encoding.UTF8);

                Console.WriteLine("Parsing completed successfully.");
                Console.WriteLine($"Output written to: {outputFilePath}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("ParsersTester <ParserName> <InputFilePath>");
            Console.WriteLine();
            PrintAvailableParsers();
        }

        private static void PrintAvailableParsers()
        {
            Console.WriteLine("Available Parsers:");
            foreach (var key in Parsers.Keys)
            {
                Console.WriteLine($" - {key}");
            }
        }
    }
}