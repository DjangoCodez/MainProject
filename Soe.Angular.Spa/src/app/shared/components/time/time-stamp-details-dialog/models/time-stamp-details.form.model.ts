import { SoeDateFormControl, SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  SoeEntityState,
  TermGroup_TimeStampEntryOriginType,
  TermGroup_TimeStampEntryStatus,
  TimeStampEntryType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ITimeStampEntryDTO,
  ITimeStampEntryExtendedDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimeStampDetailsForm {
  validationHandler: ValidationHandler;
  element: any | undefined;
}
export class TimeStampDetailsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler | undefined;

  constructor({ validationHandler, element }: ITimeStampDetailsForm) {
    super(validationHandler, {
      selectedDateFrom: new SoeDateFormControl(
        element?.selectedDateFrom || new Date(),
        {},
        'common.fromdate'
      ),
      selectedDateTo: new SoeDateFormControl(
        element?.selectedDateTo || new Date(),
        {},
        'common.todate'
      ),
    });

    this.thisValidationHandler = validationHandler;
  }
}

export class TimeStampEntryDTO implements ITimeStampEntryDTO {
  timeStampEntryId!: number;
  timeTerminalId?: number;
  actorCompanyId!: number;
  employeeId!: number;
  timeDeviationCauseId?: number;
  accountId?: number;
  timeTerminalAccountId?: number;
  timeScheduleTemplatePeriodId?: number;
  timeBlockDateId?: number;
  employeeChildId?: number;
  shiftTypeId?: number;
  timeScheduleTypeId?: number;
  originType!: TermGroup_TimeStampEntryOriginType;
  type!: TimeStampEntryType;
  terminalStampData!: string;
  note!: string;
  time!: Date;
  originalTime?: Date;
  manuallyAdjusted!: boolean;
  employeeManuallyAdjusted!: boolean;
  status!: TermGroup_TimeStampEntryStatus;
  isBreak!: boolean;
  isPaidBreak!: boolean;
  isDistanceWork!: boolean;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state!: SoeEntityState;
  extended!: ITimeStampEntryExtendedDTO[];
  employeeNr!: string;
  employeeName!: string;
  timeDeviationCauseName!: string;
  timeScheduleTypeName!: string;
  accountNr!: string;
  accountName!: string;
  timeBlockDateDate?: Date;
  adjustedTimeBlockDateDate?: Date;
  adjustedTime?: Date;
  date!: Date;
  typeName!: string;
  timeTerminalName!: string;
  isModified!: boolean;
  dateFromTimeBlockDate?: Date;

  statusText?: string;
  originTypeText?: string;
  terminalInfo?: string;
  constructor() {
    this.timeStampEntryId = 0;
    this.timeTerminalId = 0;
    this.actorCompanyId = 0;
    this.employeeId = 0;
    this.originType = TermGroup_TimeStampEntryOriginType.Unknown;
    this.type = TimeStampEntryType.Unknown;
    this.terminalStampData = '';
    this.note = '';
    this.time = new Date();
    this.manuallyAdjusted = false;
    this.employeeManuallyAdjusted = false;
    this.status = TermGroup_TimeStampEntryStatus.New;
    this.isBreak = false;
    this.isPaidBreak = false;
    this.isDistanceWork = false;
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.extended = [];
    this.employeeNr = '';
    this.employeeName = '';
    this.timeDeviationCauseName = '';
    this.timeScheduleTypeName = '';
    this.accountNr = '';
    this.accountName = '';
    this.date = new Date();
    this.typeName = '';
    this.timeTerminalName = '';
    this.isModified = false;
    this.dateFromTimeBlockDate = undefined;
    this.statusText = undefined;
    this.originTypeText = undefined;
    this.terminalInfo = undefined;
  }
}
