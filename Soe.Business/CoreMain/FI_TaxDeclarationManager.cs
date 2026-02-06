using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class FI_TaxDeclarationManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public FI_TaxDeclarationManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Export

        /// <summary>
        /// export for FinnishTax authorities (Period tax declaring)
        /// </summary>
        /// <returns>True if the Export was successfull, otherwise false</returns>
        public ActionResult Export(TextWriter sr, int actorCompanyId, string taxEra, int taxPeriod, int taxYear, bool noActivity, int selectedReason)
        {            
            var result = new ActionResult(true);
            try
            {
                #region init
               
                DateTime dateFrom = new DateTime(taxYear, 1, 1);
                DateTime dateTo = new DateTime(taxYear, 12, 31);

                if (taxEra == "K")
                {
                    dateFrom = new DateTime(taxYear, taxPeriod, 1);
                    dateTo = dateFrom.AddMonths(1).AddDays(-1);
                }
                else if (taxEra == "Q")
                {
                    switch (taxPeriod)
                    {
                        case 1:
                            dateFrom = new DateTime(taxYear, 1, 1);
                            break;
                        case 2:
                            dateFrom = new DateTime(taxYear, 4, 1);
                            break;
                        case 3:
                            dateFrom = new DateTime(taxYear, 7, 1);
                            break;
                        case 4:
                            dateFrom = new DateTime(taxYear, 10, 1);
                            break;
                    }
                    dateTo = dateFrom.AddMonths(3).AddDays(-1);
                }

                #endregion

                #region Prereq

                //Get AccountYear
                AccountYear accountYear = AccountManager.GetAccountYear(dateFrom, actorCompanyId);                
                if (accountYear == null)
                {
                    // AccountYearNr is always 0 for this year and -1 for previus year
                    result.ErrorMessage = "";
                    return result;
                }

                //Get Company and Contact
                Company company = CompanyManager.GetCompany(actorCompanyId);
                if (company == null)
                {
                    result.ErrorMessage = "";
                    return result;
                }
                //Get AccountDim std
                AccountDim accountDimStd = AccountManager.GetAccountDimStd(actorCompanyId);
                if (accountDimStd == null)
                {
                    result.ErrorMessage = "";
                    return result;
                }

                decimal periodTax = 0;
                decimal vatsum = 0;

                #endregion

                #region Starting lines

                //000: tietovirran nimi / Name of data flow
                sr.WriteLine("000:VSRALVKV");

                //198: lähetyspäivä ja -aika, pakollinen 13.6.2017 alkaen                
                sr.WriteLine("198:"+DateTime.Now.ToString("ddMMyyyyHHmmss"));

                //014: ilmoituksen tuottaneen ohjelmiston yksilöivä tieto (y-tunnus + _GO) / identifier for sending software (org.number + _GO)
                sr.WriteLine("014:1448245-0_GO");

                //048: ilmoituksen tuottanut ohjelmisto / sending software
                sr.WriteLine("048:SoftOne GO");

                //010: Y-tunnus / Personal ID, Business ID, accounting unit ID
                sr.WriteLine("010:" + company.OrgNr);

                //050: ilmoitusjakso / Reporting frequency (K=kuukausi/month, Q=vuosineljännes/quarter, V=kalenterivuosi/Calendar year)
                sr.WriteLine("050:"+ taxEra);

                //verokausi / taxperiod
                if (taxEra == "K" || taxEra == "Q")
                    sr.WriteLine("052:" + taxPeriod);

                //kohdevuosi / Year
                sr.WriteLine("053:" + taxYear);

                //056: ei toimintaa / no activity
                if (noActivity)
                    sr.WriteLine("056:1");                               
                else
                {                
                    using (CompEntities entities = new CompEntities())
                    {
                        #region Prereq

                        List<SysVatAccount> sysVatAccounts = SysDbCache.Instance.SysVatAccounts;
                        List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(actorCompanyId, null, true, false);
                        BalanceItemDTO balanceItem = new BalanceItemDTO();

                        #endregion

                        //suoritettava 24%:n vero kotimaan myynnistä / 24% tax on domestic sales
                        //suoritettava yleisen verokannan mukainen  vero kotimaan myynnistä / tax according to general vat rate on domestic sales
                        vatsum = (this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 301, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 301, 10, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 301, 2, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 301, 20, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 301, 3, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 301, 30, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 923, 1, 24)) * -1;
                        periodTax += vatsum;
                        if (vatsum != 0)
                            sr.WriteLine("301:" + Convert.ToString(vatsum));

                        //suoritettava 14%:n vero kotimaan myynnistä / 14% tax on domestic sales
                        //suoritettava 13,5%:n vero kotimaan myynnistä / 13,5% tax on domestic sales
                        vatsum = (this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 302, 1, 14) +
                         this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 302, 10, 13.5)) * -1;
                        periodTax += vatsum;
                        if (vatsum != 0)
                            sr.WriteLine("302:" + Convert.ToString(vatsum));

                        //suoritettava 10%:n vero kotimaan myynnistä / 10% tax on domestic sales
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 303, 1, 10) * -1;
                        periodTax += vatsum;
                        if (vatsum != 0)
                            sr.WriteLine("303:" + Convert.ToString(vatsum));

                        //vero tavaroiden maahantuonnista EU:n ulkopuolelta / VAT on import of goods from outside the EU
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 10, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 2, 14) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 20, 13.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 3, 10);
                        if (vatsum != 0)
                            sr.WriteLine("304:" + Convert.ToString(vatsum));

                        periodTax += vatsum;

                        //vero tavaraostoista muista EU-maista / Tax on goods purchased from other EU Member States
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 10, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 2, 14) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 20, 13.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 3, 10);
                        if (vatsum != 0)
                            sr.WriteLine("305:" + Convert.ToString(vatsum));

                        periodTax += vatsum;

                        //306 vero palveluostoista muista EU-maista / Tax on services purchased from other EU Member States
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 10, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 2, 14) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 20, 13.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 3, 10);
                        if (vatsum != 0)
                            sr.WriteLine("306:" + Convert.ToString(vatsum));

                        periodTax += vatsum;

                        //318 vero rakentamispalvelun ostoista / Tax on construction services
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 320, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 320, 10, 25.5);
                        periodTax += vatsum;
                        if (vatsum != 0)
                            sr.WriteLine("318:" + Convert.ToString(vatsum));

                        //307 Kohdekauden vähennettävä vero / Tax deductible for Period in question
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 10, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 2, 14) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 20, 13.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 3, 10) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 10, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 2, 14) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 20, 13.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 3, 10) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 10, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 2, 14) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 20, 13.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 3, 10) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 307, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 307, 10, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 307, 2, 14) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 307, 20, 13.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 307, 3, 10) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 307, 4, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 301, 2, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 301, 20, 25.5) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 320, 1, 24) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 320, 10, 25.5);
                        periodTax += (vatsum * -1);
                        if (vatsum != 0)
                            sr.WriteLine("307:" + Convert.ToString(Decimal.Round(vatsum, 2)));

                        //308 Maksettava/palautettava alv / Tax payable (301+302+303+304+305+306+318-307)
                        if (periodTax != 0)
                            sr.WriteLine("308:" + Convert.ToString(periodTax));

                        //309 O-verokannan alainen liikevaihto / Sales, taxable at zero VAT rate
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 309, 1, 100) * -1;
                        if (vatsum != 0)
                            sr.WriteLine("309:" + Convert.ToString(vatsum));

                        //310 Tavaroiden maahantuonnit EU:n ulkopuolelta / Import of goods from outside the EU
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 1, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 10, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 2, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 20, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 3, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 4, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 40, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 5, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 50, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 304, 6, 100);
                        if (vatsum != 0)
                            sr.WriteLine("310:" + Convert.ToString(vatsum));

                        //311 Tavaran myynti muihin eu-maihin / Sales of goods to other EU Member States
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 311, 1, 100) * -1;
                        if (vatsum != 0)
                            sr.WriteLine("311:" + Convert.ToString(vatsum));

                        //312 Palvelun myynti muihin eu-maihin / Sales of services to other EU Member States
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 312, 1, 100) * -1;
                        if (vatsum != 0)
                            sr.WriteLine("312:" + Convert.ToString(vatsum));

                        //313 Tavaraostot muista eu-maista / Purchases of goods from other EU Member States
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 1, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 10, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 2, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 20, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 3, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 4, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 40, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 5, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 50, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 305, 6, 100);
                        if (vatsum != 0)
                            sr.WriteLine("313:" + Convert.ToString(vatsum));

                        //314 Palveluostot muista eu-maista / Purchases of services from other EU Member States
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 1, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 10, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 2, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 20, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 3, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 4, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 40, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 5, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 50, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 306, 6, 100);
                        if (vatsum != 0)
                            sr.WriteLine("314:" + Convert.ToString(vatsum));

                        //319 Rakentamispalvelun myynnit / Total sales of construction services
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 319, 1, 100) * -1;
                        if (vatsum != 0)
                            sr.WriteLine("319:" + Convert.ToString(vatsum));

                        //320 Rakentamispalvelun ostot / Total purchases of construction services
                        vatsum = this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 320, 1, 100) +
                                 this.GetSumForSysVatAccount(entities, actorCompanyId, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, balanceItem, 320, 10, 100); ;
                        if (vatsum != 0)
                            sr.WriteLine("320:" + Convert.ToString(vatsum));
                    }
                }

                //332: Korjausilmoituksen syy = Laskuvirhe / ilmoituksen täyttövirhe
                if (selectedReason == 1)
                    sr.WriteLine("332:1");

                //333: Korjausilmoituksen syy = Verotarkastuksessa saatu ohjaus
                if (selectedReason == 2)
                    sr.WriteLine("333:1");

                //334: Korjausilmoituksen syy = Oikeuskäytännön muutos
                if (selectedReason == 3)
                    sr.WriteLine("334:1");

                //332: Korjausilmoituksen syy = Laintulkintavirhe
                if (selectedReason == 4)
                    sr.WriteLine("335:1");

                //042: yhteyshenkilön puhelinnumero / Telephone number of person to contact
                Contact contact = ContactManager.GetContactFromActor(company.ActorCompanyId);
                if (contact != null)
                {
                    ContactECom contactECom = ContactManager.GetContactECom(contact.ContactId, (int)TermGroup_SysContactEComType.PhoneJob, false);
                    if (contactECom != null)
                    {
                        string Phone = contactECom.Text;
                        if (Phone == null || Phone.Trim() == "")
                        {
                            result.ErrorMessage = "Phonenumber";
                            return result;
                        }
                        sr.WriteLine("042:" + Phone);
                    }
                }

                //trailing/endline
                sr.WriteLine("999:1");

                #endregion

                result.BooleanValue = true;
            }


            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return result;
            }
            return result;
        }

        public ActionResult Export(FinnishTaxExportDTO export, int actorCompanyId) 
        {
            ActionResult result = new ActionResult(false);

            string filePath = ConfigSettings.SOE_SERVER_DIR_TEMP_FI_TAX_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_FI_TAX_SUFFIX;
            try
            {
                using (TextWriter writer = new StreamWriter(filePath, false, Constants.ENCODING_LATIN1))
                {
                    result = this.Export(writer, actorCompanyId, export.LengthOfTaxPeriodStr, export.TaxPeriod, export.TaxPeriodYear, export.NoActivity, export.Cause);
                }

                if (result.Success)
                {
                    result.Value = new FinnishTaxExporFiletDTO()
                    {
                        Name = Constants.SOE_SERVER_FILENAME_PREFIX + Constants.SOE_SERVER_FI_TAX_PREFIX + DateTime.Now.ToString("yyyyMMdd") + Constants.SOE_SERVER_FILE_FI_TAX_SUFFIX,
                        Extension = Constants.SOE_SERVER_FILE_FI_TAX_SUFFIX,
                        Data = Convert.ToBase64String(File.ReadAllBytes(filePath))
                    };
                }
            }
            finally
            {
                if (!result.Success)
                    File.Delete(filePath);
            }

            return result;
        }


        #endregion

        #region Help methods

        private decimal GetSumForSysVatAccount(CompEntities entities, int actorCompanyId, AccountYear AYear, DateTime dateFrom, DateTime dateTo, List<SysVatAccount> sysVatAccounts, List<AccountStd> accountStds, BalanceItemDTO balanceItem, int vatnr1, int vatnr2, double rate)
        {
            decimal result = 0;

            SysVatAccount sysVatAccount = sysVatAccounts.FirstOrDefault(a => (a.VatNr1.HasValue && a.VatNr1.Value == vatnr1) && (a.VatNr2.HasValue && a.VatNr2.Value == vatnr2));

            //accounts
            if (sysVatAccount != null)
            {
                foreach (AccountStd accountStd in accountStds.Where(a => a.SysVatAccountId.HasValue && a.SysVatAccountId.Value == sysVatAccount.SysVatAccountId))
                {
                    BalanceItemDTO balanceItemAccount = AccountBalanceManager(actorCompanyId).GetBalanceChange(entities, AYear, dateFrom, dateTo, accountStd, null, actorCompanyId, false);
                    if (balanceItemAccount != null && balanceItemAccount.Balance != 0)
                        result += balanceItemAccount.Balance;
                }

                if (result != 0)
                {
                    if (vatnr1 == 923 || (vatnr1 == 301 && (vatnr2 == 3 || vatnr2 == 30)))
                    {
                        result = result * (decimal)rate / (100 + (decimal)rate);
                    }
                    else
                    {
                        result = result * (decimal)rate / 100;
                    }

                    result = Decimal.Round(result, 2);
                }
            }

            return result;

        }

        #endregion
    }
}
