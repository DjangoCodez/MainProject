using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Util;
using SoftOne.Soe.Data;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.IO;
using SoftOne.Soe.Common.DTO;
using System.Threading;
using SoftOne.Soe.Business.Util.Azure;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;

namespace Soe.WebServices.External.IO
{

    [WebService(Description = "Connect", Namespace = "http://xe.softone.se/soe/WebServices/IO/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Connect : WebserviceBase
    {

        #region Common

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }



        [WebMethod(Description = "Get valid companies for login", EnableSession = false)]
        public List<CompanyWS> GetCompanies(Login login)
        {
            List<CompanyWS> result = new List<CompanyWS>();
            ConnectUtil connectUtil = new ConnectUtil(null);

            #region Validation

            int userId = 0;
            int licenseId = 0;
            int roleId = 0;
            User user = new User();
            ParameterObject parameterObject = ParameterObject.Empty();

            string detailedMessage = string.Empty;
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
                return result;

            #endregion

            CompanyManager cm = new CompanyManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);

            List<Company> companies = cm.GetCompaniesByUser(userId, licenseId);

            if (companies.Count == 0)
            {
                return result;
            }
            else
            {
                foreach (var company in companies)
                {
                    string api = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, userId, company.ActorCompanyId, 0);

                    Guid guid = new Guid();

                    if (Guid.TryParse(api, out guid))
                    {
                        CompanyWS comp = new CompanyWS();

                        comp.CompanyNr = company.CompanyNr.HasValue ? (int)company.CompanyNr : 0;
                        comp.Name = company.Name;
                        comp.CompanyAPIKey = api;

                        result.Add(comp);

                    }
                }

                return result;
            }
        }

        [WebMethod(Description = "Get template companies", EnableSession = false)]
        public List<CompanyWS> GetTemplateCompanies(Login login)
        {
            List<CompanyWS> result = new List<CompanyWS>();
            ConnectUtil connectUtil = new ConnectUtil(null);

            #region Validation

            int userId = 0;
            int licenseId = 0;
            int roleId = 0;
            User user = new User();
            ParameterObject parameterObject = ParameterObject.Empty();

            string detailedMessage = string.Empty;
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
                return result;

            #endregion

            CompanyManager cm = new CompanyManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);

            List<Company> companies = cm.GetTemplateCompanies(licenseId);

            if (companies.Count == 0)
            {
                return result;
            }
            else
            {
                foreach (var company in companies)
                {
                    string api = sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, userId, company.ActorCompanyId, 0);

                    Guid guid = new Guid();

                    if (Guid.TryParse(api, out guid))
                    {
                        CompanyWS comp = new CompanyWS();

                        comp.CompanyNr = company.CompanyNr.HasValue ? (int)company.CompanyNr : 0;
                        comp.Name = company.Name;
                        comp.CompanyAPIKey = api;
                        comp.isTemplate = true;

                        result.Add(comp);
                    }
                }

