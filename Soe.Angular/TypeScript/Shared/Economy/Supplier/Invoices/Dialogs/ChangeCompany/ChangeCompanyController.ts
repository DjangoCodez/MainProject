import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { ISupplierService } from "../../../SupplierService";

export class ChangeCompanyController {

    resultObject: any;
    supplierByCompanyDTO: any;

    companiesFilterOptions: Array<any> = [];
    suppliersFilterOptions: Array<any> = [];
    voucherSeriesFilterOptions: Array<any> = [];

    selectedCompanyId: number = 0;
    selectedSupplierId: number = 0;
    selectedVoucherSerieId: number = 0;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private $filter,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private supplierService: ISupplierService) {

        this.loadCompanies();
    }

    // Lookups

    private loadCompanies() {
        this.supplierService.getCompaniesByUser().then((x) => {
            this.companiesFilterOptions = [];
            _.forEach(x, (y: any) => {
                this.companiesFilterOptions.push({ id: y.id, name: y.name })
            });
            if (this.companiesFilterOptions.length == 1) {
                this.selectedCompanyId = this.companiesFilterOptions[0].id;
                this.loadSuppliersForSelectedCompany();
                this.loadVoucherSeriesForSelectedCompany();
            }
        });
    }

    private loadSuppliersForSelectedCompany() {
        this.supplierByCompanyDTO = {
            ActorCompanyId: this.selectedCompanyId,
            IsActive: true,
            AddEmptyRow: false,
        }

        this.supplierService.getSuppliersByCompany(this.supplierByCompanyDTO).then((x) => {
            this.suppliersFilterOptions = [];
            _.forEach(x, (y: any) => {
                this.suppliersFilterOptions.push({ id: y.id, name: y.name })
            });
            if (this.suppliersFilterOptions.length == 1) {
                this.selectedSupplierId = this.suppliersFilterOptions[0].id;
            }
        });
    }

    private loadVoucherSeriesForSelectedCompany() {
        this.supplierService.getVoucherSeriesByCompany(this.selectedCompanyId).then((x) => {
            this.voucherSeriesFilterOptions = [];
            _.forEach(x, (y: any) => {
                this.voucherSeriesFilterOptions.push({ id: y.id, name: y.name })
            });
            if (this.voucherSeriesFilterOptions.length == 1) {
                this.selectedVoucherSerieId = this.voucherSeriesFilterOptions[0].id;
            }
        });
    }

    // Events

    companyOnChanging(item) {
        this.selectedCompanyId = item.id;
        this.loadSuppliersForSelectedCompany();
        this.loadVoucherSeriesForSelectedCompany();
    }

    supplierOnChanging(item) {
        this.selectedSupplierId = item.id;
    }

    voucherSerieOnChanging(item) {
        this.selectedVoucherSerieId = item.id;
    }

    buttonOkClick() {
        this.resultObject = {
            selectedCompanyId: this.selectedCompanyId,
            selectedSupplierId: this.selectedSupplierId,
            selectedVoucherSeriesId: this.selectedVoucherSerieId,
        }
        this.$uibModalInstance.close(this.resultObject);
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}