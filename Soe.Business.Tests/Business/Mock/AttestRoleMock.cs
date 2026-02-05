using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class AttestRoleMock
    {
        private static int id = 1;

        public static List<AttestRoleDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create("' + AttestRole.Name + '","' + AttestRole.Description + '",(SoeModule)' + CAST(AttestRole.Module as nvarchar) + ',"' + (ISNULL((select top 1 Name from CompanyExternalCode where ActorCompanyId = 701609 and Entity = 200 and RecordId = AttestRole.AttestRoleId), '')) + '"));' from AttestRole where AttestRole.ActorCompanyId = 701609 and AttestRole.State = 0 order by AttestRole.Name
             */
            #endregion

            id = 1;
            var l = new List<AttestRoleDTO>();

            l.Add(Create("Admin", "Samtliga anställda 1000", (SoeModule)5, "Admin"));
            l.Add(Create("Distrubition Gbg anv ej", "Ta bort 1001", (SoeModule)5, "Distrubition Gbg anv ej"));
            l.Add(Create("Förattest", "Reg-Klar 1002", (SoeModule)5, ""));
            l.Add(Create("Förattest Plock Bma (Anv ej)", "Reg-Klar 1121", (SoeModule)5, "Förattest Plock Bma (Anv ej)"));
            l.Add(Create("Förattest Stf. Drift Bma (Anv ej)", "Reg-Klar 1126", (SoeModule)5, "Förattest Stf. Drift Bma (Anv ej)"));
            l.Add(Create("Förattest Stf. Plock LF/Orderstart Bma (Anv ej)", "Reg-Klar 1129", (SoeModule)5, "Förattest Stf. Plock LF/Orderstart Bma (Anv ej)"));
            l.Add(Create("Konsult (Marie) (Anv ej)", "Ta bort 1149", (SoeModule)5, "Konsult (Marie) (Anv ej)"));
            l.Add(Create("Lön", "1150", (SoeModule)5, "Lön"));
            l.Add(Create("Schema", "Ej attestbehörig", (SoeModule)5, ""));
            l.Add(Create("Slutattest", "Reg-Attest 1155", (SoeModule)5, ""));
            l.Add(Create("Slutattest GruppL Matkassar Sthlm (Anv ej)", "Klar-Attest 1199", (SoeModule)5, "Slutattest GruppL Matkassar Sthlm (Anv ej)"));
            l.Add(Create("Slutattest GruppL Plock Orderstart/LF Sthlm (Anv ej)", "Klar-Attest 1203", (SoeModule)5, "Slutattest GruppL Plock Orderstart/LF Sthlm (Anv ej)"));
            l.Add(Create("Slutattest IT-Support  (Anv ej)", "Klar-Attest 1231", (SoeModule)5, ""));
            l.Add(Create("Slutattest KH Transport OH  (Anv ej)", "Klar-Attest 1269", (SoeModule)5, ""));
            l.Add(Create("Slutattest Matkassar STHLM (Anv ej)", "Klar-Attest 1313", (SoeModule)5, "Slutattest Matkassar STHLM (Anv ej)"));
            l.Add(Create("Slutattest Plock Sthlm (Anv ej)", "Klar-Attest 1336", (SoeModule)5, "Slutattest Plock Sthlm (Anv ej)"));
            l.Add(Create("Slutattest Sortiment & Inköp Färskvaror (Anv ej)", "Klar-Attest 1355", (SoeModule)5, ""));
            l.Add(Create("Slutattest VM Kyl/FoG Sthlm (Anv ej)", "Klar-Attest 1415", (SoeModule)5, "Slutattest VM Kyl/FoG Sthlm (Anv ej)"));
            l.Add(Create("Sympa", "", (SoeModule)5, ""));

            return l;
        }

        private static AttestRoleDTO Create(string name, string description, SoeModule module, string externalCode)
        {
            return new AttestRoleDTO()
            {
                AttestRoleId = id++,
                Name = name,
                Description = description,
                Module = module,
                ExternalCodes = !string.IsNullOrEmpty(externalCode) ? externalCode.ObjToList() : null,
                ExternalCodesString = externalCode.EmptyToNull(),
            };
        }
    }
}
