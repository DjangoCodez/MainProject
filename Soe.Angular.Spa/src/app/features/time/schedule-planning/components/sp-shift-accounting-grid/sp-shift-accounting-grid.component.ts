import { Component, inject, input, model, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { ShiftAccountingService } from '../../services/sp-shift-accounting.service';
import { IShiftAccountingRowDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { CoreService } from '@shared/services/core.service';
import { IAccountDimSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ProgressService } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'sp-shift-accounting-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  imports: [GridWrapperComponent],
  standalone: true,
})
export class SpShiftAccountingGridComponent
  extends GridBaseDirective<IShiftAccountingRowDTO, ShiftAccountingService>
  implements OnInit
{
  columnHeaders: { [key: number]: string } = {};
  height = model(200);
  shiftIds = input<number[] | undefined>(undefined);
  service = inject(ShiftAccountingService);
  coreService = inject(CoreService);
  readonly progress = inject(ProgressService);
  readonly performLoadAccount = new Perform<IAccountDimSmallDTO[]>(
    this.progress
  );

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_SchedulePlanning_Beta, // TODO: Change when released
      'Time.Schedule.ShiftAccounting',
      {
        lookups: [this.loadAccountDims(true)],
      }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IShiftAccountingRowDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['time.schedule.planning.shift'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'identityName',
          terms['time.schedule.planning.shift'],
          {
            flex: 20,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('dim2Name', this.columnHeaders[0] || '', {
          flex: 20,
        });
        this.grid.addColumnText('dim3Name', this.columnHeaders[1] || '', {
          flex: 20,
        });
        this.grid.addColumnText('dim4Name', this.columnHeaders[2] || '', {
          flex: 20,
        });
        this.grid.addColumnText('dim5Name', this.columnHeaders[3] || '', {
          flex: 20,
        });
        this.grid.addColumnText('dim6Name', this.columnHeaders[4] || '', {
          flex: 20,
        });

        this.grid.height = this.height;
        this.grid.setNbrOfRowsToShow(3, 10);
        super.finalizeInitGrid();
      });
  }

  private loadAccountDims(
    useCache: boolean
  ): Observable<IAccountDimSmallDTO[]> {
    return this.performLoadAccount.load$(
      this.coreService
        .getAccountDimsSmall(
          false,
          true,
          true,
          false,
          true,
          false,
          false,
          true,
          useCache,
          false,
          0,
          false
        )
        .pipe(
          tap(x => {
            this.buildDimColumns(x);
          })
        )
    );
  }

  private buildDimColumns(dims: IAccountDimSmallDTO[]) {
    let index: number = 0;
    dims.sort((a, b) => a.accountDimNr - b.accountDimNr);
    dims.forEach(dim => {
      this.columnHeaders[index] = dim.name || '';
      index++;
    });
  }

  override loadData(
    id?: number,
    additionalProps?: any
  ): Observable<IShiftAccountingRowDTO[]> {
    return this.service.getGrid(id, this.shiftIds());
  }
}
