using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class SOPPP : ImportSpecial
    {
        public SOPPP(ParameterObject parameterObject) : base(parameterObject) { }

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CompanyManager companyManager = new CompanyManager(null);
        public AccountManager accountManager = new AccountManager(null);
        public PaymentManager paymentManager = new PaymentManager(null);
        public SettingManager settingManager = new SettingManager(null);
        public CountryCurrencyManager countryCurrencyManager = new CountryCurrencyManager(null);
        public ProductManager productManager = new ProductManager(null);
        public ProductPricelistManager productPricelistManager = new ProductPricelistManager(null);

        public string ApplySOPVoucher(string content, int actorCompanyId)
        {
            string modifiedContent = string.Empty;
            string voucherNumber = string.Empty;
            string voucherSeries = string.Empty;
            List<XElement> previousVoucherRows = new List<XElement>();
            List<XElement> voucherHeads = new List<XElement>();
            XElement voucherHeadsElement = new XElement("Verifikat");
            string previousVoucherNumber = voucherNumber;
            string previousVoucherSeries = voucherSeries;
            XElement previousVoucherRow = null;
            XElement voucherHead = null;
            string voucherDate = string.Empty;
            string previousVoucherDate = string.Empty;


            //Original design
            //<Verifikatrader>
            //    <Verifikatrad>
            //      <Årsnr>7</Årsnr>
            //      <Verserie>1</Verserie>
            //      <Vernr>1</Vernr>
            //      <Radnr>100</Radnr>
            //      <Bokföringsdatum>1996-01-10</Bokföringsdatum>
            //      <Konto>1510</Konto>
            //      <Kostnadsställe></Kostnadsställe>
            //      <Projekt></Projekt>
            //      <Verifikattext>Fakturering</Verifikattext>
            //      <Saldo>1400000.00</Saldo>
            //      <Kvantitiet>0.00</Kvantitiet>

            XElement xml = null;

            try
            {
                xml = XElement.Parse(content);
            }
            catch
            {
                return modifiedContent;
            }

            List<XElement> rows = xml.Elements("Verifikatrad").ToList();

            bool firstRowOnNewVoucher = false;

            foreach (XElement voucherRow in rows)
            {
                voucherNumber = voucherRow.Element("Vernr").Value != null ? voucherRow.Element("Vernr").Value : string.Empty;
                voucherSeries = voucherRow.Element("Verserie").Value != null ? voucherRow.Element("Verserie").Value : string.Empty;
                voucherDate = voucherRow.Element("Bokföringsdatum").Value != null ? voucherRow.Element("Bokföringsdatum").Value : string.Empty;

                //First voucherRow
                if (previousVoucherNumber == string.Empty)
                {
                    previousVoucherNumber = voucherNumber;
                    previousVoucherSeries = voucherSeries;
                    previousVoucherDate = voucherDate;
                    previousVoucherRow = voucherRow;
                    continue;
                }

                //Following voucherRows on same voucher
                if (voucherNumber == previousVoucherNumber && voucherSeries == previousVoucherSeries)
                {
                    if (!firstRowOnNewVoucher)
                        previousVoucherRows.Add(previousVoucherRow);
                    firstRowOnNewVoucher = false;
                    previousVoucherRow = voucherRow;
                    previousVoucherNumber = voucherNumber;
                    previousVoucherSeries = voucherSeries;
                    previousVoucherDate = voucherDate;
                    continue;
                }

                //New Voucher, so we need to create voucherelement from previous vouchers
                if ((voucherNumber != previousVoucherNumber || voucherDate != previousVoucherDate || voucherSeries != previousVoucherSeries) && previousVoucherNumber != string.Empty)
                {
                    voucherHead = new XElement("Verhuvud",
                        new XElement("Vernr", previousVoucherNumber),
                        new XElement("Verserie", previousVoucherSeries),
                        new XElement("Datum", previousVoucherDate));

                    previousVoucherRows.Add(previousVoucherRow);
                    voucherHead.Add(previousVoucherRows);
                    voucherHeads.Add(voucherHead);
                    previousVoucherNumber = voucherNumber;
                    previousVoucherSeries = voucherSeries;
                    previousVoucherDate = voucherDate;
                    previousVoucherRows.Clear();
                    previousVoucherRow = voucherRow;
                    previousVoucherRows.Add(previousVoucherRow);
                    firstRowOnNewVoucher = true;
                }
            }

            //Last one
            if (previousVoucherRows.Count > 0)
            {
                previousVoucherRows.Add(previousVoucherRow);

                voucherHead = new XElement("Verhuvud",
                    new XElement("Vernr", previousVoucherNumber),
                    new XElement("Verserie", previousVoucherSeries),
                    new XElement("Datum", previousVoucherDate));

                voucherHead.Add(previousVoucherRows);
                voucherHeads.Add(voucherHead);
                previousVoucherRows.Clear();
            }

            voucherHeadsElement.Add(voucherHeads);

            modifiedContent = voucherHeadsElement.ToString();

            return modifiedContent;
        }

        public ActionResult CreateBaseAccount(CompEntities entities, string content, int actorCompanyId)
        {
            // <Standardkonton>
            //   <Inhemsk>
            //        <FörsäljningMomspliktig>f moms</FörsäljningMomspliktig>a
            //        <FörsäljningMomsfri>f fri</FörsäljningMomsfri>
            //            .....
            //   </Inhemsk>
            //   <Eu>
            //        <FörsäljningMomspliktig>3020</FörsäljningMomspliktig>
            //        <FörsäljningMomsfri>3020</FörsäljningMomsfri>
            //            .....
            //   </Eu>
            //   <EuTrepart>
            //        <FörsäljningMomspliktig>3057</FörsäljningMomspliktig>
            //        <FörsäljningMomsfri>3057</FörsäljningMomsfri>
            //            .....
            //  </EuTrepart>
            //  <Export>
            //    <FörsäljningMomspliktig>3030</FörsäljningMomspliktig>
            //    <FörsäljningMomsfri>3030</FörsäljningMomsfri>
            //        .....
            //  </Export>
            //</Standardkonton>

            ActionResult result = new ActionResult();
            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);
            List<Account> accounts = accountManager.GetAccountsStdsByCompany(entities, actorCompanyId, loadAccount: true);
            //Duplicate Settings in XE
            int accountInvoiceProductPurchaseAccountId = 0;
            int accountInvoiceProductSalesAccountId = 0;
            int accountInvoiceProductSalesVatFreeAccountId = 0;
            Dictionary<int, int> duplicateSettings = new Dictionary<int, int>();


            if (company == null)
            {
                result.Success = false;
                result.ErrorMessage = "No Company found";
                return result;
            }
            try
            {
                XDocument doc = null;

                try
                {
                    doc = XDocument.Parse(content);
                }
                catch
                {
                    try
                    {
                        MemoryStream stream = new MemoryStream((new UTF8Encoding()).GetBytes(content));
                        doc = XDocument.Load(stream);
                    }
                    catch
                    {
                        try
                        {
                            MemoryStream stream = new MemoryStream((new UTF8Encoding()).GetBytes(content));
                            DataSet dsImportXmlData = new DataSet();
                            dsImportXmlData.ReadXml(stream);
                            doc = GetXDocumentFromDataSet(dsImportXmlData);
                        }
                        catch
                        {
                            result.Success = false;
                            result.ErrorMessage = "Could not parse XML";
                            return result;
                        }
                    }
                }

                foreach (XElement rootElement in doc.Elements("Standardkonton"))
                {
                    foreach (XElement element in rootElement.Elements("Inhemsk"))
                    {
                        foreach (XElement subElement in element.Elements())
                        {
                            int accountId = GetBaseAccount(accounts, subElement.Value);
                            if (accountId == 0)
                                continue;

                            string name = subElement.Name.ToString();

                            int settingTypeId = 0;
                            settingTypeId = SetBaseAccountSettingTypeSOP(name);

                            if (name.Equals("Kundfordran"))
                            {
                                accountInvoiceProductPurchaseAccountId = accountId;
                                duplicateSettings.Add((int)CompanySettingType.AccountInvoiceProductPurchase, accountId);
                            }

                            if (name.Equals("FörsäljningMomspliktig"))
                            {
                                accountInvoiceProductSalesAccountId = accountId;
                                duplicateSettings.Add((int)CompanySettingType.AccountInvoiceProductSales, accountId);
                            }

                            if (name.Equals("FörsäljningMomsfri"))
                            {
                                accountInvoiceProductSalesVatFreeAccountId = accountId;
                                duplicateSettings.Add((int)CompanySettingType.AccountInvoiceProductSalesVatFree, accountId);
                            }

                            if (settingTypeId == 0)
                                continue;

                            UserCompanySetting setting = settingManager.GetUserCompanySetting(entities, SettingMainType.Company, settingTypeId, 0, actorCompanyId, 0);

                            if (setting != null)
                            {
                                setting.IntData = accountId;
                            }
                            else
                            {
                                setting = new UserCompanySetting();
                                setting.IntData = accountId;
                                setting.SettingTypeId = settingTypeId;
                                setting.ActorCompanyId = actorCompanyId;
                                setting.DataTypeId = (int)SettingDataType.Integer;

                                SetCreatedProperties(setting);
                                entities.UserCompanySetting.AddObject(setting);
                            }

                            result = SaveChanges(entities);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);

                result.Success = false;
                return result;
            }

            //Duplicates

            foreach (var setting in duplicateSettings)
            {
                UserCompanySetting duplicateSetting = settingManager.GetUserCompanySetting(entities, SettingMainType.Company, setting.Key, 0, actorCompanyId, 0);

                if (duplicateSetting == null)
                {
                    duplicateSetting = new UserCompanySetting();
                    duplicateSetting.IntData = setting.Value;
                    duplicateSetting.SettingTypeId = setting.Key;
                    duplicateSetting.ActorCompanyId = actorCompanyId;
                    duplicateSetting.DataTypeId = (int)SettingDataType.Integer;

                    SetCreatedProperties(duplicateSetting);
                    entities.UserCompanySetting.AddObject(duplicateSetting);

                    ActionResult duplicateResult = SaveChanges(entities);
                }
            }

            CreateVatCodesSOP(entities, actorCompanyId);

            return result;

        }

        public void CreatePaymentConditions(CompEntities entities, string content, int actorCompanyId, string elementName, string subElementName)
        {
            //<BetalningsvillkorKod>10</BetalningsvillkorKod>
            //<BetalningsvillkorDagar>010</BetalningsvillkorDagar>
            //<BetalningsvillkorBenämning>10 dagar netto</BetalningsvillkorBenämning>

            List<PaymentCondition> conditions = new List<PaymentCondition>();
            List<PaymentCondition> currentConditions = paymentManager.GetPaymentConditions(entities, actorCompanyId);
            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);

            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements(elementName))
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        string nrOfDays = subElement.Element("BetalningsvillkorDagar") != null ? subElement.Element("BetalningsvillkorDagar").Value : string.Empty;
                        if (nrOfDays == string.Empty)
                            continue;

                        string code = subElement.Element("BetalningsvillkorKod") != null ? subElement.Element("BetalningsvillkorKod").Value : string.Empty;
                        string name = subElement.Element("BetalningsvillkorBenämning") != null ? subElement.Element("BetalningsvillkorBenämning").Value : string.Empty;
                        string trimmedCodeString = code.TrimStart('0');
                        bool trimmedIsDifferent = (code == trimmedCodeString);
                        bool foundOnTrimmedCode = currentConditions.Any(c => c.Code == trimmedCodeString) || conditions.Any(c => c.Code == trimmedCodeString);
                        bool foundOnCode = currentConditions.Any(c => c.Code == code) || conditions.Any(c => c.Code == code);
                        name = name + " Skapad av import";

                        if (!foundOnCode && !foundOnTrimmedCode)
                        {
                            PaymentCondition condition = new PaymentCondition();
                            condition.Days = Convert.ToInt32(nrOfDays);
                            condition.Code = code;
                            condition.Name = name;
                            condition.Company = company;
                            conditions.Add(condition);
                        }
                    }
                }

                if (conditions.Count > 0)
                    SaveChanges(entities);

                foreach (var condition in conditions)
                {
                    base.TryDetachEntity(entities, condition);
                }

            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);
            }
        }

        public void CreateDeliveryConditions(CompEntities entities, string content, int actorCompanyId, string elementName, string subElementName)
        {
            //<LeveransvillkorKod>01</LeveransvillkorKod>

            List<DeliveryCondition> conditions = new List<DeliveryCondition>();
            List<DeliveryCondition> currentConditions = InvoiceManager.GetDeliveryConditions(entities, actorCompanyId);
            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);

            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements(elementName))
                {
                    foreach (XElement subElement in element.Elements(subElementName))
                    {
                        string code = subElement.Element("LeveransvillkorKod") != null ? subElement.Element("LeveransvillkorKod").Value : string.Empty;
                        if (code == string.Empty)
                            continue;

                        string trimmedCodeString = code.TrimStart('0');
                        bool trimmedIsDifferent = (code == trimmedCodeString);                      
                        string name = code + " Skapad av import";

                        if (!currentConditions.Any(c => c.Code == code) && !conditions.Any(c => c.Code == code))
                        {
                            DeliveryCondition condition = new DeliveryCondition();
                            condition.Code = code;
                            condition.Name = name;
                            condition.Company = company;
                            conditions.Add(condition);
                        }
                    }
                }

                if (conditions.Count > 0)
                    SaveChanges(entities);

                foreach (var condition in conditions)
                {
                    base.TryDetachEntity(entities, condition);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);
            }
        }

        public void CreateVatCodes(CompEntities entities, string content, int actorCompanyId, string elementName, string subElementName)
        {
            //<Momskod>2</Momskod>

            List<VatCode> codes = new List<VatCode>();
            List<VatCode> currentCodes = accountManager.GetVatCodes(entities, actorCompanyId);
            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);
            int? vatAccountId = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable1, 0, actorCompanyId, 0);

            if (vatAccountId == 0 || vatAccountId == null)
            {
                AccountDTO vatAccount = accountManager.GetAccountStdBySearch(actorCompanyId, "moms", 1).FirstOrDefault();
                if (vatAccount != null)
                    vatAccountId = vatAccount.AccountId;
            }

            if (vatAccountId == null || vatAccountId == 0 || company == null)
                return;

            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements(elementName))
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        string codeString = subElement.Element("Momskod") != null ? subElement.Element("Momskod").Value : string.Empty;
                        if (codeString == string.Empty)
                            continue;

                        string trimmedCodeString = codeString.TrimStart('0');
                        bool foundOnCode = currentCodes.Any(c => c.Code == codeString) || codes.Any(c => c.Code == codeString);
                        bool foundOnTrimmedCode = currentCodes.Any(c => c.Code == trimmedCodeString) || codes.Any(c => c.Code == trimmedCodeString);
                        string name = codeString + " Skapad av import";

                        if (!foundOnCode && !foundOnTrimmedCode)
                        {
                            VatCode code = new VatCode();
                            code.Code = codeString;
                            code.Name = name;
                            code.Company = company;
                            code.AccountId = (int)vatAccountId;
                            code.Created = DateTime.Now;
                            code.CreatedBy = "Skapad av import";
                            codes.Add(code);
                        }
                    }
                }

                if (codes.Count > 0)
                    SaveChanges(entities);

                foreach (var code in codes)
                {
                    base.TryDetachEntity(entities, code);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);
            }
        }

        public void CreatePricelistTypes(CompEntities entities, string content, int actorCompanyId, string elementName, string subElementName)
        {
            //<Standardprislista>1</Standardprislista>

            List<PriceListType> types = new List<PriceListType>();
            List<PriceListType> currentTypes = productPricelistManager.GetPriceListTypes(entities, actorCompanyId);
            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);
            int? currencyId = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CoreBaseCurrency, 0, actorCompanyId, 0);

            if (currencyId == null)
                currencyId = (int)TermGroup_Currency.SEK;

            Currency currency = countryCurrencyManager.GetCurrency(entities, (int)currencyId);

            if (currency == null)
                return;

            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements(elementName))
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        string name = subElement.Element("Standardprislista") != null ? subElement.Element("Standardprislista").Value : string.Empty;

                        if (name == "")
                            continue;

                        name = name + " Skapad av import";

                        if (!currentTypes.Any(c => c.Name == name) && !types.Any(c => c.Name == name))
                        {
                            PriceListType pricelistType = new PriceListType();
                            pricelistType.Name = name;
                            pricelistType.Description = name;
                            pricelistType.Company = company;
                            pricelistType.Currency = currency;

                            types.Add(pricelistType);
                        }
                    }
                }

                if (types.Count > 0)
                    SaveChanges(entities);

                foreach (var type in types)
                {
                    base.TryDetachEntity(entities, type);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);
            }
        }

        public void CreateCustomerCategories(CompEntities entities, string content, int actorCompanyId, string elementName, string subElementName)
        {
            //<KundkategoriKod>01</KundkategoriKod>
            //<KundkategoriBenämning>EJ RABATTKUND</KundkategoriBenämning>

            List<Category> categories = new List<Category>();
            List<Category> currentCategories = CategoryManager.GetCategories(entities, SoeCategoryType.Customer, actorCompanyId);
            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);

            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements(elementName))
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        string code = subElement.Element("KundkategoriKod") != null ? subElement.Element("KundkategoriKod").Value : string.Empty;
                        string name = subElement.Element("KundkategoriBenämning") != null ? subElement.Element("KundkategoriBenämning").Value : string.Empty;

                        if (code == "")
                            continue;

                        if (name == "")
                            name = code;

                        name = name + " Skapad av import";

                        if (!currentCategories.Any(c => c.Code == code) && !categories.Any(c => c.Code == code))
                        {
                            Category category = new Category();
                            category.Code = code;
                            category.Name = name;
                            category.Company = company;
                            category.Type = (int)SoeCategoryType.Customer;
                            categories.Add(category);
                        }
                    }
                }

                if (categories.Count > 0)
                    SaveChanges(entities);

                foreach (var category in categories)
                {
                    base.TryDetachEntity(entities, category);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);
            }
        }

        public void CreateSupplierCategories(CompEntities entities, string content, int actorCompanyId, string elementName, string subElementName)
        {
            //<LeverantörskategoriKod>02</LeverantörskategoriKod>
            //<LeverantörskategoriBenämning>Varuinköp hög moms</LeverantörskategoriBenämning>

            List<Category> categories = new List<Category>();
            List<Category> currentCategories = CategoryManager.GetCategories(entities, SoeCategoryType.Supplier, actorCompanyId);
            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);

            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements(elementName))
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        string code = subElement.Element("LeverantörskategoriKod") != null ? subElement.Element("LeverantörskategoriKod").Value : string.Empty;
                        string name = subElement.Element("LeverantörskategoriBenämning") != null ? subElement.Element("LeverantörskategoriBenämning").Value : string.Empty;

                        if (code == "")
                            continue;

                        if (name == "")
                            name = code;

                        name = name + " Skapad av import";

                        if (!currentCategories.Any(c => c.Code == code) && !categories.Any(c => c.Code == code))
                        {
                            Category category = new Category();
                            category.Code = code;
                            category.Name = name;
                            category.Company = company;
                            category.Type = (int)SoeCategoryType.Supplier;
                            categories.Add(category);
                        }
                    }
                }

                if (categories.Count > 0)
                    SaveChanges(entities);

                foreach (var category in categories)
                {
                    base.TryDetachEntity(entities, category);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);
            }
        }

        public void CreateDeliveryTypes(CompEntities entities, string content, int actorCompanyId, string elementName, string subElementName)
        {
            //<LeveranssättKod>01</LeveranssättKod>

            List<DeliveryType> types = new List<DeliveryType>();
            List<DeliveryType> currentTypes = InvoiceManager.GetDeliveryTypes(entities, actorCompanyId);
            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);

            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements(elementName))
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        string codeString = subElement.Element("LeveranssättKod") != null ? subElement.Element("LeveranssättKod").Value : string.Empty;
                        if (codeString == string.Empty)
                            continue;

                        string code = codeString;
                        string name = codeString + " Skapad av import";

                        if (!currentTypes.Any(c => c.Name == codeString) && !types.Any(c => c.Name == code))
                        {
                            DeliveryType deliveryType = new DeliveryType();
                            deliveryType.Code = code;
                            deliveryType.Name = code;
                            deliveryType.Company = company;
                            deliveryType.Created = DateTime.Now;
                            deliveryType.CreatedBy = "Skapad av import";
                            types.Add(deliveryType);
                        }
                    }
                }
                if (types.Count > 0)
                    SaveChanges(entities);

                foreach (var type in types)
                {
                    base.TryDetachEntity(entities, type);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);
            }
        }

        public string ApplyPeriodkonteringSOP(string content, int actorCompanyId)
        {
            string modifiedContent = string.Empty;

            //Original design
            //<Periodkontering>
            //   <Id>500031</Id>
            //   <Typ>1</Typ>
            //   <VerSerie>1</VerSerie>
            //   <AntalKvar>0</AntalKvar>
            //   <AntalUtförda>0</AntalUtförda>
            //   <Konto>6512</Konto>
            //   <Kställe></Kställe>
            //   <Projekt></Projekt>
            //   <SaldoTyp>0</SaldoTyp>
            //   <BeloppKvar>0</BeloppKvar>
            //   <BeloppUtförda>0</BeloppUtförda>
            //   <StartDatum>2015-01-01</StartDatum>
            //   <SenastUtfördDatum>0</SenastUtfördDatum>
            //   <KontoFördelning></KontoFördelning>
            //   <KställeFördelning></KställeFördelning>
            //   <ProjektFördelning></ProjektFördelning>
            //   <AutoBorttag>0</AutoBorttag>
            //   <PeriodkonteringRad>
            //      <RadNr>01</RadNr>
            //      <RadKontoDebet>1790</RadKontoDebet>
            //      <RadKställeDebet></RadKställeDebet>
            //      <RadProjektDebet></RadProjektDebet>
            //      <RadKontoKredit>6512</RadKontoKredit>
            //      <RadKställeKredit></RadKställeKredit>
            //      <RadProjektKredit></RadProjektKredit>
            //      <RadBeräkKod>00</RadBeräkKod>
            //      <RadSaldo>80000.00</RadSaldo>
            //      <RadKvantitet>0</RadKvantitet>
            //   </PeriodkonteringRad>
            //   <UpplagtDatum>2014-12-19</UpplagtDatum>
            //   <ÄndratDatum>2014-12-19</ÄndratDatum>
            //   <ÄndratTid>141131</ÄndratTid>
            //   <ÄndratAnvändare>CI</ÄndratAnvändare>
            //</Periodkontering>

            XElement xml = null;

            try
            {
                xml = XElement.Parse(content);
            }
            catch
            {
                return modifiedContent;
            }

            List<XElement> periodkonteringar = xml.Elements("Periodkontering").ToList();
            List<XElement> nyaPeriodkonteringar = new List<XElement>();
            DateTime startDatum = DateTime.Today;
            startDatum = startDatum.AddMonths(1);
            DateTime slutDatum;
            Decimal beloppKvar = 0;
            int antKvar = 0;
            Decimal ggrAntalKvar = 0;
            Decimal beloppUtförda = 0;
            Decimal beloppTot = 0;
            Decimal beloppMan = 0;
            int numMonths = 0;
            foreach (XElement periodkontering in periodkonteringar)
            {
               foreach (XElement subElement in periodkontering.Elements())
               {
                //    if (subElement.Name.ToString().ToLower().Equals("startdatum"))
                //        DateTime.TryParse(subElement.Value, out startDatum);
                    if (subElement.Name.ToString().ToLower().Equals("beloppkvar"))
                        Decimal.TryParse(subElement.Value, out beloppKvar);
                    if (subElement.Name.ToString().ToLower().Equals("belopputförda"))
                        Decimal.TryParse(subElement.Value, out beloppUtförda);
                    if (subElement.Name.ToString().ToLower().Equals("antalkvar"))
                        Decimal.TryParse(subElement.Value, out ggrAntalKvar);
                }

                beloppTot = beloppKvar + beloppUtförda;

                List<XElement> periodkonteringRader = periodkontering.Elements("PeriodkonteringRad").ToList();
                List<XElement> nyaPeriodkonteringRader = periodkonteringRader;
                XElement nyPeriodkontering = periodkontering;
 
                foreach (var periodkonteringRad in periodkonteringRader)
                {
                    nyPeriodkontering.Element("PeriodkonteringRad").Remove();
                    foreach (XElement subElement in periodkonteringRad.Elements())
                    {
                        if (subElement.Name.ToString().ToLower().Equals("radsaldo"))
                            beloppMan = NumberUtility.ToDecimal(subElement.Value, 2);
                    }
                    if (beloppMan > 0 && beloppTot>0)
                    {
                        numMonths = Convert.ToInt32(beloppTot / beloppMan);
                        antKvar = Convert.ToInt32(beloppKvar / beloppMan);
                        slutDatum = startDatum.AddMonths(numMonths);
                        XElement slutDat = new XElement("SlutDatum", slutDatum.ToShortDateString());
                        XElement antalKvar = new XElement("AntalKvar", antKvar);
                        nyPeriodkontering.Element("AntalKvar").Remove();
                        nyPeriodkontering.Add(slutDat);
                        nyPeriodkontering.Add(antalKvar);
                    }
                    else
                    {
                        XElement slutDat = new XElement("SlutDatum", string.Empty);
                        nyPeriodkontering.Add(slutDat);
                    }
                    

                    XElement rad = new XElement("PeriodkonteringRad",
                        new XElement("RadNr", periodkonteringRad.Element("RadNr").Value != null ? periodkonteringRad.Element("RadNr").Value : string.Empty),
                        new XElement("RadKonto", periodkonteringRad.Element("RadKontoDebet").Value != null ? periodkonteringRad.Element("RadKontoDebet").Value : string.Empty),
                        new XElement("RadKställe", periodkonteringRad.Element("RadKställeDebet").Value != null ? periodkonteringRad.Element("RadKställeDebet").Value : string.Empty),
                        new XElement("RadProjekt", periodkonteringRad.Element("RadProjektDebet").Value != null ? periodkonteringRad.Element("RadProjektDebet").Value : string.Empty),
                        new XElement("RadBeräkKod", periodkonteringRad.Element("RadBeräkKod").Value != null ? periodkonteringRad.Element("RadBeräkKod").Value : string.Empty),
                        new XElement("RadSaldo", periodkonteringRad.Element("RadSaldo").Value != null ? periodkonteringRad.Element("RadSaldo").Value : string.Empty),
                        new XElement("RadKvantitet", periodkonteringRad.Element("RadKvantitet").Value != null ? periodkonteringRad.Element("RadKvantitet").Value : string.Empty));

                    nyPeriodkontering.Add(rad);

                    XElement rad2 = new XElement("PeriodkonteringRad",
                        new XElement("RadNr", periodkonteringRad.Element("RadNr").Value != null ? periodkonteringRad.Element("RadNr").Value : string.Empty),
                        new XElement("RadKonto", periodkonteringRad.Element("RadKontoKredit").Value != null ? periodkonteringRad.Element("RadKontoKredit").Value : string.Empty),
                        new XElement("RadKställe", periodkonteringRad.Element("RadKställeKredit").Value != null ? periodkonteringRad.Element("RadKställeKredit").Value : string.Empty),
                        new XElement("RadProjekt", periodkonteringRad.Element("RadProjektKredit").Value != null ? periodkonteringRad.Element("RadProjektKredit").Value : string.Empty),
                        new XElement("RadBeräkKod", periodkonteringRad.Element("RadBeräkKod").Value != null ? periodkonteringRad.Element("RadBeräkKod").Value : string.Empty),
                        new XElement("RadSaldoOmvänt", periodkonteringRad.Element("RadSaldo").Value != null ? periodkonteringRad.Element("RadSaldo").Value : string.Empty),
                        new XElement("RadKvantitet", periodkonteringRad.Element("RadKvantitet").Value != null ? periodkonteringRad.Element("RadKvantitet").Value : string.Empty));

                    nyPeriodkontering.Add(rad2);
                }
                if (ggrAntalKvar > 0)
                {
                    nyPeriodkontering.Element("AntalKvar").Remove();
                    XElement antalKvar = new XElement("AntalKvar", ggrAntalKvar);
                    nyPeriodkontering.Add(antalKvar);
                    nyPeriodkontering.Element("StartDatum").Remove();
                    XElement startDat = new XElement("StartDatum", startDatum.ToShortDateString());
                    nyPeriodkontering.Add(startDat);
                    nyPeriodkontering.Element("SlutDatum").Remove();
                    slutDatum = startDatum.AddMonths((int)ggrAntalKvar);
                    XElement slutDat = new XElement("SlutDatum", slutDatum.ToShortDateString());
                    nyPeriodkontering.Add(slutDat);
                }
                nyaPeriodkonteringar.Add(nyPeriodkontering);
            }

            XElement nyaPeriodkonteringarRoot = new XElement("periodkonteringar");

            nyaPeriodkonteringarRoot.Add(nyaPeriodkonteringar);

            modifiedContent = nyaPeriodkonteringarRoot.ToString();

            return modifiedContent;
        }

        public int GetBaseAccount(List<Account> accounts, string number)
        {
            Account account = null;
            if (!string.IsNullOrEmpty(number))
                account = accounts.FirstOrDefault(a => a.AccountNr == number) ?? accounts.FirstOrDefault(a => a.Name == number);
            return account?.AccountId ?? 0;
        }

        public int SetBaseAccountSettingTypeSOP(string name)
        {

            int settingTypeId = 0;

            if (name == "FörsäljningMomspliktig") settingTypeId = (int)CompanySettingType.AccountCustomerSalesVat;
            else if (name == "FörsäljningMomsfri") settingTypeId = (int)CompanySettingType.AccountCustomerSalesNoVat;
            else if (name == "Fraktavgift") settingTypeId = (int)CompanySettingType.AccountCustomerFreight;
            else if (name == "Expeditionavgift") settingTypeId = (int)CompanySettingType.AccountCustomerOrderFee;
            else if (name == "Försäkringavgift") settingTypeId = (int)CompanySettingType.AccountCustomerInsurance;
            else if (name == "Dröjsmålsränta") settingTypeId = (int)CompanySettingType.AccountCustomerPenaltyInterest;
            else if (name == "Kundfordran") settingTypeId = (int)CompanySettingType.AccountCustomerClaim;
            //else if (name == "KundKassarabatt") settingTypeId = 999;
            //else if (name == "KundDifferenskonto") settingTypeId = 999;
            //else if (name == "KundAconto") settingTypeId = 999;
            else if (name == "UtgåendeMoms1") settingTypeId = (int)CompanySettingType.AccountCommonVatPayable1;
            else if (name == "UtgåendeMoms2") settingTypeId = (int)CompanySettingType.AccountCommonVatPayable2;
            else if (name == "UtgåendeMoms3") settingTypeId = (int)CompanySettingType.AccountCommonVatPayable3;
            //else if (name == "UtgåendeMoms4") settingTypeId = 999;
            //else if (name == "UtgåendeMoms5") settingTypeId = 999;
            //else if (name == "UtgåendeMoms6") settingTypeId = 999;
            else if (name == "IngåendeMoms") settingTypeId = (int)CompanySettingType.AccountCommonVatReceivable;
            else if (name == "Kassa") settingTypeId = (int)CompanySettingType.AccountCommonCheck;
            else if (name == "Post") settingTypeId = (int)CompanySettingType.AccountCommonPG;
            else if (name == "Bank") settingTypeId = (int)CompanySettingType.AccountCommonBG;
            else if (name == "Autogiro") settingTypeId = (int)CompanySettingType.AccountCommonAG;
            else if (name == "Öresavrundning") settingTypeId = (int)CompanySettingType.AccountCommonCentRounding;
            else if (name == "Valutavinst") settingTypeId = (int)CompanySettingType.AccountCommonCurrencyProfit;
            else if (name == "Valutaförlust") settingTypeId = (int)CompanySettingType.AccountCommonCurrencyLoss;
            else if (name == "Inköp") settingTypeId = (int)CompanySettingType.AccountSupplierPurchase;
            else if (name == "Levskuld") settingTypeId = (int)CompanySettingType.AccountSupplierDebt;
            else if (name == "Interims") settingTypeId = (int)CompanySettingType.AccountSupplierInterim;
            //else if (name == "LevKassarabatt") settingTypeId = 999;
            else if (name == "LevDifferenskonto") settingTypeId = (int)CompanySettingType.AccountCommonDiff;
            //else if (name == "LevAconto") settingTypeId = 999;
            //else if (name == "UtLager") settingTypeId = 999;
            //else if (name == "UtLagerförändring") settingTypeId = 999;
            //else if (name == "InLager") settingTypeId = 999;
            //else if (name == "InLagerförändring") settingTypeId = 999;
            //else if (name == "InventLager") settingTypeId = 999;
            //else if (name == "InventLagerFörändring") settingTypeId = 999;
            else if (name == "Inventarie") settingTypeId = (int)CompanySettingType.AccountInventoryInventories;
            else if (name == "AvskrivningDebet") settingTypeId = (int)CompanySettingType.AccountInventoryAccWriteOff;
            else if (name == "AvskrivningKredit") settingTypeId = (int)CompanySettingType.AccountInventoryWriteOff;
            else if (name == "ÖverAvskrivningDebet") settingTypeId = (int)CompanySettingType.AccountInventoryAccOverWriteOff;
            else if (name == "ÖverAvskrivningKredit") settingTypeId = (int)CompanySettingType.AccountInventoryOverWriteOff;
            else if (name == "Kravavgift1") settingTypeId = (int)CompanySettingType.AccountCustomerClaimCharge;
            //else if (name == "Kravavgift2") settingTypeId = 999;
            //else if (name == "Kravavgift3") settingTypeId = 999;
            //else if (name == "KassationLager") settingTypeId = 999;
            //else if (name == "KassationLagerförändring") settingTypeId = 999;
            //else if (name == "KundRadrabatt") settingTypeId = 999;
            //else if (name == "KundRabatt") settingTypeId = 999;
            //else if (name == "KundMotkontoRabatt") settingTypeId = 999;
            else if (name == "UtgåendeMoms1Omvänd") settingTypeId = (int)CompanySettingType.AccountCommonVatPayable1Reversed;
            else if (name == "UtgåendeMoms2Omvänd") settingTypeId = (int)CompanySettingType.AccountCommonVatPayable2Reversed;
            else if (name == "UtgåendeMoms3Omvänd") settingTypeId = (int)CompanySettingType.AccountCommonVatPayable3Reversed;
            //else if (name == "UtgåendeMoms4Omvänd") settingTypeId = 999;
            //else if (name == "UtgåendeMoms5Omvänd") settingTypeId = 999;
            //else if (name == "UtgåendeMoms6Omvänd") settingTypeId = 999;
            else if (name == "IngåendeMomsOmvänd") settingTypeId = (int)CompanySettingType.AccountCommonVatReceivableReversed;
            else if (name == "KundÖverbetalning") settingTypeId = (int)CompanySettingType.AccountCustomerOverpay;
            else if (name == "LevÖverbetalning") settingTypeId = (int)CompanySettingType.AccountSupplierOverpay;
            //else if (name == "InterimskontoOmvänd") settingTypeId = 999;

            return settingTypeId;
        }

        public void CreateVatCodesSOP(CompEntities entities, int actorCompanyId)
        {
            try
            {
                List<VatCode> currentVatCodes = accountManager.GetVatCodes(entities, actorCompanyId);

                if (company == null)
                    company = companyManager.GetCompany(entities, actorCompanyId);

                var defaultVatAccount1Id = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable1, 0, actorCompanyId, 0);
                var defaultVatAccount2Id = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable2, 0, actorCompanyId, 0);
                var defaultVatAccount3Id = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatPayable3, 0, actorCompanyId, 0);
                var defaultVatReceivableAccountId = settingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatReceivable, 0, actorCompanyId, 0);

                if (defaultVatAccount1Id != 0 && !currentVatCodes.Any(v => v.Code.ToLower() == "1" || v.Code == "01"))
                {
                    VatCode code1 = new VatCode();
                    code1.Code = "1";
                    code1.Name = "Moms 1";
                    code1.Company = company;
                    code1.AccountId = defaultVatAccount1Id;
                    code1.PurchaseVATAccountId = defaultVatReceivableAccountId != 0 ? (int?)defaultVatReceivableAccountId : null;
                    code1.Created = DateTime.Now;
                    code1.CreatedBy = "Skapad av import";

                    var view = AccountManager.GetAccountVatRate(entities, defaultVatAccount1Id, actorCompanyId);
                    if (view != null && view.VatRate != null)
                        code1.Percent = (decimal)view.VatRate;

                    entities.VatCode.AddObject(code1);
                }

                if (defaultVatAccount2Id != 0 && !currentVatCodes.Any(v => v.Code.ToLower() == "2" || v.Code == "02"))
                {
                    VatCode code2 = new VatCode();
                    code2.Code = "2";
                    code2.Name = "Moms 2";
                    code2.Company = company;
                    code2.AccountId = defaultVatAccount2Id;
                    code2.PurchaseVATAccountId = defaultVatReceivableAccountId != 0 ? (int?)defaultVatReceivableAccountId : null;
                    code2.Created = DateTime.Now;
                    code2.CreatedBy = "Skapad av import";

                    var view = AccountManager.GetAccountVatRate(entities, defaultVatAccount2Id, actorCompanyId);
                    if (view != null && view.VatRate != null)
                        code2.Percent = (decimal)view.VatRate;

                    entities.VatCode.AddObject(code2);
                }

                if (defaultVatAccount3Id != 0 && !currentVatCodes.Any(v => v.Code.ToLower() == "3" || v.Code == "03"))
                {
                    VatCode code3 = new VatCode();
                    code3.Code = "3";
                    code3.Name = "Moms 3";
                    code3.Company = company;
                    code3.AccountId = defaultVatAccount3Id;
                    code3.PurchaseVATAccountId = defaultVatReceivableAccountId != 0 ? (int?)defaultVatReceivableAccountId : null;
                    code3.Created = DateTime.Now;
                    code3.CreatedBy = "Skapad av import";

                    var view = AccountManager.GetAccountVatRate(entities, defaultVatAccount3Id, actorCompanyId);
                    if (view != null && view.VatRate != null)
                        code3.Percent = (decimal)view.VatRate;

                    entities.VatCode.AddObject(code3);
                }

                entities.SaveChanges();
            }
            catch (Exception ex)
            {
                LogError("Failed to save VatCodes from SOP " + ex.InnerException);
            }

        }

        public ActionResult LastControlAfterMigration(int actorCompanyId)
        {
            return CreatePaymentIOsForPaidInvoicesWithoutPayments(actorCompanyId);
        }

        public ActionResult CreatePaymentIOsForPaidInvoicesWithoutPayments(int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            DateTime now = DateTime.Now;

            List<Invoice> invoices = new List<Invoice>();
            List<Invoice> allInvoices = InvoiceManager.GetInvoices(actorCompanyId, SoeOriginType.CustomerInvoice);
            List<Invoice> payedinvoices = allInvoices.Where(i => i.FullyPayed == true && i.InvoiceDate > now.AddYears(-3)).ToList();
            List<Invoice> oldinvoices = allInvoices.Where(i => i.InvoiceDate < now.AddYears(-3)).ToList();

            invoices.AddRange(payedinvoices);
            invoices.AddRange(oldinvoices);

            List<PaymentRowImportIODTO> dtos = new List<PaymentRowImportIODTO>();

            foreach (var invoice in invoices)
            {
                var payments = invoice.PaymentRow != null ? invoice.PaymentRow.ToList() : new List<PaymentRow>();
                if (payments.Count > 0)
                    continue;

                PaymentRowImportIODTO dto = new PaymentRowImportIODTO();
                dto.Amount = invoice.TotalAmount;
                dto.PayDate = invoice.DueDate.HasValue ? (DateTime)invoice.DueDate : CalendarUtility.DATETIME_DEFAULT;
                dto.CurrencyDate = invoice.CurrencyDate;
                dto.InvoiceNr = invoice.InvoiceNr;
                dto.InvoiceSeqNr = invoice.SeqNr;
                dto.SysPaymentTypeId = (int)TermGroup_SysPaymentType.BG;
                dto.AmountCurrency = invoice.TotalAmountCurrency;
                dto.CurrencyRate = invoice.CurrencyRate;
                dto.VoucherDate = dto.PayDate;
                dto.PaymentMethodCode = "bankgiro";
                dto.Comment = "created by fix " + now.ToString();
                dto.Type = (int)SoeOriginType.CustomerPayment;
                dto.ChangeFullyPaid = false;
                dto.FullyPaid = true;
                dtos.Add(dto);
            }

            return ImportExportManager.ImportPaymentFromIO(dtos, actorCompanyId, TermGroup_IOType.Unknown, true);
        }

        public ActionResult CheckVatCodes(CompEntities entities, string content, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            decimal vat1 = 0;
            decimal vat2 = 0;
            decimal vat3 = 0;

            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);
            if (company == null)
            {
                result.Success = false;
                result.ErrorMessage = "No Company found";
                return result;
            }

            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements("Företag"))
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        if (subElement.Name.ToString().ToLower().Equals("moms1"))
                            decimal.TryParse(subElement.Value, out vat1);

                        if (subElement.Name.ToString().ToLower().Equals("moms2"))
                            decimal.TryParse(subElement.Value, out vat2);

                        if (subElement.Name.ToString().ToLower().Equals("moms3"))
                            decimal.TryParse(subElement.Value, out vat3);
                    }
                }

                if (vat1 + vat2 + vat3 > 0)
                {
                    var vatCodes = AccountManager.GetVatCodes(entities, actorCompanyId);

                    foreach (var vatcode in vatCodes)
                    {
                        if (vatcode.Code == "1" && vatcode.Percent == 0)
                            vatcode.Percent = vat1;

                        if (vatcode.Code == "2" && vatcode.Percent == 0)
                            vatcode.Percent = vat2;

                        if (vatcode.Code == "3" && vatcode.Percent == 0)
                            vatcode.Percent = vat3;
                    }

                    SaveChanges(entities);

                }
            }
            catch(Exception ex)
            {
                LogError(ex, null);
            }

            return result;
        }

        public ActionResult CreateSettingsFromSOP(CompEntities entities, string content, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);
            if (company == null)
            {
                result.Success = false;
                result.ErrorMessage = "No Company found";
                return result;
            }
            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements("Företag"))
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        int settingTypeId = 0;
                        SettingDataType dataType;
                        bool? boolData = null;
                        int? intData = null;
                        decimal? decimalData = null;
                        string stringData = null;
                        DateTime? dateData = null;
                        string value = subElement.Value;
                        bool saved = false;

                        try
                        {
                            settingTypeId = SetCompanySettingTypeSOP(entities, actorCompanyId, subElement.Name.ToString(), value, out saved, out dataType, out boolData, out intData, out decimalData, out stringData, out dateData);
                        }
                        catch
                        {
                            continue;
                        }

                        if (settingTypeId == 0 || saved)
                            continue;

                        UserCompanySetting setting = settingManager.GetUserCompanySetting(entities, SettingMainType.Company, settingTypeId, 0, actorCompanyId, 0);

                        if (setting != null)
                        {
                            switch (dataType)
                            {
                                case SettingDataType.Boolean: setting.DataTypeId = (int)SettingDataType.Boolean; setting.BoolData = boolData; break;
                                case SettingDataType.Decimal: setting.DataTypeId = (int)SettingDataType.Decimal; setting.DecimalData = decimalData; break;
                                case SettingDataType.Integer: setting.DataTypeId = (int)SettingDataType.Integer; setting.IntData = intData; break;
                                case SettingDataType.Date: setting.DataTypeId = (int)SettingDataType.Date; setting.DateData = dateData; break;
                                case SettingDataType.String: setting.DataTypeId = (int)SettingDataType.String; setting.StrData = stringData; break;
                                case SettingDataType.Time: setting.DataTypeId = (int)SettingDataType.Time; setting.DateData = dateData; break;
                            }
                        }
                        else
                        {
                            #region Add

                            setting = new UserCompanySetting();

                            switch (dataType)
                            {
                                case SettingDataType.Boolean: setting.DataTypeId = (int)SettingDataType.Boolean; setting.BoolData = boolData; setting.DataTypeId = (int)SettingDataType.Boolean; break;
                                case SettingDataType.Decimal: setting.DataTypeId = (int)SettingDataType.Decimal; setting.DecimalData = decimalData; setting.DataTypeId = (int)SettingDataType.Integer; break;
                                case SettingDataType.Integer: setting.DataTypeId = (int)SettingDataType.Integer; setting.IntData = intData; setting.DataTypeId = (int)SettingDataType.Integer; break;
                                case SettingDataType.Date: setting.DataTypeId = (int)SettingDataType.Date; setting.DateData = dateData; setting.DataTypeId = (int)SettingDataType.Date; break;
                                case SettingDataType.String: setting.DataTypeId = (int)SettingDataType.String; setting.StrData = stringData; setting.DataTypeId = (int)SettingDataType.String; break;
                                case SettingDataType.Time: setting.DataTypeId = (int)SettingDataType.Time; setting.DateData = dateData; setting.DataTypeId = (int)SettingDataType.Time; break;
                            }

                            setting.SettingTypeId = settingTypeId;
                            setting.ActorCompanyId = actorCompanyId;

                            SetCreatedProperties(setting);
                            entities.UserCompanySetting.AddObject(setting);

                            #endregion
                        }

                        result = SaveChanges(entities);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);

                result.Success = false;
                return result;
            }

            return result;

        }

        public void CreateCurrency(CompEntities entities, string content, int actorCompanyId, string elementName, string subElementName)
        {
            //<Valutakod></Valutakod>

            List<Currency> currencies = new List<Currency>();
            List<Currency> currentCurrencies = CountryCurrencyManager.GetCurrencies(entities, actorCompanyId);
            Company company = companyManager.GetCompany(entities, actorCompanyId, false, false);

            if (company == null)
                return;
            try
            {
                XDocument doc = XDocument.Parse(content);

                foreach (XElement element in doc.Elements(elementName))
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        string codeString = subElement.Element("Valutakod") != null ? subElement.Element("Valutakod").Value : string.Empty;
                        string name = string.Empty;

                        if (codeString == "")
                            continue;

                        name = codeString + " Skapad av import";

                        if (!currentCurrencies.Any(c => c.Code == codeString) && !currencies.Any(c => c.Code == codeString))
                        {
                            int? sysCurrencyId = null;

                            //TODO: Ugly as shit, fix later, need new view/SP?
                            if (CountryCurrencyManager.GetCurrencies(entities, 17).Any(c => c.Code == codeString))
                                sysCurrencyId = CountryCurrencyManager.GetCurrencies(entities, 17).FirstOrDefault(c => c.Code == codeString)?.SysCurrencyId;

                            if (sysCurrencyId == null)
                                continue;

                            Currency currency = new Currency();
                            currency.Code = codeString;
                            currency.Name = name;
                            currency.SysCurrencyId = (int)sysCurrencyId;
                            currency.Company = company;
                            currency.Created = DateTime.Now;
                            currency.CreatedBy = "Skapad av import";
                            currencies.Add(currency);
                        }
                    }
                }

                if (currencies.Count > 0)
                    SaveChanges(entities);

                foreach (var currency in currencies)
                {
                    base.TryDetachEntity(entities, currency);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.InnerException, this.log);
            }
        }

        #region HelpMethods

        private XDocument GetXDocumentFromDataSet(DataSet dataSet)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8) { Formatting = Formatting.None })
                {
                    dataSet.WriteXml(xmlTextWriter);
                    memoryStream.Position = 0;
                    var xmlReader = XmlReader.Create(memoryStream);
                    xmlReader.MoveToContent();
                    return XDocument.Load(xmlReader);
                }
            }
        }

        public static int? GetSysVatAccountFromSOPCode(string value, List<SysVatAccount> sysVatAccounts)
        {
            string newValue = string.Empty;

            int? sysVatAccountId = null;

            if (value == "SP") newValue = "5";
            if (value == "SU") newValue = "6";
            if (value == "SM") newValue = "7";
            if (value == "FF") newValue = "8";
            if (value == "U1") newValue = "10";
            if (value == "U2") newValue = "11";
            if (value == "U3") newValue = "12";
            if (value == "EI") newValue = "20";
            if (value == "TU") newValue = "21";
            if (value == "IU") newValue = "22";
            if (value == "IV") newValue = "23";
            if (value == "IT") newValue = "24";
            if (value == "U4") newValue = "30";
            if (value == "U5") newValue = "31";
            if (value == "U6") newValue = "32";
            if (value == "EE") newValue = "35";
            if (value == "EX") newValue = "36";
            if (value == "TF") newValue = "37";
            if (value == "FV") newValue = "38";
            if (value == "FT") newValue = "39";
            if (value == "ET") newValue = "40";
            if (value == "FS") newValue = "41";
            if (value == "SF") newValue = "42";
            if (value == "MI") newValue = "48";
            if (value == "BI") newValue = "50";
            if (value == "B1") newValue = "60";
            if (value == "B2") newValue = "61";
            if (value == "B3") newValue = "62";
            if (value == "BA") newValue = "50";
            if (value == "BC") newValue = "50";
            if (value == "BB") newValue = "50";
            if (value == "BL") newValue = "50";
            if (value == "FA") newValue = "51";
            if (value == "FC") newValue = "51";
            if (value == "FB") newValue = "51";
            if (value == "FL") newValue = "51";
            if (value == "KL") newValue = "52";
            if (value == "VL") newValue = "81";
            if (value == "AS") newValue = "82";
            if (value == "VP") newValue = "83";
            if (value == "AP") newValue = "84";
            if (value == "VR") newValue = "85";
            if (value == "AR") newValue = "86";

            if (sysVatAccounts.Any(a => a.VatNr1.ToString() == newValue))
                sysVatAccountId = sysVatAccounts.FirstOrDefault(a => a.VatNr1.ToString() == newValue)?.SysVatAccountId;

            return sysVatAccountId;
        }

        #endregion
    }
}
