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
import { CoreUtility } from "../../../Util/CoreUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    employmentType: any;
    employmentTypeId: number

    // Lookups
    systemTypes: any;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(private employeeService: IEmployeeService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
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
        this.employmentTypeId = parameters.id;        
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Employee_EmploymentTypes, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_EmploymentTypes].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_EmploymentTypes].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    private onDoLookUp(): ng.IPromise<any> {
        return this.loadSystemTypes();
    }

    private onLoadData() {
        if (this.employmentTypeId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    private load(): ng.IPromise<any> {
        console.log('getEmploymentType', this.employmentTypeId);
        return this.employeeService.getEmploymentType(this.employmentTypeId).then((x) => {
            console.log(x);
            this.isNew = false;
            this.employmentType = x;            
        });
    }

    private loadSystemTypes(): ng.IPromise<any> {
        return this.employeeService.getStandardEmploymentTypes(CoreUtility.languageId).then(x => {                   
            this.systemTypes = x.filter(f=> f.active);
        });
    }

    private save() {        
        this.progress.startSaveProgress((completion) => {
            this.employeeService.saveEmploymentType(this.employmentType).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        this.employmentTypeId = result.integerValue;
                        this.employmentType.employmentTypeId = this.employmentTypeId;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.employmentType);
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
        
        if (!this.employmentType.employmentTypeId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deleteEmploymentType(this.employmentType.employmentTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.employmentType, true);
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
        this.employmentTypeId = 0;
        this.employmentType = {
            employmentTypeId: 0,
            active: true,
        };
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.employmentType) {
                // Mandatory fields
                if (!this.employmentType.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}
