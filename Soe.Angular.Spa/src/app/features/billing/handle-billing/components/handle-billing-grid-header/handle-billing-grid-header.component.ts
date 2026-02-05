import { Component, EventEmitter, inject, OnInit, Output } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { forkJoin, Observable, tap } from 'rxjs';
import { HandleBillingService } from '../../services/handle-billing.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { CoreService } from '@shared/services/core.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ValidationHandler } from '@shared/handlers';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { HandleBillingHeaderForm } from '../../models/handle-billing-header-form.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IHandleBillingRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SearchCustomerInvoiceRowModel } from '../../models/handle-billing.model';

@Component({
  selector: 'soe-handle-billing-grid-header',
  templateUrl: 'handle-billing-grid-header.component.html',
  standalone: false,
})
export class HandleBillingGridHeaderComponent
  extends GridBaseDirective<IHandleBillingRowDTO, HandleBillingService>
  implements OnInit
{
  projects: any[] = [];
  orders: any[] = [];
  customers: any[] = [];
  orderTypes: any[] = [];
  orderContractTypes: any[] = [];

  service = inject(HandleBillingService);
  validationHandler = inject(ValidationHandler);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  performLoad = new Perform<any>(this.progressService);

  @Output() searchClick = new EventEmitter<SearchCustomerInvoiceRowModel>();

  form: HandleBillingHeaderForm = new HandleBillingHeaderForm({
    validationHandler: this.validationHandler,
    element: [{}],
  });

  ngOnInit(): void {
    const today = DateUtil.getToday();
    this.form.dateRange.patchValue([
      DateUtil.getDateFirstInMonth(today.addMonths(-1)),
      DateUtil.getDateLastInWeek(today),
    ]);

    this.startFlow(Feature.Billing_Order_HandleBilling, 'handleBillingGrid', {
      lookups: [
        this.loadProjects(),
        this.loadOrders(),
        this.loadCustomers(),
        this.loadOrderTypes(),
        this.loadOrderContractTypes(),
      ],
      skipInitialLoad: true,
    });
  }

  private loadLookups() {
    return this.performLoad.load(
      forkJoin([
        this.service.getProjects(),
        this.service.getOrders(),
        this.service.getCustomers(),
        this.coreService.getTermGroupContent(TermGroup.OrderType, false, true),
        this.coreService.getTermGroupContent(
          TermGroup.OrderContractType,
          false,
          true
        ),
      ]).pipe(
        tap(([projects, orders, customers, orderTypes, orderContractTypes]) => {
          this.projects = projects;
          this.orders = orders;
          this.customers = customers;
          this.orderTypes = orderTypes;
          this.orderContractTypes = orderContractTypes;
        })
      )
    );
  }

  private loadProjects(): Observable<SmallGenericType[]> {
    this.projects = [];
    return this.service.getProjects().pipe(
      tap(res => {
        this.projects = res;
      })
    );
  }

  private loadOrders(): Observable<SmallGenericType[]> {
    this.orders = [];
    return this.service.getOrders().pipe(
      tap(res => {
        this.orders = res;
      })
    );
  }

  private loadCustomers(): Observable<SmallGenericType[]> {
    this.customers = [];
    return this.service.getCustomers().pipe(
      tap(res => {
        this.customers = res;
      })
    );
  }

  private loadOrderTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.OrderType, false, true)
      .pipe(
        tap(res => {
          this.orderTypes = res;
        })
      );
  }

  private loadOrderContractTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.OrderContractType, false, true)
      .pipe(
        tap(res => {
          this.orderContractTypes = res;
        })
      );
  }

  search(): void {
    this.searchClick.emit(this.form.getSearchModel());
  }
}
