import { IImportDTO, IImportSelectionGridRowDTO } from "../../Scripts/TypeLite.Net4";

export class ImportSelectionGridRowDTO implements IImportSelectionGridRowDTO {
    public fileName: string;
    public fileType: string;
    public dataStorageId: number;
    public import: IImportDTO;
    public importId?: number;
    public importName?: string;
    public message: string;
    public doImport: boolean;
    public disableImport: boolean;

    constructor(
        FileName: string,
        FileType: string,
        DataStorageId: number,
        Import: IImportDTO,
        Message: string = "",
        DoImport: boolean | undefined
    ) {
        this.fileName = FileName;
        this.fileType = FileType;
        this.dataStorageId = DataStorageId;
        this.import = Import;
        this.importId = Import?.importId;
        this.importName = Import?.name;
        this.message = Message;
        this.doImport = DoImport ?? Message === "";
        this.disableImport = Import == undefined;
    }
}