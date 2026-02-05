import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SoeEntityImageType, ImageFormatType, SoeDataStorageRecordType } from "../../../Util/CommonEnumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class FileItemDisplayDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('FileItemDisplay', 'FileItemDisplay.html'),
            scope: {
                file: '=',
                onDelete: '&',
                onNameChanged: '&',
                onEditRoles: '&',
                showRoles: '=',
                readOnly: '='
            },
            restrict: 'E',
            replace: true,
            controller: FileItemDisplayController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class FileItemDisplayController {

    private file: any;
    private readOnly: boolean;
    private isEditingName = false;
    private fileName: string;
    onNameChanged: (file: any) => void;

    //@ngInject
    constructor(private $element,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService) {
    }

    $onInit() {
        if (this.file) {            
            if (this.file.isUploadedAsImage && !this.file.image && this.file.data)
                this.file.image = this.file.data;

            if (this.file.needsConfirmation) {
                const keys: string[] = [
                    "common.messages.confirmed",
                    "common.messages.needsconfirmation"
                ];
                this.translationService.translateMany(keys).then(terms => {
                    this.file.confirmedMessage = this.file.confirmedDate ? "{0} {1}".format(terms["common.messages.confirmed"], CalendarUtility.convertToDate(this.file.confirmedDate).toFormattedDateTime()) : terms["common.messages.needsconfirmation"];
                });
            }
        }
    }

    private hideDelete() {
        return this.file && this.file.type && this.file.type === SoeEntityImageType.SupplierInvoice;
    }

    private isWordDocument() {
        return this.isExtension("docx") || this.isExtension("doc");
    }

    private isExcelDocument() {
        return this.isExtension("xlsx") || this.isExtension("xls") || this.isExtension("csv");
    }

    private isTextDocument() {
        return this.isExtension("txt");
    }

    private isPdfDocument() {
        return this.isExtension("pdf");
    }

    private isImage() {
        return this.isExtension("jpg") || this.isExtension("jpeg") || this.isExtension("png") || this.isExtension("bmp") || this.isExtension("gif");
    }

    private isMiscDocument() {
        return !this.isWordDocument() && !this.isExcelDocument() && !this.isTextDocument() && !this.isPdfDocument() && !this.isImage();
    }

    private isExtension(extension: string): boolean {
        let name: string;
        if (this.file) {
            if (this.file.fileName)
                name = this.file.fileName;
            else if (this.file.name)
                name = this.file.name;
        }
        return name.endsWithCaseInsensitive(extension);
    }

    private beginChangeName() {
        this.isEditingName = true;
        this.fileName = this.file.description;

        this.$timeout(() => {
            this.$element.find('input').focus();
            this.$element.find('input').select();
        });
    }

    private endChangeName() {
        this.$timeout(() => {
            if (!this.file.description)
                this.file.description = this.fileName;

            // When pressing enter, this method is called twice.
            // The first time without model updated
            if (!this.isEditingName && this.file.description !== this.fileName)
                this.isEditingName = true;

            if (this.isEditingName) {
                this.isEditingName = false
                if (this.file.description !== this.fileName) {
                    this.file.isModified = true;
                    this.onNameChanged(this.file);
                }
            }
        });
    }

    private getUrl() {
        if (this.file.type && this.file.type === SoeEntityImageType.SupplierInvoice && this.file.formatType && this.file.formatType === ImageFormatType.PDF && this.file.fileName === "invoiceimage")
            return `/ajax/downloadTextFile.aspx?table=invoiceimage&cid=${soeConfig.actorCompanyId}&nr=file&type=${SoeDataStorageRecordType.InvoicePdf}&id=${this.file.imageId}`;
        else
            return `/ajax/downloadTextFile.aspx?table=datastoragerecord&id=${this.file.imageId || this.file.id || this.file.messageAttachmentId}`;
    }

    private showImage() {
        const options: angular.ui.bootstrap.IModalSettings = {
            template: `<div class="messagebox">
                                <div class="modal-header">
                                    <button type="button" class="close" data-ng-click="ctrl.cancel()">&times;</button>
                                    <a class="print pull-right" ng-click="ctrl.print();"><i class="fal fa-print"></i></a>
                                    <h6 class="modal-title">{{ctrl.description}}</h6>
                                </div>
                                <div class="modal-body" style="text-align:center">
                                    <img ng-if="ctrl.image" style="max-width: 100%;" data-ng-src="data:image/jpg;base64,{{ctrl.image.image}}" />
                                </div>
                            </div>`,
            controller: ImageController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                imageId: () => this.file.imageId || this.file.messageAttachmentId,
                description: () => this.file.description
            }
        }
        this.$uibModal.open(options);
    }
}

class ImageController {
    public image: any;
    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private coreService: ICoreService, private imageId: number, public description: string) {
        coreService.getImage(imageId).then(image => this.image = image);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public print() {
        var printWindow = window.open('', this.image.description);
        printWindow.document.write('<html><head><title>' + this.image.description + '</title>');
        printWindow.document.write('</head><body ><img src=\'data:image/jpg;base64,');
        printWindow.document.write(this.image.image);
        printWindow.document.write('\' /></body></html>');
        printWindow.document.close();
        (<any>printWindow).print();
    }
}