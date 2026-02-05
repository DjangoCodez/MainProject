import { TestCaseSettingDTO, TestCaseSettingTypeDTO } from "../../../Test/Util";
import { ICoreService } from "../../../../../Core/Services/CoreService";

export class TestGroupSettingDialogController {

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q,
        private setting: TestCaseSettingDTO,
        private settings: TestCaseSettingDTO[],
        private settingTypes: TestCaseSettingTypeDTO[],
        private coreService: ICoreService,
        private testCaseGroupId: number) {

        if (!setting) {
            this.setting = new TestCaseSettingDTO(null, null);
        }
        else {
            this.setting = setting;
        }

        this.load();
    }

    private load() {
        //this.filterSettingTypes(this.settings, this.settingTypes);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.setting.type = this.setting?.settingType?.type
        this.$uibModalInstance.close({ setting: this.setting });
    }

    private filterSettingTypes(settings: any, settingTypes: any) {
        var types = settings.map(a => {
            if (a.state === 0) {
                //a.type
            }
        });
        this.settingTypes = settingTypes.filter(x => !types.includes(x.id) || x.id === this.setting.type);
    }

    //private loadSeleniumBrowser(): ng.IPromise<any> {
    //    return this.coreService.getTermGroupContent(TermGroup.SeleniumBrowser, true, false, true).then(x => {
    //        this.seleniumBrowserTypes = x;
    //    });
    //}
}
