using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SwitchRTParser.Lib
{
    public class CNACLRParser
    {
        public static List<SwitchCNACLR> Parse(string input)
        {
            var results = new List<SwitchCNACLR>();
            if (string.IsNullOrWhiteSpace(input)) return results;

            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Look for the header row to start parsing
                if (line.Contains("Call source name") && line.Contains("Call prefix"))
                {
                    i = ParseTable(lines, i, results);
                }
            }

            return results;
        }

        private static int ParseTable(string[] lines, int headerIndex, List<SwitchCNACLR> results)
        {
            // Skip header and any separator line or empty lines
            int i = headerIndex + 1;
            while (i < lines.Length)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Contains("---") || trimmed.Contains("Call source name"))
                {
                    i++;
                }
                else
                {
                    break;
                }
            }

            while (i < lines.Length)
            {
                string line = lines[i];
                if (IsEndOfBatch(line)) break;

                var parts = SplitLine(line);
                if (parts.Length >= 5)
                {
                    var cnaclr = new SwitchCNACLR
                    {
                        CSCNAME = parts[0],
                        CLIN = parts[1],
                        P = parts[2],
                        PFX = parts[3],
                        RSNAME = parts[4]
                    };
                    results.Add(cnaclr);
                }
                i++;
            }
            return i;
        }

        private static bool IsEndOfBatch(string line)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return true;
            if (trimmed.StartsWith("To be continued...", StringComparison.OrdinalIgnoreCase)) return true;
            if (trimmed.StartsWith("(Number of results =", StringComparison.OrdinalIgnoreCase)) return true;
            if (trimmed.StartsWith("---", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private static string[] SplitLine(string line)
        {
            return Regex.Split(line.Trim(), @"\s{2,}")
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToArray();
        }
    }
}
