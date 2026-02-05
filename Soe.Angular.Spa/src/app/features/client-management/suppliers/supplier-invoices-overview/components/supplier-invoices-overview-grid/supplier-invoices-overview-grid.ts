import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { IMultiCompanyResponseDTO } from '@features/client-management/models/client-management.models';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISupplierInvoiceSummaryDTO } from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, Observable, take } from 'rxjs';
import { SupplierInvoiceOverviewService } from '../../services/supplier-invoice-overview.service';
import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ReactiveFormsModule } from '@angular/forms';
import { SelectComponent } from '@ui/forms/select/select.component';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IMultiCompanyErrorDTO } from '@shared/models/generated-interfaces/MultiCompanyResponseDTO';

@Component({
  selector: 'soe-supplier-invoices-overview-grid',
  providers: [FlowHandlerService, ToolbarService],
  imports: [GridWrapperComponent, ReactiveFormsModule, SelectComponent],
  templateUrl: './supplier-invoices-overview-grid.html',
  standalone: true,
})
export class SupplierInvoicesOverviewGrid
  extends GridBaseDirective<
    ISupplierInvoiceSummaryDTO,
    SupplierInvoiceOverviewService
  >
  implements OnInit
{
  readonly service = inject(SupplierInvoiceOverviewService);
  private validationHandler = inject(ValidationHandler);
  protected form: SoeFormGroup;
  protected companies = signal<SmallGenericType[]>([]);
  protected errors = signal<IMultiCompanyErrorDTO[]>([]);

  constructor() {
    super();

    this.form = new SoeFormGroup(this.validationHandler, {
      company: new SoeSelectFormControl(0),
    });

    effect(() => {
      const errors = this.errors();
      if (errors.length > 0) {
        alert(
          'Errors occurred while loading data:\n  - ' +
            errors.map(e => e.errorMessage).join('\n  - ')
        );
      }
    });
  }

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.ClientManagement_Supplier_Invoices,
      'ClientManagement.SupplierInvoices.Overview',
      {
        skipInitialLoad: true,
      }
    );
  }

  override loadData(): Observable<ISupplierInvoiceSummaryDTO[]> {
    return this.loadInvoices();
  }

  override onFinished(): void {
    this.loadData().subscribe();
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISupplierInvoiceSummaryDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'clientmanagement.suppliers.invoices.overview.company',
        'clientmanagement.suppliers.invoices.overview.unhandled',
        'clientmanagement.suppliers.invoices.overview.underattesting',
        'clientmanagement.suppliers.invoices.overview.readyforpayment',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'actorCompanyName',
          terms['clientmanagement.suppliers.invoices.overview.company'],
          { flex: 1 }
        );
        this.grid.addColumnNumber(
          'unhandledInvoiceCount',
          terms['clientmanagement.suppliers.invoices.overview.unhandled'],
          { flex: 1 }
        );
        this.grid.addColumnNumber(
          'attestingInvoiceCount',
          terms['clientmanagement.suppliers.invoices.overview.underattesting'],
          { flex: 1 }
        );
        this.grid.addColumnNumber(
          'paymentReadyInvoiceCount',
          terms['clientmanagement.suppliers.invoices.overview.readyforpayment'],
          { flex: 1 }
        );

        super.finalizeInitGrid();
      });
  }

  private loadInvoices(): Observable<ISupplierInvoiceSummaryDTO[]> {
    return this.performLoadData.load$(this.service.getInvoicesSummary()).pipe(
      map(
        (
          result: IMultiCompanyResponseDTO<ISupplierInvoiceSummaryDTO[]>
        ): ISupplierInvoiceSummaryDTO[] => {
          const comps = <ISupplierInvoiceSummaryDTO[]>result.value;

          this.errors.set(result.errors ?? []);
          this.companies.set(
            comps.map(
              c => new SmallGenericType(c.actorCompanyId, c.actorCompanyName)
            )
          );
          return comps;
        }
      )
    );
  }
}
