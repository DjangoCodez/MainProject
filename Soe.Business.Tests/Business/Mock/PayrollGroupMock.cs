using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class PayrollGroupMock
    {
        private static int id = 1;

        public static List<PayrollGroupDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create("' + PayrollGroup.Name + '","' + (ISNULL((select top 1 Name from CompanyExternalCode where ActorCompanyId = 701609 and Entity = 101 and RecordId = PayrollGroup.PayrollGroupId), '')) + + '"));' from PayrollGroup where PayrollGroup.ActorCompanyId = 701609 and PayrollGroup.State = 0 order by PayrollGroup.Name
             */
            #endregion

            id = 1;
            var l = new List<PayrollGroupDTO>();

            l.Add(Create("EHAO - Chaufför II Månadslön - 6 månader i företaget", "EHAO - Chaufför II Månadslön - 6 månader i företaget"));
            l.Add(Create("EHAO - Chaufför II Månadslön - Anställningstidstillägg", "EHAO - Chaufför II Månadslön - Anställningstidstillägg"));
            l.Add(Create("EHAO - Chaufför II Månadslön 18 år", "EHAO - Chaufför II Månadslön 18 år"));
            l.Add(Create("EHAO - Chaufför II Månadslön 19 år", "EHAO - Chaufför II Månadslön 19 år"));
            l.Add(Create("EHAO - Chaufför II Månadslön 20 år", "EHAO - Chaufför II Månadslön 20 år"));
            l.Add(Create("EHAO - Chaufför II timlön + 6 mån i företaget", "EHAO - Chaufför II timlön + 6 mån i företaget"));
            l.Add(Create("EHAO - Chaufför II timlön + Anställningstidstillägg", "EHAO - Chaufför II timlön + Anställningstidstillägg"));
            l.Add(Create("EHAO - Chaufför II timlön 18 år", "EHAO - Chaufför II timlön 18 år"));
            l.Add(Create("EHAO - Chaufför II timlön 19 år", "EHAO - Chaufför II timlön 19 år"));
            l.Add(Create("EHAO - Chaufför II timlön 20 år", "EHAO - Chaufför II timlön 20 år"));
            l.Add(Create("EHAO - månadslön + 6 mån i företaget", "EHAO - månadslön + 6 mån i företaget"));
            l.Add(Create("EHAO - månadslön + Anställningstidstillägg", "EHAO - månadslön + Anställningstidstillägg"));
            l.Add(Create("EHAO - månadslön 17 år", "EHAO - månadslön 17 år"));
            l.Add(Create("EHAO - månadslön 18 år", "EHAO - månadslön 18 år"));
            l.Add(Create("EHAO - månadslön 19 år", "EHAO - månadslön 19 år"));
            l.Add(Create("EHAO - månadslön 20 år", "EHAO - månadslön 20 år"));
            l.Add(Create("EHAO - timlön + 6 månader i företaget", "EHAO - timlön + 6 månader i företaget"));
            l.Add(Create("EHAO - timlön + Anställningtidstillägg", "EHAO - timlön + Anställningtidstillägg"));
            l.Add(Create("EHAO - timlön 17 år", "EHAO - timlön 17 år"));
            l.Add(Create("EHAO - timlön 18 år", "EHAO - timlön 18 år"));
            l.Add(Create("EHAO - timlön 19 år", "EHAO - timlön 19 år"));
            l.Add(Create("EHAO - timlön 20 år", "EHAO - timlön 20 år"));
            l.Add(Create("Inhyrda / praktikanter", ""));
            l.Add(Create("Unionen månadslön", "Unionen månadslön"));
            l.Add(Create("Unionen timlön", "Unionen timlön"));

            return l;
        }

        private static PayrollGroupDTO Create(string name, string externalCode)
        {
            return new PayrollGroupDTO()
            {
                PayrollGroupId = id++,
                Name = name,
                ExternalCodes = !string.IsNullOrEmpty(externalCode) ? externalCode.ObjToList() : null,
                ExternalCodesString = externalCode.EmptyToNull()
            };
        }
    }
}
