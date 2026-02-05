import { ActivateScheduleControlDTO, ActivateScheduleControlHeadDTO } from "../../../Common/Models/EmployeeScheduleDTOs";
import { EditController as AbsenceRequestsEditController } from "../../../Shared/Time/Schedule/Absencerequests/EditController";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { NotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { UrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISmallGenericType, ITimeDeviationCauseDTO } from "../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature, CompanySettingType, TermGroup, TermGroup_ControlEmployeeSchedulePlacementType } from "../../../Util/CommonEnumerations";
import { AbsenceRequestGuiMode, AbsenceRequestParentMode, AbsenceRequestViewMode, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ExportUtility } from "../../../Util/ExportUtility";
import { TimeService } from "../../Time/Timeservice";
import { Constants } from "../../../Util/Constants";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ActiveScheduleChangedScheduleDialogController } from "./ActiveScheduleChangedScheduleDialogController";

export class ActiveScheduleControlDialogController {
    public progress: IProgressHandler;
    private terms: { [index: string]: string; };
    private modifyAbsenceRequests: boolean = false;

    private gridHandler: EmbeddedGridController;
    controlEmployeeSchedulePlacementType: ISmallGenericType[];
    timeDeviationCauses: ITimeDeviationCauseDTO[] = [];
    
