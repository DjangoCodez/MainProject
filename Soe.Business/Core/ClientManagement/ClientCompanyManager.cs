
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ClientManagement;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.ClientManagement
{
    public class ClientCompanyManager : ManagerBase
    {

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ClientCompanyManager(ParameterObject parameterObject) : base(parameterObject) {}

        public CompanyConnectionRequestDTO GetMultiCompanyConnectionRequest(Guid companyGuid, string code)
        {
            return SysMultiCompanyConnector.GetConnectionRequest(companyGuid, code);
        }

        public ActionResult AcceptMultiCompanyRequest(Guid companyGuid, int actorCompanyId, int licenseId, ServiceUserDTO dto)
        {
            if (dto.UserId != 0)
                return new ActionResult((int)ActionResultSave.IncorrectInput);

            if (dto == null || dto.RoleId == 0 || string.IsNullOrEmpty(dto.UserName))
                return new ActionResult((int)ActionResultSave.IncorrectInput);

            var connectionRequest = SysMultiCompanyConnector.GetConnectionRequest(companyGuid, dto.ConnectionCode);
            if (connectionRequest is null)
            {
                return new ActionResult(false);
            }

            var compDbId = CompDbCache.Instance.SysCompDbId;
            var topEntity = SoeEntityType.User;
            var actionMethod = TermGroup_TrackChangesActionMethod.User_Save;
            var employeeUser = new EmployeeUserDTO()
            {
                UserId = dto.UserId,
                LoginName = dto.UserName,
                FirstName = connectionRequest.MCName,
                LastName = $"({connectionRequest.MCLicenseNr})",
                SaveUser = true,
                LicenseId = licenseId,
                ActorCompanyId = actorCompanyId,
            };
            var userRoles = new List<UserRolesDTO>()
            {
                new UserRolesDTO()
                {
                    ActorCompanyId = actorCompanyId,
                    DefaultCompany = true,
                    Roles = new List<UserCompanyRoleDTO>()
                    {
                        new UserCompanyRoleDTO()
                        {
                            UserId = dto.UserId,
                            ActorCompanyId = actorCompanyId,
                            RoleId = dto.RoleId,
                            Default = true,
                            IsModified = true,
                        }
                    },
                    AttestRoles = dto.AttestRoleIds.Select(r => new UserAttestRoleDTO()
                    {
                        UserId = dto.UserId,
                        AttestRoleId = r,
                        IsModified = true,
                    }).ToList()
                }
            };
            var applyFeaturesResult = ActorManager.ApplyFeaturesOnEmployee(employeeUser);
            var changesRepository = TrackChangesManager.CreateEmployeeUserChangesRepository(actorCompanyId, Guid.NewGuid(), actionMethod, topEntity, applyFeaturesResult);
            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var actorManager = new ActorManager(this.parameterObject);
                        var saveUserResult = actorManager.SaveEmployeeUser(entities, transaction, actionMethod, employeeUser, applyFeaturesResult,
                            changesRepository: changesRepository,
                            userRoles: userRoles,
                            saveRoles: true,
                            saveAttestRoles: true,
                            autoAddDefaultRole: false,
                            generateCurrentChanges: false,
                            skipCategoryCheck: true
                        );

                        if (!saveUserResult.Success)
                            return saveUserResult;

                        var userId = saveUserResult.IntDict.GetValue((int)SaveEmployeeUserResult.UserId);
                        var connectionResult = AcceptSysMultiCompanyRequest(companyGuid, dto.ConnectionCode, userId, dto.RoleId);

                        if (!connectionResult.Success)
                            return connectionResult;

                        transaction.Complete();
                        return new ActionResult()
                        {
                            Success = true,
                            IntegerValue = userId,
                        };
                    }
                }
                catch (Exception ex) {
                    LogError(ex, this.log);
                    return new ActionResult()
                    {
                        Success = false,
                        Exception = ex,
                        ErrorMessage = ex.Message,
                    };
                }
            }
        }
        public ActionResult AcceptSysMultiCompanyRequest(Guid companyGuid, string code, int userId, int roleId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                var acceptRequest = new MultiCompanyConnectionAcceptDTO()
                {
                    Code = code,
                    TCActorCompanyId = base.ActorCompanyId,
                    TCSysCompDbId = CompDbCache.Instance.SysCompDbId,
                    TCLicenseId = base.LicenseId,
                    TCUserId = userId,
                    TCRoleId = roleId,
                    TCActorCompanyGuid = companyGuid,
                    RegisteredBy = base.GetUserDetails(),
                };
                var compDbId = CompDbCache.Instance.SysCompDbId;

                actionResult = SysMultiCompanyConnector.AcceptConnection(companyGuid, compDbId, acceptRequest);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                actionResult.Success = false;
            }
            return actionResult;
        }
 
        public List<ServiceUserDTO> GetServiceUsers(Guid companyGuid, int actorCompanyId)
        {
            var compDbId = CompDbCache.Instance.SysCompDbId;
            var linkedMultiCompanies = SysMultiCompanyConnector.GetLinkedMultiCompanies(companyGuid, compDbId);
            var roles = RoleManager.GetAllRolesByCompany(actorCompanyId);
            var users = UserManager.GetUsers(linkedMultiCompanies.Select(m => m.TCUserId).ToList(), true);

            var serviceUsers = new List<ServiceUserDTO>();
            foreach (var serviceProvider in linkedMultiCompanies)
            {
                var user = users.FirstOrDefault(u => u.UserId == serviceProvider.TCUserId);
                var role = roles.FirstOrDefault(r => r.RoleId == serviceProvider.TCRoleId);
                var serviceUser = new ServiceUserDTO()
                {
                    RoleId = role.RoleId,
                    RoleName = role.Name,
                    UserId = user.UserId,
                    UserName = user.LoginName,
                    AttestRoleIds = AttestManager.GetAttestRolesForUser(actorCompanyId, user.UserId).Select(a => a.AttestRoleId).ToList(),
                    ServiceProvider = new ServiceProviderDTO()
                    {
                        CompanyName = serviceProvider.MCName,
                        LicenseName = serviceProvider.MCLicenseName,
                        LicenseNumber = serviceProvider.MCLicenseNr,
                    },
                    Created = user.Created.Value,
                    CreatedBy = user.CreatedBy,
                    Modified = user.Modified,
                    ModifiedBy = user.ModifiedBy,
                };
                serviceUsers.Add(serviceUser);
            }

            return serviceUsers;
        }
    }
}
