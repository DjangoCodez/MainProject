import { IFileUploadDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityImageType, SoeDataStorageRecordType, InvoiceAttachmentSourceType } from "../../Util/CommonEnumerations";

export class FileUploadDTO implements IFileUploadDTO {
    description: string;
    fileName: string;
    id: number;
    invoiceAttachmentId: number;
    imageId: number;
    isDeleted: boolean;
    isSupplierInvoice: boolean;
    includeWhenDistributed: boolean;
    includeWhenTransfered: boolean;
    dataStorageRecordType: SoeDataStorageRecordType;
    sourceType: InvoiceAttachmentSourceType;
    recordId: number;
}
