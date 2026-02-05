import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { SkillMatcherDTO, EmployeeSkillDTO } from "../../../../Common/Models/SkillDTOs";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { IScheduleService as ISharedScheduleService } from "../../Schedule/ScheduleService";
import { IShiftTypeDTO } from "../../../../Scripts/TypeLite.Net4";
import { CompanySettingType } from "../../../../Util/CommonEnumerations";
import { PositionDTO } from "../../../../Common/Models/EmployeePositionDTO";

export class SkillMatcherDirectiveFactory {

    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Time/Directives/SkillMatcher/Views/SkillMatcher.html'),
            scope: {
                invalid: '=',
                date: '=',
                shiftTypeIds: '=',
                employeeId: '=?',
                employeePostId: '=?',
                bindEmployeeSkills: '@',
                employeeSkills: '=?',
                nbrOfSkills: '=',
                employeePositionIds: "=?",
                ignoreEmployeeIds: "=",
                hideHeader: '@',
                employeeSkillsChanged: '&'
            },
            restrict: 'E',
            replace: true,
            controller: SkillMatcherController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class SkillMatcherController {

    // Terms
    private terms: any;

    // Init parameters
    private invalid: boolean = false;
    private date: Date;
    private shiftTypeIds: number[];
    private employeeId: number;
    private employeePostId: number;
    private bindEmployeeSkills: boolean;
    private employeeSkills: EmployeeSkillDTO[];
    private employeePositionIds: number[];
    private ignoreEmployeeIds: number[];
    private hideHeader: boolean;

    private get nbrOfSkills(): number {
        if (this.skills)
            return this.skills.length;
    }
    private set nbrOfSkills(nbr: number) { /* Not actually a setter, just to make binding work */ }

    // Company settings
    private nbrOfSkillLevels: number = 0;
    private halfPrecision: boolean = false;

    // Data
    private shiftTypes: IShiftTypeDTO[] = [];
    private skills: SkillMatcherDTO[] = [];

    private showAll: boolean = false;
    private showDateTo: boolean = false;

    private positionSkills: any[] = [];
    private positions: PositionDTO[] = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private sharedScheduleService: ISharedScheduleService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService) {
    }

    // SETUP

    public $onInit() {
        if (!this.bindEmployeeSkills)
            this.employeeSkills = [];

        this.setup();
    }

