import {
  Component,
  inject,
  Input,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { SkillsService } from '@features/time/skills/services/skills.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IEmployeeSkillDTO,
  ISkillGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { orderBy } from 'lodash';
import { BehaviorSubject, take, tap } from 'rxjs';
import { SkillMatcherDTO } from '../skill-matcher/models/skill-matcher.model';
import { ToolbarCheckboxAction } from '@ui/toolbar/toolbar-checkbox/toolbar-checkbox.component';

@Component({
  selector: 'skills',
  templateUrl: './skills.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SkillsComponent
  extends GridBaseDirective<SkillMatcherDTO>
  implements OnInit
{
  hideDate = input(false);
  form = input<SoeFormGroup | undefined>();

  @Input() selectedSkills = new BehaviorSubject<any[]>([]);

  rowSelectionChanged = output<any[]>();

  coreService = inject(CoreService);
  skillsService = inject(SkillsService);
  progressService = inject(ProgressService);
  toolbarService = inject(ToolbarService);

  private nbrOfSkillLevels: number = 1;
  selectAllSkills = signal(false);
  private levelsDict: SmallGenericType[] = [];
  skills: SkillMatcherDTO[] = [];

  performGridLoad = new Perform<ISkillGridDTO[]>(this.progressService);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'Common.Directives.Skills', {
      skipInitialLoad: true,
    });
  }

  changeSelection(value: boolean, rows: any) {
    rows.selected = value;
    let selectedRows = [];

    if (!value) {
      selectedRows = this.selectedSkills.value.filter(
        item => item.skillId !== rows.skillId
      );
      this.selectedSkills.next([]);
      this.selectedSkills.next(selectedRows);
    } else {
      selectedRows = rows;
      selectedRows.skillLevel = this.convertToLevel(selectedRows.skillRating);
      this.selectedSkills.value.push(selectedRows);
    }
    this.setChanged();
  }

  changeLevel(row: any) {
    let selectedRows = [];
    selectedRows = this.selectedSkills.value;
    selectedRows.forEach(item => {
      if (item.skillId == row.data.skillId)
        item.skillLevel = this.convertToLevel(row.data.skillRating);
    });
    this.selectedSkills.next([]);
    this.selectedSkills.next(selectedRows);
    this.setChanged();
  }

  setChanged() {
    this.rowSelectionChanged.emit(this.selectedSkills.value);
    this.form()?.markAsDirty();
    this.form()?.markAsTouched();
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
      hideClearFilters: true,
    });

    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarCheckbox('selectAll', {
          labelKey: signal('core.selectall'),
          checked: this.selectAllSkills,
          onValueChanged: event =>
            this.selectAll((event as ToolbarCheckboxAction).value),
        }),
      ],
    });
  }

  override loadCompanySettings() {
    const settingTypes: number[] = [];
    settingTypes.push(CompanySettingType.TimeNbrOfSkillLevels);
    settingTypes.push(CompanySettingType.TimeSkillLevelHalfPrecision);

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(setting => {
        this.nbrOfSkillLevels = SettingsUtil.getIntCompanySetting(
          setting,
          CompanySettingType.TimeNbrOfSkillLevels,
          this.nbrOfSkillLevels
        );
        if (this.nbrOfSkillLevels === 0) this.nbrOfSkillLevels = 1;
        this.levelsDict = [];
        if (this.nbrOfSkillLevels > 1) {
          for (let i = 1; i <= this.nbrOfSkillLevels; i++) {
            this.levelsDict.push({ id: i, name: i.toString() });
          }
        }
      })
    );
  }

  loadGridData() {
    this.performGridLoad.load(
      this.skillsService.getGrid().pipe(
        tap(value => {
          this.skills = value as unknown as SkillMatcherDTO[];
          this.setupSkillGrid();

          if (this.selectedSkills.value.length === this.skills.length)
            this.selectAllSkills.set(true);
        })
      )
    );
  }

  setupSkillGrid() {
    this.selectedSkills.subscribe(selectedData => {
      if (selectedData) {
        this.skills.forEach(skill => {
          const selectedSkill = this.selectedSkills.value.find(
            s => s.skillId === skill.skillId
          );
          if (selectedSkill) {
            skill.selected = true;
            skill.skillRating = this.convertToRating(selectedSkill.skillLevel);
            skill.dateTo = selectedSkill.dateTo;
          } else {
            skill.selected = false;
            skill.skillRating = 1; //Default;
            skill.dateTo = undefined;
          }
        });
      } else {
        //If inserting new record
        this.skills.forEach(skill => {
          skill.selected = false;
          skill.skillRating = 1; //Default;
        });
      }

      this.grid.setData(
        orderBy(this.skills, ['selected', 'name'], ['desc', 'asc'])
      );
    });
  }

  selectAll(value: boolean) {
    this.skills.forEach(skill => {
      skill.selected = value;
      this.selectEmployeeSkill(skill);
    });
    this.rowData.next(this.skills);
    this.setChanged();
  }

  private selectEmployeeSkill(row: any) {
    if (row.selected === true) {
      if (row.skillRating === 0 && this.nbrOfSkillLevels === 1)
        row.skillRating = 1;

      if (
        this.selectedSkills.value.filter(s => s.skillId == row.skillId)
          .length == 0
      ) {
        const skillToAdd: Partial<IEmployeeSkillDTO> = {};
        skillToAdd.skillId = row.skillId;
        skillToAdd.skillLevel = this.convertToLevel(row.skillRating);
        row.skillLevel = skillToAdd.skillLevel;
        this.selectedSkills.value.push(<IEmployeeSkillDTO>skillToAdd);
      } else {
        const skill = this.selectedSkills.value.filter(
          s => s.skillId == row.skillId
        )[0];
        if (skill) {
          skill.skillLevel = this.convertToLevel(row.skillRating);
          row.skillLevel = skill.skillLevel;
        }
      }
    } else {
      if (
        this.selectedSkills.value.filter(s => s.skillId == row.skillId).length >
        0
      )
        this.selectedSkills.value.splice(
          this.selectedSkills.value.indexOf(
            this.selectedSkills.value.find(s => s.skillId === row.skillId) ??
              ({ skillId: -1 } as IEmployeeSkillDTO),
            1
          )
        );
    }
  }

  private convertToRating(level: number): number {
    return this.nbrOfSkillLevels > 0
      ? Math.round(level / (100 / this.nbrOfSkillLevels))
      : 0;
  }

  private convertToLevel(rating: number): number {
    let level: number =
      this.nbrOfSkillLevels > 0
        ? Math.round(100 * (rating / this.nbrOfSkillLevels))
        : 0;
    if (level > 100) level = 100;

    return level;
  }

  override onGridReadyToDefine(grid: GridComponent<SkillMatcherDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.setNbrOfRowsToShow(8);

    this.translate
      .get([
        'common.selected',
        'common.name',
        'common.skills.date',
        'common.skills.level',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnBool('selected', terms['common.selected'], {
          width: 40,
          alignCenter: true,
          editable: true,
          suppressFilter: true,
          columnSeparator: true,
          onClick: (value, rows) => this.changeSelection(value, rows),
        });
        this.grid.addColumnText('name', terms['common.name'], { flex: 2 });
        if (this.nbrOfSkillLevels > 1) {
          this.grid.addColumnSelect(
            'skillRating',
            terms['common.skills.level'],
            this.levelsDict,
            (row: any) => this.changeLevel(row),
            {
              dropDownIdLabel: 'id',
              dropDownValueLabel: 'name',
              flex: 1,
              editable: true,
            }
          );
        }
        if (!this.hideDate()) {
          this.grid.addColumnDate('dateTo', terms['common.skills.date'], {
            flex: 1,
            editable: true,
          });
        }

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });

        this.loadGridData();
      });
  }
}
