import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IMessagingHandler } from "../../../Core/Handlers/MessagingHandler";
import { IEditControllerFlowHandler, IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";
import { Guid } from "../../../Util/StringUtility";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private matchCodeId: number;
    private _selectedAccount: any;
    private _selectedVATAccount: any;

    accountStdsDict: any = [];
    matchCode: any;
    matchCodeTypes: any = [];
    isNew = true;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;

    get selectedAccount() {
        return this._selectedAccount;
    }

    set selectedAccount(item: any) {
        this._selectedAccount = item;

        if (this.matchCode) {
            var id = 0;
            var number = '';
            if (item) {
                id = item.accountId;
                number = item.number;
            }
            this.matchCode.accountId = id;
            this.matchCode.accountNr = number;
        }
    }

    get selectedVATAccount() {
        return this._selectedVATAccount;
    }

    set selectedVATAccount(item: any) {
        this._selectedVATAccount = item;

        if (this.matchCode) {
            var id = 0;
            var number = '';
            if (item) {
                id = item.accountId;
                number = item.number;
            }
            this.matchCode.vatAccountId = id;
            this.matchCode.vatAccountNr = number;
        }
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData());
    }

    public onInit(parameters: any) {
        this.matchCodeId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Preferences_VoucherSettings_MatchCodes, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveMatchCode(this.matchCode).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.matchCodeId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.matchCode);
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
            }, error => { });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.accountingService.deleteMatchCode(this.matchCode.matchCodeId).then((result) => {
                if (result.success) {
                    completion.completed(this.matchCode);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(true);
        });
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.matchCode) {
                if (!this.matchCode.name) {
                    mandatoryFieldKeys.push("common.name");
                    mandatoryFieldKeys.push("common.type");
                }
                if (!this.matchCode.accountId) {
                    mandatoryFieldKeys.push("economy.accounting.account");
                }
                if (!this.matchCode.type) {
                    mandatoryFieldKeys.push("common.type");
                }
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Preferences_VoucherSettings_MatchCodes].readPermission;
        this.modifyPermission = response[Feature.Economy_Preferences_VoucherSettings_MatchCodes].modifyPermission;
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadAccountStdsDict(),
            () => this.loadMatchCodeTypes()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.matchCodeId > 0) {
            this.accountingService.getMatchCode(this.matchCodeId).then((x) => {

                this.isNew = false;
                this.matchCode = x;

                if (this.accountStdsDict) {
                    this.selectedAccount = _.find(this.accountStdsDict, { accountId: this.matchCode.accountId });
                    this.selectedVATAccount = _.find(this.accountStdsDict, { accountId: this.matchCode.vatAccountId });
                }

                deferral.resolve();
            });
        } else {
            this.isNew = true;
            this.matchCodeId = 0;
            this.matchCode = {};

            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadAccountStdsDict(): ng.IPromise<any> {
        return this.accountingService.getAccountStdsNumberName(true).then((x) => {
            this.accountStdsDict = x
        });
    }

    private loadMatchCodeTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.MatchCodeType, false, false).then((x) => {
            this.matchCodeTypes = x;
        });
    }
}