    private setup() {
        this.hideHeader = <any>this.hideHeader === 'true';
        this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadShiftTypes(),
            this.loadPositionSkills()
        ]).then(() => {
            this.setupWatchers();
            this.messagingService.subscribe('employeeSkillsChanged', (data) => {
                if (data.recordId !== this.employeeId && data.recordId !== this.employeePostId)
                    return;

                // Update skills
                var skillMatch: SkillMatcherDTO = data.skill;
                _.forEach(_.filter(this.employeeSkills, s => s.skillId === skillMatch.skillId), employeeSkill => {
                    employeeSkill.dateTo = skillMatch.dateTo;
                    employeeSkill.skillLevel = skillMatch.skillLevel;
                    this.validatePositions();
                });
            }, this.$scope);

            if (this.employeePostId !== 0 && this.employeeSkills.length === 0)
                this.loadEmployeeSkills();
            else if (this.employeeId !== 0 && this.employeeSkills.length === 0)
                this.loadEmployeeSkills();
            else
                this.validate();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.date, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.validate();
        });
        this.$scope.$watch(() => this.employeeId, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.loadEmployeeSkills();
        });
        this.$scope.$watch(() => this.employeePostId, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.loadEmployeeSkills();
        });
        this.$scope.$watch(() => this.shiftTypeIds, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.validate();
        });
        this.$scope.$watch(() => this.employeePositionIds, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.validatePositions();
        });
        this.$scope.$watchCollection(() => this.employeeSkills, (newVal, oldVal) => {
            this.validatePositions();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.skillmatcher.missing",
            "time.skillmatcher.levelunreached",
            "time.skillmatcher.datetopassed",
            "time.skillmatcher.ok",
            "time.skillmatcher.allok"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.TimeNbrOfSkillLevels);
        settingTypes.push(CompanySettingType.TimeSkillLevelHalfPrecision);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.nbrOfSkillLevels = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeNbrOfSkillLevels);
            this.halfPrecision = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSkillLevelHalfPrecision);
        });
    }

    private loadShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypes(false, true, false, false, false, false).then(x => {
            this.shiftTypes = x;
        });
    }

    private loadPositionSkills(): ng.IPromise<any> {
        return this.coreService.getPositions(true, true).then(x => {
            this.positions = x;
        });
    }

    private loadEmployeeSkills() {
        if (this.bindEmployeeSkills)
            return;

        if (!this.employeePostId && !this.employeeId) {
            this.employeeSkills = [];
            this.validate();
        } else {
            if (this.employeePostId) {
                this.sharedScheduleService.getEmployeePostSkills(this.employeePostId).then(x => {
                    // Map EmployeePostSkillDTO into an EmployeeSkillDTO which is used in the directive
                    this.employeeSkills = x.map(s => {
                        var obj = new EmployeeSkillDTO();
                        angular.extend(obj, s);
                        return obj;
                    });
                    this.validate();
                });
            } else {
                this.sharedScheduleService.getEmployeeSkills(this.employeeId).then(x => {
                    this.employeeSkills = x.map(s => {
                        var obj = new EmployeeSkillDTO();
                        angular.extend(obj, s);
                        return obj;
                    });
                    if (this.employeePositionIds)
                        this.validatePositions();
                    else
                        this.validate();
                });
            }
        }
    }

    // EVENTS

    private showAllChanged() {
        this.$timeout(() => {
            this.validate();
        });
    }

    private showAllPositionsChanged() {
        this.$timeout(() => {
            this.validatePositions();
        });
    }

    // HELP-METHODS

    private validate() {
        var invalid: boolean = false;
        this.skills = [];

        if (!_.includes(this.ignoreEmployeeIds, this.employeeId)) {
            // Loop through all shift types skills and create a new collection
            _.forEach(this.shiftTypeIds, shiftTypeId => {
                var shiftType = _.find(this.shiftTypes, s => s.shiftTypeId === shiftTypeId);
                if (shiftType) {
                    _.forEach(shiftType.shiftTypeSkills, shiftTypeSkill => {
                        var skill = new SkillMatcherDTO();
                        skill.shiftTypeId = shiftTypeId;
                        skill.shiftTypeName = shiftType.name;
                        skill.skillId = shiftTypeSkill.skillId;
                        skill.skillName = shiftTypeSkill.skillName;
                        skill.skillLevel = shiftTypeSkill.skillLevel;
                        this.skills.push(skill);
                    });
                }
            });

            _.forEach(this.skills, skill => {
                let empSkill = _.find(this.employeeSkills, e => e.skillId === skill.skillId);
                let empSkillLevel = empSkill && empSkill.skillLevel ? empSkill.skillLevel : 0;
                skill.missing = !empSkill;
                skill.employeeSkillLevel = empSkillLevel;
                skill.skillLevelUnreached = empSkillLevel < skill.skillLevel;
                skill.dateTo = empSkill && empSkill.dateTo;
                skill.dateToPassed = empSkill && empSkill.dateTo && CalendarUtility.convertToDate(empSkill.dateTo).isBeforeOnDay(this.date);

                skill.skillRating = this.convertToRating(skill.skillLevel);
                skill.employeeSkillRating = this.convertToRating(skill.employeeSkillLevel);

                // Create note
                if (skill.missing)
                    skill.note = this.terms["time.skillmatcher.missing"];
                else {
                    if (skill.skillLevelUnreached)
                        skill.note = this.terms["time.skillmatcher.levelunreached"];
                    if (skill.dateToPassed) {
                        if (skill.note)
                            skill.note += ", " + this.terms["time.skillmatcher.datetopassed"].toString().toLowerCase();
                        else
                            skill.note = this.terms["time.skillmatcher.datetopassed"];
                    }
                }
                if (!skill.note)
                    skill.note = this.terms["time.skillmatcher.ok"];

                skill.ok = (!skill.missing && !skill.skillLevelUnreached && !skill.dateToPassed)
                if (!skill.ok)
                    invalid = true;

                skill.visible = (!skill.ok || this.showAll);
            });

            this.showDateTo = _.filter(this.skills, s => s.dateToPassed).length > 0;
        }

        this.invalid = invalid;
    }

    private validatePositions() {
        var invalid: boolean = false;
        this.skills = [];

        // Loop through all employeePosition
        _.forEach(this.employeePositionIds, employeePositionId => {
            var empPos = _.find(this.positions, s => s.positionId === employeePositionId);
            if (empPos) {
                _.forEach(empPos.positionSkills, positionSkill => {
                    var skill = new SkillMatcherDTO();
                    skill.shiftTypeId = empPos.positionId;
                    skill.shiftTypeName = empPos.name;
                    skill.skillId = positionSkill.skillId;
                    skill.skillName = positionSkill.skillName;
                    skill.skillLevel = positionSkill.skillLevel;
                    this.skills.push(skill);
                });
            }
        });

        _.forEach(this.skills, skill => {
            let empSkill = _.find(this.employeeSkills, e => e.skillId === skill.skillId);
            let empSkillLevel = empSkill && empSkill.skillLevel ? empSkill.skillLevel : 0;
            skill.missing = !empSkill;
            skill.employeeSkillLevel = empSkillLevel;
            skill.skillLevelUnreached = empSkillLevel < skill.skillLevel;
            skill.dateTo = empSkill && empSkill.dateTo;
            skill.dateToPassed = empSkill && empSkill.dateTo && CalendarUtility.convertToDate(empSkill.dateTo).isBeforeOnDay(this.date);

            skill.skillRating = this.convertToRating(skill.skillLevel);
            skill.employeeSkillRating = this.convertToRating(skill.employeeSkillLevel);

            // Create note
            if (skill.missing)
                skill.note = this.terms["time.skillmatcher.missing"];
            else {
                if (skill.skillLevelUnreached)
                    skill.note = this.terms["time.skillmatcher.levelunreached"];
                if (skill.dateToPassed) {
                    if (skill.note)
                        skill.note += ", " + this.terms["time.skillmatcher.datetopassed"].toString().toLowerCase();
                    else
                        skill.note = this.terms["time.skillmatcher.datetopassed"];
                }
            }
            if (!skill.note)
                skill.note = this.terms["time.skillmatcher.ok"];

            skill.ok = (!skill.missing && !skill.skillLevelUnreached && !skill.dateToPassed)
            if (!skill.ok)
                invalid = true;

            skill.visible = (!skill.ok || this.showAll);
        });

        this.showDateTo = _.filter(this.skills, s => s.dateToPassed).length > 0;

        this.invalid = invalid;
    }

    private convertToRating(level: number): number {
        return (this.nbrOfSkillLevels > 0 ? Math.round(level / (100 / this.nbrOfSkillLevels)) : 0);
    }
}
