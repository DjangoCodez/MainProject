import { Component, input, Input, signal } from '@angular/core';
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
  selector: 'sp-shift-request-recipients-selected',
  imports: [GridWrapperComponent],
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  styleUrl: './sp-shift-request-recipients.component.scss',
  providers: [FlowHandlerService, ToolbarService],
})
export class SpShiftRequestRecipientsSelectedComponent extends EmbeddedGridBaseDirective<
  IAvailableEmployeesDTO,
  SpShiftRequestDialogForm
> {
  @Input({ required: true }) form!: SpShiftRequestDialogForm;
  noMargin = input(true);
  toolbarNoPadding = input(true);
  height = input(310);

  disableRemoveButton = signal(true);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'shift.request.recipients.selected', {});

    this.form.selectedEmployees.valueChanges.subscribe(v => {
      this.rowData.next(v);
      this.disableRemoveButton.set(v.length === 0);
    });
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    super.createGridToolbar({ hideNew: true });
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarButton('remove', {
          caption: signal('Ta bort markerade'),
          iconName: signal('chevrons-left'),
          disabled: this.disableRemoveButton,
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
        this.grid.addColumnText('employeeName', 'Valda', {
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
    const newSelected = this.form.selectedEmployees.value.filter(
      se => !selected.find(s => s.employeeId === se.employeeId)
    );
    const currentAvailable = this.form.availableEmployees.value;
    const newAvailable = [...currentAvailable, ...selected];
    this.form.patchSelectedEmployees(newSelected);
    this.form.patchAvailableEmployees(newAvailable);
  }
}
