using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SwitchRTParser.Lib
{
    public class RTsParser
    {
        public static List<SwitchRT> Parse(string input)
        {
            var rts = new Dictionary<string, SwitchRT>(StringComparer.OrdinalIgnoreCase);
            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.Equals("Basic parameter", StringComparison.OrdinalIgnoreCase))
                {
                    i = ParseBasicParameter(lines, i + 1, rts);
                }
                else if (line.Equals("Sub-route", StringComparison.OrdinalIgnoreCase))
                {
                    i = ParseSubRouteNames(lines, i + 1, rts);
                }
                else if (line.Equals("Select sub-route 1 by percentage", StringComparison.OrdinalIgnoreCase))
                {
                    i = ParseSubRoute1Percentage(lines, i + 1, rts);
                }
                else if (line.Equals("Select other sub-route by percentage", StringComparison.OrdinalIgnoreCase))
                {
                    i = ParseOtherSubRoutesPercentage(lines, i + 1, rts);
                }
            }

            var result = rts.Values.ToList();
            foreach (var rt in result)
            {
                rt.SubRoutes = rt.SubRoutes.OrderBy(s => s.Index).ToList();
            }

            return result;
        }

        private static int SkipToData(string[] lines, int i)
        {
            while (i < lines.Length)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Contains("---") || trimmed.Contains("Route name"))
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

        private static int ParseBasicParameter(string[] lines, int startIndex, Dictionary<string, SwitchRT> rts)
        {
            int i = SkipToData(lines, startIndex);

            while (i < lines.Length)
            {
                string line = lines[i];
                if (IsEndOfBatch(line)) break;

                var parts = SplitLine(line);
                if (parts.Length >= 4)
                {
                    string rn = parts[0];
                    string srsmRaw = parts[1];
                    string sn = parts[3];

                    if (!rts.TryGetValue(rn, out var rt))
                    {
                        rt = new SwitchRT { RN = rn };
                        rts[rn] = rt;
                    }

                    if (srsmRaw.Equals("Select by percentage", StringComparison.OrdinalIgnoreCase))
                        rt.SRSM = "PERC";
                    else if (srsmRaw.Equals("Select by sequence", StringComparison.OrdinalIgnoreCase))
                        rt.SRSM = "SEQ";
                    else
                        rt.SRSM = srsmRaw;

                    rt.SN = sn;
                }
                i++;
            }
            return i;
        }

        private static int ParseSubRouteNames(string[] lines, int startIndex, Dictionary<string, SwitchRT> rts)
        {
            int i = SkipToData(lines, startIndex);

            while (i < lines.Length)
            {
                string line = lines[i];
                if (IsEndOfBatch(line)) break;

                var parts = SplitLine(line);
                if (parts.Length > 1)
                {
                    string rn = parts[0];
                    if (!rts.TryGetValue(rn, out var rt))
                    {
                        rt = new SwitchRT { RN = rn };
                        rts[rn] = rt;
                    }

                    for (int j = 1; j < parts.Length && j <= 48; j++)
                    {
                        string name = parts[j];
                        var subRoute = rt.SubRoutes.FirstOrDefault(s => s.Index == j);
                        if (subRoute == null)
                        {
                            subRoute = new SubRoute { Index = j };
                            rt.SubRoutes.Add(subRoute);
                        }
                        subRoute.Name = name;
                    }
                }
                i++;
            }
            return i;
        }

        private static int ParseSubRoute1Percentage(string[] lines, int startIndex, Dictionary<string, SwitchRT> rts)
        {
            int i = SkipToData(lines, startIndex);

            while (i < lines.Length)
            {
                string line = lines[i];
                if (IsEndOfBatch(line)) break;

                var parts = SplitLine(line);
                if (parts.Length >= 2)
                {
                    string rn = parts[0];
                    if (int.TryParse(parts[1], out int percentage))
                    {
                        if (rts.TryGetValue(rn, out var rt))
                        {
                            var subRoute = rt.SubRoutes.FirstOrDefault(s => s.Index == 1);
                            if (subRoute == null)
                            {
                                subRoute = new SubRoute { Index = 1 };
                                rt.SubRoutes.Add(subRoute);
                            }
                            subRoute.Percentage = percentage;
                        }
                    }
                }
                i++;
            }
            return i;
        }

        private static int ParseOtherSubRoutesPercentage(string[] lines, int startIndex, Dictionary<string, SwitchRT> rts)
        {
            int i = SkipToData(lines, startIndex);

            while (i < lines.Length)
            {
                string line = lines[i];
                if (IsEndOfBatch(line)) break;

                var parts = SplitLine(line);
                if (parts.Length > 1)
                {
                    string rn = parts[0];
                    if (rts.TryGetValue(rn, out var rt))
                    {
                        for (int j = 1; j < parts.Length && j <= 48; j++)
                        {
                            if (int.TryParse(parts[j], out int percentage))
                            {
                                int subRouteIndex = j + 1;
                                var subRoute = rt.SubRoutes.FirstOrDefault(s => s.Index == subRouteIndex);
                                if (subRoute == null)
                                {
                                    subRoute = new SubRoute { Index = subRouteIndex };
                                    rt.SubRoutes.Add(subRoute);
                                }
                                subRoute.Percentage = percentage;
                            }
                        }
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
            // Also stop if we encounter another known header, just in case
            if (trimmed.Equals("Basic parameter", StringComparison.OrdinalIgnoreCase)) return true;
            if (trimmed.Equals("Sub-route", StringComparison.OrdinalIgnoreCase)) return true;
            if (trimmed.Equals("Select sub-route 1 by percentage", StringComparison.OrdinalIgnoreCase)) return true;
            if (trimmed.Equals("Select other sub-route by percentage", StringComparison.OrdinalIgnoreCase)) return true;

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
