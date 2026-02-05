import { IUrlHelperService} from "../../../Core/Services/UrlHelperService";
import { IPayrollService } from "../PayrollService";
import { Feature, TermGroup, TermGroup_SysPayrollType } from "../../../Util/CommonEnumerations";
import { PayrollProductGridDTO } from "../../../Common/Models/ProductDTOs";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    unionFee: any;
    unionFeeId: number;
    payrollPriceTypes = [];
    payrollProducts : PayrollProductGridDTO[] = [];
    associations: ISmallGenericType[] = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private payrollService: IPayrollService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private coreService: ICoreService,
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
        this.unionFeeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Payroll_UnionFee, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Payroll_UnionFee].readPermission;
        this.modifyPermission = response[Feature.Time_Payroll_UnionFee].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    // SETUP

    protected doLookups(): ng.IPromise<any> {
       return this.$q.all([this.loadPayrollPriceTypes(),
           this.loadPayrollProducts(),
           this.loadAssociations()]);
    }

    private onLoadData() {
        if (this.unionFeeId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    // LOOKUPS

    private loadPayrollPriceTypes(): ng.IPromise<any> {
        return this.payrollService.getPayrollPriceTypesDict(true).then((x) => {
            this.payrollPriceTypes = x;
        });
    }

    private loadPayrollProducts(): ng.IPromise<any> {
        return this.payrollService.getPayrollProductsGrid(true).then((x) => {
            this.payrollProducts = _.filter(x, product => product.sysPayrollTypeLevel1 == TermGroup_SysPayrollType.SE_Deduction && product.sysPayrollTypeLevel2 == TermGroup_SysPayrollType.SE_Deduction_UnionFee);           
        });
    }

    private load(): ng.IPromise<any> {
        return this.payrollService.getUnionFee(this.unionFeeId).then((x) => {
            this.isNew = false;
            this.unionFee = x;

            if (!this.unionFee.payrollPriceTypeIdPercent)
                this.unionFee.payrollPriceTypeIdPercent = 0;
            if (!this.unionFee.payrollPriceTypeIdPercentCeiling)
                this.unionFee.payrollPriceTypeIdPercentCeiling = 0;
            if (!this.unionFee.payrollPriceTypeIdFixedAmount)
                this.unionFee.payrollPriceTypeIdFixedAmount = 0;
            if (!this.unionFee.payrollProductId)
                this.unionFee.payrollProductId = 0;
        });
    }

    private loadAssociations() {
        return this.coreService.getTermGroupContent(TermGroup.UnionFeeAssociation, false, false).then(x => {
            this.associations = x;
        });
    }
    // Save
    private save() {

        this.progress.startSaveProgress((completion) => {
            this.payrollService.saveUnionFee(this.unionFee).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        this.unionFeeId = result.integerValue;
                        this.unionFee.endReasonId = this.unionFeeId;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.unionFee);
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

        if (!this.unionFee.unionFeeId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.payrollService.deleteUnionFee(this.unionFee.unionFeeId).then((result) => {
                if (result.success) {
                    completion.completed(this.unionFee, true);
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
        this.unionFeeId = 0;
        this.unionFee = {};
        this.unionFee.association = 0;
    }

    // EVENTS
    private payrollPriceTypePercentChanged(id) {
        if (id == 0) {
            this.unionFee.payrollPriceTypeIdPercentCeiling = 0;
        }
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.unionFee) {
                // Mandatory fields
                if (!this.unionFee.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}
