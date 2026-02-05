import { EmployeeTemplateDisbursementAccountDTO } from "../../../../../../../Common/Models/EmployeeTemplateDTOs";
import { ICoreService } from "../../../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../../../Util/CalendarUtility";
import { TermGroup, TermGroup_EmployeeDisbursementMethod } from "../../../../../../../Util/CommonEnumerations";

export class DisbursementAccountFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/DisbursementAccount/DisbursementAccount.html'),
            scope: {
                model: '=',
                socialSec: '=',
                forceSocialSecNbr: '=',
                isEditMode: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: DisbursementAccountController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class DisbursementAccountController {

    // Init parameters
    private model: string;
    private socialSec: string;
    private forceSocialSecNbr: boolean;
    private disbursementAccount: EmployeeTemplateDisbursementAccountDTO;
    private isEditMode: boolean;
    private onChange: Function;

    // Data
    private disbursementPaymentMethods: ISmallGenericType[];

    // Properties
    private method: number;
    private clearingNr: string;
    private accountNr: string;
    private dontValidateAccountNr: boolean;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService) {

        this.$q.all([
            this.loadDisbursementPaymentMethods()
        ]).then(() => {
            if (this.model) {
                this.setModel();
            }
        });

        if (!this.isEditMode) {
            this.$scope.$watch(() => this.socialSec, (newVal, oldVal) => {
                if (newVal !== oldVal)
                    this.setPersonAccount();
            });
        }
    }

    // SERVICE CALLS

    private loadDisbursementPaymentMethods(): ng.IPromise<any> {
        this.disbursementPaymentMethods = [];
        return this.coreService.getTermGroupContent(TermGroup.EmployeeDisbursementMethod, true, false).then((x) => {
            this.disbursementPaymentMethods = x;
            this.disbursementPaymentMethods = _.orderBy(this.disbursementPaymentMethods, ['name'], ['asc']);
        });
    }

    // EVENTS

    private paymentMethodChanged(method: TermGroup_EmployeeDisbursementMethod) {
        this.$timeout(() => {
            if (method === TermGroup_EmployeeDisbursementMethod.SE_CashDeposit) {
                this.clearingNr = '';
                this.accountNr = '';
                this.dontValidateAccountNr = false;
            } else if (method === TermGroup_EmployeeDisbursementMethod.SE_PersonAccount && this.socialSec) {
                this.setPersonAccount();
            }
        });
        this.setDirty();
    }

    private bankAccountChanged() {
        this.setDirty();
    }

    private setDirty() {
        if (this.onChange) {
            this.$timeout(() => {
                this.onChange({ jsonString: this.getJsonFromModel() });
            });
        }
    }

    // HELP-METHODS

    private setPersonAccount() {
        if (this.method === TermGroup_EmployeeDisbursementMethod.SE_PersonAccount && this.socialSec) {
            if (CalendarUtility.isValidSocialSecurityNumber(this.socialSec.trim(), true, this.forceSocialSecNbr, false)) {
                this.clearingNr = '3300';

                // Remove all but numbers
                let socialSec: string = this.socialSec.replace(/\D/g, '');
                // Remove century
                if (socialSec.length === 12)
                    socialSec = socialSec.substring(2);

                this.accountNr = socialSec;
                this.setDirty();
            }
        }
    }

    private getJsonFromModel(): string {
        this.disbursementAccount = new EmployeeTemplateDisbursementAccountDTO();
        this.disbursementAccount.method = this.method;
        this.disbursementAccount.clearingNr = this.clearingNr;
        this.disbursementAccount.accountNr = this.accountNr;
        this.disbursementAccount.dontValidateAccountNr = this.dontValidateAccountNr;

        return JSON.stringify(this.disbursementAccount);
    }

    private setModel() {
        this.disbursementAccount = new EmployeeTemplateDisbursementAccountDTO();
        angular.extend(this.disbursementAccount, JSON.parse(this.model));

        this.method = this.disbursementAccount.method;
        this.clearingNr = this.disbursementAccount.clearingNr;
        this.accountNr = this.disbursementAccount.accountNr;
        this.dontValidateAccountNr = this.disbursementAccount.dontValidateAccountNr;
    }
}
