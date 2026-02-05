import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SoeEntityType } from '@shared/models/generated-interfaces/Enumerations';
import { IExtraFieldGridDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { IAccountDimSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { ExtraFieldsService } from '../../services/extra-fields.service';
import { ExtraFieldsUrlParamsService } from '../../services/extra-fields-url.service';
import { ISearchExtraFieldsGridModel } from './extra-fields-grid-filter/extra-fields-grid-filter.component';
import { MessagingService } from '@shared/services/messaging.service';
import { Constants } from '@shared/util/client-constants';

@Component({
  selector: 'soe-extra-fields-grid',
  templateUrl: './extra-fields-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ExtraFieldsGridComponent
  extends GridBaseDirective<IExtraFieldGridDTO, ExtraFieldsService>
  implements OnInit
{
  service = inject(ExtraFieldsService);
  urlService = inject(ExtraFieldsUrlParamsService);
  messageService = inject(MessagingService);

  isForAccountDim = signal(false);
  showEntityTypeDropDown = false;

  ngOnInit(): void {
    super.ngOnInit();
    const lookups: Observable<any>[] = [this.loadFieldTypes()];

    // EntityType dropdown is hidden in initialization when entity is set from url params

    if (this.urlService.entityType() == 0) {
      this.showEntityTypeDropDown = true;
    } else {
      this.doFilter({ entity: this.urlService.entityType() });
    }

    this.startFlow(
      this.service.getPermission(this.urlService.entityType()),
      'Common.ExtraFields',
      { skipInitialLoad: true, lookups: lookups }
    );
  }

  private loadFieldTypes(): Observable<SmallGenericType[]> {
    return this.service.loadFieldTypes();
  }

  private loadAccountDims(): Observable<IAccountDimSmallDTO[]> {
    return this.service.loadAccountDims();
  }

  override onGridReadyToDefine(grid: GridComponent<IExtraFieldGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.appellation',
        'common.extrafields.fieldtype',
        'common.accountdim',
        'core.edit',
        'common.extrafields.link',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('text', terms['common.appellation'], {
          flex: 1,
        });
        this.grid.addColumnSelect(
          'type',
          terms['common.extrafields.fieldtype'],
          this.service.performFieldTypes.data || [],
          undefined,
          {
            flex: 1,
            enableHiding: true,
          }
        );
        if (this.isForAccountDim()) {
          this.grid.addColumnSelect(
            'accountDimId',
            terms['common.accountdim'],
            this.service.performAccountDims.data || [],
            undefined,
            {
              flex: 1,
              enableHiding: true,
              dropDownIdLabel: 'accountDimId',
              dropDownValueLabel: 'name',
            }
          );
        }
        this.grid.addColumnIcon('', '', {
          width: 22,
          iconName: 'link',
          tooltip: terms['common.extrafields.link'],
          showIcon: row =>
            row.sysExtraFieldId ? row.sysExtraFieldId > 0 : false,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        this.grid.onRowDoubleClicked = row => {
          this.edit(row.data);
        };
        super.finalizeInitGrid();
      });
  }

  doFilter(event: ISearchExtraFieldsGridModel) {
    this.urlService.entityType.set(event.entity);
    this.flowHandler
      .reloadPermission(
        this.service.getPermission(this.urlService.entityType())
      )
      .subscribe(() => {
        if (event.entity == SoeEntityType.None) {
          this.messageService.publish(Constants.EVENT_ENABLE_TAB_ADD, {
            enabled: false,
          });
        }
      });
    if (this.urlService.entityType() == SoeEntityType.Account) {
      this.isForAccountDim.set(true);
      this.loadAccountDims()
        .pipe(
          tap(() => {
            this.refreshGrid();
          })
        )
        .subscribe();
    } else {
      this.isForAccountDim.set(false);
      this.refreshGrid();
    }
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      entity: number;
      loadRecords: boolean;
      connectedEntity: number;
      connectedRecordId: number;
      useCache: boolean;
    }
  ): Observable<IExtraFieldGridDTO[]> {
    return super.loadData(id, {
      entity: this.urlService.entityType(),
      loadRecords: false,
      connectedEntity: 0,
      connectedRecordId: 0,
      useCache: false,
    });
  }
}
