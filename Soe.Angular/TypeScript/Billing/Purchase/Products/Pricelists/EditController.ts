import { SupplierDTO } from "../../../../Common/Models/supplierdto";
import { SupplierProductPriceComparisonDTO, SupplierProductPricelistDTO } from "../../../../Common/Models/SupplierProductDTO";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/validationsummaryhandlerfactory";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IActionResult, IImportOptionsDTO, ISupplierProductImportDTO, ISupplierProductImportRawDTO, ISupplierProductPriceComparisonDTO, ISupplierProductPricelistDTO, ISupplierProductPriceSearchDTO } from "../../../../Scripts/TypeLite.Net4";
import { ISupplierProductService } from "../../../../Shared/Billing/Purchase/Purchase/SupplierProductService";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { Feature } from "../../../../Util/CommonEnumerations";
import { SupplierHelper } from "../../../../Shared/Billing/Purchase/Helpers/SupplierHelper";
import { ImportDynamicController } from "../../../Dialogs/ImportDynamic/ImportDynamic";
import { ToolBarButton, ToolBarUtility } from "../../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../../Util/Enumerations";
import { PurchaseAmountHelper } from "../../../../Shared/Billing/Purchase/Helpers/PurchaseAmountHelper";
import { CurrencyHelper } from "../../../../Common/Directives/Helpers/CurrencyHelper";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private pricelistId: number = 0;
    private pricelistIds: number[] = [];

    private pricelist: ISupplierProductPricelistDTO;

    private supplierHelper: SupplierHelper;

    private priceRows: SupplierProductPriceComparisonDTO[] = [];
    private priceRowsLoaded: boolean = false;
    private includePricelessProducts: boolean = false;

    //gui
    private showNavigationButtons = true;
    modalInstance: any;

    private amountHelper: PurchaseAmountHelper;
    public currencies: any[];
    public isReload: boolean = false;
    public currencyHelper: CurrencyHelper;

    //@ngInject
    constructor(
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        supplierService: ISupplierService,
        private supplierProductService: ISupplierProductService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private notificationService: INotificationService,    ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        this.modalInstance = $uibModal;

        this.amountHelper = new PurchaseAmountHelper(coreService, $timeout, $q, () => null);
        this.currencyHelper = new CurrencyHelper(coreService, $timeout, this.$q);
        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.loadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.supplierHelper = new SupplierHelper(this, coreService, translationService, supplierService, urlHelperService, $q, $scope, $uibModal, (supplier: SupplierDTO) => { this.supplierChanged(supplier); });
    }

    private currencyChanged(item) {
        this.pricelist.currencyId = item;
        if (this.priceRows.filter(function (r) { return r.isModified; }).length > 0) {
            this.notificationService.showConfirmOnContinue().then(close => {
                if (close) {
                    this.amountHelper.transactionCurrencyRate;
                    this.loadPriceRows();
                }
            });
        }
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;
        if (parameters.id) {
            this.pricelistId = parameters.id;
            this.isNew = false;
        } else {
            this.pricelistId = undefined;
            this.isNew = true;
        }

        if (parameters.ids && parameters.ids.length > 0) {
            this.pricelistIds = parameters.ids;
        }
        else {
            this.showNavigationButtons = false;
        }

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Pricelists, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Suppliers_Edit, loadReadPermissions: false, loadModifyPermissions: true },
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Purchase_Pricelists].readPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Pricelists].modifyPermission;

        this.supplierHelper.setPermissions(response);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        //Navigation
        this.toolbar = toolbarFactory.createDefaultEditToolbar(
            true,
            () => {
                this.isNew = true;
                this.pricelistId = undefined;
                this.pricelist.supplierProductPriceListId = undefined;
                this.priceRows.forEach(r => {
                    r.supplierProductPriceId = undefined;
                    r.supplierProductPriceListId = undefined;
                    r.isModified = true;
                })

                this.updateTabCaption();
                this.dirtyHandler.setDirty();
            },
            () => this.isNew
        );

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton(
            "billing.purchase.product.importpricelist",
            "billing.purchase.product.importpricelist",
            IconLibrary.FontAwesome, "fa-file-import",
            () => this.importDialog(),
            () => !this.pricelist.supplierProductPriceListId || this.dirtyHandler.isDirty,
        )));

        this.toolbar.setupNavigationGroup(() => { return this.isNew }, null, (newsupplierProductId) => {
            this.pricelistId = newsupplierProductId;
            this.loadData(true);
        }, this.pricelistIds, this.pricelistId);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadSuppliers(),
            this.amountHelper.loadCurrencies(),
        ]);
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
        });
    }

    private loadSuppliers(): ng.IPromise<any> {
        return this.supplierHelper.loadSuppliers(true);
    }

    private supplierChanged(supplier: SupplierDTO) {
        this.pricelist = {
            ...this.pricelist,
            supplierId: supplier.actorSupplierId,
            supplierName: supplier.name,
        }
        if (!this.pricelistId) {
            this.amountHelper.currencyId = supplier.currencyId
        }
    }

    private loadData(updateTab = false) {
        this.pricelist = new SupplierProductPricelistDTO(this.pricelistId);
        this.priceRows = [];
        this.priceRowsLoaded = false;
        if (this.isNew) {
            this.pricelist = {
                ...this.pricelist,
            }
            this.pricelist.currencyId = this.amountHelper.currencyId;
        } else {
            this.supplierProductService.getSupplierPricelist(this.pricelistId).then(data => {
                this.amountHelper.currencyId = data.currencyId;
                data.startDate = data.startDate ? new Date(data.startDate) : undefined;
                data.endDate = data.endDate ? new Date(data.endDate) : undefined;
                this.pricelist = {
                    ...this.pricelist,
                    ...data
                }

                this.supplierHelper.setSupplierById(this.pricelist.supplierId);
                this.loadPriceRows();
                this.updateTabCaption();
                this.$scope.$applyAsync();
            })
        }
        return this.supplierHelper.loadSuppliers(true);
    }

    private loadPriceRows() {
        this.priceRowsLoaded = true;
        if (this.pricelistId) {
            return this.supplierProductService.getSupplierPricelistPrices(this.pricelistId, true).then(data => {
                /*let typedData = data as SupplierProductPriceComparisonDTO[];
                typedData = typedData.map(r => {
                    r.startDate = new Date(r.startDate)
                    return r;
                });*/
                this.priceRows = this.fixDates(data as SupplierProductPriceComparisonDTO[]);
            })
        }
    }

    private fixDates(data: SupplierProductPriceComparisonDTO[]): SupplierProductPriceComparisonDTO[] {
        const startDate = new Date("1901-01-02");
        const stopDate = new Date("9998-12-31");

        return data.map(r => {
            const rowStart = CalendarUtility.convertToDate(r.startDate);
            const rowEnd = CalendarUtility.convertToDate(r.endDate);
            const rowStartCompare = CalendarUtility.convertToDate(r.compareStartDate);
            const rowEndCompare = CalendarUtility.convertToDate(r.compareEndDate);

            if (rowStart < startDate)
                r.startDate = null;
            if (rowEnd > stopDate || rowEnd < startDate)
                r.endDate = null;
            if (rowStartCompare <= startDate)
                r.compareStartDate = null;
            if (rowEndCompare >= stopDate)
                r.compareEndDate = null;
            return r;
        });
    }

    private endDateChanged(newValue: Date) {
        this.priceRows.forEach(r => {
            r.endDate = this.pricelist.endDate;
            r.isModified = true;
        })
        this.priceRows = this.fixDates(this.priceRows);
    }

    public loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    private save(): ng.IPromise<any> {
        this.priceRows.forEach(r => {
            r.startDate = this.pricelist.startDate;
            r.endDate = this.pricelist.endDate;
        });

        return this.progress.startSaveProgress((completion) => {
            return this.supplierProductService.saveSupplierPricelist({
                priceList: this.pricelist,
                priceRows: this.priceRows.filter(r => r.isModified)
            }).then(result => {
                if (result.success) {
                    this.pricelist.supplierProductPriceListId = this.pricelistId = result.integerValue;
                    this.priceRows.forEach(r => {
                        r.isModified = false;
                    });
                    this.$scope.$broadcast('refreshRows');
                    completion.completed();
                } else {
                    completion.failed(result.errorMessage, false);
                }
            })
        }, this.guid).then(data => {
            this.isNew = false;
            this.dirtyHandler.clean();
            this.updateTabCaption()
        });
    }

    private importDialog() {
        const importCallback = (data: ISupplierProductImportRawDTO[], options: IImportOptionsDTO) => {
            const model: ISupplierProductImportDTO = {
                //importProduct: true,
                importToPriceList: true,
                importPrices: true,
                supplierId: this.pricelist.supplierId,
                priceListId: this.pricelist.supplierProductPriceListId,
                rows: data,
                options: options
            }
            return this.supplierProductService.performSupplierProductImport(model)
        }
        this.supplierProductService.getSupplierPricelistImport(true, true, false).then(data => {
            var modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Billing/Dialogs/ImportDynamic/ImportDynamic.html"),
                controller: ImportDynamicController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    translationService: () => this.translationService,
                    importDynamicDTO: () => data,
                    callback: () => importCallback,
                }
            });

            modal.result.then((result: any) => {
                this.loadData(false);
            });
        })
    }

    private delete() {
        this.progress.startLoadingProgress([
            () => this.supplierProductService.deleteSupplierPricelist(this.pricelistId).then(() => this.closeMe(true))
        ])
    }

    private updateTabCaption() {
        const termKey = this.isNew ? "billing.purchase.pricelists.new" : "billing.purchase.pricelists.pricelist";
        this.translationService.translate(termKey).then((term) => {
            if (this.isNew)
                this.messagingHandler.publishSetTabLabel(this.guid, term);
            else
                this.messagingHandler.publishSetTabLabel(this.guid, term + " " + this.pricelist.supplierName);
        });
    }

    private isReadonly() {
        return !this.isNew;
    }

    public isDisabled() {
        return !this.dirtyHandler.isDirty || this['edit'].$invalid;
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            const errors = this['edit'].$error;
        });
    }
}