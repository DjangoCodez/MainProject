import { Component, inject, signal, OnInit } from '@angular/core';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { IStaffingNeedsLocationGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { StaffingNeedsLocationGroupsEditComponent } from '../staffing-needs-location-groups-edit/staffing-needs-location-groups-edit.component';
import { StaffingNeedsLocationGroupsForm } from '../../models/staffing-needs-location-group-form.model';
import { StaffingNeedsLocationsService } from '../../services/staffing-needs-locations.service';

@Component({
  selector: 'soe-staffing-needs-locations-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StaffingNeedsLocationsGridComponent
  extends GridBaseDirective<
    IStaffingNeedsLocationGridDTO,
    StaffingNeedsLocationsService
  >
  implements OnInit
{
  service = inject(StaffingNeedsLocationsService);
  coreService = inject(CoreService);
  useAccountsHierarchy = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_NeedsSettings_Locations,
      'Time.Schedule.StaffingNeedsLocations'
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
    grid: GridComponent<IStaffingNeedsLocationGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.externalcode',
        'common.group',
        'common.user.attestrole.accounthierarchy',
        'common.description',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
        });
        this.grid.addColumnText('externalCode', terms['common.externalcode'], {
          flex: 20,
        });
        this.grid.addColumnText('groupName', terms['common.group'], {
          flex: 20,
          buttonConfiguration: {
            iconName: 'pen',
            iconPrefix: 'fal',
            onClick: r => {
              this.edit(
                {
                  ...r,
                  staffingNeedsLocationGroupId: r.groupId,
                  name: r.groupName,
                },
                {
                  filteredRows: [],
                  editComponent: StaffingNeedsLocationGroupsEditComponent,
                  editTabLabel:
                    'time.schedule.staffingneedslocationgroup.staffingneedslocationgroup',
                  FormClass: StaffingNeedsLocationGroupsForm,
                }
              );
            },
          },
        });
        if (this.useAccountsHierarchy()) {
          this.grid.addColumnText(
            'groupAccountName',
            terms['common.user.attestrole.accounthierarchy'],
            {
              flex: 20,
            }
          );
        }
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 40,
        });
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
