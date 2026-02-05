import { IReportTemplateSettingDTO } from "../../../Scripts/TypeLite.Net4";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditController as SettingDialogController } from "./SettingDialog/EditController";
import { Constants } from "../../../Util/Constants";
import { SoeReportSettingFieldMetaData } from "../../../Util/CommonEnumerations";

export class ReportFieldSettingFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            restrict: 'E',
            transclude: true,
            templateUrl: urlHelperService.getCommonDirectiveUrl('ReportFieldSetting', 'ReportFieldSetting.html'),
            scope: {
                fieldId: '=',
                sysReportTemplateId: '=',
                isSysReportTemplatePage: '=',
                hide: '=',
                fieldType: '=',
                labelKey: '@',
                settings: '=?',
                onSettingChanged: '&',
            },
            controller: ReportFieldSettingController,
            controllerAs: 'ctrl',
            bindToController: true,
            multiElement: true,
        };
    }
}

export class ReportFieldSettingController {
    private fieldId: number;
    private sysReportTemplateId: number;
    private isSysReportTemplatePage: boolean;
    private hide: boolean = false;
    private fieldType: number;
    private labelKey: string = '';
    private settings: IReportTemplateSettingDTO[] = [];
    private onSettingChanged: Function;

    get setDisabled() {
        return this.isSysReportTemplatePage && this.isVisble
            ? { 'pointer-events': 'none', 'opacity': '1', ...this.leftPadding }
            : (this.isSysReportTemplatePage || this.isSysDefaultValForced
                ? { 'pointer-events': 'none', 'opacity': '0.5', ...this.leftPadding }
                : {...this.leftPadding });
    }

    get leftPadding() {
        return this.showAsterisk
            ? { 'padding-left': '0px' }
            : { 'padding-left': '12px' };
    }

    get isVisble() {
        const val = this.settings?.find(x => x.settingType === SoeReportSettingFieldMetaData.IsVisible && x.settingField == this.fieldId)?.settingValue ?? 'false';
        return (val === 'true');
    }

    get reportTempleteId(): number {
        return this.sysReportTemplateId ?? 0;
    }

    get hideField(): boolean {
        return !!(!this.isSysReportTemplatePage && this.hide);
    }

    get showButton(): boolean {
        return !!this.isSysReportTemplatePage;
    }

    get settingValues() {
        return this.settings?.map(s => {
            return { type: s.settingType, value: s.settingValue.toString() };
        }) ?? [];
    }

    get isSysDefaultValForced(): boolean {
        return !!(this.settings?.find(x => x.settingType === SoeReportSettingFieldMetaData.ForceDefaultValue && x.settingField == this.fieldId)?.settingValue === 'true');
    }

    get showAsterisk(): boolean {
        return this.isSysDefaultValForced && !!this.isSysReportTemplatePage;
    }

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal: ng.ui.bootstrap.IModalService,
        private urlHelperService: IUrlHelperService) {
    }

    public showSettingDialog() {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getCommonDirectiveUrl("ReportFieldSetting/SettingDialog/Views", "edit.html"),
            controller: SettingDialogController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                fieldType: this.fieldType,
                modal: modal,
                feature: soeConfig.feature,
                labelKey: this.labelKey,
                settingValues: this.settingValues,
            });
        });

        modal.result.then(result => {
            this.updateSettings(result);
        }, error => {});
    }

    updateSettings(result) {
        let updated = false;
        for (let setting of result) {
            if (this.settings && this.settings.find(x => x.settingField === this.fieldId && x.settingType === setting["type"])) {
                this.settings = this.settings.map(x => {
                    if (x.settingField === this.fieldId && x.settingType === setting["type"]) {
                        x.settingValue = String(setting["value"]);
                        x.settingType = setting["type"];
                        x.isModified = true;

                        updated = true;
                    }
                    return x;
                });
            } else {
                if (!this.settings) this.settings = [];

                this.settings.push(<IReportTemplateSettingDTO>{
                    reportTemplateSettingId: 0,
                    reportTemplateId: this.reportTempleteId,
                    settingField: this.fieldId,
                    settingType: setting["type"],
                    settingValue: String(setting["value"]),
                    isModified: true,
                });
                updated = true;
            }
        }
        if (updated)
            this.onSettingChanged({ value: this.settings });
    }
}