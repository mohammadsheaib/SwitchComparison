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

    public class SwitchCNACLD
    {
        public string P { get; set; } = string.Empty;
        public string PFX { get; set; } = string.Empty;
        public string MINL { get; set; } = string.Empty;
        public string MAXL { get; set; } = string.Empty;
        public string SDESCRIPTION { get; set; } = string.Empty;
        public string SN { get; set; } = string.Empty;
        public string CLIANA { get; set; } = string.Empty;
    }
}
