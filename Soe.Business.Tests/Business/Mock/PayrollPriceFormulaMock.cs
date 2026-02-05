using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class PayrollPriceFormulaMock
    {
        private static int id = 1;

        public static List<PayrollPriceFormulaDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create("'+ PayrollPriceFormula.Code + '","' + PayrollPriceFormula.Name + '","' + ISNULL(PayrollPriceFormula.Description, '') + '"));' from PayrollPriceFormula where PayrollPriceFormula.ActorCompanyId = 291 and PayrollPriceFormula.State = 0 order by PayrollPriceFormula.Name
             */
            #endregion

            id = 1;
            var l = new List<PayrollPriceFormulaDTO>();

            l.Add(Create("AL", "Aktuell lön", ""));
            l.Add(Create("ALLART", "Aktuell lön löneart", ""));
            l.Add(Create("HAOARBTIM", "Avdrag del av dag Handels månad", ""));
            l.Add(Create("HAOARBDAG", "Avdrag hel arbetsdag Handels månadslön", ""));
            l.Add(Create("HAOARBDAG2", "Avdrag hel arbetsdag Handels månadslön", ""));
            l.Add(Create("HAOTLTOTAV", "Avdrag timlön Handels timlön", ""));
            l.Add(Create("BILERSPRIV", "Bilersättning enl schablon", ""));
            l.Add(Create("BVAN", "Branschvana", ""));
            l.Add(Create("FSTRD", "Formel FSTRD", ""));
            l.Add(Create("ALH", "HAO Aktuell månadslön", ""));
            l.Add(Create("HAOBTL", "HAO Beräknad timlön (inkl ansvarstillägg)", ""));
            l.Add(Create("HAOOB100M", "HAO Månad OB-tillägg 100%", ""));
            l.Add(Create("HAOOB50M", "HAO Månad OB-tillägg 50%", ""));
            l.Add(Create("HAOOB70M", "HAO Månad OB-tillägg 70%", ""));
            l.Add(Create("HAOMTIM", "HAO Månadslön Timlön", ""));
            l.Add(Create("SLONHAOMH", "HAO Sjuklön hel dag 2-14 månad", ""));
            l.Add(Create("HAOSJLONTI", "HAO Sjuklön Timavlönade", ""));
            l.Add(Create("HAOSJOB10T", "HAO Sjuk-ob 100% Timlön", ""));
            l.Add(Create("HAOSJOB50T", "HAO Sjuk-ob 50% Timlön", ""));
            l.Add(Create("HAOSJOB70T", "HAO Sjuk-ob 70% Timlön", ""));
            l.Add(Create("HAOOB100T", "HAO Timlön OB-tillägg 100%", ""));
            l.Add(Create("HAOOB50T", "HAO Timlön OB-tillägg 50%", ""));
            l.Add(Create("HAOOB70T", "HAO Timlön OB-tillägg 70%", ""));
            l.Add(Create("HAOTLTOT", "HAO Timlön total", ""));
            l.Add(Create("HAOOT100", "HAO Övertid 100%", ""));
            l.Add(Create("HAOOT50", "HAO Övertid 50%", ""));
            l.Add(Create("HAOOT70", "HAO Övertid 70%", ""));
            l.Add(Create("HAOOM100", "HAO Övertid mån 100%", ""));
            l.Add(Create("HAOOM50", "HAO Övertid mån 50%", ""));
            l.Add(Create("HAOOM70", "HAO Övertid mån 70%", ""));

            return l;
        }
        private static PayrollPriceFormulaDTO Create(string code, string name, string description)
        {
            return new PayrollPriceFormulaDTO()
            {
                PayrollPriceFormulaId = id++,
                Name = name,
                Description = description,
                Code = code,                
            };
        }
    }
}
