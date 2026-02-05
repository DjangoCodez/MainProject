using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static SoftOne.Soe.Business.Core.TimeEngine.TimeEngine;

namespace SoftOne.Soe.Business.Util.AnnualLeave.PreFlight
{
    public class GroupedAnnualLeaveData
    {
        public DateTime Date { get; set; }
        public decimal TotalMinutes { get; set; }
    }

    public class AnnualLeaveCalculation
    {
        public List<AnnualLeaveGroupLimitDTO> GetAnnualLeaveGroupLimits(TermGroup_AnnualLeaveGroupType type)
        {
            List<AnnualLeaveGroupLimitDTO> limits = new List<AnnualLeaveGroupLimitDTO>();

            switch (type)
            {
                case TermGroup_AnnualLeaveGroupType.Commercial:
                    AddLimitToList(limits, 200, 1, 7.5);
                    AddLimitToList(limits, 400, 2, 15);
                    AddLimitToList(limits, 600, 3, 22.5);
                    AddLimitToList(limits, 800, 4, 30);
                    AddLimitToList(limits, 1000, 5, 37.5);
                    AddLimitToList(limits, 1150, 6, 45);
                    AddLimitToList(limits, 1300, 7, 52.5);
                    AddLimitToList(limits, 1430, 8, 60);
                    AddLimitToList(limits, 1560, 9, 67.5);
                    break;
                case TermGroup_AnnualLeaveGroupType.HotelRestaurant:
                    AddLimitToList(limits, 200, 1, 7.5);
                    AddLimitToList(limits, 400, 2, 15);
                    AddLimitToList(limits, 600, 3, 22.5);
                    AddLimitToList(limits, 800, 4, 30);
                    AddLimitToList(limits, 1000, 5, 37.5);
                    AddLimitToList(limits, 1200, 6, 45);
                    AddLimitToList(limits, 1400, 7, 52.5);
                    AddLimitToList(limits, 1540, 8, 60);
                    AddLimitToList(limits, 1640, 9, 67.5);
                    break;
                case TermGroup_AnnualLeaveGroupType.HotelRestaurantNight:
                    AddLimitToList(limits, 160, 1, 6);
                    AddLimitToList(limits, 320, 2, 12);
                    AddLimitToList(limits, 480, 3, 18);
                    AddLimitToList(limits, 640, 4, 24);
                    AddLimitToList(limits, 800, 5, 30);
                    AddLimitToList(limits, 960, 6, 36);
                    AddLimitToList(limits, 1120, 7, 42);
                    AddLimitToList(limits, 1280, 8, 48);
                    break;
            }

            return limits;
        }

        public void AddLimitToList(List<AnnualLeaveGroupLimitDTO> limits, int workedHours, int nbrOfDaysAnnualLeave, double nbrOfHoursAnnualLeave)
        {
            limits.Add(new AnnualLeaveGroupLimitDTO()
            {
                WorkedMinutes = workedHours * 60,
                NbrOfDaysAnnualLeave = nbrOfDaysAnnualLeave,
                NbrOfMinutesAnnualLeave = (int)(nbrOfHoursAnnualLeave * 60)
            });
        }

        public AnnualLeaveGroupLimitDTO GetAnnualLeaveGroupLimitReached(TermGroup_AnnualLeaveGroupType type, int minutes)
        {
            // Get highest limit reached by specified minutes
            List<AnnualLeaveGroupLimitDTO> limits = GetAnnualLeaveGroupLimits(type);
            AnnualLeaveGroupLimitDTO limit = limits.OrderByDescending(l => l.WorkedMinutes).FirstOrDefault(l => l.WorkedMinutes <= minutes);

            return limit;
        }

        public int GetAnnualLeaveGroupLimitLevelReached(TermGroup_AnnualLeaveGroupType type, int minutes)
        {
            return GetAnnualLeaveGroupLimitReached(type, minutes)?.NbrOfDaysAnnualLeave ?? 0;
        }

