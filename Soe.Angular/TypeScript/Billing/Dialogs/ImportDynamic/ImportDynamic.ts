import { ISmallGenericType, IActionResult, IImportOptionsDTO, IImportDynamicResultDTO, IImportDynamicDTO, IImportFieldDTO } from "../../../Scripts/TypeLite.Net4";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Constants } from "../../../Util/Constants";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { SettingDataType, TermGroup } from "../../../Util/CommonEnumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { FieldOptionsController } from "./FieldOptions";

enum Tab {
    FileUpload = 1,
    MatchFields = 2,
    Control = 3,
    Finish = 4,
}

export class ImportDynamicController {
    private currentTab: number = 1;

    private langId: number;
    private terms: { [index: string]: string; };

    private fileType: number;
    private fileTypes: ISmallGenericType[] = [];

    private importFile: any;
    private importFileName: string;

    private rows: string[][] = [];
    private parsedRows: any[] = [];
    private referenceRow: string[] = [];
    private rowPointer: number = 0;
    private showPeek: boolean = false;
    showPeekError: boolean = false;

    private progress: IProgressHandler;

    private hoveringDropTarget: HTMLElement;

    private fields: IImportFieldDTO[] = [];
    private options: IImportOptionsDTO;
    private result: IImportDynamicResultDTO;

    private gridHandlerPeek: EmbeddedGridController;
    private gridHandlerPreview: EmbeddedGridController;
    private gridHandlerResult: EmbeddedGridController;

    private callback: (rows: any[], options: IImportOptionsDTO) => ng.IPromise<IImportDynamicResultDTO>;