    private absenceRequestLoaderPromise: Promise<any>;
    private hiddenShort: boolean = false;
    private defaultEmployeeAccountDimId: number = 0;
    private defaultEmployeeAccountDimName: string = '';
    private useAccountsHierarchy: boolean = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private timeService: TimeService,
        private coreService: ICoreService,
        private control: ActivateScheduleControlDTO,
        private activateDate: Date,
        private notificationService: NotificationService,
        private urlHelperService: UrlHelperService,
        private $scope,
        gridHandlerFactory: IGridHandlerFactory) {

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "ActivateSchedule");

        this.$q.all([
            this.loadCompanySettings(),
            this.loadPermissions(),
            this.loadTypes(),
            this.loadTerms(),
            this.loadTimeDeviationCauses(),
        ]).then(() => {
            this.populateGrid();
            this.setupGrid();
        });
    }

    private populateGrid() {
        this.control.heads.forEach((head) => {
            if (head.type !== TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest)
                head.hideCheckbox = true;
            if (head.type === TermGroup_ControlEmployeeSchedulePlacementType.ShortenIsHidden) {
                this.hiddenShort = true;
                head.startDate = head.rows[0].start; 
            }
                
        });

        this.gridHandler.gridAg.setData(this.control.heads);
    }

    private setupGrid() {
        let colDefEmployee = this.gridHandler.gridAg.addColumnText("employeeNrAndName", this.terms["common.employee"], 120, false, { enableRowGrouping: true, showRowGroup: "name" });
        this.gridHandler.gridAg.addColumnDate("startDate", this.terms["common.startdate"], 100, false, null, { editable: false });
        this.gridHandler.gridAg.addColumnDate("stopDate", this.terms["common.stopdate"], 100, false, null, { editable: false });
        this.gridHandler.gridAg.addColumnText("timeDeviationCauseName", this.terms["common.time.timedeviationcause"], 120, false, { editable: false });
        this.gridHandler.gridAg.addColumnText("typeName", this.terms["common.type"], 180, false, { editable: false });
        this.gridHandler.gridAg.addColumnText("comment", this.terms["core.info"], 180, false, { editable: false });
        this.gridHandler.gridAg.addColumnText("statusName", this.terms["common.status"], 80, false, { editable: false });
        this.gridHandler.gridAg.addColumnText("resultStatusName", this.terms["time.schedule.absencerequests.result"], 80, false, { editable: false });
        this.gridHandler.gridAg.addColumnBoolEx("reActivateAbsenceRequest", this.terms["time.schedule.activate.reactivate"], 100, { enableEdit: true, disabledField: "hideCheckbox", suppressSizeToFit: true });
        if (this.modifyAbsenceRequests)
            this.gridHandler.gridAg.addColumnIcon(null, null, 50, { icon: "fal fa-info-circle infoColor", toolTip: this.terms["core.info"], onClick: this.openInformation.bind(this), suppressSizeToFit: true, showIcon: (row) => { return row && (row.type == TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest || row.type == TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedSchedule) }  });  
        
        this.gridHandler.gridAg.options.enableRowSelection = false;
        this.gridHandler.gridAg.options.setMinRowsToShow(15);
        this.gridHandler.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: true, selectChildren: false, hideGroupPanel: true, suppressCount: true });
        this.gridHandler.gridAg.options.groupRowsByColumn(colDefEmployee, colDefEmployee.field, 1);
        this.gridHandler.gridAg.options.groupHideOpenParents = true;
        this.gridHandler.gridAg.finalizeInitGrid("time.schedule.activate", true);

    }
    public loadPermissions(): ng.IPromise<any> {
        const features: number[] = [];
        features.push(Feature.Time_Schedule_AbsenceRequests);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.modifyAbsenceRequests = x[Feature.Time_Schedule_AbsenceRequests];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
           
            if (this.useAccountsHierarchy && this.defaultEmployeeAccountDimId != 0)
                this.loadDefaultEmployeeAccount();
        });
    }


    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.status",
            "common.startdate",
            "common.employee",
            "common.stopdate",
            "common.date",
            "common.type",
            "common.messages.showrequest",
            "common.time.timedeviationcause",
            "common.start",
            "common.stop",
            "core.info",
            "time.schedule.activate.reactivate",
            "time.schedule.activate",
            "core.exportexcel",
            "time.schedule.planning.copyschedule.targetdatestart",
            "time.schedule.planning.copyschedule.targetdateend",
            "time.schedule.planning.wholedaylabel",
            "time.schedule.absencerequests.result",
            "core.warning",
            "time.schedule.activate.confirmtext",
            "time.schedule.activate.delete.message.hidden.info.accountshierarchy",
            "time.schedule.activate.delete.message.hidden.info.category",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTypes(): ng.IPromise<any> {
        this.controlEmployeeSchedulePlacementType = [];
        return this.coreService.getTermGroupContent(TermGroup.ControlEmployeeSchedulePlacementType, false, true).then((x) => {
            this.controlEmployeeSchedulePlacementType = x;
        });
    }
   
    private loadTimeDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCauses().then((x) => {
            this.timeDeviationCauses = x;
        });
    }
    private loadDefaultEmployeeAccount(): ng.IPromise<any> {
        return this.coreService.getAccountDim(this.defaultEmployeeAccountDimId, true, false, false, true).then(x => {
            this.defaultEmployeeAccountDimName = x.name;
        })
    }

    private ok() {

        var msg: string = this.terms["time.schedule.activate.confirmtext"]
        if (this.hiddenShort) {
            if (this.useAccountsHierarchy)
                msg += "\n<b>" + this.terms["time.schedule.activate.delete.message.hidden.info.accountshierarchy"] + ' ' + this.defaultEmployeeAccountDimName + '</b>'
            else
                msg += "\n<b>" + this.terms["time.schedule.activate.delete.message.hidden.info.category"] + '</b>'
        }

        var modal = this.notificationService.showDialogEx(this.terms["core.warning"], msg, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val) {
               this.close(true);
            }
        });

    }

    private exportToExcel() {

        let headers: string[] = [];
        headers.push(this.terms["common.employee"].replace("ä", "a"));
        headers.push(this.terms["common.startdate"]);
        headers.push(this.terms["common.stopdate"]);
        headers.push(this.terms["common.date"]); 
        headers.push(this.terms["common.time.timedeviationcause"]);
        headers.push(this.terms["common.type"]);
        headers.push(this.terms["time.schedule.planning.copyschedule.targetdatestart"]);
        headers.push(this.terms["time.schedule.planning.copyschedule.targetdateend"]);
        headers.push(this.terms["common.start"]);
        headers.push(this.terms["common.stop"]);
        headers.push(this.terms["time.schedule.planning.wholedaylabel"]);
        headers.push(this.terms["core.info"]);
        headers.push(this.terms["common.status"]);
        headers.push(this.terms["time.schedule.absencerequests.result"]);

        let content: string = headers.join(';') + '\r\n';
        let fileName: string = this.terms["time.schedule.activate"].replace('ö', 'o') + " ";
        _.forEach(this.control.heads, head => {
            let timeDeviationCause = this.timeDeviationCauses.find(p => p.timeDeviationCauseId === head.timeDeviationCauseId)?.name ?? " ";
            let comment = head.comment ? head.comment : " ";
            let statusName = head.statusName ? head.statusName : " ";
            let resultStatusName = head.resultStatusName ? head.resultStatusName : " ";

            if (head.rows) {
                _.forEach(head.rows, rowDetails => {
                    let rowContent: string[] = [];
                    let type = this.controlEmployeeSchedulePlacementType.find(p => p.id === rowDetails.type)?.name ?? " ";

                    rowContent.push(head.employeeNrAndName.replace("ä", "a").replace("å", "a").replace("ö", "o"));
                    rowContent.push(CalendarUtility.toFormattedDate(head.startDate));
                    rowContent.push(CalendarUtility.toFormattedDate(head.stopDate));
                    rowContent.push(CalendarUtility.toFormattedDate(rowDetails.date));
                    rowContent.push(timeDeviationCause.replace("ä", "a").replace("å", "a").replace("ö", "o"));
                    rowContent.push(type.replace("ä", "a").replace("å", "a").replace("ö", "o"));
                    rowContent.push(CalendarUtility.toFormattedTime(rowDetails.scheduleStart));
                    rowContent.push(CalendarUtility.toFormattedTime(rowDetails.scheduleStop));
                    rowContent.push(CalendarUtility.toFormattedTime(rowDetails.start));
                    rowContent.push(CalendarUtility.toFormattedTime(rowDetails.stop));
                    rowContent.push(rowDetails.isWholeDayAbsence ? "1" : "0");
                    rowContent.push(comment.replace("ä", "a").replace("å", "a").replace("ö", "o"));
                    rowContent.push(statusName.replace("ä", "a").replace("å", "a").replace("ö", "o"));
                    rowContent.push(resultStatusName.replace("ä", "a").replace("å", "a").replace("ö", "o"));

                    content += rowContent.join(';') + '\r\n'

                });
            } else {
                let rowContent: string[] = [];
                rowContent.push(head.employeeNrAndName.replace("ä", "a").replace("å", "a").replace("ö", "o"));
                rowContent.push(CalendarUtility.toFormattedDate(head.startDate));
                rowContent.push(CalendarUtility.toFormattedDate(head.stopDate));
                rowContent.push("");
                rowContent.push(timeDeviationCause.replace("ä", "a").replace("å", "a").replace("ö", "o"));
                rowContent.push("");
                rowContent.push("");
                rowContent.push("");
                rowContent.push("");
                rowContent.push("");
                rowContent.push("");
                rowContent.push(comment.replace("ä", "a").replace("å", "a").replace("ö", "o"));
                rowContent.push(statusName.replace("ä", "a").replace("å", "a").replace("ö", "o"));
                rowContent.push(resultStatusName.replace("ä", "a").replace("å", "a").replace("ö", "o"));

                content += rowContent.join(';') + '\r\n'
            }
        });
        
        ExportUtility.ExportToCSV(content, fileName + '.csv');
    }

    public openInformation(row: ActivateScheduleControlHeadDTO) {
        if (row.type == TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest)
            this.openAbsenceRequest(row);
        else if (row.type == TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasChangedSchedule)
            this.openScheduleChanges(row);
    }

    public openScheduleChanges(row: ActivateScheduleControlHeadDTO) {
       this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/ActiveScheduleControl/Views/ActiveScheduleChangedScheduleDialog.html"),
            controller: ActiveScheduleChangedScheduleDialogController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            keyboard: true,
            size: 'lg',
            
            scope: this.$scope,
            resolve: {
                scheduleChanges: () => { return row; },
            }
        });

    }

    public openAbsenceRequest(row: ActivateScheduleControlHeadDTO) {

        this.$q.all([this.absenceRequestLoaderPromise]).then(() => {
            const modal = this.$uibModal.open({
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
                    id: row.employeeRequestId,
                    employeeId: row.employeeId,
                    viewMode: AbsenceRequestViewMode.Attest,
                    loadRequestFromInterval: true,
                    shiftId: 0,
                    guiMode: AbsenceRequestGuiMode.EmployeeRequest,
                    skipXEMailOnShiftChanges: false,
                    parentMode: AbsenceRequestParentMode.SchedulePlanning,
                    timeScheduleScenarioHeadId: null,
                    hideOptionSelectedShift: false,
                    readonly: false,

                });
            });
        });
    }
    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private close(reload: boolean = false) {
        this.$uibModalInstance.close({ reload: reload });
    }
}