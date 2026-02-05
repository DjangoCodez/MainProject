// TODO Jukka 9.8.2013
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight;
using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    class PolAdapter : BaseSalaryAdapter, ISalaryAdapter
    {

        private readonly List<TransactionItem> payrollTransactionItems;
        private readonly List<ScheduleItem> scheduleItems;
        private readonly List<Employee> employees;
        private readonly DateTime periodFromDate;
        private readonly DateTime periodToDate;
        private readonly string externalExportId;
        private readonly string companyName;
        private readonly List<EmployeeChildDTO> employeeChildren;
        private readonly List<TimeDeviationCause> timeDeviationCauses;

        #region Constructors

        public PolAdapter(DateTime periodFromDate, DateTime periodToDate, string externalExportId, string companyName, List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, List<Employee> employees, List<EmployeeChildDTO> employeeChildren, List<TimeDeviationCause> timeDeviationCauses = null)
        {
            this.periodFromDate = periodFromDate;
            this.periodToDate = periodToDate;
            this.externalExportId = externalExportId;
            this.companyName = companyName;
            this.payrollTransactionItems = payrollTransactions;
            this.scheduleItems = scheduleItems;
            this.employees = employees;
            this.employeeChildren = employeeChildren;
            this.timeDeviationCauses = timeDeviationCauses;
        }

        #endregion

        #region Public methods

        public byte[] TransformSalary(XDocument baseXml)
        {
            return CreateFiles();
        }

        #endregion

        private byte[] CreateFiles()
        {
            string guid = this.externalExportId + DateTime.Now.Millisecond.ToString();
            string tempfolder = ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_SALARY_PHYSICAL;
            string zippedpath = $@"{tempfolder}{companyName}{guid}.zip";

            var polSalaryAdapter = new PolSalaryAdapter();
            var input = new SalaryAdapterInput(externalExportId, periodFromDate, periodToDate, payrollTransactionItems, scheduleItems, employees, employeeChildren, timeDeviationCauses);
            var files = polSalaryAdapter.GetSalaryFiles(input);

            Dictionary<string, byte[]> dict = files.SalaryFiles.Where(w => w.File.Length > 0).Select(s => new KeyValuePair<string, byte[]>($@"{tempfolder}{s.FileName}", s.File)).ToDictionary(x => x.Key, x => x.Value);
            if (ZipUtility.ZipFiles(zippedpath, dict))
            {
                var result = File.ReadAllBytes(zippedpath);
                File.Delete(zippedpath);

                return result;
            }

            return null;
        }
    }
}