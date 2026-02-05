import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ISoeGridOptions } from "../../../Util/SoeGridOptions";
import { CoreUtility } from "../../../Util/CoreUtility";
import { TermGroup, SoeReportTemplateType, SoeModule, UserSettingType } from "../../../Util/CommonEnumerations";
import { ReportViewDTO } from "../../Models/ReportDTOs";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class SelectReportAndAttachmentsController {
    private readonly soeGridOptions: ISoeGridOptions;
    private languages: any[];
    private languageId: number;
    private isCopy: boolean = true;
    private isReminder: boolean = false;
    private loadingReports: boolean = false;
    private mergePdfs: boolean = false;
    private reports: ReportViewDTO[] = [];
    private caption: string;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private reportTypes: SoeReportTemplateType[],
        private showCopy: boolean,
        private copyValue: boolean,
        private showEmail: boolean,
        private langId: number,
        private showLangSelection: boolean = true,
        private checklists: any[] = [],
        private attachments: any[] = [],
        private attachmentsSelected: boolean = false,
        private showReportSelection: boolean = false) {

        this.loadLanguages();
        this.loadUserSettings();

        if (this.showReportSelection) 
            this.loadReports();

        if (this.showCopy)
            this.isCopy = this.copyValue;

        this.setCaption();

        _.forEach(this.attachments, (r) => {
            r['isSelected'] = r.includeWhenDistributed;
        });
    }

    private loadLanguages() {
        this.languages = [];

        return this.coreService.getTermGroupContent(TermGroup.Language, true, false).then(data => {
            this.languages = data;
            if (this.langId !== -1) {
                this.languageId = this.langId ? this.langId : CoreUtility.sysCountryId;
            }
        });
    }

    private loadReports() {
        this.loadingReports = true;
        this.reports = [];

        return this.reportService.getReportsForType(this.reportTypes, true, false, null).then(x => {
            this.reports = x;
            this.loadingReports = false;
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.BillingMergePdfs];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.mergePdfs = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingMergePdfs, false);
        });
    }

    private setCaption() {
        if (this.showReportSelection) {
            this.caption = this.translationService.translateInstant("common.selectreport");
        }
        else {
            this.caption = this.translationService.translateInstant("common.selectattachments");
        }
    }

    printClick(report: ReportViewDTO) {
        this.close(report, false);
    }

    emailClick(report: ReportViewDTO) {
        this.close(report, true);
    }

    buttonOkClick() {
        this.close(null, false);
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    close(report: ReportViewDTO, email: boolean) {
        this.$uibModalInstance.close({
            reportId: report?.reportId ?? 0,
            reportType: report?.sysReportTemplateTypeId ?? 0,
            languageId: this.languageId,
            createCopy: this.isCopy,
            email: email,
            reminder: this.isReminder,
            attachmentIds: _.filter(this.attachments, r => r['isSelected']).map(i => i.imageId),
            checklistIds: _.filter(this.checklists, r => r['isSelected']).map(i => i.checklistHeadRecordId),
            mergePdfs: this.mergePdfs,
        });
    }
}