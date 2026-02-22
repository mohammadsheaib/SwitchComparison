using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SwitchRTParser.Lib
{
    public class CNACLDParser
    {
        public static List<SwitchCNACLD> Parse(string input)
        {
            var callPrefixData = new List<CallPrefixEntry>();
            var routeSelectionData = new Dictionary<(string P, string PFX), RouteSelectionEntry>();

            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("Call prefix data", StringComparison.OrdinalIgnoreCase))
                {
                    i = ParseCallPrefixData(lines, i + 1, callPrefixData);
                }
                else if (line.StartsWith("Route selection data", StringComparison.OrdinalIgnoreCase))
                {
                    i = ParseRouteSelectionData(lines, i + 1, routeSelectionData);
                }
            }

            var results = new List<SwitchCNACLD>();
            foreach (var cp in callPrefixData)
            {
                var cnacld = new SwitchCNACLD
                {
                    P = cp.P,
                    PFX = cp.PFX,
                    MINL = cp.MINL,
                    MAXL = cp.MAXL
                };

                if (routeSelectionData.TryGetValue((cnacld.P, cnacld.PFX), out var rs))
                {
                    cnacld.SDESCRIPTION = rs.SDESCRIPTION;
                    cnacld.SN = rs.SN;
                    cnacld.CLIANA = rs.CLIANA;
                }

                results.Add(cnacld);
            }

            return results;
        }

        private static int SkipToData(string[] lines, int i)
        {
            while (i < lines.Length)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Contains("---") || trimmed.Contains("DN set"))
                {
                    i++;
                }
                else
                {
                    break;
                }
            }
            return i;
        }

        private static int ParseCallPrefixData(string[] lines, int startIndex, List<CallPrefixEntry> callPrefixData)
        {
            int i = SkipToData(lines, startIndex);

            while (i < lines.Length)
            {
                string line = lines[i];
                if (IsEndOfBatch(line)) break;

                var parts = SplitLine(line);
                if (parts.Length >= 7)
                {
                    callPrefixData.Add(new CallPrefixEntry {
                        P = parts[0],
                        PFX = parts[1],
                        MINL = parts[5], // field number 6
                        MAXL = parts[6]  // field number 7
                    });
                }
                i++;
            }
            return i;
        }

        private static int ParseRouteSelectionData(string[] lines, int startIndex, Dictionary<(string P, string PFX), RouteSelectionEntry> routeSelectionData)
        {
            int i = SkipToData(lines, startIndex);

            while (i < lines.Length)
            {
                string line = lines[i];
                if (IsEndOfBatch(line)) break;

                var parts = SplitLine(line);
                if (parts.Length >= 16)
                {
                    string p = parts[0];
                    string pfx = parts[1];
                    var entry = new RouteSelectionEntry
                    {
                        SDESCRIPTION = parts[8], // field number 9
                        CLIANA = parts[15]       // field number 16
                    };

                    var key = (p, pfx);
                    if (!routeSelectionData.ContainsKey(key))
                    {
                        routeSelectionData[key] = entry;
                    }
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
            if (trimmed.StartsWith("Call prefix data", StringComparison.OrdinalIgnoreCase)) return true;
            if (trimmed.StartsWith("Route selection data", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private static string[] SplitLine(string line)
        {
            return Regex.Split(line.Trim(), @"\s{2,}")
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToArray();
        }

        private class CallPrefixEntry
        {
            public string P { get; set; } = string.Empty;
            public string PFX { get; set; } = string.Empty;
            public string MINL { get; set; } = string.Empty;
            public string MAXL { get; set; } = string.Empty;
        }

        private class RouteSelectionEntry
        {
            public string SDESCRIPTION { get; set; } = string.Empty;
            public string SN { get; set; } = string.Empty;
            public string CLIANA { get; set; } = string.Empty;
        }
    }
}
