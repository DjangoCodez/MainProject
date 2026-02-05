import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { Feature } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { IPayrollService } from "../../Payroll/PayrollService";
import { PayrollLevelDTO } from "../../../Common/Models/PayrollLevelDTO";
import { IFocusService } from "../../../Core/Services/focusservice";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    payrollLevelId : number;
    private payrollLevel: PayrollLevelDTO;

    // Lookups
    errorMessage: string;
    
    terms: any = [];

    //@ngInject
    constructor(
        private $q: ng.IQService, 
        private $timeout: ng.ITimeoutService,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        private focusService: IFocusService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }


    public onInit(parameters: any) {
        this.payrollLevelId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Employee_PayrollLevels, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_PayrollLevels].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_PayrollLevels].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.payrollLevelId, recordId => {
            if (recordId !== this.payrollLevelId) {
                this.payrollLevelId = recordId;
                this.onLoadData();
            }
        });
    }


    // LOOKUPS

    protected doLookups(): ng.IPromise<any> {
        return this.loadTerms().then(() => {
            return this.$q.all([
                
            ]).then(x => {
             
            });
        });
    }

    private onLoadData() {
        if (this.payrollLevelId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    } 

    private load(): ng.IPromise<any>{
        return this.payrollService.getPayrollLevel(this.payrollLevelId).then((x) => {
            this.isNew = false;
            this.payrollLevel = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.employee.payrolllevel.payrolllevel"] + ' ' + this.payrollLevel.name);
        });        
    }

    private loadTerms() {
        var keys: string[] = [
            "time.employee.payrolllevel.payrolllevel"
          
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private save() {
       
        this.progress.startSaveProgress((completion) => {
            this.payrollService.savePayrollLevel(this.payrollLevel).then((result) => {
                
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.payrollLevelId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.payrollLevel.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.payrollLevelId = result.integerValue;
                        this.payrollLevel.payrollLevelId = this.payrollLevelId;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.payrollLevel);
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
    
    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.payrollService.getPayrollLevels().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.payrollLevelId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.payrollLevelId) {
                    this.payrollLevelId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    protected delete() {
        if (!this.payrollLevel.payrollLevelId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.payrollService.deletePayrollLevel(this.payrollLevel.payrollLevelId).then((result) => {
                if (result.success) {
                    completion.completed(this.payrollLevel, true);
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

    // HELP-METHODS

    protected copy() {
        super.copy();
        this.isNew = true;
        this.payrollLevelId = 0;
        this.payrollLevel.payrollLevelId = 0;
        this.payrollLevel.name = '';
        this.focusService.focusByName("ctrl_payrollLevel_name");
    }

    private new() {
        this.isNew = true;
        this.payrollLevelId = 0;
        this.payrollLevel = new PayrollLevelDTO;
        this.payrollLevel.isActive = true;
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.payrollLevel) {
                // Mandatory fields
                if (!this.payrollLevel.name)
                    mandatoryFieldKeys.push("common.name");
               
            }
        });
    }
}
