using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using SoftOne.Status.Shared;
using SoftOne.Status.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core
{
    public class DashboardManager : ManagerBase
    {
        #region Ctor

        public DashboardManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Dashboard

        #region SysGauge

        /// <summary>
        /// Get all SysGauges for specified module
        /// </summary>
        /// <param name="module">Module filter. If 'None' no filter on module will occur</param>
        /// <param name="licenseId">License ID</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="roleId">RoleId</param>
        /// <returns>Collection of SysGauges</returns>
        public IEnumerable<SysGauge> GetSysGauges(SoeModule module, int licenseId, int actorCompanyId, int roleId)
        {
            List<SysGauge> gauges = new List<SysGauge>();

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var query = (from sg in sysEntitiesReadOnly.SysGauge
                         where sg.State == (int)SoeEntityState.Active
                         select sg);

            // Filter on module
            if (module != SoeModule.None)
                query = query.Where(sg => sg.SysGaugeModule.Any(m => m.Module == (int)module));

            foreach (var gauge in query.ToList())
            {
                // Get gauge name from SysTerm
                int sysTermId = gauge.SysTermId;

                // Special for gauges used in multiple modules
                switch ((Feature)gauge.SysFeatureId)
                {
                    case Feature.Billing_OpenShiftsGauge:
                        sysTermId = 164;
                        break;
                    case Feature.Billing_WantedShiftsGauge:
                        sysTermId = 210;
                        break;
                    case Feature.Billing_MyShiftsGauge:
                        sysTermId = 469;
                        break;
                }
                gauge.Name = GetText(sysTermId, (int)TermGroup.Dashboard);

                // Check permission
                if (FeatureManager.HasRolePermission((Feature)gauge.SysFeatureId, Permission.Readonly, roleId, actorCompanyId, licenseId))
                    gauges.Add(gauge);
            }

            return gauges;
        }

        public SysGauge GetSysGauge(int sysGaugeId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysGauge(sysEntitiesReadOnly, sysGaugeId);
        }

        public SysGauge GetSysGauge(SOESysEntities entities, int sysGaugeId)
        {
            return (from sg in entities.SysGauge
                    where sg.SysGaugeId == sysGaugeId &&
                    sg.State == (int)SoeEntityState.Active
                    select sg).FirstOrDefault();
        }

        #endregion

        #region UserGauge

        /// <summary>
        /// Get all user gauges for specifed module and user
        /// </summary>
        /// <param name="module">Module</param>
        /// <param name="licenseId">License ID</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="loadSysGaugeNames">If true, SysGauge names are set on the user gauges</param>
        /// <returns>Collection of UserGauges</returns>
        public IEnumerable<UserGauge> GetUserGauges(SoeModule module, int licenseId, int actorCompanyId, int roleId, int userId, bool loadSysGaugeNames)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserGauge.NoTracking();
            return GetUserGauges(entities, module, licenseId, actorCompanyId, roleId, userId, loadSysGaugeNames);
        }

        /// <summary>
        /// Get all user gauges for specifed module and user
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="module">Module</param>
        /// <param name="licenseId">License ID</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="loadSysGaugeNames">If true, SysGauge names are set on the user gauges</param>
        /// <returns>Collection of UserGauges</returns>
        public IEnumerable<UserGauge> GetUserGauges(CompEntities entities, SoeModule module, int licenseId, int actorCompanyId, int roleId, int userId, bool loadSysGaugeNames)
        {
            List<UserGauge> userGauges = new List<UserGauge>();

            var gauges = (from g in entities.UserGauge.Include("UserGaugeSetting")
                          where g.Module == (int)module &&
                          g.ActorCompanyId == actorCompanyId &&
                          g.RoleId == roleId &&
                          g.UserId == userId
                          orderby g.Sort
                          select g).ToList();

            foreach (var gauge in gauges)
            {
                SysGauge sysGauge = GetSysGauge(gauge.SysGaugeId);
                if (sysGauge != null)
                {
                    // Check permission
                    if (!FeatureManager.HasRolePermission((Feature)sysGauge.SysFeatureId, Permission.Readonly, roleId, actorCompanyId, licenseId))
                        continue;

                    // Get gauge name
                    if (loadSysGaugeNames)
                        gauge.SysGaugeName = sysGauge.GaugeName;

                    userGauges.Add(gauge);
                }
            }

            return userGauges;
        }
        public UserGaugeHeadDTO GetUserGaugeHead(int userGaugeHeadId, SoeModule module, int userId, int roleId, int actorCompanyId, int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserGaugeHead.NoTracking();
            return GetUserGaugeHead(entities, userGaugeHeadId, module, userId, roleId, actorCompanyId, licenseId);
        }
        public UserGaugeHeadDTO GetUserGaugeHead(CompEntities entities, int userGaugeHeadId, SoeModule module, int userId, int roleId, int actorCompanyId, int licenseId)
        {
            if (userGaugeHeadId == 0)
                userGaugeHeadId = GetPrioritizedUserGaugeHeadId(entities, actorCompanyId, userId, roleId);

            if (userGaugeHeadId == 0)
            {
                var userGauges = GetUserGauges(entities, module, licenseId, actorCompanyId, roleId, userId, true);
                return CreateTemporaryUserGaugeHead(userGauges.ToDTOs(true));
            }

            var head = GetUserGaugeHead(entities, userGaugeHeadId, userId, roleId, actorCompanyId, licenseId);

            for (int i = head.UserGauge.Count - 1; i >= 0; i--)
            {
                var element = head.UserGauge.ElementAt(i);
                if (!HasGaugePermission(element, roleId, actorCompanyId, licenseId))
                {
                    head.UserGauge.Remove(element);
                }
            }

            return head != null ? head.ToDTO() : null;
        }
        public UserGaugeHead GetUserGaugeHead(CompEntities entities, int userGaugeHeadId, int userId, int roleId, int actorCompanyId, int licenseId)
        {
            var head = entities.UserGaugeHead
                .Where(g => g.UserGaugeHeadId == userGaugeHeadId && g.ActorCompanyId == actorCompanyId && g.UserId == userId)
                .Include("UserGauge")
                .Include("UserGauge.UserGaugeSetting")
                .FirstOrDefault();

            if (head == null) return null;

            return head;
        }
        public List<UserGaugeHead> GetUserGaugeHeads(int userId, int roleId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserGaugeHead.NoTracking();
            return GetUserGaugeHeads(entities, userId, roleId, actorCompanyId);
        }
        public List<UserGaugeHead> GetUserGaugeHeads(CompEntities entities, int userId, int roleId, int actorCompanyId)
        {
            return entities.UserGaugeHead
                .Where(h => h.UserId == userId && h.ActorCompanyId == actorCompanyId)
                .ToList();
        }
        public ActionResult SaveUserGaugeHead(UserGaugeHeadDTO dto, int actorCompanyId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserGaugeHead.NoTracking();
            return SaveUserGaugeHead(entities, dto, actorCompanyId, userId);
        }
        public ActionResult SaveUserGaugeHead(CompEntities entities, UserGaugeHeadDTO dto, int actorCompanyId, int userId)
        {
            UserGaugeHead head = null;
            if (dto.UserGaugeHeadId > 0)
            {
                head = entities.UserGaugeHead
                    .Where(h => h.UserGaugeHeadId == dto.UserGaugeHeadId && h.ActorCompanyId == actorCompanyId)
                    .FirstOrDefault();

                SetModifiedProperties(head);
            }
            if (head == null)
            {
                head = new UserGaugeHead();
                entities.UserGaugeHead.AddObject(head);
                SetCreatedProperties(head);
                head.UserId = userId;
                head.ActorCompanyId = actorCompanyId;
            }

            head.Name = dto.Name;
            head.Priority = dto.Priority;
            head.Description = dto.Description;
            head.Module = (int)dto.Module;

            var result = SaveChanges(entities);
            result.IntegerValue = head.UserGaugeHeadId;

            return result;
        }
        public bool HasGaugePermission(UserGauge gauge, int roleId, int actorCompanyId, int licenseId)
        {
            SysGauge sysGauge = GetSysGauge(gauge.SysGaugeId);
            if (sysGauge != null)
            {
                // Check permission
                if (!FeatureManager.HasRolePermission((Feature)sysGauge.SysFeatureId, Permission.Readonly, roleId, actorCompanyId, licenseId))
                    return false;

                // Get gauge name
                gauge.SysGaugeName = sysGauge.GaugeName;

                return true;
            }
            return false;
        }
        public int GetPrioritizedUserGaugeHeadId(CompEntities entities, int actorCompanyId, int userId, int roleId)
        {
            return entities.UserGaugeHead
                .Where(g => g.UserId == userId && g.ActorCompanyId == actorCompanyId)
                .OrderBy(g => g.Priority)
                .Select(g => g.UserGaugeHeadId)
                .FirstOrDefault();
        }

        public UserGaugeHeadDTO CreateTemporaryUserGaugeHead(List<UserGaugeDTO> userGauges)
        {
            var head = new UserGaugeHeadDTO()
            {
                UserGauges = userGauges
            };

            return head;
        }

        public UserGauge GetUserGauge(int userGaugeId, bool loadSettings)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserGauge.NoTracking();
            return GetUserGauge(entities, userGaugeId, loadSettings);
        }

        public UserGauge GetUserGauge(CompEntities entities, int userGaugeId, bool loadSettings)
        {
            UserGauge userGauge = (from g in entities.UserGauge
                                   where g.UserGaugeId == userGaugeId
                                   select g).FirstOrDefault();

            if (userGauge != null)
            {
                if (loadSettings && !userGauge.UserGaugeSetting.IsLoaded)
                    userGauge.UserGaugeSetting.Load();

                // Get gauge name
                SysGauge sysGauge = GetSysGauge(userGauge.SysGaugeId);
                if (sysGauge != null)
                    userGauge.SysGaugeName = sysGauge.GaugeName;
            }

            return userGauge;
        }

        public ActionResult AddUserGauge(UserGaugeDTO userGaugeInput)
        {
            if (userGaugeInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "UserGauge");

            using (CompEntities entities = new CompEntities())
            {
                UserGauge userGauge = new UserGauge()
                {
                    ActorCompanyId = userGaugeInput.ActorCompanyId,
                    UserGaugeHeadId = userGaugeInput.UserGaugeHeadId,
                    RoleId = userGaugeInput.RoleId,
                    UserId = userGaugeInput.UserId,
                    SysGaugeId = userGaugeInput.SysGaugeId,
                    Module = (int)userGaugeInput.Module,
                    Sort = userGaugeInput.Sort,
                    WindowState = userGaugeInput.WindowState
                };
                ActionResult result = AddEntityItem(entities, userGauge, "UserGauge");
                if (!result.Success)
                    return result;

                // Get SysGauge
                SysGauge sysGauge = GetSysGauge(userGauge.SysGaugeId);

                result.IntegerValue = userGauge.UserGaugeId;
                result.StringValue = sysGauge.Name;
                return result;
            }
        }

        public ActionResult UpdateUserGaugeSettings(int userGaugeId, List<UserGaugeSettingDTO> settings)
        {
            if (userGaugeId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "UserGauge");

            using (CompEntities entities = new CompEntities())
            {
                UserGauge userGauge = GetUserGauge(entities, userGaugeId, true);
                if (userGauge == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "UserGauge");

                // Add or update settings
                foreach (var setting in settings)
                {
                    UserGaugeSetting originalSetting = userGauge.UserGaugeSetting.FirstOrDefault(s => s.Name == setting.Name);
                    if (originalSetting == null)
                    {
                        // Add new setting
                        originalSetting = new UserGaugeSetting()
                        {
                            UserGaugeId = userGaugeId
                        };
                        entities.UserGaugeSetting.AddObject(originalSetting);
                    }

                    // Update setting
                    originalSetting.DataType = setting.DataType;
                    originalSetting.Name = setting.Name;
                    originalSetting.StrData = setting.StrData;
                    originalSetting.IntData = setting.IntData;
                    originalSetting.DecimalData = setting.DecimalData;
                    originalSetting.BoolData = setting.BoolData;
                    originalSetting.DateData = setting.DateData;
                    originalSetting.TimeData = setting.TimeData;

                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                        return result;
                }

                // Delete settings
                foreach (var setting in userGauge.UserGaugeSetting.ToList())
                {
                    if (!settings.Any(s => s.Name == setting.Name))
                    {
                        ActionResult settingResult = DeleteEntityItem(entities, setting);
                        if (!settingResult.Success)
                            return settingResult;
                    }
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult DeleteUserGauge(int userGaugeId)
        {
            if (userGaugeId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "UserGauge");

            using (CompEntities entities = new CompEntities())
            {
                UserGauge userGauge = GetUserGauge(entities, userGaugeId, true);
                if (userGauge == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "UserGauge");

                // Delete settings
                foreach (var setting in userGauge.UserGaugeSetting.ToList())
                {
                    ActionResult settingResult = DeleteEntityItem(entities, setting);
                    if (!settingResult.Success)
                        return settingResult;
                }

                // Delete user gauge
                ActionResult result = DeleteEntityItem(entities, userGauge);
                if (result.Success)
                    result.IntegerValue = userGaugeId;

                return result;
            }
        }

        public ActionResult SaveUserGaugeSort(int userGaugeId, int sort)
        {
            if (userGaugeId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "UserGauge");

            using (CompEntities entities = new CompEntities())
            {
                UserGauge userGauge = GetUserGauge(entities, userGaugeId, true);
                if (userGauge == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "UserGauge");

                userGauge.Sort = sort;
                return SaveChanges(entities);
            }
        }

        #endregion

        #region Gauges

        #region AttestFlowGauge

        public IEnumerable<AttestFlowGaugeDTO> GetAttestFlowInvoices(int actorCompanyId)
        {
            var invoices = SupplierInvoiceManager.GetAttestWorkFlowOverview(SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyActive, TermGroup_ChangeStatusGridAllItemsSelection.All);

            if (invoices.Any())
            {

                Dictionary<int, string> attestStatesDict = AttestManager.GetAttestStatesDict(actorCompanyId, TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy, false, false);

                foreach (var invoice in invoices)
                {
                    yield return new AttestFlowGaugeDTO
                    {
                        InvoiceId = invoice.InvoiceId,
                        InvoiceNr = invoice.InvoiceNr,
                        InvoiceDate = invoice.InvoiceDate,
                        DueDate = invoice.DueDate,
                        Amount = invoice.TotalAmount,
                        SupplierName = invoice.SupplierName,
                        AttestStateId = invoice.AttestStateId.Value,
                        AttestStateName = invoice.AttestStateId.HasValue && attestStatesDict.ContainsKey(invoice.AttestStateId.Value) ? attestStatesDict[invoice.AttestStateId.Value] : string.Empty
                    };
                }
            }
        }

        #endregion

        #region EmployeeRequestsGauge

        /// <summary>
        /// Return open employee requests
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Collection of EmployeeRequestsGaugeDTO</returns>
        public IEnumerable<EmployeeRequestsGaugeDTO> GetEmployeeRequests(int actorCompanyId, int userId, int roleId, bool setEmployeeRequestTypeNames)
        {
            List<EmployeeRequestsGaugeDTO> dtos = new List<EmployeeRequestsGaugeDTO>();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeRequest.NoTracking();
            List<EmployeeRequest> requests = (from r in entities.EmployeeRequest
                                                .Include("Employee.ContactPerson").Include("TimeDeviationCause")
                                              where r.ActorCompanyId == actorCompanyId &&
                                              r.Status != (int)TermGroup_EmployeeRequestStatus.Definate &&
                                              r.Stop >= DateTime.Today.Date &&
                                              r.State == (int)SoeEntityState.Active &&
                                              (
                                                  r.Type == (int)TermGroup_EmployeeRequestType.AbsenceRequest ||
                                                  r.Type == (int)TermGroup_EmployeeRequestType.InterestRequest ||
                                                  r.Type == (int)TermGroup_EmployeeRequestType.NonInterestRequest
                                              )
                                              orderby r.Start, r.Created
                                              select r).ToList();

            if (requests.Count == 0)
                return dtos;

            List<Employee> employees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId);
            foreach (EmployeeRequest request in requests)
            {
                // Only return requests from employees within current users attest role
                if (!employees.Any(e => e.EmployeeId == request.EmployeeId))
                    continue;

                EmployeeRequestsGaugeDTO dto = new EmployeeRequestsGaugeDTO()
                {
                    RequestId = request.EmployeeRequestId,
                    AppliedDate = request.Created ?? CalendarUtility.DATETIME_DEFAULT,
                    EmployeeId = request.EmployeeId,
                    EmployeeName = request.Employee.ContactPerson.FirstName + " " + request.Employee.ContactPerson.LastName,
                    Start = request.Start,
                    Status = request.Status,
                    StatusName = GetText(request.Status, (int)TermGroup.EmployeeRequestStatus),
                    Stop = request.Stop,
                    TimeDeviationCauseId = request.TimeDeviationCause != null ? request.TimeDeviationCause.TimeDeviationCauseId : 0,
                    TimeDeviationCauseName = request.TimeDeviationCause != null ? request.TimeDeviationCause.Name : string.Empty,
                    EmployeeRequestType = (TermGroup_EmployeeRequestType)request.Type,
                };

                if (setEmployeeRequestTypeNames)
                {
                    switch (dto.EmployeeRequestType)
                    {
                        case TermGroup_EmployeeRequestType.AbsenceRequest:
                            dto.EmployeeRequestTypeName = GetText(3965, "Ledighetsansökan");
                            break;
                        case TermGroup_EmployeeRequestType.InterestRequest:
                            dto.EmployeeRequestTypeName = GetText(3913, "Vill jobba");
                            break;
                        case TermGroup_EmployeeRequestType.NonInterestRequest:
                            dto.EmployeeRequestTypeName = GetText(3914, "Kan inte jobba");
                            break;
                    }
                }

                dtos.Add(dto);
            }

            return dtos.OrderBy(d => d.EmployeeName).ThenBy(d => d.Start).ThenBy(d => d.Stop);
        }

        #endregion

        #region MapGauge

        public string GetMapStartAddress(int actorCompanyId)
        {
            string address = null;
            var items = ContactManager.GetContactAddressItems(actorCompanyId);
            ContactAddressItem item = items.FirstOrDefault(i => i.ContactAddressItemType == ContactAddressItemType.AddressVisiting);
            if (item != null)
                address = item.DisplayAddress;

            // Distribution address (Utdelning)
            if (address == null)
            {
                item = items.FirstOrDefault(i => i.ContactAddressItemType == ContactAddressItemType.AddressDistribution);
                if (item != null)
                    address = item.DisplayAddress;
            }

            // Billing address (Faktura)
            if (address == null)
            {
                item = items.FirstOrDefault(i => i.ContactAddressItemType == ContactAddressItemType.AddressBilling);
                if (item != null)
                    address = item.DisplayAddress;
            }

            return address;
        }

        public IEnumerable<MapGaugeDTO> GetMapLocations(int actorCompanyId, DateTime? dateFrom)
        {
            List<MapGaugeDTO> dtos = new List<MapGaugeDTO>();
            if (!dateFrom.HasValue)
                return dtos;

            IEnumerable<MapLocation> locations = GraphicsManager.GetMapLocations(actorCompanyId, MapLocationType.GPSLocation, SoeEntityType.User, true);
            foreach (MapLocation location in locations.Where(l => l.TimeStamp >= dateFrom.Value.Date))
            {
                dtos.Add(new MapGaugeDTO()
                {
                    ID = location.RecordId,
                    Name = ContactManager.GetContactPersonName(location.RecordId),
                    Longitude = location.Longitude,
                    Latitude = location.Latitude,
                    TimeStamp = location.TimeStamp
                });
            }

            return dtos;
        }

        public IEnumerable<OrderMapDTO> GetPlannedOrderMaps(int actorCompanyId, int roleId, DateTime? inDate)
        {
            if (!inDate.HasValue)
            {
                return new List<OrderMapDTO>();
            }
            var date = inDate.Value;

            // Get current employee
            Employee employee = EmployeeManager.GetEmployeeByUser(actorCompanyId, base.UserId);

            // Get employees current user is allowed to see
            List<int> employeeIds = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, base.UserId, roleId).Select(e => e.EmployeeId).ToList();

            // Get planned shifts for specified date
            var shifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(actorCompanyId, base.UserId, employee != null ? employee.EmployeeId : 0, roleId, date, date, employeeIds, TimeSchedulePlanningMode.OrderPlanning, TimeSchedulePlanningDisplayMode.Admin, false, false, false, includePreliminary: false, includeAbsenceRequest: false);

            return InvoiceManager.GetOrderMaps(shifts.Where(s => s.IsOrder()).Select(s => s.Order.OrderId).ToList(), date, date);
        }

        #endregion

        #region MyScheduleGauge

        public List<TimeSchedulePlanningDayDTO> GetMySchedule(int actorCompanyId, int userId, int roleId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(actorCompanyId, userId, employeeId, roleId, dateFrom, dateTo, new List<int>() { employeeId }, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, true, true, false, includePreliminary: false, includeAbsenceRequest: true, checkToIncludeDeliveryAdress: false);

            //List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();

            //// Get current employee
            //Employee currentEmployee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, loadEmployment: true, loadContactPerson: true);
            //if (currentEmployee != null)
            //{
            //    // Check if employee has any active employment
            //    Employment employment = currentEmployee.GetEmployment(dateFrom, dateTo);
            //    if (employment != null)
            //        shifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(actorCompanyId, userId, employeeId, roleId, dateFrom, dateTo, new List<int>() { employeeId }, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, true, true, false, includePreliminary: false, includeAbsenceRequest: true, checkToIncludeDeliveryAdress: false);
            //}

            //return shifts;
        }

        public List<TimeSchedulePlanningDayDTO> GetOpenShifts(int actorCompanyId, int userId, int roleId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(actorCompanyId));
            return TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(actorCompanyId, userId, employeeId, roleId, dateFrom, dateTo, new List<int>() { hiddenEmployeeId }, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, true, true, false, includePreliminary: false, includeAbsenceRequest: false, checkToIncludeDeliveryAdress: false);

            //List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();

            //// Get current employee
            //Employee currentEmployee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, loadEmployment: true, loadContactPerson: true);
            //if (currentEmployee != null)
            //{
            //    // Check if employee has any active employment
            //    Employment employment = currentEmployee.GetEmployment(dateFrom, dateTo);
            //    if (employment != null)
            //    {
            //        using var entities = CompEntitiesFactory.CreateReadOnly();
            //int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(actorCompanyId));
            //        shifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(actorCompanyId, userId, employeeId, roleId, dateFrom, dateTo, new List<int>() { hiddenEmployeeId }, TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, true, true, false, includePreliminary: false, includeAbsenceRequest: false, checkToIncludeDeliveryAdress: false);

            //    }
            //}

            //return shifts;
        }

        public List<TimeSchedulePlanningDayDTO> GetMyColleaguesSchedule(int actorCompanyId, int userId, int roleId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            Dictionary<int, string> employeesDict = GetMyColleagues(actorCompanyId, userId, roleId, employeeId, dateFrom, dateTo);
            return TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(actorCompanyId, userId, employeeId, roleId, dateFrom, dateTo, employeesDict.Keys.ToList(), TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, true, true, false, includePreliminary: false, includeAbsenceRequest: false, checkToIncludeDeliveryAdress: false);

            //List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();

            //Dictionary<int, string> employeesDict = GetMyColleagues(actorCompanyId, userId, roleId, employeeId, dateFrom, dateTo);
            //if (employeesDict.Count > 0)
            //    shifts = TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(actorCompanyId, userId, employeeId, roleId, dateFrom, dateTo, employeesDict.Keys.ToList(), TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.User, true, true, false, includePreliminary: false, includeAbsenceRequest: false, checkToIncludeDeliveryAdress: false);

            //return shifts;
        }

        private Dictionary<int, string> GetMyColleagues(int actorCompanyId, int userId, int roleId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            Dictionary<int, string> employeesDict = new Dictionary<int, string>();

            if (employeeId == 0)
                return employeesDict;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            // Get current employee
            Employee currentEmployee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, loadContactPerson: true);
            //Employee currentEmployee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, loadEmployment: true, loadContactPerson: true);

            //// Check if employee has any active employment
            //Employment employment = currentEmployee.GetEmployment(dateFrom, dateTo);
            //if (employment == null)
            //    return employeesDict;

            // Account hierarchy
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
            if (useAccountHierarchy)
            {
                #region AccountHierarchy

                List<int> employeeIds = (from e in entitiesReadOnly.Employee
                                         where e.ActorCompanyId == actorCompanyId &&
                                         !e.Hidden &&
                                         !e.Vacant &&
                                         e.State == (int)SoeEntityState.Active
                                         select e.EmployeeId).ToList();

                employeeIds = EmployeeManager.GetValidEmployeeByAccountHierarchy(entitiesReadOnly, actorCompanyId, roleId, userId, employeeIds, currentEmployee, dateFrom, dateTo, getHidden: true, useShowOtherEmployeesPermission: true, onlyDefaultAccounts: false, ignoreAttestRoles: true);
                List<Employee> employees = (from e in entitiesReadOnly.Employee
                                            .Include("ContactPerson")
                                            .Include("EmployeeAccount.Children")
                                            where e.ActorCompanyId == actorCompanyId &&
                                            employeeIds.Contains(e.EmployeeId) &&
                                            e.EmployeeId != employeeId
                                            select e).ToList();

                foreach (var employee in employees)
                {
                    bool isValid = false;

                    // GetValidEmployeeByAccountHierarchy only checks accounts not dates
                    // Check that employee account dates are within specified range
                    if (!employee.EmployeeAccount.IsNullOrEmpty())
                    {
                        foreach (EmployeeAccount account in employee.EmployeeAccount)
                        {
                            if (EmployeeManager.IsEmployeeAccountValid(account, dateFrom, dateTo))
                            {
                                isValid = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        isValid = true;
                    }

                    if (isValid)
                        employeesDict.Add(employee.EmployeeId, employee.EmployeeNrAndName);
                }

                #endregion
            }
            else
            {
                #region Categories

                if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_SeeOtherEmployeesShifts, Permission.Readonly, roleId, actorCompanyId))
                {
                    Dictionary<int, string> categories = CategoryManager.GetCategoriesForRoleFromTypeDict(actorCompanyId, userId, employeeId, SoeCategoryType.Employee, false, true, false, dateFrom, dateTo);
                    employeesDict.AddRange(EmployeeManager.GetEmployeesDictByCategories(actorCompanyId, categories.Select(x => x.Key).ToList(), false));
                }

                #endregion
            }

            if (employeesDict.Count > 1 && employeesDict.ContainsKey(employeeId))
                employeesDict.Remove(employeeId);
            else if (!employeesDict.Any() && currentEmployee != null)
                employeesDict.Add(employeeId, currentEmployee.EmployeeNrAndName);

            return employeesDict;
        }

        #endregion

        #region MyShiftsGauge

        /// <summary>
        /// Return all shifts for specified employee between specified dates.
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dateFrom">Date range from</param>
        /// <param name="dateTo">Date range to</param>
        /// <returns>Collection of MyShiftsGaugeDTO</returns>
        public IEnumerable<MyShiftsGaugeDTO> GetMyShifts(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeScheduleEmployeePeriod.NoTracking();
            List<TimeScheduleTemplateBlock> blocks = (from tb in entities.TimeScheduleTemplateBlock
                                                        .Include("ShiftType")
                                                      where tb.EmployeeId == employeeId &&
                                                      tb.TimeScheduleEmployeePeriod != null &&
                                                      !tb.IsPreliminary &&
                                                     (tb.Date.HasValue && tb.Date.Value >= dateFrom && tb.Date.Value <= dateTo) &&
                                                     ((tb.StartTime != tb.StopTime && !tb.TimeDeviationCauseId.HasValue) || tb.TimeDeviationCauseId.HasValue) &&
                                                      tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None &&
                                                      tb.State == (int)SoeEntityState.Active
                                                      orderby tb.Date, tb.StartTime, tb.StopTime, tb.ShiftTypeId
                                                      select tb).ToList();

            blocks = blocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();
            // Convert to DTO
            List<MyShiftsGaugeDTO> list = new List<MyShiftsGaugeDTO>();
            foreach (TimeScheduleTemplateBlock block in blocks)
            {
                list.Add(new MyShiftsGaugeDTO()
                {
                    TimeScheduleTemplateBlockId = block.TimeScheduleTemplateBlockId,
                    Date = block.Date ?? CalendarUtility.DATETIME_DEFAULT,
                    Time = String.Format("{0} - {1}", block.StartTime.ToShortTimeString(), block.StopTime.ToShortTimeString()),
                    ShiftTypeId = block.ShiftTypeId ?? 0,
                    ShiftTypeName = block.ShiftTypeId.HasValue ? block.ShiftType.Name : String.Empty,
                    ShiftStatus = (TermGroup_TimeScheduleTemplateBlockShiftStatus)block.ShiftStatus,
                    ShiftStatusName = GetText(block.ShiftStatus, (int)TermGroup.TimeScheduleEmployeePeriodStatus),
                    ShiftUserStatus = (TermGroup_TimeScheduleTemplateBlockShiftUserStatus)block.ShiftUserStatus,
                    ShiftUserStatusName = GetText(block.ShiftUserStatus, (int)TermGroup.TimeScheduleEmployeePeriodUserStatus),
                });
            }

            return list;
        }

        #endregion

        #region OpenShiftsGauge

        /// <summary>
        /// Return all open or unwanted shifts between specified dates.
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="dateFrom">Date range from</param>
        /// <param name="dateTo">Date range to</param>
        /// <returns>Collection of OpenShiftsGaugeDTO</returns>
        public IEnumerable<OpenShiftsGaugeDTO> GetOpenShiftsForGauge(int actorCompanyId, int roleId, int userId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(actorCompanyId));

            // Check if user has attestroles
            TimeSchedulePlanningDisplayMode displayMode = TimeSchedulePlanningDisplayMode.User;
            List<AttestRole> attestRoles = AttestManager.GetAttestRolesForUser(actorCompanyId, userId, DateTime.Today, SoeModule.Time);
            // Set as admin where count > 0
            if (attestRoles.Any())
                displayMode = TimeSchedulePlanningDisplayMode.Admin;

            List<int> colleagues = new List<int>();
            if (displayMode == TimeSchedulePlanningDisplayMode.User)
                colleagues = GetMyColleagues(actorCompanyId, userId, roleId, employeeId, dateFrom, dateTo).Keys.ToList();

            entities.TimeScheduleEmployeePeriod.NoTracking();
            List<TimeScheduleTemplateBlock> blocks = (from tb in entities.TimeScheduleTemplateBlock
                                                          .Include("ShiftType")
                                                          .Include("TimeScheduleTemplateBlockQueue")
                                                      where tb.Employee != null && tb.Employee.ActorCompanyId == actorCompanyId &&
                                                      tb.Employee.State == (int)SoeEntityState.Active &&
                                                      (!colleagues.Any() || colleagues.Contains(tb.EmployeeId.Value) || tb.EmployeeId == hiddenEmployeeId) &&
                                                      tb.TimeScheduleEmployeePeriod != null &&
                                                      (tb.Date >= dateFrom && tb.Date <= dateTo) &&
                                                      (tb.StartTime != tb.StopTime) &&
                                                      tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None &&
                                                      (tb.ShiftStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Open || tb.ShiftUserStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted) &&
                                                      !tb.IsPreliminary &&
                                                      tb.State == (int)SoeEntityState.Active
                                                      orderby tb.Date, tb.StartTime, tb.StopTime, tb.ShiftTypeId
                                                      select tb).ToList();

            blocks = blocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();

            // This filter is performed after the actual SQL query.
            // The reason is that if there are a lot of shift types, the query will be too complex and will crash.
            if (blocks.Any())
            {
                List<int> shiftTypeIds = TimeScheduleManager.GetShiftTypeIdsForUser(null, actorCompanyId, roleId, userId, employeeId, displayMode == TimeSchedulePlanningDisplayMode.Admin, dateFrom, dateTo, true);
                blocks = blocks.Where(b => (b.ShiftTypeId.HasValue && shiftTypeIds.Contains(b.ShiftTypeId.Value)) || !b.ShiftTypeId.HasValue).ToList();
            }

            List<int> validAccountIds = null;
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
            if (useAccountHierarchy)
            {
                if (displayMode == TimeSchedulePlanningDisplayMode.Admin)
                    validAccountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(actorCompanyId, roleId, userId, dateFrom, dateTo);
                else
                    validAccountIds = AccountManager.GetValidAccountIdsForEmployee(actorCompanyId, employeeId, dateFrom, dateTo, false, true, false);
            }

            // Convert to DTO
            List<OpenShiftsGaugeDTO> list = new List<OpenShiftsGaugeDTO>();
            foreach (TimeScheduleTemplateBlock block in blocks)
            {
                if (block.ShiftStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Open && block.EmployeeId != hiddenEmployeeId)
                    continue;

                // If hidden employee, only return shifts valid for current user
                if (validAccountIds != null && block.EmployeeId == hiddenEmployeeId && (!block.AccountId.HasValue || !validAccountIds.Contains(block.AccountId.Value)))
                    continue;

                list.Add(new OpenShiftsGaugeDTO()
                {
                    TimeScheduleTemplateBlockId = block.TimeScheduleTemplateBlockId,
                    Date = block.Date ?? CalendarUtility.DATETIME_DEFAULT,
                    Time = String.Format("{0} - {1}", block.StartTime.ToShortTimeString(), block.StopTime.ToShortTimeString()),
                    ShiftTypeId = block.ShiftTypeId.HasValue ? block.ShiftTypeId.Value : 0,
                    ShiftTypeName = block.ShiftTypeId.HasValue ? block.ShiftType.Name : String.Empty,
                    OpenType = (block.ShiftStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Open ? 1 : 2),
                    OpenTypeName = String.Empty,
                    NbrInQueue = block.TimeScheduleTemplateBlockQueue.Count,
                    IamInQueue = block.TimeScheduleTemplateBlockQueue.Where(w => w.State == (int)SoeEntityState.Active).Any(q => q.EmployeeId == employeeId),
                    Link = block.Link
                });
            }


            // Order shifts so linked shifts are together
            List<OpenShiftsGaugeDTO> result = new List<OpenShiftsGaugeDTO>();
            foreach (OpenShiftsGaugeDTO shift in list.ToList())
            {
                if (!string.IsNullOrEmpty(shift.Link))
                {
                    List<OpenShiftsGaugeDTO> linkedShifts = list.Where(s => s.Link == shift.Link).OrderBy(s => s.Date).ThenBy(s => s.Time).ToList();
                    result.AddRange(linkedShifts);
                    list.RemoveRange(linkedShifts);
                }
                else
                {
                    result.Add(shift);
                    list.Remove(shift);
                }
            }

            return result;
        }

        #endregion

        #region PerformanceAnalyzerGauge

        private readonly string sysLogErrorKey = "sysLogErrorKey";

        public DashboardStatisticsDTO GetPerformanceTestResults(string dashboardStatisticTypeKey, DateTime dateFrom, DateTime dateTo, TermGroup_PerformanceTestInterval interval)
        {
            if (dashboardStatisticTypeKey != sysLogErrorKey)
            {
                DashboardStatisticsDTO dto = new DashboardStatisticsDTO();
                bool isTestCase = dashboardStatisticTypeKey.ToLower().Contains("selenium");
                int statusServiceTypeId = GetIdFromKey(dashboardStatisticTypeKey);

                List<StatusResultAggregatedDTO> statusResultAggregatedDTO;
                if (!isTestCase)
                {
                    statusResultAggregatedDTO = SoftOneStatusConnector.GetStatusResultAggregates(statusServiceTypeId, CalendarUtility.GetBeginningOfDay(dateFrom), CalendarUtility.GetEndOfDay(dateTo));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Average", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Median", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Min", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Max", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Percential10", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Percential90", statusResultAggregatedDTO, interval));
                }
                else
                {
                    statusResultAggregatedDTO = SoftOneStatusConnector.GetStatusResultAggregatesFromTestCaseId(statusServiceTypeId, CalendarUtility.GetBeginningOfDay(dateFrom), CalendarUtility.GetEndOfDay(dateTo));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Average", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Median", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Min", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Max", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Percential10", statusResultAggregatedDTO, interval));
                    dto.DashboardStatisticRows.Add(CreateDashboardStatisticRowDTO("Percential90", statusResultAggregatedDTO, interval));
                }

                return dto;
            }
            else
            {
                dateFrom = interval == TermGroup_PerformanceTestInterval.Hour ? new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, dateFrom.Hour, 0, 0) : dateFrom.Date;
                string sql = "";

                var dto = new DashboardStatisticRowDTO("Average", DashboardStatisticsRowType.TimeValue, new List<DashboardStatisticPeriodDTO>());

                if (interval == TermGroup_PerformanceTestInterval.Day)
                {
                    sql = $"SELECT CONVERT(date, sl.Date) AS[date_truncated], COUNT(*) AS[records_in_interval] FROM syslog  AS sl where sl.Date BETWEEN '{CalendarUtility.ToSqlFriendlyDateTime(dateFrom)}' AND '{CalendarUtility.ToSqlFriendlyDateTime(dateTo)}' GROUP BY CONVERT(date, sl.Date) ORDER BY[date_truncated]";
                }
                else
                {
                    sql = $"SELECT   DATEADD(HOUR, DATEDIFF(HOUR, '2000', sl.[date])/1, '2000') AS [date_truncated], COUNT(*) AS[records_in_interval] FROM syslog AS sl where  sl.Date BETWEEN '{CalendarUtility.ToSqlFriendlyDateTime(dateFrom)}' AND '{CalendarUtility.ToSqlFriendlyDateTime(dateTo)}' GROUP BY  DATEADD(HOUR, DATEDIFF(HOUR, '2000', sl.[date]) / 1, '2000') ORDER BY[date_truncated]";
                }

                using (SqlConnection sqlConnection = new SqlConnection(FrownedUponSQLClient.GetADOConnectionStringForSysEntities()))
                {
                    var reader = FrownedUponSQLClient.ExcuteQuery(sqlConnection, sql, 60);

                    if (reader != null)
                    {
                        while (reader.Read())
                        {

                            dto.DashboardStatisticPeriods.Add(
                                new DashboardStatisticPeriodDTO(DashboardStatisticsRowType.TimeValue,
                                (DateTime)reader["date_truncated"],
                                interval == TermGroup_PerformanceTestInterval.Hour ? ((DateTime)reader["date_truncated"]).AddHours(1).AddMilliseconds(-1) : ((DateTime)reader["date_truncated"]).AddDays(1).AddMilliseconds(-1),
                                 (int)reader["records_in_interval"]));
                        }
                    }

                    DateTime start = dateFrom;
                    DateTime current = start;
                    DateTime stop = dateTo;

                    while (current <= stop)
                    {
                        if (!dto.DashboardStatisticPeriods.Any(c => c.From == current))
                            dto.DashboardStatisticPeriods.Add(
                             new DashboardStatisticPeriodDTO(DashboardStatisticsRowType.TimeValue,
                             current,
                             interval == TermGroup_PerformanceTestInterval.Hour ? current.AddHours(1).AddMilliseconds(-1) : current.AddDays(1).AddMilliseconds(-1),
                             0));

                        current = current.AddMinutes(interval == TermGroup_PerformanceTestInterval.Hour ? 60 : 60 * 24);
                    }

                    dto.DashboardStatisticPeriods = dto.DashboardStatisticPeriods.OrderBy(o => o.From).ToList();
                }

                DashboardStatisticsDTO dashboardStatisticsDTO = new DashboardStatisticsDTO();
                dashboardStatisticsDTO.DashboardStatisticRows.Add(dto);
                return dashboardStatisticsDTO;
            }

        }

        public DashboardStatisticRowDTO CreateDashboardStatisticRowDTO(string propName, List<StatusResultAggregatedDTO> statusResultAggregatedDTO, TermGroup_PerformanceTestInterval interval)
        {
            DashboardStatisticRowDTO dto = new DashboardStatisticRowDTO(propName, DashboardStatisticsRowType.TimeValue, new List<DashboardStatisticPeriodDTO>());

            if (statusResultAggregatedDTO == null)
                return new DashboardStatisticRowDTO("", DashboardStatisticsRowType.TimeValue, new List<DashboardStatisticPeriodDTO>());

            foreach (var aggregatedDTO in statusResultAggregatedDTO.OrderBy(o => o.From).GroupBy(g => interval == TermGroup_PerformanceTestInterval.Hour ? g.From : g.From.Date))
            {
                decimal value = 0;
                var first = aggregatedDTO.First();

                if (interval == TermGroup_PerformanceTestInterval.Hour)
                {
                    value = (decimal)first.GetType().GetProperty(propName).GetValue(first);
                }
                else
                {
                    if (value.Equals("Min"))
                        value = aggregatedDTO.OrderBy(f => f.Min).First().Min;
                    if (value.Equals("Max"))
                        value = aggregatedDTO.OrderBy(f => f.Min).Last().Max;
                    else
                    {
                        foreach (var row in aggregatedDTO.ToList())
                            value += (decimal)row.GetType().GetProperty(propName).GetValue(row);

                        value = value / aggregatedDTO.Count();
                    }
                }

                dto.DashboardStatisticPeriods.Add(
                new DashboardStatisticPeriodDTO(DashboardStatisticsRowType.TimeValue,
                interval == TermGroup_PerformanceTestInterval.Hour ? first.From : first.From.Date,
                interval == TermGroup_PerformanceTestInterval.Hour ? first.To : CalendarUtility.GetEndOfDay(first.From),
                value));
            }
            return dto;
        }

        public DashboardStatisticType GetDashboardStatisticType(string dashBoardStatisticTypeKey)
        {
            List<DashboardStatisticType> allTypes = GetDashboardStatisticTypes(SoftOne.Status.Shared.ServiceType.Unknown);
            return allTypes.FirstOrDefault(f => f.Key == dashBoardStatisticTypeKey);
        }

        public List<DashboardStatisticType> GetDashboardStatisticTypes(SoftOne.Status.Shared.ServiceType serviceType)
        {
            string cachekey = $"GetDashboardStatisticTypes#{serviceType}";
            var fromcache = BusinessMemoryCache<List<DashboardStatisticType>>.Get(cachekey);
            if (!fromcache.IsNullOrEmpty())
                return fromcache;

            List<DashboardStatisticType> statisticTypes = new List<DashboardStatisticType>();
            var testCases = GetTestCases(TestCaseType.Selenium);

            if (serviceType != SoftOne.Status.Shared.ServiceType.Selenium)
            {
                var types = GetStatusServiceTypes();

                foreach (var type in types)
                {
                    statisticTypes.Add(new DashboardStatisticType()
                    {
                        DashboardStatisticsType = DashboardStatisticsType.PerformanceAnalyzer,
                        Key = type.StatusActionJobSetting != null ? GetKey(type.StatusServiceTypeId, type.StatusActionJobSetting.Domain, type.StatusActionJobSetting.SeleniumType) : GetKey(type.StatusServiceTypeId, "", SeleniumType.Unknown),
                        Name = type.StatusActionJobSetting != null ? $"{type.StatusActionJobSetting.SeleniumType}#{type.StatusActionJobSetting.Domain}" : type.StatusServiceDTO.Name + type.ServiceType,
                        Decription = string.Empty
                    });

                    if (serviceType != SoftOne.Status.Shared.ServiceType.Selenium && !statisticTypes.Any(a => a.Key == sysLogErrorKey))
                    {
                        statisticTypes.Add(new DashboardStatisticType()
                        {
                            DashboardStatisticsType = DashboardStatisticsType.PerformanceAnalyzer,
                            Key = sysLogErrorKey,
                            Name = "SysLog Errors",
                            Decription = string.Empty
                        });
                    }
                }
            }
            else if (testCases != null)
            {
                foreach (var testCase in testCases)
                {
                    statisticTypes.Add(new DashboardStatisticType()
                    {
                        DashboardStatisticsType = DashboardStatisticsType.PerformanceAnalyzer,
                        Key = GetKey(testCase.TestCaseId, testCase.Name, testCase.SeleniumType),
                        Name = testCase.SeleniumType + "#" + testCase.Name,
                        Decription = testCase.Description
                    });
                }
            }

            BusinessMemoryCache<List<DashboardStatisticType>>.Set(cachekey, statisticTypes, 60 * 10);
            return statisticTypes;
        }

        private string GetKey(int id, string domain, SeleniumType seleniumType)
        {
            if (seleniumType != SeleniumType.Unknown)
                return $"selenium_{id}_{seleniumType}_{domain}_";
            else
                return $"serviceTypeId_{id}___";
        }

        private int GetIdFromKey(string dashBoardStatisticTypeKey)
        {
            var values = dashBoardStatisticTypeKey.Split('_');
            int.TryParse(values[1], out int value);
            return value;
        }

        private List<StatusServiceTypeDTO> GetStatusServiceTypes(SoftOne.Status.Shared.ServiceType serviceType = SoftOne.Status.Shared.ServiceType.Unknown)
        {
            if (serviceType == SoftOne.Status.Shared.ServiceType.Unknown)
                return SoftOneStatusConnector.GetStatusServiceTypes();
            else
                return SoftOneStatusConnector.GetStatusServiceTypes(serviceType);
        }

        private List<TestCaseDTO> GetTestCases(TestCaseType testCaseType = TestCaseType.Unknown)
        {
            return SoftOneStatusConnector.GetTestCases(testCaseType);
        }

        #endregion

        #region TaskWatchLogGauge

        public List<string> GetTaskWatchLogTasks(DateTime dateFrom, DateTime dateTo, int? actorCompanyId, int? userId)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TaskWatchLog.NoTracking();
            List<string> logs = (from t in entities.TaskWatchLog
                                 where DbFunctions.TruncateTime(t.Start) >= dateFrom && DbFunctions.TruncateTime(t.Stop) <= dateTo &&
                                 (!actorCompanyId.HasValue || t.ActorCompanyId == actorCompanyId) &&
                                 (!userId.HasValue || t.UserId == userId)
                                 select t.Name).Distinct().ToList();

            return logs;
        }

        public DashboardStatisticsDTO GetTaskWatchLogResult(string task, DateTime dateFrom, DateTime dateTo, TermGroup_PerformanceTestInterval interval, TermGroup_TaskWatchLogResultCalculationType calculationType, int? actorCompanyId, int? userId)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);
            int intervalMinutes = GetPerformanceTestIntervalMinutes(interval);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TaskWatchLog.NoTracking();
            List<TaskWatchLog> allLogs = (from t in entities.TaskWatchLog
                                          where t.Name == task &&
                                          DbFunctions.TruncateTime(t.Start) >= dateFrom && DbFunctions.TruncateTime(t.Stop) <= dateTo &&
                                          (!actorCompanyId.HasValue || t.ActorCompanyId == actorCompanyId) &&
                                          (!userId.HasValue || t.UserId == userId)
                                          select t).ToList();

            DashboardStatisticsDTO dto = new DashboardStatisticsDTO();
            DashboardStatisticRowDTO minRow = new DashboardStatisticRowDTO("Min", DashboardStatisticsRowType.TimeValue, new List<DashboardStatisticPeriodDTO>());
            dto.DashboardStatisticRows.Add(minRow);
            DashboardStatisticRowDTO maxRow = new DashboardStatisticRowDTO("Max", DashboardStatisticsRowType.TimeValue, new List<DashboardStatisticPeriodDTO>());
            dto.DashboardStatisticRows.Add(maxRow);
            DashboardStatisticRowDTO averageRow = new DashboardStatisticRowDTO("Average", DashboardStatisticsRowType.TimeValue, new List<DashboardStatisticPeriodDTO>());
            dto.DashboardStatisticRows.Add(averageRow);
            DashboardStatisticRowDTO medianRow = new DashboardStatisticRowDTO("Median", DashboardStatisticsRowType.TimeValue, new List<DashboardStatisticPeriodDTO>());
            dto.DashboardStatisticRows.Add(medianRow);

            if (allLogs.Count > 0)
            {
                DateTime currentTime = dateFrom;
                while (currentTime.AddMinutes(intervalMinutes) <= dateTo)
                {
                    IEnumerable<TaskWatchLog> logsInInterval = allLogs.Where(l => l.Start >= currentTime && l.Start < currentTime.AddMinutes(intervalMinutes));

                    List<int> durations = logsInInterval.Select(l => (calculationType == TermGroup_TaskWatchLogResultCalculationType.Record ? l.DurationPerRecord : l.Duration) ?? 0).ToList();
                    if (durations.Count > 0)
                    {
                        // Min
                        int minValue = durations.Min();
                        minRow.DashboardStatisticPeriods.Add(new DashboardStatisticPeriodDTO(DashboardStatisticsRowType.TimeValue, currentTime, currentTime.AddMinutes(intervalMinutes), minValue));

                        // Max
                        int maxValue = durations.Max();
                        maxRow.DashboardStatisticPeriods.Add(new DashboardStatisticPeriodDTO(DashboardStatisticsRowType.TimeValue, currentTime, currentTime.AddMinutes(intervalMinutes), maxValue));

                        // Average
                        int averageValue = Convert.ToInt32(Decimal.Divide(durations.Sum(), durations.Count));
                        averageRow.DashboardStatisticPeriods.Add(new DashboardStatisticPeriodDTO(DashboardStatisticsRowType.TimeValue, currentTime, currentTime.AddMinutes(intervalMinutes), averageValue));

                        // Median
                        int medianValue = durations.OrderBy(l => l).Skip(Convert.ToInt32(Decimal.Divide(durations.Count, 2))).Take(1).FirstOrDefault();
                        medianRow.DashboardStatisticPeriods.Add(new DashboardStatisticPeriodDTO(DashboardStatisticsRowType.TimeValue, currentTime, currentTime.AddMinutes(intervalMinutes), medianValue));
                    }

                    currentTime = currentTime.AddMinutes(intervalMinutes);
                }

            }

            return dto;
        }

        private int GetPerformanceTestIntervalMinutes(TermGroup_PerformanceTestInterval interval)
        {
            int minutes = 0;

            switch (interval)
            {
                case TermGroup_PerformanceTestInterval.Day:
                    minutes = 24 * 60;
                    break;
                case TermGroup_PerformanceTestInterval.Hour:
                    minutes = 60;
                    break;
            }

            return minutes;
        }

        #endregion

        #region TimeStampAttendanceGauge

        public IEnumerable<TimeStampAttendanceGaugeDTO> GetTimeStampAttendance(int actorCompanyId, int userId, int roleId, TermGroup_TimeStampAttendanceGaugeShowMode showMode, bool onlyIn, bool onlyIncludeAttestRoleEmployees = true, bool includeEmployeeNrInString = true, int? timeTerminalId = null, bool includeBreaks = false, bool includeMissingEmployees = false, bool isMobile = false, int? employeeId = null)
        {
            #region Prereq

            bool singleEmployee = employeeId.HasValue;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<EmployeeGroup> employeeGroups = (includeMissingEmployees && !singleEmployee && onlyIncludeAttestRoleEmployees) ? EmployeeManager.GetEmployeeGroups(actorCompanyId) : null;
            List<Employee> employees = !singleEmployee && onlyIncludeAttestRoleEmployees ? EmployeeManager.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, active: true, useShowOtherEmployeesPermission: true, addEmployeeGroupInfo: includeMissingEmployees) : new List<Employee>(0);
            List<int> employeeIds = employees.Any() ? employees.Select(e => e.EmployeeId).ToList() : new List<int>();
            if (singleEmployee)
                employeeIds.Add(employeeId.Value);

            List<int> employeeIdsAlreadyAdded = new List<int>();
            Dictionary<int, string> types = base.GetTermGroupDict(TermGroup.TimeStampEntryType);

            #region Limits

            bool limitByTimeTerminal = false;
            List<int> terminalEmployeeIds = null;
            bool isGrouped = false;
            List<int> groupedTerminalIds = null;
            List<int> employeeIdsIn = new List<int>();

            if (timeTerminalId.HasValue)
            {
                string terminalGroupName = TimeStampManager.GetTimeTerminalStringSetting(TimeTerminalSettingType.TerminalGroupName, timeTerminalId.Value);
                isGrouped = !terminalGroupName.IsNullOrEmpty();
                if (isGrouped)
                {
                    groupedTerminalIds = (from s in entitiesReadOnly.TimeTerminalSetting
                                          where s.TimeTerminal.ActorCompanyId == actorCompanyId &&
                                          s.Type == (int)TimeTerminalSettingType.TerminalGroupName &&
                                          s.StrData == terminalGroupName
                                          select s.TimeTerminal.TimeTerminalId).ToList();
                }

                if (TimeStampManager.GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, timeTerminalId.Value))
                {
                    limitByTimeTerminal = true;
                    List<int> categoryIds = TimeStampManager.GetCategoriesByTimeTerminal(actorCompanyId, timeTerminalId.Value).Select(c => c.CategoryId).ToList();
                    terminalEmployeeIds = EmployeeManager.GetEmployeeIdsByCategoryIds(actorCompanyId, categoryIds);
                }
                else if (TimeStampManager.GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, timeTerminalId.Value))
                {
                    limitByTimeTerminal = true;
                    terminalEmployeeIds = TimeStampManager.GetEmployeeIdsByTimeTerminalAccount(actorCompanyId, timeTerminalId.Value);
                }
            }

            #endregion

            #endregion

            List<TimeStampAttendanceGaugeDTO> result = new List<TimeStampAttendanceGaugeDTO>(); ;
            entitiesReadOnly.TimeStampAttendanceView.NoTracking();
            if (onlyIn)
            {
                var query = (from t in entitiesReadOnly.TimeStampAttendanceView
                             where t.ActorCompanyId == actorCompanyId
                             group t by t.EmployeeId into g
                             let latest = g.OrderByDescending(l => l.Time).ThenByDescending(l => l.Created).ThenByDescending(l => l.TimeStampEntryId).FirstOrDefault()
                             select new
                             {
                                 EmployeeId = g.Key,
                                 EmployeeNr = latest.EmployeeNr,
                                 Time = latest.Time,
                                 FirstName = latest.FirstName,
                                 LastName = latest.LastName,
                                 Type = latest.Type,
                                 TimeTerminalId = latest.TimeTerminalId,
                                 TimeTerminalName = latest.TimeTerminalName,
                                 TimeDeviationCauseName = latest.TimeDeviationCauseName,
                                 AccountName = latest.AccountName,
                                 IsBreak = latest.IsBreak,
                                 IsPaidBreak = latest.IsPaidBreak,
                                 IsDistanceWork = latest.IsDistanceWork
                             });

                if (singleEmployee)
                    query = query.Where(t => t.EmployeeId == employeeId.Value);
                else if (onlyIncludeAttestRoleEmployees)    // Only return entries from employees within current users attest role
                    query = query.Where(t => employeeIds.Contains(t.EmployeeId));

                if (limitByTimeTerminal && !terminalEmployeeIds.IsNullOrEmpty())
                    query = query.Where(t => terminalEmployeeIds.Contains(t.EmployeeId));

                if (showMode == TermGroup_TimeStampAttendanceGaugeShowMode.AllToday)
                    query = query.Where(t => t.Time >= DateTime.Today);

                var times = query.ToList();
                times = times.OrderBy(t => t.FirstName).ThenBy(t => t.LastName).ThenBy(t => t.EmployeeId).ThenBy(t => t.Time).ToList();

                foreach (var time in times)
                {
                    if (timeTerminalId.HasValue && time.TimeTerminalId.HasValue)
                    {
                        // If current terminal is not grouped, only return stamps registered on current terminal
                        // If current terminal is grouped, return stamps registered on all grouped terminals

                        if (!isGrouped && time.TimeTerminalId.Value != timeTerminalId.Value)
                            continue;
                        else if (isGrouped && !groupedTerminalIds.Contains(time.TimeTerminalId.Value))
                            continue;
                    }

                    if (time.Type != (int)TermGroup_TimeStampEntryType.In)
                    {
                        if (!includeBreaks)
                            continue;
                        else if (!time.IsBreak)
                            continue;
                    }

                    string outputName = string.Format("{0} {1}", time.FirstName, time.LastName);
                    if (includeEmployeeNrInString)
                        outputName = string.Format("({0}) {1}", time.EmployeeNr, outputName);

                    TimeStampAttendanceGaugeDTO dto = new TimeStampAttendanceGaugeDTO()
                    {
                        EmployeeId = time.EmployeeId,
                        EmployeeNr = time.EmployeeNr,
                        Time = time.Time,
                        Type = (TimeStampEntryType)time.Type,
                        TypeName = types[time.Type],
                        Name = outputName,
                        TimeTerminalName = time.TimeTerminalName,
                        TimeDeviationCauseName = time.TimeDeviationCauseName,
                        AccountName = time.AccountName,
                        IsBreak = time.IsBreak,
                        IsPaidBreak = time.IsPaidBreak,
                        IsDistanceWork = time.IsDistanceWork
                    };
                    if (isMobile && dto.Type == TimeStampEntryType.In)
                    {
                        employeeIdsIn.Add(time.EmployeeId);
                    }
                    employeeIdsAlreadyAdded.Add(dto.EmployeeId);
                    result.Add(dto);
                }
            }
            else
            {
                IQueryable<TimeStampAttendanceView> query = (from t in entitiesReadOnly.TimeStampAttendanceView
                                                             where t.ActorCompanyId == actorCompanyId
                                                             select t);

                if (singleEmployee)
                    query = query.Where(t => t.EmployeeId == employeeId.Value);
                else if (onlyIncludeAttestRoleEmployees)    // Only return entries from employees within current users attest role
                    query = query.Where(t => employeeIds.Contains(t.EmployeeId));

                if (showMode == TermGroup_TimeStampAttendanceGaugeShowMode.AllToday)
                    query = query.Where(t => t.Time >= DateTime.Today);

                query = query.OrderBy(t => t.FirstName).ThenBy(t => t.LastName).ThenBy(t => t.EmployeeId).ThenBy(t => t.Time);

                List<TimeStampAttendanceView> times = query.ToList();
                foreach (TimeStampAttendanceView time in times)
                {
                    if (!isGrouped && timeTerminalId.HasValue && time.TimeTerminalId != timeTerminalId.Value)
                        continue;

                    // Extensions
                    time.TypeName = types[time.Type];

                    employeeIdsAlreadyAdded.Add(time.EmployeeId);
                    result.Add(time.ToGaugeDTO());
                }
            }

            if (isMobile && result.Any())
            {
                List<int> distinctEmployeeIds = result.Select(x => x.EmployeeId).Distinct().ToList();
                Account account = new Account();
                List<Tuple<int, DateTime>> employeeScheduleStartTuples = TimeScheduleManager.GetEmployeesWithinSchedule(entitiesReadOnly, actorCompanyId, employees.Where(emp => distinctEmployeeIds.Contains(emp.EmployeeId)).ToList(), DateTime.Now, ref account, null, checkMobileIn: true, employeeIdsIn);
                foreach (Tuple<int, DateTime> employeeScheduleStartTuple in employeeScheduleStartTuples)
                {
                    IEnumerable<TimeStampAttendanceGaugeDTO> employeeDtos = result.Where(x => x.EmployeeId == employeeScheduleStartTuple.Item1);
                    foreach (TimeStampAttendanceGaugeDTO employeeDto in employeeDtos)
                    {
                        employeeDto.ScheduleStartTime = employeeScheduleStartTuple.Item2;
                    }
                }
            }

            if (includeMissingEmployees)
            {
                string notStampedInTerm = GetText(8646, "Ej instämplad");

                //Find employees not in the list
                List<Employee> missingEmployees = new List<Employee>();

                foreach (Employee emp in employees)
                {
                    EmployeeGroup employeeGroup = emp.GetEmployeeGroup(null, employeeGroups);
                    if (employeeGroup != null && !employeeGroup.AutogenTimeblocks && !employeeIdsAlreadyAdded.Contains(emp.EmployeeId))
                        missingEmployees.Add(emp);
                }

                Account account = new Account();
                List<Tuple<int, DateTime>> missing = TimeScheduleManager.GetEmployeesWithinSchedule(entitiesReadOnly, actorCompanyId, missingEmployees, DateTime.Now, ref account, null);

                foreach (Tuple<int, DateTime> miss in missing)
                {
                    Employee missingEmployee = employees.FirstOrDefault(e => e.EmployeeId == miss.Item1);

                    TimeStampAttendanceGaugeDTO dto = new TimeStampAttendanceGaugeDTO()
                    {
                        IsMissing = true,
                        Time = DateTime.Now,
                        TypeName = notStampedInTerm,
                        Name = missingEmployee.FirstName + " " + missingEmployee.LastName,
                        TimeTerminalName = string.Empty,
                        TimeDeviationCauseName = string.Empty,
                        AccountName = string.Empty,
                        ScheduleStartTime = miss.Item2,
                    };

                    result.Add(dto);
                }
            }

            return result;
        }

        #endregion

        #region WantedShiftsGauge

        /// <summary>
        /// Return shifts that are either open or unwanted with at least one employee in queue.
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="userId"> User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Collection of WantedShiftsGaugeDTO</returns>
        public IEnumerable<WantedShiftsGaugeDTO> GetWantedShifts(int actorCompanyId, int userId, int roleId)
        {
            List<WantedShiftsGaugeDTO> dtos = new List<WantedShiftsGaugeDTO>();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
            int currentEmployeeId = EmployeeManager.GetEmployeeIdForUser(userId, actorCompanyId);

            List<Employee> employees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, getHidden: true);
            List<int> employeeIds = employees.Select(x => x.EmployeeId).ToList();
            List<int> shiftTypeIds = TimeScheduleManager.GetShiftTypeIdsForUser(null, actorCompanyId, roleId, userId, EmployeeManager.GetEmployeeIdForUser(userId, actorCompanyId), true);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<TimeScheduleTemplateBlock> blocks = GetEmployeeDataInBatches(GetDataInBatchesModel.Create(entitiesReadOnly, actorCompanyId, employeeIds), GetShiftsIfQueueExists);

            blocks = blocks.Where(tb => !tb.TimeScheduleScenarioHeadId.HasValue &&
                                                         tb.TimeScheduleEmployeePeriodId.HasValue &&
                                                         tb.StartTime != tb.StopTime &&
                                                         !tb.IsPreliminary &&
                                                         (!tb.ShiftTypeId.HasValue || shiftTypeIds.Contains(tb.ShiftTypeId.Value)))
                                                .OrderBy(tb => tb.Date)
                                                .ThenBy(tb => tb.StartTime)
                                                .ThenBy(tb => tb.StopTime)
                                                .ThenBy(tb => tb.ShiftTypeId)
                                                .ToList();

            if (blocks.IsNullOrEmpty())
                return dtos;

            // Get valid accounts to be able to filter shifts
            List<int> validAccountIds = null;
            if (useAccountHierarchy)
            {
                DateTime dateFrom = blocks.First().Date.HasValue ? blocks.First().Date.Value : DateTime.Today;
                DateTime dateTo = blocks.Last().Date.HasValue ? blocks.Last().Date.Value : DateTime.Today;
                List<AttestRoleUser> attestRoleUsers = AttestManager.GetAttestRoleUsers(entitiesReadOnly, actorCompanyId, userId, dateFrom, dateTo);
                if (!attestRoleUsers.IsNullOrEmpty())
                    validAccountIds = AccountManager.GetAccountIdsFromHierarchyByUserSetting(actorCompanyId, roleId, userId, dateFrom, dateTo);
                else
                    validAccountIds = AccountManager.GetValidAccountsForEmployee(actorCompanyId, currentEmployeeId, 0, 0, dateFrom, dateTo, onlyParents: true).Select(x => x.AccountId).ToList();
            }

            List<int> otherEmployeeIds = new List<int>();
            foreach (TimeScheduleTemplateBlock block in blocks.ToList())
            {
                if (validAccountIds != null && (!block.AccountId.HasValue || !validAccountIds.Contains(block.AccountId.Value)))
                {
                    blocks.Remove(block);
                    continue;
                }

                foreach (var id in block.TimeScheduleTemplateBlockQueue.Where(w => w.State == (int)SoeEntityState.Active).Select(x => x.EmployeeId))
                {
                    if (!employeeIds.Contains(id))
                        otherEmployeeIds.Add(id);
                }
            }

            if (blocks.IsNullOrEmpty())
                return dtos;

            if (otherEmployeeIds.Any())
                employees.AddRange(EmployeeManager.GetAllEmployeesByIds(actorCompanyId, otherEmployeeIds, loadContact: true));

            List<ShiftType> shiftTypes = TimeScheduleManager.GetShiftTypes(actorCompanyId);

            // Convert to DTO
            foreach (TimeScheduleTemplateBlock block in blocks)
            {
                StringBuilder sb = new StringBuilder();

                foreach (TimeScheduleTemplateBlockQueue queue in block.TimeScheduleTemplateBlockQueue.Where(q => q.State == (int)SoeEntityState.Active && q.Type == (int)TermGroup_TimeScheduleTemplateBlockQueueType.Wanted).OrderBy(q => q.Sort))
                {
                    var employeeInQueue = employees.FirstOrDefault(e => e.EmployeeId == queue.EmployeeId);
                    if (employeeInQueue == null)
                        continue;

                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(employeeInQueue.Name);
                }

                ShiftType shiftType = block.ShiftTypeId.HasValue ? shiftTypes.FirstOrDefault(x => x.ShiftTypeId == block.ShiftTypeId.Value) : null;
                Employee employee = block.EmployeeId.HasValue ? employees.FirstOrDefault(x => x.EmployeeId == block.EmployeeId.Value) : null;
                WantedShiftsGaugeDTO dto = new WantedShiftsGaugeDTO()
                {
                    TimeScheduleTemplateBlockId = block.TimeScheduleTemplateBlockId,
                    Date = block.Date ?? CalendarUtility.DATETIME_DEFAULT,
                    Time = String.Format("{0} - {1}", block.StartTime.ToShortTimeString(), block.StopTime.ToShortTimeString()),
                    ShiftTypeId = block.ShiftTypeId ?? 0,
                    ShiftTypeName = shiftType?.Name,
                    EmployeeId = block.EmployeeId ?? 0,
                    Employee = employee?.Name,
                    OpenType = (block.ShiftStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Open ? 1 : 2),
                    OpenTypeName = String.Empty,
                    EmployeesInQueue = sb.ToString(),
                    Link = block.Link
                };

                dtos.Add(dto);
            }

            return dtos;
        }

        private List<TimeScheduleTemplateBlock> GetShiftsIfQueueExists(GetDataInBatchesModel model)
        {
            if (!model.IsValid())
                return new List<TimeScheduleTemplateBlock>();

            return (from tb in model.Entities.TimeScheduleTemplateBlock
                            .Include("TimeScheduleTemplateBlockQueue")
                    where
                    tb.EmployeeId.HasValue && model.BatchIds.Contains(tb.EmployeeId.Value) &&
                    tb.Date >= DateTime.Today.Date &&
                    tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None &&
                    tb.State == (int)SoeEntityState.Active &&
                    (tb.ShiftStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Open || tb.ShiftUserStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted) &&
                    tb.TimeScheduleTemplateBlockQueue.Any(q => q.Type == (int)TermGroup_TimeScheduleTemplateBlockQueueType.Wanted)
                    select tb).ToList();
        }

        #endregion

        #region Insights
        public ReportPrintoutDTO GetInsightsForDashboard(int actorCompanyId, int roleId, int userId, int reportId, int dataSelectionId, int columnSelectionId)
        {

            var report = ReportManager.GetReport(reportId, actorCompanyId, loadSysReportTemplateType: true);
            var selections = new List<ReportDataSelectionDTO>();

            var columnSelection = ReportManager.GetReportUserSelection(columnSelectionId, true, true);
            if (columnSelection != null) selections.AddRange(ReportDataSelectionDTO.FromJSON(columnSelection.Selection));
            var dataSelection = ReportManager.GetReportUserSelection(dataSelectionId, true, true);
            if (dataSelection != null) selections.AddRange(ReportDataSelectionDTO.FromJSON(dataSelection.Selection));

            var job = new ReportJobDefinitionDTO()
            {
                ReportId = reportId,
                ExportType = TermGroup_ReportExportType.Insight,
                SysReportTemplateTypeId = (SoeReportTemplateType)report.SysReportTemplateTypeId,
                Selections = selections,
            };

            return ReportDataManager.CreateInsight(job, actorCompanyId, userId, roleId);
        }

        #endregion

        #endregion

        #endregion
    }
}
