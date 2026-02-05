import { computed, inject, Injectable, Signal, signal } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceFeatureService {
  private readonly coreService = inject(CoreService);

  // Future could be smart to divide into read & write.
  private static readonly ALL_MODIFY_FEATURES = [
    Feature.Economy_Supplier_Invoice_Invoices_Edit,
    Feature.Economy_Preferences_Currency, // Use currency
    Feature.Economy_Distribution_Reports_Selection, // Invoice report
    Feature.Economy_Distribution_Reports_Selection_Download, // Invoice report
    Feature.Economy_Supplier_Suppliers_Edit, // Edit supplier
    Feature.Economy_Supplier_Invoice_Unlock, // Unlock invoice
    Feature.Economy_Supplier_Invoice_AddImage, // Add an invoice image
    Feature.Economy_Supplier_Invoice_ChangeCompany, // Change company
    Feature.Economy_Supplier_Invoice_AttestFlow, // AttestFlow
    Feature.Economy_Supplier_Invoice_AttestFlow_Admin, // AttestFlow administrator
    Feature.Economy_Supplier_Invoice_AttestFlow_Add, // Add AttestFlow
    Feature.Economy_Supplier_Invoice_AttestFlow_Cancel, // Cancel AttestFlow
    Feature.Economy_Supplier_Invoice_AttestFlow_TransferToLedger, // Transfer attest rows to accounting rows
    Feature.Economy_Supplier_Invoice_Project, // Add project rows
    Feature.Billing_Order_Orders_Edit, // Edit orders
    Feature.Billing_Purchase_Purchase_Edit, // Edit Purchase
    Feature.Economy_Supplier_Invoice_Invoices_Edit_UnlockAccounting, // Unlock accountingrows
    Feature.Economy_Supplier_Invoice_Finvoice, // Finvoice permission
    Feature.Economy_Intrastat, // Intrastat permission
    Feature.Economy_Supplier_Invoice_ProductRows, // Supplier invoice product rows permission
  ];

  private readonly features = signal<{ [key: number]: boolean } | null>(null);
  private readonly hasFeature = (feature: number): Signal<boolean> =>
    computed(() => this.features()?.[feature] ?? false);

  public readonly hasCurrency = this.hasFeature(
    Feature.Economy_Preferences_Currency
  );

  public readonly hasProductRows = this.hasFeature(
    Feature.Economy_Supplier_Invoice_ProductRows
  );

  public loadFeatures() {
    return this.coreService
      .hasModifyPermissions(SupplierInvoiceFeatureService.ALL_MODIFY_FEATURES)
      .pipe(tap(features => this.features.set(features)));
  }
}
