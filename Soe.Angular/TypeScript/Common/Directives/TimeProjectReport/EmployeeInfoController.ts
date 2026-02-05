import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ProjectTimeBlockDTO, EmployeeScheduleTransactionInfoDTO } from "../../Models/ProjectDTO";

export class EmployeeInfoController {

    // Data
    private deletedRows: ProjectTimeBlockDTO[] = [];

    // Lookups

    // Terms
    private terms: any = [];

    // GUI
    private internalNoteExpanded: boolean = false;

    //@ngInject
    constructor(private $uibModalInstance,
        protected $uibModal,
        protected coreService: ICoreService,
        protected urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private title: string,
        private infoItem: EmployeeScheduleTransactionInfoDTO) {

        this.init();
    }

    // SETUP

    private init() {
        this.loadLookups();
    }

    private loadLookups() {
        this.$q.all([
            this.loadTerms()]).then(() => {
            });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "billing.project.timesheet.edittime.title",
            "billing.project.timesheet.edittime.noproject",
            "billing.project.timesheet.edittime.noorder"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private close() {
        this.$uibModalInstance.close();
    }
}
