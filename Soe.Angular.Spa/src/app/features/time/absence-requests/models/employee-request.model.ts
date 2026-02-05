import {
  SoeEntityState,
  TermGroup_EmployeeRequestResultStatus,
  TermGroup_EmployeeRequestStatus,
  TermGroup_EmployeeRequestType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IEmployeeRequestDTO,
  IExtendedAbsenceSettingDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

// export class EmployeeRequestsDTO implements IEmployeeRequestDTO {
//   employeeRequestId: number = 0;
//   actorCompanyId: number = 0;
//   employeeId: number = 0;
//   timeDeviationCauseId?: number | undefined;
//   type: TermGroup_EmployeeRequestType =
//     TermGroup_EmployeeRequestType.AbsenceRequest;
//   status: TermGroup_EmployeeRequestStatus =
//     TermGroup_EmployeeRequestStatus.None;
//   resultStatus: TermGroup_EmployeeRequestResultStatus =
//     TermGroup_EmployeeRequestResultStatus.None;
//   start: Date = new Date();
//   stop: Date = new Date();
//   startString: string = '';
//   stopString: string = '';
//   comment: string = '';
//   reActivate: boolean = false;
//   created?: Date | undefined;
//   createdString: string = '';
//   createdBy: string = '';
//   modified?: Date | undefined;
//   modifiedBy: string = '';
//   state: SoeEntityState = SoeEntityState.Active;
//   employeeName: string = '';
//   timeDeviationCauseName: string = '';
//   statusName: string = '';
//   resultStatusName: string = '';
//   extendedSettings: IExtendedAbsenceSettingDTO =
//     new ExtendedAbsenceSettingDTO();
//   requestIntersectsWithCurrent: boolean = false;
//   intersectMessage: string = '';
//   employeeChildId?: number | undefined;
//   employeeChildName: string = '';
//   categoryNamesString: string = '';
//   accountNamesString: string = '';
//   isSelected: boolean = false;
// }

export class EmployeeRequestsDTO implements IEmployeeRequestDTO {
  employeeRequestId!: number;
  actorCompanyId!: number;
  employeeId!: number;
  timeDeviationCauseId?: number | undefined;
  type!: TermGroup_EmployeeRequestType;
  status!: TermGroup_EmployeeRequestStatus;
  resultStatus!: TermGroup_EmployeeRequestResultStatus;
  start!: Date;
  stop!: Date;
  startString!: string;
  stopString!: string;
  comment!: string;
  reActivate!: boolean;
  created?: Date | undefined;
  createdString!: string;
  createdBy!: string;
  modified?: Date | undefined;
  modifiedBy!: string;
  state!: SoeEntityState;
  employeeName!: string;
  timeDeviationCauseName!: string;
  statusName!: string;
  resultStatusName!: string;
  extendedSettings!: IExtendedAbsenceSettingDTO;
  requestIntersectsWithCurrent!: boolean;
  intersectMessage!: string;
  employeeChildId?: number | undefined;
  employeeChildName!: string;
  categoryNamesString!: string;
  accountNamesString!: string;
  isSelected!: boolean;
}
