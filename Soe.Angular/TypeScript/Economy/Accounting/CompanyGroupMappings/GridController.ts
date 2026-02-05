import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { SoeGridOptionsEvent, IconLibrary } from "../../../Util/Enumerations";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { AddCompanyGroupMappingController } from "./Dialogs/AddCompanyGroupMapping/AddCompanyGroupMapping";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //Properties
    private usedCompanyIds: number[];

    //modal
    private modalInstance: any;

    //@ngInject
    constructor(
        $uibModal,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,) {
        super(gridHandlerFactory, "Economy.Accounting.CompanyGroup.CompanyGroupMapping", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Accounting_CompanyGroup_TransferDefinitions, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());

        var groupAdd = ToolBarUtility.createGroup(new ToolBarButton("common.createnew", "common.createnew", IconLibrary.FontAwesome, "fa-plus", () => {
            this.openCompanyGroupMappingDialog(null);
        }));
        this.toolbar.addButtonGroup(groupAdd);
    }

    protected setupGrid() {
        // Columns
        var keys: string[] = [
            "common.number",
            "common.name",
            "common.description",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("number", terms["common.number"], null);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("economy.accounting.companygroup.mappings", true);
        });
    }

    edit(row) {
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission)) 
            this.openCompanyGroupMappingDialog(row);
    }

    public loadGridData() {
        // Load data
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getCompanyGroupMappings().then(data => {
                this.setData(data);
            });
        }]);
    }

    protected openCompanyGroupMappingDialog(row?: any) {
        // handle excluded
        var excluded = row ? _.filter(this.usedCompanyIds, (id) => id !== row.childActorCompanyId) : this.usedCompanyIds;

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Economy/Accounting/CompanyGroupMappings/Dialogs/AddCompanyGroupMapping/AddCompanyGroupMapping.html"),
            controller: AddCompanyGroupMappingController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                accountingService: () => { return this.accountingService },
                notificationService: () => { return this.notificationService },
                commonCustomerService: () => { return null },
                companyGroupMappingId: () => { return row ? row.companyGroupMappingHeadId : undefined },
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                if (result.delete) {
                    this.progress.startDeleteProgress((completion) => {
                        this.accountingService.deleteCompanyGroupMapping(result.item.companyGroupMappingHeadId).then((result) => {
                            if (result.success)
                                completion.completed(null);
                            else
                                completion.failed(result.errorMessage);
                        });
                    }, null).then(() => {
                        this.loadGridData();
                    });
                }
                else if(result.item) {
                    this.progress.startSaveProgress((completion) => {
                        this.accountingService.saveCompanyGroupMapping(result.item).then((result) => {
                            if (result.success)
                                completion.completed();
                            else
                                completion.failed(result.errorMessage);
                        });
                    }, null).then(() => {
                        this.loadGridData();
                    });
                }
            }
        });
    }
}
