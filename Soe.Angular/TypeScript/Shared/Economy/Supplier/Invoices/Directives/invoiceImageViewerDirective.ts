import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { Guid } from "../../../../../Util/StringUtility";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { TermGroup_EDISourceType, SoeDataStorageRecordType, SoeReportTemplateType, InvoiceAttachmentSourceType } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";

//@ngInject
export function InvoiceImageViewerDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        restrict: "E",
        templateUrl: urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/invoiceImageViewer.html'),
        replace: true,
        scope: {
            guid: "=",
            ediType: "=",
            ediEntryId: "=",
            scanningEntryId: "=",
            invoiceImage: "=",
            isReadonly: "=",
            supplierInvoiceId: "=",
            invoiceNr: "=",
            onDelete: "&",
            showDelete: "=?",
            scale: "=?"
        },
        bindToController: true,
        controllerAs: "ctrl",
        controller: InvoiceImageViewerController
    }
}

export class InvoiceImageViewerController {
    private guid: Guid;
    private ediType: TermGroup_EDISourceType;
    private ediEntryId: number;
    private scanningEntryId: number;
    private invoiceImage: any;
    private isReadonly: boolean;
    private image: any;
    private hasMultiplePages: boolean;
    private currentImageIndex: number;
    private pdf: any;
    private failed = false;
    private id: number;
    private supplierInvoiceId: number;
    private invoiceNr: string;
    private imageFormatType: any;
    private showDelete: boolean;
    private downloadUrl: string;
    
    //@ngInject
    constructor(
        $scope: ng.IScope,
        private messagingService: IMessagingService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $uibModal) {

        let watchUnRegisterCallbacks = [];
        watchUnRegisterCallbacks.push(
            $scope.$watch(() => this.invoiceImage, () => {
                this.imageLoaded(this.invoiceImage);
            })
        );

        watchUnRegisterCallbacks.push(
            $scope.$watch(() => this.supplierInvoiceId, () => {
                if (this.supplierInvoiceId) {
                    this.downloadUrl = this.getUrl();
                }
            })
        );
        /*
        var invoiceWatch = $scope.$watch(() => this.ediEntryId, () => {
            if (this.ediEntryId && (!this.invoiceId || this.invoiceId === 0)) {
                this.getImageByEdiEntryId();
            }
        });
        var fileWatch = $scope.$watch(() => this.uploadedFile, () => {
            if (this.uploadedFile) {
                this.id = this.uploadedFile.id;
                this.getImageByFileId();
            } else {
                this.id = 0;
                this.pdf = null;
                this.image = null;
            }
        });*/
        $scope.$on("$destroy", () => {
            console.log("destroy");
            _.forEach(watchUnRegisterCallbacks, (callBack) => {
                callBack();
            });
        });
    }

    private imageLoaded(result) {
        this.pdf = null;
        this.image = null;
        this.failed = false;
        if (!result)
            return;

        if (this.invoiceImage && this.invoiceImage.sourceType === InvoiceAttachmentSourceType.Edi)
            this.showDelete = false;

        this.id = result.id;
        this.imageFormatType = result.imageFormatType;
        if (result.imageFormatType === SoeDataStorageRecordType.InvoicePdf) {
            this.pdf = result.image;
        } else if (result.imageFormatType === SoeDataStorageRecordType.InvoiceBitmap) {
            this.image = result;
            if (result.images && result.images.length > 1) {
                this.hasMultiplePages = true;
                this.currentImageIndex = 0;
            }

        } else {
            const keys: string[] = [
                "core.warning",
                "common.uploadfailedinvalidfileformat"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["economy.supplier.invoice.interimaccountmissing.title"], terms["common.uploadfailedinvalidfileformat"] + "\r(*.pdf, *.jpg, *.bmp)", SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                this.failed = true;
            });
        }
    }

    private getUrl() {
        if (this.supplierInvoiceId && this.supplierInvoiceId > 0)
            return `/ajax/downloadTextFile.aspx?table=invoiceimage&id=${this.supplierInvoiceId}&cid=${CoreUtility.actorCompanyId}&nr=${this.invoiceNr}&type=${this.imageFormatType}`;
        else
            return `/ajax/downloadTextFile.aspx?table=datastoragerecord&id=${this.id}`;
    }

    private showImage() {
        const options: angular.ui.bootstrap.IModalSettings = {
            template: `<div class="messagebox">
                                <div class="modal-header">
                                    <button type="button" class="close" data-ng-click="ctrl.cancel()">&times;</button>
                                    <a class="print pull-right" ng-click="ctrl.print();"><i class="fal fa-print"></i></a>
                                    <h6 class="modal-title">{{ctrl.image.description || ''}}</h6>
                                </div>
                                <div class="modal-body" style="text-align:center">
                                    <img ng-if="ctrl.image" style="max-width: 100%;" data-ng-src="data:image/jpg;base64,{{ctrl.image.image}}" />
                                </div>
                            </div>`,
            controller: ImageController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                image: () => this.image,
                ediType: () => this.ediType,
                ediEntryId: () => this.ediEntryId,
                scanningEntryId: () => this.scanningEntryId
            }
        }
        this.$uibModal.open(options);
    }

    private deleteInvoiceImage() {
        this.messagingService.publish(Constants.EVENT_DELETE_SUPPLIER_INVOICE_IMAGE, { guid: this.guid });
        this.image = null;
        this.pdf = null;
    }

    private navigateFirst() {
        if (this.currentImageIndex != 0) {
            this.image.image = this.image.images[0];
            this.currentImageIndex = 0;
        }
    }

    private navigatePrevious() {
        if (this.currentImageIndex != 0) {
            this.currentImageIndex = this.currentImageIndex - 1;
            this.image.image = this.image.images[this.currentImageIndex];
        }
    }

    private navigateNext() {
        var lastIndex = this.image.images.length - 1;
        if (this.currentImageIndex != lastIndex) {
            this.currentImageIndex = this.currentImageIndex + 1;
            this.image.image = this.image.images[this.currentImageIndex];
        }
    }

    private navigateLast() {
        var lastIndex = this.image.images.length - 1;
        if (this.currentImageIndex != lastIndex) {
            this.image.image = this.image.images[lastIndex];
            this.currentImageIndex = lastIndex;
        }
    }
}

class ImageController {
    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private image: any, private ediType: number, private ediEntryId: number, private scanningEntryId: number) {
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public print() {
        if (this.ediType == TermGroup_EDISourceType.Scanning) {
            var scannedPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.ReadSoftScanningSupplierInvoice + "&scanningentryid=" + this.scanningEntryId + "&c=" + CoreUtility.actorCompanyId;
            window.open(scannedPdfReportUrl, '_blank');
        } else if (this.ediType == TermGroup_EDISourceType.EDI) {
            var ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SymbrioEdiSupplierInvoice + "&edientryid=" + this.ediEntryId;
            window.open(ediPdfReportUrl, '_blank');
        } else {
            var printWindow = window.open('', this.image.description || '');
            printWindow.document.write('<html><head><title>' + (this.image.description || '') + '</title>');
            printWindow.document.write('</head><body ><img src=\'data:image/jpg;base64,');
            printWindow.document.write(this.image.image);
            printWindow.document.write('\' /></body></html>');
            printWindow.document.close();
            (<any>printWindow).print();
        }
    }
}