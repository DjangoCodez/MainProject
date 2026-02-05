using Bridge.Shared.Connector;
using Newtonsoft.Json;
using SoftOne.Communicator.Shared.Client;
using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core.SoftOneAI;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Communicator;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using SoftOne.Soe.Shared.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core
{
    public class ApiManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConnectUtil connectUtil;

        #endregion

        #region Ctor

        public ApiManager(ParameterObject parameterObject) : base(parameterObject)
        {
            this.connectUtil = new ConnectUtil(null);
        }

        #endregion

        #region Validation

        public ParameterObject GetParameterObject()
        {
            return this.parameterObject;
        }

        public bool ValidateToken(Guid companyApiKey, Guid connectApiKey, string token, out string validatationResult, Dictionary<string, string> headers = null)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    if (!headers.IsNullOrEmpty())
                        token = headers.FirstOrDefault(f => f.Key == "SOEAuthToken").Value?.ToString();

                    if (string.IsNullOrEmpty(token))
                    {
                        validatationResult = GetText(11913, "Anropet saknar Token");
                        return false;
                    }
                }

                var userId = 0;
                var tokenResult = connectUtil.ValidateToken(companyApiKey.ToString(), connectApiKey.ToString(), token, ref userId);
                if (!tokenResult.Success)
                {
                    validatationResult = GetText(11914, "Det token som skickades innehåller ogiltig information");
                    if (!string.IsNullOrEmpty(tokenResult.ErrorMessage))
                        validatationResult += $". {tokenResult.ErrorMessage}";

                    LogInfo($"API: ValidateToken found invalid token information for companyApiKey ( {companyApiKey} ) and connectApiKey ( {connectApiKey} ) userId {userId} token: {token}");
                    return false;
                }

                validatationResult = connectUtil.ValidateToken(token, companyApiKey, out ParameterObject localParameterObject, out int _);
                if (!string.IsNullOrEmpty(validatationResult))
                {
                    LogInfo($"API: ValidateToken failed with message: {validatationResult} for ( {companyApiKey} ) and connectApiKey ( {connectApiKey} ) userId {userId} token: {token}");
                    return false;
                }

                string companyKeyCheck = connectUtil.ValidateCompanyApiKeyAndActorCompanyId(companyApiKey, localParameterObject.ActorCompanyId);
                if (!string.IsNullOrEmpty(companyKeyCheck))
                {
                    validatationResult = GetText(11915, "Anropet innehåller ogiltiga nycklar");
                    LogInfo(companyKeyCheck);
                    return false;
                }

                this.parameterObject = localParameterObject;

                if (this.parameterObject.SoeUser?.LangId != null && this.parameterObject.SoeUser.LangId.Value != (int)TermGroup_Languages.Unknown)
                    SetLanguage(this.parameterObject.SoeUser.LangId.Value);

                return string.IsNullOrEmpty(validatationResult);
            }
            catch (Exception ex)
            {
                validatationResult = GetText(11914, "Det token som skickades innehåller ogiltig information") + "_";
                LogError(ex, log);
                return false;
            }
        }

        public bool ValidateToken(Guid companyApiKey, Guid connectApiKey, string token, Feature feature, out string validatationResult)
        {
            var result = ValidateToken(companyApiKey, connectApiKey, token, out validatationResult);
            if (result)
            {
                result = CheckPermission(feature);
                if (!result)
                {
                    validatationResult = GetText(11916, "Användaren saknar behörighet för funktionen") + $": {(int)feature}";
                    LogInfo($"Not enough permission ( {feature} ) for user {parameterObject.UserId} for the specific api call");
                }
            }
            return result;
        }

        public bool CheckPermission(Feature feature)
        {
            if (parameterObject == null)
                return false;

            var featureManager = new FeatureManager(parameterObject);
            return featureManager.HasRolePermission(feature, Permission.Modify, parameterObject.RoleId, parameterObject.ActorCompanyId, parameterObject.LicenseId);
        }

        #endregion

        #region ApiMessage



        public TResult ExecuteWithApiLogging<TResult>(
            int actorCompanyId,
            SoeEntityType entityType,
            TermGroup_ApiMessageType messageType,
            TermGroup_ApiMessageSourceType sourceType,
            Func<TResult> operation,
            object dataForLogging,
            int recordCount,
            string comment = null)
        {
            // Initialize API message
            ApiMessageDTO dto = new ApiMessageDTO(entityType, messageType, sourceType);
            ApiMessage apiMessage = null;

            try
            {
                // Try to initialize API message
                TryInitApiMessage(dto, dataForLogging, out apiMessage);
                TryUpdateApiMessage(dto, apiMessage, TermGroup_ApiMessageStatus.Processing, recordCount, comment);

                // Execute operation
                TResult result = operation();

                // Determine success (if TResult is ActionResult, use its Success flag)
                bool isSuccess = result is ActionResult actionResult ? actionResult.Success : result != null;

                // Finalize API logging
                TryUpdateApiMessage(dto, apiMessage, isSuccess ? TermGroup_ApiMessageStatus.Processed : TermGroup_ApiMessageStatus.Error);

                return result;
            }
            catch (Exception ex)
            {
                // Log and update API message in case of failure
                TryUpdateApiMessage(dto, apiMessage, TermGroup_ApiMessageStatus.Error);
                LogError(ex, log);

                // If TResult is ActionResult, return an error response; otherwise, return default
                if (typeof(TResult) == typeof(ActionResult))
                    return (TResult)(object)new ActionResult { Success = false, ErrorMessage = ex.Message };

                return default;
            }
        }

        public ActionResult TryInitApiMessage(ApiMessageDTO dto, object data, out ApiMessage apiMessage)
        {
            apiMessage = null;

            using (CompEntities entities = new CompEntities())
            {
                apiMessage = new ApiMessage()
                {
                    CompanyApiKey = dto.CompanyApiKey.ToString(),
                    ConnectApiKey = dto.ConnectApiKey.ToString(),
                    EntityType = (int)dto.EntityType,
                    Type = (int)dto.Type,
                    Status = (int)TermGroup_ApiMessageStatus.Initialized,
                    Message = SerializeApiData(data),
                    SourceType = (int)dto.SourceType,

                    //Set FK
                    ActorCompanyId = base.ActorCompanyId,
                };
                SetCreatedProperties(apiMessage);
                entities.ApiMessage.AddObject(apiMessage);
                return SaveChanges(entities);
            }
        }

        public ActionResult TryUpdateApiMessage(ApiMessageDTO dto, ApiMessage inputApiMessage, TermGroup_ApiMessageStatus status, int? recordCount = null, string comment = null)
        {
            if (inputApiMessage == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ApiMessage");

            using (CompEntities entities = new CompEntities())
            {
                ApiMessage apiMessage = ApiDataManager.GetApiMessage(entities, inputApiMessage.ApiMessageId);
                if (apiMessage == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ApiMessage");

                apiMessage.Status = (int)status;
                if (recordCount.HasValue)
                    apiMessage.RecordCount = recordCount.Value;
                if (comment != null)
                    apiMessage.Comment = comment.Left(512);
                SetModifiedProperties(apiMessage);

                var result = SaveChanges(entities);
                if (result.Success)
                    dto.UpdateStatus(status);

                return result;
            }
        }

        private ActionResult ValidApiMessageFile(TermGroup_ApiMessageType type, byte[] file, out string json)
        {
            json = null;

            if (file == null)
                return new ActionResult((int)ActionResultSave.IncorrectInput, "File not found");

            SoeEntityType entityType = GetApiMessageEntityType(type);
            if (entityType == SoeEntityType.None)
                return new ActionResult((int)ActionResultSave.IncorrectInput, "Invalid type");

            Stream stream = new MemoryStream(file)
            {
                Position = 0
            };
            using (StreamReader sr = new StreamReader(stream))
            {
                json = sr.ReadToEnd();
            }
            if (string.IsNullOrEmpty(json))
                return new ActionResult((int)ActionResultSave.IncorrectInput, "Invalid file");

            return new ActionResult(true);
        }

        public void ApiMessageFailedValidation(ApiMessageDTO dto, object data)
        {
            if (TryInitApiMessage(dto, data, out ApiMessage apiMessage).Success)
                TryUpdateApiMessage(dto, apiMessage, TermGroup_ApiMessageStatus.FailedValidation);
        }

        private string SerializeApiData(object data)
        {
            try
            {
                return JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }).ToString();
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return ex.ToString();
            }
        }

        private SoeEntityType GetApiMessageEntityType(TermGroup_ApiMessageType type)
        {
            switch (type)
            {
                case TermGroup_ApiMessageType.Employee:
                    return SoeEntityType.Employee;
                default:
                    return SoeEntityType.None;
            }
        }

        #endregion

        #region API - EmployeeChanges

        #region Entry points

        public EmployeeChangeResult ImportEmployeeChangesExtensive(ApiMessageDTO dto, List<EmployeeChangeIODTO> employeeChanges, out ActionResult result, bool saveChangesOnValidationErrors = true)
        {
            result = ImportEmployeeChanges(dto, employeeChanges, out EmployeeUserImportBatch batch, saveChangesOnValidationErrors: saveChangesOnValidationErrors);
            result.Value = batch;

            EmployeeChangeResult employeeChangeResult = result.Success ?
                new EmployeeChangeResult(employeeChanges) :
                new EmployeeChangeResult(GetText(11964, "Inläsning misslyckades"));

            if (!string.IsNullOrEmpty(employeeChangeResult.InvalidMessage))
                result.ErrorMessage += employeeChangeResult.InvalidMessage;

            return employeeChangeResult;
        }

        public ActionResult ImportEmployeeChangesFromFile(TermGroup_ApiMessageType type, TermGroup_ApiMessageSourceType sourceType, byte[] file)
        {
            ActionResult result = ValidApiMessageFile(type, file, out string json);
            if (!result.Success)
                return result;

            try
            {
                ApiMessageDTO dto = new ApiMessageDTO(SoeEntityType.Employee, type, sourceType);
                List<EmployeeChangeIODTO> employeeChanges = JsonConvert.DeserializeObject<List<EmployeeChangeIODTO>>(json);
                ImportEmployeeChangesExtensive(dto, employeeChanges, out result);
                return result;
            }
            catch
            {
                return new ActionResult((int)ActionResultSave.IncorrectInput);
            }
        }

        public EmployeeChangeResult ImportEmployeeChangesFromMassUpdate(List<EmployeeChangeIODTO> employeeChanges, out ActionResult result)
        {
            ApiMessageDTO dto = new ApiMessageDTO(SoeEntityType.Employee, TermGroup_ApiMessageType.Employee, TermGroup_ApiMessageSourceType.MassUpdateEmployeeFields);
            return ApiManager.ImportEmployeeChangesExtensive(dto, employeeChanges, out result);
        }

        public EmployeeChangeResult ImportEmployeeChangesFromEmployeeTemplate(EmployeeChangeIODTO employeeChange, out ActionResult result)
        {
            ApiMessageDTO dto = new ApiMessageDTO(SoeEntityType.Employee, TermGroup_ApiMessageType.Employee, TermGroup_ApiMessageSourceType.EmployeeTemplate);
            return ApiManager.ImportEmployeeChangesExtensive(dto, new List<EmployeeChangeIODTO> { employeeChange }, out result, saveChangesOnValidationErrors: false);
        }

        public EmployeeChangeResult ImportEmployeeChangesFromBridge(List<EmployeeChangeIODTO> employeeChanges, List<ScheduledJobSetting> settings, out ActionResult result)
        {
            DateTime start = DateTime.Now;
            ApiMessageDTO dto = new ApiMessageDTO(SoeEntityType.Employee, TermGroup_ApiMessageType.Employee, TermGroup_ApiMessageSourceType.API);
            result = new ActionResult();

            while (employeeChanges.Any())
            {
                var batch = employeeChanges.Take(300).ToList();
                employeeChanges = employeeChanges.Where(w => !batch.Contains(w)).ToList();

                if (!employeeChanges.Any())
                {
                    if (settings.Any())
                        IntepretErrorsInImport(start, settings);

                    return ApiManager.ImportEmployeeChangesExtensive(dto, batch, out result);
                }

                ApiManager.ImportEmployeeChangesExtensive(dto, batch, out result);
            }

            if (settings.Any())
                IntepretErrorsInImport(start, settings);
            return new EmployeeChangeResult();
        }

        public void IntepretErrorsInImport(DateTime from, List<ScheduledJobSetting> settings)
        {
            var messages = ApiDataManager.GetApiMessages(base.ActorCompanyId, TermGroup_ApiMessageType.Employee, DateTime.Now.AddHours(-3), DateTime.Now);
            var filteredMessages = messages.Where(w => w.Created >= from && string.IsNullOrEmpty(w.CompanyApiKey) && string.IsNullOrEmpty(w.ConnectApiKey)).ToList();
            var employeeName = "Anna Andersson";
            var companyName = CompanyManager.GetCompany(base.ActorCompanyId, true)?.Name ?? "N/A";

            foreach (var msg in filteredMessages)
            {
                foreach (var detail in msg.ApiMessageChange.GroupBy(g => (g.Identifier, g.RecordId)))
                {
                    if (!detail.Any(a => !string.IsNullOrEmpty(a.Error)))
                        continue;

                    StringBuilder prompt = new StringBuilder();

                    prompt.AppendLine("You are investigating HR master-data import errors into a Time Attendance system. The logs contain errors that require human interpretation.");
                    prompt.AppendLine("You are an expert in both systems: our Time Attendance system (target) and Visma HR+ (master).");
                    prompt.AppendLine("Goal: Explain WHY the error happens, identify which system is wrong, and give concrete steps to fix it (exact dates/records to adjust).");
                    prompt.AppendLine("");
                    prompt.AppendLine("Rules / assumptions:");
                    prompt.AppendLine("1) Visma HR+ is the master. If Visma HR+ is logically consistent, then our system's data is assumed to be wrong and should be corrected by re-sync (not manual edits).");
                    prompt.AppendLine("2) In Visma HR+: EmploymentAgreement must be fully within its Employment (Agreement.From >= Employment.From AND Agreement.To <= Employment.To, with open-ended allowed).");
                    prompt.AppendLine("3) In our system: Employments must NOT overlap in time. Open-ended employments overlap any later employment start date unless the earlier one has an end date.");
                    prompt.AppendLine("");
                    prompt.AppendLine("Input below includes: (A) error messages, (B) employments in our system, (C) employments + agreements in Visma HR+.");
                    prompt.AppendLine("");
                    prompt.AppendLine("Your output MUST be short and action-oriented, and follow this exact format:");
                    prompt.AppendLine("");
                    prompt.AppendLine("1) Swedish (first) – use clear Swedish payroll/HR wording.");
                    prompt.AppendLine("   - Orsak (1–2 meningar): explain the cause based on dates.");
                    prompt.AppendLine("   - Åtgärd i Visma HR+ (bullet list): what to change/check in master, with the specific employment/agreement dates referenced.");
                    prompt.AppendLine("   - Åtgärd i vårt system (bullet list): what will be corrected by sync and what (if anything) to clean up (e.g., remove/close overlapping record).");
                    prompt.AppendLine("   - Kontroll efter fix (1–2 bullets): what to verify after next import.");
                    prompt.AppendLine("");
                    prompt.AppendLine("2) English version – same structure, concise.");
                    prompt.AppendLine("");
                    prompt.AppendLine("Important requirements:");
                    prompt.AppendLine("- Do NOT give generic advice. Refer to the exact records by their DateFrom/DateTo and explain which ones overlap or violate rules.");
                    prompt.AppendLine("- If you suspect the Visma HR+ setup is illogical, say exactly what is illogical and how to correct it in Visma HR+.");
                    prompt.AppendLine("- If Visma HR+ is logical, explicitly say: 'Fix is needed in our system via sync/cleanup' and specify what record(s) are wrong in our system.");
                    prompt.AppendLine("- If the error text is Swedish, interpret it and map it to the rule that is violated.");
                    prompt.AppendLine("- Double check dates and logic; do NOT make assumptions.");
                    prompt.AppendLine("- Keep the total answer under ~600 characters per language.");
                    prompt.AppendLine("");

                    // Errors
                    StringBuilder errors = new StringBuilder();
                    errors.AppendLine("These are the errors from the logged import:");
                    foreach (var error in detail.Where(w => !string.IsNullOrEmpty(w.Error)))
                    {
                        errors.AppendLine($"- {error.Error}");
                    }
                    prompt.AppendLine(errors.ToString());

                    // Employee + our system employments
                    EmployeeUserDTO employeeUser = ActorManager.GetEmployeeUserDTOFromEmployee(detail.Key.RecordId.Value, base.ActorCompanyId, loadEmployeeAccounts: true, fromApi: true);

                    prompt.AppendLine($"Employee: {employeeName} (EmployeeNr: {employeeUser.EmployeeNr})");
                    prompt.AppendLine("Our system – current employments:");
                    foreach (var item in employeeUser.Employments)
                    {
                        prompt.AppendLine($"- From {item.DateFrom.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} To {item.DateTo?.ToShortDateString()} | EmployeeGroup: {item.EmployeeGroupName}");
                    }

                    // Visma employments
                    var vismaEmployments = BridgeManager.GetVismaPayrollEmploymentsForEmployeeNr(employeeUser.EmployeeNr, settings);

                    prompt.AppendLine("");
                    prompt.AppendLine("Master system: Visma HR+ (Employment + EmploymentAgreement; Agreement must be within Employment).");
                    prompt.AppendLine("Visma HR+ – current employments:");
                    foreach (var item in vismaEmployments.Where(w => w.EmployeeNr == employeeUser.EmployeeNr))
                    {
                        prompt.AppendLine($"- Employment: From {item.StartDate.ToShortDateString()} To {item.EndDate.ToShortDateString()}");

                        foreach (var agreement in item.VismaPayrollEmploymentagreements)
                        {
                            // Prefer EndDate, otherwise TemporaryEmploymentEndDate, otherwise open-ended
                            DateTime? agreementTo = agreement.EndDate ?? agreement.TemporaryEmploymentEndDate;
                            prompt.AppendLine($"  - Agreement: From {agreement.StartDate?.ToShortDateString()} To {agreement.EndDate?.ToShortDateString()}");
                        }
                    }

                    // Close instruction
                    prompt.AppendLine("");
                    prompt.AppendLine("Now produce the output exactly in the required format.");

                    AIManager aIManager = new AIManager(this.GetParameterObject());
                    string aiResponse = aIManager.SimpleQuery(prompt.ToString());

                    // Keep your existing replacement
                    aiResponse = aiResponse.Replace(employeeName, employeeUser.Name);

                    CommunicatorConnector.SendCommunicatorMessage(new CommunicatorMessage()
                    {
                        Sender = new CommunicatorPerson() { Email = "noreply@softone.se" },
                        Recievers = new List<CommunicatorPerson>() { new CommunicatorPerson() { Email = "Rickard.Dahlgren@softone.se" }, new CommunicatorPerson() { Email = "support@softone.se" } },
                        Subject = $"{companyName} : [AI beta] Guidance to fix import errors for employee {employeeUser.Name} ({employeeUser.EmployeeNr})",
                        Body = aiResponse
                    });
                }
            }
        }

        public EmployeeUserImportBatch ImportEmployeeChangesFromTest(EmployeeChangeIOItem container, List<EmployeeUserDTO> employeeUsers)
        {
            container.SetAsTest();
            return CreateEmployeeUserImportBatch(container, employeeUsers, saveChanges: false);
        }

        public ActionResult ImportEmployeeChanges(ApiMessageDTO dto, List<EmployeeChangeIODTO> employeeChanges, out EmployeeUserImportBatch batch, bool isLegacyMode = false, bool saveChangesOnValidationErrors = true)
        {
            ActionResult result;
            ApiMessage apiMessage = null;
            batch = null;

            try
            {
                result = TryInitApiMessage(dto, employeeChanges, out apiMessage);
                if (result.Success)
                {
                    result = TryUpdateApiMessage(dto, apiMessage, TermGroup_ApiMessageStatus.Processing);
                    if (result.Success)
                    {
                        batch = CreateEmployeeUserImportBatch(employeeChanges, saveChanges: dto.SourceType != TermGroup_ApiMessageSourceType.APIManualOnlyLogging, saveChangesOnValidationErrors: saveChangesOnValidationErrors);
                        if (batch != null)
                            batch.IsLegacyMode = isLegacyMode;

                        result = SaveApiMessageChanges(apiMessage, batch, employeeChanges);
                        if (result.Success)
                        {
                            ActionResult resultApiMessage = TryUpdateApiMessage(dto, apiMessage, TermGroup_ApiMessageStatus.Processed, recordCount: batch?.Imports.Count, comment: batch?.Result.ErrorMessage.EmptyToNull());
                            if (!resultApiMessage.Success)
                                base.LogError("TryUpdateApiMessage failed: " + resultApiMessage.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(11964, "Inläsning misslyckades"));
                TryUpdateApiMessage(dto, apiMessage, TermGroup_ApiMessageStatus.Error, comment: result.ErrorMessage.EmptyToNull());
            }

            return result;
        }

        #endregion

        #region Language

        public List<GenericType> GetApiTerms(TermGroup termGroup, int langId)
        {
            return GetTermGroupContent(termGroup, langId: langId);
        }

        private Dictionary<TermGroup, List<GenericType>> GetEmployeeTermsByGroupFromCache(List<EmployeeChangeIODTO> employeeChanges)
        {
            int langId = GetLangId();

            Dictionary<TermGroup, List<GenericType>> terms = new Dictionary<TermGroup, List<GenericType>>()
            {
                { TermGroup.ApiEmployee, GetTermGroupContent(TermGroup.ApiEmployee) }
            };
            AddTerm(EmployeeChangeType.PayrollStatisticsPersonalCategory);
            AddTerm(EmployeeChangeType.PayrollStatisticsWorkTimeCategory);
            AddTerm(EmployeeChangeType.PayrollStatisticsSalaryType);
            AddTerm(EmployeeChangeType.AFACategory);
            AddTerm(EmployeeChangeType.AFASpecialAgreement);
            AddTerm(EmployeeChangeType.CollectumITPPlan);
            AddTerm(EmployeeChangeType.KPABelonging);
            AddTerm(EmployeeChangeType.KPAEndCode);
            AddTerm(EmployeeChangeType.KPAAgreementType);
            AddTerm(EmployeeChangeType.GTPAgreementNumber);

            return terms;

            void AddTerm(EmployeeChangeType employeeChangeType)
            {
                if (ContainsAnyRow(employeeChanges, employeeChangeType))
                {
                    TermGroup termGroup = GetEmployeeTermGroup(employeeChangeType);
                    terms.Add(termGroup, GetApiTerms(termGroup, langId));
                }
            }
        }

        public TermGroup GetEmployeeTermGroup(EmployeeChangeType employeeChangeType)
        {
            switch (employeeChangeType)
            {
                case EmployeeChangeType.PayrollStatisticsPersonalCategory:
                    return TermGroup.PayrollReportsPersonalCategory;
                case EmployeeChangeType.PayrollStatisticsWorkTimeCategory:
                    return TermGroup.PayrollReportsWorkTimeCategory;
                case EmployeeChangeType.PayrollStatisticsSalaryType:
                    return TermGroup.PayrollReportsSalaryType;
                case EmployeeChangeType.AFACategory:
                    return TermGroup.PayrollReportsAFACategory;
                case EmployeeChangeType.AFASpecialAgreement:
                    return TermGroup.PayrollReportsAFASpecialAgreement;
                case EmployeeChangeType.CollectumITPPlan:
                    return TermGroup.PayrollReportsCollectumITPplan;
                case EmployeeChangeType.KPABelonging:
                    return TermGroup.KPABelonging;
                case EmployeeChangeType.KPAEndCode:
                    return TermGroup.KPAEndCode;
                case EmployeeChangeType.KPAAgreementType:
                    return TermGroup.KPAAgreementType;
                case EmployeeChangeType.GTPAgreementNumber:
                    return TermGroup.GTPAgreementNumber;
                case EmployeeChangeType.IFPaymentCode:
                    return TermGroup.IFPaymentCode;
                case EmployeeChangeType.BygglosenSalaryType:
                    return TermGroup.BygglosenSalaryType;

                default:
                    return TermGroup.Unknown;
            }
        }

        public string GetEmployeeTerm(int field, int langId)
        {
            string term = GetText(field, TermGroup.EmployeeChangeFieldType, langId);
            if (term.IsNullOrEmpty())
                term = field.ToString();
            return term;
        }

        #endregion

        #region Workers

        private EmployeeUserImportBatch CreateEmployeeUserImportBatch(List<EmployeeChangeIODTO> employeeChanges, bool addOnlyNew = false, bool saveChanges = true, bool saveChangesOnValidationErrors = true)
        {
            if (employeeChanges == null)
                return new EmployeeUserImportBatch(new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11890, "Inget data skickades")));

            CompanyDTO company = CompanyManager.GetCompany(base.ActorCompanyId, true)?.ToCompanyDTO();
            if (company == null)
                return new EmployeeUserImportBatch(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

            EmployeeChangeIOItem container = CreateEmployeeChangeContainer(employeeChanges);
            List<EmployeeUserDTO> employeeUsers = GetEmployeeUsers(container, company, addOnlyNew);

            return CreateEmployeeUserImportBatch(container, employeeUsers, saveChanges, saveChangesOnValidationErrors);
        }

        /// <summary>
        /// IMPORTANT! This method is part of unit tests and should not be making any database calls
        /// </summary>
        private EmployeeUserImportBatch CreateEmployeeUserImportBatch(EmployeeChangeIOItem container, List<EmployeeUserDTO> employeeUsers, bool saveChanges = true, bool saveChangesOnValidationErrors = true)
        {
            if (container?.EmployeeChangeIODTOs == null)
                return new EmployeeUserImportBatch();

            EmployeeUserImportBatch batch = new EmployeeUserImportBatch(container.EmployeeChangeIODTOs.Count);

            foreach (EmployeeChangeIODTO employeeChange in container.EmployeeChangeIODTOs)
            {
                EmployeeUserDTO employeeUser = employeeUsers?.FirstOrDefault(i => i.EmployeeNr == employeeChange.EmployeeNr || (!string.IsNullOrEmpty(employeeChange.EmployeeExternalCode) && i.ExternalCode == employeeChange.EmployeeExternalCode));
                if (employeeUser == null || batch.Imports.Any(i => i.EmployeeUser?.EmployeeNr == employeeChange.EmployeeNr))
                    continue;

                EmployeeUserImport import = CreateEmployeeUserImport(container, employeeChange, employeeUser, saveChanges, saveChangesOnValidationErrors);
                if (import == null)
                    continue;

                batch.Imports.Add(import);
                batch.ValidationErrors.AddRange(employeeChange.GetValidationErrorStrings());
            }

            return batch;
        }

        /// <summary>
        /// IMPORTANT! This method is part of unit tests and should not be making any database calls
        /// </summary>
        private EmployeeUserImport CreateEmployeeUserImport(EmployeeChangeIOItem container, EmployeeChangeIODTO employeeChange, EmployeeUserDTO employeeUser, bool saveChanges, bool saveChangesOnValidationErrors)
        {
            if (container == null || employeeChange == null || employeeUser == null)
                return null;

            EmployeeUserImport import = new EmployeeUserImport(employeeUser);

            if (employeeUser.IsNew)
                employeeChange.AddMandatoryFields();

            bool dontValidateSocialSecNbr = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeDontValidateSocialSecNbr, 0, base.ActorCompanyId, 0);
            if (dontValidateSocialSecNbr && employeeChange.EmployeeChangeRowIOs.Any(a => a.EmployeeChangeType == EmployeeChangeType.SocialSec))
                employeeChange.EmployeeChangeRowIOs.Where(a => a.EmployeeChangeType == EmployeeChangeType.SocialSec && string.IsNullOrEmpty(a.OptionalExternalCode)).ToList().ForEach(f => f.OptionalExternalCode = "force");

            List<EmployeeChangeRowValidation> validationErrors = employeeChange.Validate(container.GetTerms(), container.GetPermissions(), container.GetCompanyCountry());
            if (!validationErrors.IsNullOrEmpty())
                validationErrors.ForEach(validationError => employeeUser.AddErrorChange((int)validationError.Type, validationError.Message));

            container.ApplyChangesToEmployee(import);

            bool hasChanges = import.HasChanges();
            bool hasValidationErrors = !validationErrors.IsNullOrEmpty();

            if (hasChanges && saveChanges && (!hasValidationErrors || saveChangesOnValidationErrors))
                SaveChangesToEmployeeUser(ref import, container, employeeChange);
            else if (!hasChanges && !hasValidationErrors)
                AddNothingChangedError(import, container);

            return import;
        }

        private EmployeeChangeIOItem CreateEmployeeChangeContainer(List<EmployeeChangeIODTO> employeeChanges)
        {
            return new EmployeeChangeIOItem(employeeChanges, this.GetEmployeeLookup(employeeChanges), this.GetApiPermissions(employeeChanges));
        }

        private ApiLookupEmployee GetEmployeeLookup(List<EmployeeChangeIODTO> employeeChanges)
        {
            ApiConfig config = new ApiConfig(10 * 60, this.GetApiSettings());
            ApiLookupEmployee lookup = new ApiLookupEmployee(
                config,
                base.ActorCompanyId,
                this.GetDefaultRoleId(),
                this.GetDefaultEmployeeGroupId(),
                this.GetDefaultPayrollGroupId(),
                this.GetDefaultVacationGroupId(),
                this.GetCompanyCountryId(),
                this.GetEmployeeGroupsFromCache(config),
                this.GetPayrollGroupsFromCache(config),
                this.GetVacationGroupsFromCache(config),
                this.GetAnnualLeaveGroupsFromCache(config));
            lookup.SetOptionalLookups(
                this.GetEmployeeTermsByGroupFromCache(employeeChanges),
                this.GetAccountDimsFromCache(config, employeeChanges),
                this.GetAccountsInternalsFromCache(config, employeeChanges),
                this.GetTimeAttestRolesFromCache(config, employeeChanges),
                this.GetContactAddressesTypes(config, employeeChanges),
                this.GetContactEComsTypesFromCache(config, employeeChanges),
                this.GetPositionsFromCache(config, employeeChanges),
                this.GetEndReasonsFromCache(config, employeeChanges),
                this.GetEmploymentTypesFromCache(config, employeeChanges),
                this.GetExtraFields(config, employeeChanges),
                this.GetExtraFieldRecords(config, employeeChanges),
                this.GetPayrollPriceTypesFromCache(config, employeeChanges),
                this.GetPayrollLevelsFromCache(config, employeeChanges),
                this.GetPayrollPriceFormulasFromCache(config, employeeChanges),
                this.GetUserRolesFromCache(config, employeeChanges),
                this.GetTimeDeviationCausesFromCache(config, employeeChanges),
                this.GetTimeWorkAccountsFromCache(config, employeeChanges));
            return lookup;
        }

        private List<EmployeeUserDTO> GetEmployeeUsers(EmployeeChangeIOItem container, CompanyDTO company, bool addOnlyNew)
        {
            if (container == null || container.EmployeeChangeIODTOs.IsNullOrEmpty())
                return new List<EmployeeUserDTO>();

            bool doSuggestEmployeeNrAsUsername = SettingManager.GetCompanyBoolSetting(CompanySettingType.TimeSuggestEmployeeNrAsUsername);

            List<EmployeeUserDTO> employeeUsers = new List<EmployeeUserDTO>();
            foreach (EmployeeChangeIODTO io in container.EmployeeChangeIODTOs)
            {
                EmployeeUserDTO employeeUser = GetEmployeeUser(container, io, company, addOnlyNew, doSuggestEmployeeNrAsUsername);
                if (employeeUser == null)
                    continue;

                if (io.DoLoadContactAddresses())
                    container.TryAddContactAddressItems(employeeUser.EmployeeNr, ContactManager.GetContactAddressItems(employeeUser.ActorContactPersonId, address: null));
                if (io.DoLoadEmployeePositions())
                    container.TryAddEmployeePositions(employeeUser.EmployeeNr, EmployeeManager.GetEmployeePositions(employeeUser.EmployeeId).ToDTOs().ToList());
                if (io.DoLoadExtraFieldRecords())
                    container.TryAddExtraFieldRecords(employeeUser.EmployeeNr, ExtraFieldManager.GetExtraFieldRecords(employeeUser.EmployeeId, (int)SoeEntityType.Employee, ActorCompanyId).ToDTOs());

                employeeUsers.Add(employeeUser);
            }
            return employeeUsers;
        }

        private EmployeeUserDTO GetEmployeeUser(EmployeeChangeIOItem container, EmployeeChangeIODTO io, CompanyDTO company, bool addOnlyNew, bool doSuggestEmployeeNrAsUsername)
        {
            if (company == null || io == null || io.EmployeeNr.IsNullOrEmpty())
                return null;

            Employee employee = null;
            if (!string.IsNullOrEmpty(io.EmployeeExternalCode))
                employee = EmployeeManager.GetEmployeeByExternalCode(io.EmployeeExternalCode, company.ActorCompanyId, onlyActive: false);
            if (employee == null && !string.IsNullOrEmpty(io.EmployeeNr))
                employee = EmployeeManager.GetEmployeeByNr(io.EmployeeNr, company.ActorCompanyId, onlyActive: false);

            if (employee != null && addOnlyNew)
                return null;

            EmployeeUserDTO employeeUser = null;
            if (employee != null)
                employeeUser = ActorManager.GetEmployeeUserDTOFromEmployee(employee.EmployeeId, company.ActorCompanyId,
                    loadEmploymentAccounting: io.DoLoadEmploymentAccounts() || container.GetBoolSetting(TermGroup_ApiSettingType.KeepEmploymentAccount),
                    loadEmploymentPriceTypes: io.DoLoadEmploymentPriceTypes() || container.GetBoolSetting(TermGroup_ApiSettingType.KeepEmploymentPriceType),
                    loadEmploymentVacationGroups: true,
                    loadEmploymentVacationGroupSE: false,
                    loadEmployeeAccounts: true,
                    loadEmployeeTimeWorkAccounts: io.DoLoadEmployeeTimeWorkAccounts(),
                    loadRoles: io.DoLoadRoles(),
                    loadExternalAuthId: io.DoLoadUserExternalAuth(),
                    fromApi: true
                    );

            if (employeeUser == null)
                employeeUser = new EmployeeUserDTO()
                {
                    DefaultActorCompanyId = company.ActorCompanyId,
                    ActorCompanyId = company.ActorCompanyId,
                    LicenseId = company.LicenseId,
                    EmployeeNr = io.EmployeeNr,
                    LoginName = io.EmployeeNr,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = string.Empty,
                    IsNew = true,
                    IsMobileUser = true,
                    State = SoeEntityState.Active,
                };

            employeeUser.SavingFromApi = true;

            if (employeeUser.UserId == 0 || (doSuggestEmployeeNrAsUsername && employeeUser.IsNew))
                employeeUser.UserId = UserManager.GetUserOnLicense(company.LicenseNr, employeeUser.EmployeeNr, employeeUser.IsNew)?.UserId ?? 0;

            return employeeUser;
        }

        private ApiMessageChange ConvertToApiMessageChange(EmployeeChangeDTO change, EmployeeUserDTO employee)
        {
            return new ApiMessageChange()
            {
                Type = (int)TermGroup_ApiMessageChangeType.Employee,
                FieldType = change.FieldType,
                Identifier = employee.EmployeeNr,
                RecordId = employee.EmployeeId,
                FromDate = change.FromDate,
                ToDate = change.ToDate,
                FromValue = change.FromValue,
                ToValue = change.ToValue,
                FromValueName = change.FromValueName,
                ToValueName = change.ToValueName,
                Error = change.Error,
            };
        }

        private ApiMessageChange ConvertToApiMessageChange(EmploymentChangeDTO change, EmployeeUserDTO employee)
        {
            return new ApiMessageChange()
            {
                Type = (int)TermGroup_ApiMessageChangeType.Employment,
                FieldType = (int)change.FieldType,
                Identifier = employee.EmployeeNr,
                RecordId = employee.EmployeeId,
                FromDate = change.FromDate,
                ToDate = change.ToDate,
                FromValue = change.FromValue,
                ToValue = change.ToValue,
                FromValueName = change.FromValueName,
                ToValueName = change.ToValueName
            };
        }

        private bool ContainsAnyRow(List<EmployeeChangeIODTO> employeeChanges, params EmployeeChangeType[] changeTypes)
        {
            if (employeeChanges.IsNullOrEmpty() || changeTypes.IsNullOrEmpty())
                return false;

            return employeeChanges.Any(employeeChange => employeeChange.ContainsAnyType(changeTypes));
        }

        /// <summary>
        /// Deprecated. Should be moved when all api-customers use Employees/ImportChanges insted of Employees/Changes
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        private ActionResult ConvertToResult(List<EmployeeChangeIODTO> employeeChanges, EmployeeUserImportBatch batch)
        {
            if (batch == null)
                batch = new EmployeeUserImportBatch(new ActionResult(false));

            int nrOfReceivedEmployees = employeeChanges.GetNrOfReceivedEmployees();
            int nrOfCommittedEmployees = employeeChanges.GetNrOfCommittedEmployees();
            int nrOfPartlyCommittedEmployees = employeeChanges.GetNrOfPartlyCommittedEmployees();
            int nrOfUnCommittedEmployees = employeeChanges.GetNrOfUnCommittedEmployees();

            //ErrorMessage
            if (batch.Result.Success)
            {
                if (nrOfCommittedEmployees > 0 && nrOfPartlyCommittedEmployees == 0 && nrOfUnCommittedEmployees == 0)
                    batch.Result.ErrorMessage = GetText(5574, "Anställda inlästa");
                else if (nrOfCommittedEmployees > 0 || nrOfPartlyCommittedEmployees > 0)
                    batch.Result.ErrorMessage = GetText(11962, "Anställda delvis inlästa");
                else
                    batch.Result.ErrorMessage = GetText(11963, "Inga anställda inlästa");
            }
            else
                batch.Result.ErrorMessage = GetText(11963, "Inga anställda inlästa");
            if (!batch.ValidationErrors.IsNullOrEmpty() && batch.IsLegacyMode)
                batch.Result.ErrorMessage += $". {GetText(11891, "Valideringsfel uppstod")}";

            //Strings
            if (!batch.ValidationErrors.IsNullOrEmpty())
                batch.Result.Strings = batch.ValidationErrors;
            if (batch.Result.Strings == null)
                batch.Result.Strings = new List<string>();
            batch.Result.Strings.Add($"{GetText(11894, "Antal skickade anställda")} {nrOfReceivedEmployees}");
            batch.Result.Strings.Add($"{GetText(11895, "Antal inlästa anställda")} {nrOfCommittedEmployees}");
            batch.Result.Strings.Add($"{GetText(11961, "Antal ej inlästa anställda")} {nrOfUnCommittedEmployees}");

            //Misc
            batch.Result.StackTrace = batch.StackTrace?.ToString().EmptyToNull();
            batch.Result.IntegerValue = nrOfCommittedEmployees;
            batch.Result.IntDict = null;

            return batch.Result;
        }

        private ActionResult SaveApiMessageChanges(ApiMessage inputApiMessage, EmployeeUserImportBatch batch, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (batch == null || batch.Imports.IsNullOrEmpty())
                return new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                ApiMessage apiMessage = ApiDataManager.GetApiMessage(entities, inputApiMessage.ApiMessageId);
                if (apiMessage == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ApiMessage");

                List<ApiMessageChange> apiMessageChanges = new List<ApiMessageChange>();
                List<EmployeeUserImport> imports = batch.Imports.Where(i => i.EmployeeUser.HasChanges()).ToList();

                foreach (EmployeeUserImport import in imports)
                {
                    foreach (EmployeeChangeDTO change in import.GetEmployeeChangesToLog())
                    {
                        if (!apiMessageChanges.ContainsChange(import.EmployeeUser.EmployeeId, change.FieldType, change.ToValue, change.FromDate, change.ToDate))
                            apiMessageChanges.Add(ConvertToApiMessageChange(change, import.EmployeeUser));
                    }

                    foreach (EmploymentChangeDTO change in import.GetEmploymentChangesToLog())
                    {
                        if (!apiMessageChanges.ContainsChange(import.EmployeeUser.EmployeeId, (int)change.FieldType, change.ToValue, change.FromDate, change.ToDate))
                            apiMessageChanges.Add(ConvertToApiMessageChange(change, import.EmployeeUser));
                    }

                    if (import.Result.Success)
                    {
                        if (batch.HasValidationErrors)
                            apiMessage.ValidationMessage = GetText(12016, "Fel uppstod. Expandera för mer information");
                        else if (batch.IsAlreadyUpdated)
                            apiMessage.ValidationMessage = GetText(12054, "Inga förändringar. Alla uppgifter är redan uppdaterade");
                    }
                    else if (batch.IsLegacyMode)
                    {
                        batch.ValidationErrors.Add(import.Result.ErrorMessage);
                    }
                }

                List<EmployeeUserImport> importsWithErrorMessages = imports.Where(import => !import.Result.ErrorMessage.IsNullOrEmpty()).ToList();
                if (importsWithErrorMessages.Any())
                {
                    if (imports.Count == 1)
                        apiMessage.ValidationMessage = importsWithErrorMessages.First().Result.ErrorMessage;
                    else if (imports.Count > 1)
                        apiMessage.ValidationMessage = GetText(12016, "Fel uppstod. Expandera för mer information");
                }

                if (apiMessageChanges.Any() || batch.HasValidationErrors || !apiMessage.ValidationMessage.IsNullOrEmpty())
                {
                    apiMessage.ApiMessageChange.AddRange(apiMessageChanges);
                    if (batch.IsLegacyMode && batch.HasValidationErrors)
                        apiMessage.ValidationMessage = batch.ValidationErrors.ToCommaSeparated();

                    SaveChanges(entities);
                }
            }

            return ConvertToResult(employeeChanges, batch);
        }

        private void SaveChangesToEmployeeUser(ref EmployeeUserImport import, EmployeeChangeIOItem container, EmployeeChangeIODTO employeeChange)
        {
            if (import?.EmployeeUser == null || container == null || employeeChange == null)
                return;

            import.EmployeeUser.SaveEmployee = true;
            import.EmployeeUser.SaveUser = container.SaveUser;

            import.Result = ActorManager.SaveEmployeeUser(
                TermGroup_TrackChangesActionMethod.Employee_Import,
                import.EmployeeUser,
                contactAddresses: container.GetContactAddressItems(import.EmployeeUser.EmployeeNr),
                employeePositions: container.GetEmployeePositions(import.EmployeeUser.EmployeeNr),
                employeeTax: import.EmployeeUser.EmployeeTax,
                userRoles: import.EmployeeUser.UserRoles,
                saveRoles: import.EmployeeUser.DoSaveRoles(),
                saveAttestRoles: import.EmployeeUser.DoSaveAttestRoles(),
                doAcceptAttestedTemporaryEmployments: true,
                logChanges: true,
                skipCategoryCheck: true,
                onlyValidateAttestRolesInCompany: true,
                extraFields: container.GetExtraFieldRecords(import.EmployeeUser.EmployeeNr));

            if (import.Result.Success)
            {
                employeeChange.HasChanges = true;
            }
            else
            {
                import.EmployeeUser.AddErrorChange(EmployeeUserImport.SAVE_FAILED, $"{GetText(12055, "Sparning misslyckades")}. {import.Result.ErrorMessage}");
                if (import.IsNewEmployee)
                    import.Result.ErrorMessage = $"{string.Format(GetText(11892, "Import av anställd {0} misslyckades"), import.EmployeeUser.EmployeeNr)}. {import.Result.ErrorMessage}{Environment.NewLine}";
                else
                    import.Result.ErrorMessage = $"{string.Format(GetText(11893, "Uppdatering av anställd {0} misslyckades"), import.EmployeeUser.EmployeeNr)}. {import.Result.ErrorMessage}{Environment.NewLine}";

                employeeChange.SaveFailed(import.Result.ErrorMessage);
            }

            if (import.Result.IntDict != null && import.Result.IntDict.ContainsKey((int)SaveEmployeeUserResult.EmployeeId))
                import.EmployeeUser.EmployeeId = import.Result.IntDict[(int)SaveEmployeeUserResult.EmployeeId];
        }

        private void AddNothingChangedError(EmployeeUserImport import, EmployeeChangeIOItem container)
        {
            if (import?.EmployeeUser == null || container == null)
                return;

            import.EmployeeUser.AddErrorChange(EmployeeUserImport.NOTHING_UPDATED, GetText(12054, "Inga förändringar. Alla uppgifter är redan uppdaterade", forceDefaultTerm: container.IsTest));
        }

        #endregion

        #region Lookups

        private CacheConfig GetApiCacheConfig(ApiConfig config)
        {
            return CacheConfig.Company(ActorCompanyId, seconds: config?.CacheSeconds);
        }

        private int GetDefaultRoleId()
        {
            return SettingManager.GetCompanyIntSetting(CompanySettingType.DefaultRole);
        }

        private int GetDefaultEmployeeGroupId()
        {
            return SettingManager.GetCompanyIntSetting(CompanySettingType.TimeDefaultEmployeeGroup);
        }

        private int GetDefaultPayrollGroupId()
        {
            return SettingManager.GetCompanyIntSetting(CompanySettingType.TimeDefaultPayrollGroup);
        }

        private int GetDefaultVacationGroupId()
        {
            return SettingManager.GetCompanyIntSetting(CompanySettingType.TimeDefaultVacationGroup);
        }

        public TermGroup_Country GetCompanyCountryId()
        {
            return (TermGroup_Country)CompanyManager.GetCompanySysCountryId(base.ActorCompanyId);
        }

        private List<AccountDTO> GetAccountsInternalsFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!config.Settings.GetBool(TermGroup_ApiSettingType.KeepEmploymentAccount) && !ContainsAnyRow(employeeChanges,
                EmployeeChangeType.HierarchicalAccount,
                EmployeeChangeType.AttestRole,
                EmployeeChangeType.EmploymentStopDateChange,
                EmployeeChangeType.AccountNrSieDim))
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountInternalsFromCache(entities, GetApiCacheConfig(config));
        }

        private List<AccountDimDTO> GetAccountDimsFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!config.Settings.GetBool(TermGroup_ApiSettingType.KeepEmploymentAccount) && !ContainsAnyRow(employeeChanges,
                EmployeeChangeType.AccountNrSieDim))
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountDimsFromCache(entities, GetApiCacheConfig(config));
        }

        private List<EmploymentTypeDTO> GetEmploymentTypesFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges,
                EmployeeChangeType.EmploymentType) &&
                !ContainsAnyRow(employeeChanges,
                EmployeeChangeType.NewEmployments))
                return null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return base.GetEmploymentTypesFromCache(entitiesReadOnly, GetApiCacheConfig(config), (TermGroup_Languages)GetLangId());
        }

        private List<EmployeeGroupDTO> GetEmployeeGroupsFromCache(ApiConfig config)
        {
            if (config == null)
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeGroupsFromCache(entities, GetApiCacheConfig(config), loadExternalCode: true).ToDTOs().ToList();
        }

        private List<PayrollGroupDTO> GetPayrollGroupsFromCache(ApiConfig config)
        {
            if (config == null)
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollGroupsFromCache(entities, GetApiCacheConfig(config), loadExternalCode: true).ToDTOs().ToList();
        }

        private List<VacationGroupDTO> GetVacationGroupsFromCache(ApiConfig config)
        {
            if (config == null)
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetVacationGroupsFromCache(entities, GetApiCacheConfig(config), loadExternalCode: true).ToDTOs().ToList();
        }

        private List<AnnualLeaveGroupDTO> GetAnnualLeaveGroupsFromCache(ApiConfig config)
        {
            if (config == null)
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAnnualLeaveGroupsFromCache(entities, GetApiCacheConfig(config)).ToDTOs().ToList();
        }

        private List<PayrollPriceTypeDTO> GetPayrollPriceTypesFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!config.Settings.GetBool(TermGroup_ApiSettingType.KeepEmploymentPriceType) && !ContainsAnyRow(employeeChanges, EmployeeChangeType.EmploymentPriceType))
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollPriceTypeDTOsFromCache(entities, GetApiCacheConfig(config)).ToList();
        }

        private List<PayrollLevelDTO> GetPayrollLevelsFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges, EmployeeChangeType.EmploymentPriceType))
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollLevelsFromCache(entities, GetApiCacheConfig(config)).ToDTOs().ToList();
        }

        private List<PayrollPriceFormulaDTO> GetPayrollPriceFormulasFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges, EmployeeChangeType.BygglosenSalaryFormula))
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollPriceFormulaDTOsFromCache(entities, GetApiCacheConfig(config)).ToList();
        }

        private List<PositionDTO> GetPositionsFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges,
                EmployeeChangeType.EmployeePosition))
                return null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return base.GetPositionsFromCache(entitiesReadOnly, GetApiCacheConfig(config));
        }

        private List<AttestRoleDTO> GetTimeAttestRolesFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges,
                EmployeeChangeType.AttestRole,
                EmployeeChangeType.EmploymentStopDateChange))
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeAttestRolesFromCache(entities, GetApiCacheConfig(config), includeInactive: true, loadExternalCode: true).ToDTOs().ToList();
        }

        private List<RoleDTO> GetUserRolesFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges,
                EmployeeChangeType.UserRole,
                EmployeeChangeType.DefaultUserRole))
                return null;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetRolesFromCache(entities, GetApiCacheConfig(config), loadExternalCode: true).ToDTOs().ToList();
        }

        private List<GenericType> GetContactEComsTypesFromCache(ApiConfig lookup, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (lookup == null)
                return null;
            if (!ContainsAnyRow(employeeChanges,
                EmployeeChangeType.Email,
                EmployeeChangeType.PhoneHome,
                EmployeeChangeType.PhoneMobile,
                EmployeeChangeType.PhoneJob,
                EmployeeChangeType.ClosestRelativeNr,
                EmployeeChangeType.ClosestRelativeName,
                EmployeeChangeType.ClosestRelativeRelation,
                EmployeeChangeType.ClosestRelativeHidden,
                EmployeeChangeType.ClosestRelativeNr2,
                EmployeeChangeType.ClosestRelativeName2,
                EmployeeChangeType.ClosestRelativeRelation2,
                EmployeeChangeType.ClosestRelativeHidden2))
                return null;
            return GetTermGroupContent(TermGroup.SysContactEComType);
        }

        private List<GenericType> GetContactAddressesTypes(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges,
                EmployeeChangeType.Address,
                EmployeeChangeType.AddressCO,
                EmployeeChangeType.AddressPostCode,
                EmployeeChangeType.AddressPostalAddress,
                EmployeeChangeType.AddressCountry,
                EmployeeChangeType.AddressHidden))
                return null;
            return GetTermGroupContent(TermGroup.SysContactAddressType);
        }

        private List<ExtraFieldRecordDTO> GetExtraFieldRecords(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges, EmployeeChangeType.ExtraFieldEmployee))
                return null;

            var employeeNumbersWithExtraFields = employeeChanges.Where(w => w.EmployeeChangeRowIOs.Any(a => a.EmployeeChangeType == EmployeeChangeType.ExtraFieldEmployee)).Select(s => s.EmployeeNr).Distinct().ToList();
            if (!employeeNumbersWithExtraFields.Any())
                return null;

            var employees = EmployeeManager.GetAllEmployeesByNumbers(ActorCompanyId, employeeNumbersWithExtraFields);
            if (!employees.Any())
                return null;

            return ExtraFieldManager.GetExtraFieldWithRecords(employees.Select(s => s.EmployeeId).ToList(), (int)SoeEntityType.Employee, ActorCompanyId, 0);
        }

        private List<ExtraFieldDTO> GetExtraFields(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges, EmployeeChangeType.ExtraFieldEmployee))
                return null;

            return ExtraFieldManager.GetExtraFields((int)SoeEntityType.Employee, ActorCompanyId, false, loadExternalCodes: true, loadValues: true).ToDTOs().ToList();
        }

        private List<EndReasonDTO> GetEndReasonsFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges,
                EmployeeChangeType.EmploymentEndReason))
                return null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEndReasonsFromCache(entities, GetApiCacheConfig(config));
        }

        private List<TimeDeviationCauseDTO> GetTimeDeviationCausesFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges,
                EmployeeChangeType.NewEmployments))
                return null;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeDeviationCausesFromCache(entities, GetApiCacheConfig(config)).ToDTOs().ToList();
        }

        private List<TimeWorkAccountDTO> GetTimeWorkAccountsFromCache(ApiConfig config, List<EmployeeChangeIODTO> employeeChanges)
        {
            if (config == null)
                return null;
            if (!ContainsAnyRow(employeeChanges,
                EmployeeChangeType.TimeWorkAccount))
                return null;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimeWorkAccountsFromCache(entities, GetApiCacheConfig(config)).ToDTOs().ToList();
        }

        private Dictionary<EmployeeChangeType, bool> GetApiPermissions(List<EmployeeChangeIODTO> employeeChanges)
        {
            Dictionary<EmployeeChangeType, bool> permissions = new Dictionary<EmployeeChangeType, bool>();

            List<EmployeeChangeRowIODTO> rows = employeeChanges?.Where(change => change?.EmployeeChangeRowIOs != null).SelectMany(change => change.EmployeeChangeRowIOs).ToList() ?? new List<EmployeeChangeRowIODTO>();

            foreach (EmployeeChangeType type in Enum.GetValues(typeof(EmployeeChangeType)))
            {
                bool hasPermission = true;

                if (rows.Any(a => a.EmployeeChangeType == type))
                {
                    switch (type)
                    {
                        case EmployeeChangeType.SocialSec:
                            hasPermission = HasModifyPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);
                            break;
                        case EmployeeChangeType.EmploymentPriceType:
                            hasPermission = HasModifyPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary);
                            break;
                        case EmployeeChangeType.AccountNrSieDim:
                            hasPermission = HasModifyPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts);
                            break;
                        case EmployeeChangeType.DisbursementAccountNr:
                        case EmployeeChangeType.DisbursementMethod:
                        case EmployeeChangeType.DontValidateDisbursementAccountNr:
                        case EmployeeChangeType.DisbursementCountryCode:
                        case EmployeeChangeType.DisbursementBIC:
                        case EmployeeChangeType.DisbursementIBAN:
                            hasPermission = HasModifyPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount);
                            break;
                    }
                }

                permissions.Add(type, hasPermission);
            }

            return permissions;
        }

        private List<ApiSettingDTO> GetApiSettings()
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return ApiDataManager.GetApiSettings(entitiesReadOnly).ToDTOs().ToList();
        }

        private bool HasModifyPermission(Feature feature)
        {
            return FeatureManager.HasRolePermission(feature, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId);
        }

        #endregion

        #endregion
    }
}
