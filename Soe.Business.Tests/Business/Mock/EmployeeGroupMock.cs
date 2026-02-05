using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class EmployeeGroupMock
    {
        private static int id = 1;

        public static List<EmployeeGroupDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create("' + EmployeeGroup.Name + '",' + CASE EmployeeGroup.AutogenTimeblocks WHEN 1 THEN 'true' ELSE 'false' END + ',"' + (ISNULL((select top 1 Name from CompanyExternalCode where ActorCompanyId = 701609 and Entity = 100 and RecordId = EmployeeGroup.EmployeeGroupId), '')) + + '"));' from EmployeeGroup where EmployeeGroup.ActorCompanyId = 701609 and EmployeeGroup.State = 0 order by EmployeeGroup.Name
             */
            #endregion

            id = 1;

            id = 1;
            var l = new List<EmployeeGroupDTO>();

            l.Add(Create("* Handels Månadsavlönad arbetare (Anv ej)", false, ""));
            l.Add(Create("* Handels Timavlönad arbetare (Anv ej)", false, ""));
            l.Add(Create("* Handels Timavlönad deltid (Anv ej)", false, ""));
            l.Add(Create("*Unionen Tjänsteman Timlön - avvikelse (Anv ej)", true, ""));
            l.Add(Create("E-Handels Månadsavlönad arbetare", false, "E-Handels Månadsavlönad arbetare"));
            l.Add(Create("E-Handels Månadsavlönad arbetare Skift", false, "E-Handels Månadsavlönad arbetare Skift"));
            l.Add(Create("E-Handels Månadsavlönad Chaufför", false, "E-Handels Månadsavlönad Chaufför"));
            l.Add(Create("E-Handels Månadsavlönad Chaufför Skift", false, "E-Handels Månadsavlönad Chaufför Skift"));
            l.Add(Create("E-Handels Timavlönad arbetare", false, "E-Handels Timavlönad arbetare"));
            l.Add(Create("E-Handels Timavlönad arbetare Skift", false, "E-Handels Timavlönad arbetare Skift"));
            l.Add(Create("E-Handels Timavlönad Chaufför", false, "E-Handels Timavlönad Chaufför"));
            l.Add(Create("E-Handels Timavlönad Chaufför Skift", false, "E-Handels Timavlönad Chaufför Skift"));
            l.Add(Create("E-Handels Timavlönad deltid", false, "E-Handels Timavlönad deltid"));
            l.Add(Create("E-Handels Timavlönad deltid Chaufför", false, "E-Handels Timavlönad deltid Chaufför"));
            l.Add(Create("E-Handels Timavlönad deltid Chaufför Skift", false, "E-Handels Timavlönad deltid Chaufför Skift"));
            l.Add(Create("E-Handels Timavlönad deltid Skift", false, "E-Handels Timavlönad deltid Skift"));
            l.Add(Create("Inhyrda", false, ""));
            l.Add(Create("Unionen TJ + Förskjuten avvikelse 40h", true, "Unionen TJ + Förskjuten avvikelse 40h"));
            l.Add(Create("Unionen TJ + Förskjuten stämpla 40h", false, "Unionen TJ + Förskjuten stämpla 40h"));
            l.Add(Create("Unionen Tjänsteman", true, "Unionen Tjänsteman"));
            l.Add(Create("Unionen Tjänsteman 38,42", true, "Unionen Tjänsteman 38,42"));
            l.Add(Create("Unionen Tjänsteman 38:30 + Förskjuten avvikelserapp", true, "Unionen Tjänsteman 38:30 + Förskjuten avvikelserapp"));
            l.Add(Create("Unionen Tjänsteman 38:30 + Förskjuten stämpla", false, "Unionen Tjänsteman 38:30 + Förskjuten stämpla"));
            l.Add(Create("Unionen Tjänsteman Timlön", false, "Unionen Tjänsteman Timlön"));

            return l;
        }

        private static EmployeeGroupDTO Create(string name, bool autogenTimeblocks, string externalCode)
        {
            return new EmployeeGroupDTO()
            {
                EmployeeGroupId = id++,
                Name = name,
                AutogenTimeblocks = autogenTimeblocks,
                ExternalCodes = !string.IsNullOrEmpty(externalCode) ? externalCode.ObjToList() : null,
                ExternalCodesString = externalCode.EmptyToNull()
            };
        }
    }
}
