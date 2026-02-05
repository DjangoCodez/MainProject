import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { Feature, SoeEntityState } from "../../../../Util/CommonEnumerations";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISystemService } from "../../SystemService"
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../../Util/Enumerations";
import { ISelectedItemsService } from "../../../../Core/Services/SelectedItemsService";
import { AddJobController } from "./Dialogs/AddRegisteredJob/AddRegisteredJob";
import { SysJobDTO } from "../../../../Common/Models/SysJobDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //Terms
    terms: { [index: string]: string; };

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private notificationService: INotificationService,
        private $timeout: ng.ITimeoutService,
        private $filter: ng.IFilterService,
        private $uibModal,
        private $q: ng.IQService,
        private selectedItemsService: ISelectedItemsService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,) {
        super(gridHandlerFactory, "Manage.System.Scheduler.RegisteredJobs", progressHandlerFactory, messagingHandlerFactory);

        this.selectedItemsService.setup($scope, "sysJobId", (items: number[]) => this.updateJobState(items));

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
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.addStandardMenuItems();

        var translationKeys: string[] = [
            "common.active",
            "common.name",
            "common.description",
            "manage.system.scheduler.composition",
            "manage.system.scheduler.classname",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnActive("isActive", terms["common.active"], 50, (params) => this.selectedItemsService.CellChanged(params));
            this.gridAg.addColumnText("name", terms["common.name"], null, false);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnText("assemblyName", terms["manage.system.scheduler.composition"], null, true);
            this.gridAg.addColumnText("className", terms["manage.system.scheduler.classname"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("manage.system.scheduler.registeredjobs", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData(), true, () => this.selectedItemsService.Save(), () => { return !this.selectedItemsService.SelectedItemsExist() });
        if (this.toolbar) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.system.scheduler.registernewjob", "manage.system.scheduler.registernewjobtooltip", IconLibrary.FontAwesome, "fa-plus", () => {
                this.edit(null);
            })));
            /*this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.system.scheduler.runtimeshort", "manage.system.scheduler.runtime", IconLibrary.FontAwesome, "fa-download", () => {

            })));*/
        }
    }

    private loadGridData(useCache: boolean = true) {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.systemService.getRegisteredJobs(useCache).then(data => {
                this.gridAg.setData(data);
            });
        }]);

    }

    private saveJob(job: SysJobDTO) {
        this.progress.startSaveProgress((completion) => {
            this.systemService.saveJob(job).then((result) => {
                if (result.success)
                    completion.completed();
                else
                    completion.failed(result.errorMessage);
            });
        }, null).then(() => {
            this.loadGridData(false);
        });
    }

    private deleteJob(jobId: number) {
        this.progress.startDeleteProgress((completion) => {
            this.systemService.deleteJob(jobId).then((result) => {
                if (result.success)
                    completion.completed(null);
                else
                    completion.failed(result.errorMessage);
            });
        }, null).then(() => {
            this.loadGridData(false);
        });
    }

    private updateJobState(items: any) {
        var dict: any = {};

        _.forEach(items, (id: number) => {
            var entity: any = this.gridAg.options.findInData((ent: any) => ent["sysJobId"] === id);
            if (entity)
                dict[id] = entity.isActive ? SoeEntityState.Active : SoeEntityState.Inactive;
        });

        if (Object.keys(dict).length > 0) {
            this.progress.startSaveProgress((completion) => {
                this.systemService.updateAttestRuleState(dict).then((result) => {
                    if (result.success)
                        completion.completed();
                    else
                        completion.failed(result.errorMessage);
                });
            }, null).then(() => {
                this.loadGridData(false);
            });
        }
    }

    edit(row: SysJobDTO) {
        if (!row) {
            row = new SysJobDTO();
            row.state = SoeEntityState.Active;
        }

        // Send message to TabsController
        if (this.readPermission || this.modifyPermission) {
            var modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/Scheduler/RegisteredJobs/Dialogs/AddRegisteredJob/AddRegisteredJob.html"),
                controller: AddJobController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'md',
                resolve: {
                    job: () => { return row }
                }
            });

            modal.result.then((result: any) => {
                if (result) {
                    if (result.delete) {
                        this.deleteJob(result.item.companyGroupAdministrationId);
                    } else if (result.item) {
                        this.saveJob(result.item);
                    }
                }
            });
        }
    }

    private reloadData() {
        this.loadGridData(false);
    }
}
