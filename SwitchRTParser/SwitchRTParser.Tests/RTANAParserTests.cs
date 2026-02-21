using Xunit;
using SwitchRTParser.Lib;
using System.Linq;

namespace SwitchRTParser.Tests
{
    public class RTANAParserTests
    {
        [Fact]
        public void Parse_EmptyInput_ReturnsEmptyList()
        {
            var result = RTANAParser.Parse("");
            Assert.Empty(result);
        }

        [Fact]
        public void Parse_SingleBatch_ReturnsExpectedResults()
        {
            // Providing a realistic header with many columns
            string input = @"
RETCODE = 0  Operation succeeded

Route selection analysis
------------------------
 Route selection name  Index  Other1  Other2  Other3  Other4  Other5  Other6  Sub-route selection mode  Route 1 name

 SCC_RSN_UNITED_KINGDOM_ORANGE  1  VAL1  VAL2  VAL3  VAL4  VAL5  VAL6  Select by percentage      SCC_RT_UNITED_KINGDOM_ORANGE
 SCC_RSN_SOUTH_KOREA_FIXED      2  VAL1  VAL2  VAL3  VAL4  VAL5  VAL6  Select by percentage      SCC_RT_SOUTH_KOREA_FIXED
 SCC_RSN_TEST_SEQ               3  VAL1  VAL2  VAL3  VAL4  VAL5  VAL6  Select by sequence        SCC_RT_TEST_SEQ

(Number of results = 3)
";
            var result = RTANAParser.Parse(input);
            Assert.Equal(3, result.Count);

            Assert.Equal("SCC_RSN_UNITED_KINGDOM_ORANGE", result[0].RSN);
            Assert.Equal("PERC", result[0].RTSM);
            Assert.Equal("SCC_RT_UNITED_KINGDOM_ORANGE", result[0].RN);

            Assert.Equal("SCC_RSN_SOUTH_KOREA_FIXED", result[1].RSN);
            Assert.Equal("PERC", result[1].RTSM);
            Assert.Equal("SCC_RT_SOUTH_KOREA_FIXED", result[1].RN);

            Assert.Equal("SCC_RSN_TEST_SEQ", result[2].RSN);
            Assert.Equal("SEQ", result[2].RTSM);
            Assert.Equal("SCC_RT_TEST_SEQ", result[2].RN);
        }

        [Fact]
        public void Parse_MultipleBatches_AggregatesCorrectly()
        {
            string input = @"
Route selection name  Index  Other1  Other2  Other3  Other4  Other5  Other6  Sub-route selection mode  Route 1 name

 RT1                   1  VAL1  VAL2  VAL3  VAL4  VAL5  VAL6  Select by percentage      RN1

To be continued...

Route selection name  Index  Other1  Other2  Other3  Other4  Other5  Other6  Sub-route selection mode  Route 1 name

 RT2                   2  VAL1  VAL2  VAL3  VAL4  VAL5  VAL6  Select by sequence        RN2

(Number of results = 2)
";
            var result = RTANAParser.Parse(input);
            Assert.Equal(2, result.Count);

            Assert.Equal("RT1", result[0].RSN);
            Assert.Equal("PERC", result[0].RTSM);
            Assert.Equal("RN1", result[0].RN);

            Assert.Equal("RT2", result[1].RSN);
            Assert.Equal("SEQ", result[1].RTSM);
            Assert.Equal("RN2", result[1].RN);
        }

        [Fact]
        public void Parse_FallbackIndices_WorksWhenHeaderMismatch()
        {
            // Wait, my code looks for "Route selection name" in the line to START parsing.
            // If the header doesn't match, it won't even start parsing unless I change the entry condition.
            // Current entry condition: line.Contains("Route selection name") && line.Contains("Sub-route selection mode")

            string inputCorrectHeaderLine = @"
 Route selection name  H2  H3  H4  H5  H6  H7  H8  Sub-route selection mode  Route 1 name
 RT_FALLBACK           2   3   4   5   6   7   8   Select by percentage      RN_FALLBACK
";
            var result = RTANAParser.Parse(inputCorrectHeaderLine);
            Assert.Single(result);
            Assert.Equal("RT_FALLBACK", result[0].RSN);
            Assert.Equal("PERC", result[0].RTSM);
            Assert.Equal("RN_FALLBACK", result[0].RN);
        }
    }
}
