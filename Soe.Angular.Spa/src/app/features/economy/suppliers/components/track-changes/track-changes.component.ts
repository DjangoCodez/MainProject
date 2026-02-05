import {
  Component,
  inject,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITrackChangesLogDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { TrackChangesService } from '@shared/services/track-changes.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { of, take, tap } from 'rxjs';
import {
  TrackChangesForm,
  TrackChangesRowsFilter,
} from './track-changes-form.model';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';

@Component({
  selector: 'soe-track-changes',
  templateUrl: './track-changes.component.html',
  styleUrls: ['./track-changes.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: true,
  imports: [
    SharedModule,
    CommonModule,
    ReactiveFormsModule,
    DatepickerComponent,
    GridWrapperComponent,
    IconButtonComponent,
  ],
})
export class TrackChangesComponent
  extends GridBaseDirective<any>
  implements OnChanges, OnInit
{
  @Input({ required: true }) entityType!: number;
  @Input({ required: true }) entityId!: number;

  validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  form = new TrackChangesForm({
    validationHandler: this.validationHandler,
    element: new TrackChangesRowsFilter(),
  });

  performAction = new Perform<ITrackChangesLogDTO[]>(this.progressService);

  private trackChangesService: TrackChangesService =
    inject(TrackChangesService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Supplier_Suppliers_TrackChanges,
      'Common.Directives.TrackChanges',
      {
        skipInitialLoad: true,
        lookups: [this.executeLookups()],
      }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.grid && (changes.entityType || changes.entityId)) {
      this.rowData.next([]);
      this.loadGridData();
    }
  }

  executeLookups() {
    return of(this.loadGridData());
  }

  loadGridData() {
    const filter = this.form.value as TrackChangesRowsFilter;
    this.performAction.load(
      this.trackChangesService
        .getTrackChangesLog(
          this.entityType,
          this.entityId,
          filter.from.toDateTimeString(),
          filter.to.toDateTimeString()
        )
        .pipe(tap(data => this.rowData.next(data)))
    );
  }

  override onGridReadyToDefine(grid: GridComponent<any>) {
    super.onGridReadyToDefine(grid);
    this.grid.agGrid.api.sizeColumnsToFit(); //Waybe should an attribute directive linked to the grid component?

    this.translate
      .get([
        'common.modifiedby',
        'common.field',
        'common.modified',
        'common.from',
        'common.to',
        'common.type',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('columnText', terms['common.field'], {
          flex: 50,
          enableHiding: false,
        });
        this.grid.addColumnText('actionText', terms['common.type'], {
          flex: 50,
          enableHiding: false,
        });
        this.grid.addColumnText('fromValueText', terms['common.from'], {
          flex: 50,
          enableHiding: false,
        });
        this.grid.addColumnText('toValueText', terms['common.to'], {
          flex: 50,
          enableHiding: false,
        });
        this.grid.addColumnDateTime('created', terms['common.modified'], {
          flex: 50,
          enableHiding: false,
        });
        this.grid.addColumnText('createdBy', terms['common.modifiedby'], {
          flex: 50,
          enableHiding: false,
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();
      });
  }

  moveDatesForward() {
    this.form.moveForward();
    this.loadGridData();
  }

  moveDatesBackward() {
    this.form.moveBackward();
    this.loadGridData();
  }
}
