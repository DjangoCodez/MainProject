import { GridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { SkillMatcherDTO, EmployeeSkillDTO } from "../../Models/SkillDTOs";
import { SoeEntityType, Feature, CompanySettingType } from "../../../Util/CommonEnumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";

export class SkillsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('Skills', 'Skills.html'),
            scope: {
                recordEntity: '@',
                recordId: '=',
                selectedSkills: '=',
                nbrOfRows: '@',
                hideDate: '@',
                readOnly: '=',
                hideHeader: '@',
            },
            restrict: 'E',
            replace: true,
            controller: SkillsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class SkillsController extends GridControllerBase {

    // Setup
    private recordEntity: SoeEntityType;
    private recordId: number;
    private selectedSkills: EmployeeSkillDTO[];
    private nbrOfRows: number;
    private hideDate: boolean;
    private readOnly: boolean;

    // Converted init parameters
    private minRowsToShow: number = 8; // Default

    // Company settings
    private nbrOfSkillLevels: number = 1;
    private halfPrecision: boolean = false;

    // Collections
    private terms: any;
    private skills: SkillMatcherDTO[] = [];
    private levelsDict: any[];

    private selectAllSkills: boolean = false;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Common.Directives.Skills", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public $onInit() {
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.enableDoubleClick = false;
        this.soeGridOptions.setMinRowsToShow(this.nbrOfRows ? this.nbrOfRows : this.minRowsToShow);
    }

    public setupGrid() {
        this.startLoad();
        this.$q.all([this.loadTerms(),
        this.loadCompanySettings(),
        this.loadAllSkills()]).then(() => this.setupGridColumns());
    }

    private setupGridColumns() {
        this.soeGridOptions.addColumnBool("selected", this.terms["common.categories.selected"], "60", true, "selectEmployeeSkill", null, "readOnly");

        this.soeGridOptions.addColumnText("name", this.terms["common.name"], null);
        if (this.nbrOfSkillLevels > 1) {
            var colDefRating = this.soeGridOptions.addColumnSelect("skillRating", this.terms["common.skills.level"], "60", this.levelsDict, false, true, "skillRating", "value", "value", "selectEmployeeSkill");
            colDefRating.cellEditableCondition = ($scope) => { return !this.readOnly; }
        }
        if (!this.hideDate) {
            var colDefDateTo = this.soeGridOptions.addColumnDate("dateTo", this.terms["common.skills.date"], "100");
            colDefDateTo.enableCellEdit = true;
            colDefDateTo.cellEditableCondition = ($scope) => { return !this.readOnly; }
        }

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.soeGridOptions.subscribe(events);

        super.gridDataLoaded(this.sortEmployeeSkills());

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.readOnly, () => {
            // HACK: cellEditableCondition does not work with bool columns
            _.forEach(this.skills, skill => {
                skill['readOnly'] = this.readOnly;
            });
        });
        this.$scope.$watch(() => this.selectedSkills, () => {
            super.gridDataLoaded(this.sortEmployeeSkills());
            if (this.selectedSkills) {
                if (this.selectedSkills.length === this.skills.length)
                    this.selectAllSkills = true;
            }
        });
    }

    // Lookups

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.categories.selected",
            "common.name",
            "common.skills.date",
            "common.skills.level"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.TimeNbrOfSkillLevels);
        settingTypes.push(CompanySettingType.TimeSkillLevelHalfPrecision);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.nbrOfSkillLevels = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeNbrOfSkillLevels, this.nbrOfSkillLevels);
            if (this.nbrOfSkillLevels === 0)
                this.nbrOfSkillLevels = 1;
            this.levelsDict = [];
            if (this.nbrOfSkillLevels > 1) {
                for (var i = 1; i <= this.nbrOfSkillLevels; i++) {
                    this.levelsDict.push({ value: i, label: i });
                }
            }
            this.halfPrecision = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSkillLevelHalfPrecision);
        });
    }

    private loadAllSkills(): ng.IPromise<any> {
        return this.coreService.getSkills(true).then(x => {
            this.skills = x;
            if (this.selectedSkills) {
                if (this.selectedSkills.length === this.skills.length)
                    this.selectAllSkills = true;
            }
        });
    }

    // Events

    private selectAll() {
        this.$timeout(() => {
            _.forEach(this.skills, skill => {
                skill.selected = this.selectAllSkills;
                this.selectEmployeeSkill(skill);
            });
        });
    }

    private selectEmployeeSkill(row: SkillMatcherDTO) {
        this.$timeout(() => {
            if (row.selected === true) {
                if (row.skillRating === 0 && this.nbrOfSkillLevels === 1)
                    row.skillRating = 1;

                if (_.filter(this.selectedSkills, s => s.skillId == row.skillId).length == 0) {
                    var skillToAdd = new EmployeeSkillDTO();
                    skillToAdd.skillId = row.skillId;
                    skillToAdd.skillLevel = this.convertToLevel(row.skillRating);
                    row.skillLevel = skillToAdd.skillLevel;
                    this.selectedSkills.push(skillToAdd);
                } else {
                    var skill = _.filter(this.selectedSkills, s => s.skillId == row.skillId)[0];
                    if (skill) {
                        skill.skillLevel = this.convertToLevel(row.skillRating);
                        row.skillLevel = skill.skillLevel;
                    }
                }
            } else {
                if (_.filter(this.selectedSkills, s => s.skillId == row.skillId).length > 0)
                    this.selectedSkills.splice(this.selectedSkills.indexOf(_.find(this.selectedSkills, s => s.skillId == row.skillId)), 1);
            }

            this.employeeSkillChanged(row);
        });
    }

    private afterCellEdit(row: any, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue)
            return;

        if (colDef.field === 'dateTo') {
            let skill = _.find(this.selectedSkills, s => s.skillId === row.skillId);
            if (skill) {
                skill.dateTo = row.dateTo;
                skill.skillLevel = this.convertToLevel(row.skillRating);
                row.skillLevel = skill.skillLevel;
                this.employeeSkillChanged(row);
            }
        }
    }

    private employeeSkillChanged(row: SkillMatcherDTO) {
        this.messagingService.publish('employeeSkillsChanged', { recordId: this.recordId, skill: row });
    }

    // Help-methods

    private sortEmployeeSkills() {
        // Mark selected skills
        if (this.selectedSkills) {
            _.forEach(this.skills, skill => {
                let selectedSkill = _.find(this.selectedSkills, s => s.skillId === skill.skillId);
                if (selectedSkill) {
                    skill.selected = true;
                    skill.skillRating = this.convertToRating(selectedSkill.skillLevel);
                    skill.dateTo = selectedSkill.dateTo;
                } else {
                    skill.selected = false;
                    skill.skillRating = 1; //Default;
                    skill.dateTo = null;
                }
            });
        } else {
            //If inserting new record
            _.forEach(this.skills, skill => {
                skill.selected = false;
                skill.skillRating = 1; //Default;
            });
        }

        // Sort to get selected skills at the top
        return _.orderBy(this.skills, ['selected', 'name'], ['desc', 'asc']);
    }

    private convertToRating(level: number): number {
        return (this.nbrOfSkillLevels > 0 ? Math.round(level / (100 / this.nbrOfSkillLevels)) : 0);
    }

    private convertToLevel(rating: number): number {
        var level: number = (this.nbrOfSkillLevels > 0 ? Math.round((100 * (rating / this.nbrOfSkillLevels))) : 0);
        if (level > 100)
            level = 100;

        return level;
    }
}