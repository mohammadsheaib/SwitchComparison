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
            string input = @"
RETCODE = 0  Operation succeeded

Route selection analysis
------------------------
 Route selection name           Sub-route selection mode  Route 1 name

 SCC_RSN_UNITED_KINGDOM_ORANGE  Select by percentage      SCC_RT_UNITED_KINGDOM_ORANGE
 SCC_RSN_SOUTH_KOREA_FIXED      Select by percentage      SCC_RT_SOUTH_KOREA_FIXED
 SCC_RSN_TEST_SEQ               Select by sequence        SCC_RT_TEST_SEQ

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
Route selection analysis
------------------------
 Route selection name           Sub-route selection mode  Route 1 name

 RT1                            Select by percentage      RN1

To be continued...

Route selection analysis
------------------------
 Route selection name           Sub-route selection mode  Route 1 name

 RT2                            Select by sequence        RN2

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
        public void Parse_UnknownRTSM_ReturnsAsIs()
        {
            string input = @"
 Route selection name           Sub-route selection mode  Route 1 name
 RT_UNKNOWN                     Something Else            RN_UNKNOWN
";
            var result = RTANAParser.Parse(input);
            Assert.Single(result);
            Assert.Equal("Something Else", result[0].RTSM);
        }
    }
}
