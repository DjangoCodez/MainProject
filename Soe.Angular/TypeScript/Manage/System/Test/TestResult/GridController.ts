import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { Feature, ScheduledJobState, TermGroup } from "../../../../Util/CommonEnumerations";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISystemService } from "../../SystemService"
import { TestCaseGroupDTO, TestCaseGroupResultDTO } from "../Util";

class TestGroupResultOverview {
    testCaseGroupDTO: TestCaseGroupDTO;
    groupResultDTO: TestCaseGroupResultDTO;
}

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private settingTypes: any;
    private testCaseGroups: any[] = [];

    private _selectedTestCaseGroup: any = 0;
    get selectedTestCaseGroup() {
        return this._selectedTestCaseGroup;
    }
    set selectedTestCaseGroup(item: any) {
        this._selectedTestCaseGroup = item;
        this.reloadData();
    }

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private notificationService: INotificationService,
        private $timeout: ng.ITimeoutService,
        private $filter: ng.IFilterService,
        private $uibModal,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,) {
        super(gridHandlerFactory, "Manage.System.Scheduler.ScheduledJobs", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.getTestCaseGroups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {
        var translationKeys: string[] = [
            "common.name",
            "common.description",
            "common.number",
            "manage.system.syscompany.syscompdb",
            "manage.system.scheduler.executiontime",
            "common.status",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "manage.system.scheduler.active",
            "manage.system.scheduler.noofactive",
            "manage.system.scheduler.runnow",
            "manage.system.scheduler.activate",
            "manage.system.scheduler.showhistory",
            "manage.system.scheduler.batchnr",
            "common.dashboard.syslog.level",
            "common.time",
            "common.message",
            "manage.system.scheduler.logfor"
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.gridAg.enableMasterDetail(true, null, null, true);
            this.gridAg.options.setDetailCellDataCallback((params) => {
                this.getTestCaseResults(params);
            });
            this.gridAg.addColumnNumber("testCaseGroupId", terms["common.number"], null, { enableHiding: true });
            this.gridAg.addColumnText("name", terms["common.name"], null, false, { enableHiding: true });
            this.gridAg.addColumnText("description", terms["common.description"], null, true, { enableHiding: true });
            this.gridAg.addColumnDateTime("requestStarted", terms["manage.system.scheduler.executiontime"], null, true);
            this.gridAg.addColumnDateTime("requestEnded", terms["manage.system.scheduler.executiontime"], null, true);
            this.gridAg.addColumnNumber("successPercent", "Success rate", null, { enableHiding: true });
            this.gridAg.addColumnNumber("testCaseGroupResultId", "testCaseGroupResultId", null, { enableHiding: true });


            const expanderColumn = this.gridAg.detailOptions.addColumnText("expander", "", 100, {
                enableResizing: false,
                enableHiding: false
            });
            expanderColumn.cellRenderer = "agGroupCellRenderer";
            expanderColumn.filter = false;
            expanderColumn.suppressSizeToFit = true;
            expanderColumn.width = 20;
            expanderColumn.suppressExport = true;
            expanderColumn.pinned = "left";
            expanderColumn.cellClass = "soe-ag-cell-expander";
            
            this.gridAg.detailOptions.addColumnNumber("testCaseId", "TestCaseId", null);
            this.gridAg.detailOptions.addColumnNumber("testCaseResultId", "TestCaseResultId", null);
            this.gridAg.detailOptions.addColumnText("name", "name", null);
            this.gridAg.detailOptions.addColumnText("testCaseResultGuid", "testCaseResultGuid", null);
            this.gridAg.detailOptions.addColumnDateTime("requestStarted", "Request started", null);
            this.gridAg.detailOptions.addColumnDateTime("requestEnded", "Request ended", null);
            this.gridAg.detailOptions.addColumnBool("success", "Success", null, { enableEdit: false });

            const detailOfDetailsGrid = new SoeGridOptionsAg("TestCaseStep", this.$timeout);
            this.gridAg.detailOptions.enableMasterDetail(detailOfDetailsGrid, null, null, true);
            this.gridAg.detailOptions.setDetailCellDataCallback((params) => {
                this.getTestCaseSteps(params);
            });
            detailOfDetailsGrid.addColumnNumber("sequenceNumber", "sequenceNumber", null);
            detailOfDetailsGrid.addColumnText("description", "Description", null);
            detailOfDetailsGrid.addColumnText("comment", "Comment", null);
            detailOfDetailsGrid.addColumnNumber("timeOffset", "Offset (s)", null);
            detailOfDetailsGrid.addColumnBool("success", "Success", null,{});


            this.gridAg.finalizeInitGrid("manage.system.test.testresultoverview", true);
        });
    }
    private getTestCaseResults(params) {
        params.successCallback(params.data.testCaseResults);
    }

    private getTestCaseSteps(params) {
        let steps = params.data.testCaseStepResults;
        if (steps) {
            steps = steps.sort((a, b) => a.sequenceNumber - b.sequenceNumber);
        } else {
            steps = [];
        }
        params.successCallback(steps);
    }
    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
    }

    private getData() {
        if (!this.selectedTestCaseGroup) {
            return this.systemService.getTestCaseGroupOverview();
        }
        else {
            return this.systemService.getTestCaseGroupOverviewByGroup(this.selectedTestCaseGroup);
        }
    }

    private loadGridData() {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.getData().then(data => {
                let rows = data as TestGroupResultOverview[];
                let gridData = [];
                rows.forEach(row => {
                    if (row.groupResultDTO && row.groupResultDTO.testCaseResults) {
                        row.groupResultDTO.testCaseResults.forEach((res, i) => {
                            res = {
                                ...res,
                                ...res.testCase,
                                expander: " "
                            }

                            row.groupResultDTO.testCaseResults[i] = res;
                        })
                    }
                    gridData.push({
                        ...row.groupResultDTO,
                        ...row.testCaseGroupDTO,
                        expander: " "
                    })
                })
                this.gridAg.setData(gridData);
            });
        }]);

    }

    private getTestCaseGroups() {
        return this.systemService.getTestCaseGroups()
            .then(data => {
                this.testCaseGroups = data.map(r => {
                    return { id: r.testCaseGroupId, name: r.name }
                })
                this.testCaseGroups.unshift({ id: 0, name: "" })
            })
    }

    private reloadData() {
        this.loadGridData();
    }
}
