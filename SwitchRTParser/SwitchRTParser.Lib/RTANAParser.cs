using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SwitchRTParser.Lib
{
    public class RTANAParser
    {
        public static List<SwitchRTANA> Parse(string input)
        {
            var results = new List<SwitchRTANA>();
            if (string.IsNullOrWhiteSpace(input)) return results;

            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Look for the header row to start parsing
                if (line.Contains("Route selection name", StringComparison.OrdinalIgnoreCase) &&
                    line.Contains("Sub-route selection mode", StringComparison.OrdinalIgnoreCase))
                {
                    i = ParseTable(lines, i, results);
                }
            }

            return results;
        }

        private static int ParseTable(string[] lines, int headerIndex, List<SwitchRTANA> results)
        {
            string headerLine = lines[headerIndex];
            var headerParts = SplitLine(headerLine);

            int rsnIndex = Array.FindIndex(headerParts, p => p.Equals("Route selection name", StringComparison.OrdinalIgnoreCase));
            int rtsmIndex = Array.FindIndex(headerParts, p => p.Equals("Sub-route selection mode", StringComparison.OrdinalIgnoreCase));
            int rnIndex = Array.FindIndex(headerParts, p => p.Equals("Route 1 name", StringComparison.OrdinalIgnoreCase));

            // Fallback if header names don't match exactly but we know the expected positions from user
            if (rsnIndex == -1) rsnIndex = 0;
            if (rtsmIndex == -1) rtsmIndex = 8;
            if (rnIndex == -1) rnIndex = 9;

            // Skip header and any separator line or empty lines
            int i = headerIndex + 1;
            while (i < lines.Length)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Contains("---") || trimmed.Contains("Route selection name", StringComparison.OrdinalIgnoreCase))
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
                if (parts.Length > Math.Max(rsnIndex, Math.Max(rtsmIndex, rnIndex)))
                {
                    var rtana = new SwitchRTANA
                    {
                        RSN = parts[rsnIndex],
                        RTSM = MapRTSM(parts[rtsmIndex]),
                        RN = parts[rnIndex]
                    };
                    results.Add(rtana);
                }
                i++;
            }
            return i;
        }

        private static string MapRTSM(string raw)
        {
            if (raw.Equals("Select by percentage", StringComparison.OrdinalIgnoreCase))
                return "PERC";
            if (raw.Equals("Select by sequence", StringComparison.OrdinalIgnoreCase))
                return "SEQ";
            return raw;
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
