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
import { Feature, TermGroup, TermGroup_TimePeriodType } from "../../../Util/CommonEnumerations";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private terms: { [index: string]: string; };

    private accumulatorTypes: ISmallGenericType[];
    private timePeriodHeads: ISmallGenericType[];
    private selectedCount: number = 0;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private timeService: ITimeService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Time.TimeAccumulators", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timeAccumulatorId', 'name');
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
            .onBeforeSetUpGrid(() => this.loadAccumulatorTypes())
            .onBeforeSetUpGrid(() => this.loadTimePeriodHeads())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_TimeAccumulator, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {
        var keys: string[] = [
            "common.active",
            "common.name",
            "common.description",
            "common.type",
            "core.edit",
            "core.workfailed",
            "core.worked",
            "time.time.timeaccumulator.showintimereports",
            "time.time.timeperiod.timeperiodhead"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnBool("isActive", this.terms["common.active"], 30, false, null, null, true);
            this.gridAg.addColumnText("name", this.terms["common.name"], null);
            this.gridAg.addColumnText("description", this.terms["common.description"], null, true);
            this.gridAg.addColumnSelect("typeName", this.terms["common.type"], null, { displayField: "typeName", selectOptions: this.accumulatorTypes, dropdownValueLabel: "name" });
            this.gridAg.addColumnSelect("timePeriodHeadName", this.terms["time.time.timeperiod.timeperiodhead"], null, { displayField: "timePeriodHeadName", selectOptions: this.timePeriodHeads, dropdownValueLabel: "name" });
            this.gridAg.addColumnBool("showInTimeReports", this.terms["time.time.timeaccumulator.showintimereports"], null);
            this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this), false);

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
                this.$timeout(() => {
                    this.selectedCount = this.gridAg.options.getSelectedCount();
                });
            }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: any) => {
                this.$timeout(() => {
                    this.selectedCount = this.gridAg.options.getSelectedCount();
                });
            }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("time.time.timeaccumulator.timeaccumulators", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
        var group = ToolBarUtility.createGroup(new ToolBarButton("time.time.timeaccumulator.recalculate", "time.time.timeaccumulator.recalculate", IconLibrary.FontAwesome, "fa-calculator", () => {
            this.recalculate();
        }, () => {
            return this.selectedCount === 0;
        }, () => {
            return !this.modifyPermission;
        }));
        this.toolbar.addButtonGroup(group);
    }

    // SERVICE CALLS   

    private loadAccumulatorTypes(): ng.IPromise<any> {
        this.accumulatorTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.TimeAccumulatorType, false, true).then(x => {
            this.accumulatorTypes = x;
        });
    }

    private loadTimePeriodHeads(): ng.IPromise<any> {
        return this.timeService.getTimePeriodHeadsDict(TermGroup_TimePeriodType.RuleWorkTime, false).then(x => {
            this.timePeriodHeads = x;
        });
    }

    public loadGridData(useCache: boolean) {
        this.progress.startLoadingProgress([() => {
            return this.timeService.getTimeAccumulators(false).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

    // EVENTS   

    private recalculate() {
        var ids: number[] = this.gridAg.options.getSelectedIds("timeAccumulatorId");
        if (ids.length > 0) {
            this.progress.startWorkProgress((completion) => {
                this.timeService.recalculateTimeAccumulators(ids).then(result => {
                    if (result.success) {
                        completion.completed(null, true);
                        this.reloadData();
                    } else {
                        completion.failed(this.terms["core.workfailed"]);
                    }
                }, error => {
                    completion.failed(this.terms["core.workfailed"]);
                });
            });
        }
    }
}