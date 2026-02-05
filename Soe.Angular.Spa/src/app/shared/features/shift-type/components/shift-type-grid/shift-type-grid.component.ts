import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  CompanySettingType,
  Feature,
  TermGroup,
  TermGroup_TimeScheduleTemplateBlockType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IShiftTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { IconUtil } from '@shared/util/icon-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { AccountDimDTO } from '@src/app/features/economy/accounting-coding-levels/models/accounting-coding-levels.model';
import { TimeService } from '@src/app/features/time/services/time.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, Observable, take, tap } from 'rxjs';
import { ShiftTypeParamsService } from '../../services/shift-type-params.service';
import { ShiftTypeService } from '../../services/shift-type.service';

@Component({
  selector: 'soe-shift-type-grid',
  templateUrl: './shift-type-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ShiftTypeGridComponent
  extends GridBaseDirective<IShiftTypeGridDTO, ShiftTypeService>
  implements OnInit
{
  service = inject(ShiftTypeService);
  coreService = inject(CoreService);
  timeService = inject(TimeService);
  progressService = inject(ProgressService);
  urlService = inject(ShiftTypeParamsService);

  performGridLoad = new Perform<IShiftTypeGridDTO[]>(this.progressService);

  private timeScheduleTemplateBlockTypes: ISmallGenericType[] = [];
  private readonly timeScheduleTypeVisible = signal(false);
  private timeScheduleTypes: ISmallGenericType[] = [];
  private readonly useAccountsHierarchy = signal(false);

  isRemoveDisable = signal(true);
  shiftTypeAccountDim!: AccountDimDTO;

  ngOnInit(): void {
    const feature = this.urlService.isOrder
      ? Feature.Billing_Preferences_InvoiceSettings_ShiftType
      : Feature.Time_Preferences_ScheduleSettings_ShiftType;

    super.ngOnInit();
    this.startFlow(feature, 'time.schedule.shifttype.shifttype', {
      lookups: [
        this.loadTimeScheduleTemplateBlockTypes(),
        this.loadTimeScheduleTypes(),
        this.loadShiftTypeAccountDim(),
        this.loadCompanySettings(),
      ],
      useLegacyToolbar: true,
    });
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => this.refreshGrid(),
      },
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'remove'),
          label: 'core.delete',
          title: 'core.delete',
          onClick: () => this.deleteSelectedEntries(),
          disabled: this.isRemoveDisable,
          hidden: signal(false),
        }),
      ],
    });
  }

  private loadTimeScheduleTemplateBlockTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeScheduleTemplateBlockType,
        false,
        false
      )
      .pipe(
        tap(x => {
          this.timeScheduleTemplateBlockTypes = x;
        })
      );
  }

  private loadTimeScheduleTypes(): Observable<ISmallGenericType[]> {
    return this.timeService.getTimeScheduleTypesDict(false, true).pipe(
      tap(x => {
        this.timeScheduleTypes = x;
        if (this.timeScheduleTypes.length > 1)
          this.timeScheduleTypeVisible.set(true);
      })
    );
  }

  private loadShiftTypeAccountDim(): Observable<AccountDimDTO> {
    return this.service.getShiftTypeAccountDim(true, false).pipe(
      tap(x => {
        this.shiftTypeAccountDim = x;
      })
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

  override loadData(
    id?: number | undefined,
    useCache: boolean = true,
    keepDialog: boolean = false
  ): Observable<IShiftTypeGridDTO[]> {
    return this.performGridLoad.load$(
      this.service
        .getGrid(undefined, {
          loadAccounts: false,
          loadSkills: false,
          loadEmployeeStatisticsTargets: false,
          setTimeScheduleTemplateBlockTypeName: true,
          setCategoryNames: true,
          setAccountingString: true,
          setSkillNames: true,
          setTimeScheduleTypeName: true,
          loadHierarchyAccounts: false,
        })
        .pipe(
          map(data => {
            data.forEach((y: IShiftTypeGridDTO) => {
              if (y.color && y.color.length === 9)
                y.color = '#' + y.color.substring(3);
            });

            if (this.urlService.isOrder) {
              data = data.filter(
                x =>
                  x.timeScheduleTemplateBlockType ===
                    TermGroup_TimeScheduleTemplateBlockType.Booking ||
                  x.timeScheduleTemplateBlockType ===
                    TermGroup_TimeScheduleTemplateBlockType.Order
              );
            }
            if (!this.urlService.isOrder) {
              data = data.filter(
                x =>
                  // Don't show Order-types in Shifttype-page
                  x.timeScheduleTemplateBlockType !==
                  TermGroup_TimeScheduleTemplateBlockType.Order
              );
            }
            return data;
          })
        ),
      { keepExistingDialog: keepDialog }
    );
  }

  private deleteSelectedEntries() {
    let ids = '';
    const selectedIds = this.grid.getSelectedIds('shiftTypeId');

    selectedIds.forEach(f => {
      if (this.grid.getSelectedCount() == 1) {
        this.performGridLoad.crud(
          CrudActionTypeEnum.Delete,
          this.service.delete(f),
          () => this.refreshGrid(),
          undefined,
          {}
        );
      }
      if (this.grid.getSelectedCount() > 1)
        ids = ids != '' ? (ids = ids + ',' + f) : f.toString();
    });

    if (this.grid.getSelectedCount() > 1) {
      this.performGridLoad.crud(
        CrudActionTypeEnum.Delete,
        this.service.bulkDelete(ids),
        () => this.refreshGrid(),
        () => this.loadData(undefined, true, true).subscribe(),
        { showDialogOnComplete: true, showDialogOnError: true }
      );
    }
  }

  triggerSelectedItemCalc() {
    if (this.grid.getSelectedCount() > 0) this.isRemoveDisable.set(false);
    else this.isRemoveDisable.set(true);
  }

  override onGridReadyToDefine(grid: GridComponent<IShiftTypeGridDTO>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'core.delete',
        'core.deleteselectedwarning',
        'core.edit',
        'common.accounting',
        'common.categories',
        'common.code',
        'common.color',
        'common.customer.invoices.noshifttype',
        'common.description',
        'common.name',
        'common.number',
        'common.skills.skills',
        'common.type',
        'time.schedule.scheduletype.scheduletype',
        'time.schedule.shifttype.linkedtoaccount',
        'time.schedule.shifttype.externalcode',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnSelect(
          'timeScheduleTemplateBlockType',
          terms['common.type'],
          this.timeScheduleTemplateBlockTypes || [],
          undefined,
          {
            flex: 5,
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            enableHiding: true,
            editable: false,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 12,
        });
        this.grid.addColumnText(
          'externalCode',
          terms['time.schedule.shifttype.externalcode'],
          {
            flex: 5,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 18,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'needsCode',
          this.shiftTypeAccountDim
            ? terms['common.number']
            : terms['common.code'],
          {
            enableHiding: true,
            flex: 5,
          }
        );
        if (this.timeScheduleTypeVisible())
          this.grid.addColumnSelect(
            'timeScheduleTypeId',
            terms['time.schedule.scheduletype.scheduletype'],
            this.timeScheduleTypes || [],
            undefined,
            {
              dropDownIdLabel: 'id',
              dropDownValueLabel: 'name',
              enableHiding: true,
              editable: false,
              enableGrouping: true,
              flex: 10,
            }
          );
        if (!this.useAccountsHierarchy())
          this.grid.addColumnText('categoryNames', terms['common.categories'], {
            enableGrouping: true,
            enableHiding: true,
            flex: 15,
          });
        this.grid.addColumnText('skillNames', terms['common.skills.skills'], {
          enableGrouping: true,
          enableHiding: true,
          flex: 15,
        });
        this.grid.addColumnText(
          'accountingStringAccountNames',
          terms['common.accounting'],
          { enableGrouping: true, enableHiding: true, flex: 15 }
        );
        this.grid.addColumnShape('color', terms['common.color'], {
          width: 100,
          enableHiding: true,
          shape: 'rectangle',
          colorField: 'color',
          tooltipField: 'color',
        });
        if (this.shiftTypeAccountDim)
          this.grid.addColumnIcon('', '', {
            tooltip: terms['time.schedule.shifttype.linkedtoaccount'],
            iconName: 'link',
            showIcon: row => !!(row && row.accountId),
            enableHiding: true,
            width: 22,
          });

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        this.grid.useGrouping({
          selectChildren: true,
          groupSelectsFiltered: true,
        });
        super.finalizeInitGrid();
      });
  }

  setFilteredData(showShiftTypesWithInactiveAccounts: boolean) {
    this.service.showShiftTypesWithInactiveAccounts.set(
      showShiftTypesWithInactiveAccounts
    );
    this.loadData().subscribe();
    this.refreshGrid();
  }
}
