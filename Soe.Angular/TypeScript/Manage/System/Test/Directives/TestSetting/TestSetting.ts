import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { TestCaseSettingDTO, TestCaseSettingType } from "../../Util";
import { SoeEntityState, TermGroup } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class TestSettingController {
    private testCaseSettings: TestCaseSettingDTO[];
    private settingTypes: ISmallGenericType[] = [];
    private seleniumBrowserTypes: ISmallGenericType[];
    private capabilities: any = {};

    constructor(private coreService: ICoreService,
        private $q: ng.IQService) {
    }

    private onInit() {

    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadSettingTypes(),
            this.loadSeleniumBrowser(),
        ])
    }

    private setSettings() {
        _.forEach(this.testCaseSettings, (setting) => {
            console.log(setting)
            if (setting.state == SoeEntityState.Active && setting.type == TestCaseSettingType.Capabilities) {
                this.setCapabilities(setting)
            }
            else if (setting.state == SoeEntityState.Active) {
                switch (setting.type) {
                    case TestCaseSettingType.SeleniumBrowser:
                        setting['icon'] = "fal fa-globe";
                        setting['name'] = this.settingTypes.find(i => setting.type === i.id).name;
                        setting['value'] = this.seleniumBrowserTypes.find(i => setting.intValue === i.id).name;
                        break;
                    default: //string
                        setting['icon'] = "fal fa-font";
                        setting['name'] = this.settingTypes.find(i => setting.type === i.id).name;
                        setting['value'] = setting.stringValue ? setting.stringValue : "";
                        break;
                }
            }
        });
    }
    

    private setCapabilities(setting: any) {
        if (setting && setting.stringValue) {
            this.capabilities = JSON.parse(setting.stringValue)
        }
    }

    //lookups
    private loadSeleniumBrowser(): ng.IPromise<any> { //
        return this.coreService.getTermGroupContent(TermGroup.SeleniumBrowser, true, false, true).then(x => {
            this.seleniumBrowserTypes = x;
        });
    }

    private loadSettingTypes() {
        return this.coreService.getTermGroupContent(TermGroup.TestCaseSettingType, true, false, true).then(x => {
            this.settingTypes = x;
        });
    }

}


export class TestSettingDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("../../Directives/TestSetting/TestSetting.html"),
            scope: {
                testCaseSettings: "=",
            },
            restrict: 'E',
            replace: true,
            controller: TestSettingController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}