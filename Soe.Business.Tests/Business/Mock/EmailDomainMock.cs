using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class EmailDomainMock
    {
        public static List<string> Mock()
        {
            return new List<string>()
            {
                "se",
                "com",
                "org",
            };
        }
    }
}
