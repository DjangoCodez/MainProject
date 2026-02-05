import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Feature, SoeOriginStatusClassificationGroup } from "../../../Util/CommonEnumerations";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private privatePersonsModified = [];
    editContactPersonPermission = false;
    
    //@ngInject
    constructor(
        private $window,
        $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        private selectedItemsService: ISelectedItemsService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Supplier.Suppliers", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('actorSupplierId', 'name');
        this.selectedItemsService.setup($scope, "actorSupplierId", (items: number[]) => this.save(items));

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(false); });
        }

        this.flowHandler.start([
            { feature: Feature.Economy_Supplier_Suppliers_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Manage_ContactPersons_Edit, loadModifyPermissions: true }
        ]);
    }

    openSupplierCentral(row) {
        HtmlUtility.openInSameTab(this.$window, "/soe/economy/supplier/suppliercentral/?supplier=" + row.actorSupplierId + "&classificationgroup=" + SoeOriginStatusClassificationGroup.HandleSupplierInvoicesAttestFlow);
    }

    showContactPersons(row) {
        HtmlUtility.openInSameTab(this.$window, "/soe/manage/contactpersons/?actor=" + row.actorSupplierId);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData(false), this.modifyPermission, () => this.selectedItemsService.Save(), () => { return this.saveButtonIsDisabled() });
    }

    private saveButtonIsDisabled(): boolean {
        return (this.privatePersonsModified.length === 0 && !this.selectedItemsService.SelectedItemsExist());
    }

    private setUpGrid() {
        // Columns
        const keys: string[] = [
            "common.active",
            "common.number",
            "common.name",
            "common.orgnrshort",
            "common.categories",
            "common.contactaddresses.ecommenu.phonehome",
            "common.contactaddresses.ecommenu.phonejob",
            "common.contactaddresses.ecommenu.phonemobile",
            "common.email",
            "economy.supplier.invoice.paytoaccount",
            "economy.supplier.supplier.opensuppliercentral",
            "economy.supplier.supplier.showcontactpersons",
            "economy.accounting.paymentcondition.paymentcondition",
            "core.edit",
            "common.privateperson",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];
        
        this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnActive("actorSupplierId", terms["common.active"], 60, (params) => this.selectedItemsService.CellChanged(params) );
            this.gridAg.addColumnText("supplierNr", terms["common.number"], 80, null, { sort: 'desc' });
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("orgNr", terms["common.orgnrshort"], null);
            this.gridAg.addColumnText("categories", terms["common.categories"], null);
            this.gridAg.addColumnText("homePhone", terms["common.contactaddresses.ecommenu.phonehome"], null, true, { enableHiding: true, hide: true });
            this.gridAg.addColumnText("workPhone", terms["common.contactaddresses.ecommenu.phonejob"], null, true);
            this.gridAg.addColumnText("mobilePhone", terms["common.contactaddresses.ecommenu.phonemobile"], null, true);
            this.gridAg.addColumnText("email", terms["common.email"], null, true);
            this.gridAg.addColumnText("payToAccount", terms["economy.supplier.invoice.paytoaccount"], null, true, { enableHiding: true, hide:true });
            this.gridAg.addColumnText("paymentCondition", terms["economy.accounting.paymentcondition.paymentcondition"], null, true, { enableHiding: true, hide: true });
            this.gridAg.addColumnBool("isPrivatePerson", terms["common.privateperson"], 40, true, this.isPrivateChanged.bind(this));
            const colDefSupplierCentral = this.gridAg.addColumnIcon(null, terms["economy.supplier.supplier.opensuppliercentral"], 30, { suppressExport: true });
            if (colDefSupplierCentral) {
                colDefSupplierCentral.cellRenderer = function (params) {
                    if (params.data) {
                        return '<a href="/soe/economy/supplier/suppliercentral/?supplier=' + params.data.actorSupplierId + '"><span class="gridCellIcon fal fa-calculator-alt"></span></a>'
                    }
                }
            }
            if (this.editContactPersonPermission)
                this.gridAg.addColumnIcon(null, terms["economy.supplier.supplier.showcontactpersons"], 30, { suppressExport: true, icon: "fal fa-male", onClick: this.showContactPersons.bind(this) });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("economy.supplier.supplier.suppliers", true,undefined,true);
        });
    }

    private isPrivateChanged(row): void {
        this.$timeout(() => {
            const exist = this.privatePersonsModified.filter((x) => x.id === row.data.actorSupplierId);

            if (exist && exist.length > 0) {
                exist[0].isPrivatePerson = row.data.isPrivatePerson;
                return;
            }

            this.privatePersonsModified.push({ id: row.data.actorSupplierId, isPrivatePerson: row.data.isPrivatePerson });
        });
    }

    private loadGridData(useCache = true) {
        this.gridAg.clearData();
        this.privatePersonsModified = [];

        this.progress.startLoadingProgress([() => {
            return this.supplierService.getSuppliers(false, useCache).then(data => {
                this.setData(data);
            });
        }]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Economy_Supplier_Suppliers_Edit].modifyPermission;
        this.editContactPersonPermission = response[Feature.Manage_ContactPersons_Edit].modifyPermission;
        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private save(items: number[]) {
        const dict: any = {};

        _.forEach(items, (id: number) => {
            // Find entity
            const entity: any = this.gridAg.options.findInData((ent: any) => ent["actorSupplierId"] === id);

            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if (this.privatePersonsModified.length > 0) {
            this.supplierService.updateSuppliersIsPrivatePerson(this.privatePersonsModified).then(() => {
                this.privatePersonsModified = [];
            })
        }

        if ((dict !== undefined) && (Object.keys(dict).length > 0)) {

            this.supplierService.updateSuppliersState(dict).then(() => {
                this.loadGridData(false);
            });
        }
    }
}