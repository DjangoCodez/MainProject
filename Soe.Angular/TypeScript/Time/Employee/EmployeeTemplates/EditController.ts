import { IEmployeeService } from "../EmployeeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { EmployeeTemplateDTO } from "../../../Common/Models/EmployeeTemplateDTOs";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IPromise } from "angular";
import { IFocusService } from "../../../Core/Services/focusservice";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { EmployeeCollectiveAgreementDTO } from "../../../Common/Models/EmployeeCollectiveAgreementDTOs";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string };
    private titleInfo: string;

    // Company settings
    private useAnnualLeave = false;

    // Data
    private employeeTemplate: EmployeeTemplateDTO;
    private employeeTemplateId: number
    private collectiveAgreement: EmployeeCollectiveAgreementDTO;

    // Lookups
    private collectiveAgreements: ISmallGenericType[];

    // ToolBar
    protected buttonGroups = new Array<ToolBarButtonGroup>();

    // Flags
    private headInitiallyOpen = false;
    private isCopy = false;
    private hasInvalidPosition: boolean;
    private hasRemainingSystemRequiredFields: boolean;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private focusService: IFocusService,
        private employeeService: IEmployeeService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookUp())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.employeeTemplateId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Employee_EmployeeTemplates, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_EmployeeTemplates].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_EmployeeTemplates].modifyPermission;
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAnnualLeave);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAnnualLeave = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAnnualLeave);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private onDoLookUp(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadCollectiveAgreements()
        ]);
    }

    private onLoadData() {
        if (this.employeeTemplateId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.headInitiallyOpen = true;
            this.new();
        }
    }

    private new() {
        this.isNew = true;
        this.employeeTemplateId = 0;
        this.employeeTemplate = new EmployeeTemplateDTO();
        this.employeeTemplate.isActive = true;

        this.isCopy = false;
    }

    protected copy() {
        if (!this.employeeTemplate)
            this.new();
        else {
            this.isNew = true;
            this.employeeTemplate.employeeTemplateId = this.employeeTemplateId = 0;
            this.employeeTemplate.name = undefined;

            if (this.employeeTemplate.employeeTemplateGroups) {
                this.employeeTemplate.employeeTemplateGroups.forEach(group => {
                    group.employeeTemplateGroupId = 0;
                    if (group.employeeTemplateGroupRows) {
                        group.employeeTemplateGroupRows.forEach(gRow => {
                            gRow.employeeTemplateGroupId = 0;
                            gRow.employeeTemplateGroupRowId = 0;
                        });
                    }
                });
            }
        }

        this.isCopy = true;

        this.focusService.focusByName("ctrl_employeeTemplate_name");
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys = [
            "time.employee.employeetemplate.titleinfo"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
            this.titleInfo = this.terms["time.employee.employeetemplate.titleinfo"];
        });
    }

    private loadCollectiveAgreements(): IPromise<any> {
        return this.employeeService.getEmployeeCollectiveAgreementsDict(false).then(x => {
            this.collectiveAgreements = x;
        });
    }

    private loadCollectiveAgreement(): ng.IPromise<any> {
        return this.employeeService.getEmployeeCollectiveAgreement(this.employeeTemplate.employeeCollectiveAgreementId).then(x => {
            this.collectiveAgreement = x;
        });
    }

    private load(): ng.IPromise<any> {
        return this.employeeService.getEmployeeTemplate(this.employeeTemplateId).then(x => {
            this.isNew = false;
            this.employeeTemplate = x;
            this.isCopy = false;
            this.loadCollectiveAgreement();
        });
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.employeeService.saveEmployeeTemplate(this.employeeTemplate).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        this.employeeTemplateId = this.employeeTemplate.employeeTemplateId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.employeeTemplate);
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
            });
    }

    private delete() {
        if (!this.employeeTemplate.employeeTemplateId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deleteEmployeeTemplate(this.employeeTemplateId).then(result => {
                if (result.success) {
                    completion.completed(this.employeeTemplate, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(true);
        });
    }

    // EVENTS

    private collectiveAgreementChanged() {
        this.$timeout(() => {
            this.loadCollectiveAgreement();
        });
    }

    private designerChanged() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.employeeTemplate) {
                let errors = this['edit'].$error;

                // Mandatory fields
                if (!this.employeeTemplate.employeeCollectiveAgreementId)
                    mandatoryFieldKeys.push("time.employee.employeecollectiveagreement.employeecollectiveagreement");
                if (!this.employeeTemplate.name)
                    mandatoryFieldKeys.push("common.name");

                if (errors['validPosition'])
                    validationErrorKeys.push("time.employee.employeetemplate.invalidposition");

                if (errors['noRemainingSystemRequiredFields'])
                    validationErrorKeys.push("time.employee.employeetemplate.hasremainingsystemrequiredfields");
            }
        });
    }
}
