import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SettingMainType, UserSettingType } from "../../../Util/CommonEnumerations";

export class AccordionSettingsController {
    private width = 7
    private terms: { [index: string]: string; };
    private showSlider = false;
    private sliderDescription: string;
    //@ngInject
    constructor(private $uibModalInstance,
        private coreService: ICoreService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private accordionList: any[],
        private userSettingType: number,
        private userSliderSettingType: number) {


    }

    private $onInit() {
        this.loadTerms().then(() => {
            if (this.userSliderSettingType > 0) {
                this.showSlider = true;
            }

            this.loadSettings();
        })
    }

    private loadSettings() {
        var settingTypeIds: number[] = [];
        settingTypeIds.push(this.userSettingType);
        if (this.showSlider) {
            settingTypeIds.push(this.userSliderSettingType)
        }

        return this.coreService.getUserSettings(settingTypeIds, false).then(x => {

            if (this.showSlider && x[this.userSliderSettingType]) {
                this.width = x[this.userSliderSettingType]
            }
            var expanderSettings = x[this.userSettingType];
            if (expanderSettings) {
                var settings = expanderSettings.split(";");

                _.forEach(this.accordionList, (r) => {
                    r['isSelected'] = _.includes(settings, r.name);
                });
            }
        });
    }

    buttonOkClick() {
        this.save().then(() => { this.$uibModalInstance.close('ok') })
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.accordionslidersettinginfo",
            "common.accordionslidersettinginfoattest"
        ]
        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;

            if (this.userSliderSettingType) {
                switch (this.userSliderSettingType) {
                    case UserSettingType.BillingSupplierInvoiceSlider:
                        this.sliderDescription = this.terms["common.accordionslidersettinginfo"]
                        break;
                    case UserSettingType.BillingSupplierAttestSlider:
                        this.sliderDescription = this.terms["common.accordionslidersettinginfoattest"]
                        break;
                }
            }
        })
    }

    private save(): ng.IPromise<any> {
        var saveResolves: Array<ng.IPromise<any>> = [];

        //Slider
        if (this.showSlider && this.width > 0 && this.width <= 12) {
            saveResolves.push(this.coreService.saveIntSetting(SettingMainType.User, this.userSliderSettingType, this.width))
        }

        //Expander
        var expanderSettingString: string = "";
        _.forEach(this.accordionList, (r) => {
            if (r['isSelected']) {
                expanderSettingString += r.name + ";";
            }
        });
        saveResolves.push(this.coreService.saveStringSetting(SettingMainType.User, this.userSettingType, expanderSettingString))

        return this.$q.all(saveResolves)
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}