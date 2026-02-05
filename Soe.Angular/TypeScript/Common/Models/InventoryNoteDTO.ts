import { IInventoryNoteDTO } from "../../Scripts/TypeLite.Net4";

export class InventoryNoteDTO implements IInventoryNoteDTO {
    actorCompanyId: number;
    inventoryId: number;
    description: string;
    notes: string;
}