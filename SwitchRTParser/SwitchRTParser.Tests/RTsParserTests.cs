using Xunit;
using SwitchRTParser.Lib;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;

namespace SwitchRTParser.Tests
{
    public class RTsParserTests
    {
        [Fact]
        public void Parse_AllFile_ReturnsExpectedRTs()
        {
            string filePath = "/app/LST RT - ALL.txt";
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "LST RT - ALL.txt");
            }

            string input = File.ReadAllText(filePath);
            var result = RTsParser.Parse(input);

            Assert.NotEmpty(result);

            // Check BOTH_ICSCF
            var bothIcscf = result.FirstOrDefault(r => r.RN == "BOTH_ICSCF");
            Assert.NotNull(bothIcscf);
            Assert.Equal("PERC", bothIcscf.SRSM);
            Assert.Equal("SCC", bothIcscf.SN);

            // Sub-routes for BOTH_ICSCF
            // File shows 6 valid sub-routes, and then INVALIDs up to 48
            Assert.Equal(48, bothIcscf.SubRoutes.Count);

            Assert.Equal("SCC_SRT_THK_ICSCF_3", bothIcscf.SubRoutes[0].Name);
            Assert.Equal(1, bothIcscf.SubRoutes[0].Index);
            Assert.Equal(17, bothIcscf.SubRoutes[0].Percentage);

            Assert.Equal("SCC_SRT_THK_ICSCF_2", bothIcscf.SubRoutes[1].Name);
            Assert.Equal(2, bothIcscf.SubRoutes[1].Index);
            Assert.Equal(17, bothIcscf.SubRoutes[1].Percentage);

            Assert.Equal("SCC_SRT_THK_ICSCF", bothIcscf.SubRoutes[2].Name);
            Assert.Equal(3, bothIcscf.SubRoutes[2].Index);
            Assert.Equal(16, bothIcscf.SubRoutes[2].Percentage);

            Assert.Equal("SCC_SRT_QOA_ICSCF_3", bothIcscf.SubRoutes[3].Name);
            Assert.Equal(4, bothIcscf.SubRoutes[3].Index);
            Assert.Equal(17, bothIcscf.SubRoutes[3].Percentage);

            Assert.Equal("SCC_SRT_QOA_ICSCF_SIG04", bothIcscf.SubRoutes[4].Name);
            Assert.Equal(5, bothIcscf.SubRoutes[4].Index);
            Assert.Equal(17, bothIcscf.SubRoutes[4].Percentage);

            Assert.Equal("SCC_SRT_QOA_ICSCF", bothIcscf.SubRoutes[5].Name);
            Assert.Equal(6, bothIcscf.SubRoutes[5].Index);
            Assert.Equal(16, bothIcscf.SubRoutes[5].Percentage);

            // Check an INVALID subroute
            var invalidSubRoute = bothIcscf.SubRoutes.FirstOrDefault(s => s.Index == 7);
            Assert.NotNull(invalidSubRoute);
            Assert.Equal("INVALID", invalidSubRoute.Name);
            Assert.Equal(0, invalidSubRoute.Percentage);
        }

        [Fact]
        public void Parse_EmptyInput_ReturnsEmptyList()
        {
            var result = RTsParser.Parse("");
            Assert.Empty(result);
        }

        [Fact]
        public void Parse_MultipleBatches_AggregatesCorrectly()
        {
            string input = @"
Basic parameter
---------------
 Route name                     Sub-route selection mode  Sub-route selection based on NER and ASR  Server name  Route Number  Priority codec           Called number change name  Managed object group  Codec list name  TGRP value

 RT1                            Select by percentage      NULL                                      SCC          1001          Not differentiate codec  DEFAULT                    PUBLIC                INVALID          INVALID

To be continued...

Basic parameter
---------------
 Route name                     Sub-route selection mode  Sub-route selection based on NER and ASR  Server name  Route Number  Priority codec           Called number change name  Managed object group  Codec list name  TGRP value

 RT2                            Select by sequence        NULL                                      SCC          1002          Not differentiate codec  DEFAULT                    PUBLIC                INVALID          INVALID

(Number of results = 1)

Sub-route
---------
 Route name                    Sub-route 1                    Sub-route 2
 RT1                           SR1_1                          SR1_2
 RT2                           SR2_1                          SR2_2

Select sub-route 1 by percentage
--------------------------------
 Route name                      Percentage of sub-route 1
 RT1                             60
 RT2                             100

Select other sub-route by percentage
------------------------------------
 Route name                   Percentage of sub-route 2
 RT1                          40
 RT2                          0
";
            var result = RTsParser.Parse(input);
            Assert.Equal(2, result.Count);

            var rt1 = result.FirstOrDefault(r => r.RN == "RT1");
            Assert.NotNull(rt1);
            Assert.Equal("PERC", rt1.SRSM);
            Assert.Equal(2, rt1.SubRoutes.Count);
            Assert.Equal("SR1_1", rt1.SubRoutes[0].Name);
            Assert.Equal(60, rt1.SubRoutes[0].Percentage);
            Assert.Equal("SR1_2", rt1.SubRoutes[1].Name);
            Assert.Equal(40, rt1.SubRoutes[1].Percentage);

            var rt2 = result.FirstOrDefault(r => r.RN == "RT2");
            Assert.NotNull(rt2);
            Assert.Equal("SEQ", rt2.SRSM);
            Assert.Equal(2, rt2.SubRoutes.Count);
            Assert.Equal("SR2_1", rt2.SubRoutes[0].Name);
            Assert.Equal(100, rt2.SubRoutes[0].Percentage);
            Assert.Equal("SR2_2", rt2.SubRoutes[1].Name);
            Assert.Equal(0, rt2.SubRoutes[1].Percentage);
        }

        [Fact]
        public void Parse_HandlesInvalidSubRoutes()
        {
             string input = @"
Basic parameter
---------------
 Route name                     Sub-route selection mode  Sub-route selection based on NER and ASR  Server name  Route Number
 RT_INV                         Select by percentage      NULL                                      SCC          2000

Sub-route
---------
 Route name                    Sub-route 1                    Sub-route 2
 RT_INV                        SR1                            INVALID

Select sub-route 1 by percentage
--------------------------------
 Route name                      Percentage of sub-route 1
 RT_INV                          100

Select other sub-route by percentage
------------------------------------
 Route name                   Percentage of sub-route 2
 RT_INV                       0
";
             var result = RTsParser.Parse(input);
             var rt = result.FirstOrDefault(r => r.RN == "RT_INV");
             Assert.NotNull(rt);
             Assert.Equal(2, rt.SubRoutes.Count);
             Assert.Equal("SR1", rt.SubRoutes[0].Name);
             Assert.Equal("INVALID", rt.SubRoutes[1].Name);
        }

    }
}
