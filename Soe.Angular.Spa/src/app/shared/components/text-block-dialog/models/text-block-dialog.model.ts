import { ITextBlockModel } from '@shared/models/generated-interfaces/CoreModels';
import {
  ProductRowsContainers,
  SimpleTextEditorDialogMode,
  SoeEntityState,
  SoeEntityType,
  TermGroup_Languages,
  TextBlockType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ICompTermDTO,
  ITextblockDTO,
  ITextblockDTOBase,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

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
}
export class TextBlockModel implements ITextBlockModel {
  entity!: number;
  textBlock!: TextblockDTO;
  translations!: ICompTermDTO[];
}
export class TextBlockDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  disableClose?: boolean;

  //Extention
  text?: string;
  editPermission!: boolean;
  entity: SoeEntityType;
  type: TextBlockType;
  headline?: string;
  mode: SimpleTextEditorDialogMode;
  container?: ProductRowsContainers;
  langId: TermGroup_Languages;
  maxTextLength?: number;
  textboxTitle?: string;

  constructor() {
    this.disableClose = true;

    this.editPermission = false;
    this.entity = SoeEntityType.None;
    this.type = TextBlockType.TextBlockEntity;
    this.headline = '';
    this.mode = SimpleTextEditorDialogMode.Base;
    this.langId = TermGroup_Languages.Unknown;
    this.maxTextLength = undefined;
    this.textboxTitle = undefined;
  }
}
