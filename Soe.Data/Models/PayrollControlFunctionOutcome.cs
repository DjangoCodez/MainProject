using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Status.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class PayrollControlFunctionOutcome : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public string StatusName { get; set; }

        public PayrollControlFunctionOutcomeValues Values 
        {
            get => PayrollControlFunctionOutcomeValues.FromJson(this);            
        }

        public string Key => this.Values.Key();
        public string UserFriendlyKey => this.Values.UserFriendlyKey();

        public static PayrollControlFunctionOutcome Create(int actorCompanyId, int employeeId, int employeeTimePeriodId, TermGroup_PayrollControlFunctionType type, decimal? value1 = null, decimal? value2 = null)
        {
            return new PayrollControlFunctionOutcome
            {
                ActorCompanyId = actorCompanyId,
                EmployeeId = employeeId,
                EmployeeTimePeriodId = employeeTimePeriodId,
                Comment = "",
                Type = (int)type,
                Status = (int)TermGroup_PayrollControlFunctionStatus.Opened,
                Value = PayrollControlFunctionOutcomeValues.ToJson(PayrollControlFunctionOutcomeValues.Create(type, value1, value2)),
            };
        }

        public static PayrollControlFunctionOutcome CreatePayrollPeriodHasNotBeenCalculated(int employeeId)
        {
            return PayrollControlFunctionOutcome.Create(0, employeeId, 0, TermGroup_PayrollControlFunctionType.PeriodHasNotBeenCalculated);            
        }

        public static PayrollControlFunctionOutcome CreatePayrollPeriodHasNetSalaryDiff(int employeeId, PayrollCalculationPeriodSumDTO periodSumDTO)
        {
            if (periodSumDTO.HasNetSalaryDiff)
                return PayrollControlFunctionOutcome.Create(0, employeeId, 0, TermGroup_PayrollControlFunctionType.NetSalaryDiff, periodSumDTO.Net, periodSumDTO.TransactionNet);
            else
                return null;
        }

        public bool IsStoppingPayrollWarning()
        {
            return PayrollWarningsUtil.GetStoppingPayrollWarnings().Contains((TermGroup_PayrollControlFunctionType)this.Type);
        }
    }

    public partial class PayrollControlFunctionOutcomeChange : ICreated
    {
        public string TypeName { get; set; }
        public string FieldTypeName { get; set; }
        public string FromValueName { get; set; }
        public string ToValueName { get; set; }

        public static PayrollControlFunctionOutcomeChange Create(int employeeId, int employeeTimePeriodId, TermGroup_PayrollControlFunctionOutcomeChangeType type, TermGroup_PayrollControlFunctionOutcomeChangeFieldType fieldType, string fromValue, string toValue)
        {
            return new PayrollControlFunctionOutcomeChange
            {
                EmployeeId = employeeId,
                EmployeeTimePeriodId = employeeTimePeriodId,                
                Type = (int)type,
                FieldType = (int)fieldType,
                FromValue = fromValue,
                ToValue = toValue,
            };
        }
    }

    public class RecalculatePayrollControlFunctions
    {
        public int ActorCompanyId { get; private set; }
        public Employee Employee { get; private set; }
        public EmployeeTimePeriod EmployeeTimePeriod { get; private set; }                
        public TimePeriod TimePeriod { get; private set; }

        private readonly List<PayrollControlFunction> controlFunctions;

        public bool AddControlFunction(TermGroup_PayrollControlFunctionType type)
        {
            if (controlFunctions.Any(x => x.Type == type))
                return false;

            controlFunctions.Add(new PayrollControlFunction(type, ActorCompanyId, Employee.EmployeeId, EmployeeTimePeriod.EmployeeTimePeriodId));

            return true;

        }
        public RecalculatePayrollControlFunctions(int actorCompanyId, Employee employee, EmployeeTimePeriod employeeTimePeriod, TimePeriod timePeriod)
        {
            ActorCompanyId = actorCompanyId;
            Employee = employee;
            EmployeeTimePeriod = employeeTimePeriod;
            TimePeriod = timePeriod;            
            controlFunctions = new List<PayrollControlFunction>();

            foreach (var type in (TermGroup_PayrollControlFunctionType[])Enum.GetValues(typeof(TermGroup_PayrollControlFunctionType)))
            {
                if (type == TermGroup_PayrollControlFunctionType.PeriodHasChanged)
                    continue;

                AddControlFunction(type);
            }   
        }
        
        public static List<PayrollControlFunctionOutcome> GetInMemoryWarnings(int employeeId, int employeeTimePeriodId, PayrollCalculationPeriodSumDTO periodSumDTO)
        {
            List<PayrollControlFunctionOutcome> warnings = new List<PayrollControlFunctionOutcome>();

            if (employeeTimePeriodId == 0)
            {
                warnings.Add(PayrollControlFunctionOutcome.CreatePayrollPeriodHasNotBeenCalculated(employeeId));
            }
            else if(periodSumDTO != null)
            {
                var warning = PayrollControlFunctionOutcome.CreatePayrollPeriodHasNetSalaryDiff(employeeId, periodSumDTO);
                if(warning != null)
                    warnings.Add(warning);
            }

            return warnings;
        }


        public List<PayrollControlFunction> Run(PayrollCalculationPeriodSumDTO periodSumDTO, EmployeeVacationPeriodDTO vacationPeriodDTO, ActionResult validateEmployeeAccountsResult)
        {
            foreach (var controlFunction in this.controlFunctions)
            {
                var previousOutcome = this.EmployeeTimePeriod.PayrollControlFunctionOutcome.FirstOrDefault(x => x.Type == (int)controlFunction.Type);
                if (previousOutcome != null)
                    controlFunction.SetPreviousOutcome(previousOutcome);

                switch (controlFunction.Type)
                {
                    #region Period sums
                 
                    case TermGroup_PayrollControlFunctionType.TaxMissing:
                        if (periodSumDTO != null && periodSumDTO.IsTaxMissing)
                            controlFunction.ActivateWarning(periodSumDTO.Tax);
                        break;
                    case TermGroup_PayrollControlFunctionType.EmploymentTaxMissing:
                        if (periodSumDTO != null && periodSumDTO.IsEmploymentTaxMissing)
                            controlFunction.ActivateWarning(0);
                        break;
                    
                    case TermGroup_PayrollControlFunctionType.EmploymentTaxDiff:
                        if (periodSumDTO != null && periodSumDTO.HasEmploymentTaxDiff)
                            controlFunction.ActivateWarning(periodSumDTO.EmploymentTaxCredit, periodSumDTO.EmploymentTaxDebit);
                        break;
                    case TermGroup_PayrollControlFunctionType.SupplementChargeDiff:
                        if (periodSumDTO != null && periodSumDTO.HasSupplementChargeDiff)
                            controlFunction.ActivateWarning(periodSumDTO.SupplementChargeCredit, periodSumDTO.SupplementChargeDebit);
                        break;
                    case TermGroup_PayrollControlFunctionType.GrossSalaryNegative:
                        if (periodSumDTO != null && periodSumDTO.IsGrossSalaryNegative)
                            controlFunction.ActivateWarning(periodSumDTO.Gross);
                        break;
                    case TermGroup_PayrollControlFunctionType.NetSalaryMissing:
                        if (periodSumDTO != null && periodSumDTO.IsNetSalaryMissing)
                            controlFunction.ActivateWarning(periodSumDTO.Net);
                        break;
                    case TermGroup_PayrollControlFunctionType.NetSalaryNegative:
                        if (periodSumDTO != null && periodSumDTO.IsNetSalaryNegative)
                            controlFunction.ActivateWarning(periodSumDTO.Net);
                        break;
                    case TermGroup_PayrollControlFunctionType.NetSalaryDiff:
                        if (periodSumDTO != null && periodSumDTO.HasNetSalaryDiff)
                            controlFunction.ActivateWarning(periodSumDTO.Net, periodSumDTO.TransactionNet);
                        break;
                    case TermGroup_PayrollControlFunctionType.BenefitNegative:
                        if (periodSumDTO != null && periodSumDTO.IsBenefitNegative)
                            controlFunction.ActivateWarning(periodSumDTO.BenefitInvertExcluded);
                        break;

                    #endregion

                    #region Vacation

                    case TermGroup_PayrollControlFunctionType.NegativeVacationDays:                        
                        if (vacationPeriodDTO != null && vacationPeriodDTO.HasNegativeVacationDays)
                            controlFunction.ActivateWarning();
                        break;

                    #endregion

                    #region Employee

                    case TermGroup_PayrollControlFunctionType.VacationGroupMissing:
                        if (!this.TimePeriod.IsExtraPeriod())
                        {
                            int vacationGroupId = this.Employee?.GetEmployment(this.TimePeriod.StartDate, this.TimePeriod.StopDate, false)?.GetVacationGroup(this.TimePeriod.StartDate, this.TimePeriod.StopDate, forward: false)?.VacationGroupId ?? 0;
                            if (vacationGroupId == 0)
                                controlFunction.ActivateWarning();
                        }
                        break;
                    case TermGroup_PayrollControlFunctionType.NewEmployeeInPeriod:
                        if (this.Employee != null  && !this.TimePeriod.IsExtraPeriod() && this.Employee.IsEmployeedInPeriod(this.TimePeriod))
                            controlFunction.ActivateWarning();
                        break;
                    case TermGroup_PayrollControlFunctionType.EndDateInPeriodFinalSalaryNotChosen:
                        if (this.Employee != null && !this.TimePeriod.IsExtraPeriod() && this.Employee.HasEndDateInPeriodAndFinalsalaryNotChosen(this.TimePeriod))
                            controlFunction.ActivateWarning();
                        break;
                    case TermGroup_PayrollControlFunctionType.NewAgreementInPeriod:
                        if (this.Employee != null && !this.TimePeriod.IsExtraPeriod() && this.Employee.HasNewAgreementInPeriod(this.TimePeriod))
                            controlFunction.ActivateWarning();
                        break;
                    case TermGroup_PayrollControlFunctionType.DisbursementMethodIsUnknown:
                        if (this.Employee != null && this.Employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.Unknown)
                            controlFunction.ActivateWarning();
                        break;
                    case TermGroup_PayrollControlFunctionType.DisbursementMethodIsCash:
                        if (this.Employee != null && this.Employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_CashDeposit)
                            controlFunction.ActivateWarning();
                        break;
                    case TermGroup_PayrollControlFunctionType.DisbursementAccountNrIsMissing:
                        if ((this.Employee != null && this.Employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_AccountDeposit || this.Employee.DisbursementMethod == (int)TermGroup_EmployeeDisbursementMethod.SE_PersonAccount) && string.IsNullOrEmpty(this.Employee.DisbursementAccountNr))
                            controlFunction.ActivateWarning();
                        break;
                    case TermGroup_PayrollControlFunctionType.OverlappingMainAffiliation:         
                        if(!validateEmployeeAccountsResult.Success && validateEmployeeAccountsResult.ErrorNumber == (int)ActionResultSave.EmployeeOverlappingMainAffiliation)
                            controlFunction.ActivateWarning();
                        break;
                    case TermGroup_PayrollControlFunctionType.EmployeePositionMissing:
                        if(!Employee.EmployeePosition.Any())
                            controlFunction.ActivateWarning();
                        break;


                    #endregion

                    default:
                        break;

                }

                controlFunction.Update(TermGroup_PayrollControlFunctionOutcomeChangeType.Automatic);
            }

            return this.controlFunctions;
        }
    }


    public class PayrollPeriodHasChangedWarning
    {
        private List<PayrollControlFunction> ControlFunctions;
        private int ActorCompanyId;

        public PayrollPeriodHasChangedWarning(int actorCompanyId, List<EmployeeTimePeriod> periods)
        {
            Init(actorCompanyId, periods);
        }
        public PayrollPeriodHasChangedWarning(int actorCompanyId, EmployeeTimePeriod period)
        {
            Init(actorCompanyId, new List<EmployeeTimePeriod> { period });
        }

        private void Init(int actorCompanyId, List<EmployeeTimePeriod> periods)
        {
            this.ActorCompanyId = actorCompanyId;
            this.ControlFunctions = new List<PayrollControlFunction>();
            foreach (var period in periods)
            {
                var controlFunction = new PayrollControlFunction(TermGroup_PayrollControlFunctionType.PeriodHasChanged, ActorCompanyId, period.EmployeeId, period.EmployeeTimePeriodId);
                controlFunction.SetPreviousOutcome(period.GetControlFunctionOutcome(controlFunction.Type));

                this.ControlFunctions.Add(controlFunction);
            }
        }

        public List<PayrollControlFunction> Activate()
        {
            foreach (var controlFunction in ControlFunctions)
            {
                controlFunction.ActivateWarning();
                controlFunction.Update(TermGroup_PayrollControlFunctionOutcomeChangeType.Automatic);
            }
            return ControlFunctions;
        }
        public List<PayrollControlFunction> DeActivate()
        {
            foreach (var controlFunction in ControlFunctions)
            {                
                controlFunction.DeActivateWarning();
                controlFunction.Update(TermGroup_PayrollControlFunctionOutcomeChangeType.Automatic);
            }
            return ControlFunctions;
        }
    }

    public class PayrollControlFunction
    {
        public TermGroup_PayrollControlFunctionType Type { get; private set; }
        public int ActorCompanyId { get; private set; }
        public int EmployeeId { get; private set; }
        public int EmployeeTimePeriodId { get; private set; }
        public List<PayrollControlFunctionOutcomeChange> Changes { get; private set; }

        /// <summary>
        /// The last ran or updated outcome of the control function, i.e the entity in the database
        /// </summary>
        private PayrollControlFunctionOutcome PreviousOutcome;

        /// <summary>
        /// The in memory outcome of the control function
        /// </summary>
        private PayrollControlFunctionOutcome CurrentOutcome;

        public PayrollControlFunctionOutcome WarningOutcome { get { return PreviousOutcome; } }
        

        public PayrollControlFunction(TermGroup_PayrollControlFunctionType type, int actorCompanyId, int employeeId, int employeeTimePeriodId)
        {
            Type = type;
            ActorCompanyId = actorCompanyId;
            EmployeeId = employeeId;
            EmployeeTimePeriodId = employeeTimePeriodId;
        }

        public bool SetPreviousOutcome(PayrollControlFunctionOutcome previousOutcome)
        {
            if (previousOutcome != null && this.Type != (TermGroup_PayrollControlFunctionType)previousOutcome.Type)
                return false;

            PreviousOutcome = previousOutcome;

            return true;
        }

        public bool ActivateWarning(decimal? value1 = null, decimal? value2 = null)
        {
            return SetCurrentOutcome(PayrollControlFunctionOutcome.Create(ActorCompanyId, EmployeeId, EmployeeTimePeriodId, Type, value1, value2));
        }

        public bool DeActivateWarning()
        {
            return SetCurrentOutcome(null);
        }

        private bool SetCurrentOutcome(PayrollControlFunctionOutcome currentOutcome)
        {
            if (currentOutcome != null && this.Type != (TermGroup_PayrollControlFunctionType)currentOutcome.Type)
                return false;

            CurrentOutcome = currentOutcome;
            
            return true;
        }


        public void Update(TermGroup_PayrollControlFunctionOutcomeChangeType changeType)
        {
            Changes = new List<PayrollControlFunctionOutcomeChange>();

            if (PreviousOutcome == null && CurrentOutcome == null)
                return;

            if (changeType == TermGroup_PayrollControlFunctionOutcomeChangeType.Manual && (PreviousOutcome == null || CurrentOutcome == null))
                return;
            
            if (changeType == TermGroup_PayrollControlFunctionOutcomeChangeType.Manual)
            {
                if (PreviousOutcome != null && CurrentOutcome != null)
                {
                    if (PreviousOutcome.Status != CurrentOutcome.Status)
                    {
                        Changes.Add(PayrollControlFunctionOutcomeChange.Create(
                            EmployeeId, 
                            EmployeeTimePeriodId, 
                            changeType, 
                            TermGroup_PayrollControlFunctionOutcomeChangeFieldType.Status, 
                            PreviousOutcome.Status.ToString(),
                            CurrentOutcome.Status.ToString()));

                        PreviousOutcome.Status = CurrentOutcome.Status;
                    }

                    if (PreviousOutcome.Comment != CurrentOutcome.Comment)
                    {                        
                        Changes.Add(PayrollControlFunctionOutcomeChange.Create(
                            EmployeeId, 
                            EmployeeTimePeriodId, 
                            changeType, 
                            TermGroup_PayrollControlFunctionOutcomeChangeFieldType.Comment, 
                            PreviousOutcome.Comment, 
                            CurrentOutcome.Comment));

                        PreviousOutcome.Comment = CurrentOutcome.Comment;
                    }
                }
            }
            else  if (changeType == TermGroup_PayrollControlFunctionOutcomeChangeType.Automatic)
            {
                if (PreviousOutcome == null && CurrentOutcome != null)
                {
                    //First time the control function has an outcome
                    PreviousOutcome = CurrentOutcome;
                }
                else if (PreviousOutcome != null && CurrentOutcome == null)
                {
                    if (PreviousOutcome.State == (int)SoeEntityState.Active)
                    {
                        //The control function has not an outcome anymore/warning                     
                        Changes.Add(PayrollControlFunctionOutcomeChange.Create(
                            EmployeeId, 
                            EmployeeTimePeriodId, 
                            changeType, 
                            TermGroup_PayrollControlFunctionOutcomeChangeFieldType.State, 
                            ((int)SoeEntityState.Active).ToString(), 
                            ((int)SoeEntityState.Deleted).ToString()));

                        PreviousOutcome.State = (int)SoeEntityState.Deleted;
                    }
                }
                else if (PreviousOutcome != null && CurrentOutcome != null)
                {
                    if (PreviousOutcome.State == (int)SoeEntityState.Deleted && CurrentOutcome.State == (int)SoeEntityState.Active)
                    {
                        //A warning went from not active to active

                        //Set the warning to active
                        Changes.Add(PayrollControlFunctionOutcomeChange.Create(
                            EmployeeId, 
                            EmployeeTimePeriodId, 
                            changeType, 
                            TermGroup_PayrollControlFunctionOutcomeChangeFieldType.State, 
                            PreviousOutcome.State.ToString(), 
                            CurrentOutcome.State.ToString()));

                        PreviousOutcome.State = CurrentOutcome.State;

                        //Set the status to the new status (Opened)
                        if (PreviousOutcome.Status != CurrentOutcome.Status)
                        {
                            Changes.Add(PayrollControlFunctionOutcomeChange.Create
                                        (EmployeeId,
                                        EmployeeTimePeriodId,
                                        changeType,
                                        TermGroup_PayrollControlFunctionOutcomeChangeFieldType.Status,
                                        PreviousOutcome.Status.ToString(),
                                        CurrentOutcome.Status.ToString()));

                            PreviousOutcome.Status = CurrentOutcome.Status;
                        }
                    }
                    else if (PreviousOutcome.Status != CurrentOutcome.Status)
                    {
                        if (PreviousOutcome.Status == (int)TermGroup_PayrollControlFunctionStatus.Attention)
                            return;

                        if (PreviousOutcome.Status == (int)TermGroup_PayrollControlFunctionStatus.HideforPeriod && CurrentOutcome.Status == (int)TermGroup_PayrollControlFunctionStatus.Opened)
                        {
                            //Set the previous outcome to opended only if key has changed
                            if (PreviousOutcome.Key != CurrentOutcome.Key)
                            {                         
                                Changes.Add(PayrollControlFunctionOutcomeChange.Create
                                    (EmployeeId, 
                                    EmployeeTimePeriodId, 
                                    changeType, 
                                    TermGroup_PayrollControlFunctionOutcomeChangeFieldType.Status, 
                                    PreviousOutcome.Status.ToString(), 
                                    CurrentOutcome.Status.ToString()));

                                PreviousOutcome.Status = CurrentOutcome.Status;
                            }
                        }
                        else 
                        {                            
                            Changes.Add(PayrollControlFunctionOutcomeChange.Create(
                                EmployeeId, 
                                EmployeeTimePeriodId, 
                                changeType, 
                                TermGroup_PayrollControlFunctionOutcomeChangeFieldType.Status, 
                                PreviousOutcome.Status.ToString(), 
                                CurrentOutcome.Status.ToString()));

                            PreviousOutcome.Status = CurrentOutcome.Status;
                        }
                    }

                    //Always check the values
                    if(PreviousOutcome.Key != CurrentOutcome.Key)
                    {
                        Changes.Add(PayrollControlFunctionOutcomeChange.Create(
                              EmployeeId,
                              EmployeeTimePeriodId,
                              changeType,
                              TermGroup_PayrollControlFunctionOutcomeChangeFieldType.Value,
                              PreviousOutcome.UserFriendlyKey,
                              CurrentOutcome.UserFriendlyKey));

                        PreviousOutcome.Value = CurrentOutcome.Value;
                    }                    
                }             
            }            
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PayrollControlFunctionOutcomeValues
    {
        [JsonProperty("decValue1")]
        public decimal? DecimalValue1  { get; private set; }
        [JsonProperty("decValue2")]
        public decimal? DecimalValue2 { get; private set; }


        public TermGroup_PayrollControlFunctionType Type { get; private set; }

        public PayrollControlFunctionOutcomeValues()
        {
        }

        public static PayrollControlFunctionOutcomeValues Create(TermGroup_PayrollControlFunctionType type, decimal? decimalValue1 = null, decimal? decimalValue2 = null)
        {
            return new PayrollControlFunctionOutcomeValues
            {
                Type = type,
                DecimalValue1 = decimalValue1,
                DecimalValue2 = decimalValue2
            };
        }

        public static PayrollControlFunctionOutcomeValues FromJson(PayrollControlFunctionOutcome outcome)
        {
            var values =  JsonConvert.DeserializeObject<PayrollControlFunctionOutcomeValues>(outcome.Value);
            values.Type = (TermGroup_PayrollControlFunctionType)outcome.Type;
            return values;
        }
        public static string ToJson(PayrollControlFunctionOutcomeValues values)
        {
            return JsonConvert.SerializeObject(values, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented,
            });
        }

        public string UserFriendlyKey()
        {
            //TODO: In the future we may want return something like ""Net salary is neagtive : -1000" instead of just "-1000"
            string key = "";
            switch (this.Type)
            {

                #region No value

                case TermGroup_PayrollControlFunctionType.NegativeVacationDays:
                case TermGroup_PayrollControlFunctionType.VacationGroupMissing:
                case TermGroup_PayrollControlFunctionType.PeriodHasNotBeenCalculated:
                case TermGroup_PayrollControlFunctionType.DisbursementMethodIsUnknown:
                case TermGroup_PayrollControlFunctionType.DisbursementMethodIsCash:
                case TermGroup_PayrollControlFunctionType.DisbursementAccountNrIsMissing:
                case TermGroup_PayrollControlFunctionType.NewEmployeeInPeriod:
                case TermGroup_PayrollControlFunctionType.EndDateInPeriodFinalSalaryNotChosen:
                case TermGroup_PayrollControlFunctionType.NewAgreementInPeriod:
                case TermGroup_PayrollControlFunctionType.PeriodHasChanged:
                case TermGroup_PayrollControlFunctionType.OverlappingMainAffiliation:
                case TermGroup_PayrollControlFunctionType.EmployeePositionMissing:
                    key = "";
                    break;

                #endregion

                #region DecimalValue1

                case TermGroup_PayrollControlFunctionType.TaxMissing:
                case TermGroup_PayrollControlFunctionType.EmploymentTaxMissing:
                case TermGroup_PayrollControlFunctionType.GrossSalaryNegative:
                case TermGroup_PayrollControlFunctionType.NetSalaryMissing:
                case TermGroup_PayrollControlFunctionType.NetSalaryNegative:
                case TermGroup_PayrollControlFunctionType.BenefitNegative:
                    key = $"{DecimalValue1}";                    
                    break;

                #endregion

                #region DecimalValue1 and DecimalValue2

                case TermGroup_PayrollControlFunctionType.EmploymentTaxDiff:                  
                case TermGroup_PayrollControlFunctionType.SupplementChargeDiff:                    
                case TermGroup_PayrollControlFunctionType.NetSalaryDiff:
                    key = $"{DecimalValue1}, {DecimalValue2}";
                    break;

                #endregion


                default:
                    break;
            }

            return key;
        }
        /// <summary>
        /// Is used to compare the values of the control function outcomes, too see if the actual values has changed from one calculation to an other
        /// </summary>
        /// <returns></returns>
        public string Key()
        {
            string key = "";
            switch (this.Type)
            {

                #region No value

                case TermGroup_PayrollControlFunctionType.NegativeVacationDays:
                case TermGroup_PayrollControlFunctionType.VacationGroupMissing:
                case TermGroup_PayrollControlFunctionType.PeriodHasNotBeenCalculated:
                case TermGroup_PayrollControlFunctionType.DisbursementMethodIsUnknown:
                case TermGroup_PayrollControlFunctionType.DisbursementMethodIsCash:
                case TermGroup_PayrollControlFunctionType.DisbursementAccountNrIsMissing:
                case TermGroup_PayrollControlFunctionType.NewEmployeeInPeriod:
                case TermGroup_PayrollControlFunctionType.EndDateInPeriodFinalSalaryNotChosen:
                case TermGroup_PayrollControlFunctionType.NewAgreementInPeriod:
                case TermGroup_PayrollControlFunctionType.PeriodHasChanged:
                case TermGroup_PayrollControlFunctionType.OverlappingMainAffiliation:
                case TermGroup_PayrollControlFunctionType.EmployeePositionMissing:

                    key = "";
                    break;

                #endregion

                #region DecimalValue1

                case TermGroup_PayrollControlFunctionType.TaxMissing:                   
                case TermGroup_PayrollControlFunctionType.EmploymentTaxMissing:                    
                case TermGroup_PayrollControlFunctionType.GrossSalaryNegative:                    
                case TermGroup_PayrollControlFunctionType.NetSalaryMissing:                                        
                case TermGroup_PayrollControlFunctionType.NetSalaryNegative:
                case TermGroup_PayrollControlFunctionType.BenefitNegative:
                    key = $"{Type.ToString()}_{DecimalValue1}";
                    break;

                #endregion

                #region DecimalValue1 and DecimalValue2
                
                case TermGroup_PayrollControlFunctionType.EmploymentTaxDiff:                                        
                case TermGroup_PayrollControlFunctionType.SupplementChargeDiff:                    
                case TermGroup_PayrollControlFunctionType.NetSalaryDiff:
                    key = $"{Type.ToString()}_{DecimalValue1}_{DecimalValue2}";
                    break;

                #endregion


                default:
                    break;
            }

            return key;
        }        
    }    


    public static partial class EntityExtensions
    {
        #region PayrollControlFunctionOutcome

        public static PayrollControlFunctionOutcomeDTO ToDTO(this PayrollControlFunctionOutcome e, bool loadChanges = false)
        {
            if (e == null)
                return null;

            var dto = new PayrollControlFunctionOutcomeDTO()
            {
                PayrollControlFunctionOutcomeId = e.PayrollControlFunctionOutcomeId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                EmployeeTimePeriodId = e.EmployeeTimePeriodId,
                Type = (TermGroup_PayrollControlFunctionType)e.Type,
                TypeName = e.TypeName,
                Value = e.Value,
                IsStoppingPayrollWarning = PayrollWarningsUtil.GetStoppingPayrollWarnings().Contains((TermGroup_PayrollControlFunctionType)(object)e.Type),
                Status = (TermGroup_PayrollControlFunctionStatus)e.Status,
                StatusName = e.StatusName,
                Comment = e.Comment,
                hasChanges = e.PayrollControlFunctionOutcomeChange.Any(),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State

            };
            
                            
            //Set Json properties
            dto.DecimalValue1 = e.Values?.DecimalValue1;
            dto.DecimalValue2 = e.Values?.DecimalValue2;
                        
            if (e.PayrollControlFunctionOutcomeChange != null && loadChanges)
                dto.Changes = e.PayrollControlFunctionOutcomeChange.ToDTOs().ToList() ?? new List<PayrollControlFunctionOutcomeChangeDTO>();

            return dto;
        }

        public static IEnumerable<PayrollControlFunctionOutcomeDTO> ToDTOs(this IEnumerable<PayrollControlFunctionOutcome> l, bool loadChanges = false)
        {
            var dtos = new List<PayrollControlFunctionOutcomeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(loadChanges));
                }
            }
            return dtos;
        }

        #endregion

        #region PayrollControlFunctionOutcomeChange

        public static PayrollControlFunctionOutcomeChangeDTO ToDTO(this PayrollControlFunctionOutcomeChange e)
        {
            if (e == null)
                return null;

            return new PayrollControlFunctionOutcomeChangeDTO()
            {
                PayrollControlFunctionOutcomeChangeId = e.PayrollControlFunctionOutcomeChangeId,
                PayrollControlFunctionOutcomeId = e.PayrollControlFunctionOutcomeId,
                EmployeeId = e.EmployeeId,
                EmployeeTimePeriodId = e.EmployeeTimePeriodId,
                Type = (TermGroup_PayrollControlFunctionOutcomeChangeType)e.Type,
                TypeName = e.TypeName,
                FieldType = (TermGroup_PayrollControlFunctionOutcomeChangeFieldType)e.FieldType,
                FieldTypeName = e.FieldTypeName,
                FromValue = e.FromValue,
                ToValue = e.ToValue,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
            };

        }

        public static IEnumerable<PayrollControlFunctionOutcomeChangeDTO> ToDTOs(this IEnumerable<PayrollControlFunctionOutcomeChange> l)
        {
            var dtos = new List<PayrollControlFunctionOutcomeChangeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion
    }     
}

