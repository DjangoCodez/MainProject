import { Component, inject, Input, input, OnInit, signal } from '@angular/core';
import { EgAttestTransitionsForm } from '@features/time/employee-groups/models/eg-attest-transitions-form.model';
import { EmployeeGroupsForm } from '@features/time/employee-groups/models/employee-groups-form.model';
import { EmployeeGroupsService } from '@features/time/employee-groups/services/employee-groups.service';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  TermGroup_AttestEntity,
} from '@shared/models/generated-interfaces/Enumerations';
import { IEmployeeGroupAttestTransitionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';

@Component({
  selector: 'soe-eg-attest-transitions-grid',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EgAttestTransitionsGridComponent
  extends EmbeddedGridBaseDirective<
    IEmployeeGroupAttestTransitionDTO,
    EmployeeGroupsForm
  >
  implements OnInit
{
  @Input({ required: true }) form!: EmployeeGroupsForm;
  noMargin = input(true);
  height = input(60);

  employeeGroupService = inject(EmployeeGroupsService);

  payrollTimeTransitions: SmallGenericType[] = [];
  invoiceTimeTransitions: SmallGenericType[] = [];

  entities: SmallGenericType[] = [];

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Groups_Edit,
      'time.employee.employeegroups.attest-transitions',
      {
        skipInitialLoad: true,
        lookups: [
          this.loadPayrollTimeAttestTransitions(),
          this.loadAttestEntities(),
          this.loadInvoiceTimeAttestTransitions(),
        ],
      }
    );
    this.form.attestTransition.valueChanges.subscribe(v => this.initRows(v));
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    super.createGridToolbar();
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal(
            'time.employee.employeegroup.attest.validattesttransitions'
          ),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<IEmployeeGroupAttestTransitionDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.type',
        'time.employee.employeegroup.transition',
        'core.permission',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'entity',
          terms['common.type'],
          this.entities || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 50,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnSelect(
          'attestTransitionId',
          terms['time.employee.employeegroup.transition'],
          [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 50,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
            dynamicSelectOptions: (row: any) => {
              const entity = row?.data?.entity || 0;
              if (entity == TermGroup_AttestEntity.InvoiceTime) {
                return this.invoiceTimeTransitions;
              } else if (entity == TermGroup_AttestEntity.PayrollTime) {
                return this.payrollTimeTransitions;
              } else return [];
            },
          }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
          suppressFilter: true,
          suppressFloatingFilter: true,
        });

        this.grid.setNbrOfRowsToShow(1, 10);
        this.grid.context.suppressFiltering = true;
        super.finalizeInitGrid({ hidden: true });
      });
  }
  private initRows(rows: IEmployeeGroupAttestTransitionDTO[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: Partial<IEmployeeGroupAttestTransitionDTO> = {
      attestTransitionId: 0,
      entity: 0,
    };
    super.addRow(
      row as IEmployeeGroupAttestTransitionDTO,
      this.form.attestTransition,
      EgAttestTransitionsForm
    );
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.attestTransition);
  }

  //Load Data

  loadPayrollTimeAttestTransitions(): Observable<SmallGenericType[]> {
    return this.employeeGroupService
      .getAttestTransitionsDict(TermGroup_AttestEntity.PayrollTime)
      .pipe(tap(x => (this.payrollTimeTransitions = x)));
  }

  loadInvoiceTimeAttestTransitions(): Observable<SmallGenericType[]> {
    return this.employeeGroupService
      .getAttestTransitionsDict(TermGroup_AttestEntity.InvoiceTime)
      .pipe(tap(x => (this.invoiceTimeTransitions = x)));
  }

  loadAttestEntities(): Observable<SmallGenericType[]> {
    return this.employeeGroupService
      .getAttestEntitiesGenericList(false)
      .pipe(tap(x => (this.entities = x)));
  }
}
