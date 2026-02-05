using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Util;
using SoftOne.Soe.Data;
using System.Xml.Linq;
using System.Xml;
using SoftOne.Soe.Common.Util;

namespace Soe.WebServices.External.IO
{
    [WebService(Description = "IO Service", Namespace = "http://xe.softone.se/soe/WebServices/External/Mobile")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class IOService : WebserviceBase
    {
        //#region Constants

        //private const string VALIDATION_SOURCE_ERRORMESSAGE = "Invalid source";

        //#endregion

        //#region Variables

        //public AuthenticationHeader SecurityCredentials;

        //private const String USERNAME = "IO";
        //private const String PASSWORD = "F_2!REYF_2!REY";
        //private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //#endregion

        //#region Common

        //[WebMethod]
        //public string HelloWorld()
        //{
        //    return "Hello World";
        //}

        //[WebMethod]
        //public string ExportEmployeesTest()
        //{
        //    return
        //        "<Employees>" +
        //            "<Employee>" +
        //                "<EmployeeId>5893</EmployeeId>" +
        //                "<EmployeeNr>200</EmployeeNr>" +
        //                "<FirstName>Anders</FirstName>" +
        //                "<LastName>Andersson</LastName>" +
        //                "<EmployeeGroupName>Avtal</EmployeeGroupName>" +
        //                "<SocialSec>620114-6074</SocialSec>" +
        //                "<EmploymentDate>2007-05-01 00:00:00</EmploymentDate>" +
        //                "<EndDate></EndDate>" +
        //                "<WorkPercentage>100,00</WorkPercentage>" +
        //                "<TaxRate></TaxRate>" +
        //                "<Position>0</Position>" +
        //                "<Sex>0</Sex>" +
        //                "<Email>orjan@orjan.se</Email>" +
        //                "<PhoneHome>0800-500664</PhoneHome>" +
        //                "<PhoneMobile>0800-500665</PhoneMobile>" +
        //                "<PhoneJob></PhoneJob>" +
        //                "<ClosestRelativeNr></ClosestRelativeNr>" +
        //                "<ClosestRelativeName>Therese Mirvana 0800-500664</ClosestRelativeName>" +
        //                "<ClosestRelativeRelation></ClosestRelativeRelation>" +
        //                "<DistributionAddress>Bondsvängen 14</DistributionAddress>" +
        //                "<DistributionCoAddress></DistributionCoAddress>" +
        //                "<DistributionPostalCode>145 00</DistributionPostalCode>" +
        //                "<DistributionPostalAddress>SMÅSTAD</DistributionPostalAddress>" +
        //                "<DistributionCountry></DistributionCountry>" +
        //                "<CategoryCode1 />" +
        //                "<CategoryCode2 />" +
        //                "<CategoryCode3 />" +
        //                "<CategoryCode4 />" +
        //                "<CategoryCode5 />" +
        //                "<SecondaryCategoryCode1 />" +
        //                "<SecondaryCategoryCode2 />" +
        //                "<SecondaryCategoryCode3 />" +
        //                "<SecondaryCategoryCode4 />" +
        //                "<SecondaryCategoryCode5 />" +
        //                "<DefaultTimeDeviationCauseName />" +
        //                "<DefaultTimeCodeName />" +
        //                "<CostAccountStd />" +
        //                "<CostAccountInternal1 />" +
        //                "<CostAccountInternal2 />" +
        //                "<CostAccountInternal3 />" +
        //                "<CostAccountInternal4 />" +
        //                "<CostAccountInternal5 />" +
        //                "<IncomeAccountStd />" +
        //                "<IncomeAccountInternal1 />" +
        //                "<IncomeAccountInternal2 />" +
        //                "<IncomeAccountInternal3 />" +
        //                "<IncomeAccountInternal4 />" +
        //                "<IncomeAccountInternal5 />" +
        //                "<ExternalSchedule>true</ExternalSchedule>" +
        //                "<LoginName>200</LoginName>" +
        //                "<Password />" +
        //                "<ChangePassword>True</ChangePassword>" +
        //                "<LangId>Svenska</LangId>" +
        //                "<DefaultCompanyName>Test Tiltid</DefaultCompanyName>" +
        //                "<RoleName1 />" +
        //                "<RoleName2 />" +
        //                "<RoleName3 />" +
        //                "<AttestRoleName1 />" +
        //                "<AttestRoleName2 />" +
        //                "<AttestRoleName3 />" +
        //                "<AttestRoleName4 />" +
        //                "<AttestRoleName5 />" +
        //                "<Note />" +
        //                "<State>0</State>" +
        //              "</Employee>" +
        //          "<Employees>";
        //}

        //#endregion

        //#region Time

        //#region Export

        //[WebMethod(Description = "Get XE Employees", EnableSession = false)]
        //public string ExportEmployees(int source, string apiKey, DateTime lastSynchDate)
        //{
        //    #region Validation

        //    int companyId;
        //    TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
        //    TermGroup_IOType ioType = TermGroup_IOType.Unknown;

        //    string validationMessage = Validate(source, apiKey, out companyId, out ioSource, out ioType);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    ImportExportManager iom = new ImportExportManager(GetParameterObject(companyId));
        //    var xdoc = iom.ExportEmployees(ioSource, ioType, lastSynchDate, companyId);

        //    return xdoc.ToString();
        //}

        //[WebMethod(Description = "Get XE Employees. Return employees in xml. lastSynchDate format: yyyy-MM-dd hh:MM:ss", EnableSession = false)]
        //public string ExportEmployees2(int source, string apiKey, string lastSynchDate)
        //{
        //    #region Validation

        //    DateTime dateTime;
        //    string validationMessage = ValidateLastSynchDate(lastSynchDate, out dateTime);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    return ExportEmployees(source, apiKey, dateTime);
        //}

        //[WebMethod(Description = "Get XE approved shifts. Return approved shifts in xml", EnableSession = false)]
        //public string ExportApprovedShifts(int source, string apiKey, DateTime lastSynchDate)
        //{
        //    #region Validation

        //    int companyId;
        //    TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
        //    TermGroup_IOType ioType = TermGroup_IOType.Unknown;

        //    string validationMessage = Validate(source, apiKey, out companyId, out ioSource, out ioType);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    ImportExportManager iom = new ImportExportManager(GetParameterObject(companyId));
        //    var xdoc = iom.ExportApprovedShifts(lastSynchDate, companyId);

        //    return xdoc.ToString();
        //}

        //[WebMethod(Description = "Get XE approved shifts. Return approved shifts in xml. lastSynchDate format: yyyy-MM-dd hh:MM:ss", EnableSession = false)]
        //public string ExportApprovedShifts2(int source, string apiKey, string lastSynchDate)
        //{
        //    #region Validation

        //    DateTime dateTime;
        //    string validationMessage = ValidateLastSynchDate(lastSynchDate, out dateTime);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    return ExportApprovedShifts(source, apiKey, dateTime);
        //}

        //[WebMethod(Description = "Get XE approved absence. Return approved absence in xml", EnableSession = false)]
        //public string ExportApprovedAbsence(int source, string apiKey, DateTime lastSynchDate)
        //{
        //    #region Validation

        //    int companyId;
        //    TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
        //    TermGroup_IOType ioType = TermGroup_IOType.Unknown;

        //    string validationMessage = Validate(source, apiKey, out companyId, out ioSource, out ioType);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    ImportExportManager iom = new ImportExportManager(GetParameterObject(companyId));
        //    var xdoc = iom.ExportApprovedAbsence(lastSynchDate, companyId);

        //    return xdoc.ToString();
        //}

        //[WebMethod(Description = "Get XE approved absence. Return approved absence in xml. lastSynchDate format: yyyy-MM-dd hh:MM:ss", EnableSession = false)]
        //public string ExportApprovedAbsence2(int source, string apiKey, string lastSynchDate)
        //{
        //    #region Validation

        //    DateTime dateTime;
        //    string validationMessage = ValidateLastSynchDate(lastSynchDate, out dateTime);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    return ExportApprovedAbsence(source, apiKey, dateTime);
        //}

        //[WebMethod(Description = "Get XE schedule. Return schedule in xml", EnableSession = false)]
        //public string ExportSchedules(int source, string apiKey, DateTime lastSynchDate)
        //{
        //    #region Validation

        //    int companyId;
        //    TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
        //    TermGroup_IOType ioType = TermGroup_IOType.Unknown;

        //    string validationMessage = Validate(source, apiKey, out companyId, out ioSource, out ioType);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    ImportExportManager iom = new ImportExportManager(GetParameterObject(companyId));
        //    var xdoc = iom.ExportSchedules(lastSynchDate, companyId);

        //    return xdoc.ToString();
        //}

        //[WebMethod(Description = "Get XE schedule. Return schedule in xml. lastSynchDate format: yyyy-MM-dd hh:MM:ss", EnableSession = false)]
        //public string ExportSchedules2(int source, string apiKey, string lastSynchDate)
        //{
        //    #region Validation

        //    DateTime dateTime;
        //    string validationMessage = ValidateLastSynchDate(lastSynchDate, out dateTime);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    return ExportSchedules(source, apiKey, dateTime);
        //}

        //#endregion

        //#region Import

        //[WebMethod(Description = "Update XE Employees. Return number of employees affected", EnableSession = false)]
        //public string ImportEmployees(int source, string apiKey, string xml)
        //{
        //    #region Validation

        //    int companyId;
        //    TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
        //    TermGroup_IOType ioType = TermGroup_IOType.Unknown;

        //    string validationMessage = Validate(source, apiKey, out companyId, out ioSource, out ioType);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    var m = new ImportExportManager(GetParameterObject(companyId));
        //    var result = m.ImportToEmployeeIO(ioSource, ioType, xml.ToString(), companyId, false);

        //    return result.IntegerValue.ToString();
        //}

        //[WebMethod(Description = "Update XE schedules. Return number of schedules affected", EnableSession = false)]
        //public string ImportSchedules(int source, string apiKey, string xml)
        //{
        //    #region Validation

        //    int companyId;
        //    TermGroup_IOSource ioSource = TermGroup_IOSource.Unknown;
        //    TermGroup_IOType ioType = TermGroup_IOType.Unknown;

        //    string validationMessage = Validate(source, apiKey, out companyId, out ioSource, out ioType);
        //    if (!String.IsNullOrEmpty(validationMessage))
        //        return validationMessage;

        //    #endregion

        //    var m = new ImportExportManager(GetParameterObject(companyId));
        //    var result = m.ImportTimeScheduleSyncEntrys(ioSource, ioType, xml.ToString(), companyId);

        //    return result.IntegerValue.ToString();
        //}

        //#endregion

        //#endregion

        //#region Help-methods

        //private string Validate(int source, string apiKey, out int companyId, out TermGroup_IOSource ioSource, out TermGroup_IOType ioType)
        //{
        //    ioSource = TermGroup_IOSource.Unknown;
        //    ioType = TermGroup_IOType.WebService;

        //    string validationMessage = "";
        //    companyId = 0;

        //    if (!ValidateCompany(apiKey, out companyId))
        //        validationMessage = VALIDATION_APIKEY_ERRORMESSAGE;
        //    if (!ValidateSource(source, out ioSource))
        //        validationMessage = VALIDATION_SOURCE_ERRORMESSAGE;

        //    return validationMessage;
        //}

        //private string ValidateLastSynchDate(string lastSynchDate, out DateTime dateTime)
        //{
        //    string validationMessage = "";

        //    if (!DateTime.TryParse(lastSynchDate, out dateTime))
        //        validationMessage = VALIDATION_LASTSYNCHDATE_ERRORMESSAGE;

        //    return validationMessage;
        //}

        //private bool ValidateCompany(string apiKey, out int companyId)
        //{
        //    bool valid = false;
        //    companyId = 0;

        //    int? actorCompanyId = new CompanyManager(null).GetActorCompanyIdFromApiKey(apiKey);
        //    if (actorCompanyId.HasValue && actorCompanyId.Value > 0)
        //    {
        //        companyId = actorCompanyId.Value;
        //        valid = true;
        //    }

        //    return valid;
        //}

        //private bool ValidateSource(int source, out TermGroup_IOSource ioSource)
        //{
        //    ioSource = TermGroup_IOSource.Unknown;

        //    bool valid = false;
        //    switch (source)
        //    {
        //        case (int)TermGroup_IOSource.TilTid:
        //            ioSource = TermGroup_IOSource.TilTid;
        //            valid = true;
        //            break;
        //        case (int)TermGroup_IOSource.FlexForce:
        //            ioSource = TermGroup_IOSource.FlexForce;
        //            valid = false; //Not supported
        //            break;
        //    }
        //    return valid;
        //}

        //#endregion
    }

}
