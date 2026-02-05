import { Component, computed, inject, signal } from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import {
  ExternalCompanyGridRow,
  ExternalCompanySearchDialogData,
  ExternalCompanySearchFilter,
} from './models/external-company-search-dialog-data.model';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ExternalCompanySearchForm } from './models/external-company-search-dialog-form.model';
import { ValidationHandler } from '@shared/handlers';
import { DynamicGridModule } from '../dynamic-grid/dynamic-grid.module';
import { ColDef } from 'ag-grid-enterprise';
import { BehaviorSubject, take, tap } from 'rxjs';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ExternalCompanySearchService } from './services/external-company-search.service';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-external-company-search-dialog',
  templateUrl: './external-company-search-dialog.component.html',
  standalone: true,
  imports: [
    DialogComponent,
    ReactiveFormsModule,
    TranslateModule,
    TextboxComponent,
    ExpansionPanelComponent,
    DynamicGridModule,
    ButtonComponent,
  ],
})
export class ExternalCompanySearchDialogComponent extends DialogComponent<ExternalCompanySearchDialogData> {
  private readonly progress = inject(ProgressService);
  private readonly performLoad = new Perform(this.progress);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly translate = inject(TranslateService);
  private readonly service = inject(ExternalCompanySearchService);
  private readonly messageBox = inject(MessageboxService);
  private selectedRow = signal<ExternalCompanyGridRow | undefined>(undefined);
  protected form = new ExternalCompanySearchForm({
    validationHandler: this.validationHandler,
    element: this.data?.searchFilter,
  });
  protected gridRef?: GridComponent<ExternalCompanyGridRow>;
  protected gridColumns: ColDef[] = [];
  protected gridData = new BehaviorSubject<ExternalCompanyGridRow[]>([]);
  protected disableOK = computed((): boolean => {
    const row = this.selectedRow();
    return !row;
  });

  constructor() {
    super();
    this.setGridColumns();
  }

  private setGridColumns(): void {
    this.translate
      .get([
        'common.external.company.search.dialog.grid.regnumber',
        'common.external.company.search.dialog.grid.companyname',
        'common.external.company.search.dialog.grid.address',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.gridColumns = [
          ColumnUtil.createColumnText(
            'registrationNr',
            terms['common.external.company.search.dialog.grid.regnumber'],
            {
              enableHiding: false,
              suppressFilter: true,
              flex: 1,
              suppressSizeToFit: true,
              minWidth: 100,
            }
          ),
          ColumnUtil.createColumnText(
            'name',
            terms['common.external.company.search.dialog.grid.companyname'],
            {
              enableHiding: false,
              suppressFilter: true,
              flex: 2,
              suppressSizeToFit: true,
              minWidth: 100,
            }
          ),
          ColumnUtil.createColumnText(
            'addressStr',
            terms['common.external.company.search.dialog.grid.address'],
            {
              enableHiding: false,
              suppressFilter: true,
              flex: 3,
              suppressSizeToFit: true,
              minWidth: 100,
            }
          ),
        ];
      });
  }

  protected onSearchClick(): void {
    const filter = new ExternalCompanySearchFilter(this.form.value);
    if (!filter || filter.isEmpty()) {
      return;
    }

    this.performLoad.load(
      this.service.searchCompanies(this.data.searchProvider, filter).pipe(
        tap(rows => {
          if (!rows || rows.length === 0) {
            this.messageBox.information(
              this.translate.instant('core.info'),
              this.translate.instant(
                'common.external.company.search.dialog.result.nodata'
              )
            );
            this.gridData.next([]);
          } else {
            this.gridData.next(rows);
          }
        })
      )
    );
  }

  protected onRowSelectionChanged(rows: ExternalCompanyGridRow[]): void {
    this.selectedRow.set(rows && rows.length > 0 ? rows[0] : undefined);
  }

  protected onOkClick(): void {
    this.messageBox
      .question(
        this.translate.instant('core.warning'),
        this.translate.instant(
          'common.external.company.search.dialog.dataupdate.warning'
        )
      )
      .afterClosed()
      .subscribe(result => {
        if (result.result === true) {
          this.data.result = this.selectedRow();
          this.dialogRef.close(this.data);
        }
      });
  }

  protected onCancelClick(): void {
    this.dialogRef.close();
  }
}
