import { SoeEntityImageType, SoeEntityType, ImageFormatType } from "../../Util/CommonEnumerations";
import { ICoreService } from "../../Core/Services/CoreService";
import { FileUploadDTO } from "../../Common/Models/FileUploadDTO";
import { IDirtyHandler } from "../../Core/Handlers/DirtyHandler";
import { ImageDTO } from "../Models/ImageDTO";
import { StringUtility } from "../../Util/StringUtility";

export class FilesHelper {

    public filesRendered = false;
    public filesLoaded = false;
    public files: any[] = [];
    public nbrOfFiles: any;
    public loadingFiles = false;

    public addAttachementsToEInvoice = false;
    public addSupplierInvoicesToEInvoice = false;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private $q: ng.IQService,
        private clientDirtyHandler: IDirtyHandler,
        private setDirtyOnUpload: boolean,
        public entity: SoeEntityType,
        public type: SoeEntityImageType,
        private getRecordId: () => number,
        private defaultUploadTerm?: string,
        private overrideRecordCheck?: boolean,
    ) {
    }

    public loadFiles(reload = false, projectId: number = null, useThumbnails: boolean = true): ng.IPromise<any> {
        
        const recordId = this.getRecordId();
        if (recordId == null && !this.overrideRecordCheck) {
            const deferral = this.$q.defer();
            deferral.reject("RecordId is empty");
            return deferral.promise;
        }

        this.filesRendered = true;
        if (this.filesLoaded && reload) {
            this.filesLoaded = false;
        }

        if (this.filesLoaded || !recordId) {
            this.filesLoaded = true;
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }

        this.loadingFiles = true;
        this.files = [];
        
        return this.coreService.getImages(this.type, this.entity, recordId, useThumbnails, projectId).then(x => {
            this.files = x.map(dto => {
                const obj = new ImageDTO();
                angular.extend(obj, dto);

                if (!obj.fileName)
                    obj.fileName = " ";
                if (!obj.description)
                    obj.description = " ";
                if (obj.image)
                    obj.image = <number[]>obj.image;

                obj.extension = StringUtility.getFileExtension(obj.fileName);

                if (obj.includeWhenDistributed === undefined)
                    obj.includeWhenDistributed = this.addSupplierInvoicesToEInvoice;

                return obj;
            });

            this.nbrOfFiles = this.files.length;
            this.loadingFiles = false;
            this.filesLoaded = true;
        });
    }

    public fileUploadedCallback(result, setAsModified = true, setRecordId = false) {
        const obj = new ImageDTO();
        angular.extend(obj, result);
        obj.imageId = result.id;
        obj.isModified = setAsModified;
        obj.isAdded = true;
        obj.canDelete = true;
        obj.description = obj.fileName;
        obj.extension = StringUtility.getFileExtension(obj.fileName);
        obj.includeWhenDistributed = this.addAttachementsToEInvoice;
        obj.includeWhenTransfered = true;
        obj.created = new Date();
        if (this.defaultUploadTerm)
            obj.connectedTypeName = this.defaultUploadTerm;

        if(setRecordId)
            obj.recordId = this.getRecordId();

        this.files.push(obj);
        this.nbrOfFiles = this.files.length;
        if (this.setDirtyOnUpload)
            this.trySetAsDirty();
    }

    public fileNameChanged(file) {
        this.trySetAsDirty();
    }

    public getAsDTOs(onlyModified?: boolean, skip?: SoeEntityImageType, setIsSupplierInvoice?: boolean): FileUploadDTO[] {
        let filteredFiles = this.files;
        if (onlyModified) {
            let temp = [];
            _.forEach(filteredFiles, (f) => {
                if (f.isModified)
                    temp.push(f);
            });
            filteredFiles = temp;
        }
        if (skip) {
            let temp = [];
            _.forEach(filteredFiles, (f) => {
                if (!f.type || f.type !== skip)
                    temp.push(f);
            });
            filteredFiles = temp;
        }

        if (this.filesLoaded) {
            return filteredFiles.map(dto => {
                const obj = new FileUploadDTO();
                if (dto.id)
                    obj.id = dto.id;
                else if (dto.formatType === ImageFormatType.NONE || dto.formatType === ImageFormatType.PDF || dto.dataStorageRecordType)
                    obj.id = dto.imageId;
                else
                    obj.imageId = dto.imageId;

                obj.isSupplierInvoice = setIsSupplierInvoice ? dto.isSupplierInvoice : false;
                obj.fileName = dto.fileName;
                obj.description = dto.description;
                obj.isDeleted = dto.isDeleted;
                obj.includeWhenDistributed = dto.includeWhenDistributed;
                obj.includeWhenTransfered = dto.includeWhenTransfered;
                obj.invoiceAttachmentId = dto.invoiceAttachmentId;
                obj.dataStorageRecordType = dto.dataStorageRecordType;
                obj.sourceType = dto.sourceType;
                obj.recordId = dto.recordId;

                return obj;
            });
        }
        else { return []; }
    }

    public reset() {
        this.files = [];
        this.filesLoaded = false;
        this.filesRendered = false;
    }

    public changeTransferBatch(value: boolean) {
        _.forEach(this.files, (file) => {
            file.includeWhenTransfered = value;
        });
    }

    public changeDistributeBatch(value: boolean) {
        _.forEach(this.files, (file) => {
            file.includeWhenDistributed = value;
            file.isModified = true;
        });
    }

    public nbrOfFilesChanged(nbrOfFiles: number) {
        this.trySetAsDirty(() => this.files.length !== nbrOfFiles);

        if (nbrOfFiles === 0)
            nbrOfFiles = null;

        if (this.nbrOfFiles != "*" || nbrOfFiles != null)
            this.nbrOfFiles = nbrOfFiles;
    }

    private trySetAsDirty(additionalCheck: () => boolean = () => true) {
        if (!this.clientDirtyHandler)
            return;

        const canBeDirty = this.getRecordId() && !this.clientDirtyHandler.isDirty && additionalCheck()
        if (canBeDirty) {
            this.clientDirtyHandler.setDirty();
        }
    }
}