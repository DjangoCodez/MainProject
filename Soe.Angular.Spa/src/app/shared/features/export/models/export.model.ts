import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import {
  IExportDTO,
  IExportGridDTO,
} from '@shared/models/generated-interfaces/ExportDTO';

export class ExportDTO implements IExportDTO {
  exportId: number;
  actorCompanyId: number;
  exportDefinitionId: number;
  module: number;
  standard: boolean;
  name: string;
  filename: string;
  emailaddress: string;
  subject: string;
  attachFile: boolean;
  sendDirect: boolean;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: number;
  guid: string;
  specialFunctionality: string;
  reportSelectionId?: number;
  customType: number;

  constructor() {
    this.exportId = 0;
    this.actorCompanyId = 0;
    this.exportDefinitionId = 0;
    this.module = 0;
    this.standard = false;
    this.name = '';
    this.filename = '';
    this.emailaddress = '';
    this.subject = '';
    this.attachFile = false;
    this.sendDirect = false;
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.guid = '';
    this.specialFunctionality = '';
    this.customType = 0;
  }
}

export class ExportGridDTO implements IExportGridDTO {
  exportId: number;
  name: string;

  constructor() {
    this.exportId = 0;
    this.name = '';
  }
}
