import { signal } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  IAttestPayrollTransactionDTO,
  IAttestTransitionLogDTO,
} from '@shared/models/generated-interfaces/AttestDTO';
import { IGetProjectTimeBlocksForTimesheetModel } from '@shared/models/generated-interfaces/CoreModels';
import {
  SoeTimePayrollScheduleTransactionType,
  SoeTimeCodeType,
  TermGroup_TimeCodeRegistrationType,
  OrderInvoiceRegistrationType,
  TermGroup_ProjectAllocationType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IEmployeeProjectInvoiceDTO,
  IProjectInvoiceSmallDTO,
  IProjectSmallDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';
import {
  IAccountDimDTO,
  IAccountDTO,
  IAccountingSettingsRowDTO,
  IEmployeeScheduleTransactionInfoDTO,
  IEmployeeSmallDTO,
  IProjectTimeBlockDTO,
  IProjectTimeBlockSaveDTO,
  IProjectTimeMatrixDTO,
  IProjectTimeMatrixSaveRowDTO,
  ITimeDeviationCauseDTO,
  IValidateProjectTimeBlockSaveDTO,
  IValidateProjectTimeBlockSaveRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { Constants } from '@shared/util/client-constants';
import { DateUtil } from '@shared/util/date-util';

export class GetProjectTimeBlocksForTimesheetModel
  implements IGetProjectTimeBlocksForTimesheetModel
{
  employeeId!: number;
  from!: Date;
  to!: Date;
  employees!: number[];
  projects!: number[];
  orders!: number[];
  groupByDate!: boolean;
  incPlannedAbsence!: boolean;
  incInternOrderText!: boolean;
  employeeCategories!: number[];
  timeDeviationCauses!: number[];
}

export class ProjectTimeReportGridHeaderDTO {
  from!: Date;
  to!: Date;
  employeeIds!: number[];
  projectIds!: number[];
  orderIds!: number[];
  categoriesIds!: number[];
  timeDeviationCauseIds!: number[];
  dateRange!: [Date?, Date?];
}

export class ProjectWeekReportGridHeaderDTO {
  employeeId!: number;
  weekNr!: number;
  showWeekend!: boolean;
  timeProjectFrom!: Date;
}

export class ProjectTimeMatrixDTO implements IProjectTimeMatrixDTO {
  employeeId!: number;
  timeDeviationCauseId!: number;
  timeDeviationCauseName!: string;
  projectId!: number;
  projectNr!: string;
  projectName!: string;
  customerId!: number;
  customerName!: string;
  customerInvoiceId!: number;
  invoiceNr!: string;
  timeCodeId!: number;
  timeCodeName!: string;
  projectInvoiceWeekId!: number;
  rows!: ProjectTimeMatrixSaveRowDTO[];

  //Extensions
  invoiceQuantityFormatted_0!: number;
  invoiceQuantityFormatted_1!: number;
  invoiceQuantityFormatted_2!: number;
  invoiceQuantityFormatted_3!: number;
  invoiceQuantityFormatted_4!: number;
  invoiceQuantityFormatted_5!: number;
  invoiceQuantityFormatted_6!: number;
  timePayrollQuantityFormatted_0!: number;
  timePayrollQuantityFormatted_1!: number;
  timePayrollQuantityFormatted_2!: number;
  timePayrollQuantityFormatted_3!: number;
  timePayrollQuantityFormatted_4!: number;
  timePayrollQuantityFormatted_5!: number;
  timePayrollQuantityFormatted_6!: number;
  isModified: boolean = false;
  isDeleted!: boolean;
  invoiceNrName!: string;

  noteIcon_1!: string;
  noteIcon_2!: string;
  noteIcon_3!: string;
  noteIcon_4!: string;
  noteIcon_5!: string;
  noteIcon_6!: string;
  noteIcon_0!: string;
}

export class ProjectTimeMatrixSaveRowDTO
  implements IProjectTimeMatrixSaveRowDTO
{
  projectTimeBlockId!: number;
  payrollQuantity!: number;
  internalNote!: string;
  externalNote!: string;
  isPayrollEditable!: boolean;
  isInvoiceEditable!: boolean;
  employeeChildId!: number;
  payrollStateColor!: string;
  invoiceStateColor!: string;
  weekDay!: number;
  invoiceQuantity!: number;
  invoiceQuantityString!: string;
  isModified: boolean = false;
}

export class WeekReportFooterDTO {
  workedTime!: number;
  invoicedTime!: number;
}

export class NoteDialogDTO {
  date!: string;
  externalNote!: string;
  internalNote!: string;
  projectInvoiceWeekId!: number;
  projectTimeBlockId!: number;
}

export class EditTimeReportDialogDTO implements DialogData {
  title!: string;
  size?: DialogSize | undefined;
  bindToController!: boolean;
  employeesDict!: SmallGenericType[];
  employees!: IEmployeeTimeCodeDTO[];
  projectsDict: any;
  ordersDict: any;
  timeCodes!: SmallGenericType[];
  employeeDict!: SmallGenericType[];
  timeDeviationCauseDict!: SmallGenericType[];
  invoiceTimePermission!: boolean;
  workTimePermission!: boolean;
  defaultTimeCodeId!: number;

  //Extensions
  employeeId!: number;
  projectId!: number;
  employeeName!: string;
  projects: any;
  orders: any;
  timeDeviationCauses!: ITimeDeviationCauseDTO[];
  useExtendedTimeRegistration!: boolean;
  invoiceTimeAsWorkTime!: boolean;
  isNew = signal(true);
  rows: ProjectTimeBlockDTO[] = [];
  employee!: IEmployeeTimeCodeDTO;
  isTimeSheet!: boolean;
  isProjectCentral!: boolean;
  projectInvoices!: IEmployeeProjectInvoiceDTO[];
  employeeDaysWithSchedule!: IEmployeeScheduleTransactionInfoDTO[];
}

export class EmployeeInformationDialogDTO implements DialogData {
  title!: string;
  size?: DialogSize | undefined;

  //Extensions
  data!: IEmployeeScheduleTransactionInfoDTO;
}

export class AttestPayrollTransactionDTO
  implements IAttestPayrollTransactionDTO
{
  guidId!: string;
  parentGuidId!: string;
  guidIdTimeBlock!: string;
  guidIdTimeCodeTransaction!: string;
  attestItemUniqueId!: string;
  payrollCalculationProductUniqueId!: string;
  employeeId!: number;
  timePayrollTransactionId!: number;
  allTimePayrollTransactionIds!: number[];
  unitPrice?: number | undefined;
  unitPriceCurrency?: number | undefined;
  unitPriceEntCurrency?: number | undefined;
  unitPriceGrouping?: number | undefined;
  unitPricePayrollSlipGrouping?: number | undefined;
  commentGrouping!: number;
  earningTimeAccumulatorId?: number;
  amount?: number | undefined;
  amountCurrency?: number | undefined;
  amountEntCurrency?: number | undefined;
  vatAmount?: number | undefined;
  vatAmountCurrency?: number | undefined;
  vatAmountEntCurrency?: number | undefined;
  quantity!: number;
  quantityString!: string;
  isQuantityOrFixed!: boolean;
  reversedDate?: Date | undefined;
  isReversed!: boolean;
  isPreliminary!: boolean;
  isExported!: boolean;
  isEmploymentTaxAndHidden!: boolean;
  isBelowEmploymentTaxLimitRuleHidden!: boolean;
  isBelowEmploymentTaxLimitRuleFromPreviousPeriods!: boolean;
  manuallyAdded!: boolean;
  comment!: string;
  hasComment!: boolean;
  hasInfo!: boolean;
  addedDateFrom?: Date | undefined;
  addedDateTo?: Date | undefined;
  isAdded!: boolean;
  isFixed!: boolean;
  isCentRounding!: boolean;
  isQuantityRounding!: boolean;
  isSpecifiedUnitPrice!: boolean;
  isAdditionOrDeduction!: boolean;
  isVacationReplacement!: boolean;
  isAddedOrFixed!: boolean;
  isRounding!: boolean;
  showEdit!: boolean;
  isPayrollProductChainMainParent!: boolean;
  includedInPayrollProductChain!: boolean;
  updateChildren!: boolean;
  parentId?: number | undefined;
  unionFeeId?: number | undefined;
  isUnionFee!: boolean;
  employeeVehicleId?: number | undefined;
  isEmployeeVehicle!: boolean;
  employeeChildId?: number | undefined;
  employeeChildName!: string;
  payrollStartValueRowId?: number | undefined;
  isPayrollStartValue!: boolean;
  retroactivePayrollOutcomeId?: number | undefined;
  isRetroactive!: boolean;
  vacationYearEndRowId?: number | undefined;
  isVacationYearEnd!: boolean;
  isVacationFiveDaysPerWeek!: boolean;
  transactionSysPayrollTypeLevel1?: number | undefined;
  transactionSysPayrollTypeLevel2?: number | undefined;
  transactionSysPayrollTypeLevel3?: number | undefined;
  transactionSysPayrollTypeLevel4?: number | undefined;
  absenceIntervalNr!: number;
  created?: Date | undefined;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  payrollPriceFormulaId?: number | undefined;
  payrollPriceTypeId?: number | undefined;
  formula!: string;
  formulaPlain!: string;
  formulaExtracted!: string;
  formulaNames!: string;
  formulaOrigin!: string;
  payrollCalculationPerformed?: boolean | undefined;
  isDistributed!: boolean;
  isAverageCalculated!: boolean;
  timeUnit!: number;
  quantityWorkDays!: number;
  quantityCalendarDays!: number;
  calenderDayFactor!: number;
  quantityDays!: number;
  scheduleTransactionType!: SoeTimePayrollScheduleTransactionType;
  isScheduleTransaction!: boolean;
  payrollProductId!: number;
  payrollProductNumber!: string;
  payrollProductName!: string;
  payrollProductString!: string;
  payrollProductShortName!: string;
  payrollProductFactor!: number;
  payrollProductPayed!: boolean;
  payrollProductExport!: boolean;
  payrollProductUseInPayroll!: boolean;
  payrollProductSysPayrollTypeLevel1?: number | undefined;
  payrollProductSysPayrollTypeLevel2?: number | undefined;
  payrollProductSysPayrollTypeLevel3?: number | undefined;
  payrollProductSysPayrollTypeLevel4?: number | undefined;
  timeCodeTransactionId?: number | undefined;
  startTime?: Date | undefined;
  startTimeString!: string;
  stopTime?: Date | undefined;
  stopTimeString!: string;
  timeCodeType!: SoeTimeCodeType;
  timeCodeRegistrationType!: TermGroup_TimeCodeRegistrationType;
  noOfPresenceWorkOutsideScheduleTime?: number | undefined;
  isPresenceWorkOutsideScheduleTime!: boolean;
  noOfAbsenceAbsenceTime?: number | undefined;
  isAbsenceAbsenceTime!: boolean;
  timeBlockDateId!: number;
  date!: Date;
  timeBlockId?: number | undefined;
  attestStateId!: number;
  attestStateName!: string;
  attestStateColor!: string;
  attestStateInitial!: boolean;
  attestStateSort!: number;
  hasSameAttestState!: boolean;
  hasAttestState!: boolean;
  attestTransitionLogs!: IAttestTransitionLogDTO[];
  accountDims!: IAccountDimDTO[];
  accountStd!: IAccountDTO;
  accountInternals!: IAccountDTO[];
  accountingShortString!: string;
  accountingLongString!: string;
  accountingSettings!: IAccountingSettingsRowDTO[];
  accountingDescription!: string;
  accountStdId!: number;
  accountInternalIds!: number[];
  timePeriodId?: number | undefined;
  timePeriodName!: string;
  retroTransactionType!: string;
  invoiceQuantity?: number | undefined;
  isModified!: boolean;
  isSelected!: boolean;
  isSelectDisabled!: boolean;
  isPresence!: boolean;
  isAbsence!: boolean;
  payrollImportEmployeeTransactionId?: number | undefined;
  productId!: number;
}

export class ProjectTimeBlockDTO implements IProjectTimeBlockDTO {
  projectTimeBlockId!: number;
  timeSheetWeekId!: number;
  timeBlockDateId!: number;
  timeDeviationCauseId!: number;
  timeDeviationCauseName!: string;
  employeeChildId!: number;
  employeeChildName!: string;
  employeeId!: number;
  employeeName!: string;
  employeeNr!: string;
  customerId!: number;
  customerName!: string;
  date!: Date;
  yearMonth!: string;
  yearWeek!: string;
  year!: string;
  month!: string;
  week!: string;
  weekDay!: string;
  hasComment!: boolean;
  startTime!: Date;
  stopTime!: Date;
  invoiceQuantity!: number;
  internalNote!: string;
  externalNote!: string;
  employeeIsInactive!: boolean;
  created?: Date | undefined;
  createdBy!: string;
  modified?: Date | undefined;
  modifiedBy!: string;
  showInvoiceRowAttestState!: boolean;
  showPayrollAttestState!: boolean;
  isEditable: boolean = true;
  isPayrollEditable!: boolean;
  orderClosed!: boolean;
  projectId!: number;
  projectInvoiceWeekId!: number;
  projectNr!: string;
  projectName!: string;
  allocationType!: TermGroup_ProjectAllocationType;
  registrationType!: OrderInvoiceRegistrationType;
  customerInvoiceId!: number;
  invoiceNr!: string;
  referenceOur!: string;
  internOrderText!: string;
  timeCodeId!: number;
  timeCodeName!: string;
  timePayrollTransactionIds!: number[];
  timePayrollQuantity!: number;
  timePayrollAttestStateId!: number;
  timePayrollAttestStateName!: string;
  timePayrollAttestStateColor!: string;
  calculateAsOtherTimeInSales!: boolean;
  additionalTime!: boolean;
  isSalaryPayrollType!: boolean;
  scheduledQuantity!: number;
  mandatoryTime!: boolean;
  timeInvoiceTransactionId!: number;
  customerInvoiceRowAttestStateId!: number;
  customerInvoiceRowAttestStateName!: string;
  customerInvoiceRowAttestStateColor!: string;

  // extensions
  noteIcon!: string;
  showProjectButton!: boolean;
  timeCodeReadOnly: boolean = false;
  showProjectButtonshowOrderButton!: boolean;
  showOrderButton!: boolean;
  showCustomerButton!: boolean;
  invoiceQuantityFormatted = 0;
  timePayrollQuantityFormatted = 0;
  filteredTimeCodes: any[] = [];
  useExtendedTimeRegistration!: boolean;
  hasError!: boolean;
  autoGenTimeAndBreakForProject!: boolean;
  isDeleted!: boolean;
  isModified: boolean = false;
  selectedEmployee: any;
  isNew: boolean = false;
  errorText: string = '';
  payrollQuantityChanged = false;
  children: SmallGenericType[] = [];
  filteredProjects: IProjectSmallDTO[] = [];
  filteredInvoices: IProjectInvoiceSmallDTO[] = [];
  selectedOrder!: IProjectInvoiceSmallDTO;
  selectedProject!: IProjectSmallDTO;

  constructor() {
    this.startTime = new Date(new Date('1900-01-01').setHours(0, 0, 0, 0));
    this.stopTime = new Date(new Date('1900-01-01').setHours(0, 0, 0, 0));
    this.noteIcon = 'file';
    this.hasError = false;
  }
}

export class ValidateProjectTimeBlockSaveDTO
  implements IValidateProjectTimeBlockSaveDTO
{
  employeeId!: number;
  autoGenTimeAndBreakForProject!: boolean;
  rows: IValidateProjectTimeBlockSaveRowDTO[] = [];
}

export class ProjectTimeBlockSaveDTO implements IProjectTimeBlockSaveDTO {
  projectTimeBlockId!: number;
  isFromTimeSheet!: boolean;
  timeBlockDateId?: number | undefined;
  date?: Date | undefined;
  timeSheetWeekId?: number | undefined;
  projectInvoiceWeekId?: number | undefined;
  projectInvoiceDayId?: number | undefined;
  timeDeviationCauseId?: number | undefined;
  actorCompanyId!: number;
  employeeId!: number;
  projectId?: number | undefined;
  timeCodeId!: number;
  customerInvoiceId?: number | undefined;
  invoiceQuantity!: number;
  timePayrollQuantity!: number;
  internalNote!: string;
  externalNote!: string;
  state!: SoeEntityState;
  from?: Date | undefined;
  to?: Date | undefined;
  autoGenTimeAndBreakForProject!: boolean;
  employeeChildId?: number | undefined;
  mandatoryTime!: boolean;
  additionalTime!: boolean;

  public fromProjectTimeBlock(
    row: ProjectTimeBlockDTO,
    actorCompanyId: number,
    isTimeSheet: boolean,
    isProjectCentral: boolean
  ) {
    this.projectTimeBlockId = row.projectTimeBlockId;
    this.actorCompanyId = actorCompanyId;
    this.customerInvoiceId = row.customerInvoiceId;
    this.date = row.date;
    this.employeeId = row.employeeId;
    this.from = row.startTime
      ? row.startTime
      : new Date(1900, 1, 1).beginningOfDay();
    this.to = row.stopTime
      ? row.stopTime
      : new Date(1900, 1, 1).beginningOfDay();
    this.timeDeviationCauseId = row.timeDeviationCauseId;
    this.externalNote = row.externalNote;
    this.internalNote = row.internalNote;
    this.invoiceQuantity = row.invoiceQuantity;
    this.isFromTimeSheet = isTimeSheet || isProjectCentral;
    this.projectId = isTimeSheet ? row.projectId : this.projectId;
    this.projectInvoiceDayId = 0;
    this.projectInvoiceWeekId = row.projectInvoiceWeekId;
    this.state = row.isDeleted ? SoeEntityState.Deleted : SoeEntityState.Active;
    this.timeBlockDateId = row.timeBlockDateId;
    this.timeCodeId = row.timeCodeId;
    this.timePayrollQuantity = row.timePayrollQuantity;
    this.timeSheetWeekId = row.timeSheetWeekId;
    this.autoGenTimeAndBreakForProject = row.autoGenTimeAndBreakForProject;
    this.employeeChildId = row.employeeChildId;
  }
}

export class StartTimeDTO {
  startTime!: Date;
  employeeId!: number;
  date!: Date;
}

export class EmployeeScheduleTransactionInfoDTO
  implements IEmployeeScheduleTransactionInfoDTO
{
  employeeId!: number;
  employeeGroupId!: number;
  date!: Date;
  timeDeviationCauseId!: number;
  autoGenTimeAndBreakForProject!: boolean;
  timeBlocks!: IProjectTimeBlockDTO[];
  scheduleBlocks!: IProjectTimeBlockDTO[];
}

export class GetProjectTimeBlocksForMatrixModel {
  selectedEmp!: number;
  from!: Date;
}

export interface IEmployeeTimeCodeDTO extends IEmployeeSmallDTO {
  autoGenTimeAndBreakForProject: boolean;
  defaultTimeCodeId: number;
  employeeGroupId: number;
  timeDeviationCauseId: number;
}
