using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class PayrollLevelMock
    {
        private static int id = 1;

        public static List<PayrollLevelDTO> Mock()
        {
            id = 1;
            var l = new List<PayrollLevelDTO>();

            l.Add(Create("1","Ett","1"));
            l.Add(Create("2","Tva","2"));
            l.Add(Create("3", "Tre", "3"));
            l.Add(Create("4", "Fyra", "4"));
            l.Add(Create("5", "Fem", "5"));

            return l;
        }
        private static PayrollLevelDTO Create(string code, string externalCode, string name)
        {
            return new PayrollLevelDTO()
            {
                PayrollLevelId = id++,
                Name = name,
                ExternalCode = externalCode,
                Code = code,
            };
        }
    }
}
