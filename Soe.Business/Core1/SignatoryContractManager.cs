using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.SignatoryContract;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class SignatoryContractManager : ManagerBase
    {
        #region Variables 
        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public SignatoryContractManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Entry points

        public bool UsesSignatoryContractForPermission(TermGroup_SignatoryContractPermissionType permissionType)
        {
            using (var context = new SignatoryRequestContext(this.parameterObject))
                return UsesSignatoryContractForPermission(context, permissionType);
        }

        public GetPermissionResultDTO Authorize(AuthorizeRequestDTO authorizeRequest)
        {
            using (var context = new SignatoryRequestContext(this.parameterObject))
                return Authorize(context, authorizeRequest);
        }

        public AuthenticationResultDTO ValidateAuthenticationResponse(AuthenticationResponseDTO response)
        {
            using (var context = new SignatoryRequestContext(this.parameterObject))
                return ValidateAuthenticationResponse(context, response);
        }

        public List<SignatoryContractGridDTO> GetSignatoryContractsGrid(int? signatoryContractId)
        {
            List<SignatoryContract> signatoryContracts;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            using (SignatoryRequestContext context = new SignatoryRequestContext(this.parameterObject, entitiesReadOnly))
            {
                signatoryContracts = this.GetSignatoryContractsGridData(
                    context, signatoryContractId);
            }

            List<SignatoryContractGridDTO> dtos = MapSignatoryContractsToDTO(signatoryContracts);
            return dtos;
        }

        public SignatoryContractDTO GetSignatoryContract(int signatoryContractId)
        {
            SignatoryContractDTO dto = null;
            SignatoryContract signatoryContract = null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            using (SignatoryRequestContext context = new SignatoryRequestContext(this.parameterObject, entitiesReadOnly))
            {
                signatoryContract = this.GetSignatoryContractData(
                    signatoryContractId, context);
            }

            dto = signatoryContract.ToDTO();

            return dto;
        }

        public List<SignatoryContractDTO> GetSignatoryContractSubContract(int signatoryContractId)
        {
            List<SignatoryContract> subContracts;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            using (SignatoryRequestContext context = new SignatoryRequestContext(this.parameterObject, entitiesReadOnly))
            {
                subContracts = this.GetSignatoryContractSubContractData(
                    signatoryContractId, context);
            }

            Dictionary<int, string> permissionTerms = GetPermissionTermsDict();

            List<SignatoryContractDTO> subContractsDtos = subContracts
                .ToDTOs(permissionTerms)
                .ToList();

            return subContractsDtos;
        }

        public List<SignatoryContractPermissionEditItem> GetPermissionTerms(int signatoryContractId)
        {
            Dictionary<int, string> permissionTerms = GetPermissionTermsDict();
            List<int> permissionTypes = new List<int>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (signatoryContractId > 0)
            {
                using (SignatoryRequestContext context = new SignatoryRequestContext(this.parameterObject, entitiesReadOnly))
                {
                    permissionTypes = this.GetSignatoryContractPermissionTypes(
                        context, signatoryContractId);
                }
            }

            List<SignatoryContractPermissionEditItem> permissionItems = permissionTerms
                .Select((p) =>
                {
                    SignatoryContractPermissionEditItem pdi
                        = new SignatoryContractPermissionEditItem
                        {
                            Id = p.Key,
                            Name = p.Value,
                            IsSelected = permissionTypes.Contains(p.Key)
                        };

                    return pdi;
                })
                .ToList();

            return permissionItems;

        }

        public ActionResult SaveSignatoryContract(SignatoryContractDTO inputSignatoryContract)
        {
            ActionResult result = new ActionResult();


            Dictionary<int, string> permissionTerms = GetPermissionTermsDict();
            Dictionary<int, string> authenticationMethodTerms
                = GetAuthenticationMethodTermsDict();

            if (inputSignatoryContract.SignatoryContractId == 0
                && inputSignatoryContract.SubContracts.Count > 0)
            {
                result = new ActionResult(
                    false,
                    (int)ActionResultSave.SignatoryContractAddContractWithSubContracts,
                    GetText(1652, 1006));
            }
            else if (inputSignatoryContract.PermissionTypes.Count == 0
                || inputSignatoryContract.SubContracts.Any(c => c.PermissionTypes.Count == 0)
                || inputSignatoryContract.PermissionTypes.Any(pt =>
                    !permissionTerms.ContainsKey(pt))
                || inputSignatoryContract.SubContracts.Any(c =>
                    c.PermissionTypes.Any(pt => !permissionTerms.ContainsKey(pt))))
            {
                result = new ActionResult(
                    false,
                    (int)ActionResultSave.SignatoryContractInvalidPermission,
                    GetText(1641, 1006));
            }
            else if (inputSignatoryContract
                .SubContracts
                .Any(c => c
                    .PermissionTypes
                    .Any(pt => !inputSignatoryContract
                        .PermissionTypes.Any(ipt => pt == ipt))))
            {
                result = new ActionResult(
                    false,
                    (int)ActionResultSave.SignatoryContractSubContractInvalidPermission,
                    GetText(1651, 1006));
            }
            else if (inputSignatoryContract.SubContracts
                .GroupBy(c => c.RecipientUserId).Any(g => g.Count() > 1))
            {
                result = new ActionResult(
                    false,
                    (int)ActionResultSave.SignatoryContractDuplicateChildren,
                    GetText(1642, 1006));
            }
            else if (inputSignatoryContract.SignatoryContractId == 0
                && !authenticationMethodTerms
                .ContainsKey(inputSignatoryContract.RequiredAuthenticationMethodType))
            {
                result = new ActionResult(
                    false,
                    (int)ActionResultSave.SignatoryContractInvalidAuthenticationMethodType,
                    GetText(1643, 1006));
            }
            else
            {
                int currentSignedByUserId = GetCurrentSignedByUserId();

                List<int> inputUsers = inputSignatoryContract.SubContracts
                    .Select(c => c.RecipientUserId)
                    .Union(new List<int>
                        {
                            inputSignatoryContract.RecipientUserId,
                            currentSignedByUserId
                        })
                    .Distinct()
                    .ToList();

                using (SignatoryRequestContext context
                    = new SignatoryRequestContext(this.parameterObject))
                {

                    List<User> users = context.Entities.User
                        .Where(u => inputUsers.Contains(u.UserId))
                        .ToList();

                    if (users.Count != inputUsers.Count)
                    {
                        result.Success = false;
                        result.ErrorMessage = GetText(1288, "Användare hittades inte");
                        result.ErrorNumber = (int)ActionResultSave.SignatoryContractInvalidUser;
                    }
                    else
                    {
                        SignatoryContract signatoryContract;
                        User currentSignedByUser = users
                            .First(u => u.UserId == currentSignedByUserId);
                        if (inputSignatoryContract.SignatoryContractId > 0)
                        {
                            signatoryContract = GetAllSignatoryContractData(
                                inputSignatoryContract.SignatoryContractId, context);

                            bool hasAdditionalModifyPermission = HasAdditionalModifyPermission(
                                signatoryContract);

                            if (!hasAdditionalModifyPermission)
                            {
                                result = new ActionResult(
                                    false,
                                    (int)ActionResultSave.InsufficienPermissionToSave,
                                    GetText(1155, "Behörighet saknas"));
                            }
                            else if (inputSignatoryContract
                                .SubContracts
                                .Any(c => c
                                    .PermissionTypes
                                    .Any(pt => !signatoryContract
                                        .SignatoryContractPermission.Any(scp => pt == scp.PermissionType))))
                            {
                                result = new ActionResult(
                                    false,
                                    (int)ActionResultSave.SignatoryContractSubContractInvalidPermission,
                                    GetText(1651, 1006));
                            }
                            else
                            {
                                ModifySignatoryContract(
                                    signatoryContract,
                                    users,
                                    inputSignatoryContract,
                                    context,
                                    currentSignedByUser);

                                result = SaveChanges(context.Entities);
                                result.IntegerValue = signatoryContract.SignatoryContractId;
                            }

                        }
                        else
                        {
                            bool hasAdditionalAddPermission = HasAdditionalAddPermission();

                            if (!hasAdditionalAddPermission)
                            {
                                result = new ActionResult(
                                    false,
                                    (int)ActionResultSave.InsufficienPermissionToSave,
                                    GetText(1155, "Behörighet saknas"));
                            }
                            else
                            {
                                signatoryContract = new SignatoryContract();

                                AddSignatoryContract(
                                    signatoryContract,
                                    users,
                                    inputSignatoryContract,
                                    context,
                                    currentSignedByUser);

                                result = SaveChanges(context.Entities);
                                result.IntegerValue = signatoryContract.SignatoryContractId;
                            }

                        }
                    }

                }
            }

            return result;
        }

        public ActionResult RevokeSignatoryContract(
            int signatoryContractId,
            SignatoryContractRevokeDTO inputSignatoryContract)
        {
            ActionResult result = new ActionResult();

            using (SignatoryRequestContext context
                    = new SignatoryRequestContext(this.parameterObject))
            {
                SignatoryContract signatoryContract = GetAllSignatoryContractDataForRevoke(
                    signatoryContractId, context);

                if (signatoryContract == null)
                {
                    result = new ActionResult(
                        (int)ActionResultSave.EntityNotFound, "SignatoryContract");
                }
                else
                {
                    bool hasAdditionalRevokePermission = HasAdditionalRevokePermission(
                        signatoryContract);

                    if (hasAdditionalRevokePermission)
                    {
                        string revokedBy = GetUserDetails();
                        DateTime now = DateTime.UtcNow;
                        signatoryContract.RevokedAtUTC = now;
                        signatoryContract.RevokedBy = revokedBy;
                        signatoryContract.RevokedReason = inputSignatoryContract.RevokedReason;

                        foreach (SignatoryContract child in signatoryContract.SignatoryContract1)
                        {
                            child.RevokedAtUTC = now;
                            child.RevokedBy = revokedBy;
                            child.RevokedReason = inputSignatoryContract.RevokedReason;
                        }

                        SetModifiedProperties(signatoryContract);
                        result = SaveChanges(context.Entities);
                    }
                    else
                    {
                        result = new ActionResult(
                            false,
                            (int)ActionResultSave.InsufficienPermissionToSave,
                            GetText(1155, "Behörighet saknas"));
                    }

                }
            }

            return result;
        }

        #endregion


        #region Flows
        private GetPermissionResultDTO Authorize(
            SignatoryRequestContext context, AuthorizeRequestDTO authorizeRequest)
        {

            // Get relevant signatory contract
            // If authentication is required, start the authentication flow
            // Log the usage of the permission

            string startMessage = $"Authorize called for permission {authorizeRequest.PermissionType}";

            if (authorizeRequest.SignatoryContractId.HasValue)
            {
                startMessage += $" and signatory contract {authorizeRequest.SignatoryContractId.Value}";
            }

            context.Logs.Add(
                SignatoryContractLogType.Info,
                authorizeRequest.PermissionType,
                startMessage
                );
            string label = GetPermissionName(authorizeRequest.PermissionType);

            if (context.IdLoginGuid == Guid.Empty)
            {
                context.Logs.Add(
                    SignatoryContractLogType.Error,
                    authorizeRequest.PermissionType,
                    GetText(1155, "Behörighet saknas"));
                return AuthenticationDetailsFactory.Reject(authorizeRequest.PermissionType, label);
            }

            var permission = GetSignatoryContractPermission(
                context, authorizeRequest);
            if (!permission.IsValid())
            {
                context.Logs.Add(
                    SignatoryContractLogType.Error,
                    authorizeRequest.PermissionType,
                    GetText(6051, "Kunde inte hitta fullmakt"));
                return AuthenticationDetailsFactory.Reject(authorizeRequest.PermissionType, label);
            }

            var contract = permission.SignatoryContract;
            if (!contract.IsValid())
            {
                context.Logs.Add(
                    SignatoryContractLogType.Error,
                    authorizeRequest.PermissionType,
                    GetText(6052, "Fullmakten är inte legitim"));
                return AuthenticationDetailsFactory.Reject(authorizeRequest.PermissionType, label);
            }
            context.Logs.SetSignatoryContractId(contract.SignatoryContractId);

            if (contract.GetRequiredAuthenticationMethodType() == SignatoryContractAuthenticationMethodType.None)
            {
                context.Logs.Add(
                    SignatoryContractLogType.Info,
                    authorizeRequest.PermissionType,
                    $"Accepted, no authentication required");
                return AuthenticationDetailsFactory.Accept_NoAuthenticationRequired(authorizeRequest.PermissionType, label);
            }

            var openRequest = GetActiveContractAuthenticationRequest(context, contract.SignatoryContractId);
            if (openRequest.IsAuthenticated())
            {
                context.Logs.Add(
                    SignatoryContractLogType.Info,
                    authorizeRequest.PermissionType,
                    "Accepted, authentication already in progress");
                return AuthenticationDetailsFactory.Accept_IsAuthenticated(
                    authorizeRequest.PermissionType, label);
            }

            var authenticationDetails = CreateAuthenticationRequest(
                context,
                contract,
                authorizeRequest.PermissionType);
            return AuthenticationDetailsFactory.Reject_AuthenticationRequired(
                authenticationDetails, authorizeRequest.PermissionType, label);
        }

        private AuthenticationResultDTO ValidateAuthenticationResponse(SignatoryRequestContext context, AuthenticationResponseDTO response)
        {
            var authenticationRequest = GetAuthenticationRequest(context, response.SignatoryContractAuthenticationRequestId);

            if (!authenticationRequest.IsValid())
            {
                context.Logs.Add(SignatoryContractLogType.Warning, TermGroup_SignatoryContractPermissionType.Unknown, GetText(6053, "Förfrågan har utgått"));
                return AuthenticationResultFactory.Failure("Authentication request not valid");
            }

            if (authenticationRequest.IsAuthenticated())
            {
                context.Logs.Add(SignatoryContractLogType.Info, TermGroup_SignatoryContractPermissionType.Unknown, "Authentication request already authenticated");
                return AuthenticationResultFactory.Success();
            }

            if (authenticationRequest.GetAuthenticationMethodType() == SignatoryContractAuthenticationMethodType.Password)
            {
                bool isValid = ValidateUserNamePassword(context, response);

                if (!isValid)
                {
                    return AuthenticationResultFactory.Failure(GetText(6054, "Angivna uppgifter är felaktiga"));
                }
                return SetAsAuthenticated(context, authenticationRequest);
            }

            if (authenticationRequest.GetAuthenticationMethodType() == SignatoryContractAuthenticationMethodType.PasswordSMSCode)
            {
                bool pwIsValid = ValidateUserNamePassword(context, response);
                var code = CodeGenerator.FromGuid(authenticationRequest.ExternalId);
                bool codeIsValid = response.Code == $"{code}";
                if (!pwIsValid || !codeIsValid)
                {
                    return AuthenticationResultFactory.Failure(GetText(6054, "Angivna uppgifter är felaktiga"));
                }
                return SetAsAuthenticated(context, authenticationRequest);
            }

            return AuthenticationResultFactory.Failure("Unknown");
        }

        #endregion

        #region Save Changes

        private bool HasAdditionalModifyPermission(SignatoryContract signatoryContract)
        {
            int currentSignedByUserId = GetCurrentSignedByUserId();
            int mainRecipientUserId = signatoryContract
                .SignatoryContractRecipient
                .First()
                .RecipientUserId;
            bool hasAdditionalModifyPermission =
                mainRecipientUserId == currentSignedByUserId
                && !parameterObject.IsSupportLoggedIn
                && (!parameterObject.SupportUserId.HasValue
                    || parameterObject.SupportUserId == 0);


            return hasAdditionalModifyPermission;
        }

        private bool HasAdditionalAddPermission()
        {
            bool hasAdditionalAddPermission = parameterObject.SupportUserId > 0;

            return hasAdditionalAddPermission;
        }

        private bool HasAdditionalRevokePermission(SignatoryContract signatoryContract)
        {
            int currentSignedByUserId = GetCurrentSignedByUserId();
            int mainRecipientUserId = signatoryContract
                .SignatoryContractRecipient
                .First()
                .RecipientUserId;

            bool hasAdditionalRevokePermission =
                mainRecipientUserId == currentSignedByUserId
                || parameterObject.SupportUserId > 0;


            return hasAdditionalRevokePermission;
        }

        private int GetCurrentSignedByUserId()
        {
            return UserId;
        }

        private void ModifySignatoryContract(
            SignatoryContract signatoryContract,
            List<User> users,
            SignatoryContractDTO inputSignatoryContract,
            SignatoryRequestContext context,
            User signedByUser)
        {

            List<SignatoryContract> toRemoveChildren = signatoryContract.SignatoryContract1
                .Where(sc => !inputSignatoryContract.SubContracts
                    .Any(c => c.SignatoryContractId == sc.SignatoryContractId))
                .ToList();

            foreach (SignatoryContract child in toRemoveChildren)
            {
                List<SignatoryContractPermission> toRemovePermissions = child
                    .SignatoryContractPermission
                    .ToList();
                foreach (SignatoryContractPermission childPermission in toRemovePermissions)
                {
                    context.Entities.SignatoryContractPermission.DeleteObject(childPermission);
                }

                List<SignatoryContractAuthenticationRequest> toRemoveAuthRequests = child
                    .SignatoryContractAuthenticationRequest
                    .ToList();
                foreach (SignatoryContractAuthenticationRequest childAuthRequest in toRemoveAuthRequests)
                {
                    context.Entities.SignatoryContractAuthenticationRequest.DeleteObject(childAuthRequest);
                }

                List<SignatoryContractRecipient> toRemoveRecipients = child
                    .SignatoryContractRecipient
                    .ToList();
                foreach (SignatoryContractRecipient childRecipient in toRemoveRecipients)
                {
                    context.Entities.SignatoryContractRecipient.DeleteObject(childRecipient);
                }

                List<SignatoryContractLog> toRemoveLogs = child
                    .SignatoryContractLog
                    .ToList();
                foreach (SignatoryContractLog childLog in toRemoveLogs)
                {
                    context.Entities.SignatoryContractLog.DeleteObject(childLog);
                }

                signatoryContract.SignatoryContract1.Remove(child);
                context.Entities.SignatoryContract.DeleteObject(child);
            }

            foreach (SignatoryContract child in signatoryContract.SignatoryContract1)
            {
                SignatoryContractDTO modifiedChild =
                    inputSignatoryContract.SubContracts
                    .First(c => c.SignatoryContractId == child.SignatoryContractId);

                if (child.SignatoryContractRecipient.Count > 0
                    && child.SignatoryContractRecipient.First().RecipientUserId
                    != modifiedChild.RecipientUserId)
                {
                    User user = users.First(u => u.UserId == modifiedChild.RecipientUserId);
                    child.SignatoryContractRecipient.First().RecipientUserId
                        = modifiedChild.RecipientUserId;
                    child.SignatoryContractRecipient.First().RecipientIdLoginGuid
                        = user.idLoginGuid ?? Guid.Empty;
                }
                else if (child.SignatoryContractRecipient.Count == 0)
                {
                    AddRecipientToSignatoryContract(
                        child, modifiedChild, users);
                }

                List<SignatoryContractPermission> toRemoveChildrenPermissions = child
                    .SignatoryContractPermission
                    .Where(sc => !modifiedChild.PermissionTypes.Contains(sc.PermissionType))
                    .ToList();

                foreach (SignatoryContractPermission childPermission in toRemoveChildrenPermissions)
                {
                    child.SignatoryContractPermission.Remove(childPermission);
                    context.Entities.SignatoryContractPermission.DeleteObject(childPermission);
                }

                AddPermissionsToSignatoryContract(modifiedChild, child);

            }

            AddChildrenToSignatoryContract(
                inputSignatoryContract, signatoryContract, users, signedByUser);

            SetModifiedProperties(signatoryContract);
        }


        private void AddSignatoryContract(
            SignatoryContract signatoryContract,
            List<User> users,
            SignatoryContractDTO inputSignatoryContract,
            SignatoryRequestContext context,
            User signedByUser)
        {

            signatoryContract.ActorCompanyId = ActorCompanyId;
            signatoryContract.SignedByUserId = signedByUser.UserId;
            signatoryContract.SignedByIdLoginGuid = signedByUser.idLoginGuid ?? Guid.Empty;
            signatoryContract.RequiredAuthenticationMethodType = inputSignatoryContract.RequiredAuthenticationMethodType;
            signatoryContract.CreationMethodType = 1;

            AddPermissionsToSignatoryContract(inputSignatoryContract, signatoryContract);

            AddRecipientToSignatoryContract(
                signatoryContract,
                inputSignatoryContract,
                users);

            AddChildrenToSignatoryContract(
                inputSignatoryContract, signatoryContract, users, signedByUser);

            SetCreatedProperties(signatoryContract);

            context.Entities.SignatoryContract.AddObject(signatoryContract);
        }

        private void AddChildrenToSignatoryContract(
            SignatoryContractDTO input,
            SignatoryContract signatoryContract,
            List<User> users,
            User signedByUser)
        {
            List<SignatoryContractDTO> newChildrenInputs = input.SubContracts
                .Where(c => c.SignatoryContractId < 0)
                .ToList();

            foreach (SignatoryContractDTO newChildInput in newChildrenInputs)
            {
                SignatoryContract newChild = new SignatoryContract
                {
                    ActorCompanyId = signatoryContract.ActorCompanyId,
                    SignedByUserId = signedByUser.UserId,
                    SignedByIdLoginGuid = signedByUser.idLoginGuid ?? Guid.Empty,
                    RequiredAuthenticationMethodType = signatoryContract.RequiredAuthenticationMethodType,
                    CreationMethodType = 2,
                    ParentSignatoryContractId = signatoryContract.SignatoryContractId,
                    SignatoryContract2 = signatoryContract,
                };

                AddPermissionsToSignatoryContract(newChildInput, newChild);

                AddRecipientToSignatoryContract(newChild, newChildInput, users);

                SetCreatedProperties(newChild);
                signatoryContract.SignatoryContract1.Add(newChild);
            }
        }

        private void AddRecipientToSignatoryContract(
            SignatoryContract signatoryContract,
            SignatoryContractDTO inputSignatoryContract,
            List<User> users)
        {
            User recipientUser = users
                .First(u => u.UserId == inputSignatoryContract.RecipientUserId);
            SignatoryContractRecipient recipient = new SignatoryContractRecipient
            {
                RecipientUserId = inputSignatoryContract.RecipientUserId,
                RecipientIdLoginGuid = recipientUser.idLoginGuid ?? Guid.Empty,
                SignatoryContract = signatoryContract
            };
            signatoryContract.SignatoryContractRecipient.Add(recipient);

        }

        private void AddPermissionsToSignatoryContract(
            SignatoryContractDTO input,
            SignatoryContract signatoryContract)
        {
            List<SignatoryContractPermission> newPermissions = input
                .PermissionTypes
                .Where(pt => !signatoryContract
                    .SignatoryContractPermission
                    .Any(p => p.PermissionType == pt))
                .Distinct()
                .Select(pt => new SignatoryContractPermission
                {
                    PermissionType = pt,
                    SignatoryContract = signatoryContract
                })
                .ToList();

            if (newPermissions.Any())
            {
                signatoryContract.SignatoryContractPermission.AddRange(newPermissions);
            }

        }

        #endregion

        #region Actions
        private AuthenticationDetailsDTO CreateAuthenticationRequest(SignatoryRequestContext context, SignatoryContract signatoryContract, TermGroup_SignatoryContractPermissionType permissionType)
        {
            var authenticationResult = new SignatoryContractAuthenticationRequest()
            {
                UserId = context.UserId,
                SignatoryContractId = signatoryContract.SignatoryContractId,
                AuthenticatedAtUTC = null,
                RequestedAtUTC = DateTime.UtcNow,
                ExpiresAtUTC = DateTime.UtcNow.AddMinutes(5),
                AuthenticationMethodType = signatoryContract.RequiredAuthenticationMethodType,
                ExternalId = Guid.NewGuid()
            };
            context.Entities.SignatoryContractAuthenticationRequest.AddObject(authenticationResult);
            context.Entities.SaveChanges();

            var success = TryStartAuthentication(context, authenticationResult, permissionType);

            if (!success)
            {
                return new AuthenticationDetailsDTO
                {
                    Message = GetText(6055, "Kunde inte påbörja autentisering")
                };
            }

            return new AuthenticationDetailsDTO
            {
                AuthenticationRequestId = authenticationResult.SignatoryContractAuthenticationRequestId,
                AuthenticationMethodType = signatoryContract.GetRequiredAuthenticationMethodType(),
            };
        }
        private AuthenticationResultDTO SetAsAuthenticated(SignatoryRequestContext context, SignatoryContractAuthenticationRequest request)
        {
            context.Logs.Add(SignatoryContractLogType.Info, TermGroup_SignatoryContractPermissionType.Unknown, "Authentication successful");
            request.AuthenticatedAtUTC = DateTime.UtcNow;
            var result = SaveChanges(context.Entities);
            return AuthenticationResultFactory.Success();
        }

        private bool ValidateUserNamePassword(SignatoryRequestContext context, AuthenticationResponseDTO response)
        {
            var guid = SoftOneIdConnector.GetIdLoginGuidUsingUsernameAndPassword(
                context.IdLoginGuid,
                response.Username,
                response.Password);
            return guid == context.IdLoginGuid;
        }

        private bool TryStartAuthentication(SignatoryRequestContext context, SignatoryContractAuthenticationRequest request, TermGroup_SignatoryContractPermissionType permissionType)
        {
            var method = request.GetAuthenticationMethodType();
            if (method == SignatoryContractAuthenticationMethodType.PasswordSMSCode)
            {
                var code = CodeGenerator.FromGuid(request.ExternalId);
                var success = SoftOneIdConnector.SendMessage(
                    context.IdLoginGuid,
                    subject: GetOTPMessage(permissionType),
                    body: $"{code}");
                if (success)
                {
                    context.Logs.Add(SignatoryContractLogType.Info, TermGroup_SignatoryContractPermissionType.Unknown, "Sent SMS with code");
                    return true;

                }
                context.Logs.Add(SignatoryContractLogType.Error, TermGroup_SignatoryContractPermissionType.Unknown, "Could not send SMS");
                return false;
            }
            return true;
        }

        #endregion

        #region Queries
        public SignatoryContractAuthenticationRequest GetAuthenticationRequest(SignatoryRequestContext context, int authRequestId)
        {

            return context.Entities.SignatoryContractAuthenticationRequest
                .Include("SignatoryContract")
                .FirstOrDefault(r => r.SignatoryContractAuthenticationRequestId == authRequestId &&
                                     r.UserId == context.UserId &&
                                     r.SignatoryContract.ActorCompanyId == context.ActorCompanyId &&
                                     r.SignatoryContract.SignatoryContractRecipient.Any(rec =>
                                            rec.RecipientUserId == context.UserId &&
                                            rec.RecipientIdLoginGuid == context.IdLoginGuid)
                                     );
        }

        public SignatoryContractAuthenticationRequest GetActiveContractAuthenticationRequest(SignatoryRequestContext context, int signatoryContractId)
        {
            var now = DateTime.UtcNow;
            return context.Entities.SignatoryContractAuthenticationRequest
                .FirstOrDefault(r => r.SignatoryContractId == signatoryContractId &&
                                     r.UserId == context.UserId &&
                                     r.AuthenticatedAtUTC != null &&
                                     r.ExpiresAtUTC > now);
        }

        private bool UsesSignatoryContractForPermission(SignatoryRequestContext context, TermGroup_SignatoryContractPermissionType permissionType)
        {
            return context.Entities.SignatoryContractPermission
                .Any(p => p.PermissionType == (int)permissionType &&
                                     p.SignatoryContract.ActorCompanyId == context.ActorCompanyId &&
                                     p.SignatoryContract.RevokedAtUTC == null);
        }

        private SignatoryContractPermission GetSignatoryContractPermission(
            SignatoryRequestContext context, AuthorizeRequestDTO authorizeRequest)
        {
            // Get the signatory contract that allows for the specific permission.
            // Order by SignatoryContractId to get the top "contract" in case of multiple contracts.
            // Ensure it has not been revoked.

            return context.Entities.SignatoryContractPermission
                .Include("SignatoryContract")
                .OrderByDescending(r => r.SignatoryContractId)
                .FirstOrDefault(p => (!authorizeRequest.SignatoryContractId.HasValue
                                        || p.SignatoryContractId == authorizeRequest.SignatoryContractId) &&
                                     p.PermissionType == (int)authorizeRequest.PermissionType &&
                                     p.SignatoryContract.RevokedAtUTC == null &&
                                     p.SignatoryContract.ActorCompanyId == context.ActorCompanyId &&
                                     p.SignatoryContract.SignatoryContractRecipient.Any(r =>
                                            r.RecipientUserId == context.UserId &&
                                            r.RecipientIdLoginGuid == context.IdLoginGuid)
                                     );
        }

        private List<SignatoryContract> GetSignatoryContractsGridData(
            SignatoryRequestContext context, int? signatoryContractId)
        {
            return context.Entities.SignatoryContract
                    .Include("SignatoryContractPermission")
                    .Include("SignatoryContractRecipient")
                    .Include("SignatoryContractRecipient.User")
                    .Where(sc => sc.ActorCompanyId == context.ActorCompanyId
                        && !sc.ParentSignatoryContractId.HasValue
                        && (!signatoryContractId.HasValue || sc.SignatoryContractId == signatoryContractId))
                    .ToList();
        }

        private SignatoryContract GetSignatoryContractData(
            int signatoryContractId, SignatoryRequestContext context)
        {
            return context.Entities.SignatoryContract
                .Include("User")
                .Include("SignatoryContractPermission")
                .Include("SignatoryContractRecipient")
                .Include("SignatoryContractRecipient.User")
                .Include("SignatoryContract1")
                .Include("SignatoryContract1.SignatoryContractPermission")
                .Include("SignatoryContract1.SignatoryContractRecipient")
                .Include("SignatoryContract1.SignatoryContractRecipient.User")
                .FirstOrDefault(s => s.SignatoryContractId == signatoryContractId);
        }

        private SignatoryContract GetAllSignatoryContractData(
            int signatoryContractId, SignatoryRequestContext context)
        {
            return context.Entities.SignatoryContract
                .Include("User")
                .Include("SignatoryContractPermission")
                .Include("SignatoryContractRecipient")
                .Include("SignatoryContractRecipient.User")
                .Include("SignatoryContract1")
                .Include("SignatoryContract1.SignatoryContractPermission")
                .Include("SignatoryContract1.SignatoryContractAuthenticationRequest")
                .Include("SignatoryContract1.SignatoryContractLog")
                .Include("SignatoryContract1.SignatoryContractRecipient")
                .Include("SignatoryContract1.SignatoryContractRecipient.User")
                .FirstOrDefault(s => s.SignatoryContractId == signatoryContractId);
        }

        private SignatoryContract GetAllSignatoryContractDataForRevoke(
            int signatoryContractId, SignatoryRequestContext context)
        {
            return context.Entities.SignatoryContract
                .Include("User")
                .Include("SignatoryContract1")
                .Include("SignatoryContractRecipient")
                .FirstOrDefault(s => s.SignatoryContractId == signatoryContractId);
        }

        private List<SignatoryContract> GetSignatoryContractSubContractData(
            int signatoryContractId, SignatoryRequestContext context)
        {
            return context.Entities.SignatoryContract
                .Include("SignatoryContractPermission")
                .Include("User")
                .Include("SignatoryContractRecipient")
                .Include("SignatoryContractRecipient.User")
                .Where(s => s.ParentSignatoryContractId == signatoryContractId)
                .ToList();
        }

        private List<int> GetSignatoryContractPermissionTypes(
            SignatoryRequestContext context, int signatoryContractId)
        {
            return context.Entities.SignatoryContractPermission
                .Where(p => p.SignatoryContractId == signatoryContractId)
                .Select(p => p.PermissionType)
                .ToList();
        }


        #endregion


        #region Terms

        private string GetOTPMessage(TermGroup_SignatoryContractPermissionType permission)
        {
            return GetText(6050, "Bekräfta användandet av behörighet [{0}] med kod")
                .Replace("{0}", GetPermissionName(permission));
        }

        private string GetPermissionName(TermGroup_SignatoryContractPermissionType permission)
        {
            return GetText((int)permission, (int)TermGroup.SignatoryContractPermissionType);
        }

        #endregion

        #region Term Groups

        private Dictionary<int, string> GetAuthenticationMethodTermsDict()
        {
            Dictionary<int, string> authenticationMethodTerms = GetTermGroupDict(
                TermGroup.SignatoryContractAuthenticationMethodType)
                .OrderBy(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            return authenticationMethodTerms;
        }

        private Dictionary<int, string> GetPermissionTermsDict()
        {
            Dictionary<int, string> permissionTerms = GetTermGroupDict(
                TermGroup.SignatoryContractPermissionType)
                .OrderBy(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            return permissionTerms;
        }



        #endregion

        #region Maps

        private List<SignatoryContractGridDTO> MapSignatoryContractsToDTO(
            List<SignatoryContract> signatoryContracts)
        {
            Dictionary<int, string> permissionTerms = GetPermissionTermsDict();
            Dictionary<int, string> authenticationMethodTerms
                = GetAuthenticationMethodTermsDict();

            List<SignatoryContractGridDTO> dtos = signatoryContracts
                .ToGridDTOs(permissionTerms, authenticationMethodTerms)
                .ToList();

            return dtos;
        }

        #endregion
    }

    #region Utility classes
    public static class AuthenticationDetailsFactory
    {
        public static GetPermissionResultDTO Reject(TermGroup_SignatoryContractPermissionType type, string label)
        {
            return new GetPermissionResultDTO
            {
                PermissionType = type,
                PermissionLabel = label,
                HasPermission = false,
                IsAuthorized = false,
                IsAuthenticationRequired = null,
                IsAuthenticated = null,
                AuthenticationDetails = null,
            };
        }
        public static GetPermissionResultDTO Accept_NoAuthenticationRequired(TermGroup_SignatoryContractPermissionType type, string label)
        {
            return new GetPermissionResultDTO
            {
                PermissionType = type,
                PermissionLabel = label,
                HasPermission = true,
                IsAuthorized = true,
                IsAuthenticationRequired = false,
                IsAuthenticated = null,
                AuthenticationDetails = null,
            };
        }
        public static GetPermissionResultDTO Reject_AuthenticationRequired(AuthenticationDetailsDTO authenticationDetails, TermGroup_SignatoryContractPermissionType type, string label)
        {
            return new GetPermissionResultDTO
            {
                PermissionType = type,
                PermissionLabel = label,
                HasPermission = true,
                IsAuthorized = false,
                IsAuthenticated = false,
                IsAuthenticationRequired = true,
                AuthenticationDetails = authenticationDetails,
            };
        }
        public static GetPermissionResultDTO Accept_IsAuthenticated(TermGroup_SignatoryContractPermissionType type, string label)
        {
            return new GetPermissionResultDTO
            {
                PermissionType = type,
                PermissionLabel = label,
                HasPermission = true,
                IsAuthorized = true,
                IsAuthenticated = true,
                IsAuthenticationRequired = true,
                AuthenticationDetails = null,
            };
        }
    }
    public static class AuthenticationResultFactory
    {
        public static AuthenticationResultDTO Success()
        {
            return new AuthenticationResultDTO
            {
                Success = true,
            };
        }
        public static AuthenticationResultDTO Failure(string message)
        {
            return new AuthenticationResultDTO
            {
                Success = false,
                Message = message,
            };
        }
    }
    static class CodeGenerator
    {
        public static string FromGuid(Guid guid)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert GUID to byte array and hash it
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(guid.ToString()));

                // Convert first 8 bytes of the hash to an integer
                long hashInt = BitConverter.ToInt64(hash, 0);

                // Ensure a positive number and take last 6 digits
                return (Math.Abs(hashInt) % 1000000).ToString("D6");
            }
        }
    }

    public enum SignatoryContractLogType
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
    }
    public class Logs
    {
        private List<LogParam> LogParams { get; set; } = new List<LogParam>();
        private int UserId { get; set; }
        private int? SignatoryContractId { get; set; }

        public Logs(int userId)
        {
            this.UserId = userId;
        }

        public void SetSignatoryContractId(int signatoryContractId)
        {
            this.SignatoryContractId = signatoryContractId;
        }

        public void Add(SignatoryContractLogType type, TermGroup_SignatoryContractPermissionType permissionType, string message)
        {
            var logParam = new LogParam(this.UserId, this.SignatoryContractId, permissionType, type, message);
            LogParams.Add(logParam);
        }

        public void Add(SignatoryContractLogType type, string message)
        {
            var logParam = new LogParam(this.UserId, this.SignatoryContractId, TermGroup_SignatoryContractPermissionType.Unknown, type, message);
            LogParams.Add(logParam);
        }

        public List<LogParam> Get()
        {
            return LogParams;
        }
    }
    public class LogParam
    {
        public string Message { get; set; }
        public int UserId { get; set; }
        public int? SignatoryContractId { get; set; }
        public SignatoryContractLogType LogType { get; set; }
        public TermGroup_SignatoryContractPermissionType PermissionType { get; set; }
        public DateTime CreatedAtUTC { get; set; } = DateTime.UtcNow;

        public LogParam(int userId, int? signatoryContractId, TermGroup_SignatoryContractPermissionType permissionType, SignatoryContractLogType logType, string message)
        {
            this.SignatoryContractId = signatoryContractId;
            this.PermissionType = permissionType;
            this.UserId = userId;
            this.LogType = logType;
            this.Message = message;
        }
    }

    public class SignatoryRequestContext : IDisposable
    {
        public int UserId { get; set; }
        public Guid IdLoginGuid { get; set; }
        public int ActorCompanyId { get; set; }
        public CompEntities Entities { get; set; }
        public Logs Logs { get; set; }

        public SignatoryRequestContext(ParameterObject parameterObject) :
            this(parameterObject.UserId, parameterObject.IdLoginGuid.Value, parameterObject.ActorCompanyId, new CompEntities())
        {
        }

        public SignatoryRequestContext(ParameterObject parameterObject, CompEntities entities) :
            this(parameterObject.UserId, parameterObject.IdLoginGuid.Value, parameterObject.ActorCompanyId, entities)
        {
        }

        public SignatoryRequestContext(int userId, Guid idLoginGuid, int actorCompanyId, CompEntities entities)
        {
            this.UserId = userId;
            this.ActorCompanyId = actorCompanyId;
            this.Entities = entities;
            this.IdLoginGuid = idLoginGuid;
            this.Logs = new Logs(userId);
        }

        public void WriteLogs()
        {
            if (this.Logs.Get().IsNullOrEmpty()) return;

            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                foreach (var log in this.Logs.Get())
                {
                    var logRecord = new SignatoryContractLog
                    {
                        ActorCompanyId = this.ActorCompanyId,
                        UserId = log.UserId,
                        SignatoryContractId = log.SignatoryContractId,
                        CreatedAtUTC = log.CreatedAtUTC,
                        LogType = (int)log.LogType,
                        PermissionType = (int)log.PermissionType,
                        Message = log.Message,
                    };
                    this.Entities.SignatoryContractLog.AddObject(logRecord);
                }
                this.Entities.SaveChanges();
                transaction.Complete();
            }
        }

        public void Dispose()
        {
            this.WriteLogs();
            this.Entities.Dispose();
        }
    }
    #endregion
}
