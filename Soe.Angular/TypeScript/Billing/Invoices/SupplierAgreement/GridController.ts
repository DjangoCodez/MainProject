import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Feature, SoeSupplierAgreemntCodeType } from "../../../Util/CommonEnumerations";
import { SupplierAgreementButtonFunctions } from "../../../Util/Enumerations";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { DeleteSupplierAgreementController } from "./Dialogs/DeleteSupplierAgreement/DeleteSupplierAgreementController";
import { ImportSupplierAgreementController } from "./Dialogs/ImportSupplierAgreement/ImportSupplierAgreementController";
import { SetGeneralDiscountController } from "./Dialogs/SetGeneralDiscount/SetGeneralDiscountController";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { SupplierAgreementDTO } from "../../../Common/Models/SupplierAgreementDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private readonly AllSuppliers = 99999;
    private wholesellersDict: ISmallGenericType[] = [];
    private pricelists: any[] = [];
    private buttonFunctions: any[] = [];
    private codeTypes: ISmallGenericType[] = [];

    private _selectedProvider: ISmallGenericType;
    get selectedProvider() {
        return this._selectedProvider;
    }
    set selectedProvider(item: ISmallGenericType) {
        if (this._selectedProvider !== item) {
            this._selectedProvider = item;
            this.loadGridData();
        }
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal,
        private invoiceService: IInvoiceService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Invoices.SupplierAgreements", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.onBeforeSetUpGrid())
            .onSetUpGrid(() => this.setupGrid())
    }

    public onInit(parameters: any) {
        this.flowHandler.start([
            { feature: Feature.Billing_Preferences_InvoiceSettings_SupplierAgreement, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Preferences_InvoiceSettings_SupplierAgreement_Edit, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Preferences_InvoiceSettings_SupplierAgreement].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_InvoiceSettings_SupplierAgreement_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());

        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));

        this.setupButtonFunctions();
    }
    
    private loadPriceLists(): ng.IPromise<any> {
        return this.invoiceService.getPriceListsDict(true).then((x: ISmallGenericType[]) => {
            this.pricelists = x;
        });
    }

    private loadAgreementsProviders(): ng.IPromise<any> {
        
        return this.invoiceService.getSupplierAgreementProviders().then((data: ISmallGenericType[]) => {
            this.wholesellersDict = data;
            const keys = [
                "common.all",
                "common.searchinvoiceproduct.selectwholeseller"
            ]
            this.translationService.translateMany(keys).then((terms) => {
                this.wholesellersDict.unshift(new SmallGenericType(this.AllSuppliers, terms["common.all"]));
                const firstRow = new SmallGenericType(0, terms["common.searchinvoiceproduct.selectwholeseller"])
                this.wholesellersDict.unshift(firstRow);
                this.selectedProvider = firstRow;
            })
        });
    }

    public loadGridData() {
        if (!this.selectedProvider || this.selectedProvider.id === 0) {
            this.setData([]);
            return;
        }
        this.progress.startLoadingProgress([() => {
            return this.invoiceService.getSupplierAgreements(this.selectedProvider && this.selectedProvider.id != this.AllSuppliers ? this.selectedProvider.id : 0).then((x) => {
                this.setData(x);
            });
        }]);
    }

    private setupSoeSupplierAgreemntCodeTypes(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.invoices.supplieragreement.materialclass",
            "billing.invoices.supplieragreement.productnr",
            "common.general"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.codeTypes.push({ id: <number>SoeSupplierAgreemntCodeType.Generic, name: terms["common.general"] });
            this.codeTypes.push({ id: <number>SoeSupplierAgreemntCodeType.MaterialCode, name: terms["billing.invoices.supplieragreement.materialclass"] });
            this.codeTypes.push({ id: <number>SoeSupplierAgreemntCodeType.Product, name: terms["billing.invoices.supplieragreement.productnr"] });
        });
    }

    private onBeforeSetUpGrid(): ng.IPromise<any> {
        return this.$q.all([this.loadPriceLists(), this.loadAgreementsProviders(), this.setupSoeSupplierAgreemntCodeTypes()]);
    }

    private setupGrid() {
        this.gridAg.options.enableRowSelection = false;

        const keys: string[] = [
            "common.customer.customer.wholesellername",
            "billing.order.pricelisttype",
            "billing.invoices.supplieragreement.materialclassproductnr",
            "billing.productrows.dialogs.discountpercent",
            "common.date",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnSelect("wholesellerName", terms["common.customer.customer.wholesellername"], null, { displayField: "wholesellerName", selectOptions: this.wholesellersDict, dropdownValueLabel: "name", populateFilterFromGrid: true });
            this.gridAg.addColumnSelect("priceListTypeName", terms["billing.order.pricelisttype"], null, { displayField: "priceListTypeName", selectOptions: this.pricelists, dropdownValueLabel: "name" });
            this.gridAg.addColumnText("code", terms["billing.invoices.supplieragreement.materialclassproductnr"], null);
            this.gridAg.addColumnNumber("discountPercent", terms["billing.productrows.dialogs.discountpercent"], null, null);
            this.gridAg.addColumnDate("date", terms["common.date"], null);
            if (this.modifyPermission) {
                this.gridAg.addColumnEdit(terms["core.edit"], this.initAddDiscount.bind(this), false, (row: SupplierAgreementDTO) => { return row.sysWholesellerId === 23 || row.sysWholesellerId === 94 || row.codeType === SoeSupplierAgreemntCodeType.Generic });
            }

            this.gridAg.finalizeInitGrid("billing.invoices.supplieragreements.agreements", true);
        });
    }

    private setupButtonFunctions() {
        const keys: string[] = [
            "billing.invoices.supplieragreement.adddiscount",
            "billing.invoices.supplieragreement.deleteagreement",
            "billing.invoices.supplieragreement.addagreement",
            "billing.invoices.supplieragreement.addgeneraldiscount"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.buttonFunctions.push({ id: SupplierAgreementButtonFunctions.AddAgreement, name: terms["billing.invoices.supplieragreement.addagreement"], icon: 'fal fa-plus' });
            this.buttonFunctions.push({ id: SupplierAgreementButtonFunctions.RemoveAgreement, name: terms["billing.invoices.supplieragreement.deleteagreement"], icon: 'fal fa-times iconDelete', disabled: () => { return false } });
            this.buttonFunctions.push({ id: SupplierAgreementButtonFunctions.AddRow, name: terms["billing.invoices.supplieragreement.adddiscount"], icon: 'fal fa-plus' });
        });
    }

    private executeButtonFunction(option) {
        switch (option.id) {
            case SupplierAgreementButtonFunctions.AddAgreement:
                this.initAddSupplierAgreement();
                break;
            case SupplierAgreementButtonFunctions.RemoveAgreement:
                this.initDeleteSupplierAgreement();
                break;
            case SupplierAgreementButtonFunctions.AddRow:
                this.initAddDiscount(null);
                break;
        }
    }

    private initAddDiscount(row: SupplierAgreementDTO) {
        if (!row) {
            row = new SupplierAgreementDTO();
            row.codeType = SoeSupplierAgreemntCodeType.Generic;
            row.sysWholesellerId = this.selectedProvider?.id ?? 0;
        }
        else if (row.rebateListId) {
            //dialog things it is supplier agreement providers ids
            row.sysWholesellerId = this.wholesellersDict.find(x => x.name === row.wholesellerName).id;
        }

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Invoices/SupplierAgreement/Dialogs/SetGeneralDiscount/Views/setgeneraldiscount.html"),
            controller: SetGeneralDiscountController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                wholesellers: () => { return this.wholesellersDict },
                pricelistTypes: () => { return this.pricelists },
                codeTypes: () => { return this.codeTypes },
                row: () => { return row }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.addDiscount(result);
            }
        }, () => {
            //Cancel
        });
    }

    private addDiscount(inputResult: any) {

        const dto = inputResult.row;

        this.progress.startSaveProgress((completion) => {
            this.invoiceService.saveSupplierAgreementDiscount(dto).then(( saveResult ) => {
                if (saveResult.success) {
                    this.loadGridData();
                    completion.completed(null, null, true);
                }
                else {
                    completion.failed(saveResult.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null);
    }

    private initAddSupplierAgreement() {

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Invoices/SupplierAgreement/Dialogs/ImportSupplierAgreement/Views/importsupplieragreement.html"),
            controller: ImportSupplierAgreementController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                wholesellers: () => { return this.wholesellersDict },
                pricelistTypes: () => { return this.pricelists }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.addSupplierAgreement(result);
            }
        }, () => {
            //Cancel
        });
    }

    private addSupplierAgreement(inputResult: any) {
        this.progress.startSaveProgress((completion) => {
            this.invoiceService.saveSupplierAgreement(inputResult.wholesellerId, inputResult.pricelistId ? inputResult.pricelistId : 0, inputResult.generalDiscount, inputResult.bytes).then((saveResult) => {
                if (saveResult.success) {
                    this.loadGridData();
                    completion.completed(null, null, true);
                }
                else {
                    completion.failed(saveResult.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null);
    }

    private initDeleteSupplierAgreement() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Invoices/SupplierAgreement/Dialogs/DeleteSupplierAgreement/Views/deletesupplieragreement.html"),
            controller: DeleteSupplierAgreementController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                wholesellers: () => { return this.wholesellersDict },
                pricelistTypes: () => { return this.pricelists }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.deleteSupplierAgreement(result);
            }
        }, () => {
            //Cancel
        });
    }

    private deleteSupplierAgreement(inputResult: any) {
        this.progress.startSaveProgress((completion) => {
            this.invoiceService.deleteSupplierAgreement(inputResult.wholesellerId, inputResult.pricelistId ? inputResult.pricelistId : 0).then(( saveResult ) => {
                if (saveResult.success) {
                    this.loadGridData();
                    completion.completed(null, null, true);
                }
                else {
                    completion.failed(saveResult.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null);
    }
}