using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SoftOne.Soe.Common.DTO
{
    public class CompanySetupWizardDTO
    {
        //General
        public int UserId { get; set; }
        public int ActorCompanyId { get; set; }
        public int LicenseId { get; set; }

        //Company details
        public int CompanyType { get; set; }
        public string CompanyName { get; set; }
        public string CompanyOrgNr { get; set; }
        public int? CompanyNr { get; set; }
        public string CompanyShortName { get; set; }
        public CompanyLogoDTO CompanyLogo { get; set; }
        public List<ContactAddressItem> ContactAddresses { get; set; }

        //Company settings
        public int UserListReportId { get; set; }

        //Employees
        public List<CompanySetupEmployeeDTO> Employees { get; set; }

        //Accounting
        public DateTime AccountingYearFrom { get; set; }
        public DateTime AccountingYearTo { get; set; }
        public CompanySetupWizardPaymentMethodDTO CustomerBankGiro { get; set; }
        public CompanySetupWizardPaymentMethodDTO CustomerPostGiro { get; set; }
        public CompanySetupWizardPaymentMethodDTO SupplierPostGiro { get; set; }
        public CompanySetupWizardPaymentMethodDTO SupplierBankGiro { get; set; }

        public string ErrorMessage { get; set; }
        public bool Success { get; set; }

        #region Interface implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(ref T field, T value, string name)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }

    public class CompanySetupWizardCompany
    {
        public int CompanyType { get; set; }
        public string Name { get; set; }
        public string OrgNr { get; set; }
        public string CompanyNr { get; set; }
    }

    public class CompanyLogoDTO
    {
        public byte[] Logo { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
    }

    public class CompanySetupEmployeeDTO : INotifyPropertyChanged
    {
        public int EmployeeId { get; set; }

        private string userName;
        public string UserName { get { return userName; } set { SetProperty(ref userName, value, "UserName"); } }
        private string employeeNr;
        public string EmployeeNr { get { return employeeNr; } set { SetProperty(ref employeeNr, value, "EmployeeNr"); } }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        private string email;
        public string Email { get { return email; } set { SetProperty(ref email, value, "Email"); this.UserName = value; } }
        private string password;
        public string Password { get { return password; } set { SetProperty(ref password, value, "Password"); } }

        private int roleId;
        public int RoleId { get { return roleId; } set { SetProperty(ref roleId, value, "RoleId"); } }
        public string RoleName { get; set; }

        public string CategoryName { get; set; }
        private int? categoryId;
        public int? CategoryId { get { return categoryId; } set { SetProperty(ref categoryId, value, "CategoryId"); } }

        private int? timeCodeId;
        public int? TimeCodeId { get { return timeCodeId; } set { SetProperty(ref timeCodeId, value, "TimeCodeId"); } }
        public string TimeCodeString { get; set; }

        private int? attestRoleId;
        public int? AttestRoleId { get { return attestRoleId; } set { SetProperty(ref attestRoleId, value, "AttestRoleId"); } }

        public string EmployeeNrSort { get; set; }

        #region Interface implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(ref T field, T value, string name)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }

    public class CompanySetupWizardPaymentMethodDTO
    {
        public string BankAccountNr { get; set; }
        public int AccountId { get; set; }
        public string CustomerNr { get; set; }

        public int PaymentMethodId { get; set; }
    }

    public class EmployeeValidationResult
    {
        public string LoginName { get; set; }
        public int LoginNameCount { get; set; }
        public int EmployeeNrCount { get; set; }
        public bool IsValid { get; set; }
        public int RowId { get; set; }
    }
}
