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
                if (line.Contains("Route selection name") && line.Contains("Sub-route selection mode"))
                {
                    i = ParseTable(lines, i, results);
                }
            }

            return results;
        }

        private static int ParseTable(string[] lines, int headerIndex, List<SwitchRTANA> results)
        {
            // Skip header and any separator line or empty lines
            int i = headerIndex + 1;
            while (i < lines.Length)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Contains("---") || trimmed.Contains("Route selection name"))
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
                if (parts.Length >= 3)
                {
                    var rtana = new SwitchRTANA
                    {
                        RSN = parts[0],
                        RTSM = MapRTSM(parts[1]),
                        RN = parts[2]
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
