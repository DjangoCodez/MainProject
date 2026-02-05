using SoftOne.Soe.Shared.DTO;
using System.Collections.Generic;
using SoftOne.Soe.Common.Util;

namespace Soe.Business.Tests.Business.API.Employee
{
    internal class ImportEmployeeChangesTestsInput
    {
        public EmployeeChangeType Type { get; set; }
        public List<string> EmployeeNrs { get; set; }
        public ImportEmployeeChangesTestsInputParameters Parameters { get; set; }

        public ImportEmployeeChangesTestsInput(EmployeeChangeType type, ImportEmployeeChangesTestsInputParameters parameters, params string[] employeeNrs)
        {
            this.Type = type;
            this.EmployeeNrs = new List<string>();
            if (!employeeNrs.IsNullOrEmpty())
                this.EmployeeNrs.AddRange(employeeNrs);
            this.Parameters = parameters ?? new ImportEmployeeChangesTestsInputParameters();
        }
    }

    internal class ImportEmployeeChangesTestsInputParameters
    {
        public bool NewValue { get; set; }
        public bool NewOptionalExternalCode1 { get; set; }
        public bool NewOptionalExternalCode2 { get; set; }
        public bool UseFirstEmployment { get; set; }
        public bool NewFromDate { get; set; }
        public bool NewToDate { get; set; }
        public bool ForceInvalid { get; set; }
        public bool Delete { get; set; }
        public bool CustomFlag { get; set; } //Used differently for each flag, look at implementation to see usage

        public ImportEmployeeChangesTestsInputParameters(bool newValue = false, bool newOptionalExternalCode1 = false, bool newOptionalExternalCode2 = false, bool useFirstEmployment = true, bool newFromDate = false, bool newToDate = false, bool forceInvalid = false, bool delete = false, bool customFlag = false)
        {
            this.NewValue = newValue;
            this.NewOptionalExternalCode1 = newOptionalExternalCode1;
            this.NewOptionalExternalCode2 = newOptionalExternalCode2;
            this.UseFirstEmployment = useFirstEmployment;
            this.NewFromDate = newFromDate;
            this.NewToDate = newToDate;
            this.ForceInvalid = forceInvalid;
            this.Delete = delete;
            this.CustomFlag = customFlag;
        }
    }
}
