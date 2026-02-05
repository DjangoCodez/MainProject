import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { IIncomingDeliveryGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs';
import { IncomingDeliveriesService } from '../../services/incoming-deliveries.service';

@Component({
  selector: 'soe-incoming-deliveries-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class IncomingDeliveriesGridComponent
  extends GridBaseDirective<IIncomingDeliveryGridDTO, IncomingDeliveriesService>
  implements OnInit
{
  // Services
  service = inject(IncomingDeliveriesService);
  private readonly coreService = inject(CoreService);
  private readonly progress = inject(ProgressService);
  private readonly performLoad = new Perform(this.progress);

  // Company settings
  useAccountsHierarchy = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries,
      'Time.Schedule.IncomingDeliveries'
    );
  }

  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([CompanySettingType.UseAccountHierarchy])
      .pipe(
        tap((x: any) => {
          this.useAccountsHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseAccountHierarchy
            )
          );
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<IIncomingDeliveryGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.dailyrecurrencepattern',
        'common.dailyrecurrencepattern.rangetype',
        'common.dailyrecurrencepattern.startdate',
        'common.description',
        'common.name',
        'common.user.attestrole.accounthierarchy',
        'core.edit',
        'time.schedule.incomingdelivery.row.length',
        'time.schedule.incomingdelivery.row.nbrofpackages',
        'time.schedule.incomingdelivery.row.offsetdays',
        'time.schedule.incomingdelivery.row.starttime',
        'time.schedule.incomingdelivery.row.stoptime',
        'time.schedule.incomingdeliverytype.incomingdeliverytype',
        'time.schedule.shifttype.shifttype',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        // Master
        this.grid.addColumnText('name', terms['common.name'], { flex: 20 });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 25,
        });
        if (this.useAccountsHierarchy()) {
          this.grid.addColumnText(
            'accountName',
            terms['common.user.attestrole.accounthierarchy'],
            {
              flex: 20,
              enableHiding: true,
            }
          );
        }
        this.grid.addColumnText(
          'recurrenceStartsOnDescription',
          terms['common.dailyrecurrencepattern.startdate'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'recurrenceEndsOnDescription',
          terms['common.dailyrecurrencepattern.rangetype'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'recurrencePatternDescription',
          terms['common.dailyrecurrencepattern'],
          {
            flex: 25,
            enableHiding: true,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        // Detail
        this.grid.enableMasterDetail(
          {
            // Only show details expander if it has any data
            isRowMaster: (dataItem: IIncomingDeliveryGridDTO) => {
              return dataItem.hasRows;
            },

            columnDefs: [
              ColumnUtil.createColumnText('name', terms['common.name'], {
                flex: 10,
              }),
              ColumnUtil.createColumnText(
                'description',
                terms['common.description'],
                {
                  flex: 20,
                }
              ),
              ColumnUtil.createColumnText(
                'shiftTypeName',
                terms['time.schedule.shifttype.shifttype'],
                {
                  flex: 20,
                }
              ),
              ColumnUtil.createColumnText(
                'typeName',
                terms[
                  'time.schedule.incomingdeliverytype.incomingdeliverytype'
                ],
                {
                  flex: 20,
                }
              ),
              ColumnUtil.createColumnNumber(
                'nbrOfPackages',
                terms['time.schedule.incomingdelivery.row.nbrofpackages'],
                {
                  flex: 10,
                }
              ),
              ColumnUtil.createColumnNumber(
                'offsetDays',
                terms['time.schedule.incomingdelivery.row.offsetdays'],
                {
                  flex: 10,
                }
              ),
              ColumnUtil.createColumnTime(
                'startTime',
                terms['time.schedule.incomingdelivery.row.starttime'],
                {
                  flex: 10,
                }
              ),
              ColumnUtil.createColumnTime(
                'stopTime',
                terms['time.schedule.incomingdelivery.row.stoptime'],
                {
                  flex: 10,
                }
              ),
              ColumnUtil.createColumnNumber(
                'length',
                terms['time.schedule.incomingdelivery.row.length'],
                {
                  flex: 10,
                }
              ),
            ],
          },
          {
            addDefaultExpanderCol: false,
            getDetailRowData: this.loadDetailRows.bind(this),
          }
        );

        super.finalizeInitGrid();
      });
  }

  loadDetailRows(params: any) {
    if (!params.data['detailsLoaded']) {
      this.performLoad.load(
        this.service.getRows(params.data.incomingDeliveryHeadId).pipe(
          tap(data => {
            params.data['detailsRows'] = data;
            params.data['detailsLoaded'] = true;
            params.successCallback(data);
          })
        )
      );
    } else {
      params.successCallback(params.data['detailsRows']);
    }
  }
}
