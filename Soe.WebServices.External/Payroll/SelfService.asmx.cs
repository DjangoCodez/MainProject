using System.Web.Services;

namespace Soe.WebServices.External.Payroll
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class SelfService : WebserviceBase
    {
        #region Constants

        private const string PAYROLL_LOGINNAME = "Payroll";

        #endregion

        #region Common

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        #endregion

        #region Employee

        //[WebMethod(Description = "Get employees for Payroll Update. Use user with name Payroll, ShortCoName = actorcompanyid", EnableSession = false)]
        //public SSData[] GetEmployees(string shortCoName, string password, DateTime fromDate)
        //{
        //    SSData[] employeeStruct = new SSData[0];

        //    return employeeStruct;

        //    ////Managers
        //    //EmployeeManager em = new EmployeeManager(null);
        //    //UserManager um = new UserManager(null);
        //    //ContactManager cm = new ContactManager(null);
        //    //LoginManager lm = new LoginManager(null);
        //    //CompanyManager cpm = new CompanyManager(null);

        //    //// Variables
        //    //Int32 i = 0;
        //    //int actorCompanyId = int.Parse(shortCoName);

        //    //// Get Company, User, Role
        //    //string detailedMessage = "";
        //    //Company company = cpm.GetCompany(actorCompanyId, true);
        //    //User user = um.GetUser(company.License.LicenseNr, PAYROLL_LOGINNAME);
        //    //Role role = new Role();

        //    //SoeLoginState state = lm.LoginUser(company.License.LicenseNr, PAYROLL_LOGINNAME, password, out detailedMessage, out company, out user, out role, false, false);
        //    //if (state != SoeLoginState.OK)
        //    //    return employeeStruct;

        //    ////Get number of Employees that are changed or created
        //    //List<Employee> employees = null;
        //    //employees = em.GetAllEmployees(actorCompanyId, active: true, loadEmployment: true, loadUser: true, loadContact: true);
        //    //employees = employees.Where(e => ((e.Modified.HasValue && e.Modified >= fromDate) || (e.Created.HasValue && e.Created >= fromDate))).ToList();

        //    ////Update struct
        //    //employeeStruct = new SSData[employees.Count()];

        //    ////Get information on updated or created
        //    //foreach (Employee employee in employees)
        //    //{
        //    //    // Create temporary SoftOne Sync Data SSData
        //    //    SSData sSDatatmp;

        //    //    int contactId = 0;
        //    //    Contact contact = employee.ContactPerson.Actor.Contact.FirstOrDefault();
        //    //    if (contact != null)
        //    //        contactId = contact.ContactId;

        //    //    // Get information from employee table
        //    //    if (employee.EmployeeNr != null)
        //    //        sSDatatmp.EmployeeNumber = employee.EmployeeNr.ToString();
        //    //    else sSDatatmp.EmployeeNumber = "";

        //    //    if (employee.SocialSec != null)
        //    //        sSDatatmp.PersonalNumber = string.Empty;
        //    //    else
        //    //        sSDatatmp.PersonalNumber = string.Empty;

        //    //    if (employee.FirstName != null)
        //    //        sSDatatmp.PreName = employee.FirstName.ToString();
        //    //    else
        //    //        sSDatatmp.PreName = "";

        //    //    if (employee.LastName != null)
        //    //        sSDatatmp.SurName = employee.LastName.ToString();
        //    //    else
        //    //        sSDatatmp.SurName = "";

        //    //    // Get Distribution adress
        //    //    List<ContactAddressRow> contactAddressRows = cm.GetContactAddressRows(contactId, (int)TermGroup_SysContactAddressType.Distribution);

        //    //    // Get Address
        //    //    ContactAddressRow contactAddressRowAddress = (from Row in contactAddressRows
        //    //                                                  where Row.ContactAddress.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution &&
        //    //                                                  Row.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address
        //    //                                                  select Row).FirstOrDefault<ContactAddressRow>();

        //    //    if (contactAddressRowAddress != null)
        //    //        sSDatatmp.Address = contactAddressRowAddress.Text.ToString();
        //    //    else
        //    //        sSDatatmp.Address = "";

        //    //    //Get PostalCode
        //    //    ContactAddressRow contactAddressRowZipCode = (from Row in contactAddressRows
        //    //                                                  where Row.ContactAddress.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution &&
        //    //                                                  Row.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode
        //    //                                                  select Row).FirstOrDefault<ContactAddressRow>();

        //    //    if (contactAddressRowZipCode != null)
        //    //        sSDatatmp.ZipCode = contactAddressRowZipCode.Text.ToString();
        //    //    else
        //    //        sSDatatmp.ZipCode = "";

        //    //    // Get PostalAddress
        //    //    ContactAddressRow contactAddressRowCity = (from Row in contactAddressRows
        //    //                                               where Row.ContactAddress.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution &&
        //    //                                                Row.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress
        //    //                                               select Row).FirstOrDefault<ContactAddressRow>();

        //    //    if (contactAddressRowCity != null)
        //    //        sSDatatmp.City = contactAddressRowCity.Text.ToString();
        //    //    else
        //    //        sSDatatmp.City = "";

        //    //    // Get PhoneHome
        //    //    ContactECom phoneHome = cm.GetContactECom(contactId, (int)TermGroup_SysContactEComType.PhoneHome, false);
        //    //    if (phoneHome != null)
        //    //        sSDatatmp.PhoneHome = phoneHome.Text.ToString();
        //    //    else
        //    //        sSDatatmp.PhoneHome = "";

        //    //    //Get PhoneMobile
        //    //    ContactECom phoneMobil = cm.GetContactECom(contactId, (int)TermGroup_SysContactEComType.PhoneMobile, false);
        //    //    if (phoneMobil != null)
        //    //        sSDatatmp.PhoneCellular = phoneMobil.Text.ToString();
        //    //    else
        //    //        sSDatatmp.PhoneCellular = "";

        //    //    //Get PhoneJob
        //    //    ContactECom phoneJob = cm.GetContactECom(contactId, (int)TermGroup_SysContactEComType.PhoneJob, false);
        //    //    if (phoneJob != null)
        //    //        sSDatatmp.PhoneWork = phoneJob.Text.ToString();
        //    //    else
        //    //        sSDatatmp.PhoneWork = "";

        //    //    // Get Email
        //    //    ContactECom email = cm.GetContactECom(contactId, (int)TermGroup_SysContactEComType.Email, false);
        //    //    if (email != null)
        //    //        sSDatatmp.EMail = email.Text.ToString();
        //    //    else
        //    //        sSDatatmp.EMail = "";

        //    //    // Last changed or modified
        //    //    if (employee.Modified.HasValue)
        //    //        sSDatatmp.changeDate = employee.Modified.ToString();
        //    //    else
        //    //        sSDatatmp.changeDate = employee.Created.ToString();

        //    //    // Department not valid in XE.
        //    //    sSDatatmp.depNumber = "";

        //    //    // Get OrgNr
        //    //    if (employee.Company.OrgNr.Length > 0)
        //    //        sSDatatmp.orgNumber = employee.Company.OrgNr.ToString();
        //    //    else
        //    //        sSDatatmp.orgNumber = "";

        //    //    // Add to employeestruct
        //    //    employeeStruct[i++] = sSDatatmp;
        //    //}

        //    //return employeeStruct;
        //}

        #endregion

        #region Structs

        public struct SSData
        {
            public string EmployeeNumber;
            public string PersonalNumber;
            public string PreName;
            public string SurName;
            public string Address;
            public string ZipCode;
            public string City;
            public string PhoneHome;
            public string PhoneCellular;
            public string PhoneWork;
            public string EMail;
            public string changeDate;
            public string depNumber;
            public string orgNumber;
        }

        #endregion
    }
}