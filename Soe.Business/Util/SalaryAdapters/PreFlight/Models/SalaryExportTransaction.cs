using Microsoft.Owin.Security.Provider;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ZXing.OneD;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models
{
    public class SalaryExportTransaction
    {
        public SalaryExportTransaction() { }

        public SalaryExportTransaction(List<SalaryExportTransaction> children)
        {
            Children = children;
        }

        public string EmployeeCode { get; set; }
        public int? EmployeeChildId { get; set; }
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal Hours { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public SalaryExportCostAllocation CostAllocation { get; set; }
        public bool IsAbsence { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public SalaryExportTransactionDateMergeType DateMergeType { get; set; }
        public List<SalaryExportTransaction> Children { get; set; } = new List<SalaryExportTransaction>();
        public bool OfFullDayAbsence { get; set; }
        public bool IsRegistrationQuantity { get; set; }
        public bool IsPaidVacation { get; set; }
        public string ExternalCode { get; set; }

        public override string ToString()
        {
            return $"EmployeeCode {EmployeeCode} , Code: {Code}, Date: {Date}, FromDate: {FromDate}, ToDate: {ToDate}, Hours: {Hours}, Amount: {Amount}, CostAllocation: {CostAllocation}, IsAbsence: {IsAbsence}, Guid: {Guid}, OfFullDayAbsence: {OfFullDayAbsence}, IsRegistrationQuantity: {IsRegistrationQuantity}, IsPaidVacation: {IsPaidVacation}, ExternalCode: {ExternalCode}";
        }


        public string AllocateCost(SalaryExportTransactionGroupType salaryExportTransactionGroupType, bool useCostCenter, bool useProject, bool useDepartment)
        {
            if (salaryExportTransactionGroupType != SalaryExportTransactionGroupType.ByCostAllocation || CostAllocation == null)
                return "";

            string groupCode = "";

            if (useCostCenter) groupCode += CostAllocation.Costcenter;
            if (useProject) groupCode += CostAllocation.Project;
            if (useDepartment) groupCode += CostAllocation.Department;
            return salaryExportTransactionGroupType == SalaryExportTransactionGroupType.ByCostAllocation ? groupCode : "";
        }

        public SalaryExportTransaction Clone()
        {
            return new SalaryExportTransaction()
            {
                EmployeeCode = EmployeeCode,
                Code = Code,
                Date = Date,
                FromDate = FromDate,
                ToDate = ToDate,
                Hours = Hours,
                Amount = Amount,
                Quantity = Quantity,
                CostAllocation = CostAllocation.Clone(),
                Guid = Guid,
                IsAbsence = IsAbsence,
                DateMergeType = DateMergeType,
                Children = Children?.Select(s => s.Clone()).ToList() ?? new List<SalaryExportTransaction>(),
                IsRegistrationQuantity = IsRegistrationQuantity,
            };
        }
    }

    public class SalaryMergeInput
    {
        public SalaryMergeInput(string productCode, SalaryExportTransactionGroupType salaryExportTransactionGroupType, SalaryExportTransactionDateMergeType salaryExportTransactionDateMergeType, List<SalaryExportTransaction> salaryExportTransactions,
            bool useCostCenter = true, bool useProject = false, bool department = false)
        {
            ProductCode = productCode;
            SalaryExportTransactionGroupType = salaryExportTransactionGroupType;
            SalaryExportTransactionDateMergeType = salaryExportTransactionDateMergeType;
            SalaryExportTransactions = salaryExportTransactions;
            this.UseCostCenter = useCostCenter;
            this.UseProject = useProject;
            this.UseDepartment = department;
        }
        public bool UseCostCenter { get; private set; }
        public bool UseProject { get; private set; }
        public bool UseDepartment { get; private set; }

        public string ProductCode { get; set; }
        public SalaryExportTransactionGroupType SalaryExportTransactionGroupType { get; set; } = SalaryExportTransactionGroupType.ByCode;
        public SalaryExportTransactionDateMergeType SalaryExportTransactionDateMergeType { get; set; } = SalaryExportTransactionDateMergeType.Day;
        public List<SalaryExportTransaction> SalaryExportTransactions { get; set; } = new List<SalaryExportTransaction>();
    }

    public class SalaryExportTrancationGroup
    {
        public List<string> ValidationErrors { get; private set; } = new List<string>();
        public SalaryMergeInput input { get; private set; }
        public string ProductCode => input.ProductCode;
        public SalaryExportTransactionGroupType SalaryExportTransactionGroupType => input.SalaryExportTransactionGroupType;
        public SalaryExportTransactionDateMergeType SalaryExportTransactionDateMergeType => input.SalaryExportTransactionDateMergeType;
        public List<SalaryExportTransaction> InitialTransactions => input.SalaryExportTransactions;
        public List<SalaryExportTransaction> MergedTransactions { get; private set; }

        public SalaryExportTrancationGroup(SalaryMergeInput input)
        {
            this.input = input;
        }

        private bool ValidInput()
        {
            return ValidateOnlyOneEmployeeInGroup() && ValidateDates() && ValidateProductCode();
        }

        private bool ValidateDates()
        {
            var rangeStart = DateTime.Now.AddYears(-10);
            var rangeEnd = DateTime.Now.AddYears(10);
            if (!InitialTransactions.All(a => a.Date > rangeStart && a.Date < rangeEnd))
            {
                ValidationErrors.Add("Invalid dates");
                return false;
            }

            return true;
        }

        private bool ValidateOnlyOneEmployeeInGroup()
        {
            if (InitialTransactions.GroupBy(g => g.EmployeeCode).Count() != 1)
            {
                ValidationErrors.Add("More than one employee Code");
                return false;
            }

            return true;
        }

        private bool ValidateProductCode()
        {
            if (!InitialTransactions.All(a => a.Code == ProductCode))
            {
                ValidationErrors.Add("Different ProductCodes is not allowed");
                return false;
            }

            return true;
        }

        public void MergeSalaryExportTransaction()
        {
            if (ValidInput())
                MergedTransactions = MergeSalaryExportTransactionByDate(SalaryExportTransactionDateMergeType);
        }

        private List<SalaryExportTransaction> MergeSalaryExportTransactionByDate(SalaryExportTransactionDateMergeType salaryExportTransactionDateMergeType)
        {
            if (!ValidInput())
                return new List<SalaryExportTransaction>();

            List<SalaryExportTransaction> mergedTransactions = new List<SalaryExportTransaction>();

            switch (salaryExportTransactionDateMergeType)
            {
                case SalaryExportTransactionDateMergeType.Day:
                    foreach (var ByDate in InitialTransactions.GroupBy(g => $"{g.Date}#{g.AllocateCost(SalaryExportTransactionGroupType, input.UseCostCenter, input.UseProject, input.UseDepartment)}"))
                    {
                        mergedTransactions.Add(new SalaryExportTransaction(ByDate.ToList())
                        {
                            Amount = ByDate.Sum(s => s.Amount),
                            Code = ByDate.First().Code,
                            Date = ByDate.First().Date,
                            Hours = ByDate.Sum(s => s.Hours),
                            Quantity = ByDate.Sum(s => s.Quantity),
                            CostAllocation = ByDate.First().CostAllocation,
                            EmployeeCode = ByDate.First().EmployeeCode,
                            DateMergeType = SalaryExportTransactionDateMergeType.Day,
                            EmployeeChildId = ByDate.FirstOrDefault(f => f.EmployeeChildId.HasValue)?.EmployeeChildId,
                            OfFullDayAbsence = ByDate.First().OfFullDayAbsence,
                            IsAbsence = ByDate.First().IsAbsence,
                            IsRegistrationQuantity = ByDate.First().IsRegistrationQuantity,
                            IsPaidVacation = ByDate.First().IsPaidVacation,
                            ExternalCode = ByDate.First().ExternalCode,
                        });
                    }
                    break;
                case SalaryExportTransactionDateMergeType.Week:
                    foreach (var ByWeek in InitialTransactions.OrderBy(o => o.Date).GroupBy(g => new { year = g.Date.Year, WeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(g.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday), group = $"{g.AllocateCost(SalaryExportTransactionGroupType, input.UseCostCenter, input.UseProject, input.UseDepartment)}" }))
                    {
                        mergedTransactions.Add(new SalaryExportTransaction(ByWeek.ToList())
                        {
                            Amount = ByWeek.Sum(s => s.Amount),
                            Code = ByWeek.First().Code,
                            Date = ByWeek.First().Date,
                            Hours = ByWeek.Sum(s => s.Hours),
                            Quantity = ByWeek.Sum(s => s.Quantity),
                            FromDate = ByWeek.First().Date,
                            ToDate = ByWeek.Last().Date,
                            CostAllocation = ByWeek.First().CostAllocation,
                            EmployeeCode = ByWeek.First().EmployeeCode,
                            DateMergeType = SalaryExportTransactionDateMergeType.Week,
                            EmployeeChildId = ByWeek.FirstOrDefault(f => f.EmployeeChildId.HasValue)?.EmployeeChildId,
                            IsAbsence = ByWeek.First().IsAbsence,
                            IsRegistrationQuantity = ByWeek.First().IsRegistrationQuantity,
                            IsPaidVacation = ByWeek.First().IsPaidVacation,
                            ExternalCode = ByWeek.First().ExternalCode,
                        });
                    }
                    break;
                case SalaryExportTransactionDateMergeType.Month:
                    foreach (var ByMonth in InitialTransactions.OrderBy(o => o.Date).GroupBy(g => new { g.Date.Year, g.Date.Month, group = $"{g.AllocateCost(SalaryExportTransactionGroupType, input.UseCostCenter, input.UseProject, input.UseDepartment)}" }))
                    {
                        mergedTransactions.Add(new SalaryExportTransaction()
                        {
                            Amount = ByMonth.Sum(s => s.Amount),
                            Code = ByMonth.First().Code,
                            Date = ByMonth.First().Date,
                            Hours = ByMonth.Sum(s => s.Hours),
                            Quantity = ByMonth.Sum(s => s.Quantity),
                            FromDate = ByMonth.First().Date,
                            ToDate = ByMonth.Last().Date,
                            CostAllocation = ByMonth.First().CostAllocation,
                            EmployeeCode = ByMonth.First().EmployeeCode,
                            Children = ByMonth.ToList(),
                            DateMergeType = SalaryExportTransactionDateMergeType.Month,
                            EmployeeChildId = ByMonth.FirstOrDefault(f => f.EmployeeChildId.HasValue)?.EmployeeChildId,
                            IsAbsence = ByMonth.First().IsAbsence,
                            IsRegistrationQuantity = ByMonth.First().IsRegistrationQuantity,
                            IsPaidVacation = ByMonth.First().IsPaidVacation,
                            ExternalCode = ByMonth.First().ExternalCode,
                        });
                    }
                    break;
                case SalaryExportTransactionDateMergeType.CoherentTimeDateIntervals:
                    MergedTransactions = new List<SalaryExportTransaction>();
                    var orderedTransactions = MergeSalaryExportTransactionByDate(SalaryExportTransactionDateMergeType.Day).OrderBy(o => o.Date).ToList();

                    while (orderedTransactions.Count > 0)
                    {
                        var currentTransaction = orderedTransactions.First();
                        var sequenceTransactions = new List<SalaryExportTransaction> { currentTransaction };
                        orderedTransactions.RemoveAt(0);

                        for (int i = 0; i < sequenceTransactions.Count; i++)
                        {
                            var nextDayTransaction = orderedTransactions.FirstOrDefault(t => t.Date == sequenceTransactions[i].Date.AddDays(1));

                            if (nextDayTransaction != null)
                            {
                                sequenceTransactions.Add(nextDayTransaction);
                                orderedTransactions.Remove(nextDayTransaction);
                            }
                        }

                        // Merge all transactions in the sequence
                        var mergedTransaction = new SalaryExportTransaction()
                        {
                            Amount = sequenceTransactions.Sum(t => t.Amount),
                            Code = sequenceTransactions.First().Code,
                            Date = sequenceTransactions.First().Date,
                            FromDate = sequenceTransactions.First().Date,
                            ToDate = sequenceTransactions.Last().Date,
                            Hours = sequenceTransactions.Sum(t => t.Hours),
                            Quantity = sequenceTransactions.Sum(t => t.Quantity),
                            CostAllocation = sequenceTransactions.First().CostAllocation,
                            EmployeeCode = sequenceTransactions.First().EmployeeCode,
                            Children = sequenceTransactions.ToList(),
                            DateMergeType = SalaryExportTransactionDateMergeType.CoherentTimeDateIntervals,
                            EmployeeChildId = sequenceTransactions.FirstOrDefault(f => f.EmployeeChildId.HasValue)?.EmployeeChildId,
                            IsAbsence = sequenceTransactions.First().IsAbsence,
                            IsRegistrationQuantity = sequenceTransactions.First().IsRegistrationQuantity,
                            IsPaidVacation = sequenceTransactions.First().IsPaidVacation,
                            ExternalCode = sequenceTransactions.First().ExternalCode,
                        };

                        mergedTransactions.Add(mergedTransaction);
                    }
                    break;
                default:
                    break;
            }

            return mergedTransactions;
        }
    }

    public enum SalaryExportTransactionGroupType
    {
        ByCode = 0,
        ByCostAllocation = 1,
    }

    public enum SalaryExportTransactionDateMergeType
    {
        Day = 1,
        Week = 2,
        Month = 3,
        CoherentTimeDateIntervals = 4
    }

}

