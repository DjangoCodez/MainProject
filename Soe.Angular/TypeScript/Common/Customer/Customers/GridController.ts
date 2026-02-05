import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICommonCustomerService } from "../CommonCustomerService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { Feature, SoeEntityType, SoeModule } from "../../../Util/CommonEnumerations";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";
import { ISoeCellValueChanged } from "../../../Util/SoeGridOptionsAg";
import { IconLibrary, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { BatchUpdateController } from "../../Dialogs/BatchUpdate/BatchUpdateDirective";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridEvent } from "../../../Util/SoeGridOptions";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Permissions
    private categoriesPermission = false;
    private editContactPersonPermission = false;
    private batchUpdatePermission = false;
    private privatePersonsModified = [];
    modalInstance: any;

    private selectedCount: number = 0;

    //@ngInject
    constructor($uibModal,
        private $window,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private selectedItemsService: ISelectedItemsService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Common.Customer.Customers", progressHandlerFactory, messagingHandlerFactory);
        this.setIdColumnNameOnEdit("actorCustomerId");
        super.onTabActivetedAndModified(() => {
            this.loadGridData();
        });

        this.selectedItemsService.setup($scope, "actorCustomerId", (items: number[]) => this.save(items));

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadModifyPermissions())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
        
        const feature = soeConfig.moduleName === SoeModule[SoeModule.Economy].toLowerCase() ? 
            Feature.Economy_Customer_Customers 
            : Feature.Billing_Customer_Customers;
        this.flowHandler.start({ feature: feature, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData(), true, () => this.selectedItemsService.Save(), () => { return this.saveButtonIsDisabled() });
        
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.batchupdate.title", "common.batchupdate.title", IconLibrary.FontAwesome, "fa-pencil",
            () => { this.openBatchUpdate(); }, () => { return this.selectedCount == 0; }, () => { return !this.batchUpdatePermission }
        )));
    }

    private saveButtonIsDisabled(): boolean
    {
        return (this.privatePersonsModified.length === 0 && !this.selectedItemsService.SelectedItemsExist() );
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "common.active",
            "common.number",
            "common.name",
            "common.orgnr",
            "common.categories.categories",
            "common.customer.customer.invoicereference",
            "common.customer.customer.opencustomercentral",
            "common.customer.customer.showcontactpersons",
            "core.edit",
            "common.service",
            "common.contactaddresses.addressmenu.visiting",
            "common.contactaddresses.addressmenu.billing",
            "common.contactaddresses.addressmenu.delivery",
            "common.contactaddresses.ecommenu.phonehome",
            "common.email",
            "common.privateperson",
            "common.contactaddresses.ecommenu.phonemobile",
            "common.contactaddresses.ecommenu.phonejob",
            "common.customer.customer.invoicedeliverytype"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnActive("isActive", terms["common.active"], 60, (params) => this.selectedItemsService.CellChanged(params));
            this.gridAg.addColumnText("customerNr", terms["common.number"], 100, null, { sort: 'desc' });
            this.gridAg.addColumnText("name", terms["common.name"], null, true);
            this.gridAg.addColumnText("orgNr", terms["common.orgnr"], null, true);
            if (this.categoriesPermission)
                this.gridAg.addColumnText("categories", terms["common.categories.categories"], null, true);
            this.gridAg.addColumnText("invoiceReference", terms["common.customer.customer.invoicereference"], null, true);
            this.gridAg.addColumnText("gridPaymentServiceText", terms["common.service"], null, true);
            this.gridAg.addColumnText("invoiceDeliveryTypeText", terms["common.customer.customer.invoicedeliverytype"], null, true, { hide: true });
            this.gridAg.addColumnText("gridAddressText", terms["common.contactaddresses.addressmenu.visiting"], null, true);
            this.gridAg.addColumnText("gridBillingAddressText", terms["common.contactaddresses.addressmenu.billing"], null, true); 
            this.gridAg.addColumnText("gridDeliveryAddressText", terms["common.contactaddresses.addressmenu.delivery"], null, true);

            this.gridAg.addColumnText("gridHomePhoneText", terms["common.contactaddresses.ecommenu.phonehome"], null, true);
            this.gridAg.addColumnText("gridMobilePhoneText", terms["common.contactaddresses.ecommenu.phonemobile"], null, true);
            this.gridAg.addColumnText("gridWorkPhoneText", terms["common.contactaddresses.ecommenu.phonejob"], null, true);
            
            this.gridAg.addColumnText("gridEmailText", terms["common.email"], null, true);

            this.gridAg.addColumnBool("isPrivatePerson", terms["common.privateperson"], 30, true, this.isPrivateChanged.bind(this));

            const colDefCustomerCentral = this.gridAg.addColumnIcon(null, terms["common.customer.customer.opencustomercentral"], null, { suppressExport: true });
            if (colDefCustomerCentral) {
                colDefCustomerCentral.cellRenderer = function (params) {
                    if (params.data) {
                        
                        return '<a href="/soe/' + soeConfig.moduleName + '/customer/customercentral/?customer=' + params.data.actorCustomerId + '"><span class="gridCellIcon fal fa-calculator-alt"></span></a>'
                    }
                }
            }
            if (this.editContactPersonPermission)
                this.gridAg.addColumnIcon(null, terms["common.customer.customer.showcontactpersons"], null, { suppressExport: true, icon: "fal fa-male", onClick: this.showContactPersons.bind(this) });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rowNode) => { this.selectionChanged() }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (rowNode) => { this.selectionChanged() }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("common.customer.customer.customers", true, undefined, true);
        });
    }

    private isPrivateChanged(params: ISoeCellValueChanged): void {

        const row = params.data;
        const exist = this.privatePersonsModified.filter((x) => x.id == row.actorCustomerId);

        if (exist && exist.length > 0) {
            exist[0].isPrivatePerson = row.isPrivatePerson;
            return;
        }

        this.privatePersonsModified.push({ id: row.actorCustomerId, isPrivatePerson: row.isPrivatePerson });
        this.$scope.$applyAsync();
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Manage_ContactPersons_Edit,
            Feature.Common_Categories_Customer,
            Feature.Economy_Customer_Customers_BatchUpdate,
            Feature.Billing_Customer_Customers_BatchUpdate
        ];

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (x[Feature.Manage_ContactPersons_Edit])
                this.editContactPersonPermission = true;
            if (x[Feature.Common_Categories_Customer])
                this.categoriesPermission = true;
            if (x[Feature.Economy_Customer_Customers_BatchUpdate] || x[Feature.Billing_Customer_Customers_BatchUpdate])
                this.batchUpdatePermission = true;
        });
    }

    selectionChanged() {
        this.$timeout(() => {
            this.selectedCount = this.gridAg.options.getSelectedCount();
            console.log(this.selectedCount)
        });
    }

    public loadGridData() {
        this.privatePersonsModified = [];
        
        // Load data
        this.progress.startLoadingProgress([() => {
            return this.commonCustomerService.getCustomers(false).then((customers: any[]) => {
                this.setData(customers);
                this.selectedCount = 0;
            });
        }]);
    }

    private openBatchUpdate() {

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/BatchUpdate/Views/BatchUpdate.html"),
            controller: BatchUpdateController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                entityType: () => { return SoeEntityType.Customer },
                selectedIds: () => { return _.map(this.gridAg.options.getSelectedRows(), 'actorCustomerId') }
            }
        });

        modal.result.then(data => {
            // Reset cache
            this.loadGridData();
        }, function () {
            // Cancelled
        });
        this.$scope.$applyAsync();
        return modal;
    }

    private save(items: number[]) {
        const dict: any = {};

        _.forEach(items, (id: number) => {
            // Find entity
            var entity: any = this.gridAg.options.findInData((ent: any) => ent["actorCustomerId"] === id);

            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if ( (dict !== undefined) && (Object.keys(dict).length > 0) ) {
            this.commonCustomerService.updateCustomersState(dict).then(() => {
                this.loadGridData();
            });
        }

        if (this.privatePersonsModified.length > 0) {
            this.commonCustomerService.updateCustomersIsPrivatePerson(this.privatePersonsModified).then(() => {
                this.privatePersonsModified = [];
            })
        }
    }

    private showContactPersons(row) {
        HtmlUtility.openInSameTab(this.$window, "/soe/manage/contactpersons/?actor=" + row.actorSupplierId);
    }
}