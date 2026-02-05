import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ShiftTypeLinkDTO, ShiftTypeDTO } from "../../../Common/Models/ShiftTypeDTO";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { SelectShiftTypesController } from "../../Dialogs/SelectShiftTypes/SelectShiftTypesController";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private shiftTypeLinks: ShiftTypeLinkDTO[];

    // Lookups 
    private gridTerms: any;
    private shiftTypes: ShiftTypeDTO[];

    // Grid
    protected gridOptions: ISoeGridOptions;
    protected currentlyEditing: {
        entity: any;
        colDef: uiGrid.IColumnDef;
    };

    private modalInstance: any;

    // CompanySettings

    //@ngInject
    constructor(
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private focusService: IFocusService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory
    ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.initGrid();
        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    onInit(parameters: any) {
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([
            { feature: Feature.Time_Preferences_ScheduleSettings_ShiftTypeLink, loadModifyPermissions: true },
        ])
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Time_Preferences_ScheduleSettings_ShiftTypeLink].modifyPermission;
    }

    private onDoLookups() {
        return this.$q.all([
            this.loadShiftTypes()])
            .then(() => {
                return this.$q.all([
                    this.setupGrid()
                ]);
            });
    }

    private onLoadData(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        this.scheduleService.getShiftTypeLinks().then((x: ShiftTypeLinkDTO[]) => {
            // Convert to typed DTOs
            this.shiftTypeLinks = x.map(s => {
                var obj = new ShiftTypeLinkDTO;
                angular.extend(obj, s);
                return obj;
            });

            this.resetShiftTypeLinks(null);
            deferral.resolve();
        });

        return deferral.promise;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.schedule.shifttypelink.loadshifttypelinks", "time.schedule.shifttypelink.loadshifttypelinks", IconLibrary.FontAwesome, "fa-sync", () => { this.onLoadData(); })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.schedule.shifttypelink.newshifttypelink", "time.schedule.shifttypelink.newshifttypelink", IconLibrary.FontAwesome, "fa-plus", () => { this.addShiftTypeLink(); }, () => { return !this.isValidForLoad(); })));
    }

    // LOOKUPS

    private loadShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypes(false, false, false, false, false, false).then(x => {
            this.shiftTypes = x;
        });
    }

    // SETUP

    private initGrid() {
        this.gridOptions = new SoeGridOptions("Time.Schedule.ShiftTypeLink", this.$timeout, this.uiGridConstants);
        this.gridOptions.enableGridMenu = false;
        this.gridOptions.enableColumnMenus = false;
        this.gridOptions.showGridFooter = false;
        this.gridOptions.enableRowSelection = true;
        this.gridOptions.expandableRowScope = {};
        this.gridOptions.enableDoubleClick = false;
        this.gridOptions.setData([]);
    }

    private setupGrid(): ng.IPromise<any> {

        var keys: string[] = [
            "core.delete",
            "core.edit",
            "common.shifttype",
            "time.time.shifttypelink.nrofshifttypes",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.gridTerms = terms;

            this.gridOptions.addColumnText("nrOfShiftTypes", terms["time.time.shifttypelink.nrofshifttypes"], "100");
            this.gridOptions.addColumnText("shiftTypeNames", terms["common.shifttype"], null, true, null, terms["core.edit"], null, null, null, null, "fal fa-pencil iconEdit", "selectShiftTypes");

            if (this.modifyPermission)
                this.gridOptions.addColumnDelete(terms["core.delete"], "deleteShiftTypeLinks");
        });
    }

    // ACTIONS

    public addShiftTypeLink(empty: boolean = false) {
        this.translationService.translate("core.notspecified").then((term) => {
            var shiftTypeLink = new ShiftTypeLinkDTO();
            shiftTypeLink.guid = null;
            shiftTypeLink.shiftTypes = [];

            if (!this.shiftTypeLinks)
                this.shiftTypeLinks = [];
            this.shiftTypeLinks.push(shiftTypeLink);
            this.gridOptions.focusRowByRow(shiftTypeLink, 0);
        });
    }

    public deleteShiftTypeLinks(shiftTypeLink: ShiftTypeLinkDTO) {
        shiftTypeLink.shiftTypes = [];
    }

    public saveShiftTypeLinks() {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveShiftTypeLinks(this.shiftTypeLinks).then((result) => {
                if (result.success) {
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.shiftTypeLinks);
                    this.dirtyHandler.clean();
                    this.onLoadData();
                } else {
                    completion.failed(result.errorMessage);
                }
            })
        }, this.guid);
    }

    // HELP-METHODS

    protected selectShiftTypes(row) {
        if (!Array.isArray(row.shiftTypes))
            row.shiftTypes = [row.shiftTypes];

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/SelectShiftTypes/selectShiftTypes.html"),
            controller: SelectShiftTypesController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                shiftTypes: () => { return this.shiftTypes },
                selectedShiftTypes: () => { return _.map(_.filter(row.shiftTypes, s => s != null), s => s['shiftTypeId']) }
            }
        });

        modal.result.then(result => {
            if (result && result.success) {
                row.shiftTypes = _.filter(this.shiftTypes, s => _.includes(result.selectedShiftTypes, s.shiftTypeId));
            }
        }, function () {
            // Cancelled
        });

        return modal;
    }

    protected resetShiftTypeLinks(breakTemplate: any) {
        if (this.shiftTypeLinks) {
            this.gridOptions.setData(this.shiftTypeLinks);
        }
        if (breakTemplate) {
            this.gridOptions.scrollToFocus(breakTemplate, 1);
        }
    }

    protected isValidForLoad() {
        return this.shiftTypes;
    }

    // VALIDATION

    public showValidationError() {

    }
}