import {
  SoeEntityState,
  SoeTimeCodeType,
  TermGroup_AdjustQuantityByBreakTime,
  TermGroup_ExpenseType,
  TermGroup_TimeCodeClassification,
  TermGroup_TimeCodeRegistrationType,
  TermGroup_TimeCodeRoundingType,
  TermGroup_TimeCodeRuleType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ITimeCodeBreakTimeCodeDeviationCauseDTO,
  ITimeCodeInvoiceProductDTO,
  ITimeCodePayrollProductDTO,
  ITimeCodeRuleDTO,
  ITimeCodeSaveDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
export class TimeCodeMaterialDTO implements ITimeCodeSaveDTO {
  timeCodeId!: number;
  type: SoeTimeCodeType = SoeTimeCodeType.Material;
  registrationType!: TermGroup_TimeCodeRegistrationType;
  code!: string;
  name!: string;
  description!: string;
  roundingType!: TermGroup_TimeCodeRoundingType;
  roundingValue!: number;
  roundStartTime!: boolean;
  roundingTimeCodeId?: number;
  roundingGroupKey!: string;
  minutesByConstantRules!: number;
  factorBasedOnWorkPercentage!: boolean;
  payed!: boolean;
  adjustQuantityByBreakTime!: TermGroup_AdjustQuantityByBreakTime;
  adjustQuantityTimeCodeId?: number;
  adjustQuantityTimeScheduleTypeId?: number;
  timeCodeRuleType!: TermGroup_TimeCodeRuleType;
  timeCodeRuleTime?: Date;
  timeCodeRuleValue?: number;
  state: SoeEntityState = 0;
  invoiceProducts!: ITimeCodeInvoiceProductDTO[];
  payrollProducts!: ITimeCodePayrollProductDTO[];
  kontekId?: number;
  isAbsence!: boolean;
  expenseType!: TermGroup_ExpenseType;
  comment!: string;
  stopAtDateStart!: boolean;
  stopAtDateStop!: boolean;
  stopAtPrice!: boolean;
  stopAtVat!: boolean;
  stopAtAccounting!: boolean;
  stopAtComment!: boolean;
  commentMandatory!: boolean;
  minMinutes!: number;
  maxMinutes!: number;
  defaultMinutes!: number;
  startType!: number;
  stopType!: number;
  startTime?: Date;
  startTimeMinutes!: number;
  stopTimeMinutes!: number;
  template!: boolean;
  timeCodeBreakGroupId?: number;
  timeCodeRules!: ITimeCodeRuleDTO[];
  timeCodeDeviationCauses!: ITimeCodeBreakTimeCodeDeviationCauseDTO[];
  employeeGroupIds!: number[];
  note!: string;
  isWorkOutsideSchedule!: boolean;
  hideForEmployee!: boolean;
  showInTerminal!: boolean;
  fixedQuantity!: number;
  classification!: TermGroup_TimeCodeClassification;
}

export class TimeCodeInvoiceProductDTO implements ITimeCodeInvoiceProductDTO {
  timeCodeInvoiceProductId!: number;
  timeCodeId!: number;
  invoiceProductId!: number;
  factor!: number;
  invoiceProductPrice!: number;
}
