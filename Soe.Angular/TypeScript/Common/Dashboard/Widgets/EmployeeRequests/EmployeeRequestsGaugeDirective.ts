import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { EditController as AbsenceRequestsEditController } from "../../../../Shared/Time/Schedule/Absencerequests/EditController";
import { SettingDataType, SoeModule, TermGroup_EmployeeRequestType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { AbsenceRequestGuiMode, AbsenceRequestParentMode, AbsenceRequestViewMode } from "../../../../Util/Enumerations";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { EmployeeRequestsGaugeDTO, UserGaugeSettingDTO } from "../../../Models/DashboardDTOs";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class EmployeeRequestsGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('EmployeeRequests', 'EmployeeRequestsGauge.html'), EmployeeRequestsGaugeController);
    }
}

class EmployeeRequestsGaugeController extends WidgetControllerBase {
    private soeGridOptions: ISoeGridOptions;
    private employeeRequests: EmployeeRequestsGaugeDTO[] = [];

    private hideRequests: boolean = false;
    private hideAvailability: boolean = false;

    // Terms
    private terms: { [index: string]: string; };

    // Flags
    private isSchedulePlanningMode: boolean = false;
    private isOrderPlanningMode: boolean = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $uibModal,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        uiGridConstants: uiGrid.IUiGridConstants) {
        super($timeout, $q, uiGridConstants);

        this.soeGridOptions = this.createGrid();
    }

    protected setup(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.widgetHasSettings = true;
        this.widgetCss = 'col-sm-6';

        if (this.widgetUserGauge.module === SoeModule.Time)
            this.isSchedulePlanningMode = true;
        else if (this.widgetUserGauge.module === SoeModule.Billing)
            this.isOrderPlanningMode = true;

        this.loadSettings();
        this.setupGrid();

        deferral.resolve();
        return deferral.promise;
    }

    protected setupGrid(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "common.type",
            "common.from",
            "common.to",
            "common.time.timedeviationcause",
            "common.employee",
            "common.status",
            "common.dashboard.employeerequests.title.req",
            "common.dashboard.employeerequests.title.avail",
            "common.dashboard.employeerequests.title.availandreq"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.soeGridOptions.addColumnText("employeeRequestTypeName", this.terms["common.type"], "15%");
            this.soeGridOptions.addColumnDate("start", this.terms["common.from"], "15%");
            this.soeGridOptions.addColumnDate("stop", this.terms["common.to"], "15%");
            this.soeGridOptions.addColumnText("timeDeviationCauseName", this.terms["common.time.timedeviationcause"], "15%");
            this.soeGridOptions.addColumnText("employeeName", this.terms["common.employee"], null);
            this.soeGridOptions.addColumnText("statusName", this.terms["common.status"], "20%");
            this.soeGridOptions.addColumnIcon("", "fal fa-pencil iconEdit", terms["core.edit"], "edit", null, "enableEdit");

            this.setTitle();
        });
    }

    protected load() {
        super.load();
        this.coreService.getEmployeeRequestsWidgetData(true).then(x => {
            // Settings
            if (this.hideRequests) {
                x = _.filter(x, y => y.employeeRequestType !== TermGroup_EmployeeRequestType.AbsenceRequest);
            }
            if (this.hideAvailability) {
                x = _.filter(x, y => y.employeeRequestType !== TermGroup_EmployeeRequestType.NonInterestRequest && y.employeeRequestType !== TermGroup_EmployeeRequestType.InterestRequest);
            }
            this.employeeRequests = x;

            this.soeGridOptions.setData(this.employeeRequests);
            super.loadComplete(this.employeeRequests.length);
        });
    }

    public loadSettings() {
        var settingShowAbsenseRequests: UserGaugeSettingDTO = this.getUserGaugeSetting('ShowAbsenseRequests');
        this.hideRequests = !(settingShowAbsenseRequests ? settingShowAbsenseRequests.boolData : true);

        var settingShowAvailability: UserGaugeSettingDTO = this.getUserGaugeSetting('ShowAvailability');
        this.hideAvailability = !(settingShowAvailability ? settingShowAvailability.boolData : true);
    }

    public saveSettings() {
        var settings: UserGaugeSettingDTO[] = [];

        var settingShowAbsenseRequests = new UserGaugeSettingDTO('ShowAbsenseRequests', SettingDataType.Boolean);
        settingShowAbsenseRequests.boolData = !this.hideRequests;
        settings.push(settingShowAbsenseRequests);

        var settingShowAvailability = new UserGaugeSettingDTO('ShowAvailability', SettingDataType.Boolean);
        settingShowAvailability.boolData = !this.hideAvailability;
        settings.push(settingShowAvailability);

        this.coreService.saveUserGaugeSettings(this.widgetUserGauge.userGaugeId, settings).then(result => {
            if (result.success)
                this.widgetUserGauge.userGaugeSettings = settings;

            this.setTitle();
        });
    }

    private setTitle() {
        if (!this.hideRequests && !this.hideAvailability)
            this.widgetTitle = this.terms["common.dashboard.employeerequests.title.availandreq"];
        else if (!this.hideRequests && this.hideAvailability)
            this.widgetTitle = this.terms["common.dashboard.employeerequests.title.req"];
        else if (this.hideRequests && !this.hideAvailability)
            this.widgetTitle = this.terms["common.dashboard.employeerequests.title.avail"];
        else
            this.widgetTitle = '';
    }

    private enableEdit(row: EmployeeRequestsGaugeDTO): boolean {
        return row.employeeRequestType === TermGroup_EmployeeRequestType.AbsenceRequest;
    }

    private edit(row: EmployeeRequestsGaugeDTO) {
        if (!row || !this.enableEdit(row))
            return;

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html"),
            controller: AbsenceRequestsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: row.requestId,
                employeeId: row.employeeId,
                viewMode: AbsenceRequestViewMode.Attest,
                guiMode: AbsenceRequestGuiMode.EmployeeRequest,
                skipXEMailOnShiftChanges: false,
                shiftId: 0,
                date: row.start,
                hideOptionSelectedShift: this.isOrderPlanningMode,
                parentMode: AbsenceRequestParentMode.SchedulePlanning,
                timeScheduleScenarioHeadId: null,
            });
        });

        modal.result.then(employeeIds => {
            this.reload();
        });
    }
}