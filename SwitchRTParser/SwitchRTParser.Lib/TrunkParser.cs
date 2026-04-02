using System;
using System.Text.RegularExpressions;

namespace SwitchRTParser.Lib
{
    public class TrunkParser
    {
        public static ComparisonTrunk? Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var result = new ComparisonTrunk();

            // Extract Trunk group name
            result.TrunkName = ExtractValue(input, "Trunk group name") ?? string.Empty;

            // Extract Call-out authority
            string? outAuthority = ExtractValue(input, "Call-out authority");
            result.IsOutActivated = !string.IsNullOrEmpty(outAuthority) && !outAuthority.Equals("NULL", StringComparison.OrdinalIgnoreCase);

            // Extract Call-in authority
            string? inAuthority = ExtractValue(input, "Call-in authority");
            result.IsInActivated = !string.IsNullOrEmpty(inAuthority) && !inAuthority.Equals("NULL", StringComparison.OrdinalIgnoreCase);

            return result;
        }

        private static string? ExtractValue(string input, string key)
        {
            // Use Multiline and anchor to start of line
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
