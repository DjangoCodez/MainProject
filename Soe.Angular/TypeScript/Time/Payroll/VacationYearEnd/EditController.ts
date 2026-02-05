import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IPayrollService } from "../PayrollService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { Feature, TermGroup_VacationYearEndStatus, TermGroup_SysPageStatusSiteType, TermGroup_VacationYearEndHeadContentType, ApplicationSettingType, SettingMainType } from "../../../Util/CommonEnumerations";
import { SelectionCollection } from "../../../Core/RightMenu/ReportMenu/SelectionCollection";
import { EmployeeSelectionDTO, SelectionDTO } from "../../../Common/Models/ReportDataSelectionDTO";
import { Constants } from "../../../Util/Constants";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IEmployeeSelectionDTO, IVacationYearEndEmployeeResultDTO } from "../../../Scripts/TypeLite.Net4";
import { VacationYearEndResultDialogController } from "../../Dialogs/VacationYearEndResult/VacationYearEndResultDialog";
import { SaveProgressCompletion } from "../../../Core/Handlers/ProgressHandler";


export class EditController extends EditControllerBase2 implements ICompositionEditController {
    private date: Date;
    private selectedVacationGroups = [];
    private siteType: TermGroup_SysPageStatusSiteType;
    private showTestDate: boolean;
    private useTestDate: boolean;
    private testDate: Date;
    private vacationGroups = [];
    private endDates = [];
    private fromDate: Date;
    private toDate: Date;
    private selections: SelectionCollection;
    private userSelectionInput: EmployeeSelectionDTO;
    private selectedVacationGroupNames: string;

    // Properties
    private _contentType: TermGroup_VacationYearEndHeadContentType;
    private get contentType(): TermGroup_VacationYearEndHeadContentType {
        return this._contentType;
    }
    private set contentType(type: TermGroup_VacationYearEndHeadContentType) {
        this._contentType = type;
        if (this.isEmployee && this.endDates.length === 0)
            this.loadVacationGroupEndDates();
    }

    private get isEmployee(): boolean {
        return this.contentType === TermGroup_VacationYearEndHeadContentType.Employee;
    }

    private get isVacationGroup(): boolean {
        return this.contentType === TermGroup_VacationYearEndHeadContentType.VacationGroup;
    }

    private get selectedEmployeeIds(): number[] {
        let selections: SelectionDTO[] = this.selections.materialize();
        let empSelection: EmployeeSelectionDTO = <EmployeeSelectionDTO>_.find(selections, s => s.key === 'employees');
        if (empSelection)
            return empSelection.employeeIds;

        return [];
    }

