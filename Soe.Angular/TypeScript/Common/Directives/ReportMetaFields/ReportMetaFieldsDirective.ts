import _ from "lodash";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IReportDTO, IReportTemplateSettingDTO } from "../../../Scripts/TypeLite.Net4";
import { SoeReportTemplateType, SoeReportSettingFieldMetaData } from "../../../Util/CommonEnumerations";
import { ReportTemplateDTO } from "../../Models/ReportDTOs";

export class ReportMetaFieldsFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: urlHelperService.getCommonDirectiveUrl('ReportMetaFields', 'ReportMetaFields.html'),
            scope: {
                isSysReportTemplate: '=',
                dirtyHandler:'=',
                report: '=?',
                reportTemplate: '=?',
            },
            controller: ReportMetaFieldsController,
            controllerAs: 'ctrl',
            bindToController: true,
        };
    }
}

interface ReportMetaFields {
    _includeAllHistoricalData: boolean;
    _includeBudget: boolean;
    _showRowsByAccount: boolean;
    _nrOfDecimals: number;
    _noOfYearsBackinPreviousYear: number;
    _detailedInformation: boolean;
    _showInAccountingReports: boolean;
}

type FieldSettings = { [key: number]: IReportTemplateSettingDTO[] };

export class ReportMetaFieldsController {
    private isSysReportTemplate: boolean = false;
    private dirtyHandler: IDirtyHandler;
    private report: IReportDTO = <IReportDTO>{};
    private reportTemplate: ReportTemplateDTO = <ReportTemplateDTO>{};

    private settings: FieldSettings = {};

    get fieldSettigs() {
        if (this.isSysReportTemplate)
            return this.reportTemplate?.reportTemplateSettings ?? [];
        else 
            return this.report?.reportTemplateSettings ?? [];
    }

    set fieldSettigs(value: IReportTemplateSettingDTO[]) {
        if (this.isSysReportTemplate)
            this.reportTemplate.reportTemplateSettings = value;
    }

    get sysReportTemplateId(): number {
        if (this.isSysReportTemplate)
            return this.reportTemplate?.reportTemplateId ?? 0;

        return this.report?.reportTemplateId ?? 0;
    }

