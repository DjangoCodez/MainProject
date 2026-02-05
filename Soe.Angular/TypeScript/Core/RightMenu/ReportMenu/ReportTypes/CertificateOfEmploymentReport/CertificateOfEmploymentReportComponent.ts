import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { DateSelectionDTO, EmployeeSelectionDTO, BoolSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IDateSelectionDTO, IEmployeeSelectionDTO, IBoolSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../Util/Constants";
import { ICoreService } from "../../../../Services/CoreService";
import { SettingMainType, CompanySettingType, Feature } from "../../../../../Util/CommonEnumerations";

export class CertificateOfEmploymentReport {

    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: CertificateOfEmploymentReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/CertificateOfEmploymentReport/CertificateOfEmploymentReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "certificateOfEmploymentReport";

    //binding fields
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private date: Date;
    private hasNoSettingToSendToArbetsgivarIntygNu: boolean;
    private hasPermissionToSendToArbetsgivarIntygNu: boolean;

    //user selections
    private userSelectionInputDate: DateSelectionDTO;
    private userSelectionInputEmployee: EmployeeSelectionDTO;
    private userSelectionInputSendToArbetsgivarIntygNu: BoolSelectionDTO;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private $scope: ng.IScope) {
        this.date = new Date();
        this.hasNoSettingToSendToArbetsgivarIntygNu = false
        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.userSelectionInputDate = this.userSelection.getDateSelection();
            this.userSelectionInputEmployee = this.userSelection.getEmployeeSelection();

            this.date = this.userSelectionInputDate ? this.userSelectionInputDate.date : new Date();
        });
    }

    public $onInit() {
        this.loadModifyPermissions();
        this.hasNoSettingToSendToArbetsgivarIntygNu = false
        this.hasSettingToSendToArbetsgivarIntygNu();
    }

    public hasSettingToSendToArbetsgivarIntygNu() {
        return this.coreService.getStringSetting(SettingMainType.Company, CompanySettingType.PayrollArbetsgivarintygnuApiNyckel).then((key: string) => {
            if (key && key.length > 1) {
                this.loadModifyPermissions().then((permission: boolean) => {
                  if (this.hasPermissionToSendToArbetsgivarIntygNu)
                        this.hasNoSettingToSendToArbetsgivarIntygNu = false;
                    else
                        this.hasNoSettingToSendToArbetsgivarIntygNu = true;
                });
            }
            else {
                return this.hasNoSettingToSendToArbetsgivarIntygNu = true;
            }
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];
        features.push(Feature.Time_Employee_SendToArbetsgivarIntygNU);
        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.hasPermissionToSendToArbetsgivarIntygNu = x[Feature.Time_Employee_SendToArbetsgivarIntygNU];

            if (!this.hasPermissionToSendToArbetsgivarIntygNu)
                this.hasNoSettingToSendToArbetsgivarIntygNu = true;
        });
    }

    public onDateSelectionUpdated(selection: IDateSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE, selection);

        this.date = selection.date;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    public onBoolSelectionInputSendToArbetsgivarIntygNu(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_SENDTOARBETSGIVARINTYGNU, selection);
    }
}
