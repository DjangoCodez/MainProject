using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class RoleMock
    {
        private static int id = 1;

        public static List<RoleDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 20 'l.Add(Create("' + [Role].Name + '","' + (ISNULL((select top 1 Name from CompanyExternalCode where ActorCompanyId = 701609 and Entity = 201 and RecordId = [Role].RoleId), '')) + '"));' from [Role] where [Role].ActorId = 701609 and [Role].State = 0 order by [Role].Name
             */
            #endregion

            id = 1;
            var l = new List<RoleDTO>();
                
            l.Add(Create("Anställd - avvikelserapportera", "Anställd - avvikelserapportera"));
            l.Add(Create("Anställd - stämpla", "Anställd - stämpla"));
            l.Add(Create("API", ""));
            l.Add(Create("Arbetsledare", ""));
            l.Add(Create("Attestant vikariat", ""));
            l.Add(Create("Chef tjänstemän", ""));
            l.Add(Create("HR", ""));
            l.Add(Create("HR Partner", ""));
            l.Add(Create("Kommunikation", ""));
            l.Add(Create("Rapporter", ""));
            l.Add(Create("Receptionist", ""));
            l.Add(Create("Schema/Endast Läs Attest", ""));
            l.Add(Create("Schema/Frånvaroanmälan", ""));
            l.Add(Create("Schema/Frånvaroanmälan LÄSA", ""));
            l.Add(Create("Schema/Tid Endast Läsa", ""));
            l.Add(Create("Schema-Attest", ""));
            l.Add(Create("Superuser", ""));
            l.Add(Create("Superuser plus", ""));
            l.Add(Create("Sympa ", ""));
            l.Add(Create("Systemadmin", ""));

            return l;
        }
        private static RoleDTO Create(string name, string externalCode)
        {
            return new RoleDTO()
            {
                RoleId = id++,
                Name = name,
                ActualName = name,
                ExternalCodes = !string.IsNullOrEmpty(externalCode) ? externalCode.ObjToList() : null,
                ExternalCodesString = externalCode.EmptyToNull(),
            };
        }
    }
}
