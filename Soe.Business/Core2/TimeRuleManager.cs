using Newtonsoft.Json;
using SoftOne.Soe.Business.Core.CrGen;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class TimeRuleManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimeRuleManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region TimeRuleCache

        public List<TimeRule> GetTimeRulesFromCache(CompEntities entities, int actorCompanyId, bool onlyFromCache = false)
        {
            return base.GetTimeRulesFromCache(entities, CacheConfig.Company(actorCompanyId, 60 * 60), onlyFromCache: onlyFromCache);
        }

        public void FlushTimeRulesFromCache(int actorCompanyId)
        {
            base.FlushTimeRulesFromCache(CacheConfig.Company(actorCompanyId));
        }

        #endregion

        #region TimeRule

        public IQueryable<TimeRule> GetTimeRulesQuery(CompEntities entities, int actorCompanyId, bool? active = true, List<int> timeRuleIds = null, bool excludeInternal = false, bool onlyIwh = false)
        {
            var query = entities.TimeRule.Where(tr => tr.ActorCompanyId == actorCompanyId).FilterActive(active);
            if (!timeRuleIds.IsNullOrEmpty())
                query = query.Where(tr => timeRuleIds.Contains(tr.TimeRuleId));
            if (onlyIwh)
                query = query.Where(tr => tr.IsInconvenientWorkHours);
            if (excludeInternal)
                query = query.Where(tr => !tr.Internal);
            return query;
        }

        public List<TimeRuleGridDTO> GetTimeRuleGridDTOs(bool? active = null, Guid? cacheKey = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeRule.NoTracking();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeRuleRow.NoTracking();
            return GetTimeRuleGridDTOs(entities, active, cacheKey, includeExpressions: true);
        }

        public List<TimeRuleGridDTO> GetTimeRuleGridDTOs(CompEntities entities, bool? active = null, Guid? cacheKey = null, bool includeExpressions = false)
        {
            IQueryable<TimeRule> query = GetTimeRulesQuery(entities, base.ActorCompanyId, active: active, excludeInternal: true)
                .Include("TimeRuleRow.TimeDeviationCause")
                .Include("TimeRuleRow.DayType")
                .Include("TimeRuleRow.EmployeeGroup")
                .Include("TimeRuleRow.TimeScheduleType");
            if (includeExpressions) query = query
                .Include("TimeRuleExpression.TimeRuleOperand");

            List<TimeRule> timeRules = query.ToList();
            if (timeRules.IsNullOrEmpty())
                return new List<TimeRuleGridDTO>();

            LoadRecursive(timeRules);
            return ConvertToTimeRuleGrid(entities, cacheKey, includeExpressions, timeRules);
        }

        public List<TimeRule> GetAllTimeRules(int actorCompanyId, bool? active = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAllTimeRules(entities, actorCompanyId, active);
        }

        public List<TimeRule> GetAllTimeRules(CompEntities entities, int actorCompanyId, bool? active = null)
        {
            return GetTimeRulesQuery(entities, actorCompanyId, active).ToList();
        }

        public List<TimeRule> GetAllTimeRulesRecursive(int actorCompanyId, bool onlyIwh = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeRule.NoTracking();
            return GetAllTimeRulesRecursive(entities, actorCompanyId, onlyIwh);
        }

        public List<TimeRule> GetAllTimeRulesRecursive(CompEntities entities, int actorCompanyId, bool onlyIwh = false)
        {
            IQueryable<TimeRule> query = GetTimeRulesQuery(entities, actorCompanyId, active: true, onlyIwh: onlyIwh)
                .Include("TimeRuleRow.TimeScheduleType")
                .Include("TimeRuleExpression.TimeRuleOperand");

            List<TimeRule> timeRules = query.ToList();
            if (timeRules.IsNullOrEmpty())
                return new List<TimeRule>();

            LoadRecursive(timeRules);
            if (onlyIwh)
                CollectTimeRuleIwhProperties(entities, timeRules);

            return timeRules;
        }

        public Dictionary<int, string> GetAllTimeRulesDiscardedStateDict(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeRule.NoTracking();
            return GetAllTimeRulesDiscardedStateDict(entities, actorCompanyId);
        }

        public Dictionary<int, string> GetAllTimeRulesDiscardedStateDict(CompEntities entities, int actorCompanyId)
        {
            return entities.TimeRule.Where(tr => tr.ActorCompanyId == actorCompanyId).ToDictionary(k => k.TimeRuleId, v => v.Name);
        }

        public TimeRule GetTimeRule(int timeRuleId, int actorCompanyId, bool? active = null, bool loadRows = false, bool loadExpressions = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeRule.NoTracking();
            return GetTimeRule(entities, timeRuleId, actorCompanyId, active, loadRows, loadExpressions);
        }

        public TimeRule GetTimeRule(CompEntities entities, int timeRuleId, int actorCompanyId, bool? active = null, bool loadRows = false, bool loadExpressions = false)
        {
            var query = GetTimeRulesQuery(entities, base.ActorCompanyId, active: active).Where(tr => tr.TimeRuleId == timeRuleId);
            if (loadRows) query = query
                .Include("TimeRuleRow.TimeDeviationCause")
                .Include("TimeRuleRow.DayType")
                .Include("TimeRuleRow.EmployeeGroup")
                .Include("TimeRuleRow.TimeScheduleType");
            if (loadExpressions) query = query
                .Include("TimeRuleExpression.TimeRuleOperand.TimeRuleExpressionRecursive");

            TimeRule timeRule = query.FirstOrDefault();
            if (timeRule == null)
                return null;

            TimeCode timeCode = TimeCodeManager.GetTimeCode(entities, timeRule.TimeCodeId, actorCompanyId, onlyActive: false);
            CollectTimeRuleProperties(entities, timeRule, timeCode, setRowProperties: loadRows, setExpressionProperties: loadExpressions);
            return timeRule;
        }

        public TimeRuleImportedDetailsDTO GetTimeRuleImportedDetails(int actorCompanyId, int timeRuleId, bool loadDetails)
        {
            TimeRuleImportedDetailsDTO dto = new TimeRuleImportedDetailsDTO();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.DataStorage.NoTracking();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DataStorageRecord.NoTracking();
            DataStorageRecord record = (from d in entities.DataStorageRecord.Include("DataStorage.Parent")
                                        where d.Type == (int)SoeDataStorageRecordType.TimeRuleImport_Rule &&
                                        d.RecordId == timeRuleId &&
                                        d.DataStorage.ActorCompanyId == actorCompanyId
                                        select d).FirstOrDefault();

            if (record != null && record.DataStorage != null && record.DataStorage.Parent != null)
            {
                DataStorage parent = record.DataStorage.Parent;

                dto.CompanyName = parent.Name;
                dto.Imported = parent.Created;
                dto.ImportedBy = parent.CreatedBy;

                if (loadDetails)
                {
                    if (parent.DataCompressed != null)
                    {
                        parent.Data = CompressionUtil.Decompress(parent.DataCompressed);
                        dto.OriginalJson = Encoding.GetEncoding("ISO-8859-1").GetString(parent.Data);
                    }

                    if (record.DataStorage.DataCompressed != null)
                    {
                        record.DataStorage.Data = CompressionUtil.Decompress(record.DataStorage.DataCompressed);
                        dto.Json = Encoding.GetEncoding("ISO-8859-1").GetString(record.DataStorage.Data);
                    }
                }
            }

            return dto;
        }

        public bool IsTimeRuleUsed(CompEntities entities, int timeRuleId)
        {
            return entities.TimeCodeTransaction.Any(i => i.TimeRuleId == timeRuleId && i.State == (int)SoeEntityState.Active);
        }

        private bool ValidateTimeRuleData(List<TimeRuleFormulaWidget> widgets)
        {
            foreach (TimeRuleFormulaWidget widget in widgets)
            {
                switch (widget.TimeRuleType)
                {
                    case SoeTimeRuleOperatorType.TimeRuleOperatorBalance:
                    case SoeTimeRuleOperatorType.TimeRuleOperatorNot:
                        if (!widget.LeftValueId.HasValue || widget.LeftValueId.Value == 0)
                            return false;
                        break;
                }
            }

            return true;
        }

        public ActionResult ValidateTimeRuleStructure(List<TimeRuleFormulaWidget> widgets)
        {
            int countStartParenthesis = 0;
            int countEndParenthesis = 0;
            TimeRuleFormulaWidget previousWidget = null;

            // No widgets
            if (!widgets.Any())
                return new ActionResult(GetText(12251, 1004)); // time.time.timerule.formulaerror.nowidgets

            foreach (TimeRuleFormulaWidget widget in widgets.OrderBy(w => w.Sort))
            {
                if (widget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorEndParanthesis)
                    countEndParenthesis++;
                else if (widget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis)
                    countStartParenthesis++;

                if (previousWidget == null)
                {
                    // First widget                
                    if (widget.IsOperator && widget.TimeRuleType != SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis)
                        return new ActionResult(GetText(12250, 1004)); // time.time.timerule.formulaerror.firstwidgetincorrect
                }
                else
                {
                    // Operator following a start parenthesis
                    if (widget.IsOperator && previousWidget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis)
                        return new ActionResult(GetText(12252, 1004)); // time.time.timerule.formulaerror.operatorafterparentheses

                    // Operator following another operator
                    if ((widget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorAnd || widget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorOr) &&
                        (previousWidget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorAnd || previousWidget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorOr))
                        return new ActionResult(GetText(12253, 1004)); // time.time.timerule.formulaerror.severaloperatorsinarow

                    // Expression following another expression
                    if (widget.IsExpression && previousWidget.IsExpression)
                        return new ActionResult(GetText(12254, 1004)); // time.time.timerule.formulaerror.severalexpressionsinarow

                    // Expression following an end parenthesis
                    if (widget.IsExpression && previousWidget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorEndParanthesis)
                        return new ActionResult(GetText(12259, 1004)); // time.time.timerule.formulaerror.expressionafterparenthesis

                    if (widget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis && previousWidget.IsExpression)
                        return new ActionResult(GetText(12260, 1004)); // time.time.timerule.formulaerror.startparenthesisafterexpression
                }

                previousWidget = widget;
            }

            // Check last item
            if (previousWidget != null &&
                (
                previousWidget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorAnd ||
                previousWidget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorOr ||
                previousWidget.TimeRuleType == SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis)
                )
                return new ActionResult(GetText(12255, 1004)); // time.time.timerule.formulaerror.lastwidgetincorrect

            // Check number of parenthesis
            if (countStartParenthesis != countEndParenthesis)
                return new ActionResult(GetText(12256, 1004)); // time.time.timerule.formulaerror.incorrectamountofparenthesis

            // Validate data
            if (!ValidateTimeRuleData(widgets))
                return new ActionResult(GetText(12261, 1004)); // time.time.timerule.formulaerror.expressionmissingdata

            return new ActionResult(true);
        }

        public ActionResult ValidateTimeRule(TimeRuleDTO timeRule)
        {
            if (timeRule == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeRule");

            //Validate expressions
            if (timeRule.TimeRuleExpressions != null)
            {
                //Validate that expression
                foreach (var expression in timeRule.TimeRuleExpressions)
                {
                    //Validate that operands
                    foreach (var operand in expression.TimeRuleOperands)
                    {
                        //Validate balance operands
                        if (operand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorBalance)
                        {
                            //Validate that balance operands doesnt use the TimeCode that the rule generates
                            if (operand.LeftValueType == SoeTimeRuleValueType.TimeCodeLeft && operand.LeftValueId.HasValue && operand.LeftValueId.Value == timeRule.TimeCodeId)
                                return new ActionResult((int)ActionResultSave.TimeRuleTimeCodeCannotBeUsedInBalanceOperand);
                            if (operand.RightValueType == SoeTimeRuleValueType.TimeCodeRight && operand.RightValueId.HasValue && operand.RightValueId.Value == timeRule.TimeCodeId)
                                return new ActionResult((int)ActionResultSave.TimeRuleTimeCodeCannotBeUsedInBalanceOperand);
                        }
                    }
                }
            }

            return new ActionResult(true);
        }

        public ActionResult SaveTimeRule(TimeRuleDTO timeRuleInput, int actorCompanyId, bool setTimeRuleOperatorType)
        {
            using (CompEntities entities = new CompEntities())
            {
                return SaveTimeRule(entities, timeRuleInput, actorCompanyId, setTimeRuleOperatorType);
            }
        }

        public ActionResult SaveTimeRule(CompEntities entities, TimeRuleDTO timeRuleInput, int actorCompanyId, bool setTimeRuleOperatorType)
        {
            if (timeRuleInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeRule");
            if (timeRuleInput.TimeDeviationCauseIds.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91988, "Minst en orsak måste anges"));

            ActionResult result = new ActionResult(true);
            int timeRuleId = timeRuleInput.TimeRuleId;

            try
            {
                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "Company");

                result = ValidateTimeRule(timeRuleInput);
                if (!result.Success)
                    return result;

                if (setTimeRuleOperatorType && timeRuleInput.TimeRuleExpressions != null)
                {
                    foreach (List<TimeRuleOperandDTO> operands in timeRuleInput.TimeRuleExpressions.Where(e => e.TimeRuleOperands != null).Select(e => e.TimeRuleOperands))
                    {
                        foreach (TimeRuleOperandDTO operand in operands)
                        {
                            operand.LeftValueType = GetLeftValueType(operand.LeftValueId);
                            operand.RightValueType = GetRightValueType(operand.RightValueId);
                        }
                    }
                }

                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    TimeRule timeRule = GetTimeRule(entities, timeRuleId, actorCompanyId, loadRows: true, loadExpressions: true);
                    if (timeRule == null)
                    {
                        timeRule = new TimeRule()
                        {
                            Company = company
                        };
                        SetCreatedProperties(timeRule);
                        entities.TimeRule.AddObject(timeRule);
                    }
                    else
                    {
                        SetModifiedProperties(timeRule);
                    }

                    SetTimeRuleProperties(entities, timeRule, timeRuleInput, actorCompanyId, true, true);

                    result = SaveChanges(entities, transaction);
                    if (result.Success)
                    {
                        transaction.Complete();
                        timeRuleId = timeRule.TimeRuleId;
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result = new ActionResult(ex);
            }
            finally
            {
                FlushTimeRulesFromCache(actorCompanyId);

                if (result.Success)
                    result.IntegerValue = timeRuleId;
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                entities.Connection.Close();
            }

            return result;
        }

        public ActionResult DeleteTimeRule(int actorCompanyId, int timeRuleId)
        {
            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                TimeRule timeRule = GetTimeRule(entities, timeRuleId, actorCompanyId);
                if (timeRule == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeRule");

                if (IsTimeRuleUsed(entities, timeRule.TimeRuleId))
                    return new ActionResult((int)ActionResultDelete.TimeRuleIsUsed, GetText(11872, "Tidsregeln används och kan inte tas bort. Slutdatum kan sättas istället"));

                result = ChangeEntityState(entities, timeRule, SoeEntityState.Deleted, true);

                // If time rule is imported, also clean up in DataStorage and DataStorageRecord
                if (result.Success && timeRule.Imported)
                {
                    DataStorageRecord ruleRecord = (from d in entities.DataStorageRecord
                                                    where d.Type == (int)SoeDataStorageRecordType.TimeRuleImport_Rule &&
                                                    d.RecordId == timeRuleId
                                                    select d).FirstOrDefault();

                    if (ruleRecord != null)
                    {
                        DataStorage ruleStorage = (from d in entities.DataStorage
                                                   where d.ActorCompanyId == actorCompanyId &&
                                                   d.DataStorageId == ruleRecord.DataStorageId &&
                                                   d.Type == (int)SoeDataStorageRecordType.TimeRuleImport_Rule &&
                                                   d.State == (int)SoeEntityState.Active
                                                   select d).FirstOrDefault();

                        if (ruleStorage != null)
                        {
                            DataStorage parentStorage = (from d in entities.DataStorage.Include("Children")
                                                         where d.ActorCompanyId == actorCompanyId &&
                                                         d.DataStorageId == ruleStorage.ParentDataStorageId &&
                                                         d.Type == (int)SoeDataStorageRecordType.TimeRuleImport_ExportedRules &&
                                                         d.State == (int)SoeEntityState.Active
                                                         select d).FirstOrDefault();

                            int nbrOfChildren = parentStorage?.Children.Count(c => c.State == (int)SoeEntityState.Active) ?? 0;

                            ChangeEntityState(entities, ruleStorage, SoeEntityState.Deleted, false);
                            if (nbrOfChildren == 1)
                                ChangeEntityState(entities, parentStorage, SoeEntityState.Deleted, false);
                        }

                        entities.DeleteObject(ruleRecord);
                        SaveChanges(entities);
                    }
                }
            }

            FlushTimeRulesFromCache(actorCompanyId);

            return result;
        }

        public ActionResult UpdateTimeRulesState(Dictionary<int, bool> timeRules, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                foreach (KeyValuePair<int, bool> timeRule in timeRules)
                {
                    TimeRule originalTimeRule = GetTimeRule(entities, timeRule.Key, actorCompanyId);
                    if (originalTimeRule == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

                    ChangeEntityState(originalTimeRule, timeRule.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                }

                return SaveChanges(entities);
            }
        }

        public TimeRuleExportImportDTO ExportTimeRules(List<int> timeRuleIds, int actorCompanyId)
        {
            TimeRuleExportImportDTO result = new TimeRuleExportImportDTO()
            {
                TimeRules = new List<TimeRuleEditDTO>(),
                TimeCodes = new List<TimeRuleExportImportTimeCodeDTO>(),
                EmployeeGroups = new List<TimeRuleExportImportEmployeeGroupDTO>(),
                TimeScheduleTypes = new List<TimeRuleExportImportTimeScheduleTypeDTO>(),
                TimeDeviationCauses = new List<TimeRuleExportImportTimeDeviationCauseDTO>(),
                DayTypes = new List<TimeRuleExportImportDayTypeDTO>(),
                ExportedFromCompany = CompanyManager.GetCompanyName(actorCompanyId)
            };

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.TimeRule.NoTracking();
            var query = GetTimeRulesQuery(entitiesReadOnly, actorCompanyId, active: null, timeRuleIds: timeRuleIds, excludeInternal: true)
                .Include("TimeRuleRow.TimeScheduleType")
                .Include("TimeRuleExpression.TimeRuleOperand.TimeRuleExpressionRecursive");

            List<TimeRule> timeRules = query.ToList();
            if (timeRules.IsNullOrEmpty())
                return result;

            List<TimeCode> allTimeCodes = TimeCodeManager.GetTimeCodes(actorCompanyId);
            CollectTimeRuleRowsProperties(timeRules);
            CollectTimeRuleIwhProperties(entitiesReadOnly, timeRules);

            List<int> timeCodeIds = new List<int>();
            foreach (TimeRule timeRule in timeRules)
            {
                foreach (TimeRuleExpression exp in timeRule.TimeRuleExpression)
                {
                    foreach (TimeRuleOperand op in exp.TimeRuleOperand)
                    {
                        if ((SoeTimeRuleValueType)op.LeftValueType == SoeTimeRuleValueType.TimeCodeLeft && op.LeftValueId.HasValue)
                            timeCodeIds.Add(op.LeftValueId.Value);
                        if ((SoeTimeRuleValueType)op.RightValueType == SoeTimeRuleValueType.TimeCodeRight && op.RightValueId.HasValue)
                            timeCodeIds.Add(op.RightValueId.Value);
                    }
                }

                TimeRuleEditDTO dto = timeRule.ToEditDTO();
                if (!dto.EmployeeGroupIds.Any())
                    dto.EmployeeGroupIds.Add(0);
                if (!dto.TimeScheduleTypeIds.Any())
                    dto.TimeScheduleTypeIds.Add(0);

                dto.ExportStartExpression = GetTimeRuleExpressionText(timeRule.GetStartExpression().ToDTO(), allTimeCodes);
                dto.ExportStopExpression = GetTimeRuleExpressionText(timeRule.GetStopExpression().ToDTO(), allTimeCodes);
                result.TimeRules.Add(dto);
            }

            // Add used TimeCodes
            timeCodeIds.AddRange(timeRules.Select(r => r.TimeCodeId).ToList());
            timeCodeIds = timeCodeIds.Distinct().ToList();
            List<TimeCode> timeCodes = allTimeCodes.Where(t => timeCodeIds.Contains(t.TimeCodeId)).OrderBy(t => t.Name).ToList();
            timeCodes.ForEach(t => result.TimeCodes.Add(new TimeRuleExportImportTimeCodeDTO() { TimeCodeId = t.TimeCodeId, Code = t.Code, Name = t.Name }));

            // Add used EmployeeGroups
            List<int> employeeGroupIds = timeRules.SelectMany(r => r.EmployeeGroupIds).Distinct().ToList();
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId)).Where(e => e.ActorCompanyId == actorCompanyId && employeeGroupIds.Contains(e.EmployeeGroupId)).OrderBy(e => e.Name).ToList();
            employeeGroups.ForEach(e => result.EmployeeGroups.Add(new TimeRuleExportImportEmployeeGroupDTO() { EmployeeGroupId = e.EmployeeGroupId, Name = e.Name }));
            if (timeRules.Any(r => r.EmployeeGroupIds.Contains(0)))
                result.EmployeeGroups.Insert(0, new TimeRuleExportImportEmployeeGroupDTO() { EmployeeGroupId = 0, Name = GetText(4366, "Alla") });

            // Add used TimeScheduleTypes
            List<int> timeScheduleTypeIds = timeRules.SelectMany(r => r.TimeScheduleTypeIds).Distinct().ToList();
            List<TimeScheduleType> timeScheduleTypes = GetTimeScheduleTypesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId)).Where(t => timeScheduleTypeIds.Contains(t.TimeScheduleTypeId)).OrderBy(t => t.Name).ToList();
            timeScheduleTypes.ForEach(t => result.TimeScheduleTypes.Add(new TimeRuleExportImportTimeScheduleTypeDTO() { TimeScheduleTypeId = t.TimeScheduleTypeId, Code = t.Code, Name = t.Name }));
            if (timeRules.Any(r => r.TimeScheduleTypeIds.Contains(0)))
                result.TimeScheduleTypes.Insert(0, new TimeRuleExportImportTimeScheduleTypeDTO() { TimeScheduleTypeId = 0, Name = GetText(5777, "Utan särskild schematyp") });

            // Add used TimeDeviationCauses
            List<int> timeDeviationCauseIds = timeRules.SelectMany(r => r.TimeDeviationCauseIds).Distinct().ToList();
            List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId)).Where(t => timeDeviationCauseIds.Contains(t.TimeDeviationCauseId)).OrderBy(t => t.Name).ToList();
            timeDeviationCauses.ForEach(t => result.TimeDeviationCauses.Add(new TimeRuleExportImportTimeDeviationCauseDTO() { TimeDeviationCauseId = t.TimeDeviationCauseId, Type = (TermGroup_TimeDeviationCauseType)t.Type, Name = t.Name }));

            // Add used DayTypes
            List<int> dayTypeIds = timeRules.SelectMany(r => r.DayTypeIds).Distinct().ToList();
            List<DayType> dayTypes = GetDayTypesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId)).Where(d => dayTypeIds.Contains(d.DayTypeId)).OrderBy(d => d.Name).ToList();
            dayTypes.ForEach(d => result.DayTypes.Add(new TimeRuleExportImportDayTypeDTO() { DayTypeId = d.DayTypeId, SysDayTypeId = d.SysDayTypeId, Type = (TermGroup_SysDayType)d.Type, Name = d.Name }));

            return result;
        }

        public TimeRuleExportImportDTO ImportTimeRules(Stream stream, int actorCompanyId)
        {
            TimeRuleExportImportDTO importDTO = new TimeRuleExportImportDTO();

            string jsonString = null;
            stream.Position = 0;
            using (StreamReader sr = new StreamReader(stream))
            {
                jsonString = sr.ReadToEnd();
            }

            if (!String.IsNullOrEmpty(jsonString))
            {
                importDTO = JsonConvert.DeserializeObject<TimeRuleExportImportDTO>(jsonString);
                importDTO.OriginalJson = jsonString;
            }

            List<TimeCode> allTimeCodes = TimeCodeManager.GetTimeCodes(actorCompanyId);

            List<TimeRuleEditDTO> rules = importDTO.TimeRules;

            // Local cache
            Dictionary<int, int> replacementTimeCodes = new Dictionary<int, int>();
            Dictionary<int, int> replacementEmployeeGroups = new Dictionary<int, int>();
            Dictionary<int, int> replacementTimeScheduleTypes = new Dictionary<int, int>();
            Dictionary<int, int> replacementTimeDeviationCauses = new Dictionary<int, int>();
            Dictionary<int, int> replacementDayTypes = new Dictionary<int, int>();

            // Replace ID's by matching om codes, names etc.
            foreach (TimeRuleEditDTO rule in rules)
            {
                rule.ExportImportUnmatched = new List<TimeRuleExportImportUnmatchedDTO>();
                rule.TimeRuleId = 0;

                #region TimeCode

                int newTimeCodeId = 0;

                // Get TimeCode from export
                TimeRuleExportImportTimeCodeDTO exportTimeCode = importDTO.TimeCodes.FirstOrDefault(t => t.TimeCodeId == rule.TimeCodeId);
                if (exportTimeCode != null)
                {
                    newTimeCodeId = GetReplacementTimeCode(actorCompanyId, rule.TimeCodeId, replacementTimeCodes, exportTimeCode);
                    if (newTimeCodeId != 0)
                    {
                        // TimeCode found, cache for coming rules
                        if (!replacementTimeCodes.ContainsKey(rule.TimeCodeId))
                            replacementTimeCodes.Add(rule.TimeCodeId, newTimeCodeId);
                        exportTimeCode.MatchedTimeCodeId = newTimeCodeId;
                    }
                    else
                    {
                        // TimeCode not found, store exported value to show in GUI
                        rule.ExportImportUnmatched.Add(new TimeRuleExportImportUnmatchedDTO(TimeRuleExportImportUnmatchedType.TimeCode, exportTimeCode.TimeCodeId, exportTimeCode.Code, exportTimeCode.Name));
                    }
                }

                #endregion

                #region EmployeeGroup

                List<int> newEmployeeGroupIds = new List<int>();
                foreach (int employeeGroupId in rule.EmployeeGroupIds)
                {
                    if (replacementEmployeeGroups.ContainsKey(employeeGroupId))
                    {
                        // EmployeeGroup already used in a previous rule
                        newEmployeeGroupIds.Add(replacementEmployeeGroups[employeeGroupId]);
                    }
                    else
                    {
                        // Get EmployeeGroup from export
                        TimeRuleExportImportEmployeeGroupDTO exportEmployeeGroup = importDTO.EmployeeGroups.FirstOrDefault(e => e.EmployeeGroupId == employeeGroupId);
                        if (exportEmployeeGroup != null)
                        {
                            if (employeeGroupId == 0)
                            {
                                // "All"
                                replacementEmployeeGroups.Add(employeeGroupId, 0);
                                newEmployeeGroupIds.Add(0);
                                exportEmployeeGroup.MatchedEmployeeGroupId = 0;
                            }
                            else
                            {
                                // Try find EmployeeGroup based on Name
                                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                                entitiesReadOnly.EmployeeGroup.NoTracking();
                                EmployeeGroup employeeGroup = entitiesReadOnly.EmployeeGroup.FirstOrDefault(e => e.ActorCompanyId == actorCompanyId && e.State == (int)SoeEntityState.Active && e.Name == exportEmployeeGroup.Name);
                                if (employeeGroup != null)
                                {
                                    // EmployeeGroup found, cache for coming rules
                                    replacementEmployeeGroups.Add(employeeGroupId, employeeGroup.EmployeeGroupId);
                                    newEmployeeGroupIds.Add(employeeGroup.EmployeeGroupId);
                                    exportEmployeeGroup.MatchedEmployeeGroupId = employeeGroup.EmployeeGroupId;
                                }
                                else
                                {
                                    // EmployeeGroup not found, store exported value to show in GUI
                                    rule.ExportImportUnmatched.Add(new TimeRuleExportImportUnmatchedDTO(TimeRuleExportImportUnmatchedType.EmployeeGroup, exportEmployeeGroup.EmployeeGroupId, null, exportEmployeeGroup.Name));
                                }
                            }
                        }
                    }
                }

                #endregion

                #region TimeScheduleType

                List<int> newTimeScheduleTypeIds = new List<int>();
                foreach (int timeScheduleTypeId in rule.TimeScheduleTypeIds)
                {
                    if (replacementTimeScheduleTypes.ContainsKey(timeScheduleTypeId))
                    {
                        // TimeScheduleType already used in a previous rule
                        newTimeScheduleTypeIds.Add(replacementTimeScheduleTypes[timeScheduleTypeId]);
                    }
                    else
                    {
                        // Get TimeScheduleType from export
                        TimeRuleExportImportTimeScheduleTypeDTO exportTimeScheduleType = importDTO.TimeScheduleTypes.FirstOrDefault(e => e.TimeScheduleTypeId == timeScheduleTypeId);
                        if (exportTimeScheduleType != null)
                        {
                            if (timeScheduleTypeId == 0)
                            {
                                // "All"
                                replacementTimeScheduleTypes.Add(timeScheduleTypeId, 0);
                                newTimeScheduleTypeIds.Add(0);
                                exportTimeScheduleType.MatchedTimeScheduleTypeId = 0;
                            }
                            else
                            {
                                // Try find TimeScheduleType based on Name
                                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                                entitiesReadOnly.TimeScheduleType.NoTracking();
                                TimeScheduleType timeScheduleType = entitiesReadOnly.TimeScheduleType.FirstOrDefault(t => t.ActorCompanyId == actorCompanyId && t.State == (int)SoeEntityState.Active && t.Code == exportTimeScheduleType.Code) ??
                                                                    entitiesReadOnly.TimeScheduleType.FirstOrDefault(t => t.ActorCompanyId == actorCompanyId && t.State == (int)SoeEntityState.Active && t.Name == exportTimeScheduleType.Name);
                                if (timeScheduleType != null)
                                {
                                    // TimeScheduleType found, cache for coming rules
                                    replacementTimeScheduleTypes.Add(timeScheduleTypeId, timeScheduleType.TimeScheduleTypeId);
                                    newTimeScheduleTypeIds.Add(timeScheduleType.TimeScheduleTypeId);
                                    exportTimeScheduleType.MatchedTimeScheduleTypeId = timeScheduleType.TimeScheduleTypeId;
                                }
                                else
                                {
                                    // TimeScheduleType not found, store exported value to show in GUI
                                    rule.ExportImportUnmatched.Add(new TimeRuleExportImportUnmatchedDTO(TimeRuleExportImportUnmatchedType.TimeScheduleType, exportTimeScheduleType.TimeScheduleTypeId, exportTimeScheduleType.Code, exportTimeScheduleType.Name));
                                }
                            }
                        }
                    }
                }

                #endregion

                #region TimeDeviationCause

                List<int> newTimeDeviationCauseIds = new List<int>();
                foreach (int timeDeviationCauseId in rule.TimeDeviationCauseIds)
                {
                    if (replacementTimeDeviationCauses.ContainsKey(timeDeviationCauseId))
                    {
                        // TimeDeviationCause already used in a previous rule
                        newTimeDeviationCauseIds.Add(replacementTimeDeviationCauses[timeDeviationCauseId]);
                    }
                    else
                    {
                        // Get TimeDeviationCause from export
                        TimeRuleExportImportTimeDeviationCauseDTO exportTimeDeviationCause = importDTO.TimeDeviationCauses.FirstOrDefault(e => e.TimeDeviationCauseId == timeDeviationCauseId);
                        if (exportTimeDeviationCause != null)
                        {
                            // Try find TimeDeviationCause based on Name
                            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                            entitiesReadOnly.TimeDeviationCause.NoTracking();
                            TimeDeviationCause timeDeviationCause = entitiesReadOnly.TimeDeviationCause.FirstOrDefault(t => t.ActorCompanyId == actorCompanyId && t.State == (int)SoeEntityState.Active && t.Name == exportTimeDeviationCause.Name);
                            if (timeDeviationCause != null)
                            {
                                // TimeDeviationCause found, cache for coming rules
                                replacementTimeDeviationCauses.Add(timeDeviationCauseId, timeDeviationCause.TimeDeviationCauseId);
                                newTimeDeviationCauseIds.Add(timeDeviationCause.TimeDeviationCauseId);
                                exportTimeDeviationCause.MatchedTimeDeviationCauseId = timeDeviationCause.TimeDeviationCauseId;
                            }
                            else
                            {
                                // TimeDeviationCause not found, store exported value to show in GUI
                                rule.ExportImportUnmatched.Add(new TimeRuleExportImportUnmatchedDTO(TimeRuleExportImportUnmatchedType.TimeDeviationCause, exportTimeDeviationCause.TimeDeviationCauseId, null, exportTimeDeviationCause.Name));
                            }
                        }
                    }
                }

                #endregion

                #region DayType

                List<int> newDayTypeIds = new List<int>();
                foreach (int dayTypeId in rule.DayTypeIds)
                {
                    if (replacementDayTypes.ContainsKey(dayTypeId))
                    {
                        // DayType already used in a previous rule
                        newDayTypeIds.Add(replacementDayTypes[dayTypeId]);
                    }
                    else
                    {
                        // Get DayType from export
                        TimeRuleExportImportDayTypeDTO exportDayType = importDTO.DayTypes.FirstOrDefault(e => e.DayTypeId == dayTypeId);
                        if (exportDayType != null)
                        {
                            // Try find DayType based on Name
                            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                            entitiesReadOnly.DayType.NoTracking();
                            DayType dayType = entitiesReadOnly.DayType.FirstOrDefault(d => d.ActorCompanyId == actorCompanyId && d.State == (int)SoeEntityState.Active && d.Name == exportDayType.Name);
                            if (dayType != null)
                            {
                                // DayType found, cache for coming rules
                                replacementDayTypes.Add(dayTypeId, dayType.DayTypeId);
                                newDayTypeIds.Add(dayType.DayTypeId);
                                exportDayType.MatchedDayTypeId = dayType.DayTypeId;
                            }
                            else
                            {
                                // DayType not found, store exported value to show in GUI
                                rule.ExportImportUnmatched.Add(new TimeRuleExportImportUnmatchedDTO(TimeRuleExportImportUnmatchedType.DayType, exportDayType.DayTypeId, null, exportDayType.Name));
                            }
                        }
                    }
                }

                #endregion

                #region TimeRuleExpressions

                foreach (TimeRuleExpressionDTO exp in rule.TimeRuleExpressions)
                {
                    foreach (TimeRuleOperandDTO op in exp.TimeRuleOperands.Where(o => o.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorBalance || o.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorNot))
                    {
                        #region LeftValue

                        if (op.LeftValueId.HasValue)
                        {
                            switch (op.LeftValueType)
                            {
                                case SoeTimeRuleValueType.TimeCodeLeft:
                                    int newLeftValueId = 0;
                                    TimeRuleExportImportTimeCodeDTO exportOperandTimeCode = importDTO.TimeCodes.FirstOrDefault(t => t.TimeCodeId == op.LeftValueId.Value);
                                    if (exportOperandTimeCode != null)
                                    {
                                        newLeftValueId = GetReplacementTimeCode(actorCompanyId, op.LeftValueId.Value, replacementTimeCodes, exportOperandTimeCode);
                                        if (newLeftValueId != 0)
                                        {
                                            // TimeCode found, cache for coming rules
                                            if (!replacementTimeCodes.ContainsKey(op.LeftValueId.Value))
                                                replacementTimeCodes.Add(op.LeftValueId.Value, newLeftValueId);
                                            exportOperandTimeCode.MatchedTimeCodeId = newLeftValueId;
                                        }
                                        else
                                        {
                                            // TimeCode not found, store exported value to show in GUI
                                            rule.ExportImportUnmatched.Add(new TimeRuleExportImportUnmatchedDTO(TimeRuleExportImportUnmatchedType.TimeCode, exportOperandTimeCode.TimeCodeId, exportOperandTimeCode.Code, exportOperandTimeCode.Name));
                                        }
                                    }
                                    break;
                            }
                        }

                        #endregion

                        #region RightValue

                        if (op.RightValueId.HasValue)
                        {
                            switch (op.RightValueType)
                            {
                                case SoeTimeRuleValueType.TimeCodeRight:
                                    int newRightValueId = 0;
                                    TimeRuleExportImportTimeCodeDTO exportOperandTimeCode = importDTO.TimeCodes.FirstOrDefault(t => t.TimeCodeId == op.RightValueId.Value);
                                    if (exportOperandTimeCode != null)
                                    {
                                        newRightValueId = GetReplacementTimeCode(actorCompanyId, op.RightValueId.Value, replacementTimeCodes, exportOperandTimeCode);
                                        if (newRightValueId != 0)
                                        {
                                            // TimeCode found, cache for coming rules
                                            if (!replacementTimeCodes.ContainsKey(op.RightValueId.Value))
                                                replacementTimeCodes.Add(op.RightValueId.Value, newRightValueId);
                                            exportOperandTimeCode.MatchedTimeCodeId = newRightValueId;
                                        }
                                        else
                                        {
                                            // TimeCode not found, store exported value to show in GUI
                                            rule.ExportImportUnmatched.Add(new TimeRuleExportImportUnmatchedDTO(TimeRuleExportImportUnmatchedType.TimeCode, exportOperandTimeCode.TimeCodeId, exportOperandTimeCode.Code, exportOperandTimeCode.Name));
                                        }
                                    }
                                    break;
                            }
                        }

                        #endregion
                    }
                }

                rule.ImportStartExpression = GetTimeRuleExpressionText(rule.GetStartExpression(), allTimeCodes);
                rule.ImportStopExpression = GetTimeRuleExpressionText(rule.GetStopExpression(), allTimeCodes);

                #endregion
            }

            return importDTO;
        }

        public TimeRuleExportImportDTO ImportTimeRuleMatch(TimeRuleExportImportDTO dto, int actorCompanyId)
        {
            List<TimeCode> allTimeCodes = TimeCodeManager.GetTimeCodes(actorCompanyId);

            #region TimeCode

            foreach (TimeRuleExportImportTimeCodeDTO timeCode in dto.TimeCodes.Where(t => t.MatchedTimeCodeId > 0).ToList())
            {
                foreach (TimeRuleEditDTO rule in dto.TimeRules.Where(t => t.TimeCodeId == timeCode.TimeCodeId).ToList())
                {
                    rule.TimeCodeId = timeCode.MatchedTimeCodeId;
                }
            }

            #endregion

            #region EmployeeGroup

            foreach (TimeRuleExportImportEmployeeGroupDTO employeeGroup in dto.EmployeeGroups.Where(e => e.MatchedEmployeeGroupId > 0).ToList())
            {
                foreach (TimeRuleEditDTO rule in dto.TimeRules.Where(t => t.EmployeeGroupIds.Contains(employeeGroup.EmployeeGroupId)).ToList())
                {
                    rule.EmployeeGroupIds.Remove(employeeGroup.EmployeeGroupId);
                    rule.EmployeeGroupIds.Add(employeeGroup.MatchedEmployeeGroupId);
                }
            }

            #endregion

            #region TimeScheduleType

            foreach (TimeRuleExportImportTimeScheduleTypeDTO timeScheduleType in dto.TimeScheduleTypes.Where(t => t.MatchedTimeScheduleTypeId > 0).ToList())
            {
                foreach (TimeRuleEditDTO rule in dto.TimeRules.Where(t => t.TimeScheduleTypeIds.Contains(timeScheduleType.TimeScheduleTypeId)).ToList())
                {
                    rule.TimeScheduleTypeIds.Remove(timeScheduleType.TimeScheduleTypeId);
                    rule.TimeScheduleTypeIds.Add(timeScheduleType.MatchedTimeScheduleTypeId);
                }
            }

            #endregion

            #region TimeDeviationCause

            foreach (TimeRuleExportImportTimeDeviationCauseDTO timeDeviationCause in dto.TimeDeviationCauses.Where(e => e.MatchedTimeDeviationCauseId > 0).ToList())
            {
                foreach (TimeRuleEditDTO rule in dto.TimeRules.Where(t => t.TimeDeviationCauseIds.Contains(timeDeviationCause.TimeDeviationCauseId)).ToList())
                {
                    rule.TimeDeviationCauseIds.Remove(timeDeviationCause.TimeDeviationCauseId);
                    rule.TimeDeviationCauseIds.Add(timeDeviationCause.MatchedTimeDeviationCauseId);
                }
            }

            #endregion

            #region DayType

            foreach (TimeRuleExportImportDayTypeDTO dayType in dto.DayTypes.Where(e => e.MatchedDayTypeId > 0).ToList())
            {
                foreach (TimeRuleEditDTO rule in dto.TimeRules.Where(t => t.DayTypeIds.Contains(dayType.DayTypeId)).ToList())
                {
                    rule.DayTypeIds.Remove(dayType.DayTypeId);
                    rule.DayTypeIds.Add(dayType.MatchedDayTypeId);
                }
            }

            #endregion

            #region TimeRuleExpressions

            foreach (TimeRuleExportImportTimeCodeDTO timeCode in dto.TimeCodes.Where(t => t.MatchedTimeCodeId > 0).ToList())
            {
                foreach (TimeRuleEditDTO rule in dto.TimeRules)
                {
                    foreach (TimeRuleExpressionDTO exp in rule.TimeRuleExpressions)
                    {
                        foreach (TimeRuleOperandDTO op in exp.TimeRuleOperands.Where(o => o.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorBalance || o.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorNot))
                        {
                            if (op.LeftValueId.HasValue && op.LeftValueId.Value == timeCode.TimeCodeId && op.LeftValueType == SoeTimeRuleValueType.TimeCodeLeft)
                                op.LeftValueId = timeCode.MatchedTimeCodeId;
                            if (op.RightValueId.HasValue && op.RightValueId.Value == timeCode.TimeCodeId && op.RightValueType == SoeTimeRuleValueType.TimeCodeRight)
                                op.RightValueId = timeCode.MatchedTimeCodeId;
                        }
                    }
                }
            }

            foreach (TimeRuleEditDTO rule in dto.TimeRules)
            {
                rule.ImportStartExpression = GetTimeRuleExpressionText(rule.GetStartExpression(), allTimeCodes);
                rule.ImportStopExpression = GetTimeRuleExpressionText(rule.GetStopExpression(), allTimeCodes);
            }

            #endregion

            return dto;
        }

        public ActionResult ImportTimeRulesSave(TimeRuleExportImportDTO timeRulesInput, int actorCompanyId)
        {
            ActionResult result;

            using (CompEntities entities = new CompEntities())
            {
                // Save import file in DataStorage
                DataStorage fileStorage = new DataStorage()
                {
                    ActorCompanyId = ActorCompanyId,
                    Type = (int)SoeDataStorageRecordType.TimeRuleImport_ExportedRules,
                    Data = Encoding.GetEncoding("ISO-8859-1").GetBytes(timeRulesInput.OriginalJson),
                    FileSize = timeRulesInput.OriginalJson.Length,
                    Name = timeRulesInput.ExportedFromCompany,
                    OriginType = (int)SoeDataStorageOriginType.Json,
                };
                SetCreatedProperties(fileStorage);
                GeneralManager.CompressStorage(entities, fileStorage, false, false);
                entities.DataStorage.AddObject(fileStorage);

                // Loop over rules
                foreach (TimeRuleEditDTO timeRule in timeRulesInput.TimeRules)
                {
                    timeRule.Imported = true;

                    // Save TimeRule
                    ActionResult saveResult = SaveTimeRule(entities, timeRule.ToDTO(), actorCompanyId, false);
                    if (saveResult.Success)
                    {
                        // Save TimeRule in DataStorage
                        string json = JsonConvert.SerializeObject(timeRule);

                        DataStorage ruleStorage = new DataStorage()
                        {
                            ActorCompanyId = ActorCompanyId,
                            Parent = fileStorage,
                            Type = (int)SoeDataStorageRecordType.TimeRuleImport_Rule,
                            Data = Encoding.GetEncoding("ISO-8859-1").GetBytes(json),
                            FileSize = json.Length,
                            OriginType = (int)SoeDataStorageOriginType.Json,
                        };
                        SetCreatedProperties(ruleStorage);
                        GeneralManager.CompressStorage(entities, ruleStorage, false, false);
                        entities.DataStorage.AddObject(ruleStorage);

                        // Create link between DataStorage and TimeRule
                        DataStorageRecord ruleRecord = new DataStorageRecord()
                        {
                            DataStorage = ruleStorage,
                            Entity = (int)SoeEntityType.TimeRule,
                            Type = (int)SoeDataStorageRecordType.TimeRuleImport_Rule,
                            RecordId = saveResult.IntegerValue,
                        };
                        SetCreatedProperties(ruleRecord);
                        entities.DataStorageRecord.AddObject(ruleRecord);
                    }
                }

                result = SaveChanges(entities);
            }

            return result;
        }

        #region Help-methods

        private List<TimeRuleGridDTO> ConvertToTimeRuleGrid(CompEntities entities, Guid? cacheKey, bool includeExpressions, List<TimeRule> timeRules)
        {
            List<TimeRuleGridDTO> dtos = new List<TimeRuleGridDTO>();

            CacheConfig config = CacheConfig.Company(ActorCompanyId, cacheKey);
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, config);
            List<TimeCode> timeCodes = GetTimeCodeFromsCache(entities, config);
            List<DayType> dayTypes = GetDayTypesFromCache(entities, config);
            List<TimeScheduleType> scheduleTypes = GetTimeScheduleTypesFromCache(entities, config);

            foreach (var timeRulesByTimeCode in timeRules.GroupBy(t => t.TimeCodeId))
            {
                TimeCode timeCode = timeCodes.FirstOrDefault(t => t.TimeCodeId == timeRulesByTimeCode.Key);

                foreach (TimeRule timeRule in timeRulesByTimeCode)
                {
                    CollectTimeRuleProperties(entities, timeRule, timeCode, setRowProperties: true, setExpressionProperties: true, employeeGroups, timeCodes, dayTypes, scheduleTypes);

                    dtos.Add(new TimeRuleGridDTO()
                    {
                        TimeRuleId = timeRule.TimeRuleId,
                        ActorCompanyId = timeRule.ActorCompanyId,
                        IsActive = timeRule.State == (int)SoeEntityState.Active,
                        Name = timeRule.Name,
                        Description = timeRule.Description,
                        Sort = timeRule.Sort,
                        StartDate = timeRule.StartDate,
                        StopDate = timeRule.StopDate,
                        Type = (SoeTimeRuleType)timeRule.Type,
                        TypeName = GetText(timeRule.Type, (int)TermGroup.TimeRuleType),
                        StartDirection = timeRule.RuleStartDirection == (int)SoeTimeRuleDirection.Forward ? SoeTimeRuleDirection.Forward : SoeTimeRuleDirection.Backward,
                        StartDirectionName = GetText(timeRule.RuleStartDirection, (int)TermGroup.TimeRuleDirection),
                        Internal = timeRule.Internal,
                        IsInconvenientWorkHours = timeRule.IsInconvenientWorkHours ? GetText(5713, "Ja") : GetText(5714, "Nej"),
                        IsStandby = timeRule.IsStandby ? GetText(5713, "Ja") : GetText(5714, "Nej"),
                        TimeCodeMaxLength = timeRule.TimeCodeMaxLength,
                        TimeCodeId = timeRule.TimeCodeId,
                        TimeCodeName = timeCode?.Name,
                        EmployeeGroupNames = timeRule.EmployeeGroupNames,
                        DayTypeNames = timeRule.DayTypeNames,
                        TimeDeviationCauseNames = timeRule.TimeDeviationCauseNames,
                        TimeScheduleTypesNames = timeRule.TimeScheduleTypeNames,
                        StartExpression = includeExpressions ? GetTimeRuleExpressionText(timeRule.GetStartExpression().ToDTO(), timeCodes) : string.Empty,
                        StopExpression = includeExpressions ? GetTimeRuleExpressionText(timeRule.GetStopExpression().ToDTO(), timeCodes) : string.Empty,
                        Imported = timeRule.Imported,
                        StandardMinutes = timeRule.StandardMinutes?.ToString() ?? string.Empty,
                        BreakIfAnyFailed = timeRule.BreakIfAnyFailed ? GetText(5713, "Ja") : GetText(5714, "Nej"),
                        AdjustStartToTimeBlockStart = timeRule.AdjustStartToTimeBlockStart ? GetText(5713, "Ja") : GetText(5714, "Nej"),
                    });
                }
            }

            return dtos.OrderBy(r => r.Name).ToList();
        }

        private string GetTimeRuleExpressionText(TimeRuleExpressionDTO timeRuleExpression, List<TimeCode> allTimeCodes)
        {
            StringBuilder expression = new StringBuilder();

            try
            {
                if (timeRuleExpression != null)
                {
                    #region Operands

                    foreach (TimeRuleOperandDTO timeRuleOperand in timeRuleExpression.TimeRuleOperands.OrderBy(i => i.OrderNbr))
                    {
                        if (timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorAnd)
                        {
                            #region And

                            expression.Append($" {GetText(5661, "och").ToUpper()}");

                            #endregion
                        }
                        else if (timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorOr)
                        {
                            #region Or

                            expression.Append($" {GetText(11843, "eller").ToUpper()}");

                            #endregion
                        }
                        else if (timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorScheduleIn)
                        {
                            #region Schedule in

                            expression.Append($" {GetText(11844, "Schema in")}");
                            if (timeRuleOperand.Minutes != 0)
                                expression.Append($" ({timeRuleOperand.GetScheduleMinutesMessage()})");

                            #endregion
                        }
                        else if (timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorScheduleOut)
                        {
                            #region Schedule out

                            expression.Append($" {GetText(11845, "Schema ut")}");
                            if (timeRuleOperand.Minutes != 0)
                                expression.Append($" ({timeRuleOperand.GetScheduleMinutesMessage()})");

                            #endregion
                        }
                        else if (timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorClock)
                        {
                            #region Clock

                            expression.Append($" {GetText(11846, "Klockslag")} {CalendarUtility.GetDateFromMinutes(timeRuleOperand.Minutes, true):HH:mm}");
                            if (timeRuleOperand.IsClockPrevDay())
                                expression.Append($" ({GetText(11847, "föreg. dygn")})");
                            else if (timeRuleOperand.IsClockNextDay())
                                expression.Append($" ({GetText(11848, "nästa dygn")})");

                            #endregion
                        }
                        else if (timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorBalance || timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorNot)
                        {
                            #region TimeRuleOperatorBalance / TimeRuleOperatorNot

                            if (timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorBalance)
                                expression.Append($" {GetText(11849, "Saldo")}");
                            else if (timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorNot)
                                expression.Append($" {GetText(11850, "Inte samtidigt som")}");

                            int operandParts = timeRuleOperand.OperatorType == SoeTimeRuleOperatorType.TimeRuleOperatorBalance ? 2 : 1;
                            for (int operandCounter = 1; operandCounter <= operandParts; operandCounter++)
                            {
                                if (operandCounter == 2)
                                {
                                    switch (timeRuleOperand.ComparisonOperator)
                                    {
                                        case SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLessThan:
                                            expression.Append($" <");
                                            break;
                                        case SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLessThanOrEqualsTo:
                                            expression.Append($" <=");
                                            break;
                                        case SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorEqualsTo:
                                            expression.Append($" =");
                                            break;
                                        case SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorGreaterThanOrEqualsTo:
                                            expression.Append($" >=");
                                            break;
                                        case SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorGreaterThan:
                                            expression.Append($" >");
                                            break;
                                        case SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorNotEqual:
                                            expression.Append($" <>");
                                            break;
                                    }
                                }

                                int type = operandCounter == 1 ? (int)timeRuleOperand.LeftValueType : (int)timeRuleOperand.RightValueType;
                                int? id = operandCounter == 1 ? timeRuleOperand.LeftValueId : timeRuleOperand.RightValueId;

                                switch (type)
                                {
                                    case (int)SoeTimeRuleValueType.TimeCodeLeft:
                                    case (int)SoeTimeRuleValueType.TimeCodeRight:
                                        expression.Append($" {allTimeCodes.FirstOrDefault(i => i.TimeCodeId == id)?.Name ?? $"[{id}]"}");
                                        break;
                                    case (int)SoeTimeRuleValueType.ScheduleRight:
                                    case (int)SoeTimeRuleValueType.ScheduleLeft:
                                        expression.Append($" {GetText(11851, "Schematid")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.ScheduleAndBreakLeft:
                                        expression.Append($" {GetText(110680, "Schematid och planerad rast")} ");
                                        break;
                                    case (int)SoeTimeRuleValueType.SchedulePlusOvertimeInOvertimePeriod:
                                        expression.Append($" {GetText(11851, "Schematid+beordrad tid i period")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.Presence:
                                        expression.Append($" {GetText(11852, "Närvaro")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.PresenceWithinSchedule:
                                        expression.Append($" {GetText(11853, "Närvaro inom schema")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.PresenceBeforeSchedule:
                                        expression.Append($" {GetText(11855, "Närvaro innan schema")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.PresenceAfterSchedule:
                                        expression.Append($" {GetText(11856, "Närvaro efter schema")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.PresenceInScheduleHole:
                                        expression.Append($" {GetText(11857, "Närvaro inom hål")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.Payed:
                                        expression.Append($" {GetText(11858, "Betald tid")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.PayedBeforeSchedule:
                                        expression.Append($" {GetText(11859, "Betald innan schema")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.PayedBeforeSchedulePlusSchedule:
                                        expression.Append($" {GetText(11860, "Betald innan schema+schematid")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.PayedAfterSchedule:
                                        expression.Append($" ({GetText(11861, "Betald efter schema")}");
                                        break;
                                    case (int)SoeTimeRuleValueType.PayedAfterSchedulePlusSchedule:
                                        expression.Append($" ({GetText(11862, "Betald efter schema+schematid")}");
                                        break;
                                }
                            }

                            #endregion
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                // Invalid expressions make the grid crash
                base.LogError(ex, this.log);
                expression = new StringBuilder(GetText(4077, "Felaktigt uttryck"));
            }

            return expression.ToString();
        }

        private SoeTimeRuleValueType GetLeftValueType(int? value)
        {
            if (!value.HasValue)
                value = 0;

            switch (value.Value)
            {
                case Constants.TIMERULEOPERAND_LEFTVALUEID_SCHEDULE:
                    return SoeTimeRuleValueType.ScheduleLeft;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_SCHEDULEANDBREAK:
                    return SoeTimeRuleValueType.ScheduleAndBreakLeft;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_SCHEDULE_PLUS_OVERTIME_OVERTIMEPERIOD:
                    return SoeTimeRuleValueType.SchedulePlusOvertimeInOvertimePeriod;

                case Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCE:
                    return SoeTimeRuleValueType.Presence;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCEWITHINSCHEDULE:
                    return SoeTimeRuleValueType.PresenceWithinSchedule;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCEBEFORESCHEDULE:
                    return SoeTimeRuleValueType.PresenceBeforeSchedule;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCEAFTERSCHEDULE:
                    return SoeTimeRuleValueType.PresenceAfterSchedule;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCEINSCHEDULEHOLE:
                    return SoeTimeRuleValueType.PresenceInScheduleHole;

                case Constants.TIMERULEOPERAND_LEFTVALUEID_PAYED:
                    return SoeTimeRuleValueType.Payed;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_PAYEDBEFORESCHEDULE:
                    return SoeTimeRuleValueType.PayedBeforeSchedule;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_PAYEDBEFORESCHEDULE_PLUS_SCHEDULE:
                    return SoeTimeRuleValueType.PayedBeforeSchedulePlusSchedule;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_PAYEDAFTERSCHEDULE:
                    return SoeTimeRuleValueType.PayedAfterSchedule;
                case Constants.TIMERULEOPERAND_LEFTVALUEID_PAYEDAFTERSCHEDULE_PLUS_SCHEDULE:
                    return SoeTimeRuleValueType.PayedAfterSchedulePlusSchedule;

                default:
                    return SoeTimeRuleValueType.TimeCodeLeft;
            }
        }

        private SoeTimeRuleValueType GetRightValueType(int? value)
        {
            if (!value.HasValue)
                value = 0;

            switch (value.Value)
            {
                case Constants.TIMERULEOPERAND_RIGHTVALUEID_SCHEDULE:
                    return SoeTimeRuleValueType.ScheduleRight;
                case Constants.TIMERULEOPERAND_RIGHTVALUEID_FULLTIMEWEEK:
                    return SoeTimeRuleValueType.FulltimeWeek;
                default:
                    return SoeTimeRuleValueType.TimeCodeRight;
            }
        }

        private void LoadRecursive(List<TimeRule> timeRules)
        {
            if (!timeRules.IsNullOrEmpty())
                timeRules.ForEach(timeRule => LoadRecursive(timeRule));
        }

        private void LoadRecursive(TimeRule timeRule)
        {
            if (timeRule == null || timeRule.TimeRuleExpression.IsNullOrEmpty())
                return;

            foreach (TimeRuleExpression expression in timeRule.TimeRuleExpression)
            {
                LoadRecursiveExpression(expression);
            }
        }

        private void SetTimeRuleProperties(CompEntities entities, TimeRule timeRule, TimeRuleDTO timeRuleInput, int actorCompanyId, bool createRows = true, bool createExpressions = true)
        {
            if (timeRule == null || timeRuleInput == null)
                return;

            timeRule.Type = (int)timeRuleInput.Type;
            timeRule.Name = timeRuleInput.Name;
            timeRule.Description = timeRuleInput.Description;
            timeRule.Sort = timeRuleInput.Sort;
            timeRule.RuleStartDirection = timeRuleInput.RuleStartDirection;
            timeRule.RuleStopDirection = timeRuleInput.RuleStopDirection;
            timeRule.TimeCodeMaxLength = timeRuleInput.TimeCodeMaxLength;
            timeRule.TimeCodeMaxPerDay = timeRuleInput.TimeCodeMaxPerDay;
            timeRule.Factor = timeRuleInput.Factor;
            timeRule.BelongsToGroup = timeRuleInput.BelongsToGroup;
            timeRule.Internal = timeRuleInput.Internal;
            timeRule.IsInconvenientWorkHours = timeRuleInput.IsInconvenientWorkHours;
            timeRule.IsStandby = timeRuleInput.IsStandby;
            timeRule.StartDate = timeRuleInput.StartDate;
            timeRule.StopDate = timeRuleInput.StopDate;
            timeRule.Imported = timeRuleInput.Imported;
            timeRule.StandardMinutes = timeRuleInput.StandardMinutes;
            timeRule.BreakIfAnyFailed = timeRuleInput.BreakIfAnyFailed;
            timeRule.AdjustStartToTimeBlockStart = timeRuleInput.AdjustStartToTimeBlockStart;
            timeRule.State = (int)timeRuleInput.State;
            timeRule.TimeCodeId = timeRuleInput.TimeCodeId;
            timeRule.BalanceRuleSettingId = null;

            if (createRows)
            {
                List<int> timeDeviationCauseIds = timeRuleInput.TimeDeviationCauseIds;
                List<int> dayTypeIds = timeRuleInput.DayTypeIds;
                List<int> employeeGroupIds = timeRuleInput.EmployeeGroupIds;
                List<int> timeScheduleTypeIds = timeRuleInput.TimeScheduleTypeIds;

                DeleteTimeRuleRows(entities, timeRule);
                CreateTimeRuleRows(timeRule, timeDeviationCauseIds, dayTypeIds, employeeGroupIds, timeScheduleTypeIds, actorCompanyId);
            }
            if (createExpressions)
            {
                DeleteTimeRuleExpressions(entities, timeRule);
                CreateTimeRuleExpressions(entities, timeRule, timeRuleInput);
            }
        }

        private void CollectTimeRuleProperties(CompEntities entities, TimeRule timeRule, TimeCode timeRuleTimeCode, bool setRowProperties = false, bool setExpressionProperties = false, List<EmployeeGroup> allEmployeeGroups = null, List<TimeCode> allTimeCodes = null, List<DayType> allDayTypes = null, List<TimeScheduleType> allScheduleTypes = null)
        {
            if (timeRule == null)
                return;

            timeRule.SourceId = (int)SoeRuleCopySource.Existing;
            timeRule.SourceName = GetText(timeRule.SourceId, (int)TermGroup.TimeRuleCopySource);
            if (timeRuleTimeCode != null)
            {
                timeRule.TimeCodeName = timeRuleTimeCode.Name;
                timeRule.UsedTimeCodesByRule.Add(timeRuleTimeCode);
            }

            if (setRowProperties)
                CollectTimeRulExpressionsProperties(entities, timeRule, allTimeCodes);
            if (setExpressionProperties)
                CollectTimeRuleRowsPropertiesExtended(timeRule, allEmployeeGroups, allDayTypes, allScheduleTypes);
        }

        private void CollectTimeRuleRowsPropertiesExtended(TimeRule timeRule, List<EmployeeGroup> allEmployeeGroups, List<DayType> allDayTypes, List<TimeScheduleType> allScheduleTypes)
        {
            if (timeRule.TimeRuleRow.IsNullOrEmpty())
                return;

            var employeeGroupsDict = new Dictionary<int, string>();
            var timeDeviationCausesDict = new Dictionary<int, string>();
            var dayTypesDict = new Dictionary<int, string>();
            var timeScheduleTypesDict = new Dictionary<int, string>();

            bool containsAllEmployeeGroups = timeRule.ContainsAllEmployeeGroups();
            if (containsAllEmployeeGroups)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(GetText(4366, "Alla"));
                if (!allEmployeeGroups.IsNullOrEmpty())
                    sb.Append($" ({allEmployeeGroups.OrderBy(i => i.Name).Select(i => i.Name).ToCommaSeparated()})");
                employeeGroupsDict.Add(0, sb.ToString());
            }

            bool containsAllDayTypes = timeRule.ContainsAllDayTypes();
            if (containsAllDayTypes)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(GetText(4366, "Alla"));
                if (!allDayTypes.IsNullOrEmpty())
                    sb.Append($" ({allDayTypes.OrderBy(i => i.Name).Select(i => i.Name).ToCommaSeparated()})");
                dayTypesDict.Add(0, sb.ToString());
            }

            var timeScheduleTypeAll = timeRule.GetAllTimeScheduleType();
            bool containsAllTimeScheduleTypes = timeScheduleTypeAll != null;
            if (containsAllTimeScheduleTypes)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(GetText(4366, "Alla"));
                if (!allScheduleTypes.IsNullOrEmpty())
                    sb.Append($" ({allScheduleTypes.Where(i => !i.IsAll).OrderBy(i => i.Name).Select(i => i.Name).ToCommaSeparated()})");
                timeScheduleTypesDict.Add(0, sb.ToString());
                timeScheduleTypesDict.Add(timeScheduleTypeAll.TimeScheduleTypeId, sb.ToString());
            }

            foreach (TimeRuleRow timeRuleRow in timeRule.TimeRuleRow)
            {
                if (!timeDeviationCausesDict.ContainsKey(timeRuleRow.TimeDeviationCauseId) && timeRuleRow.TimeDeviationCause != null)
                    timeDeviationCausesDict.Add(timeRuleRow.TimeDeviationCauseId, timeRuleRow.TimeDeviationCause.Name);
                if (!containsAllDayTypes && timeRuleRow.DayType != null && !dayTypesDict.ContainsKey(timeRuleRow.DayType.DayTypeId))
                    dayTypesDict.Add(timeRuleRow.DayType.DayTypeId, timeRuleRow.DayType.Name);
                if (!containsAllEmployeeGroups && timeRuleRow.EmployeeGroup != null && !employeeGroupsDict.ContainsKey(timeRuleRow.EmployeeGroup.EmployeeGroupId))
                    employeeGroupsDict.Add(timeRuleRow.EmployeeGroup.EmployeeGroupId, timeRuleRow.EmployeeGroup.Name);
                if (!containsAllTimeScheduleTypes && timeRuleRow.TimeScheduleType != null && !timeScheduleTypesDict.ContainsKey(timeRuleRow.TimeScheduleType.TimeScheduleTypeId))
                    timeScheduleTypesDict.Add(timeRuleRow.TimeScheduleType.TimeScheduleTypeId, timeRuleRow.TimeScheduleType.Name);
            }

            timeRule.TimeDeviationCausesDict = timeDeviationCausesDict;
            timeRule.DayTypesDict = dayTypesDict;
            timeRule.TimeDeviationCauseIds = timeDeviationCausesDict.Select(i => i.Key).ToList();
            timeRule.TimeDeviationCauseNames = StringUtility.GetCommaSeparatedString<string>(timeDeviationCausesDict.Select(i => i.Value).OrderBy(v => v).ToList());
            timeRule.DayTypeIds = dayTypesDict.Select(i => i.Key).ToList();
            timeRule.DayTypeNames = StringUtility.GetCommaSeparatedString<string>(dayTypesDict.Select(i => i.Value).OrderBy(v => v).ToList());
            timeRule.EmployeeGroupIds = employeeGroupsDict.Select(i => i.Key).ToList();
            timeRule.EmployeeGroupNames = StringUtility.GetCommaSeparatedString<string>(employeeGroupsDict.Select(i => i.Value).OrderBy(v => v).ToList());
            timeRule.TimeScheduleTypeIds = timeScheduleTypesDict.Select(i => i.Key).ToList();
            timeRule.TimeScheduleTypeNames = StringUtility.GetCommaSeparatedString<string>(timeScheduleTypesDict.Select(i => i.Value).OrderBy(v => v).ToList());
        }

        private void CollectTimeRuleRowsProperties(List<TimeRule> timeRules)
        {
            if (!timeRules.IsNullOrEmpty())
                timeRules.ForEach(timeRule => CollectTimeRuleRowsProperties(timeRule));
        }

        private void CollectTimeRuleRowsProperties(TimeRule timeRule)
        {
            if (timeRule == null)
                return;

            timeRule.EmployeeGroupIds = new List<int>();
            timeRule.TimeScheduleTypeIds = new List<int>();
            timeRule.TimeDeviationCauseIds = new List<int>();
            timeRule.DayTypeIds = new List<int>();

            bool containsAllEmployeeGroups = timeRule.ContainsAllEmployeeGroups();
            if (containsAllEmployeeGroups)
                timeRule.EmployeeGroupIds.Add(0);

            TimeScheduleType allTimeScheduleType = timeRule.GetAllTimeScheduleType();
            bool containsAllTimeScheduleTypes = allTimeScheduleType != null;
            if (containsAllTimeScheduleTypes)
                timeRule.TimeScheduleTypeIds.Add(allTimeScheduleType.TimeScheduleTypeId);

            bool containsAllDayTypes = timeRule.ContainsAllDayTypes();

            foreach (TimeRuleRow timeRuleRow in timeRule.TimeRuleRow)
            {
                if (!containsAllEmployeeGroups && timeRuleRow.EmployeeGroupId.HasValue && !timeRule.EmployeeGroupIds.Contains(timeRuleRow.EmployeeGroupId.Value))
                    timeRule.EmployeeGroupIds.Add(timeRuleRow.EmployeeGroupId.Value);
                if (!containsAllTimeScheduleTypes && timeRuleRow.TimeScheduleTypeId.HasValue && !timeRule.TimeScheduleTypeIds.Contains(timeRuleRow.TimeScheduleTypeId.Value))
                    timeRule.TimeScheduleTypeIds.Add(timeRuleRow.TimeScheduleTypeId.Value);
                if (!timeRule.TimeDeviationCauseIds.Contains(timeRuleRow.TimeDeviationCauseId))
                    timeRule.TimeDeviationCauseIds.Add(timeRuleRow.TimeDeviationCauseId);
                if (!containsAllDayTypes && timeRuleRow.DayTypeId.HasValue && !timeRule.DayTypeIds.Contains(timeRuleRow.DayTypeId.Value))
                    timeRule.DayTypeIds.Add(timeRuleRow.DayTypeId.Value);
            }
        }

        private void CollectTimeRulExpressionsProperties(CompEntities entities, TimeRule timeRule, List<TimeCode> allTimeCodes)
        {
            if (timeRule.TimeRuleExpression.IsNullOrEmpty())
                return;

            foreach (var timeRuleOperands in timeRule.TimeRuleExpression.Select(e => e.TimeRuleOperand))
            {
                if (timeRuleOperands.IsNullOrEmpty())
                    continue;

                foreach (TimeRuleOperand operand in timeRuleOperands)
                {
                    if (operand.LeftValueId.HasValue && operand.LeftValueType.HasValue && operand.LeftValueType.Value == (int)SoeTimeRuleValueType.TimeCodeLeft && !timeRule.UsedTimeCodesByRule.Any(t => t.TimeCodeId == operand.LeftValueId.Value))
                    {
                        TimeCode operandTimeCodeLeft = GetOperandTimeCode(operand.LeftValueId.Value);
                        if (operandTimeCodeLeft != null)
                            timeRule.UsedTimeCodesByRule.Add(operandTimeCodeLeft);
                    }
                    if (operand.RightValueId.HasValue && operand.RightValueType.HasValue && operand.RightValueType.Value == (int)SoeTimeRuleValueType.TimeCodeRight && !timeRule.UsedTimeCodesByRule.Any(t => t.TimeCodeId == operand.RightValueId.Value))
                    {
                        TimeCode operandTimeCodeRight = GetOperandTimeCode(operand.RightValueId.Value);
                        if (operandTimeCodeRight != null)
                            timeRule.UsedTimeCodesByRule.Add(operandTimeCodeRight);
                    }
                }
            }

            TimeCode GetOperandTimeCode(int timeCodeId)
            {
                TimeCode operandTimeCode = allTimeCodes?.FirstOrDefault(i => i.TimeCodeId == timeCodeId);
                if (operandTimeCode == null)
                {
                    operandTimeCode = TimeCodeManager.GetTimeCode(entities, timeCodeId, timeRule.ActorCompanyId, onlyActive: false);
                    if (operandTimeCode != null && allTimeCodes != null)
                        allTimeCodes.Add(operandTimeCode);
                }
                return operandTimeCode;
            }
        }

        private void CollectTimeRuleIwhProperties(CompEntities entities, List<TimeRule> timeRules)
        {
            if (timeRules.IsNullOrEmpty())
                return;

            var inconvenientWorkHourRules = timeRules.Where(tr => tr.IsInconvenientWorkHours).ToList();
            if (inconvenientWorkHourRules.IsNullOrEmpty())
                return;

            foreach (var timeRulesByTimeCode in inconvenientWorkHourRules.GroupBy(tr => tr.TimeCodeId))
            {
                TimeCode timeCode = TimeCodeManager.GetTimeCodeWithPayrollProducts(entities, timeRulesByTimeCode.Key, timeRulesByTimeCode.First().ActorCompanyId);
                PayrollProduct payrollProduct = timeCode?.TimeCodePayrollProduct?.FirstOrDefault()?.PayrollProduct;
                if (payrollProduct == null)
                    continue;

                foreach (TimeRule timeRule in timeRulesByTimeCode)
                {
                    timeRule.PayrollProductId = payrollProduct.ProductId;
                    timeRule.PayrollProductName = payrollProduct.Name;
                    timeRule.PayrollProductFactor = payrollProduct.Factor;
                    timeRule.PayrollProductExternalCode = payrollProduct.ExternalNumberOrNumber;
                    timeRule.NrOfTimeCodePayrollProducts = timeCode.TimeCodePayrollProduct.Count;
                    if (timeRule.NrOfTimeCodePayrollProducts > 1)
                        timeRule.Information = GetText(5928, "Det finns mer än en mappning Tidkod-Löneart. Den första används.");
                }
            }
        }

        private int GetReplacementTimeCode(int actorCompanyId, int timeCodeId, Dictionary<int, int> replacementTimeCodes, TimeRuleExportImportTimeCodeDTO exportTimeCode)
        {
            if (replacementTimeCodes.ContainsKey(timeCodeId))
            {
                // TimeCode already used in a previous rule
                return replacementTimeCodes[timeCodeId];
            }
            else
            {
                if (exportTimeCode != null)
                {
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    entitiesReadOnly.TimeCode.NoTracking();
                    TimeCode timeCode = entitiesReadOnly.TimeCode.FirstOrDefault(t => t.ActorCompanyId == actorCompanyId && t.Code == exportTimeCode.Code) ??
                                        entitiesReadOnly.TimeCode.FirstOrDefault(t => t.ActorCompanyId == actorCompanyId && t.State == (int)SoeEntityState.Active && t.Name == exportTimeCode.Name);
                    if (timeCode != null)
                        return timeCode.TimeCodeId;
                }
            }

            return 0;
        }

        #endregion

        #endregion

        #region TimeRuleRow

        public void CreateTimeRuleRows(TimeRule timeRule, List<int> timeDeviationCauseIds, List<int> dayTypeIds, List<int> employeeGroupIds, List<int> timeScheduleTypeIds, int actorCompanyId)
        {
            int nrOfRows = 0;

            //Check TimeDeviationCause
            if (timeDeviationCauseIds == null)
                timeDeviationCauseIds = new List<int>();
            if (timeDeviationCauseIds.Count == 0)
                return; //Must have at least 1
            if (timeDeviationCauseIds.Count > nrOfRows)
                nrOfRows = timeDeviationCauseIds.Count;

            //Check DayType
            if (dayTypeIds == null)
                dayTypeIds = new List<int>();
            if (dayTypeIds.Count == 0)
                dayTypeIds.Add(0); //Add "all"
            if (dayTypeIds.Count > nrOfRows)
                nrOfRows = dayTypeIds.Count;

            //Check EmployeeGroup
            if (employeeGroupIds == null)
                employeeGroupIds = new List<int>();
            if (employeeGroupIds.Count == 0)
                employeeGroupIds.Add(0); //Add "all"
            if (employeeGroupIds.Count > nrOfRows)
                nrOfRows = employeeGroupIds.Count;

            //Check TimeScheduleType
            if (timeScheduleTypeIds == null)
                timeScheduleTypeIds = new List<int>();
            if (timeScheduleTypeIds.Count == 0)
                timeScheduleTypeIds.Add(0); //Add "all"
            if (timeScheduleTypeIds.Count > nrOfRows)
                nrOfRows = timeScheduleTypeIds.Count;

            //Fill all dicts to same size - complete by first value in each dict
            for (int pos = 0; pos < nrOfRows; pos++)
            {
                int counter = pos + 1;

                int timeDeviationCauseId = timeDeviationCauseIds.Count >= counter ? timeDeviationCauseIds[pos] : timeDeviationCauseIds[0];

                int? dayTypeId = dayTypeIds.Count >= counter ? dayTypeIds[pos] : dayTypeIds[0];
                if (dayTypeId == 0)
                    dayTypeId = null;

                int? employeeGroupId = employeeGroupIds.Count >= counter ? employeeGroupIds[pos] : employeeGroupIds[0];
                if (employeeGroupId == 0)
                    employeeGroupId = null;

                int? timeScheduleTypeId = timeScheduleTypeIds.Count >= counter ? timeScheduleTypeIds[pos] : timeScheduleTypeIds[0];
                if (timeScheduleTypeId == 0)
                    timeScheduleTypeId = null;

                CreateTimeRuleRow(timeRule, timeDeviationCauseId, dayTypeId, employeeGroupId, timeScheduleTypeId, actorCompanyId);
            }
        }

        public void CreateTimeRuleRow(TimeRule timeRule, int timeDeviationCauseId, int? dayTypeId, int? employeeGroupId, int? timeScheduleTypeId, int actorCompanyId)
        {
            if (timeRule == null)
                return;

            if (!timeRule.IsAdded() && !timeRule.TimeRuleRow.IsLoaded)
                timeRule.TimeRuleRow.Load();

            timeRule.TimeRuleRow.Add(new TimeRuleRow()
            {
                //Set FK
                ActorCompanyId = actorCompanyId,
                TimeRuleId = timeRule.TimeRuleId,
                TimeDeviationCauseId = timeDeviationCauseId,
                DayTypeId = dayTypeId,
                EmployeeGroupId = employeeGroupId,
                TimeScheduleTypeId = timeScheduleTypeId,
            });
        }

        private void DeleteTimeRuleRows(CompEntities entities, TimeRule timeRule)
        {
            if (timeRule == null || timeRule.IsAdded())
                return;

            // Delete previous TimeRuleRows, will be recreated below
            List<TimeRuleRow> timeRuleRows = timeRule.TimeRuleRow.ToList();
            for (int i = timeRuleRows.Count - 1; i >= 0; i--)
            {
                TimeRuleRow timeRuleRow = timeRuleRows[i];
                entities.DeleteObject(timeRuleRow);
            }
        }

        #endregion

        #region TimeRuleExpression

        private void LoadRecursiveExpression(TimeRuleExpression expression)
        {
            foreach (var operand in expression.TimeRuleOperand)
            {
                if (!operand.TimeRuleExpressionRecursiveReference.IsLoaded)
                    operand.TimeRuleExpressionRecursiveReference.Load();
                if (operand.TimeRuleExpressionRecursive == null)
                    continue;
                LoadRecursiveExpression(operand.TimeRuleExpressionRecursive);
            }
        }

        private void AddRecursiveRuleReference(CompEntities entities, TimeRuleExpressionDTO expressionInput, TimeRuleExpression expression, TimeRule timeRule)
        {
            if (expression.TimeRule == null)
                expression.TimeRule = timeRule;

            foreach (var operandInput in expressionInput.TimeRuleOperands)
            {
                if (operandInput.TimeRuleExpressionRecursive != null)
                    AddRecursiveRuleReference(entities, operandInput.TimeRuleExpressionRecursive, expression, timeRule);
            }
        }

        private void AddRecursive(CompEntities entities, TimeRuleExpression expression)
        {
            //Add expression
            entities.TimeRuleExpression.AddObject(expression);

            //Add operands
            foreach (var operand in expression.TimeRuleOperand)
            {
                entities.TimeRuleOperand.AddObject(operand);

                //Add recursive rules
                if (operand.TimeRuleExpressionRecursive != null)
                    AddRecursive(entities, operand.TimeRuleExpressionRecursive);
            }
        }

        private void CreateTimeRuleExpressions(CompEntities entities, TimeRule timeRule, TimeRuleDTO timeRuleInput)
        {
            foreach (TimeRuleExpressionDTO expressionInput in timeRuleInput.TimeRuleExpressions.ToList())
            {
                // Create expression
                TimeRuleExpression expression = new TimeRuleExpression()
                {
                    IsStart = expressionInput.IsStart
                };
                timeRule.TimeRuleExpression.Add(expression);

                AddRecursiveRuleReference(entities, expressionInput, expression, timeRule);

                foreach (TimeRuleOperandDTO operandInput in expressionInput.TimeRuleOperands.OrderBy(o => o.OrderNbr))
                {
                    // Create operand
                    TimeRuleOperand operand = new TimeRuleOperand()
                    {
                        OperatorType = (int)operandInput.OperatorType,
                        LeftValueType = (int)operandInput.LeftValueType,
                        LeftValueId = operandInput.LeftValueId,
                        RightValueType = (int)operandInput.RightValueType,
                        RightValueId = operandInput.RightValueId,
                        Minutes = operandInput.Minutes,
                        ComparisonOperator = (int)operandInput.ComparisonOperator,
                        OrderNbr = operandInput.OrderNbr,

                        //Set FK
                        TimeRuleExpressionRecursiveId = operandInput.TimeRuleExpressionRecursiveId,
                    };

                    expression.TimeRuleOperand.Add(operand);
                }
            }
        }

        private void DeleteTimeRuleExpressions(CompEntities entities, TimeRule timeRule)
        {
            if (timeRule == null || timeRule.IsAdded())
                return;

            // Delete previous TimeRuleExpression and TimeRuleOperand, will be recreated below
            List<TimeRuleExpression> expressions = timeRule.TimeRuleExpression.ToList();
            for (int i = expressions.Count - 1; i >= 0; i--)
            {
                TimeRuleExpression expression = expressions[i];
                List<TimeRuleOperand> operands = expression.TimeRuleOperand.ToList();
                foreach (TimeRuleOperand operand in operands)
                {
                    entities.DeleteObject(operand);
                }
                entities.DeleteObject(expression);
            }
        }

        #endregion

        #region TimeRuleTimeCode

        public Dictionary<int, string> GetTimeRuleTimeCodesLeft()
        {
            Dictionary<int, string> dict = new Dictionary<int, string> { { 0, " " } };
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_SCHEDULE, GetText(12221, (int)TermGroup.AngularTime, "Schematid"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_SCHEDULEANDBREAK, GetText(14135, (int)TermGroup.AngularTime, "Schematid och planerad rast"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_SCHEDULE_PLUS_OVERTIME_OVERTIMEPERIOD, GetText(12232, (int)TermGroup.AngularTime, "Schematid + beordrad tid i period"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCE, GetText(12222, (int)TermGroup.AngularTime, "Närvaro"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCEWITHINSCHEDULE, GetText(12223, (int)TermGroup.AngularTime, "Närvaro inom schema"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCEBEFORESCHEDULE, GetText(12224, (int)TermGroup.AngularTime, "Närvaro innan schema"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCEAFTERSCHEDULE, GetText(12225, (int)TermGroup.AngularTime, "Närvaro efter schema"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PRESENCEINSCHEDULEHOLE, GetText(12226, (int)TermGroup.AngularTime, "Närvaro inom hål"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PAYED, GetText(12227, (int)TermGroup.AngularTime, "Betald tid"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PAYEDBEFORESCHEDULE, GetText(12228, (int)TermGroup.AngularTime, "Betald tid innan schema"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PAYEDBEFORESCHEDULE_PLUS_SCHEDULE, GetText(12229, (int)TermGroup.AngularTime, "Betald tid innan schema + schematid"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PAYEDAFTERSCHEDULE, GetText(12230, (int)TermGroup.AngularTime, "Betald tid efter schema"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_LEFTVALUEID_PAYEDAFTERSCHEDULE_PLUS_SCHEDULE, GetText(12231, (int)TermGroup.AngularTime, "Betald tid efter schema + schematid"));
            AddTimeRuleOperandTimeCodes(dict);
            return dict;
        }

        public Dictionary<int, string> GetTimeRuleTimeCodesRight()
        {
            Dictionary<int, string> dict = new Dictionary<int, string> { { 0, " " } };
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_RIGHTVALUEID_SCHEDULE, GetText(12221, (int)TermGroup.AngularTime, "Schematid"));
            AddTimeRuleOperandDefinedValue(dict, Constants.TIMERULEOPERAND_RIGHTVALUEID_FULLTIMEWEEK, GetText(12233, (int)TermGroup.AngularTime, "Heltidsmått"));
            AddTimeRuleOperandTimeCodes(dict);
            return dict;
        }

        private void AddTimeRuleOperandDefinedValue(Dictionary<int, string> dict, int type, string label)
        {
            dict.Add(type, $"{Constants.TIMERULEOPERAND_LABELPREFIX} {label}");
        }

        private void AddTimeRuleOperandTimeCodes(Dictionary<int, string> dict)
        {
            Dictionary<int, string> timeCodeDict = TimeCodeManager.GetTimeCodesDict(base.ActorCompanyId, false, false, SoeTimeCodeType.WorkAndAbsenseAndAdditionDeduction);
            foreach (KeyValuePair<int, string> timeCode in timeCodeDict)
            {
                dict.Add(timeCode.Key, timeCode.Value);
            }
        }

        #endregion

        #region TimeAbsenceRule

        public List<TimeAbsenceRuleHead> GetTimeAbsenceRules(GetTimeAbsenceRulesInput input)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeAbsenceRuleHead.NoTracking();
            return GetTimeAbsenceRules(entities, input);
        }

        public List<TimeAbsenceRuleHead> GetTimeAbsenceRules(CompEntities entities, GetTimeAbsenceRulesInput input)
        {
            List<TimeAbsenceRuleHead> timeAbsenceRules =
                BuidTimeAbsenceRuleQuery(entities, input)
                .Where(t => t.ActorCompanyId == input.ActorCompanyId && t.State == (int)SoeEntityState.Active).OrderBy(h => h.Name)
                .ToList();

            SetTimeAbsenceRelations(entities, timeAbsenceRules, input);
            return timeAbsenceRules;
        }

        public TimeAbsenceRuleHead GetTimeAbsenceRuleHead(GetTimeAbsenceRulesInput input)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeAbsenceRuleHead.NoTracking();
            return GetTimeAbsenceRuleHead(entities, input);
        }

        public TimeAbsenceRuleHead GetTimeAbsenceRuleHead(CompEntities entities, GetTimeAbsenceRulesInput input)
        {
            if (!input.TimeAbsenceRuleHeadId.HasValue)
                return null;

            TimeAbsenceRuleHead timeAbsenceRuleHead =
                BuidTimeAbsenceRuleQuery(entities, input)
                .FirstOrDefault(t => t.ActorCompanyId == input.ActorCompanyId && t.TimeAbsenceRuleHeadId == input.TimeAbsenceRuleHeadId && t.State == (int)SoeEntityState.Active);

            SetTimeAbsenceRuleRowRelations(timeAbsenceRuleHead?.TimeAbsenceRuleRow?.ToList());
            return timeAbsenceRuleHead;
        }

        public List<TimeAbsenceRuleRow> GetTimeAbsenceRuleRows(int timeAbsenceRuleHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeAbsenceRuleHead.NoTracking();
            return GetTimeAbsenceRuleRows(entities, timeAbsenceRuleHeadId);
        }

        public List<TimeAbsenceRuleRow> GetTimeAbsenceRuleRows(CompEntities entities, int timeAbsenceRuleHeadId)
        {
            List<TimeAbsenceRuleRow> timeAbsenceRuleRows = entities.TimeAbsenceRuleRow.Include("PayrollProduct")
                .Where(r => r.TimeAbsenceRuleHeadId == timeAbsenceRuleHeadId && r.Start == (int)SoeEntityState.Active)
                .ToList();

            SetTimeAbsenceRuleRowRelations(timeAbsenceRuleRows);
            return timeAbsenceRuleRows;
        }

        public ActionResult SaveTimeAbsenceRuleHead(TimeAbsenceRuleHeadDTO timeAbsenceRuleHeadItem, List<TimeAbsenceRuleRowDTO> timeAbsenceRuleRowItems)
        {
            if (timeAbsenceRuleHeadItem == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeAbsenceRuleHead");

            if (timeAbsenceRuleRowItems.ContainsScope(TermGroup_TimeAbsenceRuleRowScope.Calendaryear) &&
                timeAbsenceRuleRowItems.ContainsScope(TermGroup_TimeAbsenceRuleRowScope.Coherent))
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91949, "Intervaller måste ha samma beräkningsperiod"));

            // Default result is successful
            ActionResult result = new ActionResult();

            int timeAbsenceRuleHeadId = timeAbsenceRuleHeadItem.TimeAbsenceRuleHeadId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        Company company = CompanyManager.GetCompany(entities, base.ActorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                        if (timeAbsenceRuleHeadItem.TimeCodeId.ToNullable() == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8327, "Tidkod kunde inte hittas"));

                        List<TimeAbsenceRuleRow> timeAbsenceRuleRowsInput = timeAbsenceRuleRowItems.FromDTO();

                        #endregion

                        #region TimeAbsenceRuleHead

                        // Get existing TimeAbsenceRuleHead
                        var timeAbsenceRulesInput = new GetTimeAbsenceRulesInput(base.ActorCompanyId, timeAbsenceRuleHeadId)
                        {
                            LoadEmployeeGroups = true,
                            LoadRows = true,
                            LoadRowProducts = true,
                        };
                        TimeAbsenceRuleHead timeAbsenceRuleHead = GetTimeAbsenceRuleHead(entities, timeAbsenceRulesInput);
                        if (timeAbsenceRuleHead == null)
                        {
                            #region Add

                            timeAbsenceRuleHead = new TimeAbsenceRuleHead();
                            SetCreatedProperties(timeAbsenceRuleHead);
                            entities.TimeAbsenceRuleHead.AddObject(timeAbsenceRuleHead);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(timeAbsenceRuleHead);

                            #endregion
                        }

                        timeAbsenceRuleHead.Company = company;
                        timeAbsenceRuleHead.TimeCodeId = timeAbsenceRuleHeadItem.TimeCodeId;
                        timeAbsenceRuleHead.Type = (int)timeAbsenceRuleHeadItem.Type;
                        timeAbsenceRuleHead.Name = timeAbsenceRuleHeadItem.Name;
                        timeAbsenceRuleHead.Description = timeAbsenceRuleHeadItem.Description;
                        timeAbsenceRuleHead.State = (int)timeAbsenceRuleHeadItem.State;
                        SetTimeAbsenceRuleEmployeeGroups(entities, timeAbsenceRuleHead, timeAbsenceRuleHeadItem.EmployeeGroupIds);

                        #endregion

                        #region TimeAbsenceRuleRow

                        foreach (TimeAbsenceRuleRow timeAbsenceRuleRow in timeAbsenceRuleHead.TimeAbsenceRuleRow.Where(r => r.State == (int)SoeEntityState.Active).ToList())
                        {
                            TimeAbsenceRuleRow timeAbsenceRuleRowInput = timeAbsenceRuleRowsInput.FirstOrDefault(r => r.TimeAbsenceRuleRowId == timeAbsenceRuleRow.TimeAbsenceRuleRowId);
                            if (timeAbsenceRuleRowInput != null)
                            {
                                #region Update

                                timeAbsenceRuleRow.PayrollProductId = timeAbsenceRuleRowInput.PayrollProductId;
                                timeAbsenceRuleRow.HasMultiplePayrollProducts = timeAbsenceRuleRowInput.HasMultiplePayrollProducts;
                                timeAbsenceRuleRow.Type = timeAbsenceRuleRowInput.Type;
                                timeAbsenceRuleRow.Scope = timeAbsenceRuleRowInput.Scope;
                                timeAbsenceRuleRow.Start = timeAbsenceRuleRowInput.Start;
                                timeAbsenceRuleRow.Stop = timeAbsenceRuleRowInput.Stop;
                                timeAbsenceRuleRow.State = timeAbsenceRuleRowInput.State;
                                SetModifiedProperties(timeAbsenceRuleRow);

                                // Always delete and recreate product rows
                                foreach (var ruleRowPayrollProduct in timeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.ToList())
                                {
                                    entities.DeleteObject(ruleRowPayrollProduct);
                                }

                                foreach (var rowPayrollProductInput in timeAbsenceRuleRowInput.TimeAbsenceRuleRowPayrollProducts.Where(r => r.SourcePayrollProductId != 0).ToList())
                                {
                                    TimeAbsenceRuleRowPayrollProducts timeAbsenceRuleRowPayrollProduct = new TimeAbsenceRuleRowPayrollProducts()
                                    {
                                        SourcePayrollProductId = rowPayrollProductInput.SourcePayrollProductId,
                                        TargetPayrollProductId = rowPayrollProductInput.TargetPayrollProductId.HasValue && rowPayrollProductInput.TargetPayrollProductId != 0 ? rowPayrollProductInput.TargetPayrollProductId.Value : (int?)null,
                                    };
                                    timeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.Add(timeAbsenceRuleRowPayrollProduct);
                                }

                                // Detach input TimeAbsenceRuleRow to prevent adding new
                                base.TryDetachEntity(entities, timeAbsenceRuleRowInput);

                                #endregion
                            }
                            else
                            {
                                #region Delete

                                if (timeAbsenceRuleRow.State != (int)SoeEntityState.Deleted)
                                    ChangeEntityState(timeAbsenceRuleRow, SoeEntityState.Deleted);

                                #endregion
                            }
                        }

                        #region Add

                        List<TimeAbsenceRuleRow> timeAbsenceRuleRowsInputToAdd = timeAbsenceRuleRowsInput.Where(r => r.TimeAbsenceRuleRowId == 0).ToList();
                        foreach (TimeAbsenceRuleRow timeAbsenceRuleRowInputToAdd in timeAbsenceRuleRowsInputToAdd)
                        {
                            SetCreatedProperties(timeAbsenceRuleRowInputToAdd);
                            timeAbsenceRuleHead.TimeAbsenceRuleRow.Add(timeAbsenceRuleRowInputToAdd);
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            transaction.Complete();
                            timeAbsenceRuleHeadId = timeAbsenceRuleHead.TimeAbsenceRuleHeadId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = timeAbsenceRuleHeadId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DeleteTimeAbsenceRuleHead(int timeAbsenceRuleHeadId)
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeAbsenceRuleHead timeAbsenceRuleHead = GetTimeAbsenceRuleHead(entities, new GetTimeAbsenceRulesInput(base.ActorCompanyId, timeAbsenceRuleHeadId));
                if (timeAbsenceRuleHead == null)
                    return new ActionResult((int)ActionResultDelete.EntityIsNull, "TimeAbsenceRuleHead");

                // Set the inventory to deleted
                return ChangeEntityState(entities, timeAbsenceRuleHead, SoeEntityState.Deleted, true);
            }
        }

        public void CreateTimeAbsenceRuleHeadEmployeeGroup(CompEntities entities, TimeAbsenceRuleHead timeAbsenceRuleHead, int employeeGroupId)
        {
            if (timeAbsenceRuleHead == null)
                return;

            TimeAbsenceRuleHeadEmployeeGroup ruleEmployeeGroup = new TimeAbsenceRuleHeadEmployeeGroup()
            {
                TimeAbsenceRuleHead = timeAbsenceRuleHead,
                EmployeeGroupId = employeeGroupId,
            };
            SetCreatedProperties(ruleEmployeeGroup);
            entities.AddToTimeAbsenceRuleHeadEmployeeGroup(ruleEmployeeGroup);
        }

        #region Help-methods

        private IQueryable<TimeAbsenceRuleHead> BuidTimeAbsenceRuleQuery(CompEntities entities, GetTimeAbsenceRulesInput input)
        {
            IQueryable<TimeAbsenceRuleHead> query = entities.TimeAbsenceRuleHead;
            if (input.LoadCompany)
                query = query.Include("Company");
            if (input.LoadTimeCode)
                query = query.Include("TimeCode");
            if (input.LoadEmployeeGroups)
                query = query.Include("TimeAbsenceRuleHeadEmployeeGroup");
            if (input.LoadRows)
            {
                query = query.Include("TimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts");
                if (input.LoadRowProducts)
                {
                    query = query.Include("TimeAbsenceRuleRow.PayrollProduct");
                    query = query.Include("TimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.SourcePayrollProduct");
                    query = query.Include("TimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.TargetPayrollProduct");
                }
            }
            return query;
        }

        private void SetTimeAbsenceRelations(CompEntities entities, List<TimeAbsenceRuleHead> timeAbsenceRules, GetTimeAbsenceRulesInput input)
        {
            if (timeAbsenceRules.IsNullOrEmpty() || !input.LoadAnyRelation())
                return;

            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(input.ActorCompanyId));
            string allTerm = GetText(4366, "Alla");

            foreach (TimeAbsenceRuleHead timeAbsenceRule in timeAbsenceRules)
            {
                timeAbsenceRule.TypeName = GetText(timeAbsenceRule.Type, (int)TermGroup.TimeAbsenceRuleType);
                if (input.LoadCompany)
                    timeAbsenceRule.CompanyName = timeAbsenceRule.Company?.Name ?? string.Empty;
                if (input.LoadTimeCode)
                    timeAbsenceRule.TimeCodeName = timeAbsenceRule.TimeCode?.Name ?? string.Empty;
                if (input.LoadEmployeeGroups)
                {
                    List<int> employeeGroupIds = timeAbsenceRule.GetEmployeeGroupIds();
                    timeAbsenceRule.EmployeeGroupNames = employeeGroupIds.IsNullOrEmpty() ? allTerm : employeeGroups.Where(eg => employeeGroupIds.Contains(eg.EmployeeGroupId)).Select(eg => eg.Name).Distinct().ToCommaSeparated();
                }

                if (input.LoadRows)
                    SetTimeAbsenceRuleRowRelations(timeAbsenceRule.TimeAbsenceRuleRow?.ToList());
            }
        }

        private void SetTimeAbsenceRuleRowRelations(List<TimeAbsenceRuleRow> timeAbsenceRuleRows)
        {
            if (timeAbsenceRuleRows.IsNullOrEmpty())
                return;

            foreach (TimeAbsenceRuleRow row in timeAbsenceRuleRows.Where(i => i.State == (int)SoeEntityState.Active))
            {
                if (!row.PayrollProductReference.IsLoaded)
                    row.PayrollProductReference.Load();

                row.PayrollProductName = row.PayrollProduct?.Name;
                row.TypeName = GetText(row.Type, (int)TermGroup.TimeAbsenceRuleRowType);
                row.ScopeName = GetText(row.Scope, (int)TermGroup.TimeAbsenceRuleRowScope);

                if (!row.TimeAbsenceRuleRowPayrollProducts.IsLoaded)
                    row.TimeAbsenceRuleRowPayrollProducts.Load();

                foreach (TimeAbsenceRuleRowPayrollProducts productRow in row.TimeAbsenceRuleRowPayrollProducts)
                {
                    if (!productRow.SourcePayrollProductReference.IsLoaded)
                        productRow.SourcePayrollProductReference.Load();
                    if (!productRow.TargetPayrollProductReference.IsLoaded)
                        productRow.TargetPayrollProductReference.Load();
                }
            }
        }

        private void SetTimeAbsenceRuleEmployeeGroups(CompEntities entities, TimeAbsenceRuleHead timeAbsenceRuleHead, List<int> inputEmployeeGroupIds)
        {
            if (timeAbsenceRuleHead == null)
                return;

            if (timeAbsenceRuleHead.TimeAbsenceRuleHeadId > 0 && !timeAbsenceRuleHead.TimeAbsenceRuleHeadEmployeeGroup.IsLoaded)
                timeAbsenceRuleHead.TimeAbsenceRuleHeadEmployeeGroup.Load();

            List<TimeAbsenceRuleHeadEmployeeGroup> timeAbsenceRuleEmployeeGroups = timeAbsenceRuleHead.GetEmployeeGroups();

            inputEmployeeGroupIds = inputEmployeeGroupIds?.Where(id => id > 0).ToList() ?? new List<int>();

            foreach (int employeeGroupId in inputEmployeeGroupIds)
            {
                if (!timeAbsenceRuleEmployeeGroups.Any(reg => reg.EmployeeGroupId == employeeGroupId))
                    CreateTimeAbsenceRuleHeadEmployeeGroup(entities, timeAbsenceRuleHead, employeeGroupId);
            }
            foreach (TimeAbsenceRuleHeadEmployeeGroup ruleEmployeeGroup in timeAbsenceRuleEmployeeGroups)
            {
                if (!inputEmployeeGroupIds.Contains(ruleEmployeeGroup.EmployeeGroupId))
                    ChangeEntityState(ruleEmployeeGroup, SoeEntityState.Deleted);
            }
        }

        #endregion

        #endregion
    }
}
