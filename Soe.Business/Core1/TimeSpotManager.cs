using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    /// <summary>
    /// Manager/Funnel with all things concerning TimeSpot
    /// </summary>
    public class TimeSpotManager
    {
        #region Constants

        protected const string THREAD = "Timespot";

        #endregion

        #region Ctor

        public TimeSpotManager()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Kontrollera om givet guid finns och är giltigt
        /// </summary>
        /// <param name="GUID">Identifierare för företaget</param>
        /// <returns>>=1 (värdet på fältet cntrl_id) om ok, annars 0, -1 vid error.</returns>
        public int ValidateControlInfo(string GUID)
        {
            //GUID = ActorCompanyId
            //cntrl_id = TimeSpotId

            try
            {
                #region Init

                CompanyManager cm = new CompanyManager(null);

                int actorCompanyId = Convert.ToInt32(GUID);

                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                {
                    return Convert.ToInt32(GUID);
                }

                Company company = cm.GetCompany(actorCompanyId);

                #endregion

                #region Perform

                return company != null && company.TimeSpotId.HasValue ? company.TimeSpotId.Value : 0;

                #endregion
            }
            catch (Exception ex)
            {
                ex.ToString();

                return -1;
            }
        }

        public int tsValidateControlInfo(string GUID)
        {
            //GUID = TimeTerminalId

            try
            {
                #region Init

                TimeStampManager tsm = new TimeStampManager(null);
                int timeTerminalId = Convert.ToInt32(GUID);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(timeTerminalId);

                #endregion

                #region Perform

                return tt != null ? tt.TimeTerminalId : 0;

                #endregion
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
                return -1;
            }
        }

        /// <summary>
        /// Kontrollerar om det finns några ändringar i registret med anställda
        /// Returnerar 0 om inget har hänt. 
        /// </summary>
        /// <param name="GUID">Identifierare för företaget</param>
        /// <param name="modified">Datum från vilket ändringar skall kontrolleras</param>
        /// <returns>Returnerar 1 om något har hänt, annars 0</returns>
        public int CheckEmployeeChanges(string GUID, DateTime modified)
        {
            // Kontrollera om något har häntDvs anställda registret i WSTcom_db är oförändrat till innehåll och status
            //  Om något har hänt har WTService skrivit detta i tabellen CNTRL_INFO	( Objekt = EMPLOYEE_INFO )

            try
            {
                #region Init

                EmployeeManager em = new EmployeeManager(null);

                int actorCompanyId = Convert.ToInt32(GUID);

                #endregion

                #region Perform

                List<Employee> employees = em.GetAllEmployees(actorCompanyId, active: null, loadEmployment: true);
                int count = employees.Count(e => !e.CurrentEmployeeGroup.AutogenTimeblocks && ((e.Modified.HasValue && e.Modified >= modified) || (e.Created.HasValue && e.Created >= modified)));

                return count;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return -1;
            }
        }

        public int tsCheckEmployeeChanges(string GUID, DateTime modified)
        {
            // Kontrollera om något har häntDvs anställda registret i WSTcom_db är oförändrat till innehåll och status
            //  Om något har hänt har WTService skrivit detta i tabellen CNTRL_INFO	( Objekt = EMPLOYEE_INFO )

            try
            {
                #region Init

                EmployeeManager em = new EmployeeManager(null);
                TimeStampManager tsm = new TimeStampManager(null);
                int timeTerminalId = Convert.ToInt32(GUID);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(timeTerminalId);

                int actorCompanyId = tt.ActorCompanyId;

                #endregion

                #region Perform

                List<Employee> employees = em.GetAllEmployees(actorCompanyId, active: null, loadEmployment: true);
                int count = employees.Count(e => !e.CurrentEmployeeGroup.AutogenTimeblocks && ((e.Modified.HasValue && e.Modified >= modified) || (e.Created.HasValue && e.Created >= modified)));

                return count;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return -1;
            }
        }

        /// <summary>
        /// Hämta information om anställda
        /// </summary>
        /// <param name="GUID">Identifierare för företag</param>
        /// <param name="modified">Datum från vilket ändringar skall kontrolleras</param> 
        /// <param name="employeeNr">Hämta endast anställ med angivet nummer </param>
        /// <returns>Array med en rad per anställd</returns>
        public string[] GetEmployeeInfo(string GUID, DateTime modified, string employeeNr) //Klar
        {
            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                CompanyManager cm = new CompanyManager(null);
                TimeStampManager tsm = new TimeStampManager(null);

                int actorCompanyId = Convert.ToInt32(GUID);
                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                    actorCompanyId = 167444;

                Company company = cm.GetCompany(actorCompanyId);

                #endregion

                #region Perform

                int i = 0;
                List<TimeSpotEmployeeView> timeSpotEmployees = tsm.GetTimeSpotEmployeeForCompany(actorCompanyId);
                foreach (TimeSpotEmployeeView employee in timeSpotEmployees)
                {
                    TsEmployee tsEmployee = new TsEmployee();
                    tsEmployee.ConvertSoeEmployee(employee, company);
                    string packedRow = tsEmployee.PackString();
                    retvalList.Add(packedRow);
                    i++;
                }

                // Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        public string[] tsGetEmployeeInfo(string GUID, DateTime modified, string employeeNr) //Klar
        {
            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                EmployeeManager em = new EmployeeManager(null);
                TimeStampManager tsm = new TimeStampManager(null);
                CompanyManager cm = new CompanyManager(null);
                int timeTerminalId = Convert.ToInt32(GUID);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(timeTerminalId);
                int actorCompanyId = tt.ActorCompanyId;

                Company company = cm.GetCompany(actorCompanyId);

                #endregion

                #region Prereq

                List<Employee> employees = new List<Employee>();
                List<Category> categories = tsm.GetCategoriesByTimeTerminal(actorCompanyId, timeTerminalId);

                foreach (Category category in categories)
                {
                    employees.AddRange(em.GetEmployeesByCategory(category.CategoryId, actorCompanyId));
                }

                #endregion

                #region Perform

                int i = 0;
                foreach (Employee employee in employees)
                {
                    TsEmployee tsEmployee = new TsEmployee();
                    tsEmployee.tsConvertSoeEmployee(employee, company);
                    string packedRow = tsEmployee.PackString();
                    retvalList.Add(packedRow);
                    i++;
                }

                // Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        /// <summary>
        /// Ta emot en stämplingstransaktion och sparar i databasen
        /// </summary>
        /// <param name="GUID">Identifierare för företag</param>
        /// <param name="transString">Packade transaktioner</param>
        /// <returns>-1:ogiltig trans, 0 kan inte spara i db, 1 sparad i db</returns>
        public int GetTerminalTransaction(string GUID, string transString)
        {
            try
            {
                // Tar emot en strängarray enligt specificerat format
                // Packar upp arrayen och sparar informationen i databasen i en transaktion
                // Kontrollerar att uppdateringen i databasen har gått rätt. Om inte gör en rollback och returnerar -1
                // Om Ok commit i databasen och returnera 1

                #region Init

                ActionResult result = new ActionResult();

                TimeStampManager tsm = new TimeStampManager(null);

                int actorCompanyId = Convert.ToInt32(GUID);
                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                    actorCompanyId = 167444;

                #endregion

                #region Prereq

                // Packa upp strängen
                TsTransaction tsTransaction = new TsTransaction();

                try
                {
                    tsTransaction.UnpackString(transString);

                    TimeStampEntryRawDTO dto = new TimeStampEntryRawDTO()
                    {
                        ActorCompanyRecordId = actorCompanyId,
                        EmployeeNr = String.IsNullOrEmpty(tsTransaction.EmpNum) ? String.Empty : tsTransaction.EmpNum,
                        Status = (int)TermGroup_TimeStampEntryStatus.New,
                        Time = tsTransaction.Date,
                        TerminalStampData = transString,
                        TimeTerminalRecordId = tsTransaction.MachineId.Length > 0 ? Convert.ToInt32(tsTransaction.MachineId) : 0,
                        TimeDeviationCauseRecordId = tsTransaction.TimeCode.Length > 0 ? Convert.ToInt32(tsTransaction.TimeCode) : 0,
                        AccountRecordId = tsTransaction.CostPlace.Length > 0 ? Convert.ToInt32(tsTransaction.CostPlace) : 0,
                        IsBreak = false,
                    };

                    if (tsTransaction.Type == "A")
                        dto.Type = TimeStampEntryType.In;
                    else if (tsTransaction.Type == "B")
                        dto.Type = TimeStampEntryType.Out;
                    else
                        dto.Type = TimeStampEntryType.Unknown;

                    // if the terminal is not found, look for other terminal in that company
                    if (dto.TimeTerminalRecordId == 0)
                    {
                        List<TimeTerminal> tt = tsm.GetTimeTerminals(actorCompanyId, TimeTerminalType.TimeSpot, true, false, false);

                        foreach (var item in tt)
                        {
                            dto.TimeTerminalRecordId = item.TimeTerminalId;
                            dto.TerminalStampData = transString + " New Timeterminal ";

                        }

                    }

                    result = tsm.SaveTimeStampEntryRaw(dto);

                }
                catch (Exception ex)
                {
                    LogError(ex);
                    return 1; // return 1 not -1 in this case. Stamp in terminal will never work, but is logged in syslog                    
                }

                #endregion

                return result.Success.ToInt();
            }
            catch (Exception ex)
            {
                LogError(ex);
                return 1; // return 1 not -1 in this case. Stamp in terminal will never work, but is logged                
            }
        }

        public int tsGetTerminalTransaction(string GUID, string transString)
        {
            try
            {
                // Tar emot en strängarray enligt specificerat format
                // Packar upp arrayen och sparar informationen i databasen i en transaktion
                // Kontrollerar att uppdateringen i databasen har gått rätt. Om inte gör en rollback och returnerar -1
                // Om Ok commit i databasen och returnera 1

                #region Init

                ActionResult result = new ActionResult();

                TimeStampManager tsm = new TimeStampManager(null);
                int timeTerminalId = Convert.ToInt32(GUID);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(timeTerminalId);
                int actorCompanyId = tt.ActorCompanyId;

                #endregion

                #region Prereq

                // Packa upp strängen
                TsTransaction tsTransaction = new TsTransaction();

                try
                {
                    tsTransaction.UnpackString(transString);

                    TimeStampEntryRawDTO dto = new TimeStampEntryRawDTO()
                    {
                        ActorCompanyRecordId = actorCompanyId,
                        EmployeeNr = String.IsNullOrEmpty(tsTransaction.EmpNum) ? String.Empty : tsTransaction.EmpNum,
                        Status = (int)TermGroup_TimeStampEntryStatus.New,
                        Time = tsTransaction.Date,
                        TerminalStampData = transString,
                        TimeTerminalRecordId = tsTransaction.MachineId.Length > 0 ? Convert.ToInt32(tsTransaction.MachineId) : 0,
                        TimeDeviationCauseRecordId = tsTransaction.TimeCode.Length > 0 ? Convert.ToInt32(tsTransaction.TimeCode) : 0,
                        AccountRecordId = tsTransaction.CostPlace.Length > 0 ? Convert.ToInt32(tsTransaction.CostPlace) : 0,
                        IsBreak = false,
                    };

                    if (tsTransaction.Type == "A")
                        dto.Type = TimeStampEntryType.In;
                    else if (tsTransaction.Type == "B")
                        dto.Type = TimeStampEntryType.Out;
                    else
                        dto.Type = TimeStampEntryType.Unknown;

                    // if the terminal is not found, look for other terminal in that company
                    if (dto.TimeTerminalRecordId == 0)
                    {
                        List<TimeTerminal> tt2 = tsm.GetTimeTerminals(actorCompanyId, TimeTerminalType.TimeSpot, true, false, false);

                        foreach (var item in tt2)
                        {
                            dto.TimeTerminalRecordId = item.TimeTerminalId;
                            dto.TerminalStampData = transString + " New Timeterminal ";

                        }

                    }

                    result = tsm.SaveTimeStampEntryRaw(dto);

                }
                catch (Exception ex)
                {
                    LogError(ex);
                    return 1;// return 1 not -1 in this case. Stamp in terminal will never work, but is logged in syslog                    
                }

                #endregion

                return result.Success.ToInt();
            }
            catch (Exception ex)
            {
                LogError(ex);
                return 1; // return 1 not -1 in this case. Stamp in terminal will never work, but is logged                
            }
        }

        /// <summary>
        /// Kontrollera om det finns uppdaterade ackar
        /// </summary>
        /// <param name="GUID">Identifierare för företaget</param>
        /// <param name="modified">Datum från vilket ändringar skall kontrolleras</param>
        /// <returns>Returnerar antalet uppdaterade rader. 0 anger att inget är ändrat och ett negativt värde är felkod</returns>
        public int CheckAccChanges(string GUID, DateTime modified)
        {
            //Returnerar 0 om inget har hänt. Dvs saldo registret i WSTcom_db är oförändrat till innehåll och status
            //Returnerar 1 om något har hänt
            //Om något har hänt har WTService skrivit detta i tabellen CNTRL_INFO	( Objekt = ACC_INFO )

            try
            {
                #region Init

                TimeAccumulatorManager am = new TimeAccumulatorManager(null);

                int actorCompanyId = Convert.ToInt32(GUID);

                #endregion

                #region Perform

                return am.GetTimeAccumulators(actorCompanyId).Count(a => (a.Modified.HasValue && a.Modified >= modified) || (a.Created.HasValue && a.Created >= modified));

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);

                return -1;
            }
        }

        public int tsCheckAccChanges(string GUID, DateTime modified)
        {
            //Returnerar 0 om inget har hänt. Dvs saldo registret i WSTcom_db är oförändrat till innehåll och status
            //Returnerar 1 om något har hänt
            //Om något har hänt har WTService skrivit detta i tabellen CNTRL_INFO	( Objekt = ACC_INFO )

            try
            {
                #region Init

                TimeAccumulatorManager am = new TimeAccumulatorManager(null);
                TimeStampManager tsm = new TimeStampManager(null);
                int timeTerminalId = Convert.ToInt32(GUID);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(timeTerminalId);
                int actorCompanyId = tt.ActorCompanyId;

                GUID = GUID.Replace(Environment.NewLine, "");

                #endregion

                #region Perform

                return am.GetTimeAccumulators(actorCompanyId).Count(a => (a.Modified.HasValue && a.Modified >= modified) || (a.Created.HasValue && a.Created >= modified));

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);

                return -1;
            }
        }

        /// <summary>
        /// Hämtar ackar för anställda
        /// </summary>
        /// <param name="GUID">Företagsidentifierare</param>
        /// <param name="modified">Datum från vilket ändringar skall kontrolleras</param>
        /// <param name="employeeNr">Hämta endast anställ med angivet nummer </param>
        /// <returns>Ackar</returns>
        public string[] GetAccInfo(string GUID, DateTime modified, string employeeNr)
        {
            //Hämtar anställdas ackar i databasen WSTcom_db
            //Packar ihop varje ackumulator enligt ett visst format
            //Stoppar varje ackumulator i en strängarray
            //Returnerar strängarrayen som ett returvärde.
            //Returnerar strängarrayen med specialtecken (ERROR) i första post (Värde i andra post (-1), om något har gått fel och skriver till loggfilen.

            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                EmployeeManager em = new EmployeeManager(null);
                CompanyManager cm = new CompanyManager(null);
                TimeStampManager tsm = new TimeStampManager(null);

                GUID = GUID.Replace(Environment.NewLine, "");
                int actorCompanyId = Convert.ToInt32(GUID);

                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                    actorCompanyId = 167444;

                if (employeeNr == "")
                    return new string[0];

                Company company = cm.GetCompany(actorCompanyId);

                #endregion

                #region Prereq

                Employee employee = em.GetEmployeeByNr(employeeNr, actorCompanyId, loadEmployment: true);
                if (employee == null)
                    return new string[0];

                #endregion

                #region Perform

                int i = 0;
                var accumulatorItems = tsm.GetTimeAccumulator(actorCompanyId, employee.EmployeeId, employee.CurrentEmployeeGroupId, DateTime.Today, DateTime.Now);
                foreach (TSTimeAccumulatorEmployeeItem item in accumulatorItems)
                {
                    TsAcc acc = new TsAcc();
                    acc.ConvertSOETimeAccumulator(item, (int)company.TimeSpotId, employee.EmployeeNr, i + 1);
                    string packedRow = acc.PackString();
                    retvalList.Add(packedRow);
                    i++;
                }

                //Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        public string[] tsGetAccInfo(string GUID, DateTime modified, string employeeNr)
        {
            //Hämtar anställdas ackar i databasen WSTcom_db
            //Packar ihop varje ackumulator enligt ett visst format
            //Stoppar varje ackumulator i en strängarray
            //Returnerar strängarrayen som ett returvärde.
            //Returnerar strängarrayen med specialtecken (ERROR) i första post (Värde i andra post (-1), om något har gått fel och skriver till loggfilen.

            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                EmployeeManager em = new EmployeeManager(null);
                TimeStampManager tsm = new TimeStampManager(null);

                #endregion

                #region Prereq

                GUID = GUID.Replace(Environment.NewLine, "");
                int timeTerminalId = Convert.ToInt32(GUID);

                TimeTerminal timeTerminal = tsm.GetTimeTerminalDiscardState(timeTerminalId);
                Company company = timeTerminal.Company;

                int i = 0;

                #endregion

                #region Perform

                #region All Employees

                if (employeeNr == "")
                {
                    List<Employee> employees = new List<Employee>();
                    List<Category> categories = tsm.GetCategoriesByTimeTerminal(company.ActorCompanyId, timeTerminalId);

                    foreach (Category category in categories)
                    {
                        employees.AddRange(em.GetEmployeesByCategory(category.CategoryId, company.ActorCompanyId));
                    }

                    foreach (Employee employee in employees)
                    {
                        var accumulatorItemsForCompany = tsm.GetTimeAccumulator(company.ActorCompanyId, employee.EmployeeId, employee.GetCurrentEmployeeGroupId(em.GetEmployeeGroupsFromCache(company.ActorCompanyId)), DateTime.Today, DateTime.Now);
                        foreach (var timeAccumulator in accumulatorItemsForCompany)
                        {
                            TsAcc acc = new TsAcc();
                            acc.ConvertSOETimeAccumulator(timeAccumulator, (int)company.TimeSpotId, employee.EmployeeNr, i + 1);
                            string packedRow = acc.PackString();
                            retvalList.Add(packedRow);
                            i++;
                        }
                        i = 0;
                    }

                    //Kopiera svaret till sträng-array
                    retval = new string[retvalList.Count];

                    i = 0;
                    foreach (string s in retvalList)
                    {
                        retval[i] = s;
                        i++;
                    }

                    return retval;
                }

                #endregion

                #region One Employee

                Employee currentEmployee = em.GetEmployeeByNr(employeeNr, company.ActorCompanyId);
                if (currentEmployee == null)
                    return new string[0];

                var accumulatorItems = tsm.GetTimeAccumulator(company.ActorCompanyId, currentEmployee.EmployeeId, currentEmployee.CurrentEmployeeGroupId, DateTime.Today, DateTime.Now);
                foreach (TSTimeAccumulatorEmployeeItem item in accumulatorItems)
                {
                    TsAcc acc = new TsAcc();
                    acc.ConvertSOETimeAccumulator(item, (int)company.TimeSpotId, currentEmployee.EmployeeNr, i + 1);
                    string packedRow = acc.PackString();
                    retvalList.Add(packedRow);
                    i++;
                }

                //Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        /// <summary>
        /// Verifierar företagsnummer och returnerar företagets GUID
        /// </summary>
        /// <param name="timeSpot">TimeSpot nummer</param>
        /// <returns>Företagets GUID</returns>
        public string ValidateCompanyNumer(string timeSpot)
        {
            try
            {
                #region Init

                CompanyManager cm = new CompanyManager(null);

                #endregion

                #region Prereq

                timeSpot = timeSpot.Replace(Environment.NewLine, "");

                Company company = cm.GetCompanyFromTimeSpot(Convert.ToInt32(timeSpot));

                #endregion

                #region Perform

                if (timeSpot.ToString() == "999999" || timeSpot.ToString() == "999998" || timeSpot.ToString() == "999997" || timeSpot.ToString() == "TimeSpot")
                    return timeSpot;
                else
                    return company != null ? company.ActorCompanyId.ToString() : String.Empty;

                #endregion
            }
            catch (System.NullReferenceException)
            {
                return "";	// Hittar inte företagsnumret
            }
            catch (Exception ex)
            {
                LogError(ex);
                return "";
            }
        }

        public string tsValidateCompanyNumer(string timeSpot)
        {
            try
            {
                #region Init

                TimeStampManager tsm = new TimeStampManager(null);
                timeSpot = timeSpot.Replace(Environment.NewLine, "");
                int timeTerminalId = Convert.ToInt32(timeSpot);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(timeTerminalId);

                #endregion

                #region Perform

                return tt != null ? tt.TimeTerminalId.ToString() : String.Empty;

                #endregion
            }
            catch (System.NullReferenceException nrEx)
            {
                LogError(nrEx);
                return "";	// Hittar inte terminalen
            }
            catch (Exception ex)
            {
                LogError(ex);
                return "";
            }
        }

        /// <summary>
        /// Sparar stämplingsterminalens identifierare i tabell och/eller hämtar sätter namn och id.
        /// </summary>
        /// <param name="GUID">Företagsidentifierare</param>
        /// <param name="MAC">MAC address</param>
        /// <param name="name">Namn på terminalen.</param>
        /// <param name="timeTerminalId">Id på terminalen</param>
        /// <returns>Mindre än noll om fel har uppstått.</returns>
        public int SetMachineIdentity(string GUID, string MAC, ref string name, ref int timeTerminalId)
        {
            try
            {
                #region Init

                TimeStampManager tsm = new TimeStampManager(null);
                CompanyManager cm = new CompanyManager(null);

                int.TryParse(GUID, out int actorCompanyId);
                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                    actorCompanyId = 167444;

                if (string.IsNullOrEmpty(name))
                    name = "X";

                #endregion

                #region Perform

                TimeTerminal terminal = tsm.GetTimeTerminal(actorCompanyId, MAC, TimeTerminalType.TimeSpot);
                if (terminal == null)
                {
                    terminal = new TimeTerminal();
                    terminal.Type = (int)TimeTerminalType.TimeSpot;
                    terminal.MacAddress = MAC;
                    terminal.Name = name;
                    terminal.Registered = true;
                    terminal.State = (int)SoeEntityState.Active;
                    terminal.Company = cm.GetCompany(actorCompanyId);
                    terminal.TimeTerminalGuid = Guid.NewGuid();

                    ActionResult result = tsm.AddTimeTerminal(terminal, actorCompanyId);
                    terminal.TimeTerminalId = result.IntegerValue;
                }

                // Set last sync time
                terminal.LastSync = DateTime.Now;
                tsm.UpdateTimeTerminal(terminal);

                name = terminal.Name;
                timeTerminalId = terminal.TimeTerminalId;

                return terminal.TimeTerminalId;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return -1;
            }
        }

        public int tsSetMachineIdentity(string GUID, string MAC, ref string name, ref int timeTerminalId)
        {
            try
            {
                #region Init

                TimeStampManager tsm = new TimeStampManager(null);
                CompanyManager cm = new CompanyManager(null);

                GUID = GUID.Replace(Environment.NewLine, "");
                int.TryParse(GUID, out int ttId);

                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(ttId);

                int actorCompanyId = tt.ActorCompanyId;

                #endregion

                #region Perform

                TimeTerminal terminal = tsm.GetTimeTerminal(actorCompanyId, MAC, TimeTerminalType.TimeSpot);
                if (terminal == null)
                {
                    // Create new terminal
                    terminal = new TimeTerminal();
                    terminal.Type = (int)TimeTerminalType.TimeSpot;
                    terminal.MacAddress = MAC;
                    terminal.Name = name;
                    terminal.Registered = true;
                    terminal.State = (int)SoeEntityState.Active;
                    terminal.Company = cm.GetCompany(actorCompanyId);
                    terminal.TimeTerminalGuid = Guid.NewGuid();

                    ActionResult result = tsm.AddTimeTerminal(terminal, actorCompanyId);
                    terminal.TimeTerminalId = result.IntegerValue;
                }

                // Set last sync time
                terminal.LastSync = DateTime.Now;
                tsm.UpdateTimeTerminal(terminal);

                name = terminal.Name;
                timeTerminalId = terminal.TimeTerminalId;

                return terminal.TimeTerminalId;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return -1;	// Annat fel
            }
        }

        /// <summary>
        /// Hämta senaste stämplingar för anställd
        /// </summary>
        /// <param name="GUID">Företagsidentifierare</param>
        /// <param name="employeeNr">Anställdanummer</param>
        /// <returns>Transaktioner</returns>
        public string[] GetTransactions(string GUID, string employeeNr, int take)
        {
            //Hämtar en anställds senaste stämplingar
            //Packar ihop enligt ett visst format
            //Stoppar varje värde i en strängarray
            //Returnerar strängarrayen som ett returvärde.
            //Returnerar strängarrayen med specialtecken (ERROR) i första post (Värde i andra post (-1), om något har gått fel och skriver till loggfilen.

            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                TimeStampManager tsm = new TimeStampManager(null);
                EmployeeManager em = new EmployeeManager(null);

                if (employeeNr == null)
                    return new string[0];

                int actorCompanyId = Convert.ToInt32(GUID);
                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                    actorCompanyId = 167444;

                #endregion

                #region Perform

                //Max 50
                if (take > 50)
                    take = 50;

                int i = 0;

                List<TimeSpotTimeStampView> timeStampEntries = new List<TimeSpotTimeStampView>();

                int employeeId = 0;
                if (employeeNr.Length > 0)
                {
                    Employee employee = em.GetEmployeeByNr(employeeNr, actorCompanyId);
                    if (employee != null)
                        employeeId = employee.EmployeeId;

                    if (employee == null)
                        return new string[0];

                    timeStampEntries = tsm.GetTimeSpotTimeStampForEmployee(employeeId, new DateTime(1969, 01, 01), take);
                }
                else
                {
                    timeStampEntries = tsm.GetTimeSpotTimeStampForCompany(actorCompanyId, new DateTime(1969, 01, 01));
                }

                foreach (TimeSpotTimeStampView timeStampEntry in timeStampEntries)
                {
                    TsTransaction tsTransaction = new TsTransaction();
                    tsTransaction.ConvertSOETimeStampEntry(timeStampEntry);
                    string packedRow = tsTransaction.PackString();
                    retvalList.Add(packedRow);
                    i++;
                }

                // Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        public string[] tsGetTransactions(string GUID, string employeeNr, int take)
        {
            //Hämtar en anställds senaste stämplingar
            //Packar ihop enligt ett visst format
            //Stoppar varje värde i en strängarray
            //Returnerar strängarrayen som ett returvärde.
            //Returnerar strängarrayen med specialtecken (ERROR) i första post (Värde i andra post (-1), om något har gått fel och skriver till loggfilen.

            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                TimeStampManager tsm = new TimeStampManager(null);
                EmployeeManager em = new EmployeeManager(null);
                int timeTerminalId = Convert.ToInt32(GUID);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(timeTerminalId);

                if (employeeNr == null)
                    return new string[0];

                int actorCompanyId = tt.ActorCompanyId;

                #endregion

                #region Perform

                //Max 50
                if (take > 50)
                    take = 50;

                int i = 0;

                List<TimeSpotTimeStampView> timeStampEntries = new List<TimeSpotTimeStampView>();

                int employeeId = 0;
                if (employeeNr.Length > 0)
                {
                    Employee employee = em.GetEmployeeByNr(employeeNr, actorCompanyId);
                    if (employee != null)
                        employeeId = employee.EmployeeId;
                    else
                        return new string[0];

                    timeStampEntries = tsm.GetTimeSpotTimeStampForEmployee(employeeId, new DateTime(1969, 01, 01), take);
                }
                else
                {
                    List<Employee> employees = new List<Employee>();
                    List<Category> categories = tsm.GetCategoriesByTimeTerminal(actorCompanyId, timeTerminalId);

                    foreach (Category category in categories)
                    {
                        employees.AddRange(em.GetEmployeesByCategory(category.CategoryId, actorCompanyId));
                    }

                    foreach (var employeeGroup in employees.GroupBy(e => e.EmployeeId))
                    {
                        timeStampEntries.AddRange(tsm.GetTimeSpotTimeStampForEmployee(employeeGroup.Key, new DateTime(1969, 01, 01), take));
                    }
                }

                foreach (TimeSpotTimeStampView timeStampEntry in timeStampEntries)
                {
                    TsTransaction tsTransaction = new TsTransaction();
                    tsTransaction.ConvertSOETimeStampEntry(timeStampEntry);
                    string packedRow = tsTransaction.PackString();
                    retvalList.Add(packedRow);
                    i++;
                }

                // Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        /// <summary>
        /// Hämtar alla KSK-definitioner för en anställd
        /// </summary>
        /// <param name="GUID">Företagsidentifierare</param>
        /// <param name="employeeNr">Anställdanummer</param>
        /// <returns>Tidkoder</returns>
        public string[] GetEmpTimeCodes(string GUID, string employeeNr)
        {
            try
            {
                #region Perform

                return GetTimeCodes(GUID, employeeNr);

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        public string[] tsGetEmpTimeCodes(string GUID, string employeeNr)
        {
            try
            {
                #region Perform

                return tsGetTimeCodes(GUID, employeeNr);

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        /// <summary>
        /// Hämtar alla anställningsnummer för de som har nya definitioner för KSK
        /// </summary>
        /// <param name="GUID">Företagsidentifierare</param>
        /// <param name="modified">Datum från vilket ändringar skall kontrolleras</param>
        /// <returns>Tidkoder</returns>
        public string[] CheckEmpTimeCodes(string GUID, DateTime modified)
        {
            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                TimeStampManager tsm = new TimeStampManager(null);

                int actorCompanyId = Convert.ToInt32(GUID);
                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                    actorCompanyId = 167444;

                #endregion

                #region Perform

                int i = 0;
                List<TimeSpotTimeCodeViewForEmployee> timeCodes = tsm.GetTimeSpotTimeCodeForCompany(actorCompanyId, modified);
                foreach (var timeCodesGroup in timeCodes.GroupBy(p => p.EmployeeNr))
                {
                    TsEmpList tsEmpList = new TsEmpList();
                    string packedRow = tsEmpList.PackString(timeCodesGroup.Key);
                    retvalList.Add(packedRow);
                    i++;
                }

                // Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        public string[] tsCheckEmpTimeCodes(string GUID, DateTime modified)
        {
            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                TimeStampManager tsm = new TimeStampManager(null);
                int TimeTerminalId = Convert.ToInt32(GUID);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(TimeTerminalId);
                int actorCompanyId = tt.ActorCompanyId;

                int i = 0;

                #endregion

                #region Perform

                List<TimeSpotTimeCodeViewForEmployee> timeCodes = tsm.GetTimeSpotTimeCodeForCompany(actorCompanyId, modified);
                foreach (var timeCodeGroup in timeCodes.GroupBy(p => p.EmployeeNr))
                {
                    TsEmpList tsEmpList = new TsEmpList();
                    string packedRow = tsEmpList.PackString(timeCodeGroup.Key);
                    retvalList.Add(packedRow);
                    i++;
                }

                // Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        /// <summary>
        /// Hämtar alla orsaker och konton
        /// </summary>
        /// <param name="GUID">Företagsidentifierare</param>
        /// <param name="employeeNr">Anställdanummer</param>
        /// <returns>Orsaker och konton</returns>
        public string[] GetTimeCodes(string GUID, string employeeNr = null)
        {
            //Hämtar en anställds senaste stämplingar
            //Packar ihop enligt ett visst format
            //Stoppar varje värde i en strängarray
            //Returnerar strängarrayen som ett returvärde.
            //Returnerar strängarrayen med specialtecken (ERROR) i första post (Värde i andra post (-1), om något har gått fel och skriver till loggfilen.

            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                EmployeeManager em = new EmployeeManager(null);
                TimeStampManager tsm = new TimeStampManager(null);

                int actorCompanyId = Convert.ToInt32(GUID);
                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                    actorCompanyId = 167444;

                #endregion

                #region Perform

                Employee employeeForEmployeeNr = null;
                List<Employee> employees = new List<Employee>();
                int i = 0;

                if (employeeNr != null)
                {
                    employeeForEmployeeNr = em.GetEmployeeByNr(employeeNr, actorCompanyId);
                    if (employeeForEmployeeNr == null)
                        return new string[0];

                    employees.Add(employeeForEmployeeNr);

                    List<TimeSpotTimeCodeViewForEmployee> timecodes = tsm.GetTimeSpotTimeCodeForEmployee(employeeForEmployeeNr.ActorCompanyId);

                    foreach (var item in timecodes)
                    {
                        TsTimeCode code = new TsTimeCode();
                        code.ConvertTimeCodeFromViewForEmployee(item, 0);
                        string packedRow = code.PackString();
                        retvalList.Add(packedRow);
                        i++;
                    }
                }
                else
                {
                    List<TimeSpotTimeCodeView> timeCodes = tsm.GetTimeSpotTimeCodeForCompany(actorCompanyId);
                    foreach (var timeCode in timeCodes)
                    {
                        TsTimeCode code = new TsTimeCode();
                        code.ConvertTimeCodeFromView(timeCode, 0);
                        string packedRow = code.PackString();
                        retvalList.Add(packedRow);
                        i++;
                    }
                }

                // Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        public string[] tsGetTimeCodes(string GUID, string employeeNr = null)
        {
            //Hämtar en anställds senaste stämplingar
            //Packar ihop enligt ett visst format
            //Stoppar varje värde i en strängarray
            //Returnerar strängarrayen som ett returvärde.
            //Returnerar strängarrayen med specialtecken (ERROR) i första post (Värde i andra post (-1), om något har gått fel och skriver till loggfilen.

            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                EmployeeManager em = new EmployeeManager(null);
                TimeStampManager tsm = new TimeStampManager(null);
                int TimeTerminalId = Convert.ToInt32(GUID);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(TimeTerminalId);
                int actorCompanyId = tt.ActorCompanyId;

                #endregion

                #region Perform

                Employee employeeForEmployeeNr = null;
                List<Employee> employees = new List<Employee>();
                int i = 0;

                if (employeeNr != null)
                {
                    employeeForEmployeeNr = em.GetEmployeeByNr(employeeNr, actorCompanyId);
                    if (employeeForEmployeeNr == null)
                        return new string[0];

                    employees.Add(employeeForEmployeeNr);

                    List<TimeSpotTimeCodeViewForEmployee> timeCodes = tsm.GetTimeSpotTimeCodeForEmployee(employeeForEmployeeNr.ActorCompanyId);
                    foreach (var timeCode in timeCodes)
                    {
                        TsTimeCode code = new TsTimeCode();
                        code.ConvertTimeCodeFromViewForEmployee(timeCode, 0);
                        string packedRow = code.PackString();
                        retvalList.Add(packedRow);
                        i++;
                    }
                }
                else
                {
                    List<TimeSpotTimeCodeView> timeCodes = tsm.GetTimeSpotTimeCodeForCompany(actorCompanyId);
                    foreach (var timeCode in timeCodes)
                    {
                        TsTimeCode code = new TsTimeCode();
                        code.ConvertTimeCodeFromView(timeCode, 0);
                        string packedRow = code.PackString();
                        retvalList.Add(packedRow);
                        i++;
                    }
                }

                // Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new string[0];
            }
        }

        /// <summary>
        /// Koppla person till kortnummer
        /// </summary>
        /// <param name="GUID">Företagsidentifierare</param>
        /// <param name="employeeNr">Anställningsnummer</param>
        /// <param name="cardNumber">Kortnummer</param>
        /// <returns>Antalet påverkade rader i databasen, 1=OK</returns>
        public int SetEmployeeCardNumer(string GUID, string employeeNr, string cardNumber)
        {
            try
            {
                #region Init

                EmployeeManager em = new EmployeeManager(null);

                int actorCompanyId = Convert.ToInt32(GUID);
                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                    actorCompanyId = 167444;

                #endregion

                #region Perform

                Employee employee = em.GetEmployeeByNr(employeeNr, actorCompanyId);
                if (employee != null)
                    employee.CardNumber = cardNumber;

                if (employee == null)
                    return 0;

                ActionResult result = em.UpdateEmployee(employee, actorCompanyId);
                return result.ObjectsAffected;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return 0;
            }
        }

        public int tsSetEmployeeCardNumer(string GUID, string employeeNr, string cardNumber)
        {
            try
            {
                #region Init

                TimeStampManager tsm = new TimeStampManager(null);
                EmployeeManager em = new EmployeeManager(null);
                int ttId = Convert.ToInt32(GUID);
                TimeTerminal tt = tsm.GetTimeTerminalDiscardState(ttId);

                int actorCompanyId = tt.ActorCompanyId;

                #endregion

                #region Perform

                Employee employee = em.GetEmployeeByNr(employeeNr, actorCompanyId);
                if (employee != null)
                    employee.CardNumber = cardNumber;

                ActionResult result = em.UpdateEmployee(employee, actorCompanyId);
                return result.ObjectsAffected;

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                return 0;
            }
        }

        /// <summary>
        /// Hämtar scheman för den anställe
        /// </summary>
        /// <param name="GUID">Företagsidentifierare</param>
        /// <param name="employeeNr">Anställningsnummer</param>
        /// <returns>Schema</returns>
        public string[] GetSchedule(string GUID, string employeeNr, DateTime dateFrom, DateTime dateTo, DateTime synchTime)
        {
            //Hämtar en scheman
            //Packar ihop enligt ett visst format
            //Stoppar varje värde i en strängarray
            //Returnerar strängarrayen som ett returvärde.

            try
            {
                #region Init

                List<string> retvalList = new List<string>();
                string[] retval;

                TimeScheduleManager tsm = new TimeScheduleManager(null);
                EmployeeManager em = new EmployeeManager(null);
                CompanyManager cm = new CompanyManager(null);

                int actorCompanyId = Convert.ToInt32(GUID);
                if (GUID.ToString() == "999999" || GUID.ToString() == "999998" || GUID.ToString() == "999997" || GUID.ToString() == "TimeSpot")
                    actorCompanyId = 167444;

                Company company = cm.GetCompany(actorCompanyId);

                #endregion

                #region Perform

                List<TimeScheduleTemplateBlock> templateBlocks = new List<TimeScheduleTemplateBlock>();
                if (employeeNr.Length > 0)
                {
                    Employee employee = em.GetEmployeeByNr(employeeNr, actorCompanyId);
                    if (employee != null)
                        templateBlocks = tsm.GetEmployeeScheduleDaysforTerminal(employee.EmployeeId, dateFrom, dateTo, synchTime, actorCompanyId);

                    if (employee == null)
                        return new string[0];
                }
                else
                {
                    templateBlocks = tsm.GetEmployeeScheduleDaysforTerminal(null, dateFrom, dateTo, synchTime, actorCompanyId);
                }

                int i = 0;

                foreach (TimeScheduleTemplateBlock templateBlock in templateBlocks)
                {
                    TsTransaction tsTransaction = new TsTransaction();
                    tsTransaction.ConvertSOETimeScheduleBlock(templateBlock, company);
                    string packedRow = tsTransaction.PackStringSchedule();
                    retvalList.Add(packedRow);
                    i++;
                }

                // Kopiera svaret till sträng-array
                retval = new string[retvalList.Count];

                i = 0;
                foreach (string s in retvalList)
                {
                    retval[i] = s;
                    i++;
                }

                return retval;

                #endregion
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
                return new string[0];
            }
        }

        #endregion

        #region Logging

        private void LogError(Exception ex)
        {
            SysLogManager slm = new SysLogManager(null);
            slm.AddSysLogErrorMessage(Environment.MachineName, THREAD, ex);
        }

        #endregion
    }

    #region Help classes

    public class TsEmployee
    {
        // Databasfält
        public int id;                         // emp_id				integer			IDENTITY (1, 1)		not null,
        public int cntrl_id;                   // cntrl_id			integer	null,
        public String Company;                 // emp_company			varchar(30)		not null,
        public String Name;                    // emp_name			varchar(80)		not null,		
        public String EmpNum;                  // emp_empnum			varchar(10)		not null,
        // Närvarostatus
        // Värden är: 	
        //  0,1,2 => Ute, 3,4,5 => Inne, 
        //  10,11,12 => Frånvarokod
        //  23,24,25 => Övertidskod 
        public String Status;                  // emp_status			varchar(10)	,				-- Not 1
        public String Dep;                     // emp_dep				varchar(60)	,
        public String Password;                // emp_password			varchar(10)	,	-- Impl. i Tmon
        /// <summary>
        /// Kortnummer vid stämpling med magnedkort eller RFID
        /// </summary>
        public String CardNumber;              // emp_cardnumber			varchar(80)	,	-- Impl. i Tmon
        public int LampNum;                    // emp_lampnum			int		,	-- Impl. i Tmon
        public int LampSch;                    // emp_lampsch			int    Default 0,	-- Impl. i Tmon	
        public int Dialog;                     // emp_dialog    Dialog vid stämpling på terminal: 0 = Visa normal dialog, 1 = Visa ingen dialog, 2 = Visa dialog på begäran
        //// Finns i alla tabeller! Styr vad som skall göras av respektive system
        //// 0	=> 	Klar
        //// 1	=>	Ny post
        //// 2	=>	Ändrad post
        //// 3	=>	Borttag
        public String CostPlace;
        public int RecordStatus;               // emp_record_status		integer		 	not null,		-- Not 2 
        public DateTime ChangedDate;           // emp_changed_date		datetime	Default GetDate()	,

        public TsEmployee()
        {
        }

        public string PackString()
        {
            string ret = "#" +
                this.CardNumber.ToString() + "#" +
                this.Company.ToString() + "#" +
                this.EmpNum.ToString() + "#" +
                this.Name.ToString() + "#" +
                this.Status.ToString() + "#" +
                this.Dialog.ToString() + "#" +
                this.CostPlace.ToString() + "#"; //Automatisk stämpling: ROK 051123
            ret = ret.Replace("##", "#-#");
            ret = ret.Replace("##", "#-#");
            return ret;
        }

        public void UnpackString(string paket)
        {
            string[] arr = paket.Split(new char[] { '#' });
            int index = 0;
            this.CardNumber = arr[index++];
            this.Company = arr[index++];
            this.EmpNum = arr[index++];
            this.Name = arr[index++];
            this.Status = arr[index++];
            this.Dialog = Convert.ToInt32(arr[index++]);
            this.CostPlace = arr[index++];
        }

        public void ConvertSoeEmployee(TimeSpotEmployeeView emp, Company comp)
        {
            //Konverterar SOEEmpoyee till den typ av användare som används sedan tidigare i wt

            try
            {
                if (emp.CardNumber != null)
                {
                    this.CardNumber = emp.CardNumber;
                }
                else
                {
                    this.CardNumber = "-";
                }
            }
            catch
            {
                this.CardNumber = "-";
            }
            try
            {
                if (emp.modified != null)
                    this.ChangedDate = (DateTime)emp.modified;
                else
                    this.ChangedDate = (DateTime)emp.Created;
            }
            catch
            {
                this.ChangedDate = DateTime.Now;
            }
            try
            {
                this.Company = comp.Name;
            }
            catch
            {
                this.Company = "";
            }
            try
            {
                if (comp.TimeSpotId != null)
                    this.cntrl_id = (int)comp.TimeSpotId;
                else
                    this.cntrl_id = 0;
            }
            catch
            {
                this.cntrl_id = 0;
            }
            try
            {
                string DepartmentName = "";
                this.Dep = DepartmentName;
            }
            catch
            {
                this.Dep = "";
            }
            try
            {
                this.EmpNum = emp.EmployeeNr.ToString();
            }
            catch
            {
                this.EmpNum = "";
            }
            try
            {
                this.id = 0;
            }
            catch
            {
                this.id = 0;
            }
            try
            {
                this.LampNum = 0;
            }
            catch
            {
                this.LampNum = -1;
            }
            try
            {
                this.LampSch = 0;
            }
            catch
            {
                this.LampSch = -1;
            }
            try
            {
                this.Dialog = 0; //Titta på senare
            }
            catch
            {
                this.Dialog = 0;
            }
            try
            {
                this.Name = emp.Name;
            }
            catch
            {
                this.Name = "";
            }
            try
            {
                this.Password = ""; //But why?
            }
            catch
            {
                this.Password = "";
            }
            try
            {
                this.RecordStatus = 0;
            }
            catch
            {
                this.RecordStatus = 0;
            }
            try
            {
                this.Status = "0";
            }
            catch
            {
                this.Status = "";
            }
            try
            {
                this.CostPlace = "";
            }
            catch
            {
                this.CostPlace = "";
            }
        }

        public void tsConvertSoeEmployee(Employee emp, Company comp)
        {
            //Konverterar SOEEmpoyee till den typ av användare som används sedan tidigare i wt

            try
            {
                if (emp.CardNumber != null)
                {
                    this.CardNumber = emp.CardNumber;
                }
                else
                {
                    this.CardNumber = "-";
                }
            }
            catch
            {
                this.CardNumber = "-";
            }
            try
            {
                if (emp.Modified != null)
                    this.ChangedDate = (DateTime)emp.Modified;
                else
                    this.ChangedDate = (DateTime)emp.Created;
            }
            catch
            {
                this.ChangedDate = DateTime.Now;
            }
            try
            {
                this.Company = comp.Name;
            }
            catch
            {
                this.Company = "";
            }
            try
            {
                if (comp.TimeSpotId != null)
                    this.cntrl_id = (int)comp.TimeSpotId;
                else
                    this.cntrl_id = 0;
            }
            catch
            {
                this.cntrl_id = 0;
            }
            try
            {
                string DepartmentName = "";
                this.Dep = DepartmentName;
            }
            catch
            {
                this.Dep = "";
            }
            try
            {
                this.EmpNum = emp.EmployeeNr.ToString();
            }
            catch
            {
                this.EmpNum = "";
            }
            try
            {
                this.id = 0;
            }
            catch
            {
                this.id = 0;
            }
            try
            {
                this.LampNum = 0;
            }
            catch
            {
                this.LampNum = -1;
            }
            try
            {
                this.LampSch = 0;
            }
            catch
            {
                this.LampSch = -1;
            }
            try
            {
                this.Dialog = 0; //Titta på senare
            }
            catch
            {
                this.Dialog = 0;
            }
            try
            {
                this.Name = emp.Name;
            }
            catch
            {
                this.Name = "";
            }
            try
            {
                this.Password = ""; //But why?
            }
            catch
            {
                this.Password = "";
            }
            try
            {
                this.RecordStatus = 0;
            }
            catch
            {
                this.RecordStatus = 0;
            }
            try
            {
                this.Status = "0";
            }
            catch
            {
                this.Status = "";
            }
            try
            {
                this.CostPlace = "";
            }
            catch
            {
                this.CostPlace = "";
            }
        }
    }

    public class TsTransaction
    {
        /// <summary>
        /// Databas-id
        /// tr_id integer IDENTITY(1, 1) not null,
        /// </summary>
        public int Id = 0;
        /// <summary>
        /// kontroll-id 
        /// cntrl_id integer null
        /// </summary>
        public int CntrlId = 0;
        /// <summary>
        /// Anställningsnummer 
        /// tr_empnum varchar(10) not null,
        /// </summary>
        public String EmpNum = "";
        /// <summary>
        /// Transaktionstyp
        ///  A	=> 	IN
        ///  B	=>	UT
        /// tr_type				varchar(10)						not null,	-- Not 6
        /// </summary>
        public String Type = "";
        /// <summary>
        /// tr_timecode			varchar(10)						not null,	-- '0'	Inget						
        /// Timecode
        /// </summary>
        public String TimeCode;
        /// <summary>
        /// Datum och tid för transaktionen
        /// tr_date				Datetime						not null,
        /// </summary>
        public DateTime Date;
        /// <summary>
        /// Status: 0=>Klar, 1=>Ny post, 2=>Ändrad post, 3=>Borttag
        /// tr_record_status	integer							not null,
        /// </summary>
        public int RecordStatus = -1;
        /// <summary>
        /// Datumstämpel i databasen 
        /// tr_changed_date		Datetime	Default getdate(),
        /// </summary>
        public DateTime ChangedDate;
        /// <summary>
        /// Id för stämplande maskin
        /// </summary>
        public String MachineId = "0"; //DONE ROK 051122: Default MachineId "0"
        /// <summary>
        /// Stämplat kostnadsställe på transaktionen
        /// </summary>
        public String CostPlace;

        public String AccountName;

        public String AbsenceName;

        public String StartTime;

        public String StopTime;

        public String ScheduleDate;

        /// <summary>
        /// Packa upp packad sträng
        /// </summary>
        /// <param name="paket">Packad sträng</param>
        public void UnpackString(string paket)
        {
            string[] arr = paket.Split(new char[] { '#' });
            int index = 0;

            this.EmpNum = arr[index++];
            this.Type = arr[index++];
            this.TimeCode = arr[index++];
            try
            {
                CultureInfo MyCultureInfo = new CultureInfo("sv-SE");
                this.Date = DateTime.ParseExact(arr[index++], "yyyy-MM-dd HH:mm", MyCultureInfo);
            }
            catch
            {
                throw new Exception("Ogiltigt datumformat för transaktion: " + arr[index - 1]);
            }
            try
            {
                this.MachineId = arr[index++];
            }
            catch
            {
                this.MachineId = "0";
            }
            try
            {
                this.CostPlace = arr[index++];
            }
            catch
            {
                this.CostPlace = "";
            }
        }

        public string PackString()
        {
            string ret =
                this.EmpNum.ToString() + " #" +
                this.Type.ToString() + " #" +
                this.TimeCode.ToString() + " #" +
                this.Date.ToString("yyyy-MM-dd HH:mm") + " #" +
                this.MachineId.ToString() + " #" +
                this.CostPlace.ToString() + " #" +
                this.AbsenceName.ToString() + " #" +
                this.AccountName.ToString() + " #"
                ;
            return ret;
        }

        //Packa ihop schemasträng för utskick till terminal

        public string PackStringSchedule()
        {
            string ret =
                this.EmpNum.ToString() + " #" +
                this.Type.ToString() + " #" +
                this.StartTime.ToString() + " #" +
                this.StopTime.ToString() + " #" +
                this.ScheduleDate.ToString() + " #";
            return ret;
        }
        public void ConvertSOETimeStampEntry(TimeSpotTimeStampView timeStampEntry)
        {
            try
            {
                this.Id = timeStampEntry.Id;
            }
            catch
            {
                this.Id = 0;
            }
            try
            {
                this.CntrlId = timeStampEntry.ActorCompanyId;
            }
            catch
            {
                this.Id = 0;
            }
            try
            {
                this.EmpNum = timeStampEntry.EmployeeNr.ToString();
            }
            catch
            {
                this.EmpNum = "";
            }
            try
            {
                if (timeStampEntry.Type == (int)TimeStampEntryType.In)
                    this.Type = "A";
                else if (timeStampEntry.Type == (int)TimeStampEntryType.Out)
                    this.Type = "B";
                else
                    this.Type = "C";
            }
            catch
            {
                this.Type = "";
            }
            try
            {

                this.TimeCode = timeStampEntry.TimeDeviationCauseId.ToString();
                if (timeStampEntry.TimeDeviationCauseId == null)
                    this.TimeCode = "";
            }
            catch (Exception)
            {
                this.TimeCode = "";
            }
            try
            {
                this.Date = timeStampEntry.Time;
            }
            catch
            {
                this.Date = new DateTime();
            }
            try
            {
                this.RecordStatus = 0;
            }
            catch
            {
                this.RecordStatus = -1;
            }
            try
            {
                this.ChangedDate = timeStampEntry.Time;
            }
            catch
            {
                this.ChangedDate = new DateTime();
            }
            try
            {
                this.MachineId = timeStampEntry.TimeTerminalId.ToString();
            }
            catch (Exception)
            {
                this.MachineId = "";
            }
            try
            {

                this.CostPlace = timeStampEntry.AccountNr;
                if (timeStampEntry.AccountNr == null)
                    this.CostPlace = "";
            }
            catch (Exception)
            {
                this.CostPlace = "";
            }
            try
            {

                this.AbsenceName = timeStampEntry.TimeDeviationCauseName;
                if (timeStampEntry.TimeDeviationCauseName == null)
                    this.AbsenceName = "";
            }
            catch (Exception)
            {
                this.AbsenceName = "";
            }
            try
            {

                this.AccountName = timeStampEntry.AccountName;
                if (timeStampEntry.AccountName == null)
                    this.AccountName = "";
            }
            catch (Exception)
            {
                this.AccountName = "";
            }
        }

        public void ConvertSOETimeScheduleBlock(TimeScheduleTemplateBlock tstb, Company c)
        {

            {
                this.Id = 0;
            }
            try
            {
                if (tstb.Employee != null)
                    this.EmpNum = tstb.Employee.EmployeeNr.ToString();
            }
            catch
            {
                this.EmpNum = "";
            }
            try
            {
                this.Type = "Schema";
            }
            catch
            {
                this.Type = "";
            }
            try
            {
                this.StartTime = tstb.StartTime.TimeOfDay.ToString();
            }
            catch
            {
                this.StartTime = "";
            }
            try
            {

                this.StopTime = tstb.StopTime.TimeOfDay.ToString();
            }
            catch
            {
                this.StopTime = "";
            }

            try
            {
                if (tstb.Date != null)
                    this.ScheduleDate = tstb.Date.ToString();

            }
            catch
            {
                this.ScheduleDate = '0'.ToString();
            }
        }


        public override bool Equals(object obj)
        {
            TsTransaction emp2 = (TsTransaction)obj;
            if (this.Date != emp2.Date)
                return (false);
            if (this.EmpNum != emp2.EmpNum)
                return (false);
            if (this.TimeCode != emp2.TimeCode)
                return (false);
            if (this.Type != emp2.Type)
                return (false);
            if (this.MachineId != emp2.MachineId)
                return (false);
            if (this.CostPlace != emp2.CostPlace)
                return (false);
            return (true);
        }
        public override Int32 GetHashCode()
        {
            return this.Id;
        }

    }

    public class TsAcc
    {
        public TsAcc()
        {
        }

        // acc_id integer    IDENTITY (1, 1)		not null,
        public int Id = 0;
        // cntrl_id integer	null,
        public int CntrlId = 0;
        // acc_empnum	    varchar(10)			not null,
        public String Empnum = "";
        // acc_row	        integer				not null, -- Vilken rad på displayen
        public int Row = 0;
        //acc_name	        varchar(30)			not null,
        public String name = "";
        //acc_period_name	varchar(30),
        public String PeriodName = "";
        //acc_period_value	float ,
        public double PeriodValue = 0.0;
        //acc_year_value	float ,
        public double YearValue = 0.0;
        //acc_record_status	integer ,
        public int RecordStatus = 0;
        //acc_changed_date	Datetime 	        Default getdate(),
        public DateTime ChangedDate;

        public void ConvertSOETimeAccumulator(TSTimeAccumulatorEmployeeItem accumulator, int timeSpotId, string employeeNr, int rowNr)
        {
            try
            {
                this.Id = accumulator.TimeAccumulatorId;
            }
            catch
            {
                this.Id = 0;
            }
            try
            {
                this.CntrlId = timeSpotId;
            }
            catch
            {
                this.CntrlId = 0;
            }
            try
            {
                this.Empnum = employeeNr;
            }
            catch
            {
                this.Empnum = "";
            }
            try
            {
                this.Row = rowNr;
            }
            catch
            {
                this.Row = 0;
            }
            try
            {
                this.name = accumulator.Name;
            }
            catch
            {
                this.name = "";
            }
            try
            {
                this.PeriodName = accumulator.TimeAccumulatorId > 0 ? "Period: " : " ";//(string)reader["acc_period_name"];
            }
            catch
            {
                this.PeriodName = "";
            }
            try
            {
                this.PeriodValue = Convert.ToDouble(accumulator.SumPeriod);// (double)reader["acc_period_value"];
            }
            catch
            {
                this.PeriodValue = 0.0;
            }
            try
            {
                this.YearValue = Convert.ToDouble(accumulator.SumAccToday); // (double)reader["acc_year_value"];
            }
            catch
            {
                this.YearValue = 0.0;
            }
            try
            {
                this.RecordStatus = 0; // (int)reader["acc_record_status"];
            }
            catch
            {
                this.RecordStatus = 0;
            }
            try
            {
                this.ChangedDate = accumulator.SyncDate;
            }
            catch
            {
                this.ChangedDate = new DateTime();
            }
        }
        public string PackString()
        {
            string ret = "";
            ret =
                this.Empnum.ToString() + "#" +
                this.Row.ToString() + "#" +
                this.name.ToString() + " " +
                this.PeriodName.ToString() + " " +
                CalendarUtility.FormatMinutes(Convert.ToInt32(this.PeriodValue)) + " " +
                (String.IsNullOrEmpty(this.PeriodName.Trim()) ? " " : "År: " + CalendarUtility.FormatMinutes(Convert.ToInt32(this.YearValue))) + "#" +
                " " + "#" +
                " " + "#" +
                " ";
            return ret;
        }

    }

    public class TsEmpList
    {
        public TsEmpList()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        private string EmpNum;


        public void ConvertSoeTimeDeviationCause(Employee emp)
        {
            try
            {
                this.EmpNum = emp.EmployeeNr.ToString();
            }
            catch
            {
                this.EmpNum = "";
            }
        }

        public string PackString(string empNum)
        {
            if (empNum == null)
                empNum = "";
            return empNum.ToString() + "#";
        }
    }

    public class TsTimeCode
    {
        public TsTimeCode()
        {
        }

        #region Variables

        public int Id = 0;                 //tc_id				integer		IDENTITY (1, 1)		not null,
        public String Code = "";            //tc_code				varchar(10)				not null,
        public String Desc = "";            //tc_desc				varchar(60),
        // ( tc_type)
        // Talar om vad det är för typ av tidkod
        // 0	=>	Stämplingsfunktion
        // 1	=>	Frånvarolöneart
        public int Type = -1;               //tc_type				integer,				--	Not 4	
        public int Sort = 0;
        public String EmpNum = "";
        public DateTime ChangedDate;   //tc_changed_date		datetime	default GetDate(),

        #endregion

        public void ConvertSoeTimeDeviationCause(TimeDeviationCause timeDeviationCause, int sort)
        {
            try
            {
                this.Id = timeDeviationCause.TimeDeviationCauseId;
            }
            catch
            {
                this.Id = 0;
            }
            try
            {
                this.Code = timeDeviationCause.TimeDeviationCauseId.ToString();
            }
            catch
            {
                this.Code = "";
            }
            try
            {
                this.Desc = timeDeviationCause.Name;
            }
            catch
            {
                this.Desc = "";
            }
            try
            {
                int type = 0;

                if (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                    type = 1;
                else if (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Presence || timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence)
                    type = 0;
                else
                    type = 2;

                this.Type = type;
            }
            catch
            {
                this.Type = -1;
            }
            try
            {
                this.Sort = this.Type;// (int)reader["tc_order"];
            }
            catch
            {
                this.Sort = -1;
            }
            try
            {
                DateTime SoeChangeDate;

                if (timeDeviationCause.Modified.HasValue)
                    SoeChangeDate = (DateTime)timeDeviationCause.Modified;
                else
                    SoeChangeDate = (DateTime)timeDeviationCause.Created;

                this.ChangedDate = SoeChangeDate;
            }
            catch
            {
                this.ChangedDate = new DateTime();
            }
            try
            {
                this.EmpNum = "";//emp.EmployeeNr.ToString();
            }
            catch
            {
                this.EmpNum = "";
            }
        }

        public void ConvertTimeCodeFromView(TimeSpotTimeCodeView timeSpotTimeCode, int sort)
        {
            try
            {
                this.Id = timeSpotTimeCode.id;
            }
            catch
            {
                this.Id = 0;
            }
            try
            {
                this.Code = timeSpotTimeCode.id.ToString();
            }
            catch
            {
                this.Code = "";
            }
            try
            {
                this.Desc = timeSpotTimeCode.Name;
            }
            catch
            {
                this.Desc = "";
            }
            try
            {
                this.Type = Convert.ToInt32(timeSpotTimeCode.Type);
            }
            catch
            {
                this.Type = -1;
            }
            try
            {
                if (this.Type != 2)
                {
                    this.Sort = this.Type;// (int)reader["tc_order"];
                }
                else
                {
                    this.Sort = 0;
                }
            }
            catch
            {
                this.Sort = -1;
            }
            try
            {
                DateTime SoeChangeDate;

                if (timeSpotTimeCode.Modified.HasValue)
                    SoeChangeDate = (DateTime)timeSpotTimeCode.Modified;
                else
                    SoeChangeDate = (DateTime)timeSpotTimeCode.Created;

                this.ChangedDate = SoeChangeDate;
            }
            catch
            {
                this.ChangedDate = new DateTime();
            }
            try
            {
                this.EmpNum = "";//emp.EmployeeNr.ToString();
            }
            catch
            {
                this.EmpNum = "";
            }
        }

        public void ConvertTimeCodeFromViewForEmployee(TimeSpotTimeCodeViewForEmployee timeSpotTimeCode, int sort)
        {
            try
            {
                this.Id = timeSpotTimeCode.id;
            }
            catch
            {
                this.Id = 0;
            }
            try
            {
                this.Code = timeSpotTimeCode.id.ToString();
            }
            catch
            {
                this.Code = "";
            }
            try
            {
                this.Desc = timeSpotTimeCode.Name;
            }
            catch
            {
                this.Desc = "";
            }
            try
            {
                this.Type = Convert.ToInt32(timeSpotTimeCode.Type);
            }
            catch
            {
                this.Type = -1;
            }
            try
            {
                if (this.Type != 2)
                {
                    this.Sort = this.Type;// (int)reader["tc_order"];
                }
                else
                {
                    this.Sort = 0;
                }
            }
            catch
            {
                this.Sort = -1;
            }
            try
            {
                DateTime SoeChangeDate;

                if (timeSpotTimeCode.Modified.HasValue)
                    SoeChangeDate = (DateTime)timeSpotTimeCode.Modified;
                else
                    SoeChangeDate = (DateTime)timeSpotTimeCode.Created;

                this.ChangedDate = SoeChangeDate;
            }
            catch
            {
                this.ChangedDate = new DateTime();
            }
            try
            {
                this.EmpNum = "";//emp.EmployeeNr.ToString();
            }
            catch
            {
                this.EmpNum = "";
            }
        }




        public void ConvertSoeAccount(Account account, Employee employee, int sort)
        {
            try
            {
                this.Id = account.AccountId;
            }
            catch
            {
                this.Id = 0;
            }
            try
            {
                this.Code = account.AccountId.ToString();
            }
            catch
            {
                this.Code = "";
            }
            try
            {
                this.Desc = account.Name;
            }
            catch
            {
                this.Desc = "";
            }
            try
            {
                int type = 2;

                this.Type = type;
            }
            catch
            {
                this.Type = -1;
            }
            try
            {
                this.Sort = sort;// (int)reader["tc_order"];
            }
            catch
            {
                this.Sort = -1;
            }
            try
            {
                DateTime SoeChangeDate;

                if (account.Modified.HasValue)
                    SoeChangeDate = (DateTime)account.Modified;
                else
                    SoeChangeDate = (DateTime)account.Created;

                this.ChangedDate = SoeChangeDate;
            }
            catch
            {
                this.ChangedDate = new DateTime();
            }
            try
            {
                this.EmpNum = employee.EmployeeNr.ToString();
            }
            catch
            {
                this.EmpNum = "";
            }
        }

        public string PackString()
        {
            string ret =
                this.Code.ToString() + "#" +
                this.Desc.ToString() + "#" +
                this.Type.ToString() + "#" +
                this.Sort.ToString() + "#" +
                this.EmpNum.ToString() + "#"
                ;
            return ret;
        }

    }

    #endregion
}
