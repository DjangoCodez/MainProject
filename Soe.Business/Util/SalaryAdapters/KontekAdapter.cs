using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    public class KontekAdapter : ISalarySplittedFormatAdapter
    {
        public XDocument schedule;
        public XDocument salary;
        private XNamespace nsS;
        private XNamespace nsDt;
        private XNamespace nsRs;
        private XNamespace nsZ;

        #region Salary

        /// <summary>
        /// Transforming to konteks format
        /// </summary>
        /// <param name="baseXml"></param>
        /// <returns></returns>
        public byte[] TransformSalary(XDocument baseXml)
        {
            XDocument doc = CreateSalaryDocument();
            doc.Root.Add(GetSalarySchema());
            doc.Root.Add(GetSalaryData(baseXml));

            MemoryStream stream = new MemoryStream();
            doc.Save(stream);
            return stream.ToArray();
        }

        private XDocument CreateSalaryDocument()
        {
            XDocument doc = new XDocument(
                 new XDeclaration("1.0", Constants.ENCODING_LATIN1_NAME, "true"));

            nsS = "uuid:BDC6E3F0-6DA3-11d1-A2A3-00AA00C14882";
            nsDt = "uuid:C2F41010-65B3-11d1-A29F-00AA00C14882";
            nsRs = "urn:schemas-microsoft-com:rowset";
            nsZ = "#RowsetSchema";

            XElement root = new XElement("xml",
                new XAttribute(XNamespace.Xmlns + "s", nsS.NamespaceName),
                 new XAttribute(XNamespace.Xmlns + "dt", nsDt.NamespaceName),
                 new XAttribute(XNamespace.Xmlns + "rs", nsRs.NamespaceName),
                 new XAttribute(XNamespace.Xmlns + "z", nsZ.NamespaceName));

            doc.Add(root);
            return doc;
        }

        #region Data

        private XElement GetSalaryData(XDocument baseXml)
        {
            XElement data = new XElement(nsRs + "data");
            List<XElement> employeeTransactions = new List<XElement>();
            IEnumerable<XElement> employees = (from e in baseXml.Descendants("employees")
                                               select e);

            foreach (XElement employee in employees.Elements("employee"))
            {
                employeeTransactions.AddRange(GetEmployeeSalaryBasedTransactions(employee));
                employeeTransactions.AddRange(GetEmployeeAbsenceBasedTransactions(employee));
            }

            employeeTransactions.ForEach(i => data.Add(i));
            return data;
        }

        private List<XElement> GetEmployeeAbsenceBasedTransactions(XElement employee)
        {
            List<XElement> result = new List<XElement>();
            IEnumerable<XElement> transactions = employee.Descendants("absences");
            foreach (XElement invoicetrans in transactions.Elements("invoicetransactions"))
            {
                foreach (XElement trans in invoicetrans.Elements("transaction"))
                {
                    result.Add(GetSalaryTransactionElement(employee, trans));
                }
            }
            foreach (XElement invoicetrans in transactions.Elements("payrolltransactions"))
            {
                foreach (XElement trans in invoicetrans.Elements("transaction"))
                {
                    result.Add(GetSalaryTransactionElement(employee, trans));
                }
            }
            return result;
        }

        private List<XElement> GetEmployeeSalaryBasedTransactions(XElement employee)
        {
            List<XElement> result = new List<XElement>();
            IEnumerable<XElement> transactions = employee.Descendants("transactions");
            foreach (XElement invoicetrans in transactions.Elements("invoicetransactions"))
            {
                foreach (XElement trans in invoicetrans.Elements("transaction"))
                {
                    result.Add(GetSalaryTransactionElement(employee, trans));
                }
            }
            foreach (XElement invoicetrans in transactions.Elements("payrolltransactions"))
            {
                foreach (XElement trans in invoicetrans.Elements("transaction"))
                {
                    result.Add(GetSalaryTransactionElement(employee, trans));
                }
            }
            return result;
        }

        private XElement GetSalaryTransactionElement(XElement employee, XElement trans)
        {
            bool absence = false;
            bool.TryParse(trans.Attribute("isabsence").Value, out absence);

            string accountName = string.Empty;
            string accountInternalName = string.Empty;
            XElement account = trans.Element("account");
            if (account != null)
            {
                accountName = account.Attribute("nr").Value;
            }
            XElement accountInternal = trans.Element("accountinternal");
            if (accountInternal != null)
            {
                accountInternalName = accountInternal.Attribute("nr").Value;
            }

            XElement transaction = new XElement(nsZ + "Tidtransar",
                new XAttribute("Transtyp", (absence ? 3 : 1)),
                new XAttribute("AnstId", FormatEmployeeId(employee.Attribute("nr").Value)),
                new XAttribute("Kod", trans.Attribute("productnumber").Value),
                new XAttribute("DatumFrom", GetDate(trans.Attribute("date").Value)),
                new XAttribute("DatumTom", GetDate(trans.Attribute("date").Value)),
                //new XAttribute("Niva4", ?,
                //new XAttribute("Apris", ?,
                //new XAttribute("Belopp", ?,
                //new XAttribute("Kostnadsstalle", ?,
                //new XAttribute("Omfattning", ?,
                //new XAttribute("Moms", ?,
                new XAttribute("Antal", GetAmount(trans.Attribute("quantity").Value)));

            if (!string.IsNullOrEmpty(accountName))
            {
                transaction.Add(new XAttribute("Konto", accountName));
            }
            if (!string.IsNullOrEmpty(accountInternalName))
            {
                transaction.Add(new XAttribute("Objekt", accountInternalName));
            }

            return transaction;
        }

        /// <summary>
        /// EmployeeId in kontek is a blank padded string of the length of four characters
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        private string FormatEmployeeId(string employeeId)
        {
            if (employeeId.Length < 5)
            {
                while (employeeId.Length != 4)
                    employeeId = " " + employeeId;
            }

            return employeeId;
        }

        #endregion

        #region Schema

        private XElement GetSalarySchema()
        {
            return new XElement(nsS + "Schema",
                                new XAttribute("id", "RowsetSchema"),
                                new XElement(nsS + "ElementType",
                                    new XAttribute("name", "Tidtransar"),
                                    new XAttribute("content", "eltOnly"),
                                    GetSchemaAttribute("TransTyp", 1, "int", 4, 10),
                                    GetSchemaAttribute("AnstId", 2, "string", 4),
                                    GetSchemaAttribute("Kod", 3, "int", 4, 10),
                                    GetSchemaAttribute("DatumFrom", 4, "int", 4, 10),
                                    GetSchemaAttribute("DatumTom", 5, "int", 4, 10),
                                    GetSchemaAttribute("Antal", 6, "float", 8, 15),
                                    GetSchemaAttribute("Apris", 7, "float", 8, 15),
                                    GetSchemaAttribute("Belopp", 8, "float", 8, 15),
                                    GetSchemaAttribute("Konto", 9, "string", 10),
                                    GetSchemaAttribute("Kostnadsstalle", 10, "string", 10),
                                    GetSchemaAttribute("Niva4", 11, "string", 32),
                                    GetSchemaAttribute("Objekt", 12, "string", 10),
                                    GetSchemaAttribute("Omfattning", 13, "float", 8, 15),
                                    GetSchemaAttribute("Moms", 14, "float", 8, 15),
                                    new XElement(nsS + "extends",
                                        new XAttribute("type", "rs:rowbase"))
                                ));
        }

        #endregion

        #endregion

        #region Schedule

        public byte[] TransformSchedule(XDocument baseXml)
        {
            XDocument doc = CreateScheduleDocument();
            doc.Root.Add(GetScheduleSchema());
            doc.Root.Add(GetScheduleData(baseXml));

            MemoryStream stream = new MemoryStream();
            doc.Save(stream);
            return stream.ToArray();
        }

        private XDocument CreateScheduleDocument()
        {
            XDocument doc = new XDocument(
                 new XDeclaration("1.0", Constants.ENCODING_LATIN1_NAME, "true"));

            XElement root = new XElement("xml",
                new XAttribute(XNamespace.Xmlns + "s", nsS.NamespaceName),
                 new XAttribute(XNamespace.Xmlns + "dt", nsDt.NamespaceName),
                 new XAttribute(XNamespace.Xmlns + "rs", nsRs.NamespaceName),
                 new XAttribute(XNamespace.Xmlns + "z", nsZ.NamespaceName));

            doc.Add(root);
            return doc;
        }

        #region Data

        private XElement GetScheduleData(XDocument baseXml)
        {
            XElement data = new XElement(nsRs + "data");
            IEnumerable<XElement> employees = baseXml.Descendants("employees");

            List<XElement> employeeTransactions = new List<XElement>();
            foreach (var employee in employees.Elements("employee"))
            {
                employeeTransactions.AddRange(GetEmployeeScheduleTransactions(employee));
            }

            employeeTransactions.ForEach(i => data.Add(i));
            return data;
        }

        private IEnumerable<XElement> GetEmployeeScheduleTransactions(XElement employee)
        {
            List<XElement> result = new List<XElement>();
            IEnumerable<XElement> schedules = employee.Descendants("schedules");
            foreach (XElement schedule in schedules.Elements("schedule"))
            {
                foreach (XElement day in schedule.Elements("day"))
                {
                    result.Add(GetScheduleTransactionElement(
                        employee.Attribute("nr").Value,
                        day.Attribute("date").Value,
                        day.Attribute("productnumber").Value,
                        GetHoursFromTotalMinutes(day.Attribute("totaltimemin").Value, day.Attribute("totalbreakmin").Value)));
                }
            }
            return result;
        }

        private string GetHoursFromTotalMinutes(string totalMinutes, string totalBreakMinutes)
        {
            double breakMinutes = 0;
            double minutes = 0;
            double.TryParse(totalMinutes, out minutes);
            double.TryParse(totalBreakMinutes, out breakMinutes);
            minutes -= breakMinutes;
            totalMinutes = Math.Round(minutes / 60, 2, MidpointRounding.ToEven).ToString();
            totalMinutes = totalMinutes.Replace(",", ".");
            return totalMinutes;
        }

        private XElement GetScheduleTransactionElement(string employeeNr, string transactionDate, string productNumber, string transactionHours)
        {
            return new XElement(nsZ + "Tidtransar",
                new XAttribute("Transtyp", "10"),//always
                new XAttribute("AnstId", FormatEmployeeId(employeeNr)),
                new XAttribute("Kod", productNumber),
                new XAttribute("Datum", GetDate(transactionDate)),
                new XAttribute("Timmar", transactionHours));
        }

        #endregion

        #region Schema

        private XElement GetScheduleSchema()
        {
            return new XElement(nsS + "Schema",
                                new XAttribute("id", "RowsetSchema"),
                                new XElement(nsS + "ElementType",
                                    new XAttribute("name", "Tidtransar"),
                                    new XAttribute("content", "eltOnly"),
                                    GetSchemaAttribute("TransTyp", 1, "int", 4, 10),
                                    GetSchemaAttribute("AnstId", 2, "string", 4),
                                    GetSchemaAttribute("Datum", 3, "int", 4, 10),
                                    GetSchemaAttribute("Timmar", 4, "float", 8, 15),
                                    new XElement(nsS + "extends",
                                        new XAttribute("type", "rs:rowbase"))
                                ));
        }

        #endregion

        #endregion

        #region Help methods

        private string GetDate(string date)
        {
            DateTime dateTime = new DateTime();
            DateTime.TryParse(date, out dateTime);
            return dateTime.ToShortDateString().Replace("-", "");
        }

        public static string AddLocalNames(XDocument kontekXml)
        {
            string kontekXmlWithLocalizedNames = kontekXml.ToString();

            //todo traverse nodes, replace names

            return kontekXmlWithLocalizedNames;
        }

        private string GetAmount(string amount)
        {
            decimal value = 0;
            amount = amount.Replace(".", ",");
            decimal.TryParse(amount, out value);
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);
            string result = value.ToString();
            result = result.Replace(",", ".");
            return result;
        }

        private XElement GetSchemaAttribute(string name, int number, string type, int maxLength, int precision)
        {
            return new XElement(nsS + "AttributeType",
               new XAttribute("name", name),
               new XAttribute(nsRs + "number", number.ToString()),
               new XAttribute(nsRs + "nullable", "true"),
               new XAttribute(nsRs + "write", "true"),
               new XElement(nsS + "datatype",
                   new XAttribute(nsDt + "type", type),
                   new XAttribute(nsDt + "maxLength", number.ToString()),
                   new XAttribute(nsRs + "precision", precision.ToString())
               )
           );
        }
        private XElement GetSchemaAttribute(string name, int number, string type, int maxLength)
        {
            return new XElement(nsS + "AttributeType",
               new XAttribute("name", name),
               new XAttribute(nsRs + "number", number.ToString()),
               new XAttribute(nsRs + "nullable", "true"),
               new XAttribute(nsRs + "write", "true"),
               new XElement(nsS + "datatype",
                   new XAttribute(nsDt + "type", type),
                   new XAttribute(nsDt + "maxLength", number.ToString())
               )
           );
        }

        #endregion
    }
}