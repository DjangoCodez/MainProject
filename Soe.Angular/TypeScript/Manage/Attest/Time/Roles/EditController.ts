import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IAttestService } from "../../AttestService";
import { AttestPeriodType, CompanySettingType, Feature, SoeEntityState, SoeModule, TermGroup_AttestEntity } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { IAttestRoleDTO } from "../../../../Scripts/TypeLite.Net4";
import { AttestStateDTO } from "../../../../Common/Models/AttestStateDTO";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { AttestRoleMappingDTO } from "../../../../Common/Models/AttestRoleMappingDTO";
import { SettingsUtility } from "../../../../Util/SettingsUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {


    // Data       
    attestRole: IAttestRoleDTO;
    attestRoleId: number = 0;
    attestStates: AttestStateDTO[];
    periodTypes: SmallGenericType[] = [];
    selectedSigningAttestRoles: AttestRoleMappingDTO[] = [];
    showConnectedSigningRoles: boolean;

    // Company settings
    private useAccountsHierarchy: boolean = false;

    // Terms
    private terms: { [index: string]: string; };

    public parameters: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private coreService: ICoreService,
        private attestService: IAttestService
    ) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
    }

    public onInit(parameters: any) {
        this.attestRoleId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Attest_Time_AttestRoles_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Attest_Time_AttestRoles_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Attest_Time_AttestRoles_Edit].modifyPermission;
    }

    // LOOKUPS
    private onDoLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadCompanySettings(),
            () => this.loadPermissions(),
            () => this.loadTerms(),
            () => this.loadAttestStates(),
            () => this.loadPeriodTypes(),
        ]).then(x => {
            this.load();
        });
    }

    // SERVICE CALLS
    private loadPermissions(): ng.IPromise<any> {
        let featureIds: number[] = [];

        featureIds.push(Feature.Manage_Signing_Document_Roles);

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.showConnectedSigningRoles = x[Feature.Manage_Signing_Document_Roles];
        });
    }
    private load(): ng.IPromise<any> {
        let deferral = this.$q.defer();
        if (this.attestRoleId > 0) {
            this.attestService.getAttestRole(this.attestRoleId)
                .then((x) => {
                    this.attestRole = x;
                    this.selectedSigningAttestRoles = [];
                    _.forEach(this.attestRole.attestRoleMapping, (item: AttestRoleMappingDTO) => {
                        if (item.entity == TermGroup_AttestEntity.SigningDocument)
                            this.selectedSigningAttestRoles.push(item);
                    });

                    this.isNew = false;
                    deferral.resolve();
                });
        } else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    public loadAttestStates() {
        return this.attestService.getAttestStates(TermGroup_AttestEntity.Unknown, SoeModule.Time, false).then((x) => {
            this.attestStates = x;
        });
    }

    private loadPeriodTypes(): ng.IPromise<any> {
        const keys: string[] = [
            "manage.attest.role.theday",
            "manage.attest.role.theweek",
            "manage.attest.role.themonth",
            "manage.attest.role.theperiod"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.periodTypes = [];
            this.periodTypes.push({ id: AttestPeriodType.Unknown, name: "" });
            this.periodTypes.push({ id: AttestPeriodType.Day, name: terms["manage.attest.role.theday"] });
            this.periodTypes.push({ id: AttestPeriodType.Week, name: terms["manage.attest.role.theweek"] });
            this.periodTypes.push({ id: AttestPeriodType.Month, name: terms["manage.attest.role.themonth"] });
            this.periodTypes.push({ id: AttestPeriodType.Period, name: terms["manage.attest.role.theperiod"] });
        });
    }


    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "manage.attest.role.showemployeeswithoutaccounts",
            "manage.attest.role.showallaccounts"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    // ACTIONS

    private save() {
        if (this.attestRole.active)
            this.attestRole.state = SoeEntityState.Active;
        else
            this.attestRole.state = SoeEntityState.Inactive;

        this.progress.startSaveProgress((completion) => {

            this.attestRole.attestRoleMapping = [];
            _.forEach(this.selectedSigningAttestRoles, (item: AttestRoleMappingDTO) => {
                this.attestRole.attestRoleMapping.push(item);
            });

            this.attestService.saveAttestRole(this.attestRole).then((result) => {
                if (result.success) {
                    if (!this.attestRoleId) {
                        this.attestRoleId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.attestRole);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.load();
            }, error => {

            });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.attestService.deleteAttestRole(this.attestRoleId).then((result) => {
                if (result.success) {
                    completion.completed(this.attestRole);
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

    // HELP-METHODS   
    private new() {
        this.isNew = true;
        this.attestRoleId = 0;
        this.attestRole = ({} as IAttestRoleDTO);
        this.attestRole.module = SoeModule.Time;
        this.attestRole.primaryCategoryRecords = [];
        this.attestRole.secondaryCategoryRecords = [];
        this.selectedSigningAttestRoles = [];
    }

    private setDirty(force: boolean = false) {
        if (force) {
            this.$scope.$applyAsync(() => {
                this['edit'].$pristine = false;
                this['edit'].$dirty = true;
            });
        }
        this.dirtyHandler.setDirty();
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.attestRole) {
                if (!this.attestRole.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
}
