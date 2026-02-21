using System.Collections.Generic;

namespace SwitchRTParser.Lib
{
    public class SwitchRT
    {
        public string RN { get; set; } = string.Empty;
        public string SRSM { get; set; } = string.Empty;
        public List<SubRoute> SubRoutes { get; set; } = new List<SubRoute>();
        public string SN { get; set; } = string.Empty;
    }

    public class SubRoute
    {
        public string Name { get; set; } = string.Empty;
        public int Percentage { get; set; }
        public int Index { get; set; }
    }

    public class SwitchRTANA
    {
        public string RSN { get; set; } = string.Empty;
        public string RN { get; set; } = string.Empty;
        public string RTSM { get; set; } = string.Empty;
    }

    public class SwitchCNACLR
    {
        public string CSCNAME { get; set; } = string.Empty;
        public string CLIN { get; set; } = string.Empty;
        public string P { get; set; } = string.Empty;
        public string PFX { get; set; } = string.Empty;
        public string RSNAME { get; set; } = string.Empty;
    }
}
