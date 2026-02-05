import { ScheduledJobSettingDTO } from "../../../../../Common/Models/ScheduledJobDTOs";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SettingDataType, TermGroup } from "../../../../../Util/CommonEnumerations";
import { SettingDialogController } from "./SettingDialogController";

export class SettingsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Manage/Registry/ScheduledJobs/Directives/Settings/Settings.html'),
            scope: {
                readOnly: '=',
                settings: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: SettingsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class SettingsController {

    // Init parameters
    private readOnly: boolean;
    private settings: ScheduledJobSettingDTO[];
    private onChange: Function;

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private types: ISmallGenericType[];

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private coreService: ICoreService) {

        this.$q.all([
            this.loadTerms(),
            this.loadTypes()
        ]);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.yes",
            "core.no"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ScheduledJobSettingType, true, true, true).then(x => {
            this.types = x;
        });
    }

    // EVENTS

    private editSetting(setting: ScheduledJobSettingDTO) {
        if (this.readOnly)
            return;

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/Registry/ScheduledJobs/Directives/Settings/SettingDialog.html"),
            controller: SettingDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                types: () => { return this.types },
                setting: () => { return setting },
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result && result.setting) {
                if (!result.setting.scheduledJobSettingId || result.setting.scheduledJobSettingId === 0) {
                    setting = new ScheduledJobSettingDTO();
                    this.setValues(result.setting, setting);
                    this.settings.push(setting);
                } else {
                    let existing = _.find(this.settings, s => s.scheduledJobSettingId === result.setting.scheduledJobSettingId);
                    if (existing)
                        this.setValues(result.setting, existing);
                }

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private setValues(settingFrom: ScheduledJobSettingDTO, settingTo: ScheduledJobSettingDTO) {
        settingTo.boolData = undefined;
        settingTo.dateData = undefined;
        settingTo.decimalData = undefined;
        settingTo.intData = undefined;
        settingTo.strData = undefined;
        settingTo.timeData = undefined;
        settingTo.options = undefined;

        switch (settingFrom.dataType) {
            case SettingDataType.Boolean:
                settingTo.boolData = settingFrom.boolData;
                break;
            case SettingDataType.Date:
                settingTo.dateData = settingFrom.dateData;
                break;
            case SettingDataType.Decimal:
                settingTo.decimalData = settingFrom.decimalData;
                break;
            case SettingDataType.Integer:
                settingTo.intData = settingFrom.intData;
                break;
            case SettingDataType.String:
                settingTo.strData = settingFrom.strData;
                break;
            case SettingDataType.Time:
                settingTo.timeData = settingFrom.timeData;
                break;
        }

        settingTo.type = settingFrom.type;
        settingTo.dataType = settingFrom.dataType;
        settingTo.name = settingFrom.name;
        settingTo.options = settingFrom.options;
        settingTo.setValue(this.terms["core.yes"], this.terms["core.no"]);
    }

    private deleteSetting(setting: ScheduledJobSettingDTO) {
        _.pull(this.settings, setting);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS


}
