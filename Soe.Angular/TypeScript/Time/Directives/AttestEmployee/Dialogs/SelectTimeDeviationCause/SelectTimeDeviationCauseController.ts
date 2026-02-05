import { AccountingSettingsRowDTO } from "../../../../../Common/Models/AccountingSettingsRowDTO";
import { AccountInternalDTO } from "../../../../../Common/Models/AccountInternalDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ITimeDeviationCauseGridDTO, ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { Feature, TimeAttestMode } from "../../../../../Util/CommonEnumerations";
import { IEmployeeService } from "../../../../Employee/EmployeeService";

export class SelectTimeDeviationCauseController {

    // Data
    private settings: AccountingSettingsRowDTO[];
    private employeeChilds: ISmallGenericType[] = [];
    private accountSetting: AccountingSettingsRowDTO;
    private settingTypes: SmallGenericType[] = [];
    private terms: { [index: string]: string; };
    private showStdAccount = false;

    // Properties
    private _selectedTimeDeviationCause: ITimeDeviationCauseGridDTO
    private get selectedTimeDeviationCause(): ITimeDeviationCauseGridDTO {
        return this._selectedTimeDeviationCause;
    }
    private set selectedTimeDeviationCause(cause: ITimeDeviationCauseGridDTO) {
        this._selectedTimeDeviationCause = cause;

        if (!cause.specifyChild)
            this.employeeChildId = undefined;
    }
    private readPermission: boolean = false;
    private editPermission: boolean = false;
    private isMyTime: TimeAttestMode;

    private employeeChildId: number;
    private comment: string;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private employeeService: IEmployeeService,
        private employeeId: number,
        private timeDeviationCauses: ITimeDeviationCauseGridDTO[],
        accountSetting: AccountingSettingsRowDTO,
        private deviationAccounts: AccountInternalDTO,
        private coreService: ICoreService) {

        this.loadEmployeeChilds().then(() => {
            // If employee does not have any children, remove causes where children is mandatory
            if (this.employeeChilds.length === 0)
                this.timeDeviationCauses = _.filter(this.timeDeviationCauses, t => !t.specifyChild);
        });
        this.loadTerms().then(() => {
            this.isMyTime = soeConfig.attestMode ? soeConfig.attestMode : TimeAttestMode.Time;
            this.loadReadPermissions();
            this.loadModifyPermissions();
            this.initAccountSetting();
            this.settingTypes.push(new SmallGenericType(0, this.terms["common.accountingsettings.account"]));
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.accountingsettings.account",
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }
    private loadReadPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Time_Attest_SpecifyAccountingOnDeviations);
        featureIds.push(Feature.Time_Time_AttestUser_SpecifyAccountingOnDeviations);

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            if (this.isMyTime == TimeAttestMode.Time) {
                this.readPermission = x[Feature.Time_Time_Attest_SpecifyAccountingOnDeviations];
            } else {
                this.readPermission = x[Feature.Time_Time_AttestUser_SpecifyAccountingOnDeviations];
            }
        });

    }
    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Time_Attest_SpecifyAccountingOnDeviations);
        featureIds.push(Feature.Time_Time_AttestUser_SpecifyAccountingOnDeviations);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (this.isMyTime == TimeAttestMode.Time) {
                this.editPermission = x[Feature.Time_Time_Attest_SpecifyAccountingOnDeviations];
            } else {
                this.editPermission = x[Feature.Time_Time_AttestUser_SpecifyAccountingOnDeviations];               
            }
        });
    }
    private initAccountSetting() {
        if (this.deviationAccounts != null) {
            let row = new AccountingSettingsRowDTO(0);
            this.settings = [];

            for (var i = 0; i < 5; i++) {
                row[`account${i + 1}Id`] = this.deviationAccounts[i]?.accountId ?? 0;
                row[`account${i + 1}Nr`] = this.deviationAccounts[i]?.accountNr ?? '';
                row[`account${i + 1}Name`] = this.deviationAccounts[i]?.name ?? '';
                row[`accountDim${i + 1}Nr`] = this.deviationAccounts[i]?.accountDimNr ?? 0;
            }
            this.settings.push(row);
            this.accountingChanged();
        }
    }

    // SERVICE CALLS

    private loadEmployeeChilds(): ng.IPromise<any> {
        return this.employeeService.getEmployeeChildsDict(this.employeeId, false).then(x => {
            this.employeeChilds = x;

            if (this.employeeChilds.length > 0)
                this.employeeChildId = this.employeeChilds[0].id;
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.close();
    }

    private close() {
        this.$uibModalInstance.close({ success: true, timeDeviationCauseId: this.selectedTimeDeviationCause ? this.selectedTimeDeviationCause.timeDeviationCauseId : undefined, employeeChildId: this.employeeChildId, comment: this.comment, accountSetting: this.accountSetting });
    }

    private isNoteRequired(): boolean {
        
        return (this.selectedTimeDeviationCause && this.selectedTimeDeviationCause.mandatoryNote === true)
    }
    private isNoteInvalid(): boolean {

        return (this.isNoteRequired() && (!this.comment || this.comment.length == 0))
    }

    private accountingChanged() {
        this.accountSetting = this.settings[0];
    }
}

