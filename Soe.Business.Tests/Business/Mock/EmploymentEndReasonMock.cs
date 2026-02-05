using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class EmploymentEndReasonMock
    {
        public static List<EndReasonDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create(' + CAST(EndReason.EndReasonId as nvarchar) + ',"' + ISNULL(EndReason.Name, '') + '","' + ISNULL(EndReason.Code, '') + '"));' from EndReason where EndReason.ActorCompanyId = 701609 and EndReason.State = 0 order by EndReason.Name
             */
            #endregion

            var l = new List<EndReasonDTO>();

            //Sys
            l.Add(Create(0, "Okänd"));
            l.Add(Create(-1, "Byte av anställningsavtal"));
            l.Add(Create(-2, "Byte mellan koncernföretag"));
            l.Add(Create(-3, "Dödsfall"));
            l.Add(Create(-4, "Egen begäran"));
            l.Add(Create(-5, "Pension"));
            l.Add(Create(-6, "Tidsbegränsad anställning upphör"));
            l.Add(Create(-7, "Uppsagd pga arbetsbrist"));
            l.Add(Create(-8, "Uppsagd pga avsked"));
            //Comp
            l.Add(Create(51, "Avbruten provanställning från arbetsgivarens sida", ""));
            l.Add(Create(52, "Provanställning övergår ej till tillsvidareanställning", ""));

            return l;
        }

        private static EndReasonDTO Create(int id, string name)
        {
            return new EndReasonDTO(id, name, null, true);
        }
        private static EndReasonDTO Create(int id, string name, string code)
        {
            return new EndReasonDTO(id, name, code, false);
        }
    }
}
