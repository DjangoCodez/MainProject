import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IImportOptionsDTO, ISupplierProductImportDTO, ISupplierProductImportRawDTO } from "../../../../../Scripts/TypeLite.Net4";
import { SupplierProductService } from "../../../../../Shared/Billing/Purchase/Purchase/SupplierProductService";
import { SupplierService } from "../../../../../Shared/Economy/Supplier/SupplierService";
import { ImportDynamicController } from "../../../../Dialogs/ImportDynamic/ImportDynamic";

export class ImportProductsDialog {
    private suppliers: any[] = [];
    private selectedSupplier: any;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $scope: ng.IScope,
        private supplierProductService: SupplierProductService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private $uibModal,
        private supplierService: SupplierService,
    ) {
        this.supplierService.getSuppliersDict(true, true, false).then(data => {
            this.suppliers = data;
        })
    }



    private cancel() {
        this.$uibModalInstance.close();
    }
    private openImport() {
        const id = this.selectedSupplier?.id
        if (id) {
            this.openImportDialog(id);
        }
        else {
            this.openImportDialog(null);
        }
    }

    private openImportDialog(id?: number) {
        const multipleSuppliers = id === null;
        const importCallback = (data: ISupplierProductImportRawDTO[], options: IImportOptionsDTO) => {
            const model: ISupplierProductImportDTO = {
                importToPriceList: false,
                importPrices: true,
                supplierId: id,
                priceListId: null,
                rows: data,
                options: options
            }
            return this.supplierProductService.performSupplierProductImport(model)
        }
        this.supplierProductService.getSupplierPricelistImport(false, true, multipleSuppliers).then(data => {
            var modal = this.$uibModal.open({
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
                this.cancel();
            });
        })
    }
}