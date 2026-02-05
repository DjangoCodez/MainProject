import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IEmployeeCSRExportDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { EmployeeCsrExportService } from '../../services/employee-csr-export.service';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, take, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Perform } from '@shared/util/perform.class';
import { IGetCSRResponseModel } from '@shared/models/generated-interfaces/EconomyModels';
import { ICsrResponseDTO } from '@shared/models/generated-interfaces/CsrResponseDTO';
import { ValidationHandler } from '@shared/handlers';
import { EmployeeCsrExportForm } from '../../models/employee-csr-export-form.model';

enum ViewTypes {
  ViewAll = 3,
  ViewExported = 2,
  ViewNotExported = 1,
}

enum YearTypes {
  CurrentYear = 0,
  NextYear = 1,
}

@Component({
  standalone: false,
  templateUrl: './employee-csr-export-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class EmployeeCsrExportGridComponent
  extends GridBaseDirective<IEmployeeCSRExportDTO>
  implements OnInit
{
  validationHandler = inject(ValidationHandler);
  employeeCsrExportService = inject(EmployeeCsrExportService);
  coreService = inject(CoreService);
  messageboxService = inject(MessageboxService);
  form: EmployeeCsrExportForm | undefined;
  viewTypes: ISmallGenericType[] | undefined;
  allCsrRows: IEmployeeCSRExportDTO[] | undefined;

  performGridLoad = new Perform<IEmployeeCSRExportDTO[]>(this.progressService);
  performSave = new Perform<ICsrResponseDTO[]>(this.progressService);

  somethingSelected = false;

  ngOnInit(): void {
    super.ngOnInit();

    const currentMonth = new Date().getMonth() + 1;
    const december = 12;
    const selectedYearType =
      currentMonth !== december ? YearTypes.CurrentYear : YearTypes.NextYear;

    this.form = new EmployeeCsrExportForm(
      this.validationHandler,
      selectedYearType
    );

    this.startFlow(
      Feature.Time_Employee_Csr_Export,
      'Time.Employee.Csr.Export',
      {
        lookups: this.loadViewTypes(),
      }
    );
  }

  private get selectedYear(): number {
    const currentYear: number = new Date().getFullYear();
    const selectedYearType: YearTypes =
      this.form?.controls.selectedYearType.value ?? YearTypes.CurrentYear;
    return currentYear + selectedYearType;
  }

  onGridReadyToDefine(grid: GridComponent<IEmployeeCSRExportDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'time.employee.employeenumber',
        'time.employee.csr.personnumber',
        'time.employee.name',
        'time.employee.csr.exportdate',
        'time.employee.csr.importdate',
        'common.message',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'employeeNr',
          terms['time.employee.employeenumber'],
          { flex: 10 }
        );
        this.grid.addColumnText(
          'employeeSocialSec',
          terms['time.employee.csr.personnumber'],
          { flex: 15, enableHiding: true }
        );
        this.grid.addColumnText('employeeName', terms['time.employee.name'], {
          flex: 20,
          enableHiding: true,
        });

        this.grid.addColumnDate(
          'csrExportDate',
          terms['time.employee.csr.exportdate'],
          { flex: 15, enableHiding: true }
        );
        this.grid.addColumnDate(
          'csrImportDate',
          terms['time.employee.csr.importdate'],
          { flex: 15, enableHiding: true }
        );
        this.grid.addColumnText('message', terms['common.message'], {
          flex: 25,
          enableHiding: true,
        });

        this.grid.enableRowSelection();
        super.finalizeInitGrid();
      });
  }

  loadViewTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.CsrGridSelection, false, false, false)
        .pipe(
          tap(value => {
            this.viewTypes = value;
            this.form?.controls.currentViewType.setValue(ViewTypes.ViewAll);
          })
        )
    );
  }

  override loadData(
    id?: number | undefined
  ): Observable<IEmployeeCSRExportDTO[]> {
    return this.performGridLoad.load$(
      this.employeeCsrExportService.getEmployeesForCsrExport(this.selectedYear)
    );
  }

  updateGridData(csrResponses: ICsrResponseDTO[]) {
    this.loadData().subscribe((data: IEmployeeCSRExportDTO[]) => {
      this.allCsrRows = data || ([] as IEmployeeCSRExportDTO[]);
      this.allCsrRows.forEach(row => {
        const csrResponse = csrResponses.find(
          r => r.employeeId === row.employeeId
        );
        if (csrResponse) {
          (row as any).message = csrResponse.errorMessage;
        }
      });
      this.showFilteredEmployees();
    });
  }

  override onAfterLoadData(data?: IEmployeeCSRExportDTO[]): void {
    this.allCsrRows = data || [];
    this.showFilteredEmployees();
  }

  showFilteredEmployees(): void {
    this.allCsrRows = this.allCsrRows || [];

    const currentViewType: ViewTypes =
      this.form?.controls.currentViewType.value ?? ViewTypes.ViewAll;

    switch (currentViewType) {
      case ViewTypes.ViewAll:
        this.grid?.setData(this.allCsrRows);
        break;
      case ViewTypes.ViewNotExported:
        this.grid?.setData(this.allCsrRows.filter(r => !r.csrExportDate));
        break;
      case ViewTypes.ViewExported:
        this.grid?.setData(this.allCsrRows.filter(r => !!r.csrExportDate));
        break;
      default:
        this.grid?.setData([]);
        break;
    }
  }

  viewTypeChanged(event: any): void {
    this.showFilteredEmployees();
  }

  yearTypeChanged(event: any): void {
    this.refreshGrid();
  }

  selectionChanged(rows: IEmployeeCSRExportDTO[]): void {
    this.somethingSelected = rows.length > 0;
  }

  updateButtonClicked(): void {
    const selectedRows: IEmployeeCSRExportDTO[] =
      this.grid?.getSelectedRows() || [];
    const validIds: number[] = [];
    let invalidSocialSecCount: number = 0;

    selectedRows.forEach(r => {
      const isValid =
        (r.employeeSocialSec || '').split('-').join('').length === 12;

      if (isValid) {
        validIds.push(r.employeeId);
      } else {
        invalidSocialSecCount++;
      }
    });

    if (invalidSocialSecCount > 0) {
      const mb = this.messageboxService.warning(
        this.translate.instant('core.warning'),
        this.translate
          .instant('time.employee.csr.invalidsocialsecwarning')
          .format(invalidSocialSecCount),
        { buttons: 'ok' }
      );

      mb.afterClosed().subscribe(() => {
        this.getCsrInquiries(validIds, this.selectedYear);
      });
    } else {
      this.getCsrInquiries(validIds, this.selectedYear);
    }
  }

  getCsrInquiries(employeeIds: number[], selectedYear: number): void {
    if (employeeIds.length === 0) {
      return;
    }

    const model: IGetCSRResponseModel = {
      idsToTransfer: employeeIds,
      year: selectedYear,
    };

    this.performSave
      .load$(
        this.employeeCsrExportService.getCsrInquiries(model).pipe(
          tap((response: ICsrResponseDTO[]) => {
            if (response && response.length > 0) {
              const errorMessages: string[] = response
                .filter(r => r.errorMessage)
                .map(r => r.errorMessage);

              if (errorMessages.length > 0) {
                this.messageboxService.error(
                  this.translate.instant('core.error'),
                  errorMessages.join('\n')
                );
              }
              this.updateGridData(response);
            }
          })
        )
      )
      .subscribe();
  }
}
