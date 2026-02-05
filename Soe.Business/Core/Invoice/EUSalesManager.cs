using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core
{
    public class EUSalesManager : ManagerBase
    {
        #region Ctor

        public EUSalesManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public List<SalesEUDTO> GetSales(int actorCompanyId, DateTime startDate, DateTime stopDate)
        {
            var euSalesDTOs = new List<SalesEUDTO>();
            var sysEuCountries = CountryCurrencyManager.GetEUSysCountrieIds(startDate);

            using (var entities = new CompEntities())
            {
                var sales = entities.GetEUSales(actorCompanyId, startDate, stopDate, string.Join(",", sysEuCountries) );
                foreach (var sale in sales)
                {
                    var euSaleDTO = new SalesEUDTO
                    {
                        ActorId = sale.ActorId.HasValue ? sale.ActorId.Value : 0,
                        CustomerName = sale.CustomerName,
                        CustomerNr = sale.CustomerNr,
                        SumGoodsSale = sale.SumGoodsSale.HasValue ? sale.SumGoodsSale.Value : 0,
                        SumServiceSale = sale.SumServiceSale.HasValue ? sale.SumServiceSale.Value : 0,
                        SumTriangulationSales = sale.SumTriangulationSales.HasValue ? sale.SumTriangulationSales.Value : 0,
                        VATNr = sale.VatNr
                    };
                    euSalesDTOs.Add(euSaleDTO);
                }
            }

            return euSalesDTOs;
        }

        public List<SalesEUDetailDTO> GetSalesDetails(int actorId, DateTime startDate, DateTime stopDate)
        {
            var euSalesDTOs = new List<SalesEUDetailDTO>();
            var sysEuCountries = CountryCurrencyManager.GetEUSysCountrieIds(startDate);

            using (var entities = new CompEntities())
            {
                var invoices = entities.GetEUSalesDetail(actorId, startDate, stopDate, string.Join(",", sysEuCountries));
                foreach (var invoice in invoices)
                {
                    var euSaleDTO = new SalesEUDetailDTO
                    {
                        CustomerInvoiceId = invoice.InvoiceId,
                        InvoiceNr = invoice.InvoiceNr,
                        InvoiceDate = invoice.InvoiceDate,
                        TotalAmountExVat = invoice.TotalAmount - invoice.VATAmount,
                        SumGoodsSale = invoice.SumGoodsSale.HasValue ? invoice.SumGoodsSale.Value : 0,
                        SumServiceSale = invoice.SumServiceSale.HasValue ? invoice.SumServiceSale.Value : 0,
                        TriangulationSales = invoice.TriangulationSales
                    };
                    euSalesDTOs.Add(euSaleDTO);
                }
            }
            return euSalesDTOs;
        }

       public ActionResult GetExportFiles(int actorCompanyId, DatePeriodType period, DateTime startDate, DateTime stopDate)
        {
            var result = new ActionResult();
            var values = new List<string>();

            int startMonth = startDate.Month;
            int stopMonth = stopDate.Month;

            while (startMonth <= stopMonth)
            {
                var periodStartDate = startDate;
                var periodStopDate = stopDate;

                //month or quarter
                switch (period)
                {
                    case DatePeriodType.Month:
                        periodStartDate = startDate;
                        periodStopDate = startDate.AddMonths(1).AddDays(-1);
                        startMonth += 1;
                        startDate = startDate.AddMonths(1);
                        break;
                    case DatePeriodType.Quarter:
                        periodStartDate = startDate;
                        periodStopDate = startDate.AddMonths(3).AddDays(-1);
                        startMonth += 3;
                        startDate = startDate.AddMonths(3);
                        break;
                    default:
                        throw new Exception("GetSalesEUExportFile got illigal periodtype");
                }

                result = GetExportFile(actorCompanyId, period, periodStartDate, periodStopDate);
                if (!result.Success)
                    return result;
                values.Add(result.Value.ToString());
            }
            result.Value = values;
            return result;
        }

        public ActionResult GetExportFile(int actorCompanyId, DatePeriodType period, DateTime startDate, DateTime stopDate)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            switch ((TermGroup_Languages)company.SysCountryId)
            {
                case TermGroup_Languages.Swedish:
                    return GetExportFileSE(company, period, startDate, stopDate);
                case TermGroup_Languages.Finnish:
                    return GetExportFileFI(company, period, startDate, stopDate);
                default:
                    return new ActionResult(false, 0, "Unsupported country");
            }
        }

        public ActionResult GetExportFileSE(Company company, DatePeriodType period, DateTime startDate, DateTime stopDate)
        {
            //https://www.skatteverket.se/download/18.353fa3f313ec5f91b9515e3/1370441866460/27805.pdf
            var newLine = "\r\n";
            var items = GetSales(company.ActorCompanyId, startDate, stopDate);
            var resultString = new StringBuilder("SKV574008;" + newLine, items.Count * 50);

            if (string.IsNullOrEmpty(company.VatNr))
            {
                return new ActionResult(false, 0, "Företagets momsnummer är inte ifyllt!");
            }

            resultString.Append(company.VatNr + ";");

            //month or quarter
            switch (period)
            {
                case DatePeriodType.Month:
                    resultString.Append(startDate.ToString("yy") + startDate.ToString("MM") + ";");
                    break;
                case DatePeriodType.Quarter:
                    resultString.Append(startDate.ToString("yy") + "-" + CalendarUtility.GetQuarterNr(startDate) + ";");
                    break;
                default:
                    throw new Exception("GetSalesEUExportFile got illigal periodtype");
            }

            //Uppgiftslämnare, name = max 35 characters
            var user = UserManager.GetUser(UserId);
            if (string.IsNullOrEmpty(user.Name))
            {
                return new ActionResult(false, 0, "Användaren saknar namn.");
            }

            if (!user.ContactPersonReference.IsLoaded)
            {
                user.ContactPersonReference.Load();
            }
            //email and phone
            var contactInfo = ContactManager.GetContactEComsFromActor(user.ContactPerson.ActorContactPersonId, false);
            var phone = contactInfo.FirstOrDefault(x => x.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob);
            if (phone == null)
            {
                phone = contactInfo.FirstOrDefault(x => x.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile);
            }
            if (phone == null || string.IsNullOrEmpty(phone.Text))
            {
                return new ActionResult(false, 0, "Användaren saknar telefonnummer.");
            }

            var email = contactInfo.FirstOrDefault(x => x.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);

            resultString.Append(user.Name.Substring(0, Math.Min(user.Name.Length, 35)) + ";" + phone.Text + ";" + (email == null ? "" : email.Text));

            resultString.Append(newLine);

            foreach (var item in items)
            {
                if (item.SumGoodsSale == 0 && item.SumTriangulationSales == 0 && item.SumServiceSale == 0)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(item.VATNr))
                {
                    return new ActionResult(false, 0, $"VAT-nr saknas för {item.CustomerNr}-{item.CustomerName}");
                }
                
                resultString.Append(item.VATNr + ";");
                resultString.Append( (item.SumGoodsSale != 0 ? decimal.Truncate(item.SumGoodsSale).ToString() : "") + ";");
                resultString.Append( (item.SumTriangulationSales != 0 ? decimal.Truncate(item.SumTriangulationSales).ToString() : "") + ";");
                resultString.Append( (item.SumServiceSale != 0 ? decimal.Truncate(item.SumServiceSale).ToString() : "") + ";");
                resultString.Append(newLine);
            }

            return new ActionResult { Value = resultString.ToString() };
        }


        public ActionResult GetExportFileFI(Company company, DatePeriodType period, DateTime startDate, DateTime stopDate)
        {
            //https://www.vero.fi/contentassets/ef5905e0f5b74bcba89aa9ba9c34015d/tietuekuvaus_vsralvyv_swe.pdf
            var newLine = "\r\n";
            var items = GetSales(company.ActorCompanyId, startDate, stopDate);
            var resultString = new StringBuilder("000:VSRALVYV" + newLine, items.Count * 50);

            resultString.Append("198:" + DateTime.Now.ToString("ddMMyyyyhhmmss") + newLine);

            //VAT
            if (string.IsNullOrEmpty(company.VatNr))
            {
                return new ActionResult(false, 0, "Företagets momsnummer är inte ifyllt!");
            }

            resultString.Append("010:" + company.VatNr + newLine);

            resultString.Append("052:" + startDate.ToString("M") + newLine);
            resultString.Append("053:" + startDate.ToString("yyyy") + newLine);
            resultString.Append("001:" + items.Count.ToString() + newLine);

            string VATWhitoutCountry;
            int i = 1;
            foreach (var item in items)
            {
                var VATcountryCode = StringUtility.LeftExtractLetters(item.VATNr,2, out VATWhitoutCountry);
                resultString.Append("102:" + VATcountryCode + newLine);
                resultString.Append("103:" + VATWhitoutCountry + newLine);
                resultString.Append("210:" + item.SumGoodsSale.ToString() + newLine);
                resultString.Append("211:" + item.SumServiceSale.ToString() + newLine);
                resultString.Append("211:" + item.SumTriangulationSales.ToString() + newLine);
                resultString.Append("009:" + i.ToString() + newLine);
                i++;
            }

            resultString.Append("048:SoftOne GO" + newLine);
            resultString.Append("014:1448245-0_GO" + newLine);
            resultString.Append("999:1" + newLine);

            return new ActionResult { Value = resultString.ToString() };
        }
    }
}
