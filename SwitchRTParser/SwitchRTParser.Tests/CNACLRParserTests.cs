using Xunit;
using SwitchRTParser.Lib;
using System.Linq;

namespace SwitchRTParser.Tests
{
    public class CNACLRParserTests
    {
        [Fact]
        public void Parse_EmptyInput_ReturnsEmptyList()
        {
            var result = CNACLRParser.Parse("");
            Assert.Empty(result);
        }

        [Fact]
        public void Parse_SingleBatch_ReturnsExpectedResults()
        {
            string input = @"
RETCODE = 0  Operation succeeded

CNACLR
------
 Call source name    Call prefix    DN set    Caller number    Route selection name

 QOA_BICS_ONA        EEEEEEEE       9         00013682485014   QOA_RSN_SWEEDEN_LYCA_1
 TEST_CSC            123            0         123456           TEST_RSN

(Number of results = 2)
";
            var result = CNACLRParser.Parse(input);
            Assert.Equal(2, result.Count);

            Assert.Equal("QOA_BICS_ONA", result[0].CSCNAME);
            Assert.Equal("EEEEEEEE", result[0].CLIN);
            Assert.Equal("9", result[0].P);
            Assert.Equal("00013682485014", result[0].PFX);
            Assert.Equal("QOA_RSN_SWEEDEN_LYCA_1", result[0].RSNAME);

            Assert.Equal("TEST_CSC", result[1].CSCNAME);
            Assert.Equal("123", result[1].CLIN);
            Assert.Equal("0", result[1].P);
            Assert.Equal("123456", result[1].PFX);
            Assert.Equal("TEST_RSN", result[1].RSNAME);
        }

        [Fact]
        public void Parse_MultipleBatches_AggregatesCorrectly()
        {
            string input = @"
CNACLR
------
 Call source name    Call prefix    DN set    Caller number    Route selection name

 CSC1                CLIN1          P1        PFX1             RSN1

To be continued...

CNACLR
------
 Call source name    Call prefix    DN set    Caller number    Route selection name

 CSC2                CLIN2          P2        PFX2             RSN2

(Number of results = 2)
";
            var result = CNACLRParser.Parse(input);
            Assert.Equal(2, result.Count);

            Assert.Equal("CSC1", result[0].CSCNAME);
            Assert.Equal("CLIN1", result[0].CLIN);
            Assert.Equal("CSC2", result[1].CSCNAME);
            Assert.Equal("CLIN2", result[1].CLIN);
        }

        [Fact]
        public void Parse_NonConsecutiveColumns_ReturnsExpectedResults()
        {
            // CSCName: 1st (index 0)
            // P: 3rd (index 2)
            // PFX: 5th (index 4)
            // CLIN: 9th (index 8)
            // RSName: 14th (index 13)

            string input = @"
CNACLR
------
 Call source name  Col2  DN set  Col4  Caller number  Col6  Col7  Col8  Call prefix  Col10  Col11  Col12  Col13  Route selection name

 CSC_VAL           C2    P_VAL   C4    PFX_VAL        C6    C7    C8    CLIN_VAL     C10    C11    C12    C13    RSN_VAL

(Number of results = 1)
";
            var result = CNACLRParser.Parse(input);
            Assert.Single(result);

            Assert.Equal("CSC_VAL", result[0].CSCNAME);
            Assert.Equal("P_VAL", result[0].P);
            Assert.Equal("PFX_VAL", result[0].PFX);
            Assert.Equal("CLIN_VAL", result[0].CLIN);
            Assert.Equal("RSN_VAL", result[0].RSNAME);
        }
    }
}
