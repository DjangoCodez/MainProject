using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class PayrollPriceTypeMock
    {
        private static int id = 1;

        public static List<PayrollPriceTypeDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create(' + CAST(PayrollPriceType.Type as nvarchar) + ',"' + PayrollPriceType.Code + '","' + PayrollPriceType.Name + '","' + ISNULL(PayrollPriceType.Description, '') + '"));' from PayrollPriceType where PayrollPriceType.ActorCompanyId = 701609 and PayrollPriceType.State = 0 order by PayrollPriceType.Name
             */
            #endregion

            id = 1;
            var l = new List<PayrollPriceTypeDTO>();

            l.Add(Create(3, "ANSM", "Anställningstidstillägg (månadslön)", ""));
            l.Add(Create(1, "ANSTL", "Anställningstidstillägg (timlön)", ""));
            l.Add(Create(3, "ANSTTILLM", "Anställningstidstillägg HAO månadslön", ""));
            l.Add(Create(1, "ANSTTILLT", "Anställningstidstillägg HAO timlön", ""));
            l.Add(Create(3, "ATM", "Ansvarstillägg Tjm månadslön", ""));
            l.Add(Create(1, "ATT", "Ansvarstillägg Tjm timlön", ""));
            l.Add(Create(0, "HAOGAR31", "E-hand - garantibelopp arbetstagare med 3 års branschvana", ""));
            l.Add(Create(0, "HAOGAR182", "E-handel - garantibelopp arbetstagare som fyllt 18 år", ""));
            l.Add(Create(0, "FRYS", "FRYSTILLÄGG", ""));
            l.Add(Create(0, "HAOGAR3", "HAO - garantibelopp arbetstagare med 3 års branschvana", ""));
            l.Add(Create(0, "HAOGAR18", "HAO - garantibelopp arbetstagare som fyllt 18 år", ""));
            l.Add(Create(3, "ITM", "Individuellt tillägg (månadslön)", ""));
            l.Add(Create(3, "POTTMAN", "Individuellt tillägg (pott heltid) HAO Månadslön", ""));
            l.Add(Create(1, "POTTTIM", "Individuellt tillägg (pott) HAO Timlön", ""));
            l.Add(Create(1, "ITT", "Individuellt tillägg (timlön)", ""));
            l.Add(Create(0, "KMERSSKPL", "KM-ers skattepl", ""));
            l.Add(Create(3, "KTM", "Kompetenstillägg (månadslön)", ""));
            l.Add(Create(1, "KTT", "Kompetenstillägg (timlön)", ""));
            l.Add(Create(0, "LONEVPENSI", "Löneväxling (brutto) mot pension", "Registreras positivt"));
            l.Add(Create(0, "MALTIDERS", "Måltidsersättning", ""));
            l.Add(Create(3, "ML", "Månadslön heltid", ""));
            l.Add(Create(3, "MLT", "Månadslön heltid (tarifflön)", ""));
            l.Add(Create(2, "PTM", "Personligt tillägg HAO månadslön", ""));
            l.Add(Create(1, "PTT", "Personligt tillägg HAO timlön", ""));
            l.Add(Create(0, "SEMTP05", "Semester - procentsats rörligt semestertillägg (0,5)", ""));
            l.Add(Create(0, "SEMP46", "Semester - procentsats semesterlön/semesteravdrag (4,6)", ""));
            l.Add(Create(0, "SEMTP08", "Semester - procentsats semestertillägg (0,8)", ""));
            l.Add(Create(1, "TL", "Timlön (minimilön)", ""));
            l.Add(Create(1, "TLT", "Timlön (tarifflön)", ""));

            return l;
        }
        private static PayrollPriceTypeDTO Create(int type, string code, string name, string description)
        {
            return new PayrollPriceTypeDTO()
            {
                PayrollPriceTypeId = id++,
                Name = name,
                Description = description,
                Code = code,
                Type = type
            };
        }
    }
}
