using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class TimeRuleCopyManager : ManagerBase
    {
        #region Variables

        private bool copyPerformed = false;
        private int actorCompanyId;
        private readonly IEnumerable<int> selectedEmployeeGroupIds;
        private readonly IEnumerable<SelectedItemDTO> selectedTimeRules;
        private readonly IEnumerable<SelectedItemDTO> selectedTimeAbsenceRules;
        private readonly IEnumerable<SelectedItemDTO> selectedAttestRules;
        private readonly IEnumerable<MatchedItemDTO> matchedDayTypes;
        private readonly IEnumerable<MatchedItemDTO> matchedTimeDeviationCauses;
        private readonly IEnumerable<MatchedItemDTO> matchedTimeCodes;
        private readonly IEnumerable<MatchedItemDTO> matchedPayrollProducts;
        private readonly IEnumerable<MatchedItemDTO> dayTypesToCopy;
        private readonly IEnumerable<MatchedItemDTO> timeDeviationCausesToCopy;
        private readonly IEnumerable<MatchedItemDTO> timeCodesToCopy;
        private readonly IEnumerable<MatchedItemDTO> payrollProductsToCopy;

        private readonly CompEntities entities;
        private readonly TransactionScope transaction;

        #endregion

        #region Ctor

        public TimeRuleCopyManager(
            ParameterObject parameterObject,
            CompEntities entities,
            TransactionScope transaction,
            int actorCompanyId,
            IEnumerable<int> selectedEmployeeGroupIds,
            IEnumerable<SelectedItemDTO> selectedTimeRules,
            IEnumerable<SelectedItemDTO> selectedTimeAbsenceRules,
            IEnumerable<SelectedItemDTO> selectedAttestRules,
            IEnumerable<MatchedItemDTO> matchedDayTypes,
            IEnumerable<MatchedItemDTO> matchedTimeDeviationCauses,
            IEnumerable<MatchedItemDTO> matchedTimeCodes,
            IEnumerable<MatchedItemDTO> matchedPayrollProducts,
            IEnumerable<MatchedItemDTO> dayTypesToCopy,
            IEnumerable<MatchedItemDTO> timeDeviationCausesToCopy,
            IEnumerable<MatchedItemDTO> timeCodesToCopy,
            IEnumerable<MatchedItemDTO> payrollProductsToCopy)
            : base(parameterObject)
        {
            this.parameterObject = parameterObject;
            this.entities = entities;
            this.transaction = transaction;
            this.actorCompanyId = actorCompanyId;
            this.selectedEmployeeGroupIds = selectedEmployeeGroupIds;
            this.selectedTimeRules = selectedTimeRules;
            this.selectedTimeAbsenceRules = selectedTimeAbsenceRules;
            this.selectedAttestRules = selectedAttestRules;
            this.matchedDayTypes = matchedDayTypes;
            this.matchedTimeDeviationCauses = matchedTimeDeviationCauses;
            this.matchedTimeCodes = matchedTimeCodes;
            this.matchedPayrollProducts = matchedPayrollProducts;
            this.dayTypesToCopy = dayTypesToCopy;
            this.timeDeviationCausesToCopy = timeDeviationCausesToCopy;
            this.timeCodesToCopy = timeCodesToCopy;
            this.payrollProductsToCopy = payrollProductsToCopy;
        }

        #endregion

        #region Properties

        #region Target properties

        private Company _company;
        private Company Company
        {
            get
            {
                return _company ?? (_company = CompanyManager.GetCompany(this.entities, this.actorCompanyId));
            }
        }

        private IEnumerable<EmployeeGroup> _employeeGroups;
        private IEnumerable<EmployeeGroup> EmployeeGroups
        {
            get
            {
                return _employeeGroups ?? (_employeeGroups = this.GetEmployeeGroups());
            }
        }

        #endregion

        #region Rule components

        private IDictionary<MatchedItemDTO, DayType> _dayTypes;
        private IDictionary<MatchedItemDTO, DayType> DayTypes
        {
            get
            {
                return _dayTypes ?? (_dayTypes = this.ProduceDayTypes());
            }
        }

        private IDictionary<MatchedItemDTO, TimeDeviationCause> _timeDeviationCauses;
        private IDictionary<MatchedItemDTO, TimeDeviationCause> TimeDeviationCauses
        {
            get
            {
                return _timeDeviationCauses ?? (_timeDeviationCauses = this.ProduceTimeDeviationCauses());
            }
        }

        private IDictionary<MatchedItemDTO, TimeCode> _timeCodes;
        private IDictionary<MatchedItemDTO, TimeCode> TimeCodes
        {
            get
            {
                return _timeCodes ?? (_timeCodes = this.ProduceTimeCodes());
            }
        }

        private IDictionary<MatchedItemDTO, PayrollProduct> _payrollProducts;
        private IDictionary<MatchedItemDTO, PayrollProduct> PayrollProducts
        {
            get
            {
                return _payrollProducts ?? (_payrollProducts = this.ProducePayrollProducts());
            }
        }

        #endregion

        #endregion

        #region Produce rule components
        
        private ActionResult ProduceRuleComponents()
        {
            _dayTypes = this.ProduceDayTypes();
            _payrollProducts = this.ProducePayrollProducts();
            _timeDeviationCauses = this.ProduceTimeDeviationCauses();
            _timeCodes = this.ProduceTimeCodes();

            ActionResult result = this.SaveChanges(this.entities);
            return result;
        }

        private IDictionary<MatchedItemDTO, DayType> ProduceDayTypes()
        {
            var retVal = this.dayTypesToCopy.ToDictionary(x => x, x => this.CreateDayType(x));
            foreach (var match in matchedDayTypes)
            {
                retVal[match] = CalendarManager.GetDayType(this.entities, match.TargetItemId, this.actorCompanyId);
            }
            return retVal;
        }

        private IDictionary<MatchedItemDTO, TimeDeviationCause> ProduceTimeDeviationCauses()
        {
            var retVal = this.timeDeviationCausesToCopy.ToDictionary(x => x, x => this.CreateTimeDeviationCause(x));
            foreach (var match in matchedTimeDeviationCauses)
            {
                retVal[match] = TimeDeviationCauseManager.GetTimeDeviationCause(this.entities, match.TargetItemId, this.actorCompanyId, false);
            }
            return retVal;
        }

        private IDictionary<MatchedItemDTO, TimeCode> ProduceTimeCodes()
        {
            var retVal = this.timeCodesToCopy.ToDictionary(x => x, x => this.CreateTimeCode(x));
            foreach (var match in this.matchedTimeCodes)
            {
                retVal[match] = TimeCodeManager.GetTimeCode(this.entities, match.TargetItemId, this.actorCompanyId, false);
            }
            return retVal;
        }

        private IDictionary<MatchedItemDTO, PayrollProduct> ProducePayrollProducts()
        {
            var retVal = this.payrollProductsToCopy.ToDictionary(x => x, x => this.CreatePayrollProduct(x));
            foreach (var match in this.matchedPayrollProducts)
            {
                retVal[match] = ProductManager.GetPayrollProduct(this.entities, match.TargetItemId);
            }
            return retVal;
        }

        private PayrollProduct CreatePayrollProduct(MatchedItemDTO match)
        {            
            PayrollProduct sourcePayrollProduct = ProductManager.GetPayrollProduct(this.entities, match.SourceItemId);
            if (sourcePayrollProduct == null)
                return null;

            PayrollProduct targetPayrollProduct = new PayrollProduct()
            {
                //Product
                Type = (int)SoeProductType.PayrollProduct,
                Number = sourcePayrollProduct.Number,
                Name = sourcePayrollProduct.Name,
                Description = sourcePayrollProduct.Description,
                AccountingPrio = sourcePayrollProduct.AccountingPrio,

                //PayrollProduct
                SysPayrollProductId = sourcePayrollProduct.SysPayrollProductId,
                Factor = sourcePayrollProduct.Factor,
                PayrollType = sourcePayrollProduct.PayrollType,
                ShortName = sourcePayrollProduct.ShortName,
                Export = sourcePayrollProduct.Export,
                ExcludeInWorkTimeSummary = sourcePayrollProduct.ExcludeInWorkTimeSummary,
                Payed = sourcePayrollProduct.Payed,
                SysPayrollTypeLevel1 = sourcePayrollProduct.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = sourcePayrollProduct.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = sourcePayrollProduct.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = sourcePayrollProduct.SysPayrollTypeLevel4,
                ResultType = sourcePayrollProduct.ResultType,
                AverageCalculated = sourcePayrollProduct.AverageCalculated,
                IncludeAmountInExport = sourcePayrollProduct.IncludeAmountInExport,
                UseInPayroll = sourcePayrollProduct.UseInPayroll,

            };
            SetCreatedProperties(targetPayrollProduct);
            this.Company.Product.Add(targetPayrollProduct);

            return targetPayrollProduct;
        }

        #endregion

        #region Create copies

        private DayType CreateDayType(MatchedItemDTO match)
        {
            DayType sourceDayType = CalendarManager.GetDayType(this.entities, match.SourceItemId, match.CompanyId);
            if (sourceDayType == null)
                return null;

            DayType targetDayType = new DayType
            {
                Name = sourceDayType.Name,
                Description = sourceDayType.Description,
                Type = sourceDayType.Type,
                StandardWeekdayFrom = sourceDayType.StandardWeekdayFrom,
                StandardWeekdayTo = sourceDayType.StandardWeekdayTo,

                //Set FK
                ActorCompanyId = this.actorCompanyId,

            };
            return base.AddEntity(this.entities, targetDayType);
        }

        private TimeDeviationCause CreateTimeDeviationCause(MatchedItemDTO match)
        {
            TimeDeviationCause sourceDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(entities, match.SourceItemId, match.CompanyId, false);
            if (sourceDeviationCause == null)
                return null;

            TimeDeviationCause targetDeviationCause = new TimeDeviationCause
            {
                Name = sourceDeviationCause.Name,
                Description = sourceDeviationCause.Description,
                Type = sourceDeviationCause.Type,
                TypeName = sourceDeviationCause.TypeName,
                ExtCode = sourceDeviationCause.ExtCode,
                ImageSource = sourceDeviationCause.ImageSource,
                OnlyWholeDay = sourceDeviationCause.OnlyWholeDay,

                //Set FK
                ActorCompanyId = this.Company.ActorCompanyId,
                TimeCodeId = this.GetTargetTimeCode(sourceDeviationCause.TimeCode, match.CompanyId)?.TimeCodeId,
            };
            entities.TimeDeviationCause.AddObject(targetDeviationCause);

            return base.AddEntity(this.entities, targetDeviationCause);
        }

        private TimeCode CreateTimeCode(MatchedItemDTO match)
        {
            TimeCode sourceTimeCode = TimeCodeManager.GetTimeCodeWithProducts(this.entities, match.SourceItemId, match.CompanyId);
            if (sourceTimeCode == null)
                return null;

            TimeCode targetTimeCode;
            if (sourceTimeCode is TimeCodeWork timeCodeWork)
                targetTimeCode = new TimeCodeWork { IsWorkOutsideSchedule = timeCodeWork.IsWorkOutsideSchedule };
            else
                targetTimeCode = new TimeCode();
            targetTimeCode.Description = sourceTimeCode.Description;
            targetTimeCode.Code = sourceTimeCode.Code;
            targetTimeCode.Name = sourceTimeCode.Name;
            targetTimeCode.RegistrationType = sourceTimeCode.RegistrationType;
            targetTimeCode.Classification = sourceTimeCode.Classification;
            targetTimeCode.RoundingType = sourceTimeCode.RoundingType;
            targetTimeCode.RoundingValue = sourceTimeCode.RoundingValue;
            targetTimeCode.RoundingTimeCodeId = sourceTimeCode.RoundingTimeCodeId;
            targetTimeCode.RoundingGroupKey = sourceTimeCode.RoundingGroupKey;
            targetTimeCode.Type = sourceTimeCode.Type;            

            //Set FK
            targetTimeCode.ActorCompanyId = this.actorCompanyId;

            foreach (TimeCodePayrollProduct payrollProduct in sourceTimeCode.TimeCodePayrollProduct)
            {
                targetTimeCode.TimeCodePayrollProduct.Add(new TimeCodePayrollProduct
                {
                    Factor = payrollProduct.Factor,

                    //Set references
                    TimeCode = targetTimeCode,
                    PayrollProduct = this.GetTargetPayrollProduct(payrollProduct.ProductId, match.CompanyId),
                });
            }

            if (sourceTimeCode.TimeCodeInvoiceProduct.Count > 0)
            {
                throw new NotImplementedException("InvoiceProducts cannot be selected in Wizard for either copy or match. Implement this feature in Time rule copy wizard.");
            }

            return base.AddEntity(this.entities, targetTimeCode, "TimeCode");
        }

        public ActionResult Copy(int actorCompanyId)
        {
            try
            {
                if (this.copyPerformed)
                    throw new InvalidOperationException("Can only copy once. Create new TimeRuleCopyManager to copy another set of rules.");

                this.actorCompanyId = actorCompanyId;

                ActionResult result = this.ProduceRuleComponents();
                if (!result.Success)
                {
                    if (result.Exception != null)
                        result.ErrorMessage = result.Exception.Message;

                    return result;
                }

                this.CopyTimeRules();
                this.CopyTimeAbsenceRules();
                this.CopyAttestRules();

                result = this.SaveChanges(entities, transaction);

                TimeRuleManager.FlushTimeRulesFromCache(this.actorCompanyId);

                return result;
            }
            catch (ActionFailedException afex)
            {
                transaction.Dispose();
                ActionResult result = new ActionResult(afex.ErrorNumber, afex.Message);
                result.Exception = afex.InnerException;
                return result;
            }
            catch (Exception ex)
            {
                transaction.Dispose();
                return new ActionResult(ex);
            }
            finally
            {
                this.copyPerformed = true;
            }
        }

        private void CopyTimeRules()
        {
            foreach (var selectedTimeRule in selectedTimeRules)
            {
                this.CopyTimeRule(this.EmployeeGroups.ToList(), selectedTimeRule);
            }
        }

        private void CopyTimeRule(List<EmployeeGroup> employeeGroups, SelectedItemDTO match)
        {
            TimeRule source = TimeRuleManager.GetTimeRule(this.entities, match.ItemId, match.CompanyId, active: true, loadRows: true, loadExpressions: true);
            this.CopyTimeRule(employeeGroups, source, match.CompanyId);
        }

        private void CopyTimeRule(List<EmployeeGroup> employeeGroups, TimeRule sourceTimeRule, int sourceCompanyId)
        {
            if (sourceTimeRule == null)
                return;

            if (!sourceTimeRule.TimeRuleRow.IsLoaded)
                sourceTimeRule.TimeRuleRow.Load();

            TimeRule targetTimeRule = new TimeRule
            {
                Type = sourceTimeRule.Type,
                Name = sourceTimeRule.Name,
                Description = sourceTimeRule.Description,
                StartDate = sourceTimeRule.StartDate,
                StopDate = sourceTimeRule.StopDate,
                RuleStartDirection = sourceTimeRule.RuleStartDirection,
                RuleStopDirection = sourceTimeRule.RuleStopDirection,
                Factor = sourceTimeRule.Factor,
                BelongsToGroup = sourceTimeRule.BelongsToGroup,
                IsInconvenientWorkHours = sourceTimeRule.IsInconvenientWorkHours,
                TimeCodeMaxLength = sourceTimeRule.TimeCodeMaxLength,
                TimeCodeMaxPerDay = sourceTimeRule.TimeCodeMaxPerDay,
                Sort = sourceTimeRule.Sort,
                Internal = sourceTimeRule.Internal,
                StandardMinutes = sourceTimeRule.StandardMinutes,
                BreakIfAnyFailed = sourceTimeRule.BreakIfAnyFailed,
                AdjustStartToTimeBlockStart = sourceTimeRule.AdjustStartToTimeBlockStart,

                //Set FK
                ActorCompanyId = actorCompanyId,

                //Set references
                TimeCode = this.GetTargetTimeCode(sourceTimeRule.TimeCodeId, sourceCompanyId),
            };

            List<int> targetDeviationCauseIds = new List<int>();
            List<int> targetDayTypeIds = new List<int>();
            List<int> targetEmployeeGroupIds = new List<int>();

            foreach (TimeRuleRow sourceTimeRuleRow in sourceTimeRule.TimeRuleRow)
            {
                TimeDeviationCause targetTimeDeviationCause = this.GetTargetTimeDeviationCause(sourceTimeRuleRow.TimeDeviationCauseId, sourceCompanyId);
                if (targetTimeDeviationCause != null && !targetDeviationCauseIds.Any(x => x == targetTimeDeviationCause.TimeDeviationCauseId))
                    targetDeviationCauseIds.Add(targetTimeDeviationCause.TimeDeviationCauseId);

                DayType targetDayType = this.GetTargetDayType(sourceTimeRuleRow.DayTypeId, sourceCompanyId);
                if (targetDayType != null && !targetDayTypeIds.Any(x => x == targetDayType.DayTypeId))
                    targetDayTypeIds.Add(targetDayType.DayTypeId);

                //TimeScheduleType not implemented
            }

            foreach (var targetEmployeeGroup in employeeGroups)
	        {
		        if(!targetEmployeeGroupIds.Any(x => x == targetEmployeeGroup.EmployeeGroupId))
                    targetEmployeeGroupIds.Add(targetEmployeeGroup.EmployeeGroupId);
	        }
            
            TimeRuleManager.CreateTimeRuleRows(targetTimeRule, targetDeviationCauseIds, targetDayTypeIds, targetEmployeeGroupIds, new List<int>(), this.actorCompanyId);

            foreach (TimeRuleExpression sourceExpression in sourceTimeRule.TimeRuleExpression)
            {
                TimeRuleExpression targetExpression = this.CopyTimeRuleExpression(sourceExpression, sourceCompanyId);
                if (targetExpression == null)
                    continue;

                targetTimeRule.TimeRuleExpression.Add(targetExpression);
            }

            base.AddEntity(this.entities, targetTimeRule);
        }

        private void CopyTimeAbsenceRules()
        {
            foreach (EmployeeGroup employeeGroup in this.EmployeeGroups)
            {
                foreach (var selectedTimeAbsenceRule in selectedTimeAbsenceRules)
                {
                    this.CopyTimeAbsenceRule(selectedTimeAbsenceRule, employeeGroup);
                }
            }
        }

        private void CopyTimeAbsenceRule(SelectedItemDTO match, EmployeeGroup employeeGroup)
        {
            var input = new GetTimeAbsenceRulesInput(this.actorCompanyId, match.ItemId)
            {
                LoadEmployeeGroups = true,
                LoadRows = true,
                LoadRowProducts = true,
            };
            TimeAbsenceRuleHead sourceAbsenceRule = TimeRuleManager.GetTimeAbsenceRuleHead(this.entities, input);
            if (sourceAbsenceRule == null)
                return;

            TimeAbsenceRuleHead targetAbsenceRule = new TimeAbsenceRuleHead
            {
                Name = sourceAbsenceRule.Name,
                Description = sourceAbsenceRule.Description,
                Type = sourceAbsenceRule.Type,

                //Set FK
                ActorCompanyId = this.actorCompanyId,

                //Set referenecs
                TimeCode = this.GetTargetTimeCode(sourceAbsenceRule.TimeCodeId, match.CompanyId),
            };

            if (employeeGroup != null)
                TimeRuleManager.CreateTimeAbsenceRuleHeadEmployeeGroup(entities, targetAbsenceRule, employeeGroup.EmployeeGroupId);

            //Add references
            targetAbsenceRule.TimeAbsenceRuleRow.AddRange(sourceAbsenceRule.TimeAbsenceRuleRow.Select(x => this.CopyTimeAbsenceRuleRow(x, match.CompanyId)));

            base.AddEntity(this.entities, targetAbsenceRule);
        }

        private void CopyAttestRule(SelectedItemDTO match)
        {
            AttestRuleHead sourceAttestRule = AttestManager.GetAttestRuleHead(this.entities, match.ItemId);
            if (sourceAttestRule == null)
                return;

            AttestRuleHead targetAttestRule = new AttestRuleHead
            {
                Name = sourceAttestRule.Name,
                Description = sourceAttestRule.Description,
                Module = sourceAttestRule.Module,

                //Set FK
                ActorCompanyId = this.actorCompanyId,

                //Set references
                DayType = this.GetTargetDayType(sourceAttestRule.DayTypeId, match.CompanyId),
            };

            //Add references
            targetAttestRule.EmployeeGroup.AddRange(EmployeeGroups);
            targetAttestRule.AttestRuleRow.AddRange(sourceAttestRule.AttestRuleRow.Select(arr => this.CopyAttestRuleRow(arr)));

            base.AddEntity(this.entities, targetAttestRule);
        }

        private void CopyAttestRules()
        {
            foreach (var selectedAttestRule in this.selectedAttestRules)
            {
                this.CopyAttestRule(selectedAttestRule);
            }
        }

        private TimeRuleExpression CopyTimeRuleExpression(TimeRuleExpression sourceExpression, int sourceCompanyId)
        {
            if (sourceExpression == null)
                return null;

            TimeRuleExpression targetExpression = new TimeRuleExpression
            {
                IsStart = sourceExpression.IsStart,
            };

            foreach (TimeRuleOperand sourceOperand in sourceExpression.TimeRuleOperand)
            {
                TimeRuleOperand targetOperand = this.CopyTimeRuleOperand(sourceOperand, sourceCompanyId);
                targetExpression.TimeRuleOperand.Add(targetOperand);
            }

            return base.AddEntity(this.entities, targetExpression);
        }

        private TimeRuleOperand CopyTimeRuleOperand(TimeRuleOperand sourceOperand, int sourceCompanyId)
        {
            if (sourceOperand == null)
                return null;

            TimeRuleOperand targetOperand = new TimeRuleOperand
            {
                OperatorType = sourceOperand.OperatorType,
                LeftValueType = sourceOperand.LeftValueType,
                RightValueType = sourceOperand.RightValueType,
                Minutes = sourceOperand.Minutes,
                ComparisonOperator = sourceOperand.ComparisonOperator,
                OrderNbr = sourceOperand.OrderNbr,
            };

            if (sourceOperand.LeftValueId.HasValue)
            {
                if (sourceOperand.IsLeftValueTimeCode())
                    targetOperand.LeftValueId = this.GetTargetTimeCode(sourceOperand.LeftValueId.Value, sourceCompanyId).TimeCodeId;
                else
                    targetOperand.LeftValueId = sourceOperand.LeftValueId;
            }

            if (sourceOperand.RightValueId.HasValue)
            {
                if (sourceOperand.IsRightValueTimeCode())
                    targetOperand.RightValueId = this.GetTargetTimeCode(sourceOperand.RightValueId.Value, sourceCompanyId).TimeCodeId;
                else
                    targetOperand.RightValueId = sourceOperand.RightValueId;
            }

            if (!sourceOperand.TimeRuleExpressionRecursiveReference.IsLoaded)
                sourceOperand.TimeRuleExpressionRecursiveReference.Load();

            if (sourceOperand.TimeRuleExpressionRecursive != null)
            {
                TimeRuleExpression targetExpressionRecursive = new TimeRuleExpression
                {
                    IsStart = sourceOperand.TimeRuleExpressionRecursive.IsStart,
                };
                targetOperand.TimeRuleExpressionRecursive = targetExpressionRecursive;
            }

            return base.AddEntity(this.entities, targetOperand);
        }

        private TimeAbsenceRuleRow CopyTimeAbsenceRuleRow(TimeAbsenceRuleRow sourceAbsenceRuleRow, int sourceCompanyId)
        {
            if (sourceAbsenceRuleRow == null)
                return null;

            TimeAbsenceRuleRow targetAbsenceRuleRow = new TimeAbsenceRuleRow
            {
                Type = sourceAbsenceRuleRow.Type,
                Start = sourceAbsenceRuleRow.Start,
                Stop = sourceAbsenceRuleRow.Stop,
                HasMultiplePayrollProducts = sourceAbsenceRuleRow.HasMultiplePayrollProducts,

                //Set references
                PayrollProduct = this.GetTargetPayrollProduct(sourceAbsenceRuleRow.PayrollProductId, sourceCompanyId),
            };

            foreach (var payrollProduct in sourceAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts)
            {
                targetAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.Add(new TimeAbsenceRuleRowPayrollProducts
                {
                    SourcePayrollProduct = this.GetTargetPayrollProduct(payrollProduct.SourcePayrollProductId, sourceCompanyId),
                    TargetPayrollProduct = this.GetTargetPayrollProduct(payrollProduct.TargetPayrollProductId, sourceCompanyId),

                    //Set references
                    TimeAbsenceRuleRow = targetAbsenceRuleRow,
                });
            }

            return base.AddEntity(this.entities, targetAbsenceRuleRow);
        }

        private AttestRuleRow CopyAttestRuleRow(AttestRuleRow sourceAttestRuleRow)
        {
            if (sourceAttestRuleRow == null)
                return null;

            AttestRuleRow targetAttestRuleRow = new AttestRuleRow
            {
                ComparisonOperator = sourceAttestRuleRow.ComparisonOperator,
                LeftValueId = sourceAttestRuleRow.LeftValueId,
                LeftValueType = sourceAttestRuleRow.LeftValueType,
                Minutes = sourceAttestRuleRow.Minutes,
                RightValueId = sourceAttestRuleRow.RightValueId,
                RightValueType = sourceAttestRuleRow.RightValueType,
            };

            return base.AddEntity(this.entities, targetAttestRuleRow);
        }

        #endregion

        #region Get entities

        private IEnumerable<EmployeeGroup> GetEmployeeGroups()
        {
            return this.selectedEmployeeGroupIds.Select(eg => EmployeeManager.GetEmployeeGroup(this.entities, eg));
        }

        private TimeCode GetTargetTimeCode(TimeCode timeCode, int sourceCompanyId)
        {
            return timeCode == null ? null : this.TimeCodes[new MatchedItemDTO { CompanyId = sourceCompanyId, SourceItemId = timeCode.TimeCodeId }];
        }

        private TimeCode GetTargetTimeCode(int timeCodeId, int sourceCompanyId)
        {
            return this.TimeCodes[new MatchedItemDTO { CompanyId = sourceCompanyId, SourceItemId = timeCodeId }];
        }

        private DayType GetTargetDayType(int? dayTypeId, int sourceCompanyId)
        {
            if (dayTypeId.HasValue)
            {
                return this.GetTargetDayType(dayTypeId.Value, sourceCompanyId);
            }
            return null;
        }

        private DayType GetTargetDayType(int dayTypeId, int sourceCompanyId)
        {
            return this.DayTypes[new MatchedItemDTO { CompanyId = sourceCompanyId, SourceItemId = dayTypeId }];
        }

        private TimeDeviationCause GetTargetTimeDeviationCause(int timeDeviationCauseId, int sourceCompanyId)
        {
            return this.TimeDeviationCauses[new MatchedItemDTO { CompanyId = sourceCompanyId, SourceItemId = timeDeviationCauseId }];
        }

        private PayrollProduct GetTargetPayrollProduct(int? productId, int sourceCompanyId)
        {
            if (productId.HasValue)
            {
                return this.PayrollProducts[new MatchedItemDTO { CompanyId = sourceCompanyId, SourceItemId = productId.Value }];
            }
            return null;
        }

        #endregion
    }
}
