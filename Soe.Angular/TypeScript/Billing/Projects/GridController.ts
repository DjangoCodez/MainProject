import { ICoreService } from "../../Core/Services/CoreService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { IInvoiceService } from "../../Shared/Billing/Invoices/InvoiceService";
import { ProjectListGridButtonFunctions } from "../../Util/Enumerations";
import { HtmlUtility } from "../../Util/HtmlUtility";
import { Feature, TermGroup, TermGroup_ProjectStatus } from "../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../Core/Handlers/GridHandler";
import { IAccountingService } from "../../Shared/Economy/Accounting/AccountingService";
import { AccountDimSmallDTO } from "../../Common/Models/AccountDimDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Functions
    buttonFunctions: any = [];

    //Statuses
    projectStatuses: any = [];
    gridProjectStatuses: any = [];
    accountDims: AccountDimSmallDTO[] = [];

    gridFooterComponentUrl: any;

    private onlyMineLocked: boolean;
    private hasEditProjectPermission: boolean;
    private hasProjectCentralPermission: boolean;

    private _loadMine: boolean;
    get loadMine(): boolean {
        return this._loadMine;
    }
    set loadMine(value: boolean) {
        this._loadMine = value;
        this.reloadGridFromFilter();
    }

    private _projectStatusSelection: any;
    get projectStatusSelection() {
        return this._projectStatusSelection
    }
    set projectStatusSelection(value: any) {
        this._projectStatusSelection = value;
        this.reloadGridFromFilter();
    }
    
    //@ngInject
    constructor(
        private $window,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private invoiceService: IInvoiceService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Projects.Project", progressHandlerFactory, messagingHandlerFactory);
        this.onTabActivetedAndModified(() => this.reloadGridFromFilter());

        this.gridFooterComponentUrl = urlHelperService.getViewUrl("gridFooter.html");
        
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.beforeSetupGrid() )
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.reloadGridFromFilter());
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadGridFromFilter());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start({ feature: Feature.Billing_Project_Edit, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private loadProjectStatus(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ProjectStatus, false, false).then(x => {
            this.projectStatuses = x;
            this.projectStatusSelection = this.projectStatuses.filter(y => y.id == 2)[0];

            for (const element of this.projectStatuses) {
                let status = element;
                this.buttonFunctions.push({ id: status.id, name: status.name });
                this.gridProjectStatuses.push({ value: status.name, label: status.name });
            }
        });
    }

    private beforeSetupGrid(): ng.IPromise<any> {
        return this.$q.all([
            this.loadProjectStatus(),
            this.loadAccountDims(),
            this.loadPermissions()
        ]);
    }

    private loadAccountDims(): ng.IPromise<any>{
        return this.accountingService.getAccountDimsSmall(false, false, false, true).then((dims) => {
            this.accountDims = dims;
        });
    }

    private loadPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Billing_Project_Edit,
            Feature.Billing_Project_ProjectsUser,
            Feature.Billing_Project_Central
        ];
        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.hasEditProjectPermission = x[Feature.Billing_Project_Edit];
            this.onlyMineLocked = x[Feature.Billing_Project_ProjectsUser];
            this.hasProjectCentralPermission = x[Feature.Billing_Project_Central];
            this.loadMine = this.onlyMineLocked ? true : false;
        });
    }

    public setupGrid() {
        
        // Columns
        const keys: string[] = [
            "billing.projects.list.project",
            "billing.projects.list.status",
            "billing.projects.list.number",
            "billing.projects.list.name",
            "billing.projects.list.info",
            "billing.projects.list.categories",
            "billing.projects.list.customer",
            "billing.projects.list.underproject",
            "billing.projects.list.openprojectcentral",
            "billing.projects.list.leader",
            "core.edit",
            "common.all"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.projectStatuses.push({ id: 0, name: terms["common.all"] });
            //this.grid.addColumnSelect("statusName", terms["billing.projects.list.status"], "10%", this.gridProjectStatuses);
            this.gridAg.addColumnText("number", terms["billing.projects.list.number"], null);

            this.gridAg.addColumnText("name", terms["billing.projects.list.name"], null);
            this.gridAg.addColumnText("description", terms["billing.projects.list.info"], null, true);
            this.gridAg.addColumnText("categories", terms["billing.projects.list.categories"], null, true);
            this.gridAg.addColumnText("customerName", terms["billing.projects.list.customer"], null);
            this.gridAg.addColumnText("childProjects", terms["billing.projects.list.underproject"], null,true);
            this.gridAg.addColumnText("managerName", terms["billing.projects.list.leader"], null);

            if (this.accountDims) {
                this.accountDims.forEach((ad, i) => {
                    let index = i + 1;
                    if (ad.accountDimNr !== 1) {
                        this.gridAg.addColumnText("defaultDim" + index + "AccountName", ad.name, null, true, { enableHiding: true, hide: true });
                    }
                });
            }

            if (this.hasProjectCentralPermission) {
                const colDefProjectCentral = this.gridAg.addColumnIcon("projectId", null, null, { suppressFilter: true })
                if (colDefProjectCentral) {
                    colDefProjectCentral.cellRenderer = function (params) {
                        if (params.value) {
                            return '<a href="/soe/billing/project/central/?project=' + params.value + '"><span class="gridCellIcon fal fa-calculator-alt"></span></a>'
                        }
                    }
                }
            }

            if (this.hasEditProjectPermission)
                this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("billing.projects.list.project", true);
        });
    }

    public loadGridData() {
        // Load data
        if (this.projectStatusSelection) {
            this.progress.startLoadingProgress([() => {
                return this.invoiceService.getProjectsForList(this.projectStatusSelection.id, this.loadMine).then((x) => {
                    return x;
                }).then(data => {
                    this.setData(data);
                });
            }]);
        }
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

    private executeButtonFunction(option) {

        switch (option.id) {
            case ProjectListGridButtonFunctions.Planned:
                this.transfer(TermGroup_ProjectStatus.Planned);
                break;
            case ProjectListGridButtonFunctions.Active:
                this.transfer(TermGroup_ProjectStatus.Active);
                break;
            case ProjectListGridButtonFunctions.Locked:
                this.transfer(TermGroup_ProjectStatus.Locked);
                break;
            case ProjectListGridButtonFunctions.Ended:
                this.transfer(TermGroup_ProjectStatus.Finished);
                break;
            case ProjectListGridButtonFunctions.Hidden:
                this.transfer(TermGroup_ProjectStatus.Hidden);
                break;
        }
    }

    private transfer(newState: number) {
        var dict: any = [];

        //Create a collection of entries
        var rows = this.gridAg.options.getSelectedRows();
        
        if (rows.length === 0)
            return;

        _.forEach(rows, (y: any) => {
            dict.push(y.projectId);
        });

        this.invoiceService.updateProjectStatus(dict, newState).then((result) => {
            if (result.success) {
               // if (result.stringValue)
               //     this.failedWork(result.stringValue);
                this.loadGridData();
            }
            else {
                //this.failedSave(result.errorMessage);
            }
        }, error => {
            //this.failedWork(error.message);
        });

    }

    private openProjectCentral(row: any) {
        HtmlUtility.openInSameTab(this.$window, "/soe/billing/project/central/?project=" + row.projectId);
    }
}