        public List<AnnualLeaveTransactionEarned> GetAnnualLeaveTransactionsEarned(EmployeePeriodData employeeData, List<AnnualLeaveGroupDTO> annualLeaveGroupDTOs, int startNbrOfDays = 0, int startAccMinutes = 0)
        {
            List<AnnualLeaveTransactionEarned> transactionsEarned = new List<AnnualLeaveTransactionEarned>();

            if (!employeeData.EmploymentCalendar.Any())
                return transactionsEarned;

            DateTime firstDate = employeeData.EmploymentCalendar.Min(x => x.Date);

            // Exclude not valid absence
            employeeData.Transactions = employeeData.Transactions.Where(t => !t.IsInvalidAbsenceAsWorkTime()).ToList();
            
            DateTime lastWorkTimeDate = employeeData.Transactions.Where(t => t.IsWorkTime()).OrderByDescending(t => t.Date).Select(t => t.Date).FirstOrDefault();
            if (lastWorkTimeDate != null && lastWorkTimeDate != default)
                lastWorkTimeDate = lastWorkTimeDate.AddDays(1);
            else
                lastWorkTimeDate = new DateTime(DateTime.Today.Year, 1, 1);
            
            // Group transactions by date
            List<GroupedAnnualLeaveData> groupedData = employeeData.Transactions
                    .GroupBy(t => t.Date)
                    .Select(g => new GroupedAnnualLeaveData
                    {
                        Date = g.Key,
                        TotalMinutes = g.Sum(t => t.Quantity)
                    })
                    .ToList();

            // Group schedule by date
            // TODO: We should filter out absence that are valid
            List<GroupedAnnualLeaveData> groupedSchedule = employeeData.ScheduleBlocks
                    .Where(s => s.Date >= lastWorkTimeDate)
                    .GroupBy(s => s.Date)
                    .Select(g => new GroupedAnnualLeaveData
                    {
                        Date = g.Key.Value,
                        TotalMinutes = g.Sum(s => s.IsBreak ? -s.TotalMinutes : s.TotalMinutes)
                    })
                    .ToList();

            // Group ingoing manually earned annual leave transactions by date
            List<GroupedAnnualLeaveData> groupedManuallyEarned = employeeData.IngoingAnnualLeaveTransactions
                    .GroupBy(t => t.DateEarned)
                    .Select(g => new GroupedAnnualLeaveData
                    {
                        Date = g.Key.Value,
                        TotalMinutes = g.Sum(t => t.AccumulatedMinutes)
                    })
                    .ToList();


            // merge groupedSchedule into groupedData
            foreach (var schedule in groupedSchedule)
            {
                var existing = groupedData.FirstOrDefault(x => x.Date == schedule.Date);
                if (existing != null)
                {
                    // If exists, add the minutes
                    groupedData = groupedData.Select(x => x.Date == schedule.Date ? new GroupedAnnualLeaveData { Date = x.Date, TotalMinutes = x.TotalMinutes + schedule.TotalMinutes } : x).ToList();
                }
                else
                {
                    // If not exists, add new entry
                    var newData = new GroupedAnnualLeaveData { Date = schedule.Date, TotalMinutes = schedule.TotalMinutes };
                    groupedData.Add(newData);
                }
            }

            // merge groupedManuallyEarned into groupedData
            foreach (var manual in groupedManuallyEarned)
            {
                var existing = groupedData.FirstOrDefault(x => x.Date == manual.Date);
                if (existing != null)
                {
                    // If exists, add the minutes
                    groupedData = groupedData.Select(x => x.Date == manual.Date ? new GroupedAnnualLeaveData { Date = x.Date, TotalMinutes = x.TotalMinutes + manual.TotalMinutes } : x).ToList();
                }
                else
                {
                    // If not exists, add new entry
                    var newData = new GroupedAnnualLeaveData { Date = manual.Date, TotalMinutes = manual.TotalMinutes };
                    groupedData.Add(newData);
                }
            }


            int totalMinutes = startAccMinutes;
            int lastNbrOfDays = startNbrOfDays;
            int qualifyingMonths = 0;
            int gapBetweenEmployments = 0;
            bool firstEmploymentNeedsToBeVerified = false;
            DateTime qualifyingEndDate = DateTime.MinValue;
            AnnualLeaveGroupDTO lastAnnualLeaveGroup = null;

            foreach (EmploymentCalenderDTO employmentCalendar in employeeData.EmploymentCalendar.Where(c => c.Date >= employeeData.PeriodStart).ToList())
            {
                DateTime currentDate = employmentCalendar.Date;

                // If no employment on first date, flag that first employment found needs to be verified for qualifying period
                if (currentDate == firstDate && employmentCalendar.EmploymentId == 0)
                    firstEmploymentNeedsToBeVerified = true;

                // Employment exists on this date
                if (employmentCalendar.EmploymentId > 0)
                {
                    EmploymentCalenderDTO currentEmploymentCalendarDTO = employeeData.EmploymentCalendar.FirstOrDefault(x => x.Date == currentDate);
                    AnnualLeaveGroupDTO currentAnnualLeaveGroup = annualLeaveGroupDTOs.FirstOrDefault(g => g.AnnualLeaveGroupId == currentEmploymentCalendarDTO.AnnualLeaveGroupId);

                    if (currentAnnualLeaveGroup != null)
                    {
                        // Gap days and qualifying months according to agreement
                        int gapDays = currentAnnualLeaveGroup.GapDays;
                        qualifyingMonths = currentAnnualLeaveGroup.QualifyingMonths ?? 0;


                        // If this is the first employment, set qualifying end date
                        if (employeeData.FirstEmployment != null && employmentCalendar.EmploymentId == employeeData.FirstEmployment.EmploymentId && currentDate == firstDate)
                        {
                            gapBetweenEmployments = 0;
                            qualifyingEndDate = employeeData.FirstEmployment.DateFrom.Value.AddMonths(qualifyingMonths);
                        }
                        else if (firstEmploymentNeedsToBeVerified)
                        {
                            // Need to count gap days from previous employments end date until today
                            var currentHistoryDate = currentDate.AddDays(-1);
                            gapBetweenEmployments = 0;

                            while (currentHistoryDate > currentDate.AddDays(-gapDays - 1))
                            {
                                var currentHistoryEmploymentCalendar = employeeData.EmploymentCalendar.FirstOrDefault(x => x.Date == currentHistoryDate);
                                if (currentHistoryEmploymentCalendar != null && currentHistoryEmploymentCalendar != default && currentHistoryEmploymentCalendar.EmploymentId == 0)
                                {
                                    gapBetweenEmployments++;
                                    currentHistoryDate = currentHistoryDate.AddDays(-1);
                                }
                            }
                        }

                        // If gap is larger than allowed, set new end date for qualifying period according to agreement
                        if (gapBetweenEmployments >= gapDays)
                        {
                            qualifyingEndDate = currentDate.AddMonths(qualifyingMonths);
                            // totalMinutes = 0; // Reset total minutes if gap is larger than allowed
                        }

                        // if not within qualifying period
                        if (currentDate > qualifyingEndDate)
                        {
                            // Find worked transactions
                            var workedTransactions = groupedData.FirstOrDefault(x => x.Date == currentDate);
                            if (workedTransactions != null && workedTransactions != default)
                            {
                                totalMinutes += (int)workedTransactions.TotalMinutes;

                                // Check if currentAnnualLeaveGroup is different from lastAnnualLeaveGroup and if so, calculate factor of exceeded hours for last limit into current limit.
                                if (lastAnnualLeaveGroup != null && currentAnnualLeaveGroup.AnnualLeaveGroupId != lastAnnualLeaveGroup.AnnualLeaveGroupId)
                                {
                                    var lastTransaction = transactionsEarned.Last();
                                    List<AnnualLeaveGroupLimitDTO> lastLimits = GetAnnualLeaveGroupLimits(lastTransaction.Type).OrderByDescending(x => x.NbrOfDaysAnnualLeave).ToList();
                                    AnnualLeaveGroupLimitDTO lastLimitPassed = lastLimits.FirstOrDefault(l => l.NbrOfDaysAnnualLeave == lastTransaction.Level);
                                    if (lastLimitPassed != null && lastLimitPassed != default)
                                    {
                                        // Calculate factor of exceeded hours for last limit into current limit
                                        int exceededMinutes = lastTransaction.AccumulatedMinutes - lastLimitPassed.WorkedMinutes;
                                        int factor = exceededMinutes / lastLimits.FirstOrDefault(l => l.NbrOfDaysAnnualLeave == 1).WorkedMinutes;

                                        // Reduce recently added minutes
                                        totalMinutes = totalMinutes - exceededMinutes;
                                        // Add thos again with factor
                                        totalMinutes += (int)(exceededMinutes * factor);
                                    }
                                }

                                // Check current limits
                                List<AnnualLeaveGroupLimitDTO> currentLimits = GetAnnualLeaveGroupLimits(currentAnnualLeaveGroup.Type).OrderBy(x => x.NbrOfDaysAnnualLeave).ToList();
                                AnnualLeaveGroupLimitDTO limit = currentLimits.OrderByDescending(x => x.NbrOfDaysAnnualLeave).FirstOrDefault(l => totalMinutes >= l.WorkedMinutes);

                                if (limit != null && limit != default)
                                {
                                    // If current exceeded limit is bigger than last limit level (comparing the days) then add a new transaction
                                    if (limit.NbrOfDaysAnnualLeave > lastNbrOfDays)
                                    {
                                        int accLimitMinutes = 0;
                                        for (int i = lastNbrOfDays; i < limit.NbrOfDaysAnnualLeave; i++)
                                        {
                                            var accumulatedMinutes = totalMinutes;
                                            var currentLimit = currentLimits[i];
                                            var nextAccMinutes = currentLimits[i + 1]?.WorkedMinutes ?? 525600; // 1 year in minutes if no next limit
                                            accLimitMinutes += currentLimit.WorkedMinutes;

                                            if (totalMinutes > accLimitMinutes && totalMinutes >= nextAccMinutes)
                                                accumulatedMinutes = currentLimit.WorkedMinutes;

                                            transactionsEarned.Add(new AnnualLeaveTransactionEarned
                                            {
                                                EmployeeId = employeeData.Employee.EmployeeId,
                                                Date = currentDate,
                                                Days = 1,
                                                Minutes = currentLimits.FirstOrDefault(l => l.NbrOfDaysAnnualLeave == 1).NbrOfMinutesAnnualLeave,
                                                Level = currentLimit.NbrOfDaysAnnualLeave,
                                                AccumulatedMinutes = accumulatedMinutes,
                                                Type = currentAnnualLeaveGroup.Type
                                            });
                                        }
                                        lastNbrOfDays = limit.NbrOfDaysAnnualLeave;
                                        lastAnnualLeaveGroup = currentAnnualLeaveGroup;
                                    }
                                }
                            }
                        }
                    }
                    // Reset gap if employment exists
                    gapBetweenEmployments = 0;
                    firstEmploymentNeedsToBeVerified = false;
                }
                else
                {
                    // Employment does not exist, increment gap
                    gapBetweenEmployments++;
                }
            }

            return transactionsEarned;
        }
    }
}
