import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { IIncomingDeliveryTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { IncomingDeliveryTypesService } from '../../services/incoming-delivery-types.service';

@Component({
  selector: 'soe-incoming-delivery-types-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class IncomingDeliveryTypesGridComponent
  extends GridBaseDirective<
    IIncomingDeliveryTypeGridDTO,
    IncomingDeliveryTypesService
  >
  implements OnInit
{
  service = inject(IncomingDeliveryTypesService);
  coreService = inject(CoreService);
  useAccountsHierarchy = signal(false);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_IncomingDeliveryType,
      'Time.Schedule.IncomingDeliveryTypes'
    );
  }

  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([CompanySettingType.UseAccountHierarchy])
      .pipe(
        tap(x => {
          this.useAccountsHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseAccountHierarchy
            )
          );
        })
      );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IIncomingDeliveryTypeGridDTO>
  ) {
    super.onGridReadyToDefine(grid);
    this.translate

      .get([
        'common.name',
        'common.description',
        'common.length',
        'time.schedule.incomingdeliverytype.nbrofpersons',
        'common.user.attestrole.accounthierarchy',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 30,
        });
        this.grid.addColumnNumber('length', terms['common.length'], {
          flex: 10,
          enableHiding: true,
        });
        this.grid.addColumnNumber(
          'nbrOfPersons',
          terms['time.schedule.incomingdeliverytype.nbrofpersons'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        if (this.useAccountsHierarchy()) {
          this.grid.addColumnText(
            'accountName',
            terms['common.user.attestrole.accounthierarchy'],
            {
              flex: 30,
              enableHiding: true,
            }
          );
        }
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        super.finalizeInitGrid();
      });
  }
}
