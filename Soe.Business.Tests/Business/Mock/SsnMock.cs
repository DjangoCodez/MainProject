using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class SsnMock
    {
        public static List<string> Mock(TermGroup_Sex sex)
        {
            if (sex == TermGroup_Sex.Male)
            {
                return new List<string>()
                {
                    "20010101-7911",
                    "20010101-0932",
                };
            }
            else if (sex == TermGroup_Sex.Female)
            {
                return new List<string>()
                {
                    "20010101-4462",
                    "20010101-1260",
                };
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
