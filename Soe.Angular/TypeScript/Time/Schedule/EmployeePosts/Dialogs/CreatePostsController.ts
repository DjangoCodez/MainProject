import { IScheduleService } from "../../ScheduleService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { CompanySettingType } from "../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { EmbeddedGridController } from "../../../../Core/Controllers/EmbeddedGridController";

export class CreatePostsController {

    private progress: IProgressHandler;

    // Terms
    private terms: { [index: string]: string; };

    // Company settings
    private useAccountsHierarchy = false;

    // Properties
    private _selectedDate: Date;
    private get selectedDate(): Date {
        return this._selectedDate;
    }
    private set selectedDate(date: Date) {
        // Always start on a monday
        this._selectedDate = date ? date.beginningOfWeek() : undefined;
    }

    private startDateOptions = {
        dateDisabled: this.disabledFromDates,
        customClass: this.getDayClass
    };

    // Grid
    private gridHandler: EmbeddedGridController;

    // Flags
    private executing = false;

    private modalInstance: any;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private $uibModalInstance,
        $uibModal,
        private $q: ng.IQService,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.selectedDate = undefined;

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "time.schedule.employeepost.createposts");

        this.progress.startLoadingProgress([() => {
            return this.loadLookups().then(() => {
                this.setupGrid();
            });
        }]);

        this.modalInstance = $uibModal;
    }


    private setupGrid() {
        this.gridHandler.gridAg.options.enableRowSelection = true;
        this.gridHandler.gridAg.options.enableGridMenu = false;
        this.gridHandler.gridAg.options.enableContextMenu = false;
        this.gridHandler.gridAg.options.enableFiltering = false;
        this.gridHandler.gridAg.options.showAlignedFooterGrid = false;
        this.gridHandler.gridAg.options.setMinRowsToShow(10);

        // Columns
        const keys: string[] = [
            "core.datefrom",
            "core.workfailed",
            "common.name",
            "common.selected",
            "common.employee",
            "common.user.attestrole.accounthierarchy",
            "time.schedule.employeepost.worktimeweek",
            "time.employee.employeegroup.employeegroup",
            "time.employee.employee.accountswithdefault"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.gridHandler.gridAg.addColumnText("employeeName", terms["common.employee"], 250);
            this.gridHandler.gridAg.addColumnText("employeeGroupName", terms["time.employee.employeegroup.employeegroup"], null);
            this.gridHandler.gridAg.addColumnDate("dateFrom", terms["core.datefrom"], 100, true);
            if (this.useAccountsHierarchy)
                this.gridHandler.gridAg.addColumnText("name", terms["time.employee.employee.accountswithdefault"], null, true);

            this.gridHandler.gridAg.finalizeInitGrid("time.schedule.employeepost.createposts", false);
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }


    // Service calls

    private loadLookups(): ng.IPromise<any> {
        let deferral = this.$q.defer<any>();

        this.$q.all([
            this.loadCompanySettings()
        ]).then(() => {
            deferral.resolve();
        });

        return deferral.promise;
    }


    // Events

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.executing = true;

        this.progress.startWorkProgress((completion) => {
            this.scheduleService.createEmployeePostsFromEmployments(this.gridHandler.gridAg.options.getSelectedIds("employmentId"), this.selectedDate ? this.selectedDate : CalendarUtility.getDateNow()).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                    this.executing = false;
                    this.$uibModalInstance.close();
                } else {
                    completion.failed(this.terms["core.workfailed"]);
                }
            }, error => {
                completion.failed(this.terms["core.workfailed"]);
            });
        });
    }

    private search() {
        return this.scheduleService.getEmploymentsForCreatingEmployeePosts(this.selectedDate ? this.selectedDate : CalendarUtility.getDateNow()).then(x => {
            this.gridHandler.gridAg.options.setData(x);
        });
    }

    // Help-methods

    private disabledFromDates(data) {
        return (data.mode === 'day' && data.date.getDay() !== 1);
    }

    private getDayClass(data) {
        return (data.mode === 'day' && data.date.getDay() !== 1) ? 'disabledDate' : '';
    }
}
