using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class PositionsMock
    {
        private static int id = 1;

        public static List<PositionDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create("' + ISNULL(Position.Code, '') + '","' + Position.Name + '","' + ISNULL(Position.Description, '') + '"));' from Position where Position.ActorCompanyId = 701609 and Position.State = 0 order by Position.Name
             */
            #endregion

            id = 1;
            var l = new List<PositionDTO>();

            l.Add(Create("432100", "Arbetsledare inom lager och terminal", ""));
            l.Add(Create("112010", "Chefredaktörer och programchefer", ""));
            l.Add(Create("111300", "Chefstjänstemän i intresseorganisationer", ""));
            l.Add(Create("241200", "Controller", ""));
            l.Add(Create("121100", "Ekonomi- och finanschefer, nivå 1", ""));
            l.Add(Create("411100", "Ekonomiassistenter m.fl.", ""));
            l.Add(Create("515200", "Fastighetssamordnare", ""));
            l.Add(Create("832910", "Förare av lätt lastbil", ""));
            l.Add(Create("332200", "Företagssäljare", ""));
            l.Add(Create("332300", "Inköpare och upphandlare", ""));
            l.Add(Create("411500", "Inköps- och orderassistenter", ""));
            l.Add(Create("132100", "Inköps-, logistik- och transportchefer, nivå 1", ""));
            l.Add(Create("251110", "IT-arkitekter", ""));
            l.Add(Create("131100", "IT-chefer, nivå 1", ""));
            l.Add(Create("512005", "Kockar", ""));
            l.Add(Create("422200", "Kundtjänstpersonal", ""));
            l.Add(Create("411210", "Löneadministratör", ""));
            l.Add(Create("411400", "Marknads- och försäljningsassistenter", ""));
            l.Add(Create("122100", "Personal- och HR-chefer, nivå 1", ""));
            l.Add(Create("242300", "Personal- och HR-specialister", ""));
            l.Add(Create("1732", "Platschefer inom Handels", ""));
            l.Add(Create("125120", "Produktchefer, nivå 1", ""));
            l.Add(Create("932000", "Produktionsmedarbetare", ""));
            l.Add(Create("331300", "Redovisningsekonomer", ""));
            l.Add(Create("4323", "Transportledare och transportsamordnare", ""));
            l.Add(Create("112000", "Verkställande direktörer m.fl.", "Högsta chefer i verksamhet med chefshierarki."));
            l.Add(Create("2519", "Övriga IT-specialister", ""));

            return l;
        }
        private static PositionDTO Create(string code, string name, string description)
        {
            return new PositionDTO()
            {
                PositionId = id++,
                Name = name,
                Description = description,
                Code = code,
            };
        }
    }
}
