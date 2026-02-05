import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ProjectTimeBlockDTO } from "../../../Common/Models/ProjectDTO";
import { TimeProjectContainer } from "../../../Util/Enumerations";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { IProjectSmallDTO } from "../../../Scripts/TypeLite.Net4";
import { IProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { TermGroup_ProjectType, Feature } from "../../../Util/CommonEnumerations";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ISoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { AttestStateDTO } from "../../../Common/Models/AttestStateDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Guid } from "../../../Util/StringUtility";

export class ExpenseEditController extends EditControllerBase2 implements ICompositionEditController {

    // Permissions
    private modifyOtherEmployeesPermission: boolean = false;

    // Properties
    private isLocked = true;
    private projectTimeBlockRows: ProjectTimeBlockDTO[];
    private projectType = TermGroup_ProjectType.TimeProject;
    private projectContainer = TimeProjectContainer.TimeSheet;
    private employeeId = 0;
    private timeProjectFrom: Date;
    private timeProjectTo: Date;
    private groupByDate = false;
    private dateRangeText: string;

    // User settings
    private userSettingTimeAttestDisableSaveAttestWarning = false;

    //Attest
    private userValidPayrollAttestStates: AttestStateDTO[] = [];
    private userValidPayrollAttestStatesOptions: any = [];

    // Flags
    private loadingTimeProjectRows = false;
    private loadTimeProjectRowsTimeout: any;
    private useExtendedTimeRegistration = false;
    private hasSelectedRows = false;
    private useProjectTimeBlocks = false;
    private usePayroll = false;
    private usedPayrollSince: Date;
    private resultLoaded = false;

    // Collections
    private terms: { [index: string]: string };
    private employees: SmallGenericType[];
    private customers: SmallGenericType[];
    private projects: IProjectSmallDTO[];
    //private projectInvoices: SoftOne.IProjectInvoiceSmallDTO[];

    //Project central
    projectId: number;
    includeChildProjects: boolean;
    orders: number[];

    // Migration
    currentGuid: Guid;
    timerToken: any;
    modal: angular.ui.bootstrap.IModalService;

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    get showMigrateButton(): boolean {
        return CoreUtility.isSupportAdmin && this.useProjectTimeBlocks && this.usePayroll;
    }

    //@ngInject
    constructor(
        urlHelperService: IUrlHelperService,
        projectService: IProjectService,
        coreService: ICoreService,
        $q: ng.IQService,
        $scope: ng.IScope,
        $timeout: ng.ITimeoutService,
        notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response));
    }

    // SETUP
    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Order_Orders_Edit_Expenses, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Billing_Order_Orders_Edit_Expenses].modifyPermission;
        this.isLocked = !this.modifyPermission;
    }
}