                return result;
            }
        }

        [WebMethod(Description = "Get template companies", EnableSession = false)]
        public ConnectResult CreateCompanyFromTemplate(Login login, string companyApiKey, int companyNr, string companyName, string currency, string enterpriseCurrency)
        {
            ConnectResult connectResult = new ConnectResult();
            ConnectUtil connectUtil = new ConnectUtil(null);

            #region Validation

            int userId = 0;
            int licenseId = 0;
            int roleId = 0;
            User user = new User();
            ParameterObject parameterObject = ParameterObject.Empty();

            string detailedMessage = string.Empty;
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
            {
                connectResult.ErrorMessage = detailedMessage;
                connectResult.Success = false;
                return connectResult;
            }

            //Get company

            CompanyManager cm = new CompanyManager(parameterObject);

            int? templateActorCompanyId = cm.GetActorCompanyIdFromApiKey(companyApiKey);

            if (!templateActorCompanyId.HasValue)
            {
                Thread.Sleep(5000);
                connectResult.ErrorMessage = "No templateCompany found with that API-Key";
                connectResult.Success = false;
                return connectResult;
            }

            //Validate permission to create new company

            List<Company> companies = cm.GetCompaniesByUser(userId, licenseId);
            List<Company> allCompaniesOnLicense = cm.GetCompaniesByLicense(licenseId);
            FeatureManager fm = new FeatureManager(null);
            bool gotPermissionToEditCompany = false;

            if (companies.Count == 0)
            {
                connectResult.ErrorMessage = "No templateCompany found with that API-Key";
                connectResult.Success = false;
                return connectResult;
            }
            else
            {
                foreach (var company in companies)
                {
                    if (fm.HasRolePermission(Feature.Manage_Companies_Edit, Permission.Modify, roleId, company.ActorCompanyId, company.LicenseId) || fm.HasRolePermission(Feature.Time_Import_XEConnect, Permission.Modify, roleId, company.ActorCompanyId, company.LicenseId) || fm.HasRolePermission(Feature.Economy_Import_XEConnect, Permission.Modify, roleId, company.ActorCompanyId, company.LicenseId))
                    {
                        gotPermissionToEditCompany = true;
                        break;
                    }
                }
            }

            if (!gotPermissionToEditCompany)
            {
                connectResult.ErrorMessage = "No permission to create new company";
                connectResult.Success = false;
                return connectResult;
            }

            //license
            {
                LicenseManager lm = new LicenseManager(null);
                CountryCurrencyManager ccm = new CountryCurrencyManager(null);
                int? baseSysCurrencyId = null;
                int? enterpriseSysCurrencyId = null;

                List<SysCurrency> sysCurrencies = ccm.GetSysCurrencies();

                if (!string.IsNullOrEmpty(currency))
                {
                    foreach (var curr in sysCurrencies)
                    {
                        if (curr.Code.ToLower().Equals(currency.ToLower()))
                        {
                            baseSysCurrencyId = (int?)curr.SysCurrencyId;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(enterpriseCurrency))
                {
                    foreach (var curr in sysCurrencies)
                    {
                        if (curr.Code.ToLower().Equals(enterpriseCurrency.ToLower()))
                        {
                            enterpriseSysCurrencyId = (int?)curr.SysCurrencyId;
                            break;
                        }
                    }
                }

                License license = lm.GetLicense(licenseId);

                if (license != null && allCompaniesOnLicense.Count < license.NrOfCompanies)
                {
                    int actorCompanyId = connectUtil.CreateCompany(licenseId, userId, (int)templateActorCompanyId, companyNr, companyName, baseSysCurrencyId, enterpriseSysCurrencyId);
                    cm.CopyAllFromTemplateCompany((int)templateActorCompanyId, actorCompanyId, userId, update: true);
                }
            }

            #endregion


            return connectResult;
        }

        #endregion

        #region Import

        [WebMethod(Description = "Import with XE Connect (Text file in string)", EnableSession = false)]
        public ConnectResult ConnectImportString(Login login, int source, string companyApiKey, string importApiKey, int importHeadId, string text)
        {
            ConnectResult result = new ConnectResult();
            ConnectUtil connectUtil = new ConnectUtil(null);
            result.Success = false;

            #region Validation

            string detailedMessage;
            int userId, roleId, licenseId;
            User user;
            ParameterObject parameterObject;
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
            {
                result.ErrorMessage = detailedMessage;
                return result;
            }

            int actorcompanyId;
            TermGroup_IOSource ioSource;
            TermGroup_IOType ioType;
            int? sysDefinitionId;
            int? compImportId;

            string validationMessage = connectUtil.Validate(source, companyApiKey, importApiKey, importHeadId, out actorcompanyId, out ioSource, out ioType, out sysDefinitionId, out compImportId);
            if (!String.IsNullOrEmpty(validationMessage))
            {
                result.ErrorMessage = validationMessage;
                return result;
            }

            if (sysDefinitionId == null)
            {
                result.ErrorMessage = "No SysDefinition found";
                return result;
            }

            #endregion

            #region Convert

            List<Byte[]> contents = new List<Byte[]>();
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            contents.Add(bytes);

            if (text.Length < 600)
            {
                try
                {
                    AzureStorageDTO azureStorageDTO = connectUtil.GetAzureStorageDTOFromString(text);

                    if (!string.IsNullOrEmpty(azureStorageDTO.ContainerName))
                    {
                        BlobUtil blobUtil = new BlobUtil();

                        LogInfo(blobUtil.Init(azureStorageDTO.ContainerName));

                        contents = new List<byte[]>();
                        byte[] zippedData = blobUtil.DownloadArray(azureStorageDTO.guid);
                        blobUtil.DeleteFile(azureStorageDTO.guid);

                        using (var outputStream = new MemoryStream())
                        using (var inputStream = new MemoryStream(zippedData))
                        {
                            using (var zipInputStream = new ZipInputStream(inputStream))
                            {
                                zipInputStream.GetNextEntry();
                                zipInputStream.CopyTo(outputStream);
                            }
                            contents.Add(outputStream.ToArray());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }

            #endregion

            try
            {
                

                LogInfo("Before import " + actorcompanyId.ToString() + " " + ioSource.ToString() + " " + user.LoginName + " " + companyApiKey + " " + sysDefinitionId.ToString());

                var iem = new ImportExportManager(parameterObject);
                var actionResult = iem.XEConnectWebService(actorcompanyId, user, (int)sysDefinitionId, false, importHeadId, contents, true);
                LogInfo(actionResult.IntegerValue.ToString() + " Integer value");

                result = connectUtil.ActionResult2ConnectResult(actionResult);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return result;
        }

        [WebMethod(Description = "Import with XE Connect (ByteArray)", EnableSession = false)]
        public ConnectResult ConnectImportByteArray(Login login, int source, string companyApiKey, string importApiKey, int importHeadId, byte[] content)
        {
            ConnectResult result = new ConnectResult();
            ConnectUtil connectUtil = new ConnectUtil(null);
            result.Success = false;

            #region Validation

            string detailedMessage;
            int userId, roleId, licenseId;
            User user;
            ParameterObject parameterObject;
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
            {
                result.ErrorMessage = detailedMessage;
                return result;
            }

            int actorcompanyId;
            TermGroup_IOSource ioSource;
            TermGroup_IOType ioType;
            int? sysDefinitionId;
            int? compImportId;
            string validationMessage = connectUtil.Validate(source, companyApiKey, importApiKey, importHeadId, out actorcompanyId, out ioSource, out ioType, out sysDefinitionId, out compImportId);
            if (!String.IsNullOrEmpty(validationMessage))
            {
                result.ErrorMessage = validationMessage;
                return result;
            }
            if (sysDefinitionId == null)
            {
                result.ErrorMessage = "No SysDefinition found";
                return result;
            }

            #endregion

            #region Convert

            List<Byte[]> contents = new List<Byte[]>();
            contents.Add(content);

            var iem = new ImportExportManager(parameterObject);
            var actionResult = iem.XEConnectWebService(actorcompanyId, user, (int)sysDefinitionId, false, importHeadId, contents, false);

            result = connectUtil.ActionResult2ConnectResult(actionResult);

            #endregion

            return result;
        }

        [WebMethod(Description = "Import Supplier invoice image (ByteArray), one image at the time", EnableSession = false)]
        public ConnectResult ConnectImportSupplierInvoiceImage(Login login, int source, string companyApiKey, string invoiceNr, string supplierNr, byte[] image)
        {
            ConnectResult result = new ConnectResult();
            ConnectUtil connectUtil = new ConnectUtil(null);
            result.Success = false;

            #region Validation

            int userId = 0;
            int licenseId = 0;
            int roleId = 0;
            User user = new User();
            ParameterObject parameterObject = ParameterObject.Empty();

            string detailedMessage = string.Empty;
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
            {
                result.ErrorMessage = detailedMessage;
                return result;
            }

            int actorCompanyId;
            TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
            TermGroup_IOType ioType = TermGroup_IOType.Unknown;

            string validationMessage = connectUtil.Validate(source, companyApiKey, out actorCompanyId, out ioSource, out ioType);
            if (!String.IsNullOrEmpty(validationMessage))
            {
                result.ErrorMessage = validationMessage;
                return result;
            }

            #endregion

            var m = new ImportExportManager(parameterObject);
            var actionResult = m.ImportSupplierInvoiceImage(actorCompanyId, invoiceNr, supplierNr, image);

            result = connectUtil.ActionResult2ConnectResult(actionResult);

            return result;
        }

        [WebMethod(Description = "Import Supplier invoice image (ByteArray), one image at the time with seqNumber", EnableSession = false)]
        public ConnectResult ConnectImportSupplierInvoiceImage2(Login login, int source, string companyApiKey, string invoiceNr, string supplierNr, byte[] image, int? seqNumber)
        {
            ConnectResult result = new ConnectResult();
            ConnectUtil connectUtil = new ConnectUtil(null);
            result.Success = false;

            #region Validation

            int userId = 0;
            int licenseId = 0;
            int roleId = 0;
            User user = new User();
            ParameterObject parameterObject = ParameterObject.Empty();

            string detailedMessage = string.Empty;
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
            {
                result.ErrorMessage = detailedMessage;
                return result;
            }

            int actorCompanyId;
            TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
            TermGroup_IOType ioType = TermGroup_IOType.Unknown;

            string validationMessage = connectUtil.Validate(source, companyApiKey, out actorCompanyId, out ioSource, out ioType);
            if (!String.IsNullOrEmpty(validationMessage))
            {
                result.ErrorMessage = validationMessage;
                return result;
            }

            #endregion

            var m = new ImportExportManager(parameterObject);
            var actionResult = m.ImportSupplierInvoiceImage(actorCompanyId, invoiceNr, supplierNr, image, seqNumber);

            result = connectUtil.ActionResult2ConnectResult(actionResult);

            return result;
        }

        #endregion

        #region Report
        [WebMethod(Description = "Get Time Attendance View", EnableSession = false)]
        public ReportResult GetReport(Login login, string companyApiKey, ReportSelectionWS reportSelectionWS)
        {
            ReportResult result = new ReportResult();

            #region Validation

            ConnectUtil connectUtil = new ConnectUtil(null);
            string errorMessage = string.Empty;

            bool validated = true;
            string detailedMessage;
            int userId;
            int licenseId;
            int roleId;
            User user;
            ParameterObject parameterObject;            
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
            {
                validated = false;
                errorMessage = "Login failed";
                Thread.Sleep(3000);
            }

            if (!validated)
            {
                result.Success = false;
                result.ErrorMessage = errorMessage;
            }

            int actorCompanyId;
            TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
            TermGroup_IOType ioType = TermGroup_IOType.Unknown;

            string validationMessage = connectUtil.Validate((int)TermGroup_IOSource.Connect, companyApiKey, out actorCompanyId, out ioSource, out ioType);
            if (!String.IsNullOrEmpty(validationMessage))
            {
                validated = false;
                errorMessage = "Validation failed";
                Thread.Sleep(3000);
            }

            #endregion

            #region Print

            ReportDataManager reportDataManager = new ReportDataManager(parameterObject);
            List<EvaluatedSelection> esc = new List<EvaluatedSelection>();

            reportDataManager.PrintReportPackageId(esc);

            #endregion

            return result;
        }

        #endregion

        #region Validation  Service
        [WebMethod(Description = "Validate list of key items before import", EnableSession = false)]
        public ConnectResult ValidateService(Login login, string companyApiKey, List<IODictionaryTypeValidation> IODictionaryTypeValidationList)
        {
            ConnectResult result = new ConnectResult();
            ConnectUtil connectUtil = new ConnectUtil(null);
            List<IODictionaryTypeValidation> newIODictionaryTypeValidationList = new List<IODictionaryTypeValidation>();
            ParameterObject parameterObject = ParameterObject.Empty();

            #region Validation

            bool validated = true;
            int userId = 0;
            int licenseId = 0;
            int roleId = 0;
            User user = new User();
            string errorMessage = string.Empty;

            string detailedMessage = string.Empty;
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject))
            {
                validated = false;
                errorMessage = "Login failed";
                Thread.Sleep(3000);
            }

            if (!validated)
            {
                result.Success = false;
                result.ErrorMessage = errorMessage;
            }

            int actorCompanyId;
            TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
            TermGroup_IOType ioType = TermGroup_IOType.Unknown;

            string validationMessage = connectUtil.Validate((int)TermGroup_IOSource.Connect, companyApiKey, out actorCompanyId, out ioSource, out ioType);
            if (!String.IsNullOrEmpty(validationMessage))
            {
                validated = false;
                errorMessage = "Validation failed";
                Thread.Sleep(3000);
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                result.ErrorMessage = errorMessage;
                result.Success = false;

                return result;
            }

            #endregion

            #region Validate List

            AccountManager accountManager = new AccountManager(GetParameterObject(actorCompanyId, userId));
            ProductManager productManager = new ProductManager(GetParameterObject(actorCompanyId, userId));
            EmployeeManager employeeManager = new EmployeeManager(GetParameterObject(actorCompanyId, userId));
            PayrollManager payrollManager = new PayrollManager(GetParameterObject(actorCompanyId, userId));

            bool loadProducts = false;
            bool loadAccounts = false;
            bool loadEmployeeGroups = false;
            bool loadPayrollGroups = false;
            bool loadVacationGroups = false;

            List<Product> products = new List<Product>();
            List<Account> accounts = new List<Account>();
            List<PayrollPriceType> payrollPriceTypes = new List<PayrollPriceType>();
            List<EmployeeGroup> employeeGroups = new List<EmployeeGroup>();
            List<PayrollGroup> payrollGroups = new List<PayrollGroup>();
            List<VacationGroup> vactionGroups = new List<VacationGroup>();
            int accountDimStdId = 0;
            int accountDim2Id = 0;
            int accountDim3Id = 0;
            int accountDim4Id = 0;
            int accountDim5Id = 0;
            int accountDim6Id = 0;

            List<AccountDim> accountDims = new List<AccountDim>();

            foreach (var item in IODictionaryTypeValidationList)
            {
                if (item.IODictionaryType == IODictionaryType.AccountNr ||
                    item.IODictionaryType == IODictionaryType.AccountInternalDim2Nr ||
                    item.IODictionaryType == IODictionaryType.AccountInternalDim3Nr ||
                    item.IODictionaryType == IODictionaryType.AccountInternalDim4Nr ||
                    item.IODictionaryType == IODictionaryType.AccountInternalDim5Nr ||
                    item.IODictionaryType == IODictionaryType.AccountInternalDim6Nr)
                    loadAccounts = true;

                if (item.IODictionaryType == IODictionaryType.InvoiceProduct || item.IODictionaryType == IODictionaryType.PayrollProduct)
                    loadProducts = true;

                if (item.IODictionaryType == IODictionaryType.EmployeeGroup)
                    loadEmployeeGroups = true;

                if (item.IODictionaryType == IODictionaryType.PayrollGroup)
                    loadPayrollGroups = true;

                if (item.IODictionaryType == IODictionaryType.VacationGroup)
                    loadVacationGroups = true;
            }

            if (loadProducts)
            {
                products = productManager.GetProducts(actorCompanyId,null,false).ToList();
                payrollPriceTypes = payrollManager.GetPayrollPriceTypes(actorCompanyId, null, false);
            }


            if (loadAccounts)
            {
                accountDims = accountManager.GetAccountDimInternalsByCompany(actorCompanyId, true);
                accounts = accountManager.GetAccounts(actorCompanyId);

                int dimCount = 2;

                foreach (var dim in accountDims.OrderBy(a => a.AccountDimNr))
                {
                    if (dim.AccountDimNr == 1)
                    {
                        accountDimStdId = dim.AccountDimId;
                        continue;
                    }

                    if (dimCount == 2)
                        accountDim2Id = dim.AccountDimId;

                    if (dimCount == 3)
                        accountDim3Id = dim.AccountDimId;

                    if (dimCount == 4)
                        accountDim4Id = dim.AccountDimId;

                    if (dimCount == 5)
                        accountDim5Id = dim.AccountDimId;

                    if (dimCount == 6)
                        accountDim6Id = dim.AccountDimId;

                    dimCount++;
                }

            }

            if (loadEmployeeGroups)
                employeeGroups = employeeManager.GetEmployeeGroups(actorCompanyId);

            if (loadVacationGroups)
                vactionGroups = payrollManager.GetVacationGroups(actorCompanyId);

            if (loadPayrollGroups)
                payrollGroups = payrollManager.GetPayrollGroups(actorCompanyId);

            foreach (var item in IODictionaryTypeValidationList)
            {
                IODictionaryTypeValidation newItem = item;
                newItem.Validated = false;

                string lowerCode = item.NoOrCode.ToLower();
                string lowerName = item.Name.ToLower();

                if (string.IsNullOrEmpty(item.NoOrCode) && string.IsNullOrEmpty(item.Name))
                {
                    item.ErrorMessage = "Number, Code and Name is empty";
                    newIODictionaryTypeValidationList.Add(newItem);
                    continue;
                }

                switch (item.IODictionaryType)
                {
                    case IODictionaryType.AccountNr:
                        if (accounts.Any(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDimStdId))
                        {
                            newItem.Validated = true;

                            if (string.IsNullOrEmpty(item.Name))
                                newItem.Name = accounts.FirstOrDefault(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDimStdId).Name;
                        }
                        break;
                    case IODictionaryType.AccountInternalDim2Nr:
                        if (accounts.Any(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim2Id))
                        {
                            newItem.Validated = true;

                            if (string.IsNullOrEmpty(item.Name))
                                newItem.Name = accounts.FirstOrDefault(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim2Id).Name;
                        }
                        break;
                    case IODictionaryType.AccountInternalDim3Nr:
                        if (accounts.Any(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim3Id))
                        {
                            newItem.Validated = true;

                            if (string.IsNullOrEmpty(item.Name))
                                newItem.Name = accounts.FirstOrDefault(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim3Id).Name;
                        }
                        break;
                    case IODictionaryType.AccountInternalDim4Nr:
                        if (accounts.Any(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim4Id))
                        {
                            newItem.Validated = true;

                            if (string.IsNullOrEmpty(item.Name))
                                newItem.Name = accounts.FirstOrDefault(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim4Id).Name;
                        }
                        break;
                    case IODictionaryType.AccountInternalDim5Nr:
                        if (accounts.Any(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim5Id))
                        {
                            newItem.Validated = true;

                            if (string.IsNullOrEmpty(item.Name))
                                newItem.Name = accounts.FirstOrDefault(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim5Id).Name;
                        }
                        break;
                    case IODictionaryType.AccountInternalDim6Nr:
                        if (accounts.Any(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim6Id))
                        {
                            newItem.Validated = true;

                            if (string.IsNullOrEmpty(item.Name))
                                newItem.Name = accounts.FirstOrDefault(a => a.AccountNr.ToLower() == lowerCode && a.AccountDimId == accountDim6Id).Name;
                        }
                        break;
                    case IODictionaryType.EmployeeGroup:
                        if (employeeGroups.Any(a => a.Name.ToLower() == lowerName))
                            newItem.Validated = true;
                        break;
                    case IODictionaryType.VacationGroup:
                        if (vactionGroups.Any(a => a.Name.ToLower() == lowerName))
                            newItem.Validated = true;
                        break;
                    case IODictionaryType.PayrollGroup:
                        if (payrollGroups.Any(a => a.Name.ToLower() == lowerName))
                            newItem.Validated = true;
                        break;
                    case IODictionaryType.PayrollProduct:
                        if (products.Any(p => p.Type == (int)SoeProductType.PayrollProduct && p.Number == lowerCode))
                        {
                            newItem.Validated = true;

                            if (string.IsNullOrEmpty(item.Name))
                                newItem.Name = products.FirstOrDefault(p => p.Type == (int)SoeProductType.PayrollProduct && p.Number == lowerCode).Name;
                        }
                        else
                        {
                            if (payrollPriceTypes.Any(p => p.Code.ToLower() == lowerCode))
                            {
                                newItem.Validated = true;

                                if (string.IsNullOrEmpty(item.Name))
                                    newItem.Name = payrollPriceTypes.FirstOrDefault(p => p.Code.ToLower() == lowerCode).Name;
                            }
                        }
                        break;
                    case IODictionaryType.InvoiceProduct:
                        if (products.Any(p => p.Type == (int)SoeProductType.InvoiceProduct && p.Number == lowerCode))
                        {
                            newItem.Validated = true;

                            if (string.IsNullOrEmpty(item.Name))
                                newItem.Name = products.FirstOrDefault(p => p.Type == (int)SoeProductType.InvoiceProduct && p.Number == lowerCode).Name;
                        }
                        break;
                }

                if (!item.Validated)
                    item.ErrorMessage = "Not found in XE";

                if (!newIODictionaryTypeValidationList.Any(i => i.IODictionaryType == newItem.IODictionaryType && i.NoOrCode.ToLower() == lowerCode && i.Name.ToLower() == newItem.Name.ToLower()))
                    newIODictionaryTypeValidationList.Add(newItem);
            }

            #endregion
            result.IODictionaryTypeValidations = new List<IODictionaryTypeValidation>();
            result.IODictionaryTypeValidations.AddRange(newIODictionaryTypeValidationList);

            return result;
        }


        #endregion

        #region TimeStamp

        [WebMethod(Description = "Get Time Attendance View", EnableSession = false)]
        public ConnectResult GetTimeAttendanceView(Login login, string companyApiKey, bool onlyIn, bool onlyIncludeAttestRoleEmployees = true, bool includeEmployeeNrInString = true, int? timeTerminalId = null)
        {
            ConnectResult result = new ConnectResult();

            ConnectUtil connectUtil = new ConnectUtil(null);
            int actorCompanyId = 0;
            int userId = 0;
            int licenseId = 0;
            int roleId = 0;
            User user = new User();
            ParameterObject parameterObject = ParameterObject.Empty();

            #region Validation

            string detailedMessage = string.Empty;
            if (!connectUtil.ValidateLogin(login, out detailedMessage, out userId, out user, out licenseId, out roleId, out parameterObject, ignoreImportPermission: true))
            {
                result.ErrorMessage = detailedMessage;
                result.Success = false;
                return result;
            }

            if (!connectUtil.ValidateCompany(companyApiKey, out actorCompanyId))
            {
                result.ErrorMessage = "Invalid companyApiKey";
                result.Success = false;
                return result;
            }

            #endregion

            DashboardManager dbm = new DashboardManager(parameterObject);
            var iDTO = dbm.GetTimeStampAttendance(actorCompanyId, userId, roleId, TermGroup_TimeStampAttendanceGaugeShowMode.AllLast24Hours, onlyIn, onlyIncludeAttestRoleEmployees, includeEmployeeNrInString, timeTerminalId);

            result.timeStampAttendanceGaugeDTOs = new List<TimeStampAttendanceGaugeDTO>();

            foreach (TimeStampAttendanceGaugeDTO dto in iDTO)
                result.timeStampAttendanceGaugeDTOs.Add(dto);

            return result;
        }

        #endregion

        #region Help-methods


        #endregion
    }



}
