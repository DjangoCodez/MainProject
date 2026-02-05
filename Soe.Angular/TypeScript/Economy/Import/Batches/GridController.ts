import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler"
import { IImportService } from "../ImportService";
import { Feature, TermGroup, SettingMainType, UserSettingType, TermGroup_GridDateSelectionType } from "../../../Util/CommonEnumerations";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { Constants } from "../../../Util/Constants";
import { EditController } from "./EditController";
import { ImportBatchDTO } from "../../../Common/Models/ImportBatchDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {   

    // Lookups 
    batches: ImportBatchDTO[];
    IOImportHeadTypes: any[];
    dateSelectionDict: any[];

    private _selectedIOImportHeadType: any;
    get selectedIOImportHeadType() {
        return this._selectedIOImportHeadType;
    }
    set selectedIOImportHeadType(item: any) {
        this._selectedIOImportHeadType = item;
        if (this.selectedIOImportHeadType) {
            this.updateIOImportHeadType();
        }
    }

    private _dateSelection: number = TermGroup_GridDateSelectionType.One_Month;
    get dateSelection() {
        return this._dateSelection;
    }
    set dateSelection(item: number) {
        this._dateSelection = item;
        if (this._dateSelection)
            this.updateDateSelection();
    }

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private importService: IImportService,        
        private coreService: ICoreService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        
        
    ) {
        super(gridHandlerFactory, "Economy.Import.Batches", progressHandlerFactory, messagingHandlerFactory);        

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;                
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))            
            .onBeforeSetUpGrid(() => this.onBeforeSetUpGrid())
            .onSetUpGrid(() => this.setupGrid())            
            .onLoadGridData(() => this.loadGridData());
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;        
        this.flowHandler.start({ feature: Feature.Economy_Import_XEConnect, loadReadPermissions: true, loadModifyPermissions: true });
    }

    public setupGrid() {
        
        var keys: string[] = [
            "common.name",
            "common.connect.import",
            "common.connect.imports",
            "common.status",
            "common.connect.importtype",
            "common.connect.importheadtype",
            "core.edit",
            "common.created",
            "economy.import.batches.batch"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            
            this.gridAg.addColumnSelect("typeName", terms["common.connect.importtype"], 10, { displayField: "typeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnSelect("sourceName", terms["common.connect.import"], 10, { displayField: "sourceName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnSelect("importHeadTypeName", terms["common.connect.importheadtype"], 10, { displayField: "importHeadTypeName", selectOptions: this.IOImportHeadTypes });
            this.gridAg.addColumnSelect("statusName", terms["common.status"], 10, { displayField: "statusName", selectOptions:null, populateFilterFromGrid:true });
            this.gridAg.addColumnText("batchId", terms["economy.import.batches.batch"], 25)
            this.gridAg.addColumnDateTime("created", terms["common.created"], 5)
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);    

            this.gridAg.finalizeInitGrid("common.connect.imports", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getGlobalUrl("economy/import/batches/views/gridHeader.html") );
    }   

    private updateIOImportHeadType() {
        //this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.VoucherSeriesSelection, this.selectedIOImportHeadType).then((x) => {
        this.reloadGridFromFilter();
        //});
    }

    private updateDateSelection() {
        this.reloadGridFromFilter();
    }

    private onBeforeSetUpGrid(): ng.IPromise<any> {
        return this.$q.all([
                this.loadAllItemsSelection(),
                this.loadIOImportHeadTypes()
            ])
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

    // SERVICE CALLS   
    public loadGridData() {
        if (this.selectedIOImportHeadType) {
            this.progress.startLoadingProgress([() => {
                return this.importService.getBatches(this.selectedIOImportHeadType, this.dateSelection).then((x) => {
                    _.forEach(x, (row) => {
                        var statuses: string = "";
                        _.forEach(row.statusName, (status) => {
                            if (statuses.length > 0)
                                statuses += ",";
                            statuses += status;
                        });
                        row.statusName = statuses;
                    });
                    this.batches = x;
                });
            }]).then(() => {
                this.setData(this.batches);
            });
        }
    }     

    //protected onDoLookups() {
    //    return this.progress.startLoadingProgress([
    //        () => this.loadIOSources(),
    //        () => this.loadIOTypes(),
    //        () => this.loadIOStatuses(),
    //        () => this.loadIOImportHeadTypes(),           
    //    ]);                
    //}

    private loadIOImportHeadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.IOImportHeadType, false, false).then((x) => {
            this.IOImportHeadTypes = x;
        })
    }

    private loadAllItemsSelection(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.GridDateSelectionType, false, true, true).then((x) => {
            this.dateSelectionDict = x;
        });
    }

    // EVENTS   
    edit(row) {                         
        var keys: string[] = [           
            "economy.import.batches.batch"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            var message = new TabMessage(terms["economy.import.batches.batch"] + " " + row.batchId, row.recordId, EditController, { importHeadType: row.importHeadType, batchId: row.batchId }, this.urlHelperService.getViewUrl("edit.html"));
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
            //this.messagingHandler.publishEditRow(row);
        });
    }
}