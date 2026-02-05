using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO.SoftOneLogger;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Logger
{
    public class LoggerManager : ManagerBase
    {
        #region Variables

        private readonly Dictionary<int, Employee> cachedEmployees = new Dictionary<int, Employee>();
        private readonly Dictionary<int, ContactPerson> cachedContactPersons = new Dictionary<int, ContactPerson>();
        private readonly Dictionary<int, Customer> cachedCustomers = new Dictionary<int, Customer>();
        private readonly Dictionary<int, Supplier> cachedSuppliers = new Dictionary<int, Supplier>();

        #endregion

        #region Ctor

        public LoggerManager(ParameterObject parameterObject) : base(parameterObject)
        {

        }

        #endregion

        #region PersonalDataLogBatch

        public void CreatePersonalDataLogFireAndForget(object model, Guid guid, TermGroup_PersonalDataActionType personalDataActionType, PersonalDataBatchType batchType = PersonalDataBatchType.Unspecified, int? batchRecordId = null, string url = "")
        {
            if (model == null)
                return;

            if (personalDataActionType == TermGroup_PersonalDataActionType.Read)
                Task.Run(() => CreatePersonalDataLog(model, guid, batchType, batchRecordId, url));
        }

        public async Task CreatePersonalDataLog(object model, Guid guid, TermGroup_PersonalDataActionType personalDataActionType, PersonalDataBatchType batchType = PersonalDataBatchType.Unspecified, int? batchRecordId = null, string url = "")
        {
            if (model == null)
                return;

            if (model is EntityObject)
            {
                Log("CreatePersonalDataLog Model is EntityObject: " + model.GetType().ToString() + " " + (string.IsNullOrEmpty(url) ? "": url));
                return;
            }

            if (personalDataActionType == TermGroup_PersonalDataActionType.Read)
                await CreatePersonalDataLog(model, guid, batchType, batchRecordId, url);
        }

        private async Task CreatePersonalDataLog(object model, Guid guid, PersonalDataBatchType batchType = PersonalDataBatchType.Unspecified, int? batchRecordId = null, string url = "")
        {
            bool extensiveLogging = url.ToLower().Contains("extensivelogging");
            string objectName = string.Empty;
            List<PersonalDataLogDTO> personalDataLogs = new List<PersonalDataLogDTO>();
            try
            {
                if (model == null)
                    return;

                List<PersonalDataLogDTO> rawPersonalDataLogs = GeneratePersonalDataLogs(model, extensiveLogging: extensiveLogging);
                if (rawPersonalDataLogs == null || rawPersonalDataLogs.Count == 0)
                    return;

                if (extensiveLogging)
                    LogCollector.LogError("CreatePersonalDataLog rawPersonalDataLogs done");

                foreach (var group in rawPersonalDataLogs.GroupBy(g => g.ActionType.ToString() + "#" + g.RecordId + "#" + g.PersonalDataType + "#" + g.InformationType))
                {
                    personalDataLogs.Add(group.First());
                }

                if (extensiveLogging)
                    LogCollector.LogError("CreatePersonalDataLog grouping done rawPersonalDataLogs " + rawPersonalDataLogs.Count.ToString() + " personalDataLogs" + personalDataLogs.Count.ToString());

                if (model is IEnumerable)
                    objectName = "List<" + personalDataLogs.FirstOrDefault().ObjectName + ">";
                else if (model.GetType().IsClass)
                    objectName = model.GetType() != null ? model.GetType().Name : string.Empty;
            }
            catch (Exception ex)
            {
                Log("CreatePersonalDataLog prereq" + (string.IsNullOrEmpty(url) ? "" : url) + ex.ToString());
            }

            try
            {
                PersonalDataLogBatchDTO personalDataBatch = GeneratePersonalDataBatch(guid, batchType, batchRecordId, objectName, url, personalDataLogs);
                await LoggerConnector.SavePersonalDataLog(personalDataBatch, extensiveLogging);
            }
            catch (Exception ex)
            {
                Log("CreatePersonalDataLog batch " + (string.IsNullOrEmpty(url) ? "" : url) + ex.ToString());
            }

        }

        public void Log(string message)
        {
            LogCollector.LogError(message);
        }

        public PersonalDataLogBatchDTO GeneratePersonalDataBatch(Guid guid, PersonalDataBatchType batchType = PersonalDataBatchType.Unspecified, int? batchRecordId = null, string objectName = "", string url = "", List<PersonalDataLogDTO> personalDataLogs = null)
        {
            return new PersonalDataLogBatchDTO()
            {
                //Keys
                ActorCompanyId = parameterObject.ActorCompanyId,
                UserId = parameterObject.UserId.ToNullable(),
                RoleId = parameterObject.RoleId.ToNullable(),
                SupportUserId = (parameterObject.IsSupportLoggedInByCompany || parameterObject.IsSupportLoggedInByUser) ? parameterObject.SupportUserId : (int?)null,
                Batch = guid,
                SysCompDBId = SysServiceManager.GetSysCompDBId() ?? 0,
                LoginGuid = parameterObject.IdLoginGuid,

                //Properties
                BatchType = batchType,
                BatchRecordId = batchRecordId,
                TimeStamp = DateTime.UtcNow,
                IPAddress = parameterObject?.ExtendedUserParams?.IpAddress,
                MachineName = Environment.MachineName,
                ObjectName = objectName,
                RequestUrl = string.IsNullOrEmpty(url) ? parameterObject?.ExtendedUserParams?.Request : url,

                //Relations
                PersonalDataLog = personalDataLogs != null ? personalDataLogs : new List<PersonalDataLogDTO>(),
            };
        }

        public List<PersonalDataLogDTO> GeneratePersonalDataLogs(object model, TermGroup_PersonalDataType dataType = TermGroup_PersonalDataType.Employee, List<TermGroup_PersonalDataInformationType> filterInformationTypes = null, bool extensiveLogging = false)
        {
            List<PersonalDataLogDTO> personalDataLogs = new List<PersonalDataLogDTO>();

            if (model == null)
                return personalDataLogs;

            if (model is string)
            {
                return personalDataLogs;
            }
            if (model is IEnumerable enumerable)
            {
                #region Model is list

                foreach (var item in enumerable)
                {
                    personalDataLogs.AddRange(GeneratePersonalDataLogs(item, extensiveLogging: extensiveLogging));
                }

                #endregion
            }
            else
            {
                #region Model is class

                if (!IsValidForLogging(model))
                    return personalDataLogs;

                PropertyInfo[] properties = model.GetProperties();
                foreach (PropertyInfo propInfo in properties)
                {
                    var value = propInfo.GetValue(model, null);

                    if (propInfo.IsList())
                    {
                        #region Property is List

                        if (value != null && !value.IsDictionary())
                        {
                            bool? isclass = null;
                            foreach (object item in value as IEnumerable)
                            {
                                // Optimization to not call GetType().IsClass for every item in a long list
                                if (!isclass.HasValue)
                                    isclass = item.GetType().IsClass;
                                if (isclass.Value)
                                    personalDataLogs.AddRange(GeneratePersonalDataLogs(item, extensiveLogging: extensiveLogging));
                            }
                        }

                        #endregion
                    }
                    else if (propInfo.IsClass())
                    {
                        #region Property is Class

                        if (value != null)
                        {
                            personalDataLogs.AddRange(GeneratePersonalDataLogs(value, extensiveLogging: extensiveLogging));
                        }

                        #endregion
                    }
                    else
                    {
                        #region Property is Value

                        int recordId = 0;
                        if (IsValidPropertyToLog(model, propInfo, properties, filterInformationTypes, out recordId))
                        {
                            TermGroup_PersonalDataInformationType informationType = propInfo.GetPersonalDataInformationType();

                            if (!personalDataLogs.Any(w => w.InformationType == informationType))
                            {
                                PersonalDataLogDTO log = new PersonalDataLogDTO()
                                {
                                    RecordId = recordId,
                                    PersonalDataType = dataType,
                                    InformationType = informationType,
                                    ActionType = TermGroup_PersonalDataActionType.Read,
                                    ObjectName = model.GetType().Name
                                };
                                personalDataLogs.Add(log);
                            }
                        }

                        #endregion
                    }
                }

                #endregion
            }

            return personalDataLogs;
        }

        #endregion

        #region PersonalDataLogMessageDTO

        public List<PersonalDataLogMessageDTO> GetPersonalDataLogs(int recordId, TermGroup_PersonalDataType dataType, TermGroup_PersonalDataInformationType informationType, TermGroup_PersonalDataActionType actionType, DateTime? dateFrom = null, DateTime? dateTo = null, int? take = null)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            List<PersonalDataLogBatchDTO> batches = LoggerConnector.GetPersonalDataLogs(base.ActorCompanyId, recordId, SysServiceManager.GetSysCompDBId(), dataType, informationType, actionType, dateFrom, dateTo, take);
            return GenerateLogMessages(batches, byUser: false);
        }

        public List<PersonalDataLogMessageDTO> GetPersonalDataLogsCausedByEmployee(int employeeId, int? recordId, TermGroup_PersonalDataType dataType, TermGroup_PersonalDataInformationType informationType, TermGroup_PersonalDataActionType actionType, DateTime? dateFrom = null, DateTime? dateTo = null, int? take = null)
        {
            User user = UserManager.GetUserByEmployeeId(employeeId, base.ActorCompanyId);
            if (user == null)
                return new List<PersonalDataLogMessageDTO>();

            return GetPersonalDataLogsCausedByUser(user.UserId, recordId, dataType, informationType, actionType, dateFrom, dateTo, take);
        }

        public List<PersonalDataLogMessageDTO> GetPersonalDataLogsCausedByUser(int userId, int? recordId, TermGroup_PersonalDataType dataType, TermGroup_PersonalDataInformationType informationType, TermGroup_PersonalDataActionType actionType, DateTime? dateFrom = null, DateTime? dateTo = null, int? take = null)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            List<PersonalDataLogBatchDTO> batches = LoggerConnector.GetPersonalDataLogsCausedByUser(base.ActorCompanyId, userId, recordId, SysServiceManager.GetSysCompDBId(), dataType, informationType, actionType, dateFrom, dateTo, take);
            return GenerateLogMessages(batches, byUser: true);
        }

        #endregion

        #region Help-methods 

        private List<PersonalDataLogMessageDTO> GenerateLogMessages(List<PersonalDataLogBatchDTO> batches, bool byUser = false)
        {
            List<PersonalDataLogMessageDTO> messages = new List<PersonalDataLogMessageDTO>();

            if (batches == null || batches.Count == 0)
                return messages;

            int batchNbr = 0;
            string prevBatch = String.Empty;
            List<User> users = new List<User>();
            List<Role> roles = new List<Role>();

            foreach (PersonalDataLogBatchDTO batch in batches.Where(i => !i.PersonalDataLog.IsNullOrEmpty()))
            {
                #region PersonalDataLogBatchDTO

                if (String.IsNullOrEmpty(prevBatch) || batch.Batch.ToString() != prevBatch)
                    batchNbr++;

                prevBatch = batch.Batch.ToString();

                #region User

                User user = null;
                if (batch.UserId.HasValue)
                {
                    user = users.FirstOrDefault(i => i.UserId == batch.UserId);
                    if (user == null)
                    {
                        user = UserManager.GetUser(batch.UserId.Value);
                        if (user != null)
                            users.Add(user);
                    }
                }

                string loginName = user?.LoginName ?? "[]";
                string userText = batch.SupportUserId.HasValue ? $"SoftOne ({batch.SupportUserId.Value})" : loginName;

                #endregion

                #region Role

                Role role = null;
                if (batch.RoleId.HasValue)
                {
                    role = roles.FirstOrDefault(i => i.RoleId == batch.RoleId.Value);
                    if (role == null)
                    {
                        role = RoleManager.GetRole(batch.RoleId.Value, batch.ActorCompanyId);
                        if (role != null)
                            roles.Add(role);
                    }
                }

                string roleText = role != null ? RoleManager.GetRoleName(role.RoleId) : "[]";

                #endregion

                #endregion

                #region PersonalDataLog

                List<PersonalDataLogDTO> logs = batch.PersonalDataLog.ToList();

                if (byUser)
                {
                    #region By User

                    foreach (var logsByActionType in batch.PersonalDataLog.GroupBy(i => i.ActionType))
                    {
                        string actionTypeText = logsByActionType.Key == TermGroup_PersonalDataActionType.Read ? GetText(11742, "Läst") : GetText(11743, "Förändrat");
                        string actionTypeMessage = logsByActionType.Key == TermGroup_PersonalDataActionType.Read ? GetText(11737, "såg") : GetText(11738, "ändrade");

                        foreach (var logsByPersonalDataType in logsByActionType.GroupBy(i => i.PersonalDataType))
                        {
                            string personalDataTypeText = GetText((int)logsByPersonalDataType.Key, (int)TermGroup.PersonalDataType).ToLower();

                            foreach (var logsByRecord in batch.PersonalDataLog.GroupBy(i => i.RecordId))
                            {
                                string recordText = GetRecordText(logsByRecord.Key, logsByPersonalDataType.Key);

                                messages.Add(new PersonalDataLogMessageDTO()
                                {
                                    Batch = batch.Batch,
                                    BatchNbr = batchNbr,
                                    TimeStamp = CalendarUtility.ClearSeconds(batch.TimeStamp.ToLocalTime()),
                                    UserName = userText,
                                    Url = batch.RequestUrl,
                                    InformationTypeText = GetInformationTypeText(logsByPersonalDataType.ToList()),
                                    ActionTypeText = actionTypeText,
                                    Message = $"{GetText(1062, "Användare")} {userText} {GetText(11741, "med roll")} {roleText} {actionTypeMessage} {GetText(11755, "personliga uppgifter för")} {personalDataTypeText} {recordText}",
                                });
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    #region For user

                    foreach (var logsByActionType in batch.PersonalDataLog.GroupBy(i => i.ActionType))
                    {
                        string actionTypeText = logsByActionType.Key == TermGroup_PersonalDataActionType.Read ? GetText(11742, "Läst") : GetText(11743, "Förändrat");
                        string actionTypeMessage = logsByActionType.Key == TermGroup_PersonalDataActionType.Read ? GetText(11737, "såg") : GetText(11738, "ändrade");

                        messages.Add(new PersonalDataLogMessageDTO()
                        {
                            Batch = batch.Batch,
                            BatchNbr = batchNbr,
                            TimeStamp = CalendarUtility.ClearSeconds(batch.TimeStamp.ToLocalTime()),
                            UserName = userText,
                            Url = batch.RequestUrl,
                            InformationTypeText = GetInformationTypeText(logs),
                            ActionTypeText = actionTypeText,
                            Message = $"{GetText(1062, "Användare")} {userText} {GetText(11741, "med roll")} {roleText} {actionTypeMessage} {GetText(11740, "dina personliga uppgifter")}",
                        });
                    }

                    #endregion
                }

                #endregion
            }

            return messages;
        }

        private string GetInformationTypeText(List<PersonalDataLogDTO> logs)
        {
            StringBuilder sb = new StringBuilder();

            List<TermGroup_PersonalDataInformationType> informationTypes = logs.Select(i => i.InformationType).Distinct().ToList();
            foreach (TermGroup_PersonalDataInformationType informationType in informationTypes)
            {
                if (sb.Length > 0)
                    sb.Append(",");
                sb.Append(GetInformationTypeText(informationType));
            }

            return sb.ToString();
        }

        private string GetInformationTypeText(TermGroup_PersonalDataInformationType personalDataInformationType)
        {
            string informationTypeText = "";

            switch (personalDataInformationType)
            {
                case TermGroup_PersonalDataInformationType.Unspecified:
                    informationTypeText = GetText(11736, "Allmän");
                    break;
                case TermGroup_PersonalDataInformationType.SocialSec:
                    informationTypeText = GetText(11734, "Personnummer");
                    break;
                case TermGroup_PersonalDataInformationType.EmployeeMeeting:
                    informationTypeText = GetText(11735, "Samtal");
                    break;
                case TermGroup_PersonalDataInformationType.IllnessInformation:
                    informationTypeText = GetText(11751, "Sjukdomsinställningar");
                    break;
                case TermGroup_PersonalDataInformationType.ParentalLeaveAndChild:
                    informationTypeText = GetText(11752, "Föräldraledighets- och barninformation");
                    break;
                case TermGroup_PersonalDataInformationType.SalaryDistress:
                    informationTypeText = GetText(11753, "Skattejämkning");
                    break;
                case TermGroup_PersonalDataInformationType.Unionfee:
                    informationTypeText = GetText(11754, "Fackförbundsavgift");
                    break;
                default:
                    break;
            }

            return informationTypeText;
        }

        private string GetRecordText(int recordId, TermGroup_PersonalDataType personalDataType)
        {
            string recordText = "";

            switch (personalDataType)
            {
                case TermGroup_PersonalDataType.Employee:
                    if (!this.cachedEmployees.ContainsKey(recordId))
                        this.cachedEmployees.Add(recordId, EmployeeManager.GetEmployee(recordId, base.ActorCompanyId, loadContactPerson: true, onlyActive: false));
                    Employee employee = this.cachedEmployees[recordId];
                    if (employee != null && employee.ContactPerson != null)
                        recordText = employee.ContactPerson.Name;
                    break;
                case TermGroup_PersonalDataType.ContactPerson:
                    if (!this.cachedContactPersons.ContainsKey(recordId))
                        this.cachedContactPersons.Add(recordId, ContactManager.GetContactPerson(recordId, onlyActive: false));
                    ContactPerson contactPerson = this.cachedContactPersons[recordId];
                    if (contactPerson != null)
                        recordText = contactPerson.Name;
                    break;
                case TermGroup_PersonalDataType.Customer:
                    if (!this.cachedCustomers.ContainsKey(recordId))
                        this.cachedCustomers.Add(recordId, CustomerManager.GetCustomer(recordId));
                    Customer customer = this.cachedCustomers[recordId];
                    if (customer != null)
                        recordText = customer.Name;
                    break;
                case TermGroup_PersonalDataType.Supplier:
                    if (!this.cachedSuppliers.ContainsKey(recordId))
                        this.cachedSuppliers.Add(recordId, SupplierManager.GetSupplier(recordId));
                    Supplier supplier = this.cachedSuppliers[recordId];
                    if (supplier != null)
                        recordText = supplier.Name;
                    break;
                case TermGroup_PersonalDataType.HouseholdApplicant:
                    //TODO
                    break;
            }

            return recordText;
        }

        private bool IsValidForLogging(object model)
        {
            if (model == null)
                return false;

            Type type = model.GetType();
            if (!type.HasLogAttribute())
                return false;

            return IsValidPropertyAttribute(type);
        }

        private bool IsValidPropertyAttribute(Type type)
        {
            if (type.HasEmployeeIdAttribute())
                return true;
            if (type.HasLogActorIdAttribute())
                return true;
            return true;
        }

        private bool IsValidPropertyToLog(object model, PropertyInfo propInfo, PropertyInfo[] allProperties, List<TermGroup_PersonalDataInformationType> filterInformationTypes, out int recordId)
        {
            if (IsValidEmployeePropertyToLog(model, propInfo, allProperties, filterInformationTypes, out recordId))
                return true;
            if (IsValidActorPropertyToLog(model, propInfo, allProperties, filterInformationTypes, out recordId))
                return true;
            if (IsValidHouseHoldApplicantIdProperty(model, propInfo, allProperties, out recordId))
                return true;
            return false;
        }


        private bool IsValidEmployeePropertyToLog(object model, PropertyInfo propInfo, PropertyInfo[] allProperties, List<TermGroup_PersonalDataInformationType> filterInformationTypes, out int recordId)
        {
            recordId = 0;

            if (model == null)
                return false;

            if (!propInfo.HasEmployeeLoggingAttribute(filterInformationTypes))
                return false;

            object propValue = GetValidValue(model, propInfo);
            if (propValue == null)
                return false;

            foreach (PropertyInfo currentPropInfo in allProperties)
            {
                if (!currentPropInfo.HasEmployeeIdAttribute())
                    continue;

                object currentPropValue = currentPropInfo.GetValue(model, null);
                if (currentPropValue != null && Int32.TryParse(currentPropValue.ToString(), out recordId) && recordId != 0)
                    break;
            }

            return recordId > 0;
        }

        private bool IsValidActorPropertyToLog(object model, PropertyInfo propInfo, PropertyInfo[] allProperties, List<TermGroup_PersonalDataInformationType> filterInformationTypes, out int recordId)
        {
            recordId = 0;

            if (model == null)
                return false;

            if (!propInfo.HasActorLoggingAttribute(filterInformationTypes))
                return false;

            object propValue = GetValidValue(model, propInfo);
            if (propValue == null)
                return false;

            foreach (PropertyInfo currentPropInfo in allProperties)
            {
                if (!currentPropInfo.HasActorIdAttribute())
                    continue;

                object currentPropValue = currentPropInfo.GetValue(model, null);
                if (currentPropValue != null && Int32.TryParse(currentPropValue.ToString(), out recordId) && recordId != 0)
                    break;
            }

            return recordId > 0;
        }

        private bool IsValidHouseHoldApplicantIdProperty(object model, PropertyInfo propInfo, PropertyInfo[] allProperties, out int recordId)
        {
            recordId = 0;

            if (model == null)
                return false;

            if (!propInfo.HasLogHouseholdDeductionApplicantIdAttribute())
                return false;

            object propValue = propInfo.GetValue(model, null);
            if (propValue == null)
                return false;

            foreach (PropertyInfo currentPropInfo in allProperties)
            {
                if (!currentPropInfo.HasLogHouseholdDeductionApplicantIdAttribute())
                    continue;

                object currentPropValue = currentPropInfo.GetValue(model, null);
                if (currentPropValue != null && Int32.TryParse(currentPropValue.ToString(), out recordId) && recordId != 0)
                    break;
            }

            return recordId > 0;
        }

        private object GetValidValue(object model, PropertyInfo propInfo)
        {
            if (model == null || propInfo == null)
                return null;

            object propValue = propInfo.GetValue(model, null);
            if (propValue == null)
                return null;

            if (propInfo.PropertyType == typeof(string))
            {
                string strValue = propValue.ToString();
                if (String.IsNullOrEmpty(strValue))
                    return null;

                if (propInfo.HasLogSocSecAttribute() && strValue.EndsWith(Constants.SOCIALSEC_ANONYMIZE))
                    return null;
            }
            else if (propInfo.PropertyType == typeof(bool))
            {
                bool boolValue = StringUtility.GetBool(propValue);
                if (!boolValue)
                    return null;
            }

            return propValue;
        }

        #endregion
    }
}
