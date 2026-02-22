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
 DN set  Call prefix  F3  F4  F5  Minimum number length  Maximum number length
 9       000255       X   X   X   10                     32
 10      000256       X   X   X   11                     33

Route selection data
--------------------
 DN set  Call prefix  F3  F4  F5  F6  F7  F8  Description         F10  F11  F12  F13  F14  F15  Caller number and route selection name analysis flag
 9       000255       X   X   X   X   X   X   DEST_SWEDEN_LYCA    X    X    X    X    X    X    TRUE
 10      000256       X   X   X   X   X   X   DEST_UK_O2          X    X    X    X    X    X    FALSE
";
            var result = CNACLDParser.Parse(input);

            Assert.Equal(2, result.Count);

            var first = result.FirstOrDefault(r => r.P == "9" && r.PFX == "000255");
            Assert.NotNull(first);
            Assert.Equal("10", first.MINL);
            Assert.Equal("32", first.MAXL);
            Assert.Equal("DEST_SWEDEN_LYCA", first.SDESCRIPTION);
            Assert.Equal("TRUE", first.CLIANA);

            var second = result.FirstOrDefault(r => r.P == "10" && r.PFX == "000256");
            Assert.NotNull(second);
            Assert.Equal("11", second.MINL);
            Assert.Equal("33", second.MAXL);
            Assert.Equal("DEST_UK_O2", second.SDESCRIPTION);
            Assert.Equal("FALSE", second.CLIANA);
        }

        [Fact]
        public void Parse_CorrelatesUsingBothFields()
        {
             string input = @"
Call prefix data
----------------
 DN set  Call prefix  F3  F4  F5  Minimum number length  Maximum number length
 9       000255       X   X   X   10                     32
 9       000256       X   X   X   11                     33

Route selection data
--------------------
 DN set  Call prefix  F3  F4  F5  F6  F7  F8  Description         F10  F11  F12  F13  F14  F15  Caller number and route selection name analysis flag
 9       000255       X   X   X   X   X   X   DESC_A              X    X    X    X    X    X    TRUE
 9       000256       X   X   X   X   X   X   DESC_B              X    X    X    X    X    X    FALSE
";
            var result = CNACLDParser.Parse(input);

            Assert.Equal(2, result.Count);

            var first = result.FirstOrDefault(r => r.P == "9" && r.PFX == "000255");
            Assert.Equal("DESC_A", first?.SDESCRIPTION);
            Assert.Equal("TRUE", first?.CLIANA);

            var second = result.FirstOrDefault(r => r.P == "9" && r.PFX == "000256");
            Assert.Equal("DESC_B", second?.SDESCRIPTION);
            Assert.Equal("FALSE", second?.CLIANA);
        }

        [Fact]
        public void Parse_MultipleBatches_AggregatesCorrectly()
        {
            string input = @"
Call prefix data
----------------
 DN set  Call prefix  F3  F4  F5  Minimum number length  Maximum number length
 1       001          X   X   X   10                     20

To be continued...

Call prefix data
----------------
 DN set  Call prefix  F3  F4  F5  Minimum number length  Maximum number length
 2       002          X   X   X   10                     20

Route selection data
--------------------
 DN set  Call prefix  F3  F4  F5  F6  F7  F8  Description         F10  F11  F12  F13  F14  F15  Caller number and route selection name analysis flag
 1       001          X   X   X   X   X   X   DESC1               X    X    X    X    X    X    TRUE

To be continued...

Route selection data
--------------------
 DN set  Call prefix  F3  F4  F5  F6  F7  F8  Description         F10  F11  F12  F13  F14  F15  Caller number and route selection name analysis flag
 2       002          X   X   X   X   X   X   DESC2               X    X    X    X    X    X    FALSE
";
            var result = CNACLDParser.Parse(input);

            Assert.Equal(2, result.Count);

            var first = result.FirstOrDefault(r => r.P == "1" && r.PFX == "001");
            Assert.NotNull(first);
            Assert.Equal("DESC1", first.SDESCRIPTION);

            var second = result.FirstOrDefault(r => r.P == "2" && r.PFX == "002");
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
