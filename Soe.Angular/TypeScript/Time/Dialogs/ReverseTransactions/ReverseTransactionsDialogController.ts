import { DialogControllerBase } from "../../../Core/Controllers/DialogControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITimeService } from "../../../Time/Time/TimeService";
import { ITimeService as ISharedTimeService } from "../../../Shared/Time/Time/TimeService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { TimePeriodDTO } from "../../../Common/Models/TimePeriodDTO";
import { ITimeDeviationCauseDTO } from "../../../Scripts/TypeLite.Net4";

export class ReverseTransactionsDialogController extends DialogControllerBase {    
    
    private edit: ng.IFormController;

    private employeeChilds: SmallGenericType[];
    private rbAbsenceType: number = 0;

    private _selectedTimePeriod;
    get selectedTimePeriod(): TimePeriodDTO {
        return this._selectedTimePeriod;
    }
    set selectedTimePeriod(item: TimePeriodDTO) {
        this._selectedTimePeriod = item;                    
        this.edit.$dirty = true;        
    }

    private _selectedTimeDeviationCause;
    get selectedTimeDeviationCause(): ITimeDeviationCauseDTO {
        return this._selectedTimeDeviationCause;
    }
    set selectedTimeDeviationCause(item: ITimeDeviationCauseDTO) {
        this._selectedTimeDeviationCause = item;

        this.tryLoadEmployeeChilds();
    }

    private _selectedEmployeeChild: SmallGenericType;
    get selectedEmployeeChild() {
        return this._selectedEmployeeChild;
    }
    set selectedEmployeeChild(item: SmallGenericType) {        
        this._selectedEmployeeChild = item;        
    }    

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private timeService: ITimeService,
        private sharedTimeService: ISharedTimeService,
        translationService: ITranslationService,
        coreService: ICoreService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,      
        private employeeId: number,
        private usePayroll: boolean,
        private timePeriods: TimePeriodDTO[],
        private deviationCauses: ITimeDeviationCauseDTO[],
        private dates: Date[]) {        

        super(null, translationService, coreService, notificationService, urlHelperService);
        this.modifyPermission = true;               
        
    }

    public cancel() {
        this.$uibModalInstance.close();
    }


    //Service calls
    public save() {    

        if (this.rbAbsenceType == 1 && !this.selectedTimeDeviationCause)
            return;        
        if (this.usePayroll && !this.selectedTimePeriod)
            return;
        
        this.startSave();        
        this.timeService.reverseTransactions(this.employeeId, this.dates, (this.rbAbsenceType == 1 && this.selectedTimeDeviationCause) ? this.selectedTimeDeviationCause.timeDeviationCauseId : null, this.selectedTimePeriod ? this.selectedTimePeriod.timePeriodId : null, this.selectedEmployeeChild ? this.selectedEmployeeChild.id : null).then((result) => {
            if (result.success) {
                this.$uibModalInstance.close(true);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    
    }    
   
    private loadEmployeeChilds(): ng.IPromise<any> {
        return this.sharedTimeService.getEmployeeChildsDict(this.employeeId, false).then(x => {
            this.employeeChilds = x;
            if (this.employeeChilds.filter(ec => ec.id !== 0).length === 1) {
                this.selectedEmployeeChild = _.first(this.employeeChilds.filter(f => f.id !== 0));
            }
        });
    }

    //Help methods    
    private tryLoadEmployeeChilds() {
        if (this.employeeChilds && this.employeeChilds.length !== 0)
            return;

        if (!this.selectedTimeDeviationCause || (this.selectedTimeDeviationCause && this.selectedTimeDeviationCause.specifyChild === false))
            return;

        this.loadEmployeeChilds();
    }

    protected validate() {
        if (this.rbAbsenceType == 1 && !this.selectedTimeDeviationCause)
            this.mandatoryFieldKeys.push("time.time.attest.reversetransactions.absencecause");
        if (this.showEmployeeChild() && !this.selectedEmployeeChild)
            this.mandatoryFieldKeys.push("time.schedule.absencerequests.employeechild");
        if (this.usePayroll && !this.selectedTimePeriod)
            this.mandatoryFieldKeys.push("time.time.attest.reversetransactions.payrollperiod");
    }    

    private showEmployeeChild(): boolean {
        if (this.selectedTimeDeviationCause) {
            return (this.selectedTimeDeviationCause.specifyChild === true);
        }
    }
}
