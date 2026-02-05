import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { TermCollection } from '@shared/localization/term-types';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ICardNumberGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { EmployeeCardNumbersService } from '../../services/employee-card-numbers.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-employee-card-numbers-grid',
  templateUrl: './employee-card-numbers-grid.component.html',
  styleUrls: ['./employee-card-numbers-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmployeeCardNumbersGridComponent
  extends GridBaseDirective<ICardNumberGridDTO, EmployeeCardNumbersService>
  implements OnInit
{
  service = inject(EmployeeCardNumbersService);
  progressService = inject(ProgressService);
  messageboxService = inject(MessageboxService);
  performAction = new Perform<any>(this.progressService);

  infoText = signal('');

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_CardNumbers,
      'Time.Employee.Cardnumber.Cardnumbers'
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super
      .loadTerms([
        'time.employee.cardnumber.inforow1',
        'time.employee.cardnumber.inforow2',
        'time.employee.cardnumber.inforow3',
        'time.employee.cardnumber.deletewarning',
      ])
      .pipe(
        tap(() => {
          this.setInfoText();
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<ICardNumberGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'time.employee.cardnumber.number',
        'time.employee.employeenumber',
        'time.employee.name',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'cardNumber',
          terms['time.employee.cardnumber.number'],
          {
            flex: 20,
          }
        );
        this.grid.addColumnText(
          'employeeNumber',
          terms['time.employee.employeenumber'],
          {
            flex: 40,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('employeeName', terms['time.employee.name'], {
          flex: 40,
          enableHiding: true,
        });

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.triggerDelete(row);
          },
        });
        this.grid.setNbrOfRowsToShow(37, 37);
        this.grid.dynamicHeight = true;
        super.finalizeInitGrid();
      });
  }

  performDelete(row: ICardNumberGridDTO): void {
    this.performAction.crud(
      CrudActionTypeEnum.Delete,
      this.service?.delete(row.employeeId),
      this.updateGrid.bind(this)
    );
  }

  private updateGrid(result: BackendResponse): void {
    if (result.success) this.refreshGrid();
  }

  triggerDelete(row: ICardNumberGridDTO): void {
    const message = this.terms['time.employee.cardnumber.deletewarning']
      .replace('{0}', row.cardNumber)
      .replace('{1}', row.employeeName);

    const mb = this.messageboxService.warning('core.delete', message);
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.performDelete(row);
    });
  }

  private setInfoText() {
    this.infoText.set(
      [
        this.terms['time.employee.cardnumber.inforow1'],
        this.terms['time.employee.cardnumber.inforow2'],
        this.terms['time.employee.cardnumber.inforow3'],
      ].join('\n')
    );
  }
}
