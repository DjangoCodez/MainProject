import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { SoeOriginStatus } from '@shared/models/generated-interfaces/Enumerations';
import { AccountDistributionEntryDTO } from '../../../models/inventory-writeoffs.model';
import { ISaveInventoryNotesModel } from '@shared/models/generated-interfaces/EconomyModels';

export class InventoryNotesDialogData implements DialogData {
  title!: string;
  content?: string;
  size?: DialogSize;
  newStatus!: SoeOriginStatus;
  rows!: AccountDistributionEntryDTO[];
  selectedRow!: AccountDistributionEntryDTO;
  hideFooter: boolean;

  constructor(
    _selectedRow: AccountDistributionEntryDTO,
    _rows: AccountDistributionEntryDTO[]
  ) {
    this.title = _selectedRow.inventoryName;
    this.selectedRow = _selectedRow;
    this.rows = _rows;
    this.hideFooter = true;
  }
}

export class SaveInventoryNotesModel implements ISaveInventoryNotesModel {
  inventoryId: number;
  name: string;
  notes: string;
  description: string;

  constructor(
    _inventoryId: number,
    _inventoryName: string = '',
    _inventoryNotes: string = '',
    _inventoryDescription: string = ''
  ) {
    this.inventoryId = _inventoryId;
    this.name = _inventoryName;
    this.notes = _inventoryNotes;
    this.description = _inventoryDescription;
  }

  static fromAccountDistributionEntryDTO(item: AccountDistributionEntryDTO) {
    return new SaveInventoryNotesModel(
      item.inventoryId as number,
      item.inventoryName,
      item.inventoryNotes,
      item.inventoryDescription
    );
  }
}
