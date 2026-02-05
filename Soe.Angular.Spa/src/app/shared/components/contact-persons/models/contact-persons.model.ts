import {
  TermGroup_Sex,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IContactPersonDTO,
  IContactPersonGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class ContactPersonDTO implements IContactPersonDTO {
  actorContactPersonId: number;
  consentDate?: Date;
  consentModified?: Date;
  consentModifiedBy: string;
  created?: Date;
  createdBy!: string;
  description: string;
  categoryIds: number[];
  categoryRecords: any[];
  email: string;
  firstAndLastName: string;
  firstName: string;
  hasConsent: boolean;
  hasConsentId!: number;
  lastName: string;
  modified?: Date;
  modifiedBy!: string;
  phoneNumber: string;
  position!: number;
  positionName: string;
  sex: TermGroup_Sex;
  socialSec: string;
  state: SoeEntityState;
  categoryString!: string | undefined;

  constructor() {
    this.actorContactPersonId = 0;
    this.firstName = '';
    this.lastName = '';
    this.firstAndLastName = '';
    this.position = 0;
    this.description = '';
    this.socialSec = '';
    this.sex = TermGroup_Sex.Unknown;
    this.state = SoeEntityState.Active;
    this.positionName = '';
    this.email = '';
    this.phoneNumber = '';
    this.hasConsent = true;
    this.consentModifiedBy = '';
    this.categoryIds = [];
    this.categoryRecords = [];
    this.consentDate = undefined;
  }
}

export class ContactPersonGridDTO implements IContactPersonGridDTO {
  actorContactPersonId: number;
  firstName: string;
  lastName: string;
  firstAndLastName: string;
  position: number;
  description: string;
  socialSec: string;
  sex: TermGroup_Sex;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  positionName: string;
  email: string;
  phoneNumber: string;
  categoryString: string;
  supplierId?: number;
  supplierName: string;
  supplierNr: string;
  customerId?: number;
  customerName: string;
  customerNr: string;
  hasConsent: boolean;
  hasConsentId: number;
  consentDate?: Date;
  consentModified?: Date;
  consentModifiedBy: string;

  constructor() {
    this.actorContactPersonId = 0;
    this.firstName = '';
    this.lastName = '';
    this.firstAndLastName = '';
    this.position = 0;
    this.description = '';
    this.socialSec = '';
    this.sex = TermGroup_Sex.Unknown;
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.positionName = '';
    this.email = '';
    this.phoneNumber = '';
    this.categoryString = '';
    this.supplierName = '';
    this.supplierNr = '';
    this.customerName = '';
    this.customerNr = '';
    this.hasConsent = true;
    this.hasConsentId = 1;
    this.consentModifiedBy = '';
  }
}
