import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup, ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { SoeGridOptionsEvent, IconLibrary } from "../../../Util/Enumerations";
import { Feature, SoeEntityImageType, SoeDataStorageRecordType, ImageFormatType, InvoiceAttachmentSourceType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Guid, StringUtility } from "../../../Util/StringUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { ImageDTO } from "../../Models/ImageDTO";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { ShowImageController } from "../../Dialogs/ShowImage/ShowImageController";

class DocumentsDirectiveController extends GridControllerBase2Ag implements ICompositionGridController {
    guid: Guid;
    readOnly: boolean;
    permission: any;
    parentFeature: any;
    loading: boolean;
    private files: ImageDTO[];

    // Lookups
    terms: { [index: string]: string; };
    invoiceEditPermission: boolean;
    invoiceBillingTypes: any[];

    // Rows
    images: ImageDTO[];

    // Flags
    progressBusy: boolean = true;
    resetGridData: boolean = false;
    transferAll: boolean;
    distributeAll: boolean;

    // ToolBar
    protected buttonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(
        private $window,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Common.Directives.Documents", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())

        this.onInit({});
    }

    onInit(parameters: any) {
        this.parameters = parameters;

        this.flowHandler.start([{ feature: this.permission, loadReadPermissions: true, loadModifyPermissions: true }]);

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.files.length, (newVal, oldVal) => {
            if (newVal) {
                this.handleRows();
                this.resetGridData = false;
            }
        });
        this.$scope.$watch(() => this.resetGridData, (newVal, oldVal) => {
            if (newVal) {
                this.handleRows();
                this.resetGridData = false;
            }
        });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[this.permission].readPermission;
        this.modifyPermission = response[this.permission].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("core.fileupload.choosefilestoupload", "core.fileupload.choosefilestoupload", IconLibrary.FontAwesome, "fa-plus", () => { this.uploadItem(); }, null, () => { return this.readOnly; })));
    }

    public setupGrid(): void {
        var translationKeys: string[] = [
            "core.delete",
            "core.filename",
            "common.description",
            "common.fileextension",
            "common.documents.includetype",
            "common.sent",
            "common.documents.includewhensent",
            "common.download",
            "common.documents.includeinvoice",
            "common.documents.includeorderinvoice",
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.terms = terms;

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.options.setMinRowsToShow(10);

            this.gridAg.addColumnText("fileName", this.terms["core.filename"], null, true, { editable: (row) => row.canDelete && !this.readOnly });
            this.gridAg.addColumnText("description", this.terms["common.description"], null, true, { editable: (row) => row.canDelete && !this.readOnly, onChanged: () => this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid }) });
            this.gridAg.addColumnSelect("fileFormat", this.terms["common.fileextension"], null, { enableHiding: true, displayField: "fileFormat", populateFilterFromGrid: true, selectOptions: [] });
            this.gridAg.addColumnSelect("connectedTypeName", this.terms["common.documents.includetype"], null, { enableHiding: true, displayField: "connectedTypeName", populateFilterFromGrid: true, selectOptions: [] });
            this.gridAg.addColumnDateTime("lastSentDate", this.terms["common.sent"], null);
            this.gridAg.addColumnIcon("icon", "...", null, { toolTip: this.terms["common.download"], onClick: this.show.bind(this) });
            this.gridAg.addColumnBoolEx("includeWhenDistributed", this.terms["common.documents.includewhensent"], 100, { enableEdit: true, onChanged: this.updateIncludeItem.bind(this), toolTip: this.terms["common.documents.includewhensent"] });

            var includeTerm = this.getInclundeOnTransferTerm();
            if (includeTerm)
                this.gridAg.addColumnBoolEx("includeWhenTransfered", includeTerm, 100, { enableEdit: true, onChanged: this.updateIncludeItem.bind(this), toolTip: includeTerm });

            this.gridAg.addColumnIcon(null, null, null, { toolTip: this.terms["core.delete"], icon: "fal fa-times iconDelete", showIcon: (row) => row.canDelete, onClick: this.deleteFile.bind(this), suppressFilter: true });

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.show(row); }));
            events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("billing.project.central.supplierinvoices", true);

            if (this.files.length > 0) {
                this.$timeout(() => {
                    this.handleRows();
                });
            }
        });
    }

    private afterCellEdit(row: ImageDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'fileName':
                if (StringUtility.getFileExtension(newValue) !== StringUtility.getFileExtension(oldValue)) {
                    row.fileName = row.fileName + "." + row.extension;
                    this.setGridData();
                }
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid });
                break;
            case 'description':
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid });
                break;
        }
    }

    private getInclundeOnTransferTerm() {
        switch (this.parentFeature) {
            case Feature.Billing_Order_Status:
                return this.terms["common.documents.includeinvoice"];
            case Feature.Billing_Offer_Status:
            case Feature.Billing_Contract_Status:
                return this.terms["common.documents.includeorderinvoice"];
            default:
                return undefined;
        }
    }

    private deleteFile(row: ImageDTO) {
        if (row.isAdded) {
            _.pull(this.files, row);
        }
        else {
            row['isDeleted'] = true;
            row['isModified'] = true;
        }
        this.setGridData();

        this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid });
    }

    private handleRows() {
        _.forEach(this.files, y => {
            y.setFileFormat();
            y.setIcon();
        });

        this.setGridData();
        this.progressBusy = false;
    }

    private setGridData() {
        this.gridAg.setData(_.filter(this.files, (row) => !row['isDeleted']));
    }

    private show(file: ImageDTO) {
        if (file.isImage())
            this.showImage(file);
        else if (file.type && file.type === SoeEntityImageType.SupplierInvoice && file.formatType && file.formatType === ImageFormatType.PDF && file.fileName === "invoiceimage")
            HtmlUtility.openInSameTab(this.$window, `/ajax/downloadTextFile.aspx?table=invoiceimage&cid=${soeConfig.actorCompanyId}&nr=file&type=${SoeDataStorageRecordType.InvoicePdf}&id=${file.imageId}&useedi=${file.sourceType == InvoiceAttachmentSourceType.Edi}`);
        else
            HtmlUtility.openInSameTab(this.$window, `/ajax/downloadTextFile.aspx?table=datastoragerecord&id=${file.imageId}&cid=${soeConfig.actorCompanyId}&useedi=${file.sourceType == InvoiceAttachmentSourceType.Edi}`);
    }

    private showImage(file: any) {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/ShowImage/ShowImage.html"),
            controller: ShowImageController,
            controllerAs: "ctrl",
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                imageId: () => file.imageId ? file.imageId : file.id,
                description: () => file.description
            }
        }
        this.$uibModal.open(options);
    }

    public uploadItem() {

    }

    public updateIncludeItem(file: any) {
        file.isModified = true;
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid });
    }

    private reloadData() {
        this.progressBusy = true;
        this.images = [];
        this.gridAg.setData(null);
    }
}

export class DocumentsDirectiveControllerFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('Documents', 'Documents.html'),
            scope: {
                guid: "=",
                readOnly: "=",
                files: "=",
                permission: "=",
                parentFeature: "=",
                loading: "=",
                resetGridData: "=?",
            },
            restrict: 'E',
            replace: true,
            controller: DocumentsDirectiveController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}
