import { Component, computed, input, Input, signal } from '@angular/core';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { IAvailableEmployeesDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { SpShiftRequestDialogForm } from '../sp-shift-request-dialog-form.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { FlowHandlerService } from '@shared/services/flow-handler.service';

@Component({
  selector: 'sp-shift-request-recipients-available',
  imports: [GridWrapperComponent],
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  styleUrl: './sp-shift-request-recipients.component.scss',
  providers: [FlowHandlerService, ToolbarService],
})
export class SpShiftRequestRecipientsAvailableComponent extends EmbeddedGridBaseDirective<
  IAvailableEmployeesDTO,
  SpShiftRequestDialogForm
> {
  @Input({ required: true }) form!: SpShiftRequestDialogForm;
  noMargin = input(true);
  toolbarNoPadding = input(true);
  height = input(310);

  disableAddButton = computed(() => this.selectedRows().length === 0);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'shift.request.recipients.available', {});

    this.form.availableEmployees.valueChanges.subscribe(v => {
      this.rowData.next(v);
    });
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    super.createGridToolbar({ hideNew: true });
    this.toolbarService.createItemGroup({
      alignLeft: false,
      items: [
        this.toolbarService.createToolbarButton('add', {
          caption: signal('common.receiverslist.move'),
          iconName: signal('chevrons-right'),
          disabled: this.disableAddButton,
          onAction: () => this.moveSelected(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<IAvailableEmployeesDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    // TODO: New term
    this.translate
      .get(['common.employee'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('employeeName', 'Tillgängliga', {
          resizable: false,
          sort: 'asc',
          flex: 100,
        });

        this.grid.context.suppressGridMenu = true;
        this.grid.setRowHeight(22);
        this.grid.enableRowSelection();
        super.finalizeInitGrid({ hidden: true });
      });
  }

  private moveSelected(): void {
    const selected = this.selectedRows();
    const newAvailable = this.form.availableEmployees.value.filter(
      ae => !selected.find(s => s.employeeId === ae.employeeId)
    );
    const currentSelected = this.form.selectedEmployees.value;
    const newSelected = [...currentSelected, ...selected];
    this.form.patchSelectedEmployees(newSelected);
    this.form.patchAvailableEmployees(newAvailable);
  }
}
