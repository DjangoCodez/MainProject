import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ProjectUserDTO } from "../../../../Common/Models/ProjectDTO";
import { ToolBarButton, ToolBarUtility, ToolBarButtonGroup } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { AddProjectUserController } from "../Dialogs/AddProjectUsers/AddProjectUserController";
import { Feature, SoeEntityState } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { GridControllerBaseAg } from "../../../../Core/Controllers/GridControllerBaseAg";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { Guid } from "../../../../Util/StringUtility";

export class ProjectPersonsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Projects/Directives/ProjectPersons.html'),
            scope: {
                projectId: '=',
                readOnly: '=?',
                projectUsers: '=',
                parentGuid: '='
            },
            restrict: 'E',
            replace: true,
            controller: ProjectPersonsDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class ProjectPersonsDirectiveController extends GridControllerBaseAg {
    // Setup
    private projectUsers: ProjectUserDTO[];
    private readOnly: boolean;
    private projectId: number;
    private parentGuid: Guid;
    private calculatedCostPermission = false;

    // dims
    private dim2Header: any;
    private dim3Header: any;
    private dim4Header: any;
    private dim5Header: any;
    private dim6Header: any;

    // ToolBar
    protected buttonGroups = new Array<ToolBarButtonGroup>();
    
    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants) {
        super("Billing.Projects.Directives.ProjectPersons", "", Feature.Economy_Supplier_Invoice_Invoices_Edit, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        //this.init();
    }

    /*public init() {
        this.setupCustomToolBar();
    }*/

    protected setupCustomToolBar() {
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("billing.projects.list.new_person", "billing.projects.list.new_person", IconLibrary.FontAwesome, "fa-plus",
            () => { this.addRow(); },
            null,
            () => { return this.readOnly; })));
    }

    private loadPagePermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Billing_Project_EmployeeCalculateCost,
        ];

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.calculatedCostPermission = x[Feature.Billing_Project_EmployeeCalculateCost];
        });
    }

    public setupGrid() {
        this.loadPagePermissions().then( () => {
            const keys: string[] = [
                "billing.projects.list.name",
                "billing.projects.list.participanttype",
                "billing.projects.list.starting",
                "billing.projects.list.ending",
                "billing.projects.list.timecodename",
                "core.edit",
                "core.delete",
                "core.aggrid.totals.filtered",
                "core.aggrid.totals.total",
                "billing.project.calculatedcost"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.progressBusy = false;

                this.soeGridOptions.enableGridMenu = false;
                this.soeGridOptions.enableFiltering = true;
                this.soeGridOptions.enableRowSelection = false;
                this.soeGridOptions.setMinRowsToShow(8);

                this.soeGridOptions.addColumnText("name", terms["billing.projects.list.name"], null);
                this.soeGridOptions.addColumnText("typeName", terms["billing.projects.list.participanttype"], null);
                this.soeGridOptions.addColumnDate("dateFrom", terms["billing.projects.list.starting"], null);
                this.soeGridOptions.addColumnDate("dateTo", terms["billing.projects.list.ending"], null);

                this.soeGridOptions.addColumnText("timeCodeName", terms["billing.projects.list.timecodename"], null);

                if (this.calculatedCostPermission) {
                    this.soeGridOptions.addColumnNumber("employeeCalculatedCost", terms["billing.project.calculatedcost"], null);
                }

                this.soeGridOptions.addColumnEdit(terms["core.edit"], this.edit.bind(this));
                this.soeGridOptions.addColumnDelete(terms["core.delete"], this.deleteRow.bind(this));

                const events: GridEvent[] = [];
                events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); }));
                this.soeGridOptions.subscribe(events);

                this.soeGridOptions.finalizeInitGrid();

                this.$timeout(() => {
                    this.soeGridOptions.addTotalRow("#totals-grid", {
                        filtered: terms["core.aggrid.totals.filtered"],
                        total: terms["core.aggrid.totals.total"]
                    });
                    this.loadGrid(false);
                }, 50);
            });
        });
    }

    // Actions
    private addRow() {
        this.showUserDialog(null);
    }

    protected edit(row: any) {
        this.showUserDialog(row);
    }

    private showUserDialog(user: ProjectUserDTO) {
        if (!this.projectUsers)
            this.projectUsers = [];

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Projects/Dialogs/AddProjectUsers/addprojectuser.html"),
            controller: AddProjectUserController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                type: () => { return user ? user.type : null },
                user: () => { return user ? user.userId : null },
                timecode: () => { return user ? user.timeCodeId : null },
                from: () => { return user ? user.dateFrom : null },
                to: () => { return user ? user.dateTo : null },
                employeeCalculatedCost: () => { return user ? user.employeeCalculatedCost : null },
                userReadonly: () => { return user ? true : false },
                calculatedCostPermission: () => { return this.calculatedCostPermission },
            }
        }

        this.$uibModal.open(options).result.then(x => {
            if (!user) {
                user = new ProjectUserDTO();
                user.userId = x.user;
                user.name = x.username;
                user.state = SoeEntityState.Active;
                this.projectUsers.push(user);
            }

            user.type = x.type;
            user.typeName = x.typename;
            user.timeCodeId = x.timecode;
            user.timeCodeName = x.timecodename;
            user.dateFrom = x.from;
            user.dateTo = x.to;
            user.employeeCalculatedCost = x.employeeCalculatedCost;
            user.isModified = true;

            this.loadGrid(true);
        });
    }

    private loadGrid(setDirty: boolean) {
        this.soeGridOptions.setData(this.projectUsers.filter(x => x.state === SoeEntityState.Active));
        if (setDirty)
            this.setAsDirty(true);
    }

    protected deleteRow(row: ProjectUserDTO) {
        row.state = SoeEntityState.Deleted;
        row.isModified = true;
        this.loadGrid(true);
    }

    private setAsDirty(isDirty = true) {
        this.messagingService.publish(
            Constants.EVENT_SET_DIRTY,
            {
                guid: this.parentGuid,
                dirty: isDirty
            }
        );
    }
}
