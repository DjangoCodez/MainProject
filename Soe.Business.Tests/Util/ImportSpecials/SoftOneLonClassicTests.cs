using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.ImportSpecials;
using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.ImportSpecials.Tests
{
    [TestClass()]
    public class SoftOneLonClassicTests
    {
        [TestMethod()]
        public void ParseExampleFile1()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/Util/ImportSpecials/ExampleFiles/SoftOneLonClassic/example1.dat";
            SoftOneLonClassic classic = new SoftOneLonClassic();

            List<PayrollProductGridDTO> payrollProducts = new List<PayrollProductGridDTO>() { new PayrollProductGridDTO() { Number = "134" } };
            List<TimeDeviationCauseDTO> timeDeviationCauses = new List<TimeDeviationCauseDTO>() { new TimeDeviationCauseDTO() { ExtCode = "SJU" } };
            List<AccountDimDTO> accountDims = new List<AccountDimDTO>() { new AccountDimDTO() { AccountDimNr = 1, Accounts = new List<AccountDTO>() { new AccountDTO() { AccountNr = "7010" } } } };
            accountDims.Add(new AccountDimDTO() { AccountDimNr = 2, SysSieDimNr = 1, Accounts = new List<AccountDTO>() { new AccountDTO() { AccountNr = "10" } } });
            accountDims.Add(new AccountDimDTO() { AccountDimNr = 3, SysSieDimNr = 2, Accounts = new List<AccountDTO>() { new AccountDTO() { AccountNr = "20" } } });
            accountDims.Add(new AccountDimDTO() { AccountDimNr = 4, SysSieDimNr = 6, Accounts = new List<AccountDTO>() { new AccountDTO() { AccountNr = "30" } } });
            List<EmployeeDTO> employees = new List<EmployeeDTO>() { new EmployeeDTO() { EmployeeNr = "105" } };
            employees.Add(new EmployeeDTO() { EmployeeNr = "113" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "121" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "130" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "140" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "141" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "142" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "155" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "163" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "165" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "178" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "180" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "185" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "193" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "194" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "197" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "199" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "2" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "204" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "209" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "219" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "221" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "235" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "238" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "244" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "248" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "258" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "259" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "276" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "280" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "282" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "283" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "286" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "288" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "289" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "290" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "291" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "292" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "293" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "294" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "295" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "296" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "54" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "78" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "80" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "87" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "904" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "905" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "906" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "914" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "915" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "917" });
            employees.Add(new EmployeeDTO() { EmployeeNr = "921" });            
            employees.Add(new EmployeeDTO()           {                EmployeeNr = "923"            });
            var head = classic.ParseToPayrollImportHead(1, File.ReadAllBytes(path), DateTime.Today, employees, accountDims, payrollProducts, timeDeviationCauses);
            Assert.IsNotNull(head);
        }

        [TestMethod()]
        public void ParseToPayrollImportHeadTest()
        {
            SoftOneLonClassic classic = new SoftOneLonClassic();

            List<PayrollProductGridDTO> payrollProducts = new List<PayrollProductGridDTO>() { new PayrollProductGridDTO() { Number = "134" } };
            List<TimeDeviationCauseDTO> timeDeviationCauses = new List<TimeDeviationCauseDTO>() { new TimeDeviationCauseDTO() { ExtCode = "SJU" } };
            List<AccountDimDTO> accountDims = new List<AccountDimDTO>() { new AccountDimDTO() { AccountDimNr = 1, Accounts = new List<AccountDTO>() { new AccountDTO() { AccountNr = "7010" } } } };
            accountDims.Add(new AccountDimDTO() { AccountDimNr = 2, SysSieDimNr = 1, Accounts = new List<AccountDTO>() { new AccountDTO() { AccountNr = "10" } } });
            accountDims.Add(new AccountDimDTO() { AccountDimNr = 3, SysSieDimNr = 2, Accounts = new List<AccountDTO>() { new AccountDTO() { AccountNr = "20" } } });
            accountDims.Add(new AccountDimDTO() { AccountDimNr = 4, SysSieDimNr = 6, Accounts = new List<AccountDTO>() { new AccountDTO() { AccountNr = "30" } } });
            List<EmployeeDTO> employees = new List<EmployeeDTO>() { new EmployeeDTO() { EmployeeNr = "1000" } };
            var head = classic.ParseToPayrollImportHead(1, Encoding.UTF8.GetBytes(ExempleFile1()), DateTime.Today, employees, accountDims, payrollProducts, timeDeviationCauses);
            Assert.IsNotNull(head);
        }

        private string ExempleFile1()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("1000,,134,1" + Environment.NewLine);
            sb.Append("1000,,SJU,1.0,20230117,20230117,01,01,0,7010;10;20;30,3" + Environment.NewLine);
            sb.Append("@,1,8.00,16.30,8.00" + Environment.NewLine);
            sb.Append("@,2,7.00,16.15,8.25" + Environment.NewLine);
            sb.Append("@,3,7.00,14.00,6.20" + Environment.NewLine);
            sb.Append("@,4,7.00,15.45,8.00" + Environment.NewLine);
            // Schematransaktion för anställd 10001
            sb.Append("1000,1,@,2023-01-16" + Environment.NewLine);
            sb.Append("1000,2,@,2023-01-17" + Environment.NewLine);
            sb.Append("1000,3,@,2023-01-18" + Environment.NewLine);
            sb.Append("1000,4,@,2023-01-19" + Environment.NewLine);
            sb.Append("1000,3,@,2023-01-20" + Environment.NewLine);
            sb.Append("1000,2,@,2023-01-21" + Environment.NewLine);
            return sb.ToString();
        }
    }
}