    modalInstance: any;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        progressHandlerFactory: IProgressHandlerFactory,
        private $timeout: ng.ITimeoutService,
        gridHandlerFactory: IGridHandlerFactory,
        private $scope: ng.IScope,
        private importDynamicDTO: IImportDynamicDTO,
        callback: (rows: any[], options: IImportOptionsDTO) => ng.IPromise<IImportDynamicResultDTO>,
        private $q: ng.IQService,
        $uibModal) {
        this.callback = callback;
        this.modalInstance = $uibModal;


        this.fields = importDynamicDTO.fields;
        this.fields.map(f => f.index = -1)
        this.options = importDynamicDTO.options;

        this.progress = progressHandlerFactory.create()
        this.$q.all([
            this.loadTerms(),
            this.loadFileTypes(),
        ]);

        this.gridHandlerPeek = new EmbeddedGridController(gridHandlerFactory, "billing.purchase.importdynamic.peek");
        this.gridHandlerPeek.gridAg.options.enableRowSelection = false;

        this.gridHandlerPreview = new EmbeddedGridController(gridHandlerFactory, "billing.purchase.importdynamic.preview");
        this.gridHandlerPreview.gridAg.options.enableRowSelection = false;

        this.gridHandlerResult = new EmbeddedGridController(gridHandlerFactory, "billing.purchase.importdynamic.result");
        this.gridHandlerResult.gridAg.options.enableRowSelection = false;
        this.setupPreviewGridColumns();
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "billing.productrows.productnr",
            "billing.productrows.text",
            "billing.order.syswholeseller",
            "billing.productrows.dialogs.changingwholeseller",
            "billing.productrows.dialogs.failedwholesellerchange",
            "billing.stock.stocksaldo.importstockbalance",
            "common.message",
            "common.row",
            "common.importdynamic.cellmissingvalue",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.setupResultGridColumns();
        });
    }

    private setupPreviewGridColumns() {
        this.fields.forEach(f => {
            switch (f.dataType) {
                case SettingDataType.String:
                    this.gridHandlerPreview.gridAg.addColumnText(f.field, f.label, null);
                    break;
                case SettingDataType.Integer:
                case SettingDataType.Decimal:
                    this.gridHandlerPreview.gridAg.addColumnNumber(f.field, f.label, null);
                    break;
                case SettingDataType.Date:
                    this.gridHandlerPreview.gridAg.addColumnDateTime(f.field, f.label, null);
                    break;
            }
        })
        this.gridHandlerPreview.gridAg.options.setMinRowsToShow(20);
        this.gridHandlerPreview.gridAg.finalizeInitGrid("billing.purchase.importdynamic.preview", true);
    }

    private setupResultGridColumns() {
        this.gridHandlerResult.gridAg.addColumnNumber("rowNr", this.terms["common.row"], null);
        this.gridHandlerResult.gridAg.addColumnText("message", this.terms["common.message"], null);
        this.gridHandlerResult.gridAg.options.setMinRowsToShow(20);
        this.gridHandlerResult.gridAg.finalizeInitGrid("billing.purchase.importdynamic.result", true);
    }

    private setupPeekGridColumns(cols: number, firstRow: string[]) {
        this.gridHandlerPeek.gridAg.options.resetColumnDefs();
        for (let index = 0; index < cols; index++) {
            const indexS = String(index + 1)
            let name = firstRow ? firstRow[index] : indexS;
            this.gridHandlerPeek.gridAg.addColumnText(name, "", null, false, { minWidth: 100 });
        }

        // this.gridHandlerPeek.gridAg.options.setAutoHeight(true);
        this.gridHandlerPeek.gridAg.finalizeInitGrid("billing.purchase.importdynamic.peek", false);
    }


    private loadFileTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ImportDynamicFileTypes, false, true, true).then(terms => {
            this.fileTypes = terms;
        })
    }

    private fileTypeChanged() {
        this.importFileName = "";
        this.rows = []
        this.gridHandlerPeek.gridAg.setData([]);
    }

    private nextTab() {
        this.setTab(this.currentTab + 1);
    }

    private previousTab() {
        this.setTab(this.currentTab - 1);
    }

    private previousRow() {
        if (this.rowPointer === 0) {
            this.rowPointer = this.rows.length - 1;
        } else {
            this.rowPointer--;
        }
        this.referenceRow = this.rows[this.rowPointer];
    }

    private nextRow() {
        if (this.rowPointer === this.rows.length - 1) {
            this.rowPointer = 0;
        } else {
            this.rowPointer++;
        }
        this.referenceRow = this.rows[this.rowPointer];
    }

    private getReferenceValue(idx: number) {
        if (idx == -1) return "";
        return this.referenceRow[idx] === "" ? this.terms["common.importdynamic.cellmissingvalue"] : this.referenceRow[idx];
    }

    private columnIndexIsLinkedToField(idx: number) {
        return !!this.fields.find(f => f.index == idx);
    }

    private setTab(tab: Tab) {
        if (tab < Tab.FileUpload) {
            this.currentTab = Tab.FileUpload;
        }
        else if (tab > Tab.Finish) {
            this.currentTab = Tab.Finish;
        } else {
            this.currentTab = tab;
        }
        switch (this.currentTab) {
            case Tab.FileUpload:
                break;
            case Tab.MatchFields:
                this.enableDnD();
                break;
            case Tab.Control:
                this.coreService.parseRows({ fields: this.fields, options: this.options, data: this.rows }).then(rows => {
                    this.parsedRows = rows;
                    this.gridHandlerPreview.gridAg.setData(this.parsedRows);
                })
                break;
            case Tab.Finish:
                const performImport = () => this.callback(this.parsedRows, this.options).then(data => {
                    this.gridHandlerResult.gridAg.setData(data.logs);
                    this.result = data;
                })
                this.progress.startLoadingProgress([performImport])
                break;
        }
    }

    private tabDisabled(tab: Tab = this.currentTab) {
        switch (tab) {
            case Tab.FileUpload:
                return !(this.fileType && this.rows && this.rows.length > 0)
            case Tab.MatchFields:
                const field = this.fields.find(f => f.isRequired && f.index == -1);
                return !!field;
            case Tab.Control:
                return false;
            case Tab.Finish:
                return false;
            default:
                return true;
        }
    }

    private enableDnD() {
        this.$timeout(() => {
            $(".draggable").draggable({
                revert: true,
                revertDuration: 0,
                helper: 'clone',
                appendTo: ".dnd-zone",
                stop: (e: any, ui) => this.processDroppableDrop(e, this.hoveringDropTarget, angular.extend(ui, { draggable: $(e.target) })),
                drag: this.onDraggableDrag.bind(this)
            });
        }, 0);
    }

    private processDroppableDrop(e: MouseEvent, target: HTMLElement, ui: any) {
        if (target && target.id && ui && ui.draggable && ui.draggable) {
            let source = ui.draggable[0];
            let field = this.fields.find(f => f.field === target.id);
            field.index = Number(source.id);
            this.$scope.$applyAsync();
        }
    }

    private getRoot() {
        return document.getElementById('drop-zone');
    }

    private onDraggableDrag(e: MouseEvent, ui: JQueryUI.DraggableEventUIParams) {
        const root = this.getRoot();

        if (this.hoveringDropTarget)
            $(this.hoveringDropTarget).removeClass('drop-target');

        this.hoveringDropTarget = null;

        this.hoveringDropTarget =
            this.querySelectElementMouseInBounds(root, '.drop-zone', e)
    }

    private isWithinBounds(element: HTMLElement, e: MouseEvent): boolean {
        const bounds = element.getBoundingClientRect();
        const x = e.clientX, y = e.clientY;

        return bounds.left <= x && x <= bounds.right &&
            bounds.top <= y && y <= bounds.bottom;
    }


    private querySelectElementMouseInBounds(root: HTMLElement, selector: string, e: MouseEvent): HTMLElement {
        const matches = root.querySelectorAll(selector);

        for (let i = 0; i < matches.length; i++) {
            const curr = <HTMLElement>matches[i];
            if (this.isWithinBounds(curr, e)) return curr;
        }

        return null;
    }

    private clearField(field) {
        field.index = -1;
    }

    private openFieldSettings(field) {
        const tempField = { ...field }
        let uniqueValues = [];
        if (field.index != -1 && field.enableValueMapping) {
            let allValues = this.rows.map(r => r[field.index]);
            // if (this.options.skipFirstRow) allValues.shift();
            uniqueValues = allValues.filter((item, i, ar) => {
                if (this.options.skipFirstRow && i == 0) {
                    return false;
                }
                return ar.indexOf(item) === i
            });
        }
        if (field && field.index !== null && field.index !== undefined) {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Billing/Dialogs/ImportDynamic/FieldOptions.html"),
                controller: FieldOptionsController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    field: () => tempField,
                    uniqueValues: () => uniqueValues,
                }
            });

            modal.result.then((value: any) => {
                if (value) {
                    //only apply changes if actually clicked "ok"
                    const i = this.fields.findIndex(f => f.field === field.field);
                    this.fields[i] = value;
                }
            });
        }
    }

    private save() {
    }

    private close() {
        this.$uibModalInstance.close();
    }
    private okValid() {
    }

    private addFile() {
        this.translationService.translate("core.fileupload.choosefiletoimport").then((term) => {
            var url = CoreUtility.apiPrefix + Constants.WEBAPI_CORE_IMPORTDYNAMIC_GETFILECONTENT + (this.fileType || 1);
            var modal = this.notificationService.showFileUpload(url, term, true, true, false);
            modal.result.then(res => {
                let result: IActionResult = res.result;
                if (result.success) {
                    this.rows = result.value?.$values || [];
                    this.importFileName = result.value2;
                    if (this.rows.length > 0) {
                        this.referenceRow = this.rows[0];
                        this.setPeekValues(this.options.skipFirstRow);
                        this.showPeekError = false;
                    } else {
                        this.gridHandlerPeek.gridAg.setData([]);
                        this.showPeek = false;
                        this.showPeekError = true;
                    }
                } else {
                    //this.failedWork(result.errorMessage);
                }
            }, error => {
                //this.failedWork(error.message)
            });
        });
    }

    private setPeekValues(firstRowIsHeader: boolean) {
        const data = this.rows;
        if (!data || data.length == 0) return;
        const firstRow = data[0];

        let peekData = data.slice(0, 5);
        let values = [];
        peekData.forEach(r => {
            let row = {};
            r.forEach((c, i) => {
                row[String(i)] = c;
            })
            values.push(row);
        })
        this.setupPeekGridColumns(this.referenceRow.length, null)
        this.gridHandlerPeek.gridAg.setData(values);
        this.showPeek = true;
    }
}