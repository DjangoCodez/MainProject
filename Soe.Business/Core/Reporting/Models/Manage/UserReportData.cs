using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Manage
{
    public class UserReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly UserReportDataOutput _reportDataOutput;
        private readonly UserReportDataInput _reportDataInput;

        private bool loadSession => _reportDataInput.Columns.Any(a => a.Column == TermGroup_UserMatrixColumns.LastLogin);
        private bool loadRoles => _reportDataInput.Columns.Any(a => a.Column == TermGroup_UserMatrixColumns.Roles);
        private bool loadAttestRoles => _reportDataInput.Columns.Any(a => a.Column == TermGroup_UserMatrixColumns.AttestRoles);
        private bool loadEmployee => _reportDataInput.Columns.Any(a => a.Column == TermGroup_UserMatrixColumns.EmployeeNr);

        public UserReportData(ParameterObject parameterObject, UserReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new UserReportDataOutput(reportDataInput);
        }

        public UserReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            #region Get selections

            if (!TryGetDateFromSelection(reportResult, out DateTime selectionDate))
                return new ActionResult(false);
            if (!TryGetUserIdsFromSelection(reportResult, out List<int> selectionUserIds, out bool selectionIncludeInactive))
                return new ActionResult(false);

            TryGetBoolFromSelection(reportResult, out bool selectionOptimizeGrouping, "uniqueRowUserRoleAttestRole");

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, reportResult.ActorCompanyId);

                #region Load users

                List<User> usersList;
                if (!selectionUserIds.IsNullOrEmpty())
                {
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    usersList = entitiesReadOnly.User.Include(i => i.AttestRoleUser).Include(i => i.UserCompanyRole).Where(w => selectionUserIds.Contains(w.UserId)).ToList();
                }
                else
                {
                    usersList = UserManager.GetUsersByCompany(base.ActorCompanyId, base.RoleId, base.UserId, setDefaultRoleName: true);

                    if (selectionIncludeInactive)
                    {
                        List<User> usersInactive = UserManager.GetUsersByCompany(base.ActorCompanyId, base.RoleId, base.UserId, setDefaultRoleName: true, active: false);

                        if (!usersInactive.IsNullOrEmpty())
                        {
                            usersList = usersList.Concat(usersInactive).ToList();
                        }
                    }
                }

                #endregion

                List<Account> companyAccounts = new List<Account>();
                if (useAccountHierarchy)
                    companyAccounts = AccountManager.GetAccounts(base.ActorCompanyId);
                List<Role> companyRoles = RoleManager.GetRolesByCompany(entities, base.ActorCompanyId);
                //List<int> companyRoleIds = companyRoles.Select(s => s.RoleId).ToList();

                List<UserItem> returnedItems = new List<UserItem>();
                List<UserItem> mergedItems = new List<UserItem>();

                foreach (var user in usersList)
                {
                    if (selectionUserIds.Contains(user.UserId) || selectionUserIds.IsNullOrEmpty())
                    {
                        #region Load employee
                        Employee userEmployee = null;
                        if (loadEmployee)
                        {
                            userEmployee = EmployeeManager.GetEmployeeByUser(base.ActorCompanyId, user.UserId, true, loadEmployeeAccount: true, ignoreState: true);
                        }
                        #endregion

                        #region Load eCom
                        Contact cont;

                        if (!user.ContactPersonReference.IsLoaded)
                            user.ContactPersonReference.Load();
                        cont = ContactManager.GetContactFromActor(user.ContactPerson.ActorContactPersonId, loadActor: true, loadAllContactInfo: true);

                        ContactECom eCom = null;
                        if (cont != null)
                            eCom = ContactManager.GetContactECom(cont.ContactId, (int)TermGroup_SysContactEComType.Email, true);
                        #endregion

                        #region Load roles

                        List<AttestRole> attestRoles;
                        List<UserCompanyRole> userRoles;
                        if (loadRoles || loadAttestRoles)
                        {
                            List<AttestRole> attestRolesForUser = AttestManager.GetAttestRolesForUser(base.ActorCompanyId, user.UserId, selectionDate);
                            attestRoles = AttestManager.GetAttestRoles(base.ActorCompanyId, loadAttestRoleUser: true, loadExternalCode: true).Where(x => attestRolesForUser.Any(y => y.AttestRoleId == x.AttestRoleId)).ToList();
                            userRoles = user.UserCompanyRole.Where(a => a.State == (int)SoeEntityState.Active).ToList();
                        }
                        else
                        {
                            attestRoles = new List<AttestRole>();
                            userRoles = new List<UserCompanyRole>();
                        }

                        #endregion

                        List<int> intersectedRoleIds = new List<int>();
                        if (!userRoles.IsNullOrEmpty())
                        {
                            intersectedRoleIds = companyRoles.Select(x => x.RoleId).Intersect(userRoles.Select(x => x.RoleId))?.ToList();
                        }

                        if (!selectionOptimizeGrouping)
                        {
                            List<AttestRoleUser> storedAttestRoleUsers = new List<AttestRoleUser>();
                            List<string> attestRoleAccounts = new List<string>();

                            if (useAccountHierarchy)
                            {
                                foreach (AttestRole attestRole in attestRoles)
                                {
                                    foreach (var attestRoleUserByAccounts in attestRole.AttestRoleUser.Where(i => i.State == (int)SoeEntityState.Active && i.UserId == user.UserId).GroupBy(g => $"{g.AttestRoleId}#{g.AccountId}"))
                                    {
                                        AttestRoleUser attestRoleuserAccount = attestRoleUserByAccounts.First();

                                        if (attestRoleuserAccount.Account != null)
                                            attestRoleAccounts.Add(attestRoleuserAccount.Account?.AccountNrPlusName);

                                        if (attestRoleuserAccount.AccountId.HasValue && !storedAttestRoleUsers.Any(w => w.AccountId == attestRoleuserAccount.AccountId.Value))
                                            storedAttestRoleUsers.Add(attestRoleuserAccount);
                                    }
                                }
                            }

                            List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                            if (userEmployee != null && !userEmployee.EmployeeAccount.IsNullOrEmpty())
                                employeeAccounts = userEmployee.EmployeeAccount.Where(r => (r.DateFrom <= selectionDate) && (!r.DateTo.HasValue || r.DateTo >= selectionDate) && r.State == (int)SoeEntityState.Active).ToList();


                            #region Item

                            var userItem = new UserItem()
                            {
                                LoginName = user?.LoginName ?? string.Empty,
                                Email = eCom?.Text ?? string.Empty,
                                EmployeeNr = userEmployee?.EmployeeNr ?? string.Empty,
                                Name = user?.Name ?? string.Empty,
                                Roles = !intersectedRoleIds.IsNullOrEmpty() ? string.Join(", ", companyRoles.Where(x => intersectedRoleIds.Contains(x.RoleId))?.Select(t => t.Name)) : string.Empty,
                                AttestRoles = !attestRoles.IsNullOrEmpty() ? string.Join(", ", attestRoles.Select(t => t.Name ?? string.Empty)) : string.Empty,
                                AttestRoleAccount = !attestRoleAccounts.IsNullOrEmpty() ? string.Join(", ", attestRoleAccounts) : string.Empty,
                                DateCreated = user?.Created,
                                CreatedBy = user?.CreatedBy ?? string.Empty,
                                DateModified = user?.Modified,
                                ModifiedBy = user?.ModifiedBy ?? string.Empty,
                                State = user?.State ?? (int)SoeEntityState.Deleted,
                                IsActive = user?.State == (int)SoeEntityState.Active,
                                IsMobileUser = user?.IsMobileUser ?? false,
                                RoleDateFrom = userRoles?.Where(a => a.DateFrom.HasValue)?.OrderBy(a => a.DateFrom.Value).FirstOrDefault()?.DateFrom,
                                RoleDateTo = userRoles?.Where(a => a.DateTo.HasValue)?.OrderBy(a => a.DateTo.Value).LastOrDefault()?.DateTo,
                                AttestRoleDateFrom = storedAttestRoleUsers?.Where(a => a.DateFrom.HasValue)?.OrderBy(a => a.DateFrom.Value).FirstOrDefault()?.DateFrom,
                                AttestRoleDateTo = storedAttestRoleUsers?.Where(a => a.DateTo.HasValue)?.OrderBy(a => a.DateTo.Value).LastOrDefault()?.DateTo,
                                ShowAllCategories = attestRoles?.Any(r => r.ShowAllCategories) ?? false,
                                ShowUncategorized = attestRoles?.Any(r => r.ShowUncategorized) ?? false,
                                AccountName = !employeeAccounts.IsNullOrEmpty() ? string.Join(", ", employeeAccounts.Select(a => companyAccounts.FirstOrDefault(c => c.AccountId == a.AccountId)?.AccountNrPlusName)) : string.Empty
                            };

                            if (loadSession)
                            {
                                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                                userItem.LastLogin = user != null ? entitiesReadOnly.UserSession.OrderByDescending(o => o.Login).FirstOrDefault(u => u.User.UserId == user.UserId)?.Login : null;
                            }

                            _reportDataOutput.UserItems.Add(userItem);

                            #endregion
                        }
                        else
                        {
                            List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                            if (userEmployee != null && !userEmployee.EmployeeAccount.IsNullOrEmpty())
                                employeeAccounts = userEmployee.EmployeeAccount.Where(r => (r.DateFrom <= selectionDate) && (!r.DateTo.HasValue || r.DateTo >= selectionDate) && r.State == (int)SoeEntityState.Active).ToList();
                            
                            if (employeeAccounts.IsNullOrEmpty())
                                employeeAccounts.Add(new EmployeeAccount()); //Add empty

                            if (userRoles.IsNullOrEmpty())
                                userRoles.Add(new UserCompanyRole()); //Add empty

                            if (attestRoles.IsNullOrEmpty())
                                attestRoles.Add(new AttestRole()); //Add empty

                            if (intersectedRoleIds.IsNullOrEmpty())
                                intersectedRoleIds.Add(0); //Add empty

                            foreach (var userRoleId in intersectedRoleIds)
                            {
                                Role cRole = companyRoles.FirstOrDefault(r => r.RoleId == userRoleId);
                                UserCompanyRole uRole = userRoles.FirstOrDefault(r => r.RoleId == userRoleId);

                                foreach (var attestRole in attestRoles)
                                {
                                    foreach (var attestRoleUserByAccounts in attestRole.AttestRoleUser.Where(i => i.State == (int)SoeEntityState.Active && i.UserId == user.UserId).GroupBy(g => $"{g.AttestRoleId}#{g.AccountId}"))
                                    {
                                        AttestRoleUser attestRoleuserAccount = attestRoleUserByAccounts.First();

                                        foreach (int? accountId in employeeAccounts.Select(ea => ea.AccountId))
                                        {
                                            #region Item

                                            var userItem = new UserItem()
                                            {
                                                LoginName = user?.LoginName ?? string.Empty,
                                                Email = eCom?.Text ?? string.Empty,
                                                EmployeeNr = userEmployee?.EmployeeNr ?? string.Empty,
                                                Name = user?.Name ?? string.Empty,
                                                Roles = cRole?.Name ?? string.Empty,
                                                AttestRoles = attestRole.Name,
                                                AttestRoleAccount = attestRoleuserAccount.Account?.AccountNrPlusName,
                                                DateCreated = user?.Created,
                                                CreatedBy = user?.CreatedBy ?? string.Empty,
                                                DateModified = user?.Modified,
                                                ModifiedBy = user?.ModifiedBy ?? string.Empty,
                                                State = user?.State ?? (int)SoeEntityState.Deleted,
                                                IsActive = user?.State == (int)SoeEntityState.Active,
                                                IsMobileUser = user?.IsMobileUser ?? false,
                                                RoleDateFrom = uRole?.DateFrom,
                                                RoleDateTo = uRole?.DateTo,
                                                AttestRoleDateFrom = attestRoleuserAccount.DateFrom,
                                                AttestRoleDateTo = attestRoleuserAccount.DateTo,
                                                ShowAllCategories = attestRole.ShowAllCategories,
                                                ShowUncategorized = attestRole.ShowUncategorized,
                                                AccountName = companyAccounts?.FirstOrDefault(a => a.AccountId == accountId)?.AccountNrPlusName ?? string.Empty,
                                                AccountId = accountId
                                            };

                                            if (loadSession)
                                            {
                                                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                                                userItem.LastLogin = user != null ? entitiesReadOnly.UserSession.OrderByDescending(o => o.Login).FirstOrDefault(u => u.User.UserId == user.UserId)?.Login : null;
                                            }

                                            returnedItems.Add(userItem);

                                            #endregion
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (selectionOptimizeGrouping)
                {
                    var columns = _reportDataInput.Columns.Select(s => s.Column).ToList();

                    foreach (var grouped in returnedItems.GroupBy(g => g.GroupOn(columns)))
                    {
                        mergedItems.Add(grouped.First());
                    }

                    _reportDataOutput.UserItems = mergedItems.ToList();
                }
            }
            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }
    }

    public class UserReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_UserMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public UserReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_UserMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_UserMatrixColumns.Unknown;
        }
    }

    public class UserReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<UserItem> UserItems { get; set; }
        public UserReportDataInput Input { get; set; }

        public UserReportDataOutput(UserReportDataInput input)
        {
            this.UserItems = new List<UserItem>();
            this.Input = input;
        }
    }

    public class UserReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<UserReportDataReportDataField> Columns { get; set; }

        public UserReportDataInput(CreateReportResult reportResult, List<UserReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }
}
