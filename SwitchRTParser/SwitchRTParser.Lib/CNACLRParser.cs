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
            string headerLine = lines[headerIndex];
            var headerParts = SplitLine(headerLine);

            int cscIdx = Array.FindIndex(headerParts, h => h.Equals("Call source name", StringComparison.OrdinalIgnoreCase));
            int clinIdx = Array.FindIndex(headerParts, h => h.Equals("Call prefix", StringComparison.OrdinalIgnoreCase));
            int pIdx = Array.FindIndex(headerParts, h => h.Equals("DN set", StringComparison.OrdinalIgnoreCase));
            int pfxIdx = Array.FindIndex(headerParts, h => h.Equals("Caller number", StringComparison.OrdinalIgnoreCase));
            int rsIdx = Array.FindIndex(headerParts, h => h.Equals("Route selection name", StringComparison.OrdinalIgnoreCase));

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

                // Ensure we have at least enough parts to reach the maximum required index
                int maxIdx = Math.Max(cscIdx, Math.Max(clinIdx, Math.Max(pIdx, Math.Max(pfxIdx, rsIdx))));

                if (parts.Length > 0)
                {
                    var cnaclr = new SwitchCNACLR
                    {
                        CSCNAME = cscIdx >= 0 && cscIdx < parts.Length ? parts[cscIdx] : string.Empty,
                        CLIN = clinIdx >= 0 && clinIdx < parts.Length ? parts[clinIdx] : string.Empty,
                        P = pIdx >= 0 && pIdx < parts.Length ? parts[pIdx] : string.Empty,
                        PFX = pfxIdx >= 0 && pfxIdx < parts.Length ? parts[pfxIdx] : string.Empty,
                        RSNAME = rsIdx >= 0 && rsIdx < parts.Length ? parts[rsIdx] : string.Empty
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
