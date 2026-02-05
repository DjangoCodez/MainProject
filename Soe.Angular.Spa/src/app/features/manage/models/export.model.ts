import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import {
  IExportDefinitionDTO,
  IExportDefinitionLevelColumnDTO,
  IExportDefinitionLevelDTO,
} from '@shared/models/generated-interfaces/ExportDTO';

export class ExportDefinitionDTO implements IExportDefinitionDTO {
  exportDefinitionId: number;
  actorCompanyId: number;
  sysExportHeadId?: number;
  name: string;
  type?: number;
  separator: string;
  xmlTagHead: string;
  specialFunctionality: string;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  exportDefinitionLevels: ExportDefinitionLevelDTO[];
  exportHeadName: string;
  isStandardExport: string;
  exportTypeName: string;
  reportSelectionId?: number;
  reportUserSelection: any;

  constructor() {
    this.exportDefinitionId = 0;
    this.actorCompanyId = 0;
    this.name = '';
    this.separator = '';
    this.xmlTagHead = '';
    this.specialFunctionality = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.exportDefinitionLevels = [];
    this.exportHeadName = '';
    this.isStandardExport = '';
    this.exportTypeName = '';
  }
}

export class ExportDefinitionLevelDTO implements IExportDefinitionLevelDTO {
  exportDefinitionLevelId: number;
  exportDefinitionId: number;
  level: number;
  xml: string;
  useColumnHeaders: boolean;
  exportDefinitionLevelColumns: ExportDefinitionLevelColumnDTO[];

  constructor() {
    this.exportDefinitionId = 0;
    this.exportDefinitionLevelId = 0;
    this.level = 0;
    this.xml = '';
    this.useColumnHeaders = false;
    this.exportDefinitionLevelColumns = [];
  }
}

export class ExportDefinitionLevelColumnDTO
  implements IExportDefinitionLevelColumnDTO
{
  exportDefinitionLevelColumnId: number;
  exportDefinitionLevelId: number;
  name: string;
  description: string;
  key: string;
  position: number;
  defaultValue: string;
  matrixDefinitionColumn: any;
  isDelimiter: boolean;
  isPosition: boolean;
  fillChar: string;
  fillBeginning?: boolean;
  formatDate: string;
  columnLength: number;
  xmlTag: string;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  columnHeader: string;
  convertValue: string;
  getDateValueFromKey: string;

  constructor() {
    this.exportDefinitionLevelColumnId = 0;
    this.exportDefinitionLevelId = 0;
    this.name = '';
    this.description = '';
    this.key = '';
    this.position = 0;
    this.defaultValue = '';
    this.isDelimiter = false;
    this.isPosition = false;
    this.fillChar = '';
    this.formatDate = '';
    this.columnLength = 0;
    this.xmlTag = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.columnHeader = '';
    this.convertValue = '';
    this.getDateValueFromKey = '';
    this.state = SoeEntityState.Active;
  }
}
