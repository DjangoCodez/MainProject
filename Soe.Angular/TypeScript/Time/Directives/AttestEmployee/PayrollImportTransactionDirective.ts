import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { AttestEmployeeDayDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxImage } from "../../../Util/Enumerations";
import { PayrollImportEmployeeTransactionDTO } from "../../../Common/Models/PayrollImport";
import { TermGroup_PayrollImportEmployeeTransactionStatus } from "../../../Util/CommonEnumerations";

export class PayrollImportTransactionDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestEmployee/Views/PayrollImportTransaction.html'),
            scope: {
                model: '=',
                onCreateTimeBlock: '&',
                onResizeNeeded: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PayrollImportTransactionController,
            controllerAs: 'ctrl',
            bindToController: true,
        };
    }
}

export class PayrollImportTransactionController {

    // Init parameters
    private model: AttestEmployeeDayDTO;
    private onCreateTimeBlock: Function;
    private onResizeNeeded: Function;

    // Terms
    private terms: { [index: string]: string; };

    // Flags
    private expanded: boolean = false;

    // Properties
    private selectedTransaction: PayrollImportEmployeeTransactionDTO;
    private sortBy: string = 'code';
    private sortByReverse: boolean = false;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private notificationService: INotificationService) {
    }

    public $onInit() {
        this.loadTerms();
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "common.comment",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    // EVENTS

    private toggleExpanded() {
        this.expanded = !this.expanded;

        if (this.onResizeNeeded) {
            this.$timeout(() => {
                this.onResizeNeeded();
            });
        }
    }

    private sort(column: string) {
        this.sortByReverse = !this.sortByReverse && this.sortBy === column;
        this.sortBy = column;
    }

    private showComment(trans: PayrollImportEmployeeTransactionDTO) {
        this.notificationService.showDialogEx(this.terms["common.comment"], trans.note, SOEMessageBoxImage.Information);
    }

    private createTimeBlock(trans: PayrollImportEmployeeTransactionDTO) {
        if (trans.status === TermGroup_PayrollImportEmployeeTransactionStatus.Unprocessed && this.onCreateTimeBlock)
            this.onCreateTimeBlock({ trans: trans });
    }
}
