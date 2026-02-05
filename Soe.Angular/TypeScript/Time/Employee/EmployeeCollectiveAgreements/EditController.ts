import { IEmployeeService } from "../EmployeeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { Feature } from "../../../Util/CommonEnumerations";
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
import { EmployeeCollectiveAgreementDTO } from "../../../Common/Models/EmployeeCollectiveAgreementDTOs";
import { IEmployeeGroupSmallDTO, IPayrollGroupSmallDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IPromise } from "angular";
import { IPayrollService } from "../../Payroll/PayrollService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private employeeCollectiveAgreement: EmployeeCollectiveAgreementDTO;
    private employeeCollectiveAgreementId: number

    // Lookups
    private employeeGroups: IEmployeeGroupSmallDTO[];
    private payrollGroups: IPayrollGroupSmallDTO[];
    private vacationGroups: ISmallGenericType[];

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private employeeService: IEmployeeService,
        private payrollService: IPayrollService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookUp())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }


    public onInit(parameters: any) {
        this.employeeCollectiveAgreementId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Employee_EmployeeCollectiveAgreements, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_EmployeeCollectiveAgreements].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_EmployeeCollectiveAgreements].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    private onDoLookUp(): ng.IPromise<any> {
        return this.$q.all([
            this.loadEmployeeGroups(),
            this.loadPayrollGroups(),
            this.loadVacationGroups()
        ]);
    }

    private onLoadData() {
        if (this.employeeCollectiveAgreementId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    private load(): ng.IPromise<any> {
        return this.employeeService.getEmployeeCollectiveAgreement(this.employeeCollectiveAgreementId).then(x => {
            this.isNew = false;
            this.employeeCollectiveAgreement = x;
        });
    }

    private loadEmployeeGroups(): IPromise<any> {
        return this.employeeService.getEmployeeGroupsSmall().then(x => {
            this.employeeGroups = x;
        });
    }

    private loadPayrollGroups(): IPromise<any> {
        return this.payrollService.getPayrollGroupsSmall(false, false).then(x => {
            this.payrollGroups = x;
        });
    }

    private loadVacationGroups(): IPromise<any> {
        return this.employeeService.getVacationGroups(false, true).then(x => {
            this.vacationGroups = x;
        });
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.employeeService.saveEmployeeCollectiveAgreement(this.employeeCollectiveAgreement).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        this.employeeCollectiveAgreementId = this.employeeCollectiveAgreement.employeeCollectiveAgreementId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.employeeCollectiveAgreement);
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

    protected delete() {
        if (!this.employeeCollectiveAgreement.employeeCollectiveAgreementId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deleteEmployeeCollectiveAgreement(this.employeeCollectiveAgreementId).then(result => {
                if (result.success) {
                    completion.completed(this.employeeCollectiveAgreement, true);
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

    private new() {
        this.isNew = true;
        this.employeeCollectiveAgreementId = 0;
        this.employeeCollectiveAgreement = new EmployeeCollectiveAgreementDTO();
        this.employeeCollectiveAgreement.isActive = true;
    }

    // VALIDATION

    public showValidationError() {

        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.employeeCollectiveAgreement) {
                // Mandatory fields
                if (!this.employeeCollectiveAgreement.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });

    }
}
