import {
  Component,
  Input,
  EventEmitter,
  OnInit,
  Output,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IDistributionCodePeriodDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, BehaviorSubject } from 'rxjs';
import { DistributionCodeHeadForm } from '../../../models/distribution-codes-head-form.model';
import {
  DistributionCodeHeadDTO,
  PeriodSummery,
} from '../../../models/distribution-codes.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { CellValueChangedEvent } from 'ag-grid-community';

@Component({
  selector: 'soe-distribution-codes-edit-grid',
  templateUrl: './distribution-codes-edit-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DistributionCodesEditGridComponent
  extends GridBaseDirective<IDistributionCodePeriodDTO>
  implements OnInit, OnChanges
{
  @Input({ required: true }) form!: DistributionCodeHeadForm;
  @Input() rows!: BehaviorSubject<IDistributionCodePeriodDTO[]>;
  @Input() sumPercent!: number;
  @Input() diff!: number;
  @Input() subLevelDict: DistributionCodeHeadDTO[] = [];
  @Input() hideSublevelColumn = true;
  @Input() hidePeriodInfoColumn = true;

  @Output() updateSummery = new EventEmitter<PeriodSummery>();

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Preferences_VoucherSettings_DistributionCodes_Edit,
      '',
      { skipInitialLoad: true }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.hideSublevelColumn || changes.hidePeriodInfoColumn)
      this.hideColumns();
  }

  onCellValueChanged(event: CellValueChangedEvent): void {
    this.form?.markAsDirty();
    if (event.colDef.field == 'percent') {
      this.sumPercent = this.sumPercent + (event.newValue - event.oldValue);
      this.diff = this.sumPercent - 100;
      this.updateSummery.emit({
        diff: this.diff,
        sumPercent: this.sumPercent,
      });
    }

    if (event.newValue !== event.oldValue) {
      this.form?.customPeriodsPatchValue(this.rows.value);
    }
  }

  onGridReadyToDefine(grid: GridComponent<IDistributionCodePeriodDTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.setNbrOfRowsToShow(12, 20);
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      onRowEditingStopped: () => {
        this.form?.customPeriodsPatchValue(this.rows.value);
        this.form?.updateValueAndValidity();
      },
    });

    this.translate
      .get([
        'common.periodnumber',
        'economy.accounting.salesbudget.subtyperow',
        'economy.accounting.distributioncode.subtypetext',
        'common.portionprocentage',
        'common.comment',
        'economy.accounting.distributioncode.diffValidation',
        'economy.accounting.distributioncode.sumperiods',
        'economy.accounting.distributioncode.sumpercent',
        'economy.accounting.distributioncode.diff',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const shareColumnName = terms['common.portionprocentage'];
        this.grid.addColumnText('number', terms['common.periodnumber'], {
          flex: 1,
        });
        this.grid.addColumnAutocomplete<DistributionCodeHeadDTO>(
          'parentToDistributionCodePeriodId',
          terms['economy.accounting.salesbudget.subtyperow'],
          {
            width: 500,
            editable: true,
            source: _ => this.subLevelDict,
            optionIdField: 'distributionCodeHeadId',
            optionNameField: 'name',
          }
        );
        this.grid.addColumnText(
          'periodSubTypeName',
          terms['economy.accounting.distributioncode.subtypetext'],
          { flex: 1 }
        );
        this.grid.addColumnNumber('percent', shareColumnName + ' (%)', {
          decimals: 2,
          flex: 1,
          editable: true,
        });
        this.grid.addColumnText('comment', terms['common.comment'], {
          flex: 1,
          editable: true,
        });
        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });

        this.hideColumns();
      });
  }

  hideColumns() {
    if (this.grid) {
      if (this.hideSublevelColumn)
        this.grid.hideColumns(['parentToDistributionCodePeriodId']);
      else this.grid.showColumns(['parentToDistributionCodePeriodId']);

      if (this.hidePeriodInfoColumn)
        this.grid.hideColumns(['periodSubTypeName']);
      else this.grid.showColumns(['periodSubTypeName']);
    }
  }
}
