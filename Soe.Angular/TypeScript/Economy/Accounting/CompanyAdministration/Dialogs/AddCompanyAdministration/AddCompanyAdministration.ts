import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { IAccountingService } from "../../../../../Shared/Economy/Accounting/AccountingService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { Constants } from "../../../../../Util/Constants";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/ProgressHandlerFactory";

export class AddCompanyAdministrationController {

    langId: number;
    terms: { [index: string]: string; };

    childCompanies: ISmallGenericType[] = [];
    companyGroupMappings: ISmallGenericType[] = [];

    companyGroupAdministration: any;

    progress: IProgressHandler;

    get validToSave() {
        return this.companyGroupAdministration && this.companyGroupAdministration.childActorCompanyId && this.companyGroupAdministration.childActorCompanyId > 0 && this.companyGroupAdministration.companyGroupMappingHeadId && this.companyGroupAdministration.companyGroupMappingHeadId > 0 && (this.companyGroupAdministration.conversionfactor && this.companyGroupAdministration.conversionfactor > 0);
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private companyIdsToExclude: number[],
        private companyAdministrationId?: number) {

        this.progress = progressHandlerFactory.create();
        this.setup();
    }

    private setup() {
        if (this.companyAdministrationId) {
            this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadChildCompanies(),
                () => this.loadCompanyGroupMappings(),
                () => this.loadExistingCompanyAdministration(),
            ]);
        }
        else {
            this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadChildCompanies(),
                () => this.loadCompanyGroupMappings(),
            ]).then(() => {
                this.companyGroupAdministration = {}
                this.companyGroupAdministration.groupCompanyActorCompanyId = soeConfig.actorCompanyId;
                this.companyGroupAdministration.conversionfactor = 1; 
                this.companyGroupAdministration.matchInternalAccountOnNr = false;
            });
        }
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "billing.productrows.productnr",
            "billing.productrows.text",
            "billing.order.syswholeseller",
            "billing.productrows.dialogs.changingwholeseller",
            "billing.productrows.dialogs.failedwholesellerchange",
            "billing.stock.stocksaldo.importstockbalance"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadChildCompanies(): ng.IPromise<any> {
        return this.accountingService.getChildCompaniesDict().then((result) => {
            var companies = [];
            _.forEach(result, (company) => {
                if (!_.includes(this.companyIdsToExclude, company.id))
                    companies.push(company);
            });
            this.childCompanies = companies;
        });
    }

    private loadCompanyGroupMappings(): ng.IPromise<any> {
        return this.accountingService.getCompanyGroupMappingsDict(true).then((result) => {
            this.companyGroupMappings = result;
        });
    }

    private loadExistingCompanyAdministration(): ng.IPromise<any> {
        return this.accountingService.getCompanyAdministration(this.companyAdministrationId).then((result) => {
            this.companyGroupAdministration = result;
        });
    }

    private save() {
        //this.importStockBalances().then(() => {
            this.$uibModalInstance.close({ item: this.companyGroupAdministration });
        //})
    }

    private delete() {
        this.$uibModalInstance.close({ item: this.companyGroupAdministration, delete: true });
    }

    private close() {
        this.$uibModalInstance.close();
    }
}