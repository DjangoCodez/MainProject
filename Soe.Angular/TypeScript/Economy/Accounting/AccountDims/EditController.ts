import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Feature, TermGroup, CompanySettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { AccountDimDTO } from "../../../Common/Models/AccountDimDTO";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private accountDimId: number;
    private isStdAccountDim: boolean;
    private inactivatePermission = false;

    accountDim: AccountDimDTO;
    result: any;
    sysAccountStdTypes: any = [];
    useAccountsHierarchy = false;
    chars = [];
    sieDims: any = [];
    loading = false;
    vatDeductionExists = false;
    resetAccountInternals = false;

    _useVatDeduction: boolean;
    set useVatDeduction(value: boolean) {
        this._useVatDeduction = value;
        if (!this.loading)
            this.useVatDeductionClick();
    }
    get useVatDeduction(): boolean {
        return this._useVatDeduction;
    }

    //ParentDim
    private accountDims: any = [];

    private edit: ng.IFormController;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $timeout: ng.ITimeoutService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData());
    }

    public onInit(parameters: any) {
        this.accountDimId = parameters.id;
        this.isStdAccountDim = parameters.isStdAccountDim;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Accounting_AccountRoles_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public validateAccountDimNr() {
        this.$timeout(() => {
            this.accountingService.validateAccountDimNr(this.accountDim.accountDimNr, this.accountDim.accountDimId ?? 0).then((result) => {
                if (!result.success)
                    this.notificationService.showDialog(this.translationService.translateInstant("core.warning"), result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        });
    }

    public save() {
        this.accountDim.useVatDeduction = this.useVatDeduction;
        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveAccountDim(this.accountDim, this.resetAccountInternals).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.accountDimId = result.integerValue;

                    //this.closeMe(false);

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.accountDim);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {

            var accountDimIdArray = [];
            accountDimIdArray.push(this.accountDimId);

            this.accountingService.deleteAccountDims(accountDimIdArray).then((result) => {
                if (result.success) {
                    completion.completed(result, false, result.infoMessage);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    private useVatDeductionClick() {
        if (this.accountDim.useVatDeduction) {
            this.resetAccountInternals = false;

            // Show verification dialog
            var keys: string[] = [
                "core.warning",
                "economy.accounting.accountdim.vatdeductionwarning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.accounting.accountdim.vatdeductionwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        this.resetAccountInternals = true;
                    }
                },
                    (cancel) => {
                        this._useVatDeduction = !this.useVatDeduction;
                    });
            });
        }
    }

    public showValidationError() {
        var errors = this['edit'].$error;

        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.accountDim) {
                // Mandatory fields
                if (!this.accountDim.accountDimNr)
                    mandatoryFieldKeys.push("common.number");
                if (!this.accountDim.shortName)
                    mandatoryFieldKeys.push("common.shortname");
                if (!this.accountDim.name)
                    mandatoryFieldKeys.push("common.name");

                if (errors['linkedToMultiple'])
                    validationErrorKeys.push("economy.accounting.cannotbelinkedtobothprojectandshifttype");
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Accounting_AccountRoles_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_AccountRoles_Edit].modifyPermission;
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadModifyPermissions(),
            () => this.loadCompanySettings(),
            () => this.loadChars(),
            () => this.loadSie(),
            () => this.loadAccountStdTypes(),
            () => this.loadAccountDims()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        this.loading = true;
        var deferral = this.$q.defer();

        if (this.accountDimId > 0) {
            this.accountingService.getAccountDim(this.accountDimId, false, false, true, false).then(x => {
                this.isNew = false;
                this.accountDim = x;
                this.useVatDeduction = this.accountDim.useVatDeduction;
                this.loading = false;
                deferral.resolve();
            });
        } else {
            this.isNew = true;
            this.accountDimId = 0;
            this.accountDim = new AccountDimDTO();
            this.accountDim.isActive = true;
            this.loading = true;

            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];
        features.push(Feature.Economy_Accounting_AccountRoles_Inactivate);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.inactivatePermission = x[Feature.Economy_Accounting_AccountRoles_Inactivate];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadChars(): ng.IPromise<any> {
        return this.accountingService.getAccountDimChars().then((x) => {
            this.chars = x;
        });
    }

    private loadSie(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SieAccountDim, false, false).then((x) => {
            this.sieDims = x;
        });
    }

    private loadAccountStdTypes(): ng.IPromise<any> {
        return this.accountingService.getSysAccountStdTypes().then((x) => {
            this.sysAccountStdTypes = x;
        });
    }

    private loadAccountDims() {
        return this.accountingService.getAccountDims(false, true, false, false, false, false).then((data: AccountDimDTO[]) => {
            let vatDeductionOnCurrentDim = data.filter(x => x.accountDimId === this.accountDimId && x.useVatDeduction).length > 0; 
            this.accountDims.push({ id: 0, name: " " });
            _.forEach(data, (y: any) => {
                if (y.accountDimId !== this.accountDimId) {
                    if (!this.vatDeductionExists && y.useVatDeduction && !vatDeductionOnCurrentDim)
                        this.vatDeductionExists = true;
                    this.accountDims.push({ id: y.accountDimId, name: y.name });
                }
            });
        });
    }
}