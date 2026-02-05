using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Data
{

    public sealed class ExtensionCache
    {
        #region Variables

        private static readonly object employeePayrollGroupLock = new object();
        private List<EmployeePayrollGroupExtensionCacheItem> employeePayrollGroupCache;

        private static readonly object employmentCalendarLock = new object();
        private ConcurrentDictionary<string, List<EmploymentCalenderDTO>> employmentCalenderCache;

        #endregion

        #region Ctor

        private ExtensionCache()
        {
        }

        #region Fully lazy instantiation

        public static ExtensionCache Instance
        {
            get
            {
                return NestedSingleton.instance;
            }
        }

        private class NestedSingleton
        {
            internal static readonly ExtensionCache instance = new ExtensionCache();

            static NestedSingleton()
            {
            }
        }

        #endregion

        #endregion

        #region EmployeePayrollGroup

        public EmployeePayrollGroupExtensionCacheItem GetEmployeePayrollGroupExtensionCache(int actorCompanyId)
        {
            List<EmployeePayrollGroupExtensionCacheItem> cacheItems = GetEmployeePayrollGroups(actorCompanyId);
            RemoveExpiredEmployeePayrollGroup(cacheItems);
            return cacheItems.FirstOrDefault(i => i.ValidTo > DateTime.UtcNow);
        }

        public void AddToEmployeePayrollGroupExtensionCaches(int actorCompanyId, List<EmployeeGroup> employeeGroups, List<PayrollGroup> payrollGroups, List<PayrollPriceType> payrollPriceTypes, List<AnnualLeaveGroup> annualLeaveGroups, DateTime? validTo = null)
        {
            AddToEmployeePayrollGroupExtensionCaches(actorCompanyId, employeeGroups?.ToDTOs().ToList(), payrollGroups?.ToDTOs().ToList(), payrollPriceTypes?.ToDTOs().ToList(), annualLeaveGroups?.ToDTOs().ToList(), validTo);
        }

        public void AddToEmployeePayrollGroupExtensionCaches(int actorCompanyId, List<EmployeeGroupDTO> employeeGroups, List<PayrollGroupDTO> payrollGroups, List<PayrollPriceTypeDTO> payrollPriceTypes, List<AnnualLeaveGroupDTO> annualLeaveGroups, DateTime? validTo = null)
        {
            EmployeePayrollGroupExtensionCacheItem cacheItem = new EmployeePayrollGroupExtensionCacheItem(actorCompanyId, validTo, employeeGroups, payrollGroups, payrollPriceTypes, annualLeaveGroups);
            AddEmployeePayrollGroup(cacheItem);
            FlushEmployeePayrollGroupExtensionCaches(actorCompanyId);
        }

        public void ExtendEmployeePayrollGroupExtensionCache(int actorCompanyId, DateTime oldValidTo, DateTime newValidTo)
        {
            EmployeePayrollGroupExtensionCacheItem cacheItem = GetEmployeePayrollGroup(actorCompanyId, oldValidTo);
            ExtendEmployeePayrollGroup(cacheItem, newValidTo);
        }

        public void FlushEmployeePayrollGroupExtensionCaches(int? actorCompanyId = null)
        {
            lock (employeePayrollGroupLock)
            {
                if (actorCompanyId.HasValue)
                {
                    RemoveExpiredEmployeePayrollGroup(GetEmployeePayrollGroups(actorCompanyId.Value));
                }
                else
                {
                    this.employeePayrollGroupCache = null;
                }
            }
        }


        #region Help-methods

        private List<EmployeePayrollGroupExtensionCacheItem> GetEmployeePayrollGroups(int actorCompanyId)
        {
            return this.employeePayrollGroupCache?.Where(i => i.ActorCompanyId == actorCompanyId).ToList() ?? new List<EmployeePayrollGroupExtensionCacheItem>();
        }

        private EmployeePayrollGroupExtensionCacheItem GetEmployeePayrollGroup(int actorCompanyId, DateTime validTo)
        {
            return this.employeePayrollGroupCache?.FirstOrDefault(i => i.ActorCompanyId == actorCompanyId && i.ValidTo == validTo);
        }

        private void AddEmployeePayrollGroup(EmployeePayrollGroupExtensionCacheItem cacheItem)
        {
            if (cacheItem == null)
                return;

            lock (employeePayrollGroupLock)
            {
                if (this.employeePayrollGroupCache == null)
                    this.employeePayrollGroupCache = new List<EmployeePayrollGroupExtensionCacheItem>();
                this.employeePayrollGroupCache.Add(cacheItem);
            }
        }

        private void ExtendEmployeePayrollGroup(EmployeePayrollGroupExtensionCacheItem cacheItem, DateTime validTo)
        {
            if (cacheItem == null)
                return;

            lock (employeePayrollGroupLock)
            {
                cacheItem.UpdateValidTo(validTo);
            }
        }

        private void RemoveExpiredEmployeePayrollGroup(List<EmployeePayrollGroupExtensionCacheItem> cacheItems)
        {
            if (cacheItems.IsNullOrEmpty())
                return;

            foreach (EmployeePayrollGroupExtensionCacheItem expiredCacheItem in cacheItems.Where(w => w.ValidTo < DateTime.UtcNow))
            {
                RemoveExpiredEmployeePayrollGroup(expiredCacheItem);
            }
        }

        private void RemoveExpiredEmployeePayrollGroup(EmployeePayrollGroupExtensionCacheItem cacheItem)
        {
            if (cacheItem == null)
                return;

            lock (employeePayrollGroupLock)
            {
                employeePayrollGroupCache.Remove(cacheItem);
            }
        }

        #endregion

        #endregion

        #region EmploymentCalendar

        public int? GetEmployeeGroupIdFromExtensionCache(int employeeId, DateTime? date)
        {
            if (employmentCalenderCache == null)
                return null;

            List<EmploymentCalenderDTO> employmentCalenders = GetEmploymentCalenders(employeeId);
            EmploymentCalenderDTO cacheItem = GetEmploymentCalender(employmentCalenders, employeeId, date);
            if (cacheItem == null)
                return null;

            int? id = cacheItem.EmployeeGroupId;
            if (!cacheItem.IsValid())
            {
                employmentCalenderCache.TryRemove(cacheItem.Key, out employmentCalenders);
                id = null;
            }

            return id;
        }

        public int? GetPayrollGroupIdFromExtensionCache(int employeeId, DateTime? date)
        {
            if (employmentCalenderCache == null)
                return null;

            List<EmploymentCalenderDTO> employmentCalenders = GetEmploymentCalenders(employeeId);
            EmploymentCalenderDTO cacheItem = GetEmploymentCalender(employmentCalenders, employeeId, date);
            if (cacheItem == null)
                return null;

            int? id = cacheItem.PayrollGroupId;
            if (!cacheItem.IsValid())
            {
                employmentCalenderCache.TryRemove(cacheItem.Key, out employmentCalenders);
                id = null;
            }

            return id;
        }

        public int? GetAnnualLeaveGroupIdFromExtensionCache(int employeeId, DateTime? date)
        {
            if (employmentCalenderCache == null)
                return null;

            List<EmploymentCalenderDTO> employmentCalenders = GetEmploymentCalenders(employeeId);
            EmploymentCalenderDTO cacheItem = GetEmploymentCalender(employmentCalenders, employeeId, date);
            if (cacheItem == null)
                return null;

            int? id = cacheItem.AnnualLeaveGroupId;
            if (!cacheItem.IsValid())
            {
                employmentCalenderCache.TryRemove(cacheItem.Key, out employmentCalenders);
                id = null;
            }

            return id;
        }

        public int? GetDayTypeIdFromExtensionCache(int employeeId, DateTime? date)
        {
            if (employmentCalenderCache == null)
                return null;

            List<EmploymentCalenderDTO> employmentCalenders = GetEmploymentCalenders(employeeId);
            EmploymentCalenderDTO cacheItem = GetEmploymentCalender(employmentCalenders, employeeId, date);
            if (cacheItem == null)
                return null;

            int? id = cacheItem.DayTypeId;
            if (!cacheItem.IsValid())
            {
                employmentCalenderCache.TryRemove(cacheItem.Key, out employmentCalenders);
                id = null;
            }

            return id;
        }

        public void AddToEmploymentCalendarExtensionCaches(List<EmploymentCalenderDTO> items, DateTime? validTo = null)
        {
            if (!validTo.HasValue)
                validTo = DateTime.UtcNow.AddSeconds(60);

            foreach (EmploymentCalenderDTO item in items)
            {
                item.CacheValidTo = validTo.Value;
            }

            foreach (var item in items.GroupBy(g => g.EmployeeId))
            {
                string key = $"{item.Key}";
                List<EmploymentCalenderDTO> employmentCalenderDTOs;

                lock (employmentCalendarLock)
                {
                    if (this.employmentCalenderCache == null)
                        this.employmentCalenderCache = new ConcurrentDictionary<string, List<EmploymentCalenderDTO>>();

                    employmentCalenderCache.TryRemove(key, out employmentCalenderDTOs);
                    this.employmentCalenderCache.TryAdd(key, item.ToList());
                }
            }
        }

        public void ExtendEmploymentCalendarCache(List<int> employeeIds, DateTime oldValidTo, DateTime newValidTo)
        {
            if (employeeIds.IsNullOrEmpty())
                return;

            foreach (int employeeId in employeeIds)
            {
                ExtendEmploymentCalendarCache(employeeId, oldValidTo, newValidTo);
            }
        }

        public void ExtendEmploymentCalendarCache(int employeeId, DateTime oldValidTo, DateTime newValidTo)
        {
            List<EmploymentCalenderDTO> employmentCalenders = GetEmploymentCalenders(employeeId);
            if (employmentCalenders == null)
                return;

            var cacheItems = employmentCalenders.Where(w => w != null && w.EmployeeId == employeeId && w.CacheValidTo == oldValidTo).ToList();
            foreach (var item in cacheItems)
            {
                item.CacheValidTo = newValidTo;
            }
        }

        public void FlushEmploymentCalendar()
        {
            this.employmentCalenderCache = null;
        }

        #region Help-methods

        public List<EmploymentCalenderDTO> GetEmploymentCalenders(int employeeId)
        {
            List<EmploymentCalenderDTO> employmentCalenders;
            employmentCalenderCache.TryGetValue($"{employeeId}", out employmentCalenders);
            return employmentCalenders;
        }

        private EmploymentCalenderDTO GetEmploymentCalender(List<EmploymentCalenderDTO> employmentCalenders, int employeeId, DateTime? date)
        {
            if (date == null)
                date = DateTime.Now.Date;

            return employmentCalenders?.FirstOrDefault(i => i != null && i.EmployeeId == employeeId && i.Date == date);
        }

        #endregion

        #endregion
    }

    public class EmployeePayrollGroupExtensionCacheItem
    {
        public int ActorCompanyId { get; private set; }
        public DateTime ValidTo { get; private set; }
        public List<EmployeeGroupDTO> EmployeeGroups { get; private set; }
        public List<PayrollGroupDTO> PayrollGroups { get; private set; }
        public List<PayrollPriceTypeDTO> PayrollPriceTypes { get; private set; }
        public List<AnnualLeaveGroupDTO> AnnualLeaveGroups { get; private set; }

        public EmployeePayrollGroupExtensionCacheItem(int actorCompanyId, DateTime? validTo, List<EmployeeGroupDTO> employeeGroups, List<PayrollGroupDTO> payrollGroups, List<PayrollPriceTypeDTO> payrollPriceTypes, List<AnnualLeaveGroupDTO> annualLeaveGroups)
        {
            ActorCompanyId = actorCompanyId;
            ValidTo = validTo ?? DateTime.UtcNow.AddMinutes(10);
            EmployeeGroups = employeeGroups;
            PayrollGroups = payrollGroups;
            PayrollPriceTypes = payrollPriceTypes;
            AnnualLeaveGroups = annualLeaveGroups;
        }

        public void UpdateValidTo(DateTime validTo)
        {
            this.ValidTo = validTo;
        }
    }
}