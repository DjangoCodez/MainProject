using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class VacationGroupMock
    {
        private static int id = 1;

        public static List<VacationGroupDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create("' + VacationGroup.Name + '","' + (ISNULL((select top 1 Name from CompanyExternalCode where ActorCompanyId = 701609 and Entity = 102 and RecordId = VacationGroup.VacationGroupId), '')) + + '"));' from VacationGroup where VacationGroup.ActorCompanyId = 701609 and VacationGroup.State = 0 order by VacationGroup.Nameselect top 30 'l.Add(Create("' + VacationGroup.Name + '","' + (ISNULL((select top 1 Name from CompanyExternalCode where ActorCompanyId = 701609 and Entity = 102 and RecordId = VacationGroup.VacationGroupId), '')) + + '"));' from VacationGroup where VacationGroup.ActorCompanyId = 701609 and VacationGroup.State = 0 order by VacationGroup.Name
             */
            #endregion

            id = 1;
            var l = new List<VacationGroupDTO>();

            l.Add(Create("EHAO - Direktutbetald semesterersättning", "EHAO - Direktutbetald semesterersättning"));
            l.Add(Create("EHAO - Månadslön", "EHAO - Månadslön"));
            l.Add(Create("EHAO - Timlön", "EHAO - Timlön"));
            l.Add(Create("HRF", ""));
            l.Add(Create("Ingen beräkning/Praktikanter", ""));
            l.Add(Create("Unionen Direktutbetald semesterersättning", ""));
            l.Add(Create("Unionen Månadslön", "Unionen Månadslön"));
            l.Add(Create("Unionen Timlön", "Unionen Timlön"));

            return l;
        }
        private static VacationGroupDTO Create(string name, string externalCode)
        {
            return new VacationGroupDTO()
            {
                VacationGroupId = id++,
                Name = name,
                ExternalCodes = !string.IsNullOrEmpty(externalCode) ? externalCode.ObjToList() : null,
                ExternalCodesString = externalCode.EmptyToNull()
            };
        }
    }
}
