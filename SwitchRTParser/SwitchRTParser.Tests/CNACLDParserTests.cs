using Xunit;
using SwitchRTParser.Lib;
using System.Linq;
using System.Collections.Generic;

namespace SwitchRTParser.Tests
{
    public class CNACLDParserTests
    {
        [Fact]
        public void Parse_ValidInput_ReturnsExpectedData()
        {
            string input = @"
Call prefix data
----------------
 DN set  Call prefix  Minimum number length  Maximum number length
 9       000255       10                     32
 10      000256       11                     33

Route selection data
--------------------
 DN set  Description         Server name  Caller number and route selection name analysis flag
 9       DEST_SWEDEN_LYCA    SCC          TRUE
 10      DEST_UK_O2          SCC          FALSE
";
            var result = CNACLDParser.Parse(input);

            Assert.Equal(2, result.Count);

            var first = result.FirstOrDefault(r => r.P == "9");
            Assert.NotNull(first);
            Assert.Equal("000255", first.PFX);
            Assert.Equal("10", first.MINL);
            Assert.Equal("32", first.MAXL);
            Assert.Equal("DEST_SWEDEN_LYCA", first.SDESCRIPTION);
            Assert.Equal("SCC", first.SN);
            Assert.Equal("TRUE", first.CLIANA);

            var second = result.FirstOrDefault(r => r.P == "10");
            Assert.NotNull(second);
            Assert.Equal("000256", second.PFX);
            Assert.Equal("11", second.MINL);
            Assert.Equal("33", second.MAXL);
            Assert.Equal("DEST_UK_O2", second.SDESCRIPTION);
            Assert.Equal("SCC", second.SN);
            Assert.Equal("FALSE", second.CLIANA);
        }

        [Fact]
        public void Parse_MissingSN_ReturnsExpectedData()
        {
            string input = @"
Call prefix data
----------------
 DN set  Call prefix  Minimum number length  Maximum number length
 9       000255       10                     32

Route selection data
--------------------
 DN set  Description         Caller number and route selection name analysis flag
 9       DEST_SWEDEN_LYCA    TRUE
";
            var result = CNACLDParser.Parse(input);

            Assert.Single(result);
            var first = result[0];
            Assert.Equal("9", first.P);
            Assert.Equal("DEST_SWEDEN_LYCA", first.SDESCRIPTION);
            Assert.Equal("", first.SN);
            Assert.Equal("TRUE", first.CLIANA);
        }

        [Fact]
        public void Parse_MultipleBatches_AggregatesCorrectly()
        {
            string input = @"
Call prefix data
----------------
 DN set  Call prefix  Minimum number length  Maximum number length
 1       001          10                     20

To be continued...

Call prefix data
----------------
 DN set  Call prefix  Minimum number length  Maximum number length
 2       002          10                     20

Route selection data
--------------------
 DN set  Description         Server name  Caller number and route selection name analysis flag
 1       DESC1               SCC          TRUE

To be continued...

Route selection data
--------------------
 DN set  Description         Server name  Caller number and route selection name analysis flag
 2       DESC2               SCC          FALSE
";
            var result = CNACLDParser.Parse(input);

            Assert.Equal(2, result.Count);

            var first = result.FirstOrDefault(r => r.P == "1");
            Assert.NotNull(first);
            Assert.Equal("DESC1", first.SDESCRIPTION);

            var second = result.FirstOrDefault(r => r.P == "2");
            Assert.NotNull(second);
            Assert.Equal("DESC2", second.SDESCRIPTION);
        }

        [Fact]
        public void Parse_EmptyInput_ReturnsEmptyList()
        {
            var result = CNACLDParser.Parse("");
            Assert.Empty(result);
        }
    }
}
