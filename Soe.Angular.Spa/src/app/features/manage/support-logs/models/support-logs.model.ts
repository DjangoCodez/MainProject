import { ISysLogDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { ISearchSysLogsDTO } from '@shared/models/generated-interfaces/SearchSysLogsDTO';

export type LevelOption = {
  id: number;
  idStr: string;
  name: string;
};
export class SysLogDTO implements ISysLogDTO {
  sysLogId: number;
  date!: Date;
  level: string;
  message: string;
  exception: string;
  licenseId: string;
  licenseNr: string;
  actorCompanyId: string;
  companyName: string;
  roleId: string;
  roleName: string;
  userId: string;
  loginName: string;
  taskWatchLogId?: number;
  taskWatchLogStart: string;
  taskWatchLogStop: string;
  taskWatchLogName: string;
  taskWatchLogParameters: string;
  recorId?: number;
  application: string;
  from: string;
  hostName: string;
  ipNr: string;
  lineNumber: string;
  logClass: string;
  logger: string;
  referUri: string;
  requestUri: string;
  session: string;
  source: string;
  targetSite: string;
  thread: string;
  dateStr: string;

  constructor() {
    this.sysLogId = 0;
    this.level = '';
    this.message = '';
    this.exception = '';
    this.licenseId = '';
    this.licenseNr = '';
    this.actorCompanyId = '';
    this.companyName = '';
    this.roleId = '';
    this.roleName = '';
    this.userId = '';
    this.loginName = '';
    this.taskWatchLogStart = '';
    this.taskWatchLogStop = '';
    this.taskWatchLogName = '';
    this.taskWatchLogParameters = '';
    this.application = '';
    this.from = '';
    this.hostName = '';
    this.ipNr = '';
    this.lineNumber = '';
    this.logClass = '';
    this.logger = '';
    this.referUri = '';
    this.requestUri = '';
    this.session = '';
    this.source = '';
    this.targetSite = '';
    this.thread = '';
    this.dateStr = '';
  }
}
export class SearchSysLogsDTO implements ISearchSysLogsDTO {
  fromDate?: Date;
  toDate?: Date;
  fromTime?: Date;
  toTime?: Date;
  level: string;
  licenseSearch: string;
  companySearch: string;
  roleSearch: string;
  userSearch: string;
  incMessageSearch: string;
  exlMessageSearch: string;
  incExceptionSearch: string;
  exExceptionSearch: string;
  noOfrecords?: number;
  showUnique: boolean;
  levelSelect: number = 1;

  constructor() {
    this.level = '';
    this.licenseSearch = '';
    this.companySearch = '';
    this.roleSearch = '';
    this.userSearch = '';
    this.incMessageSearch = '';
    this.exlMessageSearch = '';
    this.incExceptionSearch = '';
    this.exExceptionSearch = '';
    this.showUnique = false;
  }
}
