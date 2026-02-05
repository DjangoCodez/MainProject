import { IProjectSearchModel } from '@shared/models/generated-interfaces/EconomyModels';
import { TermGroup_ProjectStatus } from '@shared/models/generated-interfaces/Enumerations';
import { IProjectSearchResultDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { ISaveUserCompanySettingModel } from '@shared/models/generated-interfaces/TimeModels';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class SelectProjectDialogFormDTO {
  showWithoutCustomer!: boolean;
  projectsWithoutCustomer!: boolean;
  showFindHidden!: boolean;
  showMine!: boolean;
}

export class ProjectSearchModel implements IProjectSearchModel {
  number!: string;
  name!: string;
  customerNr!: string;
  customerName!: string;
  managerName!: string;
  orderNr!: string;
  onlyActive!: boolean;
  hidden!: boolean;
  showWithoutCustomer!: boolean;
  loadMine!: boolean;
  customerId?: number;
  showAllProjects: boolean;
  constructor(
    number: string,
    name: string,
    customerNr: string,
    customerName: string,
    managerName: string,
    orderNr: string,
    onlyActive: boolean,
    hidden: boolean,
    showWithoutCustomer: boolean,
    loadMine: boolean,
    customerId?: number,
    showAllProjects?: boolean
  ) {
    this.number = number;
    this.name = name;
    this.customerNr = customerNr;
    this.customerName = customerName;
    this.managerName = managerName;
    this.orderNr = orderNr;
    this.onlyActive = onlyActive;
    this.hidden = hidden;
    this.showWithoutCustomer = showWithoutCustomer;
    this.loadMine = loadMine;
    this.customerId = customerId;
    this.showAllProjects = showAllProjects ?? false;
  }
}

export class SelectProjectDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;

  customerId!: number;
  projectsWithoutCustomer!: boolean;
  showFindHidden!: boolean;
  loadHidden!: boolean;
  useDelete?: boolean;
  currentProjectNr?: string;
  currentProjectId?: number;
  showAllProjects?: boolean;
  excludeProjectId?: number;
}

export class ProjectSearchResultDTO implements IProjectSearchResultDTO {
  projectId!: number;
  number!: string;
  name!: string;
  status: TermGroup_ProjectStatus = TermGroup_ProjectStatus.Unknown;
  statusName!: string;
  managerName!: string;
  managerUserId?: number;
  customerId?: number;
  customerNr!: string;
  customerName!: string;
  orderNr!: string;
  orderNbrs!: string[];
}

export class SaveUserCompanySettingModel
  implements ISaveUserCompanySettingModel
{
  settingMainType!: number;
  settingTypeId!: number;
  boolValue!: boolean;
  intValue!: number;
  stringValue!: string;
  constructor(
    settingMainType: number,
    settingTypeId: number,
    boolValue: boolean
  ) {
    this.settingMainType = settingMainType;
    this.settingTypeId = settingTypeId;
    this.boolValue = boolValue;
  }
}
