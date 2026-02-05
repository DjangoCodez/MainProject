import { ICoreService } from "../../Core/Services/CoreService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { StateAnalysisController } from "../StateAnalysis/StateAnalysisController";
import { HtmlUtility } from "../../Util/HtmlUtility";
import { Feature, LicenseSettingType, TermGroup, TermGroup_BrandingCompanies } from "../../Util/CommonEnumerations";
import { SettingsUtility } from "../../Util/SettingsUtility";
import { SmallGenericType } from "../Models/SmallGenericType";

export class StartController {

    progressBusy = true;
    settingsLoaded = false;
    brandingCompany: TermGroup_BrandingCompanies;
    brands: SmallGenericType[];
    brandName: string;
    assemblyVersion: string;
    assemblyDate: string;
    modules = [];
    stateAnalysisModule = null;
    baseUrl: string;

    //@ngInject
    constructor(private $window: ng.IWindowService,
        private $uibModal,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService) {

        this.baseUrl = urlHelperService.getGlobalUrl('');

        this.loadLicenceSettings().then(() => {
            this.loadBrandingCompanies();
        });
        this.loadAssemblyInfo();
        this.loadModules();
    }

    // License settings
    private loadLicenceSettings(): ng.IPromise<any> {
        return this.coreService.getLicenseSettings([LicenseSettingType.BrandingCompany]).then(x => {
            this.brandingCompany = SettingsUtility.getIntLicenseSetting(x, LicenseSettingType.BrandingCompany, TermGroup_BrandingCompanies.SoftOne);
        })
    }

    // Branding
    private loadBrandingCompanies(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.BrandingCompanies, false, false).then(x => {
            this.brands = x;
            this.brandName = this.brands.find(b => b.id == this.brandingCompany).name;
            this.settingsLoaded = true;
        });
    }

    // Assembly info
    private loadAssemblyInfo(): ng.IPromise<any> {
        return this.coreService.getAssemblyVersion().then(version => {
            const versionLabel = this.translationService.translateInstant("core.version");

            let versionArr = version.split('(',);
            if (versionArr.length === 2) {
                this.assemblyVersion = versionArr[0].trim();
                this.assemblyDate = `${versionLabel}: ${versionArr[1].replace(")", "").trim()}`;
            } else {
                this.assemblyVersion = version;
            }

        });
    }

    // Modules
    private loadModules() {
        let featureIds: number[] = [];
        featureIds.push(Feature.Billing);
        featureIds.push(Feature.Economy);
        featureIds.push(Feature.Time);
        featureIds.push(Feature.ClientManagement);
        featureIds.push(Feature.Manage);
        featureIds.push(Feature.Common_HideStateAnalysis);

        this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            if (x[Feature.Billing]) {
                this.addModule(1, "billing", "fa-chart-line", "common.start.module.billing", "billing/?c=" + soeConfig.actorCompanyId);
            }
            if (x[Feature.Economy]) {
                this.addModule(2, "economy", "fa-calculator", "common.start.module.economy", "economy/?c=" + soeConfig.actorCompanyId);
            }
            if (x[Feature.Time]) {
                this.addModule(3, "time", "fa-user-friends", "common.start.module.time", "time/?c=" + soeConfig.actorCompanyId);
            }
            if (x[Feature.ClientManagement]) {
                this.addModule(4, "clientmanagement", "fa-buildings", "common.start.module.clientmanagement", "clientmanagement/clients/?c=" + soeConfig.actorCompanyId);
            }
            if (x[Feature.Manage]) {
                this.addModule(5, "manage", "fa-cog", "common.start.module.manage", "manage/?c=" + soeConfig.actorCompanyId);
            }
            if (!x[Feature.Common_HideStateAnalysis]) {
                this.translationService.translate("common.start.module.stateanalysis").then((term) => {
                    this.stateAnalysisModule = {
                        name: "stateanalysis",
                        icon: "fa-chart-pie",
                        label: term,
                    }
                });
            }

            this.progressBusy = false;
        });
    }

    private addModule(sort: number, name: string, icon: string, labelKey: string, url: string) {
        this.translationService.translate(labelKey).then((term) => {
            const mod = {
                sort: sort,
                name: name,
                icon: icon,
                label: term,
                url: url
            }
            this.modules.push(mod);
        });
    }

    private onLogoSelect() {
        HtmlUtility.openInNewTab(this.$window, "http://www.softone.se");
    }

    private onModuleSelect(mod: any) {
        this.progressBusy = true;
        mod.selected = true;
        HtmlUtility.openInSameTab(this.$window, mod.url);
    }

    showStateAnalysis(): any {
        this.$uibModal.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("StateAnalysis", "stateanalysis.html"),
            controller: StateAnalysisController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
            }
        });
    }
}