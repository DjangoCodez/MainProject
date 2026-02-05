import { Component, Input, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  Feature,
  TermGroup_AccountStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, take } from 'rxjs';
import { VoucherSeriesTypeDTO } from 'src/app/features/economy/models/voucher-series-type.model';
import { VoucherSeriesDTO } from '../../../../models/account-years-and-periods.model';
import { VoucherSeriesRowsForm } from '../../../../models/voucher-series-type-rows-form.model';

@Component({
  selector: 'soe-voucher-series-type-grid',
  templateUrl: './voucher-series-type-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class VoucherSeriesTypeGridComponent
  extends GridBaseDirective<VoucherSeriesDTO>
  implements OnInit
{
  @Input() rows!: BehaviorSubject<VoucherSeriesDTO[]>;
  @Input() voucherSeriesTypes: BehaviorSubject<VoucherSeriesTypeDTO[]> =
    new BehaviorSubject<VoucherSeriesTypeDTO[]>([]);
  @Input() isNew!: boolean;
  @Input() status!: number;
  @Input() headForm!: SoeFormGroup;

  validationHandler = inject(ValidationHandler);
  filteredVoucherSeriesTypes: BehaviorSubject<VoucherSeriesTypeDTO[]> =
    new BehaviorSubject<VoucherSeriesTypeDTO[]>([]);
  form: VoucherSeriesRowsForm = new VoucherSeriesRowsForm({
    validationHandler: this.validationHandler,
    element: new VoucherSeriesDTO(),
  });
  gridNumberArray: number[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_Vouchers_Edit,
      'Common.Customer.AccountYear.VoucherSeries',
      { skipInitialLoad: true }
    );
    if (this.isNew) this.headForm.markAsDirty();
  }

  getGridIds() {
    this.rows.subscribe(rows => {
      rows.forEach(row => {
        this.gridNumberArray.push(row.voucherSeriesTypeId);
      });
    });
  }

  override onGridReadyToDefine(grid: GridComponent<VoucherSeriesDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'economy.accounting.voucherseriestype.voucherseriestypenr',
        'economy.accounting.accountyear.startnumber',
        'economy.accounting.accountyear.lastnumber',
        'economy.accounting.accountyear.lastvoucherdate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');
        this.grid.addColumnNumber(
          'voucherSeriesTypeNr',
          terms['economy.accounting.voucherseriestype.voucherseriestypenr'],
          {
            flex: 1,
          }
        ),
          this.grid.addColumnText(
            'voucherSeriesTypeName',
            terms['common.period'],
            {
              flex: 1,
            }
          ),
          this.grid.addColumnNumber(
            'startNr',
            terms['economy.accounting.accountyear.startnumber'],
            {
              flex: 1,
            }
          ),
          this.grid.addColumnNumber(
            'voucherNrLatest',
            terms['economy.accounting.accountyear.lastnumber'],
            {
              flex: 1,
              clearZero: true,
              editable: row => {
                return (
                  (row.data &&
                    !row.data.voucherDateLatest &&
                    this.status < TermGroup_AccountStatus.Closed) ||
                  false
                );
              },
            }
          ),
          this.grid.addColumnDate(
            'voucherDateLatest',
            terms['economy.accounting.accountyear.lastvoucherdate'],
            {
              flex: 1,
            }
          ),
          this.grid.addColumnIconDelete({
            tooltip: terms['core.deleterow'],
            showIcon: row => row.isModified,
            onClick: row => {
              this.delete(row as VoucherSeriesDTO);
            },
          });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
        if (this.isNew) this.generateNewVoucherTypes();
      });
  }

  override onFinished(): void {
    this.getGridIds();
    this.generateDropdownList();
  }

  private delete(row: VoucherSeriesDTO) {
    row.isDeleted = true;
    this.grid.deleteRow(row);
    this.rows.next(this.grid.agGrid.rowData ?? []);
    this.headForm.markAsDirty();

    this.gridNumberArray = this.gridNumberArray.filter(
      obj => obj !== row.voucherSeriesTypeId
    );
    this.generateDropdownList();
  }

  public addVoucherSeries() {
    let voucherSeriesGrid: VoucherSeriesDTO[] = [];
    voucherSeriesGrid = this.rows.value;

    const newS = this.voucherSeriesTypes.value.find(
      v => v.voucherSeriesTypeId === this.form.value.voucherSeriesTypes
    );
    if (newS) {
      const newRow = new VoucherSeriesDTO();
      newRow.isModified = true;
      newRow.voucherSeriesTypeId = newS.voucherSeriesTypeId;
      newRow.voucherSeriesTypeName = newS.name;
      newRow['voucherSeriesTypeNr'] = newS.voucherSeriesTypeNr;
      newRow['startNr'] = newS.startNr;

      voucherSeriesGrid.push(newRow);

      this.gridNumberArray.push(newRow.voucherSeriesTypeId);
      this.rows.next(voucherSeriesGrid);
      this.generateDropdownList();
      this.headForm.markAsDirty();
      if (this.filteredVoucherSeriesTypes)
        this.form.patchValue({
          voucherSeriesTypes:
            this.filteredVoucherSeriesTypes.value[0].voucherSeriesTypeId,
        });
    }
  }

  generateNewVoucherTypes() {
    this.rows.next([]);
    const voucherSeries: VoucherSeriesDTO[] = [];

    this.voucherSeriesTypes.asObservable().subscribe(types => {
      types?.forEach(type => {
        const serie = new VoucherSeriesDTO();
        serie.voucherSeriesTypeId = type.voucherSeriesTypeId;
        serie.voucherSeriesTypeName = type.name;
        serie['voucherSeriesTypeNr'] = type.voucherSeriesTypeNr;
        serie['startNr'] = type.startNr;
        serie.isModified = true;
        voucherSeries.push(serie);
      });
      this.rows.next(voucherSeries);
    });
  }

  generateDropdownList() {
    let existingValuesList: VoucherSeriesTypeDTO[] = [];

    if (this.gridNumberArray)
      existingValuesList = this.voucherSeriesTypes.value.filter(x => {
        return !this.gridNumberArray.some(t => x.voucherSeriesTypeId === t);
      });
    else existingValuesList = this.voucherSeriesTypes.value;
    this.filteredVoucherSeriesTypes.next(existingValuesList);
  }
}
