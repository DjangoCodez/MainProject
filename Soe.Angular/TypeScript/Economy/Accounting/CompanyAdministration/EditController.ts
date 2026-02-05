import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    //Data
    accountDimId: number;
    accountDim: any;

    result: any;
    isStdAccountDim: boolean;

    //Lookups
    sysAccountStdTypes: any;
    chars = [];
    sieDims: any;

    //@ngInject
    constructor(
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            //.onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {

        this.accountDimId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Accounting_CompanyGroup_Companies_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Accounting_CompanyGroup_Companies_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_CompanyGroup_Companies_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    // LOOKUPS
    private onLoadData(): ng.IPromise<any> {
        if (this.accountDimId > 0) {
            return this.accountingService.getAccountDim(this.accountDimId, false, false, false).then((x) => {
                this.isNew = false;
                this.accountDim = x;
                if (this.accountDim) {
                    if (this.accountDim.accountDimNr === 1) {
                        this.loadAccountStdTypes();
                    }
                    else {
                        this.loadChars();
                        this.loadSie();
                    }
                }
            });
        }
        else {
            this.new();
        }
    }

    private loadChars() {
        this.accountingService.getAccountDimChars().then((x) => {
            this.chars = x;
        })
    }

    private loadSie() {
        this.coreService.getTermGroupContent(TermGroup.SieAccountDim, false, false).then((x) => {
            this.sieDims = x;
        });
    }

    private loadAccountStdTypes() {
        this.accountingService.getSysAccountStdTypes().then((x) => {
            this.sysAccountStdTypes = x;
        });
    }

    // ACTIONS

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveAccountDim(this.accountDim, false).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.accountDimId = result.integerValue;

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

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.accountDimId = 0;
        this.accountDim = {
        };
        //TODO
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.accountDim) {
                if (!this.accountDim.accountDimNr) {
                    mandatoryFieldKeys.push("common.number");
                }
                if (!this.accountDim.shortName) {
                    mandatoryFieldKeys.push("common.shortname");
                }
                if (!this.accountDim.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
}