    //@ngInject
    constructor(
       
        private notificationService: INotificationService,
        private $q: ng.IQService,
        protected $uibModal,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        protected urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private coreService: ICoreService,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
       
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
            
            
        this.fromDate = CalendarUtility.getDateToday().beginningOfYear();
        this.toDate = CalendarUtility.getDateToday().endOfYear();
        this.selections = new SelectionCollection();
       
   
    }
    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Time_Payroll_VacationYearEnd, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Payroll_VacationYearEnd].readPermission;
        this.modifyPermission = response[Feature.Time_Payroll_VacationYearEnd].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
       
    }

    // SETUP
    protected doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadSiteType(),
            this.loadVacationGroups()
        ]).then(() => {
            this.contentType = TermGroup_VacationYearEndHeadContentType.VacationGroup;
        });   
    }

    private loadSiteType(): ng.IPromise<any> {
        return this.coreService.getSiteType().then(type => {
            this.siteType = type;
            if (this.siteType === TermGroup_SysPageStatusSiteType.Test) {
                this.showTestDate = true;
                return this.$q.resolve();
            }
            else {
                return this.coreService
                    .getBoolSetting(SettingMainType.Application, ApplicationSettingType.ShowVacationYearEndTestDate)
                    .then(x => {
                        this.showTestDate = x;
                    });
            }
        });
    }

    private loadVacationGroups(): ng.IPromise<any> {
        return this.payrollService.getVacationGroupsDict(false).then((x) => {
            this.vacationGroups = [];
            _.forEach(x, (vacationGroup: any) => {
                this.vacationGroups.push({
                    id: vacationGroup.id,
                    label: vacationGroup.name
                });
            });
        });
    }
    private loadVacationGroupEndDates(): ng.IPromise<any> {
  
        this.date = null;
        return this.payrollService.getVacationGroupEndDates(_.map(this.vacationGroups, v => v.id)).then(x => {
            this.endDates = [];
            _.forEach(x, (date: Date) => {
                date = CalendarUtility.convertToDate(date);
                this.endDates.push({
                    date: date,
                    name: date.toFormattedDate()
                });
            });
            if (this.endDates.length > 0) {
                this.endDates = _.orderBy(this.endDates, d => d.date, 'desc');
                this.date = this.endDates[0].date;
            }
        });
    }
    // EVENTS
    private selectionChanged() {
        this.getDates();
        this.setSelectedVacationGroupNames();
        this.dirtyHandler.setDirty();
    }

    private onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);

        if (selection.employeeIds.length > 0)
            this.validateVacationYearEnd();
    }

    // ACTIONS
  
    private getDates() {
        if (this.selectedVacationGroups.length === 0) {
            this.date = null;
            return;
        }

        this.payrollService.getVacationGroupEndDates(this.selectedVacationGroups.map(v => v.id)).then(x => {
            if (x.length == 0) {
                // No dates, clear date
                this.date = null;
            } else if (x.length == 1) {
                // One date, use it
                this.date = new Date(x[0]);
            } else {
                // Multiple dates, clear date and show error message
                this.date = null;

                let keys: string[] = [
                    "error.invalid_parameters",
                    "time.payroll.vacationyearend.error.multipledates"
                ];
                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialogEx(terms["error.invalid_parameters"], terms["time.payroll.vacationyearend.error.multipledates"], SOEMessageBoxImage.Error);
                });
            }
        });
    }
    private validateVacationYearEnd(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        let vacationGroupIds = this.isVacationGroup ? this.selectedVacationGroups.map(v => v.id) : [];
        let employeeIds = this.isEmployee ? this.selectedEmployeeIds : [];
        if (vacationGroupIds.length === 0 && employeeIds.length === 0) {
            deferral.resolve(false);
        } else {
            this.payrollService.validateVacationYearEnd(this.date, vacationGroupIds, employeeIds).then(result => {
                if (!result.success) {
                    this.translationService.translate("error.invalid_parameters").then(term => {
                        this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                    });
                }
                deferral.resolve(result.success);
            });
        }

        return deferral.promise;
    }

    private initSave() {
        if (this.isEmployee) {
            this.validateVacationYearEnd().then(passed => {
                if (passed)
                    this.save();
            })
        } else {
            this.save();
        }
    }
    private save() {
        let contentTypeIds: number[] = this.isVacationGroup ? this.selectedVacationGroups.map(v => v.id) : this.selectedEmployeeIds;
        var dateToUse = this.date;
        if (this.useTestDate) {
            dateToUse = this.testDate;
        }

         this.progress.startSaveProgress((completion) => {
            this.payrollService.saveVacationYearEnd(this.contentType, contentTypeIds, dateToUse).then((ret) => {
                if (ret.result.success) {
                    if (ret.employeeResults && ret.employeeResults.find(f => f.status === TermGroup_VacationYearEndStatus.Failed)) {
                        if (ret.employeeResults.filter(f => f.status === TermGroup_VacationYearEndStatus.Failed).length == ret.employeeResults.length) {
                            this.translationService.translate("time.payroll.vacationyearend.doneallfails").then(term => {
                                this.openResultDialog(ret.employeeResults, completion, term);
                            });
                        } else {
                            this.translationService.translate("time.payroll.vacationyearend.donewithfails").then(term => {
                                this.openResultDialog(ret.employeeResults, completion, term);
                            });
                        }
                    } else {
                        this.translationService.translate("time.payroll.vacationyearend.done").then(term => {
                            this.openResultDialog(ret.employeeResults, completion, term);
                        });
                    }
                } else {
                    completion.failed(ret.result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.closeMe(true);
            });
    }



    // HELP-METHODS
    public openResultDialog(data: IVacationYearEndEmployeeResultDTO[], completion: SaveProgressCompletion, header: string) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/VacationYearEndResult/Views/VacationYearEndResultDialog.html"),
            controller: VacationYearEndResultDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                results: () => { return data },
                date: () => { return this.date },
                showVacationGroup: () => { return true },
                header: () => { return header }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
        }, () => {
            completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.selectedVacationGroups);
        });
    }

    private setSelectedVacationGroupNames() {
        var vacationGroupNames: string[] = [];
        var groups: any[] = _.sortBy(this.vacationGroups, g => g.label);
        _.forEach(groups, vacationGroup => {
            if (_.includes(this.selectedVacationGroups.map(v => v.id), vacationGroup.id))
                vacationGroupNames.push(vacationGroup.label);
        });

        this.selectedVacationGroupNames = vacationGroupNames.join(', ');
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.isVacationGroup && this.selectedVacationGroups.length === 0) {
                validationErrorKeys.push("time.payroll.vacationyearend.error.missinggroups");
            }

            if (this.useTestDate) {
                if (!this.testDate)
                   validationErrorKeys.push("time.payroll.vacationyearend.error.missingdate");
            }
            else {
                if (!this.date)
                    validationErrorKeys.push("time.payroll.vacationyearend.error.missingdate");
            }
        });
    }
}
