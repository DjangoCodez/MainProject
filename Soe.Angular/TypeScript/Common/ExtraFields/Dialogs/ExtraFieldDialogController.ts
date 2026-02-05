import { ChecklistRowDTO } from "../../../Common/Models/ChecklistRowDTO"
import { NotificationService } from "../../../Core/Services/NotificationService";
import { TranslationService } from "../../../Core/Services/TranslationService";
import { ICoreService, CoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { SoeEntityState, SoeEntityType, TermGroup, TermGroup_ChecklistRowType, TermGroup_ExtraFieldType } from "../../../Util/CommonEnumerations";
import { ExtraFieldDTO } from "../../Models/ExtraFieldDTO";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { AccountDimSmallDTO } from "../../Models/AccountDimDTO";

export class ExtraFieldDialogController {

    private isNew: boolean;
    private fieldTypes: any[];
    private translations: any[];
    private isForAccountDim: boolean;
    private accountDims: any[];

    private dialogform: ng.IFormController;

    //Modal
    protected progressModalMetaData;
    protected progressModal;

    // Progress bar
    protected progressMessage: string;
    protected progressBusy: boolean = true;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: TranslationService,
        private notificationService: NotificationService,
        private coreService: CoreService,
        //private accountingService: IAccountingService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $uibModal,
        private $scope: ng.IScope,
        private entity: SoeEntityType,
        private extraField: ExtraFieldDTO) {

        this.isNew = !extraField;
        this.isForAccountDim = entity == SoeEntityType.Account;
        
        if (!this.extraField) {
            this.extraField = new ExtraFieldDTO();
            this.extraField.extraFieldId = 0;
            this.extraField.entity = entity;
            this.extraField.type = TermGroup_ExtraFieldType.FreeText;
        }
        if (entity == SoeEntityType.Account)
            this.extraField.connectedEntity = SoeEntityType.AccountDim;
    }

    public $onInit() {
        this.$q.all([
            this.loadFieldTypes(),
            this.loadAccountDims(),
            ]);
    }

    private loadFieldTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ExtraFieldTypes, false, false).then(x => {
            x = _.orderBy(x, 'id');
            this.fieldTypes = _.filter(x, (y) => y.id < 7);
            //this.fieldTypes = x.slice(0, x.length - 1);
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, true, true, true, false, true, false).then((dims) => {
            this.accountDims = dims;
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        if (this.isValid(this.translations) == true) {
            this.$scope.$broadcast('stopEditing', {});

            this.$timeout(() => {
                this.extraField.translations = _.filter(this.translations, (t) => t.state === SoeEntityState.Active);
                this.$uibModalInstance.close(this.extraField);
            });
        }
    }

    private isValid(extraFields: any) {
        var valueArr = extraFields.map(function (ef) { return ef.langName });
        const self = this; //accessing this file's scope inside callback function
        var isValid = true;
        valueArr.some(function (item, idx) {
            //check for duplicates
            if ((valueArr.indexOf(item) != idx) == true) { 
                isValid = false;
                self.failedSave("");
                self.translationService.translate('common.extrafields.warning.duplicateRecord').then((term) => {
                    self.showErrorDialog(term);
                })
                return isValid;
            }
            //check for empty fields
            if (item == undefined) { 
                isValid = false;
                self.translationService.translate("common.extrafields.warning.emptyRecord").then((term) => {
                    self.showErrorDialog(term);
                })
                return isValid;
            }
        });
        return isValid;
    }

    protected failedSave(message = "", closeDialog: boolean = false) {
        if (closeDialog) {
            this.stopProgress();
            if (this.progressModal)
                this.progressModal.close();
        } else {
            if (!this.progressModalMetaData) {
                this.progressModalMetaData = {};
                this.progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
                this.progressModalMetaData.showclose = true;
            }
            if (message === "") {
                this.translationService.translate('core.savefailed').then(s => this.progressModalMetaData.text = s);
            } else {
                this.progressModalMetaData.text = message;
            }
            this.stopProgress();
        }
    }

    protected showErrorDialog(message) {
        this.notificationService.showDialog("", message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
        this.stopProgress();
    }

    protected stopProgress() {
        this.progressBusy = false;
        this.progressMessage = '';
    }
}
