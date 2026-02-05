import { ICoreService } from "../../../Core/Services/CoreService";
import { EmailTemplateType, SettingMainType, TermGroup, UserSettingType } from "../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class SelectEmailController {

    private emailTemplates: any[];
    private languages: any[];
    private selectedTemplateId: any;
    private selectedLanguageId: any;
    private selectedReportId: any;
    private mergePdfs: boolean = false;
    //@ngInject
    constructor(
        private $uibModalInstance,
        private coreService: ICoreService,
        private defaultEmail: number,
        private defaultEmailTemplateId: number,
        private recipients: any[],
        private attachments: any[],
        private attachmentsSelected: boolean,
        private checklists: any[],
        private types: any,
        private grid?: boolean,
        private type?: number,
        private showReportSelection?: boolean,
        private reports?: any[],
        private defaultReportTemplateId?: number,
        private langId?: number,
    ) {
        this.loadLanguages();
        this.loadUserSettings();
        this.setup();
    }

    private setup() {
        this.selectedTemplateId = this.defaultEmailTemplateId;
        this.selectedReportId = this.defaultReportTemplateId;
        this.loadTemplates();
        _.forEach(this.recipients, (r) => {
                if (r.id === this.defaultEmail)
                    r['isSelected'] = true;
        });

        _.forEach(this.attachments, (r) => {
            r['isSelected'] = r.includeWhenDistributed;
        });

        /*if (this.attachmentsSelected) {
            _.forEach(this.attachments, (r) => {
                r['isSelected'] = true;
            });
        }*/

        _.forEach(this.checklists, (r) => {
            r['isSelected'] = r.addAttachementsToEInvoice;
        });
    }

    private loadLanguages() {
        this.languages = [];

        return this.coreService.getTermGroupContent(TermGroup.Language, false, false).then(data => {
            this.languages = data;
            this.selectedLanguageId = this.langId ? this.langId : CoreUtility.sysCountryId;
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.BillingMergePdfs];
        
        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.mergePdfs = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingMergePdfs, false);
        });
    }

    private loadTemplates() {
        this.emailTemplates = [];
        if (this.type !== null && this.type !== undefined) {
            return this.coreService.getEmailTemplatesByType(this.type).then((x) => {
                _.forEach(x, (y) => {
                    switch (y.type) {
                        case EmailTemplateType.Invoice:
                            y['typename'] = this.types["billing.invoices.invoice"];
                            break;
                        case EmailTemplateType.Reminder:
                            y['typename'] = this.types["common.customer.invoices.reminder"];
                            break;
                        case EmailTemplateType.PurchaseOrder:
                            y['typename'] = this.types["billing.purchase.list.purchase"];
                    }
                });
                this.emailTemplates = x;

                // Set default
                if (!this.selectedTemplateId)
                    this.selectedTemplateId = this.emailTemplates[0].emailTemplateId;
            });}
        else {
            return this.coreService.getEmailTemplates().then((x) => {
                _.forEach(x, (y) => {
                    switch (y.type) {
                        case EmailTemplateType.Invoice:
                            y['typename'] = this.types["billing.invoices.invoice"];
                            break;
                        case EmailTemplateType.Reminder:
                            y['typename'] = this.types["common.customer.invoices.reminder"];
                            break;
                        case EmailTemplateType.PurchaseOrder:
                            y['typename'] = this.types["billing.purchase.list.purchase"];
                    }
                });
                this.emailTemplates = x;

                // Set default
                if (!this.selectedTemplateId)
                    this.selectedTemplateId = this.emailTemplates[0].emailTemplateId;
            });
        }
    }

    private saveUserSettings() {
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.BillingMergePdfs, this.mergePdfs);
    }

    buttonSendDisabled() {
        return this.showReportSelection ? (!this.selectedReportId || !this.selectedTemplateId) : !this.selectedTemplateId;
    }

    buttonCancelClick() {
        this.close(false);
    }

    buttonSendClick() {
        this.close(true);
    }

    close(send: boolean) {
        if (send && this.selectedTemplateId) {
            this.saveUserSettings();
            this.$uibModalInstance.close({
                emailTemplateId: this.selectedTemplateId,
                recipients: _.filter(this.recipients, r => r['isSelected']),
                attachments: _.filter(this.attachments, r => r['isSelected']),
                checklists: _.filter(this.checklists, r => r['isSelected']),
                mergePdfs: this.mergePdfs, 
                reportId: this.selectedReportId,
                languageId: this.selectedLanguageId,
            });
        }
        else {
            this.$uibModalInstance.dismiss('cancel');
        }
    }
}