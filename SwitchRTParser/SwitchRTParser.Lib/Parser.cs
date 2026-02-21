using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SwitchRTParser.Lib
{
    public class Parser
    {
        public static SwitchRT? Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var result = new SwitchRT();

            // Extract Route Name (RN)
            result.RN = ExtractValue(input, "Route name") ?? string.Empty;

            // Extract Sub-route selection mode (SRSM)
            string? srsmRaw = ExtractValue(input, "Sub-route selection mode");
            if (srsmRaw != null)
            {
                if (srsmRaw.Equals("Select by percentage", StringComparison.OrdinalIgnoreCase))
                    result.SRSM = "PERC";
                else if (srsmRaw.Equals("Select by sequence", StringComparison.OrdinalIgnoreCase))
                    result.SRSM = "SEQ";
                else
                    result.SRSM = srsmRaw;
            }

            // Extract Server Name (SN)
            result.SN = ExtractValue(input, "Server name") ?? string.Empty;

            // Extract Sub-routes
            string?[] subRouteNames = new string?[49]; // 1-based indexing
            int[] subRoutePercentages = new int[49];

            // Use Multiline and anchor to start of line to avoid matching "Percentage of sub-route"
            var nameRegex = new Regex(@"^\s*Sub-route\s+(?<index>\d+)\s*=\s*(?<value>.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = nameRegex.Matches(input);
            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups["index"].Value, out int index) && index >= 1 && index <= 48)
                {
                    subRouteNames[index] = match.Groups["value"].Value.Trim();
                }
            }

            var percRegex = new Regex(@"^\s*Percentage of sub-route\s+(?<index>\d+)\s*=\s*(?<value>\d+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var percMatches = percRegex.Matches(input);
            foreach (Match match in percMatches)
            {
                if (int.TryParse(match.Groups["index"].Value, out int index) && index >= 1 && index <= 48)
                {
                    if (int.TryParse(match.Groups["value"].Value, out int percentage))
                    {
                        subRoutePercentages[index] = percentage;
                    }
                }
            }

            // Combine into SubRoutes list, filtering out INVALID
            for (int i = 1; i <= 48; i++)
            {
                string? name = subRouteNames[i];
                if (!string.IsNullOrEmpty(name) && !name.Equals("INVALID", StringComparison.OrdinalIgnoreCase))
                {
                    result.SubRoutes.Add(new SubRoute
                    {
                        Name = name,
                        Percentage = subRoutePercentages[i],
                        Index = i
                    });
                }
            }

            return result;
        }

        private static string? ExtractValue(string input, string key)
        {
            // Anchor to start of line for keys to be more robust
            var regex = new Regex($@"^\s*{Regex.Escape(key)}\s*=\s*(?<value>.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var match = regex.Match(input);
            if (match.Success)
            {
                return match.Groups["value"].Value.Trim();
            }
            return null;
        }
    }
}
