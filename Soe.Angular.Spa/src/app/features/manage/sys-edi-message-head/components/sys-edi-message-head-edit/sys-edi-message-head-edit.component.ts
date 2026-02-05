import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnInit,
  Output,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SysCompanyService } from '@src/app/features/manage/system/sys-company/services/sys-company.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { BehaviorSubject, Observable, of, take, tap } from 'rxjs';
import { SysEdiMessageHeadForm } from '../../models/sys-edi-message-head-form.model';
import {
  ISysEdiMessageRowDTO,
  SysEdiMessageHeadDTO,
} from '../../models/sys-edi-message-head.model';
import { SysEdiMessageHeadService } from '../../services/sys-edi-message-head.service';

@Component({
  selector: 'soe-sys-edi-message-head-edit',
  templateUrl: './sys-edi-message-head-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SysEdiMessageHeadEditComponent
  extends EditBaseDirective<
    SysEdiMessageHeadDTO,
    SysEdiMessageHeadService,
    SysEdiMessageHeadForm
  >
  implements OnInit
{
  @Input() form: SysEdiMessageHeadForm | undefined;
  @Output() onGridDefined = new EventEmitter<
    GridComponent<ISysEdiMessageRowDTO>
  >();
  service = inject(SysEdiMessageHeadService);
  sysCompanyService = inject(SysCompanyService);
  sysCompanies: ISmallGenericType[] = [];
  gridEdiMessageRows!: GridComponent<ISysEdiMessageRowDTO>;
  rowData = new BehaviorSubject<ISysEdiMessageRowDTO[]>([]);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Manage_System, {
      lookups: this.loadSysCompanies(),
    });
  }
  onCellValueChanged(event: CellValueChangedEvent): void {
    this.form?.setDirtyOnEdiMessageRowChange(event.data.sysEdiMessageRowId);
  }

  setupGrid(grid: GridComponent<ISysEdiMessageRowDTO>) {
    this.gridEdiMessageRows = grid;
    this.translate
      .get([
        'SellerArticleNumber',
        'SellerArticleDescription1',
        'SellerArticleDescription2',
        'SellerRowNumber',
        'BuyerArticleNumber',
        'BuyerRowNumber',
        'DeliveryDate',
        'BuyerReference',
        'BuyerObjectId',
        'Quantity',
        'UnitCode',
        'UnitPrice',
        'DiscountPercent',
        'DiscountAmount',
        'DiscountPercent1',
        'DiscountAmount1',
        'DiscountPercent2',
        'DiscountAmount2',
        'NetAmount',
        'VatAmount',
        'VatPercentage',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.gridEdiMessageRows.addColumnText(
          'rowSellerArticleNumber',
          terms['SellerArticleNumber'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowSellerArticleDescription1',
          terms['SellerArticleDescription1'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowSellerArticleDescription2',
          terms['SellerArticleDescription2'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowSellerRowNumber',
          terms['SellerRowNumber'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowBuyerArticleNumber',
          terms['BuyerArticleNumber'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowBuyerRowNumber',
          terms['BuyerRowNumber'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowDeliveryDate',
          terms['DeliveryDate'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowBuyerReference',
          terms['BuyerReference'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowBuyerObjectId',
          terms['BuyerObjectId'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowQuantity',
          terms['Quantity'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowUnitCode',
          terms['UnitCode'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowUnitPrice',
          terms['UnitPrice'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowDiscountPercent',
          terms['DiscountPercent'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowDiscountAmount',
          terms['DiscountAmount'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowDiscountPercent1',
          terms['DiscountPercent1'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowDiscountAmount1',
          terms['DiscountAmount1'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowDiscountPercent2',
          terms['DiscountPercent2'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowDiscountAmount2',
          terms['DiscountAmount2'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowNetAmount',
          terms['NetAmount'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowVatAmount',
          terms['VatAmount'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'rowVatPercentage',
          terms['VatPercentage'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnText(
          'productNumber',
          terms['billing.stock.stockinventory.productnr'],
          { flex: 1, editable: true }
        );
        this.gridEdiMessageRows.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.rowDelete(row);
          },
        });
        this.gridEdiMessageRows.finalizeInitGrid();
        this.onGridDefined.emit(this.gridEdiMessageRows);
        this.setGridData(this.form?.value.sysEdiEdiMessageRowDTOs);
      });
  }

  setGridData(rows: ISysEdiMessageRowDTO[]) {
    this.gridEdiMessageRows.setData(rows);
  }
  rowDelete(row: ISysEdiMessageRowDTO): void {}

  loadSysCompanies(): Observable<ISmallGenericType[]> {
    return this.sysCompanyService
      .getSysCompanyDict()
      .pipe(tap(x => (this.sysCompanies = x)));
  }

  loadData(): Observable<void> {
    return of(
      this.performLoadData.load(
        this.service.get(this.form?.getIdControl()?.value).pipe(
          tap(value => {
            this.form?.reset(value);
            this.form?.customPatchValue(value);
            if (this.gridEdiMessageRows) {
              this.setGridData(value.sysEdiEdiMessageRowDTOs);
            }
          })
        )
      )
    );
  }
}
