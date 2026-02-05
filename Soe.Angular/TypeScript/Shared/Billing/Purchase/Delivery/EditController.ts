import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/validationsummaryhandlerfactory";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { Feature, SettingMainType, UserSettingType, } from "../../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IconLibrary, PurchaseDeliveryEditSaveFunctions } from "../../../../Util/Enumerations";
import { ISupplierService } from "../../../Economy/Supplier/SupplierService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ToolBarButton, ToolBarUtility } from "../../../../Util/ToolBarUtility";
import { PurchaseDeliveryDTO, PurchaseDeliveryRowDTO, PurchaseDeliverySaveDTO } from "../../../../Common/Models/PurchaseDeliveryDTO";
import { SupplierHelper } from "../../Purchase/Helpers/SupplierHelper";
import { SupplierDTO } from "../../../../Common/Models/supplierdto";
import { IPurchaseService } from "./../Purchase/PurchaseService";
import { IActionResult, IPurchaseSmallDTO } from "../../../../Scripts/TypeLite.Net4";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { IShortCutService } from "../../../../Core/Services/ShortCutService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private purchaseId: number;
    private purchaseDeliveryId: number;
    private purchaseDelivery: PurchaseDeliveryDTO;
    private deliveryRows: PurchaseDeliveryRowDTO[] = [];

    private purchaseOrders: IPurchaseSmallDTO[] = [];
    private filteredPurchaseOrders: any[] = [];
    
    //permissions
    private orderRowsPermission: boolean = false;

    //directives
    private customerInvoiceRowsRendered: boolean = false;
    private reloadCustomerInvoiceRows;


    private _defaultDeliveryDate: Date = new Date();
    get defaultDeliveryDate() {
        return this._defaultDeliveryDate;
    }
    set defaultDeliveryDate(val: Date) {
        if (!val) return;
        this.deliveryRows = this.deliveryRows.map(r => {
            if (r.isLocked) {
                return r
            }
            else {
                r.deliveryDate = val;
                return r;
            }
        })
        this._defaultDeliveryDate = val;
    }

    private _selectedPurchase: IPurchaseSmallDTO;
    get selectedPurchase() {
        return this._selectedPurchase;
    }
    set selectedPurchase(item: IPurchaseSmallDTO) {
        this._selectedPurchase = item;
        if (item) {
            this.supplierHelper.setSupplierById(item.supplierId);
        }
    }
    
    private _setFinalDelivery: boolean = false;
    get setFinalDelivery() {
        return this._setFinalDelivery;
    }
    set setFinalDelivery(val: boolean) {
        if (val === undefined || val === null) val = false;

        this.deliveryRows = this.deliveryRows.map(r => {
            if (r.isLocked) return r;
            if (val !== r["setRowAsDelivered"])
                r.isModified = true;
            r["setRowAsDelivered"] = val;
            return r;
        })
        this._setFinalDelivery = val;
    }

    private _copyQty: boolean = false;
    get copyQty() {
        return this._copyQty;
    }
    set copyQty(val: boolean) {
        if (val !== this._copyQty) {
            this._copyQty = val;
            this.saveCopyQtySetting();
        }
    }

    //gui
    private saveFunctions: any = [];

    //helpers
    private supplierHelper: SupplierHelper;

    private originDescription: string;

    //@ngInject
    constructor(
        shortCutService: IShortCutService,
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private supplierService: ISupplierService,
        private purchaseService: IPurchaseService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.loadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.supplierHelper = new SupplierHelper(this, coreService, translationService, this.supplierService, urlHelperService, $q, $scope, $uibModal, (supplier: SupplierDTO) => { this.supplierChanged(supplier); });

        shortCutService.bindSaveAndClose($scope, () => { this.save(true); });
        shortCutService.bindSave($scope, () => { this.save(false); });
    }

    public onInit(parameters: any) {

        this.purchaseDeliveryId = parameters.id;
        this.purchaseId = parameters.purchaseId;

        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Purchase_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Orders_Edit_ProductRows, loadReadPermissions: false, loadModifyPermissions: true }
        ]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false,null);

        const statusGroup = ToolBarUtility.createGroup();

        statusGroup.buttons.push(new ToolBarButton("", "common.customer.invoices.reloadorder", IconLibrary.FontAwesome, "fa-sync", () => {
            this.loadData();
        }, null, () => {
            return false;
        }));

        this.toolbar.addButtonGroup(statusGroup);

        this.setupFunctions();
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Purchase_Purchase_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Purchase_Edit].modifyPermission;
        this.orderRowsPermission = response[Feature.Billing_Order_Orders_Edit_ProductRows].modifyPermission;
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadUserSettings(),
            this.loadPurchaseOrders(),
        ]).then(() => {
            this.loadSuppliers();
            this.filterPurchaseOrders(undefined);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.DeliveredQtySameAsPurchased];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this._copyQty = SettingsUtility.getBoolUserSetting(x, UserSettingType.DeliveredQtySameAsPurchased, false);
        });
    }

    private setupFunctions() {
        // Functions
        const keys: string[] = [
            "core.save",
            "core.saveandclose",
            "common.report.report.print",
            "common.email",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.saveFunctions = [];
            this.saveFunctions.push({ id: PurchaseDeliveryEditSaveFunctions.Save, name: terms["core.save"] + " (Ctrl+S)", icon: 'fal fa-fw fa-save' });
            this.saveFunctions.push({ id: PurchaseDeliveryEditSaveFunctions.SaveAndClose, name: terms["core.saveandclose"] + " (Ctrl+Enter)", icon: 'fal fa-fw fa-save' });
        });
    }    

    private executeSaveFunction(option) {
        switch (option.id) {
            case PurchaseDeliveryEditSaveFunctions.Save:
                this.save(false);
                break;
            case PurchaseDeliveryEditSaveFunctions.SaveAndClose:
                this.save(true);
                break;
        }
    }

    private loadPurchaseOrders(): ng.IPromise<any> {
        if (this.purchaseDeliveryId > 0) {
            return;
        }

        if (this.purchaseId) {
            return this.purchaseService.getPurchaseOrderSmall(this.purchaseId).then((data: IPurchaseSmallDTO) => {
                this.purchaseOrders.push(data);
                this.selectedPurchase = data;
                // Set value to display in typeahead
                _.forEach(this.purchaseOrders, (p) => p['displayValue'] = p.purchaseNr);
            });
        }
        else {
            return this.purchaseService.getPurchaseOrdersForSelect(true).then((data: IPurchaseSmallDTO[]) => {
                this.purchaseOrders = data;

                // Set value to display in typeahead
                _.forEach(this.purchaseOrders, (p) => p['displayValue'] = p.purchaseNr);
            });
        }
    }

    private loadSuppliers(): ng.IPromise<any> {
        if (this.purchaseId)
        {
            this.setSingleSupplier(this.selectedPurchase.supplierId, this.selectedPurchase.supplierName);
        }
        else if (!this.purchaseDeliveryId) {
            return this.supplierHelper.loadSuppliers(false).then(r => {
                this.supplierHelper.setSupplierById(this.purchaseDelivery.supplierId || this.selectedPurchase.supplierId);
            });
        }
    }


    protected filterPurchaseOrders(supplierNr: string) {
        this.filteredPurchaseOrders = [];
        this.filteredPurchaseOrders.push({ purchaseId: 0, name: " " });
        if (supplierNr) {
            this.filteredPurchaseOrders = this.purchaseOrders.filter(p => p.supplierNr === supplierNr);
        }
        else {
            this.filteredPurchaseOrders = [...this.purchaseOrders];
        }
    }

    private loadData(updateTab = false): ng.IPromise<any> {
        const deferral = this.$q.defer();
        if (this.purchaseDeliveryId) {
            this.isNew = false;
            this.purchaseService.getDelivery(this.purchaseDeliveryId).then((data: PurchaseDeliveryDTO) => {
                this.purchaseDelivery = data;
                this.defaultDeliveryDate = new Date(this.purchaseDelivery.deliveryDate);

                this.setSingleSupplier(this.purchaseDelivery.supplierId, this.purchaseDelivery.supplierName);
                                
                this.purchaseService.getDeliveryRows(this.purchaseDeliveryId).then((rows: PurchaseDeliveryRowDTO[]) => {
                    rows.forEach(r => r['setRowAsDelivered'] = r.isLocked);
                    this.deliveryRows = rows;
                    this.shouldOpenCustomerInvoiceRows();
                });

                if (updateTab) {
                    this.updateTabCaption();
                }

                deferral.resolve();
            });
        }
        else if (this.purchaseId) {
            this.new();

            this.selectedPurchase = _.find(this.purchaseOrders, (p) => p.purchaseId === this.purchaseId);

            if (this.selectedPurchase) {
                this.loadPurchase();
            }
            
            deferral.resolve();
        }
        else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadPurchase() {
        const purchaseId = this.selectedPurchase?.purchaseId || 0;
        const supplierId = this.supplierHelper?.selectedSupplier?.id || 0;
        this.originDescription = this.selectedPurchase?.originDescription || "";

        if (!purchaseId && !supplierId) {
            return;
        }

        return this.purchaseService.getDeliveryRowsFromPurchase(purchaseId, supplierId).then((data: PurchaseDeliveryRowDTO[]) => {
            this.deliveryRows = [];
            data.forEach((row: PurchaseDeliveryRowDTO) => {
                const rowDto = new PurchaseDeliveryRowDTO();
                angular.extend(rowDto, { ...row, deliveryDate: row.isLocked ? row.deliveryDate : this.defaultDeliveryDate });
                this.deliveryRows.push(rowDto);
            });

            if (this.copyQty) {
                this.updateDeliveryQty(false);
            }
        });
    }

    private shouldOpenCustomerInvoiceRows() {
        if (this.setFinalDelivery || this.deliveryRows.some(r => r.isLocked)) {
            if (!this.customerInvoiceRowsRendered)
                this.customerInvoiceRowsRendered = true;
            else if (this.reloadCustomerInvoiceRows) this.reloadCustomerInvoiceRows();
        }
    }

    private save(closeAfterSave: boolean): ng.IPromise<any> {
        return this.progress.startSaveProgress((completion) => {

            const saveData = new PurchaseDeliverySaveDTO();
            saveData.purchaseDeliveryId = this.purchaseDeliveryId;
            saveData.supplierId = this.purchaseDelivery.supplierId;
            saveData.deliveryDate = this.defaultDeliveryDate;

            saveData.rows = [];
            this.deliveryRows.filter(d => d.isModified ).forEach(r => {
                saveData.rows.push({ 
                    deliveredQuantity: r.deliveredQuantity,
                    deliveryDate: r.deliveryDate,
                    purchaseDeliveryRowId: r.purchaseDeliveryRowId,
                    purchasePrice: r.purchasePrice,
                    purchasePriceCurrency: r.purchasePriceCurrency,
                    purchaseRowId: r.purchaseRowId,
                    isModified: r.isModified,
                    setRowAsDelivered: r['setRowAsDelivered'],
                    purchaseNr: r.purchaseNr,
                });
            })

            this.purchaseService.saveDeliveryRows(saveData).then((result: IActionResult) => {
                if (result.success) {
                    this.isNew = false;
                    this.purchaseDelivery.purchaseDeliveryId = this.purchaseDeliveryId = result.integerValue;
                    this.purchaseDelivery.deliveryNr = result.integerValue2;

                    this.deliveryRows.forEach(r => {
                        r.isModified = false;
                        r.isLocked = true;
                    });

                    this.$scope.$broadcast('refreshRows');
                    this.updateTabCaption();
                    this.shouldOpenCustomerInvoiceRows();

                    completion.completed(this.getSaveEvent(), this.purchaseDelivery);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            });

        }, this.guid).then(data => {
            this.dirtyHandler.clean();
            if (closeAfterSave) {
                this.closeMe(true);
            }
        });
    }

    private new() {
        this.isNew = true;
        this.purchaseDelivery = new PurchaseDeliveryDTO();
        if (this.selectedPurchase) {
            this.purchaseDelivery.supplierId = this.selectedPurchase.supplierId;
        }
    }

    private setSingleSupplier(id:number, name:string) {
        this.supplierHelper.AddSetSupplier({ id: id, name: name })
    }

    private supplierChanged(supplier: SupplierDTO) {
        this.purchaseDelivery.supplierId = supplier.actorSupplierId ? supplier.actorSupplierId : 0;
        this.filterPurchaseOrders( supplier ? supplier.supplierNr : "" );
    }

    private updateTabCaption() {
        const termKey = this.isNew ? "billing.purchase.delivery.new_delivery" : "billing.purchase.delivery.delivery";
        this.translationService.translate(termKey).then((term) => {
            if (this.isNew)
                this.messagingHandler.publishSetTabLabel(this.guid, term);
            else
                this.messagingHandler.publishSetTabLabel(this.guid, term + " " + this.purchaseDelivery.deliveryNr, this.purchaseDeliveryId);
        });
    }

    private updateDeliveryQty(refresh: boolean) {
        var setDirty = false;
        this.deliveryRows.forEach(r => {
            if (!r.deliveredQuantity) {
                this.setDeliveredQuantity(r, r.remainingQuantity);
                r.isModified = true;
                setDirty = true;
            }
        });

        if (refresh) {
            this.$scope.$broadcast('refreshRows');
        }

        if (setDirty) {
            this.dirtyHandler.setDirty();
        }
    }

    private setDeliveredQuantity(row: PurchaseDeliveryRowDTO, newQuantity: number) {
        const diff = row.deliveredQuantity - newQuantity;
        row.remainingQuantity += diff;
        row.deliveredQuantity = newQuantity;
        row['setRowAsDelivered'] = row.remainingQuantity <= 0;
        row.isModified = true;
    }

    private saveCopyQtySetting() {
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.DeliveredQtySameAsPurchased, this.copyQty);
        if (this.copyQty) {
            this.updateDeliveryQty(true);
        }
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            const errors = this['edit'].$error;
        });
    }

    private isDisabled() {
        return !this.dirtyHandler.isDirty || this["edit"].$invalid;
    }

}