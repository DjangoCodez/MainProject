using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Import;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class ExcelImportManager : ImportExportManager
    {
        #region Constants

        //Used to find the correct sheet
        private const string EXCELIMPORT_TABLENAME_SUPPLIERS = "leverantörer$";
        private const string EXCELIMPORT_TABLENAME_CUSTOMERS = "kunder$";
        private const string EXCELIMPORT_TABLENAME_CONTACTPERSONS = "kontaktpersoner$";
        private const string EXCELIMPORT_TABLENAME_PRODUCTS = "artiklar$";
        private const string EXCELIMPORT_TABLENAME_EMPLOYEES = "anställda$";
        private const string EXCELIMPORT_TABLENAME_CUSOMTERCATEGORIES = "kundkategori$";
        private const string EXCELIMPORT_TABLENAME_PRODUCTGROUPS = "artikel$";
        private const string EXCELIMPORT_TABLENAME_PAYROLLSTARTVALUES = "startvärden$";
        private const string EXCELIMPORT_TABLENAME_ACCOUNTS = "accounts$";
        private const string EXCELIMPORT_TABLENAME_TAXDEDUCTIONCONTACTS = "skattereduktionskontakter$";
        private const string EXCELIMPORT_TABLENAME_PRICELISTS = "prislistor$";
        private const string EXCELIMPORT_TABLENAME_AVTALS = "avtals$";
		private static string EXCELIMPORT_TABLENAME_COMMODITYCODES => $"{DateTime.Today.ToString("yyyy")}$";


		private readonly List<string> EXCELIMPORT_TABLENAMES = new List<string>()
        {
            EXCELIMPORT_TABLENAME_SUPPLIERS,
            EXCELIMPORT_TABLENAME_CUSTOMERS,
            EXCELIMPORT_TABLENAME_CONTACTPERSONS,
            EXCELIMPORT_TABLENAME_PRODUCTS,
            EXCELIMPORT_TABLENAME_EMPLOYEES,
            EXCELIMPORT_TABLENAME_CUSOMTERCATEGORIES,
            EXCELIMPORT_TABLENAME_PRODUCTGROUPS,
            EXCELIMPORT_TABLENAME_PAYROLLSTARTVALUES,
            EXCELIMPORT_TABLENAME_ACCOUNTS,
            EXCELIMPORT_TABLENAME_TAXDEDUCTIONCONTACTS,
            EXCELIMPORT_TABLENAME_PRICELISTS,
            EXCELIMPORT_TABLENAME_AVTALS,
			EXCELIMPORT_TABLENAME_COMMODITYCODES
		};

        //Fields
        private const string CATEGORY_FIELD_NAME = "CategoryCode";
        private const string SECONDARYCATEGORY_FIELD_NAME = "SecondaryCategoryCode";
        private const string ACCOUNTSTD_FIELD_NAME = "AccountStd";
        private const string ACCOUNTINTERNAL_FIELD_NAME = "AccountInternal";

        //AccountInternals
        const int SUPPLIER_NO_OF_ACCOUNTINTERNALS = 5;
        const int CUSTOMER_NO_OF_ACCOUNTINTERNALS = 5;
        const int PRODUCTS_NO_OF_ACCOUNTINTERNALS = 5;

        //Categories
        const int CUSTOMER_NO_OF_CATEGORIES = 5;
        const int PRODUCTS_NO_OF_CATEGORIES = 5;

        #endregion

        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Translation
        private Dictionary<string, string> translationDict;

        #endregion

        #region Ctor

        public ExcelImportManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        #region Entry point

        public ActionResult ImportCommodityCodes(string pathOnServer, DateTime startDateOfYear, string extention)
        {
            DataSet ds;

            try
            {
                if (extention == ".csv")
                {
                    ds = CreateDataSet(pathOnServer, null, true);
                }
                else
                {
                    string provider = GetProvider(pathOnServer);
                    if (String.IsNullOrEmpty(provider))
                        return new ActionResult(false, 0, GetFileNotSupportedMessage());

                    var connectionStringBuilder = GetConnectionString(provider, pathOnServer);
                    if (connectionStringBuilder == null)
                        return new ActionResult(false, 0, GetFileNotSupportedMessage());

                    ds = CreateDataSet(pathOnServer, connectionStringBuilder, false);
                }
                return ImportCommodityCodes(ds, startDateOfYear);
            }
            catch (Exception ex)
            {
                AddError(ErrorMessageType.UnkownError, exception: ex);
                return new ActionResult(false, 0, GetText(4683, "Import misslyckades."));
            }
        }
        public ActionResult ImportCommodityCodes(DataSet ds, DateTime startDateOfYear)
        {
            if (ds != null)
            {
                DateTime lastDateOfPreviousYear = new DateTime((startDateOfYear.Year - 1), 12, 31);
                List<DataRow> rows = ds.Tables[0].Rows.Cast<DataRow>().ToList();
                if (rows == null || rows.Count == 0)
                    return new ActionResult(false, 0, GetFileIsEmptyMessage());

                int headerRowIndex = 0;
                string codeHeader = "";

                //Check first & second rows to get the column name
				for (headerRowIndex = 0; headerRowIndex < 2; headerRowIndex++)
                {
					codeHeader = rows[headerRowIndex][0].ToString().Trim();

                    if (!string.IsNullOrWhiteSpace(codeHeader)) break;
				}

                if (string.IsNullOrWhiteSpace(codeHeader) || !(codeHeader.ToLower() == "cn8code" || codeHeader.ToLower() == "varukod"))
                {
                    return new ActionResult(false, 0, GetText(516, "Filen innehåller felaktiga rubriker eller saknar data."));
                }

                var fileLang = "en-US";
                switch (codeHeader.ToLower())
                {
                    case "cn8code":
                        fileLang = "en-US";
                        break;
                    case "varukod":
                        fileLang = "sv-SE";
                        break;
                    default:
                        break;
                }
                try
                {
                    using (var entities = new SOESysEntities())
                    {
                        var sysLanguageId = (from l in entities.SysLanguage
                                             where l.LangCode == fileLang
                                             select l.SysLanguageId).FirstOrDefault();


                        var existingCodes = (from ic in entities.SysIntrastatCode.Include("SysIntrastatText")
                                             where ic.State == (int)SoeEntityState.Active
                                             select ic).ToList();

                        List<SysIntrastatCode> newCodes = new List<SysIntrastatCode>();
                        List<string> uploadedCodes = new List<string>();

						foreach (DataRow row in rows.Skip(headerRowIndex + 1))
						{
							try
							{
								var code = row[0].ToString().Trim();

								//only 8 digit codes is possible to import to SCB
								if (code.Length != 8)
								{
									continue;
								}

								uploadedCodes.Add(code);
								var commodityCode = existingCodes.Where(x => x.Code == code).Select(s => s).FirstOrDefault();
								if (commodityCode == null)
								{
									//new intrastat code
									var newCode = new SysIntrastatCode()
									{
										SysIntrastatCodeId = 0,
										Code = code,
										StartDate = startDateOfYear,
										EndDate = null,
										UseOtherQualifier = (row[2].ToString().Replace('-', ' ').Trim().Length > 0),

									};
									newCode.SysIntrastatText = new List<SysIntrastatText>();
									newCode.SysIntrastatText.Add(new SysIntrastatText()
									{
										SysIntrastatCodeId = 0,
										SysIntrastatTextId = 0,
										SysLanguageId = sysLanguageId,
										Text = row[1].ToString().Trim()

									});

									CommodityCodeManager.AddCommodityCode(entities, newCode);
									newCodes.Add(newCode);
								}
								else
								{
									commodityCode.EndDate = null;
									commodityCode.UseOtherQualifier = row.ItemArray.Length > 2 && (row[2].ToString().Replace('-', ' ').Trim().Length > 0);
									if (commodityCode.SysIntrastatText == null)
									{
										commodityCode.SysIntrastatText = new List<SysIntrastatText>()
											{
												new SysIntrastatText()
												{
													SysIntrastatCodeId = 0,
													SysIntrastatTextId = 0,
													SysLanguageId = sysLanguageId,
													Text = row[1].ToString().Trim()
												}
											};
									}
									else
									{
										if (commodityCode.SysIntrastatText.Any(a => a.SysLanguageId == sysLanguageId))
										{
											var textUpdated = commodityCode.SysIntrastatText.Where(a => a.SysLanguageId == sysLanguageId).Select(s => s).FirstOrDefault();
											if (textUpdated != null)
											{
												textUpdated.Text = row[1].ToString().Trim();
											}
										}
										else
										{
											commodityCode.SysIntrastatText.Add(new SysIntrastatText()
											{
												SysIntrastatCodeId = commodityCode.SysIntrastatCodeId,
												SysIntrastatTextId = 0,
												SysLanguageId = sysLanguageId,
												Text = row[1].ToString().Trim()

											});
										}
									}
									CommodityCodeManager.UpdateCommodityCode(entities, commodityCode);
								}

							}
							catch (ArgumentException ax)
							{
								//Break import
								AddTerminationError(TerminationMessageType.FieldMissing, exception: ax);
								return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());
							}
						}

						foreach (var existingCode in existingCodes)
						{
							if (!uploadedCodes.Any(a => a == existingCode.Code) && existingCode.EndDate == null)
								existingCode.EndDate = lastDateOfPreviousYear;
						}

						entities.SaveChanges();
					}
				}
                catch (Exception ex)
                {
                    AddError(ErrorMessageType.UnkownError, exception: ex);
                    return new ActionResult(false, 0, GetImportFailedMessage());
                }
            }
            else
            {
                return new ActionResult(false, 0, GetText(4683, "Import misslyckades."));
            }
            return new ActionResult(true, 0, GetText(515, "Importen slutförd."));
        }

        public ActionResult Import(string pathOnServer, int actorCompanyId, Dictionary<string, string> parameters = null, bool doNotModifyWithEmpty = false)
        {
            #region Init

            this.langId = GetLangId();

            string provider = GetProvider(pathOnServer);
            if (String.IsNullOrEmpty(provider))
                return new ActionResult(false, 0, GetFileNotSupportedMessage());

            var connectionStringBuilder = GetConnectionString(provider, pathOnServer);
            if (connectionStringBuilder == null)
                return new ActionResult(false, 0, GetFileNotSupportedMessage());

            #endregion

            //Find if the excel contains a valid sheet
            foreach (string tableName in EXCELIMPORT_TABLENAMES)
            {
                //Try import
                DataSet ds = CreateDataSet(pathOnServer, connectionStringBuilder, tableName);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                    return ImportExcelContent(ds, tableName, actorCompanyId, parameters, doNotModifyWithEmpty);
            }

            return new ActionResult(false, 0, GetFileCannotBeReadMessage());
        }

        public ActionResult ImportExcelFromAngular(ExcelImportDTO model)
        {
            ActionResult result = new ActionResult();
            string pathOnServer = String.Empty;
            try
            {
                //Validate
                string fileName = ValidatePostedFile(model.Filename, true);

                //Save temp-file
                pathOnServer = SaveTempFileToServer(model.Bytes, fileName);

                //Import
                result = Import(pathOnServer, base.ActorCompanyId, doNotModifyWithEmpty: model.DoNotUpdateWithEmptyValues);
                if (!result.Success)
                {
                    string errorMessage = "";
                    switch (result.ErrorNumber)
                    {
                        case (int)ActionResultSave.UserCannotBeAddedLicenseViolation:
                            errorMessage = String.Format(GetText(2055, "Alla användare kunde inte importeras, licensen tillåter inte fler användare. Max {0} st"), result.IntegerValue);
                            break;
                        case (int)ActionResultSave.EmployeeCannotBeAddedLicenseViolation:
                            errorMessage = String.Format(GetText(5766, "Alla anställda kunde inte importeras, licensen tillåter inte fler anställda. Max {0} st"), result.IntegerValue);
                            break;
                        case (int)ActionResultSave.EmployeeNumberExists:
                            errorMessage = String.Format(GetText(5882, "Anställningsnumret '{0}' är upptaget"), result.StringValue);
                            break;
                        default:
                            errorMessage = result.ErrorMessage;
                            break;

                    }
                    result.ErrorMessage = errorMessage;
                }
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
            finally
            {
                //Remove temp-file
                if (!String.IsNullOrEmpty(pathOnServer))
                    RemoveFileFromServer(pathOnServer);
            }

            return result;
        }

        #endregion

        #region Import type

        private ActionResult ImportExcelContent(DataSet ds, string tableName, int actorCompanyId, Dictionary<string, string> parameters, bool doNotModifyWithEmpty = false)
        {
            ActionResult result = new ActionResult(false);

            #region Prereq

            if (ds == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DataSet");

            ImportType importType = GetImportType(tableName);
            if (importType == ImportType.Unknown)
                return new ActionResult(false, 0, GetFileNotMatchingSpecificationMessage());

            DataRowCollection rows = ds.Tables[0].Rows;
            if (rows == null || rows.Count == 0)
                return new ActionResult(false, 0, GetFileIsEmptyMessage());

            #endregion

            #region Init

            this.rowNr = 0;
            this.conflicts = new List<ImportExportConflictItem>();
            this.translationDict = new Dictionary<string, string>();

            //Set TableName
            ds.Tables[0].TableName = tableName;

            //Translate Columns
            TranslateDataSet(ds, importType);

            #endregion

            #region Import

            switch (importType)
            {
                case ImportType.Customer:
                    result = ImportCustomers(rows, actorCompanyId, doNotModifyWithEmpty);
                    break;
                case ImportType.Supplier:
                    result = ImportSuppliers(rows, actorCompanyId, doNotModifyWithEmpty);
                    break;
                case ImportType.ContactPerson:
                    result = ImportContactPersons(rows, actorCompanyId, doNotModifyWithEmpty);
                    break;
                case ImportType.Product:
                    result = ImportProducts(rows, actorCompanyId, doNotModifyWithEmpty);
                    break;
                case ImportType.Employee:
                    result = ImportEmployees(rows, actorCompanyId);
                    break;
                case ImportType.CustomerGroup:
                    result = ImportCustomerCategories(rows, actorCompanyId);
                    break;
                case ImportType.ProductGroup:
                    result = ImportProductGroups(rows, actorCompanyId);
                    break;
                case ImportType.PayrollStartValue:
                    result = ImportPayrollStartValues(rows, actorCompanyId, parameters);
                    break;
                case ImportType.Accounts:
                    result = ImportAccounts(rows, actorCompanyId, doNotModifyWithEmpty);
                    break;
                case ImportType.TaxDeductionContacts:
                    result = ImportTaxDeductionContacts(rows, actorCompanyId, doNotModifyWithEmpty);
                    break;
                case ImportType.Pricelists:
                    result = ImportPricelists(rows, actorCompanyId, doNotModifyWithEmpty);
                    break;
                case ImportType.Agreements:
                    result = ImportAgreements(rows, actorCompanyId, doNotModifyWithEmpty);
                    break;
            }

            result.Value = this.conflicts;

            #endregion

            return result;
        }

        private ActionResult ImportCustomers(DataRowCollection rows, int actorCompanyId, bool doNotModifyWithEmpty = false)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (!IsColumnsMatching(rows, typeof(ExcelColumnCustomer)))
                    return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

                //Company
                this.company = CompanyManager.GetCompany(entities, actorCompanyId, true);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Customers
                List<Customer> customers = CustomerManager.GetCustomersByCompany(entities, this.company.ActorCompanyId, onlyActive: false);

                //SysWholesellers
                List<SysWholeseller> sysWholesellers = SysPriceListManager.GetSysWholesellers();

                #endregion

                foreach (DataRow row in rows)
                {
                    try
                    {
                        #region Init

                        this.rowNr++;

                        //First row is a header row for semantics in excel spreadsheet
                        if (this.rowNr == 1)
                            continue;

                        int actorId = 0;

                        #endregion

                        #region Mandatory columns

                        var columnCustomerNr = ExcelUtil.GetColumnValue(row, ExcelColumnCustomer.CustomerNr);
                        var columnName = ExcelUtil.GetColumnValue(row, ExcelColumnCustomer.Name);
                        var columnVatType = ExcelUtil.GetColumn(row, ExcelColumnCustomer.VatType);

                        //Validate exists
                        if (columnCustomerNr == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnCustomer.CustomerNr));
                        if (columnName == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnCustomer.Name));
                        if (columnVatType == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnCustomer.VatType));

                        // Convert newline characters in columnName
                        if (columnName != null)
                        {
                            string nameString = columnName.ToString();
                            if (nameString.Contains('\n') && !nameString.Contains(Environment.NewLine))
                            {
                                // We don't want linebreaks at the end of the name
                                columnName = nameString.TrimEnd('\n').Replace("\n", Environment.NewLine);
                            }
                        }

                        //Validate value
                        if (!EnumUtility.GetValue(columnVatType, out int vatType, typeof(TermGroup_InvoiceVatType)))
                            AddError(ErrorMessageType.MandatoryColumnInvalid, GetColumnName(ExcelColumnCustomer.VatType));

                        if (HasErrors())
                            continue;

                        #endregion

                        #region Optional columns

                        var columnOrgNr = ExcelUtil.GetColumn(row, ExcelColumnCustomer.OrgNr);
                        var columnVatNr = ExcelUtil.GetColumn(row, ExcelColumnCustomer.VatNr);
                        var columnSupplierNr = ExcelUtil.GetColumn(row, ExcelColumnCustomer.SupplierNr);
                        var columnCountry = ExcelUtil.GetColumn(row, ExcelColumnCustomer.Country);
                        var columnCurrency = ExcelUtil.GetColumn(row, ExcelColumnCustomer.Currency);
                        var columnPaymentCondition = ExcelUtil.GetColumn(row, ExcelColumnCustomer.PaymentCondition);
                        var columnGracePeriod = ExcelUtil.GetColumn(row, ExcelColumnCustomer.GracePeriod);
                        var columnBillingTemplate = ExcelUtil.GetColumn(row, ExcelColumnCustomer.BillingTemplate);
                        var columnInvoiceReference = ExcelUtil.GetColumn(row, ExcelColumnCustomer.InvoiceReference);
                        var columnDeliveryMethod = ExcelUtil.GetColumn(row, ExcelColumnCustomer.DeliveryMethod);
                        var columnDeliveryCondition = ExcelUtil.GetColumn(row, ExcelColumnCustomer.DeliveryCondition);
                        var columnDefaultPriceListType = ExcelUtil.GetColumn(row, ExcelColumnCustomer.DefaultPriceListType);
                        var columnDefaultWholeseller = ExcelUtil.GetColumn(row, ExcelColumnCustomer.DefaultWholeseller);
                        var columnDiscountMerchandise = ExcelUtil.GetColumn(row, ExcelColumnCustomer.DiscountMerchandise);
                        var columnDiscountService = ExcelUtil.GetColumn(row, ExcelColumnCustomer.DiscountService);
                        var columnActive = ExcelUtil.GetColumn(row, ExcelColumnCustomer.Active);
                        var columnFinvoiceAddr = ExcelUtil.GetColumn(row, ExcelColumnCustomer.FinvoiceAddress);
                        var columnFinvoiceOper = ExcelUtil.GetColumn(row, ExcelColumnCustomer.FinvoiceOperator);
                        var columnSysLanguageId = ExcelUtil.GetColumn(row, ExcelColumnCustomer.Language);
                        var columnPayingCustomerNr = ExcelUtil.GetColumn(row, ExcelColumnCustomer.PayingCustomerId);
                        var columnInvoiceDeliveryType = ExcelUtil.GetColumn(row, ExcelColumnCustomer.InvoiceDeliveryType);
                        var columnGLNNr = ExcelUtil.GetColumn(row, ExcelColumnCustomer.GLNNumber);
                        var columnNote = ExcelUtil.GetColumn(row, ExcelColumnCustomer.Note);

                        #endregion

                        #region Customer

                        Customer customer = CustomerManager.GetCustomerByNr(entities, actorCompanyId, columnCustomerNr.ToString(), customers);

                        bool newCustomer = customer == null;
                        bool currentDoNotModifyWithEmpty = !newCustomer && doNotModifyWithEmpty;

                        if (customer == null)
                        {
                            #region Add

                            //Add Customer
                            customer = new Customer()
                            {
                                CustomerNr = columnCustomerNr.ToString(),
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(customer);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(customer);

                            //Make sure Actor is loaded
                            if (!customer.ActorReference.IsLoaded)
                                customer.ActorReference.Load();

                            #endregion
                        }

                        //Mandatory
                        customer.Name = columnName.ToString();
                        customer.VatType = vatType;

                        //Optional
                        customer.OrgNr = StringUtility.ModifyValue(customer.OrgNr, StringUtility.GetStringValue(columnOrgNr), currentDoNotModifyWithEmpty);
                        customer.VatNr = StringUtility.ModifyValue(customer.VatNr, StringUtility.GetStringValue(columnVatNr), currentDoNotModifyWithEmpty);
                        customer.SupplierNr = StringUtility.ModifyValue(customer.SupplierNr, StringUtility.GetStringValue(columnSupplierNr), currentDoNotModifyWithEmpty);
                        customer.GracePeriodDays = StringUtility.ModifyValue(customer.GracePeriodDays, StringUtility.GetInt(columnGracePeriod), currentDoNotModifyWithEmpty);
                        customer.InvoiceReference = StringUtility.ModifyValue(customer.InvoiceReference, StringUtility.GetStringValue(columnInvoiceReference), currentDoNotModifyWithEmpty);
                        customer.DiscountMerchandise = StringUtility.ModifyValue(customer.DiscountMerchandise, NumberUtility.ToNullableDecimal(columnDiscountMerchandise, 2), currentDoNotModifyWithEmpty);
                        customer.DiscountService = StringUtility.ModifyValue(customer.DiscountService, NumberUtility.ToNullableDecimal(columnDiscountService, 2), currentDoNotModifyWithEmpty);
                        customer.State = StringUtility.GetBool(columnActive.ToString(), true) ? (int)SoeEntityState.Active : (int)SoeEntityState.Inactive;
                        customer.InvoiceDeliveryType = StringUtility.GetNullableInt(columnInvoiceDeliveryType);
                        if (String.IsNullOrEmpty(customer.Note))
                            customer.Note = StringUtility.ModifyValue(customer.Note, StringUtility.GetStringValue(columnNote), currentDoNotModifyWithEmpty);

                        //FinvoiceFields
                        string finvAddr = StringUtility.GetStringValue(columnFinvoiceAddr);
                        string finvOper = StringUtility.GetStringValue(columnFinvoiceOper);
                        if (finvAddr.HasValue())
                            customer.FinvoiceAddress = finvAddr;
                        if (finvOper.HasValue())
                            customer.FinvoiceOperator = finvOper;
                        if (finvOper.HasValue() && finvAddr.HasValue())
                        {
                            customer.IsFinvoiceCustomer = true;
                            // Add Electronic deliverytype
                            customer.InvoiceDeliveryType = (int)SoeInvoiceDeliveryType.Electronic;
                        }

                        // Optional, Customer language / Jukka 
                        // Accepts either language by number or language code
                        // 1 = sv-SE = Swedish
                        // 2 = en-US = English
                        // 3 = fi-FI = Finnish
                        // 4 = nb-NO = Norwegian
                        if (columnSysLanguageId != null)
                        {
                            customer.SysLanguageId = GetLangId(columnSysLanguageId);
                        }

                        //Optional FK
                        customer.BillingTemplate = GetBillingInvoiceReportId(entities, columnBillingTemplate, customer.BillingTemplate, currentDoNotModifyWithEmpty);
                        customer.SysCountryId = (int?)CountryCurrencyManager.GetSysCountry(columnCountry?.ToString())?.SysCountryId;
                        customer.SysWholeSellerId = GetSysWholeSellerId(sysWholesellers, columnDefaultWholeseller, customer.SysWholeSellerId, currentDoNotModifyWithEmpty);
                        if (columnPayingCustomerNr.ToString().HasValue())
                            customer.PayingCustomerId = GetPayingCustomerId(entities, columnPayingCustomerNr, customer.PayingCustomerId, currentDoNotModifyWithEmpty);

                        //Optional relations
                        int currencyId = GetCurrencyId(entities, columnCurrency, customer.CurrencyId, currentDoNotModifyWithEmpty);
                        int? deliveryTypeId = GetDeliveryTypeId(entities, columnDeliveryMethod, customer.DeliveryTypeId);
                        int? deliveryConditionId = GetDeliveryConditionId(entities, columnDeliveryCondition, customer.DeliveryConditionId);
                        int? paymentConditionId = GetPaymentConditionId(entities, columnPaymentCondition, customer.PaymentConditionId, currentDoNotModifyWithEmpty);
                        int? defaultPriceListTypeId = GetPricelistTypeId(entities, columnDefaultPriceListType, customer.PriceListTypeId, currentDoNotModifyWithEmpty);

                        #endregion

                        #region Add relations

                        //Account
                        if (!TryAddCustomerAccountStds(entities, row, customer, CUSTOMER_NO_OF_ACCOUNTINTERNALS, currentDoNotModifyWithEmpty))
                            AddWarning(WarningMessageType.SaveAccountsFailed);

                        #endregion

                        #region Save

                        result = newCustomer
                            ? CustomerManager.AddCustomer(entities, customer, deliveryTypeId, deliveryConditionId, paymentConditionId, defaultPriceListTypeId, currencyId, this.company.ActorCompanyId)
                            : CustomerManager.UpdateCustomer(entities, customer, deliveryTypeId, deliveryConditionId, paymentConditionId, defaultPriceListTypeId, currencyId);

                        if (!result.Success)
                        {
                            AddError(ErrorMessageType.SaveFailed, identifier: String.Format("{0}. {1}", customer.CustomerNr, customer.Name));
                            continue;
                        }

                        //Update counters
                        if (newCustomer)
                            itemsAdded++;
                        else
                            itemsUpdated++;

                        //Set id's
                        actorId = customer.Actor.ActorId;

                        //Add to collection
                        if (newCustomer)
                            customers.Add(customer);

                        #endregion

                        #region Save relations

                        Contact contact = null;
                        if (TrySaveContact(entities, actorId, TermGroup_SysContactType.Company, ref contact))
                        {
                            TrySaveContactEcom(entities, row, contact, currentDoNotModifyWithEmpty, true);
                            TrySaveContactAddresses(entities, row, contact, currentDoNotModifyWithEmpty);
                        }

                        TrySaveCategories(entities, row, customer.ActorCustomerId, SoeCategoryType.Customer, SoeCategoryRecordEntity.Customer, false, CUSTOMER_NO_OF_CATEGORIES);
                        TrySaveCategories(entities, row, customer.ActorCustomerId, SoeCategoryType.Customer, SoeCategoryRecordEntity.Customer, true, CUSTOMER_NO_OF_CATEGORIES);


                        #endregion
                    }
                    catch (ArgumentException ax)
                    {
                        //Break import
                        AddTerminationError(TerminationMessageType.FieldMissing, exception: ax);
                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());
                    }
                    catch (Exception ex)
                    {
                        AddError(ErrorMessageType.UnkownError, exception: ex);
                    }
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.Customer);

            return result;
        }

        private ActionResult ImportSuppliers(DataRowCollection rows, int actorCompanyId, bool doNotModifyWithEmpty = false)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (!IsColumnsMatching(rows, typeof(ExcelColumnSupplier)))
                    return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

                //Company
                this.company = CompanyManager.GetCompany(entities, actorCompanyId, true);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                List<Supplier> suppliers = SupplierManager.GetSuppliersByCompany(entities, actorCompanyId, false);

                #endregion

                foreach (DataRow row in rows)
                {
                    try
                    {
                        #region Init

                        this.rowNr++;

                        //First row is a header row for semantics in excel spreadsheet
                        if (this.rowNr == 1)
                            continue;

                        int actorId = 0;

                        #endregion

                        #region Mandatory columns

                        var columnSupplierNr = ExcelUtil.GetColumnValue(row, ExcelColumnSupplier.SupplierNr);
                        var columnName = ExcelUtil.GetColumnValue(row, ExcelColumnSupplier.Name);
                        var columnVatType = ExcelUtil.GetColumn(row, ExcelColumnSupplier.VatType);

                        //Validate exists
                        if (columnSupplierNr == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnSupplier.SupplierNr));
                        if (columnName == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnSupplier.Name));
                        if (columnVatType == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnSupplier.VatType));

                        //Validate value
                        if (!EnumUtility.GetValue(columnVatType, out int vatType, typeof(TermGroup_InvoiceVatType)))
                            AddError(ErrorMessageType.MandatoryColumnInvalid, GetColumnName(ExcelColumnSupplier.VatType));

                        if (HasErrors())
                            continue;

                        #endregion

                        #region Optional columns

                        var columnOrgNr = ExcelUtil.GetColumn(row, ExcelColumnSupplier.OrgNr);
                        var columnVatNr = ExcelUtil.GetColumn(row, ExcelColumnSupplier.VatNr);
                        var columnVatCode = ExcelUtil.GetColumn(row, ExcelColumnSupplier.VatCode);
                        var columnRiksbanksCode = ExcelUtil.GetColumn(row, ExcelColumnSupplier.RiksbanksCode);
                        var columnOurCustomerNr = ExcelUtil.GetColumn(row, ExcelColumnSupplier.OurCustomerNr);
                        var columnFactoringSupplier = ExcelUtil.GetColumn(row, ExcelColumnSupplier.FactoringSupplier);
                        var columnCountry = ExcelUtil.GetColumn(row, ExcelColumnSupplier.Country);
                        var columnCurrency = ExcelUtil.GetColumn(row, ExcelColumnSupplier.Currency);
                        var columnPaymentCondition = ExcelUtil.GetColumn(row, ExcelColumnSupplier.PaymentCondition);
                        var columnCopyInvoiceNrToOcr = ExcelUtil.GetColumn(row, ExcelColumnSupplier.CopyInvoiceNrToOcr);
                        var columnBlockPayment = ExcelUtil.GetColumn(row, ExcelColumnSupplier.BlockPayment);
                        var columnManualAccounting = ExcelUtil.GetColumn(row, ExcelColumnSupplier.ConfirmAccounts);
                        var columnActive = ExcelUtil.GetColumn(row, ExcelColumnCustomer.Active);

                        #endregion

                        #region Supplier

                        Supplier supplier = SupplierManager.GetSupplierBySupplierNr(entities, this.company.ActorCompanyId, columnSupplierNr.ToString().ToLower(), false, suppliers);

                        bool newSupplier = supplier == null;
                        bool currentDoNotModifyWithEmpty = !newSupplier && doNotModifyWithEmpty;

                        if (supplier == null)
                        {
                            #region Add

                            supplier = new Supplier()
                            {
                                ActorCompanyId = actorCompanyId,
                                SupplierNr = columnSupplierNr.ToString(),
                            };
                            SetCreatedProperties(supplier);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(supplier);

                            #endregion
                        }

                        //Mandatory
                        supplier.Name = columnName.ToString();
                        supplier.VatType = vatType;

                        //Optional
                        supplier.OrgNr = StringUtility.ModifyValue(supplier.OrgNr, StringUtility.GetStringValue(columnOrgNr), currentDoNotModifyWithEmpty);
                        supplier.VatNr = StringUtility.ModifyValue(supplier.VatNr, StringUtility.GetStringValue(columnVatNr), currentDoNotModifyWithEmpty);
                        supplier.RiksbanksCode = StringUtility.ModifyValue(supplier.RiksbanksCode, StringUtility.GetStringValue(columnRiksbanksCode), currentDoNotModifyWithEmpty);
                        supplier.OurCustomerNr = StringUtility.ModifyValue(supplier.OurCustomerNr, StringUtility.GetStringValue(columnOurCustomerNr), currentDoNotModifyWithEmpty);
                        supplier.CopyInvoiceNrToOcr = StringUtility.ModifyValue(supplier.CopyInvoiceNrToOcr, StringUtility.GetNullableBool(columnCopyInvoiceNrToOcr), currentDoNotModifyWithEmpty);
                        supplier.BlockPayment = StringUtility.ModifyValue(supplier.BlockPayment, StringUtility.GetNullableBool(columnBlockPayment), currentDoNotModifyWithEmpty);
                        supplier.ManualAccounting = StringUtility.ModifyValue(supplier.ManualAccounting, StringUtility.GetNullableBool(columnManualAccounting), currentDoNotModifyWithEmpty);
                        supplier.State = StringUtility.ModifyValue(supplier.State, (int)StringUtility.GetEntityState(columnActive), currentDoNotModifyWithEmpty);

                        //Optional FK
                        supplier.SysCountryId = (int?)CountryCurrencyManager.GetSysCountry(columnCountry?.ToString())?.SysCountryId;

                        //Optional relations
                        int currencyId = GetCurrencyId(entities, columnCurrency, supplier.CurrencyId, currentDoNotModifyWithEmpty);
                        int? paymentConditionId = GetPaymentConditionId(entities, columnPaymentCondition, supplier.PaymentConditionId, currentDoNotModifyWithEmpty);
                        int? factoringSupplierId = GetFactoringSupplierId(entities, columnFactoringSupplier, supplier.FactoringSupplierId, currentDoNotModifyWithEmpty);
                        int? vatCodeId = GetVatCodeId(entities, columnVatCode, supplier.VatCodeId, currentDoNotModifyWithEmpty);

                        #endregion

                        #region Add relations

                        //Account
                        if (!TryAddSupplierAccountStds(entities, row, supplier, SUPPLIER_NO_OF_ACCOUNTINTERNALS, currentDoNotModifyWithEmpty))
                            AddWarning(WarningMessageType.SaveAccountsFailed);

                        #endregion

                        #region Save

                        result = newSupplier
                            ? SupplierManager.AddSupplier(entities, supplier, paymentConditionId, factoringSupplierId, currencyId, this.company.ActorCompanyId, vatCodeId)
                            : SupplierManager.UpdateSupplier(entities, supplier, paymentConditionId, factoringSupplierId, currencyId, vatCodeId);

                        if (!result.Success)
                        {
                            AddError(ErrorMessageType.SaveFailed, identifier: String.Format("{0}. {1}", supplier.SupplierNr, supplier.Name));
                            continue;
                        }

                        //Update counters
                        if (newSupplier)
                            itemsAdded++;
                        else
                            itemsUpdated++;

                        //Make sure Actor is loaded
                        if (!supplier.ActorReference.IsLoaded)
                            supplier.ActorReference.Load();

                        //Set id's
                        actorId = supplier.Actor.ActorId;

                        //Add to collection
                        if (newSupplier)
                            suppliers.Add(supplier);

                        #endregion

                        #region Save relations

                        Contact contact = null;
                        if (TrySaveContact(entities, actorId, TermGroup_SysContactType.Company, ref contact))
                        {
                            TrySaveContactEcom(entities, row, contact, currentDoNotModifyWithEmpty);
                            TrySaveContactAddresses(entities, row, contact, currentDoNotModifyWithEmpty);
                            TrySavePaymentInformation(entities, row, contact, currentDoNotModifyWithEmpty);
                        }

                        #endregion
                    }
                    catch (ArgumentException ax)
                    {
                        //Break import
                        AddTerminationError(TerminationMessageType.FieldMissing, exception: ax);
                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());
                    }
                    catch (Exception ex)
                    {
                        AddError(ErrorMessageType.UnkownError, exception: ex);
                    }
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.Supplier);

            return result;
        }

        private ActionResult ImportProducts(DataRowCollection rows, int actorCompanyId, bool doNotModifyWithEmpty = false)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (!IsColumnsMatching(rows, typeof(ExcelColumnProduct)))
                    return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

                //Company
                this.company = CompanyManager.GetCompany(entities, actorCompanyId, true);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Vatcodes
                IEnumerable<VatCode> vatCodes = AccountManager.GetVatCodes(actorCompanyId);

                #endregion

                foreach (DataRow row in rows)
                {
                    try
                    {
                        #region Init

                        this.rowNr++;

                        //First row is a header row for semantics in excel spreadsheet
                        if (this.rowNr == 1)
                            continue;

                        #endregion

                        #region Mandatory columns

                        //Mandatory fields
                        var columnProductNr = ExcelUtil.GetColumnValue(row, ExcelColumnProduct.ProductNr);
                        var columnName = ExcelUtil.GetColumnValue(row, ExcelColumnProduct.Name);
                        var columnVatType = ExcelUtil.GetColumn(row, ExcelColumnProduct.VatType);

                        //Validate exists
                        if (columnProductNr == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnProduct.ProductNr));
                        if (columnName == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnProduct.Name));
                        if (columnVatType == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnProduct.VatType));

                        //Validate mandatory fields
                        if (columnProductNr == null || columnName == null)
                            continue;

                        //Validate value
                        if (!EnumUtility.GetValue(columnVatType, out int vatType, typeof(TermGroup_InvoiceProductVatType)))
                            AddError(ErrorMessageType.MandatoryColumnInvalid, GetColumnName(ExcelColumnProduct.VatType));

                        if (HasErrors())
                            continue;

                        #endregion

                        #region Optional columns

                        var columnDescription = ExcelUtil.GetColumn(row, ExcelColumnProduct.Description)?.ToString().Trim();
                        var columnProductUnit = ExcelUtil.GetColumn(row, ExcelColumnProduct.Unit)?.ToString().Trim();

                        var columnEAN = ExcelUtil.GetColumn(row, ExcelColumnProduct.EAN);
                        var columnProductGroup = ExcelUtil.GetColumn(row, ExcelColumnProduct.ProductGroup);
                        var columnPurchasePrice = ExcelUtil.GetColumn(row, ExcelColumnProduct.PurchasePrice);
                        var columnPrice = ExcelUtil.GetColumn(row, ExcelColumnProduct.Price);
                        var columnPriceListType = ExcelUtil.GetColumn(row, ExcelColumnProduct.PriceListType);
                        var columnVatCode = ExcelUtil.GetColumn(row, ExcelColumnProduct.VatCode);

                        #endregion

                        #region InvoiceProduct

                        InvoiceProduct invoiceProduct = ProductManager.GetInvoiceProductByProductNr(entities, columnProductNr.ToString(), this.company.ActorCompanyId);

                        bool newProduct = invoiceProduct == null;
                        bool currentDoNotModifyWithEmpty = !newProduct && doNotModifyWithEmpty;

                        if (invoiceProduct == null)
                        {
                            #region Add

                            invoiceProduct = new InvoiceProduct()
                            {
                                Type = (int)SoeProductType.InvoiceProduct,
                                AccountingPrio = "1=0,2=0,3=0,4=0,5=0,6=0",
                            };
                            SetCreatedProperties(invoiceProduct);

                            invoiceProduct.Company.Add(this.company);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(invoiceProduct);

                            #endregion
                        }

                        //Mandatory
                        invoiceProduct.Number = StringUtility.GetStringValue(columnProductNr);
                        invoiceProduct.Name = StringUtility.GetStringValue(columnName);
                        invoiceProduct.VatType = vatType;

                        //Optional
                        invoiceProduct.Description = StringUtility.ModifyValue(invoiceProduct.Description, StringUtility.GetStringValue(columnDescription), currentDoNotModifyWithEmpty);
                        invoiceProduct.EAN = StringUtility.ModifyValue(invoiceProduct.EAN, StringUtility.GetStringValue(columnEAN), currentDoNotModifyWithEmpty);
                        invoiceProduct.PurchasePrice = StringUtility.ModifyValue(invoiceProduct.PurchasePrice, NumberUtility.ToNullableDecimal(columnPurchasePrice, 2), currentDoNotModifyWithEmpty);

                        //VatCode
                        var vatCode = StringUtility.GetStringValue(columnVatCode);
                        if (vatCode != String.Empty)
                        {
                            IEnumerable<int> vCode = (from aru in vatCodes
                                                      where aru.Code == vatCode
                                                      select aru.VatCodeId);
                            if (vCode != null)
                                invoiceProduct.VatCodeId = vCode.First();
                        }
                        #endregion

                        #region Add relations

                        //Account
                        if (!TryAddProductAccountStds(entities, row, invoiceProduct, PRODUCTS_NO_OF_ACCOUNTINTERNALS, currentDoNotModifyWithEmpty))
                            AddWarning(WarningMessageType.SaveAccountsFailed);

                        TryAddProductGroup(entities, columnProductGroup, invoiceProduct);
                        TryAddProductUnit(entities, columnProductUnit, invoiceProduct);
                        TryAddPriceList(entities, columnPriceListType, columnPrice, invoiceProduct, currentDoNotModifyWithEmpty);

                        #endregion

                        #region Save

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            AddError(ErrorMessageType.SaveFailed, identifier: String.Format("{0}. {1}", invoiceProduct.Number, invoiceProduct.Name));
                            continue;
                        }

                        //Update counters
                        if (newProduct)
                            itemsAdded++;
                        else
                            itemsUpdated++;

                        #endregion

                        #region Save relations

                        TrySaveCategories(entities, row, invoiceProduct.ProductId, SoeCategoryType.Product, SoeCategoryRecordEntity.Product, false, PRODUCTS_NO_OF_CATEGORIES);

                        #endregion
                    }
                    catch (ArgumentException ax)
                    {
                        //Break import
                        AddTerminationError(TerminationMessageType.FieldMissing, exception: ax);
                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());
                    }
                    catch (Exception ex)
                    {
                        AddError(ErrorMessageType.UnkownError, exception: ex);
                    }
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.Product);

            return result;
        }

        private ActionResult ImportContactPersons(DataRowCollection rows, int actorCompanyId, bool doNotModifyWithEmpty = false)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (!IsColumnsMatching(rows, typeof(ExcelColumnContactPerson)))
                    return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

                //Company
                this.company = CompanyManager.GetCompany(entities, actorCompanyId, true);
                if (this.company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                var contactPersons = ContactManager.GetContactPersonsAll(entities, actorCompanyId);

                #endregion

                foreach (DataRow row in rows)
                {
                    try
                    {
                        #region Init

                        this.rowNr++;

                        //First row is a header row for semantics in excel spreadsheet
                        if (this.rowNr == 1)
                            continue;

                        #endregion

                        #region Mandatory columns

                        var columnFirstName = ExcelUtil.GetColumnValue(row, ExcelColumnContactPerson.FirstName);
                        var columnLastName = ExcelUtil.GetColumnValue(row, ExcelColumnContactPerson.LastName);

                        //Validate exists
                        if (columnFirstName == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnContactPerson.FirstName));
                        if (columnLastName == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnContactPerson.LastName));

                        if (HasErrors())
                            continue;

                        #endregion

                        #region Optional columns

                        var columnSex = ExcelUtil.GetColumn(row, ExcelColumnContactPerson.Sex);
                        var columnPosition = ExcelUtil.GetColumn(row, ExcelColumnContactPerson.Position);

                        //Validate sex
                        int? sex = null;
                        if (StringUtility.HasValue(columnSex))
                        {
                            if (EnumUtility.GetValue(columnSex, out int sexValue, typeof(TermGroup_Sex)))
                                sex = sexValue;
                            else
                                AddWarning(WarningMessageType.OptionalFieldInvalid, GetColumnName(ExcelColumnContactPerson.Sex));
                        }

                        //Validate sex
                        int? position = null;
                        if (StringUtility.HasValue(columnPosition))
                        {
                            if (EnumUtility.GetValue(columnPosition, out int positionValue, typeof(TermGroup_ContactPersonPosition)))
                                position = positionValue;
                            else
                                AddWarning(WarningMessageType.OptionalFieldInvalid, GetColumnName(ExcelColumnContactPerson.Position));
                        }

                        #endregion

                        #region ContactPerson

                        ContactPerson contactPerson = null;
                        if (columnFirstName != null && columnLastName != null)
                            contactPerson = contactPersons.FirstOrDefault(p => p.FirstName == columnFirstName.ToString() && p.LastName == columnLastName.ToString());

                        bool newContactPerson = contactPerson == null;
                        bool currentDoNotModifyWithEmpty = !newContactPerson && doNotModifyWithEmpty;

                        if (contactPerson == null)
                        {
                            #region Add

                            contactPerson = new ContactPerson()
                            {
                                FirstName = columnFirstName?.ToString() ?? string.Empty,
                                LastName = columnLastName?.ToString() ?? string.Empty,
                            };
                            SetCreatedProperties(contactPerson);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(contactPerson);

                            #endregion
                        }

                        //Mandatory
                        contactPerson.FirstName = StringUtility.ModifyValue(contactPerson.FirstName, columnFirstName.ToString(), currentDoNotModifyWithEmpty);
                        contactPerson.LastName = StringUtility.ModifyValue(contactPerson.LastName, columnLastName.ToString(), currentDoNotModifyWithEmpty);

                        //Optional
                        contactPerson.Position = GetIntId(position, contactPerson.Position, currentDoNotModifyWithEmpty);
                        contactPerson.Sex = GetIntId(sex, contactPerson.Sex, currentDoNotModifyWithEmpty);

                        #endregion

                        #region Save

                        result = newContactPerson
                            ? ContactManager.AddContactPerson(entities, contactPerson, this.company.ActorCompanyId)
                            : ContactManager.UpdateContactPerson(entities, contactPerson);

                        if (!result.Success)
                        {
                            AddError(ErrorMessageType.SaveFailed, identifier: String.Format("{0}", contactPerson.Name));
                            continue;
                        }

                        //Update counters
                        if (newContactPerson)
                            itemsAdded++;
                        else
                            itemsUpdated++;

                        //Make sure Actor is loaded
                        if (!contactPerson.ActorReference.IsLoaded)
                            contactPerson.ActorReference.Load();

                        //Set id's
                        int actorId = contactPerson.Actor.ActorId;

                        #endregion

                        #region Map to supplier

                        var columnMapToSupplierNr = ExcelUtil.GetColumn(row, ExcelColumnContactPerson.MapToSupplierNr);
                        if (StringUtility.HasValue(columnMapToSupplierNr))
                        {
                            var supplier = SupplierManager.GetSupplierBySupplierNr(entities, actorCompanyId, columnMapToSupplierNr.ToString().Trim(), true);
                            if (supplier != null)
                            {
                                result = ContactManager.MapActorToContactPerson(entities, contactPerson, supplier.ActorSupplierId);
                                if (!result.Success)
                                {
                                    AddError(ErrorMessageType.SaveFailed, identifier: String.Format("{0} {1}", contactPerson.Name, supplier.SupplierNr));
                                    continue;
                                }
                            }
                        }

                        #endregion

                        #region Map to customer

                        var columnMapToCustomerNr = ExcelUtil.GetColumn(row, ExcelColumnContactPerson.MapToCustomerNr);
                        if (StringUtility.HasValue(columnMapToCustomerNr))
                        {
                            var customer = CustomerManager.GetCustomerByNr(entities, actorCompanyId, columnMapToCustomerNr.ToString().Trim());
                            if (customer != null)
                            {
                                result = ContactManager.MapActorToContactPerson(entities, contactPerson, customer.ActorCustomerId);
                                if (!result.Success)
                                {
                                    AddError(ErrorMessageType.SaveFailed, identifier: String.Format("{0} {1}", contactPerson.Name, customer.CustomerNr));
                                    continue;
                                }
                            }
                        }

                        #endregion

                        #region Save relations

                        Contact contact = null;
                        if (TrySaveContact(entities, actorId, TermGroup_SysContactType.Company, ref contact))
                        {
                            TrySaveContactEcom(entities, row, contact, currentDoNotModifyWithEmpty);
                            TrySaveContactAddresses(entities, row, contact, currentDoNotModifyWithEmpty);
                        }

                        #endregion
                    }
                    catch (ArgumentException ax)
                    {
                        //Break import
                        AddTerminationError(TerminationMessageType.FieldMissing, exception: ax);
                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());
                    }
                    catch (Exception ex)
                    {
                        AddError(ErrorMessageType.UnkownError, exception: ex);
                    }
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.ContactPerson);

            return result;
        }

        private ActionResult ImportEmployees(DataRowCollection rows, int actorCompanyId, bool doNotModifyRolesWithEmpty = false, bool discardLicenseCheckes = false)
        {
            bool doNotModifyWithEmpty = true; //Because consultants uncheck it by mistake

            #region Prereq

            if (!IsColumnsMatching(rows, typeof(ExcelColumnEmployee)))
                return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

            #endregion

            EmployeeIOItem employeeIOItem = new EmployeeIOItem(TermGroup_IOSource.XE, TermGroup_IOType.WebService, TermGroup_IOImportHeadType.Employee, actorCompanyId);

            foreach (DataRow row in rows.Cast<DataRow>().Skip(2))
            {
                employeeIOItem.CreateEmployeeIO(row);
            }

            return ImportEmployees(employeeIOItem.EmployeeIOs, actorCompanyId, discardLicenseCheckes, doNotModifyWithEmpty, doNotModifyRolesWithEmpty);
        }

        private ActionResult ImportCustomerCategories(DataRowCollection rows, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (!IsColumnsMatching(rows, typeof(ExcelColumnCustomeCategory)))
                    return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

                //Company
                this.company = CompanyManager.GetCompany(entities, actorCompanyId, true);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                #endregion

                foreach (DataRow row in rows)
                {
                    try
                    {
                        #region Init

                        this.rowNr++;

                        //First row is a header row for semantics in excel spreadsheet
                        if (this.rowNr == 1)
                            continue;

                        #endregion

                        #region Mandatory columns

                        var columnName = ExcelUtil.GetColumnValueByInt(row, (int)ExcelColumnCustomeCategory.Name);
                        var columnCode = ExcelUtil.GetColumnValueByInt(row, (int)ExcelColumnCustomeCategory.Code);

                        //Validate exists
                        if (columnName == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnCustomeCategory.Name));
                        if (columnCode == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnCustomeCategory.Code));

                        if (HasErrors())
                            continue;

                        #endregion

                        Category category = CategoryManager.GetCategory(entities, columnCode?.ToString(), (int)SoeCategoryType.Customer, actorCompanyId);

                        bool newCategory = category == null;

                        if (category == null)
                        {
                            #region Add

                            //Add Customer
                            category = new Category()
                            {
                                Code = columnCode?.ToString(),
                                Name = columnName?.ToString(),
                                State = (int)SoeEntityState.Active,
                                ActorCompanyId = actorCompanyId,
                                Type = (int)SoeCategoryType.Customer
                            };
                            SetCreatedProperties(category);
                            entities.AddObject("Category", category);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(category);
                            if (columnName != null)
                                category.Name = columnName.ToString();

                            #endregion
                        }

                        #region Save

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            AddError(ErrorMessageType.SaveFailed, identifier: String.Format("{0}. {1}", category.Name, category.Code));
                            continue;
                        }

                        //Update counters
                        if (newCategory)
                            itemsAdded++;
                        else
                            itemsUpdated++;

                        #endregion

                    }
                    catch (ArgumentException ax)
                    {
                        //Break import
                        AddTerminationError(TerminationMessageType.FieldMissing, exception: ax);
                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());
                    }
                    catch (Exception ex)
                    {
                        AddError(ErrorMessageType.UnkownError, exception: ex);
                    }
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.CustomerGroup);

            return result;
        }

        private ActionResult ImportProductGroups(DataRowCollection rows, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (!IsColumnsMatching(rows, typeof(ExcelColumnProductGroup)))
                    return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

                //Company
                this.company = CompanyManager.GetCompany(entities, actorCompanyId, true);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                #endregion

                foreach (DataRow row in rows)
                {
                    try
                    {
                        #region Init

                        this.rowNr++;

                        //First row is a header row for semantics in excel spreadsheet
                        if (this.rowNr == 1)
                            continue;

                        #endregion

                        #region Mandatory columns

                        //Mandatory fields
                        var columnCode = ExcelUtil.GetColumnValueByInt(row, (int)ExcelColumnProductGroup.Code);
                        var columnName = ExcelUtil.GetColumnValueByInt(row, (int)ExcelColumnProductGroup.Name);

                        //Validate mandatory fields
                        if (columnCode == null || columnName == null)
                            continue;

                        if (HasErrors())
                            continue;

                        #endregion

                        #region InvoiceProduct

                        ProductGroup productGroup = ProductGroupManager.GetProductGroup(entities, this.company.ActorCompanyId, columnCode.ToString());

                        bool newProductGroup = productGroup == null;

                        if (productGroup == null)
                        {
                            #region Add

                            productGroup = new ProductGroup()
                            {
                                Code = columnCode.ToString(),
                                Company = this.company,
                                Name = columnName.ToString()
                            };
                            SetCreatedProperties(productGroup);
                            entities.AddObject("ProductGroup", productGroup);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(productGroup);
                            productGroup.Name = columnName.ToString();

                            #endregion
                        }

                        #endregion

                        #region Save

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            AddError(ErrorMessageType.SaveFailed, identifier: String.Format("{0}. {1}", columnCode.ToString(), columnName.ToString()));
                            continue;
                        }

                        //Update counters
                        if (newProductGroup)
                            itemsAdded++;
                        else
                            itemsUpdated++;

                        #endregion

                    }
                    catch (ArgumentException ax)
                    {
                        //Break import
                        AddTerminationError(TerminationMessageType.FieldMissing, exception: ax);
                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());
                    }
                    catch (Exception ex)
                    {
                        AddError(ErrorMessageType.UnkownError, exception: ex);
                    }
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.ProductGroup);

            return result;
        }

        public (ActionResult, List<PayrollStartValueRowDTO>) ParsePayrollStartValues(Stream stream, int sysCountryId, int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            ActionResult result = new ActionResult();
            List<PayrollStartValueRowDTO> rowDtos = new List<PayrollStartValueRowDTO>();
            try
            {
                MemoryStream ms = new MemoryStream();
                stream.CopyTo(ms);
                ExcelHelper helper = new ExcelHelper();
                DataSet ds = helper.GetDataSet(ms.ToArray(), true);
                DataRowCollection rows = ds.Tables[0].Rows;

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                this.company = CompanyManager.GetCompany(entitiesReadOnly, actorCompanyId, true);
                if (company == null)
                    return (new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")), null);

                if (rows == null || rows.Count == 0)
                    return (new ActionResult(false, 0, GetFileIsEmptyMessage()), null);

                if (!IsColumnsMatching(rows, typeof(ExcelColumnPayrollStartValue)))
                    return (new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage()), null);


                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                List<SysPayrollStartValue> sysPayrollStartValues = (from spsv in sysEntitiesReadOnly.SysPayrollStartValue
                                                                    where spsv.SysCountryId == sysCountryId
                                                                    select spsv).ToList();

                if (sysPayrollStartValues == null || sysPayrollStartValues.Count == 0)
                    return (new ActionResult(false, (int)ActionResultSave.Unknown, GetText(10060, "Startvärden för det valda landet stöds inte")), null);

                List<PayrollProduct> payrollProducts = new List<PayrollProduct>();
                List<Employee> employees = new List<Employee>();
                this.rowNr = 1;
                foreach (DataRow row in rows)
                {

                    this.rowNr++;

                    ////First row is a header row for semantics in excel spreadsheet
                    //if (this.rowNr == 1)
                    //    continue;

                    #region Columns

                    var columnCode = row[getIndex(ExcelColumnPayrollStartValue.Code)];
                    int code = StringUtility.GetInt(columnCode);

                    var columnAppellation = row[getIndex(ExcelColumnPayrollStartValue.Appellation)];
                    string appellation = StringUtility.GetStringValue(columnAppellation);

                    var columnEmployeeNr = row[getIndex(ExcelColumnPayrollStartValue.EmployeeNr)];
                    string employeeNr = StringUtility.GetStringValue(columnEmployeeNr);

                    var columnQuantity = row[getIndex(ExcelColumnPayrollStartValue.Quantity)];
                    decimal quantity = StringUtility.GetDecimal(columnQuantity);

                    var columnAmount = row[getIndex(ExcelColumnPayrollStartValue.Amount)];
                    decimal amount = StringUtility.GetDecimal(columnAmount);

                    var columnPayrollProductNr = row[getIndex(ExcelColumnPayrollStartValue.PayrollProductNr)];
                    string payrollProductNr = StringUtility.GetStringValue(columnPayrollProductNr);

                    var columnDate = row[getIndex(ExcelColumnPayrollStartValue.Date)];
                    DateTime? date = CalendarUtility.GetNullableDateTime(columnDate);

                    var columnScheduleTime = row[getIndex(ExcelColumnPayrollStartValue.ScheduleTime)];
                    int scheduleTime = StringUtility.GetInt(columnScheduleTime);

                    var columnAbsenceTime = row[getIndex(ExcelColumnPayrollStartValue.AbsenceTime)];
                    int absenceTime = StringUtility.GetInt(columnAbsenceTime);

                    #endregion

                    #region Validation

                    bool predefinedValue = code > 0;

                    //Assume row is not used, skip row
                    if (String.IsNullOrEmpty(employeeNr) && quantity == 0 && amount == 0)
                        continue;

                    if (predefinedValue && String.IsNullOrEmpty(appellation))
                        return (new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10061, "Obligatoriska kolumner saknar värde. Rad {0}, kolumn {1}"), this.rowNr, "B")), null);
                    if (String.IsNullOrEmpty(employeeNr))
                        return (new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10061, "Obligatoriska kolumner saknar värde. Rad {0}, kolumn {1}"), this.rowNr, "C")), null);
                    if (quantity == 0)
                        return (new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10061, "Obligatoriska kolumner saknar värde. Rad {0}, kolumn {1}"), this.rowNr, "D")), null);
                    if (!predefinedValue && !date.HasValue)
                        return (new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10061, "Obligatoriska kolumner saknar värde. Rad {0}, kolumn {1}"), this.rowNr, "G")), null);
                    if (date.HasValue && (date.Value < dateFrom || date.Value > dateTo))
                        return (new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(9984, "Datumet {0} ligger utanför importens datumintervall. Rad {1}, kolumn {2}"), date.Value.ToShortDateString(), this.rowNr, "G")), null);

                    Employee employee = employees.FirstOrDefault(e => e.EmployeeNr == employeeNr);
                    if (employee == null)
                    {
                        employee = EmployeeManager.GetEmployeeByNr(entitiesReadOnly, employeeNr, actorCompanyId, onlyActive: false);
                        if (employee == null)
                            return (new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10067, "Anställd {0} hittades inte. Rad {1}"), employeeNr, this.rowNr)), null);

                        employees.Add(employee);
                    }

                    if (predefinedValue && String.IsNullOrEmpty(payrollProductNr))
                    {
                        //Get default from SysPayrollStartValue
                        var sysPayrollStartValue = sysPayrollStartValues.FirstOrDefault(i => i.SysPayrollStartValueId == code);
                        if (sysPayrollStartValue == null || String.IsNullOrEmpty(sysPayrollStartValue.PayrollProductNr))
                            return (new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10062, "Löneart hittades varken i fil eller som standard. Rad {0}"), this.rowNr)), null);

                        payrollProductNr = sysPayrollStartValue.PayrollProductNr;
                    }

                    PayrollProduct payrollProduct = payrollProducts.FirstOrDefault(i => i.Number == payrollProductNr);
                    if (payrollProduct == null)
                    {
                        payrollProduct = ProductManager.GetPayrollProductByNumber(entitiesReadOnly, payrollProductNr, actorCompanyId);
                        if (payrollProduct == null)
                            return (new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10062, "Löneart {0} hittades inte. Rad {1}"), payrollProductNr, this.rowNr)), null);

                        payrollProducts.Add(payrollProduct);
                    }

                    #endregion

                    var payrollStartValueRow = new PayrollStartValueRowDTO()
                    {
                        SysPayrollStartValueId = code,
                        Quantity = quantity,
                        Amount = amount,
                        Date = predefinedValue ? dateTo : date.Value,
                        SysPayrollTypeLevel1 = (TermGroup_SysPayrollType)(payrollProduct.SysPayrollTypeLevel1 ?? 0),
                        SysPayrollTypeLevel2 = (TermGroup_SysPayrollType)(payrollProduct.SysPayrollTypeLevel2 ?? 0),
                        SysPayrollTypeLevel3 = (TermGroup_SysPayrollType)(payrollProduct.SysPayrollTypeLevel3 ?? 0),
                        SysPayrollTypeLevel4 = (TermGroup_SysPayrollType)(payrollProduct.SysPayrollTypeLevel4 ?? 0),
                        ScheduleTimeMinutes = predefinedValue ? (int?)null : scheduleTime,
                        AbsenceTimeMinutes = predefinedValue ? (int?)null : absenceTime,
                        EmployeeNr = employee.EmployeeNr,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = employee.EmployeeId,
                        ProductId = payrollProduct.ProductId,
                    };

                    rowDtos.Add(payrollStartValueRow);


                    itemsAdded++;

                }

                int getIndex(ExcelColumnPayrollStartValue enumColumn)
                {
                    return (int)enumColumn - 1;
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result = new ActionResult(ex);
            }

            return (result, rowDtos);
        }
        private ActionResult ImportPayrollStartValues(DataRowCollection rows, int actorCompanyId, Dictionary<string, string> parameters)
        {
            ActionResult result = new ActionResult();

            #region Prereq        

            if (!IsColumnsMatching(rows, typeof(ExcelColumnPayrollStartValue)))
                return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

            //Parameter: dateFrom, dateTo (mandatory)
            DateTime? dateFrom = parameters.ContainsKey("dateFrom") ? CalendarUtility.GetDateTime(parameters["dateFrom"]) : (DateTime?)null;
            DateTime? dateTo = parameters.ContainsKey("dateTo") ? CalendarUtility.GetDateTime(parameters["dateTo"]) : (DateTime?)null;
            if (!dateFrom.HasValue || !dateTo.HasValue)
                return new ActionResult(false, (int)ActionResultSave.Unknown, GetText(10058, "Datum måste anges"));
            if (!dateFrom.HasValue || !dateTo.HasValue)
                return new ActionResult(false, (int)ActionResultSave.Unknown, GetText(10059, "Land måste anges"));

            //Parameter: sysCountryId (mandatory)
            int? sysCountryId = parameters.ContainsKey("sysCountryId") ? StringUtility.GetInt(parameters["sysCountryId"]) : (int?)null;
            List<SysPayrollStartValue> sysPayrollStartValues = null;
            if (sysCountryId.HasValue)
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                sysPayrollStartValues = (from spsv in sysEntitiesReadOnly.SysPayrollStartValue
                                         where spsv.SysCountryId == sysCountryId.Value
                                         select spsv).ToList();
            }
            if (sysPayrollStartValues == null || sysPayrollStartValues.Count == 0)
                return new ActionResult(false, (int)ActionResultSave.Unknown, GetText(10060, "Startvärden för det valda landet stöds inte"));

            //Parameters: importedFrom (optional)
            string importedFrom = parameters.ContainsKey("importedFrom") ? parameters["importedFrom"] : null;

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                //Company
                this.company = CompanyManager.GetCompany(entities, actorCompanyId, true);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                if (PayrollManager.GetPayrollStartValueHead(entities, actorCompanyId) != null)
                    return new ActionResult(false, (int)ActionResultSave.Unknown, GetText(8630, "En import existerar redan, den måste tas bort innan en ny import kan göras"));


                PayrollStartValueHead payrollStartValueHead = null;
                List<PayrollProduct> payrollProducts = new List<PayrollProduct>();

                #endregion

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        foreach (DataRow row in rows)
                        {
                            try
                            {
                                #region Init

                                this.rowNr++;

                                //First row is a header row for semantics in excel spreadsheet
                                if (this.rowNr == 1)
                                    continue;

                                #endregion

                                #region Columns

                                var columnCode = ExcelUtil.GetColumn(row, ExcelColumnPayrollStartValue.Code);
                                int code = StringUtility.GetInt(columnCode);

                                var columnAppellation = ExcelUtil.GetColumn(row, ExcelColumnPayrollStartValue.Appellation);
                                string appellation = StringUtility.GetStringValue(columnAppellation);

                                var columnEmployeeNr = ExcelUtil.GetColumn(row, ExcelColumnPayrollStartValue.EmployeeNr);
                                string employeeNr = StringUtility.GetStringValue(columnEmployeeNr);

                                var columnQuantity = ExcelUtil.GetColumn(row, ExcelColumnPayrollStartValue.Quantity);
                                decimal quantity = StringUtility.GetDecimal(columnQuantity);

                                var columnAmount = ExcelUtil.GetColumn(row, ExcelColumnPayrollStartValue.Amount);
                                decimal amount = StringUtility.GetDecimal(columnAmount);

                                var columnPayrollProductNr = ExcelUtil.GetColumn(row, ExcelColumnPayrollStartValue.PayrollProductNr);
                                string payrollProductNr = StringUtility.GetStringValue(columnPayrollProductNr);

                                var columnDate = ExcelUtil.GetColumn(row, ExcelColumnPayrollStartValue.Date);
                                DateTime? date = CalendarUtility.GetNullableDateTime(columnDate);

                                var columnScheduleTime = ExcelUtil.GetColumn(row, ExcelColumnPayrollStartValue.ScheduleTime);
                                int scheduleTime = StringUtility.GetInt(columnScheduleTime);

                                var columnAbsenceTime = ExcelUtil.GetColumn(row, ExcelColumnPayrollStartValue.AbsenceTime);
                                int absenceTime = StringUtility.GetInt(columnAbsenceTime);

                                #endregion

                                #region Validation

                                bool predefinedValue = code > 0;

                                //Assume row is not used, skip row
                                if (String.IsNullOrEmpty(employeeNr) && quantity == 0 && amount == 0)
                                    continue;

                                if (predefinedValue && String.IsNullOrEmpty(appellation))
                                    return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10061, "Obligatoriska kolumner saknar värde. Rad {0}, kolumn {1}"), this.rowNr, "B"));
                                if (String.IsNullOrEmpty(employeeNr))
                                    return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10061, "Obligatoriska kolumner saknar värde. Rad {0}, kolumn {1}"), this.rowNr, "C"));
                                if (quantity == 0)
                                    return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10061, "Obligatoriska kolumner saknar värde. Rad {0}, kolumn {1}"), this.rowNr, "D"));
                                if (!predefinedValue && !date.HasValue)
                                    return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10061, "Obligatoriska kolumner saknar värde. Rad {0}, kolumn {1}"), this.rowNr, "G"));
                                if (date.HasValue && (date.Value < dateFrom || date.Value > dateTo))
                                    return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(9984, "Datumet {0} ligger utanför importens datumintervall. Rad {1}, kolumn {2}"), date.Value.ToShortDateString(), this.rowNr, "G"));

                                Employee employee = EmployeeManager.GetEmployeeByNr(entities, employeeNr, actorCompanyId, onlyActive: false);
                                if (employee == null)
                                    return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10067, "Anställd {0} hittades inte. Rad {1}"), employeeNr, this.rowNr));

                                if (predefinedValue && String.IsNullOrEmpty(payrollProductNr))
                                {
                                    //Get default from SysPayrollStartValue
                                    var sysPayrollStartValue = sysPayrollStartValues.FirstOrDefault(i => i.SysPayrollStartValueId == code);
                                    if (sysPayrollStartValue == null || String.IsNullOrEmpty(sysPayrollStartValue.PayrollProductNr))
                                        return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10062, "Löneart hittades varken i fil eller som standard. Rad {0}"), this.rowNr));

                                    payrollProductNr = sysPayrollStartValue.PayrollProductNr;
                                }

                                PayrollProduct payrollProduct = payrollProducts.FirstOrDefault(i => i.Number == payrollProductNr);
                                if (payrollProduct == null)
                                {
                                    payrollProduct = ProductManager.GetPayrollProductByNumber(entities, payrollProductNr, actorCompanyId);
                                    if (payrollProduct == null)
                                        return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10062, "Löneart {0} hittades inte. Rad {1}"), payrollProductNr, this.rowNr));

                                    payrollProducts.Add(payrollProduct);
                                }

                                #endregion

                                #region PayrollStartValueHead

                                if (payrollStartValueHead == null)
                                {
                                    payrollStartValueHead = new PayrollStartValueHead()
                                    {
                                        DateFrom = dateFrom.Value,
                                        DateTo = dateTo.Value,
                                        ImportedFrom = importedFrom,

                                        //Set FK
                                        ActorCompanyId = actorCompanyId,
                                    };
                                    SetCreatedProperties(payrollStartValueHead);
                                    entities.PayrollStartValueHead.AddObject(payrollStartValueHead);

                                    if (payrollStartValueHead.PayrollStartValueRow == null)
                                        payrollStartValueHead.PayrollStartValueRow = new EntityCollection<PayrollStartValueRow>();
                                }

                                #endregion

                                #region PayrollStartValueRow

                                if (predefinedValue && payrollStartValueHead.PayrollStartValueRow.Any(i => i.SysPayrollStartValueId == code && i.EmployeeId == employee.EmployeeId))
                                    return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(10063, "Fil innehåller flera rader med samma kod. Rad {0}"), this.rowNr));

                                var payrollStartValueRow = new PayrollStartValueRow()
                                {
                                    SysPayrollStartValueId = code,
                                    Quantity = quantity,
                                    Amount = amount,
                                    Date = predefinedValue ? dateTo.Value : date.Value,
                                    SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                                    SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                                    SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                                    SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,
                                    ScheduleTimeMinutes = predefinedValue ? (int?)null : scheduleTime,
                                    AbsenceTimeMinutes = predefinedValue ? (int?)null : absenceTime,

                                    //Set FK
                                    ActorCompanyId = actorCompanyId,
                                    EmployeeId = employee.EmployeeId,
                                    ProductId = payrollProduct.ProductId,
                                };
                                SetCreatedProperties(payrollStartValueRow);
                                payrollStartValueHead.PayrollStartValueRow.Add(payrollStartValueRow);

                                #endregion

                                #region Save

                                result = SaveChanges(entities);
                                if (!result.Success)
                                    return result;

                                //Update counters
                                itemsAdded++;

                                #endregion
                            }
                            catch (Exception ex)
                            {
                                base.LogError(ex, this.log);
                                result = new ActionResult(ex);
                            }
                        }

                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.PayrollStartValue);

            return result;
        }

        private ActionResult ImportAccounts(DataRowCollection rows, int actorCompanyId, bool doNotModifyWithEmpty = false)
        {
            ActionResult result = new ActionResult();

            #region Prereq

            if (!IsColumnsMatching(rows, typeof(ExcelColumnAccount)))
                return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

            //Company
            this.company = CompanyManager.GetCompany(actorCompanyId, true);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(actorCompanyId, false, false, true);
            List<Account> accountStds = AccountManager.GetAccountsStdsByCompany(actorCompanyId, loadAccount: true);
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(actorCompanyId, true);
            List<SysVatAccount> sysVatAccounts = SysDbCache.Instance.SysVatAccounts;
            List<SysAccountSruCode> sysAccountSruCodes = SysDbCache.Instance.SysAccountSruCodes;

            #endregion

            foreach (DataRow row in rows)
            {
                using (CompEntities entities = new CompEntities())
                {
                    try
                    {
                        #region Init

                        this.rowNr++;

                        //First row is a header row for semantics in excel spreadsheet
                        if (this.rowNr == 1)
                            continue;

                        #endregion

                        #region Mandatory columns

                        var columnAccountNr = ExcelUtil.GetColumnValue(row, ExcelColumnAccount.AccountNr);
                        var columnAccountName = ExcelUtil.GetColumnValue(row, ExcelColumnAccount.AccountName);
                        var columnAccountType = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountType);
                        var columnAccountDimNr = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDimNr);

                        //Validate exists
                        if (columnAccountNr == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnAccount.AccountNr));
                        if (columnAccountName == null)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnAccount.AccountName));
                        if (columnAccountType == null && StringUtility.GetInt(columnAccountDimNr, 1) == 1)
                            AddError(ErrorMessageType.MandatoryColumnMissing, GetColumnName(ExcelColumnAccount.AccountType));

                        //Validate value
                        if (StringUtility.GetInt(columnAccountDimNr, 1) == 1 && !EnumUtility.GetValue(columnAccountType, out _, typeof(TermGroup_AccountType)))
                            AddError(ErrorMessageType.MandatoryColumnInvalid, GetColumnName(ExcelColumnAccount.AccountType));

                        if (HasErrors())
                            continue;

                        #endregion

                        #region Optional columns

                        var columnDescription = ExcelUtil.GetColumn(row, ExcelColumnAccount.Description);
                        var columnVatAccountNr = ExcelUtil.GetColumn(row, ExcelColumnAccount.VatAccountNr);
                        var columnIsAccrualAccount = ExcelUtil.GetColumn(row, ExcelColumnAccount.IsAccrualAccount);
                        var columnAmountStop = ExcelUtil.GetColumn(row, ExcelColumnAccount.AmountStop);
                        var columnRowTextStop = ExcelUtil.GetColumn(row, ExcelColumnAccount.RowTextStop);
                        var columnUnitStop = ExcelUtil.GetColumn(row, ExcelColumnAccount.UnitStop);
                        var columnUnit = ExcelUtil.GetColumn(row, ExcelColumnAccount.Unit);
                        var columnSRUCode1 = ExcelUtil.GetColumn(row, ExcelColumnAccount.SRUCode1);
                        var columnSRUCode2 = ExcelUtil.GetColumn(row, ExcelColumnAccount.SRUCode2);
                        var columnAccountDim2Nr = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim2Nr);
                        var columnAccountDim2Default = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim2Default);
                        var columnAccountDim2MandatoryLevel = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim2MandatoryLevel);
                        var columnAccountDim3Nr = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim3Nr);
                        var columnAccountDim3Default = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim3Default);
                        var columnAccountDim3MandatoryLevel = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim3MandatoryLevel);
                        var columnAccountDim4Nr = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim4Nr);
                        var columnAccountDim4Default = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim4Default);
                        var columnAccountDim4MandatoryLevel = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim4MandatoryLevel);
                        var columnAccountDim5Nr = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim5Nr);
                        var columnAccountDim5Default = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim5Default);
                        var columnAccountDim5MandatoryLevel = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim5MandatoryLevel);
                        var columnAccountDim6Nr = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim6Nr);
                        var columnAccountDim6Default = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim6Default);
                        var columnAccountDim6MandatoryLevel = ExcelUtil.GetColumn(row, ExcelColumnAccount.AccountDim6MandatoryLevel);
                        var columnExcludeVatVerification = ExcelUtil.GetColumn(row, ExcelColumnAccount.ExcludeVatVerification);


                        #endregion

                        #region Account                        

                        int accountDimId = accountDims.Where(i => i.AccountDimNr == StringUtility.GetInt(columnAccountDimNr, 1)).Select(i => i.AccountDimId).FirstOrDefault();
                        Account account = AccountManager.GetAccountByNr(entities, StringUtility.GetStringValue(columnAccountNr), accountDimId, actorCompanyId, loadAccount: true, loadAccountDim: true, loadAccountMapping: true, loadAccountSru: true);

                        bool newAccount = account == null;
                        bool currentDoNotModifyWithEmpty = !newAccount && doNotModifyWithEmpty;

                        if (account == null)
                        {
                            #region Add

                            //Add Account
                            account = new Account()
                            {
                                AccountNr = columnAccountNr.ToString(),
                                ActorCompanyId = actorCompanyId,
                                AccountStd = new AccountStd(),
                            };
                            SetCreatedProperties(account);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(account);

                            #endregion
                        }

                        //Mandatory
                        account.Name = columnAccountName.ToString();
                        account.AccountStd.AccountTypeSysTermId = StringUtility.GetInt(columnAccountType);

                        //Optional

                        account.Description = StringUtility.ModifyValue(account.Description, StringUtility.GetStringValue(columnDescription), currentDoNotModifyWithEmpty);
                        account.AccountStd.isAccrualAccount = StringUtility.GetNullableBool(columnIsAccrualAccount);
                        account.AccountStd.AmountStop = StringUtility.GetInt(columnAmountStop, 1);
                        account.AccountStd.RowTextStop = StringUtility.GetBool(columnRowTextStop);
                        account.AccountStd.UnitStop = StringUtility.GetBool(columnUnitStop);
                        account.AccountStd.Unit = StringUtility.ModifyValue(account.AccountStd.Unit, StringUtility.GetStringValue(columnUnit), currentDoNotModifyWithEmpty);
                        account.AccountStd.ExcludeVatVerification = StringUtility.GetNullableBool(columnExcludeVatVerification);

                        //sysVatAccountId
                        int vatAccountNr = StringUtility.GetInt(columnVatAccountNr);
                        account.AccountStd.SysVatAccountId = sysVatAccounts.Where(i => i.VatNr1 == vatAccountNr).Select(i => i.SysVatAccountId).FirstOrDefault();

                        //SRU codes                        
                        string sruCode1 = StringUtility.GetStringValue(columnSRUCode1);
                        if (sruCode1 != "" && sruCode1 != "0" && sysAccountSruCodes.Any(s => s.SruCode == sruCode1))
                        {
                            AccountSru accountSru = new AccountSru();
                            accountSru.AccountStd = account.AccountStd;
                            accountSru.SysAccountSruCodeId = sysAccountSruCodes.FirstOrDefault(s => s.SruCode == sruCode1)?.SysAccountSruCodeId ?? 0;
                            account.AccountStd.AccountSru.Add(accountSru);
                        }

                        string sruCode2 = StringUtility.GetStringValue(columnSRUCode2);
                        if (sruCode2 != "" && sruCode2 != "0" && sysAccountSruCodes.Any() && sysAccountSruCodes.Any(s => s.SruCode == sruCode2))
                        {
                            AccountSru accountSru = new AccountSru();
                            accountSru.AccountStd = account.AccountStd;
                            accountSru.SysAccountSruCodeId = sysAccountSruCodes.FirstOrDefault(s => s.SruCode == sruCode2)?.SysAccountSruCodeId ?? 0;
                            account.AccountStd.AccountSru.Add(accountSru);
                        }

                        #endregion

                        #region Save

                        result = newAccount
                            ? AccountManager.AddAccount(entities, account, accountDimId, actorCompanyId, base.UserId)
                            : AccountManager.UpdateAccount(account, actorCompanyId, base.UserId);

                        if (!result.Success)
                        {
                            AddError(ErrorMessageType.SaveFailed, identifier: String.Format("{0}. {1}", account.AccountNr, account.Name));
                            continue;
                        }

                        List<AccountMapping> accountMappings = new List<AccountMapping>();

                        AccountMapping accountMapping2 = GetAccountMapping(account.AccountId, accountDims, StringUtility.GetInt(columnAccountDim2Nr), StringUtility.GetStringValue(columnAccountDim2Default), StringUtility.GetInt(columnAccountDim2MandatoryLevel, 0), accountInternals);
                        if (accountMapping2 != null)
                            accountMappings.Add(accountMapping2);

                        AccountMapping accountMapping3 = GetAccountMapping(account.AccountId, accountDims, StringUtility.GetInt(columnAccountDim3Nr), StringUtility.GetStringValue(columnAccountDim3Default), StringUtility.GetInt(columnAccountDim3MandatoryLevel, 0), accountInternals);
                        if (accountMapping3 != null)
                            accountMappings.Add(accountMapping3);

                        AccountMapping accountMapping4 = GetAccountMapping(account.AccountId, accountDims, StringUtility.GetInt(columnAccountDim4Nr), StringUtility.GetStringValue(columnAccountDim4Default), StringUtility.GetInt(columnAccountDim4MandatoryLevel, 0), accountInternals);
                        if (accountMapping4 != null)
                            accountMappings.Add(accountMapping4);

                        AccountMapping accountMapping5 = GetAccountMapping(account.AccountId, accountDims, StringUtility.GetInt(columnAccountDim5Nr), StringUtility.GetStringValue(columnAccountDim5Default), StringUtility.GetInt(columnAccountDim5MandatoryLevel, 0), accountInternals);
                        if (accountMapping5 != null)
                            accountMappings.Add(accountMapping5);

                        AccountMapping accountMapping6 = GetAccountMapping(account.AccountId, accountDims, StringUtility.GetInt(columnAccountDim6Nr), StringUtility.GetStringValue(columnAccountDim6Default), StringUtility.GetInt(columnAccountDim6MandatoryLevel, 0), accountInternals);
                        if (accountMapping6 != null)
                            accountMappings.Add(accountMapping6);

                        if (accountMappings.Any())
                        {
                            foreach (AccountMapping accountMapping in accountMappings)
                            {
                                AccountMapping existingAccountMapping = AccountManager.GetAccountMapping(account.AccountId, accountMapping.AccountDimId, actorCompanyId, true, true, true);
                                if (existingAccountMapping != null)
                                    AccountManager.UpdateAccountMapping(entities, existingAccountMapping, actorCompanyId, accountMapping.MandatoryLevel ?? 0, accountMapping.DefaultAccountId ?? 0);
                                else
                                    AccountManager.AddAccountMapping(entities, accountMapping, account.AccountId, accountMapping.AccountDimId, accountMapping.DefaultAccountId ?? 0, actorCompanyId);
                            }
                        }

                        //Update counters
                        if (newAccount)
                            itemsAdded++;
                        else
                            itemsUpdated++;

                        //Add to collection
                        if (newAccount)
                            accountStds.Add(account);

                        #endregion

                        #region Save relations



                        #endregion
                    }
                    catch (ArgumentException ax)
                    {
                        //Break import
                        AddTerminationError(TerminationMessageType.FieldMissing, exception: ax);
                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());
                    }
                    catch (Exception ex)
                    {
                        AddError(ErrorMessageType.UnkownError, exception: ex);
                    }
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.Accounts);

            return result;

        }

        private ActionResult ImportTaxDeductionContacts(DataRowCollection rows, int actorCompanyId, bool doNotModifyWithEmpty = false)
        {
            ActionResult result = new ActionResult();

            #region Prereq        

            if (!IsColumnsMatching(rows, typeof(ExcelColumnTaxDeductionContact)))
                return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                //Company
                this.company = CompanyManager.GetCompany(entities, actorCompanyId, true);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Customers
                List<Customer> customers = CustomerManager.GetCustomersByCompany(entities, this.company.ActorCompanyId, onlyActive: false);

                #endregion

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        foreach (DataRow row in rows)
                        {
                            try
                            {
                                #region Init

                                this.rowNr++;

                                //First row is a header row for semantics in excel spreadsheet
                                if (this.rowNr == 1)
                                    continue;

                                #endregion

                                #region Columns

                                var columnCustomerNr = ExcelUtil.GetColumn(row, ExcelColumnTaxDeductionContact.CustomerNr);
                                string customerNr = StringUtility.GetStringValue(columnCustomerNr);

                                var columnCustomerName = ExcelUtil.GetColumn(row, ExcelColumnTaxDeductionContact.CustomerName);
                                string customerName = StringUtility.GetStringValue(columnCustomerName);

                                var columnSocialSeqNr = ExcelUtil.GetColumn(row, ExcelColumnTaxDeductionContact.SocialSeqNr);
                                string socialSeqNr = StringUtility.GetStringValue(columnSocialSeqNr);

                                var columnName = ExcelUtil.GetColumn(row, ExcelColumnTaxDeductionContact.Name);
                                string name = StringUtility.GetStringValue(columnName);

                                var columnProperty = ExcelUtil.GetColumn(row, ExcelColumnTaxDeductionContact.Property);
                                string property = StringUtility.GetStringValue(columnProperty);

                                var columnApartmentNr = ExcelUtil.GetColumn(row, ExcelColumnTaxDeductionContact.ApartmentNr);
                                string apartmentNr = StringUtility.GetStringValue(columnApartmentNr);

                                var columnCooperativeOrgNr = ExcelUtil.GetColumn(row, ExcelColumnTaxDeductionContact.CooperativeOrgNr);
                                string cooperativeOrgNr = StringUtility.GetStringValue(columnCooperativeOrgNr);

                                #endregion

                                #region Validation

                                //Assume row is not used, skip row
                                if ((String.IsNullOrEmpty(customerNr) && String.IsNullOrEmpty(customerName)) || String.IsNullOrEmpty(socialSeqNr) || String.IsNullOrEmpty(name))
                                    continue;

                                // Get customer by number
                                Customer customer = customers.FirstOrDefault(c => c.CustomerNr == customerNr);
                                if (customer == null)
                                {
                                    // Get customer by name
                                    customer = customers.FirstOrDefault(c => c.Name == customerName);
                                    if (customer == null)
                                    {
                                        AddWarning(WarningMessageType.CustomerMissing, identifier: customerNr + " " + customerName);
                                        continue;
                                    }
                                    //return new ActionResult(false, (int)ActionResultSave.Unknown, String.Format(GetText(7615, "Kund {0} hittades inte. Rad {1}"), customerNr, this.rowNr));
                                }

                                var existingContacts = CustomerManager.GetHouseholdTaxDeductionApplicants(entities, customer.ActorCustomerId);
                                var currentContact = existingContacts.FirstOrDefault(c => c.SocialSecNr == socialSeqNr);

                                #endregion

                                #region PayrollStartValueHead

                                if (currentContact == null)
                                {
                                    currentContact = new HouseholdTaxDeductionApplicant()
                                    {
                                        Name = name,
                                        Property = property,
                                        ApartmentNr = apartmentNr,
                                        CooperativeOrgNr = cooperativeOrgNr,
                                        SocialSecNr = socialSeqNr,

                                        //Set FK
                                        ActorCustomerId = customer.ActorCustomerId,
                                    };

                                    SetCreatedProperties(currentContact);
                                    entities.HouseholdTaxDeductionApplicant.AddObject(currentContact);
                                }
                                else
                                {
                                    if (!doNotModifyWithEmpty || !String.IsNullOrEmpty(name))
                                        currentContact.Name = name;
                                    if (!doNotModifyWithEmpty || !String.IsNullOrEmpty(property))
                                        currentContact.Property = property;
                                    if (!doNotModifyWithEmpty || !String.IsNullOrEmpty(apartmentNr))
                                        currentContact.ApartmentNr = apartmentNr;
                                    if (!doNotModifyWithEmpty || !String.IsNullOrEmpty(cooperativeOrgNr))
                                        currentContact.CooperativeOrgNr = cooperativeOrgNr;
                                    if (!doNotModifyWithEmpty || !String.IsNullOrEmpty(socialSeqNr))
                                        currentContact.SocialSecNr = socialSeqNr;

                                    SetModifiedProperties(currentContact);
                                }

                                #endregion

                                #region Save

                                result = SaveChanges(entities);
                                if (!result.Success)
                                    return result;

                                //Update counters
                                itemsAdded++;

                                #endregion
                            }
                            catch (Exception ex)
                            {
                                base.LogError(ex, this.log);
                                result = new ActionResult(ex);
                            }
                        }

                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.TaxDeductionContacts);

            return result;
        }

        private ActionResult ImportPricelists(DataRowCollection rows, int actorCompanyId, bool doNotModifyWithEmpty = false)
        {
            ActionResult result = new ActionResult();

            #region Prereq        

            if (!IsColumnsMatching(rows, typeof(ExcelColumnPricelist)))
                return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                // Company
                this.company = CompanyManager.GetCompany(entities, actorCompanyId, true);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                // Customers
                var existingPricelistTypes = ProductPricelistManager.GetPriceListTypes(entities, actorCompanyId, true);

                // Currencies
                var currencies = CountryCurrencyManager.GetCurrenciesWithSysCurrency(entities, actorCompanyId);

                // Products
                var products = ProductManager.GetProductsSmall(entities, actorCompanyId, true);

                #endregion

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        try
                        {
                            #region Parse

                            var priceListObjects = new List<ImportPriceList>();
                            foreach (DataRow row in rows)
                            {
                                #region Init

                                this.rowNr++;

                                //First row is a header row for semantics in excel spreadsheet
                                if (this.rowNr == 1)
                                    continue;

                                #endregion

                                var nameColumn = ExcelUtil.GetColumn(row, ExcelColumnPricelist.Name);
                                var descriptionColumn = ExcelUtil.GetColumn(row, ExcelColumnPricelist.Description);
                                var currencyColumn = ExcelUtil.GetColumn(row, ExcelColumnPricelist.Currency);
                                var productNrColumn = ExcelUtil.GetColumn(row, ExcelColumnPricelist.ProductNr);
                                var productNameColumn = ExcelUtil.GetColumn(row, ExcelColumnPricelist.ProductName);
                                var priceColumn = ExcelUtil.GetColumn(row, ExcelColumnPricelist.Price);
                                var quantityColumn = ExcelUtil.GetColumn(row, ExcelColumnPricelist.Quantity);
                                var startDateColumn = ExcelUtil.GetColumn(row, ExcelColumnPricelist.StartDate);
                                var endDateColumn = ExcelUtil.GetColumn(row, ExcelColumnPricelist.EndDate);

                                priceListObjects.Add(new ImportPriceList()
                                {
                                    Name = StringUtility.GetStringValue(nameColumn).Trim(),
                                    Description = StringUtility.GetStringValue(descriptionColumn).Trim(),
                                    Currency = StringUtility.GetStringValue(currencyColumn).Trim(),
                                    ProductNr = StringUtility.GetStringValue(productNrColumn).Trim(),
                                    ProductName = StringUtility.GetStringValue(productNameColumn).Trim(),
                                    Price = StringUtility.GetStringValue(priceColumn).Trim(),
                                    Quantity = StringUtility.GetStringValue(quantityColumn).Trim(),
                                    StartDate = StringUtility.GetStringValue(startDateColumn).Trim(),
                                    EndDate = StringUtility.GetStringValue(endDateColumn).Trim()
                                });
                            }

                            #endregion

                            #region Create PriceListType and PriceLists

                            var priceListTypeGroups = priceListObjects
                                .GroupBy(p => new { p.Name, p.Currency })
                                .Select(g => new { g.Key.Name, g.Key.Currency, PriceListTypes = g })
                                .GroupBy(g => g.Currency);

                            foreach (var priceListTypeGroupCurrency in priceListTypeGroups)
                            {
                                foreach (var priceListTypeGroup in priceListTypeGroupCurrency)
                                {
                                    PriceListType existingPriceListType = null;
                                    foreach (var priceListType in priceListTypeGroup.PriceListTypes)
                                    {
                                        if (String.IsNullOrEmpty(priceListType.Name) || String.IsNullOrEmpty(priceListType.Currency))
                                            break;

                                        var currency = currencies.FirstOrDefault(c => c.Code == priceListType.Currency);
                                        if (currency == null)
                                            break;

                                        if (existingPriceListType == null)
                                        {
                                            existingPriceListType = existingPricelistTypes.FirstOrDefault(p => p.Name == priceListType.Name && p.Currency.CurrencyId == currency.CurrencyId);
                                            if (existingPriceListType == null)
                                            {
                                                existingPriceListType = new PriceListType()
                                                {
                                                    Name = priceListType.Name,
                                                    Description = priceListType.Description,

                                                    // References
                                                    Company = company,
                                                    Currency = currency,
                                                };

                                                SetCreatedProperties(existingPriceListType);

                                                result = AddEntityItem(entities, existingPriceListType, "PriceListType");
                                                if (!result.Success)
                                                    return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());

                                                //Update counters
                                                itemsAdded++;
                                            }
                                            else
                                            {
                                                if (!doNotModifyWithEmpty || !String.IsNullOrEmpty(priceListType.Description))
                                                    existingPriceListType.Description = priceListType.Description;

                                                existingPriceListType.Currency = currency;

                                                SetModifiedProperties(existingPriceListType);

                                                //Update counters
                                                itemsUpdated++;
                                            }
                                        }

                                        var product = products.FirstOrDefault(p => p.Number == priceListType.ProductNr);
                                        if (product != null)
                                        {
                                            if (String.IsNullOrEmpty(priceListType.Price))
                                                continue;

                                            Decimal.TryParse(priceListType.Price.Replace('.', ','), out decimal price);
                                            Decimal.TryParse(priceListType.Quantity.Replace('.', ','), out decimal quantity);
                                            DateTime.TryParse(priceListType.StartDate, out DateTime start);
                                            DateTime.TryParse(priceListType.EndDate, out DateTime stop);

                                            var priceListQuery = (from p in existingPriceListType.PriceList
                                                                  where p.ProductId == product.ProductId &&
                                                                  p.Price == price &&
                                                                  p.Quantity == quantity
                                                                  select p);

                                            if (start == DateTime.MinValue)
                                                priceListQuery = priceListQuery.Where(p => p.StartDate == CalendarUtility.DATETIME_DEFAULT);
                                            else
                                                priceListQuery = priceListQuery.Where(p => p.StartDate == start);

                                            if (stop == DateTime.MinValue)
                                                priceListQuery = priceListQuery.Where(p => p.StopDate == new DateTime(9999, 01, 01));
                                            else
                                                priceListQuery = priceListQuery.Where(p => p.StopDate == stop);

                                            var priceList = priceListQuery.FirstOrDefault();
                                            if (priceList == null)
                                            {
                                                priceList = new PriceList()
                                                {
                                                    Price = price,
                                                    Quantity = quantity,
                                                    DiscountPercent = 0,
                                                    StartDate = start == DateTime.MinValue ? CalendarUtility.DATETIME_DEFAULT : start,
                                                    StopDate = stop == DateTime.MinValue ? new DateTime(9999, 01, 01) : stop,

                                                    // References
                                                    PriceListType = existingPriceListType,
                                                    ProductId = product.ProductId,
                                                };

                                                SetCreatedProperties(priceList);

                                                result = AddEntityItem(entities, priceList, "PriceList");
                                                if (!result.Success)
                                                    return new ActionResult(false, (int)ActionResultSave.NothingSaved, GetImportFailedMessage());
                                            }
                                            else
                                            {
                                                if (!doNotModifyWithEmpty || price != 0)
                                                    priceList.Price = price;
                                                if (!doNotModifyWithEmpty || quantity != 0)
                                                    priceList.Quantity = quantity;

                                                priceList.StartDate = start == DateTime.MinValue ? CalendarUtility.DATETIME_DEFAULT : start;
                                                priceList.StopDate = stop == DateTime.MinValue ? new DateTime(9999, 01, 01) : stop;

                                                SetModifiedProperties(priceList);
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region Save

                            result = SaveChanges(entities);
                            if (!result.Success)
                                return result;

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            base.LogError(ex, this.log);
                            result = new ActionResult(ex);
                        }

                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.Pricelists);

            return result;
        }

        private ActionResult ImportAgreements(DataRowCollection rows, int actorCompanyId, bool doNotModifyWithEmpty = false)
        {
            ActionResult result = new ActionResult();

            #region Prereq        

            if (!IsColumnsMatching(rows, typeof(ExcelColumnAgreement)))
                return new ActionResult(false, 0, GetFileInvalidNoOfColumnsMessage());

            #endregion

            #region Parse

            var agreementList = new List<ImportAgreement>();
            foreach (DataRow row in rows)
            {
                #region Init

                this.rowNr++;

                //First row is a header row for semantics in excel spreadsheet
                if (this.rowNr == 1)
                    continue;

                #endregion

                var customerNrColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.CustomerNr);
                var agreementGroupColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.AgreementGroup);
                var internalTextColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.InternalText);
                var markingColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.Marking);
                var agreementNrColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.AgreementNr);
                var deliveryNameColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.DeliveryName);
                var deliveryStreetColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.DeliveryStreet);
                var deliveryPostalCodeColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.DeliveryPostalCode);
                var deliveryPostalAddressColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.DeliveryPostalAddress);
                var nextInvoiceDateColumn = ExcelUtil.GetColumn(row, ExcelColumnAgreement.NextInvoiceDate);
                var category1Column = ExcelUtil.GetColumn(row, ExcelColumnAgreement.Category1);
                var category2Column = ExcelUtil.GetColumn(row, ExcelColumnAgreement.Category2);
                var category3Column = ExcelUtil.GetColumn(row, ExcelColumnAgreement.Category3);

                agreementList.Add(new ImportAgreement()
                {
                    CustomerNr = StringUtility.GetStringValue(customerNrColumn).Trim(),
                    AgreementGroup = StringUtility.GetStringValue(agreementGroupColumn).Trim(),
                    InternalText = StringUtility.GetStringValue(internalTextColumn).Trim(),
                    InvoiceLabel = StringUtility.GetStringValue(markingColumn).Trim(),
                    AgreementNr = StringUtility.GetStringValue(agreementNrColumn).Trim(),
                    DeliveryName = StringUtility.GetStringValue(deliveryNameColumn).Trim(),
                    DeliveryStreet = StringUtility.GetStringValue(deliveryStreetColumn).Trim(),
                    DeliveryPostalCode = StringUtility.GetStringValue(deliveryPostalCodeColumn).Trim(),
                    DeliveryPostalAddress = StringUtility.GetStringValue(deliveryPostalAddressColumn).Trim(),
                    NextInvoiceDate = StringUtility.GetStringValue(nextInvoiceDateColumn).Trim(),
                    Categories = new string[]
    {
                        StringUtility.GetStringValue(category1Column).Trim(),
                        StringUtility.GetStringValue(category2Column).Trim(),
                        StringUtility.GetStringValue(category3Column).Trim()
    }
                });
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                //Customers
                var customers = CustomerManager.GetCustomersByCompanySmall(entities, actorCompanyId, onlyActive: true);

                var contractGroups = ContractManager.GetContractGroups(entities, actorCompanyId);

                var categories = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Contract, actorCompanyId);

                var companyCurrency = CountryCurrencyManager.GetCompanyBaseCurrency(entities, actorCompanyId);

                var account = AccountManager.GetAccountYear(entities, DateTime.Now, actorCompanyId, false);

                int voucherSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceVoucherSeriesType, 0, actorCompanyId, 0);

                var voucherSeries = VoucherManager.GetVoucherSerieByYear(entities, account.AccountYearId, voucherSeriesTypeId);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        try
                        {
                            #region Create Contracts

                            foreach (var data in agreementList)
                            {
                                if (string.IsNullOrEmpty(data.CustomerNr))
                                    continue;

                                var customer = customers.FirstOrDefault(c => c.CustomerNr == data.CustomerNr);

                                if (customer == null)
                                    continue;

                                var contractGroup = contractGroups.FirstOrDefault(cg => cg.Name == data.AgreementGroup);

                                if (contractGroup == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11876, "Avtalsgrupp saknas"));

                                var saveDTO = new CustomerInvoiceSaveDTO
                                {
                                    ActorId = customer.ActorCustomerId,
                                    OriginDescription = data.InternalText,
                                    BillingType = TermGroup_BillingType.Debit,
                                    VatType = TermGroup_InvoiceVatType.None,
                                    OrderType = TermGroup_OrderType.Contract,
                                    RegistrationType = OrderInvoiceRegistrationType.Contract,
                                    ContractGroupId = contractGroup.ContractGroupId,
                                    VoucherSeriesTypeId = voucherSeries.VoucherSeriesTypeId,
                                    VoucherSeriesId = voucherSeries.VoucherSeriesId,
                                    CurrencyId = companyCurrency.CurrencyId,
                                    OriginStatus = SoeOriginStatus.Origin,
                                    ContractNr = data.AgreementNr,
                                    InvoiceLabel = data.InvoiceLabel,
                                    InvoiceDate = string.IsNullOrEmpty(data.StartDate) ? DateTime.Today : CalendarUtility.GetDateTime(data.StartDate, "yyyy-MM-dd"),
                                    NextContractPeriodDate = string.IsNullOrEmpty(data.NextInvoiceDate) ? (DateTime?)null : CalendarUtility.GetDateTime(data.NextInvoiceDate, "yyyy-MM-dd"),
                                    CurrencyDate = DateTime.Today,
                                    CurrencyRate = 1
                                };

                                if (saveDTO.NextContractPeriodDate == null)
                                {
                                    saveDTO.NextContractPeriodDate = ContractManager.GetNextContactPeriodDate(contractGroup, saveDTO.InvoiceDate.Value, out var nextContractPeriodYear, out var nextContractPeriodValue);
                                    saveDTO.NextContractPeriodYear = nextContractPeriodYear;
                                    saveDTO.NextContractPeriodValue = nextContractPeriodValue;
                                }

                                if (!string.IsNullOrEmpty(data.DeliveryStreet))
                                {
                                    result = ContactManager.AddOrFindActorAddress(entities, TermGroup_SysContactAddressType.Delivery, customer.ActorCustomerId, new ContactAdressIODTO
                                    {
                                        Name = data.DeliveryName,
                                        Address = data.DeliveryStreet,
                                        PostalCode = data.DeliveryPostalCode,
                                        PostalAddress = data.DeliveryPostalAddress
                                    });

                                    if (!result.Success)
                                        return result;

                                    saveDTO.DeliveryAddressId = result.IntegerValue;
                                }

                                result = InvoiceManager.SaveCustomerInvoice(entities, transaction, saveDTO, new List<CustomerInvoiceRowDTO>(), new List<AccountingRowDTO>(), actorCompanyId, null, false, false, false, false, true);

                                if (!result.Success)
                                    return result;

                                #region Categories

                                var categoryIds = new List<int>();
                                foreach (var category in data.Categories)
                                {
                                    if (string.IsNullOrEmpty(category))
                                        continue;

                                    var categoryId = categories.FirstOrDefault(c => c.Category.Name == category)?.CategoryId;
                                    if (categoryId != null && !categoryIds.Contains(categoryId.Value))
                                        categoryIds.Add(categoryId.Value);
                                }

                                CategoryManager.SaveCompanyCategoryRecords(entities, transaction, categoryIds, actorCompanyId, SoeCategoryType.Contract, SoeCategoryRecordEntity.Contract, result.IntegerValue);

                                #endregion

                                // Update counters
                                itemsAdded++;
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            base.LogError(ex, this.log);
                            result = new ActionResult(ex);
                        }


                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result = new ActionResult(ex);
                }
                finally
                {
                    if (result.Success)
                    {
                        // Set success properties if needed
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            if (result.Success)
                result.ErrorMessage = GetImportSuccededMessage(ImportType.Agreements);

            return result;
        }

        #endregion

        #region Add relations

        private bool TryAddSupplierAccountStds(CompEntities entities, DataRow row, Supplier supplier, int noOfAccountInternals, bool doNotModifyWithEmpty = false)
        {
            return base.TryAddSupplierAccountStds(entities, supplier, GetSupplierAccounts(entities, row, noOfAccountInternals), doNotModifyWithEmpty);
        }

        private bool TryAddCustomerAccountStds(CompEntities entities, DataRow row, Customer customer, int noOfAccountInternals, bool doNotModifyWithEmpty = false)
        {
            return base.TryAddCustomerAccountStds(entities, customer, GetCustomerAccounts(entities, row, noOfAccountInternals), doNotModifyWithEmpty);
        }

        private bool TryAddProductAccountStds(CompEntities entities, DataRow row, Product product, int noOfAccountInternals, bool doNotModifyWithEmpty = false)
        {
            return base.TryAddProductAccountStds(entities, product, GetProductAccounts(entities, row, noOfAccountInternals), doNotModifyWithEmpty);
        }

        private bool TryAddPaymentInformation(CompEntities entities, DataRow row, Contact contact, bool doNotModifyWithEmpty = false)
        {
            if (row == null || contact == null || contact.Actor == null)
                return false;

            #region Prereq

            if (base.CanEntityLoadReferences(entities, contact.Actor) && !contact.Actor.PaymentInformation.IsLoaded)
                contact.Actor.PaymentInformation.Load();

            //Optional columns
            var columnStandardPaymentType = ExcelUtil.GetColumn(row, ExcelColumnPaymentInformation.StandardPaymentType);

            #endregion

            #region PaymentInformation

            int defaultSysPaymentTypeId = PaymentManager.GetStandardPaymentType(columnStandardPaymentType);

            PaymentInformation paymentInformation = contact.Actor.PaymentInformation.FirstOrDefault(i => i.State == (int)SoeEntityState.Active);
            if (paymentInformation == null)
            {
                #region Add

                paymentInformation = new PaymentInformation()
                {
                    DefaultSysPaymentTypeId = defaultSysPaymentTypeId,

                    // References
                    Actor = contact.Actor,
                };
                SetCreatedProperties(paymentInformation);

                #endregion
            }
            else
            {
                #region Update

                if (IsOkToUpdateValue(doNotModifyWithEmpty, columnStandardPaymentType) && paymentInformation.DefaultSysPaymentTypeId != defaultSysPaymentTypeId)
                {
                    paymentInformation.DefaultSysPaymentTypeId = defaultSysPaymentTypeId;
                    SetModifiedProperties(paymentInformation);
                }

                //Make sure PaymentInformationRow is loaded
                if (!paymentInformation.PaymentInformationRow.IsLoaded)
                    paymentInformation.PaymentInformationRow.Load();

                #endregion
            }

            #endregion

            #region PaymentInformationRow

            TryAddPaymentInformationRow(entities, paymentInformation, defaultSysPaymentTypeId, TermGroup_SysPaymentType.BG, StringUtility.NullToEmpty(ExcelUtil.GetColumn(row, ExcelColumnPaymentInformation.BgNr)), doNotModifyWithEmpty);
            TryAddPaymentInformationRow(entities, paymentInformation, defaultSysPaymentTypeId, TermGroup_SysPaymentType.PG, StringUtility.NullToEmpty(ExcelUtil.GetColumn(row, ExcelColumnPaymentInformation.PgNr)), doNotModifyWithEmpty);
            TryAddPaymentInformationRow(entities, paymentInformation, defaultSysPaymentTypeId, TermGroup_SysPaymentType.Bank, StringUtility.NullToEmpty(ExcelUtil.GetColumn(row, ExcelColumnPaymentInformation.BankNr)), doNotModifyWithEmpty);
            if (defaultSysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
            {
                var bic = StringUtility.NullToEmpty(ExcelUtil.GetColumn(row, ExcelColumnPaymentInformation.Bic));
                var iban = StringUtility.NullToEmpty(ExcelUtil.GetColumn(row, ExcelColumnPaymentInformation.Iban));
                if (bic.Length > 0 && iban.Length > 0)
                    TryAddPaymentInformationRow(entities, paymentInformation, defaultSysPaymentTypeId, TermGroup_SysPaymentType.BIC, string.Concat(bic, "/", iban), doNotModifyWithEmpty);
            }
            if (defaultSysPaymentTypeId == (int)TermGroup_SysPaymentType.SEPA)
            {
                var bic = StringUtility.NullToEmpty(ExcelUtil.GetColumn(row, ExcelColumnPaymentInformation.Bic));
                var iban = StringUtility.NullToEmpty(ExcelUtil.GetColumn(row, ExcelColumnPaymentInformation.Iban));
                if (bic.Length > 0 && iban.Length > 0)
                    TryAddPaymentInformationRow(entities, paymentInformation, defaultSysPaymentTypeId, TermGroup_SysPaymentType.SEPA, string.Concat(bic, "/", iban), doNotModifyWithEmpty);
            }

            #endregion

            return true;
        }

        #endregion

        #region Save relations

        private void TrySaveContactEcom(CompEntities entities, DataRow row, Contact contact, bool doNotModifyWithEmpty = false, bool importGLN = false)
        {
            base.TrySaveContactEcom(entities, contact, GetEComs(row), doNotModifyWithEmpty, importGLN);
        }

        private void TrySaveContactAddresses(CompEntities entities, DataRow row, Contact contact, bool doNotModifyWithEmpty = false)
        {
            base.TrySaveContactAddresses(entities, contact, GetAddresses(row), doNotModifyWithEmpty);
        }

        private void TrySaveCategories(CompEntities entities, DataRow row, int recordId, SoeCategoryType categoryType, SoeCategoryRecordEntity entity, bool secondary, int noOfCategories)
        {
            if (secondary)
                base.TrySaveCategories(entities, recordId, GetImportCategoryRecords(entities, categoryType, GetFieldsFromRow(row, SECONDARYCATEGORY_FIELD_NAME, noOfCategories)), categoryType, entity, secondary);
            else
                base.TrySaveCategories(entities, recordId, GetImportCategoryRecords(entities, categoryType, GetFieldsFromRow(row, CATEGORY_FIELD_NAME, noOfCategories)), categoryType, entity, secondary);
        }

        private void TrySavePaymentInformation(CompEntities entities, DataRow row, Contact contact, bool doNotModifyWithEmpty = false)
        {
            //Add
            if (!TryAddPaymentInformation(entities, row, contact, doNotModifyWithEmpty))
                AddWarning(WarningMessageType.AddPaymentInformationFailed);

            //Save
            var result = SaveChanges(entities);
            if (!result.Success)
                AddWarning(WarningMessageType.SavePaymentInformationFailed);
        }

        #endregion

        #region Help-methods

        private string GetProvider(string pathOnServer)
        {
            string provider = String.Empty;

            //Look for valid extensions
            string extension = Path.GetExtension(pathOnServer).ToLower();
            if (extension == ".xlsx" || extension == ".csv")
                provider = "Microsoft.ACE.OLEDB.12.0";
            else if (extension == ".xls")
                provider = "Microsoft.Jet.OLEDB.4.0";

            return provider;
        }

        private OleDbConnectionStringBuilder GetConnectionString(string provider, string pathOnServer)
        {
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(pathOnServer))
                return null;

            OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();
            connectionStringBuilder.Provider = provider;
            connectionStringBuilder.DataSource = pathOnServer;
            connectionStringBuilder["Extended Properties"] = "Excel 8.0;HDR=No";

            return connectionStringBuilder;
        }

        private DataSet CreateDataSet(string pathOnServer, OleDbConnectionStringBuilder connectionStringBuilder, bool readAsFlatFile)
        {
            if (!File.Exists(pathOnServer))
                return null;

            DataSet ds = null;
            if (readAsFlatFile)
            {
                ds = new DataSet();
                DataTable dt = new DataTable();
                dt.Columns.Add("Code");
                dt.Columns.Add("Description");
                dt.Columns.Add("UseOtherQualifier");
                string csvData;
                using (StreamReader sr = new StreamReader(pathOnServer))
                {
                    csvData = sr.ReadToEnd().ToString();
                    string[] row = csvData.Split('\n');
                    for (int i = 0; i < row.Count(); i++)
                    {
                        if (row[i].Trim().Contains(","))
                        {
                            string[] rowData = row[i].Split(',');
                            if (rowData.Length > 0)
                            {
                                var cd = rowData[0]?.Replace("\n", "").Replace("\r", "") ?? string.Empty;
                                var desc = "";
                                var uoq = "";
                                if (rowData.Length >= 1)
                                {
                                    desc = rowData[1]?.Replace("\n", "").Replace("\r", "") ?? string.Empty;
                                }

                                if (rowData.Length >= 2)
                                {
                                    uoq = rowData[2]?.Replace("\n", "").Replace("\r", "") ?? string.Empty;
                                }
                                if (cd != "" && desc != "")
                                {
                                    DataRow dr = dt.NewRow();
                                    dr["Code"] = cd;
                                    dr["Description"] = desc;
                                    dr["UseOtherQualifier"] = uoq;
                                    dt.Rows.Add(dr);
                                }
                            }
                        }
                    }
                }
                ds.Tables.Add(dt);

            }
            else
            {
                OleDbConnection excelConnection = new OleDbConnection((connectionStringBuilder == null ? string.Empty : connectionStringBuilder.ConnectionString));
                OleDbCommand excelCommand = new OleDbCommand();

                try
                {
                    excelCommand.Connection = excelConnection;

                    //Check if the Sheet Exists
                    excelConnection.Open();

                    //Get the Schema of the WorkBook
                    DataTable excelSchema = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    excelConnection.Close();

                    //Read Data from Sheet
                    excelConnection.Open();

                    //Find Sheet
                    string sheetName = String.Empty;
                    for (int i = 0; i < excelSchema.Rows.Count; i++)
                    {
                        var rawSheetNameObj = excelSchema.Rows[i]["TABLE_NAME"];
                        if (rawSheetNameObj == null)
                            continue;

                        var rawSheetName = rawSheetNameObj.ToString();
                        if (string.IsNullOrEmpty(rawSheetName))
                            continue;

                        // Normalize and whitelist check.
                        var unquoted = rawSheetName.Trim().Trim('\'', '"').Trim();
                        var unquotedLowerForWhitelist = unquoted.ToLowerInvariant();

                        if (!EXCELIMPORT_TABLENAMES.Contains(unquotedLowerForWhitelist))
                            continue;

                        if (!TryGetSafeSheetName(rawSheetName, out string safeSheetName))
                            continue;

                        OleDbDataAdapter adapter = new OleDbDataAdapter();
                        ds = new DataSet();
                        excelCommand.CommandText = GetSelectCommand(safeSheetName);
                        adapter.SelectCommand = excelCommand;
                        adapter.Fill(ds);

                        // intentionally do not break here — allow processing multiple matching sheets
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    ds = null;
                }
                finally
                {
                    //Close connection
                    excelConnection.Close();

                    excelCommand.Dispose();
                    excelConnection.Dispose();
                }
            }

            return ds;
        }


        private DataSet CreateDataSet(string pathOnServer, OleDbConnectionStringBuilder connectionStringBuilder, string tableName)
        {
            if (!File.Exists(pathOnServer))
                return null;

            DataSet ds = null;
            OleDbConnection excelConnection = new OleDbConnection(connectionStringBuilder.ConnectionString);
            OleDbCommand excelCommand = new OleDbCommand();

            try
            {
                excelCommand.Connection = excelConnection;

                //Check if the Sheet Exists
                excelConnection.Open();

                //Get the Schema of the WorkBook
                DataTable excelSchema = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                excelConnection.Close();

                //Read Data from Sheet
                excelConnection.Open();

                //Find Sheet
                for (int i = 0; i < excelSchema.Rows.Count; i++)
                {
                    var rawSheetNameObj = excelSchema.Rows[i]["TABLE_NAME"];
                    if (rawSheetNameObj == null)
                        continue;

                    var rawSheetName = rawSheetNameObj.ToString();
                    if (string.IsNullOrEmpty(rawSheetName))
                        continue;

                    // Normalize and whitelist check.
                    var unquoted = rawSheetName.Trim().Trim('\'', '"').Trim();
                    var unquotedLowerForWhitelist = unquoted.ToLowerInvariant();

                    if (!EXCELIMPORT_TABLENAMES.Contains(unquotedLowerForWhitelist))
                        continue;

                    if (!TryGetSafeSheetName(rawSheetName, out string safeSheetName))
                        continue;

                    // Normalize both names for robust comparison (ignore surrounding quotes/case and optional trailing '$')
                    var unquotedForComparison = unquoted.ToLowerInvariant().TrimEnd('$');
                    var requestedTableNormalized = (tableName ?? string.Empty).Trim().Trim('\'', '"').ToLowerInvariant().TrimEnd('$');

                    if (string.Equals(unquotedForComparison, requestedTableNormalized, StringComparison.OrdinalIgnoreCase))
                    {
                        OleDbDataAdapter adapter = new OleDbDataAdapter();
                        ds = new DataSet();
                        excelCommand.CommandText = GetSelectCommand(safeSheetName);
                        adapter.SelectCommand = excelCommand;
                        adapter.Fill(ds);

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                ds = null;
            }
            finally
            {
                //Close connection
                excelConnection.Close();

                excelCommand.Dispose();
                excelConnection.Dispose();
            }

            return ds;
        }

        private string GetSelectCommand(string sheetName)
        {
            return String.Format("SELECT * FROM [{0}]", sheetName);
        }

        private bool TryGetSafeSheetName(string rawSheetName, out string safeSheetName)
        {
            safeSheetName = null;
            if (string.IsNullOrEmpty(rawSheetName))
                return false;
            // Remove any surrounding quotes
            var unquoted = rawSheetName.Trim().Trim('\'', '"').Trim();
            // Ensure it ends with '$' or '$''
            if (!unquoted.EndsWith("$") && !unquoted.EndsWith("$'") && !unquoted.EndsWith("$\""))
                return false;
            safeSheetName = unquoted;
            return true;
        }

        private string[] GetFieldsFromRow(DataRow row, string prefix, int nrOf)
        {
            string[] names = new string[nrOf];

            for (int i = 0; i < nrOf; i++)
            {
                var column = ExcelUtil.GetColumnValue(row, prefix + (i + 1));
                if (StringUtility.HasValue(column))
                    names[i] = column.ToString();
            }

            return names;
        }

        private List<ImportAccount> GetSupplierAccounts(CompEntities entities, DataRow row, int noOfAccountInternals)
        {
            List<ImportAccount> accounts = new List<ImportAccount>();

            foreach (SupplierAccountType type in Enum.GetValues(typeof(SupplierAccountType)))
            {
                string prefix = "";
                switch (type)
                {
                    case SupplierAccountType.Credit:
                        prefix = "Credit";
                        break;
                    case SupplierAccountType.Debit:
                        prefix = "Debit";
                        break;
                    case SupplierAccountType.VAT:
                        prefix = "Vat";
                        break;
                    case SupplierAccountType.Interim:
                        prefix = "Interim";
                        break;
                }

                //Load accounts
                accounts.Add(GetAccount(entities, row, (int)type, prefix, noOfAccountInternals));
            }

            return accounts;
        }

        private List<ImportAccount> GetCustomerAccounts(CompEntities entities, DataRow row, int noOfAccountInternals)
        {
            List<ImportAccount> accounts = new List<ImportAccount>();

            foreach (CustomerAccountType type in Enum.GetValues(typeof(CustomerAccountType)))
            {
                string prefix = "";
                switch (type)
                {
                    case CustomerAccountType.Credit:
                        prefix = "Credit";
                        break;
                    case CustomerAccountType.Debit:
                        prefix = "Debit";
                        break;
                    case CustomerAccountType.VAT:
                        prefix = "Vat";
                        break;
                }

                //Load accounts
                accounts.Add(GetAccount(entities, row, (int)type, prefix, noOfAccountInternals));
            }

            return accounts;
        }

        private List<ImportAccount> GetProductAccounts(CompEntities entities, DataRow row, int noOfAccountInternals)
        {
            List<ImportAccount> accounts = new List<ImportAccount>();

            foreach (ProductAccountType type in Enum.GetValues(typeof(ProductAccountType)))
            {
                string prefix = "";
                switch (type)
                {
                    case ProductAccountType.Purchase:
                        prefix = "Credit";
                        break;
                    case ProductAccountType.Sales:
                        prefix = "Debit";
                        break;
                    case ProductAccountType.VAT:
                        prefix = "Vat";
                        break;
                    case ProductAccountType.SalesNoVat:
                        prefix = "VatFree";
                        break;
                }

                //Load accounts
                accounts.Add(GetAccount(entities, row, (int)type, prefix, noOfAccountInternals));
            }

            return accounts;
        }

        private ImportAccount GetAccount(CompEntities entities, DataRow row, int type, string prefix, int noOfAccountInternals)
        {
            string accountStdName = "";
            string[] accountInternalNames = new string[noOfAccountInternals];

            //Get AccountStd
            var columnAccountStd = ExcelUtil.GetColumn(row, prefix + ACCOUNTSTD_FIELD_NAME);
            if (StringUtility.HasValue(columnAccountStd))
                accountStdName = columnAccountStd.ToString();

            //Get AccountInternals
            for (int i = 0; i < noOfAccountInternals; i++)
            {
                var columnAccountInternal = ExcelUtil.GetColumn(row, prefix + ACCOUNTINTERNAL_FIELD_NAME + (i + 1));
                if (StringUtility.HasValue(columnAccountInternal))
                    accountInternalNames[i] = columnAccountInternal.ToString();
            }

            return base.CreateImportAccount(entities, type, accountStdName, false, accountInternalNames);
        }

        private List<ImportAddress> GetAddresses(DataRow row)
        {
            List<ImportAddress> addresses = new List<ImportAddress>();

            //Distribution
            addresses.Add(GetAddress(row, ExcelColumnAddress.DistributionAddress, TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address));
            addresses.Add(GetAddress(row, ExcelColumnAddress.DistributionCoAddress, TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO));
            addresses.Add(GetAddress(row, ExcelColumnAddress.DistributionPostalCode, TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode));
            addresses.Add(GetAddress(row, ExcelColumnAddress.DistributionPostalAddress, TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress));
            addresses.Add(GetAddress(row, ExcelColumnAddress.DistributionCountry, TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country));

            //Visiting
            addresses.Add(GetAddress(row, ExcelColumnAddress.VisitingAddress, TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.StreetAddress));
            addresses.Add(GetAddress(row, ExcelColumnAddress.VisitingDoorCode, TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.EntranceCode));
            addresses.Add(GetAddress(row, ExcelColumnAddress.VisitingPostalCode, TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.PostalCode));
            addresses.Add(GetAddress(row, ExcelColumnAddress.VisitingPostalAddress, TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.PostalAddress));
            addresses.Add(GetAddress(row, ExcelColumnAddress.VisitingCountry, TermGroup_SysContactAddressType.Visiting, TermGroup_SysContactAddressRowType.Country));

            //Billing
            addresses.Add(GetAddress(row, ExcelColumnAddress.BillingAddress, TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Address));
            addresses.Add(GetAddress(row, ExcelColumnAddress.BillingCoAddress, TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.AddressCO));
            addresses.Add(GetAddress(row, ExcelColumnAddress.BillingPostalCode, TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalCode));
            addresses.Add(GetAddress(row, ExcelColumnAddress.BillingPostalAddress, TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalAddress));
            addresses.Add(GetAddress(row, ExcelColumnAddress.BillingCountry, TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Country));

            //Delivery
            addresses.Add(GetAddress(row, ExcelColumnAddress.DeliveryAddressName, TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Name));
            addresses.Add(GetAddress(row, ExcelColumnAddress.DeliveryAddress, TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Address));
            addresses.Add(GetAddress(row, ExcelColumnAddress.DeliveryCoAddress, TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.AddressCO));
            addresses.Add(GetAddress(row, ExcelColumnAddress.DeliveryPostalCode, TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalCode));
            addresses.Add(GetAddress(row, ExcelColumnAddress.DeliveryPostalAddress, TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalAddress));
            addresses.Add(GetAddress(row, ExcelColumnAddress.DeliveryCountry, TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Country));

            //Headquarter
            addresses.Add(GetAddress(row, ExcelColumnAddress.HeadquarterAddress, TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.PostalAddress));
            addresses.Add(GetAddress(row, ExcelColumnAddress.HeadquarterCountry, TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.Country));

            return addresses;
        }

        private ImportAddress GetAddress(DataRow row, ExcelColumnAddress columnAddress, TermGroup_SysContactAddressType adressType, TermGroup_SysContactAddressRowType adressRowType)
        {
            return base.CreateImportAddress(adressType, adressRowType, ExcelUtil.GetColumnValue(row, columnAddress));
        }

        private List<ImportECom> GetEComs(DataRow row)
        {
            List<ImportECom> ecoms = new List<ImportECom>();

            foreach (ExcelColumnECom column in Enum.GetValues(typeof(ExcelColumnECom)))
            {
                TermGroup_SysContactEComType type = TermGroup_SysContactEComType.Unknown;

                switch (column)
                {
                    case ExcelColumnECom.Email:
                    case ExcelColumnECom.Email2:
                        type = TermGroup_SysContactEComType.Email;
                        break;
                    case ExcelColumnECom.PhoneHome:
                        type = TermGroup_SysContactEComType.PhoneHome;
                        break;
                    case ExcelColumnECom.PhoneJob:
                        type = TermGroup_SysContactEComType.PhoneJob;
                        break;
                    case ExcelColumnECom.PhoneMobile:
                        type = TermGroup_SysContactEComType.PhoneMobile;
                        break;
                    case ExcelColumnECom.Fax:
                        type = TermGroup_SysContactEComType.Fax;
                        break;
                    case ExcelColumnECom.Web:
                        type = TermGroup_SysContactEComType.Web;
                        break;
                    case ExcelColumnECom.GlnNumber:
                        type = TermGroup_SysContactEComType.GlnNumber;
                        break;
                }

                if (type == TermGroup_SysContactEComType.Unknown)
                    continue;

                ecoms.Add(GetECom(row, type, column));
            }

            return ecoms;
        }

        private ImportECom GetECom(DataRow row, TermGroup_SysContactEComType type, ExcelColumnECom column)
        {
            return base.CreateImportECom(type, ExcelUtil.GetColumnValue(row, column));
        }

        public List<ExcelImportTemplateDTO> GetExcelImportTemplates()
        {
            List<ExcelImportTemplateDTO> templates = new List<ExcelImportTemplateDTO>();

            if (this.GetLangId() == (int)TermGroup_Languages.Finnish)
            {
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 1, Description = TermCacheManager.Instance.GetText(4263, 1, "Filmall för kundimport"), Href = "../../../common/excelimport/AsiakasTuonti.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 2, Description = TermCacheManager.Instance.GetText(4264, 1, "Filmall för leverantörimport"), Href = "../../../common/excelimport/ToimittajaTuonti.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 3, Description = TermCacheManager.Instance.GetText(4265, 1, "Filmall för artikelimport"), Href = "../../../common/excelimport/TuoteTuonti.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 4, Description = TermCacheManager.Instance.GetText(5575, 1, "Filmall för anställdaimport"), Href = "../../../common/excelimport/Työntekijätuonti.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 5, Description = TermCacheManager.Instance.GetText(4266, 1, "Filmall för kontaktpersonsimport"), Href = "../../../common/excelimport/YhteyshenkilöTuonti.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 6, Description = TermCacheManager.Instance.GetText(4596, 1, "Filmall för artikelkategori import"), Href = "../../../common/excelimport/TuoteryhmäTuonti.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 7, Description = TermCacheManager.Instance.GetText(4597, 1, "Filmall för kundkategori import"), Href = "../../../common/excelimport/AsiakasryhmäTuonti.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 8, Description = TermCacheManager.Instance.GetText(4693, 1, "Filmall för kontoimport"), Href = "../../../common/excelimport/TiliTuonti.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 9, Description = TermCacheManager.Instance.GetText(7731, 1, "Filmall för import av prislistor"), Href = "../../../common/excelimport/Hinnastotuonti.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 11, Description = TermCacheManager.Instance.GetText(7780, 1, "Filmall för produktrader"), Href = "../../../common/excelimport/Tuotuoterivejä_FI.xlsx" });
            }
            else
            {
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 1, Description = TermCacheManager.Instance.GetText(4263, 1, "Filmall för kundimport"), Href = "../../../common/excelimport/KundImport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 2, Description = TermCacheManager.Instance.GetText(4264, 1, "Filmall för leverantörimport"), Href = "../../../common/excelimport/LeverantörImport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 3, Description = TermCacheManager.Instance.GetText(4265, 1, "Filmall för artikelimport"), Href = "../../../common/excelimport/ArtikelImport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 4, Description = TermCacheManager.Instance.GetText(5575, 1, "Filmall för anställdaimport"), Href = "../../../common/excelimport/AnstalldImport_NY.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 5, Description = TermCacheManager.Instance.GetText(4266, 1, "Filmall för kontaktpersonsimport"), Href = "../../../common/excelimport/KontaktPersonsImport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 6, Description = TermCacheManager.Instance.GetText(4596, 1, "Filmall för artikelkategori import"), Href = "../../../common/excelimport/ArtikelKategoriImport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 7, Description = TermCacheManager.Instance.GetText(4597, 1, "Filmall för kundkategori import"), Href = "../../../common/excelimport/KundKategoriImport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 8, Description = TermCacheManager.Instance.GetText(4693, 1, "Filmall för kontoimport"), Href = "../../../common/excelimport/Kontoimport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 8, Description = TermCacheManager.Instance.GetText(7614, 1, "Filmall för import av skattereduktionskontakter"), Href = "../../../common/excelimport/SkattereduktionskontaktsImport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 9, Description = TermCacheManager.Instance.GetText(7731, 1, "Filmall för import av prislistor"), Href = "../../../common/excelimport/Prislisteimport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 10, Description = TermCacheManager.Instance.GetText(7779, 1, "Filmall för avtalsimport"), Href = "../../../common/excelimport/AvtalsImport.xlsx" });
                templates.Add(new ExcelImportTemplateDTO() { ExcelImportTemplateDTOID = 11, Description = TermCacheManager.Instance.GetText(7780, 1, "Filmall för produktrader"), Href = "../../../common/excelimport/ArtikelraderImport_SVE.xlsx" });
            }

            return templates;
        }

        public ExcelImportTemplateDTO GetProductRowsExcelTemplate()
        {
            return GetExcelImportTemplates().FirstOrDefault(x => x.ExcelImportTemplateDTOID == 11);
        }

        #endregion


        #endregion

        #region Translate

        private void TranslateDataSet(DataSet ds, ImportType importType)
        {
            DataRow headlineRow = ds.Tables[0].Rows[0];
            DataColumnCollection columns = ds.Tables[0].Columns;
            foreach (DataColumn column in columns)
            {
                TranslateColumn(headlineRow, column, importType);
            }
        }

        private void TranslateColumn(DataRow headlineRow, DataColumn column, ImportType importType)
        {
            if (headlineRow == null || column == null)
                return;

            if (TryTranslateColumn(importType, column.Caption, out string translation))
            {
                //Get original headline
                object headlineColumn = ExcelUtil.GetColumn(headlineRow, column.ColumnName);

                //Set translation
                column.ColumnName = translation;

                //Add original headline
                if (StringUtility.HasValue(headlineColumn))
                {
                    if (this.translationDict == null)
                        this.translationDict = new Dictionary<string, string>();
                    this.translationDict.Add(translation, headlineColumn.ToString());
                }
            }
        }

        private bool TryTranslateColumn(ImportType importType, string caption, out string translation)
        {
            translation = "";
            bool isDefined = false;

            Type enumType = null;
            switch (importType)
            {
                case ImportType.Customer:
                    enumType = typeof(ExcelColumnCustomer);
                    break;
                case ImportType.Supplier:
                    enumType = typeof(ExcelColumnSupplier);
                    break;
                case ImportType.ContactPerson:
                    enumType = typeof(ExcelColumnContactPerson);
                    break;
                case ImportType.Product:
                    enumType = typeof(ExcelColumnProduct);
                    break;
                case ImportType.Employee:
                    enumType = typeof(ExcelColumnEmployee);
                    break;
                case ImportType.PayrollStartValue:
                    enumType = typeof(ExcelColumnPayrollStartValue);
                    break;
                case ImportType.Accounts:
                    enumType = typeof(ExcelColumnAccount);
                    break;
                case ImportType.TaxDeductionContacts:
                    enumType = typeof(ExcelColumnTaxDeductionContact);
                    break;
                case ImportType.Pricelists:
                    enumType = typeof(ExcelColumnPricelist);
                    break;
                case ImportType.Agreements:
                    enumType = typeof(ExcelColumnAgreement);
                    break;
            }

            if (enumType != null)
            {
                int id = Convert.ToInt32(caption.Substring(1));
                isDefined = Enum.IsDefined(enumType, id);
                if (isDefined)
                    translation = Enum.GetName(enumType, id);
            }

            return isDefined;
        }

        #endregion

        #region DataRow/DataColumn

        private string GetColumnName(Enum column)
        {
            string columnName = "";
            if (column != null)
            {
                if (translationDict.ContainsKey(column.ToString()))
                    columnName = translationDict[column.ToString()];
                else
                    columnName = column.ToString();
            }
            return columnName;
        }

        private bool IsColumnsMatching(DataRowCollection rows, Type enumType)
        {
            if (rows.Count == 0)
                return true;

            return rows[0].Table.Columns.Count == Enum.GetNames(enumType).Count();
        }

        private ImportType GetImportType(string tableName)
        {
            if (tableName == EXCELIMPORT_TABLENAME_SUPPLIERS)
                return ImportType.Supplier;
            else if (tableName == EXCELIMPORT_TABLENAME_CUSTOMERS)
                return ImportType.Customer;
            else if (tableName == EXCELIMPORT_TABLENAME_CONTACTPERSONS)
                return ImportType.ContactPerson;
            else if (tableName == EXCELIMPORT_TABLENAME_PRODUCTS)
                return ImportType.Product;
            else if (tableName == EXCELIMPORT_TABLENAME_EMPLOYEES)
                return ImportType.Employee;
            else if (tableName == EXCELIMPORT_TABLENAME_CUSOMTERCATEGORIES)
                return ImportType.CustomerGroup;
            else if (tableName == EXCELIMPORT_TABLENAME_PRODUCTGROUPS)
                return ImportType.ProductGroup;
            else if (tableName == EXCELIMPORT_TABLENAME_PAYROLLSTARTVALUES)
                return ImportType.PayrollStartValue;
            else if (tableName == EXCELIMPORT_TABLENAME_ACCOUNTS)
                return ImportType.Accounts;
            else if (tableName == EXCELIMPORT_TABLENAME_TAXDEDUCTIONCONTACTS)
                return ImportType.TaxDeductionContacts;
            else if (tableName == EXCELIMPORT_TABLENAME_PRICELISTS)
                return ImportType.Pricelists;
            else if (tableName == EXCELIMPORT_TABLENAME_AVTALS)
                return ImportType.Agreements;
            else
                return ImportType.Unknown;
        }

        #endregion

        #region Messages

        private string GetFileCannotBeReadMessage()
        {
            return GetText(4261, "Filen kunde inte läsas");
        }

        public string GetFileNotSupportedMessage()
        {
            return GetText(4260, "Filen är inte på ett format som stöds");
        }

        private string GetFileNotMatchingSpecificationMessage()
        {
            return GetText(4262, "Filinnehållet matchar inte någon specifikation");
        }

        private string GetFileInvalidNoOfColumnsMessage()
        {
            return GetText(5688, "Filinnehållet matchar inte mallen. Ladda ner senaste mallen");
        }

        public string GetFileIsEmptyMessage()
        {
            return GetText(5621, "Filen har inget innehåll");
        }

        #endregion
    }
}
