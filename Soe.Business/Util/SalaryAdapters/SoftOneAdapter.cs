using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    public class SoftOneAdapter : ISalaryAdapter
    {
        private static readonly Random Rng = new Random();
        private readonly CompEntities context;
        private string vatProductNr;
        int productId = 0;

        #region Constructors

        public SoftOneAdapter(CompEntities entities, int actorCompanyId)
        {
            context = entities;

            // Fetch from companysetting
            SettingManager sm = new SettingManager(null);
            productId = sm.GetIntSetting(context, SettingMainType.Company, (int)CompanySettingType.SalaryExportVatProductId, 0, actorCompanyId, 0);
        }

        #endregion

        #region Salary

        /// <summary>
        /// Transforming to softones format
        /// </summary>
        /// <param name="baseXml"></param>
        /// <returns></returns>
        public byte[] TransformSalary(XDocument baseXml)
        {
            string doc = string.Empty;
            if (context != null)
                doc = GetSchedule(baseXml);
            doc += GetTimeTransactions(baseXml);
            return Encoding.UTF8.GetBytes(doc);
        }

        #region Data

        private string GetTimeTransactions(XDocument baseXml)
        {
            var sb = new StringBuilder();
            sb.Append("//Transaktioner");
            sb.Append(Environment.NewLine);
            IEnumerable<XElement> employees = (from e in baseXml.Descendants("employees")
                                               select e);

            foreach (XElement employee in employees.Elements("employee"))
            {
                sb.Append(GetEmployeeSalaryBasedTransactions(employee));
                sb.Append(GetEmployeeAbsenceBasedTransactions(employee));
            }

            return sb.ToString();
        }

        private string GetEmployeeAbsenceBasedTransactions(XElement employee)
        {
            var sb = new StringBuilder();

            IEnumerable<XElement> transactions = employee.Descendants("absences");
            foreach (XElement invoicetrans in transactions.Elements("invoicetransactions"))
            {
                foreach (XElement trans in invoicetrans.Elements("transaction"))
                {
                    sb.Append(GetSalaryTransactionElement(employee, trans));
                }
            }
            foreach (XElement payrolltrans in transactions.Elements("payrolltransactions"))
            {
                foreach (XElement trans in payrolltrans.Elements("transaction"))
                {
                    sb.Append(GetSalaryTransactionElement(employee, trans));
                }
            }
            return sb.ToString();
        }

        private string GetEmployeeSalaryBasedTransactions(XElement employee)
        {
            var sb = new StringBuilder();

            IEnumerable<XElement> transactions = employee.Descendants("transactions");
            foreach (XElement invoicetrans in transactions.Elements("invoicetransactions"))
            {
                foreach (XElement trans in invoicetrans.Elements("transaction"))
                {
                    sb.Append(GetSalaryTransactionElement(employee, trans));
                }
            }
            foreach (XElement payrolltrans in transactions.Elements("payrolltransactions"))
            {
                foreach (XElement trans in payrolltrans.Elements("transaction"))
                {
                    sb.Append(GetSalaryTransactionElement(employee, trans));
                }
            }


            return sb.ToString();
        }

        private string GetSalaryTransactionElement(XElement employee, XElement trans, bool useVatAmount = false)
        {
            var sb = new StringBuilder();

            if (trans != null)
            {
                bool absence;
                bool.TryParse(trans.Attribute("isabsence").Value, out absence);
                bool isRegistrationTypeQuantity;
                bool isRegistrationTypeTime;
                bool.TryParse(trans.Attribute("isRegistrationTypeQuantity").Value, out isRegistrationTypeQuantity);
                bool.TryParse(trans.Attribute("isRegistrationTypeTime").Value, out isRegistrationTypeTime);

                sb.Append(employee.Attribute("nr").Value);
                sb.Append(",");
                //sb.Append(employee.Attribute("employeegroupname").Value); //category, not required
                sb.Append(",");
                if (useVatAmount)
                {
                    sb.Append(vatProductNr);
                }
                else
                {
                    sb.Append(trans.Attribute("productnumber").Value);
                }
                sb.Append(",");
                sb.Append(isRegistrationTypeTime ? SalaryExportUtil.GetTimeFromMinutes(trans.Attribute("quantity").Value) : trans.Attribute("quantity").Value);
                sb.Append(",");
                sb.Append(GetDate(trans.Attribute("date").Value, true));
                sb.Append(",");
                sb.Append(GetDate(trans.Attribute("date").Value, true));
                sb.Append(",");
                if (trans.Attribute("productnumber").Value == "3")
                {
                    sb.Append("1,1,");
                }
                else
                {
                    sb.Append(",,");
                }
                if (useVatAmount)
                {
                    sb.Append((trans.Attribute("vatAmount").Value).Replace(",", "."));
                }
                else
                {
                    string amount = ((trans.Attribute("amount").Value).Replace(",", "."));

                    try
                    {
                        if (productId > 0)
                        {
                            if (trans.Attribute("vatAmount").Value != null || !"0".Equals(trans.Attribute("vatAmount").Value.ToString()) || !String.IsNullOrEmpty(trans.Attribute("amount").Value.ToString()) || !String.IsNullOrEmpty(trans.Attribute("vatAmount").Value.ToString()))
                            {

                                decimal decAmount = Convert.ToDecimal((trans.Attribute("amount").Value));

                                string fix = ((trans.Attribute("vatAmount").Value)).Replace(".", ",");

                                decimal decVatAmount = Convert.ToDecimal(fix);

                                decimal withoutVatamount = decAmount - decVatAmount;

                                amount = withoutVatamount.ToString().Replace(",", ".");
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        amount = ((trans.Attribute("amount").Value).Replace(",", "."));
                        exp.ToString();
                    }

                    sb.Append(amount);

                }

                sb.Append(",");
                if (!String.IsNullOrEmpty((trans.Attribute("amount").Value)) && !useVatAmount)
                    sb.Append(GetAccountInfo(trans.Element("account"), trans.Element("internalaccounts")));
                else
                    sb.Append(GetAccountInfo(trans.Element("internalaccounts")));

                sb.Append(",");
                if ((trans.Element("internalaccounts").HasElements) && !String.IsNullOrEmpty((trans.Attribute("amount").Value)))
                    sb.Append("3");
                else if (trans.Element("internalaccounts").HasElements)
                    sb.Append("2");
                else if (!String.IsNullOrEmpty((trans.Attribute("amount").Value)))
                    sb.Append("1");
                else
                    sb.Append("");
                sb.Append(",");
                //As of now comments are not included in the file.
                //sb.Append(trans.Attribute("comment").Value.Replace(",","."));
                //sb.Append(GetAbsencePercentage(absence));
                sb.Append(Environment.NewLine);

                // Do another row for vat
                if (!useVatAmount)
                {
                    decimal vatAmount;
                    if (decimal.TryParse(trans.Attribute("vatAmount").Value.Replace('.', ','), out vatAmount) && vatAmount > 0)
                    {
                        if (string.IsNullOrEmpty(vatProductNr))
                        {
                            if (productId > 0)
                            {
                                var pm = new ProductManager(null);
                                PayrollProduct product = pm.GetPayrollProduct(context, productId);
                                vatProductNr = product.Number;
                            }
                        }

                        if (!string.IsNullOrEmpty(vatProductNr))
                        {
                            // Recursive method to do the same thing again but with uservatamount set to true.
                            sb.Append(GetSalaryTransactionElement(employee, trans, true));
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private static string GetAccountInfo(XElement account, XElement internalAccounts)
        {
            if (account == null)
                return string.Empty;
            var sb = new StringBuilder();
            sb.Append(account.Attribute("nr").Value);
            foreach (XElement internalAccount in internalAccounts.Elements())
            {
                sb.Append(";");
                sb.Append(internalAccount.Attribute("nr").Value);
            }
            return sb.ToString();
        }

        private static string GetAccountInfo(XElement internalAccounts)
        {
            var sb = new StringBuilder();

            string dim1 = ";";
            string dim2 = ";";
            string dim3 = ";";
            string dim4 = ";";
            string dim5 = ";";
            string dim6 = ";";


            foreach (XElement internalAccount in internalAccounts.Elements())
            {
                if (internalAccount.Attribute("siedimnr").Value == "1")
                    dim1 += internalAccount.Attribute("nr").Value;

                if (internalAccount.Attribute("siedimnr").Value == "2")
                    dim2 += internalAccount.Attribute("nr").Value;

                if (internalAccount.Attribute("siedimnr").Value == "3")
                    dim3 += internalAccount.Attribute("nr").Value;

                if (internalAccount.Attribute("siedimnr").Value == "4")
                    dim4 += internalAccount.Attribute("nr").Value;

                if (internalAccount.Attribute("siedimnr").Value == "5")
                    dim5 += internalAccount.Attribute("nr").Value;

                if (internalAccount.Attribute("siedimnr").Value == "6")
                    dim6 += internalAccount.Attribute("nr").Value;
            }

            sb.Append(dim1 + dim2 + dim3 + dim4 + dim5 + dim6);
    
            return sb.ToString();
        }

        private static int GetNumberOfDays(string fromdate, string todate)
        {
            int result = 0;
            DateTime to;
            DateTime from;

            if (DateTime.TryParse(fromdate, out from) && DateTime.TryParse(todate, out to))
            {
                result = (int)(to - from).TotalDays;
            }
            if (result == 0)
                result++;
            return result;
        }

        private static int GetNumberOfScheduleDays(string fromdate, string todate)
        {
            int days = 0;
            DateTime from;
            DateTime to;

            if (DateTime.TryParse(fromdate, out from) && DateTime.TryParse(todate, out to))
            {
                days = (int)(to - from).TotalDays;
                if (days == 0)
                    days++;
            }

            return days;
        }

        #endregion

        #endregion

        #region Schedule

        #region Data

        private string GetSchedule(XDocument baseXml)
        {
            var sb = new StringBuilder();
            sb.Append("//Schema");
            sb.Append(Environment.NewLine);
            var uniqueScheduleDefinitions = new List<Day>();
            //var identifiers = new Dictionary<string, List<string>>();
            IEnumerable<XElement> employees = baseXml.Descendants("employees");
            foreach (XElement employee in employees.Elements("employee"))
            {
                IEnumerable<XElement> schedules = employee.Descendants("schedules");
                foreach (XElement schedule in schedules.Elements("schedule"))
                {
                    foreach (XElement day in schedule.Elements("day"))
                    {
                        //string id = day.Attribute("timescheduletemplateperiodid").Value;

                        //Not added to an identifier
                        //if (identifiers.Values.Where(i => i.Contains(id)).Count() > 0) continue;

                        string start = GetTimeFromClock(day.Attribute("starttime").Value);
                        string stop = GetTimeFromClock(day.Attribute("stoptime").Value);
                        string totalHours = GetHoursFromTotalMinutes(day.Attribute("totaltimemin").Value,
                                                                     day.Attribute("totalbreakmin").Value);

                        //Look if we already have the this days definition
                        if (uniqueScheduleDefinitions.Any(i => i.Start == start && i.Stop == stop && i.TotalHours == totalHours))
                        {
                            /*  string idf = (from x in scheduleDays
                                            where x.Start == start
                                                  && x.Stop == stop
                                                  && x.TotalHours == totalHours
                                            select x.Identifier).FirstOrDefault();
                              if (idf != null)
                              {
                                  if (identifiers.ContainsKey(idf))
                                      identifiers[idf].Add(id);
                              }*/
                            continue;
                        }

                        //Create schedule definition
                        sb.Append("@,");
                        string identifier = GetNewSoftOneId(context);
                        sb.Append(identifier);
                        sb.Append(",");
                        sb.Append(start);
                        sb.Append(",");
                        sb.Append(stop);
                        sb.Append(",");
                        sb.Append(totalHours);
                        sb.Append(Environment.NewLine);

                        //identifiers.Add(identifier, new List<string> {id});
                        uniqueScheduleDefinitions.Add(new Day
                        {
                            Identifier = identifier,
                            Start = start,
                            Stop = stop,
                            //TimeScheduleTemplatePeriodId = id,
                            TotalHours = totalHours,
                        });
                    }
                }
            }

            foreach (XElement employee in employees.Elements("employee"))
            {
                IEnumerable<XElement> schedules = employee.Descendants("schedules");
                foreach (XElement schedule in schedules.Elements("schedule"))
                {
                    foreach (XElement day in schedule.Elements("day"))
                    {
                        //Get schedule transactions
                        string start = GetTimeFromClock(day.Attribute("starttime").Value);
                        string stop = GetTimeFromClock(day.Attribute("stoptime").Value);
                        string totalHours = GetHoursFromTotalMinutes(day.Attribute("totaltimemin").Value,
                                                                     day.Attribute("totalbreakmin").Value);
                        string identifier = string.Empty;
                        /*foreach (var pair in identifiers)
                        {
                            if (pair.Value.Contains(day.Attribute("timescheduletemplateperiodid").Value))
                                identifier = pair.Key;
                        }*/

                        string idf = (from x in uniqueScheduleDefinitions
                                      where x.Start == start
                                            && x.Stop == stop
                                            && x.TotalHours == totalHours
                                      select x.Identifier).FirstOrDefault();
                        if (idf != null)
                        {
                            identifier = idf;
                        }


                        sb.Append(employee.Attribute("nr").Value);
                        sb.Append(",");
                        sb.Append(identifier);
                        sb.Append(",");
                        sb.Append("@");
                        sb.Append(",");
                        sb.Append(GetDate(day.Attribute("date").Value, false));
                        sb.Append(Environment.NewLine);
                    }
                }
            }
            return sb.ToString();
        }

        #endregion

        #endregion

        #region Help methods

        private static string GetDate(string date, bool noMarkup)
        {
            DateTime dateTime;
            DateTime.TryParse(date, out dateTime);
            if (noMarkup)
                return dateTime.ToString("yyyyMMdd");
            return dateTime.ToString("yyyy-MM-dd");
        }

        private static string GetTimeFromClock(string clock)
        {
            return clock.Replace(":", ".");
        }

        private static string GetHoursFromTotalMinutes(string totalMinutes, string totalBreakMinutes)
        {
            double breakMinutes;
            double minutes;
            double.TryParse(totalMinutes, out minutes);

            // if break on zero day
            if (minutes == 0)
                return GetClock(0);

            double.TryParse(totalBreakMinutes, out breakMinutes);
            minutes -= breakMinutes;
            return GetClock(minutes);
        }

        private static string GetClock(double minutes)
        {
            var clock = new StringBuilder();
            int hours = (int)minutes / 60;
            if (hours < 10)
                clock.Append("0");
            clock.Append(hours);
            clock.Append(".");
            int min = (int)minutes - (hours * 60);
            if (min < 10)
                clock.Append("0");
            clock.Append(min);
            return clock.ToString();
        }

        private string GetNewSoftOneId(CompEntities entities)
        {
            string result = GetUniqueSoftOneId(entities);
            var identifier = new TimeSalaryExportSoftOneIdentifier
            {
                id = result,
            };
            entities.TimeSalaryExportSoftOneIdentifier.AddObject(identifier);

            return result;
        }

        private static bool SoftOneIdExists(CompEntities entities, string result)
        {
            return (from x in entities.TimeSalaryExportSoftOneIdentifier
                    where x.id.Equals(result)
                    select x.id).Count() > 0;
        }

        private static string GetUniqueSoftOneId(CompEntities entities)
        {
            string result = GenerateSoftOneId();
            if (SoftOneIdExists(entities, result))
                GetUniqueSoftOneId(entities);
            return result;
        }

        private static string GenerateSoftOneId()
        {
            char[] valid = {
                               'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R',
                               'S', 'T', 'U', 'V', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k',
                               'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'x', 'y', 'z', '1', '2', '3', '4',
                               '5', '6', '7', '8', '9', '0'
                           };
            var sb = new StringBuilder("");
            for (int i = 0; i < 10; i++)
                sb.Append(valid[Rng.Next(valid.Length)]);
            return sb.ToString();
        }

        #endregion

        #region Nested type: Day

        private class Day
        {
            public string Identifier { get; set; }
            public string TimeScheduleTemplatePeriodId { private get; set; }
            public string Start { get; set; }
            public string Stop { get; set; }
            public string TotalHours { get; set; }
        }

        #endregion


    }
}