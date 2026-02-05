using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.ImportSpecials.Interfaces;
using SoftOne.Soe.Business.Util.ImportSpecials.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class PaXml2 : IPayrollImportable
    {
        paxml _model;
        byte[] _file;

        public PaXml2(byte[] file)
        {
            _file = file;
            var path = $@"{ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL}\{Guid.NewGuid()}";
            try
            {
                File.WriteAllBytes(path, file);
                string xmlString = File.ReadAllText(path);
                var serializer = new XmlSerializer(typeof(paxml));

                using (TextReader reader = new StringReader(xmlString))
                    _model = (paxml)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                LogCollector.LogCollector.LogError(ex.ToString());
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }

        }
        public List<string> GetEmployeeNrs()
        {
            List<string> employeeNrs = new List<string>();
            if (_model?.lonetransaktioner?.lonetrans != null)
            {
                employeeNrs.AddRange(_model.lonetransaktioner.lonetrans.Select(s => s.anstid));
            }

            if (_model?.tidtransaktioner?.tidtrans != null)
            {
                employeeNrs.AddRange(_model.tidtransaktioner.tidtrans.Select(s => s.anstid));
            }

            return employeeNrs.Distinct().ToList();
        }
        public PayrollImportHeadDTO ParseToPayrollImportHead(int actorCompanyId, byte[] file, DateTime? paymentDate, List<EmployeeDTO> employees, List<AccountDimDTO> accountDims, List<PayrollProductGridDTO> payrollProducts, List<TimeDeviationCauseDTO> timeDeviationCauses)
        {
            if (_model == null)
                return null;

            this._file = file;

            PayrollImportHeadDTO payrollImportHead = new PayrollImportHeadDTO()
            {
                ActorCompanyId = actorCompanyId,
                File = _file,
                PaymentDate = paymentDate,
                DateFrom = _model.header.datum,
                DateTo = _model.header.datum,
            };

            var employeeNrs = GetEmployeeNrs();

            if (_model.lonetransaktioner?.lonetrans != null)
            {
                var transactionGroupOnEmployee = _model.lonetransaktioner.lonetrans.GroupBy(g => g.anstid);

                foreach (var employeeGroup in transactionGroupOnEmployee)
                {
                    PayrollImportEmployeeDTO payrollImportEmployee = new PayrollImportEmployeeDTO();
                    var employee = employees.FirstOrDefault(f => f.EmployeeNr == employeeGroup.Key);

                    if (employee == null)
                        continue;

                    payrollImportHead.Employees.Add(payrollImportEmployee);       
                    payrollImportEmployee.EmployeeId = employee.EmployeeId;
                    if (payrollImportHead.DateFrom == payrollImportHead.DateTo && payrollImportHead.DateTo == _model.header.datum && _model.schematransaktioner != null) 
                    {
                        payrollImportHead.DateFrom = _model.schematransaktioner.schema.Where(w => w.anstid == employee.EmployeeNr).OrderBy(o => o.dag.OrderBy(oo => oo.datum).FirstOrDefault()?.datum).SelectMany(s => s.dag.Select(ss => ss.datum)).FirstOrDefault();
                        payrollImportHead.DateTo = _model.schematransaktioner.schema.Where(w => w.anstid == employee.EmployeeNr).OrderBy(o => o.dag.OrderBy(oo => oo.datum).LastOrDefault()?.datum).SelectMany(s => s.dag.Select(ss => ss.datum)).LastOrDefault();
                    }
                    foreach (var trans in employeeGroup)
                    {
                        var transaction = new PayrollImportEmployeeTransactionDTO();
                        TermGroup_PayrollResultType payrollResultType = TermGroup_PayrollResultType.Time;

                        if (!string.IsNullOrEmpty(trans.lonart))
                        {
                            var deviation = timeDeviationCauses.FirstOrDefault(w => !string.IsNullOrEmpty(w.ExtCode) && w.ExtCode.ToLower() == trans.lonart.ToLower());

                            if (deviation == null)
                            {
                                foreach (var item in timeDeviationCauses.Where(w => !string.IsNullOrEmpty(w.ExtCode) && w.ExtCode.Contains("#")))
                                {
                                    var match = item.ExternalCodes.FirstOrDefault(w => !string.IsNullOrEmpty(w) && w.ToLower() == trans.lonart.ToLower());

                                    if (!string.IsNullOrWhiteSpace(match))
                                    {
                                        deviation = item;
                                        break;
                                    }
                                }
                            }

                            if (deviation != null)
                            {
                                transaction.TimeDeviationCauseId = deviation.TimeDeviationCauseId;
                                transaction.Type = TermGroup_PayrollImportEmployeeTransactionType.DeviationCause;
                            }

                            var payrollProduct = payrollProducts.FirstOrDefault(f => f.ExternalNumber == trans.lonart);

                            if (payrollProduct == null)
                                payrollProduct = payrollProducts.FirstOrDefault(f => f.Number == trans.lonart);

                            if (payrollProduct != null)
                            {
                                transaction.PayrollProductId = payrollProduct.ProductId;
                                transaction.Type = TermGroup_PayrollImportEmployeeTransactionType.PayrollProduct;
                                payrollResultType = payrollProduct.ResultType;
                            }

                            if (deviation != null && payrollProduct != null)
                            {
                                payrollImportHead.ErrorMessage = trans.lonart + " Exists as both payrollproduct and timedeviationcause, adjust in file and try again";
                            }

                        }
                        else
                            continue;

                        transaction.Date = trans.datum != DateTime.MinValue ? trans.datum : payrollImportHead.DateTo.Date; // trans.datum 
                        transaction.Quantity = payrollResultType == TermGroup_PayrollResultType.Time || transaction.TimeDeviationCauseId.HasValue ? decimal.Multiply(trans.antal, 60) : trans.antal;
                        transaction.Amount = trans.belopp;
                        transaction.Code = trans.lonart;
                        transaction.AccountCode = trans.kontonr;
                        transaction.AccountStdId = GetAccountFromAccountNr(trans.kontonr, accountDims, isSTD: true)?.AccountId ?? null;
                        transaction.AccountInternals = GetAccounts(accountDims, trans.resenheter);
                        if (transaction.Amount > 0 && transaction.Quantity == 0)
                            transaction.Quantity = 1;

                        payrollImportEmployee.Transactions.Add(transaction);
                    }
                }
            }

            if (_model.tidtransaktioner?.tidtrans != null)
            {
                var transactionGroupOnEmployee = _model.tidtransaktioner.tidtrans.GroupBy(g => g.anstid);

                foreach (var employeeGroup in transactionGroupOnEmployee)
                {

                    var employee = employees.FirstOrDefault(f => f.EmployeeNr == employeeGroup.Key);

                    if (employee == null)
                        continue;

                    PayrollImportEmployeeDTO payrollImportEmployee = payrollImportHead.Employees.FirstOrDefault(f => f.EmployeeId == employee.EmployeeId);

                    if (payrollImportEmployee == null)
                    {
                        payrollImportEmployee = new PayrollImportEmployeeDTO();
                        payrollImportEmployee.EmployeeId = employee.EmployeeId;
                        payrollImportHead.Employees.Add(payrollImportEmployee);
                    }

                    foreach (var trans in employeeGroup)
                    {
                        var transaction = new PayrollImportEmployeeTransactionDTO();
                        TermGroup_PayrollResultType payrollResultType = TermGroup_PayrollResultType.Time;
                        var tidkod = trans.tidkod.ToString();
                        if (!string.IsNullOrEmpty(tidkod))
                        {
                            var deviation = timeDeviationCauses.FirstOrDefault(w => !string.IsNullOrEmpty(w.ExtCode) && w.ExtCode.ToLower() == tidkod.ToLower());

                            if (deviation == null)
                            {
                                foreach (var item in timeDeviationCauses.Where(w => !string.IsNullOrEmpty(w.ExtCode) && w.ExtCode.Contains("#")))
                                {
                                    var match = item.ExternalCodes.FirstOrDefault(w => !string.IsNullOrEmpty(w) && w.ToLower() == tidkod.ToLower());

                                    if (!string.IsNullOrWhiteSpace(match))
                                    {
                                        deviation = item;
                                        break;
                                    }
                                }
                            }

                            if (deviation != null)
                            {
                                transaction.TimeDeviationCauseId = deviation.TimeDeviationCauseId;
                                transaction.Type = TermGroup_PayrollImportEmployeeTransactionType.DeviationCause;
                            }

                            var payrollProduct = payrollProducts.FirstOrDefault(f => f.ExternalNumber == trans.tidkod.ToString());

                            if (payrollProduct == null)
                                payrollProduct = payrollProducts.FirstOrDefault(f => f.Number == tidkod);

                            if (payrollProduct != null)
                            {
                                transaction.PayrollProductId = payrollProduct.ProductId;
                                transaction.Type = TermGroup_PayrollImportEmployeeTransactionType.PayrollProduct;
                                payrollResultType = payrollProduct.ResultType;
                            }

                            if (deviation != null && payrollProduct != null)
                            {
                                payrollImportHead.ErrorMessage = trans.tidkod + " Exists as both payrollproduct and timedeviationcause, adjust in file and try again";
                            }

                        }
                        else
                            continue;

                        transaction.Date = trans.datum;
                        transaction.Quantity = payrollResultType == TermGroup_PayrollResultType.Time || transaction.TimeDeviationCauseId.HasValue ? decimal.Multiply(trans.timmar, 60) : trans.timmar;
                        transaction.Amount = 0;
                        transaction.Code = tidkod;
                        transaction.AccountCode = trans.kontonr;
                        transaction.AccountStdId = GetAccountFromAccountNr(trans.kontonr, accountDims, isSTD: true)?.AccountId ?? null;
                        transaction.AccountInternals = GetAccounts(accountDims, trans.resenheter);
                        payrollImportEmployee.Transactions.Add(transaction);
                    }
                }
            }

            if (_model.schematransaktioner?.schema != null)
            {
                var transactionGroupOnEmployee = _model.schematransaktioner.schema.GroupBy(g => g.anstid);

                foreach (var employeeGroup in transactionGroupOnEmployee)
                {

                    var employee = employees.FirstOrDefault(f => f.EmployeeNr == employeeGroup.Key);

                    if (employee == null)
                        continue;

                    PayrollImportEmployeeDTO payrollImportEmployee = payrollImportHead.Employees.FirstOrDefault(f => f.EmployeeId == employee.EmployeeId);

                    if (payrollImportEmployee == null)
                    {
                        payrollImportEmployee = new PayrollImportEmployeeDTO();
                        payrollImportEmployee.EmployeeId = employee.EmployeeId;
                        payrollImportHead.Employees.Add(payrollImportEmployee);
                    }

                    foreach (var trans in employeeGroup)
                    {

                        payrollImportEmployee.Schedule = new List<PayrollImportEmployeeScheduleDTO>();

                        foreach (var schedule in trans.dag)
                        {
                            var payrollImportEmployeeSchedule = new PayrollImportEmployeeScheduleDTO()
                            {
                                Date = schedule.datum,
                                Quantity = decimal.Multiply(schedule.timmar, 60),
                                StartTime = GetTime(schedule.starttid),
                                StopTime = GetTime(schedule.sluttid),
                            };

                            if (payrollImportEmployeeSchedule.StartTime == payrollImportEmployeeSchedule.StopTime)
                                payrollImportEmployeeSchedule.StopTime = payrollImportEmployeeSchedule.StartTime.AddHours(Convert.ToDouble(schedule.timmar));

                            payrollImportEmployee.Schedule.Add(payrollImportEmployeeSchedule);
                            var breakTime = Convert.ToDecimal((payrollImportEmployeeSchedule.StopTime - payrollImportEmployeeSchedule.StartTime).TotalMinutes) - decimal.Multiply(payrollImportEmployeeSchedule.Quantity, 60) ;

                            if (breakTime > 0)
                            {
                                var payrollImportEmployeeScheduleBreak = new PayrollImportEmployeeScheduleDTO()
                                {
                                    Date = schedule.datum,
                                    Quantity = breakTime,
                                    StartTime = CalendarUtility.AdjustAccordingToInterval(CalendarUtility.GetMiddleTime(payrollImportEmployeeSchedule.StopTime, payrollImportEmployeeSchedule.StopTime).AddMinutes(-Convert.ToInt32(decimal.Divide(breakTime, 2))), Convert.ToInt32(breakTime), 15),
                                    IsBreak = true,
                                };

                                payrollImportEmployeeScheduleBreak.StopTime = payrollImportEmployeeScheduleBreak.StartTime.AddMinutes(Convert.ToInt32(breakTime));
                                payrollImportEmployee.Schedule.Add(payrollImportEmployeeScheduleBreak);
                            }

                        }
                    }
                }
            }



            return payrollImportHead;
        }

        private DateTime GetTime(DateTime time)
        {
            return CalendarUtility.DATETIME_DEFAULT.AddHours(time.Hour != 0 ? time.Hour : 8).AddMinutes(time.Minute);
        }

        private List<PayrollImportEmployeeTransactionAccountInternalDTO> GetAccounts(List<AccountDimDTO> accountDims, resenheterTYPE resenheter)
        {
            List<PayrollImportEmployeeTransactionAccountInternalDTO> payrollImportEmployeeTransactionAccountInternal = new List<PayrollImportEmployeeTransactionAccountInternalDTO>();

            if (resenheter?.resenhet == null)
                return payrollImportEmployeeTransactionAccountInternal;

            List<AccountDTO> accounts = new List<AccountDTO>();
            foreach (var item in resenheter.resenhet)
            {
                var account = GetAccountFromAccountNr(item.id, accountDims, (TermGroup_SieAccountDim)item.dim);

                if (account != null)
                    accounts.Add(account);
            }

            foreach (var acc in accounts)
            {
                var dim = accountDims.FirstOrDefault(f => f.AccountDimId == acc.AccountDimId);
                payrollImportEmployeeTransactionAccountInternal.Add(new PayrollImportEmployeeTransactionAccountInternalDTO()
                {
                    AccountSIEDimNr = dim?.SysSieDimNr ?? 0,
                    AccountCode = acc.AccountNr,
                    AccountId = acc.AccountId
                });
            }


            return payrollImportEmployeeTransactionAccountInternal;
        }

        private AccountDTO GetAccountFromAccountNr(string accountNr, List<AccountDimDTO> dims, TermGroup_SieAccountDim sieAccountDim = TermGroup_SieAccountDim.CostCentre, bool isSTD = false)
        {

            try
            {
                if (string.IsNullOrEmpty(accountNr))
                    return null;

                AccountDimDTO dim = null;

                dim = dims.FirstOrDefault(f => f.SysSieDimNr == (int)sieAccountDim);
                if (dim == null)
                    return null;

                if (isSTD)
                {
                    dim = null;
                    dim = dims.FirstOrDefault(f => f.AccountDimNr == Constants.ACCOUNTDIM_STANDARD);
                    if (dim == null)
                        return null;
                }

                return dim.Accounts?.FirstOrDefault(a => a.AccountNr == accountNr || (!string.IsNullOrEmpty(a.ExternalCode) && a.ExternalCode.Equals(accountNr, StringComparison.OrdinalIgnoreCase)));
            }
            catch
            {
                return null;
            }
        }
    }
}
