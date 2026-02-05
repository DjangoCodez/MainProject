import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { CoreUtility } from "../../Util/CoreUtility";
import { ShowPdfController } from "../../Common/Dialogs/ShowPdf/ShowPdfController";
import { SettingMainType, SoeDataStorageRecordType, UserSettingType } from "../../Util/CommonEnumerations";
import { ICoreService } from "../../Core/Services/CoreService";
import { SettingsUtility } from "../../Util/SettingsUtility";
import { Guid } from "../../Util/StringUtility";

declare var pdfjsLib;

export class PdfViewerController {
    public pdf: number;
    public id: number;
    private supplierInvoiceId: number;
    private ediEntryId: number;
    private invoiceNr: string;
    private alignRight: boolean;
    private hideDownload: boolean;
    private showDelete: boolean;
    private onDelete: any;
    private scale: number;

    private pdfDoc;
    private pageNum = 1;
    private numberOfPages: number;
    private pageRendering = false;
    private pageNumPending;
    private viewIsSetup = false;

    private scales: any[];
    private viewer: Element;
    private canvasHeight: number;
    private guid = Guid.newGuid();

    private enhanced = true;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private $element,
        private $uibModal,
        private urlHelperService: IUrlHelperService) {

        this.createScales();
        this.loadEnchancedSetting();

        const pdfWatch = this.$scope.$watch(() => this.pdf, () => {
            if (!this.viewIsSetup) {
                const element = document.getElementsByClassName("pdf-holder temp")[0];
                element.classList.remove("temp");
                element.id = this.guid;
                this.setupViewer();
                this.viewIsSetup = true;
            }

            if (this.pdf) {
                this.viewer.scrollTop = 0;
                this.setup();
                this.pdfLoaded(this.pdf);
            }
        })

        $scope.$on("$destroy", () => {
            pdfWatch();
        });

        pdfjsLib.GlobalWorkerOptions.workerSrc = '/cssjs/pdfjs/pdf.worker.min.js';
    }

    private loadEnchancedSetting() {
        const settingTypes: number[] = [UserSettingType.SupplierInvoiceSowPDFEnhanced];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            const setting = SettingsUtility.getBoolUserSetting(x, UserSettingType.SupplierInvoiceSowPDFEnhanced, this.enhanced);
            if (setting != this.enhanced) {
                this.enhanced = setting;
            }
        });
    }

    private enhance() {
        this.enhanced = !this.enhanced;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.SupplierInvoiceSowPDFEnhanced, this.enhanced);
        this.updatePage();
    }

    private createScales() {
        const min = 0.25;
        const max = 4;
        const step = 0.25;
        this.scales = [];

        let current = min;
        while (current <= max) {
            this.scales.push({
                value: current,
                name: (current * 100) + '%'
            });

            current += step;
        }
    }
    private pdfLoaded(pdf) {

        if (this.pdfDoc) {
            this.pdfDoc.loadingTask.destroy();
        }

        pdfjsLib.getDocument(this.convertDataToBinary(pdf)).promise.then((pdfDoc_) => {
            // Since this is an async call, we need to trigger Angular digest loop again, once finished,
            // otherwise the pdf will not show up (at least not in IE).
            this.$scope.$apply(() => {
                this.pdfDoc = pdfDoc_;
                this.numberOfPages = this.pdfDoc.numPages;

                // Initial/first page rendering
                this.renderPages();
            })
        }, (error) => {
            console.log("PDFJS Error", error);
        });
    }
    private setup() {
        this.pageNum = 1;
        this.scale ||= 1;
        pdfjsLib.disableWorker = true;
    }

    private setupViewer() {
        this.viewer = document.getElementById(this.guid);

        let scrollDebounce = true;
        this.viewer.addEventListener("scroll", () => {
            if (scrollDebounce) {
                scrollDebounce = false;
                this.updatePageNumber();
                this.$scope.$apply();
            }

            setTimeout(() => {
                scrollDebounce = true;
            }, 50)
        })
    }

    private updatePageNumber() {
        //We update the page number when the second page fills the viewer more than the first.
        const scrollHeight = this.viewer.scrollHeight;
        const heightOffset = this.viewer.clientHeight * 0.5;
        const scrollTop = this.viewer.scrollTop + heightOffset
        this.pageNum = Math.floor(this.numberOfPages * scrollTop / scrollHeight) + 1;
    }

    private convertDataToBinary(dataURI): Uint8Array {
        const raw = window.atob(dataURI);
        const rawLength = raw.length;
        const array = new Uint8Array(new ArrayBuffer(rawLength));

        for (let i = 0; i < rawLength; i++) {
            array[i] = raw.charCodeAt(i);
        }
        return array;
    }

    private zoomIn() {
        this.scale += 0.25;

        this.updatePage();
    }

    private zoomOut() {
        if (this.scale > 0.25)
            this.scale -= 0.25;

        this.updatePage();
    }

    private updatePage() {
        this.renderPages();
    }

    private renderPages() {
        const numPages = this.pdfDoc.numPages;
        this.pageRendering = true;
        let outputScale = 1;
        if (window.devicePixelRatio > 0) {
            outputScale = window.devicePixelRatio;
        }

        if (this.enhanced) {
            outputScale = Math.max(window.devicePixelRatio, 1.8);
        }

        this.viewer.innerHTML = "";

        const promises = [];
        for (let page = 1; page <= numPages; page++) {
            const canvas = document.createElement("canvas");
            canvas.className = "pdf-canvas";
            this.viewer.appendChild(canvas);
            promises.push(this.renderOnCanvas(page, canvas));
        }
        Promise.all(promises).then(() => {
            this.pageRendering = false;
        })
    }

    private renderOnCanvas(pageNumber: number, canvas: HTMLCanvasElement) {
        //this.pageRendering = true;
        let outputScale = 1;
        if (window.devicePixelRatio > 0) {
            outputScale = window.devicePixelRatio;
        }

        if (this.enhanced) {
            outputScale = Math.max(window.devicePixelRatio, 1.8);
        }
        this.pdfDoc.getPage(pageNumber).then((pdfPage) => {
            const viewport = pdfPage.getViewport({ scale: this.scale });

            canvas.height = Math.floor(viewport.height * outputScale);
            canvas.width = Math.floor(viewport.width * outputScale);

            canvas.style.width = Math.floor(viewport.width) + "px";
            this.canvasHeight = Math.floor(viewport.height);
            canvas.style.height = this.canvasHeight + "px";

            const transform = [outputScale, 0, 0, outputScale, 0, 0]

            // Render PDF page into canvas context
            const renderContext = {
                canvasContext: canvas.getContext("2d"),
                viewport: viewport,
                transform: transform,
            };
            return pdfPage.render(renderContext);
        });
    }

    private onPrevPage() {
        if (this.pageNum <= 1) {
            return;
        }
        this.pageNum--;
        this.scrollToPage(this.pageNum);
    }

    private onNextPage() {
        if (this.pageNum >= this.pdfDoc.numPages) {
            return;
        }
        this.pageNum++;
        this.scrollToPage(this.pageNum);
    }

    private onFirstPage() {
        this.pageNum = 1;
        this.scrollToPage(this.pageNum);
    }

    private onLastPage() {
        this.pageNum = this.pdfDoc.numPages;
        this.scrollToPage(this.pageNum);
    }

    private scrollToPage(pageNum) {
        const scrollHeight = this.viewer.scrollHeight;
        const factor = scrollHeight / this.numberOfPages;
        this.viewer.scrollTop = 10 + (pageNum - 1) * factor;
    }

    public getUrl() {
        if (this.supplierInvoiceId) {
            return `/ajax/downloadTextFile.aspx?table=invoiceimage&id=${this.supplierInvoiceId}&cid=${CoreUtility.actorCompanyId}&nr=${this.invoiceNr}&type=${SoeDataStorageRecordType.InvoicePdf}`;
        }
        else if (this.ediEntryId) {
            return `/ajax/downloadTextFile.aspx?table=invoiceimage&useedi=true&id=${this.ediEntryId}&cid=${CoreUtility.actorCompanyId}&nr=${this.invoiceNr}&type=${SoeDataStorageRecordType.InvoicePdf}`;
        }
        else
            return `/ajax/downloadTextFile.aspx?table=datastoragerecord&id=${this.id}`;
    }

    public openDialog() {
        this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/ShowPdf/ShowPdf.html"),
            controller: ShowPdfController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                pdf: () => { return this.pdf },
                storageRecordId: () => { return undefined; },
                invoiceId: () => { return this.supplierInvoiceId; },
                invoiceNr: () => { return undefined; },
                companyId: () => { return soeConfig.actorCompanyId; },
            }
        });
    }
}

//@ngInject
export function PdfViewerDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        restrict: "E",
        templateUrl: urlHelperService.getGlobalUrl('Common/PdfViewer/pdfviewer.html'),
        replace: true,
        scope: {
            pdf: "=",
            id: "=",
            supplierInvoiceId: "=",
            ediEntryId: "=",
            invoiceNr: "=",
            alignRight: "@",
            hideDownload: "@",
            onDelete: "&",
            showDelete: "=?",
            scale: "=?"
        },
        bindToController: true,
        controllerAs: "ctrl",
        controller: PdfViewerController
    }
}