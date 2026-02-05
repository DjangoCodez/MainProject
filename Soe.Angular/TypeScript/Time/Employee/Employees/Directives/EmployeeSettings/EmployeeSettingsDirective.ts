import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { SettingDataType, SoeEntityState, TermGroup_EmployeeSettingType } from "../../../../../Util/CommonEnumerations";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { EmployeeSettingDTO, EmployeeSettingTypeDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { EmployeeSettingDialogController } from "./EmployeeSettingDialogController";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { EmployeeService } from "../../../EmployeeService";

export class EmployeeSettingsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmployeeSettings/Views/EmployeeSettings.html'),
            scope: {
                labelKey: '@',
                settings: '=',
                area: '=',
                readOnly: '=',
                onChange: '&',
                hideValue: '=',
                infoHeader: '@'
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeSettingsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeSettingsController {
    // Inputs
    private labelKey: string;
    private settings: EmployeeSettingDTO[];
    private area: TermGroup_EmployeeSettingType;
    private readOnly: boolean;
    private onChange: Function;
    private hideValue: boolean = false;
    private infoHeader: string = '';
    
    // Terms
    private terms: { [index: string]: string; };
    private label: string;

    // Data
    private allTypes: EmployeeSettingTypeDTO[] = [];
    private groupTypes: EmployeeSettingTypeDTO[] = [];
    private types: EmployeeSettingTypeDTO[] = [];
    private filteredSettings: EmployeeSettingDTO[];
    private selectedSetting: EmployeeSettingDTO;
    private tmpEmployeeSettingIdCounter: number = 0;

    // Flags
    private showAllGenerations: boolean = false;
    private showHeader: boolean = false;
    private headerText: string = '';
    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private employeeService: EmployeeService) {
    }

    $onInit() {
        if (!this.settings)
            this.settings = [];
        this.$q.all([
            this.loadTerms(),
            this.loadAvailableSettingTypes()
        ]).then(() => {
            this.setNamesOnAll();
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.settings, (newVal, oldVal) => {
            this.setFilteredSettings();
            this.setNamesOnAll();
            this.selectedSetting = this.settings && this.settings.length > 0 ? _.orderBy(this.settings, ['sortableDate'], ['desc'])[0] : null;
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [];
        keys.push(this.labelKey);
        if (this.infoHeader !== undefined && this.infoHeader != '') {
            keys.push(this.infoHeader);
            this.showHeader = true;
        }

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.label = this.terms[this.labelKey];
            this.headerText = this.terms[this.infoHeader];
        });
    }

    private loadAvailableSettingTypes(): ng.IPromise<any> {
        return this.employeeService.getAvailableEmployeeSettingsByArea(this.area).then(x => {
            this.allTypes = x;

            this.allTypes.forEach(t => {
                if (t.employeeSettingGroupType !== TermGroup_EmployeeSettingType.None) {
                    if (t.employeeSettingType === TermGroup_EmployeeSettingType.None)
                        this.groupTypes.push(t);
                    else
                        this.types.push(t);
                }
            });
        });
    }

    private setNamesOnAll() {
        this.settings.forEach(s => {
            if (s.employeeSettingAreaType === this.area)
                this.setNames(s);
        });
    }

    private setNames(setting: EmployeeSettingDTO) {
        setting.groupTypeName = this.getGroupTypeName(setting.employeeSettingGroupType);
        setting.typeName = this.getTypeName(setting.employeeSettingType);

        if (setting.dataType === SettingDataType.Integer) {
            const type = this.types.find(t => t.employeeSettingType === setting.employeeSettingType);
            if (type?.hasOptions) {
                const opt = type.options.find(o => o.id === setting.intData);
                if (opt)
                    setting.optionName = opt.name;
            }
        }
    }

    // EVENTS

    private showAllGenerationsChanged() {
        this.$timeout(() => {
            this.setFilteredSettings();
        });
    }

    private editSetting(setting: EmployeeSettingDTO) {
        let isNew = false;
        if (!setting) {
            isNew = true;
            setting = new EmployeeSettingDTO();
            setting.tmpEmployeeSettingId = ++this.tmpEmployeeSettingIdCounter;
            setting.employeeSettingAreaType = this.area;
        }

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/EmployeeSettings/Views/EmployeeSettingDialog.html"),
            controller: EmployeeSettingDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                groupTypes: () => { return this.groupTypes },
                types: () => { return this.types },
                settings: () => { return this.settings },
                setting: () => { return setting },
                isNew: () => { return isNew }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.setting) {
                if (isNew) {
                    // Add new setting to the original collection
                    if (!this.settings)
                        this.settings = [];
                    this.updateSetting(setting, result.setting);
                    this.settings.push(setting);
                } else {
                    // Update original setting
                    let originalSetting = _.find(this.settings, s => setting.employeeSettingId ? s.employeeSettingId === setting.employeeSettingId : s.tmpEmployeeSettingId === setting.tmpEmployeeSettingId);
                    if (originalSetting)
                        this.updateSetting(originalSetting, result.setting);
                }

                this.setFilteredSettings();

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private updateSetting(setting: EmployeeSettingDTO, input: EmployeeSettingDTO) {
        setting.state = SoeEntityState.Active;
        setting.isModified = true;
        setting.employeeSettingGroupType = input.employeeSettingGroupType;
        setting.employeeSettingType = input.employeeSettingType;
        setting.validFromDate = input.validFromDate;
        setting.validToDate = input.validToDate;
        setting.dataType = input.dataType;
        setting.strData = input.strData;
        setting.intData = input.intData;
        setting.decimalData = input.decimalData;
        setting.boolData = input.boolData;
        setting.dateData = input.dateData;
        setting.timeData = input.timeData;

        this.setNames(setting);
    }

    private deleteSetting(setting: EmployeeSettingDTO) {
        // If not saved yet, just remove it from the collection
        // Otherwise set state so it will be deleted on server
        if (!setting.employeeSettingId) {
            _.pull(this.settings, setting);
        } else {
            setting.state = SoeEntityState.Deleted;
            setting.isModified = true;
        }
        this.setFilteredSettings();

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private getGroupType(type: TermGroup_EmployeeSettingType) {
        return this.allTypes.find(t => t.employeeSettingGroupType === type);
    }

    private getType(type: TermGroup_EmployeeSettingType) {
        return this.allTypes.find(t => t.employeeSettingType === type);
    }

    private getGroupTypeName(type: TermGroup_EmployeeSettingType): string {
        return this.getGroupType(type)?.name || '';
    }

    private getTypeName(type: TermGroup_EmployeeSettingType): string {
        return this.getType(type)?.name || '';
    }

    private setFilteredSettings() {
        // Reset current
        _.forEach(this.settings, s => {
            if (s.employeeSettingAreaType === this.area)
                s.isCurrent = false;
        });

        // Set current
        let tmpSettings = _.filter(this.settings, s => s.employeeSettingAreaType === this.area && (!s.validFromDate || s.validFromDate.isSameOrBeforeOnDay(CalendarUtility.getDateToday())));
            let types: TermGroup_EmployeeSettingType[] = _.uniq(_.map(tmpSettings, f => f.employeeSettingType));
            _.forEach(types, type => {
                let settingsOfType = _.filter(tmpSettings, f => f.employeeSettingType === type);
                if (settingsOfType.length > 0)
                    _.orderBy(settingsOfType, 'sortableDate', 'desc')[0].isCurrent = true;
            });
       
        this.filteredSettings = _.orderBy(_.filter(this.settings, s => s.employeeSettingAreaType === this.area && s.state === SoeEntityState.Active && (this.showAllGenerations || s.isCurrent)), ['employeeSettingAreaType', 'isCurrent', 'sortableDate'], ['asc', 'desc', 'desc']);
    }
}