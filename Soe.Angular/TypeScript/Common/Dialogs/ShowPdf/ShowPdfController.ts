import { Guid } from "../../../Util/StringUtility";

declare var pdfjsLib;

export class ShowPdfController {
    private pdfDoc;
    private pageNum = 1;
    private numberOfPages: number;
    private pageRendering = false;
    private pageNumPending;
    private scale = 1.40;
    private viewer: Element;
    private canvasHeight: number;
    private guid = Guid.newGuid();
    private viewIsSetup = false;

    private scales: any[];

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $scope: ng.IScope,
        //private $element,
        private pdf: any,
        private storageRecordId: any,
        private invoiceId: number,
        private invoiceNr: any,
        private companyId: number,
    ) {
        this.$scope.$watch(() => this.pdf, () => {
            if (!this.viewIsSetup) {
                const element = document.getElementsByClassName("pdf-holder temp")[0];
                element.classList.remove("temp");
                element.id = this.guid;
                this.setupViewer();
                this.viewIsSetup = true;
            }

            if (this.pdf) {
                this.setup();
                this.pdfLoaded(this.pdf);
            }
        })

        pdfjsLib.GlobalWorkerOptions.workerSrc = '/cssjs/pdfjs/pdf.worker.min.js';
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
            });
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
                this.onScroll();
                this.$scope.$apply();
            }

            setTimeout(() => {
                scrollDebounce = true;
            }, 50)
        })

    }

    private onScroll() {
        //We update the page number when the second page fills the viewer more than the first.
        const scrollHeight = this.viewer.scrollHeight;
        const heightOffset = this.viewer.clientHeight * 0.5;
        const scrollTop = this.viewer.scrollTop + heightOffset;
        this.pageNum = Math.floor(this.numberOfPages * scrollTop / scrollHeight) + 1;
    }


    private convertDataToBinary(dataURI): Uint8Array {
        var raw = window.atob(dataURI);
        var rawLength = raw.length;
        var array = new Uint8Array(new ArrayBuffer(rawLength));

        for (var i = 0; i < rawLength; i++) {
            array[i] = raw.charCodeAt(i);
        }
        return array;
    }

    private renderPages() {
        const numPages = this.pdfDoc.numPages;
        this.pageRendering = true;
        let outputScale = 1;
        if (window.devicePixelRatio > 0) {
            outputScale = window.devicePixelRatio;
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

    buttonCancelClick() {
        this.close();
    }

    close() {
        this.$uibModalInstance.dismiss('cancel');
    }


    private renderOnCanvas(pageNumber: number, canvas: HTMLCanvasElement) {
        //this.pageRendering = true;
        let outputScale = 1;
        if (window.devicePixelRatio > 0) {
            outputScale = window.devicePixelRatio;
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


    private scrollToPage(pageNum) {
        const scrollHeight = this.viewer.scrollHeight;
        const factor = scrollHeight / this.numberOfPages;
        this.viewer.scrollTop = 10 + (pageNum - 1) * factor;
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

    public getUrl() {
        if (this.invoiceId && this.invoiceId > 0)
            return `/ajax/downloadTextFile.aspx?table=invoiceimage&id=${this.invoiceId}&cid=${this.companyId}&nr=${this.invoiceNr}&type=52`;
        else
            return `/ajax/downloadTextFile.aspx?table=datastoragerecord&id=${this.storageRecordId}`;
    }
}