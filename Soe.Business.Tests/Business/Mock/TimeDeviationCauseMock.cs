using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class TimeDeviationCauseMock
    {
        private static int id = 1;

        public static List<TimeDeviationCauseDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create(' + CAST(TimeDeviationCause.Type as nvarchar) + ',"' + ISNULL(TimeDeviationCause.Name, '') + '","' + ISNULL(TimeDeviationCause.ExtCode, '') + '","' + ISNULL(TimeDeviationCause.Description, '') + '"));' from TimeDeviationCause where TimeDeviationCause.ActorCompanyId = 701609 and TimeDeviationCause.State = 0 order by TimeDeviationCause.Name
             */
            #endregion

            id = 1;
            var l = new List<TimeDeviationCauseDTO>();

            l.Add(Create(1, "10-dagar vid barns födelse", "", ""));
            l.Add(Create(3, "11155770112", "24", ""));
            l.Add(Create(1, "Facklig tid betald", "", ""));
            l.Add(Create(2, "Facklig tid betald (Närvaro)", "", ""));
            l.Add(Create(1, "Facklig tid ej betald", "", ""));
            l.Add(Create(1, "Föräldraledighet", "", ""));
            l.Add(Create(1, "Graviditetspenning", "", "Graviditetspenning"));
            l.Add(Create(1, "Helglön", "", ""));
            l.Add(Create(2, "Mertid", "", ""));
            l.Add(Create(1, "Närståendevård", "", ""));
            l.Add(Create(1, "Ogiltig frånvaro", "", ""));
            l.Add(Create(1, "Ogiltig Frånvaro Hel dag", "", "Ogiltig Frånvaro Hel dag"));
            l.Add(Create(1, "Permission", "", ""));
            l.Add(Create(3, "Schema Tid", "", ""));
            l.Add(Create(1, "Semester", "", ""));
            l.Add(Create(1, "Sen ankomst", "", ""));
            l.Add(Create(1, "Sjuk", "", ""));
            l.Add(Create(1, "Smittobärare", "", ""));
            l.Add(Create(1, "Tidig hemgång", "", ""));
            l.Add(Create(1, "Tjänstledig", "", ""));
            l.Add(Create(1, "Uttag Tidbank", "", ""));
            l.Add(Create(1, "Vård av barn", "", ""));
            l.Add(Create(2, "Övertid betald", "", ""));
            l.Add(Create(2, "Övertid Komp", "", ""));

            return l;
        }
        private static TimeDeviationCauseDTO Create(int type, string name, string externalCode, string description)
        {
            return new TimeDeviationCauseDTO()
            {
                TimeDeviationCauseId = id++,
                Type = (TermGroup_TimeDeviationCauseType)type,
                Name = name,
                ExtCode = externalCode.EmptyToNull(),
                Description = description
            };
        }
    }
}
