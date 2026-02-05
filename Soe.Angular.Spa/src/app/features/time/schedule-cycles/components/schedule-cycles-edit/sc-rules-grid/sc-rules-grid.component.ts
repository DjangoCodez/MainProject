import { Component, inject, Input, input, OnInit, signal } from '@angular/core';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { IScheduleCycleRuleDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { ScheduleCycleRuleTypesService } from '../../../../schedule-cycle-rule-types/services/schedule-cycle-rule-types.service';
import { ScheduleCyclesForm } from '../../../models/schedule-cycles-form.model';
import { ScheduleCycleRuleForm } from '../../../models/sc-rules-form.model';

@Component({
  selector: 'soe-sc-rules-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class ScRulesGridComponent
  extends EmbeddedGridBaseDirective<IScheduleCycleRuleDTO, ScheduleCyclesForm>
  implements OnInit
{
  @Input({ required: true }) form!: ScheduleCyclesForm;
  noMargin = input(true);
  height = input(150);

  gridToolbarService = inject(ToolbarService);
  scheduleCycleRuleTypesService = inject(ScheduleCycleRuleTypesService);

  ruleTypes: SmallGenericType[] = [];

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_StaffingNeeds_ScheduleCycle,
      'time.schedule.schedulecycle.rules',
      {
        lookups: [this.loadRuleTypes()],
      }
    );

    this.form.scheduleCycleRuleDTOs.valueChanges.subscribe(v =>
      this.initRows(v)
    );
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarButtonAdd({
          caption: signal('time.schedule.schedulecycle.addnewrule'),
          onAction: () => this.addRow(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<IScheduleCycleRuleDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.schedule.schedulecycle.rule',
        'time.schedule.schedulecycle.minoccurrences',
        'time.schedule.schedulecycle.maxoccurrences',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'scheduleCycleRuleTypeId',
          terms['time.schedule.schedulecycle.rule'],
          this.ruleTypes || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 50,
            editable: true,
          }
        );

        this.grid.addColumnNumber(
          'minOccurrences',
          terms['time.schedule.schedulecycle.minoccurrences'],
          {
            flex: 25,
            editable: true,
          }
        );

        this.grid.addColumnNumber(
          'maxOccurrences',
          terms['time.schedule.schedulecycle.maxoccurrences'],
          {
            flex: 25,
            editable: true,
          }
        );

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });

        this.grid.setNbrOfRowsToShow(3, 10);
        super.finalizeInitGrid({ hidden: true });
      });
  }

  private initRows(rows: IScheduleCycleRuleDTO[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: IScheduleCycleRuleDTO = {
      scheduleCycleRuleId: 0,
      scheduleCycleId: this.form.value.scheduleCycleId || 0,
      scheduleCycleRuleTypeId: 0,
      minOccurrences: 0,
      maxOccurrences: 0,
      state: SoeEntityState.Active,
      createdBy: '',
      modifiedBy: '',
      scheduleCycleRuleTypeDTO: null as any,
    };
    super.addRow(row, this.form.scheduleCycleRuleDTOs, ScheduleCycleRuleForm);
  }

  override deleteRow(row: IScheduleCycleRuleDTO) {
    super.deleteRow(row, this.form.scheduleCycleRuleDTOs);
  }

  loadRuleTypes(): Observable<SmallGenericType[]> {
    return this.scheduleCycleRuleTypesService
      .getDict(true)
      .pipe(tap(x => (this.ruleTypes = x)));
  }
}
