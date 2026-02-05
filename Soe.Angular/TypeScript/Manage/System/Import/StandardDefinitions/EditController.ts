import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IEditControllerFlowHandler, IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbar } from "../../../../Core/Handlers/Toolbar";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandler } from "../../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ISystemService } from "../../SystemService";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, TermGroup, SoeReportTemplateType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ContractGroupDTO } from "../../../../Common/Models/ContractDTO";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { lang } from "moment";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { SysImportDefinitionDTO, SysImportHeadDTO, SysImportDefinitionLevelColumnSettings } from "../../../../Common/Models/SysImportDTO";
import { ISysImportSelectDTO, ISysImportDefinitionLevelColumnSettings } from "../../../../Scripts/TypeLite.Net4";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private sysImportDefinitionId: number;
    private sysImportHeadId: number;

    // Models
    private sysImportDefinition: SysImportDefinitionDTO;
    private sysImportHead: SysImportHeadDTO;
    private sysImportLevel: ISysImportSelectDTO;

    // Terms
    private terms: { [index: string]: string; };

    // Collections
    private sysImportHeads: any[];
    private definitionTypes: any[];
    private updateTypes: any[];

    // Grids
    private levelsGrid: ISoeGridOptionsAg;
    private headGrid: ISoeGridOptionsAg;
    private definitionGrid: ISoeGridOptionsAg;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout,
        private coreService: ICoreService,
        private systemService: ISystemService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private notificationService: INotificationService,
        private translationService: ITranslationService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        
        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
        
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.sysImportDefinitionId = parameters.id;

        this.levelsGrid = SoeGridOptionsAg.create("LevelsGrid", this.$timeout);
        this.headGrid = SoeGridOptionsAg.create("HeadGrid", this.$timeout);
        this.definitionGrid = SoeGridOptionsAg.create("DefinitionGrid", this.$timeout);

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_System].readPermission;
        this.modifyPermission = response[Feature.Manage_System].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private doLookups(): ng.IPromise<any> {
        if (this.sysImportDefinitionId) {
            return this.$q.all([
                this.loadTerms(),
                this.loadSysImportHeads(),
                this.loadDefinitionTypes(),
                this.loadUpdateTypes(),
                this.loadDefinition()]).then(() => {
                    this.setupGrids();
                    if (this.sysImportDefinition.sysImportHeadId)
                        this.loadSysImportHead(this.sysImportDefinition.sysImportHeadId);
                    else
                        this.new();

                });
        } else {
            return this.$q.all([
                this.loadTerms(),
                this.loadSysImportHeads(),
                this.loadDefinitionTypes(),
                this.loadUpdateTypes()]).then(() => {
                    this.setupGrids();
                    this.new();
                });
        }
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.warning",
            "common.unsavedchanges",
            "common.column",
            "common.datatype",
            "manage.system.import.xmltag",
            "manage.system.import.standardvalue",
            "manage.system.import.convert",
            "common.remove",
            "manage.system.import.xmlnode",
            "manage.system.import.multilevelpost",
            "manage.system.import.multilevelpost",
            "manage.system.import.position",
            "manage.system.import.characters",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadSysImportHeads(): ng.IPromise<any> {
        return this.systemService.getSysImportHeadsDict().then(x => {
            this.sysImportHeads = x;
        });
    }

    private loadDefinitionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SysImportDefinitionType, false, false).then(x => {
            this.definitionTypes = x;
        });
    }

    private loadUpdateTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SysImportDefinitionUpdateType, false, false).then(x => {
            this.updateTypes = x;
        });
    }

    private loadSysImportHead(sysImportHeadId: number) {
        return this.systemService.getSysImportHead(sysImportHeadId).then(x => {
            this.sysImportHead = x;
            this.levelsGrid.setData(this.sysImportHead.sysImportSelects);

            this.$timeout(() => {
                var row = _.find(this.sysImportHead.sysImportSelects, (l) => l.level === 1);
                this.levelsGrid.selectRow(row);
                this.sysImportLevelChanged(row)
            });
        });
    }

    private setupGrids() {
        this.levelsGrid.enableRowSelection = false;
        this.levelsGrid.enableSingleSelection();
        this.levelsGrid.enableFiltering = false;
        this.levelsGrid.addColumnText("name", this.terms["common.name"], null);

        let lEvents: GridEvent[] = [];
        lEvents.push(new GridEvent(SoeGridOptionsEvent.RowClicked, (row) => { this.sysImportLevelChanged(row) }));
        this.levelsGrid.subscribe(lEvents);
        this.levelsGrid.finalizeInitGrid();     

        this.headGrid.enableRowSelection = false;
        this.headGrid.enableFiltering = false;
        this.headGrid.addColumnText("text", this.terms["common.column"], null);
        this.headGrid.addColumnText("dataType", this.terms["common.datatype"], null);

        let hEvents: GridEvent[] = [];
        hEvents.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.addDefinitionRow(row) }));
        this.headGrid.subscribe(hEvents);
        this.headGrid.finalizeInitGrid();     

        this.definitionGrid.enableRowSelection = false;
        this.definitionGrid.enableFiltering = false;
        this.definitionGrid.addColumnIsModified("isModified", "", 20); 
        this.definitionGrid.addColumnText("text", this.terms["common.column"], null, { editable: true });
        this.definitionGrid.addColumnText("xmlTag", this.terms["manage.system.import.xmltag"], null, { editable: true });
        this.definitionGrid.addColumnNumber("position", this.terms["manage.system.import.position"], null, { editable: true });
        this.definitionGrid.addColumnNumber("from", this.terms["common.from"], null, { editable: true });
        this.definitionGrid.addColumnNumber("characters", this.terms["manage.system.import.characters"], null, { editable: true });
        this.definitionGrid.addColumnText("standard", this.terms["manage.system.import.standardvalue"], null, { editable: true });
        this.definitionGrid.addColumnText("convert", this.terms["manage.system.import.convert"], null, { editable: true });
        this.definitionGrid.addColumnIcon(null, " ", null, { icon: "fal fa-times iconDelete", toolTip: this.terms["common.remove"], onClick: this.deleteDefinitionRow.bind(this) });

        this.definitionGrid.finalizeInitGrid();
    }

    private addDefinitionRow(row: any) {
        var currentLevel = _.find(this.sysImportDefinition.sysImportDefinitionLevels, (lev) => lev.level === this.sysImportLevel.level);
        if (currentLevel) {
            if (!_.find(currentLevel.columns, { 'column': row.column })) {
                var column = new SysImportDefinitionLevelColumnSettings();
                column.level = this.sysImportLevel.level;
                column.column = row.column;
                column.text = row.text;
                column.isModified = true;
                currentLevel.columns.push(column);
                this.definitionGrid.setData(currentLevel.columns);

                this.dirtyHandler.setDirty();
            }
        }
    }

    private deleteDefinitionRow(row: any) {
        var currentLevel = _.find(this.sysImportDefinition.sysImportDefinitionLevels, (lev) => lev.level === this.sysImportLevel.level);
        if (currentLevel) {
            _.remove(currentLevel.columns, { 'column': row.column });
            this.definitionGrid.setData(currentLevel.columns);

            this.dirtyHandler.setDirty();
        }
    }

    private setColumnVisibility() {
        this.$timeout(() => {
            if (this.sysImportDefinition.type === 0) {
                this.definitionGrid.showColumn("xmlTag");
                this.definitionGrid.hideColumn("position");
                this.definitionGrid.hideColumn("from");
                this.definitionGrid.hideColumn("characters");
            }
            else if (this.sysImportDefinition.type === 1) {
                this.definitionGrid.hideColumn("xmlTag");
                this.definitionGrid.showColumn("position");
                this.definitionGrid.hideColumn("from");
                this.definitionGrid.hideColumn("characters");
            }
            else {
                this.definitionGrid.hideColumn("xmlTag");
                this.definitionGrid.hideColumn("position");
                this.definitionGrid.showColumn("from");
                this.definitionGrid.showColumn("characters");
            }
            this.definitionGrid.sizeColumnToFit();
        });
    }

    private sysImportHeadChanged(id: number) {
        if (this.dirtyHandler.isDirty) {
            var modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.unsavedchanges"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.loadSysImportHead(id);
                    this.sysImportHeadId = id;
                }
                else {
                    this.sysImportDefinition.sysImportHeadId = this.sysImportHeadId;
                }
            });
        }
        else {
            this.loadSysImportHead(id);
            this.sysImportHeadId = id;
        }
    }

    private typeChanged() {
        this.setColumnVisibility();
        this.sysImportLevelChanged(undefined);
    }

    private sysImportLevelChanged(level: ISysImportSelectDTO) {
        if (!level)
            return;

        this.sysImportLevel = level;

        // Handle selectable rows
        var rows = [];
        if (this.sysImportDefinition.type === 0) {
            rows.push({ text: this.terms["manage.system.import.xmlnode"], column: 'xmltag' });
            rows = _.concat(rows, _.filter(this.sysImportLevel.settingObjects, (row) => row.column !== 'xmltag' && row.column !== 'recordtype'));
        }
        else {
            rows.push({ text: this.terms["manage.system.import.multilevelpost"], column: 'recordtype' });
            rows = _.concat(rows, _.filter(this.sysImportLevel.settingObjects, (row) => row.column !== 'xmltag' && row.column !== 'recordtype'));
        }
        this.headGrid.setData(rows);

        // Handle selected
        var currentLevel = _.find(this.sysImportDefinition.sysImportDefinitionLevels, (lev) => lev.level === this.sysImportLevel.level);
        if (currentLevel) {
            this.definitionGrid.setData(currentLevel.columns);
        }
        else {
            var newLevel = {
                sysImportDefinitionId: undefined,
                sysImportDefinitionLevelId: undefined,
                level: this.sysImportLevel.level,
                xml: undefined,
                columns: []
            };
            this.sysImportDefinition.sysImportDefinitionLevels.push(newLevel);
            this.definitionGrid.setData(newLevel.columns);
        }
    }

    private loadDefinition(reload: boolean = false) {
        return this.systemService.getSysImportDefinition(this.sysImportDefinitionId).then(x => {
            this.isNew = false;
            this.sysImportDefinition = x;
            this.setColumnVisibility();

            if (reload) {
                this.$timeout(() => {
                    var row = _.find(this.sysImportHead.sysImportSelects, (l) => l.level === 1);
                    this.levelsGrid.selectRow(row);
                    this.sysImportLevelChanged(row)
                });
            }
        });
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            // Set module
            this.sysImportDefinition.module = this.sysImportHead ? this.sysImportHead.module : 0;

            this.systemService.saveSysImportDefinition(this.sysImportDefinition).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.sysImportDefinitionId = this.sysImportDefinition.sysImportDefinitionId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.sysImportDefinition);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.headGrid.clearData();
                this.definitionGrid.clearData();
                this.loadDefinition(true);
                this.dirtyHandler.clean();
            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.systemService.deleteSysImportDefinition(this.sysImportDefinitionId).then((result) => {
                if (result.success) {
                    completion.completed(null);
                    this.new();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private new() {
        this.isNew = true;
        this.sysImportDefinition = new SysImportDefinitionDTO();
        this.sysImportDefinition.type = 0;
        this.sysImportDefinition.sysImportDefinitionLevels = [];
    }

    protected copy() {
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.sysImportDefinition) {
                if (!this.sysImportDefinition.sysImportDefinitionId)
                    mandatoryFieldKeys.push("manage.system.import.headdefinition");
                if (!this.sysImportDefinition.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}