import { Component, inject, signal, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  TermGroup,
  TermGroup_EmployeeRequestStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { IEmployeeRequestGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, Observable, take, tap } from 'rxjs';
import { AbsenceService } from '../../services/absence.service';
import { MultiValueCellRenderer } from '@ui/grid/cell-renderers/multi-value-cell-renderer/multi-value-cell-renderer.component';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

enum SelectItems {
  All = 1,
  Preliminary = 2,
  Definitive = 3,
}
@Component({
  selector: 'soe-absence-requests-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AbsenceRequestsGridComponent
  extends GridBaseDirective<IEmployeeRequestGridDTO, AbsenceService>
  implements OnInit
{
  readonly service = inject(AbsenceService);
  private readonly coreService = inject(CoreService);
  // timeDeviationCauseService = inject(TimeDeviationCausesService);

  // Data
  private statusDict: SmallGenericType[] = [];
  private resultStatusDict: SmallGenericType[] = [];
  private selectItems: SmallGenericType[] = [];

  // Flags
  private useAccountsHierarchy = signal(false);
  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_AbsenceRequests,
      'Time.Schedule.AbsenceRequests',
      {
        lookups: [
          this.getTimeDeviationCauses(),
          this.getResultStatusItems(),
          this.getStatusItems(),
        ],
      }
    );

    this.createSelectItems();
    // this.setRecordStatusTypeIcon(this.rowData.);

    console.log(SoeConfigUtil.getCustomValue('feature'));
    console.log(Feature.Time_Schedule_AbsenceRequests);
    console.log(SoeConfigUtil.feature);
  }

  override createGridToolbar(config?: Partial<ToolbarGridConfig>): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarSelect('loadPreliminary', {
          items: signal(this.selectItems),
          onValueChanged: event => this.loadSettingChanged(event?.value),
          initialSelectedId: signal(SelectItems.All),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IEmployeeRequestGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.addContextMenu({}, [
      this.grid.contextMenuService.editButton({
        action: params => {
          this.edit(params?.node?.data as IEmployeeRequestGridDTO);
        },
      }),
    ]);
    this.translate
      .get([
        'common.employee',
        'core.edit',
        'common.time.timedeviationcause',
        'common.from',
        'common.to',
        'core.created',
        'common.status',
        'time.schedule.absencerequests.result',
        'time.employee.employee.categories',
        'time.employee.employee.accountswithdefault',
        'common.note',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnIcon('rowIcon', '', {
          useIconFromField: true,
          showIcon: () => true,
          editable: false,
          enableHiding: false,
          // iconClassField: 'rowIconClass',
          // iconAnimationField: 'rowIconAnimation',
        });
        this.grid.addColumnText('employeeName', terms['common.employee'], {
          flex: 15,
          enableGrouping: true,
        });
        this.grid.addColumnSelect(
          'timeDeviationCauseId',
          terms['common.time.timedeviationcause'],
          this.service.performAbsenceDeviationCauses.data || [],
          null,
          {
            flex: 10,
            enableGrouping: true,
          }
        );
        this.grid.addColumnDate('start', terms['common.from'], {
          flex: 10,
          enableGrouping: true,
        });
        this.grid.addColumnDate('stop', terms['common.to'], {
          flex: 10,
          enableGrouping: true,
        });

        this.grid.addColumnSelect(
          'status',
          terms['common.status'],
          this.statusDict || [],
          null,
          {
            flex: 10,
            enableGrouping: true,
          }
        );
        this.grid.addColumnSelect(
          'resultStatus',
          terms['time.schedule.absencerequests.result'],
          this.resultStatusDict || [],
          null,
          {
            flex: 10,
            enableGrouping: true,
          }
        );
        if (this.useAccountsHierarchy()) {
          this.grid.addColumnText(
            'accountNames',
            terms['time.employee.employee.accountswithdefault'],
            {
              flex: 15,
              enableGrouping: true,
              cellRenderer: MultiValueCellRenderer,
              filter: 'agSetColumnFilter',
            }
          );
        } else {
          this.grid.addColumnText(
            'categoryNames',
            terms['time.employee.employee.categories'],
            {
              flex: 15,
              enableGrouping: true,
              cellRenderer: MultiValueCellRenderer,
              filter: 'agSetColumnFilter',
            }
          );
        }
        this.grid.addColumnText('comment', terms['common.note'], {
          flex: 10,
        });
        this.grid.addColumnDate('created', terms['core.created'], {
          flex: 10,
          enableGrouping: true,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        this.grid.useGrouping({
          selectChildren: false,
          groupSelectsFiltered: true,
        });

        super.finalizeInitGrid();
      });
  }

  // Load data
  override loadData(
    id?: number,
    additionalProps?: any
  ): Observable<IEmployeeRequestGridDTO[]> {
    return this.performLoadData
      .load$(this.service.getGrid(id, additionalProps), {
        showDialogDelay: 1000,
      })
      .pipe(
        map(data => {
          data.forEach((d: IEmployeeRequestGridDTO) => this.setRowIcon(d));
          return data;
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

  private getTimeDeviationCauses(): Observable<SmallGenericType[]> {
    return this.service.getTimeDeviationCausesAbsenceDict(false);
  }

  private getStatusItems(
    addEmptyRow: boolean = false,
    skipUnknown: boolean = false
  ): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.EmployeeRequestStatus,
        addEmptyRow,
        skipUnknown
      )
      .pipe(
        tap(x => {
          return (this.statusDict = x);
        })
      );
  }

  private getResultStatusItems(
    addEmptyRow: boolean = false,
    skipUnknown: boolean = false
  ): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.EmployeeRequestResultStatus,
        addEmptyRow,
        skipUnknown
      )
      .pipe(
        tap(x => {
          return (this.resultStatusDict = x);
        })
      );
  }

  // Helper
  private createSelectItems() {
    this.translate
      .get([
        'time.schedule.absencerequests.loadpreliminary',
        'time.schedule.absencerequests.loaddefinitive',
        'time.schedule.absencerequests.loadall',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        return (this.selectItems = [
          {
            id: SelectItems.All,
            name: terms['time.schedule.absencerequests.loadall'],
          },
          {
            id: SelectItems.Preliminary,
            name: terms['time.schedule.absencerequests.loadpreliminary'],
          },
          {
            id: SelectItems.Definitive,
            name: terms['time.schedule.absencerequests.loaddefinitive'],
          },
        ]);
      });
  }

  private loadSettingChanged(value: any) {
    console.log(value);
    switch (value) {
      case SelectItems.All:
        this.service.loadDefinitive.set(true);
        this.service.loadPreliminary.set(true);
        break;
      case SelectItems.Definitive:
        this.service.loadDefinitive.set(true);
        this.service.loadPreliminary.set(false);
        break;
      case SelectItems.Preliminary:
        this.service.loadDefinitive.set(false);
        this.service.loadPreliminary.set(true);
        break;
      default:
        this.service.loadDefinitive.set(true);
        this.service.loadPreliminary.set(true);
        break;
    }
    this.loadData().subscribe();
    this.refreshGrid();
  }

  private setRowIcon(record: IEmployeeRequestGridDTO) {
    let rowIcon = '';
    let rowClass = '';
    let rowAnim = '';
    switch (record.status) {
      case TermGroup_EmployeeRequestStatus.Definate:
        rowIcon = 'check';
        rowClass = '';
        rowAnim = '';
        break;
      case TermGroup_EmployeeRequestStatus.RequestPending:
      case TermGroup_EmployeeRequestStatus.Preliminary:
      case TermGroup_EmployeeRequestStatus.PartlyDefinate:
      case TermGroup_EmployeeRequestStatus.Restored:
        rowIcon = 'clock';
        rowClass = '';
        rowAnim = '';
        break;
      default:
        rowIcon = '';
        rowClass = '';
        rowAnim = '';
        break;
    }
    (record as any)['rowIcon'] = rowIcon;
    (record as any)['rowIconClass'] = rowClass;
    (record as any)['rowIconAnimation'] = rowAnim;
  }
}
