import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    grossProfitCodeId: number;
    grossProfitCode: any;

    // Filter options                
    accountYearFilterOptions: Array<any> = [];
    accountDimFilterOptions: Array<any> = [];
    accountFilterOptions: Array<any> = [];

    // Current selected
    currentAccountYearId: any;
    _currentAccountDimId: any;
    get currentAccountDimId() {
        return this._currentAccountDimId;
    }
    set currentAccountDimId(item: any) {
        this._currentAccountDimId = item;
        this.loadAccounts();
        if (this.grossProfitCode) {
            this.grossProfitCode.accountDimId = item;
        }
    }
    _currentAccountId: any;
    get currentAccountId() {
        return this._currentAccountId;
    }
    set currentAccountId(item: any) {
        if (item) {
            this._currentAccountId = item;
            if (this.grossProfitCode) {
                this.grossProfitCode.accountId = item;
            }
        }
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData()) //this.doLookups())
            .onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.grossProfitCodeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    // SETUP

    private onDoLookups() {
        return this.$q.all([this.loadAccountYearDict(), this.loadAccountDims()]);
    }

    public accountYearOnChanging(selectedAccountYear) {
        this.grossProfitCode.accountYearId = this.currentAccountYearId = selectedAccountYear;
        this.loadAccounts();
    }

    public accountDimOnChanging(selectedAccountDimId) {
        this.currentAccountDimId = selectedAccountDimId;
        this.loadAccounts();
    }

    // LOOKUPS
    private onLoadData(): ng.IPromise<any> {
        if (this.grossProfitCodeId > 0) {
            return this.accountingService.getGrossProfitCode(this.grossProfitCodeId).then((x) => {
                this.isNew = false;
                this.grossProfitCode = x;
                if (this.grossProfitCode) {
                    this.currentAccountYearId = this.grossProfitCode.accountYearId;
                    this.currentAccountDimId = this.grossProfitCode.accountDimId;
                    this.currentAccountId = this.grossProfitCode.accountId;
                }
            });
        }
        else {
            this.new();
        }
    }

    private loadAccountYearDict(): ng.IPromise<any> {
        return this.accountingService.getAccountYearDict(false).then((x) => {
            _.forEach(x, (y: any) => {
                this.accountYearFilterOptions.push({ id: y.id, name: y.name });
            });

            this.accountYearFilterOptions.reverse(); //reverse, to set the latest accountyear on top       
            this.currentAccountYearId = this.accountYearFilterOptions[0].id; //set default accountyear                      
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, true, false, false).then((x) => {
            _.forEach(x, (y: any) => {
                this.accountDimFilterOptions.push({ id: y.accountDimId, name: y.name });
            });
        });
    }

    private loadAccounts() {
        if (this.currentAccountDimId && this.currentAccountYearId) {
            this.accountFilterOptions = [];
            this.accountingService.getAccounts(this.currentAccountDimId, this.currentAccountYearId).then((x) => {
                _.forEach(x, (y: any) => {
                    this.accountFilterOptions.push({ id: y.accountId, name: y.name });
                });
            });
        }
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveGrossProfitCode(this.grossProfitCode).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.grossProfitCodeId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.grossProfitCode);
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
            this.accountingService.deleteGrossProfitCode(this.grossProfitCode.grossProfitCodeId).then((result) => {
                if (result.success) {
                    completion.completed(this.grossProfitCode);
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

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.grossProfitCodeId = 0;
        this.grossProfitCode = {
            accountYearId: this.currentAccountYearId,
            name: "",
            code: 0,
            description: "",
            openingBalance: 100.0000,
            period1: 0.0000,
            period2: 0.0000,
            period3: 0.0000,
            period4: 0.0000,
            period5: 0.0000,
            period6: 0.0000,
            period7: 0.0000,
            period8: 0.0000,
            period9: 0.0000,
            period10: 0.0000,
            period11: 0.0000,
            period12: 0.0000,
            period13: 0.0000,
            period14: 0.0000,
            period15: 0.0000,
            period16: 0.0000,
            period17: 0.0000,
            period18: 0.0000,
        };
    }

    protected copy() {

        this.isNew = true;
        // this.isDirty = true;
        this.grossProfitCodeId = 0;

        var copyOfGrossProfitCode = this.grossProfitCode;
        this.new();

        this.grossProfitCode = copyOfGrossProfitCode;
        this.grossProfitCode.grossProfitCodeId = 0;

        this.messagingService.publish(Constants.EVENT_EDIT_NEW, this.guid);

        var keys: string[] = [
            "economy.accounting.grossprofitcode.new_grossprofitcode"
        ];
        this.translationService.translateMany(keys).then((terms) => {

            // set tab label to current view definition
            this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                guid: this.guid,
                label: "{0}".format(terms["economy.accounting.grossprofitcode.new_grossprofitcode"]),
                // label: "{0} - {1}".format(this.terms[""], label),
            });
        });
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.grossProfitCode) {
                if (!this.grossProfitCode.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.grossProfitCode.code) {
                    mandatoryFieldKeys.push("common.code");
                }
                if (this.grossProfitCode.openingBalance == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.openingbalance");
                }
                if (this.grossProfitCode.period1 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period1");
                }
                if (this.grossProfitCode.period2 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period2");
                }
                if (this.grossProfitCode.period3 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period3");
                }
                if (this.grossProfitCode.period4 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period4");
                }
                if (this.grossProfitCode.period5 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period5");
                }
                if (this.grossProfitCode.period6 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period6");
                }
                if (this.grossProfitCode.period7 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period7");
                }
                if (this.grossProfitCode.period8 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period8");
                }
                if (this.grossProfitCode.period9 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period9");
                }
                if (this.grossProfitCode.period10 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period10");
                }
                if (this.grossProfitCode.period11 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period11");
                }
                if (this.grossProfitCode.period12 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period12");
                }
                if (this.grossProfitCode.period13 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period13");
                }
                if (this.grossProfitCode.period14 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period14");
                }
                if (this.grossProfitCode.period15 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period15");
                }
                if (this.grossProfitCode.period16 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period16");
                }
                if (this.grossProfitCode.period17 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period17");
                }
                if (this.grossProfitCode.period18 == null) {
                    mandatoryFieldKeys.push("economy.accounting.grossprofitcode.period18");
                }
            }
        });
    }
}
