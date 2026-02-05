import {
  Component,
  computed,
  effect,
  inject,
  input,
  model,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { LabelComponent } from '@ui/label/label.component';
import { SkillMatcherDTO } from './models/skill-matcher.model';
import {
  IEmployeeSkillDTO,
  IShiftTypeDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ShiftTypeService } from '@shared/features/shift-type/services/shift-type.service';
import { forkJoin, Observable, of, tap } from 'rxjs';
import { DateUtil } from '@shared/util/date-util'
import { NumberUtil } from '@shared/util/number-util'
import { SettingsUtil, UserCompanySettingCollection } from '@shared/util/settings-util';
import { CompanySettingType } from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { TermCollection } from '@shared/localization/term-types';
import { DatePipe } from '@angular/common';
import { SkillService } from '@shared/services/time/skill.service';

@Component({
  selector: 'skill-matcher',
  imports: [
    CheckboxComponent,
    ExpansionPanelComponent,
    LabelComponent,
    DatePipe,
    TranslatePipe,
  ],
  templateUrl: './skill-matcher.component.html',
  styleUrl: './skill-matcher.component.scss',
})
export class SkillMatcherComponent implements OnInit {
  labelKey = input('common.skills.skills');
  date = input.required<Date>();
  shiftTypeIds = input.required<number[]>();
  employeeId = input<number>(0);
  employeePostId = input<number>(0);
  employeePositionIds = input<number[]>([]);
  ignoreEmployeeIds = input<number[]>([]);
  employeeSkills = model<IEmployeeSkillDTO[]>([]);
  hideHeader = input(false);

  validated = output<boolean>();

  // Services
  private readonly coreService = inject(CoreService);
  private readonly shiftTypeService = inject(ShiftTypeService);
  private readonly skillService = inject(SkillService);
  private readonly translate = inject(TranslateService);

  // Company settings
  nbrOfSkillLevels = signal(0);
  halfPrecision = signal(false);

  // Data
  private terms: TermCollection = {};
  skills = signal<SkillMatcherDTO[]>([]);
  visibleSkills = computed(() => this.skills().filter(s => s.visible));

  private shiftTypes: IShiftTypeDTO[] = [];

  showAll = signal(false);
  showDateTo = signal(false);
  isValid = signal(false);

  labelClass = computed(() => {
    return this.isValid() ? 'color-text' : 'color-error';
  });

  prevEmployeeId = 0;
  prevEmployeePostId = 0;
  prevDate?: Date = undefined;
  prevShiftTypeIds: number[] = [];

  constructor() {
    effect(() => {
      if (this.employeeId() !== this.prevEmployeeId) {
        this.prevEmployeeId = this.employeeId();
        this.loadEmployeeSkills(this.employeeId()).subscribe();
      }

      if (this.employeePostId() !== this.prevEmployeePostId) {
        this.prevEmployeePostId = this.employeePostId();
        this.loadEmployeePostSkills(this.employeePostId()).subscribe();
      }

      if (!this.prevDate || this.date().isSameDay(this.prevDate)) {
        // Do not validate initially
        if (this.prevDate) this.initValidate(this.date());
        this.prevDate = this.date();
      }

      if (
        !NumberUtil.compareArrays(this.shiftTypeIds(), this.prevShiftTypeIds)
      ) {
        // Do not validate initially
        if (this.prevShiftTypeIds.length > 0) this.initValidate(this.date());
        this.prevShiftTypeIds = [...this.shiftTypeIds()];
      }
    });
  }

  ngOnInit(): void {
    forkJoin([
      this.loadTerms(),
      this.loadCompanySettings(),
      this.loadShiftTypes(),
    ])
      .pipe(
        tap(() => {
          if (this.employeeSkills().length > 0) {
            this.initValidate();
          }
        })
      )
      .subscribe();
  }

  // SERVICE CALLS

  private loadTerms(): Observable<TermCollection> {
    return this.translate
      .get([
        'time.skillmatcher.missing',
        'time.skillmatcher.levelunreached',
        'time.skillmatcher.datetopassed',
        'time.skillmatcher.ok',
        'time.skillmatcher.allok',
      ])
      .pipe(
        tap(terms => {
          this.terms = terms;
        })
      );
  }

  private loadCompanySettings(): Observable<UserCompanySettingCollection> {
    const settingTypes: number[] = [];

    settingTypes.push(CompanySettingType.TimeNbrOfSkillLevels);
    settingTypes.push(CompanySettingType.TimeSkillLevelHalfPrecision);

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(x => {
        this.nbrOfSkillLevels.set(
          SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.TimeNbrOfSkillLevels
          )
        );
        this.halfPrecision.set(
          SettingsUtil.getBoolCompanySetting(
            x,
            CompanySettingType.TimeSkillLevelHalfPrecision
          )
        );
      })
    );
  }

  private loadShiftTypes(): Observable<IShiftTypeDTO[]> {
    return this.shiftTypeService
      .getShiftTypes(
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false
      )
      .pipe(
        tap(x => {
          this.shiftTypes = x;
        })
      );
  }

  private loadEmployeeSkills(
    employeeId: number | undefined
  ): Observable<IEmployeeSkillDTO[]> {
    if (employeeId) {
      return this.skillService.getEmployeeSkills(employeeId).pipe(
        tap(x => {
          this.employeeSkills.set(x);
          this.initValidate();
        })
      );
    } else {
      return of([]); // No employee provided
    }
  }

  private loadEmployeePostSkills(
    employeePostId: number | undefined
  ): Observable<IEmployeeSkillDTO[]> {
    if (employeePostId) {
      return this.skillService.getEmployeePostSkills(employeePostId).pipe(
        tap(x => {
          this.employeeSkills.set(x);
          this.initValidate();
        })
      );
    } else {
      return of([]); // No employee post provided
    }
  }

  // EVENTS

  showAllChanged(value: boolean): void {
    this.showAll.set(value);
    this.initValidate();
  }

  // HELP-METHODS

  private initValidate(date: Date = this.date()) {
    if (this.employeePositionIds().length > 0) {
      this.validatePositions(date);
    } else {
      this.validate(date);
    }
  }

  private validate(date: Date): void {
    let isValid = true;
    const skills: SkillMatcherDTO[] = [];

    if (!this.ignoreEmployeeIds().includes(this.employeeId())) {
      // Loop through all shift types skills and create a new collection
      this.shiftTypeIds().forEach(shiftTypeId => {
        const shiftType = this.shiftTypes.find(
          s => s.shiftTypeId === shiftTypeId
        );

        // TODO: Replace sortedSkills with shiftType.shiftTypeSkills.toSorted after updating to ES2023 in tsconfig
        if (shiftType) {
          const sortedSkills = shiftType.shiftTypeSkills.sort((a, b) =>
            a.skillName.localeCompare(b.skillName)
          );
          sortedSkills.forEach(shiftTypeSkill => {
            const skill = new SkillMatcherDTO();
            skill.shiftTypeId = shiftTypeId;
            skill.shiftTypeName = shiftType.name;
            skill.skillId = shiftTypeSkill.skillId;
            skill.skillName = shiftTypeSkill.skillName;
            skill.skillLevel = shiftTypeSkill.skillLevel;
            skills.push(skill);
          });
        }
      });

      skills.forEach(skill => {
        const empSkill = this.employeeSkills().find(
          e => e.skillId === skill.skillId
        );
        const empSkillLevel = empSkill?.skillLevel ?? 0;
        skill.missing = !empSkill;
        skill.employeeSkillLevel = empSkillLevel;
        skill.skillLevelUnreached = empSkillLevel < skill.skillLevel;
        skill.dateTo = empSkill?.dateTo;
        skill.dateToPassed = !!(
          empSkill?.dateTo &&
          DateUtil.parseDateOrJson(empSkill?.dateTo)!.isBeforeOnDay(date)
        );

        skill.skillRating = this.convertToRating(skill.skillLevel);
        skill.employeeSkillRating = this.convertToRating(
          skill.employeeSkillLevel
        );

        // Create note
        if (skill.missing) {
          skill.note = this.terms['time.skillmatcher.missing'];
        } else {
          if (skill.skillLevelUnreached) {
            skill.note = this.terms['time.skillmatcher.levelunreached'];
          }
          if (skill.dateToPassed) {
            if (skill.note) {
              skill.note += `, ${this.terms['time.skillmatcher.datetopassed'].toLowerCase()}`;
            } else {
              skill.note = this.terms['time.skillmatcher.datetopassed'];
            }
          }
        }
        if (!skill.note) {
          skill.note = this.terms['time.skillmatcher.ok'];
        }

        skill.ok =
          !skill.missing && !skill.skillLevelUnreached && !skill.dateToPassed;
        if (!skill.ok) isValid = false;

        skill.visible = !skill.ok || this.showAll();
      });

      this.showDateTo.set(skills.filter(s => s.dateToPassed).length > 0);
    }

    this.skills.set(skills);
    this.isValid.set(isValid);
    this.validated.emit(isValid);
  }

  private validatePositions(date: Date) {
    // TODO: Implement position validation logic, used on employee page
  }

  private convertToRating(level: number): number {
    return this.nbrOfSkillLevels() > 0
      ? Math.round(level / (100 / this.nbrOfSkillLevels()))
      : 0;
  }
}
