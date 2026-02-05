import { inject, Injectable, signal, WritableSignal } from '@angular/core';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import {
  TermGroup_ProjectStatus,
  TermGroup_ProjectType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { finalize, forkJoin, map, Observable, of, tap } from 'rxjs';
import { SupplierInvoiceService } from './supplier-invoice.service';
import { ProgressService } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { SupplierInvoiceCostAllocationDTO } from '../models/supplier-invoice.model';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceProjectOrderLoaderService {
  readonly service = inject(SupplierInvoiceService);
  readonly progressService = inject(ProgressService);
  readonly performLoadData = new Perform<any>(this.progressService);
  public customerInvoices: WritableSignal<ICustomerInvoiceSmallGridDTO[]> =
    signal([]);
  public projectTinyDtos: WritableSignal<IProjectTinyDTO[]> = signal([]);
  public projects: WritableSignal<ISmallGenericType[]> = signal([]);
  private loadingState: Observable<any> | null = null;
  private loaded = false;

  private performLoad() {
    this.loadingState = this.performLoadData.load$(
      forkJoin([
        this.loadOrdersForSupplierInvoiceEdit(),
        this.loadCustomerProjects(),
      ]).pipe(
        tap(() => {
          this.loaded = true;
        }),
        finalize(() => (this.loadingState = null))
      )
    );
    return this.loadingState;
  }

  public load() {
    if (this.loadingState) return this.loadingState;
    if (this.loaded) return of(true);
    return this.performLoad();
  }

  private loadOrdersForSupplierInvoiceEdit() {
    return this.service.getOrdersForSupplierInvoiceEdit(true).pipe(
      map(data => {
        return data.map(order => {
          order.customerInvoiceNumberName = truncateText(
            order.customerInvoiceNumberName
          );
          order.customerInvoiceNumberNameWithoutDescription = truncateText(
            order.customerInvoiceNumberNameWithoutDescription
          );
          return order;
        });
      }),
      tap(data => {
        this.customerInvoices.set(data);
      })
    );
  }

  private loadCustomerProjects() {
    return this.service
      .getProjectList(
        TermGroup_ProjectType.TimeProject,
        undefined,
        true,
        true,
        false
      )
      .pipe(
        tap(data => {
          const projectsArray: IProjectTinyDTO[] = [];
          projectsArray.push({
            projectId: 0,
            number: '',
            name: '',
            status: TermGroup_ProjectStatus.Active,
            parentProjectId: undefined,
            useAccounting: false,
          } as IProjectTinyDTO);
          projectsArray.push(...data);
          this.projectTinyDtos.set(projectsArray);

          this.projects.set(
            projectsArray.map(proj => {
              return {
                id: proj.projectId,
                name: proj.number + ' ' + proj.name,
              } as ISmallGenericType;
            })
          );
        })
      );
  }

  // helper functions

  getProject(productId: number | undefined): IProjectTinyDTO | undefined {
    return this.projectTinyDtos().find(p => p.projectId == productId);
  }

  getOrder(
    orderId: number | undefined
  ): ICustomerInvoiceSmallGridDTO | undefined {
    return this.customerInvoices().find(ci => ci.invoiceId == orderId);
  }

  setProjectDetails(
    row: SupplierInvoiceCostAllocationDTO,
    project: IProjectTinyDTO | undefined
  ) {
    if (project) {
      row.projectId = project.projectId;
      row.projectName = project.name;
      row.projectNr = project.number;
    }
  }
  setOrderDetails(
    row: SupplierInvoiceCostAllocationDTO,
    order: ICustomerInvoiceSmallGridDTO | undefined
  ) {
    if (order) {
      row.orderId = order.invoiceId;
      row.customerInvoiceNumberName =
        order.customerInvoiceNumberNameWithoutDescription;
      row.orderNr = order.invoiceNr;
    }
  }
}

export function truncateText(value?: string, max = 70): string {
  return value && value?.length > max
    ? value.substring(0, max) + '...'
    : value
      ? value
      : '';
}