    private visibleOptionalFields: { [key: string]: boolean } = {
        "exportFileType": false,
        "includeAllHistoricalData": false,
        "includeBudget": false,
        "noOfYearsBackinPreviousYear": false,
        "getDetailedInformation": false,
        "showInAccountingReports": false
    };

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private translationService: ITranslationService) {
        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.report, (newVal, oldVal) => {
            this.evaluateOptionalProperties();
            this.setSettings();
            this.setSettingValuesForReport();
        });
        this.$scope.$watch(() => this.reportTemplate, (newVal, oldVal) => {
            this.evaluateOptionalProperties();
            this.setSettings();
            this.setDefaultValuesForSysReportTemplate();
        });
    }

    private setSettings(): void {
        const settingDic = _.chain(this.fieldSettigs).groupBy("settingField").map((s, f) => ({ [f]: s })).value();

        for (let setting of settingDic) {
            this.settings = {
                ...this.settings,
                ...(<FieldSettings>setting)
            };
        }        
    }

    private setDefaultValuesForSysReportTemplate(): void {
        if (this.isSysReportTemplate) {
            this.report.includeAllHistoricalData = this.settings[1]?.length ? this.settings[1].find(x => x.settingType == SoeReportSettingFieldMetaData.DefaultValue)?.settingValue === "true" : false;
            this.report.includeBudget = this.settings[2]?.length ? this.settings[2].find(x => x.settingType == SoeReportSettingFieldMetaData.DefaultValue)?.settingValue === "true" : false;
            this.report.noOfYearsBackinPreviousYear = this.settings[3]?.length ? parseInt(this.settings[3].find(x => x.settingType == SoeReportSettingFieldMetaData.DefaultValue)?.settingValue ?? null) : 0;
            this.report.nrOfDecimals = this.settings[4]?.length ? parseInt(this.settings[4].find(x => x.settingType == SoeReportSettingFieldMetaData.DefaultValue)?.settingValue ?? null) : 0;
            this.report.showRowsByAccount = this.settings[5]?.length ? this.settings[5].find(x => x.settingType == SoeReportSettingFieldMetaData.DefaultValue)?.settingValue === "true" : false;
            this.report.detailedInformation = this.settings[6]?.length ? this.settings[6].find(x => x.settingType == SoeReportSettingFieldMetaData.DefaultValue)?.settingValue === "true" : false;
            this.report.showInAccountingReports = this.settings[7]?.length ? this.settings[7].find(x => x.settingType == SoeReportSettingFieldMetaData.DefaultValue)?.settingValue === "true" : false;

            this.setSettings();        }
    }

    private setSettingValuesForReport(): void {
        if (!this.isSysReportTemplate) {
            this.report.includeAllHistoricalData = <boolean>this.getSettingValue(1, this.report.includeAllHistoricalData);
            this.report.includeBudget = <boolean>this.getSettingValue(2, this.report.includeBudget);
            this.report.noOfYearsBackinPreviousYear = <number | null>this.getSettingValue(3, this.report.noOfYearsBackinPreviousYear);
            this.report.nrOfDecimals = <number | null>this.getSettingValue(4, this.report.nrOfDecimals);
            this.report.showRowsByAccount = <boolean>this.getSettingValue(5, this.report.showRowsByAccount);
            this.report.detailedInformation = <boolean>this.getSettingValue(6, this.report.detailedInformation);
            this.report.showInAccountingReports = <boolean>this.getSettingValue(7, this.report.showInAccountingReports);
        }
    }

    private getSettingValue(fieldId: number, settingValue: boolean | number | string | null): boolean | number | string | null {
        if (this.settings[fieldId]?.length) {
            const forceDefault = this.settings[fieldId].find(x => x.settingType == SoeReportSettingFieldMetaData.ForceDefaultValue)?.settingValue ?? null;
            const defaultVal = this.settings[fieldId].find(x => x.settingType == SoeReportSettingFieldMetaData.DefaultValue)?.settingValue ?? null;

            if (forceDefault === 'true') {
                settingValue = forceDefault === "true" ? true : forceDefault === "false" ? false : isNaN(Number(forceDefault)) ? forceDefault : Number(forceDefault);
            } else if (!settingValue) {
                settingValue = defaultVal === "true" ? true : defaultVal === "false" ? false : isNaN(Number(defaultVal)) ? defaultVal : Number(defaultVal);
            }                
        }

        return settingValue;
    }

    private evaluateOptionalProperties() {
        this.visibleOptionalFields["includeAllHistoricalData"] = this.isConsideredResultReport();
        this.visibleOptionalFields["includeBudget"] = this.isConsideredResultReport();
        this.visibleOptionalFields["noOfYearsBackinPreviousYear"] = this.isConsideredResultReport();
        this.visibleOptionalFields["nrOfDecimals"] = this.isConsideredAccountingReport();
        this.visibleOptionalFields["showRowsByAccount"] = this.isConsideredAccountingReport();
        this.visibleOptionalFields["getDetailedInformation"] = this.isConsideredDetailedInformationReport();
        this.visibleOptionalFields["showInAccountingReports"] = this.isConsideredAccountingReport();
        this.visibleOptionalFields["connectReportGroups"] = this.isConsideredConnectReportGroups();
    }

    private isConsideredResultReport() {
        return this.matchAnySysReportTemplateType(
            SoeReportTemplateType.ResultReport,
            SoeReportTemplateType.ResultReportV2);
    }

    private isConsideredConnectReportGroups() {
        var test = this.matchAnySysReportTemplateType(
            SoeReportTemplateType.ResultReportV2);
        return test;
    }

    private isConsideredDetailedInformationReport() {
        return this.matchAnySysReportTemplateType(
            SoeReportTemplateType.ResultReport,
            SoeReportTemplateType.BalanceReport,
            SoeReportTemplateType.PayrollAccountingReport,
            SoeReportTemplateType.PayrollVacationAccountingReport,
            SoeReportTemplateType.TimeEmployeeSchedule,
            SoeReportTemplateType.ResultReportV2,
            SoeReportTemplateType.BillingInvoice,
            SoeReportTemplateType.BillingInvoiceInterest,
            SoeReportTemplateType.BillingInvoiceReminder,
            SoeReportTemplateType.TimeMonthlyReport,
            SoeReportTemplateType.VerticalTimeTrackerAnalysis);
    }

    private isConsideredAccountingReport() {
        return this.matchAnySysReportTemplateType(
            SoeReportTemplateType.ResultReport,
            SoeReportTemplateType.BalanceReport);
    }

    private matchAnySysReportTemplateType(...types: SoeReportTemplateType[]): boolean {
        if (this.isSysReportTemplate)
            return this.reportTemplate && this.reportTemplate.sysReportTemplateTypeId
                && types.some(v => v === <SoeReportTemplateType>this.reportTemplate.sysReportTemplateTypeId);

        return this.report && this.report.sysReportTemplateTypeId
            && types.some(v => v === <SoeReportTemplateType>this.report.sysReportTemplateTypeId);
    }

    private settingsChanged(value) {
        if (this.isSysReportTemplate) {
            this.reportTemplate.reportTemplateSettings = value;
            this.dirtyHandler?.setDirty();
            this.setDefaultValuesForSysReportTemplate();
        }
    }
}