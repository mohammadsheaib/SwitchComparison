using Xunit;
using SwitchRTParser.Lib;
using System.Linq;

namespace SwitchRTParser.Tests
{
    public class ParserTests
    {
        [Fact]
        public void Parse_ProvidedExample_ReturnsCorrectData()
        {
            string input = @"LST RT:RN=""QOA_RT_TATA "",SSR=YES,QR=LOCAL;
SCCGTMSS01
+++    MSOFTX/*MEID:5 MENAME:SCCGTMSS01*/        2026-02-11 20:27:39+03:00
O&M    #63447
%%/*1879106888 MEID=005*/LST RT:RN=""QOA_RT_TATA "",SSR=YES,QR=LOCAL;%%
RETCODE = 0  Operation succeeded

Basic parameter
---------------
                              Route name  =  QOA_RT_TATA
                Sub-route selection mode  =  Select by percentage
Sub-route selection based on NER and ASR  =  NULL
                             Server name  =  SCC
                            Route Number  =  206
                          Priority codec  =  Not differentiate codec
               Called number change name  =  DEFAULT
                    Managed object group  =  PUBLIC
                         Codec list name  =  INVALID
                              TGRP value  =  INVALID
(Number of results = 1)

Sub-route
---------
  Route name  =  QOA_RT_TATA
 Sub-route 1  =  SCC_SRT_TATA2_SIP
 Sub-route 2  =  INVALID
 Sub-route 3  =  SCC_SRT_BICS_BRU_SIP
 Sub-route 4  =  INVALID
 Sub-route 5  =  SCC_SRT_BICS_MAR_SIP
 Sub-route 6  =  SCC_SRT_VODAFONE_IVC
 Sub-route 7  =  INVALID
 Sub-route 8  =  INVALID
 Sub-route 9  =  INVALID
Sub-route 10  =  INVALID
Sub-route 11  =  INVALID
Sub-route 12  =  INVALID
Sub-route 13  =  INVALID
Sub-route 14  =  INVALID
Sub-route 15  =  INVALID
Sub-route 16  =  INVALID
Sub-route 17  =  INVALID
Sub-route 18  =  INVALID
Sub-route 19  =  INVALID
Sub-route 20  =  INVALID
Sub-route 21  =  INVALID
Sub-route 22  =  INVALID
Sub-route 23  =  INVALID
Sub-route 24  =  INVALID
Sub-route 25  =  INVALID
Sub-route 26  =  INVALID
Sub-route 27  =  INVALID
Sub-route 28  =  INVALID
Sub-route 29  =  INVALID
Sub-route 30  =  INVALID
Sub-route 31  =  INVALID
Sub-route 32  =  INVALID
Sub-route 33  =  INVALID
Sub-route 34  =  INVALID
Sub-route 35  =  INVALID
Sub-route 36  =  INVALID
Sub-route 37  =  INVALID
Sub-route 38  =  INVALID
Sub-route 39  =  INVALID
Sub-route 40  =  INVALID
Sub-route 41  =  INVALID
Sub-route 42  =  INVALID
Sub-route 43  =  INVALID
Sub-route 44  =  INVALID
Sub-route 45  =  INVALID
Sub-route 46  =  INVALID
Sub-route 47  =  INVALID
Sub-route 48  =  INVALID
(Number of results = 1)

Select sub-route 1 by percentage
--------------------------------
               Route name  =  QOA_RT_TATA
Percentage of sub-route 1  =  100
(Number of results = 1)

Select other sub-route by percentage
------------------------------------
                Route name  =  QOA_RT_TATA
 Percentage of sub-route 2  =  0
 Percentage of sub-route 3  =  100
 Percentage of sub-route 4  =  0
 Percentage of sub-route 5  =  100
 Percentage of sub-route 6  =  100
 Percentage of sub-route 7  =  0
 Percentage of sub-route 8  =  0
 Percentage of sub-route 9  =  0
Percentage of sub-route 10  =  0
Percentage of sub-route 11  =  0
Percentage of sub-route 12  =  0
Percentage of sub-route 13  =  0
Percentage of sub-route 14  =  0
Percentage of sub-route 15  =  0
Percentage of sub-route 16  =  0
Percentage of sub-route 17  =  0
Percentage of sub-route 18  =  0
Percentage of sub-route 19  =  0
Percentage of sub-route 20  =  0
Percentage of sub-route 21  =  0
Percentage of sub-route 22  =  0
Percentage of sub-route 23  =  0
Percentage of sub-route 24  =  0
Percentage of sub-route 25  =  0
Percentage of sub-route 26  =  0
Percentage of sub-route 27  =  0
Percentage of sub-route 28  =  0
Percentage of sub-route 29  =  0
Percentage of sub-route 30  =  0
Percentage of sub-route 31  =  0
Percentage of sub-route 32  =  0
Percentage of sub-route 33  =  0
Percentage of sub-route 34  =  0
Percentage of sub-route 35  =  0
Percentage of sub-route 36  =  0
Percentage of sub-route 37  =  0
Percentage of sub-route 38  =  0
Percentage of sub-route 39  =  0
Percentage of sub-route 40  =  0
Percentage of sub-route 41  =  0
Percentage of sub-route 42  =  0
Percentage of sub-route 43  =  0
Percentage of sub-route 44  =  0
Percentage of sub-route 45  =  0
Percentage of sub-route 46  =  0
Percentage of sub-route 47  =  0
Percentage of sub-route 48  =  0
(Number of results = 1)

---    END
";

            var result = Parser.Parse(input);

            Assert.NotNull(result);
            Assert.Equal("QOA_RT_TATA", result.RN);
            Assert.Equal("PERC", result.SRSM);
            Assert.Equal("SCC", result.SN);

            // Valid sub-routes in the example: 1, 3, 5, 6
            Assert.Equal(4, result.SubRoutes.Count);

            Assert.Equal("SCC_SRT_TATA2_SIP", result.SubRoutes[0].Name);
            Assert.Equal(100, result.SubRoutes[0].Percentage);

            Assert.Equal("SCC_SRT_BICS_BRU_SIP", result.SubRoutes[1].Name);
            Assert.Equal(100, result.SubRoutes[1].Percentage);

            Assert.Equal("SCC_SRT_BICS_MAR_SIP", result.SubRoutes[2].Name);
            Assert.Equal(100, result.SubRoutes[2].Percentage);

            Assert.Equal("SCC_SRT_VODAFONE_IVC", result.SubRoutes[3].Name);
            Assert.Equal(100, result.SubRoutes[3].Percentage);
        }

        [Fact]
        public void Parse_SelectionModeSequence_ReturnsSEQ()
        {
            string input = @"
                              Route name  =  MY_ROUTE
                Sub-route selection mode  =  Select by sequence
                             Server name  =  MY_SERVER
";
            var result = Parser.Parse(input);
            Assert.NotNull(result);
            Assert.Equal("SEQ", result.SRSM);
        }

        [Fact]
        public void Parse_EmptyInput_ReturnsNull()
        {
            var result = Parser.Parse("");
            Assert.Null(result);
        }
    }
}
