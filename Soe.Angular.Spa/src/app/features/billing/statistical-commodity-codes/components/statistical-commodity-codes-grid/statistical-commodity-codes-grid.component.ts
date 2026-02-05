import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ICommodityCodeDTO } from '@shared/models/generated-interfaces/CommodityCodeDTO';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ProgressOptions } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { Dict } from '@ui/grid/services/selected-item.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { StatisticalCommodityCodesService } from '../../services/statistical-commodity-codes.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-statistical-commodity-codes-grid',
  templateUrl: './statistical-commodity-codes-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StatisticalCommodityCodesGridComponent
  extends GridBaseDirective<ICommodityCodeDTO, StatisticalCommodityCodesService>
  implements OnInit
{
  service = inject(StatisticalCommodityCodesService);
  progressService = inject(ProgressService);
  saveButtonDisabled = signal(true);
  performAction = new Perform<any>(this.progressService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Intrastat_Administer,
      'common.commoditycodes.codes',
      { useLegacyToolbar: true }
    );
  }

  selectedItemsChanged($event: Dict): void {
    this.saveButtonDisabled.set(Object.keys($event.dict).length <= 0);
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => this.refreshGrid(),
      },
      saveOption: {
        onClick: () => {
          this.saveStatus();
        },
        disabled: this.saveButtonDisabled,
      },
    });
  }

  saveStatus(options?: ProgressOptions) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(this.grid.selectedItemsService.toDict()).pipe(
        tap((backendResponse: BackendResponse) => {
          if (backendResponse.success) this.refreshGrid();
        })
      ),
      undefined,
      undefined,
      options
    );
  }

  onGridReadyToDefine(grid: GridComponent<ICommodityCodeDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.active',
        'common.code',
        'common.description',
        'core.edit',
        'common.commoditycodes.otherquantity',
        'common.startdate',
        'common.stopdate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('isActive', terms['common.active'], {
          editable: true,
          idField: 'sysIntrastatCodeId',
        });
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 40,
        });
        this.grid.addColumnText('text', terms['common.description'], {
          flex: 60,
        });
        this.grid.addColumnBool(
          'useOtherQuantity',
          terms['common.commoditycodes.otherquantity']
        );
        this.grid.addColumnDate('startDate', terms['common.startdate'], {
          flex: 20,
        });
        this.grid.addColumnDate('endDate', terms['common.stopdate'], {
          flex: 20,
        });

        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { onlyActive: boolean }
  ): Observable<ICommodityCodeDTO[]> {
    return super.loadData(id, { onlyActive: false });
  }
}
