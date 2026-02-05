import { IImagesDTO } from "../../Scripts/TypeLite.Net4";
import { ImageFormatType, SoeEntityImageType, SoeDataStorageRecordType, InvoiceAttachmentSourceType, TermGroup_DataStorageRecordAttestStatus } from "../../Util/CommonEnumerations";


export class ImageDTO implements IImagesDTO {
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    attestStatus: TermGroup_DataStorageRecordAttestStatus;
    canDelete: boolean;
    confirmed: boolean;
    confirmedDate: Date;
    connectedTypeName: string;
    created: Date;
    currentAttestUsers: string;
    description: string;
    fileName: string;
    formatType: ImageFormatType;
    image: number[];
    imageId: number;
    includeWhenDistributed: boolean;
    includeWhenTransfered: boolean;
    invoiceAttachmentId: number;
    lastSentDate: Date;
    needsConfirmation: boolean;
    type: SoeEntityImageType;
    dataStorageRecordType: SoeDataStorageRecordType;
    sourceType: InvoiceAttachmentSourceType;
    recordId: number;

    // Extensions
    icon: string;
    fileFormat: string;
    isModified: boolean;
    isAdded: boolean;
    extension: string;
    attestStatusText: string;

    get hideDelete() {
        return this.type && this.type === SoeEntityImageType.SupplierInvoice;
    }

    public ImageDTO() { }

    public setFileFormat() {
        let parts: string[] = this.fileName ? this.fileName.split('.') : [];
        if (parts.length > 0)
            this.fileFormat = parts[parts.length - 1];
    }

    public setIcon() {
        if (this.isWordDocument())
            this.icon = "fal fa-file-word";
        else if (this.isExcelDocument())
            this.icon = "fal fa-file-excel";
        else if (this.isTextDocument())
            this.icon = "fal fa-file-alt";
        else if (this.isPdfDocument())
            this.icon = "fal fa-file-pdf";
        else if (this.isImage())
            this.icon = "fal fa-file-image";
        else
            this.icon = "fal fa-file";
    }

    public isWordDocument(): boolean {
        return this.isExtension("docx") || this.isExtension("doc");
    }

    public isExcelDocument(): boolean {
        return this.isExtension("xlsx") || this.isExtension("xls") || this.isExtension("csv");
    }

    public isTextDocument(): boolean {
        return this.isExtension("txt");
    }

    public isPdfDocument(): boolean {
        return this.isExtension("pdf");
    }

    public isImage(): boolean {
        return this.isExtension("jpg") || this.isExtension("jpeg") || this.isExtension("png") || this.isExtension("bmp") || this.isExtension("gif");
    }

    public isMiscDocument(): boolean {
        return !this.isWordDocument && !this.isExcelDocument && !this.isTextDocument && !this.isPdfDocument && !this.isImage;
    }

    private isExtension(extension: string): boolean {
        return this.fileName && this.fileName.endsWithCaseInsensitive(extension);
    }
}
