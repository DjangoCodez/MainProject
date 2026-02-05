import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ITimeService } from "../TimeService";
import { Feature } from "../../../Util/CommonEnumerations";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IQService } from "angular";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ExportUtility } from "../../../Util/ExportUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Constants } from "../../../Util/Constants";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { TimeRuleEditDTO, TimeRuleExportImportDTO, TimeRuleGridDTO } from "../../../Common/Models/TimeRuleDTOs";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ImportTimeRulesMatchingDialogController } from "./Dialogs/ImportTimeRulesMatching/ImportTimeRulesMatchingDialogController";
import { ImportTimeRulesDialogController } from "./Dialogs/ImportTimeRules/ImportTimeRulesDialogController";
import { ImportedDetailsDialogController } from "./Dialogs/ImportedDetails/ImportedDetailsDialogController";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: { [index: string]: string; };

    // Flags
    private hasSelectedRows: boolean = false;

    //@ngInject
    constructor(
        private $q: IQService,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private selectedItemsService: ISelectedItemsService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Time.TimeRules", progressHandlerFactory, messagingHandlerFactory);

        this.useRecordNavigatorInEdit('timeRuleId', 'name');

        this.selectedItemsService.setup($scope, "timeRuleId", (items: number[]) => this.save(items));

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));

        super.onTabActivetedAndModified(() => {
            this.loadGridData(false);
        });
    }

    // SETUP

    onInit(parameters: any) {
        this.guid = parameters.guid;
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_TimeRule, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onDoLookups() {
        return this.$q.all([
            this.loadTerms()
        ]);
    }

    private setupGrid() {
        this.gridAg.addColumnActive("isActive", this.terms["common.active"], 50, (params) => this.selectedItemsService.CellChanged(params));
        this.gridAg.addColumnNumber("timeRuleId", this.terms["common.id"], 60, { enableHiding: true });
        this.gridAg.addColumnText("name", this.terms["common.name"], 150, true, { toolTipField: "name", enableHiding: true });
        this.gridAg.addColumnText("description", this.terms["common.description"], 100, true, { toolTipField: "description", enableHiding: true });
        this.gridAg.addColumnNumber("sort", this.terms["common.sort"], 60, { enableHiding: true });
        this.gridAg.addColumnText("isInconvenientWorkHours", this.terms["time.time.timerule.isiwh"], 60, true);
        this.gridAg.addColumnText("isStandby", this.terms["time.time.timerule.isstandby"], 60, true);
        this.gridAg.addColumnText("typeName", this.terms["common.type"], 80, true);
        this.gridAg.addColumnText("startDirectionName", this.terms["time.time.timerule.rulestartdirection"], 80, true);
        this.gridAg.addColumnDate("startDate", this.terms["common.datefrom"], 80, true);
        this.gridAg.addColumnDate("stopDate", this.terms["common.dateto"], 80, true);
        this.gridAg.addColumnText("timeCodeName", this.terms["common.timecode"], 100, true, { toolTipField: "timeCodeName", enableHiding: true });
        this.gridAg.addColumnNumber("timeCodeMaxLength", this.terms["time.time.timerule.timecodemaxlength"], 60, { enableHiding: true });
        this.gridAg.addColumnText("standardMinutes", this.terms["time.time.timerule.standardminutes"], 60, true);
        this.gridAg.addColumnText("breakIfAnyFailed", this.terms["time.time.timerule.breakifanyfailed"], 60, true);
        this.gridAg.addColumnText("adjustStartToTimeBlockStart", this.terms["time.time.timerule.adjuststarttotimeblockstart"], 60, true);
        this.gridAg.addColumnText("employeeGroupNames", this.terms["common.employeegroups"], 120, true, { toolTipField: "employeeGroupNames", enableHiding: true });
        this.gridAg.addColumnText("timeScheduleTypesNames", this.terms["time.schedule.scheduletype.scheduletypes"], 120, true, { toolTipField: "timeScheduleTypesNames", enableHiding: true });
        this.gridAg.addColumnText("timeDeviationCauseNames", this.terms["time.time.timedeviationcause.timedeviationcauses"], 120, true, { toolTipField: "timeDeviationCauseNames", enableHiding: true });
        this.gridAg.addColumnText("dayTypeNames", this.terms["time.schedule.daytype.daytypes"], 120, true, { toolTipField: "dayTypeNames", enableHiding: true });
        this.gridAg.addColumnText("startExpression", this.terms["time.time.timerule.startexpression"], null, true, { toolTipField: "startExpression", enableHiding: true });
        this.gridAg.addColumnText("stopExpression", this.terms["time.time.timerule.stopexpression"], null, true, { toolTipField: "stopExpression", enableHiding: true });
        if (CoreUtility.isSupportAdmin)
            this.gridAg.addColumnIcon("imported", this.terms["time.time.timerule.imported"], 60, { icon: "fal fa-cloud-download", toolTipField: "importedTooltip", showIcon: (row: TimeRuleGridDTO) => row.imported, onClick: this.showImportedDetails.bind(this), showTooltipFieldInFilter: true });
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this), false);

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("time.time.timerules.timerules", true, undefined, true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData(), true, () => this.selectedItemsService.Save(), () => { return this.saveButtonIsDisabled() });

        if (CoreUtility.isSupportAdmin) {
            let importExportGroup: ToolBarButtonGroup = ToolBarUtility.createGroup(new ToolBarButton("time.time.timerule.export", "time.time.timerule.export", IconLibrary.FontAwesome, "fa-file-export",
                () => { this.exportTimeRules(); },
                () => { return !this.hasSelectedRows; }
            ));
            importExportGroup.buttons.push(new ToolBarButton("time.time.timerule.import", "time.time.timerule.import", IconLibrary.FontAwesome, "fa-file-import",
                () => { this.importTimeRules(); }
            ));
            this.toolbar.addButtonGroup(importExportGroup);
        }
    }

    private saveButtonIsDisabled(): boolean {
        return !this.selectedItemsService.SelectedItemsExist();
    }

    // SERVICE CALLS   

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.yes",
            "core.no",
            "core.edit",
            "common.active",
            "common.datefrom",
            "common.dateto",
            "common.description",
            "common.employeegroups",
            "common.id",
            "common.name",
            "common.sort",
            "common.timecode",
            "common.type",
            "time.time.timerule.export",
            "time.time.timerule.import",
            "time.time.timerule.import.error",
            "time.time.timerule.imported",
            "time.time.timerule.isiwh",
            "time.time.timerule.isstandby",
            "time.time.timerule.rulestartdirection",
            "time.time.timerule.startexpression",
            "time.time.timerule.stopexpression",
            "time.time.timerule.timecodemaxlength",
            "time.time.timerule.timerules",
            "time.time.timerule.standardminutes",
            "time.time.timerule.breakifanyfailed",
            "time.time.timerule.adjuststarttotimeblockstart",
            "time.time.timedeviationcause.timedeviationcauses",
            "time.schedule.daytype.daytypes",
            "time.schedule.scheduletype.scheduletypes",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    public loadGridData(useCache: boolean) {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.timeService.getTimeRules().then(x => {
                // Used for filter labels
                x.forEach((item) => {
                    item['importedTooltip'] = item.imported ? this.terms["core.yes"] : this.terms["core.no"];
                });

                this.setData(x);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

    // ACTIONS

    private save(items: number[]) {
        var dict: any = {};

        _.forEach(items, (id: number) => {
            // Find entity
            var entity: any = this.gridAg.options.findInData((ent: any) => ent["timeRuleId"] === id);

            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if (dict !== undefined) {
            this.timeService.updateTimeRuleState(dict).then(() => {
                this.loadGridData(false);
            });
        }
    }

    private exportTimeRules() {
        this.timeService.exportTimeRules(this.gridAg.options.getSelectedIds('timeRuleId')).then(rules => {
            ExportUtility.Export(rules, this.terms["time.time.timerule.timerules"] + '.json');
        });
    }

    private importTimeRules() {
        var url = CoreUtility.apiPrefix + Constants.WEBAPI_TIME_TIME_TIME_RULE_IMPORT;
        var modal = this.notificationService.showFileUpload(url, this.terms["time.time.timerule.import"], true, true, false);
        modal.result.then(res => {
            if (res && res.result) {
                let importResult = new TimeRuleExportImportDTO();
                angular.extend(importResult, res.result);
                importResult.setTypes();
                this.openImportTimeRulesMatchingDialog(importResult);
            } else {
                this.notificationService.showDialogEx(this.terms["time.time.timerule.import"], this.terms["time.time.timerule.import.error"], SOEMessageBoxImage.Error);
            }
        });
    }

    private openImportTimeRulesMatchingDialog(importResult: TimeRuleExportImportDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeRules/Dialogs/ImportTimeRulesMatching/ImportTimeRulesMatchingDialog.html"),
            controller: ImportTimeRulesMatchingDialogController,
            controllerAs: "ctrl",
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                importResult: () => { return importResult },
            }
        }
        this.$uibModal.open(options).result.then((res: any) => {
            if (res && res.result) {
                this.timeService.importTimeRulesMatch(res.result).then(x => {
                    let impResult = new TimeRuleExportImportDTO();
                    angular.extend(impResult, x);
                    impResult.setTypes();

                    this.openImportTimeRulesDialog(impResult);
                });
            }
        }, (reason) => { });
    }

    private openImportTimeRulesDialog(importResult: TimeRuleExportImportDTO) {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeRules/Dialogs/ImportTimeRules/ImportTimeRulesDialog.html"),
            controller: ImportTimeRulesDialogController,
            controllerAs: "ctrl",
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                importResult: () => { return importResult },
            }
        }

        this.$uibModal.open(options).result.then(res => {
            if (res && res.result) {
                this.timeService.importTimeRulesSave(res.result).then(result => {
                    if (result.success)
                        this.reloadData();
                });
            }
        });
    }

    // EVENTS

    private showImportedDetails(row: TimeRuleGridDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeRules/Dialogs/ImportedDetails/ImportedDetailsDialog.html"),
            controller: ImportedDetailsDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                timeRuleId: () => { return row.timeRuleId },
            }
        }
        this.$uibModal.open(options);
    }

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
        });
    }
}