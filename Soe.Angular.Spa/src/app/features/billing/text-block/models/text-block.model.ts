import { ITextBlockModel } from '@shared/models/generated-interfaces/CoreModels';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import {
  ICompTermDTO,
  ITextblockDTO,
  ITextblockDTOBase,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export interface ITextBlockGridDTO extends ITextblockDTOBase {
  name: string;
  type: number;
  showInContract: boolean;
  showInOffer: boolean;
  showInOrder: boolean;
  showInInvoice: boolean;
  showInPurchase: boolean;
  textBlockTypeId: number;
  textBlockTypeName: string;
}

export class TextBlockGridDTO implements ITextBlockGridDTO {
  textblockId!: number;
  text!: string;
  name!: string;
  type!: number;
  showInContract!: boolean;
  showInOffer!: boolean;
  showInOrder!: boolean;
  showInInvoice!: boolean;
  showInPurchase!: boolean;
  isModified!: boolean;
  textBlockTypeId!: number;
  textBlockTypeName!: string;
}

export class TextBlockModel implements ITextBlockModel {
  entity!: number;
  textBlock!: TextblockDTO;
  translations!: ICompTermDTO[];
}

export class TextblockDTO implements ITextblockDTO, ITextblockDTOBase {
  textblockId!: number;
  text!: string;
  isModified!: boolean;
  actorCompanyId!: number;
  headline!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state!: SoeEntityState;
  type!: number;
  showInContract!: boolean;
  showInOffer!: boolean;
  showInOrder!: boolean;
  showInInvoice!: boolean;
  showInPurchase!: boolean;
  translations!: ICompTermDTO[];
}
