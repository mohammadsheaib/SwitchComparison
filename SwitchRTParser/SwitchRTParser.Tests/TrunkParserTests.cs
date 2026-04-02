using Xunit;
using SwitchRTParser.Lib;

namespace SwitchRTParser.Tests
{
    public class TrunkParserTests
    {
        [Fact]
        public void Parse_UnblockedTrunk_ReturnsCorrectValues()
        {
            string input = @"
SIP information
---------------
                         Trunk group number  =  38
                           Trunk group name  =  LMR_THKISBC_AIRTEL_TEST_SIP
                            Group direction  =  INOUT
                         Signal bearer type  =  UDP
                         Call-out authority  =  Intra-office
                                             =  Local
                                             =  Local toll
                                             =  National toll
                                             =  International toll
                          Call-in authority  =  Intra-office
                                             =  Local
                                             =  Local toll
                                             =  National toll
                                             =  International toll
";
            var result = TrunkParser.Parse(input);

            Assert.NotNull(result);
            Assert.Equal("LMR_THKISBC_AIRTEL_TEST_SIP", result.TrunkName);
            Assert.True(result.IsOutActivated);
            Assert.True(result.IsInActivated);
        }

        [Fact]
        public void Parse_BlockedTrunk_ReturnsCorrectValues()
        {
            string input = @"
SIP information
---------------
                         Trunk group number  =  38
                           Trunk group name  =  LMR_THKISBC_AIRTEL_TEST_SIP_BLOCKED
                            Group direction  =  INOUT
                         Signal bearer type  =  UDP
                         Call-out authority  =  NULL
                          Call-in authority  =  NULL
";
            var result = TrunkParser.Parse(input);

            Assert.NotNull(result);
            Assert.Equal("LMR_THKISBC_AIRTEL_TEST_SIP_BLOCKED", result.TrunkName);
            Assert.False(result.IsOutActivated);
            Assert.False(result.IsInActivated);
        }

        [Fact]
        public void Parse_EmptyInput_ReturnsNull()
        {
            var result = TrunkParser.Parse("");
            Assert.Null(result);
        }
    }
}
