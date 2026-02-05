import {
  SoeEntityState,
  TermGroup_TimeWorkAccountWithdrawalMethod,
  TermGroup_TimeWorkAccountYearEmployeeStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ITimeWorkAccountDTO,
  ITimeWorkAccountYearDTO,
  ITimeWorkAccountWorkTimeWeekDTO,
  ITimeWorkAccountYearEmployeeDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class TimeWorkAccountDTO implements ITimeWorkAccountDTO {
  timeWorkAccountId: number;
  actorCompanyId: number;
  name: string;
  code: string;
  usePensionDeposit: boolean;
  usePaidLeave: boolean;
  useDirectPayment: boolean;
  defaultWithdrawalMethod: TermGroup_TimeWorkAccountWithdrawalMethod;
  defaultPaidLeaveNotUsed: TermGroup_TimeWorkAccountWithdrawalMethod;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  timeWorkAccountYears: ITimeWorkAccountYearDTO[];
  isNew: boolean;

  constructor() {
    this.timeWorkAccountId = 0;
    this.actorCompanyId = 0;
    this.name = '';
    this.code = '';
    this.usePensionDeposit = false;
    this.usePaidLeave = false;
    this.useDirectPayment = false;
    this.defaultWithdrawalMethod =
      TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed;
    this.defaultPaidLeaveNotUsed =
      TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed;
    this.createdBy = '';
    this.modifiedBy = ';';
    this.state = SoeEntityState.Active;
    this.timeWorkAccountYears = [];
    this.isNew = true;
  }
}

export class TimeWorkAccountYearDTO implements ITimeWorkAccountYearDTO {
  timeWorkAccountYearId: number;
  timeWorkAccountId: number;
  earningStart: Date;
  earningStop: Date;
  withdrawalStart: Date;
  withdrawalStop: Date;
  pensionDepositPercent?: number;
  paidLeavePercent?: number;
  directPaymentPercent?: number;
  employeeLastDecidedDate: Date;
  paidAbsenceStopDate: Date;
  directPaymentLastDate: Date;
  pensionDepositPayrollProductId?: number;
  directPaymentPayrollProductId?: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  timeWorkAccountYearEmployees: ITimeWorkAccountYearEmployeeDTO[];
  timeWorkAccountWorkTimeWeeks: ITimeWorkAccountWorkTimeWeekDTO[];
  isNew: boolean;
  timeAccumulatorId?: number;

  constructor() {
    this.timeWorkAccountYearId = 0;
    this.timeWorkAccountId = 0;
    this.earningStart = new Date();
    this.earningStop = new Date();
    this.withdrawalStart = new Date();
    this.withdrawalStop = new Date();
    this.pensionDepositPercent = 0;
    this.paidLeavePercent = 0;
    this.directPaymentPercent = 0;
    this.employeeLastDecidedDate = new Date();
    this.paidAbsenceStopDate = new Date();
    this.directPaymentLastDate = new Date();
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.timeWorkAccountYearEmployees = [];
    this.timeWorkAccountWorkTimeWeeks = [];
    this.isNew = true;
  }
}

export class TimeWorkAccountWorkTimeWeekDTO
  implements ITimeWorkAccountWorkTimeWeekDTO
{
  timeWorkAccountWorkTimeWeekId: number;
  timeWorkAccountYearId: number;
  workTimeWeekFrom: number;
  workTimeWeekTo: number;
  paidLeaveTime: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;

  constructor() {
    this.timeWorkAccountWorkTimeWeekId = 0;
    this.timeWorkAccountYearId = 0;
    this.workTimeWeekFrom = 0;
    this.workTimeWeekTo = 0;
    this.paidLeaveTime = 0;
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
  }
}

export class TimeWorkAccountYearEmployeeDTO
  implements ITimeWorkAccountYearEmployeeDTO
{
  timeWorkAccountYearEmployeeId: number;
  timeWorkAccountId: number;
  employeeId: number;
  employeeName: string;
  employeeNumber: string;
  status: TermGroup_TimeWorkAccountYearEmployeeStatus;
  earningStart: Date;
  earningStop: Date;
  selectedWithdrawalMethod: TermGroup_TimeWorkAccountWithdrawalMethod;
  selectedDate?: Date;
  sentDate?: Date;
  calculatedPaidLeaveMinutes: number;
  calculatedPaidLeaveAmount: number;
  calculatedPensionDepositAmount: number;
  calculatedDirectPaymentAmount: number;
  calculatedWorkingTimePromoted: number;
  specifiedWorkingTimePromoted?: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;

  constructor() {
    this.timeWorkAccountYearEmployeeId = 0;
    this.timeWorkAccountId = 0;
    this.employeeId = 0;
    this.employeeName = '';
    this.employeeNumber = '';
    this.status = TermGroup_TimeWorkAccountYearEmployeeStatus.NotCalculated;
    this.earningStart = new Date();
    this.earningStop = new Date();
    this.selectedWithdrawalMethod =
      TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed;
    this.calculatedPaidLeaveMinutes = 0;
    this.calculatedPaidLeaveAmount = 0;
    this.calculatedPensionDepositAmount = 0;
    this.calculatedDirectPaymentAmount = 0;
    this.calculatedWorkingTimePromoted = 0;
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
  }
}
