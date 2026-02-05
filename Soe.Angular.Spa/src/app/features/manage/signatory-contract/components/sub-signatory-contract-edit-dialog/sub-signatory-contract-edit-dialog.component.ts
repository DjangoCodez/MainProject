import { AfterViewChecked, ChangeDetectorRef, Component, inject } from '@angular/core';
import { ColumnUtil } from '@ui/grid/util/column-util'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { ISubSignatoryContractEditDialogData } from '../../models/sub-signatory-contract-edit-dialog-data';
import { ValidationHandler } from '@shared/handlers';
import { SubSignatoryContractForm } from '../../models/sub-signatory-contract-form.model';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, take } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { ISignatoryContractPermissionEditItem } from '@shared/models/generated-interfaces/SignatoryContractPermissionEditItem';

@Component({
  selector: 'soe-sub-signatory-contract-edit-dialog',
  standalone: false,
  templateUrl: './sub-signatory-contract-edit-dialog.component.html',
  styleUrls: ['./sub-signatory-contract-edit-dialog.component.scss'],
})
export class SubSignatoryContractEditDialogComponent 
  extends DialogComponent<ISubSignatoryContractEditDialogData>
  implements AfterViewChecked
{
  
  private readonly validationHandler = inject(ValidationHandler);
  private readonly translate = inject(TranslateService);
  private readonly cdRef = inject(ChangeDetectorRef);

  protected readonly form: SubSignatoryContractForm;

  protected gridColumns: ColDef[] = [];
  protected readonly gridRows: BehaviorSubject<
    ISignatoryContractPermissionEditItem[]
  >;

  constructor() {
    super();
    this.form = new SubSignatoryContractForm({
      validationHandler: this.validationHandler,
      element: this.data.rowToUpdate,
    });
    const rows: ISignatoryContractPermissionEditItem[] = this.getRows();
    this.gridRows = new BehaviorSubject<ISignatoryContractPermissionEditItem[]>(
      rows
    );

    this.setGridColumns();
  }

  ngAfterViewChecked(): void {
    // need to run this if the form is invalid inizially (no user, permissions)
    // due to the console error
    this.cdRef.detectChanges();
  }

  protected ok(): void {
    this.dialogRef.close(this.form.getAllValues());
  }

  private getRows(): ISignatoryContractPermissionEditItem[] {
    let rows: ISignatoryContractPermissionEditItem[];
    if (this.data.permissionTerms.length) {
      rows = this.data.permissionTerms.map(p => {
        const isSelected = this.data.rowToUpdate.permissionTypes.some(
          pt => pt === p.id
        );

        const row: ISignatoryContractPermissionEditItem = {
          id: p.id,
          name: p.name,
          isSelected: isSelected,
        };

        return row;
      });
    } else {
      rows = [];
    }

    return rows;
  }

  private setGridColumns(): void {
    this.translate
      .get(['manage.registry.signatorycontract.permission'])
      .pipe(take(1))
      .subscribe(terms => {
        this.gridColumns = [
          ColumnUtil.createColumnBool('isSelected', '', {
            width: 40,
            editable: true,
            columnSeparator: true,
            onClick: this.toggleSelected.bind(this),
          }),
          ColumnUtil.createColumnText(
            'name',
            terms['manage.registry.signatorycontract.permission']
          ),
        ];
      });
  }

  private toggleSelected(
    isChecked: boolean,
    row: ISignatoryContractPermissionEditItem
  ): void {

    setTimeout(() => {

      const selectedPermissions: number[] = this.gridRows.value
        .filter(r => r.isSelected)
        .map(r => r.id);

      this.form.permissionTypes.setValue(selectedPermissions);  
      this.form.permissionTypes.markAsDirty();
      this.form.permissionTypes.markAsTouched();
    }, 0);
  }
}
